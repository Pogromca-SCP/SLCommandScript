using PluginAPI.Core;
using System.Collections.Generic;
using SLCommandScript.Core.Interfaces;
using System.Linq;
using PlayerRoles;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of players.
/// </summary>
public class PlayersIterable : IterableListBase<Player>
{
    /// <summary>
    /// Creates new iterable wrapper for players list.
    /// </summary>
    /// <param name="players">List of players to wrap.</param>
    public PlayersIterable(IEnumerable<Player> players) : base(players) {}

    /// <summary>
    /// Loads properties from current player and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="obj">Player to load properties from.</param>
    protected override void LoadVariables(IDictionary<string, string> targetVars, Player player)
    {
        targetVars["name"] = player.DisplayNickname;
        targetVars["id"] = player.PlayerId.ToString();
        targetVars["team"] = player.Team.ToString();
        targetVars["role"] = player.RoleName;
        targetVars["roleid"] = player.Role.ToString();
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
    public static IIterable AllClassDs() => new PlayersIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.ClassD));

    /// <summary>
    /// Retrieves iterable object for all scientists.
    /// </summary>
    /// <returns>Iterable object for all scientists.</returns>
    public static IIterable AllScientists() => new PlayersIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.Scientist));

    /// <summary>
    /// Retrieves iterable object for all MTFs.
    /// </summary>
    /// <returns>Iterable object for all MTFs.</returns>
    public static IIterable AllMTFs() => new PlayersIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.NtfPrivate
        || p.Role == RoleTypeId.NtfSergeant || p.Role == RoleTypeId.NtfSpecialist || p.Role == RoleTypeId.NtfCaptain));

    /// <summary>
    /// Retrieves iterable object for all chaos insurgents.
    /// </summary>
    /// <returns>Iterable object for all chaos insurgents.</returns>
    public static IIterable AllChaos() => new PlayersIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.ChaosConscript
        || p.Role == RoleTypeId.ChaosRifleman || p.Role == RoleTypeId.ChaosRepressor || p.Role == RoleTypeId.ChaosMarauder));

    /// <summary>
    /// Retrieves iterable object for all SCPs.
    /// </summary>
    /// <returns>Iterable object for all SCPs.</returns>
    public static IIterable AllSCPs() => new PlayersIterable(Player.GetPlayers().Where(p => p.IsSCP));

    /// <summary>
    /// Retrieves iterable object for all humans.
    /// </summary>
    /// <returns>Iterable object for all humans.</returns>
    public static IIterable AllHumans() => new PlayersIterable(Player.GetPlayers().Where(p => p.IsHuman));
}
