using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupRequestJoin)]
    public class ClientGroupRequestJoin : IReadable
    {
        public string Name { get; set; }
        public string UnknownString { get; set; }

        public void Read(GamePacketReader reader)
        {
            Name = reader.ReadWideString();
            UnknownString = reader.ReadWideString();
        }
    }
}
