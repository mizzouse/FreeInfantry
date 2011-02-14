﻿using System;
using InfServer.Network;

namespace InfServer.DirectoryServer.Directory.Protocol.Packets
{
    public class CS_Initiate : PacketBase
    {
        static public event Action<CS_Initiate, DirectoryClient> Handlers;

        public const ushort TypeID = 1;

        public UInt32 RandChallengeToken;

        public CS_Initiate() : base(TypeID)
        {
        }

        public CS_Initiate(ushort typeID, byte[] buffer, int index, int count) : base(typeID, buffer, index, count)
        {
        }

        public override void Route()
        {	//Call all handlers!
            if (Handlers != null)
                Handlers(this, ((DirectoryClient)_client));
        }

        public override void Deserialize()
        {
            _contentReader.ReadUInt32(); // Discard leading data
            RandChallengeToken = Flip(_contentReader.ReadUInt32());
        }
    }
}
