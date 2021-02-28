using Microsoft.EntityFrameworkCore.Internal;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Handler;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq; 
using GroupMember = NexusForever.WorldServer.Game.Group.Model.GroupMember;
using NetworkGroupMember = NexusForever.WorldServer.Network.Message.Model.Shared.GroupMember;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group : IUpdate, IBuildable<GroupInfo>
    {
        private readonly Dictionary<ulong, GroupInvite> invites = new Dictionary<ulong, GroupInvite>();

        private Dictionary<ulong, GroupMember> membershipsByCharacterID = new Dictionary<ulong, GroupMember>();
        private List<GroupMember> members = new List<GroupMember>();

        public int MemberCount { get => members.Count; }

        private Model.GroupMarkerInfo markerInfo;

        private LootRule lootRule = LootRule.NeedBeforeGreed;
        private LootRule lootRuleThreshold = LootRule.RoundRobin;
        private HarvestLootRule lootRuleHarvest = HarvestLootRule.FirstTagger;
        private LootThreshold lootThreshold = LootThreshold.Good;

        /// <summary>
        /// Id for the current <see cref="Group"/>
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// The <see cref="Group"/> leader
        /// </summary>
        public GroupMember Leader { get; private set; }

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
        public bool IsFull { get => members.Count >= MaxGroupSize; }

        private bool isNewGroup { get; set; }

        private UpdateTimer positionUpdateTimer;
          
        /// <summary>
        /// Creates an instance of <see cref="Group"/>
        /// </summary>
        public Group(ulong id, Player leader)
        {
            isNewGroup = true;
            Id     = id;
            Flags |= GroupFlags.OpenWorld;
            Leader = CreateMember(leader);
            markerInfo = new Model.GroupMarkerInfo(this);

            SetGroupSize();
            positionUpdateTimer = new UpdateTimer(1, false);
        }

        /// <summary>
        /// Invite the targeted <see cref="Player"/>
        /// </summary>
        public void Invite(Player inviter, Player invitedPlayer)
        {
            GroupHandler.SendGroupResult(inviter.Session, GroupResult.Sent, Id, invitedPlayer.Name);

            GroupInvite invite = CreateInvite(inviter.GroupMember, invitedPlayer, GroupInviteType.Invite);
            SendInvite(invite);
        }

        /// <summary>
        /// Create and add a new <see cref="GroupMember"/>
        /// to the <see cref="Group"/>
        /// </summary>
        public GroupMember CreateMember(Player player)
        {
            GroupMember member = new GroupMember(NextMemberId(), this, player);
            members.Add(member);
            membershipsByCharacterID.Add(player.CharacterId, member);

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

             
            positionUpdateTimer.Update(lastTick); 
            if (positionUpdateTimer.HasElapsed)
            {
                positionUpdateTimer.Reset();
                UpdatePositions();
            } 
        }

        /// <summary>
        /// Get the next available MemberId
        /// </summary>
        public ulong NextMemberId()
        {
            if (members.Count > 0)
                return members.Last().Id + 1UL;
            else
                return 1;
        }

        /// <summary>
        /// Builds all <see cref="GroupMember"/>s into <see cref="NetworkGroupMember"/>s.
        /// </summary>
        public List<NetworkGroupMember> BuildGroupMembers()
        {
            List<NetworkGroupMember> memberList = new List<NetworkGroupMember>();
            foreach (var member in members)
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

            foreach (var member in members)
                memberList.Add(member.BuildMemberInfo());

            return memberList;
        }

        /// <summary>
        /// Broadcast <see cref="IWritable"/> to all <see cref="GroupMember"/>
        /// in the <see cref="Group"/>
        /// </summary>
        public void BroadcastPacket(IWritable message)
        {
            foreach (var member in members)
            {
                // If the player is not online - can't give them the message.
                if (!GetPlayerByCharacterId(member.CharacterId, out Player memberPlayer))
                    continue;

                memberPlayer.Session.EnqueueMessageEncrypted(message);
            }
        }

        #region Invites

        /// <summary>
        /// Create a new <see cref="GroupInvite"/>
        /// </summary>
        public GroupInvite CreateInvite(GroupMember inviter, Player invitedPlayer, GroupInviteType type)
        {
            GroupInvite invite = new GroupInvite(NextInviteId(), this, invitedPlayer.CharacterId, invitedPlayer.Name, inviter, type);
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
                    if (GetPlayerByCharacterId(Leader.CharacterId, out Player leader))
                        GroupHandler.SendGroupResult(leader.Session, GroupResult.ExpiredInviter, Id, invite.InvitedCharacterName);

                    if(GetPlayerByCharacterId(invite.InvitedCharacterId, out Player invited))
                        GroupHandler.SendGroupResult(invited.Session, GroupResult.ExpiredInvitee, Id, invite.InvitedCharacterName);
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
            GroupInvite invite = invites.Values.SingleOrDefault(inv => inv.InvitedCharacterName.Equals(inviteeName));
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
            if (!invites.ContainsKey(invite.InviteId))
                return;

            // If the person who accepted the invite is not online - how TF did they accept the invite?
            if (!GetPlayerByCharacterId(invite.InvitedCharacterId, out Player targetPlayer)) 
                return;

            GroupMember addedMember = CreateMember(targetPlayer);
            if (addedMember == null)
                return;

            RemoveInvite(invite);
            AddMember(addedMember);

            GetPlayerByCharacterId(Leader.CharacterId, out Player leader);

            switch (invite.Type)
            {
                case GroupInviteType.Invite:
                    if (leader != null)
                        GroupHandler.SendGroupResult(leader.Session, GroupResult.Accepted, Id, targetPlayer.Name);

                    break;
                case GroupInviteType.Request:
                    targetPlayer.Session.EnqueueMessageEncrypted(new ServerGroupRequestJoinResult
                    {
                        GroupId = Id,
                        IsJoin = true,
                        Name = leader.Name,
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
            GroupInvite invite = invites.Values.SingleOrDefault(inv => inv.InvitedCharacterName.Equals(inviteeName));
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
            GetPlayerByCharacterId(invite.InvitedCharacterId, out Player targetPlayer); // presumable to decline the invite the player has to be online. Expire is handled seperatly.
            GetPlayerByCharacterId(invite.InvitedCharacterId, out Player leader);
            
            switch (invite.Type)
            {
                case GroupInviteType.Invite:
                    if (leader != null)
                        GroupHandler.SendGroupResult(leader.Session, GroupResult.Declined, Id, invite.InvitedCharacterName);

                    break;

                case GroupInviteType.Request:
                    targetPlayer.Session.EnqueueMessageEncrypted(new ServerGroupRequestJoinResult
                    {
                        GroupId = Id,
                        IsJoin = false,
                        Name = leader.Name, //TODO: What if the Leader is offline?
                        Result = GroupResult.Declined
                    });
                    //GroupHandler.SendGroupResult(invite.TargetPlayer.Session, GroupResult.Declined, Id, Leader.Player.Name); //TODO: Does this need to be implemented?
                    break;

                case GroupInviteType.Referral:
                    //TODO: Does this need to be implemented?
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

            GetPlayerByCharacterId(Leader.CharacterId, out Player leader);
            if (leader == null)
                return; //TODO: What if the leader is Offline? Who can we refer to? Should we refer to a raid assist or just noone and can we replay this invite once the leader comes back online?

            leader.Session.EnqueueMessageEncrypted(
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

            // Remnove the pending invite so it doesnt get re-sent when somone comes online.
            invites.Remove(invite.InviteId);
            if (!GetPlayerByCharacterId(invite.InvitedCharacterId, out Player targetPlayer)) 
                return;

            // If i cant get the player here, there is no player object to remove the invite from.
            targetPlayer.GroupInvite = null;
        }

        private void SendInvite(GroupInvite invite)
        {
            if (!GetPlayerByCharacterId(invite.InvitedCharacterId, out Player targetPlayer))
                return;

            targetPlayer.Session.EnqueueMessageEncrypted(new ServerGroupInviteReceived
            {
                GroupId = Id,
                InviterIndex = invite.Inviter.GroupIndex,
                LeaderIndex = Leader.GroupIndex,
                Members = BuildGroupMembers()
            });
        }
        #endregion

        /// <summary>
        /// Check if a <see cref="GroupMember"/> can join the <see cref="Group"/>
        /// </summary>
        public bool CanJoinGroup(out GroupResult result)
        {
            // Member count is over the max group member count.
            if (members.Count >= MaxGroupSize)
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
                positionUpdateTimer.Reset();

                foreach (var member in members)
                {
                    if (!this.GetPlayerByCharacterId(member.CharacterId, out Player player))
                        continue;

                    ServerGroupJoin groupJoinPacket = new ServerGroupJoin
                    {
                        TargetPlayer        = new TargetPlayerIdentity
                        {
                            CharacterId     = player.CharacterId,
                            RealmId         = WorldServer.RealmId
                        },
                        GroupInfo           = Build()
                    };

                    player.Session.EnqueueMessageEncrypted(groupJoinPacket);
                    foreach(GroupMember member2 in members)
                        player.Session.EnqueueMessageEncrypted(member2.BuildGroupStatUpdate());
                }
            }
            else
            {
                if (!GetPlayerByCharacterId(addedMember.CharacterId, out Player addedPlayer))
                    return;

                addedPlayer.Session.EnqueueMessageEncrypted(new ServerGroupJoin
                {
                    TargetPlayer        = new TargetPlayerIdentity
                    {
                        CharacterId     = addedPlayer.CharacterId,
                        RealmId         = WorldServer.RealmId
                    },
                    GroupInfo           = Build()
                });

                BroadcastPacket(new ServerGroupMemberAdd
                {
                    GroupId = Id,
                    AddedMemberInfo = addedMember.BuildMemberInfo()
                });

                foreach (GroupMember member2 in members)
                    addedPlayer.Session.EnqueueMessageEncrypted(member2.BuildGroupStatUpdate());
            }
        }
         
        /// <summary>
        /// Kick a <see cref="GroupMember"/> from the <see cref="Group"/>.
        /// </summary>
        public void KickMember(TargetPlayerIdentity target)
        {
            if (members.Count == 2) {
                Disband();
                return;
            }

            GroupMember kickedMember = FindMember(target);
            if (kickedMember == null)
                return;

            if (kickedMember.IsPartyLeader)
                return;

            GetPlayerByCharacterId(kickedMember.CharacterId, out Player kickedPlayer); 
            members.Remove(kickedMember);
            membershipsByCharacterID.Remove(kickedMember.CharacterId);
            

            // Tell the player they are no longer in a group.
            if (kickedPlayer != null)
            {
                kickedPlayer.GroupMember = null;
                kickedPlayer.Session.EnqueueMessageEncrypted(new ServerGroupLeave
                {
                    GroupId = Id,
                    Reason = RemoveReason.Kicked
                });
            }
             
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
            if (!this.members.Contains(memberToRemove))
                return;

            GetPlayerByCharacterId(memberToRemove.CharacterId, out Player removedPlayer);
            members.Remove(memberToRemove);
            membershipsByCharacterID.Remove(memberToRemove.CharacterId);

            if (removedPlayer != null) { 
                removedPlayer.GroupMember = null;
                removedPlayer.Session.EnqueueMessageEncrypted(new ServerGroupLeave
                {
                    GroupId = Id,
                    Reason = RemoveReason.Left
                });            
            }

            BroadcastPacket(new ServerGroupRemove
            {
                GroupId = Id,
                Reason = RemoveReason.Left,
                TargetPlayer = new TargetPlayerIdentity()
                {
                    CharacterId = memberToRemove.CharacterId,
                    RealmId = WorldServer.RealmId
                }
            });
        }

        /// <summary>
        /// Disbands and removes the group from the <see cref="GroupManager"/>
        /// </summary>
        public void Disband()
        {
            foreach (GroupMember member in members)
            {
                if (!GetPlayerByCharacterId(member.CharacterId, out Player player))
                    continue;

                player.GroupMember = null;
            }

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
            foreach (GroupMember member in members)
            {
                member.PrepareForReadyCheck();
                BroadcastPacket(new ServerGroupMemberFlagsChanged
                {
                    GroupId = Id,
                    ChangedFlags = member.Flags,
                    IsFromPromotion = false,
                    MemberIndex = member.GroupIndex,
                    TargetedPlayer = new TargetPlayerIdentity() { CharacterId = member.CharacterId, RealmId = WorldServer.RealmId },
                }); 
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
      
            foreach (GroupMember groupMember in members) {
                if (groupMember.CharacterId == target.CharacterId)
                    break; 
            }
              
            member.SetFlags(changedFlag, addPermission); 
            BroadcastPacket(new ServerGroupMemberFlagsChanged
            {
                GroupId = Id,
                ChangedFlags = member.Flags,
                IsFromPromotion = false,
                MemberIndex = member.GroupIndex,
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
            if (!GetPlayerByCharacterId(Leader.CharacterId, out Player leader))
                return; //TODO: What if the Leader is offline? presumable nothing, invites should be resent when the leader logs back in?

            leader.Session.EnqueueMessageEncrypted(
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
        /// Updates the Groups Loot Rules.
        /// </summary>
        public void UpdateLootRules(LootRule lootRulesUnderThreshold, LootRule lootRulesThresholdAndOver, LootThreshold lootThreshold, HarvestLootRule harvestLootRule)
        {
            lootRule = lootRulesUnderThreshold;
            lootRuleThreshold = lootRulesThresholdAndOver;
            this.lootThreshold = lootThreshold;
            lootRuleHarvest = harvestLootRule;

            BroadcastPacket(new ServerGroupLootRulesChange
            {
                GroupId = Id,
                UnknownDWord = 0, // maybe characterId?
                LootRulesUnderThreshold = lootRulesUnderThreshold,
                LootRulesThresholdAndOver = lootRulesThresholdAndOver,
                LootThreshold = lootThreshold,
                HarvestLootRule = harvestLootRule, 
            });
        }

        /// <summary>
        /// Marks the specified unitId with the <see cref="GroupMarker"/>.
        /// </summary>
        public void MarkUnit(uint unitId, GroupMarker marker)
        {
            markerInfo.MarkTarget(unitId, marker);
        }

        /// <summary>
        /// Promotes a <see cref="GroupMember"/> to be the new leader of the group.
        /// </summary>
        /// <param name="newLeader"></param>
        public void Promote(TargetPlayerIdentity newLeader)
        {
            GroupMember memberToPromote = Leader; 
            foreach(GroupMember member in members)
            {
                memberToPromote = member; 
                if (member.CharacterId == newLeader.CharacterId)
                    break; 
            }
            Leader = memberToPromote;

            BroadcastPacket(new ServerGroupPromote
            {
                GroupId = Id,
                LeaderIndex = Leader.GroupIndex,
                NewLeader = newLeader
            });
        }

        /// <summary>
        /// Find a <see cref="GroupMember"/> with the provided <see cref="TargetPlayerIdentity"/>
        /// </summary>
        public GroupMember FindMember(TargetPlayerIdentity target)
        {
            if (!membershipsByCharacterID.ContainsKey(target.CharacterId))
                return null;

            return membershipsByCharacterID[target.CharacterId];
        }

        /// <summary>
        /// Gets the Index of The target <see cref="GroupMember"/>.
        /// </summary>
        public uint GetMemberIndex(GroupMember groupMember)
        {
            if (groupMember.Group.Id != this.Id)
                throw new InvalidOperationException("Member does not belong to this group.");

            return (uint)this.members.IndexOf(groupMember);
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
                    CharacterId     = Leader.CharacterId,
                    RealmId         = WorldServer.RealmId
                },
                LootRule            = lootRule,
                LootRuleThreshold   = lootRuleThreshold,
                LootRuleHarvest     = lootRuleHarvest,
                LootThreshold       = lootThreshold,
                MaxGroupSize        = MaxGroupSize,
                MemberInfos         = BuildMembersInfo(),
                RealmId             = WorldServer.RealmId,
                MarkerInfo          = markerInfo.Build()
            };
        }


        /// <summary>
        /// Gets an instance of a <see cref="ICharacter"/> representing the player.
        /// </summary> 
        private void GetCharacterInfoByCharacterId(ulong characterId, out ICharacter targetPlayer)
        {
            ICharacter character = CharacterManager.Instance.GetCharacterInfo(characterId);
            targetPlayer = character;
        }

        /// <summary>
        /// Gets the <see cref="Player"/> object representing the character Id if the player is online.
        /// </summary>>
        /// <returns>A <see cref="Player"/> object if the player is online, null otherwise.</returns>
        private bool GetPlayerByCharacterId(ulong characterId, out Player targetPlayer)
        {
            GetCharacterInfoByCharacterId(characterId, out ICharacter character);
            if (character is Player player) {
                targetPlayer = player;
                return true;
            }

            targetPlayer = null;
            return false;
        }

        private void UpdatePositions()
        { 
            foreach (var member in members)
            {
                if (!GetPlayerByCharacterId(member.CharacterId, out Player player))
                    continue;

                if (member.ZoneId != player.Zone.Id)
                {
                    member.ZoneId = (ushort)player.Zone.Id;
                    members.ForEach(m =>
                    {
                       if (!GetPlayerByCharacterId(m.CharacterId, out Player p))
                           return;

                        p.Session.EnqueueMessageEncrypted(new ServerGroupUpdatePlayerRealm
                        {
                            GroupId = Id,
                            TargetPlayerIdentity = new TargetPlayerIdentity() { CharacterId = m.CharacterId, RealmId = WorldServer.RealmId },
                            MapId = p.Map.Entry.Id,
                            RealmId = WorldServer.RealmId,
                            PhaseId = 1,
                            IsSyncdToGroup = true,
                            ZoneId = member.ZoneId
                        });
                    });
                }
            }

            var updates = new Dictionary<ushort, ServerGroupPositionUpdate>();
            foreach (var member in members)
            {
                if (!GetPlayerByCharacterId(member.CharacterId, out Player player))
                    continue;

                if (!updates.TryGetValue(member.ZoneId, out ServerGroupPositionUpdate update))
                {
                    update = new ServerGroupPositionUpdate
                    {
                       GroupId = Id,
                       Updates = new List<ServerGroupPositionUpdate.UnknownStruct0>(),
                       ZoneId = member.ZoneId
                    };
                    updates.Add(member.ZoneId, update);
                }
                 
                var entry = new ServerGroupPositionUpdate.UnknownStruct0
                {
                    Identity = new TargetPlayerIdentity() { CharacterId = member.CharacterId, RealmId = WorldServer.RealmId },
                    Flags = 3,
                    Position = new Position(player.Position),
                    Unknown0 = 1
                };
                update.Updates.Add(entry);
            }

            foreach (var item in updates)
            {
                members.ForEach(m => {
                    if (!GetPlayerByCharacterId(m.CharacterId, out Player p))
                        return;

                    p.Session.EnqueueMessageEncrypted(item.Value);
                });
            }  
        } 
    }
}
