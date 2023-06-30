using NUnit.Framework;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using SLCommandScript.Core.Iterables;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class IterableListTests
{
    private static readonly string[][] _strings = { new string[] { null, null, null, null }, new[] { "example", null, "", "test" },
        new[] { "  \t ", "Test", "test", "TEST" } };

    #region Constructor Tests
    [Test]
    public void IterableList_ShouldProperlyInitialize_WhenProvidedCollectionIsNull()
    {
        // Act
        var iterable = new TestIterable(null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(_strings))]
    public void IterableList_ShouldProperlyInitialize_WhenProvidedCollectionIsNotNull(string[] strings)
    {
        // Act
        var iterable = new TestIterable(strings);

        // Assert
        iterable.IsAtEnd.Should().Be(strings.Where(s => s is not null).IsEmpty());
    }
    #endregion

    #region LoadNext Tests
    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(strings);
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(strings.Where(s => s is not null).Count());
    }

    [TestCaseSource(nameof(_strings))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(strings);
        var filteredStrings = strings.Where(s => s is not null);
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

    #region Reset Tests
    public void Reset_ShouldProperlyResetIterable(string[] strings)
    {
        // Arrange
        var iterable = new TestIterable(strings);

        // Act
        while (iterable.LoadNext(null)) {}
        iterable.Reset();

        // Assert
        iterable.IsAtEnd.Should().Be(strings.Where(s => s is not null).IsEmpty());
    }
    #endregion
}

public class TestIterable : IterableListBase<string>
{
    public TestIterable(IEnumerable<string> strings) : base(strings) {}

    protected override void LoadVariables(IDictionary<string, string> targetVars, string str)
    {
        targetVars[str] = str;
    }
}
