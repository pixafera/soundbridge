Public Class Artist
    Inherits Pixa.Soundbridge.Library.MediaObject

    Private _albums As AlbumCollection
    Private _songs As SongCollection

    Friend Sub New(ByVal server As MediaServer, ByVal name As String)
        MyBase.New(server, name)
    End Sub

    Public ReadOnly Property Albums() As AlbumCollection
        Get
            If _albums Is Nothing Then
                _albums = New AlbumCollection(Server.Soundbridge)
                Dim r As String = Client.SetBrowseFilterArtist(Name)
                If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterArtist", r)

                Dim albumNames As String() = Client.ListAlbums
                If albumNames.Length = 1 AndAlso (albumNames(0) = "ErrorDisconnected" Or albumNames(0) = "GenericError") Then ExceptionHelper.ThrowCommandReturnError("ListAlbums", albumNames(0))

                For Each s As String In albumNames
                    _albums.Add(New Album(Server, s))
                Next
            End If

            Return _albums
        End Get
    End Property

    Public ReadOnly Property Songs() As SongCollection
        Get
            If _songs Is Nothing OrElse Not _songs.IsActive Then
                _songs = New SongCollection(Server.Soundbridge)
                Dim r As String = Client.SetBrowseFilterArtist(Name)
                If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterArtist", r)

                Dim songNames As String() = Client.ListSongs
                If songNames.Length = 1 AndAlso (songNames(0) = "ErrorDisconnected" Or songNames(0) = "GenericError") Then ExceptionHelper.ThrowCommandReturnError("ListSongs", songNames(0))

                For i As Integer = 0 To songNames.Length - 1
                    _songs.Add(New Song(Server, songNames(i), i))
                Next
            End If

            Return _songs
        End Get
    End Property
End Class

Public Class ArtistCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of Artist)

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
    End Sub
End Class
