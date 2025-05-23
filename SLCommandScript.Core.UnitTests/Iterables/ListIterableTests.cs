using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using SLCommandScript.TestUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class ListIterableTests
{
    private static readonly string?[]?[] _strings = [null, [], [null, null, null, null], ["example", null, "", "test"], ["  \t ", "Test", "test", "TEST"]];

    private static readonly int[] _sizes = [-1, 0, 1, 2, 3];

    private static readonly float[] _percentages = [-1.0f, 0.0f, 0.25f, 0.1f, 0.5f, 2.5f];

    private static IEnumerable<object?[]> StringsXSizes => TestArrays.CartesianJoin(_strings, _sizes);

    private static IEnumerable<object?[]> StringsXPercentages => TestArrays.CartesianJoin(_strings, _percentages);

    #region Constructor Tests
    [Test]
    public void ListIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNull()
    {
        // Act
        var iterable = new ListIterable<string>((Func<IEnumerable<string>>?) null, null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }


    [TestCaseSource(nameof(_strings))]
    public void ListIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNotNull(string?[]? strings)
    {
        // Act
        var iterable = new ListIterable<string?>(() => strings, null);

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [Test]
    public void ListIterable_ShouldProperlyInitialize_WhenProvidedItemsAreNull()
    {
        // Act
        var iterable = new ListIterable<string>((IEnumerable<string>?) null, null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }


    [TestCaseSource(nameof(_strings))]
    public void ListIterable_ShouldProperlyInitialize_WhenProvidedItemsAreNotNull(string?[]? strings)
    {
        // Act
        var iterable = new ListIterable<string?>(strings, null);

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }
    #endregion

    #region LoadNext Tests
    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenDataSourceIsNull()
    {
        // Arrange
        var iterable = new ListIterable<string>((Func<IEnumerable<string>>?) null, null);

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, null);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedMapperIsNull(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, null);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().BeEmpty();
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().Equal(strings ?? []);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenItemsAreNull()
    {
        // Arrange
        var iterable = new ListIterable<string>((IEnumerable<string>?) null, null);

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenPredefinedAndProvidedDictionaryIsNull(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(strings!, null);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenPredefinedAndProvidedMapperIsNull(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(strings!, null);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().BeEmpty();
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlySetVariables_WhenPredefinedAndProvidedDictionaryIsNotNull(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(strings!, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().Equal(strings ?? []);
    }
    #endregion

    #region Randomize Tests
    [TestCaseSource(nameof(_strings))]
    public void Randomize_ShouldProperlyRandomizeElements(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, TestVariablesCollector.Inject);
        var count = 0;
        var variables = new TestVariablesCollector();

        // Act
        iterable.Randomize();

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().BeEquivalentTo(strings ?? []);
    }

    [TestCaseSource(nameof(StringsXSizes))]
    public void Randomize_ShouldProperlyRandomizeElements(string?[]? strings, int randAmount)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, TestVariablesCollector.Inject);
        var count = 0;

        // Act
        iterable.Randomize(randAmount);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len > randAmount && randAmount > 0 ? randAmount : len);
        count.Should().Be(len > randAmount && randAmount > 0 ? randAmount : len);
    }

    [TestCaseSource(nameof(StringsXPercentages))]
    public void Randomize_ShouldProperlyRandomizeElementsByPercentage(string?[]? strings, float percentage)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, TestVariablesCollector.Inject);
        var count = 0;
        var len = strings?.Length ?? 0;
        var randAmount = (int) (len * percentage);

        // Act
        iterable.Randomize(percentage);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len > randAmount && percentage > 0.0f ? randAmount : len);
        count.Should().Be(len > randAmount && percentage > 0.0f ? randAmount : len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Randomize_ShouldProperlyRandomizePredefinedElements(string?[]? strings)
    {
        // Arrange
        var items = strings?.ToArray();
        var iterable = new ListIterable<string>(strings!, TestVariablesCollector.Inject);
        var count = 0;
        var variables = new TestVariablesCollector();

        // Act
        iterable.Randomize();

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        items.Should().Equal(strings);
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().BeEquivalentTo(strings ?? []);
    }

    [TestCaseSource(nameof(StringsXSizes))]
    public void Randomize_ShouldProperlyRandomizePredefinedElements(string?[]? strings, int randAmount)
    {
        // Arrange
        var items = strings?.ToArray();
        var iterable = new ListIterable<string>(strings!, TestVariablesCollector.Inject);
        var count = 0;

        // Act
        iterable.Randomize(randAmount);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        var len = strings?.Length ?? 0;
        items.Should().Equal(strings);
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len > randAmount && randAmount > 0 ? randAmount : len);
        count.Should().Be(len > randAmount && randAmount > 0 ? randAmount : len);
    }

    [TestCaseSource(nameof(StringsXPercentages))]
    public void Randomize_ShouldProperlyRandomizePredefinedElementsByPercentage(string?[]? strings, float percentage)
    {
        // Arrange
        var items = strings?.ToArray();
        var iterable = new ListIterable<string>(strings!, TestVariablesCollector.Inject);
        var count = 0;
        var len = strings?.Length ?? 0;
        var randAmount = (int)(len * percentage);

        // Act
        iterable.Randomize(percentage);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        items.Should().Equal(strings);
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len > randAmount && percentage > 0.0f ? randAmount : len);
        count.Should().Be(len > randAmount && percentage > 0.0f ? randAmount : len);
    }
    #endregion

    #region Reset Tests
    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, TestVariablesCollector.Inject);

        // Act
        iterable.Reset();

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_AfterRunning(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(() => strings!, TestVariablesCollector.Inject);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetPredefinedIterable_BeforeRunning(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(strings!, TestVariablesCollector.Inject);

        // Act
        iterable.Reset();

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetPredefinedIterable_AfterRunning(string?[]? strings)
    {
        // Arrange
        var iterable = new ListIterable<string>(strings!, TestVariablesCollector.Inject);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }
    #endregion
}
