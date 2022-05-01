using System;

namespace Pixa.Soundbridge.Client
{

    public delegate void DisplayUpdateEventHandler(string data);

    public delegate void IRKeyPressedEventHandler(string data);
    public delegate void IRKeyDownEventHandler(string data);
    public delegate void IRKeyUpEventHandler(string data);

    /// <summary>
    /// Defines the methods and events that a Soundbridge is capable of supporting.
    /// </summary>
    /// <remarks></remarks>
    public interface ISoundbridgeClient
    {

        #region  Progress 
        /// <summary>
        /// Raised when a StatusAwaitingReply message is received from the
        /// Soundbridge.
        /// </summary>
        /// <remarks>Adding a handler to this event should not automatically initiate
        /// verbose Progress mode.</remarks>
        event EventHandler<RcpCommandProgressEventArgs> AwaitingReply;

        /// <summary>
        /// Raised when a StatusReceivingData message is received from the
        /// Soundbridge.
        /// </summary>
        /// <remarks>Adding a handler to this event should not automatically initiate
        /// verbose Progress mode.</remarks>
        event EventHandler<RcpCommandReceivingProgressEventArgs> ReceivingData;

        /// <summary>
        /// Raised when a StatusSendingRequest message is received from the
        /// Soundbridge.
        /// </summary>
        /// <remarks>Adding a handler to this event should not automatically initiate
        /// verbose Progress mode.</remarks>
        event EventHandler<RcpCommandProgressEventArgs> SendingRequest;
        #endregion

        #region  Methods 

        #region  Protocol Control 
        /// <summary>
        /// Aborts the sychronous transaction for the specified command.
        /// </summary>
        /// <param name="command">The command of the transaction to cancel.</param>
        /// <returns>"OK" if the cancellation was successful, "ParameterError" if
        /// an incorrect command is specified, or "ErrorTransactionNotPending" if a
        /// transaction for that command was not pending.</returns>
        string CancelTransaction(string command);

        /// <summary>
        /// Delets the current list results of the RCP session.
        /// </summary>
        /// <returns>"OK" if the deletion was successful; otherwise
        /// "ErrorNoListResults" if there was no list to delete.</returns>
        /// <remarks>Use of this command is generally unnecessary, as previous list
        /// results are always discarded each time a new command that generates list
        /// results is executed. However, some commands (see SetWiFiNetworkSelection)
        /// may require use of this command to remove ambiguity on a numeric
        /// parameter.</remarks>
        string DeleteList();

        /// <summary>
        /// Gets the current <see cref="ProgressMode"/>.
        /// </summary>
        ProgressMode GetProgressMode();

        /// <summary>
        /// Sets the current <see cref="ProgressMode"/>.
        /// </summary>
        string SetProgressMode(ProgressMode mode);
        #endregion

        #region  Host Configuration 
        /// <summary>
        /// Gets a value to indicate whether initial setup has been completed on the
        /// Soundbridge.
        /// </summary>
        /// <returns>"Complete" if initial setup has been completed; otherwise,
        /// "Incomplete".</returns>
        string GetInitialSetupComplete();

        /// <summary>
        /// Completes the initial setup process.
        /// </summary>
        /// <returns>"OK" if the command executed successfully, "NotComplete" if some
        /// initial setup steps are still outstanding, or "ErrorAlreadySet" if
        /// initial setup was already complete.</returns>
        /// <remarks>Before executing this command, all the required setup choices
        /// must have been made.</remarks>
        string SetInitialSetupComplete();

