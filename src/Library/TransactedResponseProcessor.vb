Imports System.Threading

''' <summary>
''' Processes responses for transacted RCP methods.
''' </summary>
''' <remarks></remarks>
Friend Class TransactedResponseProcessor
    Inherits Pixa.Soundbridge.Library.ResponseProcessorBase

    Private _status As TransactionStatus = TransactionStatus.Pending

    ''' <summary>
    ''' Instantiates a new instance of ResponseProcessorBase for the specified SoundbridgeClient and EventWaitHandle.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="waitHandle"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal client As SoundbridgeClient, ByVal command As String, ByVal waitHandle As EventWaitHandle)
        MyBase.New(client, command, waitHandle)
    End Sub

    ''' <summary>
    ''' Gets the status of the transaction.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>If the transaction was not initiated, this usually indicates an error.</remarks>
    Public ReadOnly Property Status() As TransactionStatus
        Get
            Return _status
        End Get
    End Property

    ''' <summary>
    ''' Processes the specified response line.
    ''' </summary>
    ''' <param name="response"></param>
    ''' <remarks></remarks>
    Public Overrides Sub Process(ByVal response As String)
        If response = "TransactionInitiated" Then
            _status = TransactionStatus.Initiated
            Exit Sub
        End If

        If response = "TransactionComplete" Then
            _status = TransactionStatus.Complete
            WaitHandle.Set()
            Exit Sub
        End If

        If response = "TransactionCanceled" Then
            _status = TransactionStatus.Canceled
            WaitHandle.Set()
            Exit Sub
        End If

        If response.StartsWith("ListResultSize") Or response = "ListResultEnd" Then Exit Sub

        AddResponse(response)

        If _status = TransactionStatus.Pending Then
            WaitHandle.Set()
            Exit Sub
        End If
    End Sub

    Public Overrides Sub PostProcess()
        If _status <> TransactionStatus.Complete And _status <> TransactionStatus.Canceled And ResponseCount = 0 Then ExceptionHelper.ThrowCommandTimeout(Command)
    End Sub
End Class
