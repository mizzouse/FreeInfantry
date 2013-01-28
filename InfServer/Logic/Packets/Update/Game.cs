﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Protocol;
using InfServer.Game;

using Assets;

namespace InfServer.Logic
{	// Logic_Game Class
	/// Handles all updates related to the game state
	///////////////////////////////////////////////////////
	class Logic_Game
	{	/// <summary>
		/// Handles an item pickup request from a client
		/// </summary>
		static public void Handle_CS_PlayerPickup(CS_PlayerPickup pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to pickup items from spec.", player);
				return;
			}

			if (player.IsDead)
			{
				Log.write(TLog.Warning, "Player {0} attempted to pickup items while dead.", player);
				return;
			}

			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerPickup(player, pkt);
				}
			);
		}

		/// <summary>
		/// Handles an item drop request from a client
		/// </summary>
		static public void Handle_CS_PlayerDrop(CS_PlayerDrop pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to drop items from spec.", player);
				return;
			}

			if (player.IsDead)
			{
				Log.write(TLog.Warning, "Player {0} attempted to drop items while dead.", player);
				return;
			}

			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerDrop(player, pkt);
				}
			);
		}

		/// <summary>
		/// Handles an portal warp request from a client
		/// </summary>
		static public void Handle_CS_PlayerPortal(CS_PlayerPortal pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to use a portal from spec.", player);
				return;
			}

			if (player.IsDead)
			{
				Log.write(TLog.Warning, "Player {0} attempted to use a portal while dead.", player);
				return;
			}

			//Find the associated portal
			LioInfo.Portal portal = player._server._assets.getLioByID(pkt.portalID) as LioInfo.Portal;

			//If it doesn't exist, nevermind
			if (portal == null)
				Log.write(TLog.Warning, "Player {0} attempted to use portal which doesn't exist.", player);
			else
			{
				player._arena.handleEvent(delegate(Arena arena)
					{
						player._arena.handlePlayerPortal(player, portal);
					}
				);
			}
		}

		/// <summary>
		/// Handles a produce request from a client
		/// </summary>
		static public void Handle_CS_PlayerProduce(CS_PlayerProduce pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to produce an item from spec.", player);
				return;
			}

			if (player.IsDead)
			{
				Log.write(TLog.Warning, "Player {0} attempted to produce an item while dead.", player);
				return;
			}

			//Pass the request on to the server
			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerProduce(player, pkt.computerID, pkt.produceItem);
				}
			);
		}

		/// <summary>
		/// Handles a switch request from a client
		/// </summary>
		static public void Handle_CS_PlayerSwitch(CS_PlayerSwitch pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to activate a switch from spec.", player);
				return;
			}

			if (player.IsDead)
			{
				Log.write(TLog.Warning, "Player {0} attempted to activate a switch while dead.", player);
				return;
			}

			//Find the associated portal
			LioInfo.Switch sw = player._server._assets.getLioByID(pkt.switchID) as LioInfo.Switch;

			//If it doesn't exist, nevermind
			if (sw == null)
				Log.write(TLog.Warning, "Player {0} attempted to use switch which doesn't exist.", player);
			else
			{
				player._arena.handleEvent(delegate(Arena arena)
					{
						player._arena.handlePlayerSwitch(player, pkt.bOpen, sw);
					}
				);
			}
		}

		/// <summary>
		/// Handles a flag request from a client
		/// </summary>
		static public void Handle_CS_PlayerFlag(CS_PlayerFlag pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to activate a flag from spec.", player);
				return;
			}

			//Find the associated flag
			LioInfo.Flag flag = player._server._assets.getLioByID(pkt.flagID) as LioInfo.Flag;

			//If it doesn't exist, nevermind
			if (flag == null)
				Log.write(TLog.Warning, "Player {0} attempted to use flag which doesn't exist.", player);
			else
			{
				player._arena.handleEvent(delegate(Arena arena)
					{
						player._arena.handlePlayerFlag(player, pkt.bPickup, pkt.bSuccess, flag);
					}
				);
			}
		}

		/// <summary>
		/// Handles a join request from a client
		/// </summary>
		static public void Handle_CS_PlayerJoin(CS_PlayerJoin pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			//Don't allow specs/vehicle entry while dead
			if (player.IsDead)
				return;

			//Do we wish to return to our base player vehicle?
			player._arena.handleEvent(delegate(Arena arena)
				{
					if (pkt.vehicleID == player._baseVehicle._id)
					{	//Yes, are we currently in a specator vehicle?
						if (player._occupiedVehicle == null)
						{
							Log.write(TLog.Error, "Player {0} attempting to return to baseVehicle from no occupied vehicle.", player);
							return;
						}

						if (player._occupiedVehicle._type.Type == Assets.VehInfo.Types.Spectator)
							//Treat it as an unspec request
							player._arena.handlePlayerJoin(player, false);
						else
							//It's a request to leave the current vehicle
							player._arena.handlePlayerEnterVehicle(player, false, (ushort)pkt.vehicleID);
					}
					//Do we wish to return to spec?
					else if (pkt.vehicleID == -1)
					{	//Check energy
						if (player._state.energy >= (player._server._zoneConfig.arena.spectatorEnergyAmount / 1000))
							player._arena.handlePlayerJoin(player, true);
					}
					else
						//An attempt to enter a vehicle?
						player._arena.handlePlayerEnterVehicle(player, true, (ushort)pkt.vehicleID);
				}
			);
		}
		
		/// <summary>
		/// Handles an explosion notifier from a client
		/// </summary>
		static public void Handle_CS_Explosion(CS_Explosion pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to trigger an explosion from spec.", player);
				return;
			}

			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerExplosion(player, pkt);
				}
			);
		}

		/// <summary>
		/// Handles a shop transaction request from the client
		/// </summary>
		static public void Handle_CS_Shop(CS_Shop pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			//Resolve the item in question
			ItemInfo item = player._server._assets.getItemByID(pkt.itemID);
			if (item == null)
			{
				Log.write(TLog.Warning, "Player {0} attempted to buy non-existent item from store.", player);
				return;
			}

			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerShop(player, item, pkt.quantity);
				}
			);

			//We will always finish the transaction
			Helpers.Player_ShopFinish(player);
		}

		/// <summary>
		/// Handles a skill shop transaction request from the client
		/// </summary>
		static public void Handle_CS_ShopSkill(CS_ShopSkill pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			//Resolve the skill in question
			SkillInfo skill = player._server._assets.getSkillByID(pkt.skillID);
			if (skill == null)
			{
				Log.write(TLog.Warning, "Player {0} attempted to buy non-existent skill from store.", player);
				return;
			}

            //Is he able to pick these classes?
            /*
            if (!player.IsSpectator && !Logic_Assets.SkillCheckTester(player, skill.SkillId, player._server._zoneConfig.arena.exitSpectatorLogic))
            {
                player.sendMessage(-1, "That is not an eligible class to play.");
                return;
            }
            */
			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerShopSkill(player, skill);
				}
			);

			//We will always finish the transaction
			Helpers.Player_ShopFinish(player);
		}

		/// <summary>
		/// Handles a skill shop transaction request from the client
		/// </summary>
		static public void Handle_CS_PlayerUseItem(CS_PlayerUseItem pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to use a warp item from spec.", player);
				return;

			}

			//Check the player isn't lying about his coordinates
			if (!Helpers.isInRange(200, player._state.positionX, player._state.positionY, pkt.posX, pkt.posY))
			{
				Log.write(TLog.Warning, "Player {0} coordinate mismatch on use item.", player);
				return;
			}

			//Resolve the item in question
			ItemInfo info = player._server._assets.getItemByID(pkt.itemID);
			if (info == null)
			{
				Log.write(TLog.Warning, "Player {0} attempted to use non-existent item.", player);
				return;
			}

			//Are we able to use it?
			if (!Logic_Assets.SkillCheck(player, info.skillLogic))
				return;

			//The action taken depends on the type of item
			player._arena.handleEvent(delegate(Arena arena)
				{
					switch (info.itemType)
					{
						case ItemInfo.ItemType.Warp:
							player._arena.handlePlayerWarp(player, info as ItemInfo.WarpItem, (ushort)pkt.targetVehicle, pkt.posX, pkt.posY);
							break;

						case ItemInfo.ItemType.VehicleMaker:
							player._arena.handlePlayerMakeVehicle(player, info as ItemInfo.VehicleMaker, pkt.posX, pkt.posY);
							break;

						case ItemInfo.ItemType.ItemMaker:
							player._arena.handlePlayerMakeItem(player, info as ItemInfo.ItemMaker, pkt.posX, pkt.posY);
							break;

						case ItemInfo.ItemType.Repair:
							player._arena.handlePlayerRepair(player, info as ItemInfo.RepairItem, (UInt16)pkt.targetVehicle, pkt.posX, pkt.posY);
							break;
					}
				}
			);


		}

        /// <summary>
        /// Handles an allow/disallow spectators request for a client
        /// </summary>
        static public void Handle_CS_AllowSpectator(CS_AllowSpectator pkt, Player player)
        {   //Allow the player's arena to handle it
            if (player._arena == null)
            {
                Log.write(TLog.Error, "Player {0} sent allow spectator request packet with no arena.", player);
                return;
            }

            //Allow/disallow spectators
            player._bAllowSpectator = pkt.allow;

            //Remove all his current spectators
            if (!player._bAllowSpectator)
            {
                foreach (Player p in player._spectators.ToList())
                {
                    p.sendMessage(-1, player._alias + " has disabled spectators");
                    p._spectating = null;
                    Helpers.Player_SpectatePlayer(p, player, true); //true = stop spectating
                }
                player._spectators.Clear();
            }
        }

		/// <summary>
		/// Handles an spectator request for a client
		/// </summary>
		static public void Handle_CS_RequestSpectator(CS_RequestSpectator pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent spectator request packet with no arena.", player);
				return;
			}

			if (!player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to spectate a player while not in spec.", player);
				return;
			}

			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerSpectate(player, pkt.playerID);
				}
			);
		}

		/// <summary>
		/// Handles an item expire notification
		/// </summary>
		static public void Handle_CS_ItemExpired(CS_ItemExpired pkt, Player player)
		{	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent item expired packet with no arena.", player);
				return;
			}

			player._arena.handleEvent(delegate(Arena arena)
				{
					player._arena.handlePlayerItemExpire(player, pkt.itemID);
				}
			);
		}

        /// <summary>
        /// Handles a vehicle pickup request
        /// </summary>
        static public void Handle_CS_VehiclePickup(CS_VehiclePickup pkt, Player player)
        {	//Allow the player's arena to handle it
			if (player._arena == null)
			{
				Log.write(TLog.Error, "Player {0} sent update packet with no arena.", player);
				return;
			}

			if (player.IsSpectator)
			{
				Log.write(TLog.Warning, "Player {0} attempted to pickup vehicle items from spec.", player);
				return;
			}

			if (player.IsDead)
			{
				Log.write(TLog.Warning, "Player {0} attempted to pickup vehicle items while dead.", player);
				return;
			}
            
            player._arena.handleEvent(delegate(Arena arena)
                {
                    player._arena.handleVehiclePickup(player, pkt);
                }
            );
        }

		/// <summary>
		/// Registers all handlers
		/// </summary>
		[Logic.RegistryFunc]
		static public void Register()
		{
			CS_PlayerPickup.Handlers += Handle_CS_PlayerPickup;
			CS_PlayerDrop.Handlers += Handle_CS_PlayerDrop;
			CS_Explosion.Handlers += Handle_CS_Explosion;
			CS_PlayerJoin.Handlers += Handle_CS_PlayerJoin;
			CS_PlayerPortal.Handlers += Handle_CS_PlayerPortal;
			CS_PlayerProduce.Handlers += Handle_CS_PlayerProduce;
			CS_PlayerSwitch.Handlers += Handle_CS_PlayerSwitch;
			CS_PlayerFlag.Handlers += Handle_CS_PlayerFlag;
			CS_Shop.Handlers += Handle_CS_Shop;
			CS_ShopSkill.Handlers += Handle_CS_ShopSkill;
			CS_PlayerUseItem.Handlers += Handle_CS_PlayerUseItem;
            CS_AllowSpectator.Handlers += Handle_CS_AllowSpectator;
			CS_RequestSpectator.Handlers += Handle_CS_RequestSpectator;
			CS_ItemExpired.Handlers += Handle_CS_ItemExpired;
            CS_VehiclePickup.Handlers += Handle_CS_VehiclePickup;
		}
	}
}
