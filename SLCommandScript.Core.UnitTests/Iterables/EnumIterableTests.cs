﻿using FluentAssertions;
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
    private static readonly bool[] _boolValues = [false, true];

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

    [Test]
    public void GetWithNone_ShouldProperlyReturnIterableObject()
    {
        // Act
        var iterable = EnumIterable<EmptyEnum>.GetWithNone();

        // Assert
        iterable.Should().NotBeNull();
    }
    #endregion

    #region Constructor Tests
    [TestCaseSource(nameof(_boolValues))]
    public void EnumIterable_ShouldProperlyInitialize_WhenProvidedEnumTypeHasNoValues(bool enableNone)
    {
        // Act
        var iterable = new EnumIterable<EmptyEnum>(enableNone);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(_boolValues))]
    public void EnumIterable_ShouldProperlyInitialize_WhenProvidedEnumTypeHasValues(bool enableNone)
    {
        // Act
        var iterable = new EnumIterable<FullEnum>(enableNone);

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
    }
    #endregion

    #region LoadNext Tests
    [TestCaseSource(nameof(_boolValues))]
    public void LoadNext_ShouldProperlyIterate_WhenEnumTypeHasNoValues(bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<EmptyEnum>(enableNone);

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(_boolValues))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull(bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
    }

    [TestCaseSource(nameof(_boolValues))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
        variables.GetArray().Should().Equal(enableNone ? _values : _values.Where(v => !v.Equals(FullEnum.None.ToString("D"))));
    }
    #endregion

    #region Randomize Tests
    [TestCaseSource(nameof(_boolValues))]
    public void Randomize_ShouldProperlyRandomizeElements(bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);
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
        count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
        variables.GetArray().Should().BeEquivalentTo(enableNone ? _values : _values.Where(v => !v.Equals(FullEnum.None.ToString("D"))));
    }

    [TestCaseSource(nameof(_sizes))]
    public void Randomize_ShouldProperlyRandomizeElements(int randAmount)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(true);
        var count = 0;

        // Act
        iterable.Randomize(randAmount);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(_values.Length > randAmount && randAmount > 0 ? randAmount : _values.Length);
    }
    #endregion

    #region Reset Tests
    [TestCaseSource(nameof(_boolValues))]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning(bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_boolValues))]
    public void Reset_ShouldProperlyResetIterable_AfterRunning(bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);

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
    None,
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