using FacilityZone = MapGeneration.FacilityZone;
using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of room iterables.
/// </summary>
public static class RoomIterablesProvider
{
    /// <summary>
    /// Retrieves all named rooms.
    /// </summary>
    private static IEnumerable<FacilityRoom> NamedRooms => Facility.Rooms.Where(r => r.Identifier.Name != RoomName.Unnamed);

    /// <summary>
    /// Retrieves iterable object for all rooms.
    /// </summary>
    /// <returns>Iterable object for all rooms.</returns>
    public static IIterable AllRooms() => new ListIterable<FacilityRoom>(() => NamedRooms, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all LCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all LCZ rooms.</returns>
    public static IIterable AllLightRooms() => new ListIterable<FacilityRoom>(() =>
        NamedRooms.Where(r => r.Zone.ZoneType == FacilityZone.LightContainment), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all HCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all HCZ rooms.</returns>
    public static IIterable AllHeavyRooms() => new ListIterable<FacilityRoom>(() =>
        NamedRooms.Where(r => r.Zone.ZoneType == FacilityZone.HeavyContainment), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all EZ rooms.
    /// </summary>
    /// <returns>Iterable object for all EZ rooms.</returns>
    public static IIterable AllEntranceRooms() => new ListIterable<FacilityRoom>(() =>
        NamedRooms.Where(r => r.Zone.ZoneType == FacilityZone.Entrance), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all surface rooms.
    /// </summary>
    /// <returns>Iterable object for all surface rooms.</returns>
    public static IIterable AllSurfaceRooms() => new ListIterable<FacilityRoom>(() =>
        NamedRooms.Where(r => r.Zone.ZoneType == FacilityZone.Surface), LoadVariables);

    /// <summary>
    /// Loads properties from room object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="room">Room to load properties from.</param>
    /// <exception cref="NullReferenceException">When provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string> targetVars, FacilityRoom room)
    {
        targetVars["id"] = room.Identifier.Name.ToString("D");
        targetVars["name"] = room.Identifier.Name.ToString();
        targetVars["zone"] = room.Zone.ZoneType.ToString();
    }
}
