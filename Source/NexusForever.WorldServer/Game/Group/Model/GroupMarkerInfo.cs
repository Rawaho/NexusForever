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
        private Dictionary<uint, GroupMarker> UnitIdUsedMarkers;

        public Group Group { get; private set; }
         
        public GroupMarkerInfo(Group group)
        {
            Group = group;
            UsedGroupMarkers = new Dictionary<GroupMarker, uint>();
            UnitIdUsedMarkers = new Dictionary<uint, GroupMarker>();
        }

        public void MarkTarget(uint unitId, GroupMarker marker)
        {
            // client send a UnitId of 0 when it wants to "unmark" somone who is marked. 
            if (unitId > 0)
            {
                Unmark(marker);
                Unmark(unitId);
                Mark(unitId, marker);          
            }
            else
            {
                Unmark(marker);
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
        private void Mark(uint unitId, GroupMarker marker)
        {
            UsedGroupMarkers.Add(marker, unitId);
            UnitIdUsedMarkers.Add(unitId, marker);

            BroadcastMarkEvent(unitId, marker);
        }
        
        private void Unmark(GroupMarker marker)
        {
            if (!UsedGroupMarkers.ContainsKey(marker))
                return;

            UsedGroupMarkers.TryGetValue(marker, out uint unitId);
            UsedGroupMarkers.Remove(marker);
            UnitIdUsedMarkers.Remove(unitId);

            BroadcastMarkEvent(0, marker);
        }
         
        private void Unmark(uint unitId)
        {
            if (!UnitIdUsedMarkers.ContainsKey(unitId))
                return;

            UnitIdUsedMarkers.TryGetValue(unitId, out GroupMarker marker);
            UsedGroupMarkers.Remove(marker);
            UnitIdUsedMarkers.Remove(unitId);

            BroadcastMarkEvent(0, marker);
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
