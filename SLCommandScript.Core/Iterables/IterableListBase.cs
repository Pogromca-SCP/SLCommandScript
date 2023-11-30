using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects.
/// </summary>
/// <typeparam name="T">Type of contained objects.</typeparam>
/// <param name="source">Source of objects to insert into wrapped list.</param>
public abstract class IterableListBase<T>(Func<IEnumerable<T>> source) : IIterable
{
    /// <summary>
    /// Randomizes provided enumerable collection.
    /// </summary>
    /// <param name="data">Collection to randomize.</param>
    /// <returns>Randomized elements.</returns>
    private static T[] Randomize(IEnumerable<T> data)
    {
        var array = data.ToArray();

        if (array.Length < 2)
        {
            return array;
        }

        var rand = new Random();

        for (var i = array.Length - 1; i > 0; --i)
        {
            var key = rand.Next(i + 1);
            (array[key], array[i]) = (array[i], array[key]);
        }

        return array;
    }

    /// <summary>
    /// Randomizes provided enumerable collection.
    /// </summary>
    /// <param name="data">Collection to randomize.</param>
    /// <param name="amount">Amount of randomized elements to retrieve.</param>
    /// <returns>Randomized elements.</returns>
    private static T[] Randomize(IEnumerable<T> data, int amount)
    {
        var original = data.ToArray();

        if (original.Length < 2)
        {
            return original;
        }

        var rand = new Random();
        var result = new T[original.Length > amount ? amount : original.Length];
        amount = 0;

        for (var i = original.Length - 1; i > 0 && amount < result.Length; --i)
        {
            var key = rand.Next(i + 1);
            result[amount] = original[key];
            original[key] = original[i];
            ++amount;
        }

        if (amount < result.Length)
        {
            result[amount] = original[0];
        }

        return result;
    }

    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
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
                _objects = _source()?.Where(o => o is not null) ?? Array.Empty<T>();

                if (_count != 0)
                {
                    _objects = _count > 0 ? Randomize(_objects, _count) : Randomize(_objects);
                }

                _enumerator = _objects.GetEnumerator();
                _count = _objects.Count();
            }

            return _current >= _count;
        }
    }

    /// <summary>
    /// Source of iterated objects.
    /// </summary>
    private readonly Func<IEnumerable<T>> _source = source;

    /// <summary>
    /// Contains wrapped list of objects.
    /// </summary>
    private IEnumerable<T> _objects = null;

    /// <summary>
    /// Contains currently used iterator.
    /// </summary>
    private IEnumerator<T> _enumerator = null;

    /// <summary>
    /// Amount of contained elements. Used for randomization limit before objects initialization.
    /// </summary>
    private int _count = 0;

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

        if (targetVars is not null)
        {
            LoadVariables(targetVars, _enumerator.Current);
        }

        ++_current;
        return true;
    }

    /// <summary>
    /// Randomizes contained elements.
    /// </summary>
    public void Randomize() => Randomize(-1);

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Amount of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(int amount)
    {
        _objects = null;
        _enumerator = null;
        _count = amount;
        _current = 0;
    }

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    public void Reset()
    {
        _enumerator = _objects?.GetEnumerator();
        _current = 0;
    }

    /// <summary>
    /// Loads properties from current object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="obj">Object to load properties from.</param>
    protected abstract void LoadVariables(IDictionary<string, string> targetVars, T obj);
}