        /// <summary>
        /// Gets a list of the setup steps that must be done to complete initial
        /// setup.
        /// </summary>
        /// <returns><list type="table">
        ///   <listheader>
        ///     <term>Setup Step</term>
        ///     <description>Actions to perform.</description>
        ///   </listheader>
        ///   <item>
        ///     <term>Language</term>
        ///     <description>The language must be set.  Call <see cref="ListLanguages"/>
        ///     to get a list of the available languages and then call <see cref="SetLanguage"/>
        ///     to make the appropriate choice.</description>
        ///   </item>
        ///   <item>
        ///     <term>TimeZone</term>
        ///     <description>The time zone must be set.  Call <see cref="ListTimeZones"/>
        ///     to get a list of the available time zones and then call <see cref="SetTimeZone"/>
        ///     to make the appropriate choice.</description>
        ///   </item>
        ///   <item>
        ///     <term>Region</term>
        ///     <description>The region must be set.  Call <see cref="ListRegions"/>
        ///     to get a list of the available regions and then call <see cref="SetRegion"/>
        ///     to make the appropriate choice.</description>
        ///   </item>
        ///   <item>
        ///     <term>TermsOfService</term>
        ///     <description>The terms of service must be displayed to the user and
        ///     accepted.  Call <see cref="GetTermsOfServiceUrl"/> to get the URL of
        ///     the terms of service to be displayed to the user and call <see cref="AcceptTermsOfService"/>
        ///     if the user accepts them.</description>
        ///   </item>
        /// </list></returns>
        string[] GetRequiredSetupSteps();

        /// <summary>
        /// Gets a list of languages supported for use on the UI display.
        /// </summary>
        /// <returns>A list of languages supported for use on the Soundbridge's UI display.</returns>
        string[] ListLanguages();

        /// <summary>
        /// Gets the name of the language used to display text.
        /// </summary>
        /// <returns>The name of the language used to display text.</returns>
        /// <remarks></remarks>
        string GetLanguage();
        string SetLanguage(string value);
        string[] ListRegions();
        string SetRegion(int index);
        string GetTermsOfServiceUrl();
        string AcceptTermsOfService();
        string[] ListTimeZones();
        string GetTimeZone();
        string SetTimeZone(int index);
        string[] GetIfConfig();
        string GetLinkStatus(string networkAdapter);
        string GetIPAddress(string networkAdapter);
        string GetMacAddress(string networkAdapter);
        string[] ListWiFiNetworks();
        string GetWiFiNetworkSelection();
        string SetWiFiNetworkSelection(int index);
        string SetWiFiPassword(string password);
        string GetConnectedWiFiNetwork();
        string[] GetWiFiSignalQuality();
        string GetTime(bool formatted);
        string GetDate(bool formatted);
        string SetTime(string value);
        string SetDate(string value);
        string GetSoftwareVersion();
        string CheckSoftwareUpgrade(bool local);
        string ExecuteSoftwareUpgrade(bool local);
        string Reboot();
        string GetFriendlyName();
        string SetFriendlyName(string name);
        string GetOption(string name);
        string SetOption(string name, string value);
        #endregion

        #region  Display Control Commands 
        event DisplayUpdateEventHandler DisplayUpdate;


        string GetVisualizer(bool verbose);
        string SetVisualizer(string name);
        [Obsolete("Use SetVisualizerMode instead")]
        string VisualizerMode(string mode);
        string GetVisualizerMode();
        string SetVisualizerMode(string mode);
        string[] ListVisualizers(bool verbose);
        string GetVizDataVU();
        string GetVizDataFreq();
        string GetVizDataScope();
        string DisplayUpdateEventSubscribe();
        string DisplayUpdateEventUnsubscribe();
        string GetDisplayData(ref bool bytedata);
        #endregion

        #region  IR Demod/Dispatch 
        event IRKeyPressedEventHandler IRKeyPressed;

        event IRKeyDownEventHandler IRKeyDown;


        event IRKeyUpEventHandler IRKeyUp;


        string IRDispatchCommand(string command);
        string IRDemodSubscribe(bool updown);
        string IRDemodUnsubscribe();
        #endregion

