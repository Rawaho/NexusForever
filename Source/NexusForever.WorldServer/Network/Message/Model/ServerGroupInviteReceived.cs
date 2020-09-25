using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupInviteReceived)]
    public class ServerGroupInviteReceived : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Unknown0 { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

        public List<GroupMember> Members = new List<GroupMember>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Unknown0);
            writer.Write(Unknown1);
            writer.Write(Unknown2);

            writer.Write(Members.Count);
            Members.ForEach(x => x.Write(writer));
        }
    }
}
