using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
public class ListIterable<TItem> : IIterable
{
    /// <inheritdoc />
    public bool IsAtEnd
    {
        get
        {
            if (_source is null && _items is null)
            {
                return true;
            }

            if (_objects is null)
            {
                _objects = (_source is null ? _items : _source()) ?? [];

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
    public int Count { get; private set; }

    /// <summary>
    /// Source of iterated objects.
    /// </summary>
    private readonly Func<IEnumerable<TItem>> _source;

    /// <summary>
    /// Original iterated objects collection.
    /// </summary>
    private readonly IEnumerable<TItem> _items;

    /// <summary>
    /// Variable mapper used for loading variables.
    /// </summary>
    private readonly Action<IDictionary<string, string>, TItem> _mapper;

    /// <summary>
    /// Contains wrapped list of objects.
    /// </summary>
    private IEnumerable<TItem> _objects;

    /// <summary>
    /// Contains currently used iterator.
    /// </summary>
    private IEnumerator<TItem> _enumerator;

    /// <summary>
    /// Random settings used for randomization.
    /// </summary>
    private IterableSettings _randomSettings;

    /// <summary>
    /// Contains index of current object.
    /// </summary>
    private int _current;

    /// <summary>
    /// Creates new lazy loaded list iterable.
    /// </summary>
    /// <param name="source">Source of objects to insert into wrapped list.</param>
    /// <param name="mapper">Variable mapper to use to load variables.</param>
    public ListIterable(Func<IEnumerable<TItem>> source, Action<IDictionary<string, string>, TItem> mapper)
    {
        Count = 0;
        _source = source;
        _items = null;
        _mapper = mapper;
        _objects = null;
        _enumerator = null;
        _randomSettings = new();
        _current = 0;
    }

    /// <summary>
    /// Creates new predefined list iterable.
    /// </summary>
    /// <param name="item">Objects to insert into wrapped list.</param>
    /// <param name="mapper">Variable mapper to use to load variables.</param>
    public ListIterable(IEnumerable<TItem> items, Action<IDictionary<string, string>, TItem> mapper)
    {
        Count = 0;
        _source = null;
        _items = items;
        _mapper = mapper;
        _objects = null;
        _enumerator = null;
        _randomSettings = new();
        _current = 0;
    }

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
    public void Randomize() => Randomize(new IterableSettings(-1));

    /// <inheritdoc />
    public void Randomize(int amount) => Randomize(new IterableSettings(amount));

    /// <inheritdoc />
    public void Randomize(float amount) => Randomize(new IterableSettings(amount));

    /// <inheritdoc />
    public void Randomize(IterableSettings settings)
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
