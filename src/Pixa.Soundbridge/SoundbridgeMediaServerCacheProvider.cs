using System;

namespace Pixa.Soundbridge
{
    internal class SoundbridgeMediaServerCacheProvider : SoundbridgeListCacheProvider<MediaServer>
    {
        public SoundbridgeMediaServerCacheProvider(Soundbridge soundbridge, SoundbridgeObject parent) : base(soundbridge, parent)
        {
        }

        protected override MediaServer CreateObject(string elementData, int index)
        {
            var tokens = elementData.Split(" ".ToCharArray(), 3, StringSplitOptions.None);
            return new MediaServer(Soundbridge, ServerListAvailabilityToMediaServerAvailability(tokens[0]), ServerListTypeToMediaServerType(tokens[1]), tokens[2], index);
        }

        /// <summary>
        /// Converts the specified string value into a <see cref="MediaServerAvailability"/>
        /// value.
        /// </summary>
        private MediaServerAvailability ServerListAvailabilityToMediaServerAvailability(string value)
        {
            switch (value ?? "")
            {
                case "kOnline":
                    {
                        return MediaServerAvailability.Online;
                    }

                case "kOffline":
                    {
                        return MediaServerAvailability.Offline;
                    }

                case "kHidden":
                    {
                        return MediaServerAvailability.Hidden;
                    }

                case "kInaccessible":
                    {
                        return MediaServerAvailability.Inaccessible;
                    }
            }

            return default;
        }

        /// <summary>
        /// Converts the specified value into a <see cref="MediaServerType"/>.
        /// </summary>
        private MediaServerType ServerListTypeToMediaServerType(string value)
        {
            switch (value ?? "")
            {
                case "kITunes":
                    {
                        return MediaServerType.Daap;
                    }

                case "kUPnP":
                    {
                        return MediaServerType.Upnp;
                    }

                case "kSlim":
                    {
                        return MediaServerType.Slim;
                    }

                case "kFlash":
                    {
                        return MediaServerType.Flash;
                    }

                case "kFavoriteRadio":
                    {
                        return MediaServerType.Radio;
                    }

                case "kAMTuner":
                    {
                        return MediaServerType.AM;
                    }

                case "kFMTuner":
                    {
                        return MediaServerType.FM;
                    }

                case "kRSP":
                    {
                        return MediaServerType.Rsp;
                    }

                case "kLinein":
                    {
                        return MediaServerType.LineIn;
                    }

                default:
                    {
                        return (MediaServerType)(-1);
                    }
            }
        }
    }
}