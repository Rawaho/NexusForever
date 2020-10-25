using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Diagnostics;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupRequestJoinResponse)]
    public class ClientGroupRequestJoinResponse : IReadable
    {
        public ulong GroupId { get; set; }

        public bool AcceptedRequest { get; set; }

        public string InviteeName { get; set; }


        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            AcceptedRequest = reader.ReadBit();
            InviteeName = reader.ReadWideString(); 
        }
    }
}
