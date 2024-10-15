using SLCommandScript.Core.Iterables;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Language;

public class TestIterable : IIterable
{
    public const int MaxIterations = 10;

    private int _index = 1;

    public bool IsAtEnd => _index > MaxIterations;

    public int Count => MaxIterations;

    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (IsAtEnd)
        {
            return false;
        }

        targetVars["i"] = _index.ToString();
        targetVars["wut?"] = "hello";
        ++_index;
        return true;
    }

    public void Randomize() {}

    public void Randomize(int amount) {}

    public void Randomize(float amount) {}

    public void Randomize(IterableSettings settings) {}

    public void Reset() => _index = 1;
}
