﻿using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GroupInfo : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Flags { get; set; } = 1;
        public List<GroupMemberInfo> MemberInfos { get; set; } = new();
        public uint MaxGroupSize { get; set; }

        public LootRule LootRule { get; set; }
        public LootThreshold LootRuleThreshold { get; set; }
        public LootThreshold LootThreshold { get; set; }
        public HarvestLootRule LootRuleHarvest { get; set; }

        public TargetPlayerIdentity LeaderIdentity { get; set; } = new();
        public ushort RealmId { get; set; }     //< Why again? Tf?

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Flags);
            writer.Write(MemberInfos.Count);
            writer.Write(MaxGroupSize);

            writer.Write(LootRule, 3u);
            writer.Write(LootRuleThreshold, 3u);
            writer.Write(LootThreshold, 4u);
            writer.Write(LootRuleHarvest, 2u);

            MemberInfos.ForEach(member => member.Write(writer));

            LeaderIdentity.Write(writer);
            writer.Write(RealmId, 14);

            // Some unk for loop size, skipping for now, don't see anything on le sniff.
            writer.Write(0);
        }
    }
}
