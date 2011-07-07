Public Class TimeZoneSetupStep
    Inherits Pixa.Soundbridge.Library.SetupStep

    Friend Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

    Protected Overrides ReadOnly Property Name() As String
        Get
            Return "Time Zone"
        End Get
    End Property

    Public Overrides Function GetSelectionList() As String()
        Return Client.ListTimeZones
    End Function

    Public Overrides Sub MakeSelection(ByVal value As String)
        Dim timeZones As String() = Client.ListTimeZones
        Dim index As Integer = Array.IndexOf(timeZones, value)

        If index = -1 Then Throw New Exception(String.Format("Couldn't find Time Zone '{0}' in the list from the Soundbridge", value))

        Dim r As String = Client.SetTimeZone(index)

        If r = "OK" Then Exit Sub
        If r = "ParameterError" Then Throw New ArgumentException("Error setting the Time Zone", "value")
        If r = "GenericError" Then Throw New Exception("Soundbridge returned GenericError")
    End Sub
End Class
