using PlayerRoles;
using SLCommandScript.Core.Iterables.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Provides additional utilities for iterables.
/// </summary>
public static class IterablesUtils
{
    #region Providers
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

        // Plugins
        { "plugin", PluginIterablesProvider.AllPlugins },

        // Enums
        { "roleid", EnumIterable<RoleTypeId>.Get },
        { "itemid", EnumIterable<ItemType>.Get }
    };
    #endregion

    /// <summary>
    /// Used for random numbers generation.
    /// </summary>
    private static readonly Random _random = new();

    /// <summary>
    /// Shuffles elements in provided enumerable collection.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in collection.</typeparam>
    /// <param name="data">Collection to shuffle.</param>
    /// <returns>Shuffled array if at least 2 elements were found.</returns>
    public static TItem[] Shuffle<TItem>(IEnumerable<TItem> data) => Shuffle(data?.ToArray());

    /// <summary>
    /// Shuffles and retrieves specific amount of elements from provided collection.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in collection.</typeparam>
    /// <param name="data">Collection to shuffle.</param>
    /// <param name="amount">Amount of elements to retrieve. Takes effect only when smaller than elements count.</param>
    /// <returns>Shuffled array if at least 2 elements were found.</returns>
    public static TItem[] Shuffle<TItem>(IEnumerable<TItem> data, int amount) => Shuffle(data?.ToArray(), amount);

    /// <summary>
    /// Shuffles and retrieves specific amount of elements from provided collection.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in collection.</typeparam>
    /// <param name="data">Collection to shuffle.</param>
    /// <param name="amount">Percent amount of elements to retrieve.</param>
    /// <returns>Shuffled array if at least 2 elements were found.</returns>
    public static TItem[] Shuffle<TItem>(IEnumerable<TItem> data, float amount) => Shuffle(data?.ToArray(), amount);

    /// <summary>
    /// Shuffles elements in provided array.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in array.</typeparam>
    /// <param name="array">Array to shuffle. This array is modified.</param>
    /// <returns>Shuffled original array if at least 2 elements were found.</returns>
    public static TItem[] Shuffle<TItem>(TItem[] array) => array is null || array.Length < 2 ? array : ShuffleArray(array);

    /// <summary>
    /// Shuffles and retrieves specific amount of elements from provided array.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in array.</typeparam>
    /// <param name="array">Array to shuffle. This array is modified.</param>
    /// <param name="amount">Amount of elements to retrieve. Takes effect only when smaller than array length.</param>
    /// <returns>New shuffled array or original array if less than 2 elements were found.</returns>
    public static TItem[] Shuffle<TItem>(TItem[] array, int amount)
    {
        if (amount < 1)
        {
            return [];
        }

        if (array is null || array.Length < 2)
        {
            return array;
        }

        if (amount >= array.Length)
        {
            return ShuffleArray(array);
        }

        var result = new TItem[amount];
        amount = 0;

        for (var i = array.Length - 1; amount < result.Length; --i)
        {
            var key = _random.Next(i + 1);
            result[amount] = array[key];
            array[key] = array[i];
            ++amount;
        }

        return result;
    }

    /// <summary>
    /// Shuffles and retrieves specific amount of elements from provided array.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in array.</typeparam>
    /// <param name="array">Array to shuffle. This array is modified.</param>
    /// <param name="amount">Percent amount of elements to retrieve.</param>
    /// <returns>New shuffled array or original array if less than 2 elements were found.</returns>
    public static TItem[] Shuffle<TItem>(TItem[] array, float amount) => Shuffle(array, array is null ? 1 : (int) (array.Length * amount));

    /// <summary>
    /// Shuffles elements in provided array.
    /// </summary>
    /// <typeparam name="TItem">Type of elements contained in array.</typeparam>
    /// <param name="array">Array to shuffle. This array is modified.</param>
    /// <returns>Shuffled original array.</returns>
    private static TItem[] ShuffleArray<TItem>(TItem[] array)
    {
        for (var i = array.Length - 1; i > 0; --i)
        {
            var key = _random.Next(i + 1);
            (array[key], array[i]) = (array[i], array[key]);
        }

        return array;
    }
}
