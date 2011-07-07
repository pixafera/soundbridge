Public Class Album
    Inherits Pixa.Soundbridge.Library.MediaObject

    Private _artist As Artist
    Private _songs As SongCollection

    Friend Sub New(ByVal server As MediaServer, ByVal name As String)
        MyClass.New(server, name, Nothing)
    End Sub

    Friend Sub New(ByVal server As MediaServer, ByVal name As String, ByVal artist As Artist)
        MyBase.New(server, name)
        _artist = artist
    End Sub

    Public ReadOnly Property Songs() As SongCollection
        Get
            If _songs Is Nothing OrElse Not _songs.IsActive Then
                _songs = New SongCollection(Server.Soundbridge)
                Dim r As String = Client.SetBrowseFilterAlbum(Name)
                If r <> "OK" Then ThrowCommandReturnError("SetBrowseFilterAlbum", r)

                If _artist IsNot Nothing Then
                    r = Client.SetBrowseFilterArtist(_artist.Name)
                    If r <> "OK" Then ThrowCommandReturnError("SetBrowseFilterArtist", r)
                End If

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

Public Class AlbumCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of Album)

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
    End Sub
End Class