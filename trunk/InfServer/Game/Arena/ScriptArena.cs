﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using InfServer.Network;
using InfServer.Protocol;
using InfServer.Logic;
using InfServer.Scripting;
using InfServer.Bots;

using Assets;

namespace InfServer.Game
{
	// ScriptArena Class
	/// Exposes the arena methodology to scripting
	///////////////////////////////////////////////////////
	public class ScriptArena : Arena
	{	// Member variables
		///////////////////////////////////////////////////
		private List<Scripts.IScript> _scripts;		//The scripts we're currently supporting
		private string _scriptType;					//The type of scripts we're instancing


		///////////////////////////////////////////////////
		// Member Functions
		///////////////////////////////////////////////////
		/// <summary>
		/// Generic constructor
		/// </summary>
		public ScriptArena(ZoneServer server, string scriptType)
			: base(server)
		{
			_scriptType = scriptType;
		}

		/// <summary>
		/// Initializes arena details
		/// </summary>
		public override void init()
		{	//Initialize the base arena class
			base.init();

            //Initialize our breakdown settings
            _breakdownSettings = new BreakdownSettings();

			//Load the associated scripts
			_scripts = Scripts.instanceScripts(this, _scriptType);
		}

		/// <summary>
		/// Allows the arena to keep it's game state up-to-date
		/// </summary>
		public override void poll()
		{	//Process the base state
			base.poll();
			
			//Poll all scripts!
			foreach (Scripts.IScript script in _scripts)
				script.poll();
		}

		#region Events
		/// <summary>
		/// Called when a player enters the game
		/// </summary>
		public override void playerEnter(Player player)
		{	//Pass it to the script environment
			base.playerEnter(player);
			callsync("Player.Enter", false, player);
		}

		/// <summary>
		/// Called when a player leaves the game
		/// </summary>
		public override void playerLeave(Player player)
		{	//Pass it to the script environment
			base.playerLeave(player);
			callsync("Player.Leave", false, player);
		}

		/// <summary>
		/// Called when the game begins
		/// </summary>
		public override void gameStart()
		{	//We're running!
			_bGameRunning = true;
            _tickGameStarted = Environment.TickCount;
			_tickGameEnded = 0;

			//Reset the flags
			flagReset();

			//What else do we need to reset?
			CfgInfo.StartGame startCfg = _server._zoneConfig.startGame;

			if (startCfg.prizeReset)
				resetItems();

			if (startCfg.vehicleReset)
				resetVehicles();

			if (startCfg.initialHides)
				initialHideSpawns();

			//Handle the start for all players
			string startGame = _server._zoneConfig.EventInfo.startGame;

			foreach (Player player in Players)
			{	//We don't want previous stats to count
				player.clearCurrentStats();

				//Reset anything else we're told to
				if (startCfg.clearProjectiles)
					player.clearProjectiles();

				if (startCfg.resetInventory && startCfg.resetCharacter)
				{
					player.resetInventory(false);
					player.resetSkills(true);
				}
				else if (startCfg.resetCharacter)
					player.resetSkills(true);
				else if (startCfg.resetInventory)
					player.resetInventory(true);

				//Run the event if necessary
				if (!player.IsSpectator)
					Logic_Assets.RunEvent(player, startGame);
			}
			
			//Pass it to the script environment
			callsync("Game.Start", false);
		}

		/// <summary>
		/// Called when the game ends
		/// </summary>
		public override void gameEnd()
		{	//We've stopped
			_bGameRunning = false;
			_tickGameEnded = Environment.TickCount;

			//Reset the game state
			flagReset();
			
			//Execute the end game event
			string endGame = _server._zoneConfig.EventInfo.endGame;
			foreach (Player player in Players)
			{	//Keep the player's game stats updated
				player.migrateStats();
				player.syncState();

				//Run the event if necessary
				if (!player.IsSpectator)
					Logic_Assets.RunEvent(player, endGame);
			}

			//Pass it to the script environment
			callsync("Game.End", false);
		}

		/// <summary>
		/// Called to reset the game state
		/// </summary>
		public override void gameReset()
		{	//Reset the game state
			flagReset();
			resetItems();
			resetVehicles();

			//Pass it to the script environment
			callsync("Game.Reset", false);
		}

		/// <summary>
		/// Creates a breakdown tailored for one player
		/// </summary>
		public override void individualBreakdown(Player from, bool bCurrent)
		{	//Give the script a chance to take over
			if (exists("Player.Breakdown") && ((bool)callsync("Player.Breakdown", false, from, bCurrent)) == false)
				return;

			//Display Team Stats?
			if (_breakdownSettings.bDisplayTeam)
			{
				from.sendMessage(0, "#Team Statistics Breakdown");

				//Make sure stats are up-to-date
				foreach (Team t in _teams.Values)
					t.precalculateStats(bCurrent);

				IEnumerable<Team> activeTeams = _teams.Values.Where(entry => entry.ActivePlayerCount > 0);
				IEnumerable<Team> rankedTeams = activeTeams.OrderByDescending(entry => entry._calculatedKills);
				int idx = 3;	//Only display top three teams

				foreach (Team t in rankedTeams)
				{
					if (idx-- == 0)
						break;

					string format = "!3rd (K={0} D={1}): {2}";

					switch (idx)
					{
						case 2:
							format = "!1st (K={0} D={1}): {2}";
							break;
						case 1:
							format = "!2nd (K={0} D={1}): {2}";
							break;
					}

					from.sendMessage(0, String.Format(format,
						t._calculatedKills, t._calculatedDeaths,
						t._name));
				}
			}

			//Do we want to display individual statistics?
			if (_breakdownSettings.bDisplayIndividual)
			{
				from.sendMessage(0, "#Individual Statistics Breakdown");

				IEnumerable<Player> rankedPlayers = _playersIngame.OrderByDescending(player => (bCurrent ? player.StatsCurrentGame.kills : player.StatsLastGame.kills));
				int idx = 3;	//Only display top three players

				foreach (Player p in rankedPlayers)
				{
					if (idx-- == 0)
						break;

					string format = "!3rd (K={0} D={1}): {2}";

					switch (idx)
					{
						case 2:
							format = "!1st (K={0} D={1}): {2}";
							break;
						case 1:
							format = "!2nd (K={0} D={1}): {2}";
							break;
					}

					from.sendMessage(0, String.Format(format,
						(bCurrent ? p.StatsCurrentGame.kills : p.StatsLastGame.kills),
						(bCurrent ? p.StatsCurrentGame.deaths : p.StatsLastGame.deaths),
						p._alias));
				}
			}
		}

