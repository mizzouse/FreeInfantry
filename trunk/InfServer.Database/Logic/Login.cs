﻿using System;
using System.Collections.Generic;
using System.Linq;

using InfServer.Data;
using InfServer.Network;
using InfServer.Protocol;

namespace InfServer.Logic
{	// Logic_Login Class
	/// Handles everything related to the zone server's login
	///////////////////////////////////////////////////////
	class Logic_Login
	{	
		/// <summary>
		/// Handles the zone login request packet 
		/// </summary>
		static public void Handle_CS_Auth(CS_Auth<Zone> pkt, Client<Zone> client)
		{	//Note the login request
			Log.write(TLog.Normal, "Login request from ({0}): {1} / {2}", client._ipe, pkt.zoneID, pkt.password);

			//Attempt to find the associated zone
			DBServer server = client._handler as DBServer;
			Data.DB.zone dbZone;

			using (InfantryDataContext db = server.getContext())
				dbZone = db.zones.SingleOrDefault(z => z.id == pkt.zoneID);
			
			//Does the zone exist?
			if (dbZone == null)
			{	//Reply with failure
				SC_Auth<Zone> reply = new SC_Auth<Zone>();

				reply.result = SC_Auth<Zone>.LoginResult.Failure;
				reply.message = "Invalid zone.";

				client.sendReliable(reply);
				return;
			}

			//Are the passwords a match?
			if (dbZone.password != pkt.password)
			{	//Oh dear.
				SC_Auth<Zone> reply = new SC_Auth<Zone>();
				reply.result = SC_Auth<Zone>.LoginResult.BadCredentials;
				client.sendReliable(reply);
				return;
			}

			//Great! Escalate our client object to a zone
			Zone zone = new Zone(client, server, dbZone);
			client._obj = zone;

			server._zones.Add(zone);
			zone._client.Destruct += delegate(NetworkClient nc)
			{
				server._zones.Remove(zone);
                //Remove them from the base db server list.
                foreach (var player in zone._players.Values)
                {
                    zone._server.lostPlayer(player);
                }
			};

			//Success!
			SC_Auth<Zone> success = new SC_Auth<Zone>();
            
			success.result = SC_Auth<Zone>.LoginResult.Success;
		    success.message = dbZone.notice;

			client.sendReliable(success);

            using (InfantryDataContext db = zone._server.getContext())
            {
                //Update and activate the zone for our directory server
                //TODO: Don't know why it only works like this,
                //modifying dbZone and submitting changes doesn't reflect
                //in the database right away
                Data.DB.zone zoneentry = db.zones.SingleOrDefault(z => z.id == pkt.zoneID);
                zoneentry.name = pkt.zoneName;
                zoneentry.description = pkt.zoneDescription;
                zoneentry.ip = pkt.zoneIP;
                zoneentry.port = pkt.zonePort;
                zoneentry.advanced = Convert.ToInt16(pkt.zoneIsAdvanced);
                zoneentry.active = 1;
                db.SubmitChanges();
            }
			Log.write("Successful login from {0} ({1})", dbZone.name, client._ipe);
		}

