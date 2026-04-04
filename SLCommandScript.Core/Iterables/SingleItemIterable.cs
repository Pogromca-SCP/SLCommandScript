using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for a single item.
/// </summary>
/// <typeparam name="TItem">Type of contained object.</typeparam>
/// <param name="item">Iterated object.</param>
/// <param name="mapper">Variable mapper to use to load variables.</param>
public class SingleItemIterable<TItem>(TItem item, Action<IDictionary<string, string>, TItem> mapper) : SingleItemIterableBase<TItem>(mapper)
{
    /// <inheritdoc />
    protected override TItem Item => _item;

    /// <summary>
    /// Currently stored item.
    /// </summary>
    private readonly TItem _item = item;
}