		/// <summary>
		/// Called when the game needs to display end game statistics
		/// </summary>
		public override void breakdown(bool bCurrent)
		{	//Show a breakdown for each player in the arena
			foreach (Player p in Players)
				individualBreakdown(p, bCurrent);
		}

		#region Handlers
		/// <summary>
		/// Triggered when a player requests to pick up an item
		/// </summary>
		public override void handlePlayerPickup(Player from, CS_PlayerPickup update)
		{	//Find the itemdrop in question
			ItemDrop drop;

			if (!_items.TryGetValue(update.itemID, out drop))
				//Doesn't exist
				return;

			//In range?
			if (!Helpers.isInRange(_server._zoneConfig.arena.itemPickupDistance,
									drop.positionX, drop.positionY,
									from._state.positionX, from._state.positionY))
				return;

			//Do we allow pickup?
			if (!_server._zoneConfig.level.allowUnqualifiedPickup &&
				!Logic_Assets.SkillCheck(from, drop.item.skillLogic))
				return;

			//Sanity checks
			if (update.quantity > drop.quantity)
				return;

			//Forward to our script
			if (!exists("Player.ItemPickup") || (bool)callsync("Player.ItemPickup", false, from, drop, update.quantity))
			{
				if (update.quantity == drop.quantity)
				{	//Delete the drop
					_items.Remove(drop.id);
					drop.quantity = 0;
				}
				else
					drop.quantity = (short)(drop.quantity - update.quantity);
                
				//Add the pickup to inventory!
				from.inventoryModify(drop.item, update.quantity);

				//Remove the item from player's clients
				Helpers.Object_ItemDropUpdate(Players, update.itemID, (ushort)drop.quantity);
			}
		}

		/// <summary>
		/// Triggered when a player requests to drop an item
		/// </summary>
		public override void handlePlayerDrop(Player from, CS_PlayerDrop update)
		{	//Get the item into
			ItemInfo item = _server._assets.getItemByID(update.itemID);
			if (item == null)
			{
				Log.write(TLog.Warning, "Player requested to drop invalid item. {0}", from);
				return;
			}

			//Perform some sanity checks
			if (!item.droppable)
				return;
			if (!Helpers.isInRange(200,
				from._state.positionX, from._state.positionY,
				update.positionX, update.positionY))
				return;

			//Forward to our script
			if (!exists("Player.ItemDrop") || (bool)callsync("Player.ItemDrop", false, from, item, update.quantity))
			{	//Update his inventory
				if (from.inventoryModify(item, -update.quantity))
					//Create an item spawn
					itemSpawn(item, update.quantity, update.positionX, update.positionY);
			}
		}

		/// <summary>
		/// Handles a player's portal request
		/// </summary>
		public override void handlePlayerPortal(Player from, LioInfo.Portal portal)
		{	//Are we able to use this portal?
			if (!Logic_Assets.SkillCheck(from, portal.PortalData.SkillLogic))
				return;

			//Correct team?
			if (portal.PortalData.Frequency != -1 && portal.PortalData.Frequency != from._team._id)
				return;

			//Obtain the warp destination
			List<LioInfo.WarpField> warp = _server._assets.Lios.getWarpGroupByID(portal.PortalData.DestinationWarpGroup);
			if (warp == null)
			{	//Strange
				Log.write(TLog.Warning, "Failed portal {0}. Unconnected warpgroup #{1}", portal, portal.PortalData.DestinationWarpGroup);
				return;
			}

			//Forward to our script
			if (!exists("Player.Portal") || (bool)callsync("Player.Portal", false, from, portal))
			{	//Do some warpage
				Logic_Lio.Warp(Helpers.WarpMode.Normal, from, warp);
			}
		}