        #region  Media Servers 
        /// <summary>
        /// Gets a list of media servers discovered by the Soundbridge.
        /// </summary>
        /// <returns>"ErrorInitialSetupRequired" if initial setup has not been
        /// completed, "GenericError" if an internal error occurred or a list of
        /// available media servers.</returns>
        /// <remarks>The contents of the list can be controlled by setting the server
        /// filter via <see cref="SetServerFilter"/>.  The results of this list can
        /// be used by the <see cref="ServerConnect"/> and <see cref="ServerLaunchUI"/>
        /// commands.</remarks>
        string[] ListServers();

        /// <summary>
        /// Sets the server filter to set which types of server are returned by <see cref="ListServers"/>.
        /// </summary>
        /// <param name="filterTokens">A space-delimited list of tokens:
        /// <list type="table">
        /// <listheader>
        ///   <term>Token</term>
        ///   <description>Description</description>
        /// </listheader>
        /// <item>
        ///   <term>"daap"</term>
        ///   <description>Includes servers using the iTunes DAAP protocol</description>
        /// </item>
        /// <item>
        ///   <term>"upnp"</term>
        ///   <description>Includes servers using the UPnP protocol (Windows Media Player, Rhapsody, Serviio, etc.)</description>
        /// </item>
        /// <item>
        ///   <term>"rsp"</term>
        ///   <description>Includes servers using the RSP protocol (Firefly)</description>
        /// </item>
        /// <item>
        ///   <term>"slim"</term>
        ///   <description>Includes SlimServers</description>
        /// </item>
        /// <item>
        ///   <term>"radio"</term>
        ///   <description>Includes the Internet Radio server.</description>
        /// </item>
        /// <item>
        ///   <term>"flash"</term>
        ///   <description>Includes the flash server, serving music off a flash card
        ///   connected locally to the Soundbridge.</description>
        /// </item>
        /// <item>
        ///   <term>"linein"</term>
        ///   <description>Includes the Line-in server, serving music from the
        ///   Soundbridge's linein connection.</description>
        /// </item>
        /// <item>
        ///   <term>"am"</term>
        ///   <description>Includes the AM Tuner on the Soundbridge Radio.</description>
        /// </item>
        /// <item>
        ///   <term>"fm"</term>
        ///   <description>Includes the FM Tuner on the Soundbridge Radio.</description>
        /// </item>
        /// <item>
        ///   <term>"all"</term>
        ///   <description>Includes all servers.</description>
        /// </item>
        /// <item>
        ///   <term>"debug"</term>
        ///   <description>Includes additional information about servers in the
        ///   <see cref="ListServers"/> output.</description>
        /// </item>
        /// </list></param>
        /// <returns></returns>
        /// <remarks></remarks>
        string SetServerFilter(string filterTokens);

        /// <summary>
        /// Sets the password to use to connect to a media server.
        /// </summary>
        /// <param name="password">The password to use to connect to a media server.</param>
        /// <returns>"OK" if the command executed successfully.</returns>
        /// <remarks>Use an empty string ("") to clear the password.  The password
        /// need only be supplied once in a session.</remarks>
        string SetServerConnectPassword(string password);

