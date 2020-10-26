using System.Collections.Generic;
using System.Linq;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Game.Group.Model
{
    public class GroupMarkerInfo: IBuildable<Network.Message.Model.Shared.GroupMarkerInfo>
    {
        private Dictionary<uint, GroupMarker> GroupMarkedTargets;
        private Dictionary<GroupMarker, uint> UsedGroupMarkers;

        public Group Group { get; private set; }
         
        public GroupMarkerInfo(Group group)
        {
            Group = group;
            GroupMarkedTargets = new Dictionary<uint, GroupMarker>();
            UsedGroupMarkers = new Dictionary<GroupMarker, uint>();
        }

        public void MarkTarget(uint unitId, GroupMarker marker)
        { 
            Unmark(unitId);
            Unmark(marker);

            // client send a UnitId of 0 when it wants to "unmark" somone who is marked. 
            if (unitId > 0)
            {
                GroupMarkedTargets.Add(unitId, marker);
                UsedGroupMarkers.Add(marker, unitId);
            }

            BroadcastMarkEvent(unitId, marker);
        }

        private void BroadcastMarkEvent(uint unitID, GroupMarker marker)
        {
            Group.BroadcastPacket(new ServerGroupMarkUnit
            {
                GroupId = Group.Id,
                Marker = marker,
                UnitId = unitID
            });
        }

        private void Unmark(uint unitId)
        {
            if (!GroupMarkedTargets.ContainsKey(unitId))
                return;

            GroupMarkedTargets.TryGetValue(unitId, out GroupMarker marker);
            GroupMarkedTargets.Remove(unitId);
            UsedGroupMarkers.Remove(marker);
            BroadcastMarkEvent(0, marker);
        }
        private void Unmark(GroupMarker marker)
        {
            if (!UsedGroupMarkers.ContainsKey(marker))
                return;

            UsedGroupMarkers.TryGetValue(marker, out uint unitId);
            UsedGroupMarkers.Remove(marker);
            GroupMarkedTargets.Remove(unitId);
            BroadcastMarkEvent(0, marker);
        }

        public Network.Message.Model.Shared.GroupMarkerInfo Build()
        {
            return new Network.Message.Model.Shared.GroupMarkerInfo
            {
                Markers = GroupMarkedTargets.Select(kvp => new MarkerInfo() { UnitId = kvp.Key, Marker = kvp.Value }).ToArray()
            };
        }
    }
}
