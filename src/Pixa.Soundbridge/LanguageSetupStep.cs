using Pixa.Soundbridge.Client;
using System;

namespace Pixa.Soundbridge
{
    public class LanguageSetupStep : SetupStep
    {
        internal LanguageSetupStep(ISoundbridgeClient client) : base(client)
        {
        }

        protected override string Name
        {
            get
            {
                return "Language";
            }
        }

        public override string[] GetSelectionList()
        {
            return Client.ListLanguages();
        }

        public override void MakeSelection(string value)
        {
            string r = Client.SetLanguage(value);
            if (r == "OK")
                return;
            if (r == "ParameterError")
                throw new ArgumentException("value was not a valid language", "value");
            if (r == "GenericError")
                throw new ArgumentException(string.Format("Couldn't set language to {0}", value), "value");
        }
    }
}