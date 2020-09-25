using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        /// <summary>
        /// Send <see cref="ServerGroupInviteResult"/> to the current <see cref="WorldSession"/>
        /// </summary>
        public static void SendGroupResult(WorldSession session, GroupResult result, ulong groupId, string targetPlayerName)
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
            var targetSession = session.GetSessionByName(groupInvite.Name);
            if (targetSession == null)
            {
                SendGroupResult(session, GroupResult.PlayerNotFound, 0, groupInvite.Name);
                return;
            }

            // Check if inviter faction is same as invited faction.
            if (targetSession.Player.Faction1 != session.Player.Faction1)
            {
                SendGroupResult(session, GroupResult.WrongFaction, 0, groupInvite.Name);
                return;
            }

            // Check if the inviter is not inviting himself (pleb)
            if (targetSession == session)
            {
                SendGroupResult(session, GroupResult.NotInvitingSelf, 0, groupInvite.Name);
                return;
            }

            var group = GroupManager.Instance.GetGroup(session.Player) ?? GroupManager.Instance.CreateGroup(session.Player);
            group.Invite(session.Player, targetSession.Player);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse response)
        {
            Console.WriteLine();
        }
    }
}
