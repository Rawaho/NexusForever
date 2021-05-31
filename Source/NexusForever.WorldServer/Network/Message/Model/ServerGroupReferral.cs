using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupReferral)]
    public class ServerGroupReferral : IWritable
    {
        public ulong GroupId { get; set; }

        public TargetPlayerIdentity InviteeIdentity { get; set; }

        public string InviteeName { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            InviteeIdentity.Write(writer);
            writer.WriteStringWide(InviteeName);
        }
    }
}
