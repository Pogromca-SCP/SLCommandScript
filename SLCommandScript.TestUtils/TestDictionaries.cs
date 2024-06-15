using System.Collections.Generic;

namespace SLCommandScript.TestUtils;

public static class TestDictionaries
{
    public static IEnumerable<KeyValuePair<TKey, TValue>> ClearDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        IEnumerable<KeyValuePair<TKey, TValue>> tmp = [..dict];
        dict.Clear();
        return tmp;
    }

    public static void SetDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        dict.Clear();

        foreach (var pair in pairs)
        {
            dict[pair.Key] = pair.Value;
        }
    }
}
