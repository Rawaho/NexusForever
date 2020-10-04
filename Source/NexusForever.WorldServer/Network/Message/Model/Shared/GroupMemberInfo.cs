using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GroupMemberInfo : IWritable
    {
        public TargetPlayerIdentity MemberIdentity { get; set; } = new TargetPlayerIdentity();
        public uint Flags { get; set; }
        public GroupMember Member { get; set; }
        public uint GroupIndex { get; set; }

        public void Write(GamePacketWriter writer)
        {
            MemberIdentity.Write(writer);
            writer.Write(Flags);
            Member.Write(writer);
            writer.Write(GroupIndex);
        }
    }
}
