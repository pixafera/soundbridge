Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Reflection
Imports System.Threading

''' <summary>
''' A class for interacting with Soundbridges and other RCP compliant devices.
''' </summary>
''' <remarks></remarks>
Public Class TcpSoundbridgeClient
    Implements Pixa.Soundbridge.Library.ISoundbridgeClient

    Private _client As TcpClient
    Private _readTimeout As Integer = 5000

#Region " Constructors "
    ''' <summary>
    ''' Creates a new SoundbridgeClient connected to the specified IPEndPoint.
    ''' </summary>
    ''' <param name="localEP"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal localEP As IPEndPoint)
        MyClass.New(New TcpClient(localEP))
    End Sub

    ''' <summary>
    ''' Creates a new SoundbridgeClient connect to the specified host.
    ''' </summary>
    ''' <param name="hostname"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal hostname As String)
        MyClass.New(hostname, 5555)
    End Sub

    ''' <summary>
    ''' Creates a new SoundbridgeClient connected to the specified host and port.
    ''' </summary>
    ''' <param name="hostname"></param>
    ''' <param name="port"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal hostname As String, ByVal port As Integer)
        MyClass.New(New TcpClient(hostname, port))
    End Sub

    'Creates a new SoundbridgeClient connected to the specified TcpClient.
    Private Sub New(ByVal client As TcpClient)
        Try
            Dim stream As NetworkStream
            Dim receivedPreamble As String

            _client = client
            stream = _client.GetStream

            'set up the reader and check we're connected to a soundbridge
            _reader = New StreamReader(stream)
            stream.ReadTimeout = ReadTimeout
            receivedPreamble = _reader.ReadLine
            If receivedPreamble <> "roku: ready" Then ExceptionHelper.ThrowUnexpectedPreamble(receivedPreamble)
            stream.ReadTimeout = Timeout.Infinite

            'Setup the writer
            _writer = New StreamWriter(stream)

            'Start the reading thread
            Dim t As New Thread(AddressOf ReadFromClient)
            t.Start()
        Catch
            _client.Close()
            Throw
        End Try
    End Sub
#End Region

#Region " Connecting "
    Public Property ReadTimeout() As Integer
        Get
            Return _readTimeout
        End Get
        Set(ByVal value As Integer)
            _readTimeout = value
        End Set
    End Property

    Public ReadOnly Property RemoteEndPoint() As IPEndPoint
        Get
            Return _client.Client.RemoteEndPoint
        End Get
    End Property

    Public Sub Close()
        _client.Close()
    End Sub
#End Region

#Region " Reading "
    Private _reader As StreamReader
    Private _processors As New Dictionary(Of String, IResponseProcessor)
    Private _processorsLock As New Object

    Private Sub ReadFromClient()
        Try

            While _client.Connected
                Dim response As String = _reader.ReadLine
                Dim parts As String() = response.Split(":", 2I, StringSplitOptions.RemoveEmptyEntries)

                If HasProcessor(parts(0)) Then
                    Dim processedResponse As String
                    Dim processor As IResponseProcessor = GetProcessor(parts(0))

                    If parts.Length = 2 Then
                        processedResponse = parts(1).Trim
                    Else
                        processedResponse = ""
                    End If

                    If processedResponse.StartsWith("data bytes: ") Then
                        processedResponse = _reader.ReadLine
                        processor.IsByteArray = True
                    End If

                    Try
                        processor.Process(processedResponse)
                    Catch ex As Exception
                        'TODO: Log this exception properly
                        Debug.WriteLine(String.Format("Exception while processing command {0}", parts(0)))
                        Debug.WriteLine(ex.ToString)
                        RemoveProcessor(processedResponse)
                    End Try
                End If
            End While
        Catch ex As Exception
            Debug.WriteLine("Exception in read thread")
            Debug.WriteLine(ex.ToString)

            Throw
        Finally
            If _client.Connected Then _client.Client.Close(ReadTimeout)
        End Try
    End Sub

    Private Sub AddProcessor(ByVal key As String, ByVal item As IResponseProcessor)
        SyncLock _processorsLock
            _processors.Add(key, item)
        End SyncLock
    End Sub

    Private Function HasProcessor(ByVal key As String) As Boolean
        SyncLock _processorsLock
            Return _processors.ContainsKey(key)
        End SyncLock
    End Function

    Private Function GetProcessor(ByVal key As String) As IResponseProcessor
        SyncLock _processorsLock
            Return _processors(key)
        End SyncLock
    End Function

    Private Sub RemoveProcessor(ByVal key As String)
        SyncLock _processorsLock
            _processors.Remove(key)
        End SyncLock
    End Sub
#End Region

#Region " Invoke "
    Private _writer As StreamWriter
    Private Shared _invokeCache As New Dictionary(Of String, RcpCommandAttribute)
    Private Shared _invokeCacheLock As New Object

    Protected Function Invoke(ByVal method As String, ByVal ParamArray args() As String) As IResponseProcessor
        If _processors.ContainsKey(method) Then ExceptionHelper.ThrowAlreadyExecuting(method)

        Dim cmd As RcpCommandAttribute

        SyncLock _invokeCacheLock
            If _invokeCache.ContainsKey(method) Then
                cmd = _invokeCache(method)
            Else
                Dim info As MethodInfo = Me.GetType.GetMethod(method)
                If info Is Nothing Then ExceptionHelper.ThrowMethodNotFound(method)

                Dim cmds() As RcpCommandAttribute = info.GetCustomAttributes(GetType(RcpCommandAttribute), False)

                If cmds.Length = 0 Then ExceptionHelper.ThrowNotRcpCommandMethod(method)
                cmd = cmds(0)
                _invokeCache.Add(method, cmd)
            End If
        End SyncLock

        Dim wait As New EventWaitHandle(False, EventResetMode.AutoReset)
        Dim processor As IResponseProcessor = cmd.CreateResponseProcessor(Me, wait)

        AddProcessor(method, processor)
        _writer.Write(method)

        If args.Length > 0 Then
            For i As Integer = 0 To args.Length - 1
                _writer.Write(" ")
                _writer.Write(args(i))
            Next
        End If

        _writer.Write(vbCrLf)
        _writer.Flush()
        wait.WaitOne() '(ReadTimeout)
        RemoveProcessor(method)
        processor.PostProcess()

        'HACK: A number of commands involve UI things and it seems that some commands don't like being executed in quick succession.
        Thread.Sleep(500)

        Return processor
    End Function
#End Region

#Region " Progress "
    Public Event AwaitingReply As EventHandler(Of RcpCommandProgressEventArgs) Implements ISoundbridgeClient.AwaitingReply
    Public Event ReceivingData As EventHandler(Of RcpCommandReceivingProgressEventArgs) Implements ISoundbridgeClient.ReceivingData
    Public Event SendingRequest As EventHandler(Of RcpCommandProgressEventArgs) Implements ISoundbridgeClient.SendingRequest

    Protected Overridable Sub OnAwaitingReply(ByVal e As RcpCommandProgressEventArgs)
        RaiseEvent AwaitingReply(Me, e)
    End Sub

    Friend Sub OnAwaitingReply(ByVal command As String)
        OnAwaitingReply(New RcpCommandProgressEventArgs(command))
    End Sub

    Protected Overridable Sub OnReceivingData(ByVal e As RcpCommandReceivingProgressEventArgs)
        RaiseEvent ReceivingData(Me, e)
    End Sub

    Friend Sub OnReceivingData(ByVal command As String)
        OnReceivingData(New RcpCommandReceivingProgressEventArgs(command))
    End Sub

    Friend Sub OnReceivingData(ByVal command As String, ByVal progress As Integer)
        OnReceivingData(New RcpCommandReceivingProgressEventArgs(command, progress))
    End Sub

    Friend Sub OnReceivingData(ByVal command As String, ByVal progress As Integer, ByVal total As Integer)
        OnReceivingData(New RcpCommandReceivingProgressEventArgs(command, progress, total))
    End Sub

    Protected Overridable Sub OnSendingRequest(ByVal e As RcpCommandProgressEventArgs)
        RaiseEvent SendingRequest(Me, e)
    End Sub

    Friend Sub OnSendingRequest(ByVal command As String)
        OnSendingRequest(New RcpCommandProgressEventArgs(command))
    End Sub
#End Region

#Region " Methods "

#Region " Protocol Control "
    <RcpSynchronousCommand("CancelTransaction")> _
    Public Function CancelTransaction(ByVal command As String) As String Implements ISoundbridgeClient.CancelTransaction
        Dim p As IResponseProcessor = Invoke("CancelTransaction", command)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("DeleteList")> _
    Public Function DeleteList() As String Implements ISoundbridgeClient.DeleteList
        Dim p As IResponseProcessor = Invoke("DeleteList")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetProgressMode")> _
    Public Function GetProgressMode() As ProgressMode Implements ISoundbridgeClient.GetProgressMode
        Dim p As IResponseProcessor = Invoke("GetProgressMode")
        If p.Response(0) = "off" Then
            Return ProgressMode.Off
        Else
            Return ProgressMode.Verbose
        End If
    End Function

    <RcpSynchronousCommand("SetProgressMode")> _
    Public Function SetProgressMode(ByVal mode As ProgressMode) As String Implements ISoundbridgeClient.SetProgressMode
        Dim p As IResponseProcessor = Invoke("SetProgressMode", If(mode = ProgressMode.Off, "off", "verbose"))
        Return p.Response(0)
    End Function
#End Region

#Region " Host Configuration "
    <RcpSynchronousCommand("GetInitialSetupComplete")> _
    Public Function GetInitialSetupComplete() As String Implements ISoundbridgeClient.GetInitialSetupComplete
        Dim p As IResponseProcessor = Invoke("GetInitialSetupComplete")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetInitialSetupComplete")> _
    Public Function SetInitialSetupComplete() As String Implements ISoundbridgeClient.SetInitialSetupComplete
        Dim p As IResponseProcessor = Invoke("SetInitialSetupComplete")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetRequiredSetupSteps")> _
    Public Function GetRequiredSetupSteps() As String() Implements ISoundbridgeClient.GetRequiredSetupSteps
        Dim p As IResponseProcessor = Invoke("GetRequiredSetupSteps")
        Return p.Response
    End Function

    <RcpSynchronousCommand("ListLanguages")> _
    Public Function ListLanguages() As String() Implements ISoundbridgeClient.ListLanguages
        Dim p As IResponseProcessor = Invoke("ListLanguages")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetLanguage")> _
    Public Function GetLanguage() As String Implements ISoundbridgeClient.GetLanguage
        Dim p As IResponseProcessor = Invoke("GetLanguage")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetLanguage")> _
    Public Function SetLanguage(ByVal value As String) As String Implements ISoundbridgeClient.SetLanguage
        Dim p As IResponseProcessor = Invoke("SetLanguage", value)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ListRegions")> _
    Public Function ListRegions() As String() Implements ISoundbridgeClient.ListRegions
        Dim p As IResponseProcessor = Invoke("ListRegions")
        Return p.Response
    End Function

    <RcpSynchronousCommand("SetRegion")> _
    Public Function SetRegion(ByVal index As Integer) As String Implements ISoundbridgeClient.SetRegion
        Dim p As IResponseProcessor = Invoke("SetRegion", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetTermsOfServiceUrl")> _
    Public Function GetTermsOfServiceUrl() As String Implements ISoundbridgeClient.GetTermsOfServiceUrl
        Dim p As IResponseProcessor = Invoke("GetTermsOfServiceUrl")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("AcceptTermsOfService")> _
    Public Function AcceptTermsOfService() As String Implements ISoundbridgeClient.AcceptTermsOfService
        Dim p As IResponseProcessor = Invoke("AcceptTermsOfService")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetIfConfig")> _
    Public Function GetIfConfig() As String() Implements ISoundbridgeClient.GetIfConfig
        Dim p As IResponseProcessor = Invoke("GetIfConfig")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetLinkStatus")> _
    Public Function GetLinkStatus(ByVal networkAdapter As String) As String Implements ISoundbridgeClient.GetLinkStatus
        Dim p As IResponseProcessor = Invoke("GetLinkStatus", networkAdapter)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetIPAddress")> _
    Public Function GetIPAddress(ByVal networkAdapter As String) As String Implements ISoundbridgeClient.GetIPAddress
        Dim p As IResponseProcessor = Invoke("GetIPAddress", networkAdapter)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetMACAddress")> _
    Public Function GetMacAddress(ByVal networkAdapter As String) As String Implements ISoundbridgeClient.GetMacAddress
        Dim p As IResponseProcessor = Invoke("GetMacAddress", networkAdapter)
        Return p.Response(0)
    End Function

    <RcpTransactedCommand("ListWiFiNetworks", True)> _
    Public Function ListWiFiNetworks() As String() Implements ISoundbridgeClient.ListWiFiNetworks
        Dim p As IResponseProcessor = Invoke("ListWiFiNetworks")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetWiFiNetworkSelection")> _
    Public Function GetWiFiNetworkSelection() As String Implements ISoundbridgeClient.GetWiFiNetworkSelection
        Dim p As IResponseProcessor = Invoke("GetWiFiNetworkSelection")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetWiFiNetworkSelection")> _
    Public Function SetWiFiNetworkSelection(ByVal index As Integer) As String Implements ISoundbridgeClient.SetWiFiNetworkSelection
        Dim p As IResponseProcessor = Invoke("SetWiFiNetworkSelection")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetWiFiPassword")> _
    Public Function SetWiFiPassword(ByVal password As String) As String Implements ISoundbridgeClient.SetWiFiPassword
        Dim p As IResponseProcessor = Invoke("SetWiFiPassword")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetConnectedWiFiNetwork")> _
    Public Function GetConnectedWiFiNetwork() As String Implements ISoundbridgeClient.GetConnectedWiFiNetwork
        Dim p As IResponseProcessor = Invoke("GetConnectedWiFiNetwork")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetWiFiSignalQuality")> _
    Public Function GetWiFiSignalQuality() As String() Implements ISoundbridgeClient.GetWiFiSignalQuality
        Dim p As IResponseProcessor = Invoke("GetWiFiSignalQuality")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetTime")> _
    Public Function GetTime(ByVal formatted As Boolean) As String Implements ISoundbridgeClient.GetTime
        Dim p As IResponseProcessor

        If formatted Then
            p = Invoke("GetTime", "verbose")
        Else
            p = Invoke("GetTime")
        End If

        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetDate")> _
    Public Function GetDate(ByVal formatted As Boolean) As String Implements ISoundbridgeClient.GetDate
        Dim p As IResponseProcessor

        If formatted Then
            p = Invoke("GetDate", "verbose")
        Else
            p = Invoke("GetDate")
        End If

        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetTime")> _
    Public Function SetTime(ByVal value As String) As String Implements ISoundbridgeClient.SetTime
        Dim p As IResponseProcessor = Invoke("SetTime", value)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetDate")> _
    Public Function SetDate(ByVal value As String) As String Implements ISoundbridgeClient.SetDate
        Dim p As IResponseProcessor = Invoke("SetDate", value)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ListTimeZones", True)> _
    Public Function ListTimeZones() As String() Implements ISoundbridgeClient.ListTimeZones
        Dim p As IResponseProcessor = Invoke("ListTimeZones")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetTimeZone")> _
    Public Function GetTimeZone() As String Implements ISoundbridgeClient.GetTimeZone
        Dim p As IResponseProcessor = Invoke("GetTimeZone")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetTimeZone")> _
    Public Function SetTimeZone(ByVal index As Integer) As String Implements ISoundbridgeClient.SetTimeZone
        Dim p As IResponseProcessor = Invoke("SetTimeZone", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetSoftwareVersion")> _
    Public Function GetSoftwareVersion() As String Implements ISoundbridgeClient.GetSoftwareVersion
        Dim p As IResponseProcessor = Invoke("GetSoftwareVersion")
        Return p.Response(0)
    End Function

    <RcpTransactedCommand("CheckSoftwareUpgrade")> _
    Public Function CheckSoftwareUpgrade(ByVal local As Boolean) As String Implements ISoundbridgeClient.CheckSoftwareUpgrade
        Dim p As IResponseProcessor

        If local Then
            p = Invoke("CheckSoftwareUpgrade", "local")
        Else
            p = Invoke("CheckSoftwareUpgrade")
        End If

        Return p.Response(0)
    End Function

    <RcpTransactedCommand("ExecuteSoftwareUpgrade")> _
    Public Function ExecuteSoftwareUpgrade(ByVal local As Boolean) As String Implements ISoundbridgeClient.ExecuteSoftwareUpgrade
        Dim p As IResponseProcessor

        If local Then
            p = Invoke("ExecuteSoftwareUpgrade", "local")
        Else
            p = Invoke("ExecuteSoftwareUpgrade")
        End If

        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Reboot")> _
    Public Function Reboot() As String Implements ISoundbridgeClient.Reboot
        Dim p As IResponseProcessor = Invoke("Reboot")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetFriendlyName")> _
    Public Function GetFriendlyName() As String Implements ISoundbridgeClient.GetFriendlyName
        Dim p As IResponseProcessor = Invoke("GetFriendlyName")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetFriendlyName")> _
    Public Function SetFriendlyName(ByVal value As String) As String Implements ISoundbridgeClient.SetFriendlyName
        Dim p As IResponseProcessor = Invoke("SetFriendlyName")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetOption")> _
    Public Function GetOption(ByVal name As String) As String Implements ISoundbridgeClient.GetOption
        Dim p As IResponseProcessor = Invoke("GetOption")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetOption")> _
    Public Function SetOption(ByVal name As String, ByVal value As String) As String Implements ISoundbridgeClient.SetOption
        Dim p As IResponseProcessor = Invoke("SetOption", name, value)
        Return p.Response(0)
    End Function
#End Region

#Region " Display Control Commands "
    Public Event DisplayUpdate(ByVal data As String) Implements ISoundbridgeClient.DisplayUpdate

    <RcpSynchronousCommand("GetVisualizer")> _
    Public Function GetVisualizer(ByVal verbose As Boolean) As String Implements ISoundbridgeClient.GetVisualizer
        Dim p As IResponseProcessor

        If verbose Then
            p = Invoke("GetVisualizer", "verbose")
        Else
            p = Invoke("GetVisualizer")
        End If

        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetVisualizer")> _
    Public Function SetVisualizer(ByVal name As String) As String Implements ISoundbridgeClient.SetVisualizer
        Dim p As IResponseProcessor = Invoke("SetVisualizer", name)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("VisualizerMode"), _
     Obsolete("Use SetVisualizerMode instead")> _
    Public Function VisualizerMode(ByVal mode As String) As String Implements ISoundbridgeClient.VisualizerMode
        Dim p As IResponseProcessor = Invoke("VisualizerMode", mode)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetVisualizerMode")> _
    Public Function GetVisualizerMode() As String Implements ISoundbridgeClient.GetVisualizerMode
        Dim p As IResponseProcessor = Invoke("GetVisualizerMode")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetVisualizerMode")> _
    Public Function SetVisualizerMode(ByVal mode As String) As String Implements ISoundbridgeClient.SetVisualizerMode
        Dim p As IResponseProcessor = Invoke("SetVisualizerMode", mode)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ListVisualizers", True)> _
    Public Function ListVisualizers(ByVal verbose As Boolean) As String() Implements ISoundbridgeClient.ListVisualizers
        Dim p As IResponseProcessor

        If verbose Then
            p = Invoke("ListVisualizers", "verbose")
        Else
            p = Invoke("ListVisualizers")
        End If

        Return p.Response
    End Function

    <RcpSynchronousCommand("GetVizDataVU")> _
    Public Function GetVizDataVU() As String Implements ISoundbridgeClient.GetVizDataVU
        Dim p As IResponseProcessor = Invoke("GetVisDataVU")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetVizDataFreq")> _
    Public Function GetVizDataFreq() As String Implements ISoundbridgeClient.GetVizDataFreq
        Dim p As IResponseProcessor = Invoke("GetVizDataFreq")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetVizDataScope")> _
    Public Function GetVizDataScope() As String Implements ISoundbridgeClient.GetVizDataScope
        Dim p As IResponseProcessor = Invoke("GetVizDataScope")
        Return p.Response(0)
    End Function

    <RcpSubscriptionCommand("DisplayUpdateEventSubscribe", "OnDisplayUpdateEventSubscribe")> _
    Public Function DisplayUpdateEventSubscribe() As String Implements ISoundbridgeClient.DisplayUpdateEventSubscribe
        Dim p As IResponseProcessor = Invoke("DisplayUpdateEventSubscribe")
        Return p.Response(0)
    End Function

    Protected Overridable Sub OnDisplayUpdate(ByVal data As String)
        RaiseEvent DisplayUpdate(data)
    End Sub

    <RcpSynchronousCommand("DisplayUpdateEventUnsubscribe")> _
    Public Function DisplayUpdateEventUnsubscribe() As String Implements ISoundbridgeClient.DisplayUpdateEventUnsubscribe
        Dim p As IResponseProcessor = Invoke("DisplayUpdateEventUnsubscribe")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetDisplayData")> _
    Public Function GetDisplayData(ByRef byteData As Boolean) As String Implements ISoundbridgeClient.GetDisplayData
        Dim p As IResponseProcessor = Invoke("GetDisplayData")
        byteData = p.IsByteArray
        Return p.Response(0)
    End Function
#End Region

#Region " IR Demod/Dispatch "
    Public Event IRKeyPressed(ByVal data As String) Implements ISoundbridgeClient.IRKeyPressed
    Public Event IRKeyDown(ByVal data As String) Implements ISoundbridgeClient.IRKeyDown
    Public Event IRKeyUp(ByVal data As String) Implements ISoundbridgeClient.IRKeyUp

    <RcpSynchronousCommand("IRDispatchCommand")> _
    Public Function IRDispatchCommand(ByVal command As String) As String Implements ISoundbridgeClient.IRDispatchCommand
        Dim p As IResponseProcessor = Invoke("IRDispatchCommand", command)
        Return p.Response(0)
    End Function

    <RcpSubscriptionCommand("IRDemodSubscribe", "OnIRKeyPressed")> _
    Public Function IRDemodSubscribe(ByVal updown As Boolean) As String Implements ISoundbridgeClient.IRDemodSubscribe
        Dim p As IResponseProcessor

        If updown Then
            p = Invoke("IRDemodSubscribe", "updown")
        Else
            p = Invoke("IRDemodSubscribe")
        End If

        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("IRDemodUnsubscribe")> _
    Public Function IRDemodUnsubscribe() As String Implements ISoundbridgeClient.IRDemodUnsubscribe
        Dim p As IResponseProcessor = Invoke("IRDemodUnsubscribe")
        Return p.Response(0)
    End Function
#End Region

#Region " Media Servers "
    <RcpSynchronousCommand("ListServers", True)> _
    Public Function ListServers() As String() Implements ISoundbridgeClient.ListServers
        Dim p As IResponseProcessor = Invoke("ListServers")
        Return p.Response
    End Function

    <RcpSynchronousCommand("SetServerFilter")> _
    Public Function SetServerFilter(ByVal filterTokens As String) As String Implements ISoundbridgeClient.SetServerFilter
        Dim p As IResponseProcessor = Invoke("SetServerFilter", filterTokens)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetServerConnectPassword")> _
    Public Function SetServerConnectPassword(ByVal password As String) As String Implements ISoundbridgeClient.SetServerConnectPassword
        Dim p As IResponseProcessor = Invoke("SetServerConnectPassword")
        Return p.Response(0)
    End Function

    <RcpTransactedCommand("ServerConnect")> _
    Public Function ServerConnect(ByVal index As Integer) As String Implements ISoundbridgeClient.ServerConnect
        Dim p As IResponseProcessor = Invoke("ServerConnect", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ServerLaunchUI")> _
    Public Function ServerLaunchUI(ByVal index As Integer) As String Implements ISoundbridgeClient.ServerLaunchUI
        Dim p As IResponseProcessor = Invoke("ServerLaunchUI", index)
        Return p.Response(0)
    End Function

    <RcpTransactedCommand("ServerDisconnect")> _
    Public Function ServerDisconnect() As String Implements ISoundbridgeClient.ServerDisconnect
        Dim p As IResponseProcessor = Invoke("ServerDisconnect")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetConnectedServer")> _
    Public Function GetConnectedServer() As String Implements ISoundbridgeClient.GetConnectedServer
        Dim p As IResponseProcessor = Invoke("GetConnectedServer")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetActiveServerInfo")> _
    Public Function GetActiveServerInfo() As String() Implements ISoundbridgeClient.GetActiveServerInfo
        Dim p As IResponseProcessor = Invoke("GetActiveServerInfo")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetServerCapabilities")> _
    Public Function ServerGetCapabilities() As String() Implements ISoundbridgeClient.ServerGetCapabilities
        Dim p As IResponseProcessor = Invoke("ServerGetCapabilities")
        Return p.Response
    End Function
#End Region

#Region " Content Selection and Playback "
    <RcpTransactedCommand("ListSongs", True)> _
    Public Function ListSongs() As String() Implements ISoundbridgeClient.ListSongs
        Dim p As IResponseProcessor = Invoke("ListSongs")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListAlbums", True)> _
    Public Function ListAlbums() As String() Implements ISoundbridgeClient.ListAlbums
        Dim p As IResponseProcessor = Invoke("ListAlbums")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListArtists", True)> _
    Public Function ListArtists() As String() Implements ISoundbridgeClient.ListArtists
        Dim p As IResponseProcessor = Invoke("ListArtists")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListComposers", True)> _
    Public Function ListComposers() As String() Implements ISoundbridgeClient.ListComposers
        Dim p As IResponseProcessor = Invoke("ListComposers")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListGenres", True)> _
    Public Function ListGenres() As String() Implements ISoundbridgeClient.ListGenres
        Dim p As IResponseProcessor = Invoke("ListGenres")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListLocations", True)> _
    Public Function ListLocations() As String() Implements ISoundbridgeClient.ListLocations
        Dim p As IResponseProcessor = Invoke("ListLocations")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListMediaLanguages", True)> _
    Public Function ListMediaLanguages() As String() Implements ISoundbridgeClient.ListMediaLanguages
        Dim p As IResponseProcessor = Invoke("ListMediaLanguages")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListPlaylists")> _
    Public Function ListPlaylists() As String() Implements ISoundbridgeClient.ListPlaylists
        Dim p As IResponseProcessor = Invoke("ListPlaylists")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListPlaylistSongs")> _
    Public Function ListPlaylistSongs(ByVal playlistIndex As Integer) As String() Implements ISoundbridgeClient.ListPlaylistSongs
        Dim p As IResponseProcessor = Invoke("ListPlaylistSongs", playlistIndex)
        Return p.Response
    End Function

    <RcpTransactedCommand("ListContainerContents")> _
    Public Function ListContainerContents() As String() Implements ISoundbridgeClient.ListContainerContents
        Dim p As IResponseProcessor = Invoke("ListContainerContents")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetCurrentContainerPath")> _
    Public Function GetCurrentContainerPath() As String Implements ISoundbridgeClient.GetCurrentContainerPath
        Dim p As IResponseProcessor = Invoke("GetCurrentContainerPath")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ContainerEnter")> _
    Public Function ContainerEnter(ByVal index As Integer) As String Implements ISoundbridgeClient.ContainerEnter
        Dim p As IResponseProcessor = Invoke("ContainerEnter", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ContainerExit")> _
    Public Function ContainerExit() As String Implements ISoundbridgeClient.ContainerExit
        Dim p As IResponseProcessor = Invoke("ContainerExit")
        Return p.Response(0)
    End Function

    <RcpTransactedCommand("SearchSongs")> _
    Public Function SearchSongs(ByVal searchString As String) As String() Implements ISoundbridgeClient.SearchSongs
        Dim p As IResponseProcessor = Invoke("SearchSongs", searchString)
        Return p.Response
    End Function

    <RcpTransactedCommand("SearchArtists")> _
    Public Function SearchArtists(ByVal searchString As String) As String() Implements ISoundbridgeClient.SearchArtists
        Dim p As IResponseProcessor = Invoke("SearchArtists", searchString)
        Return p.Response
    End Function

    <RcpTransactedCommand("SearchAlbums")> _
    Public Function SearchAlbums(ByVal searchString As String) As String() Implements ISoundbridgeClient.SearchAlbums
        Dim p As IResponseProcessor = Invoke("SearchAlbums", searchString)
        Return p.Response
    End Function

    <RcpTransactedCommand("SearchComposers")> _
    Public Function SearchComposers(ByVal searchString As String) As String() Implements ISoundbridgeClient.SearchComposers
        Dim p As IResponseProcessor = Invoke("SearchComposers", searchString)
        Return p.Response
    End Function

    <RcpTransactedCommand("SearchAll")> _
    Public Function SearchAll(ByVal searchString As String) As String() Implements ISoundbridgeClient.SearchAll
        Dim p As IResponseProcessor = Invoke("SearchAll", searchString)
        Return p.Response
    End Function

    <RcpSynchronousCommand("SetBrowseFilterArtist")> _
    Public Function SetBrowseFilterArtist(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterArtist
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterArtist", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterAlbum")> _
    Public Function SetBrowseFilterAlbum(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterAlbum
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterAlbum", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterComposer")> _
    Public Function SetBrowseFilterComposer(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterComposer
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterComposer", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterGenre")> _
    Public Function SetBrowseFilterGenre(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterGenre
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterGenre", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterLocation")> _
    Public Function SetBrowseFilterLocation(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterLocation
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterComposer", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterMediaLanguage")> _
    Public Function SetBrowseFilterMediaLanguage(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterMediaLanguage
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterMediaLanguage", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterTopStations")> _
    Public Function SetBrowseFilterTopStations(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterTopStations
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterTopStations", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseFilterFavorites")> _
    Public Function SetBrowseFilterFavorites(ByVal filterString As String) As String Implements ISoundbridgeClient.SetBrowseFilterFavorites
        Dim p As IResponseProcessor = Invoke("SetBrowseFilterFavorites", filterString)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetSongListSort")> _
    Public Function SetSongListSort(ByVal key As String) As String Implements ISoundbridgeClient.SetSongListSort
        Dim p As IResponseProcessor = Invoke("SetSongListSort", key)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetBrowseListSort")> _
    Public Function SetBrowseListSort(ByVal key As String) As String Implements ISoundbridgeClient.SetBrowseListSort
        Dim p As IResponseProcessor = Invoke("SetBrowseListSort", key)
        Return p.Response(0)
    End Function
#End Region

#Region " Getting Detailed Song Info "
    <RcpTransactedCommand("GetSongInfo")> _
    Public Function GetSongInfo(ByVal index As Integer) As String() Implements ISoundbridgeClient.GetSongInfo
        Dim p As IResponseProcessor = Invoke("GetSongInfo", index)
        Return p.Response
    End Function

    <RcpTransactedCommand("GetCurrentSongInfo")> _
    Public Function GetCurrentSongInfo() As String() Implements ISoundbridgeClient.GetCurrentSongInfo
        Dim p As IResponseProcessor = Invoke("GetCurrentSongInfo")
        Return p.Response
    End Function
#End Region

#Region " Managing the Now Playing (ad-hoc) Playlist "
    <RcpSynchronousCommand("NowPlayingClear")> _
    Public Function NowPlayingClear() As String Implements ISoundbridgeClient.NowPlayingClear
        Dim p As IResponseProcessor = Invoke("NowPlayingClear")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ListNowPlayingQueue")> _
    Public Function ListNowPlayingQueue() As String() Implements ISoundbridgeClient.ListNowPlayingQueue
        Dim p As IResponseProcessor = Invoke("ListNowPlayingQueue")
        Return p.Response
    End Function
#End Region

#Region " Intiating Media Playback "
    <RcpSynchronousCommand("PlayIndex")> _
    Public Function PlayIndex(ByVal index As Integer) As String Implements ISoundbridgeClient.PlayIndex
        Dim p As IResponseProcessor = Invoke("PlayIndex", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("NowPlayingInsert")> _
    Public Function NowPlayingInsert(ByVal songIndex As Integer, ByVal insertIndex As Integer) As String Implements ISoundbridgeClient.NowPlayingInsert
        Dim p As IResponseProcessor
        Dim arg2 As String

        If songIndex < 0 Then
            arg2 = "all"
        Else
            arg2 = songIndex
        End If

        p = Invoke("NowPlayingInsert", arg2, insertIndex)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("NowPlayingRemoveAt")> _
    Public Function NowPlayingRemoveAt(ByVal index As Integer) As String Implements ISoundbridgeClient.NowPlayingRemoveAt
        Dim p As IResponseProcessor = Invoke("NowPlayingRemoveAt", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("QueueAndPlay")> _
    Public Function QueueAndPlay(ByVal songIndex As Integer) As String Implements ISoundbridgeClient.QueueAndPlay
        Dim p As IResponseProcessor = Invoke("QueueAndPlay", songIndex)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("QueueAndPlayOne")> _
    Public Function QueueAndPlayOne(ByVal index As Integer) As String Implements ISoundbridgeClient.QueueAndPlayOne
        Dim p As IResponseProcessor

        If index < 0 Then
            p = Invoke("QueueAndPlayOne", "working")
        Else
            p = Invoke("QueueAndPlayOne", index)
        End If

        Return p.Response(0)
    End Function
#End Region

#Region " Transport "
    <RcpSynchronousCommand("Play")> _
    Public Function Play() As String Implements ISoundbridgeClient.Play
        Dim p As IResponseProcessor = Invoke("Play")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Pause")> _
    Public Function Pause() As String Implements ISoundbridgeClient.Pause
        Dim p As IResponseProcessor = Invoke("Pause")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("PlayPause")> _
    Public Function PlayPause() As String Implements ISoundbridgeClient.PlayPause
        Dim p As IResponseProcessor = Invoke("PlayPause")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Next")> _
    Public Function [Next]() As String Implements ISoundbridgeClient.Next
        Dim p As IResponseProcessor = Invoke("Next")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Previous")> _
    Public Function Previous() As String Implements ISoundbridgeClient.Previous
        Dim p As IResponseProcessor = Invoke("Previous")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Stop")> _
    Public Function [Stop]() As String Implements ISoundbridgeClient.Stop
        Dim p As IResponseProcessor = Invoke("Stop")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Shuffle")> _
    Public Function Shuffle(ByVal value As Boolean) As String Implements ISoundbridgeClient.Shuffle
        Dim sValue As String = If(value, "on", "off")
        Dim p As IResponseProcessor = Invoke("Shuffle", sValue)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Repeat")> _
    Public Function Repeat(ByVal mode As String) As String Implements ISoundbridgeClient.Repeat
        Dim p As IResponseProcessor = Invoke("Repeat", mode)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetTransportState")> _
    Public Function GetTransportState() As String Implements ISoundbridgeClient.GetTransportState
        Dim p As IResponseProcessor = Invoke("GetTransportState")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetElapsedTime")> _
    Public Function GetElapsedTime() As String Implements ISoundbridgeClient.GetElapsedTime
        Dim p As IResponseProcessor = Invoke("GetElapsedTime")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetTotalTime")> _
    Public Function GetTotalTime() As String Implements ISoundbridgeClient.GetTotalTime
        Dim p As IResponseProcessor = Invoke("GetTotalTime")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetCurrentNowPlayingIndex")> _
    Public Function GetCurrentNowPlayingIndex() As String Implements ISoundbridgeClient.GetCurrentNowPlayingIndex
        Dim p As IResponseProcessor = Invoke("GetCurrentNowPlayingIndex")
        Return p.Response(0)
    End Function
#End Region

#Region " Volume Functions "
    <RcpSynchronousCommand("GetVolume")> _
    Public Function GetVolume() As String Implements ISoundbridgeClient.GetVolume
        Dim p As IResponseProcessor = Invoke("GetVolume")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetVolume")> _
    Public Function SetVolume(ByVal level As String) As String Implements ISoundbridgeClient.SetVolume
        Dim p As IResponseProcessor = Invoke("SetVolume", level)
        Return p.Response(0)
    End Function
#End Region

#Region " Commands for Using Presets "
    <RcpSynchronousCommand("ListPresets")> _
    Public Function ListPresets() As String() Implements ISoundbridgeClient.ListPresets
        Dim p As IResponseProcessor = Invoke("ListPresets")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetPresetInfo")> _
    Public Function GetPresetInfo(ByVal id As String) As String() Implements ISoundbridgeClient.GetPresetInfo
        Dim p As IResponseProcessor = Invoke("GetPresetInfo", id)
        Return p.Response
    End Function

    <RcpSynchronousCommand("PlayPreset")> _
    Public Function PlayPreset(ByVal id As String) As String Implements ISoundbridgeClient.PlayPreset
        Dim p As IResponseProcessor = Invoke("PlayPreset", id)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetPreset")> _
    Public Function SetPreset(ByVal id As String) As String Implements ISoundbridgeClient.SetPreset
        Dim p As IResponseProcessor = Invoke("SetPreset", id, "working")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetWorkingSongInfo")> _
    Public Function GetWorkingSongInfo() As String() Implements ISoundbridgeClient.GetWorkingSongInfo
        Dim p As IResponseProcessor = Invoke("GetWorkingSongInfo")
        Return p.Response
    End Function

    <RcpSynchronousCommand("SetWorkingSongInfo")> _
    Public Function SetWorkingSongInfo(ByVal name As String, ByVal value As String) As String Implements ISoundbridgeClient.SetWorkingSongInfo
        Dim p As IResponseProcessor = Invoke("SetWorkingSongInfo", name, value)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ClearWorkingSong")> _
    Public Function ClearWorkingSong() As String Implements ISoundbridgeClient.ClearWorkingSong
        Dim p As IResponseProcessor = Invoke("ClearWorkingSong")
        Return p.Response(0)
    End Function
#End Region

#Region " Power State Commands "
    <RcpSynchronousCommand("GetPowerState")> _
    Public Function GetPowerState() As String Implements ISoundbridgeClient.GetPowerState
        Dim p As IResponseProcessor = Invoke("GetPowerState")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetPowerState")> _
    Public Function SetPowerState(ByVal value As String, ByVal reconnect As Boolean) As String Implements ISoundbridgeClient.SetPowerState
        Dim p As IResponseProcessor

        If reconnect Then
            p = Invoke("SetPowerState", value, "yes")
        Else
            p = Invoke("SetPowerState", value, "no")
        End If

        Return p.Response(0)
    End Function
#End Region
#End Region

End Class
