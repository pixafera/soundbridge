Imports System.Threading

''' <summary>
''' Provides base functionality for processing responses from the Soundbridge and signalling the requesting thread.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class ResponseProcessorBase
    Implements Pixa.Soundbridge.Library.IResponseProcessor

    ''' <summary>
    ''' Instantiates a new instance of ResponseProcessorBase for the specified SoundbridgeClient and EventWaitHandle.
    ''' </summary>
    ''' <param name="client"></param>
    ''' <param name="waitHandle"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal client As SoundbridgeClient, ByVal command As String, ByVal waitHandle As EventWaitHandle)
        _client = client
        _command = command
        _waitHandle = waitHandle
    End Sub

#Region " Client "
    Private _client As SoundbridgeClient

    ''' <summary>
    ''' Gets the client this ResponseProcessorBase is associated with.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Client() As SoundbridgeClient
        Get
            Return _client
        End Get
    End Property
#End Region

#Region " Command "
    Private _command As String

    Public ReadOnly Property Command() As String Implements IResponseProcessor.Command
        Get
            Return _command
        End Get
    End Property
#End Region

#Region " WaitHandle "
    Private _waitHandle As EventWaitHandle

    ''' <summary>
    ''' Gets the lines in the response.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Response() As String() Implements IResponseProcessor.Response
        Get
            Return _response.ToArray
        End Get
    End Property
#End Region

#Region " Response "
    Private _response As New List(Of String)

    ''' <summary>
    ''' Gets the number of lines in the response.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ResponseCount() As Integer
        Get
            Return _response.Count
        End Get
    End Property

    ''' <summary>
    ''' Gets the EventWaitHandle this ResponseProcessorBase will signal when the entire response has been received.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected ReadOnly Property WaitHandle() As EventWaitHandle
        Get
            Return _waitHandle
        End Get
    End Property

    ''' <summary>
    ''' Adds a line to the list of response lines that are of interest to consuming clients.
    ''' </summary>
    ''' <param name="item"></param>
    ''' <remarks></remarks>
    Protected Sub AddResponse(ByVal item As String)
        _response.Add(item)
    End Sub
#End Region

#Region " Process "
    ''' <summary>
    ''' Processes the specified response line.
    ''' </summary>
    ''' <param name="response"></param>
    ''' <remarks></remarks>
    Public MustOverride Sub Process(ByVal response As String) Implements IResponseProcessor.Process

    ''' <summary>
    ''' Checks the response for timeouts and error values.
    ''' </summary>
    ''' <remarks>This method will be called on the thread that called the public method on <see cref="SoundbridgeClient"/>.</remarks>
    Public MustOverride Sub PostProcess() Implements IResponseProcessor.PostProcess
#End Region

End Class
