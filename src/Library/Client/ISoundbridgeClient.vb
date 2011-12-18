Public Interface ISoundbridgeClient

#Region " Progress "
    Event AwaitingReply As EventHandler(Of RcpCommandProgressEventArgs)
    Event ReceivingData As EventHandler(Of RcpCommandReceivingProgressEventArgs)
    Event SendingRequest As EventHandler(Of RcpCommandProgressEventArgs)
#End Region

#Region " Methods "

#Region " Protocol Control "
    Function CancelTransaction(ByVal command As String) As String
    Function DeleteList() As String
    Function GetProgressMode() As ProgressMode
    Function SetProgressMode(ByVal mode As ProgressMode) As String
#End Region

#Region " Host Configuration "
    Function GetInitialSetupComplete() As String
    Function SetInitialSetupComplete() As String
    Function GetRequiredSetupSteps() As String()
    Function ListLanguages() As String()
    Function GetLanguage() As String
    Function SetLanguage(ByVal value As String) As String
    Function ListRegions() As String()
    Function SetRegion(ByVal index As Integer) As String
    Function GetTermsOfServiceUrl() As String
    Function AcceptTermsOfService() As String
    Function ListTimeZones() As String()
    Function GetTimeZone() As String
    Function SetTimeZone(ByVal index As Integer) As String
    Function GetIfConfig() As String()
    Function GetLinkStatus(ByVal networkAdapter As String) As String
    Function GetIPAddress(ByVal networkAdapter As String) As String
    Function GetMacAddress(ByVal networkAdapter As String) As String
    Function ListWiFiNetworks() As String()
    Function GetWiFiNetworkSelection() As String
    Function SetWiFiNetworkSelection(ByVal index As Integer) As String
    Function SetWiFiPassword(ByVal password As String) As String
    Function GetConnectedWiFiNetwork() As String
    Function GetWiFiSignalQuality() As String()
    Function GetTime(ByVal formatted As Boolean) As String
    Function GetDate(ByVal formatted As Boolean) As String
    Function SetTime(ByVal value As String) As String
    Function SetDate(ByVal value As String) As String
    Function GetSoftwareVersion() As String
    Function CheckSoftwareUpgrade(ByVal local As Boolean) As String
    Function ExecuteSoftwareUpgrade(ByVal local As Boolean) As String
    Function Reboot() As String
    Function GetFriendlyName() As String
    Function SetFriendlyName(ByVal name As String) As String
    Function GetOption(ByVal name As String) As String
    Function SetOption(ByVal name As String, ByVal value As String) As String
#End Region

#Region " Display Control Commands "
    Event DisplayUpdate(ByVal data As String)

    Function GetVisualizer(ByVal verbose As Boolean) As String
    Function SetVisualizer(ByVal name As String) As String
    <Obsolete("Use SetVisualizerMode instead")> Function VisualizerMode(ByVal mode As String) As String
    Function GetVisualizerMode() As String
    Function SetVisualizerMode(ByVal mode As String) As String
    Function ListVisualizers(ByVal verbose As Boolean) As String()
    Function GetVizDataVU() As String
    Function GetVizDataFreq() As String
    Function GetVizDataScope() As String
    Function DisplayUpdateEventSubscribe() As String
    Function DisplayUpdateEventUnsubscribe() As String
    Function GetDisplayData(ByRef bytedata As Boolean) As String
#End Region

#Region " IR Demod/Dispatch "
    Event IRKeyPressed(ByVal data As String)
    Event IRKeyDown(ByVal data As String)
    Event IRKeyUp(ByVal data As String)

    Function IRDispatchCommand(ByVal command As String) As String
    Function IRDemodSubscribe(ByVal updown As Boolean) As String
    Function IRDemodUnsubscribe() As String
#End Region

