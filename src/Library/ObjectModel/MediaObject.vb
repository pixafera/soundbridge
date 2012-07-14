Public Class MediaObject
   Inherits Pixa.Soundbridge.Library.SoundbridgeListObject

   Private _server As MediaServer

   Friend Sub New(ByVal server As MediaServer, ByVal index As Integer, ByVal name As String)
      MyBase.New(server.Soundbridge, index, name)
      _server = server
   End Sub

   Public ReadOnly Property Server() As MediaServer
      Get
         Return _server
      End Get
   End Property
End Class
