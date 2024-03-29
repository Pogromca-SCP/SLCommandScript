using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class PredefinedIterableTests
{
    private static void Inject(IDictionary<string, string> target, string item)
    {
        target[item] = item;
    }

    private static readonly string[][] _strings = [null, [], [null, null, null, null], ["example", null, "", "test"], ["  \t ", "Test", "test", "TEST"]];

    #region Constructor Tests
    [Test]
    public void PredefinedIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNull()
    {
        // Act
        var iterable = new PredefinedIterable<string>(null, null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }


    [TestCaseSource(nameof(_strings))]
    public void PredefinedIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNotNull(string[] strings)
    {
        // Act
        var iterable = new PredefinedIterable<string>(strings, null);

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
        var iterable = new PredefinedIterable<string>(null, null);

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
        var iterable = new PredefinedIterable<string>(strings, null);
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
    public void LoadNext_ShouldProperlyIterate_WhenProvidedMapperIsNull(string[] strings)
    {
        // Arrange
        var iterable = new PredefinedIterable<string>(strings, null);
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
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(string[] strings)
    {
        // Arrange
        var iterable = new PredefinedIterable<string>(strings, Inject);
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

    #region Reset Tests
    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning(string[] strings)
    {
        // Arrange
        var iterable = new PredefinedIterable<string>(strings, Inject);

        // Act
        iterable.Reset();

        // Assert
        var len = strings?.Length ?? 0;
        iterable.IsAtEnd.Should().Be(len < 1);
        iterable.Count.Should().Be(len);
    }

    [TestCaseSource(nameof(_strings))]
    public void Reset_ShouldProperlyResetIterable_AfterRunning(string[] strings)
    {
        // Arrange
        var iterable = new PredefinedIterable<string>(strings, Inject);

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
