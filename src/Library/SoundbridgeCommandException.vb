Imports System.Runtime.Serialization

Public Class SoundbridgeCommandException
    Inherits Pixa.Soundbridge.Library.SoundbridgeClientException

    Private _command As String

    Public Sub New(ByVal command As String)
        MyBase.New()
        _command = command
    End Sub

    Public Sub New(ByVal command As String, ByVal message As String)
        MyBase.New(message)
        _command = command
    End Sub

    Public Sub New(ByVal command As String, ByVal message As String, ByVal innerException As Exception)
        MyBase.New(message, innerException)
        _command = command
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub
End Class
