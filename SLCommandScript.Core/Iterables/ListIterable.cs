using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
/// <param name="items">Objects to insert into wrapped list.</param>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public class ListIterable<TItem>(IEnumerable<TItem> items, Action<IDictionary<string, string>, TItem> mapper) : ListIterableBase<TItem>(mapper)
{
    /// <inheritdoc />
    protected override IEnumerable<TItem> Items => _items;

    /// <summary>
    /// Original iterated objects collection.
    /// </summary>
    private readonly IEnumerable<TItem> _items = items;
}
