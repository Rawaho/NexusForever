using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupFlagsChanged)]
    public class ClientGroupFlagsChanged : IReadable
    {
        public ulong GroupId { get; set; }

        public GroupFlags NewFlags { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            NewFlags = reader.ReadEnum<GroupFlags>(32u);
        }
    }
}
