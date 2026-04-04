using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Lazy iterable wrapper for a single item.
/// </summary>
/// <typeparam name="TItem">Type of contained object.</typeparam>
/// <param name="source">Source of iterated object.</param>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public class LazySingleItemIterable<TItem>(Func<TItem> source, Action<IDictionary<string, string>, TItem> mapper) : SingleItemIterableBase<TItem>(mapper)
{
    /// <inheritdoc />
    protected override TItem Item => _source();

    /// <summary>
    /// Source of iterated object.
    /// </summary>
    private readonly Func<TItem> _source = source;
}
