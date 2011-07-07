Imports System.Globalization
Imports System.Net
Imports System.Reflection
Imports System.Text.RegularExpressions

Public Class Soundbridge
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Friend Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)

        AddHandler client.AwaitingReply, AddressOf Client_AwaitingReply
        AddHandler client.ReceivingData, AddressOf Client_ReceivingData
        AddHandler client.SendingRequest, AddressOf Client_SendingRequest
        AddHandler client.IRKeyDown, AddressOf Client_IRKeyDown
        AddHandler client.IRKeyUp, AddressOf Client_IRKeyUp

        If client.GetProgressMode = ProgressMode.Off Then client.SetProgressMode(ProgressMode.Verbose)
    End Sub

#Region " Byte Arrays "
    Friend Shared Function ResponseToByteArray(ByVal response As String) As Byte()
        If response.Length Mod 2 <> 0 Then Throw New ArgumentException("response must have an even length", "response")
        If Regex.IsMatch(response, "[^0-9a-fA-F]") Then Throw New ArgumentException("response can only contain digits and letters A-F", "response")

        Dim b(response.Length / 2) As Byte

        For i As Integer = 0 To response.Length / 2
            Byte.TryParse(response.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, b(i))
        Next

        Return b
    End Function
#End Region

#Region " Lists "
    Private _activeList As IList

    Friend Property ActiveList() As IList
        Get
            Return _activeList
        End Get
        Set(ByVal value As IList)
            _activeList = value
        End Set
    End Property
#End Region

#Region " Servers "
    Private _connectedServer As MediaServer

    Public Property ConnectedServer() As MediaServer
        Get
            Return _connectedServer
        End Get
        Friend Set(ByVal value As MediaServer)
            _connectedServer = value
        End Set
    End Property

    Public Function GetServers() As MediaServerCollection
        Return GetServers(MediaServerType.All)
    End Function

    Public Function GetServers(ByVal filter As MediaServerType) As MediaServerCollection
        Dim filterString As String = GetFilterString(filter)
        Dim r As String = Client.SetServerFilter(filterString)
        If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetServerFilter", r)

        Dim servers As String() = Client.ListServers
        If servers.Length = 1 AndAlso (servers(0) = "ErrorInitialSetupRequired" Or servers(0) = "GenericError") Then ExceptionHelper.ThrowCommandReturnError("ListServers", servers(0))

        Dim msc As New MediaServerCollection(Me)

        For i As Integer = 0 To servers.Length - 1
            Dim tokens As String() = servers(i).Split(" ", 3I, StringSplitOptions.None)
            msc.Add(New MediaServer(Me, ServerListAvailabilityToMediaServerAvailability(tokens(0)), ServerListTypeToMediaServerType(tokens(1)), tokens(2), i))
        Next

        Return msc
    End Function

    Private Function GetFilterString(ByVal value As MediaServerType) As String
        If value = MediaServerType.All Then Return "debug"

        Dim filterString As String = ""

        If value And MediaServerType.Daap Then filterString &= "daap "
        If value And MediaServerType.Upnp Then filterString &= "upnp "
        If value And MediaServerType.Rsp Then filterString &= "rsp "
        If value And MediaServerType.Slim Then filterString &= "slim "
        If value And MediaServerType.Radio Then filterString &= "radio "
        If value And MediaServerType.Flash Then filterString &= "flash "
        If value And MediaServerType.LineIn Then filterString &= "linein "
        If value And MediaServerType.AM Then filterString &= "am "
        If value And MediaServerType.FM Then filterString &= "fm "

        filterString &= "debug"
        Return filterString
    End Function

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

    Public Sub DisconnectServer()
        Client.ServerDisconnect()
    End Sub
#End Region

#Region " Progress "
    Public Event AwaitingReply As EventHandler(Of RcpCommandProgressEventArgs)
    Public Event ReceivingData As EventHandler(Of RcpCommandReceivingProgressEventArgs)
    Public Event SendingRequest As EventHandler(Of RcpCommandProgressEventArgs)

    Protected Overridable Sub OnAwaitingReply(ByVal e As RcpCommandProgressEventArgs)
        RaiseEvent AwaitingReply(Me, e)
    End Sub

    Protected Overridable Sub OnReceivingData(ByVal e As RcpCommandReceivingProgressEventArgs)
        RaiseEvent ReceivingData(Me, e)
    End Sub

    Protected Overridable Sub OnSendingRequest(ByVal e As RcpCommandProgressEventArgs)
        RaiseEvent SendingRequest(Me, e)
    End Sub

    Private Sub Client_AwaitingReply(ByVal sender As Object, ByVal e As RcpCommandProgressEventArgs)
        OnAwaitingReply(e)
    End Sub

    Private Sub Client_ReceivingData(ByVal sender As Object, ByVal e As RcpCommandReceivingProgressEventArgs)
        OnReceivingData(e)
    End Sub

    Private Sub Client_SendingRequest(ByVal sender As Object, ByVal e As RcpCommandProgressEventArgs)
        OnSendingRequest(e)
    End Sub
#End Region

