using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        /// <summary>
        /// Send <see cref="ServerGroupInviteResult"/> to the current <see cref="WorldSession"/>
        /// </summary>
        public static void SendGroupResult(WorldSession session, GroupResult result, ulong groupId = 0, string targetPlayerName = "")
        {
            session.EnqueueMessageEncrypted(new ServerGroupInviteResult
            {
                GroupId = groupId,
                Name = targetPlayerName,
                Result = result
            });
        }

        /// <summary>
        /// Asserts that the GroupId recieved from the client is a <see cref="Group"/> the Player is a member of.
        /// </summary> 
        public static void AssertGroupId(WorldSession session, ulong recievedGroupId)
        {
            if (session.Player.GroupMember == null || session.Player.GroupMember.Group == null)
                throw new InvalidPacketValueException();

            // This will need updating - we may need to track the fact the player can belong to two groups.
            //  a "Current" and a "previous"
            ulong sessionGroupId = session.Player.GroupMember.Group.Id;
            if (sessionGroupId != recievedGroupId)
                throw new InvalidPacketValueException("Player does not belong to the group they wish to perform the action on.");
        }

        /// <summary>
        /// Asserts that the <see cref="Player"/> session group member can perform the requested action.
        /// </summary> 
        public static void AssertPermission(WorldSession session, GroupMemberInfoFlags action)
        {
            if (!session.Player.GroupMember.Flags.HasFlag(action))
                throw new InvalidPacketValueException("Player does not have the Group Role required to perform that action.");
        }

        /// <summary>
        /// Asserts that the <see cref="Player"/> session group member can perform the requested action.
        /// </summary> 
        public static void AssertGroupLeader(WorldSession session)
        {
            if (!session.Player.GroupMember.IsPartyLeader)
                throw new InvalidPacketValueException("Player must be the leader of the group to perform this action.");
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite groupInvite)
        {
            ICharacter character = CharacterManager.Instance.GetCharacterInfo(groupInvite.Name);
            if (!(character is Player targetedPlayer))
                return;

            // Check if inviter faction is same as invited faction.
            if (targetedPlayer.Faction1 != session.Player.Faction1)
            {
                SendGroupResult(session, GroupResult.WrongFaction, targetPlayerName: groupInvite.Name);
                return;
            }

            // Player is already being invited by another group/player
            if (targetedPlayer.GroupInvite != null)
            {
                SendGroupResult(session, GroupResult.Pending, targetPlayerName: groupInvite.Name);
                return;
            }

            // Check if the inviter is not inviting himself (pleb)
            if (targetedPlayer.Session == session)
            {
                SendGroupResult(session, GroupResult.NotInvitingSelf, targetPlayerName: groupInvite.Name);
                return;
            }

            Group group = GroupManager.Instance.GetGroupByLeader(session.Player) ?? GroupManager.Instance.CreateGroup(session.Player);
            if (!group.CanJoinGroup(out GroupResult result))
            {
                SendGroupResult(session, result, group.Id, groupInvite.Name);
                return;
            }

            group.Invite(session.Player, targetedPlayer);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupRequestJoin)]
        public static void HandleJoinGroupRequest(WorldSession session, ClientGroupRequestJoin joinRequest)
        {
            ICharacter character = CharacterManager.Instance.GetCharacterInfo(joinRequest.Name);
            if (!(character is Player targetedPlayer))
                return;

            Group group = GroupManager.Instance.GetGroupByLeader(targetedPlayer);
            if (group == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, targetPlayerName: joinRequest.Name);
                return;
            }
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse response)
        {
            Group joinedGroup = GroupManager.Instance.GetGroupById(response.GroupId);
            if (joinedGroup == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, response.GroupId, session.Player.Name);
                return;
            }

            WorldSession leaderSession = joinedGroup.Leader.Player.Session;
            if (leaderSession == null)
                return;

            // Check if the targeted player declined the group invite.
            if (response.Result == GroupInviteResult.Declined)
            {
                SendGroupResult(leaderSession, GroupResult.Declined, response.GroupId, session.Player.Name);

                joinedGroup.RevokeInvite(session.Player.GroupInvite);
                session.Player.GroupInvite = null;

                return;
            }

            // Check if the Player can join the group
            if (!joinedGroup.CanJoinGroup(out GroupResult result))
            {
                SendGroupResult(session, result, joinedGroup.Id, session.Player.Name);
                return;
            }

            var addedMember = joinedGroup.CreateMember(session.Player);
            if (addedMember == null)
                return;

            joinedGroup.RevokeInvite(session.Player.GroupInvite);
            joinedGroup.AddMember(addedMember);

            SendGroupResult(leaderSession, GroupResult.Accepted, response.GroupId, session.Player.Name);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupKick)]
        public static void HandleGroupKick(WorldSession session, ClientGroupKick kick)
        {
            AssertGroupId(session, kick.GroupId);
            AssertPermission(session, GroupMemberInfoFlags.CanKick);

            Group group = GroupManager.Instance.GetGroupById(kick.GroupId);
            if (group == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, kick.GroupId, session.Player.Name);
                return;
            }

            // I never want to leave a group with only 1 member; So as with the Leave if there would be 1 member left after this operation
            // Just .Disband() the group.
            if (group.Members.Count == 2)
                group.Disband();
            else
                group.KickMember(kick.TargetedPlayer);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave leave)
        {
            AssertGroupId(session, leave.GroupId);

            Group group = GroupManager.Instance.GetGroupById(leave.GroupId);
            if (group == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, leave.GroupId, session.Player.Name);
                return;
            }

            // I never want to leave a group with only 1 member; So as with the Kick if there would be 1 member left after this operation
            // Just .Disband() the group.
            if (leave.ShouldDisband || group.Members.Count == 2)
            {
                group.Disband();
                return;
            }

            group.RemoveMember(session.Player.GroupMember);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupMarkUnit)]
        public static void HandleGroupMarkUnit(WorldSession session, ClientGroupMark clientMark)
        {
            AssertPermission(session, GroupMemberInfoFlags.CanMark);

            ulong groupId = session.Player.GroupMember.Group.Id;
            Group group = GroupManager.Instance.GetGroupById(groupId);
            if (group == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, groupId, session.Player.Name);
                return;
            }

            // UnitID could be either a Group Member, or a Mob.
        }

        [MessageHandler(GameMessageOpcode.ClientGroupFlagsChanged)]
        public static void HandleGroupFlagsChanged(WorldSession session, ClientGroupFlagsChanged clientGroupFlagsChanged)
        {
            AssertGroupId(session, clientGroupFlagsChanged.GroupId);
            AssertGroupLeader(session);

            Group group = GroupManager.Instance.GetGroupById(clientGroupFlagsChanged.GroupId);
            if (group == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, clientGroupFlagsChanged.GroupId, session.Player.Name);
                return;
            }

            group.SetGroupFlags(clientGroupFlagsChanged.NewFlags);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void ClientSetRole(WorldSession session, ClientGroupSetRole clientGroupSetRole)
        {
            AssertGroupId(session, clientGroupSetRole.GroupId);

            Group group = GroupManager.Instance.GetGroupById(clientGroupSetRole.GroupId);
            if (group == null)
            {
                SendGroupResult(session, GroupResult.GroupNotFound, clientGroupSetRole.GroupId, session.Player.Name);
                return;
            }

            if (group.IsRaid && !session.Player.GroupMember.IsPartyLeader) {
                AssertPermission(session, GroupMemberInfoFlags.RaidAssistant);
            }
            else {
                AssertGroupLeader(session);
            }
            group.UpdateMemberRole(clientGroupSetRole.TargetedPlayer, clientGroupSetRole.ChangedFlag, clientGroupSetRole.CurrentFlags.HasFlag(clientGroupSetRole.ChangedFlag));
        }
    }
}
