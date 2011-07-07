Imports System.Net
Imports System.Net.NetworkInformation

Public Class NetworkAdapter
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Private _status As NetworkAdapterStatus
    Private _type As NetworkAdapterType
    Private _macAddress As PhysicalAddress
    Private _ipAddress As IPAddress

    Friend Sub New(ByVal obj As SoundbridgeObject, ByVal type As NetworkAdapterType)
        MyBase.New(obj)
        Initialize(type)
    End Sub

    Friend Sub New(ByVal client As ISoundbridgeClient, ByVal type As NetworkAdapterType)
        MyBase.New(client)
        Initialize(type)
    End Sub

    Private Sub Initialize(ByVal type As NetworkAdapterType)
        Dim code As String = NetworkAdapterTypeToCode(type)

        _status = StringToNetworkAdapterStatus(Client.GetLinkStatus(code))

        If _status <> NetworkAdapterStatus.NotFound Then
            _ipAddress = IPAddress.Parse(Client.GetIPAddress(code))
            _macAddress = PhysicalAddress.Parse(Client.GetMacAddress(code))
        End If

        _type = type
    End Sub

    Public ReadOnly Property IPAddress() As IPAddress
        Get
            Return _ipAddress
        End Get
    End Property

    Public ReadOnly Property MacAddress() As PhysicalAddress
        Get
            Return _macAddress
        End Get
    End Property

    Public ReadOnly Property Status() As NetworkAdapterStatus
        Get
            Return _status
        End Get
    End Property

    Public ReadOnly Property Type() As NetworkAdapterType
        Get
            Return _type
        End Get
    End Property

    Private Function NetworkAdapterTypeToCode(ByVal type As NetworkAdapterType) As String
        Select Case type
            Case NetworkAdapterType.Ethernet
                Return "enet"

            Case NetworkAdapterType.Loopback
                Return "loop"

            Case NetworkAdapterType.Wireless
                Return "wlan"

            Case Else
                Return ""
        End Select
    End Function

    Private Function StringToNetworkAdapterStatus(ByVal value As String) As NetworkAdapterStatus
        Select Case value
            Case "Link"
                Return NetworkAdapterStatus.Link

            Case "NoLink"
                Return NetworkAdapterStatus.NoLink

            Case "ErrorNotFound"
                Return NetworkAdapterStatus.NotFound

            Case Else
                ThrowCommandReturnError("GetLinkStatus", value)
        End Select
    End Function
End Class

Public Class NetworkAdapterCollection
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject
    Implements System.Collections.Generic.ICollection(Of NetworkAdapter)

    Private _ethernet As NetworkAdapter
    Private _wireless As NetworkAdapter
    Private _loopback As NetworkAdapter
    Private _soundbridge As Soundbridge

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
        _soundbridge = sb
        _loopback = New NetworkAdapter(sb, NetworkAdapterType.Loopback)
        _ethernet = New NetworkAdapter(sb, NetworkAdapterType.Ethernet)
        _wireless = New NetworkAdapter(sb, NetworkAdapterType.Wireless)
    End Sub

    Public ReadOnly Property Soundbridge() As Soundbridge
        Get
            Return _soundbridge
        End Get
    End Property

#Region " Item "
    Default Public ReadOnly Property Item(ByVal adapter As NetworkAdapterType) As NetworkAdapter
        Get
            Select Case adapter
                Case NetworkAdapterType.Ethernet
                    Return _ethernet

                Case NetworkAdapterType.Loopback
                    Return _loopback

                Case NetworkAdapterType.Wireless
                    Return _wireless

                Case Else
                    Throw New ArgumentOutOfRangeException("Only valid NetworkAdapterTypes are accepted")
            End Select
        End Get
    End Property
#End Region

