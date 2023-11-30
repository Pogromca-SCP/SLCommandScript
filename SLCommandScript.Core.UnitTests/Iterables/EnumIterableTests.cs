using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class EnumIterableTests
{
    #region Statis Utilities
    private static readonly int[] _sizes = [-1, 0, 1, 2, 3];

    private static readonly string[] _values = ((FullEnum[]) typeof(FullEnum).GetEnumValues()).Select(v => v.ToString("D")).ToArray();
    #endregion

    #region Get Tests
    [Test]
    public void Get_ShouldProperlyReturnIterableObject()
    {
        // Act
        var iterable = EnumIterable<EmptyEnum>.Get();

        // Assert
        iterable.Should().NotBeNull();
    }
    #endregion

    #region Constructor Tests
    [Test]
    public void EnumIterable_ShouldProperlyInitialize_WhenProvidedEnumTypeHasNoValues()
    {
        // Act
        var iterable = new EnumIterable<EmptyEnum>();

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
    }


    [Test]
    public void EnumIterable_ShouldProperlyInitialize_WhenProvidedEnumTypeHasValues()
    {
        // Act
        var iterable = new EnumIterable<FullEnum>();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
    }
    #endregion

    #region LoadNext Tests
    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenEnumTypeHasNoValues()
    {
        // Arrange
        var iterable = new EnumIterable<EmptyEnum>();

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull()
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>();
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(5);
    }

    [Test]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull()
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>();
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(5);
        variables.GetArray().Should().Equal(_values);
    }
    #endregion

    #region Randomize Tests
    [Test]
    public void Randomize_ShouldProperlyRandomizeElements()
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>();
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        iterable.Randomize();

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(5);
        variables.GetArray().Should().BeEquivalentTo(_values);
    }

    [TestCaseSource(nameof(_sizes))]
    public void Randomize_ShouldProperlyRandomizeElements(int randAmount)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>();
        var count = 0;

        // Act
        iterable.Randomize(randAmount);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(5 > randAmount && randAmount > 0 ? randAmount : 5);
    }
    #endregion

    #region Reset Tests
    [Test]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning()
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>();

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_AfterRunning()
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>();

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
    }
    #endregion
}

public enum EmptyEnum : byte {}

public enum FullEnum : sbyte
{
    First = -1,
    Second,
    Third,
    Fourth,
    Fifth
}

#region Variables Collector
public class TestVariablesCollector : IDictionary<string, string>
{
    private readonly List<string> _values = [];

    public string this[string key] { get => string.Empty; set => Add(key, value); }

    public ICollection<string> Keys => null;

    public ICollection<string> Values => null;

    public int Count => 0;

    public bool IsReadOnly => false;

    public void Add(string key, string value) => _values.Add(value);

    public void Add(KeyValuePair<string, string> item) {}

    public void Clear() {}

    public bool Contains(KeyValuePair<string, string> item) => false;

    public bool ContainsKey(string key) => false;

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {}

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => null;

    public bool Remove(string key) => true;

    public bool Remove(KeyValuePair<string, string> item) => true;

    public bool TryGetValue(string key, out string value)
    {
        value = string.Empty;
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => null;

    public string[] GetArray() => _values.ToArray();
}
#endregion
