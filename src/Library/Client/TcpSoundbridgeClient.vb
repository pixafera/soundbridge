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
    Public Event AwaitingReply As EventHandler(Of RcpCommandProgressEventArgs)
    Public Event ReceivingData As EventHandler(Of RcpCommandReceivingProgressEventArgs)
    Public Event SendingRequest As EventHandler(Of RcpCommandProgressEventArgs)

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
    Public Function CancelTransaction(ByVal command As String) As String
        Dim p As IResponseProcessor = Invoke("CancelTransaction", command)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("DeleteList")> _
    Public Function DeleteList() As String
        Dim p As IResponseProcessor = Invoke("DeleteList")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetProgressMode")> _
    Public Function GetProgressMode() As ProgressMode
        Dim p As IResponseProcessor = Invoke("GetProgressMode")
        If p.Response(0) = "off" Then
            Return ProgressMode.Off
        Else
            Return ProgressMode.Verbose
        End If
    End Function

    <RcpSynchronousCommand("SetProgressMode")> _
    Public Function SetProgressMode(ByVal mode As ProgressMode) As String
        Dim p As IResponseProcessor = Invoke("SetProgressMode", If(mode = ProgressMode.Off, "off", "verbose"))
        Return p.Response(0)
    End Function
#End Region

#Region " Host Configuration "
    <RcpSynchronousCommand("GetInitialSetupComplete")> _
    Public Function GetInitialSetupComplete() As String
        Dim p As IResponseProcessor = Invoke("GetInitialSetupComplete")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetInitialSetupComplete")> _
    Public Function SetInitialSetupComplete() As String
        Dim p As IResponseProcessor = Invoke("SetInitialSetupComplete")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetRequiredSetupSteps")> _
    Public Function GetRequiredSetupSteps() As String()
        Dim p As IResponseProcessor = Invoke("GetRequiredSetupSteps")
        Return p.Response
    End Function

    <RcpSynchronousCommand("ListLanguages")> _
    Public Function ListLanguages() As String()
        Dim p As IResponseProcessor = Invoke("ListLanguages")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetLanguage")> _
    Public Function GetLanguage() As String
        Dim p As IResponseProcessor = Invoke("GetLanguage")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetLanguage")> _
    Public Function SetLanguage(ByVal value As String) As String
        Dim p As IResponseProcessor = Invoke("SetLanguage", value)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ListRegions")> _
    Public Function ListRegions() As String()
        Dim p As IResponseProcessor = Invoke("ListRegions")
        Return p.Response
    End Function

    <RcpSynchronousCommand("SetRegion")> _
    Public Function SetRegion(ByVal index As Integer) As String
        Dim p As IResponseProcessor = Invoke("SetRegion", index)
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetTermsOfServiceUrl")> _
    Public Function GetTermsOfServiceUrl() As String
        Dim p As IResponseProcessor = Invoke("GetTermsOfServiceUrl")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("AcceptTermsOfService")> _
    Public Function AcceptTermsOfService() As String
        Dim p As IResponseProcessor = Invoke("AcceptTermsOfService")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("ListTimeZones")> _
    Public Function ListTimeZones() As String()
        Dim p As IResponseProcessor = Invoke("ListTimeZones")
        Return p.Response
    End Function

    <RcpSynchronousCommand("GetTimeZone")> _
    Public Function GetTimeZone() As String
        Dim p As IResponseProcessor = Invoke("GetTimeZone")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("SetTimeZone")> _
    Public Function SetTimeZone(ByVal index As Integer) As String
        Dim p As IResponseProcessor = Invoke("SetTimeZone", index)
        Return p.Response(0)
    End Function
#End Region

#Region " IR Demod/Dispatch "
    <RcpSynchronousCommand("IrDispatchCommand")> _
    Public Function IrDispatchCommand(ByVal command As IRCommand) As String
        Dim p As IResponseProcessor = Invoke("IrDispatchCommand", GetIrCommandTranslation(command))

        Return p.Response(0)
    End Function
#End Region

#Region " Media Servers "
    <RcpSynchronousCommand("ListServers", True)> _
    Public Function ListServers() As String()
        Dim p As IResponseProcessor = Invoke("ListServers")
        Return p.Response
    End Function

    <RcpTransactedCommand("ServerConnect")> _
    Public Function ServerConnect(ByVal index As Integer) As String
        Dim p As IResponseProcessor = Invoke("ServerConnect", index)
        Return p.Response(0)
    End Function

    <RcpTransactedCommand("ServerDisconnect")> _
    Public Function ServerDisconnect() As String
        Dim p As IResponseProcessor = Invoke("ServerDisconnect")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("GetConnectedServer")> _
    Public Function GetConnectedServer() As String
        Dim p As IResponseProcessor = Invoke("GetConnectedServer")
        Return p.Response(0)
    End Function
#End Region

#Region " Content Selection and Playback "
    <RcpTransactedCommand("ListPlaylists")> _
    Public Function ListPlaylists() As String()
        Dim p As IResponseProcessor = Invoke("ListPlaylists")
        Return p.Response
    End Function

    <RcpTransactedCommand("ListPlaylistSongs")> _
    Public Function ListPlaylistSongs(ByVal playlistIndex As Integer) As String()
        Dim p As IResponseProcessor = Invoke("ListPlaylistSongs", playlistIndex)
        Return p.Response
    End Function
#End Region

#Region " Intiating Media Playback "
    <RcpSynchronousCommand("QueueAndPlay")> _
    Public Function QueueAndPlay(ByVal songIndex As Integer) As String
        Dim p As IResponseProcessor = Invoke("QueueAndPlay", songIndex)
        Return p.Response(0)
    End Function
#End Region

#Region " Transport "
    <RcpSynchronousCommand("Next")> _
    Public Function [Next]() As String
        Dim p As IResponseProcessor = Invoke("Next")
        Return p.Response(0)
    End Function

    <RcpSynchronousCommand("Shuffle")> _
    Public Function Shuffle(ByVal value As Boolean) As String
        Dim sValue As String = If(value, "on", "off")
        Dim p As IResponseProcessor = Invoke("Shuffle", sValue)
        Return p.Response(0)
    End Function
#End Region

#End Region

End Class