        /// <summary>
        /// Connects to the specified server.
        /// </summary>
        /// <param name="index">The zero-based index of the server to connect to in
        /// the list returned by <see cref="ListServers"/>.</param>
        /// <returns>
        /// <list type="table">
        /// <listheader>
        ///   <term>Value</term>
        ///   <description>Description</description>
        /// </listheader>
        /// <item>
        ///   <term>ParameterError</term>
        ///   <description>Invalid index or list results</description>
        /// </item>
        /// <item>
        ///   <term>ErrorUpgradeInProgress</term>
        ///   <description>A software upgrade is in progress.</description>
        /// </item>
        /// <item>
        ///   <term>ErrorAwaitingPostUpgradeReboot</term>
        ///   <description>A software upgrade has completed and the unit needs a
        ///   reboot.</description>
        /// </item>
        /// <item>
        ///   <term>ResourceAllocationError</term>
        ///   <description>Another user of the system is connecting or disconnecting
        ///   to or from a server, wait and try again.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedAlreadyConnected</term>
        ///   <description>The soundbridge is already connected to a media server.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedNoNetwork</term>
        ///   <description>The server requires an active Internet connection.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedBusy</term>
        ///   <description>The media server refused the connection because it was
        ///   busy.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedPassword</term>
        ///   <description>A password needs to be supplied, or the password supplied
        ///   was incorrect.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedForbidden</term>
        ///   <description>The server refused the connection.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedWMCUnauthorised</term>
        ///   <description>The device requires authorisation.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedNoConnect</term>
        ///   <description>No response from the server.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedUnknown</term>
        ///   <description>The connection failed for another reason.</description>
        /// </item>
        /// <item>
        ///   <term>GenericError</term>
        ///   <description>An internal error occurred, maybe another user attempting
        ///   to connect.</description>
        /// </item>
        /// <item>
        ///   <term>Connected</term>
        ///   <description>The unit successfully connected to the server.</description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>Connecting to a server requires that the Soundbridge be
        /// disconnected first.  To make sure that no other server is connected, call
        /// <see cref="GetConnectedServer"/> and <see cref="ServerDisconnect"/> first.</remarks>
        string ServerConnect(int index);

        /// <summary>
        /// Connects to the specified server allowing the user to enter a server
        /// password on the Soundbridge, if one is required.
        /// </summary>
        /// <param name="index">The zero-based index of the server to connect to in
        /// the list returned by <see cref="ListServers"/>.</param>
        /// <returns>
        /// <list type="table">
        /// <listheader>
        ///   <term>Value</term>
        ///   <description>Description</description>
        /// </listheader>
        /// <item>
        ///   <term>ParameterError</term>
        ///   <description>Invalid index or list results</description>
        /// </item>
        /// <item>
        ///   <term>ErrorUpgradeInProgress</term>
        ///   <description>A software upgrade is in progress.</description>
        /// </item>
        /// <item>
        ///   <term>ErrorAwaitingPostUpgradeReboot</term>
        ///   <description>A software upgrade has completed and the unit needs a
        ///   reboot.</description>
        /// </item>
        /// <item>
        ///   <term>ResourceAllocationError</term>
        ///   <description>Another user of the system is connecting or disconnecting
        ///   to or from a server, wait and try again.</description>
        /// </item>
        /// <item>
        ///   <term>ConnectionFailedAlreadyConnected</term>
        ///   <description>The soundbridge is already connected to a media server.</description>
        /// </item>
        /// <item>
        ///   <term>GenericError</term>
        ///   <description>An internal error occurred, maybe another user attempting
        ///   to connect.</description>
        /// </item>
        /// <item>
        ///   <term>Connected</term>
        ///   <description>The unit successfully connected to the server.</description>
        /// </item>
        /// </list></returns>
        /// <remarks>The <see cref="ServerLaunchUI"/> command will prompt the user 
        /// for a password on the Soundbridge should one be required.  An error may
        /// occur if the user does not enter a password.</remarks>
        string ServerLaunchUI(int index);

        /// <summary>
        /// Disconnects the Soundbridge from the media server it's currently
        /// connected to.
        /// </summary>
        /// <returns>
        /// <list type="table">
        /// <listheader>
        ///   <term>Value</term>
        ///   <description>Description</description>
        /// </listheader>
        /// <item>
        ///   <term>ErrorDisconnected</term>
        ///   <description>The Soundbridge is not connected to a server.</description>
        /// </item>
        /// <item>
        ///   <term>ResourceAllocationError</term>
        ///   <description>Another user is connecting or disconnecting.</description>
        /// </item>
        /// <item>
        ///   <term>GenericError</term>
        ///   <description>An internal error occurred, maybe another user attempting
        ///   to connect.</description>
        /// </item>
        /// <item>
        ///   <term>Disconnected</term>
        ///   <description>The unit successfully disconnected from the server.</description>
        /// </item>
        /// </list></returns>
        string ServerDisconnect();

