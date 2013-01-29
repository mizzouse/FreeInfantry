using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using InfServer.Logic.Events;
using InfServer.Network;
using InfServer.Protocol;
using InfServer.Data;

using Assets;


namespace InfServer.Game
{

	// ZoneServer Class
	/// Represents the entire server state
	///////////////////////////////////////////////////////
	public partial class ZoneServer : Server
	{	// Member variables
		///////////////////////////////////////////////////
		public ConfigSetting _config;			//Our server config
		public CfgInfo _zoneConfig;				//The zone-specific configuration file

		public AssetManager _assets;
		public Bots.Pathfinder _pathfinder;		//Global pathfinder

		public Database _db;					//Our connection to the database

        public IPEndPoint _dbEP;

		public new LogClient _logger;			//Our zone server log

		private bool _bStandalone;				//Are we in standalone mode?

        private string _name;                    //The zones name
        private string _description;             //The zones description
        private bool _isAdvanced;                //Is the zone normal/advanced?
        private string _bindIP;                  //The IP the zone is binded to
        private int _bindPort;                   //The port the zone is binded to

        private LogClient _dbLogger;
        public int _lastDBAttempt;
        public int _attemptDelay;

	    private ClientPingResponder _pingResponder;
        public Dictionary<IPAddress, DateTime> _connections;

        public Dictionary<string, Dictionary<string, DateTime>> _arenaBans; //Our arena banning list

        /// <summary>
        /// Compiled game events that have been pulled out of the zone's cfg file.
        /// </summary>
	    public Dictionary<string, GameEvent> GameEvents;

        ///////////////////////////////////////////////////
        // Accessors
        ///////////////////////////////////////////////////
        /// <summary>
        /// Gets the name of the zone
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the description of the zone
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Gets whether this zone is advanced
        /// </summary>
        public bool IsAdvanced
        {
            get
            {
                return _isAdvanced;
            }
        }

		/// <summary>
		/// Indicates whether the server is in standalone (no database) mode
		/// </summary>
		public bool IsStandalone
		{
			get
			{
				return _bStandalone;
			}

			set
			{
				//TODO: Kick all players from the server, etc
                //Disconnect everyone.
                foreach (var arena in _arenas)
                    foreach (Player p in arena.Value.Players)
                        p.disconnect();
			}
		}

        /// <summary>
        /// Gets the current IP this instance of the zoneserver is running on
        /// </summary>
        public string IP
        {
            get
            {
                return _bindIP;
            }
        }

        /// <summary>
        /// Gets the current port this instance of the zoneserver is binded to
        /// </summary>
        public int Port
        {
            get
            {
                return _bindPort;
            }
        }

		///////////////////////////////////////////////////
		// Member Functions
		///////////////////////////////////////////////////
		/// <summary>
		/// Generic constructor
		/// </summary>
		public ZoneServer()
			: base(new PacketFactory(), new Client<Player>(false))
		{
			_config = ConfigSetting.Blank;
		}

