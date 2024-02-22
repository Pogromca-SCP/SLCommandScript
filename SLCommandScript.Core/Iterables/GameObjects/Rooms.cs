﻿using FacilityZone = MapGeneration.FacilityZone;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables.GameObjects;

/// <summary>
/// Iterable wrapper for a list of rooms.
/// </summary>
/// <param name="rooms">Source of rooms to wrap.</param>
public class RoomsIterable(Func<IEnumerable<FacilityRoom>> rooms) : IterableListBase<FacilityRoom>(rooms)
{
    /// <summary>
    /// Loads properties from current room and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="room">Room to load properties from.</param>
    protected override void LoadVariables(IDictionary<string, string> targetVars, FacilityRoom room)
    {
        targetVars["id"] = room.Identifier.Name.ToString();
        targetVars["zone"] = room.Zone.ZoneType.ToString();
    }
}

/// <summary>
/// Provides multiple sources of room iterables.
/// </summary>
public static class RoomIterablesProvider
{
    /// <summary>
    /// Retrieves iterable object for all rooms.
    /// </summary>
    /// <returns>Iterable object for all rooms.</returns>
    public static IIterable AllRooms() => new RoomsIterable(() => Facility.Rooms);

    /// <summary>
    /// Retrieves iterable object for all LCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all LCZ rooms.</returns>
    public static IIterable AllLightRooms() => new RoomsIterable(() => Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.LightContainment));

    /// <summary>
    /// Retrieves iterable object for all HCZ rooms.
    /// </summary>
    /// <returns>Iterable object for all HCZ rooms.</returns>
    public static IIterable AllHeavyRooms() => new RoomsIterable(() => Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.HeavyContainment));

    /// <summary>
    /// Retrieves iterable object for all EZ rooms.
    /// </summary>
    /// <returns>Iterable object for all EZ rooms.</returns>
    public static IIterable AllEntranceRooms() => new RoomsIterable(() => Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.Entrance));

    /// <summary>
    /// Retrieves iterable object for all surface rooms.
    /// </summary>
    /// <returns>Iterable object for all surface rooms.</returns>
    public static IIterable AllSurfaceRooms() => new RoomsIterable(() => Facility.Rooms.Where(r => r.Zone.ZoneType == FacilityZone.Surface));
}