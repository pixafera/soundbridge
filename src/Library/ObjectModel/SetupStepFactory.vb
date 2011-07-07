Public Class SetupStepFactory
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Public Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

    Public Function CreateSetupStep(ByVal [step] As String) As SetupStep
        Select Case [step]
            Case "Language"
                Return New LanguageSetupStep(Client)

            Case "TimeZone"
                Return New TimeZoneSetupStep(Client)

            Case "Region"
                Return New RegionSetupStep(Client)

            Case "TermsOfService"
                Return New TermsOfServiceSetupStep(Client)

            Case Else
                Return Nothing
        End Select
    End Function
End Class
