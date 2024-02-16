using PlayerRoles;
using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Provides additional utilities for iterables.
/// </summary>
public static class IterablesUtils
{
    /// <summary>
    /// Contains iterable objects providers.
    /// </summary>
    public static Dictionary<string, Func<IIterable>> Providers { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        // Players
        { "player", PlayerIterablesProvider.AllPlayers },
        { "classd", PlayerIterablesProvider.AllClassDs },
        { "scientist", PlayerIterablesProvider.AllScientists },
        { "mtf", PlayerIterablesProvider.AllMTFs },
        { "chaos", PlayerIterablesProvider.AllChaos },
        { "scp", PlayerIterablesProvider.AllSCPs },
        { "human", PlayerIterablesProvider.AllHumans },
        { "tutorial", PlayerIterablesProvider.AllTutorials },
        { "spectator", PlayerIterablesProvider.AllSpectators },
        { "alive_player", PlayerIterablesProvider.AllAlive },
        { "disarmed_player", PlayerIterablesProvider.AllDisarmed },

        // Rooms
        { "room", RoomIterablesProvider.AllRooms },
        { "lcz_room", RoomIterablesProvider.AllLightRooms },
        { "hcz_room", RoomIterablesProvider.AllHeavyRooms },
        { "ez_room", RoomIterablesProvider.AllEntranceRooms },
        { "surface_room", RoomIterablesProvider.AllSurfaceRooms },

        // Doors
        { "door", DoorIterablesProvider.AllDoors },
        { "breakable_door", DoorIterablesProvider.AllBreakableDoors },
        { "gate", DoorIterablesProvider.AllGates },
        { "locked_door", DoorIterablesProvider.AllLockedDoors },
        { "unlocked_door", DoorIterablesProvider.AllUnlockedDoors },
        { "opened_door", DoorIterablesProvider.AllOpenedDoors },
        { "closed_door", DoorIterablesProvider.AllClosedDoors },

        // Enums
        { "role", EnumIterable<RoleTypeId>.Get },
        { "item", EnumIterable<ItemType>.Get }
    };
}
