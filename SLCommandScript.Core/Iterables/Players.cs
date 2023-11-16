using PlayerRoles;
using PluginAPI.Core;
using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of players.
/// </summary>
/// <param name="players">List of players to wrap.</param>
public class PlayersIterable(IEnumerable<Player> players) : IterableListBase<Player>(players)
{
    /// <summary>
    /// Loads properties from current player and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="player">Player to load properties from.</param>
    protected override void LoadVariables(IDictionary<string, string> targetVars, Player player)
    {
        targetVars["name"] = player.DisplayNickname;
        targetVars["id"] = player.PlayerId.ToString();
        targetVars["team"] = player.Team.ToString();
        targetVars["role"] = player.RoleName;
        targetVars["roleid"] = ((sbyte) player.Role).ToString();
    }
}

/// <summary>
/// Provides multiple sources of player iterables.
/// </summary>
public static class PlayerIterablesProvider
{
    /// <summary>
    /// Retrieves iterable object for all players.
    /// </summary>
    /// <returns>Iterable object for all players.</returns>
    public static IIterable AllPlayers() => new PlayersIterable(Player.GetPlayers());

    /// <summary>
    /// Retrieves iterable object for all class d personnel.
    /// </summary>
    /// <returns>Iterable object for all class d personnel.</returns>
    public static IIterable AllClassDs() => FilteredPlayers(p => p.Team == Team.ClassD);

    /// <summary>
    /// Retrieves iterable object for all scientists.
    /// </summary>
    /// <returns>Iterable object for all scientists.</returns>
    public static IIterable AllScientists() => FilteredPlayers(p => p.Team == Team.Scientists);

    /// <summary>
    /// Retrieves iterable object for all MTFs.
    /// </summary>
    /// <returns>Iterable object for all MTFs.</returns>
    public static IIterable AllMTFs() => FilteredPlayers(p => p.IsNTF);

    /// <summary>
    /// Retrieves iterable object for all chaos insurgents.
    /// </summary>
    /// <returns>Iterable object for all chaos insurgents.</returns>
    public static IIterable AllChaos() => FilteredPlayers(p => p.IsChaos);

    /// <summary>
    /// Retrieves iterable object for all SCPs.
    /// </summary>
    /// <returns>Iterable object for all SCPs.</returns>
    public static IIterable AllSCPs() => FilteredPlayers(p => p.IsSCP);

    /// <summary>
    /// Retrieves iterable object for all humans.
    /// </summary>
    /// <returns>Iterable object for all humans.</returns>
    public static IIterable AllHumans() => FilteredPlayers(p => p.IsHuman);

    /// <summary>
    /// Retrieves iterable object for all tutorials.
    /// </summary>
    /// <returns>Iterable object for all tutorials.</returns>
    public static IIterable AllTutorials() => FilteredPlayers(p => p.IsTutorial);

    /// <summary>
    /// Retrieves iterable object for all spectators.
    /// </summary>
    /// <returns>Iterable object for all spectators.</returns>
    public static IIterable AllSpectators() => FilteredPlayers(p => p.Role == RoleTypeId.Spectator);

    /// <summary>
    /// Retrieves iterable object for all alive players.
    /// </summary>
    /// <returns>Iterable object for all alive players.</returns>
    public static IIterable AllAlive() => FilteredPlayers(p => p.IsAlive);

    /// <summary>
    /// Retrieves iterable object for all disarmed humans.
    /// </summary>
    /// <returns>Iterable object for all disarmed humans.</returns>
    public static IIterable AllDisarmed() => FilteredPlayers(p => p.IsDisarmed);

    /// <summary>
    /// Retrieves iterable object for filtered players.
    /// </summary>
    /// <param name="filter">Filter to apply.</param>
    /// <returns>Iterable object for filtered players.</returns>
    private static IIterable FilteredPlayers(Func<Player, bool> filter) => new PlayersIterable(Player.GetPlayers().Where(filter));
}
