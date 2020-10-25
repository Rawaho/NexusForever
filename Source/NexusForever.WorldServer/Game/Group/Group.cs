using Microsoft.EntityFrameworkCore.Internal;
using NexusForever.Shared;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Handler;
using NexusForever.WorldServer.Network.Message.Model;
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
        /// Max group size for <see cref="Group"/>
        /// </summary>
        public uint MaxGroupSize { get; set; }

        /// <summary>
        /// If this group is a raid.
        /// </summary>
        public bool IsRaid { get => Flags.HasFlag(GroupFlags.Raid); }
        
        /// <summary>
        /// True if the Group is full.
        /// </summary>
        public bool IsFull { get => Members.Count >= MaxGroupSize; }

        private bool isNewGroup { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="Group"/>
        /// </summary>
        public Group(ulong id, Player leader)
        {
            isNewGroup = true;
            Id     = id;
            Flags |= GroupFlags.OpenWorld;
            Leader = CreateMember(leader);

            this.SetGroupSize();
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
                    ExpireInvite(invite);
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
        /// Builds all <see cref="GroupMember"/>s into <see cref="NetworkGroupMember"/>s.
        /// </summary>
        public List<NetworkGroupMember> BuildGroupMembers()
        {
            List<NetworkGroupMember> memberList = new List<NetworkGroupMember>();
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
        public List<GroupMemberInfo> BuildMembersInfo()
        {
            List<GroupMemberInfo> memberList = new List<GroupMemberInfo>();
            uint groupIndex = 1;

            foreach (var member in Members.Values)
                memberList.Add(member.BuildMemberInfo(groupIndex++));

            return memberList;
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

        #region Invites

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
        /// Exprires the <see cref="GroupInvite"/> notifying all relevant parties of the expiration
        /// </summary>
        /// <param name="invite">The Invite to Expire.</param>
        public void ExpireInvite(GroupInvite invite)
        {
            if (!invites.ContainsKey(invite.InviteId))
                return;

            RemoveInvite(invite);
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
        /// Accepts the <see cref="GroupInvite"/> and Adds the Invited Player to the group.
        /// </summary>
        /// <param name="invite">The <see cref="GroupInvite"/> to accept.</param>
        public void AcceptInvite(string inviteeName)
        {
            GroupInvite invite = invites.Values.Single(inv => inv.TargetPlayer.Name.Equals(inviteeName));
            if (invite == null)
                return;

            AcceptInvite(invite);
        }
        /// <summary>
        /// Accepts the <see cref="GroupInvite"/> and Adds the Invited Player to the group.
        /// </summary>
        /// <param name="invite">The <see cref="GroupInvite"/> to accept.</param>
        public void AcceptInvite(GroupInvite invite)
        {
            GroupMember addedMember = CreateMember(invite.TargetPlayer);
            if (addedMember == null)
                return;

            RemoveInvite(invite);
            AddMember(addedMember);

            switch (invite.Type)
            {
                case GroupInviteType.Invite:
                    GroupHandler.SendGroupResult(Leader.Player.Session, GroupResult.Accepted, Id, invite.TargetPlayer.Name);
                    break;
                case GroupInviteType.Request:
                    invite.TargetPlayer.Session.EnqueueMessageEncrypted(new ServerGroupRequestJoinResult
                    {
                        GroupId = Id,
                        IsJoin = true,
                        Name = Leader.Player.Name,
                        Result = GroupResult.Accepted
                    });  
                    break;
                case GroupInviteType.Referral:
                    break;
            }
            
        }
         
        /// <summary>
        /// Declines the <see cref="GroupInvite"/> by player name.
        /// </summary>
        public void DeclineInvite(string inviteeName)
        {
            GroupInvite invite = invites.Values.Single(inv => inv.TargetPlayer.Name.Equals(inviteeName));
            if (invite == null)
                return;

            DeclineInvite(invite);
        }
        /// <summary>
        /// Declines the <see cref="GroupInvite"/> notifying all relevant parties of the invite result.
        /// </summary>
        public void DeclineInvite(GroupInvite invite)
        {
            if (!invites.ContainsKey(invite.InviteId))
                return;

            RemoveInvite(invite); 
            switch (invite.Type)
            {
                case GroupInviteType.Invite:
                    GroupHandler.SendGroupResult(Leader.Player.Session, GroupResult.Declined, Id, invite.TargetPlayer.Name);
                    break;
                case GroupInviteType.Request:
                    invite.TargetPlayer.Session.EnqueueMessageEncrypted(new ServerGroupRequestJoinResult
                    {
                        GroupId = Id,
                        IsJoin = false,
                        Name = Leader.Player.Name,
                        Result = GroupResult.Declined
                    });
                    //GroupHandler.SendGroupResult(invite.TargetPlayer.Session, GroupResult.Declined, Id, Leader.Player.Name);
                    break;
                case GroupInviteType.Referral:
                    break;
            } 
        }

        /// <summary>
        /// Refers a <see cref="Player"/> to be invited to the guild.
        /// </summary>
        public void ReferMember(GroupMember inviter, Player invitee)
        {
            if (CreateInvite(inviter, invitee, GroupInviteType.Referral) == null)
                return;

            Leader.Player.Session.EnqueueMessageEncrypted(
                new ServerGroupReferral
                {
                    GroupId = Id,
                    InviteeIdentity = new TargetPlayerIdentity {CharacterId = invitee.CharacterId, RealmId = WorldServer.RealmId },
                    InviteeName = invitee.Name
                }
            );
        }

        private void RemoveInvite(GroupInvite invite)
        {
            if (!invites.ContainsKey(invite.InviteId))
                return;

            invites.Remove(invite.InviteId);
            invite.TargetPlayer.GroupInvite = null;
        }

        #endregion
         
        /// <summary>
        /// Check if a <see cref="GroupMember"/> can join the <see cref="Group"/>
        /// </summary>
        public bool CanJoinGroup(out GroupResult result)
        {
            // Member count is over the max group member count.
            if (Members.Count >= MaxGroupSize)
            {
                result = GroupResult.Full;
                return false;
            }

            // Join requests are closed.
            if ((Flags & GroupFlags.JoinRequestClosed) != 0)
            {
                result = GroupResult.NotAcceptingRequests;
                return false;
            }

            result = GroupResult.Sent;
            return true;
        }

        /// <summary>
        /// Add a new <see cref="GroupMember"/> to the <see cref="Group"/>
        /// </summary>
        public void AddMember(GroupMember addedMember)
        {
            if (isNewGroup)
            {
                isNewGroup = false;

                foreach (var member in Members.Values)
                {
                    ServerGroupJoin groupJoinPacket = new ServerGroupJoin
                    {
                        TargetPlayer        = new TargetPlayerIdentity
                        {
                            CharacterId     = member.Player.CharacterId,
                            RealmId         = WorldServer.RealmId
                        },
                        GroupInfo           = Build()
                    };

                    member.Player.Session.EnqueueMessageEncrypted(groupJoinPacket);
                    // BroadcastPacket(member.BuildGroupStatUpdate());
                }
            }
            else
            {
                uint groupIndex = 0u;
                foreach (var _ in Members.Values)
                    groupIndex++;

                addedMember.Player.Session.EnqueueMessageEncrypted(new ServerGroupJoin
                {
                    TargetPlayer        = new TargetPlayerIdentity
                    {
                        CharacterId     = addedMember.Player.CharacterId,
                        RealmId         = WorldServer.RealmId
                    },
                    GroupInfo           = Build()
                });

                BroadcastPacket(new ServerGroupMemberAdd
                {
                    GroupId = Id,
                    AddedMemberInfo = addedMember.BuildMemberInfo(groupIndex)
                });
            }
        }
         
        /// <summary>
        /// Kick a <see cref="GroupMember"/> from the <see cref="Group"/>.
        /// </summary>
        public void KickMember(TargetPlayerIdentity target)
        {
            if (this.Members.Count == 2) {
                Disband();
                return;
            }

            GroupMember kickedMember = FindMember(target);
            if (kickedMember == null)
                return;

            if (kickedMember.IsPartyLeader)
                return;

            if (!Members.Remove(kickedMember.Id))
                return;

            // Tell the player they are no longer in a group.
            kickedMember.Player.GroupMember = null;
            kickedMember.Player.Session.EnqueueMessageEncrypted(new ServerGroupLeave
            {
                GroupId = Id,
                Reason = RemoveReason.Kicked
            });
             
            // Tell Other memebers of the group this player has been kicked.
            BroadcastPacket(new ServerGroupRemove
            {
                GroupId = Id,
                Reason = RemoveReason.Kicked,
                TargetPlayer = target
            }); 
        }

        /// <summary>
        /// Removes the <see cref="GroupMember"/> from the group.
        /// </summary>
        /// <param name="memberToRemove"></param>
        public void RemoveMember(GroupMember memberToRemove)
        {
            if (!this.Members.ContainsKey(memberToRemove.Id))
                return;

            memberToRemove.Player.GroupMember = null;
            Members.Remove(memberToRemove.Id);

            memberToRemove.Player.Session.EnqueueMessageEncrypted(new ServerGroupLeave
            {
                GroupId = Id,
                Reason = RemoveReason.Left
            });
            BroadcastPacket(new ServerGroupRemove
            {
                GroupId = Id,
                Reason = RemoveReason.Left,
                TargetPlayer = new TargetPlayerIdentity()
                {
                    CharacterId = memberToRemove.Player.CharacterId,
                    RealmId = WorldServer.RealmId
                }
            });
        }

        /// <summary>
        /// Disbands and removes the group from the <see cref="GroupManager"/>
        /// </summary>
        public void Disband()
        {
            foreach (GroupMember member in Members.Values)
                member.Player.GroupMember = null;

            BroadcastPacket(new ServerGroupLeave
            {
                 GroupId = Id,
                 Reason = RemoveReason.Disband
            });
            GroupManager.Instance.RemoveGroup(this);
        }

        /// <summary>
        /// Sets the <see cref="GroupFlags"/> on the group and broadcasts the changes to all members.
        /// </summary>
        /// <param name="newFlags"></param>
        public void SetGroupFlags(GroupFlags newFlags)
        {
            bool shouldSetToRaid = !IsRaid && newFlags.HasFlag(GroupFlags.Raid);
            Flags = newFlags; 
            
            if(shouldSetToRaid)
                ConvertToRaid();
             
            BroadcastPacket(new ServerGroupFlagsChanged
            {
                GroupId = Id,
                Flags = Flags,
            });
        }

        /// <summary>
        /// Converts the Party to a raid
        /// </summary>
        public void ConvertToRaid()
        { 
            SetGroupSize();
            BroadcastPacket(new ServerGroupMaxSizeChange
            {
                GroupId = Id, 
                NewFlags = Flags,
                NewMaxSize = MaxGroupSize
            });
        }

        /// <summary>
        /// Prepares the group for a readycheck
        /// </summary>
        public void PrepareForReadyCheck()
        {
            uint memberIndex = 0;
            foreach (GroupMember member in Members.Values)
            {
                member.PrepareForReadyCheck();

                BroadcastPacket(new ServerGroupMemberFlagsChanged
                {
                    GroupId = Id,
                    ChangedFlags = member.Flags,
                    IsFromPromotion = false,
                    MemberIndex = memberIndex,
                    TargetedPlayer = new TargetPlayerIdentity() { CharacterId = member.Player.CharacterId, RealmId = WorldServer.RealmId },
                });

                memberIndex++;
            }
        }

        /// <summary>
        /// Prepares the group for a readycheck
        /// </summary>
        public void PerformReadyCheck(Player invoker, string message)
        { 
            BroadcastPacket(new ServerGroupSendReadyCheck
            {
                GroupId = Id,
                Invoker = new TargetPlayerIdentity() { CharacterId = invoker.CharacterId, RealmId = WorldServer.RealmId },
                Message = message,
            });
        }

        /// <summary>
        /// Updates the Targeted Player Role.
        /// </summary>
        /// <param name="updater">The <see cref="GroupMember"/> attempting to update the Role of the target.</param>
        /// <param name="target">The Player whose <see cref="GroupMemberInfo"/> should be updated.</param>
        /// <param name="changedFlag">The flag to change</param>
        /// <param name="addPermission">If true, adds the permission to the <see cref="GroupMember"/> otherwise revokes it.</param>
        public void UpdateMemberRole(GroupMember updater, TargetPlayerIdentity target, GroupMemberInfoFlags changedFlag, bool addPermission)
        {
            GroupMember member = FindMember(target);
            if (member == null)
                return;

            if (!updater.CanUpdateFlags(changedFlag, member))
                return;
     
            uint memberIndex = 0;
            foreach (GroupMember groupMember in Members.Values) {
                if (member.Player.CharacterId == target.CharacterId)
                    break;

                memberIndex++;
            }
              
            member.SetFlags(changedFlag, addPermission); 
            BroadcastPacket(new ServerGroupMemberFlagsChanged
            {
                GroupId = Id,
                ChangedFlags = member.Flags,
                IsFromPromotion = false,
                MemberIndex = memberIndex,
                TargetedPlayer = target                
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void HandleJoinRequest(Player prospective)
        {
            if (CreateInvite(Leader, prospective, GroupInviteType.Request) == null)
                return;

            /** Currently assuming that most of this packet is feking useless - but its what is expected.
             * It seems stupid to send GroupMemeberInfo about somone who is not in the group.
             * If they are not in the group, GroupIndex and Flags are useless.
             */
            Leader.Player.Session.EnqueueMessageEncrypted(
                new ServerGroupRequestJoinResponse
                {
                     GroupId = Id,
                     MemberInfo = new GroupMemberInfo
                     {
                         Member = prospective.BuildGroupMember(),
                         Flags = 0,  // I am assuming this is useless, the client seems todo nothing with it
                         GroupIndex = 0, // I am assuming this is useless, the client seems todo nothing with it
                         MemberIdentity = new TargetPlayerIdentity() {  CharacterId = prospective.CharacterId, RealmId = WorldServer.RealmId }
                     } 
                }
            );
        }

        /// <summary>
        /// Find a <see cref="GroupMember"/> with the provided <see cref="TargetPlayerIdentity"/>
        /// </summary>
        public GroupMember FindMember(TargetPlayerIdentity target)
        {
            foreach (GroupMember member in Members.Values)
            {
                if (member.Player.CharacterId != target.CharacterId)
                    continue;

                return member;
            }

            return null;
        }

        private void SetGroupSize()
        {
            if (IsRaid)
                MaxGroupSize = 20;
            else
                MaxGroupSize = 5;
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
                LootRule            = LootRule.NeedBeforeGreed,
                LootRuleThreshold   = LootRule.RoundRobin,
                LootRuleHarvest     = HarvestLootRule.FirstTagger,
                LootThreshold       = LootThreshold.Good,
                MaxGroupSize        = MaxGroupSize,
                MemberInfos         = BuildMembersInfo(),
                RealmId             = WorldServer.RealmId,
            };
        }
    }
}
