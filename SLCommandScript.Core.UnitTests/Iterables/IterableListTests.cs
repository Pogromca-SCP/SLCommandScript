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

    private static readonly float[] _percentages = [-1.0f, 0.0f, 0.25f, 0.1f, 0.5f, 2.5f];

    private static IEnumerable<object[]> StringsXSizes => JoinArrays(_strings, _sizes);

    private static IEnumerable<object[]> StringsXPercentages => JoinArrays(_strings, _percentages);

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
        iterable.Count.Should().Be(0);
    }


    [TestCaseSource(nameof(_strings))]
    public void IterableList_ShouldProperlyInitialize_WhenProvidedDataSourceIsNotNull(string[] strings)
    {
        // Arrange
        var filtered = strings?.Where(s => s is not null);

        // Act
        var iterable = new TestIterable(() => strings);

        // Assert
        iterable.IsAtEnd.Should().Be(filtered is null || filtered.IsEmpty());
        iterable.Count.Should().Be(filtered?.Count() ?? 0);
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
        iterable.Count.Should().Be(0);
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull(string[] strings)
    {
        // Arrange
        var filteredCount = strings?.Where(s => s is not null).Count() ?? 0;
        var iterable = new TestIterable(() => strings);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(filteredCount);
        count.Should().Be(filteredCount);
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var filteredStrings = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(filteredStrings.Count());
        count.Should().Be(filteredStrings.Count());
        variables.GetArray().Should().Equal(filteredStrings);
    }
    #endregion

    #region Randomize Tests
    [TestCaseSource(nameof(_strings))]
    public void Randomize_ShouldProperlyRandomizeElements(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var count = 0;
        var filteredStrings = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var filteredCount = filteredStrings.Count();
        var variables = new TestVariablesCollector();

        // Act
        iterable.Randomize();

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(filteredCount);
        count.Should().Be(filteredCount);
        variables.GetArray().Should().BeEquivalentTo(filteredStrings);
    }

    [TestCaseSource(nameof(StringsXSizes))]
    public void Randomize_ShouldProperlyRandomizeElements(string[] strings, int randAmount)
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
        iterable.Count.Should().Be(filteredCount > randAmount && randAmount > 0 ? randAmount : filteredCount);
        count.Should().Be(filteredCount > randAmount && randAmount > 0 ? randAmount : filteredCount);
    }

    [TestCaseSource(nameof(StringsXPercentages))]
    public void Randomize_ShouldProperlyRandomizeElementsByPercentage(string[] strings, float percentage)
    {
        // Arrange
        var iterable = new TestIterable(() => strings);
        var count = 0;
        var filteredStrings = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var filteredCount = filteredStrings.Count();
        var randAmount = (int) (filteredCount * percentage);

        // Act
        iterable.Randomize(percentage);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(filteredCount > randAmount && percentage > 0.0f ? randAmount : filteredCount);
        count.Should().Be(filteredCount > randAmount && percentage > 0.0f ? randAmount : filteredCount);
    }
    #endregion

    #region Reset Tests
    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning(string[] strings)
    {
        // Arrange
        var filtered = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var iterable = new TestIterable(() => strings);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().Be(filtered.IsEmpty());
        iterable.Count.Should().Be(filtered.Count());
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_AfterRunning(string[] strings)
    {
        // Arrange
        var filtered = strings?.Where(s => s is not null) ?? Array.Empty<string>();
        var iterable = new TestIterable(() => strings);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().Be(filtered.IsEmpty());
        iterable.Count.Should().Be(filtered.Count());
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
