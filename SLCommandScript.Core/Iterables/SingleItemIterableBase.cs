using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Base class for single item iterables.
/// </summary>
/// <typeparam name="TItem">Type of contained object.</typeparam>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public abstract class SingleItemIterableBase<TItem>(Action<IDictionary<string, string>, TItem> mapper) : IIterable
{
    /// <inheritdoc />
    public bool IsAtEnd { get; private set; } = false;

    /// <inheritdoc />
    public int Count => 1;

    /// <summary>
    /// Retrieves contained item.
    /// </summary>
    protected abstract TItem Item { get; }

    /// <summary>
    /// Variable mapper used to load variables.
    /// </summary>
    private readonly Action<IDictionary<string, string>, TItem> _mapper = mapper;

    /// <inheritdoc />
    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (IsAtEnd)
        {
            return false;
        }

        _mapper(targetVars, Item);
        IsAtEnd = true;
        return true;
    }

    /// <inheritdoc />
    public void Reload() => Reset();

    /// <inheritdoc />
    public void Randomize() => Reset();

    /// <inheritdoc />
    public void Randomize(int amount) => Reset();

    /// <inheritdoc />
    public void Randomize(float amount) => Reset();

    /// <inheritdoc />
    public void Randomize(IterableSettings settings) => Reset();

    /// <inheritdoc />
    public void Reset() => IsAtEnd = false;
}