		/// <summary>
		/// Handles a player's produce request
		/// </summary>
		public override void handlePlayerProduce(Player from, ushort computerVehID, ushort produceItem)
		{	//Make sure the item index is sensible
			if (produceItem > 15)
			{
				Log.write(TLog.Warning, "Player {0} attempted to produce item > 15.", from);
				return;
			}
			
			//Get the associated vehicle
			Vehicle vehicle;

			if ((vehicle = _vehicles.getObjByID(computerVehID)) == null)
			{
				Log.write(TLog.Warning, "Player {0} attempted to produce using invalid vehicle.", from);
				return;
			}

			Computer computer = vehicle as Computer;
			if (computer == null)
			{
				Log.write(TLog.Warning, "Player {0} attempted to produce using non-computer vehicle.", from);
				return;
			}

			//It must be in range
			if (!Helpers.isInRange( 100,
									computer._state.positionX, computer._state.positionY,
									from._state.positionX, from._state.positionY))
				return;

			//Can't produce from dead or non-computer vehicles
			if (computer.IsDead || computer._type.Type != VehInfo.Types.Computer)
				return;
			
			//Vehicle looks fine, find the produce item involved
			VehInfo.Computer computerInfo = (VehInfo.Computer)computer._type;
			VehInfo.Computer.ComputerProduct product = computerInfo.Products[produceItem];
			
			//Quick check to make sure it isn't blank
			if (product.Title == "")
				return;

			//Forward to our script
			if (!exists("Player.Produce") || (bool)callsync("Player.Produce", false, from, computer, product))
			{	//Make a produce request
				produceRequest(from, computer, product);
			}
		}

		/// <summary>
		/// Handles a player's switch request
		/// </summary>
		public override void handlePlayerSwitch(Player from, bool bOpen, LioInfo.Switch swi)
		{	//Forward to our script
			if (!exists("Player.Switch") || (bool)callsync("Player.Switch", false, from, swi))
			{	//Make a switch request
				switchRequest(false, bOpen, from, swi);
			}
		}

		/// <summary>
		/// Handles a player's flag request
		/// </summary>
		public override void handlePlayerFlag(Player from, bool bPickup, bool bInPlace, LioInfo.Flag flag)
		{	//Forward to our script
			if (!exists("Player.FlagAction") || (bool)callsync("Player.FlagAction", false, from, bPickup, bInPlace, flag))
			{	//Make a switch request
				flagRequest(false, bPickup, bInPlace, from, flag);
			}
		}

		/// <summary>
		/// Handles the spawn of a player
		/// </summary>
		public override void handlePlayerSpawn(Player from, bool bDeath)
		{	//Forward to our script
			if (!exists("Player.Spawn") || (bool)callsync("Player.Spawn", false, from, bDeath))
			{	//Did he die?
				if (bDeath)
				{	//Trigger the appropriate event
					if (from._bEnemyDeath)
						Logic_Assets.RunEvent(from, _server._zoneConfig.EventInfo.killedByEnemy);
					else
						Logic_Assets.RunEvent(from, _server._zoneConfig.EventInfo.killedByTeam);

					//Reset his bounty
					from.Bounty = _server._zoneConfig.bounty.start;
				}
			}
		}

		/// <summary>
		/// Triggered when a player wants to spec or unspec
		/// </summary>
		public override void handlePlayerJoin(Player from, bool bSpec)
		{	//Let them!
			if (bSpec)
			{	//Forward to our script
				if (!exists("Player.LeaveGame") || (bool)callsync("Player.LeaveGame", false, from))
				{	//The player has effectively left the game
					from.spec(getTeamByName("spec"));
				}
			}
			else
			{	//Do we have a full arena?
				if (PlayerCount >= _server._zoneConfig.arena.playingMax)
				{	//Yep, tell him why he can't get in
					from.sendMessage(255, "Game is full.");
					return;
				}

				//Is he able to unspec?
				if (!Logic_Assets.SkillCheck(from, _server._zoneConfig.arena.exitSpectatorLogic))
				{
					from.sendMessage(-1, _server._zoneConfig.arena.exitSpectatorMessage);
					return;
				}

				//Forward to our script
				if (exists("Player.JoinGame") && !(bool)callsync("Player.JoinGame", false, from))
					return;

				//Pick a team
				Team pick = pickAppropriateTeam(from);

				if (pick != null)
					//Great, use it
					from.unspec(pick);
				else
					from.sendMessage(-1, "Unable to pick a team.");
			}
		}

		/// <summary>
		/// Triggered when a player wants to enter a vehicle
		/// </summary>
		public override void handlePlayerEnterVehicle(Player from, bool bEnter, ushort vehicleID)
		{	//Are we trying to leave our current vehicle?
			if (!bEnter)
			{	//Forward to our script
				if (!exists("Player.LeaveVehicle") || (bool)callsync("Player.LeaveVehicle", false, from, from._occupiedVehicle))
				{   //Let's leave it!
					from._occupiedVehicle.playerLeave(true);

					//Warp the player away from the vehicle to keep him from getting "stuck"
					Random exitRadius = new Random();
					from.warp(from._state.positionX + exitRadius.Next(-_server._zoneConfig.arena.vehicleExitWarpRadius, _server._zoneConfig.arena.vehicleExitWarpRadius),
					from._state.positionY + exitRadius.Next(-_server._zoneConfig.arena.vehicleExitWarpRadius, _server._zoneConfig.arena.vehicleExitWarpRadius));
				}

				return;
			}

			//Otherwise, do we have such a vehicle?
			Vehicle entry;

			if ((entry = _vehicles.getObjByID(vehicleID)) == null)
			{
				Log.write(TLog.Warning, "Player {0} attempted to enter invalid vehicle.", from);
				return;
			}

			//It must be in range
			if (!Helpers.isInRange(_server._zoneConfig.arena.vehicleGetInDistance,
									entry._state.positionX, entry._state.positionY,
									from._state.positionX, from._state.positionY))
				return;

			//Can't enter dead vehicles
			if (entry.IsDead)
				return;

			//Forward to our script
			if (!exists("Player.EnterVehicle") || (bool)callsync("Player.EnterVehicle", false, from, entry))
				//Attempt to enter the vehicle!
				from.enterVehicle(entry);
		}

