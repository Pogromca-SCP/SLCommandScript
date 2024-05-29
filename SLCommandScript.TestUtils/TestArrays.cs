using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.TestUtils;

public static class TestArrays
{
    public static IEnumerable<object[]> CartesianJoin<TFirst, TSecond>(TFirst[] first, TSecond[] second) =>
        first.SelectMany(f => second.Select<TSecond, object[]>(s => [f, s]));
}
