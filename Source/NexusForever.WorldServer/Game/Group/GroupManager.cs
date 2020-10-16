using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
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
            Group group = new Group(NextGroupId(), player);

            groups.Add(group.Id, group);

            // Player is already leader in a group
            if (groupOwner.ContainsKey(player.CharacterId))
                return null;

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
            groupOwner.Remove(group.Leader.Player.CharacterId);
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
