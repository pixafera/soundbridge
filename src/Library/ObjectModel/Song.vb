Public Class Song
    Inherits Pixa.Soundbridge.Library.MediaObject

    Private _index As Integer

    Friend Sub New(ByVal server As MediaServer, ByVal name As String, ByVal index As Integer)
        MyBase.New(server, name)
        _index = index
    End Sub

End Class

Public Class SongCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of Song)

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
    End Sub
End Class