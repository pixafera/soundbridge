Public NotInheritable Class SoundbridgeDisplay
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Private _sb As Soundbridge
    Private _update As EventHandler(Of DisplayUpdateEventArgs)

    Public Custom Event Update As EventHandler(Of DisplayUpdateEventArgs)
        AddHandler(ByVal value As EventHandler(Of DisplayUpdateEventArgs))
            _update = [Delegate].Combine(_update, value)

            If _update IsNot Nothing Then Client.DisplayUpdateEventSubscribe()
        End AddHandler

        RemoveHandler(ByVal value As EventHandler(Of DisplayUpdateEventArgs))
            _update = [Delegate].Remove(_update, value)

            If _update Is Nothing Then Client.DisplayUpdateEventUnsubscribe()
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As SoundbridgeEventArgs)
            _update(sender, e)
        End RaiseEvent
    End Event

    Friend Sub New(ByVal sb As Soundbridge)
        MyBase.New(sb)
        _sb = sb

        AddHandler Client.DisplayUpdate, AddressOf Client_DisplayUpdate
        Client.DisplayUpdateEventSubscribe()
    End Sub

    Public ReadOnly Property SupportsVisualizers() As Boolean
        Get
            Return Client.GetVisualizer(False) <> "ErrorUnsupported"
        End Get
    End Property

    Public Property VisualizerMode() As VisualizerMode
        Get
            Return StringToVisualizerMode(Client.GetVisualizerMode)
        End Get
        Set(ByVal value As VisualizerMode)
            Client.SetVisualizerMode(VisualizerModeToString(value))
        End Set
    End Property

    Public Property VisualizerName() As String
        Get
            Dim r As String = Client.GetVisualizer(False)

            If r = "ErrorUnsupported" Then ExceptionHelper.ThrowCommandReturnError("GetVisualizer", r)

            Return r
        End Get
        Set(ByVal value As String)
            Dim r As String = Client.SetVisualizer(value)

            If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetVisualizer", r)
        End Set
    End Property

    Public ReadOnly Property VisualizerFriendlyName() As String
        Get
            Dim r As String = Client.GetVisualizer(True)

            If r = "ErrorUnsupported" Then ExceptionHelper.ThrowCommandReturnError("GetVisualizer", r)

            Return r
        End Get
    End Property

    Public Function GetVizDataVU() As Byte()
        Dim r As String = Client.GetVizDataVU
        If r = "OK" Then Return New Byte() {}
        Return Soundbridge.ResponseToByteArray(r)
    End Function

    Public Function GetVizDataFreq() As Byte()
        Dim r As String = Client.GetVizDataFreq
        If r = "OK" Then Return New Byte() {}
        Return Soundbridge.ResponseToByteArray(r)
    End Function

    Public Function GetVizDataScope() As Byte()
        Dim r As String = Client.GetVizDataScope
        If r = "OK" Then Return New Byte() {}
        Return Soundbridge.ResponseToByteArray(r)
    End Function

    Public Function GetDisplayData(ByRef textualData As String, ByVal byteData As Byte()) As Boolean
        Dim isByte As Boolean
        Dim r As String = Client.GetDisplayData(isByte)

        If isByte Then
            byteData = Soundbridge.ResponseToByteArray(r)
            Return True
        Else
            textualData = r
            Return False
        End If
    End Function

    Private Function VisualizerModeToString(ByVal value As VisualizerMode) As String
        Select Case value
            Case Library.VisualizerMode.Full
                Return "full"

            Case Library.VisualizerMode.Off
                Return "off"

            Case Library.VisualizerMode.Partial
                Return "partial"

            Case Else
                Return ""
        End Select
    End Function

    Private Function StringToVisualizerMode(ByVal value As String) As VisualizerMode
        Select Case value
            Case "full"
                Return Library.VisualizerMode.Full

            Case "off"
                Return Library.VisualizerMode.Off

            Case "partial"
                Return Library.VisualizerMode.Partial

            Case Else
                Return ""
        End Select
    End Function

    Private Sub Client_DisplayUpdate(ByVal data As String)
        Dim iData As Integer

        If Integer.TryParse(data, iData) Then RaiseEvent Update(Me, New DisplayUpdateEventArgs(_sb, iData))
    End Sub
End Class

Public Enum VisualizerMode
    Full
    [Partial]
    Off
End Enum