#Region " Wireless "
    Private _lastWifiNetworkRefresh As DateTime
    Private _wirelessNetworks As WirelessNetworkCollection

    Public ReadOnly Property WirelessNetworks() As WirelessNetworkCollection
        Get
            If Not _wirelessNetworks.IsActive OrElse (DateTime.Now - _lastWifiNetworkRefresh).TotalSeconds > 30 Then
                If _wirelessNetworks IsNot Nothing Then
                    For Each wn As WirelessNetwork In _wirelessNetworks
                        RemoveHandler wn.SelectedChanged, AddressOf WirelessNetwork_SelectedChanged
                    Next
                End If

                Dim networks() As String = Client.ListWiFiNetworks

                If networks.Length > 0 AndAlso (networks(0) = "ErrorInitialSetupRequired" OrElse networks(0) = "ErrorNoWiFiInterfaceFound" OrElse networks(0) = "ErrorWiFiInterfaceDisabled") Then ExceptionHelper.ThrowCommandReturnError("ListWiFiNetworks", networks(0))

                _wirelessNetworks = New WirelessNetworkCollection(Soundbridge)

                Dim selectedNetwork As String = Client.GetWiFiNetworkSelection
                Dim connectedNetwork As String = Client.GetConnectedWiFiNetwork

                For i As Integer = 0 To networks.Length - 1
                    Dim network As String = networks(i)
                    _wirelessNetworks.Add(New WirelessNetwork(Client, i, network, network = connectedNetwork, network = selectedNetwork))
                Next

                Soundbridge.ActiveList = _wirelessNetworks
            End If

            Return _wirelessNetworks
        End Get
    End Property

    Private Sub WirelessNetwork_SelectedChanged(ByVal sender As Object, ByVal e As EventArgs)
        For Each wn As WirelessNetwork In _wirelessNetworks
            If wn IsNot sender Then wn.Selected = False
        Next
    End Sub
#End Region

#Region " Not Supported "
    Private Sub Add(ByVal item As NetworkAdapter) Implements System.Collections.Generic.ICollection(Of NetworkAdapter).Add
        Throw New NotSupportedException("Cannot change this collection")
    End Sub

    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of NetworkAdapter).Clear
        Throw New NotSupportedException("Cannot change this collection")
    End Sub

    Public Function Remove(ByVal item As NetworkAdapter) As Boolean Implements System.Collections.Generic.ICollection(Of NetworkAdapter).Remove
        Throw New NotSupportedException("Connect change this collection")
    End Function
#End Region

#Region " ICollection "
    Public Function Contains(ByVal item As NetworkAdapter) As Boolean Implements System.Collections.Generic.ICollection(Of NetworkAdapter).Contains
        Return item IsNot Nothing AndAlso (item Is _ethernet Or item Is _wireless Or item Is _loopback)
    End Function

    Public Sub CopyTo(ByVal array() As NetworkAdapter, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of NetworkAdapter).CopyTo
        Dim i As Integer = 0

        If _loopback IsNot Nothing Then
            array(arrayIndex + i) = _loopback
            i += 1
        End If

        If _ethernet IsNot Nothing Then
            array(arrayIndex + i) = _ethernet
            i += 1
        End If

        If _wireless IsNot Nothing Then
            array(arrayIndex + i) = _wireless
            i += 1
        End If
    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of NetworkAdapter).Count
        Get
            Dim i As Integer

            If _loopback IsNot Nothing Then i += 1
            If _ethernet IsNot Nothing Then i += 1
            If _wireless IsNot Nothing Then i += 1

            Return i
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of NetworkAdapter).IsReadOnly
        Get
            Return True
        End Get
    End Property

    Public Function GetGenericEnumerator() As System.Collections.Generic.IEnumerator(Of NetworkAdapter) Implements System.Collections.Generic.IEnumerable(Of NetworkAdapter).GetEnumerator
        Return New NetworkAdapterCollectionEnumerator(Me)
    End Function

    Private Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetGenericEnumerator()
    End Function
#End Region

#Region " IEnumerator "
    Private Class NetworkAdapterCollectionEnumerator
        Implements System.Collections.Generic.IEnumerator(Of NetworkAdapter)

        Private _collection As NetworkAdapterCollection
        Private _type As NetworkAdapterType

        Public Sub New(ByVal c As NetworkAdapterCollection)
            _collection = c
        End Sub

#Region " Generic "

        Public ReadOnly Property Current() As NetworkAdapter Implements System.Collections.Generic.IEnumerator(Of NetworkAdapter).Current
            Get
                Return _collection(_type)
            End Get
        End Property

#End Region

        Public ReadOnly Property CurrentObject() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            _type += 1
            Return _type < 3
        End Function

        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            _type = 0
        End Sub

#Region " IDisposable Support "
        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
#End Region

End Class

Public Enum NetworkAdapterStatus
    Link
    NoLink
    NotFound
End Enum

Public Enum NetworkAdapterType
    Loopback = 0
    Ethernet = 1
    Wireless = 2
End Enum