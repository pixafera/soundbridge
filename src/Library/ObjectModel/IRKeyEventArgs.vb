Public Class IRKeyEventArgs
    Inherits Pixa.Soundbridge.Library.SoundbridgeEventArgs

    Private _command As IRCommand
    Private _isHandled As Boolean = False

    Public Sub New(ByVal soundbridge As Soundbridge, ByVal command As IRCommand)
        MyBase.New(soundbridge)
        _command = command
    End Sub

    Public ReadOnly Property Command() As IRCommand
        Get
            Return _command
        End Get
    End Property

    Public ReadOnly Property IsHandled() As Boolean
        Get
            Return _isHandled
        End Get
    End Property

    Public Sub Handle()
        _isHandled = True
    End Sub
End Class
