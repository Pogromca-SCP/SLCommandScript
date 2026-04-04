using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Base class for list iterables.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public abstract class ListIterableBase<TItem>(Action<IDictionary<string, string>, TItem> mapper) : IIterable
{
    /// <inheritdoc />
#pragma warning disable CS0436
    [MemberNotNullWhen(false, nameof(_enumerator))]
#pragma warning restore CS0436
    public bool IsAtEnd
    {
        get
        {
            if (_objects is null)
            {
                _objects = Items;

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
    public int Count { get; private set; } = -1;

    /// <summary>
    /// Retrieves contained items.
    /// </summary>
    protected abstract IEnumerable<TItem> Items { get; }

    /// <summary>
    /// Variable mapper used to load variables.
    /// </summary>
    private readonly Action<IDictionary<string, string>, TItem> _mapper = mapper;

    /// <summary>
    /// Contains wrapped list of objects.
    /// </summary>
    private IEnumerable<TItem>? _objects = null;

    /// <summary>
    /// Contains currently used iterator.
    /// </summary>
    private IEnumerator<TItem>? _enumerator = null;

    /// <summary>
    /// Random settings used for randomization.
    /// </summary>
    private IterableSettings _randomSettings = new();

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
        _mapper(targetVars, _enumerator.Current);
        ++_current;
        return true;
    }

    /// <inheritdoc />
    public void Reload()
    {
        Count = -1;
        _objects = null;
        _enumerator = null;
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
        Reload();
        _randomSettings = settings;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _enumerator = _objects?.GetEnumerator();
        _current = 0;
    }
}
