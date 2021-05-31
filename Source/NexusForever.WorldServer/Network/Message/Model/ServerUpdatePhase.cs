using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerUpdatePhase)]
    public class ServerUpdatePhase : IWritable
    {
        public uint CanSee { get; set; } = 1;
        public uint CanSeeMe { get; set; } = 1;

        public void Write(GamePacketWriter writer)
        {
            writer.Write(CanSee);
            writer.Write(CanSeeMe);
        }
    }
}
