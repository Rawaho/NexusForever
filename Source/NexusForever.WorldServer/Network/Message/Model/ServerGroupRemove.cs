using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupRemove)]
    public class ServerGroupRemove : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Unk0 { get; set; }
        public TargetPlayerIdentity TargetPlayer { get; set; } = new TargetPlayerIdentity();
        public RemoveReason Reason { get; set; }

        public void Write(GamePacketWriter writer)
        {
            
        }
    }
}
