Public Class RegionSetupStep
    Inherits Pixa.Soundbridge.Library.SetupStep

    Friend Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

    Protected Overrides ReadOnly Property Name() As String
        Get
            Return "Region"
        End Get
    End Property

    Public Overrides Function GetSelectionList() As String()
        Return Client.ListRegions()
    End Function

    Public Overrides Sub MakeSelection(ByVal value As String)
        Dim regions As String() = Client.ListRegions
        Dim index As Integer = Array.IndexOf(regions, value)

        If index = -1 Then Throw New ArgumentException("Specified Region was not in the list of valid regions")

        Dim r As String = Client.SetRegion(index)

        If r = "OK" Then Exit Sub
        If r = "ParameterError" Then Throw New ArgumentException("Invalid index or list results", value)
        If r = "ErrorUnsupported" Then Throw New ArgumentException("The specified region is not supported on this host", value)
        If r = "ErrorAlreadySet" Then Throw New ArgumentException("The region has already been set", value)
        If r = "GenericError" Then Throw New ArgumentException("Unable to set region", value)
    End Sub
End Class
