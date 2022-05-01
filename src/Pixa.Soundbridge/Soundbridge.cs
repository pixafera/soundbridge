using Pixa.Soundbridge.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pixa.Soundbridge {

    /// <summary>
    /// Represents a Soundbridge and provides an object-oriented API to communicate
    /// with it.
    /// </summary>
    /// <remarks></remarks>
    public class Soundbridge : SoundbridgeObject {

        /// <summary>
        /// Initialises a new instance of <see cref="Soundbridge"/>.
        /// </summary>
        /// <param name="client">The <see cref="ISoundbridgeClient"/> to use.</param>
        /// <remarks></remarks>
        public Soundbridge(ISoundbridgeClient client) : base(client) {

            _display = new SoundbridgeDisplay(this);
            client.AwaitingReply += Client_AwaitingReply;
            client.ReceivingData += Client_ReceivingData;
            client.SendingRequest += Client_SendingRequest;
            client.IRKeyDown += Client_IRKeyDown;
            client.IRKeyUp += Client_IRKeyUp;
            if (client.GetProgressMode() == ProgressMode.Off)
                client.SetProgressMode(ProgressMode.Verbose);
            Cache.RegisterCache(new SoundbridgeMediaServerCacheProvider(this, this));
        }

        #region  Connection 
        public void Close() {
            IDisposable c = Client as IDisposable;
            if (c is object) {
                c.Dispose();
            }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                Cache.DeregisterCache(this, typeof(MediaServer));
                Close();
            }
        }
        #endregion

        #region  Byte Arrays 
        // Converts a binary response into a Byte array.
        internal static byte[] ResponseToByteArray(string response) {
            if (response.Length % 2 != 0)
                throw new ArgumentException("response must have an even length", "response");
            if (Regex.IsMatch(response, "[^0-9a-fA-F]"))
                throw new ArgumentException("response can only contain digits and letters A-F", "response");
            var b = new byte[(int)Math.Round(response.Length / 2d + 1)];
            for (int i = 0, loopTo = (int)Math.Round(response.Length / 2d); i <= loopTo; i++)
                byte.TryParse(response.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out b[i]);
            return b;
        }
        #endregion

        #region  Lists 
        private IList _activeList;

        /// <summary>
        /// Gets and sets the active list.
        /// </summary>
        /// <value>The most recently requested list.</value>
        internal IList ActiveList {
            get {
                return _activeList;
            }

            set {
                _activeList = value;
            }
        }
        #endregion

        #region  Servers 
        private MediaServer _connectedServer;

        /// <summary>
        /// Gets the <see cref="MediaServer"/> that the soundbridge is currently
        /// connected to.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public MediaServer ConnectedServer {
            get {
                return _connectedServer;
            }

            internal set {
                _connectedServer = value;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="MediaServer"/>s that the <see cref="Soundbridge"/>
        /// can connect to.
        /// </summary>
        /// <returns>The list of available <see cref="MediaServer"/>s.</returns>
        /// <remarks>This method updates the active list.</remarks>
        public SoundbridgeObjectCollection<MediaServer> GetServers() {
            return GetServers(MediaServerType.All);
        }

        /// <summary>
        /// Gets a list of <see cref="MediaServer"/>s that the <see cref="Soundbridge"/>
        /// can connect to matching the specified criteria.
        /// </summary>
        /// <param name="filter">The filter to apply to the list.</param>
        /// <returns>The list of available <see cref="MediaServer"/>s matching the
        /// specified <paramref name="filter"/>.</returns>
        /// <remarks>This method updates the active list.</remarks>
        public SoundbridgeObjectCollection<MediaServer> GetServers(MediaServerType filter) {
            // The filter string is always set to debug, so we always get the additional information
            string filterString = GetFilterString(filter);
            string r = Client.SetServerFilter(filterString);
            if (r != "OK")
                ExceptionHelper.ThrowCommandReturnError("SetServerFilter", r);
            var servers = Client.ListServers();
            if (servers.Length == 1 && servers[0] == "ErrorInitialSetupRequired" | servers[0] == "GenericError")
                ExceptionHelper.ThrowCommandReturnError("ListServers", servers[0]);
            ActiveList = Cache.BuildList<MediaServer>(this, servers);
            return (SoundbridgeObjectCollection<MediaServer>)ActiveList;
        }

        /// <summary>
        /// Converts the specified <see cref="MediaServerType"/> value into a string
        /// for <see cref="ISoundbridgeClient.SetServerFilter"/>.
        /// </summary>
        private string GetFilterString(MediaServerType value) {
            if (value == MediaServerType.All)
                return "debug";
            string filterString = "";
            if (value.HasFlag(MediaServerType.Daap))
                filterString += "daap ";
            if (value.HasFlag(MediaServerType.Upnp))
                filterString += "upnp ";
            if (value.HasFlag(MediaServerType.Rsp))
                filterString += "rsp ";
            if (value.HasFlag(MediaServerType.Slim))
                filterString += "slim ";
            if (value.HasFlag(MediaServerType.Radio))
                filterString += "radio ";
            if (value.HasFlag(MediaServerType.Flash))
                filterString += "flash ";
            if (value.HasFlag(MediaServerType.LineIn))
                filterString += "linein ";
            if (value.HasFlag(MediaServerType.AM))
                filterString += "am ";
            if (value.HasFlag(MediaServerType.FM))
                filterString += "fm ";
            filterString += "debug";
            return filterString;
        }

        /// <summary>
        /// Disconnects the <see cref="Soundbridge"/> from the media server it is
        /// currently connected to.
        /// </summary>
        public void DisconnectServer() {
            string s = Client.ServerDisconnect();
            if (s == "Disconnected") {
                _connectedServer = null;
            }
        }
        #endregion

        #region  Progress 
        /// <summary>
        /// Raised when the <see cref="Soundbridge"/> is waiting for a reply.
        /// </summary>
        public event EventHandler<RcpCommandProgressEventArgs> AwaitingReply;

        /// <summary>
        /// Raised when the <see cref="Soundbridge"/> is receiving data.
        /// </summary>
        public event EventHandler<RcpCommandReceivingProgressEventArgs> ReceivingData;

        /// <summary>
        /// Raised when the <see cref="Soundbridge"/> is sending a request.
        /// </summary>
        public event EventHandler<RcpCommandProgressEventArgs> SendingRequest;

        /// <summary>
        /// Raises the <see cref="AwaitingReply"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <remarks>Subclasses overriding this method should call the base class
        /// method to ensure that the event gets raised.</remarks>
        protected virtual void OnAwaitingReply(RcpCommandProgressEventArgs e) {
            AwaitingReply?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ReceivingData"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <remarks>Subclasses overriding this method should call the base class
        /// method to ensure that the event gets raised.</remarks>
        protected virtual void OnReceivingData(RcpCommandReceivingProgressEventArgs e) {
            ReceivingData?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SendingRequest"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <remarks>Subclasses overriding this method should call the base class
        /// method to ensure that the event gets raised.</remarks>
        protected virtual void OnSendingRequest(RcpCommandProgressEventArgs e) {
            SendingRequest?.Invoke(this, e);
        }

        private void Client_AwaitingReply(object sender, RcpCommandProgressEventArgs e) {
            OnAwaitingReply(e);
        }

        private void Client_ReceivingData(object sender, RcpCommandReceivingProgressEventArgs e) {
            OnReceivingData(e);
        }

        private void Client_SendingRequest(object sender, RcpCommandProgressEventArgs e) {
            OnSendingRequest(e);
        }
        #endregion

        #region  Setup & Config 
        /// <summary>
        /// Gets a value indicating whether the <see cref="Soundbridge"/> has
        /// completed initial setup.
        /// </summary>
        /// <value>True if the Soundbridge has completed initial setup; otherwise,
        /// false.</value>
        public bool InitialSetupComplete {
            get {
                return Client.GetInitialSetupComplete() == "Complete";
            }
        }

        private SetupStepCollection _setupSteps;

        /// <summary>
        /// Gets the list of initial setup steps that must be completed.
        /// </summary>
        public SetupStepCollection SetupSteps {
            get {
                if (InitialSetupComplete)
                    throw new Exception("The Soundbridge has already been set up");
                if (_setupSteps is null) {
                    var f = new SetupStepFactory(Client);
                    var l = new List<SetupStep>();
                    foreach (string s in Client.GetRequiredSetupSteps())
                        l.Add(f.CreateSetupStep(s));
                    _setupSteps = new SetupStepCollection(l);
                }

                return _setupSteps;
            }
        }

        /// <summary>
        /// Gets the Upgrade MAC address of the <see cref="Soundbridge"/>.
        /// </summary>
        public string UpgradeMac {
            get {
                return Client.GetMacAddress("upgr");
            }
        }

        /// <summary>
        /// Gets and sets the date and time according to the <see cref="Soundbridge"/>.
        /// </summary>
        public DateTime LocalTime {
            get {
                string d = Client.GetDate(false);
                string t = Client.GetTime(false);
                return DateTime.Parse(d + " " + t);
            }

            set {
                Client.SetTime(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                Client.SetDate(value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets the firmware version of the <see cref="Soundbridge"/>.
        /// </summary>
        public Version SoftwareVersion {
            get {
                return new Version(Client.GetSoftwareVersion());
            }
        }

        /// <summary>
        /// Reboots the <see cref="Soundbridge"/>.
        /// </summary>
        /// <remarks></remarks>
        public void Reboot() {
            Client.Reboot();
        }

        /// <summary>
        /// Gets and sets of the <see cref="PowerState"/> of the <see cref="Soundbridge"/>.
        /// </summary>
        public PowerState PowerState {
            get {
                return GetPowerStateValue(Client.GetPowerState());
            }

            set {
                if (value != PowerState) {
                    Client.SetPowerState(GetPowerStateString(value), false);
                }
            }
        }

        // Converts a string value to a PowerState value.
        private PowerState GetPowerStateValue(string value) {
            switch (value ?? "") {
                case "standby": {
                        return PowerState.Standby;
                    }

                case "on": {
                        return PowerState.On;
                    }

                default: {
                        throw new ArgumentException("value must be 'standby' or 'on'", "value");
                    }
            }
        }

        // Converts a power state value to a string.
        private string GetPowerStateString(PowerState value) {
            switch (value) {
                case PowerState.Standby: {
                        return "standby";
                    }

                case PowerState.On: {
                        return "on";
                    }

                default: {
                        throw new ArgumentException("value must be a valid PowerState value", "value");
                    }
            }
        }

        private SoundbridgeDisplay _display;

        /// <summary>
        /// Gets the <see cref="SoundbridgeDisplay"/> object that can be used to
        /// interact with the display of the soundbridge.
        /// </summary>
        public SoundbridgeDisplay Display {
            get {
                return _display;
            }
        }
        #endregion

        #region  IR 
        private EventHandler<IRKeyEventArgs> _irKeyDown;
        private EventHandler<IRKeyEventArgs> _irKeyUp;
        private static Dictionary<IRCommand, string> _irCommandEnumTranslations;
        private static Dictionary<string, IRCommand> _irCommandStringTranslations;
        private static object _irCommandTranslationsLock = new object();

        /// <summary>
        /// Converts an <see cref="IRCommand"/> value into a string to send to the
        /// <see cref="Soundbridge"/>.
        /// </summary>
        /// <param name="command">The value to convert.</param>
        /// <returns>A string value representing the same button as <paramref name="command"/>.</returns>
        public static string GetIrCommandTranslation(IRCommand command) {
            lock (_irCommandTranslationsLock) {
                if (_irCommandEnumTranslations is null) {
                    BuildTranslationDictionaries();
                }

                return _irCommandEnumTranslations[command];
            }
        }

        /// <summary>
        /// Converts a string value from the <see  cref="Soundbridge"/> into an <see cref="IRCommand"/>
        /// value.
        /// </summary>
        /// <param name="command">The value to convert.</param>
        public static IRCommand GetIrCommandTransaction(string command) {
            lock (_irCommandTranslationsLock) {
                if (_irCommandStringTranslations is null) {
                    BuildTranslationDictionaries();
                }

                return _irCommandStringTranslations[command];
            }
        }

        private static void BuildTranslationDictionaries() {
            var i = default(IRCommand);
            _irCommandEnumTranslations = new Dictionary<IRCommand, string>();
            _irCommandStringTranslations = new Dictionary<string, IRCommand>();
            foreach (FieldInfo f in typeof(IRCommand).GetFields()) {
                IRCommandStringAttribute[] atts = (IRCommandStringAttribute[])f.GetCustomAttributes(typeof(IRCommandStringAttribute), false);
                if (atts.Length == 1) {
                    _irCommandEnumTranslations.Add((IRCommand)f.GetValue(i), atts[0].CommandString);
                    _irCommandStringTranslations.Add(atts[0].CommandString, (IRCommand)f.GetValue(i));
                }
            }
        }

        /// <summary>
        /// Raised when a key on the IR remote is pressed.
        /// </summary>
        public event EventHandler<IRKeyEventArgs> IRKeyDown {
            add {
                _irKeyDown = (EventHandler<IRKeyEventArgs>)Delegate.Combine(_irKeyDown, value);
                if (_irKeyDown is object)
                    Client.IRDemodSubscribe(true);
            }

            remove {
                _irKeyDown = (EventHandler<IRKeyEventArgs>)Delegate.Remove(_irKeyDown, value);
                if (_irKeyDown is null)
                    Client.IRDemodUnsubscribe();
            }
        }

        void OnIRKeyDown(object sender, IRKeyEventArgs e) {
            _irKeyDown(sender, e);
        }

        /// <summary>
        /// Raised when a key on the IR remote is depressed.
        /// </summary>
        public event EventHandler<IRKeyEventArgs> IRKeyUp {
            add {
                _irKeyUp = (EventHandler<IRKeyEventArgs>)Delegate.Combine(_irKeyUp, value);
                if (_irKeyUp is object)
                    Client.IRDemodSubscribe(true);
            }

            remove {
                _irKeyUp = (EventHandler<IRKeyEventArgs>)Delegate.Remove(_irKeyUp, value);
                if (_irKeyUp is null)
                    Client.IRDemodUnsubscribe();
            }
        }

        void OnIRKeyUp(object sender, IRKeyEventArgs e) {
            _irKeyUp(sender, e);
        }

        /// <summary>
        /// Dispatches the specified <see cref="IRCommand"/> to the <see cref="Soundbridge"/>.
        /// </summary>
        /// <param name="command">The <see cref="IRCommand"/> to execute.</param>
        public void DispatchIrCommand(IRCommand command) {
            string r = Client.IRDispatchCommand(GetIrCommandTranslation(command));
            if (r != "OK")
                ExceptionHelper.ThrowCommandReturnError("IrDispatchCommand", r);
        }

        private void Client_IRKeyDown(string data) {
            var e = new IRKeyEventArgs(this, (IRCommand)int.Parse(GetIrCommandTranslation((IRCommand)int.Parse(data))));
            OnIRKeyDown(this, e);
            if (!e.IsHandled) {
                DispatchIrCommand(e.Command);
            }
        }

        private void Client_IRKeyUp(string data) {
            var e = new IRKeyEventArgs(this, (IRCommand)int.Parse(GetIrCommandTranslation((IRCommand)int.Parse(data))));
            OnIRKeyUp(this, e);
        }
        #endregion

        #region  Cache 
        private SoundbridgeCache _cache = new SoundbridgeCache();

        internal SoundbridgeCache Cache {
            get {
                return _cache;
            }
        }
        #endregion

    }
}