		/// <summary>
		/// Handles the zone login request packet 
		/// </summary>
		static public void Handle_CS_PlayerLogin(CS_PlayerLogin<Zone> pkt, Zone zone)
		{	//Make a note
			Log.write(TLog.Inane, "Player login request for {0} on {1}", pkt.alias, zone);

			using (InfantryDataContext db = zone._server.getContext())
			{
				SC_PlayerLogin<Zone> plog = new SC_PlayerLogin<Zone>();

				plog.player = pkt.player;

				//Are they using the launcher?
				if (pkt.ticketid == "")
				{	//They're trying to trick us, jim!
					plog.bSuccess = false;
					plog.loginMessage = "Please use the Infantry launcher to run the game.";

					zone._client.send(plog);
					return;
				}
                if (pkt.ticketid.Contains(':'))
                {   //They're using the old, outdated launcher
                    plog.bSuccess = false;
                    plog.loginMessage = "Please use the updated launcher from the website";

                    zone._client.send(plog);
                    return;
                }

				Data.DB.account account = db.accounts.SingleOrDefault(acct => acct.ticket.Equals(pkt.ticketid));

				if (account == null)
				{	//They're trying to trick us, jim!
					plog.bSuccess = false;
					plog.loginMessage = "Your session id has expired. Please relogin.";

					zone._client.send(plog);
					return;
				}

				//Is there already a player online under this account?
				if (DBServer.bAllowMulticlienting && zone._server._zones.Any(z => z.hasAccountPlayer(account.id)))
				{	
					plog.bSuccess = false;
					plog.loginMessage = "Account is currently in use.";

					zone._client.send(plog);
					return;
				}
				
				//We have the account associated!
				plog.permission = (PlayerPermission)account.permission;

				//Attempt to find the related alias
				Data.DB.alias alias = db.alias.SingleOrDefault(a => a.name == pkt.alias);
				Data.DB.player player = null;
				Data.DB.stats stats = null;

				//Is there already a player online under this alias?
				if (alias != null && zone._server._zones.Any(z => z.hasAliasPlayer(alias.id)))
				{	
					plog.bSuccess = false;
					plog.loginMessage = "Alias is currently in use.";

					zone._client.send(plog);
					return;
				}

				if (alias == null && !pkt.bCreateAlias)
				{	//Prompt him to create a new alias
					plog.bSuccess = false;
					plog.bNewAlias = true;

					zone._client.send(plog);
					return;
				}
				else if (alias == null && pkt.bCreateAlias)
				{	//We want to create a new alias! Do it!
					alias = new InfServer.Data.DB.alias();

					alias.name = pkt.alias;
					alias.creation = DateTime.Now;
					alias.account1 = account;
                    alias.IPAddress = pkt.ipaddress;

					db.alias.InsertOnSubmit(alias);

					Log.write("Creating new alias {0} on account {1}", pkt.alias, account.name);
				}
				else if (alias != null)
				{	//We can't recreate an existing alias or login to one that isn't ours..
					if (pkt.bCreateAlias || 
						alias.account1 != account)
					{
						plog.bSuccess = false;
						plog.loginMessage = "The specified alias already exists.";

						zone._client.send(plog);
						return;
					}
				}

				//Do we have a player row for this zone?
				player = db.players.SingleOrDefault(
					plyr => plyr.alias1 == alias && plyr.zone1 == zone._zone);

				if (player == null)
				{	//We need to create another!
					player = new InfServer.Data.DB.player();

					player.squad1 = null;
					player.zone = zone._zone.id;
					player.alias1 = alias;

					player.lastAccess = DateTime.Now;
					player.permission = 0;

					//Create a blank stats row
					stats = new InfServer.Data.DB.stats();

					stats.zone = zone._zone.id;
					player.stats1 = stats;

					db.stats.InsertOnSubmit(stats);
					db.players.InsertOnSubmit(player);

					//It's a first-time login, so no need to load stats
					plog.bSuccess = true;
					plog.bFirstTimeSetup = true;
				}
				else
				{	//Load the player details and stats!
					plog.banner = player.banner;
					plog.permission = (PlayerPermission)Math.Max(player.permission, (int)plog.permission);
					plog.squad = (player.squad1 == null) ? "" : player.squad1.name;
					plog.bSuccess = true;

					stats = player.stats1;

					plog.stats.zonestat1 = stats.zonestat1;
					plog.stats.zonestat2 = stats.zonestat2;
					plog.stats.zonestat3 = stats.zonestat3;
					plog.stats.zonestat4 = stats.zonestat4;
					plog.stats.zonestat5 = stats.zonestat5;
					plog.stats.zonestat6 = stats.zonestat6;
					plog.stats.zonestat7 = stats.zonestat7;
					plog.stats.zonestat8 = stats.zonestat8;
					plog.stats.zonestat9 = stats.zonestat9;
					plog.stats.zonestat10 = stats.zonestat10;
					plog.stats.zonestat11 = stats.zonestat11;
					plog.stats.zonestat12 = stats.zonestat12;

					plog.stats.kills = stats.kills;
					plog.stats.deaths = stats.deaths;
					plog.stats.killPoints = stats.killPoints;
					plog.stats.deathPoints = stats.deathPoints;
					plog.stats.assistPoints = stats.assistPoints;
					plog.stats.bonusPoints = stats.bonusPoints;
					plog.stats.vehicleKills = stats.vehicleKills;
					plog.stats.vehicleDeaths = stats.vehicleDeaths;
					plog.stats.playSeconds = stats.playSeconds;

					plog.stats.cash = stats.cash;
					plog.stats.inventory = new List<PlayerStats.InventoryStat>();
					plog.stats.experience = stats.experience;
					plog.stats.experienceTotal = stats.experienceTotal;
					plog.stats.skills = new List<PlayerStats.SkillStat>();

					//Convert the binary inventory/skill data
					if (player.inventory != null)
						DBHelpers.binToInventory(plog.stats.inventory, player.inventory);
					if (player.skills != null)
						DBHelpers.binToSkills(plog.stats.skills, player.skills);

					plog.bSuccess = true;
				}
                //Rename him
                plog.alias = alias.name;
				zone._client.sendReliable(plog);

                //Modify his alias IP address and access times
                alias.IPAddress = pkt.ipaddress.Trim();
                alias.lastAccess = DateTime.Now;

                //Submit our modifications
                db.SubmitChanges();

				//Consider him loaded!
				zone.newPlayer(pkt.player.id, alias.name, player);
				Log.write("Player {0} logged into zone {1}", alias.name, zone._zone.name);
			}
		}

		/// <summary>
		/// Handles a player leave notification 
		/// </summary>
		static public void Handle_CS_PlayerLeave(CS_PlayerLeave<Zone> pkt, Zone zone)
		{	//He's gone!
			Log.write("Player {0} left zone {1}", pkt.alias, zone._zone.name);

            using (InfantryDataContext db = zone._server.getContext())
            {
                Data.DB.alias alias = db.alias.SingleOrDefault(a => a.name == pkt.alias);
                TimeSpan ts = DateTime.Now - alias.lastAccess.Value;
                Int64 minutes = ts.Minutes;
                alias.timeplayed += minutes;

                db.SubmitChanges();
            }

			zone.lostPlayer(pkt.player.id);
		}

        /// <summary>
		/// Handles a graceful zone disconnect
		/// </summary>
        static public void Handle_Disconnect(Disconnect<Zone> pkt, Zone zone)
        {
            zone.destroy();
        }

		/// <summary>
		/// Registers all handlers
		/// </summary>
		[RegistryFunc]
		static public void Register()
		{
			CS_Auth<Zone>.Handlers += Handle_CS_Auth;
			CS_PlayerLogin<Zone>.Handlers += Handle_CS_PlayerLogin;
			CS_PlayerLeave<Zone>.Handlers += Handle_CS_PlayerLeave;
            Disconnect<Zone>.Handlers += Handle_Disconnect;
		}
	}
}
