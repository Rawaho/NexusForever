using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupJoin)]
    public class ServerGroupJoin : IWritable
    {
        public TargetPlayerIdentity JoinedPlayer { get; set; } = new();
        public GroupInfo GroupInfo { get; set; } = new();

        public void Write(GamePacketWriter writer)
        {
            JoinedPlayer.Write(writer);
            GroupInfo.Write(writer);
        }
    }
}
