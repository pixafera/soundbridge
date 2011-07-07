Public Class Playlist
    Inherits Pixa.Soundbridge.Library.MediaObject

    Private _songs As SongCollection
    Private _index As Integer

    Friend Sub New(ByVal server As MediaServer, ByVal name As String, ByVal index As Integer)
        MyBase.New(server, name)
        _index = index
    End Sub

    Public ReadOnly Property Index() As Integer
        Get
            Return _index
        End Get
    End Property

    Public ReadOnly Property Songs() As SongCollection
        Get
            If Not Server.Soundbridge.ActiveList.Contains(Me) Then ExceptionHelper.ThrowObjectNotActive()

            If _songs Is Nothing OrElse Not _songs.IsActive Then
                _songs = New SongCollection(Server.Soundbridge)

                Dim songNames As String() = Client.ListPlaylistSongs(Index)
                If songNames.Length = 1 AndAlso (songNames(0) = "ErrorDisconnected" Or songNames(0) = "GenericError") Then ExceptionHelper.ThrowCommandReturnError("ListSongs", songNames(0))

                For i As Integer = 0 To songNames.Length - 1
                    _songs.Add(New Song(Server, songNames(i), i))
                Next
            End If

            Return _songs
        End Get
    End Property

End Class

Public Class PlaylistCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of Playlist)

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
    End Sub
End Class
