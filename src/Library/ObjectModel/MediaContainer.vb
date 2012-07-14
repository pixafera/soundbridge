Public Class MediaContainer
   Inherits Pixa.Soundbridge.Library.MediaObject

   Private _parent As MediaContainer
   Private _isCurrentContainer As Boolean

   Friend Sub New(ByVal server As MediaServer, ByVal index As Integer, ByVal name As String)
      MyBase.New(server, index, name)
      _isCurrentContainer = True
      Soundbridge.Cache.RegisterCache(New MediaContainerMediaContainerCacheProvider(Soundbridge, Me))
      Soundbridge.Cache.RegisterCache(New MediaContainerSongCacheProvider(Soundbridge, Me))
   End Sub

   Friend Sub New(ByVal parent As MediaContainer, ByVal index As Integer, ByVal name As String)
      MyBase.New(parent.Server, index, name)
      _parent = parent
      Soundbridge.Cache.RegisterCache(New MediaContainerMediaContainerCacheProvider(Soundbridge, Me))
      Soundbridge.Cache.RegisterCache(New MediaContainerSongCacheProvider(Soundbridge, Me))
   End Sub

   Public ReadOnly Property IsCurrentContainer() As Boolean
      Get
         Return Soundbridge.ConnectedServer Is Server And _isCurrentContainer
      End Get
   End Property

   Public ReadOnly Property Parent() As MediaContainer
      Get
         Return _parent
      End Get
   End Property

   Protected Overrides Sub Dispose(ByVal disposing As Boolean)
      MyBase.Dispose(disposing)

      If disposing Then
         Server.Soundbridge.Cache.DeregisterCache(Me, GetType(MediaContainer))
      End If
   End Sub

   Public Sub Enter()
      If IsActive Then
         Dim r As String = Client.ContainerEnter(Index)

         If r <> "OK" Then
            ThrowCommandReturnError("ContainerEnter", r)
         End If

         Server.Soundbridge.ActiveList = Nothing
         _isCurrentContainer = True
         Parent._isCurrentContainer = False
      Else
         Throw New InvalidOperationException("Can't enter a container that's not in the active list")
      End If
   End Sub

   Public Sub [Exit]()
      If IsCurrentContainer Then
         Dim r As String = Client.ContainerExit()

         If r <> "OK" Then
            ThrowCommandReturnError("ContainerExit", r)
         End If

         Server.Soundbridge.ActiveList = Nothing
         _isCurrentContainer = False
         Parent._isCurrentContainer = True
      Else
         Throw New InvalidOperationException("Can't exit a container that's not the current container")
      End If
   End Sub

   Public Function GetChildContainers() As SoundbridgeObjectCollection(Of MediaContainer)
      If IsCurrentContainer Then
         Dim containers As String() = Client.ListContainerContents
         Soundbridge.ActiveList = Soundbridge.Cache.BuildList(Of MediaContainer)(Me, containers)
         Return Soundbridge.ActiveList
      Else
         Throw New InvalidOperationException("Can't get child items for a container that's not the current container")
      End If
   End Function

   Public Function GetSongs() As SongCollection
      If IsCurrentContainer Then
         Dim songs As String() = Client.ListContainerContents
         Soundbridge.ActiveList = Soundbridge.Cache.BuildList(Of Song)(Me, songs)
         Return Soundbridge.ActiveList
      Else
         Throw New InvalidOperationException("Can't get child items for a container that's not the current container")
      End If
   End Function


End Class
