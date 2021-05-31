using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Model;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Game.Group
{
    public sealed class GroupManager : Singleton<GroupManager>, IUpdate
    {
        private Dictionary<ulong, Group> groups = new Dictionary<ulong, Group>();
        private Dictionary<ulong, Group> groupOwner = new Dictionary<ulong, Group>();

        private GroupManager() { }

        /// <summary>
        /// Create a <see cref="Group"/> for supplied <see cref="Player"/>
        /// </summary>
        public Group CreateGroup(Player player)
        {
            // Player is already leader in a group
            if (groupOwner.ContainsKey(player.CharacterId))
                return null;

            Group group = Group.CreateOpenWorld(NextGroupId(), player);
            groups.Add(group.Id, group);
            groupOwner.Add(player.CharacterId, group);

            return group;
        }

        /// <summary>
        /// Removes the Group from the Session.
        /// </summary>
        /// <param name="group">The group to remove.</param>
        public void RemoveGroup(Group group)
        {
            if (!groups.ContainsKey(group.Id))
                return;

            groups.Remove(group.Id);
            groupOwner.Remove(group.Leader.CharacterId);
        }

        /// <summary>
        /// Get the current <see cref="Group"/> from the supplied <see cref="Player"/>
        /// </summary>
        public Group GetGroupByLeader(Player player)
        {
            if (!groupOwner.TryGetValue(player.CharacterId, out var group))
                return null;

            return group;
        }

        /// <summary>
        /// Get a <see cref="Group"/> with the supplied <see cref="ulong"/> Group Id
        /// </summary>
        public Group GetGroupById(ulong groupId)
        {
            if (!groups.TryGetValue(groupId, out Group group))
                return null;

            return group;
        }

        public bool FindGroupMembershipsForPlayer(Player player, out Model.GroupMember membership1, out Model.GroupMember membership2)
        {
            membership1 = null;
            membership2 = null;

            foreach (Group group in groups.Values)
            {
                Model.GroupMember membership = group.FindMember(new TargetPlayerIdentity() { CharacterId = player.CharacterId, RealmId = WorldServer.RealmId });

                if (membership == null)
                    continue;
                
                if (!membership.Group.IsOpenWorld) // Is Instance Group.
                {
                    if (membership1 != null)
                        throw new System.InvalidOperationException("Error in restoring groups for player. Player cannot be part of two Instance groups.");

                    // If this is an instance group it MUST be group1.
                    membership1 = membership;
                }

                if (membership1 == null)
                {
                    membership1 = membership;
                    continue;
                }

                if (membership2 != null)
                    throw new System.InvalidOperationException("Error in restoring groups for player. Player cannot be part of more than two groups.");

                membership2 = membership;
            }

            return membership1 != null;
        }

        public void RestoreGroupsForPlayer(Player player)
        {
            if (!FindGroupMembershipsForPlayer(player, out Model.GroupMember membership, out Model.GroupMember membership2))
                return;
                                    
            // Add the player back to the other group IF they are a member of one first.
            if (membership2 != null)
            {
                player.AddToGroup(membership2);
                player.Session.EnqueueMessageEncrypted(new ServerGroupJoin
                {
                    TargetPlayer = new TargetPlayerIdentity
                    {
                        CharacterId = player.CharacterId,
                        RealmId = WorldServer.RealmId
                    },
                    GroupInfo = membership2.Group.Build()
                });
            }

            // Then add them back to the main group.
            player.AddToGroup(membership); 
            player.Session.EnqueueMessageEncrypted(new ServerGroupJoin
            {
                TargetPlayer = new TargetPlayerIdentity
                {
                    CharacterId = player.CharacterId,
                    RealmId = WorldServer.RealmId
                },
                GroupInfo = membership.Group.Build()
            });
        }

        public void Update(double lastTick)
        {
            foreach (var group in groups.Values)
                group.Update(lastTick);
        }

        /// <summary>
        /// Generate a new GroupId.
        /// </summary>
        private ulong NextGroupId()
        {
            if (groups.Count > 0)
                return groups.Last().Key + 1;
            else
                return 1;
        }
    }
}
