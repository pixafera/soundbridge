Public Class SetupStepCollection
    Inherits System.Collections.ObjectModel.ReadOnlyCollection(Of SetupStep)

    Public Sub New(ByVal list As IList(Of SetupStep))
        MyBase.New(list)
    End Sub
End Class
