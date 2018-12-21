﻿using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server06B9, MessageDirection.Server)]
    public class Server06B9 : IWritable
    {
        public ushort Unknown0 { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Unknown0, 14);
            writer.Write(Unknown1);
            writer.Write(Unknown2, Unknown1 * 4);
        }
    }
}
