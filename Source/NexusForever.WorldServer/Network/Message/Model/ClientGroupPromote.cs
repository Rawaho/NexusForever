using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupPromote)]
    public class ClientGroupPromote : IReadable
    {
        public ulong GroupId { get; set; }

        public TargetPlayerIdentity TargetedPlayer { get; set; } = new TargetPlayerIdentity();

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            TargetedPlayer.Read(reader);
        }
    }
}