		/// <summary>
		/// Triggered when a player notifies the server of an explosion
		/// </summary>
		public override void handlePlayerExplosion(Player from, CS_Explosion update)
		{	//Damage any computer vehicles (future, also bots) in the blast radius
			ItemInfo.Projectile usedWep = Helpers._server._assets.getItemByID(update.explosionID) as ItemInfo.Projectile;

			if (usedWep == null)
			{	//All things that explode should be projectiles. But just in case...
				Log.write(TLog.Warning, "Player fired unsupported weapon id {0}", update.explosionID);
				return;
			}

			//Forward to our script
			if (!exists("Player.Explosion") || (bool)callsync("Player.Explosion", false, from, usedWep))
			{	//Find the largest blast radius of damage types of this weapon
				int maxDamageRadius = Helpers.getMaxBlastRadius(usedWep);

				List<Vehicle> vechs = _vehicles.getObjsInRange(update.positionX, update.positionY, maxDamageRadius + 500);

				//Notify all vehicles in the vicinity
				foreach (Vehicle v in vechs)	
					if (!v.IsDead)
						v.applyExplosion(from, update.positionX, update.positionY, usedWep);
			}
		}

		/// <summary>
		/// Triggered when a player has sent an update packet
		/// </summary>
		public override void handlePlayerUpdate(Player from, CS_PlayerUpdate update)
		{	//Should we ignore this?
			if (update.bIgnored)
				return;

			//Is it firing an item?
			if (update.itemID != 0)
			{	//Let's inspect this action a little closer
				ItemInfo info = _server._assets.getItemByID(update.itemID);
				if (info == null)
				{
					Log.write(TLog.Warning, "Player {0} attempted to fire non-existent item.", from);
					return;
				}

				//Does he have it?
				Player.InventoryItem ii = from.getInventory(info);
				if (ii == null)
				{	//Is it a default item?
					if (!from.ActiveVehicle._type.InventoryItems.Any(item => item == update.itemID))
					{
						Log.write(TLog.Warning, "Player {0} attempted to fire unowned item.", from);
						return;
					}
				}

				//And does he have the appropriate skills?
				if (!Logic_Assets.SkillCheck(from, info.skillLogic))
				{
					Log.write(TLog.Warning, "Player {0} attempted unqualified use of item.", from);
					return;
				}

				//Check timings
				/*if (update.itemID != 0 && from._lastItemUse != 0 && from._lastItemUseID == update.itemID)
				{
					if (info.itemType == ItemInfo.ItemType.Projectile)
					{	//Is it nicely timed?
						ItemInfo.Projectile proj = (ItemInfo.Projectile)info;
						if (update.tickCount - from._lastItemUse < (proj.fireDelay - (proj.fireDelay / 2) + (proj.fireDelay / 4)) &&
							update.tickCount - from._lastItemUse < (proj.fireDelayOther - (proj.fireDelayOther / 2) + (proj.fireDelayOther / 4)))
						{
							update.itemID = 0;
							Log.write(TLog.Warning, "Player {0} had a suspicious reload timer.", from);
							triggerMessage(2, 1500, from._alias + " was kicked for knobbery.");
							from.disconnect();
							return;
						}
					}
					else if (info.itemType == ItemInfo.ItemType.MultiUse)
					{	//Is it nicely timed?
						ItemInfo.MultiUse multi = (ItemInfo.MultiUse)info;
						if (update.tickCount - from._lastItemUse < (multi.fireDelay - (multi.fireDelay / 2) + (multi.fireDelay / 4)) &&
							update.tickCount - from._lastItemUse < (multi.fireDelayOther - (multi.fireDelayOther / 2) + (multi.fireDelayOther / 4)))
						{	//Kick the fucker
							Log.write(TLog.Warning, "Player {0} had a suspicious reload timer.", from);
							triggerMessage(2, 1500, from._alias + " was kicked for knobbery.");
							from.disconnect();
							return;
						}
					}
				}*/

				from._lastItemUseID = update.itemID;
				if (update.itemID != 0)
					from._lastItemUse = update.tickCount;

				//We should be good. Check for ammo
				int ammoType;
				int ammoCount;

				if (info.getAmmoType(out ammoType, out ammoCount))
					if (ammoType != 0 && !from.inventoryModify(false, ammoType, -ammoCount))
						update.itemID = 0;
			}

			//Update the player's state
			from._state.energy = update.energy;

			from._state.velocityX = update.velocityX;
			from._state.velocityY = update.velocityY;
			from._state.velocityZ = update.velocityZ;

			from._state.positionX = update.positionX;
			from._state.positionY = update.positionY;
			from._state.positionZ = update.positionZ;
			
			from._state.yaw = update.yaw;
			from._state.direction = (Helpers.ObjectState.Direction)update.direction;
			from._state.unk1 = update.unk1;

			//If the player is inside a vehicle..
			if (from._occupiedVehicle != null)
			{
				//Update the vehicle state too..
				from._occupiedVehicle._state.health = update.health;

				from._occupiedVehicle._state.positionX = update.positionX;
				from._occupiedVehicle._state.positionY = update.positionY;
				from._occupiedVehicle._state.positionZ = update.positionZ;

				from._occupiedVehicle._state.velocityX = update.velocityX;
				from._occupiedVehicle._state.velocityY = update.velocityY;
				from._occupiedVehicle._state.velocityZ = update.velocityZ;

				from._occupiedVehicle._state.yaw = update.yaw;
				from._occupiedVehicle._state.direction = (Helpers.ObjectState.Direction)update.direction;
				from._occupiedVehicle._state.unk1 = update.unk1;
				from._occupiedVehicle._state.lastUpdate = from._state.lastUpdate = Environment.TickCount;

				//Update spatial data
				_vehicles.updateObjState(from._occupiedVehicle, from._occupiedVehicle._state);

				//Propagate the state
				from._occupiedVehicle.propagateState();
			}
			else
			{
				from._state.health = update.health;
				from._state.lastUpdate = Environment.TickCount;
			}

			//Send player coord updates to update spatial data
			_players.updateObjState(from, from._state);
			if (!from._bSpectator)
				_playersIngame.updateObjState(from, from._state);

			//If it's a spectator, we should not route
			if (from.IsSpectator)
				return;

			//Route it to all players!
			Helpers.Update_RoutePlayer(Players, from, update);
		}

