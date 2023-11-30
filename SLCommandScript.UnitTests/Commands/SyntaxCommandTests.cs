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
        command.Tips.Clear();
        command.Tips["test"] = null;
        command.Tips["xd"] = null;

        // Act
        var result = command.Execute(new(), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be($"Available expression tips:\ntest\r\nxd\r\n");
    }

    [Test]
    public void Execute_ShouldFail_WhenSyntaxTipDoesNotExist()
    {
        // Arrange
        var command = new SyntaxCommand();
        command.Tips.Clear();

        // Act
        var result = command.Execute(new(["xd"], 0, 1), null, out var response);

        // Assert
        result.Should().BeFalse();
        response.Should().Be("No syntax tips found for 'xd'");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var command = new SyntaxCommand();
        command.Tips.Clear();
        command.Tips["test"] = "Example text";

        // Act
        var result = command.Execute(new(["test"], 0, 1), null, out var response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be(command.Tips["test"]);
    }
    #endregion
}
