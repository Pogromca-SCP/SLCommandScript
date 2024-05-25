using PluginAPI.Core.Doors;
using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of door iterables.
/// </summary>
public static class DoorIterablesProvider
{
    /// <summary>
    /// Retrieves iterable object for all doors.
    /// </summary>
    /// <returns>Iterable object for all doors.</returns>
    public static IIterable AllDoors() => new ListIterable<FacilityDoor>(() => FacilityDoor.List, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all breakable doors.
    /// </summary>
    /// <returns>Iterable object for all breakable doors.</returns>
    public static IIterable AllBreakableDoors() => new ListIterable<FacilityDoor>(() => FacilityBreakableDoor.List, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all gates.
    /// </summary>
    /// <returns>Iterable object for all gates.</returns>
    public static IIterable AllGates() => new ListIterable<FacilityDoor>(() => FacilityGate.List, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all locked doors.
    /// </summary>
    /// <returns>Iterable object for all locked doors.</returns>
    public static IIterable AllLockedDoors() => new ListIterable<FacilityDoor>(() => FacilityDoor.List.Where(d => d.IsLocked), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all unlocked doors.
    /// </summary>
    /// <returns>Iterable object for all unlocked doors.</returns>
    public static IIterable AllUnlockedDoors() => new ListIterable<FacilityDoor>(() => FacilityDoor.List.Where(d => !d.IsLocked), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all opened doors.
    /// </summary>
    /// <returns>Iterable object for all opened doors.</returns>
    public static IIterable AllOpenedDoors() => new ListIterable<FacilityDoor>(() => FacilityDoor.List.Where(d => d.IsOpened), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all closed doors.
    /// </summary>
    /// <returns>Iterable object for all closed doors.</returns>
    public static IIterable AllClosedDoors() => new ListIterable<FacilityDoor>(() => FacilityDoor.List.Where(d => !d.IsOpened), LoadVariables);

    /// <summary>
    /// Loads properties from door object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="player">Door to load properties from.</param>
    /// <exception cref="NullReferenceException">When provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string> targetVars, FacilityDoor door)
    {
        targetVars["name"] = door.Name;
        targetVars["id"] = door.OriginalObject.NetworkDoorId.ToString();
        targetVars["lock"] = door.LockReason.ToString();
        targetVars["room"] = door.Room.Identifier.Name.ToString();
    }
}
