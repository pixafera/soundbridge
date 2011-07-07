Public Class MediaContainer
    Inherits Pixa.Soundbridge.Library.MediaObject

    Friend Sub New(ByVal server As MediaServer, ByVal name As String)
        MyBase.New(server, name)
    End Sub

End Class

Public Class MediaContainerCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObjectCollection(Of MediaContainer)

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
    End Sub
End Class
