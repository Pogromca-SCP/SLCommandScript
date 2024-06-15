﻿using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Commands;

namespace SLCommandScript.UnitTests.Commands;

[TestFixture]
public class SyntaxCommandTests
{
    private SyntaxCommand _command;

    [SetUp]
    public void SetUp()
    {
        _command ??= new();
        _command.Rules.Clear();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => _command = null;

    #region Execute Tests
    [Test]
    public void Execute_ShouldSucceed_WhenNoArgumentsArePassed()
    {
        // Arrange
        _command.Rules["test"] = null;
        _command.Rules["xd"] = null;

        // Act
        var result = _command.Execute(new(), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"Available expression/guard types:\ntest\r\nxd\r\n");
    }

    [Test]
    public void Execute_ShouldFail_WhenSyntaxTipDoesNotExist()
    {
        // Act
        var result = _command.Execute(new(["xd"], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be("No syntax rules found for 'xd'");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        _command.Rules["test"] = "Example text";

        // Act
        var result = _command.Execute(new(["test"], 0, 1), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be(_command.Rules["test"]);
    }
    #endregion
}
