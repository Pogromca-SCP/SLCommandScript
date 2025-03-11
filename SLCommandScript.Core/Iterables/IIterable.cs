using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Interface to implement in order to create a custom iterable object wrapper.
/// </summary>
public interface IIterable
{
    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    bool IsAtEnd { get; }

    /// <summary>
    /// Current amount of elements.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Performs next iteration step and loads new property values into provided dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <returns><see langword="true" /> if the iteration can continue, <see langword="false" /> otherwise.</returns>
    bool LoadNext(IDictionary<string, string> targetVars);

    /// <summary>
    /// Randomizes contained elements.
    /// </summary>
    void Randomize();

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Amount of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    void Randomize(int amount);

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Percentage of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    void Randomize(float amount);

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="settings">Settings to use for randomization, negative values disable the limit, zero disables randomization.</param>
    void Randomize(IterableSettings settings);

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    void Reset();
}
