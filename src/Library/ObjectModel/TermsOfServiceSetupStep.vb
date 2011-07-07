Public Class TermsOfServiceSetupStep
    Inherits Pixa.Soundbridge.Library.SetupStep

    Friend Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

    Protected Overrides ReadOnly Property Name() As String
        Get
            Return "Terms Of Service"
        End Get
    End Property

    Public Overrides Function GetSelectionList() As String()
        Return New String() {Client.GetTermsOfServiceUrl}
    End Function

    ''' <summary>
    ''' Accepts the Terms of Service
    ''' </summary>
    ''' <param name="value">This parameter is ignored, you can pass nothing or an empty string to it.</param>
    ''' <remarks></remarks>
    Public Overrides Sub MakeSelection(ByVal value As String)
        Client.AcceptTermsOfService()
    End Sub
End Class
