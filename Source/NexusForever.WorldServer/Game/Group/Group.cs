using NexusForever.Shared;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Handler;
using NexusForever.WorldServer.Network.Message.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NetworkGroupMember = NexusForever.WorldServer.Network.Message.Model.Shared.GroupMember;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group : IUpdate
    {
        private ConcurrentQueue<GroupInvite> invites = new ConcurrentQueue<GroupInvite>();
        private Dictionary<ulong, GroupMember> members = new Dictionary<ulong, GroupMember>();

        public ulong Id { get; }
        public GroupMember Leader { get; }

        public Group(ulong id, Player leader)
        {
            Id     = id;
            Leader = CreateMember(leader);
        }

        /// <summary>
        /// Invite the targeted <see cref="Player"/>
        /// </summary>
        public void Invite(Player inviter, Player invitedPlayer)
        {
            GroupHandler.SendGroupResult(inviter.Session, GroupResult.Sent, Id, invitedPlayer.Name);
            CreateInvite(inviter.GroupMember, invitedPlayer, GroupInviteType.Invite);
        }

        /// <summary>
        /// Create and add a new <see cref="GroupMember"/>
        /// to the <see cref="Group"/>
        /// </summary>
        private GroupMember CreateMember(Player player)
        {
            var member = new GroupMember(NextMemberId(), this, player);
            members.Add(member.Id, member);
            player.GroupMember = member;
            return member;
        }

        public void Update(double lastTick)
        {
            while (invites.TryDequeue(out var groupInvite))
                groupInvite.SendInvite();
        }

        /// <summary>
        /// Get the next available MemberId
        /// </summary>
        public ulong NextMemberId()
        {
            if (members.Count > 0)
                return members.Last().Key + 1UL;
            else
                return 1;
        }

        /// <summary>
        /// Builds all <see cref="GroupMember"/>s into <see cref="NetworkGroupMember"/>s.
        /// </summary>
        public List<NetworkGroupMember> BuildGroupMembers()
        {
            var memberList = new List<NetworkGroupMember>();
            foreach (var member in members.Values)
                memberList.Add(member.BuildGroupMember());

            return memberList;
        }

        /// <summary>
        /// Create a new <see cref="GroupInvite"/>
        /// </summary>
        public GroupInvite CreateInvite(GroupMember inviter, Player invitedPlayer, GroupInviteType type)
        {
            var invite = new GroupInvite(this, invitedPlayer, inviter, type);
            invites.Enqueue(invite);

            invitedPlayer.GroupInvite = invite;
            return invite;
        }

        /// <summary>
        /// Broadcast <see cref="IWritable"/> to all <see cref="GroupMember"/>
        /// in the <see cref="Group"/>
        /// </summary>
        public void BroadcastPacket(IWritable message)
        {
            foreach (var member in members.Values)
                member.Player.Session.EnqueueMessageEncrypted(message);
        }
    }
}
