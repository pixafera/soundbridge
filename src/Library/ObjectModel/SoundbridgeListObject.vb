Public Class SoundbridgeListObject
   Inherits Pixa.Soundbridge.Library.SoundbridgeObject

   Private _index As Integer
   Private _name As String
   Private _soundbridge As Soundbridge

   Public Sub New(ByVal sb As Soundbridge, ByVal index As Integer, ByVal name As String)
      MyBase.New(sb)
      _index = index
      _name = name
      _soundbridge = sb
   End Sub

   Public ReadOnly Property IsActive() As Boolean
      Get
         Return Soundbridge.ActiveList IsNot Nothing AndAlso Soundbridge.ActiveList.Contains(Me)
      End Get
   End Property

   Public Property Index() As Integer
      Get
         Return _index
      End Get
      Friend Set(ByVal value As Integer)
         _index = value
      End Set
   End Property

   Public ReadOnly Property Name() As String
      Get
         Return _name
      End Get
   End Property

   Public ReadOnly Property Soundbridge() As Soundbridge
      Get
         Return _soundbridge
      End Get
   End Property

End Class