		/// <summary>
		/// Triggered when a player has sent a death packet
		/// </summary>
		public override void handlePlayerDeath(Player from, CS_VehicleDeath update)
		{	//Store variables to pass to the event at the end
			Player killer = null;
			
			//Was it us that died?
			if (update.killedID != from._id)
			{	//Was it the vehicle we were in?
				if (update.killedID == from._occupiedVehicle._id)
				{	//Was it a player kill?
					if (update.type == Helpers.KillType.Player)
					{	//Sanity checks
						killer = _players.getObjByID((ushort)update.killerPlayerID);

						//Was it a player?
						if (update.killerPlayerID >= 5001 || killer == null)
						{
							Log.write(TLog.Warning, "Player {0} gave invalid player killer ID.", from);
							return;
						}
					}
					
					//Yes! Fall out of the vehicle
					from._occupiedVehicle.kill(killer);
					from._occupiedVehicle.playerLeave(true);
					return;
				}
				
				//We shouldn't be able to 'kill' anything else
				Log.write(TLog.Warning, "Player {0} died with invalid killedID #{1}", from._alias, update.killedID);
				return;
			}
			
			//Fall out of our vehicle and die!
			if (from._occupiedVehicle != null)
				from._occupiedVehicle.playerLeave(true);

			//Reset some life-specific stats
			from.Bounty = 0;

			//Mark him as dead!
			from._bEnemyDeath = true;
			from._deathTime = Environment.TickCount;

			//Was it a player kill?
			if (update.type == Helpers.KillType.Player)
			{	//Sanity checks
				killer = _players.getObjByID((ushort)update.killerPlayerID);

				//Was it a player?
				if (update.killerPlayerID < 5001)
				{
					if (killer == null)
					{
						Log.write(TLog.Warning, "Player {0} gave invalid killer ID.", from);
						return;
					}

					//Forward to our script
					if (!exists("Player.PlayerKill") || (bool)callsync("Player.PlayerKill", false, from, killer))
					{	//Handle any flags
						flagHandleDeath(from, killer);

						//Don't reward for teamkills
						if (from._team == killer._team)
							Logic_Assets.RunEvent(from, _server._zoneConfig.EventInfo.killedTeam);
						else
						{
							Logic_Assets.RunEvent(from, _server._zoneConfig.EventInfo.killedEnemy);
							Logic_Rewards.calculatePlayerKillRewards(from, killer, update);
						}

						killer.Kills++;
						from.Deaths++;
					}

					return;
				}
			}

			//Reset any flags held
			flagResetPlayer(from);

			//Was it a bot kill?
			if (update.type == Helpers.KillType.Player && update.killerPlayerID >= 5001)
			{	//Attempt to find the associated bot
				Bots.Bot bot = _vehicles.getObjByID((ushort)update.killerPlayerID) as Bots.Bot;
				
				//Note: bot can be null, for when a player is killed by the bot's projectiles after the bot is dead

				//Forward to our script
				if (!exists("Player.BotKill") || (bool)callsync("Player.BotKill", false, from, bot))
				{	//Update stats
					from.Deaths++;

					//Yes. Spoof it
					update.type = Helpers.KillType.Computer;
					Helpers.Player_RouteKill(Players, update, from, 0, 0, 0, 0);
				}
			}
			else
			{	//If he was killed by a computer vehicle..
				if (update.type == Helpers.KillType.Computer)
				{	//Get the related vehicle
					Computer cvehicle = _vehicles.getObjByID((ushort)update.killerPlayerID) as Computer;
					if (cvehicle == null)
					{
						Log.write(TLog.Warning, "Player {0} was killed by unidentifiable computer vehicle.", from);
						return;
					}

					//Forward to our script
					if (!exists("Player.ComputerKill") || (bool)callsync("Player.ComputerKill", false, from, cvehicle))
					{	//Update stats
						from.Deaths++;

						//Route
						Logic_Rewards.calculateTurretKillRewards(from, cvehicle, update);
						Helpers.Player_RouteKill(Players, update, from, 0, 0, 0, 0);
					}
				}
				else
				{	//He was killed by another phenomenon, simply
					//route the kill packet to all players.
					Helpers.Player_RouteKill(Players, update, from, 0, 0, 0, 0);
				}
			}

			//Prompt the player death event
			if (!exists("Player.Death") || (bool)callsync("Player.Death", false, from, killer, update.type))
			{	
			}
		}

