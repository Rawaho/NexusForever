using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var group = new Group(NextGroupId(), player);
            groups.Add(group.Id, group);
            groupOwner.Add(player.CharacterId, group);
            return group;
        }

        /// <summary>
        /// Get the current <see cref="Group"/> from the supplied <see cref="Player"/>
        /// </summary>
        public Group GetGroup(Player player)
        {
            if (!groupOwner.TryGetValue(player.CharacterId, out var group))
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
