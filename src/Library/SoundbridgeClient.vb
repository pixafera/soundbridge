Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Reflection
Imports System.Threading

''' <summary>
''' A class for interacting with Soundbridges and other RCP compliant devices.
''' </summary>
''' <remarks></remarks>
Public Class SoundbridgeClient

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
            Dim d As New Action(AddressOf ReadFromClient)
            d.BeginInvoke(Nothing, Nothing)
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

#Region " Methods "

#Region " IR Demod/Dispatch "
    Private Shared _irCommandTranslations As Dictionary(Of IrCommand, String)
    Private Shared _irCommandTranslationsLock As New Object

    Public Shared Function GetIrCommandTranslation(ByVal command As IrCommand) As String
        SyncLock _irCommandTranslationsLock
            If _irCommandTranslations Is Nothing Then
                Dim i As IrCommand

                _irCommandTranslations = New Dictionary(Of IrCommand, String)


                For Each f As FieldInfo In GetType(IrCommand).GetFields
                    Dim atts As IrCommandStringAttribute() = f.GetCustomAttributes(GetType(IrCommandStringAttribute), False)

                    If atts.Length = 1 Then
                        _irCommandTranslations.Add(f.GetValue(i), atts(0).CommandString)
                    End If
                Next
            End If

            Return _irCommandTranslations(command)
        End SyncLock
    End Function

    <RcpSynchronousCommand("IrDispatchCommand")> _
    Public Function IrDispatchCommand(ByVal command As IrCommand) As String
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
