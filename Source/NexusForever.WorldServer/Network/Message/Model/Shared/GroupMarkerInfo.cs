using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.IO; 

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GroupMarkerInfo : IWritable
    {
        public MarkerInfo[] Markers { get; set; }
         
        public void Write(GamePacketWriter writer)
        {
            writer.Write(Markers.Length);
            foreach (MarkerInfo markerInfo in Markers)
                markerInfo.Write(writer);
        }
    }
}
