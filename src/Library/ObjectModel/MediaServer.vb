''' <summary>
''' Represents a media streaming server that a <see cref="Soundbridge"/> can
''' connect to.
''' </summary>
''' <remarks></remarks>
Public Class MediaServer
   Inherits Pixa.Soundbridge.Library.SoundbridgeListObject

   Private _availability As MediaServerAvailability
   Private _sb As Soundbridge
   Private _type As MediaServerType
   Private _querySupport As MediaServerQuerySupport
   Private _containers As Boolean
   Private _playlists As Boolean
   Private _partialResults As Boolean

   ''' <summary>
   ''' Initialises a new instance of <see cref="MediaServer"/> from information
   ''' gleaned from the <see cref="ISoundbridgeClient.ListServers"/> command.
   ''' </summary>
   Friend Sub New(ByVal sb As Soundbridge, ByVal availability As MediaServerAvailability, ByVal type As MediaServerType, ByVal name As String, ByVal index As Integer)
      MyBase.New(sb, index, name)
      _sb = sb
      _availability = availability
      _type = type
      _container = New MediaContainer(Me, 0, "")
   End Sub

   ''' <summary>
   ''' Gets a value to determine whether the <see cref="MediaServer"/> is the
   ''' active server that the <see cref="Soundbridge"/> is connected to.
   ''' </summary>
   ''' <value>True if the <see cref="MediaServer"/> is the active server;
   ''' otherwise, false.</value>
   Public ReadOnly Property Connected() As Boolean
      Get
         Return Soundbridge.ConnectedServer Is Me
      End Get
   End Property

   ''' <summary>
   ''' Gets the availability of the media server.
   ''' </summary>
   Public ReadOnly Property Availability() As MediaServerAvailability
      Get
         Return _availability
      End Get
   End Property

   ''' <summary>
   ''' Gets the <see cref="Soundbridge"/> that received the <see cref="MediaServer"/>
   ''' information.
   ''' </summary>
   Public ReadOnly Property Soundbridge() As Soundbridge
      Get
         Return _sb
      End Get
   End Property

   ''' <summary>
   ''' Gets the media server's <see cref="MediaServerType"/>.
   ''' </summary>
   Public ReadOnly Property Type() As MediaServerType
      Get
         Return _type
      End Get
   End Property

   ''' <summary>
   ''' Connects the <see cref="Soundbridge"/> to the <see cref="MediaServer"/>.
   ''' </summary>
   ''' <remarks></remarks>
   Public Sub Connect()
      If Soundbridge.ConnectedServer IsNot Nothing Then Throw New InvalidOperationException("Can't connect to a server when there's already one connected.")

      Dim tries As Integer = 0

      While tries < 2
         Dim r As String = Client.ServerConnect(Index)

         Select Case r
            Case "Connected"
               Exit While

            Case "ConnectionFailedAlreadyConnected", "GenericError"
               Client.GetConnectedServer()
               Client.ServerDisconnect()

            Case Else
               ExceptionHelper.ThrowCommandReturnError("ServerConnect", r)

         End Select
      End While

      OnAfterConnect()
   End Sub

   ''' <summary>
   ''' Connects the <see cref="Soundbridge"/> to the <see cref="MediaServer"/>
   ''' using the specified password.
   ''' </summary>
   ''' <param name="password">The password to use to connect to the media
   ''' server.</param>
   ''' <remarks></remarks>
   Public Sub Connect(ByVal password As String)
      Dim r As String = Client.SetServerConnectPassword(password)

      If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetServerConnectPassword", r)

      Connect()
   End Sub

   ''' <summary>
   ''' Connects the <see cref="Soundbridge"/> to the <see cref="MediaServer"/>
   ''' allowing the Soundbridge's UI to prompt for the password to the media
   ''' server if one is needed.
   ''' </summary>
   ''' <remarks></remarks>
   Public Sub LaunchUi()
      Dim r As String = Client.ServerLaunchUI(Index)

      If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("ServerLaunchUI", r)
      OnAfterConnect()
   End Sub

   ''' <summary>
   ''' Sets this <see cref="MediaServer"/> as the active server.
   ''' </summary>
   ''' <remarks></remarks>
   Private Sub OnAfterConnect()
      Soundbridge.ConnectedServer = Me
      GetCapabilities()
   End Sub

   ''' <summary>
   ''' Updates the <see cref="MediaServer"/> with its capabilities.
   ''' </summary>
   Private Sub GetCapabilities()
      Dim info As String() = Client.ServerGetCapabilities

      If info.Length = 1 Then ExceptionHelper.ThrowCommandReturnError("ServerGetCapabilities", info(0))

      _querySupport = QuerySupportToMediaServerQuerySupport(info(0).Substring(14))
      _containers = (info(1).Substring(12) = "yes")
      _playlists = (info(2).Substring(11) = "yes")
      _partialResults = (info(3).Substring(16) = "yes")
   End Sub

   ''' <summary>
   ''' Converts a server type received from the <see cref="Soundbridge"/> to a
   ''' <see cref="MediaServerType"/> value.
   ''' </summary>
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

   ''' <summary>
   ''' Converts a string received from the <see cref="Soundbridge"/> to a <see cref="MediaServerQuerySupport"/>
   ''' value.
   ''' </summary>
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

   Public Overrides ReadOnly Property ShouldCacheDispose() As Boolean
      Get
         Return Not Connected
      End Get
   End Property

   Private _container As MediaContainer

   Public ReadOnly Property Container() As MediaContainer
      Get
         Return _container
      End Get
   End Property
End Class

''' <summary>
''' Represents the availability of a <see cref="MediaServer"/>.
''' </summary>
Public Enum MediaServerAvailability
   ''' <summary>
   ''' Indicates that the server is online and available to connect to.
   ''' </summary>
   Online

   ''' <summary>
   ''' Indicates that the server is offline and not available for connections.
   ''' </summary>
   ''' <remarks></remarks>
   Offline

   ''' <summary>
   ''' Inidicates that another media server should be used in preference to this
   ''' one.
   ''' </summary>
   Hidden

   ''' <summary>
   ''' Indicates that the server is inaccessible due to restrictions on the
   ''' <see cref="Soundbridge"/>.
   ''' </summary>
   Inaccessible
End Enum

''' <summary>
''' Represents the type of a <see cref="MediaServer"/>.
''' </summary>
''' <remarks></remarks>
<Flags()> _
Public Enum MediaServerType
   ''' <summary>
   ''' The server uses the iTunes DAAP protocol.
   ''' </summary>
   Daap = 1 'kITunes

   ''' <summary>
   ''' The server uses the UPnP protocol.
   ''' </summary>
   Upnp = 2 'kUPnP

   ''' <summary>
   ''' The server is running Firefly.
   ''' </summary>
   Rsp = 4 'kRSP

   ''' <summary>
   ''' The server is a SlimServer product, by SlimDevices.
   ''' </summary>
   Slim = 8 'kSlim

   ''' <summary>
   ''' The server is the Internet Radio server.
   ''' </summary>
   Radio = 16 'kFavoriteRadio

   ''' <summary>
   ''' The server is run locally from the Soundbridge, using a flash card.
   ''' </summary>
   Flash = 32 'kFlash

   ''' <summary>
   ''' The server uses the Soundbridge's LineIn input.
   ''' </summary>
   LineIn = 64 'kLineIn

   ''' <summary>
   ''' The server is the AM Tuner on the Soundbridge Radio.
   ''' </summary>
   AM = 128 'kAMTuner

   ''' <summary>
   ''' The server is the FM Tuner on the Soundbridge Radio.
   ''' </summary>
   FM = 256 'kFMTuner

   ''' <summary>
   ''' Represents all possible server types.
   ''' </summary>
   All = 511
End Enum

''' <summary>
''' Represents the level of query support a media server has.
''' </summary>
''' <remarks></remarks>
Public Enum MediaServerQuerySupport
   ''' <summary>
   ''' The server does not support queries of any kind.
   ''' </summary>
   None

   ''' <summary>
   ''' The server allows lists of songs, but no searching.
   ''' </summary>
   Songs

   ''' <summary>
   ''' The server allows for filtering using the browse filters.
   ''' </summary>
   Basic

   ''' <summary>
   ''' The server allows for queries using the search methods.
   ''' </summary>
   [Partial]
End Enum