using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupUpdatePlayerRealm)]
    public class ServerGroupUpdatePlayerRealm : IWritable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity TargetPlayerIdentity { get; set; }
        public uint RealmId { get; set; }
        public uint ZoneId { get; set; }
        public uint MapId { get; set; }
        public uint PhaseId { get; set; }
        public bool IsSyncdToGroup { get; set; }
         
        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            TargetPlayerIdentity.Write(writer);
            writer.Write(RealmId, 14);
            writer.Write(ZoneId, 15);
            writer.Write(MapId);
            writer.Write(PhaseId);
            writer.Write(IsSyncdToGroup);
        }
    }
}
