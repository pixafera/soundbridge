Public Class WirelessNetwork
   Inherits Pixa.Soundbridge.Library.SoundbridgeListObject

    Private _connected As Boolean
    Private _selected As Boolean

    Public Event SelectedChanged As EventHandler

#Region " Classes "
    Public Class SignalQuality

        Private _time As DateTime
        Private _quality As Integer
        Private _signal As Integer
        Private _noise As Integer

        Public Sub New(ByVal quality As Integer, ByVal signal As Integer, ByVal noise As Integer)
            _time = DateTime.Now
            _quality = quality
            _signal = signal
            _noise = noise
        End Sub

        Public ReadOnly Property Noise() As Integer
            Get
                Return _noise
            End Get
        End Property

        Public ReadOnly Property Quality() As Integer
            Get
                Return _quality
            End Get
        End Property

        Public ReadOnly Property Signal() As Integer
            Get
                Return _signal
            End Get
        End Property

        Public ReadOnly Property Time() As DateTime
            Get
                Return _time
            End Get
        End Property
    End Class
#End Region

    Friend Sub New(ByVal client As ISoundbridgeClient, ByVal index As Integer, ByVal name As String, ByVal connected As Boolean, ByVal selected As Boolean)
      MyBase.New(client, index, name)
    End Sub

    Public ReadOnly Property Connected() As Boolean
        Get
            Return _connected
        End Get
    End Property

    Public Property Selected() As Boolean
        Get
            Return _selected
        End Get
        Friend Set(ByVal value As Boolean)
            _selected = value
        End Set
    End Property

    Public Function GetSignalQuality() As SignalQuality
        If Not Connected Then Throw New InvalidOperationException("Can't get the signal quality when the soundbridge isn't connected to the network")

        Dim quality As Integer
        Dim signal As Integer
        Dim noise As Integer

        For Each s As String In Client.GetWiFiSignalQuality
            If s = "OK" Then Continue For

            Dim iNumberLeft As Integer = s.Length - 1

            While Char.IsNumber(s, iNumberLeft) Or s(iNumberLeft) = "-"
                iNumberLeft -= 1
            End While

            Dim sNumber As Integer = s.Substring(iNumberLeft + 1)
            Dim i As Integer

            If Integer.TryParse(sNumber, i) Then
                If s.StartsWith("quality") Then
                    quality = i
                ElseIf s.StartsWith("signal") Then
                    signal = i
                ElseIf s.StartsWith("noise") Then
                    noise = i
                End If
            End If
        Next

        Return New SignalQuality(quality, signal, noise)
    End Function

    Public Sub [Select](ByVal password As String)
        Dim r As String

        r = Client.SetWiFiNetworkSelection(Index)
        If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetWiFiNetworkSelection", r)

        If password <> "" Then
            r = Client.SetWiFiPassword(password)
            If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetWiFiPassword", r)
        End If
    End Sub
End Class
