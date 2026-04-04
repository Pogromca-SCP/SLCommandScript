using AwesomeAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class LazySingleItemIterableTests
{
    private const string TestString = "test";

    [Test]
    public void SingleItemIterable_ShouldProperlyInitialize_WhenProvidedDataSource()
    {
        // Act
        var iterable = new LazySingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionary()
    {
        // Arrange
        var iterable = new LazySingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);
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
    public void Reload_ShouldProperlyResetIterable_BeforeRunning()
    {
        // Arrange
        var iterable = new LazySingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);

        // Act
        iterable.Reload();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void Reload_ShouldProperlyResetIterable_AfterRunning()
    {
        // Arrange
        var iterable = new LazySingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();

        // Act
        while (iterable.LoadNext(variables)) {}
        iterable.Reload();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning()
    {
        // Arrange
        var iterable = new LazySingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);

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
        var iterable = new LazySingleItemIterable<string>(() => TestString, TestVariablesCollector.Inject);
        var variables = new TestVariablesCollector();

        // Act
        while (iterable.LoadNext(variables)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(1);
    }
}
