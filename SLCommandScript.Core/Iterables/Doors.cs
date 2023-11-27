using PluginAPI.Core.Doors;
using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of doors.
/// </summary>
/// <param name="door">Source of doors to wrap.</param>
public class DoorsIterable(Func<IEnumerable<FacilityDoor>> door) : IterableListBase<FacilityDoor>(door)
{
    /// <summary>
    /// Loads properties from current door and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="door">Door to load properties from.</param>
    protected override void LoadVariables(IDictionary<string, string> targetVars, FacilityDoor door)
    {
        targetVars["name"] = door.Name;
        targetVars["id"] = door.OriginalObject.NetworkDoorId.ToString();
        targetVars["lock"] = door.LockReason.ToString();
        targetVars["room"] = door.Room.Identifier.Name.ToString();
    }
}

/// <summary>
/// Provides multiple sources of door iterables.
/// </summary>
public static class DoorIterablesProvider
{
    /// <summary>
    /// Retrieves iterable object for all doors.
    /// </summary>
    /// <returns>Iterable object for all doors.</returns>
    public static IIterable AllDoors() => new DoorsIterable(() => FacilityDoor.List);

    /// <summary>
    /// Retrieves iterable object for all breakable doors.
    /// </summary>
    /// <returns>Iterable object for all breakable doors.</returns>
    public static IIterable AllBreakableDoors() => new DoorsIterable(() => FacilityBreakableDoor.List);

    /// <summary>
    /// Retrieves iterable object for all gates.
    /// </summary>
    /// <returns>Iterable object for all gates.</returns>
    public static IIterable AllGates() => new DoorsIterable(() => FacilityGate.List);

    /// <summary>
    /// Retrieves iterable object for all locked doors.
    /// </summary>
    /// <returns>Iterable object for all locked doors.</returns>
    public static IIterable AllLockedDoors() => new DoorsIterable(() => FacilityDoor.List.Where(d => d.IsLocked));

    /// <summary>
    /// Retrieves iterable object for all unlocked doors.
    /// </summary>
    /// <returns>Iterable object for all unlocked doors.</returns>
    public static IIterable AllUnlockedDoors() => new DoorsIterable(() => FacilityDoor.List.Where(d => !d.IsLocked));

    /// <summary>
    /// Retrieves iterable object for all opened doors.
    /// </summary>
    /// <returns>Iterable object for all opened doors.</returns>
    public static IIterable AllOpenedDoors() => new DoorsIterable(() => FacilityDoor.List.Where(d => d.IsOpened));

    /// <summary>
    /// Retrieves iterable object for all closed doors.
    /// </summary>
    /// <returns>Iterable object for all closed doors.</returns>
    public static IIterable AllClosedDoors() => new DoorsIterable(() => FacilityDoor.List.Where(d => !d.IsOpened));
}
