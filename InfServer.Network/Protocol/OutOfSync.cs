﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Network;

namespace InfServer.Protocol
{	/// <summary>
	/// OutOfSync is used for indicating that a reliable packet received was out of sync
	/// </summary>
	public class OutOfSync : PacketBase
	{	// Member Variables
		///////////////////////////////////////////////////
		public Int16 rNumber;

		//Packet routing
		public const ushort TypeID = (ushort)0x11;
		static public event Action<OutOfSync, Client> Handlers;


		///////////////////////////////////////////////////
		// Member Functions
		//////////////////////////////////////////////////
		/// <summary>
		/// Creates an empty packet of the specified type. This is used
		/// for constructing new packets for sending.
		/// </summary>
		public OutOfSync()
			: base(TypeID)
		{ }

		/// <summary>
		/// Creates an instance of the dummy packet used to debug communication or 
		/// to represent unknown packets.
		/// </summary>
		/// <param name="typeID">The type of the received packet.</param>
		/// <param name="buffer">The received data.</param>
		public OutOfSync(ushort typeID, byte[] buffer, int index, int count)
			: base(typeID, buffer, index, count)
		{ }

		/// <summary>
		/// Routes a new packet to various relevant handlers
		/// </summary>
		public override void Route()
		{	//Call all handlers!
			if (Handlers != null)
				Handlers(this, (Client)_client);
		}

		/// <summary>
		/// Serializes the data stored in the packet class into a byte array ready for sending.
		/// </summary>
		public override void Serialize()
		{
			
		}

		/// <summary>
		/// Deserializes the data present in the packet contents into data fields in the class.
		/// </summary>
		public override void Deserialize()
		{
			rNumber = Flip(_contentReader.ReadInt16());
		}

		/// <summary>
		/// Returns a meaningful of the packet's data
		/// </summary>
		public override string Dump
		{
			get
			{
				return "Out of sync (" + rNumber + ")";
			}
		}
	}
}
