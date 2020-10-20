using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NetworkGroupMember = NexusForever.WorldServer.Network.Message.Model.Shared.GroupMember;

namespace NexusForever.WorldServer.Game.Group.Model
{
    public class GroupMember : IBuildable<NetworkGroupMember>
    {
        public ulong Id { get; }
        public Group Group { get; }
        public Player Player { get; }
        public ushort ZoneId { get; set; }

        private GroupMemberInfoFlags flags;

        public bool IsPartyLeader => Group.Leader?.Id == Id;
        public bool CanKick => (Flags & GroupMemberInfoFlags.CanKick) != 0;
        public bool CanInvite => (Flags & GroupMemberInfoFlags.CanInvite) != 0;
        public bool CanMark => (Flags & GroupMemberInfoFlags.CanMark) != 0;
        public bool CanReadyCheck => (Flags & GroupMemberInfoFlags.CanReadyCheck) != 0;

        private bool AreFlagsSet;

        public GroupMember(ulong id, Group group, Player player)
        {
            Id      = id;
            Group   = group;
            Player  = player;
            ZoneId  = (ushort)player.Zone.Id;
            AreFlagsSet = false;
        }

        /// <summary>
        /// Generate Info flags that can be sent to the client.
        /// </summary>
        public GroupMemberInfoFlags Flags
        {
            get {
                if (!AreFlagsSet)
                    SetInitialFlags();

                return flags;
            }
            set { this.flags = value; }
        }

        /// <summary>
        /// Can this member update given flags for the given member?
        /// </summary>
        public bool CanUpdateFlags(GroupMemberInfoFlags updateFlags, GroupMember other)
        {
            if (IsPartyLeader)
                return true;

            if ((flags & GroupMemberInfoFlags.RaidAssistant) != 0)
                return true;

            if (other.Id != Id)
                return false;

            GroupMemberInfoFlags allowedFlags = GroupMemberInfoFlags.RoleFlags
                             | GroupMemberInfoFlags.HasSetReady
                             | GroupMemberInfoFlags.Ready;
            return (updateFlags & allowedFlags) == updateFlags;
        }

        /// <summary>
        /// Clear ready check related flags
        /// </summary>
        public void PrepareForReadyCheck()
        {
            GroupMemberInfoFlags unset = GroupMemberInfoFlags.HasSetReady
                      | GroupMemberInfoFlags.Ready;
            flags &= ~unset;
            flags |= GroupMemberInfoFlags.Pending;
        }

        /// <summary>
        /// Toggle flags on/off.
        /// </summary>
        public void SetFlags(GroupMemberInfoFlags flags, bool value)
        {
            if (value && (flags & GroupMemberInfoFlags.RoleFlags) != 0)
                this.flags &= ~GroupMemberInfoFlags.RoleFlags;

            if (value && (flags & GroupMemberInfoFlags.HasSetReady) != 0)
                this.flags &= ~GroupMemberInfoFlags.Pending;

            if (value)
                this.flags |= flags;
            else
                this.flags &= ~flags;
        }

        private void SetInitialFlags()
        {
            this.AreFlagsSet = true;
            if (IsPartyLeader)
                flags |= GroupMemberInfoFlags.GroupAdminFlags;
            else
                flags |= GroupMemberInfoFlags.GroupMemberFlags;
        }

        /// <summary>
        /// Build the <see cref="NetworkGroupMember"/>.
        /// </summary>
        public NetworkGroupMember Build() => Player.BuildGroupMember();

        /// <summary>
        /// Build the <see cref="GroupMemberInfo"/> model.
        /// </summary>
        public GroupMemberInfo BuildMemberInfo(uint index)
        {
            return new GroupMemberInfo
            {
                MemberIdentity  = new TargetPlayerIdentity
                {
                    CharacterId = Player.CharacterId,
                    RealmId     = WorldServer.RealmId
                },
                Flags           = Flags,
                GroupIndex      = index,
                Member          = Build()
            };
        }

        /// <summary>
        /// Build <see cref="ServerEntityGroupAssociation"/>
        /// </summary>
        public ServerEntityGroupAssociation BuildGroupAssociation()
        {
            return new ServerEntityGroupAssociation
            {
                UnitId  = Player.Guid,
                GroupId = Group.Id
            };
        }

        /// <summary>
        /// Build <see cref="ServerGroupMemberStatUpdate"/>
        /// </summary>
        public ServerGroupMemberStatUpdate BuildGroupStatUpdate()
        {
            return new ServerGroupMemberStatUpdate
            {
                GroupId             = Group.Id,
                GroupMemberId       = (ushort)Id,
                TargetPlayer        = new TargetPlayerIdentity
                {
                    CharacterId     = Player.CharacterId,
                    RealmId         = WorldServer.RealmId
                },
                Level               = (byte)Player.Level,
                EffectiveLevel      = (byte)Player.Level,
                Health              = (ushort)Player.Health,
                HealthMax           = (ushort)Player.Health,
                Shield              = (ushort)Player.Shield,
                ShieldMax           = (ushort)Player.Shield,
                InterruptArmor      = (ushort)Player.InterruptArmor,
                InterruptArmorMax   = (ushort)Player.InterruptArmor
            };
        }
    }
}
