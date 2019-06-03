﻿using NexusForever.Shared.GameTable;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    /// <summary>
    /// Used by <see cref="CurrencyManager"/>, and also used during Combat to provide Momentum Boosts.
    /// </summary>
    [Message(GameMessageOpcode.ServerCombatReward)]
    public class ServerCombatReward : IWritable
    {
        public byte Stat { get; set; }
        public ulong NewValue { get; set; }
        public uint CombatRewardId { get; set; } = 0;
        public uint TargetUnitId { get; set; } = 0;

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Stat, 5); //TODO Investigate
            writer.Write(NewValue);
            writer.Write(CombatRewardId);
            writer.Write(TargetUnitId);
        }
    }
}
