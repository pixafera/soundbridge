Friend Module ExceptionHelper

    Public Sub ThrowUnexpectedPreamble(ByVal preamble As String)
        Throw New SoundbridgeClientException(String.Format("The specified end point does not appear to be a soundbridge or RCP compliant device.  Expected 'roku:ready', found '{0}'", preamble))
    End Sub

    Public Sub ThrowAlreadyExecuting(ByVal command As String)
        Throw New SoundbridgeClientException("The specified command was already being executed")
    End Sub

    Public Sub ThrowMethodNotFound(ByVal method As String)
        Throw New SoundbridgeClientException(String.Format("Couldn't find method {0}", method))
    End Sub

    Public Sub ThrowNotRcpCommandMethod(ByVal method As String)
        Throw New SoundbridgeClientException(String.Format("An RcpCommandAttribute was not found on method {0} so it could not be executed", method))
    End Sub

    Public Sub ThrowCommandTimeout(ByVal command As String)
        Throw New SoundbridgeCommandException(command, "The command timed out")
    End Sub

    Public Sub ThrowCommandReturnError(ByVal command As String, ByVal returnValue As String)
        Throw New SoundbridgeCommandException(command, String.Format("The command '{0}' returned '{1}'", command, returnValue))
    End Sub

    Public Sub ThrowObjectNotActive()
        Throw New SoundbridgeException("Cannot perform the specified action because the object is not part of the active list")
    End Sub
End Module
