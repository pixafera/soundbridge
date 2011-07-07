Imports System.Threading

''' <summary>
''' Contains metadata about Transacted RCP commands.
''' </summary>
''' <remarks></remarks>
<AttributeUsage(AttributeTargets.Method, allowmultiple:=False)> _
Public NotInheritable Class RcpTransactedCommandAttribute
    Inherits Pixa.Soundbridge.Library.RcpCommandAttribute

    Public Sub New(ByVal command As String)
        MyBase.New(command)
    End Sub

    Public Overrides Function CreateResponseProcessor(ByVal client As TcpSoundbridgeClient, ByVal waitHandle As System.Threading.EventWaitHandle) As IResponseProcessor
        Return New TransactedResponseProcessor(client, Command, waitHandle)
    End Function
End Class
