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
        private Dictionary<GroupMarker, uint> UsedGroupMarkers;

        public Group Group { get; private set; }
         
        public GroupMarkerInfo(Group group)
        {
            Group = group;
            UsedGroupMarkers = new Dictionary<GroupMarker, uint>();
        }

        public void MarkTarget(uint unitId, GroupMarker marker)
        {
            // client send a UnitId of 0 when it wants to "unmark" somone who is marked. 
            if (unitId > 0)
            {
                Unmark(marker);
                UsedGroupMarkers.Add(marker, unitId);
            }
            else
            {
                UsedGroupMarkers.Remove(marker);
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
        private void Unmark(GroupMarker marker)
        {
            if (!UsedGroupMarkers.ContainsKey(marker))
                return;

            UsedGroupMarkers.Remove(marker); 
        }

        public Network.Message.Model.Shared.GroupMarkerInfo Build()
        {
            return new Network.Message.Model.Shared.GroupMarkerInfo
            {
                Markers = UsedGroupMarkers.Select(kvp => new MarkerInfo() { Marker = kvp.Key, UnitId = kvp.Value }).ToArray()
            };
        }
    }
}
