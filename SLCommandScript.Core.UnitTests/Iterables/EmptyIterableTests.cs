using AwesomeAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class EmptyIterableTests
{
    private readonly EmptyIterable _iterable = EmptyIterable.Instance;

    [Test]
    public void EmptyIterable_ShouldProperlyInitialize()
    {
        // Assert
        _iterable.IsAtEnd.Should().BeTrue();
        _iterable.Count.Should().Be(0);
    }

    [Test]
    public void LoadNext_ShouldReturnFalse()
    {
        // Act
        var result = _iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        _iterable.IsAtEnd.Should().BeTrue();
        _iterable.Count.Should().Be(0);
    }
}
