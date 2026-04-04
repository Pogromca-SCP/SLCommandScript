using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a list of objects.
/// </summary>
/// <typeparam name="TItem">Type of contained objects.</typeparam>
/// <param name="source">Source of objects to insert into wrapped list.</param>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public class LazyListIterable<TItem>(Func<IEnumerable<TItem>> source, Action<IDictionary<string, string>, TItem> mapper) : ListIterableBase<TItem>(mapper)
{
    /// <inheritdoc />
    protected override IEnumerable<TItem> Items => _source();

    /// <summary>
    /// Source of iterated objects.
    /// </summary>
    private readonly Func<IEnumerable<TItem>> _source = source;
}
