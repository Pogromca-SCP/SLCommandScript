using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of number range iterables.
/// </summary>
public static class RangesProvider
{
    /// <summary>
    /// Retrieves iterable object for specific range.
    /// </summary>
    /// <param name="start">First number to include.</param>
    /// <param name="end">Last number to include.</param>
    /// <returns>Iterable object for specific range.</returns>
    public static IIterable StandardRange(int start, int end) =>
        start == end ? new SingleItemIterable<int>(start, LoadVariables) : new ListIterable<int>(GetRange(start, end), LoadVariables);

    /// <summary>
    /// Loads number value and inserts it into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert value into.</param>
    /// <param name="player">Number value to load.</param>
    /// <exception cref="NullReferenceException">When provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string> targetVars, int number) => targetVars["i"] = number.ToString();

    /// <summary>
    /// Creates numbers range.
    /// </summary>
    /// <param name="start">First number to include.</param>
    /// <param name="end">Last number to include.</param>
    /// <returns>Array with specified numbers range.</returns>
    public static int[] GetRange(int start, int end)
    {
        var desc = end < start;
        var range = new int[(desc ? start - end : end - start) + 1];
        var value = start;

        for (var i = 0; i < range.Length; ++i)
        {
            range[i] = value;
            value = desc ? value - 1 : value + 1;
        }

        return range;
    }
}
