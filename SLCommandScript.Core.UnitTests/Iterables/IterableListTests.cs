using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class IterableListTests
{
    #region Test Case Sources
    private static readonly string[][] _strings = [null, [], [null, null, null, null], ["example", null, "", "test"], ["  \t ", "Test", "test", "TEST"]];

    private static readonly int[] _sizes = [-1, 0, 1, 2, 3];

    private static IEnumerable<object[]> StringsXSizes => JoinArrays(_strings, _sizes);

    private static IEnumerable<object[]> JoinArrays<TFirst, TSecond>(TFirst[] first, TSecond[] second) =>
        first.SelectMany(f => second.Select(s => new object[] { f, s }));
    #endregion

    #region Constructor Tests
    [Test]
    public void IterableList_ShouldProperlyInitialize_WhenProvidedDataSourceIsNull()
    {
        // Act
        var iterable = new TestIterable(null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
    }


    [TestCaseSource(nameof(_strings))]
    public void IterableList_ShouldProperlyInitialize_WhenProvidedDataSourceIsNotNull(string[] strings)
    {
        // Act
        var iterable = new TestIterable(() => strings);

        // Assert
        iterable.IsAtEnd.Should().Be(strings is null || strings.Where(s => s is not null).IsEmpty());
    }
    #endregion

    #region LoadNext Tests
    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenDataSourceIsNull()
    {
        // Arrange
        var iterable = new TestIterable(null);

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(strings is null ? 0 : strings.Where(s => s is not null).Count());
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var filteredStrings = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var variables = new Dictionary<string, string>();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(filteredStrings.Count());

        foreach (var str in filteredStrings)
        {
            variables[str].Should().Be(str);
        }
    }
    #endregion

    #region Randomize Tests
    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyRandomizeElements(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var count = 0;
        var filteredStrings = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var filteredCount = filteredStrings.Count();

        // Act
        iterable.Randomize();

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(filteredCount);
    }

    [TestCaseSource(nameof(StringsXSizes))]
    public void Reset_ShouldProperlyRandomizeElements(string[] strings, int randAmount)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var count = 0;
        var filteredStrings = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var filteredCount = filteredStrings.Count();

        // Act
        iterable.Randomize(randAmount);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(filteredCount > randAmount && randAmount > 0 ? randAmount : filteredCount);
    }
    #endregion

    #region Reset Tests
    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().Be(strings is null || strings.Where(s => s is not null).IsEmpty());
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_AfterRunning(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().Be(strings is null || strings.Where(s => s is not null).IsEmpty());
    }
    #endregion
}

public class TestIterable(Func<IEnumerable<string>> strings) : IterableListBase<string>(strings)
{
    protected override void LoadVariables(IDictionary<string, string> targetVars, string str)
    {
        targetVars[str] = str;
    }
}