		/// <summary>
		/// Triggered when a player attempts to use the store
		/// </summary>
		public override void handlePlayerShop(Player from, ItemInfo item, int quantity)
		{	//What is this nonsense?
			if (quantity == 0)
				return;

			//Do we have the skills required?
			if (!Logic_Assets.SkillCheck(from, item.skillLogic))
				return;

			//Get the player's related inventory item
			Player.InventoryItem ii = from.getInventory(item);

			//Are we buying or selling?
			if (quantity > 0)
			{	//Buying. Are we able to?
				if (item.buyPrice == 0)
					return;

				//Check limits
				if (item.maxAllowed != 0)
				{
					int constraint = Math.Abs(item.maxAllowed) - ((ii == null) ? (ushort)0 : ii.quantity);
					if (quantity > constraint)
						return;
				}

				//Good to go, calculate the price
				int price = item.buyPrice * quantity;

				//Do we have enough?
				if (price > from.Cash)
					return;

				//Forward to our script
				if (!exists("Shop.Buy") || (bool)callsync("Shop.Buy", false, from, item, quantity))
				{	//Perform the transaction!
					from.Cash -= price;
					from.inventoryModify(item, quantity);
				}
			}
			else
			{	//Sellable?
				if (item.sellPrice == -1)
					return;
				else if (ii == null)
					return;

				//Do we have enough items?
				if (quantity > ii.quantity)
					return;

				//Calculate the price
				int price = item.sellPrice * quantity;

				//Forward to our script
				if (!exists("Shop.Sell") || (bool)callsync("Shop.Sell", false, from, item, -quantity))
				{	//Perform the transaction!
					from.Cash -= price;
					from.inventoryModify(item, quantity);
				}
			}
		}

		/// <summary>
		/// Triggered when a player attempts to use the skill shop
		/// </summary>
		public override void handlePlayerShopSkill(Player from, SkillInfo skill)
		{   //Do we have the skills required for this?
            if (!Logic_Assets.SkillCheck(from, skill.Logic))
                return;

			//Perform the skill modify!
			from.skillModify(skill, 1);
		}

		/// <summary>
		/// Triggered when a player attempts to use a warp item
		/// </summary>
		public override void handlePlayerWarp(Player player, ItemInfo.WarpItem item, ushort targetPlayerID, short posX, short posY)
		{	//What sort of warp item are we dealing with?
			switch (item.warpMode)
			{
				case ItemInfo.WarpItem.WarpMode.Lio:
					//Are we warpable?
					if (!player.ActiveVehicle._type.IsWarpable)
						return;

					//Forward to our script
					if (exists("Player.WarpItem") && !(bool)callsync("Player.WarpItem", false, player, item, targetPlayerID, posX, posY))
						return;

					//A simple lio warp. Get the associated warpgroup
					List<LioInfo.WarpField> warps = _server._assets.Lios.getWarpGroupByID(item.warpGroup);

					if (warps == null)
					{
						Log.write(TLog.Error, "Warp group '{0}' doesn't exist.", item.warpGroup);
						break;
					}

					//Warp the player
					Logic_Lio.Warp(Helpers.WarpMode.Normal, player, warps);
					break;

				case ItemInfo.WarpItem.WarpMode.WarpTeam:
					{	//Are we warpable?
						if (!player.ActiveVehicle._type.IsWarpable)
							return;

						//Find the player in question
						Player target = _playersIngame.getObjByID(targetPlayerID);
						if (target == null)
							return;

						//Can't warp to dead people
						if (target.IsDead)
						{
							player.sendMessage(0xFF, "The player you are trying to warp to is dead.");
							return;
						}

						//Is he on the correct team?
						if (target._team != player._team)
							return;

						//Forward to our script
						if (exists("Player.WarpItem") && !(bool)callsync("Player.WarpItem", false, player, item, targetPlayerID, posX, posY))
							return;

						if (item.areaEffectRadius > 0)
						{
							foreach (Player p in getPlayersInRange(posX, posY, item.areaEffectRadius))
								p.warp(Helpers.WarpMode.Normal, target._state, (short)item.accuracyRadius, -1, 0);
						}
						else
							player.warp(Helpers.WarpMode.Normal, target._state, (short)item.accuracyRadius, -1, 0);
					}
					break;

				case ItemInfo.WarpItem.WarpMode.WarpAnyone:
					{	//Are we warpable?
						if (!player.ActiveVehicle._type.IsWarpable)
							return;

						//Find the player in question						
						Player target = _playersIngame.getObjByID(targetPlayerID);
						if (target == null)
							return;

						//Can't warp to dead people
						if (target.IsDead)
							return;

						//Forward to our script
						if (exists("Player.WarpItem") && !(bool)callsync("Player.WarpItem", false, player, item, targetPlayerID, posX, posY))
							return;

						if (item.areaEffectRadius > 0)
						{
							foreach (Player p in getPlayersInRange(posX, posY, item.areaEffectRadius))
								p.warp(Helpers.WarpMode.Normal, target._state, (short)item.accuracyRadius, -1, 0);
						}
						else
							player.warp(Helpers.WarpMode.Normal, target._state, (short)item.accuracyRadius, -1, 0);
					}
					break;

				case ItemInfo.WarpItem.WarpMode.SummonTeam:
					{	//Find the player in question
						Player target = _playersIngame.getObjByID(targetPlayerID);
						if (target == null)
							return;

						//Is he on the correct team?
						if (target._team != player._team)
							return;

						//Is he warpable?
						if (!target.IsDead && !target.ActiveVehicle._type.IsWarpable)
							return;

						//Forward to our script
						if (exists("Player.WarpItem") && !(bool)callsync("Player.WarpItem", false, player, item, targetPlayerID, posX, posY))
							return;

						if (item.areaEffectRadius > 0)
						{
							foreach (Player p in
								getPlayersInRange(target._state.positionX, target._state.positionY, item.areaEffectRadius))
							{
								p.warp(Helpers.WarpMode.Normal, player._state, (short)item.accuracyRadius, -1, 0);
							}
						}
						else
							target.warp(Helpers.WarpMode.Normal, player._state, (short)item.accuracyRadius, -1, 0);
					}
					break;

				case ItemInfo.WarpItem.WarpMode.SummonAnyone:
					{	//Find the player in question
						Player target = _playersIngame.getObjByID(targetPlayerID);
						if (target == null)
							return;

						//Is he warpable?
						if (!target.IsDead && !target.ActiveVehicle._type.IsWarpable)
							return;

						//Forward to our script
						if (exists("Player.WarpItem") && !(bool)callsync("Player.WarpItem", false, player, item, targetPlayerID, posX, posY))
							return;

						if (item.areaEffectRadius > 0)
						{
							foreach (Player p in
								getPlayersInRange(target._state.positionX, target._state.positionY, item.areaEffectRadius))
							{
								p.warp(Helpers.WarpMode.Normal, player._state, (short)item.accuracyRadius, -1, 0);
							}
						}
						else
							target.warp(Helpers.WarpMode.Normal, player._state, (short)item.accuracyRadius, -1, 0);
					}
					break;
			}

			//Indicate that it was successful
			SC_ItemReload rld = new SC_ItemReload();
			rld.itemID = (short)item.id;

			player._client.sendReliable(rld);
		}

