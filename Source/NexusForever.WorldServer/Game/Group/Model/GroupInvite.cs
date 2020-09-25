using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;

namespace NexusForever.WorldServer.Game.Group.Model
{
    public class GroupInvite
    {
        public const double InviteTimeout = 30d;

        public Group Group { get; set; }
        public Player TargetPlayer { get; set; }
        public GroupMember Inviter { get; set; }
        public GroupInviteType Type { get; set; }
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddSeconds(InviteTimeout);

        public GroupInvite(Group group, Player player, GroupMember inviter, GroupInviteType type)
        {
            Group           = group;
            TargetPlayer    = player;
            Inviter         = inviter;
            Type            = type;
        }

        /// <summary>
        /// Send the invite to the invited <see cref="TargetPlayer"/>
        /// </summary>
        public void SendInvite()
        {
            TargetPlayer.Session.EnqueueMessageEncrypted(new ServerGroupInviteReceived
            {
                GroupId = Group.Id,
                Members = Group.BuildGroupMembers()
            });
        }
    }
}
