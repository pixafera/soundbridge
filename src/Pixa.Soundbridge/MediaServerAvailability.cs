namespace Pixa.Soundbridge {
    /// <summary>
    /// Represents the availability of a <see cref="MediaServer"/>.
    /// </summary>
    public enum MediaServerAvailability {
        /// <summary>
        /// Indicates that the server is online and available to connect to.
        /// </summary>
        Online,

        /// <summary>
        /// Indicates that the server is offline and not available for connections.
        /// </summary>
        /// <remarks></remarks>
        Offline,

        /// <summary>
        /// Inidicates that another media server should be used in preference to this
        /// one.
        /// </summary>
        Hidden,

        /// <summary>
        /// Indicates that the server is inaccessible due to restrictions on the
        /// <see cref="Soundbridge"/>.
        /// </summary>
        Inaccessible
    }
}