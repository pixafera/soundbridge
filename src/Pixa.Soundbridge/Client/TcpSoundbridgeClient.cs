using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Pixa.Soundbridge.Client
{

    /// <summary>
    /// A class for interacting with Soundbridges and other RCP compliant devices.
    /// </summary>
    /// <remarks></remarks>
    public class TcpSoundbridgeClient : ISoundbridgeClient, IDisposable
    {
        private TcpClient _client;
        private int _readTimeout = 5000;

        #region  Constructors 
        /// <summary>
        /// Creates a new SoundbridgeClient connected to the specified IPEndPoint.
        /// </summary>
        /// <param name="localEP"></param>
        /// <remarks></remarks>
        public TcpSoundbridgeClient(IPEndPoint localEP) : this(new TcpClient(localEP))
        {
        }

        /// <summary>
        /// Creates a new SoundbridgeClient connect to the specified host.
        /// </summary>
        /// <param name="hostname"></param>
        /// <remarks></remarks>
        public TcpSoundbridgeClient(string hostname) : this(hostname, 5555)
        {
        }

        /// <summary>
        /// Creates a new SoundbridgeClient connected to the specified host and port.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>
        public TcpSoundbridgeClient(string hostname, int port) : this(new TcpClient(hostname, port))
        {
        }

        // Creates a new SoundbridgeClient connected to the specified TcpClient.
        private TcpSoundbridgeClient(TcpClient client)
        {
            try
            {
                _number = GetNextNumber();
                NetworkStream stream;
                string receivedPreamble;
                _client = client;
                stream = _client.GetStream();

                // set up the reader and check we're connected to a soundbridge
                _reader = new StreamReader(stream);
                stream.ReadTimeout = ReadTimeout;
                receivedPreamble = _reader.ReadLine();
                Debug.WriteLine(string.Format("{0}<: {1}", _number, receivedPreamble));
                if (receivedPreamble != "roku: ready")
                    ExceptionHelper.ThrowUnexpectedPreamble(receivedPreamble);
                stream.ReadTimeout = Timeout.Infinite;

                // Setup the writer
                _writer = new StreamWriter(stream);

                // Start the reading thread
                var t = new Thread(ReadFromClient);
                t.Start();
            }
            catch
            {
                _client.Close();
                throw;
            }
        }
        #endregion

        #region  Debug Client Number 
        private static int _nextNumber = 1;
        private static object _nextNumberLock = new object();

        public static int GetNextNumber()
        {
            lock (_nextNumberLock)
            {
                int i = _nextNumber;
                _nextNumber += 1;
                return i;
            }
        }

        private int _number;
        #endregion

        #region  Connecting 
        public int ReadTimeout
        {
            get
            {
                return _readTimeout;
            }

            set
            {
                _readTimeout = value;
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return (IPEndPoint)_client.Client.RemoteEndPoint;
            }
        }

        public void Close()
        {
            _client.Close();
            if (_readThread.IsAlive)
            {
                _readThread.Interrupt();
                // _readThread.Join()
            }
        }
        #endregion

        #region  Reading 
        private Thread _readThread;
        private StreamReader _reader;
        private Dictionary<string, IResponseProcessor> _processors = new Dictionary<string, IResponseProcessor>();
        private object _processorsLock = new object();

        private void ReadFromClient()
        {
            try
            {
                _readThread = Thread.CurrentThread;
                while (_client.Connected)
                {
                    string response = _reader.ReadLine();
                    Debug.WriteLine(string.Format("{0}<: {1}", _number, response));
                    var parts = response.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                    if (HasProcessor(parts[0]))
                    {
                        string processedResponse;
                        var processor = GetProcessor(parts[0]);
                        if (parts.Length == 2)
                        {
                            processedResponse = parts[1].Trim();
                        }
                        else
                        {
                            processedResponse = "";
                        }

                        if (processedResponse.StartsWith("data bytes: "))
                        {
                            processedResponse = _reader.ReadLine();
                            processor.IsByteArray = true;
                        }

                        try
                        {
                            processor.Process(processedResponse);
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log this exception properly
                            Debug.WriteLine(string.Format("Exception while processing command {0}", parts[0]));
                            Debug.WriteLine(ex.ToString());
                            RemoveProcessor(processedResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in read thread");
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if (_client.Connected)
                    _client.Client.Close(ReadTimeout);
            }
        }

        private void AddProcessor(string key, IResponseProcessor item)
        {
            lock (_processorsLock)
                _processors.Add(key, item);
        }

        private bool HasProcessor(string key)
        {
            lock (_processorsLock)
                return _processors.ContainsKey(key);
        }

        private IResponseProcessor GetProcessor(string key)
        {
            lock (_processorsLock)
                return _processors[key];
        }

        private void RemoveProcessor(string key)
        {
            lock (_processorsLock)
                _processors.Remove(key);
        }
        #endregion

        #region  Invoke 
        private StreamWriter _writer;
        private static Dictionary<string, RcpCommandAttribute> _invokeCache = new Dictionary<string, RcpCommandAttribute>();
        private static object _invokeCacheLock = new object();

        protected IResponseProcessor Invoke(string method, params string[] args)
        {
            if (_processors.ContainsKey(method))
                ExceptionHelper.ThrowAlreadyExecuting(method);
            RcpCommandAttribute cmd;
            lock (_invokeCacheLock)
            {
                if (_invokeCache.ContainsKey(method))
                {
                    cmd = _invokeCache[method];
                }
                else
                {
                    var info = GetType().GetMethod(method);
                    if (info is null)
                        ExceptionHelper.ThrowMethodNotFound(method);
                    RcpCommandAttribute[] cmds = (RcpCommandAttribute[])info.GetCustomAttributes(typeof(RcpCommandAttribute), false);
                    if (cmds.Length == 0)
                        ExceptionHelper.ThrowNotRcpCommandMethod(method);
                    cmd = cmds[0];
                    _invokeCache.Add(method, cmd);
                }
            }

            var wait = new EventWaitHandle(false, EventResetMode.AutoReset);
            var processor = cmd.CreateResponseProcessor(this, wait);
            AddProcessor(method, processor);
            Debug.Write(_number);
            Debug.Write(">: ");
            _writer.Write(method);
            Debug.Write(method);
            if (args.Length > 0)
            {
                for (int i = 0, loopTo = args.Length - 1; i <= loopTo; i++)
                {
                    _writer.Write(" ");
                    Debug.Write(" ");
                    _writer.Write(args[i]);
                    Debug.Write(args[i]);
                }
            }

            _writer.Write("\r\n");
            Debug.WriteLine("");
            _writer.Flush();
            wait.WaitOne(); // (ReadTimeout)
            RemoveProcessor(method);
            processor.PostProcess();

            // 'HACK: A number of commands involve UI things and it seems that some commands don't like being executed in quick succession.
            // Thread.Sleep(500)

            return processor;
        }
        #endregion

        #region  Progress 
        public event EventHandler<RcpCommandProgressEventArgs> AwaitingReply;
        public event EventHandler<RcpCommandReceivingProgressEventArgs> ReceivingData;
        public event EventHandler<RcpCommandProgressEventArgs> SendingRequest;

        protected virtual void OnAwaitingReply(RcpCommandProgressEventArgs e)
        {
            AwaitingReply?.Invoke(this, e);
        }

        internal void OnAwaitingReply(string command)
        {
            OnAwaitingReply(new RcpCommandProgressEventArgs(command));
        }

        protected virtual void OnReceivingData(RcpCommandReceivingProgressEventArgs e)
        {
            ReceivingData?.Invoke(this, e);
        }

        internal void OnReceivingData(string command)
        {
            OnReceivingData(new RcpCommandReceivingProgressEventArgs(command));
        }

        internal void OnReceivingData(string command, int progress)
        {
            OnReceivingData(new RcpCommandReceivingProgressEventArgs(command, progress));
        }

        internal void OnReceivingData(string command, int progress, int total)
        {
            OnReceivingData(new RcpCommandReceivingProgressEventArgs(command, progress, total));
        }

        protected virtual void OnSendingRequest(RcpCommandProgressEventArgs e)
        {
            SendingRequest?.Invoke(this, e);
        }

        internal void OnSendingRequest(string command)
        {
            OnSendingRequest(new RcpCommandProgressEventArgs(command));
        }
        #endregion

        #region  Methods 

        #region  Protocol Control 
        [RcpSynchronousCommand("CancelTransaction")]
        public string CancelTransaction(string command)
        {
            var p = Invoke("CancelTransaction", command);
            return p.Response[0];
        }

        [RcpSynchronousCommand("DeleteList")]
        public string DeleteList()
        {
            var p = Invoke("DeleteList");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetProgressMode")]
        public ProgressMode GetProgressMode()
        {
            var p = Invoke("GetProgressMode");
            if (p.Response[0] == "off")
            {
                return ProgressMode.Off;
            }
            else
            {
                return ProgressMode.Verbose;
            }
        }

        [RcpSynchronousCommand("SetProgressMode")]
        public string SetProgressMode(ProgressMode mode)
        {
            var p = Invoke("SetProgressMode", mode == ProgressMode.Off ? "off" : "verbose");
            return p.Response[0];
        }
        #endregion

        #region  Host Configuration 
        [RcpSynchronousCommand("GetInitialSetupComplete")]
        public string GetInitialSetupComplete()
        {
            var p = Invoke("GetInitialSetupComplete");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetInitialSetupComplete")]
        public string SetInitialSetupComplete()
        {
            var p = Invoke("SetInitialSetupComplete");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetRequiredSetupSteps")]
        public string[] GetRequiredSetupSteps()
        {
            var p = Invoke("GetRequiredSetupSteps");
            return p.Response;
        }

        [RcpSynchronousCommand("ListLanguages")]
        public string[] ListLanguages()
        {
            var p = Invoke("ListLanguages");
            return p.Response;
        }

        [RcpSynchronousCommand("GetLanguage")]
        public string GetLanguage()
        {
            var p = Invoke("GetLanguage");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetLanguage")]
        public string SetLanguage(string value)
        {
            var p = Invoke("SetLanguage", value);
            return p.Response[0];
        }

        [RcpSynchronousCommand("ListRegions")]
        public string[] ListRegions()
        {
            var p = Invoke("ListRegions");
            return p.Response;
        }

        [RcpSynchronousCommand("SetRegion")]
        public string SetRegion(int index)
        {
            var p = Invoke("SetRegion", index.ToString());
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetTermsOfServiceUrl")]
        public string GetTermsOfServiceUrl()
        {
            var p = Invoke("GetTermsOfServiceUrl");
            return p.Response[0];
        }

        [RcpSynchronousCommand("AcceptTermsOfService")]
        public string AcceptTermsOfService()
        {
            var p = Invoke("AcceptTermsOfService");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetIfConfig")]
        public string[] GetIfConfig()
        {
            var p = Invoke("GetIfConfig");
            return p.Response;
        }

        [RcpSynchronousCommand("GetLinkStatus")]
        public string GetLinkStatus(string networkAdapter)
        {
            var p = Invoke("GetLinkStatus", networkAdapter);
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetIPAddress")]
        public string GetIPAddress(string networkAdapter)
        {
            var p = Invoke("GetIPAddress", networkAdapter);
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetMACAddress")]
        public string GetMacAddress(string networkAdapter)
        {
            var p = Invoke("GetMacAddress", networkAdapter);
            return p.Response[0];
        }

        [RcpTransactedCommand("ListWiFiNetworks", true)]
        public string[] ListWiFiNetworks()
        {
            var p = Invoke("ListWiFiNetworks");
            return p.Response;
        }

        [RcpSynchronousCommand("GetWiFiNetworkSelection")]
        public string GetWiFiNetworkSelection()
        {
            var p = Invoke("GetWiFiNetworkSelection");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetWiFiNetworkSelection")]
        public string SetWiFiNetworkSelection(int index)
        {
            var p = Invoke("SetWiFiNetworkSelection");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetWiFiPassword")]
        public string SetWiFiPassword(string password)
        {
            var p = Invoke("SetWiFiPassword");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetConnectedWiFiNetwork")]
        public string GetConnectedWiFiNetwork()
        {
            var p = Invoke("GetConnectedWiFiNetwork");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetWiFiSignalQuality")]
        public string[] GetWiFiSignalQuality()
        {
            var p = Invoke("GetWiFiSignalQuality");
            return p.Response;
        }

        [RcpSynchronousCommand("GetTime")]
        public string GetTime(bool formatted)
        {
            IResponseProcessor p;
            if (formatted)
            {
                p = Invoke("GetTime", "verbose");
            }
            else
            {
                p = Invoke("GetTime");
            }

            return p.Response[0];
        }

        [RcpSynchronousCommand("GetDate")]
        public string GetDate(bool formatted)
        {
            IResponseProcessor p;
            if (formatted)
            {
                p = Invoke("GetDate", "verbose");
            }
            else
            {
                p = Invoke("GetDate");
            }

            return p.Response[0];
        }

        [RcpSynchronousCommand("SetTime")]
        public string SetTime(string value)
        {
            var p = Invoke("SetTime", value);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetDate")]
        public string SetDate(string value)
        {
            var p = Invoke("SetDate", value);
            return p.Response[0];
        }

        [RcpSynchronousCommand("ListTimeZones", true)]
        public string[] ListTimeZones()
        {
            var p = Invoke("ListTimeZones");
            return p.Response;
        }

        [RcpSynchronousCommand("GetTimeZone")]
        public string GetTimeZone()
        {
            var p = Invoke("GetTimeZone");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetTimeZone")]
        public string SetTimeZone(int index)
        {
            var p = Invoke("SetTimeZone", index.ToString());
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetSoftwareVersion")]
        public string GetSoftwareVersion()
        {
            var p = Invoke("GetSoftwareVersion");
            return p.Response[0];
        }

        [RcpTransactedCommand("CheckSoftwareUpgrade")]
        public string CheckSoftwareUpgrade(bool local)
        {
            IResponseProcessor p;
            if (local)
            {
                p = Invoke("CheckSoftwareUpgrade", "local");
            }
            else
            {
                p = Invoke("CheckSoftwareUpgrade");
            }

            return p.Response[0];
        }

        [RcpTransactedCommand("ExecuteSoftwareUpgrade")]
        public string ExecuteSoftwareUpgrade(bool local)
        {
            IResponseProcessor p;
            if (local)
            {
                p = Invoke("ExecuteSoftwareUpgrade", "local");
            }
            else
            {
                p = Invoke("ExecuteSoftwareUpgrade");
            }

            return p.Response[0];
        }

        [RcpSynchronousCommand("Reboot")]
        public string Reboot()
        {
            var p = Invoke("Reboot");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetFriendlyName")]
        public string GetFriendlyName()
        {
            var p = Invoke("GetFriendlyName");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetFriendlyName")]
        public string SetFriendlyName(string value)
        {
            var p = Invoke("SetFriendlyName");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetOption")]
        public string GetOption(string name)
        {
            var p = Invoke("GetOption");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetOption")]
        public string SetOption(string name, string value)
        {
            var p = Invoke("SetOption", name, value);
            return p.Response[0];
        }
        #endregion

        #region  Display Control Commands 
        public event DisplayUpdateEventHandler DisplayUpdate;

        [RcpSynchronousCommand("GetVisualizer")]
        public string GetVisualizer(bool verbose)
        {
            IResponseProcessor p;
            if (verbose)
            {
                p = Invoke("GetVisualizer", "verbose");
            }
            else
            {
                p = Invoke("GetVisualizer");
            }

            return p.Response[0];
        }

        [RcpSynchronousCommand("SetVisualizer")]
        public string SetVisualizer(string name)
        {
            var p = Invoke("SetVisualizer", name);
            return p.Response[0];
        }

        [RcpSynchronousCommand("VisualizerMode")]
        [Obsolete("Use SetVisualizerMode instead")]
        public string VisualizerMode(string mode)
        {
            var p = Invoke("VisualizerMode", mode);
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetVisualizerMode")]
        public string GetVisualizerMode()
        {
            var p = Invoke("GetVisualizerMode");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetVisualizerMode")]
        public string SetVisualizerMode(string mode)
        {
            var p = Invoke("SetVisualizerMode", mode);
            return p.Response[0];
        }

        [RcpSynchronousCommand("ListVisualizers", true)]
        public string[] ListVisualizers(bool verbose)
        {
            IResponseProcessor p;
            if (verbose)
            {
                p = Invoke("ListVisualizers", "verbose");
            }
            else
            {
                p = Invoke("ListVisualizers");
            }

            return p.Response;
        }

        [RcpSynchronousCommand("GetVizDataVU")]
        public string GetVizDataVU()
        {
            var p = Invoke("GetVisDataVU");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetVizDataFreq")]
        public string GetVizDataFreq()
        {
            var p = Invoke("GetVizDataFreq");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetVizDataScope")]
        public string GetVizDataScope()
        {
            var p = Invoke("GetVizDataScope");
            return p.Response[0];
        }

        [RcpSubscriptionCommand("DisplayUpdateEventSubscribe", "OnDisplayUpdate")]
        public string DisplayUpdateEventSubscribe()
        {
            var p = Invoke("DisplayUpdateEventSubscribe");
            return p.Response[0];
        }

        protected virtual void OnDisplayUpdate(string data)
        {
            DisplayUpdate?.Invoke(data);
        }

        [RcpSynchronousCommand("DisplayUpdateEventUnsubscribe")]
        public string DisplayUpdateEventUnsubscribe()
        {
            var p = Invoke("DisplayUpdateEventUnsubscribe");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetDisplayData")]
        public string GetDisplayData(ref bool byteData)
        {
            var p = Invoke("GetDisplayData");
            byteData = p.IsByteArray;
            return p.Response[0];
        }
        #endregion

        #region  IR Demod/Dispatch 
        public event IRKeyPressedEventHandler IRKeyPressed;
        public event IRKeyDownEventHandler IRKeyDown;
        public event IRKeyUpEventHandler IRKeyUp;

        [RcpSynchronousCommand("IRDispatchCommand")]
        public string IRDispatchCommand(string command)
        {
            var p = Invoke("IRDispatchCommand", command);
            return p.Response[0];
        }

        [RcpSubscriptionCommand("IRDemodSubscribe", "OnIRKeyPressed")]
        public string IRDemodSubscribe(bool updown)
        {
            IResponseProcessor p;
            if (updown)
            {
                p = Invoke("IRDemodSubscribe", "updown");
            }
            else
            {
                p = Invoke("IRDemodSubscribe");
            }

            return p.Response[0];
        }

        [RcpSynchronousCommand("IRDemodUnsubscribe")]
        public string IRDemodUnsubscribe()
        {
            var p = Invoke("IRDemodUnsubscribe");
            return p.Response[0];
        }
        #endregion

        #region  Media Servers 
        [RcpSynchronousCommand("ListServers", true)]
        public string[] ListServers()
        {
            var p = Invoke("ListServers");
            return p.Response;
        }

        [RcpSynchronousCommand("SetServerFilter")]
        public string SetServerFilter(string filterTokens)
        {
            var p = Invoke("SetServerFilter", filterTokens);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetServerConnectPassword")]
        public string SetServerConnectPassword(string password)
        {
            var p = Invoke("SetServerConnectPassword");
            return p.Response[0];
        }

        [RcpTransactedCommand("ServerConnect")]
        public string ServerConnect(int index)
        {
            var p = Invoke("ServerConnect", index.ToString());
            Thread.Sleep(500);
            return p.Response[0];
        }

        [RcpSynchronousCommand("ServerLaunchUI")]
        public string ServerLaunchUI(int index)
        {
            var p = Invoke("ServerLaunchUI", index.ToString());
            Thread.Sleep(500);
            return p.Response[0];
        }

        [RcpTransactedCommand("ServerDisconnect")]
        public string ServerDisconnect()
        {
            var p = Invoke("ServerDisconnect");
            Thread.Sleep(500);
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetConnectedServer")]
        public string GetConnectedServer()
        {
            var p = Invoke("GetConnectedServer");
            Thread.Sleep(500);
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetActiveServerInfo")]
        public string[] GetActiveServerInfo()
        {
            var p = Invoke("GetActiveServerInfo");
            return p.Response;
        }

        [RcpTransactedCommand("GetServerCapabilities")]
        public string[] ServerGetCapabilities()
        {
            var p = Invoke("ServerGetCapabilities");
            return p.Response;
        }
        #endregion

        #region  Content Selection and Playback 
        [RcpTransactedCommand("ListSongs", true)]
        public string[] ListSongs()
        {
            var p = Invoke("ListSongs");
            return p.Response;
        }

        [RcpTransactedCommand("ListAlbums", true)]
        public string[] ListAlbums()
        {
            var p = Invoke("ListAlbums");
            return p.Response;
        }

        [RcpTransactedCommand("ListArtists", true)]
        public string[] ListArtists()
        {
            var p = Invoke("ListArtists");
            return p.Response;
        }

        [RcpTransactedCommand("ListComposers", true)]
        public string[] ListComposers()
        {
            var p = Invoke("ListComposers");
            return p.Response;
        }

        [RcpTransactedCommand("ListGenres", true)]
        public string[] ListGenres()
        {
            var p = Invoke("ListGenres");
            return p.Response;
        }

        [RcpTransactedCommand("ListLocations", true)]
        public string[] ListLocations()
        {
            var p = Invoke("ListLocations");
            return p.Response;
        }

        [RcpTransactedCommand("ListMediaLanguages", true)]
        public string[] ListMediaLanguages()
        {
            var p = Invoke("ListMediaLanguages");
            return p.Response;
        }

        [RcpTransactedCommand("ListPlaylists")]
        public string[] ListPlaylists()
        {
            var p = Invoke("ListPlaylists");
            return p.Response;
        }

        [RcpTransactedCommand("ListPlaylistSongs")]
        public string[] ListPlaylistSongs(int playlistIndex)
        {
            var p = Invoke("ListPlaylistSongs", playlistIndex.ToString());
            return p.Response;
        }

        [RcpTransactedCommand("ListContainerContents")]
        public string[] ListContainerContents()
        {
            var p = Invoke("ListContainerContents");
            return p.Response;
        }

        [RcpSynchronousCommand("GetCurrentContainerPath")]
        public string GetCurrentContainerPath()
        {
            var p = Invoke("GetCurrentContainerPath");
            return p.Response[0];
        }

        [RcpSynchronousCommand("ContainerEnter")]
        public string ContainerEnter(int index)
        {
            var p = Invoke("ContainerEnter", index.ToString());
            return p.Response[0];
        }

        [RcpSynchronousCommand("ContainerExit")]
        public string ContainerExit()
        {
            var p = Invoke("ContainerExit");
            return p.Response[0];
        }

        [RcpTransactedCommand("SearchSongs")]
        public string[] SearchSongs(string searchString)
        {
            var p = Invoke("SearchSongs", searchString);
            return p.Response;
        }

        [RcpTransactedCommand("SearchArtists")]
        public string[] SearchArtists(string searchString)
        {
            var p = Invoke("SearchArtists", searchString);
            return p.Response;
        }

        [RcpTransactedCommand("SearchAlbums")]
        public string[] SearchAlbums(string searchString)
        {
            var p = Invoke("SearchAlbums", searchString);
            return p.Response;
        }

        [RcpTransactedCommand("SearchComposers")]
        public string[] SearchComposers(string searchString)
        {
            var p = Invoke("SearchComposers", searchString);
            return p.Response;
        }

        [RcpTransactedCommand("SearchAll")]
        public string[] SearchAll(string searchString)
        {
            var p = Invoke("SearchAll", searchString);
            return p.Response;
        }

        [RcpSynchronousCommand("SetBrowseFilterArtist")]
        public string SetBrowseFilterArtist(string filterString)
        {
            var p = Invoke("SetBrowseFilterArtist", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterAlbum")]
        public string SetBrowseFilterAlbum(string filterString)
        {
            var p = Invoke("SetBrowseFilterAlbum", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterComposer")]
        public string SetBrowseFilterComposer(string filterString)
        {
            var p = Invoke("SetBrowseFilterComposer", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterGenre")]
        public string SetBrowseFilterGenre(string filterString)
        {
            var p = Invoke("SetBrowseFilterGenre", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterLocation")]
        public string SetBrowseFilterLocation(string filterString)
        {
            var p = Invoke("SetBrowseFilterComposer", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterMediaLanguage")]
        public string SetBrowseFilterMediaLanguage(string filterString)
        {
            var p = Invoke("SetBrowseFilterMediaLanguage", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterTopStations")]
        public string SetBrowseFilterTopStations(string filterString)
        {
            var p = Invoke("SetBrowseFilterTopStations", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseFilterFavorites")]
        public string SetBrowseFilterFavorites(string filterString)
        {
            var p = Invoke("SetBrowseFilterFavorites", filterString);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetSongListSort")]
        public string SetSongListSort(string key)
        {
            var p = Invoke("SetSongListSort", key);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetBrowseListSort")]
        public string SetBrowseListSort(string key)
        {
            var p = Invoke("SetBrowseListSort", key);
            return p.Response[0];
        }
        #endregion

        #region  Getting Detailed Song Info 
        [RcpTransactedCommand("GetSongInfo")]
        public string[] GetSongInfo(int index)
        {
            var p = Invoke("GetSongInfo", index.ToString());
            return p.Response;
        }

        [RcpTransactedCommand("GetCurrentSongInfo")]
        public string[] GetCurrentSongInfo()
        {
            var p = Invoke("GetCurrentSongInfo");
            return p.Response;
        }
        #endregion

        #region  Managing the Now Playing (ad-hoc) Playlist 
        [RcpSynchronousCommand("NowPlayingClear")]
        public string NowPlayingClear()
        {
            var p = Invoke("NowPlayingClear");
            return p.Response[0];
        }

        [RcpSynchronousCommand("ListNowPlayingQueue")]
        public string[] ListNowPlayingQueue()
        {
            var p = Invoke("ListNowPlayingQueue");
            return p.Response;
        }
        #endregion

        #region  Intiating Media Playback 
        [RcpSynchronousCommand("PlayIndex")]
        public string PlayIndex(int index)
        {
            var p = Invoke("PlayIndex", index.ToString());
            return p.Response[0];
        }

        [RcpSynchronousCommand("NowPlayingInsert")]
        public string NowPlayingInsert(int songIndex, int insertIndex)
        {
            IResponseProcessor p;
            string arg2;
            if (songIndex < 0)
            {
                arg2 = "all";
            }
            else
            {
                arg2 = songIndex.ToString();
            }

            if (insertIndex < 0)
            {
                p = Invoke("NowPlayingInsert", arg2);
            }
            else
            {
                p = Invoke("NowPlayingInsert", arg2, insertIndex.ToString());
            }

            return p.Response[0];
        }

        public string NowPlayingInsert()
        {
            return NowPlayingInsert(-1, -1);
        }

        public string NowPlayingInsert(int insertIndex)
        {
            return NowPlayingInsert(-1, insertIndex);
        }

        [RcpSynchronousCommand("NowPlayingRemoveAt")]
        public string NowPlayingRemoveAt(int index)
        {
            var p = Invoke("NowPlayingRemoveAt", index.ToString());
            return p.Response[0];
        }

        [RcpSynchronousCommand("QueueAndPlay")]
        public string QueueAndPlay(int songIndex)
        {
            var p = Invoke("QueueAndPlay", songIndex.ToString());
            return p.Response[0];
        }

        [RcpSynchronousCommand("QueueAndPlayOne")]
        public string QueueAndPlayOne(int index)
        {
            IResponseProcessor p;
            if (index < 0)
            {
                p = Invoke("QueueAndPlayOne", "working");
            }
            else
            {
                p = Invoke("QueueAndPlayOne", index.ToString());
            }

            return p.Response[0];
        }
        #endregion

        #region  Transport 
        [RcpSynchronousCommand("Play")]
        public string Play()
        {
            var p = Invoke("Play");
            return p.Response[0];
        }

        [RcpSynchronousCommand("Pause")]
        public string Pause()
        {
            var p = Invoke("Pause");
            return p.Response[0];
        }

        [RcpSynchronousCommand("PlayPause")]
        public string PlayPause()
        {
            var p = Invoke("PlayPause");
            return p.Response[0];
        }

        [RcpSynchronousCommand("Next")]
        public string Next()
        {
            var p = Invoke("Next");
            return p.Response[0];
        }

        [RcpSynchronousCommand("Previous")]
        public string Previous()
        {
            var p = Invoke("Previous");
            return p.Response[0];
        }

        [RcpSynchronousCommand("Stop")]
        public string Stop()
        {
            var p = Invoke("Stop");
            return p.Response[0];
        }

        [RcpSynchronousCommand("Shuffle")]
        public string Shuffle(bool value)
        {
            string sValue = value ? "on" : "off";
            var p = Invoke("Shuffle", sValue);
            return p.Response[0];
        }

        [RcpSynchronousCommand("Repeat")]
        public string Repeat(string mode)
        {
            var p = Invoke("Repeat", mode);
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetTransportState")]
        public string GetTransportState()
        {
            var p = Invoke("GetTransportState");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetElapsedTime")]
        public string GetElapsedTime()
        {
            var p = Invoke("GetElapsedTime");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetTotalTime")]
        public string GetTotalTime()
        {
            var p = Invoke("GetTotalTime");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetCurrentNowPlayingIndex")]
        public string GetCurrentNowPlayingIndex()
        {
            var p = Invoke("GetCurrentNowPlayingIndex");
            return p.Response[0];
        }
        #endregion

        #region  Volume Functions 
        [RcpSynchronousCommand("GetVolume")]
        public string GetVolume()
        {
            var p = Invoke("GetVolume");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetVolume")]
        public string SetVolume(string level)
        {
            var p = Invoke("SetVolume", level);
            return p.Response[0];
        }
        #endregion

        #region  Commands for Using Presets 
        [RcpSynchronousCommand("ListPresets")]
        public string[] ListPresets()
        {
            var p = Invoke("ListPresets");
            return p.Response;
        }

        [RcpSynchronousCommand("GetPresetInfo")]
        public string[] GetPresetInfo(string id)
        {
            var p = Invoke("GetPresetInfo", id);
            return p.Response;
        }

        [RcpSynchronousCommand("PlayPreset")]
        public string PlayPreset(string id)
        {
            var p = Invoke("PlayPreset", id);
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetPreset")]
        public string SetPreset(string id)
        {
            var p = Invoke("SetPreset", id, "working");
            return p.Response[0];
        }

        [RcpSynchronousCommand("GetWorkingSongInfo")]
        public string[] GetWorkingSongInfo()
        {
            var p = Invoke("GetWorkingSongInfo");
            return p.Response;
        }

        [RcpSynchronousCommand("SetWorkingSongInfo")]
        public string SetWorkingSongInfo(string name, string value)
        {
            var p = Invoke("SetWorkingSongInfo", name, value);
            return p.Response[0];
        }

        [RcpSynchronousCommand("ClearWorkingSong")]
        public string ClearWorkingSong()
        {
            var p = Invoke("ClearWorkingSong");
            return p.Response[0];
        }
        #endregion

        #region  Power State Commands 
        [RcpSynchronousCommand("GetPowerState")]
        public string GetPowerState()
        {
            var p = Invoke("GetPowerState");
            return p.Response[0];
        }

        [RcpSynchronousCommand("SetPowerState")]
        public string SetPowerState(string value, bool reconnect)
        {
            IResponseProcessor p;
            if (reconnect)
            {
                p = Invoke("SetPowerState", value, "yes");
            }
            else
            {
                p = Invoke("SetPowerState", value, "no");
            }

            return p.Response[0];
        }
        #endregion

        #endregion


        #region  IDisposable Support 
        private bool _disposed = false;      // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                }
            }

            _disposed = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}