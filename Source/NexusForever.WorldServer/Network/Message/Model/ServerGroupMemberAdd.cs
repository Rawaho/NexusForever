using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMemberAdd)]
    public class ServerGroupMemberAdd : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Unknown0 { get; set; }
        public GroupMemberInfo AddedMemberInfo { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Unknown0);
            AddedMemberInfo.Write(writer);
        }
    }
}
