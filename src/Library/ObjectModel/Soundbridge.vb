Imports System.Globalization
Imports System.Net
Imports System.Reflection
Imports System.Text.RegularExpressions

''' <summary>
''' Represents a Soundbridge and provides an object-oriented API to communicate
''' with it.
''' </summary>
''' <remarks></remarks>
Public Class Soundbridge
   Inherits Pixa.Soundbridge.Library.SoundbridgeObject

   ''' <summary>
   ''' Initialises a new instance of <see cref="Soundbridge"/>.
   ''' </summary>
   ''' <param name="client">The <see cref="ISoundbridgeClient"/> to use.</param>
   ''' <remarks></remarks>
   Public Sub New(ByVal client As ISoundbridgeClient)
      MyBase.New(client)

      AddHandler client.AwaitingReply, AddressOf Client_AwaitingReply
      AddHandler client.ReceivingData, AddressOf Client_ReceivingData
      AddHandler client.SendingRequest, AddressOf Client_SendingRequest
      AddHandler client.IRKeyDown, AddressOf Client_IRKeyDown
      AddHandler client.IRKeyUp, AddressOf Client_IRKeyUp

      If client.GetProgressMode = ProgressMode.Off Then client.SetProgressMode(ProgressMode.Verbose)

      Cache.RegisterCache(New SoundbridgeMediaServerCacheProvider(Me, Me))
   End Sub

#Region " Connection "
   Public Sub Close()
      Dim c As IDisposable = TryCast(Client, IDisposable)

      If c IsNot Nothing Then
         c.Dispose()
      End If
   End Sub

   Protected Overrides Sub Dispose(ByVal disposing As Boolean)
      MyBase.Dispose(disposing)

      If disposing Then
         Cache.DeregisterCache(Me, GetType(MediaServer))
         Close()
      End If
   End Sub
#End Region

