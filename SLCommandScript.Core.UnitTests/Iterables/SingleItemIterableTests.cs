using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using System;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class SingleItemIterableTests
{
    private const string TestString = "test";

    #region Constructor Tests
    [Test]
    public void SingleItemIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNull()
    {
        // Act
        var iterable = new SingleItemIterable<string>((Func<string>) null, null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [Test]
    public void SingleItemIterable_ShouldProperlyInitialize_WhenProvidedDataSourceIsNotNull()
    {
        // Act
        var iterable = new SingleItemIterable<string>(() => TestString, null);

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void SingleItemIterable_ShouldProperlyInitialize_WhenProvidedItemDirectly()
    {
        // Act
        var iterable = new SingleItemIterable<string>(TestString, null);

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }
    #endregion

    #region LoadNext Tests
    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenDataSourceIsNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>((Func<string>) null, null);

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(() => TestString, null);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(1);
        count.Should().Be(1);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedMapperIsNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(() => TestString, null);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(1);
        count.Should().Be(1);
        variables.GetArray().Should().BeEmpty();
    }

    [Test]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(1);
        count.Should().Be(1);
        variables.GetArray().Should().HaveCount(1);
        variables.GetArray().Should().Contain(TestString);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedItemAndDictionaryIsNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(TestString, null);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(1);
        count.Should().Be(1);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedItemAndMapperIsNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(TestString, null);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(1);
        count.Should().Be(1);
        variables.GetArray().Should().BeEmpty();
    }

    [Test]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedItemAndDictionaryIsNotNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(TestString, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(1);
        count.Should().Be(1);
        variables.GetArray().Should().HaveCount(1);
        variables.GetArray().Should().Contain(TestString);
    }
    #endregion

    #region Reset Tests
    [Test]
    public void Reset_ShouldProperlyResetIterable_WhenSourceIsNull()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>((Func<string>) null, TestVariablesCollector.Inject);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_AfterRunning()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_BeforeRunningOnItem()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(TestString, TestVariablesCollector.Inject);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_AfterRunningOnItem()
    {
        // Arrange
        var iterable = new SingleItemIterable<string>(TestString, TestVariablesCollector.Inject);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }
    #endregion
}
