using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupSendReadyCheck)]
    public class ServerGroupSendReadyCheck : IWritable
    {
        public ulong GroupId { get; set; }

        public TargetPlayerIdentity Invoker { get; set; }

        public string Message { get; set; }


        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            Invoker.Write(writer);
            writer.WriteStringWide(Message);
        }
    }
}
