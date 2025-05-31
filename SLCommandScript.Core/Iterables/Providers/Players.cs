using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of player iterables.
/// </summary>
public static class PlayerIterablesProvider
{
    /// <summary>
    /// Retrieves all authenticated players.
    /// </summary>
    private static IEnumerable<Player> AuthPlayers => Player.GetAll(PlayerSearchFlags.AuthenticatedPlayers);

    /// <summary>
    /// Retrieves iterable object for all players.
    /// </summary>
    /// <returns>Iterable object for all players.</returns>
    public static IIterable AllPlayers() => new ListIterable<Player>(() => AuthPlayers, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all class d personnel.
    /// </summary>
    /// <returns>Iterable object for all class d personnel.</returns>
    public static IIterable AllClassDs() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.Team == Team.ClassD), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all scientists.
    /// </summary>
    /// <returns>Iterable object for all scientists.</returns>
    public static IIterable AllScientists() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.Team == Team.Scientists), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all MTFs.
    /// </summary>
    /// <returns>Iterable object for all MTFs.</returns>
    public static IIterable AllMTFs() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsNTF), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all chaos insurgents.
    /// </summary>
    /// <returns>Iterable object for all chaos insurgents.</returns>
    public static IIterable AllChaos() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsChaos), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all SCPs.
    /// </summary>
    /// <returns>Iterable object for all SCPs.</returns>
    public static IIterable AllSCPs() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsSCP), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all flamingos.
    /// </summary>
    /// <returns>Iterable object for all flamingos.</returns>
    public static IIterable AllFlamingos() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.Team == Team.Flamingos), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all humans.
    /// </summary>
    /// <returns>Iterable object for all humans.</returns>
    public static IIterable AllHumans() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsHuman), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all tutorials.
    /// </summary>
    /// <returns>Iterable object for all tutorials.</returns>
    public static IIterable AllTutorials() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsTutorial), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all spectators.
    /// </summary>
    /// <returns>Iterable object for all spectators.</returns>
    public static IIterable AllSpectators() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.Role == RoleTypeId.Spectator), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all alive players.
    /// </summary>
    /// <returns>Iterable object for all alive players.</returns>
    public static IIterable AllAlive() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsAlive), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all disarmed humans.
    /// </summary>
    /// <returns>Iterable object for all disarmed humans.</returns>
    public static IIterable AllDisarmed() => new ListIterable<Player>(() => AuthPlayers.Where(p => p.IsDisarmed), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all NPCs.
    /// </summary>
    /// <returns>Iterable object for all NPCs.</returns>
    public static IIterable AllNPCs() => new ListIterable<Player>(() => Player.GetAll(PlayerSearchFlags.RegularNpcs), LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all dummies.
    /// </summary>
    /// <returns>Iterable object for all dummies.</returns>
    public static IIterable AllDummies() => new ListIterable<Player>(() => Player.GetAll(PlayerSearchFlags.DummyNpcs), LoadVariables);

    /// <summary>
    /// Loads properties from player object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="player">Player to load properties from.</param>
    /// <exception cref="NullReferenceException">When <paramref name="targetVars" /> or provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string?> targetVars, Player player)
    {
        targetVars["name"] = player.DisplayName;
        targetVars["id"] = player.PlayerId.ToString();
        targetVars["team"] = player.Team.ToString();
        targetVars["role"] = player.Role.ToString();
        targetVars["roleid"] = player.Role.ToString("D");
    }
}
