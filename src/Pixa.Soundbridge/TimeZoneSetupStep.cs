using Pixa.Soundbridge.Client;
using System;

namespace Pixa.Soundbridge
{
    public class TimeZoneSetupStep : SetupStep
    {
        internal TimeZoneSetupStep(ISoundbridgeClient client) : base(client)
        {
        }

        protected override string Name
        {
            get
            {
                return "Time Zone";
            }
        }

        public override string[] GetSelectionList()
        {
            return Client.ListTimeZones();
        }

        public override void MakeSelection(string value)
        {
            var timeZones = Client.ListTimeZones();
            int index = Array.IndexOf(timeZones, value);
            if (index == -1)
                throw new Exception(string.Format("Couldn't find Time Zone '{0}' in the list from the Soundbridge", value));
            string r = Client.SetTimeZone(index);
            if (r == "OK")
                return;
            if (r == "ParameterError")
                throw new ArgumentException("Error setting the Time Zone", "value");
            if (r == "GenericError")
                throw new Exception("Soundbridge returned GenericError");
        }
    }
}