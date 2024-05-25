using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects which is loaded on initialization. Randomization is not supported.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
/// <param name="source">Source of objects to insert into wrapped list.</param>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public class PredefinedIterable<TItem>(IEnumerable<TItem> source, Action<IDictionary<string, string>, TItem> mapper) : IIterable
{
    /// <inheritdoc />
    public bool IsAtEnd => _source is null || _current >= Count;

    /// <inheritdoc />
    public int Count { get; } = source?.Count() ?? 0;

    /// <summary>
    /// Contains wrapped list of objects.
    /// </summary>
    private readonly IEnumerable<TItem> _source = source;

    /// <summary>
    /// Variable mapper used for loading variables.
    /// </summary>
    private readonly Action<IDictionary<string, string>, TItem> _mapper = mapper;

    /// <summary>
    /// Contains currently used iterator.
    /// </summary>
    private IEnumerator<TItem> _enumerator = source?.GetEnumerator();

    /// <summary>
    /// Contains index of current object.
    /// </summary>
    private int _current = 0;

    /// <inheritdoc />
    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (IsAtEnd)
        {
            return false;
        }

        _enumerator.MoveNext();

        if (_mapper is not null && targetVars is not null)
        {
            _mapper(targetVars, _enumerator.Current);
        }

        ++_current;
        return true;
    }

    /// <inheritdoc />
    public void Randomize() => Reset();

    /// <inheritdoc />
    public void Randomize(int amount) => Reset();

    /// <inheritdoc />
    public void Randomize(float amount) => Reset();

    /// <inheritdoc />
    public void Randomize(RandomSettings settings) => Reset();

    /// <inheritdoc />
    public void Reset()
    {
        _enumerator = _source?.GetEnumerator();
        _current = 0;
    }
}
