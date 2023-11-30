using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Commands;

namespace SLCommandScript.UnitTests.Commands;

[TestFixture]
public class SyntaxCommandTests
{
    #region Execute Tests
    [Test]
    public void Execute_ShouldSucceed_WhenNoArgumentsArePassed()
    {
        // Arrange
        var command = new SyntaxCommand();
        command.Rules.Clear();
        command.Rules["test"] = null;
        command.Rules["xd"] = null;

        // Act
        var result = command.Execute(new(), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"Available expression/guard types:\ntest\r\nxd\r\n");
    }

    [Test]
    public void Execute_ShouldFail_WhenSyntaxTipDoesNotExist()
    {
        // Arrange
        var command = new SyntaxCommand();
        command.Rules.Clear();

        // Act
        var result = command.Execute(new(["xd"], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be("No syntax rules found for 'xd'");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var command = new SyntaxCommand();
        command.Rules.Clear();
        command.Rules["test"] = "Example text";

        // Act
        var result = command.Execute(new(["test"], 0, 1), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be(command.Rules["test"]);
    }
    #endregion
}
