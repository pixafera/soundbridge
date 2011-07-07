Imports System.Threading

''' <summary>
''' Processes responses to synchronous RCP methods.
''' </summary>
''' <remarks></remarks>
Friend Class SynchronousResponseProcessor
    Inherits Pixa.Soundbridge.Library.ResponseProcessorBase

    Private _responseLength As Integer = 1
    Private _isList As Boolean

    ''' <summary>
    ''' Instantiates a new instance of SynchronousResponseProcessor for the specified SoundbridgeClient and EventWaitHandle.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="waitHandle"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal client As TcpSoundbridgeClient, ByVal command As String, ByVal waitHandle As EventWaitHandle)
        MyClass.New(client, command, waitHandle, False)
    End Sub

    ''' <summary>
    ''' Instantiates a new instance of SynchronousResponseProcessor for the specified SoundbridgeClient and command, indicating whether or not the results will be a list and if so, what error values to look out for.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="command"></param>
    ''' <param name="waithandle"></param>
    ''' <param name="isList"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal client As TcpSoundbridgeClient, ByVal command As String, ByVal waithandle As EventWaitHandle, ByVal isList As Boolean)
        MyBase.New(client, command, waithandle)
        _isList = isList
    End Sub

    ''' <summary>
    ''' Gets whether the command will return a list or not
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsList() As Boolean
        Get
            Return _isList
        End Get
    End Property

    ''' <summary>
    ''' Processes the specified response line.
    ''' </summary>
    ''' <param name="response"></param>
    ''' <remarks></remarks>
    Public Overrides Sub Process(ByVal response As String)
        If response.StartsWith("ListResultSize") Then
            Integer.TryParse(response.Substring(15), _responseLength)
            Exit Sub
        End If

        If response = "ListResultEnd" Then Exit Sub

        AddResponse(response)

        If ResponseCount = _responseLength Then WaitHandle.Set()
    End Sub

    Public Overrides Sub PostProcess()
        If Response.Length = 0 Then ExceptionHelper.ThrowCommandTimeout(Command)
    End Sub
End Class
