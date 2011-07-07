Public Class DisplayUpdateEventArgs
    Inherits Pixa.Soundbridge.Library.SoundbridgeEventArgs

    Private _change As Integer

    Public Sub New(ByVal soundbridge As Soundbridge, ByVal change As Integer)
        MyBase.New(soundbridge)
        _change = change
    End Sub

    Public ReadOnly Property Change() As Integer
        Get
            Return _change
        End Get
    End Property
End Class
