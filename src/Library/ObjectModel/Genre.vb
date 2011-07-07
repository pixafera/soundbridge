Public Class Genre
    Inherits Pixa.Soundbridge.Library.MediaObject

    Private _songs As SongCollection

    Friend Sub New(ByVal server As MediaServer, ByVal name As String)
        MyBase.New(server, name)
    End Sub

    Public ReadOnly Property Songs() As SongCollection
        Get
            If _songs Is Nothing OrElse Not _songs.IsActive Then
                _songs = New SongCollection(Server.Soundbridge)
                Dim r As String = Client.SetBrowseFilterGenre(Name)
                If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetBrowseFilterGenre", r)

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

Public Class GenreCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of Genre)

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
    End Sub
End Class
