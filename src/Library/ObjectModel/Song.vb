Public Class Song
   Inherits Pixa.Soundbridge.Library.MediaObject

   Private _hasInfo As Boolean
   Private _info As Dictionary(Of String, String)

   Friend Sub New(ByVal server As MediaServer, ByVal index As Integer, ByVal name As String)
      MyBase.New(server, index, name)
   End Sub

   Friend Sub New(ByVal parent As MediaContainer, ByVal index As Integer, ByVal name As String)
      MyBase.New(parent.Server, index, name)
   End Sub

   Public ReadOnly Property Album() As String
      Get
         Return GetSongInfo("album")
      End Get
   End Property

   Public ReadOnly Property Artist() As String
      Get
         Return GetSongInfo("artist")
      End Get
   End Property

   Public ReadOnly Property Genre() As String
      Get
         Return GetSongInfo("genre")
      End Get
   End Property

   Public ReadOnly Property HasInfo() As Boolean
      Get
         Return _hasInfo
      End Get
   End Property

   Public ReadOnly Property Id() As String
      Get
         Return GetSongInfo("id")
      End Get
   End Property

   Public ReadOnly Property Status() As String
      Get
         Return GetSongInfo("status")
      End Get
   End Property

   Public ReadOnly Property Title() As String
      Get
         Return GetSongInfo("title")
      End Get
   End Property

   ''' <summary>
   ''' Gets the length of the track in milliseconds.
   ''' </summary>
   ''' <value>The length of the track in milliseconds.</value>
   Public ReadOnly Property TrackLength() As Integer
      Get
         Return GetSongInfo("trackLengthMS")
      End Get
   End Property

   ''' <summary>
   ''' Gets the track number
   ''' </summary>
   ''' <value>The length of the track in milliseconds.</value>
   Public ReadOnly Property TrackNumber() As Integer
      Get
         Return GetSongInfo("trackNumber")
      End Get
   End Property

   Public ReadOnly Property Url() As String
      Get
         Return GetSongInfo("resource[0] url:")
      End Get
   End Property

   Public Function GetSongInfo(ByVal key As String) As String
      If Not HasInfo Then
         If IsActive Then
            Dim info As String() = Client.GetSongInfo(Index)

            _info = New Dictionary(Of String, String)()

            For Each s As String In info
               Dim parts As String() = s.Split(":", 2, StringSplitOptions.None)
               _info.Add(parts(0), parts(1))
            Next

            _hasInfo = True
         Else
            Throw New InvalidOperationException("Can't retrieve SongInfo when the MediaObject is not in the active list.")
         End If
      End If

      If _info.ContainsKey(key) Then
         Return _info(key)
      Else
         Return ""
      End If
   End Function

   Public Sub Play()
      Play(False)
   End Sub

   Public Sub Play(ByVal excludeList As Boolean)
      If IsActive Then
         If excludeList Then
            Dim r As String = Client.QueueAndPlayOne(Index)

            If r <> "OK" Then
               ThrowCommandReturnError("QueueAndPlayOne", r)
            End If
         Else
            Dim r As String = Client.QueueAndPlay(Index)

            If r <> "OK" Then
               ThrowCommandReturnError("QueueAndPlay", r)
            End If
         End If
      Else
         Throw New InvalidOperationException("Can't play a song that's not in the active list")
      End If
   End Sub

   Public Sub InsertIntoNowPlaying(ByVal targetIndex As Integer)
      If IsActive Then
         Dim r As String = Client.NowPlayingInsert(Index, targetIndex)

         If r <> "OK" Then
            ThrowCommandReturnError("NowPlayingInsert", r)
         End If
      Else
         Throw New InvalidOperationException("Can't play a song that's not in the active list")
      End If
   End Sub
End Class

Public Class SongCollection
   Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of Song)

   Friend Sub New(ByVal sb As Soundbridge)
      MyBase.New(sb)
   End Sub

   Public Sub Play()
      Play(0)
   End Sub

   Public Sub Play(ByVal startingIndex As Integer)
      If IsActive Then
         Dim r As String = Soundbridge.Client.QueueAndPlay(startingIndex)

         If r <> "OK" Then
            ThrowCommandReturnError("QueueAndPlay", r)
         End If
      Else
         Throw New InvalidOperationException("Can't play a song list that's not the active list")
      End If
   End Sub

   Public Sub InsertIntoNowPlaying(ByVal targetIndex As Integer)
      If IsActive Then
         Dim r As String = Soundbridge.Client.NowPlayingInsert(targetIndex)

         If r <> "OK" Then
            ThrowCommandReturnError("NowPlayingInsert", r)
         End If
      Else
         Throw New InvalidOperationException("Can't play a song list that's not the active list")
      End If
   End Sub
End Class