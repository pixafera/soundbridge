Public MustInherit Class SoundbridgeObjectCollection(Of T As SoundbridgeObject)
    Inherits System.Collections.ObjectModel.Collection(Of T)

    Private _sb As Soundbridge

    Protected Sub New(ByVal sb As Soundbridge)
        _sb = sb
    End Sub

    Public ReadOnly Property IsActive() As Boolean
        Get
            Return Soundbridge.ActiveList Is Me
        End Get
    End Property

    Public ReadOnly Property Soundbridge() As Soundbridge
        Get
            Return _sb
        End Get
    End Property
End Class
