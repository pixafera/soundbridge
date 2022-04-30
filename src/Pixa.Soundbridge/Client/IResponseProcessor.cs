
namespace Pixa.Soundbridge.Library
{
    /// <summary>
/// Provides an interface for objects to process responses from the Soundbridge or other RCP compliant device.
/// </summary>
/// <remarks></remarks>
    public interface IResponseProcessor
    {

        /// <summary>
    /// Gets the name of the command this <see cref="IResponseProcessor"/> is processing responses for.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        string Command { get; }
        bool IsByteArray { get; set; }

        /// <summary>
    /// Gets the lines in the response.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        string[] Response { get; }

        /// <summary>
    /// Processes a response line from the Soundbridge or other RCP compliant device.
    /// </summary>
    /// <param name="response"></param>
    /// <remarks>This method will be called on the <see cref="TcpSoundbridgeClient"/>'s receiving thread.</remarks>
        void Process(string response);

        /// <summary>
    /// Checks the response for timeouts and error values.
    /// </summary>
    /// <remarks>This method will be called on the thread that called the public method on <see cref="TcpSoundbridgeClient"/>.</remarks>
        void PostProcess();
    }
}