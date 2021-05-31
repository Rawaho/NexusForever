using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static; 
using System.IO; 

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class MarkerInfo: IWritable
    {
        public uint UnitId { get; set; }

        public GroupMarker Marker { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Marker, 32u);
            writer.Write(UnitId);
        }
    }
}
