using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Empty iterable object.
/// </summary>
public class EmptyIterable : IIterable
{
    /// <summary>
    /// Contains a reference to global empty iterable object.
    /// </summary>
    public static EmptyIterable Instance { get; } = new();

    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => true;

    /// <summary>
    /// Current amount of elements.
    /// </summary>
    public int Count => 0;

    /// <summary>
    /// Performs next iteration step and loads new property values into provided dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <returns><see langword="true" /> if the iteration can continue, <see langword="false" /> otherwise.</returns>
    public bool LoadNext(IDictionary<string, string> targetVars) => false;

    /// <summary>
    /// Randomizes contained elements.
    /// </summary>
    public void Randomize() {}

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Amount of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(int amount) {}

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Percentage of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(float amount) {}

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="settings">Settings to use for randomization, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(RandomSettings settings) {}

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    public void Reset() {}
}
