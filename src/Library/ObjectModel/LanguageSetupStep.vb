Public Class LanguageSetupStep
    Inherits Pixa.Soundbridge.Library.SetupStep

    Friend Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

    Protected Overrides ReadOnly Property Name() As String
        Get
            Return "Language"
        End Get
    End Property

    Public Overrides Function GetSelectionList() As String()
        Return Client.ListLanguages
    End Function

    Public Overrides Sub MakeSelection(ByVal value As String)
        Dim r As String = Client.SetLanguage(value)

        If r = "OK" Then Exit Sub
        If r = "ParameterError" Then Throw New ArgumentException("value was not a valid language", "value")
        If r = "GenericError" Then Throw New ArgumentException(String.Format("Couldn't set language to {0}", value), "value")
    End Sub
End Class
