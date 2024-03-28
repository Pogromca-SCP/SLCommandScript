using FacilityZone = MapGeneration.FacilityZone;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of room iterables.
/// </summary>
public static class RoomIterablesProvider
{
    /// <summary>
    /// Retrieves iterable object for all rooms.
    /// </summary>
    /// <returns>Iterable object for all rooms.</returns>
    public static IIterable AllRooms() => new IterableList<FacilityRoom>(() => Facility.Rooms, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all LCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all LCZ rooms.</returns>
    public static IIterable AllLightRooms() => new IterableList<FacilityRoom>(() =>
        Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.LightContainment), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all HCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all HCZ rooms.</returns>
    public static IIterable AllHeavyRooms() => new IterableList<FacilityRoom>(() =>
        Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.HeavyContainment), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all EZ rooms.
    /// </summary>
    /// <returns>Iterable object for all EZ rooms.</returns>
    public static IIterable AllEntranceRooms() => new IterableList<FacilityRoom>(() =>
        Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.Entrance), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all surface rooms.
    /// </summary>
    /// <returns>Iterable object for all surface rooms.</returns>
    public static IIterable AllSurfaceRooms() => new IterableList<FacilityRoom>(() =>
        Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.Surface), LoadVariables);

    /// <summary>
    /// Loads properties from room object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="room">Room to load properties from.</param>
    private static void LoadVariables(IDictionary<string, string> targetVars, FacilityRoom room)
    {
        targetVars["id"] = room.Identifier.Name.ToString();
        targetVars["zone"] = room.Zone.ZoneType.ToString();
    }
}
