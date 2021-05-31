using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    /// <summary>
    /// Tells a player they are no longer part of the group.
    /// Invokes the 'Group_Leave' event in Apollo. Which is invoked only for the current player.
    /// </summary>
    [Message(GameMessageOpcode.ServerGroupRemove)]
    public class ServerGroupRemove : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Unk0 { get; set; }
        public TargetPlayerIdentity TargetPlayer { get; set; } = new TargetPlayerIdentity();
        public RemoveReason Reason { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Unk0);

            TargetPlayer.Write(writer);
            writer.Write(Reason, 4u);
        }
    }
}
