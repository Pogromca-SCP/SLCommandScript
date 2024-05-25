using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
/// <param name="source">Source of objects to insert into wrapped list.</param>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public class ListIterable<TItem>(Func<IEnumerable<TItem>> source, Action<IDictionary<string, string>, TItem> mapper) : IIterable
{
    /// <inheritdoc />
    public bool IsAtEnd
    {
        get
        {
            if (_source is null)
            {
                return true;
            }

            if (_objects is null)
            {
                _objects = _source() ?? [];

                if (!_randomSettings.IsEmpty)
                {
                    if (_randomSettings.IsPrecise)
                    {
                        _objects = _randomSettings.Amount > 0 ? IterablesUtils.Shuffle(_objects, _randomSettings.Amount) : IterablesUtils.Shuffle(_objects);
                    }
                    else
                    {
                        _objects = _randomSettings.Percent > 0.0f ? IterablesUtils.Shuffle(_objects, _randomSettings.Percent) : IterablesUtils.Shuffle(_objects);
                    }
                }

                _enumerator = _objects.GetEnumerator();
                Count = _objects.Count();
                _current = 0;
            }

            return _current >= Count;
        }
    }

    /// <inheritdoc />
    public int Count { get; private set; } = 0;

    /// <summary>
    /// Source of iterated objects.
    /// </summary>
    private readonly Func<IEnumerable<TItem>> _source = source;

    /// <summary>
    /// Variable mapper used for loading variables.
    /// </summary>
    private readonly Action<IDictionary<string, string>, TItem> _mapper = mapper;

    /// <summary>
    /// Contains wrapped list of objects.
    /// </summary>
    private IEnumerable<TItem> _objects = null;

    /// <summary>
    /// Contains currently used iterator.
    /// </summary>
    private IEnumerator<TItem> _enumerator = null;

    /// <summary>
    /// Random settings used for randomization.
    /// </summary>
    private RandomSettings _randomSettings = new();

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
    public void Randomize() => Randomize(new RandomSettings(-1));

    /// <inheritdoc />
    public void Randomize(int amount) => Randomize(new RandomSettings(amount));

    /// <inheritdoc />
    public void Randomize(float amount) => Randomize(new RandomSettings(amount));

    /// <inheritdoc />
    public void Randomize(RandomSettings settings)
    {
        Count = 0;
        _objects = null;
        _enumerator = null;
        _randomSettings = settings;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _enumerator = _objects?.GetEnumerator();
        _current = 0;
    }
}
