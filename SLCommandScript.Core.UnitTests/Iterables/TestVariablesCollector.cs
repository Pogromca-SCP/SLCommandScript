using System.Collections;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Iterables;

public class TestVariablesCollector : IDictionary<string, string?>
{
    public static void Inject(IDictionary<string, string?> target, string item) => target[item] = item;

    private readonly List<string?> _values = [];

    public string? this[string key] { get => string.Empty; set => Add(key, value); }

    public ICollection<string>? Keys => null;

    public ICollection<string?>? Values => null;

    public int Count => 0;

    public bool IsReadOnly => false;

    public void Add(string key, string? value) => _values.Add(value);

    public void Add(KeyValuePair<string, string?> item) {}

    public void Clear() {}

    public bool Contains(KeyValuePair<string, string?> item) => false;

    public bool ContainsKey(string key) => false;

    public void CopyTo(KeyValuePair<string, string?>[] array, int arrayIndex) {}

    public IEnumerator<KeyValuePair<string, string?>>? GetEnumerator() => null;

    public bool Remove(string key) => true;

    public bool Remove(KeyValuePair<string, string?> item) => true;

    public bool TryGetValue(string key, out string value)
    {
        value = string.Empty;
        return true;
    }

    public string?[] GetArray() => [.. _values];

    IEnumerator? IEnumerable.GetEnumerator() => null;
}
