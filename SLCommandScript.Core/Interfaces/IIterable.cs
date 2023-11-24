using System.Collections.Generic;

namespace SLCommandScript.Core.Interfaces;

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
    /// Performs next iteration step and loads new property values into provided dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <returns><see langword="true" /> if the iteration can continue, <see langword="false" /> otherwise.</returns>
    bool LoadNext(IDictionary<string, string> targetVars);

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Amount of random elements to select from iterable object, zero or negative value will disable randomization.</param>
    void Randomize(int amount);

    /// <summary>
    /// Resets iteration process and disables randomization.
    /// </summary>
    void Reset();
}
