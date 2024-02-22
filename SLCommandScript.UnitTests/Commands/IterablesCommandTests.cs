using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Commands;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Iterables;
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
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers[TestIterable] = null;
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
        IterablesUtils.Providers.Clear();
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
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers[TestIterable] = null;
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
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers[TestIterable] = () => null;
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
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers[TestIterable] = () => new TestIterable(true, false);
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
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers[TestIterable] = () => new TestIterable(false, false);
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
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers[TestIterable] = () => new TestIterable(false, true);
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

    public int Count => 0;

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

    public void Randomize(float amount) {}

    public void Randomize(RandomSettings settings) {}

    public void Reset() {}
}
