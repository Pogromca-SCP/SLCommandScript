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
    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => _source is null || _current >= Count;

    /// <summary>
    /// Current amount of elements.
    /// </summary>
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

        _enumerator.MoveNext();

        if (_mapper is not null && targetVars is not null)
        {
            _mapper(targetVars, _enumerator.Current);
        }

        ++_current;
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
    public void Reset()
    {
        _enumerator = _source?.GetEnumerator();
        _current = 0;
    }
}
