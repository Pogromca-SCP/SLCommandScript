using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Commands;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Iterables;
using SLCommandScript.TestUtils;
using System;
using System.Collections.Generic;

namespace SLCommandScript.UnitTests.Commands;

[TestFixture]
public class IterablesCommandTests
{
    private const string TestIterable = "test";

    private IEnumerable<KeyValuePair<string, Func<IIterable>>> _originalIterables;

    private IterablesCommand _command;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _originalIterables = TestDictionaries.ClearDictionary(IterablesUtils.Providers);
        _command = new();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestDictionaries.SetDictionary(IterablesUtils.Providers, _originalIterables);
        _originalIterables = null;
        _command = null;
    }

    #region Execute Tests
    [Test]
    public void Execute_ShouldSucceed_WhenNoArgumentsArePassed()
    {
        // Arrange
        IterablesUtils.Providers[TestIterable] = null;

        // Act
        var result = _command.Execute(new(), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"Currently available iterables:\n{TestIterable}\r\n");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterableDoesNotExist()
    {
        // Arrange
        IterablesUtils.Providers.Clear();

        // Act
        var result = _command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' was not found in available iterables");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterableIsNull()
    {
        // Arrange
        IterablesUtils.Providers[TestIterable] = null;

        // Act
        var result = _command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' is null");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterableReturnsNull()
    {
        // Arrange
        IterablesUtils.Providers[TestIterable] = () => null;

        // Act
        var result = _command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' returned null");
    }

    [Test]
    public void Execute_ShouldFail_WhenIterationFailsToLoadElements()
    {
        // Arrange
        IterablesUtils.Providers[TestIterable] = () => new TestIterable(true, false);

        // Act
        var result = _command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be($"'{TestIterable}' has no elements");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenIterableHasNoVariables()
    {
        // Arrange
        IterablesUtils.Providers[TestIterable] = () => new TestIterable(false, false);

        // Act
        var result = _command.Execute(new([TestIterable], 0, 1), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"No variables available in '{TestIterable}'");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        IterablesUtils.Providers[TestIterable] = () => new TestIterable(false, true);

        // Act
        var result = _command.Execute(new([TestIterable], 0, 1), null, out var response);

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

    public void Randomize(IterableSettings settings) {}

    public void Reset() {}
}