		/// <summary>
		/// Triggered when a player attempts to use a vehicle creator
		/// </summary>
		public override void handlePlayerMakeVehicle(Player player, ItemInfo.VehicleMaker item, short posX, short posY)
		{	//What does he expect us to make?
			VehInfo vehinfo = _server._assets.getVehicleByID(item.vehicleID);
			if (vehinfo == null)
			{
				Log.write(TLog.Warning, "VehicleMaker Item {0} corresponds to invalid vehicle.", item);
				return;
			}

			//Expensive stuff, vehicle creation
			int ammoID;
			int ammoCount;

			if (player.Cash < item.cashCost)
				return;

			if (item.getAmmoType(out ammoID, out ammoCount))
				if (ammoID != 0 && !player.inventoryModify(ammoID, -ammoCount))
					return;

			player.Cash -= item.cashCost;

			//Forward to our script
			if (!exists("Player.MakeVehicle") || (bool)callsync("Player.MakeVehicle", false, player, item, posX, posY))
			{	//Attempt to create it 
				Vehicle vehicle = newVehicle(vehinfo, player._team, player, player._state);

				//Indicate that it was successful
				SC_ItemReload rld = new SC_ItemReload();
				rld.itemID = (short)item.id;

				player._client.sendReliable(rld);
			}
		}

		/// <summary>
		/// Triggered when a player's item expires
		/// </summary>
		public override void handlePlayerItemExpire(Player player, ushort itemTypeID)
		{	//What sort of item is this?
			ItemInfo itminfo = _server._assets.getItemByID(itemTypeID);
			if (itminfo == null)
			{
				Log.write(TLog.Warning, "Player attempted to expire an invalid item type.");
				return;
			}
		
			//Can this item expire?
			if (itminfo.expireTimer == 0)
			{	//No!
				Log.write(TLog.Warning, "Player attempted to expire an item which can't be expired: {0}", itminfo.name);
				return;
			}

			//Remove all items of this type
			player.removeAllItemFromInventory(itemTypeID);
		}

		/// <summary>
		/// Triggered when a player attempts to use an item creator
		/// </summary>
        public override void handlePlayerMakeItem(Player player, ItemInfo.ItemMaker item, short posX, short posY)
		{   //What does he expect us to make?
		    ItemInfo itminfo = _server._assets.getItemByID(item.itemMakerItemID);
		    if (itminfo == null)
		    {
		        Log.write(TLog.Warning, "ItemMaker Item {0} corresponds to invalid item.", item);
		        return;
		    }

		    //Expensive stuff, item creation
		    int ammoID;
		    int ammoCount;

		    if (player.Cash < item.cashCost)
		        return;

		    if (item.getAmmoType(out ammoID, out ammoCount))
		        if (ammoID != 0 && !player.inventoryModify(ammoID, -ammoCount))
		            return;

		    player.Cash -= item.cashCost;

		    //Forward to our script
		    if (!exists("Player.MakeItem") || (bool) callsync("Player.MakeItem", false, player, item, posX, posY))
		    {   //Do we create it in the inventory or arena?
				if (item.itemMakerQuantity > 0)
					itemSpawn(itminfo, (ushort)item.itemMakerQuantity, posX, posY);
				else
					player.inventoryModify(itminfo, Math.Abs(item.itemMakerQuantity));

		        //Indicate that it was successful
		        SC_ItemReload rld = new SC_ItemReload();
		        rld.itemID = (short) item.id;

		        player._client.sendReliable(rld);

                player.syncState();
		    }
		}

        /// <summary>
        /// Triggered when a player sends a chat command
        /// </summary>
        public override void handlePlayerChatCommand(Player player, Player recipient, string command, string payload)
        {
            if (!exists("Player.ChatCommand") || (bool)callsync("Player.ChatCommand", false, player, recipient, command, payload)) 
            { 
            }
        }

