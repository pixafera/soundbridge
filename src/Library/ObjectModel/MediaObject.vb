Public Class MediaObject
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Private _server As MediaServer
    Private _name As String

    Friend Sub New(ByVal server As MediaServer, ByVal name As String)
        MyBase.New(server)
        _server = server
        _name = name
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property Server() As MediaServer
        Get
            Return _server
        End Get
    End Property
End Class
