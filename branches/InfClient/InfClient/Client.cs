using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using InfServer;
using InfServer.Network;
using InfServer.Data;
using InfServer.Protocol;

namespace InfClient
{
    public partial class InfClient : IClient
    {
        public new LogClient _logger;
        private ClientConn<InfClient> _conn;		//Our UDP connection client

        public ManualResetEvent _syncStart;		//Used for blocking connect attempts
        public ConfigSetting _config;			//Our database-specific settings
        public bool _bLoginSuccess;				//Were we able to successfully login?

        /// <summary>
        /// Constructor
        /// </summary>
        public InfClient()
        {
            _conn = new ClientConn<InfClient>(new S2CPacketFactory<InfClient>(), this);
			_syncStart = new ManualResetEvent(false);

            //Log packets for now..
            _conn._bLogPackets = true;

            _logger = Log.createClient("Client");
			_conn._logger = _logger;
        }

        //Place holder atm.
        public bool init()
        {
            Log.write("Client initializing..");
            return true;
        }


        /// <summary>
        /// Called when making a connection to a zoneserver
        /// </summary>
        public bool connect(IPEndPoint sPoint)
        {
            bool bConnected = true;

            using (LogAssume.Assume(_logger))
            {
                Log.write("Connecting to Infantry server..");

                //Start our connection
                _conn.begin(sPoint);

                //Send our initial packet
                CS_Initial init = new CS_Initial();

                _conn._client._connectionID = init.connectionID = new Random().Next();
                init.CRCLength = Client.crcLength;
                init.udpMaxPacket = Client.udpMaxSize;

                _conn._client.send(init);

            }

            return bConnected;
        }


        static public void Handle_SC_Initial(SC_Initial pkt, Client client)
        {	//We will be sending out state packet only once, as the sync doesn't
            //really matter on the Zone -> Database connection
            CS_State csi = new CS_State();

            csi.tickCount = (ushort)Environment.TickCount;
            csi.packetsSent = client._packetsSent;
            csi.packetsReceived = client._packetsReceived;

            client.send(csi);
        }

        /// <summary>
        /// Registers all handlers
        /// </summary>
        [InfServer.Logic.RegistryFunc]
        static public void Register()
        {
            SC_Initial.Handlers += Handle_SC_Initial;
        }



        public void destroy()
        {
        }
    }
}
