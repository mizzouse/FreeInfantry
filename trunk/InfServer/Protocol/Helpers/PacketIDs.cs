﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Network;
using InfServer.Game;

using Assets;

namespace InfServer.Protocol
{	/// <summary>
	/// Provides a series of functions for easily serialization of packets
	/// </summary>
	public partial class Helpers
	{	// Member Classes
		//////////////////////////////////////////////////
		/// <summary>
		/// Houses the packet id enums
		/// </summary>
		public class PacketIDs
		{
			/// <summary>
			/// Contains C2S packet IDs
			/// </summary>
			public enum C2S
			{
				Login = 0x01,
				SetBanner = 0x02,
                //Unknown Packet #3 = 0x03, deals with a player leaving the zone but not switching arenas Note: doesnt have any data why?
                //SendBanner = 0x04,
				Shop = 0x05,
				Explosion = 0x06,
				ShopSkill = 0x07,
				PlayerSwitch = 0x08,
				PlayerFlag = 0x09,
				PlayerJoin = 0x0A,
				StartUpdate = 0x0B,
				Ready = 0x0C,
				PlayerProduce = 0x0D,
				PlayerPickup = 0x0E,
				PlayerUseItem = 0x0F,
                VehiclePickup = 0x10,
				ChartRequest = 0x11,
                //Unknown packet #18 = 0x12, has to do with joining a zone.. contains the playerID and 3 other 00 bytes
                SendBannerTo = 0x13,
                //SendBanner = 0x14,
				Frames = 0x15,
                PlayerDrop = 0x17,
                Chat = 0x18,
                //Unknown packet #25 = 0x19, has to do with being in the arena/zone - Note: this is a semi big packet
                BallPickup = 0x1A,
                BallDrop = 0x1B,
				PlayerUpdate = 0x1C,
				RequestUpdate = 0x1E,
				Environment = 0x1F,
				ItemExpired = 0x20,
				PlayerDeath = 0x21,
				PlayerPortal = 0x22,
				FileSend = 0x23,
                //SendBanner = 0x24,
				Security = 0x25,
                //SendBanner = 0x26,
                GoalScored = 0x27,
                //SendBanner = 0x28,
                //SendBanner = 0x29,
                AllowSpectator = 0x2A,
				RequestSpectator = 0x2B,
                //SendBanner = 0x2C,
                //SendBanner = 0x2D,
                //SendBanner = 0x2E,
                //SendBanner = 0x2F,
                //VehiclePickup = 0x30,
                //VehiclePickup = 0x31,
                //VehiclePickup = 0x32,
                //VehiclePickup = 0x33,
                //VehiclePickup = 0x34,
                //VehiclePickup = 0x35,
                SecurityResponse = 36,
			}

			/// <summary>
			/// Contains S2C packet IDs
			/// </summary>
			public enum S2C
			{
				Login = 0x01,
				PlayerEnter = 0x02,
				PlayerLeave = 0x03,
				EnterArena = 0x04,
				ArenaMessage = 0x05,
				SendBanner = 0x06,
				PatchInfo = 0x07,
				ArenaList = 0x08,
				BallState = 0x09,
				BindVehicle = 0x0B,
				ChangeTeam = 0x0C,
				PlayerState = 0x0D,
				VehicleDeath = 0x0F,
				Items = 0x10,
				ItemDrop = 0x11,
				Inventory = 0x12,
				Skills = 0x13,
				PlayerWarpEx = 0x14,
				ShopFinished = 0x15,
				GameReset = 0x16,
				RegQuery = 0x17,
				ItemUsed = 0x18,
				LIOUpdates = 0x19,
				Flags = 0x1A,
				PlayerUpdate = 0x1B,
				SecurityCheck = 0x1C,
				SetInGame = 0x1D,
				SetCrowns = 0x1E,
				TestFileExist = 0x1F,
				AssetUpdateInfo = 0x20,
				MultiItem = 0x21,
				Environment = 0x22,
				Chat = 0x23,
				VehicleDestroy = 0x24,
				Vehicles = 0x25,
				AssetUpdate = 0x26,
				AssetInfo = 0x27,
				PlayerTeamUpdate = 0x28,
				Projectile = 0x29,
				VehicleState = 0x2A,
				ShowGIF = 0x2B,
				PlayerWarp = 0x2C,
				ShowScoreChart = 0x2D,
				SetBounty = 0x2E,
				ConfirmFileSend = 0x2F,
				DisplayChart = 0x30,
				BannerTransfer = 0x31, /*This is NOT banner transfer, This is a packet to check if player is in the zone or not*/
				BannerInfo = 0x32,
				ItemReload = 0x33,
				PlayerSpectate = 0x35,
				FileSend = 0x36,
				ZoneList = 0x37,
			}
		}
	}
}
