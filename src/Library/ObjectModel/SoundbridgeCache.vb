''' <summary>
''' Caches List objects so that identical calls to ListXyz return the same
''' object.
''' </summary>
''' <remarks></remarks>
Public Class SoundbridgeCache
   Private Structure CacheKey
      Public Parent As Object
      Public ChildType As Type

      Public Sub New(ByVal parent As SoundbridgeObject, ByVal childType As Type)
         Me.Parent = parent
         Me.ChildType = childType
      End Sub

      Public Overrides Function GetHashCode() As Integer
         Return Parent.GetHashCode() Xor ChildType.GetHashCode()
      End Function

      Public Overrides Function Equals(ByVal obj As Object) As Boolean
         If TypeOf obj Is CacheKey Then
            Dim k As CacheKey = obj
            Return k.ChildType Is ChildType And k.Parent Is Parent
         Else
            Return False
         End If
      End Function
   End Structure

   Private _cache As New Dictionary(Of CacheKey, ISoundbridgeListCacheProvider)()

   Public Function BuildList(Of T As SoundbridgeListObject)(ByVal parent As SoundbridgeObject, ByVal listData As String()) As SoundbridgeObjectCollection(Of T)
      Dim key As New CacheKey(parent, GetType(T))
      Return _cache(key).BuildList(listData)
   End Function

   Public Sub RegisterCache(Of T As SoundbridgeListObject)(ByVal cache As SoundbridgeListCacheProvider(Of T))
      _cache.Add(New CacheKey(cache.Parent, GetType(T)), cache)
   End Sub

   Public Sub DeregisterCache(ByVal parent As SoundbridgeObject, ByVal childType As Type)
      _cache.Remove(New CacheKey(parent, childType))
   End Sub
End Class

Public Interface ISoundbridgeListCacheProvider

   ReadOnly Property ChildType() As Type
   ReadOnly Property Parent() As SoundbridgeObject

   Function BuildList(ByVal listData As String()) As IList

End Interface

Public MustInherit Class SoundbridgeListCacheProvider(Of T As SoundbridgeListObject)
   Implements Pixa.Soundbridge.Library.ISoundbridgeListCacheProvider

   Private _soundbridge As Soundbridge
   Private _parent As SoundbridgeObject
   Private _cache As New SortedList(Of String, T)

   Public Sub New(ByVal soundbridge As Soundbridge, ByVal parent As SoundbridgeObject)
      _parent = parent
      _soundbridge = soundbridge
   End Sub

   Public ReadOnly Property ChildType() As Type Implements ISoundbridgeListCacheProvider.ChildType
      Get
         Return GetType(T)
      End Get
   End Property

   Public ReadOnly Property Parent() As SoundbridgeObject Implements ISoundbridgeListCacheProvider.Parent
      Get
         Return _parent
      End Get
   End Property

   Public ReadOnly Property Soundbridge() As Soundbridge
      Get
         Return _soundbridge
      End Get
   End Property

   Public Function BuildList(ByVal listData As String()) As SoundbridgeObjectCollection(Of T)
      'Remove from the cache any elements not in listData
      Dim sortedListData As String() = listData.Clone()
      Array.Sort(sortedListData)

      Dim listI As Integer = 0
      Dim cacheI As Integer = 0

      While listI < sortedListData.Length And cacheI < _cache.Count
         Dim cache As String = _cache.Keys(cacheI)
         Dim comparison As Integer = String.Compare(_cache.Keys(cacheI), sortedListData(listI))

         If comparison >= 0 Then
            'Either the strings match, or there's a new element in listData
            'So we need to move on to the next list item
            listI += 1
         End If

         If comparison < 0 Then
            'There's an element in the cache that's not in listdata, it needs
            'to be removed, but we don't always want to remove cached objects
            'i.e. A connected media server may not appear in ListServers if it's
            'filtered out.
            'If we don't remove the item, move onto the next element in the cache
            If Not Remove(cacheI) Then
               cacheI += 1
            End If
         ElseIf comparison = 0 Then
            'The strings match, so move on to the next element in the cache.
            cacheI += 1
         End If
      End While

      'Clear out the rest of the items in the cache, bearing in mind that we
      'don't always want to remove an element.
      While cacheI < _cache.Count
         If Not Remove(cacheI) Then
            cacheI += 1
         End If
      End While

      'Build the list, drawing existing elements from listData and creating new elements
      Dim l As SoundbridgeObjectCollection(Of T) = CreateCollection()

      For i As Integer = 0 To listData.Length - 1
         Dim s As String = listData(i)

         'Use the cache if we've seen the element before, otherwise create a new
         'object.  We may need to update the item's index as this is a new list.
         Dim o As T

         If _cache.ContainsKey(s) Then
            o = _cache(s)
            o.Index = i
            l.Add(o)
         Else
            o = CreateObject(s, i)

            If o IsNot Nothing Then
               _cache.Add(s, o)
               l.Add(o)
            End If
         End If
      Next

      Return l
   End Function

   Private Function Remove(ByVal index As Integer) As Boolean
      Dim o As T = _cache.Values(index)

      If o.ShouldCacheDispose Then
         o.Dispose()
         _cache.RemoveAt(index)
         Return True
      Else
         Return False
      End If
   End Function

   Private Function IBuildList(ByVal listData As String()) As IList Implements ISoundbridgeListCacheProvider.BuildList
      Return BuildList(listData)
   End Function

   Protected Overridable Function CreateCollection() As SoundbridgeObjectCollection(Of T)
      Return New SoundbridgeObjectCollection(Of T)(_soundbridge)
   End Function

   Protected MustOverride Function CreateObject(ByVal elementData As String, ByVal index As Integer) As T

End Class

Friend Class SoundbridgeMediaServerCacheProvider
   Inherits Pixa.Soundbridge.Library.SoundbridgeListCacheProvider(Of MediaServer)

   Public Sub New(ByVal soundbridge As Soundbridge, ByVal parent As SoundbridgeObject)
      MyBase.New(soundbridge, parent)
   End Sub

   Protected Overrides Function CreateObject(ByVal elementData As String, ByVal index As Integer) As MediaServer
      Dim tokens As String() = elementData.Split(" ", 3I, StringSplitOptions.None)
      Return New MediaServer(Soundbridge, ServerListAvailabilityToMediaServerAvailability(tokens(0)), ServerListTypeToMediaServerType(tokens(1)), tokens(2), index)
   End Function

   ''' <summary>
   ''' Converts the specified string value into a <see cref="MediaServerAvailability"/>
   ''' value.
   ''' </summary>
   Private Function ServerListAvailabilityToMediaServerAvailability(ByVal value As String) As MediaServerAvailability
      Select Case value
         Case "kOnline"
            Return MediaServerAvailability.Online

         Case "kOffline"
            Return MediaServerAvailability.Offline

         Case "kHidden"
            Return MediaServerAvailability.Hidden

         Case "kInaccessible"
            Return MediaServerAvailability.Inaccessible

      End Select
   End Function

   ''' <summary>
   ''' Converts the specified value into a <see cref="MediaServerType"/>.
   ''' </summary>
   Private Function ServerListTypeToMediaServerType(ByVal value As String) As MediaServerType
      Select Case value
         Case "kITunes"
            Return MediaServerType.Daap

         Case "kUPnP"
            Return MediaServerType.Upnp

         Case "kSlim"
            Return MediaServerType.Slim

         Case "kFlash"
            Return MediaServerType.Flash

         Case "kFavoriteRadio"
            Return MediaServerType.Radio

         Case "kAMTuner"
            Return MediaServerType.AM

         Case "kFMTuner"
            Return MediaServerType.FM

         Case "kRSP"
            Return MediaServerType.Rsp

         Case "kLinein"
            Return MediaServerType.LineIn

         Case Else
            Return -1
      End Select
   End Function
