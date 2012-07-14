Public Class SoundbridgeObjectCollection(Of T As SoundbridgeListObject)
   Inherits System.Collections.ObjectModel.Collection(Of T)

   Private _sb As Soundbridge
   Private _dict As New Dictionary(Of String, T)

   Public Sub New(ByVal sb As Soundbridge)
      _sb = sb
   End Sub

   Default Public Overloads ReadOnly Property Item(ByVal key As String) As T
      Get
         Return _dict(key)
      End Get
   End Property

   Public ReadOnly Property IsActive() As Boolean
      Get
         Return Soundbridge.ActiveList Is Me
      End Get
   End Property

   Public ReadOnly Property Soundbridge() As Soundbridge
      Get
         Return _sb
      End Get
   End Property

   Public Overloads Function Contains(ByVal key As String) As Boolean
      Return _dict.ContainsKey(key)
   End Function

   Protected Overrides Sub ClearItems()
      MyBase.ClearItems()
      _dict.Clear()
   End Sub

   Protected Overrides Sub InsertItem(ByVal index As Integer, ByVal item As T)
      MyBase.InsertItem(index, item)
      If Not _dict.ContainsKey(item.Name) Then
         _dict.Add(item.Name, item)
      End If
   End Sub

   Protected Overrides Sub RemoveItem(ByVal index As Integer)
      Throw New NotSupportedException("Cannot remove items from a SoundbridgeObjectCollection")
   End Sub

   Protected Overrides Sub SetItem(ByVal index As Integer, ByVal item As T)
      Throw New NotSupportedException("Cannot set items in SoundbridgeObjectCollections")
   End Sub

End Class
