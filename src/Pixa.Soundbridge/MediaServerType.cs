using System;

namespace Pixa.Soundbridge {
    /// <summary>
    /// Represents the type of a <see cref="MediaServer"/>.
    /// </summary>
    /// <remarks></remarks>
    [Flags()]
    public enum MediaServerType {
        /// <summary>
        /// The server uses the iTunes DAAP protocol.
        /// </summary>
        Daap = 1, // kITunes

        /// <summary>
        /// The server uses the UPnP protocol.
        /// </summary>
        Upnp = 2, // kUPnP

        /// <summary>
        /// The server is running Firefly.
        /// </summary>
        Rsp = 4, // kRSP

        /// <summary>
        /// The server is a SlimServer product, by SlimDevices.
        /// </summary>
        Slim = 8, // kSlim

        /// <summary>
        /// The server is the Internet Radio server.
        /// </summary>
        Radio = 16, // kFavoriteRadio

        /// <summary>
        /// The server is run locally from the Soundbridge, using a flash card.
        /// </summary>
        Flash = 32, // kFlash

        /// <summary>
        /// The server uses the Soundbridge's LineIn input.
        /// </summary>
        LineIn = 64, // kLineIn

        /// <summary>
        /// The server is the AM Tuner on the Soundbridge Radio.
        /// </summary>
        AM = 128, // kAMTuner

        /// <summary>
        /// The server is the FM Tuner on the Soundbridge Radio.
        /// </summary>
        FM = 256, // kFMTuner

        /// <summary>
        /// Represents all possible server types.
        /// </summary>
        All = 511
    }
}