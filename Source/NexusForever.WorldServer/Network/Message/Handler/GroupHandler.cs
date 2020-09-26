using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;

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

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite groupInvite)
        {
            WorldSession targetSession = session.GetSessionByName(groupInvite.Name);
            if (targetSession == null)
            {
                SendGroupResult(session, GroupResult.PlayerNotFound, targetPlayerName: groupInvite.Name);
                return;
            }

            // Check if inviter faction is same as invited faction.
            if (targetSession.Player.Faction1 != session.Player.Faction1)
            {
                SendGroupResult(session, GroupResult.WrongFaction, targetPlayerName: groupInvite.Name);
                return;
            }

            // Player is already being invited by another group/player
            if (targetSession.Player.GroupInvite != null)
            {
                SendGroupResult(session, GroupResult.Pending, targetPlayerName: groupInvite.Name);
                return;
            }

            // Check if the inviter is not inviting himself (pleb)
            if (targetSession == session)
            {
                SendGroupResult(session, GroupResult.NotInvitingSelf, targetPlayerName: groupInvite.Name);
                return;
            }

            Group group = GroupManager.Instance.GetGroupByLeader(session.Player) ?? GroupManager.Instance.CreateGroup(session.Player);
            group.Invite(session.Player, targetSession.Player);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupRequestJoin)]
        public static void HandleJoinGroupRequest(WorldSession session, ClientGroupRequestJoin joinRequest)
        {
            var targetSession = session.GetSessionByName(joinRequest.Name);
            if (targetSession == null)
            {
                SendGroupResult(session, GroupResult.PlayerNotFound, targetPlayerName: joinRequest.Name);
                return;
            }

            Group group = GroupManager.Instance.GetGroupByLeader(targetSession.Player);
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
            // If this happens something is REALLY wrong
            if (leaderSession == null)
                return;

            // Check if the targeted player declined the group invite.
            if (response.Result == GroupInviteResult.Declined)
            {
                SendGroupResult(leaderSession, GroupResult.Declined, response.GroupId, session.Player.Name);
                return;
            }

            Game.Group.Model.GroupMember addedMember = joinedGroup.CreateMember(session.Player);
            if (addedMember == null)
                return;

            ServerGroupJoin groupJoinPacket = new ServerGroupJoin
            {
                JoinedPlayer = new TargetPlayerIdentity
                {
                    CharacterId = session.Player.CharacterId,
                    RealmId = WorldServer.RealmId
                },
                GroupInfo = joinedGroup.Build()
            };

            // Broadcast the packet to the group
            joinedGroup.BroadcastPacket(groupJoinPacket);
            joinedGroup.RevokeInvite(session.Player.GroupInvite);

            // For now send GroupResult.Accepted
            SendGroupResult(leaderSession, GroupResult.Accepted, response.GroupId, session.Player.Name);
        }
    }
}
