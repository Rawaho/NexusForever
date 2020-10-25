using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupRequestJoinResponse)]
    public class ServerGroupRequestJoinResponse : IWritable
    {
        public ulong GroupId { get; set; }

        public GroupMemberInfo MemberInfo { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            MemberInfo.Write(writer);
        }
    }
}