		/// <summary>
		/// Allows the server to preload all assets.
		/// </summary>
		public bool init()
		{	// Load configuration
			///////////////////////////////////////////////
			//Load our server config
            _connections = new Dictionary<IPAddress, DateTime>();
            Log.write(TLog.Normal, "Loading Server Configuration");
			_config = new Xmlconfig("server.xml", false).Settings;

			//Load our zone config
            Log.write(TLog.Normal, "Loading Zone Configuration");

			string filePath = AssetFileFactory.findAssetFile(_config["server/zoneConfig"].Value, "assets\\");
			if (filePath == null)
			{
				Log.write(TLog.Error, "Unable to find config file '" + _config["server/zoneConfig"].Value + "'.");
				return false;
			}

			_zoneConfig = CfgInfo.Load(filePath);

			//Load assets from zone config and populate AssMan
			try
			{
				_assets = new AssetManager();

				_assets.bUseBlobs = _config["server/loadBlobs"].boolValue;

                //Grab the latest global news if specified
                if (_config["server/updateGlobalNws"].Value.Length > 0)
                {
                    Log.write(TLog.Normal, String.Format("Grabbing latest global news from {0}...", _config["server/updateGlobalNws"].Value));
                    if (!_assets.grabGlobalNews(_config["server/updateGlobalNws"].Value, "..\\Global\\global.nws"))
                    {
                        try
                        {
                            string global;
                            if ((global = Assets.AssetFileFactory.findAssetFile("\\global.nws", _config["server/copyServerFrom"].Value)) != null)
                                System.IO.File.Copy(global, "..\\Global\\global.nws");
                        }
                        catch (System.UnauthorizedAccessException)
                        {
                        }
                    }
                }

				if (!_assets.load(_zoneConfig, _config["server/zoneConfig"].Value))
				{	//We're unable to continue
					Log.write(TLog.Error, "Files missing, unable to continue.");
					return false;
				}
			}
			catch (System.IO.FileNotFoundException ex)
			{	//Report and abort
				Log.write(TLog.Error, "Unable to find file '{0}'", ex.FileName);
				return false;
			}

			//Make sure our protocol helpers are aware
			Helpers._server = this;

			//Load protocol config settings
			base._bLogPackets = _config["server/logPackets"].boolValue;
			Client.udpMaxSize = _config["protocol/udpMaxSize"].intValue;
			Client.crcLength = _config["protocol/crcLength"].intValue;
			if (Client.crcLength > 4)
			{
				Log.write(TLog.Error, "Invalid protocol/crcLength, must be less than 4.");
				return false;
			}

			Client.connectionTimeout = _config["protocol/connectionTimeout"].intValue;
			Client.bLogUnknowns = _config["protocol/logUnknownPackets"].boolValue;

			ClientConn<Database>.clientPingFreq = _config["protocol/clientPingFreq"].intValue;


			// Load scripts
			///////////////////////////////////////////////
			Log.write("Loading scripts..");

			//Obtain the bot and operation types
			ConfigSetting scriptConfig = new Xmlconfig("scripts.xml", false).Settings;
			IList<ConfigSetting> scripts = scriptConfig["scripts"].GetNamedChildren("type");

			//Load the bot types
			List<Scripting.InvokerType> scriptingBotTypes = new List<Scripting.InvokerType>();

			foreach (ConfigSetting cs in scripts)
			{	//Convert the config entry to a bottype structure
				scriptingBotTypes.Add(
					new Scripting.InvokerType(
							cs.Value,
							cs["inheritDefaultScripts"].boolValue,
							cs["scriptDir"].Value)
				);
			}

			//Load them into the scripting engine
			Scripting.Scripts.loadBotTypes(scriptingBotTypes); 

			try
			{	//Loads!
				bool bSuccess = Scripting.Scripts.compileScripts();
				if (!bSuccess)
				{	//Failed. Exit
					Log.write(TLog.Error, "Unable to load scripts.");
					return false;
				}
			}
			catch (Exception ex)
			{	//Error while compiling
				Log.write(TLog.Exception, "Exception while compiling scripts:\n" + ex.ToString());
				return false;
			}

            if (_config["server/pathFindingEnabled"].boolValue)
            {
                Log.write("Initializing pathfinder..");
                _pathfinder = new Bots.Pathfinder(this);
                _pathfinder.beginThread();
            }
            else
            {
                Log.write("Pathfinder disabled, skipping..");
            }

            // Sets the zone settings
            //////////////////////////////////////////////
            _name = _config["server/zoneName"].Value;
            _description = _config["server/zoneDescription"].Value;
            _isAdvanced = _config["server/zoneIsAdvanced"].boolValue;
            _bindIP = _config["server/bindIP"].Value;
            _bindPort = _config["server/bindPort"].intValue;

			// Connect to the database
			///////////////////////////////////////////////
			//Attempt to connect to our database

            _dbLogger = Log.createClient("Database");
			_db = new Database(this, _config["database"], _dbLogger);
            _attemptDelay = _config["database/connectionDelay"].intValue;
            _dbEP = new IPEndPoint(IPAddress.Parse(_config["database/ip"].Value), _config["database/port"].intValue);

            _db.connect(_dbEP, true);

			//Initialize other parts of the zoneserver class
			if (!initPlayers())
				return false;
			if (!initArenas())
				return false;

            // Create the ping/player count responder
            //////////////////////////////////////////////
		    _pingResponder = new ClientPingResponder(_players);

            Log.write("Asset Checksum: " + _assets.checkSum());

            //Create a new banning list
            _arenaBans = new Dictionary<string, Dictionary<string, DateTime>>();

            InitializeGameEventsDictionary();

			return true;
		}

