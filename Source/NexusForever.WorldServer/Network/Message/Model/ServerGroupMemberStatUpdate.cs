using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMemberStatUpdate)]
    public class ServerGroupMemberStatUpdate : IWritable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity TargetPlayer { get; set; }

        public byte Level { get; set; }
        public byte EffectiveLevel { get; set; }

        public uint Unk1 { get; set; }
        public ushort GroupMemberId { get; set; }

        public GroupMember.UnknownStruct0[] SomeStatList = new GroupMember.UnknownStruct0[5];

        public ushort Health { get; set; }
        public ushort HealthMax { get; set; }
        public ushort Shield { get; set; }
        public ushort ShieldMax { get; set; }
        public ushort InterruptArmor { get; set; }
        public ushort InterruptArmorMax { get; set; }
        public ushort Absorption { get; set; }
        public ushort AbsorptionMax { get; set; }
        public ushort Mana { get; set; }
        public ushort ManaMax { get; set; }
        public ushort HealingAbsorb { get; set; }
        public ushort HealingAbsorbMax { get; set; }

        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public byte Unk5 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            TargetPlayer.Write(writer);

            writer.Write(Level, 7);
            writer.Write(EffectiveLevel, 7);

            writer.Write(Unk1, 17);
            writer.Write(GroupMemberId);

            for (var i = 0; i < 5; ++i)
            {
                SomeStatList[i] = new GroupMember.UnknownStruct0();
                SomeStatList[i].Write(writer);
            }

            writer.Write(Health);
            writer.Write(HealthMax);
            writer.Write(Shield);
            writer.Write(ShieldMax);
            writer.Write(InterruptArmor);
            writer.Write(InterruptArmorMax);
            writer.Write(Absorption);
            writer.Write(AbsorptionMax);
            writer.Write(Mana);
            writer.Write(ManaMax);
            writer.Write(HealingAbsorb);
            writer.Write(HealingAbsorbMax);

            writer.Write(Unk3);
            writer.Write(Unk4);
            writer.Write(Unk5, 3);
        }
    }
}