        /// <summary>
        /// Synchronises the RCP session's server with the server connected to by
        /// some other means.
        /// </summary>
        /// <returns>"GenericError" if an error occurred; otherwise, "OK".</returns>
        string GetConnectedServer();

        /// <summary>
        /// Gets the name and type of the currently connected server.
        /// </summary>
        /// <returns>The type and name of the currently connected server.</returns>
        /// <remarks>The values for the server type are identical to those used by
        /// the <see cref="SetServerFilter"/> command.</remarks>
        string[] GetActiveServerInfo();

        /// <summary>
        /// Gets information on the capabilities a server supports.
        /// </summary>
        /// <returns>A list of capabilties and information on the currently connected
        /// server's support for them.</returns>
        /// <remarks>The QuerySupport section has four possible values: None 
        /// indicates that no queries of any kind are support, Songs indicates that
        /// ListAlbum, ListSongs, etc. may be used, Basic indicates that filtering
        /// can be accomplished via the SetBrowseFilter commands and Partial
        /// indicates that wildcard searching can be performed.</remarks>
        string[] ServerGetCapabilities();
        #endregion

        #region  Content Selection and Playback 
        string[] ListSongs();
        string[] ListAlbums();
        string[] ListArtists();
        string[] ListComposers();
        string[] ListGenres();
        string[] ListLocations();
        string[] ListMediaLanguages();
        string[] ListPlaylists();
        string[] ListPlaylistSongs(int playlistIndex);
        string[] ListContainerContents();
        string GetCurrentContainerPath();
        string ContainerEnter(int index);
        string ContainerExit();
        string[] SearchSongs(string searchString);
        string[] SearchArtists(string searchString);
        string[] SearchAlbums(string searchString);
        string[] SearchComposers(string searchString);
        string[] SearchAll(string searchString);
        string SetBrowseFilterArtist(string filterString);
        string SetBrowseFilterAlbum(string filterString);
        string SetBrowseFilterComposer(string filterString);
        string SetBrowseFilterGenre(string filterString);
        string SetBrowseFilterLocation(string filterString);
        string SetBrowseFilterMediaLanguage(string filterString);
        string SetBrowseFilterTopStations(string filterString);
        string SetBrowseFilterFavorites(string filterString);
        string SetSongListSort(string key);
        string SetBrowseListSort(string key);
        #endregion

        #region  Getting Detailed Song Info 
        string[] GetSongInfo(int index);
        string[] GetCurrentSongInfo();
        #endregion

        #region  Managing the Now Playing (ad-hoc) Playlist 
        string NowPlayingClear();
        string[] ListNowPlayingQueue();
        #endregion

        #region  Initiating Media Playback 
        string PlayIndex(int index);
        string NowPlayingInsert(int songIndex, int insertIndex);
        string NowPlayingInsert(int insertIndex);
        string NowPlayingInsert();
        string NowPlayingRemoveAt(int index);
        string QueueAndPlay(int songIndex);
        string QueueAndPlayOne(int index);
        #endregion

        #region  Transport 
        string Play();
        string Pause();
        string PlayPause();
        string Next();
        string Previous();
        string Stop();
        string Shuffle(bool value);
        string Repeat(string mode);
        string GetTransportState();
        string GetElapsedTime();
        string GetTotalTime();
        string GetCurrentNowPlayingIndex();
        #endregion

        #region  Volume Functions 
        string GetVolume();
        string SetVolume(string level);
        #endregion

        #region  Commands For Using Presets
        string[] ListPresets();
        string[] GetPresetInfo(string id);
        string PlayPreset(string id);
        string SetPreset(string id);
        string[] GetWorkingSongInfo();
        string SetWorkingSongInfo(string name, string value);
        string ClearWorkingSong();
        #endregion

        #region  Power State Commands 
        string GetPowerState();
        string SetPowerState(string value, bool reconnect);
        #endregion
        #endregion

    }
}