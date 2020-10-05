using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMemberFlagsChanged)]
    public class ServerGroupMemberFlagsChanged : IWritable
    {
        public ulong GroupId { get; set; }
        public uint MemberIndex { get; set; } //< Not sure
        public TargetPlayerIdentity TargetedPlayer { get; set; } = new TargetPlayerIdentity();
        public GroupMemberInfoFlags ChangedFlags { get; set; }
        public bool IsFromPromotion { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(MemberIndex);
            TargetedPlayer.Write(writer);
            writer.Write(ChangedFlags, 32);
            writer.Write(IsFromPromotion);
        }
    }
}
