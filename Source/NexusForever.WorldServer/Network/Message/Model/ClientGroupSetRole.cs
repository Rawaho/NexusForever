using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupSetRole)]
    public class ClientGroupSetRole : IReadable
    {
        public ulong GroupId { get; set; }

        public TargetPlayerIdentity TargetedPlayer { get; set; } = new TargetPlayerIdentity();

        public GroupMemberInfoFlags CurrentFlags { get; set; }

        public GroupMemberInfoFlags ChangedFlag { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            TargetedPlayer.Read(reader);      
            CurrentFlags = reader.ReadEnum<GroupMemberInfoFlags>(32u);
            ChangedFlag = reader.ReadEnum<GroupMemberInfoFlags>(32u);
        }
    }
}