		/// <summary>
		/// Begins all server processes, and starts accepting clients.
		/// </summary>
		public void begin()
		{	//Start up the network
			_logger = Log.createClient("Zone");
			base._logger = Log.createClient("Network");

			IPEndPoint listenPoint = new IPEndPoint(
                IPAddress.Parse("0.0.0.0"), _bindPort);
			base.begin(listenPoint);

            _pingResponder.Begin(new IPEndPoint(IPAddress.Parse("0.0.0.0"), _bindPort + 1));

			//Start handling our arenas;
			using (LogAssume.Assume(_logger))
				handleArenas();
		}
        
        //Handles baseserver operation..
        public void poll()
        {
            int now = Environment.TickCount;

            try
            {
                if (_connections != null)
                {
                    foreach (KeyValuePair<IPAddress, DateTime> pair in _connections.ToList())
                    {
                        if (DateTime.Now > pair.Value)
                        { // Delete this entry
                            _connections.Remove(pair.Key);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Log.write(TLog.Warning, e.ToString());
            }
            //Is it time to make another attempt at connecting to the database?
            if ((now - _lastDBAttempt) > _attemptDelay && _lastDBAttempt != 0)
            {
                //Are we connected to the database currently? If so break out of this operation
                if (_db._bLoginSuccess || _attemptDelay == 0)
                    return;


                _db = new Database(this, _config["Database"], _dbLogger);
                _lastDBAttempt = now;
                //Take a stab at connecting
                if (!_db.connect(_dbEP, true))
                {//it has failed!
                    Log.write("Fail");
                    //Send out some message to all of the server's players
                    foreach (var arena in _arenas)
                    {
                        if (arena.Value._bActive)
                            arena.Value.sendArenaMessage("!An attempt to establish a connection with the database has failed, The server remains in Stand Alone Mode");
                    }
                }
                else
                {//Success!
                    //Send out some message to all of the server's players
                    foreach (var arena in _arenas)
                    {
                        //Let them know to reconnect
                        if (arena.Value._bActive)
                            arena.Value.sendArenaMessage("!Connection to the database has been re-established, Please relog to continue playing..");

                        //Disconnect everyone.
                        foreach (Player p in arena.Value.Players)
                            p.disconnect();
                    }
                }
            }
        }

        /// <summary>
        /// Recycles our zoneserver
        /// </summary>
        public void recycle()
        {
            //Loop through each arena and save stats for each player in that arena.
            Dictionary<string, Arena> alist = _arenas;
            foreach (KeyValuePair<string, Arena> arena in alist)
            {
                IEnumerable<Player> plist = arena.Value.Players;
                foreach (Player p in plist)
                {
                    //Update his stats first
                    _db.updatePlayer(p);
                }
            }

            //Disconnect from the database gracefully..
            _db.send(new Disconnect<Database>());

            //Add a little delay...
            Thread.Sleep(2000);

            //Restart!
            InfServer.Program.Restart();
        }

        private void InitializeGameEventsDictionary()
        {
            var e = _zoneConfig.EventInfo;
            GameEvents = new Dictionary<string, GameEvent>();

            //
            // Compile the event strings into game events/actions.
            //

            GameEvents["jointeam"]              = EventsActionsFactory.CreateGameEventFromString(e.joinTeam);
            GameEvents["joinspectatormode"]     = EventsActionsFactory.CreateGameEventFromString(e.exitSpectatorMode);
            GameEvents["endgame"]               = EventsActionsFactory.CreateGameEventFromString(e.endGame);
            GameEvents["soongame"]              = EventsActionsFactory.CreateGameEventFromString(e.soonGame);
            GameEvents["manualjointeam"]        = EventsActionsFactory.CreateGameEventFromString(e.manualJoinTeam);
            GameEvents["startgame"]             = EventsActionsFactory.CreateGameEventFromString(e.startGame);
            GameEvents["sysopwipe"]             = EventsActionsFactory.CreateGameEventFromString(e.sysopWipe);
            GameEvents["selfwipe"]              = EventsActionsFactory.CreateGameEventFromString(e.selfWipe);
            GameEvents["killedteam"]            = EventsActionsFactory.CreateGameEventFromString(e.killedTeam);
            GameEvents["killedenemy"]           = EventsActionsFactory.CreateGameEventFromString(e.killedEnemy);
            GameEvents["killedbyteam"]          = EventsActionsFactory.CreateGameEventFromString(e.killedByTeam);
            GameEvents["killedbyenemy"]         = EventsActionsFactory.CreateGameEventFromString(e.killedByEnemy);
            GameEvents["firsttimeinvsetup"]     = EventsActionsFactory.CreateGameEventFromString(e.firstTimeInvSetup);
            GameEvents["firsttimeskillsetup"]   = EventsActionsFactory.CreateGameEventFromString(e.firstTimeSkillSetup);
            GameEvents["hold1"]                 = EventsActionsFactory.CreateGameEventFromString(e.hold1);  
			GameEvents["hold2"]                 = EventsActionsFactory.CreateGameEventFromString(e.hold2);  
            GameEvents["hold3"]                 = EventsActionsFactory.CreateGameEventFromString(e.hold3);  
            GameEvents["hold4"]                 = EventsActionsFactory.CreateGameEventFromString(e.hold4);
            GameEvents["enterspawnnoscore"]     = EventsActionsFactory.CreateGameEventFromString(e.enterSpawnNoScore);
            GameEvents["changedefaultvehicle"]  = EventsActionsFactory.CreateGameEventFromString(e.changeDefaultVehicle);
        }


		///////////////////////////////////////////////////
		// Member Classes
		///////////////////////////////////////////////////
		/// <summary>
		/// Responds to ping and player count requests made by the client. Runs on the port
		/// above the zone server.
		/// </summary>
		private class ClientPingResponder
		{
			private Dictionary<ushort, Player> _players;
			private Thread _listenThread;
			private Socket _socket;
			private Dictionary<EndPoint, Int32> _clients;
			private Boolean _isOperating;
			private ReaderWriterLock _lock;
			private byte[] _buffer;

			public ClientPingResponder(Dictionary<ushort, Player> players)
			{
				_players = players;
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				_clients = new Dictionary<EndPoint, Int32>();
				_lock = new ReaderWriterLock();
				_buffer = new byte[4];
			}

			public void Begin(IPEndPoint listenPoint)
			{
				_listenThread = new Thread(Listen);
				_listenThread.IsBackground = true;
				_listenThread.Name = "ClientPingResponder";
				_listenThread.Start(listenPoint);
			}

			private void Listen(Object obj)
			{
				var listenPoint = (IPEndPoint)obj;
				EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

				//Prevent useless connection reset exceptions
				uint IOC_IN = 0x80000000;
				uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				_socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

				_socket.Bind(listenPoint);
				_socket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref remoteEP, OnRequestReceived, null);

				_isOperating = true;

				// Do we have clients to service?
				while (_isOperating)
				{
					Dictionary<EndPoint, Int32> queue = null;
					_lock.AcquireWriterLock(Timeout.Infinite);

					// Swap the queue
					try
					{
						queue = _clients;
						_clients = new Dictionary<EndPoint, Int32>();
					}
					finally
					{
						_lock.ReleaseWriterLock();
					}

					if (queue != null && queue.Count != 0)
					{
						// May not be synchronized, but that's okay, the client requests often.
						byte[] playerCount = BitConverter.GetBytes(_players.Count);

						foreach (var entry in queue)
						{
							// TODO: Refactor this into something cultured
							EndPoint client = entry.Key;
							byte[] token = BitConverter.GetBytes(entry.Value);

							byte[] buffer = new[]
                                                {
                                                    playerCount[0], playerCount[1], playerCount[2], playerCount[3], 
                                                    token[0], token[1], token[2], token[3]
                                                };

							_socket.SendTo(buffer, client);
						}
					}

					Thread.Sleep(10);
				}
			}

			private void OnRequestReceived(IAsyncResult result)
			{
				if (!result.IsCompleted)
				{
					// Continue anyways? Let's do it!
                    //return;
				}

				EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                int read = 4;
                try
                {
                    read = _socket.EndReceiveFrom(result, ref remoteEp);
                }
                catch (SocketException)
                {
                    //Packet is too big. Make note of it and the clients IP
                    Log.write("Malformed packet from client: " + remoteEp.ToString() + " (possible attempt to crash the zone)");
                }

				if (read != 4)
				{
					// Malformed packet, lets continue anyways and log the scums IP
                    Log.write("Malformed packet from client: " + remoteEp.ToString() + " (possible attempt to crash the zone)");
				}

				_lock.AcquireWriterLock(Timeout.Infinite);

				try
				{
					Int32 token = BitConverter.ToInt32(_buffer, 0);
					_clients[remoteEp] = token;
				}
				finally
				{
					_lock.ReleaseWriterLock();
				}
                
				remoteEp = new IPEndPoint(IPAddress.Any, 0);
				_socket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref remoteEp, OnRequestReceived, null);
			}
		}
	}
}
