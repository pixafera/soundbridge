Public Class MediaServer
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Private _active As Boolean
    Private _availability As MediaServerAvailability
    Private _name As String
    Private _index As Integer
    Private _sb As Soundbridge
    Private _type As MediaServerType
    Private _querySupport As MediaServerQuerySupport
    Private _containers As Boolean
    Private _playlists As Boolean
    Private _partialResults As Boolean

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
        _sb = sb

        Dim r As String = Client.GetConnectedServer

        If r <> "OK" Then
            Client.ServerDisconnect()
            ExceptionHelper.ThrowCommandReturnError("GetConnectedServer", r)
        End If

        Dim info As String() = Client.GetActiveServerInfo
        If info.Length = 0 Then ExceptionHelper.ThrowCommandTimeout("GetActiveServerInfo")
        If info(0) = "ErrorDisconnected" Then ExceptionHelper.ThrowCommandReturnError("GetActiveServerInfo", "ErrorDisconnected")

        _type = ActiveServerTypeToMediaServerType(info(0).Substring(6))
        _name = info(1).Substring(6)
        GetCapabilities()
        _availability = MediaServerAvailability.Online
    End Sub

    Friend Sub New(ByVal sb As Soundbridge, ByVal availability As MediaServerAvailability, ByVal type As MediaServerType, ByVal name As String, ByVal index As Integer)
        MyBase.New(sb)
        _sb = sb
        _availability = availability
        _type = type
        _name = name
        _index = index
    End Sub

    Public ReadOnly Property Active() As Boolean
        Get
            Return _active
        End Get
    End Property

    Friend ReadOnly Property Index() As Integer
        Get
            Return _index
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property Soundbridge() As Soundbridge
        Get
            Return _sb
        End Get
    End Property

    Public Sub Connect()
        Dim r As String = Client.ServerConnect(Index)

        If r <> "OK" Then Throw New Exception("Soundbridge returned '{0}'")
        OnAfterConnect()
    End Sub

    Public Sub Connect(ByVal password As String)
        Dim r As String = Client.SetServerConnectPassword(password)

        If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetServerConnectPassword", r)

        Connect()
    End Sub

    Public Sub LaunchUi()
        Dim r As String = Client.ServerLaunchUI(Index)

        If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("ServerLaunchUI", r)
        OnAfterConnect()
    End Sub

    Private Sub OnAfterConnect()
        Soundbridge.ConnectedServer = Me
        GetCapabilities()
        _active = True
    End Sub

    Private Sub GetCapabilities()
        Dim info As String() = Client.ServerGetCapabilities

        If info.Length = 1 Then ExceptionHelper.ThrowCommandReturnError("ServerGetCapabilities", info(0))

        _querySupport = QuerySupportToMediaServerQuerySupport(info(0).Substring(14))
        _containers = (info(1).Substring(12) = "yes")
        _playlists = (info(2).Substring(11) = "yes")
        _partialResults = (info(3).Substring(16) = "yes")
    End Sub

    Private Function ActiveServerTypeToMediaServerType(ByVal value As String) As MediaServerType
        Select Case value
            Case "daap"
                Return MediaServerType.Daap

            Case "upnp"
                Return MediaServerType.Upnp

            Case "rsp"
                Return MediaServerType.Rsp

            Case "slim"
                Return MediaServerType.Slim

            Case "radio"
                Return MediaServerType.Radio

            Case "flash"
                Return MediaServerType.Flash

            Case "linein"
                Return MediaServerType.LineIn

            Case "am"
                Return MediaServerType.AM

            Case "fm"
                Return MediaServerType.FM

            Case Else
                Return -1
        End Select
    End Function

    Private Function QuerySupportToMediaServerQuerySupport(ByVal value As String) As MediaServerQuerySupport
        Select Case value
            Case "None"
                Return MediaServerQuerySupport.None

            Case "Songs"
                Return MediaServerQuerySupport.Songs

            Case "Basic"
                Return MediaServerQuerySupport.Basic

            Case "Partial"
                Return MediaServerQuerySupport.Partial

            Case Else
                Return -1
        End Select
    End Function
End Class

Public Enum MediaServerAvailability
    Online
    Offline
    Hidden
    Inaccessible
End Enum

<Flags()> _
Public Enum MediaServerType
    Daap = 1 'kITunes
    Upnp = 2 'kUPnP
    Rsp = 4 'kRSP
    Slim = 8 'kSlim
    Radio = 16 'kFavoriteRadio
    Flash = 32 'kFlash
    LineIn = 64 'kLineIn
    AM = 128 'kAMTuner
    FM = 256 'kFMTuner
    All = 511
End Enum

Public Enum MediaServerQuerySupport
    None
    Songs
    Basic
    [Partial]
End Enum