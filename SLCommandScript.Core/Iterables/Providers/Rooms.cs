using LabApi.Features.Wrappers;
using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using FacilityZone = MapGeneration.FacilityZone;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of room iterables.
/// </summary>
public static class RoomIterablesProvider
{
    /// <summary>
    /// Retrieves all named rooms.
    /// </summary>
    private static IEnumerable<Room> NamedRooms => Room.List.Where(r => r.Name != RoomName.Unnamed);

    /// <summary>
    /// Retrieves iterable object for all rooms.
    /// </summary>
    /// <returns>Iterable object for all rooms.</returns>
    public static IIterable AllRooms() => new ListIterable<Room>(() => NamedRooms, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all LCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all LCZ rooms.</returns>
    public static IIterable AllLightRooms() => new ListIterable<Room>(() => NamedRooms.Where(r => r.Zone == FacilityZone.LightContainment), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all HCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all HCZ rooms.</returns>
    public static IIterable AllHeavyRooms() => new ListIterable<Room>(() => NamedRooms.Where(r => r.Zone == FacilityZone.HeavyContainment), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all EZ rooms.
    /// </summary>
    /// <returns>Iterable object for all EZ rooms.</returns>
    public static IIterable AllEntranceRooms() => new ListIterable<Room>(() => NamedRooms.Where(r => r.Zone == FacilityZone.Entrance), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all surface rooms.
    /// </summary>
    /// <returns>Iterable object for all surface rooms.</returns>
    public static IIterable AllSurfaceRooms() => new ListIterable<Room>(() => NamedRooms.Where(r => r.Zone == FacilityZone.Surface), LoadVariables);

    /// <summary>
    /// Loads properties from room object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="room">Room to load properties from.</param>
    /// <exception cref="NullReferenceException">When <paramref name="targetVars" /> or provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string?> targetVars, Room room)
    {
        targetVars["name"] = room.Name.ToString();
        targetVars["zone"] = room.Zone.ToString();
        targetVars["shape"] = room.Shape.ToString();
    }
}
