using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a single item.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
public class SingleItemIterable<TItem> : IIterable
{
    /// <inheritdoc />
    public bool IsAtEnd { get; private set; }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Randomize() => Reset();

    /// <inheritdoc />
    public void Randomize(int amount) => Reset();

    /// <inheritdoc />
    public void Randomize(float amount) => Reset();

    /// <inheritdoc />
    public void Randomize(IterableSettings settings) => Reset();

    /// <inheritdoc />
    public void Reset() => IsAtEnd = Count == 0;
}
