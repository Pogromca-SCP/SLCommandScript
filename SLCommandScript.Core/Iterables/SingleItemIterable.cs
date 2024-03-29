using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a single item.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
public class SingleItemIterable<TItem> : IIterable
{
    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd { get; private set; }

    /// <summary>
    /// Current amount of elements.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Source of iterated object.
    /// </summary>
    private readonly Func<TItem> _source;

    /// <summary>
    /// Variable mapper used for loading variables.
    /// </summary>
    private readonly Action<IDictionary<string, string>, TItem> _mapper;

    /// <summary>
    /// Currently stored item.
    /// </summary>
    private TItem _item;

    /// <summary>
    /// Creates new lazy loaded item iterable.
    /// </summary>
    /// <param name="source">Source of iterated object.</param>
    /// <param name="mapper">Variable mapper to use to load variables.</param>
    public SingleItemIterable(Func<TItem> source, Action<IDictionary<string, string>, TItem> mapper)
    {
        IsAtEnd = source is null;
        Count = source is null ? 0 : 1;
        _source = source;
        _mapper = mapper;
        _item = default;
    }

    /// <summary>
    /// Creates new predefined item iterable.
    /// </summary>
    /// <param name="item">Iterated object.</param>
    /// <param name="mapper">Variable mapper to use to load variables.</param>
    public SingleItemIterable(TItem item, Action<IDictionary<string, string>, TItem> mapper)
    {
        IsAtEnd = false;
        Count = 1;
        _source = null;
        _mapper = mapper;
        _item = item;
    }

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

        if (_source is not null)
        {
            _item = _source();
        }

        if (_mapper is not null && targetVars is not null)
        {
            _mapper(targetVars, _item);
        }

        IsAtEnd = true;
        return true;
    }

    /// <summary>
    /// Randomizes contained elements.
    /// </summary>
    public void Randomize() => Reset();

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Amount of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(int amount) => Reset();

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Percentage of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(float amount) => Reset();

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="settings">Settings to use for randomization, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(RandomSettings settings) => Reset();

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    public void Reset() => IsAtEnd = Count == 0;
}
