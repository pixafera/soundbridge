''' <summary>
''' Represents a step in the initial setup of a Soundbridge.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class SetupStep
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject

    Protected Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

    ''' <summary>
    ''' Gets the name of the setup step
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected MustOverride ReadOnly Property Name() As String


    ''' <summary>
    ''' Gets the list of options that can be chosen for this step.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public MustOverride Function GetSelectionList() As String()

    ''' <summary>
    ''' Chooses the specified value for this step in the initial setup.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Public MustOverride Sub MakeSelection(ByVal value As String)

End Class
