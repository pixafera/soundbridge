
namespace Pixa.Soundbridge.Library
{
    public class SetupStepFactory : SoundbridgeObject
    {
        public SetupStepFactory(ISoundbridgeClient client) : base(client)
        {
        }

        public SetupStep CreateSetupStep(string step)
        {
            switch (step ?? "")
            {
                case "Language":
                    {
                        return new LanguageSetupStep(Client);
                    }

                case "TimeZone":
                    {
                        return new TimeZoneSetupStep(Client);
                    }

                case "Region":
                    {
                        return new RegionSetupStep(Client);
                    }

                case "TermsOfService":
                    {
                        return new TermsOfServiceSetupStep(Client);
                    }

                default:
                    {
                        return null;
                    }
            }
        }
    }
}