#Region " Media Servers "
    Function ListServers() As String()
    Function SetServerFilter(ByVal filterTokens As String) As String
    Function SetServerConnectPassword(ByVal password As String) As String
    Function ServerConnect(ByVal index As Integer) As String
    Function ServerLaunchUI(ByVal index As Integer) As String
    Function ServerDisconnect() As String
    Function GetConnectedServer() As String
    Function GetActiveServerInfo() As String()
    Function ServerGetCapabilities() As String()
#End Region

#Region " Content Selection and Playback "
    Function ListSongs() As String()
    Function ListAlbums() As String()
    Function ListArtists() As String()
    Function ListComposers() As String()
    Function ListGenres() As String()
    Function ListLocations() As String()
    Function ListMediaLanguages() As String()
    Function ListPlaylists() As String()
    Function ListPlaylistSongs(ByVal playlistIndex As Integer) As String()
    Function ListContainerContents() As String()
    Function GetCurrentContainerPath() As String
    Function ContainerEnter(ByVal index As Integer) As String
    Function ContainerExit() As String
    Function SearchSongs(ByVal searchString As String) As String()
    Function SearchArtists(ByVal searchString As String) As String()
    Function SearchAlbums(ByVal searchString As String) As String()
    Function SearchComposers(ByVal searchString As String) As String()
    Function SearchAll(ByVal searchString As String) As String()
    Function SetBrowseFilterArtist(ByVal filterString As String) As String
    Function SetBrowseFilterAlbum(ByVal filterString As String) As String
    Function SetBrowseFilterComposer(ByVal filterString As String) As String
    Function SetBrowseFilterGenre(ByVal filterString As String) As String
    Function SetBrowseFilterLocation(ByVal filterString As String) As String
    Function SetBrowseFilterMediaLanguage(ByVal filterString As String) As String
    Function SetBrowseFilterTopStations(ByVal filterString As String) As String
    Function SetBrowseFilterFavorites(ByVal filterString As String) As String
    Function SetSongListSort(ByVal key As String) As String
    Function SetBrowseListSort(ByVal key As String) As String
#End Region

#Region " Getting Detailed Song Info "
    Function GetSongInfo(ByVal index As Integer) As String()
    Function GetCurrentSongInfo() As String()
#End Region

#Region " Managing the Now Playing (ad-hoc) Playlist "
    Function NowPlayingClear() As String
    Function ListNowPlayingQueue() As String()
#End Region

#Region " Initiating Media Playback "
    Function PlayIndex(ByVal index As Integer) As String
    Function NowPlayingInsert(ByVal songIndex As Integer, ByVal insertIndex As Integer) As String
    Function NowPlayingRemoveAt(ByVal index As Integer) As String
    Function QueueAndPlay(ByVal songIndex As Integer) As String
    Function QueueAndPlayOne(ByVal index As Integer) As String

#End Region

#Region " Transport "
    Function Play() As String
    Function Pause() As String
    Function PlayPause() As String
    Function [Next]() As String
    Function Previous() As String
    Function [Stop]() As String
    Function Shuffle(ByVal value As Boolean) As String
    Function Repeat(ByVal mode As String) As String
    Function GetTransportState() As String
    Function GetElapsedTime() As String
    Function GetTotalTime() As String
    Function GetCurrentNowPlayingIndex() As String
#End Region

#Region " Volume Functions "
    Function GetVolume() As String
    Function SetVolume(ByVal level As String) As String
#End Region

#Region " Commands For Using Presets"
    Function ListPresets() As String()
    Function GetPresetInfo(ByVal id As String) As String()
    Function PlayPreset(ByVal id As String) As String
    Function SetPreset(ByVal id As String) As String
    Function GetWorkingSongInfo() As String()
    Function SetWorkingSongInfo(ByVal name As String, ByVal value As String) As String
    Function ClearWorkingSong() As String
#End Region

#Region " Power State Commands "
    Function GetPowerState() As String
    Function SetPowerState(ByVal value As String, ByVal reconnect As Boolean) As String
#End Region
#End Region

End Interface
