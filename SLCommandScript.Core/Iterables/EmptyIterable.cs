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

    /// <inheritdoc />
    public bool IsAtEnd => true;

    /// <inheritdoc />
    public int Count => 0;

    /// <inheritdoc />
    public bool LoadNext(IDictionary<string, string?>? targetVars) => false;

    /// <inheritdoc />
    public void Randomize() {}

    /// <inheritdoc />
    public void Randomize(int amount) {}

    /// <inheritdoc />
    public void Randomize(float amount) {}

    /// <inheritdoc />
    public void Randomize(IterableSettings settings) {}

    /// <inheritdoc />
    public void Reset() {}
}
