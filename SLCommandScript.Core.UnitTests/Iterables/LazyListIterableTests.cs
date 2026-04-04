using AwesomeAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using SLCommandScript.TestUtils;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class LazyListIterableTests
{
    private static readonly string[][] _strings = [[], ["example", "", "test"], ["  \t ", "Test", "test", "TEST"]];

    private static readonly int[] _sizes = [-1, 0, 1, 2, 3];

    private static readonly float[] _percentages = [-1.0f, 0.0f, 0.25f, 0.1f, 0.5f, 2.5f];

    private static IEnumerable<object[]> StringsXSizes => TestArrays.CartesianJoin(_strings, _sizes);

    private static IEnumerable<object[]> StringsXPercentages => TestArrays.CartesianJoin(_strings, _percentages);

    [TestCaseSource(nameof(_strings))]
    public void ListIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNotNull(string[] strings)
    {
        // Act
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }  

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(string[] strings)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().Equal(strings);
    }

    [TestCaseSource(nameof(_strings))]
    public void Randomize_ShouldProperlyRandomizeElements(string[] strings)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);
        var count = 0;
        var variables = new TestVariablesCollector();

        // Act
        iterable.Randomize();

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len);
        count.Should().Be(len);
        variables.GetArray().Should().BeEquivalentTo(strings);
    }

    [TestCaseSource(nameof(StringsXSizes))]
    public void Randomize_ShouldProperlyRandomizeElements(string[] strings, int randAmount)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);
        var count = 0;
        var variables = new TestVariablesCollector();

        // Act
        iterable.Randomize(randAmount);

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len > randAmount && randAmount > 0 ? randAmount : len);
        count.Should().Be(len > randAmount && randAmount > 0 ? randAmount : len);
    }

    [TestCaseSource(nameof(StringsXPercentages))]
    public void Randomize_ShouldProperlyRandomizeElementsByPercentage(string[] strings, float percentage)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);
        var count = 0;
        var len = strings.Length;
        var randAmount = (int) (len * percentage);
        var variables = new TestVariablesCollector();

        // Act
        iterable.Randomize(percentage);

        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(len > randAmount && percentage > 0.0f ? randAmount : len);
        count.Should().Be(len > randAmount && percentage > 0.0f ? randAmount : len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reload_ShouldProperlyResetIterable_BeforeRunning(string[] strings)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);

        // Act
        iterable.Reload();

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reload_ShouldProperlyResetIterable_AfterRunning(string[] strings)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();

        // Act
        while (iterable.LoadNext(variables)) {}
        iterable.Reload();

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning(string[] strings)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);

        // Act
        iterable.Reset();

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_AfterRunning(string[] strings)
    {
        // Arrange
        var iterable = new LazyListIterable<string>(() => strings, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();

        // Act
        while (iterable.LoadNext(variables)) {}
        iterable.Reset();

        // Assert
        var len = strings.Length;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }
}
