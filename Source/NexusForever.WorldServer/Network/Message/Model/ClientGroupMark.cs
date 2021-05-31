using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupMarkUnit)]
    public class ClientGroupMark : IReadable
    {
        public GroupMarker Marker { get; set; }

        public uint UnitId { get; set; } 

        public void Read(GamePacketReader reader)
        {
            Marker = reader.ReadEnum<GroupMarker>(32u);
            UnitId = reader.ReadUInt();
        }
    }
}
