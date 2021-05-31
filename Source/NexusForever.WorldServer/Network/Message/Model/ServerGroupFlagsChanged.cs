using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupFlagsChanged)]
    public class ServerGroupFlagsChanged : IWritable
    {
        public ulong GroupId { get; set; }

        public GroupFlags Flags { get; set; }

        public uint Unk0 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);            
            writer.Write(Unk0);
            writer.Write(Flags, 32u);
        }
    }
}
