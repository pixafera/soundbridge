Public Class SoundbridgeEventArgs
    Inherits System.EventArgs

    Private _soundbridge As Soundbridge

    Public Sub New(ByVal soundbridge As Soundbridge)
        _soundbridge = soundbridge
    End Sub

    Public ReadOnly Property Soundbridge() As Soundbridge
        Get
            Return _soundbridge
        End Get
    End Property
End Class
