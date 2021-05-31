using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupLootRulesChange)]
    public class ClientGroupLootRulesChange : IReadable
    {
        public ulong GroupId { get; set; }
         
        public LootRule LootRulesUnderThreshold { get; set; }

        public LootRule LootRulesThresholdAndOver { get; set; }

        public LootThreshold Threshold { get; set; }

        public HarvestLootRule HarvestingRule { get; set; }


        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            LootRulesUnderThreshold = reader.ReadEnum<LootRule>(3u);
            LootRulesThresholdAndOver = reader.ReadEnum<LootRule>(3u);
            Threshold = reader.ReadEnum<LootThreshold>(4u);
            HarvestingRule = reader.ReadEnum<HarvestLootRule>(2u);
        }
    }
}
