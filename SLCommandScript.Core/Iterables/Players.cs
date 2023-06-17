using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;
using PluginAPI.Core;
using System.Linq;
using PlayerRoles;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for list of players.
/// </summary>
public class PlayerIterable : IIterable
{
    /// <summary>
    /// Contains wrapped list of players.
    /// </summary>
    private readonly List<Player> _players;

    /// <summary>
    /// Contains index of current player.
    /// </summary>
    private int _current;

    /// <summary>
    /// Creates new iterable wrapper for players list.
    /// </summary>
    /// <param name="players">List of players to wrap.</param>
    public PlayerIterable(IEnumerable<Player> players)
    {
        _players = new();

        if (players is not null)
        {
            _players.AddRange(players.Where(p => p is not null));
        }

        Reset();
    }

    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => _current >= _players.Count;

    /// <summary>
    /// Performs next iteration step and loads new property values into provided dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <returns><see langword="true" /> if the iteration can continue, <see langword="false" /> otherwise.</returns>
    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (IsAtEnd)
        {
            return false;
        }

        if (targetVars is not null)
        {
            var player = _players[_current];
            targetVars["name"] = player.DisplayNickname;
            targetVars["id"] = player.PlayerId.ToString();
            targetVars["team"] = player.Team.ToString();
            targetVars["role"] = player.RoleName;
            targetVars["roleid"] = player.Role.ToString();
        }

        ++_current;
        return true;
    }

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    public void Reset()
    {
        _current = 0;
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
    public static IIterable AllPlayers() => new PlayerIterable(Player.GetPlayers());

    /// <summary>
    /// Retrieves iterable object for all class d personnel.
    /// </summary>
    /// <returns>Iterable object for all class d personnel.</returns>
    public static IIterable AllClassDs() => new PlayerIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.ClassD));

    /// <summary>
    /// Retrieves iterable object for all scientists.
    /// </summary>
    /// <returns>Iterable object for all scientists.</returns>
    public static IIterable AllScientists() => new PlayerIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.Scientist));

    /// <summary>
    /// Retrieves iterable object for all MTFs.
    /// </summary>
    /// <returns>Iterable object for all MTFs.</returns>
    public static IIterable AllMTFs() => new PlayerIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.NtfPrivate
        || p.Role == RoleTypeId.NtfSergeant || p.Role == RoleTypeId.NtfSpecialist || p.Role == RoleTypeId.NtfCaptain));

    /// <summary>
    /// Retrieves iterable object for all chaos insurgents.
    /// </summary>
    /// <returns>Iterable object for all chaos insurgents.</returns>
    public static IIterable AllChaos() => new PlayerIterable(Player.GetPlayers().Where(p => p.Role == RoleTypeId.ChaosConscript
        || p.Role == RoleTypeId.ChaosRifleman || p.Role == RoleTypeId.ChaosRepressor || p.Role == RoleTypeId.ChaosMarauder));

    /// <summary>
    /// Retrieves iterable object for all SCPs.
    /// </summary>
    /// <returns>Iterable object for all SCPs.</returns>
    public static IIterable AllSCPs() => new PlayerIterable(Player.GetPlayers().Where(p => p.IsSCP));

    /// <summary>
    /// Retrieves iterable object for all humans.
    /// </summary>
    /// <returns>Iterable object for all humans.</returns>
    public static IIterable AllHumans() => new PlayerIterable(Player.GetPlayers().Where(p => p.IsHuman));
}
