using Microsoft.EntityFrameworkCore.Internal;
using NexusForever.Shared;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Handler;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;
using System.Linq;
using GroupMember = NexusForever.WorldServer.Game.Group.Model.GroupMember;
using NetworkGroupMember = NexusForever.WorldServer.Network.Message.Model.Shared.GroupMember;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group : IUpdate, IBuildable<GroupInfo>
    {
        private readonly Dictionary<ulong, GroupInvite> invites = new Dictionary<ulong, GroupInvite>();
        public Dictionary<ulong, GroupMember> Members = new Dictionary<ulong, GroupMember>();

        /// <summary>
        /// Id for the current <see cref="Group"/>
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// The <see cref="Group"/> leader
        /// </summary>
        public GroupMember Leader { get; }

        /// <summary>
        /// <see cref="GroupFlags"/> for <see cref="Group"/>
        /// </summary>
        public GroupFlags Flags { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="Group"/>
        /// </summary>
        public Group(ulong id, Player leader)
        {
            Id     = id;
            Flags |= GroupFlags.OpenWorld;
            Leader = CreateMember(leader);
        }

        /// <summary>
        /// Invite the targeted <see cref="Player"/>
        /// </summary>
        public void Invite(Player inviter, Player invitedPlayer)
        {
            GroupHandler.SendGroupResult(inviter.Session, GroupResult.Sent, Id, invitedPlayer.Name);

            GroupInvite invite = CreateInvite(inviter.GroupMember, invitedPlayer, GroupInviteType.Invite);
            invite.SendInvite();
        }

        /// <summary>
        /// Create and add a new <see cref="GroupMember"/>
        /// to the <see cref="Group"/>
        /// </summary>
        public GroupMember CreateMember(Player player)
        {
            GroupMember member = new GroupMember(NextMemberId(), this, player);
            Members.Add(member.Id, member);

            player.GroupMember = member;
            return member;
        }

        public void Update(double lastTick)
        {
            foreach (GroupInvite invite in invites.Values)
            {
                invite.ExpirationTime -= lastTick;
                if (invite.ExpirationTime <= 0d)
                {
                    // Delete the current invite
                    RevokeInvite(invite, true);
                }
            }
        }

        /// <summary>
        /// Get the next available MemberId
        /// </summary>
        public ulong NextMemberId()
        {
            if (Members.Count > 0)
                return Members.Last().Key + 1UL;
            else
                return 1;
        }

        /// <summary>
        /// Get the next available InviteId
        /// </summary>
        /// <returns></returns>
        public ulong NextInviteId()
        {
            if (invites.Count > 0)
                return invites.Last().Key + 1UL;
            else
                return 1;
        }

        /// <summary>
        /// Builds all <see cref="GroupMember"/>s into <see cref="NetworkGroupMember"/>s.
        /// </summary>
        public List<NetworkGroupMember> BuildGroupMembers()
        {
            List<NetworkGroupMember> memberList = new();
            foreach (var member in Members.Values)
            {
                NetworkGroupMember groupMember = member.Build();
                groupMember.GroupMemberId = (ushort)member.Id;
                memberList.Add(member.Build());
            }

            return memberList;
        }

        /// <summary>
        /// Builds all <see cref="GroupMember"/> into <see cref="GroupMemberInfo"/>
        /// </summary>
        public List<GroupMemberInfo> BuildMemberInfo()
        {
            List<GroupMemberInfo> memberList = new();
            uint groupIndex = 1;

            foreach (var member in Members.Values)
            {
                NetworkGroupMember groupMember = member.Build();
                groupMember.GroupMemberId = (ushort)member.Id;

                GroupMemberInfo memberInfo = new GroupMemberInfo
                {
                    Member          = groupMember,
                    GroupIndex      = groupIndex++,
                    MemberIdentity  = new TargetPlayerIdentity
                    {
                        CharacterId = member.Player.CharacterId,
                        RealmId     = WorldServer.RealmId
                    },
                    Flags           = (uint)member.Flags
                };

                memberList.Add(memberInfo);
            }

            return memberList;
        }

        /// <summary>
        /// Create a new <see cref="GroupInvite"/>
        /// </summary>
        public GroupInvite CreateInvite(GroupMember inviter, Player invitedPlayer, GroupInviteType type)
        {
            GroupInvite invite = new GroupInvite(NextInviteId(), this, invitedPlayer, inviter, type);
            if (!invites.TryAdd(invite.InviteId, invite))
                return null;

            invitedPlayer.GroupInvite = invite;
            return invite;
        }

        /// <summary>
        /// Broadcast <see cref="IWritable"/> to all <see cref="GroupMember"/>
        /// in the <see cref="Group"/>
        /// </summary>
        public void BroadcastPacket(IWritable message)
        {
            foreach (var member in Members.Values)
                member.Player.Session.EnqueueMessageEncrypted(message);
        }

        /// <summary>
        /// Revoke <see cref="GroupInvite"/>
        /// </summary>
        public void RevokeInvite(GroupInvite invite, bool isExpired = false)
        {
            if (!invites.ContainsKey(invite.InviteId))
                return;

            invites.Remove(invite.InviteId);
            invite.TargetPlayer.GroupInvite = null;

            if (!isExpired)
                return;

            switch (invite.Type)
            {
                case GroupInviteType.Invite:
                    GroupHandler.SendGroupResult(Leader.Player.Session, GroupResult.ExpiredInviter, Id, invite.TargetPlayer.Name);
                    GroupHandler.SendGroupResult(invite.TargetPlayer.Session, GroupResult.ExpiredInvitee, Id, invite.TargetPlayer.Name);
                    break;
                case GroupInviteType.Request:
                case GroupInviteType.Referral:
                    break;
            }
        }

        /// <summary>
        /// Build the <see cref="GroupInfo"/> structure from the current <see cref="Group"/>
        /// </summary>
        public GroupInfo Build()
        {
            return new GroupInfo
            {
                GroupId             = Id,
                Flags               = Flags,
                LeaderIdentity      = new TargetPlayerIdentity
                {
                    CharacterId     = Leader.Player.CharacterId,
                    RealmId         = WorldServer.RealmId
                },
                LootRule            = LootRule.FreeForAll,
                LootRuleHarvest     = HarvestLootRule.FirstTagger,
                LootRuleThreshold   = LootThreshold.Artifact,
                LootThreshold       = LootThreshold.Artifact,
                MaxGroupSize        = 5,   //< Hardcoded for now
                MemberInfos         = BuildMemberInfo(),
                RealmId             = WorldServer.RealmId,
            };
        }
    }
}
