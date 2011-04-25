''' <summary>
''' Provides metadata about synchronous RCP commands.
''' </summary>
''' <remarks></remarks>
<AttributeUsage(AttributeTargets.Method, allowmultiple:=False)> _
Public NotInheritable Class RcpSynchronousCommandAttribute
    Inherits Pixa.Soundbridge.Library.RcpCommandAttribute

    Private _isList As Boolean

    Public Sub New(ByVal command As String)
        MyClass.New(command, False)
    End Sub

    Public Sub New(ByVal command As String, ByVal isList As Boolean)
        MyBase.New(command)
        _isList = isList
    End Sub

    Public ReadOnly Property IsList() As Boolean
        Get
            Return _isList
        End Get
    End Property

    ''' <summary>
    ''' Creates an instance of <see cref="IResponseProcessor"/> capable of deal with the responses of an RCP server to synchronous commands.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="waitHandle"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function CreateResponseProcessor(ByVal client As SoundbridgeClient, ByVal waitHandle As System.Threading.EventWaitHandle) As IResponseProcessor
        Return New SynchronousResponseProcessor(client, Command, waitHandle, _isList)
    End Function

End Class
