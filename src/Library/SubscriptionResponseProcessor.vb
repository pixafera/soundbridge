Imports System.Threading

''' <summary>
''' Processes responses for subscription RCP methods.
''' </summary>
''' <remarks></remarks>
Friend Class SubscriptionResponseProcessor
    Inherits Pixa.Soundbridge.Library.ResponseProcessorBase

    Private _eventRaiser As Action(Of String)
    Private _receivedSubAck As Boolean

    ''' <summary>
    ''' Instantiates a new instance of ResponseProcessorBase for the specified SoundbridgeClient and EventWaitHandle.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="waitHandle"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal client As SoundbridgeClient, ByVal command As String, ByVal waitHandle As EventWaitHandle, ByVal eventRaiser As Action(Of String))
        MyBase.New(client, command, waitHandle)
        _eventRaiser = eventRaiser
    End Sub

    ''' <summary>
    ''' Processes the specified response line.
    ''' </summary>
    ''' <param name="response"></param>
    ''' <remarks></remarks>
    Public Overrides Sub Process(ByVal response As String)
        If _receivedSubAck Then
            _eventRaiser(response)
        Else
            AddResponse(response)
            _receivedSubAck = True
            WaitHandle.Set()
        End If
    End Sub

    Public Overrides Sub PostProcess()
        If Response.Length = 0 Then ExceptionHelper.ThrowCommandTimeout(Command)
    End Sub
End Class
