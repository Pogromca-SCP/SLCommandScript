using AwesomeAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class EnumIterableTests
{
    private static readonly int[] _sizes = [-1, 0, 1, 2, 3];

    private static readonly float[] _percentages = [-1.0f, 0.0f, 0.25f, 0.1f, 0.5f, 2.5f];

    private static readonly string[] _values = [..((FullEnum[]) typeof(FullEnum).GetEnumValues()).Select(v => v.ToString("D"))];

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

    [Test]
    public void EnumIterable_ShouldProperlyInitialize_WhenProvidedEnumTypeHasNoValues([Values] bool enableNone)
    {
        // Act
        var iterable = new EnumIterable<EmptyEnum>(enableNone);

        // Assert
        iterable.Count.Should().Be(0);
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [Test]
    public void EnumIterable_ShouldProperlyInitialize_WhenProvidedEnumTypeHasValues([Values] bool enableNone)
    {
        // Act
        var iterable = new EnumIterable<FullEnum>(enableNone);

        // Assert
        iterable.Count.Should().Be(0);
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenEnumTypeHasNoValues([Values] bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<EmptyEnum>(enableNone);

        // Act
        var result = iterable.LoadNext(null);

        // Assert
        result.Should().BeFalse();
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(0);
    }

    [Test]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull([Values] bool enableNone)
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
        iterable.Count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
        count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
    }

    [Test]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull([Values] bool enableNone)
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
        iterable.Count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
        count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
        variables.GetArray().Should().Equal(enableNone ? _values : _values.Where(v => !v.Equals(FullEnum.None.ToString("D"))));
    }

    [Test]
    public void Randomize_ShouldProperlyRandomizeElements([Values] bool enableNone)
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
        iterable.Count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
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
        iterable.Count.Should().Be(_values.Length > randAmount && randAmount > 0 ? randAmount : _values.Length);
        count.Should().Be(_values.Length > randAmount && randAmount > 0 ? randAmount : _values.Length);
    }

    [TestCaseSource(nameof(_percentages))]
    public void Randomize_ShouldProperlyRandomizeElementsByPercentage(float percentage)
    {
        // Arrange
        var randAmount = (int) (_values.Length * percentage);
        var iterable = new EnumIterable<FullEnum>(true);
        var count = 0;

        // Act
        iterable.Randomize(percentage);

        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        iterable.Count.Should().Be(_values.Length > randAmount && percentage > 0.0f ? randAmount : _values.Length);
        count.Should().Be(_values.Length > randAmount && percentage > 0.0f ? randAmount : _values.Length);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_BeforeRunning([Values] bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);

        // Act
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
    }

    [Test]
    public void Reset_ShouldProperlyResetIterable_AfterRunning([Values] bool enableNone)
    {
        // Arrange
        var iterable = new EnumIterable<FullEnum>(enableNone);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().BeFalse();
        iterable.Count.Should().Be(enableNone ? _values.Length : _values.Length - 1);
    }
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
