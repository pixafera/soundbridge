
namespace Pixa.Soundbridge.Library
{
    public class TermsOfServiceSetupStep : SetupStep
    {
        internal TermsOfServiceSetupStep(ISoundbridgeClient client) : base(client)
        {
        }

        protected override string Name
        {
            get
            {
                return "Terms Of Service";
            }
        }

        public override string[] GetSelectionList()
        {
            return new string[] { Client.GetTermsOfServiceUrl() };
        }

        /// <summary>
    /// Accepts the Terms of Service
    /// </summary>
    /// <param name="value">This parameter is ignored, you can pass nothing or an empty string to it.</param>
    /// <remarks></remarks>
        public override void MakeSelection(string value)
        {
            Client.AcceptTermsOfService();
        }
    }
}