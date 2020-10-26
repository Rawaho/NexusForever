using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMarkUnit)]
    public class ServerGroupMarkUnit : IWritable
    {
        public ulong GroupId { get; set; }

        public GroupMarker Marker { get; set; }

        public uint UnitId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Marker, 32u);
            writer.Write(UnitId);
        }
    }
}
