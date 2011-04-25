<AttributeUsage(AttributeTargets.Field, AllowMultiple:=False)> _
Public Class IrCommandStringAttribute
    Inherits System.Attribute

    Private _commandString As String

    Public Sub New(ByVal commandString As String)
        _commandString = commandString
    End Sub

    Public ReadOnly Property CommandString() As String
        Get
            Return _commandString
        End Get
    End Property
End Class
