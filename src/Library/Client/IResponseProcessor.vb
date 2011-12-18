''' <summary>
''' Provides an interface for objects to process responses from the Soundbridge or other RCP compliant device.
''' </summary>
''' <remarks></remarks>
Public Interface IResponseProcessor

    ''' <summary>
    ''' Gets the name of the command this <see cref="IResponseProcessor"/> is processing responses for.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property Command() As String

    Property IsByteArray() As Boolean

    ''' <summary>
    ''' Gets the lines in the response.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property Response() As String()

    ''' <summary>
    ''' Processes a response line from the Soundbridge or other RCP compliant device.
    ''' </summary>
    ''' <param name="response"></param>
    ''' <remarks>This method will be called on the <see cref="TcpSoundbridgeClient"/>'s receiving thread.</remarks>
    Sub Process(ByVal response As String)

    ''' <summary>
    ''' Checks the response for timeouts and error values.
    ''' </summary>
    ''' <remarks>This method will be called on the thread that called the public method on <see cref="TcpSoundbridgeClient"/>.</remarks>
    Sub PostProcess()

End Interface
