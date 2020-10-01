using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
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
        public bool CanKick => IsPartyLeader || (Flags & GroupMemberInfoFlags.CanKick) != 0;
        public bool CanInvite => IsPartyLeader || (Flags & GroupMemberInfoFlags.CanInvite) != 0;
        public bool CanMark => IsPartyLeader || (Flags & GroupMemberInfoFlags.CanMark) != 0;
        public bool CanReadyCheck => IsPartyLeader || (Flags & GroupMemberInfoFlags.CanReadyCheck) != 0;

        public GroupMember(ulong id, Group group, Player player)
        {
            Id      = id;
            Group   = group;
            Player  = player;
            ZoneId  = (ushort)player.Zone.Id;
        }

        /// <summary>
        /// Generate Info flags that can be sent to the client.
        /// </summary>
        public GroupMemberInfoFlags Flags
        {
            get
            {
                GroupMemberInfoFlags flags = this.flags;
                if (IsPartyLeader)
                    flags |= GroupMemberInfoFlags.GroupAdminFlags;
                else
                    flags |= GroupMemberInfoFlags.GroupMemberFlags;

                if ((flags & GroupMemberInfoFlags.RaidAssistant) != 0)
                    flags |= GroupMemberInfoFlags.GroupAssistantFlags;

                if ((flags & GroupMemberInfoFlags.MainTank) != 0)
                {
                    flags |= GroupMemberInfoFlags.MainTankFlags;
                    flags &= ~GroupMemberInfoFlags.RoleFlags;
                    flags |= GroupMemberInfoFlags.Tank;
                }

                if ((flags & GroupMemberInfoFlags.MainAssist) != 0)
                    flags |= GroupMemberInfoFlags.MainAssistFlags;

                return flags;
            }
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

        /// <summary>
        /// Build the <see cref="NetworkGroupMember"/>.
        /// </summary>
        public NetworkGroupMember Build() => Player.BuildGroupMember();
    }
}
