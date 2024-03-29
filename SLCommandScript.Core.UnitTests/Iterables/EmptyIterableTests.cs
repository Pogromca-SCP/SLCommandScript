using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class EmptyIterableTests
{
    #region Constructor Tests
    [Test]
    public void EmptyIterable_ShouldProperlyInitialize()
    {
        // Act
        var iterable = new EmptyIterable();

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }
    #endregion

    #region LoadNext Tests
    [Test]
    public void LoadNext_ShouldReturnFalse()
    {
        // Arrange
        var iterable = new EmptyIterable();

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }
    #endregion
}
