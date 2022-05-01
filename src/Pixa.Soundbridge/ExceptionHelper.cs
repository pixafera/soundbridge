namespace Pixa.Soundbridge {
    internal static class ExceptionHelper {
        public static void ThrowUnexpectedPreamble(string preamble) {
            throw new SoundbridgeClientException(string.Format("The specified end point does not appear to be a soundbridge or RCP compliant device.  Expected 'roku:ready', found '{0}'", preamble));
        }

        public static void ThrowAlreadyExecuting(string command) {
            throw new SoundbridgeClientException("The specified command was already being executed");
        }

        public static void ThrowMethodNotFound(string method) {
            throw new SoundbridgeClientException(string.Format("Couldn't find method {0}", method));
        }

        public static void ThrowNotRcpCommandMethod(string method) {
            throw new SoundbridgeClientException(string.Format("An RcpCommandAttribute was not found on method {0} so it could not be executed", method));
        }

        public static void ThrowCommandTimeout(string command) {
            throw new SoundbridgeCommandException(command, "The command timed out");
        }

        public static void ThrowCommandReturnError(string command, string returnValue) {
            throw new SoundbridgeCommandException(command, string.Format("The command '{0}' returned '{1}'", command, returnValue));
        }

        public static void ThrowObjectNotActive() {
            throw new SoundbridgeException("Cannot perform the specified action because the object is not part of the active list");
        }
    }
}