using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GroupMemberInfo : IWritable
    {
        public TargetPlayerIdentity MemberIdentity { get; set; } = new TargetPlayerIdentity();
        public GroupMemberInfoFlags Flags { get; set; }
        public GroupMember Member { get; set; }
        public uint GroupIndex { get; set; }

        public void Write(GamePacketWriter writer)
        {
            MemberIdentity.Write(writer);
            writer.Write(Flags, 32);
            Member.Write(writer);
            writer.Write(GroupIndex);
        }
    }
}
