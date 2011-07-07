Imports System.Threading

''' <summary>
''' Acts as a base class for providing metadata about an RCP command.
''' </summary>
''' <remarks></remarks>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False)> _
Public MustInherit Class RcpCommandAttribute
    Inherits System.Attribute

    Private _command As String

    Public Sub New(ByVal command As String)
        _command = command
    End Sub

    Public ReadOnly Property Command() As String
        Get
            Return _command
        End Get
    End Property

    ''' <summary>
    ''' Creates an instance of <see cref="IResponseProcessor"/> that can deal with the responses an RCP server will give to this command.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="waitHandle"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public MustOverride Function CreateResponseProcessor(ByVal client As TcpSoundbridgeClient, ByVal waitHandle As EventWaitHandle) As IResponseProcessor

End Class
