using Pixa.Soundbridge.Client;
using System;

namespace Pixa.Soundbridge {
    public class RegionSetupStep : SetupStep {
        internal RegionSetupStep(ISoundbridgeClient client) : base(client) {
        }

        protected override string Name {
            get {
                return "Region";
            }
        }

        public override string[] GetSelectionList() {
            return Client.ListRegions();
        }

        public override void MakeSelection(string value) {
            var regions = Client.ListRegions();
            int index = Array.IndexOf(regions, value);
            if (index == -1)
                throw new ArgumentException("Specified Region was not in the list of valid regions");
            string r = Client.SetRegion(index);
            if (r == "OK")
                return;
            if (r == "ParameterError")
                throw new ArgumentException("Invalid index or list results", value);
            if (r == "ErrorUnsupported")
                throw new ArgumentException("The specified region is not supported on this host", value);
            if (r == "ErrorAlreadySet")
                throw new ArgumentException("The region has already been set", value);
            if (r == "GenericError")
                throw new ArgumentException("Unable to set region", value);
        }
    }
}