using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Commands;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Language;
using System.Collections.Generic;

namespace SLCommandScript.UnitTests.Commands;

[TestFixture]
public class IterablesCommandTests
{
    private const string TestIterable = "test";

    #region Execute Tests
    [Test]
    public void Execute_ShouldSucceed_WhenNoArgumentsArePassed()
    {
        // Arrange
        Parser.Iterables.Clear();
        Parser.Iterables[TestIterable] = null;
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new(), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"Currently available iterables:\n{TestIterable}\r\n");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterableDoesNotExist()
    {
        // Arrange
        Parser.Iterables.Clear();
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' was not found in available iterables");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterableIsNull()
    {
        // Arrange
        Parser.Iterables.Clear();
        Parser.Iterables[TestIterable] = null;
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' is null");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterableReturnsNull()
    {
        // Arrange
        Parser.Iterables.Clear();
        Parser.Iterables[TestIterable] = () => null;
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' returned null");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterationFailsToLoadElements()
    {
        // Arrange
        Parser.Iterables.Clear();
        Parser.Iterables[TestIterable] = () => new TestIterable(true, false);
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' has no elements");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenIterableHasNoVariables()
    {
        // Arrange
        Parser.Iterables.Clear();
        Parser.Iterables[TestIterable] = () => new TestIterable(false, false);
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"No variables available in '{TestIterable}'");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        Parser.Iterables.Clear();
        Parser.Iterables[TestIterable] = () => new TestIterable(false, true);
        var command = new IterablesCommand();

        // Act
        var result = command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"Variables available in '{TestIterable}':\ntest\r\n");
    }
    #endregion
}

public class TestIterable(bool isAtEnd, bool addVars) : IIterable
{
    public bool IsAtEnd { get; } = isAtEnd;

    public bool AddVars { get; } = addVars;

    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (!IsAtEnd && AddVars)
        {
            targetVars["test"] = "test";
        }

        return IsAtEnd;
    }

    public void Randomize() {}

    public void Randomize(int amount) {}

    public void Reset() {}
}