#Region " Byte Arrays "
   'Converts a binary response into a Byte array.
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

   ''' <summary>
   ''' Gets and sets the active list.
   ''' </summary>
   ''' <value>The most recently requested list.</value>
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

   ''' <summary>
   ''' Gets the <see cref="MediaServer"/> that the soundbridge is currently
   ''' connected to.
   ''' </summary>
   ''' <value></value>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Public Property ConnectedServer() As MediaServer
      Get
         Return _connectedServer
      End Get
      Friend Set(ByVal value As MediaServer)
         _connectedServer = value
      End Set
   End Property

   ''' <summary>
   ''' Gets a list of <see cref="MediaServer"/>s that the <see cref="Soundbridge"/>
   ''' can connect to.
   ''' </summary>
   ''' <returns>The list of available <see cref="MediaServer"/>s.</returns>
   ''' <remarks>This method updates the active list.</remarks>
   Public Function GetServers() As SoundbridgeObjectCollection(Of MediaServer)
      Return GetServers(MediaServerType.All)
   End Function

   ''' <summary>
   ''' Gets a list of <see cref="MediaServer"/>s that the <see cref="Soundbridge"/>
   ''' can connect to matching the specified criteria.
   ''' </summary>
   ''' <param name="filter">The filter to apply to the list.</param>
   ''' <returns>The list of available <see cref="MediaServer"/>s matching the
   ''' specified <paramref name="filter"/>.</returns>
   ''' <remarks>This method updates the active list.</remarks>
   Public Function GetServers(ByVal filter As MediaServerType) As SoundbridgeObjectCollection(Of MediaServer)
      'The filter string is always set to debug, so we always get the additional information
      Dim filterString As String = GetFilterString(filter)
      Dim r As String = Client.SetServerFilter(filterString)
      If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetServerFilter", r)

      Dim servers As String() = Client.ListServers
      If servers.Length = 1 AndAlso (servers(0) = "ErrorInitialSetupRequired" Or servers(0) = "GenericError") Then ExceptionHelper.ThrowCommandReturnError("ListServers", servers(0))

      ActiveList = Cache.BuildList(Of MediaServer)(Me, servers)
      Return ActiveList
   End Function

   ''' <summary>
   ''' Converts the specified <see cref="MediaServerType"/> value into a string
   ''' for <see cref="ISoundbridgeClient.SetServerFilter"/>.
   ''' </summary>
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

   ''' <summary>
   ''' Disconnects the <see cref="Soundbridge"/> from the media server it is
   ''' currently connected to.
   ''' </summary>
   Public Sub DisconnectServer()
      Dim s As String = Client.ServerDisconnect()

      If s = "Disconnected" Then
         _connectedServer = Nothing
      End If
   End Sub
#End Region

#Region " Progress "
   ''' <summary>
   ''' Raised when the <see cref="Soundbridge"/> is waiting for a reply.
   ''' </summary>
   Public Event AwaitingReply As EventHandler(Of RcpCommandProgressEventArgs)

   ''' <summary>
   ''' Raised when the <see cref="Soundbridge"/> is receiving data.
   ''' </summary>
   Public Event ReceivingData As EventHandler(Of RcpCommandReceivingProgressEventArgs)

   ''' <summary>
   ''' Raised when the <see cref="Soundbridge"/> is sending a request.
   ''' </summary>
   Public Event SendingRequest As EventHandler(Of RcpCommandProgressEventArgs)

   ''' <summary>
   ''' Raises the <see cref="AwaitingReply"/> event.
   ''' </summary>
   ''' <param name="e">The event data.</param>
   ''' <remarks>Subclasses overriding this method should call the base class
   ''' method to ensure that the event gets raised.</remarks>
   Protected Overridable Sub OnAwaitingReply(ByVal e As RcpCommandProgressEventArgs)
      RaiseEvent AwaitingReply(Me, e)
   End Sub

   ''' <summary>
   ''' Raises the <see cref="ReceivingData"/> event.
   ''' </summary>
   ''' <param name="e">The event data.</param>
   ''' <remarks>Subclasses overriding this method should call the base class
   ''' method to ensure that the event gets raised.</remarks>
   Protected Overridable Sub OnReceivingData(ByVal e As RcpCommandReceivingProgressEventArgs)
      RaiseEvent ReceivingData(Me, e)
   End Sub

   ''' <summary>
   ''' Raises the <see cref="SendingRequest"/> event.
   ''' </summary>
   ''' <param name="e">The event data.</param>
   ''' <remarks>Subclasses overriding this method should call the base class
   ''' method to ensure that the event gets raised.</remarks>
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
   ''' <summary>
   ''' Gets a value indicating whether the <see cref="Soundbridge"/> has
   ''' completed initial setup.
   ''' </summary>
   ''' <value>True if the Soundbridge has completed initial setup; otherwise,
   ''' false.</value>
   Public ReadOnly Property InitialSetupComplete() As Boolean
      Get
         Return Client.GetInitialSetupComplete = "Complete"
      End Get
   End Property

   Private _setupSteps As SetupStepCollection

   ''' <summary>
   ''' Gets the list of initial setup steps that must be completed.
   ''' </summary>
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

   ''' <summary>
   ''' Gets the Upgrade MAC address of the <see cref="Soundbridge"/>.
   ''' </summary>
   Public ReadOnly Property UpgradeMac() As String
      Get
         Return Client.GetMacAddress("upgr")
      End Get
   End Property

   ''' <summary>
   ''' Gets and sets the date and time according to the <see cref="Soundbridge"/>.
   ''' </summary>
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

   ''' <summary>
   ''' Gets the firmware version of the <see cref="Soundbridge"/>.
   ''' </summary>
   Public ReadOnly Property SoftwareVersion() As Version
      Get
         Return New Version(Client.GetSoftwareVersion)
      End Get
   End Property

   ''' <summary>
   ''' Reboots the <see cref="Soundbridge"/>.
   ''' </summary>
   ''' <remarks></remarks>
   Public Sub Reboot()
      Client.Reboot()
   End Sub

   ''' <summary>
   ''' Gets and sets of the <see cref="PowerState"/> of the <see cref="Soundbridge"/>.
   ''' </summary>
   Public Property PowerState() As PowerState
      Get
         Return GetPowerStateValue(Client.GetPowerState)
      End Get
      Set(ByVal value As PowerState)
         If value <> PowerState Then
            Client.SetPowerState(GetPowerStateString(value), False)
         End If
      End Set
   End Property

   'Converts a string value to a PowerState value.
   Private Function GetPowerStateValue(ByVal value As String) As PowerState
      Select Case value
         Case "standby"
            Return Library.PowerState.Standby

         Case "on"
            Return Library.PowerState.On

         Case Else
            Throw New ArgumentException("value must be 'standby' or 'on'", "value")
      End Select
   End Function

   'Converts a power state value to a string.
   Private Function GetPowerStateString(ByVal value As PowerState) As String
      Select Case value
         Case Library.PowerState.Standby
            Return "standby"

         Case Library.PowerState.On
            Return "on"

         Case Else
            Throw New ArgumentException("value must be a valid PowerState value", "value")
      End Select
   End Function
#End Region

#Region " Display "
   Private _display As New SoundbridgeDisplay(Me)

   ''' <summary>
   ''' Gets the <see cref="SoundbridgeDisplay"/> object that can be used to
   ''' interact with the display of the soundbridge.
   ''' </summary>
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

   ''' <summary>
   ''' Converts an <see cref="IRCommand"/> value into a string to send to the
   ''' <see cref="Soundbridge"/>.
   ''' </summary>
   ''' <param name="command">The value to convert.</param>
   ''' <returns>A string value representing the same button as <paramref name="command"/>.</returns>
   Public Shared Function GetIrCommandTranslation(ByVal command As IRCommand) As String
      SyncLock _irCommandTranslationsLock
         If _irCommandEnumTranslations Is Nothing Then
            BuildTranslationDictionaries()
         End If

         Return _irCommandEnumTranslations(command)
      End SyncLock
   End Function

   ''' <summary>
   ''' Converts a string value from the <see  cref="Soundbridge"/> into an <see cref="IRCommand"/>
   ''' value.
   ''' </summary>
   ''' <param name="command">The value to convert.</param>
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

   ''' <summary>
   ''' Raised when a key on the IR remote is pressed.
   ''' </summary>
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

   ''' <summary>
   ''' Raised when a key on the IR remote is depressed.
   ''' </summary>
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

   ''' <summary>
   ''' Dispatches the specified <see cref="IRCommand"/> to the <see cref="Soundbridge"/>.
   ''' </summary>
   ''' <param name="command">The <see cref="IRCommand"/> to execute.</param>
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

#Region " Cache "
   Private _cache As New SoundbridgeCache

   Friend ReadOnly Property Cache() As SoundbridgeCache
      Get
         Return _cache
      End Get
   End Property
#End Region

End Class
