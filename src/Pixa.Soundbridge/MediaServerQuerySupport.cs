namespace Pixa.Soundbridge {
    /// <summary>
    /// Represents the level of query support a media server has.
    /// </summary>
    /// <remarks></remarks>
    public enum MediaServerQuerySupport {
        /// <summary>
        /// The server does not support queries of any kind.
        /// </summary>
        None,

        /// <summary>
        /// The server allows lists of songs, but no searching.
        /// </summary>
        Songs,

        /// <summary>
        /// The server allows for filtering using the browse filters.
        /// </summary>
        Basic,

        /// <summary>
        /// The server allows for queries using the search methods.
        /// </summary>
        Partial
    }
}