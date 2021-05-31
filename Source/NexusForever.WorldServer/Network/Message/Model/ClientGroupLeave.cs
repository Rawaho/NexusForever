using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupLeave)]
    public class ClientGroupLeave : IReadable
    {
        public ulong GroupId { get; set; }

        public bool ShouldDisband { get; set; }


        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            ShouldDisband = reader.ReadBit(); 
        }
    }
}
