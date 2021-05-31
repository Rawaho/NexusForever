using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMaxSizeChange)]
    public class ServerGroupMaxSizeChange : IWritable
    {
        public ulong GroupId { get; set; }

        public GroupFlags NewFlags { get; set; }

        public uint NewMaxSize { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(NewFlags, 32u); 
            writer.Write(NewMaxSize);            
        }
    }
}
