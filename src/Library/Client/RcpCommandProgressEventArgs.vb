Public Class RcpCommandProgressEventArgs
    Inherits System.EventArgs

    Private _command As String

    Public Sub New(ByVal command As String)
        _command = command
    End Sub

    Public ReadOnly Property Command() As String
        Get
            Return _command
        End Get
    End Property
End Class
