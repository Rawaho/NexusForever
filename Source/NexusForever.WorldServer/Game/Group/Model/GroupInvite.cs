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
        public ulong InvitedCharacterId { get; }
        public string InvitedCharacterName { get; }
        public GroupMember Inviter { get; }
        public GroupInviteType Type { get; }
        public double ExpirationTime { get; set; } = InviteTimeout;

        /// <summary>
        /// Creates an instance of <see cref="GroupInvite"/>
        /// </summary>
        public GroupInvite(ulong id, Group group, ulong invitedCharacterId, string invitedCharacterName, GroupMember inviter, GroupInviteType type)
        {
            InviteId        = id;
            Group           = group;
            InvitedCharacterId = invitedCharacterId;
            InvitedCharacterName = invitedCharacterName;
            Inviter         = inviter;
            Type            = type;
        }
    }
}