		/// <summary>
		/// Triggered when a player attempts to repair/heal
		/// </summary>
        public override void handlePlayerRepair(Player player, ItemInfo.RepairItem item, UInt16 targetVehicle, short posX, short posY)
        {	//Does the player have appropriate ammo?
			if (item.useAmmoID != 0 && !player.inventoryModify(false, item.useAmmoID, -item.ammoUsedPerShot))
				return;

            // Forward it to our script
            if(!exists("Player.Repair") || (bool)callsync("Player.Repair", false, player, item, posX, posY))
			{	//What type of repair is it?
				switch (item.repairType)
				{
					//Health and energy repair
					case 0:
					case 2:
						{	//Is it an area or individual repair?
							if (item.repairDistance > 0)
							{	//Individual! Do we have a valid target?
								Player target = _playersIngame.getObjByID(targetVehicle);
								if (target == null)
								{
									Log.write(TLog.Warning, "Player {0} attempted to use a {1} to heal a non-existent player.", player._alias, item.name);
									return;
								}

								//Is he on the correct team?
								if (target._team != player._team)
									return;

								//Is he in range?
								if (!Helpers.isInRange(item.repairDistance, target._state, player._state))
									return;

								//Repair!
								target.heal(item, player);
							}
							else if (item.repairDistance < 0)
							{	//An area heal! Get all players within this area..
								List<Player> players = getPlayersInRange(player._state.positionX, player._state.positionY, -item.repairDistance);

								//Check each player
								foreach (Player p in players)
								{	//Is he on the correct team?
									if (p._team != player._team)
										continue;

									//Can we self heal?
									if (p == player && !item.repairSelf)
										continue;

									//Heal!
									p.heal(item, player);
								}
							}
							else
							{	//A self heal! Sure you can!
								player.heal(item, player);
							}
						}
						break;

					//Vehicle repair
					case 1:
						{	//Is it an area or individual repair?
							if (item.repairDistance > 0)
							{	//Individual! Do we have a valid target?
								Vehicle target = _vehicles.getObjByID(targetVehicle);
								if (target == null)
								{
									Log.write(TLog.Warning, "Player {0} attempted to use a {1} to repair a non-existent vehicle.", player._alias, item.name);
									return;
								}

								//Is it in range?
								if (!Helpers.isInRange(item.repairDistance, target._state, player._state))
									return;

								//Is it occupied?
								if (target._inhabitant != null)
									target._inhabitant.heal(item, player);
								else
								{	//Apply the healing effect
									target._state.health = (short)Math.Min(target._type.Hitpoints, target._state.health + item.repairAmount);

									//TODO: A bit hackish, should probably standardize this or improve computer updates
									if (target is Computer)
										(target as Computer)._sendUpdate = true;
								}
							}
							else if (item.repairDistance < 0)
							{	//An area heal! Get all vehicles within this area..
								List<Vehicle> players = _vehicles.getObjsInRange(player._state.positionX, player._state.positionY, -item.repairDistance);

								//Check each vehicle
								foreach (Vehicle v in players)
								{	//Is it on the correct team?
									if (v._team != player._team)
										continue;

									//Can we self heal?
									if (v._inhabitant == player && !item.repairSelf)
										continue;

									//Repair!
									if (v._inhabitant != null)
										v._inhabitant.heal(item, player);
									else
									{	//Apply the healing effect
										v._state.health = (short)Math.Min(v._type.Hitpoints, v._state.health + item.repairAmount);

										//TODO: A bit hackish, should probably standardize this or improve computer updates
										if (v is Computer)
											(v as Computer)._sendUpdate = true;
									}
								}
							}
							else
							{	//A self heal! Sure you can!
								player.heal(item, player);
							}
						}
						break;
				}
				
				//Indicate that it was successful
				SC_ItemReload rld = new SC_ItemReload();
				rld.itemID = (short)item.id;

				player._client.sendReliable(rld);
				
				//Send an item used notification to players
				Helpers.Player_RouteItemUsed(false, Players, player, targetVehicle, (Int16)item.id, posX, posY, 0);  
            }
        }
		
		/// <summary>
		/// Triggered when a player attempts to spectate another player
		/// </summary>
		public override void handlePlayerSpectate(Player player, ushort targetPlayerID)
		{	//Make sure he's in spec himself
			if (!player.IsSpectator)
				return;
			
			//Find the player in question						
			Player target = _playersIngame.getObjByID(targetPlayerID);
			if (target == null)
				return;

			//Can't spectate other spectators
			if (target.IsSpectator)
				return;

			//TODO: Check spectator permission

			//Tell him yes!
			player.spectate(target);
		}

		/// <summary>
		/// Triggered when a vehicle is created
		/// </summary>
		/// <remarks>Doesn't catch spectator or dependent vehicle creation</remarks>
		public override void handleVehicleCreation(Vehicle created, Team team, Player creator)
		{	//Forward it to our script
			if (!exists("Vehicle.Creation") || (bool)callsync("Vehicle.Creation", false, created, team, creator))
			{	
			}
		}

		/// <summary>
		/// Triggered when a vehicle dies
		/// </summary>
		public override void handleVehicleDeath(Vehicle dead, Player killer, Player occupier)
		{	//Forward it to our script
			if (!exists("Vehicle.Death") || (bool)callsync("Vehicle.Death", false, dead, killer))
			{	//Route the death to the arena
				Helpers.Vehicle_RouteDeath(Players, killer, dead, occupier);
			}
		}

		/// <summary>
		/// Triggered when a bot is killed
		/// </summary>
		public override void handleBotDeath(Bot dead, Player killer)
		{	//Forward it to our script
			if (!exists("Bot.Death") || (bool)callsync("Bot.Death", false, dead, killer))
			{	//Route the death to the arena
				Helpers.Vehicle_RouteDeath(Players, killer, dead, null);
			}
		}
		#endregion
		#endregion
	}
}
