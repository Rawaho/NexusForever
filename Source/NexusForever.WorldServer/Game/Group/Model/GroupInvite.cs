using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Group.Model
{
    public class GroupInvite
    {
        public const double InviteTimeout = 30d;

        public ulong InviteId { get; }
        public Group Group { get;}
        public Player TargetPlayer { get; }
        public GroupMember Inviter { get; }
        public GroupInviteType Type { get; }
        public double ExpirationTime { get; set; } = InviteTimeout;

        /// <summary>
        /// Creates an instance of <see cref="GroupInvite"/>
        /// </summary>
        public GroupInvite(ulong id, Group group, Player player, GroupMember inviter, GroupInviteType type)
        {
            InviteId        = id;
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