End Class

Friend Class MediaContainerMediaContainerCacheProvider
   Inherits Pixa.Soundbridge.Library.SoundbridgeListCacheProvider(Of MediaContainer)

   Private _listIsContainers As Boolean

   Public Sub New(ByVal soundbridge As Soundbridge, ByVal parent As MediaContainer)
      MyBase.New(soundbridge, parent)
   End Sub

   Protected Overrides Function CreateObject(ByVal elementData As String, ByVal index As Integer) As MediaContainer
      If index = 0 Then
         Dim l As String() = Soundbridge.Client.GetSongInfo(index)
         _listIsContainers = Array.IndexOf(l, "format: unsupported") >= 0
      End If

      If _listIsContainers Then
         Return New MediaContainer(DirectCast(Parent, MediaContainer), index, elementData)
      Else
         Return Nothing
      End If
   End Function
End Class

Friend Class MediaContainerSongCacheProvider
   Inherits Pixa.Soundbridge.Library.SoundbridgeListCacheProvider(Of Song)

   Private _listIsSongs As Boolean

   Public Sub New(ByVal soundbridge As Soundbridge, ByVal parent As MediaContainer)
      MyBase.New(soundbridge, parent)
   End Sub

   Protected Overrides Function CreateCollection() As SoundbridgeObjectCollection(Of Song)
      Return New SongCollection(Soundbridge)
   End Function

   Protected Overrides Function CreateObject(ByVal elementData As String, ByVal index As Integer) As Song
      If index = 0 Then
         Dim l As String() = Soundbridge.Client.GetSongInfo(index)
         _listIsSongs = Array.IndexOf(l, "format: unsupported") < 0
      End If

      If _listIsSongs Then
         Return New Song(DirectCast(Parent, MediaContainer), index, elementData)
      Else
         Return Nothing
      End If
   End Function
End Class