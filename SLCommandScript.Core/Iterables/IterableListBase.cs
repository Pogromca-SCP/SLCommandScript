using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects.
/// </summary>
/// <typeparam name="T">Type of contained objects.</typeparam>
public abstract class IterableListBase<T> : IIterable
{
    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => _current >= _count;

    /// <summary>
    /// Contains wrapped list of objects.
    /// </summary>
    private readonly IEnumerator<T> _objects;

    /// <summary>
    /// Amount of contained elements.
    /// </summary>
    private readonly int _count;

    /// <summary>
    /// Contains index of current object.
    /// </summary>
    private int _current;

    /// <summary>
    /// Creates new iterable wrapper for a list.
    /// </summary>
    /// <param name="objects">Objects to insert into wrapped list.</param>
    public IterableListBase(IEnumerable<T> objects)
    {
        var list = objects?.Where(o => o is not null);
        _objects = list?.GetEnumerator();
        _count = list?.Count() ?? 0;
        _current = 0;
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

        _objects.MoveNext();

        if (targetVars is not null)
        {
            LoadVariables(targetVars, _objects.Current);
        }

        ++_current;
        return true;
    }

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    public void Reset()
    {
        _objects.Reset();
        _current = 0;
    }

    /// <summary>
    /// Loads properties from current object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="obj">Object to load properties from.</param>
    protected abstract void LoadVariables(IDictionary<string, string> targetVars, T obj);
}