#Region " Setup & Config "
    Public ReadOnly Property InitialSetupComplete() As Boolean
        Get
            Return Client.GetInitialSetupComplete = "Complete"
        End Get
    End Property

    Private _setupSteps As SetupStepCollection

    Public ReadOnly Property SetupSteps() As SetupStepCollection
        Get
            If InitialSetupComplete Then Throw New Exception("The Soundbridge has already been set up")
            If _setupSteps Is Nothing Then
                Dim f As New SetupStepFactory(Client)
                Dim l As New List(Of SetupStep)

                For Each s As String In Client.GetRequiredSetupSteps
                    l.Add(f.CreateSetupStep(s))
                Next

                _setupSteps = New SetupStepCollection(l)
            End If

            Return _setupSteps
        End Get
    End Property

    Public ReadOnly Property UpgradeMac() As String
        Get
            Return Client.GetMacAddress("upgr")
        End Get
    End Property

    Public Property LocalTime() As DateTime
        Get
            Dim d As String = Client.GetDate(False)
            Dim t As String = Client.GetTime(False)

            Return DateTime.Parse(d & " " & t)
        End Get
        Set(ByVal value As DateTime)
            Client.SetTime(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
            Client.SetDate(value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture))
        End Set
    End Property

    Public ReadOnly Property SoftwareVersion() As Version
        Get
            Return New Version(Client.GetSoftwareVersion)
        End Get
    End Property

    Public Sub Reboot()
        Client.Reboot()
    End Sub
#End Region

#Region " Display "
    Private _display As New SoundbridgeDisplay(Me)

    Public ReadOnly Property Display() As SoundbridgeDisplay
        Get
            Return _display
        End Get
    End Property
#End Region

#Region " IR "
    Private _irKeyDown As EventHandler(Of IRKeyEventArgs)
    Private _irKeyUp As EventHandler(Of IRKeyEventArgs)
    Private Shared _irCommandEnumTranslations As Dictionary(Of IRCommand, String)
    Private Shared _irCommandStringTranslations As Dictionary(Of String, IRCommand)
    Private Shared _irCommandTranslationsLock As New Object

    Public Shared Function GetIrCommandTranslation(ByVal command As IRCommand) As String
        SyncLock _irCommandTranslationsLock
            If _irCommandEnumTranslations Is Nothing Then
                BuildTranslationDictionaries()
            End If

            Return _irCommandEnumTranslations(command)
        End SyncLock
    End Function

    Public Shared Function GetIrCommandTransaction(ByVal command As String) As IRCommand
        SyncLock _irCommandTranslationsLock
            If _irCommandStringTranslations Is Nothing Then
                BuildTranslationDictionaries()
            End If

            Return _irCommandStringTranslations(command)
        End SyncLock
    End Function

    Private Shared Sub BuildTranslationDictionaries()
        Dim i As IRCommand

        _irCommandEnumTranslations = New Dictionary(Of IRCommand, String)
        _irCommandStringTranslations = New Dictionary(Of String, IRCommand)


        For Each f As FieldInfo In GetType(IRCommand).GetFields
            Dim atts As IRCommandStringAttribute() = f.GetCustomAttributes(GetType(IRCommandStringAttribute), False)

            If atts.Length = 1 Then
                _irCommandEnumTranslations.Add(f.GetValue(i), atts(0).CommandString)
                _irCommandStringTranslations.Add(atts(0).CommandString, f.GetValue(i))
            End If
        Next
    End Sub

    Public Custom Event IRKeyDown As EventHandler(Of IRKeyEventArgs)
        AddHandler(ByVal value As EventHandler(Of IRKeyEventArgs))
            _irKeyDown = [Delegate].Combine(_irKeyDown, value)

            If _irKeyDown IsNot Nothing Then Client.IRDemodSubscribe(True)
        End AddHandler

        RemoveHandler(ByVal value As EventHandler(Of IRKeyEventArgs))
            _irKeyDown = [Delegate].Remove(_irKeyDown, value)

            If _irKeyDown Is Nothing Then Client.IRDemodUnsubscribe()
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As IRKeyEventArgs)
            _irKeyDown(sender, e)
        End RaiseEvent
    End Event

    Public Custom Event IRKeyUp As EventHandler(Of IRKeyEventArgs)
        AddHandler(ByVal value As EventHandler(Of IRKeyEventArgs))
            _irKeyUp = [Delegate].Combine(_irKeyUp, value)

            If _irKeyUp IsNot Nothing Then Client.IRDemodSubscribe(True)
        End AddHandler

        RemoveHandler(ByVal value As EventHandler(Of IRKeyEventArgs))
            _irKeyUp = [Delegate].Remove(_irKeyUp, value)

            If _irKeyUp Is Nothing Then Client.IRDemodUnsubscribe()
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As IRKeyEventArgs)
            _irKeyUp(sender, e)
        End RaiseEvent
    End Event

    Public Sub DispatchIrCommand(ByVal command As IRCommand)
        Dim r As String = Client.IRDispatchCommand(GetIrCommandTranslation(command))

        If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("IrDispatchCommand", r)
    End Sub

    Private Sub Client_IRKeyDown(ByVal data As String)
        Dim e As New IRKeyEventArgs(Me, GetIrCommandTranslation(data))

        RaiseEvent IRKeyDown(Me, e)

        If Not e.IsHandled Then
            DispatchIrCommand(e.Command)
        End If
    End Sub

    Private Sub Client_IRKeyUp(ByVal data As String)
        Dim e As New IRKeyEventArgs(Me, GetIrCommandTranslation(data))

        RaiseEvent IRKeyUp(Me, e)
    End Sub
#End Region

End Class
