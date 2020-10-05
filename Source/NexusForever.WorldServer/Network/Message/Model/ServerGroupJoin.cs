using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupJoin)]
    public class ServerGroupJoin : IWritable
    {
        public TargetPlayerIdentity TargetPlayer { get; set; } = new TargetPlayerIdentity();
        public GroupInfo GroupInfo { get; set; } = new GroupInfo();

        public void Write(GamePacketWriter writer)
        {
            TargetPlayer.Write(writer);
            GroupInfo.Write(writer);
        }
    }
}
