Public MustInherit Class SoundbridgeObject

    Private _client As TcpSoundbridgeClient

    Protected Sub New(ByVal obj As SoundbridgeObject)
        MyClass.New(obj.Client)
    End Sub

    Protected Sub New(ByVal client As ISoundbridgeClient)
        _client = client
    End Sub

    Protected ReadOnly Property Client() As ISoundbridgeClient
        Get
            Return _client
        End Get
    End Property

End Class
