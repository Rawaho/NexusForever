using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupInviteResponse)]
    public class ClientGroupInviteResponse : IReadable
    {
        public ulong GroupId { get; set; }
        public GroupInviteResult Result { get; set; }
        public uint Unk1 { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            Result  = reader.ReadEnum<GroupInviteResult>(1);
            Unk1    = reader.ReadUInt();
        }
    }
}
