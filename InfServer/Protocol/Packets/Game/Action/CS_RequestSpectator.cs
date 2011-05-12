﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Network;
using InfServer.Game;

namespace InfServer.Protocol
{	/// <summary>
	/// CS_RequestSpectator is used when a player requests to spectate another player
	/// </summary>
	public class CS_RequestSpectator : PacketBase
	{	// Member Variables
		///////////////////////////////////////////////////
		public UInt16 playerID;			//The player to spectator
		public Int32 unk1;	

		//Packet routing
		public const ushort TypeID = (ushort)Helpers.PacketIDs.C2S.RequestSpectator;
		static public Action<CS_RequestSpectator, Player> Handlers;


		///////////////////////////////////////////////////
		// Member Functions
		//////////////////////////////////////////////////
		/// <summary>
		/// Creates an instance of the dummy packet used to debug communication or 
		/// to represent unknown packets.
		/// </summary>
		/// <param name="typeID">The type of the received packet.</param>
		/// <param name="buffer">The received data.</param>
		public CS_RequestSpectator(ushort typeID, byte[] buffer, int index, int count)
			: base(typeID, buffer, index, count)
		{
		}

		/// <summary>
		/// Routes a new packet to various relevant handlers
		/// </summary>
		public override void Route()
		{	//Call all handlers!
			if (Handlers != null)
				Handlers(this, ((Client<Player>)_client)._obj);
		}

		/// <summary>
		/// Deserializes the data present in the packet contents into data fields in the class.
		/// </summary>
		public override void Deserialize()
		{
			playerID = _contentReader.ReadUInt16();
			unk1 = _contentReader.ReadInt32();
		}

		/// <summary>
		/// Returns a meaningful of the packet's data
		/// </summary>
		public override string Dump
		{
			get
			{
				return "Player spectate request";
			}
		}
	}
}
