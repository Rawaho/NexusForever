using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupJoin)]
    public class ServerGroupJoin : IWritable
    {
        public void Write(GamePacketWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
