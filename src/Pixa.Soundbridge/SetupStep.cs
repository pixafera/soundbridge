using Pixa.Soundbridge.Client;

namespace Pixa.Soundbridge
{
    /// <summary>
    /// Represents a step in the initial setup of a Soundbridge.
    /// </summary>
    /// <remarks></remarks>
    public abstract class SetupStep : SoundbridgeObject
    {
        protected SetupStep(ISoundbridgeClient client) : base(client)
        {
        }

        /// <summary>
        /// Gets the name of the setup step
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected abstract string Name { get; }


        /// <summary>
        /// Gets the list of options that can be chosen for this step.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string[] GetSelectionList();

        /// <summary>
        /// Chooses the specified value for this step in the initial setup.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public abstract void MakeSelection(string value);
    }
}