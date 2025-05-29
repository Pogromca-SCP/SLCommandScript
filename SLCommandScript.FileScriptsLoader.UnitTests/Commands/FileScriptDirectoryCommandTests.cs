using CommandSystem;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Commands;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptDirectoryCommandTests
{
    private const string TestName = "test";

    [Test]
    public void FileScriptDirectoryCommand_ShouldProperlyInitialize_WhenNullsProvided()
    {
        // Act
        var result = new FileScriptDirectoryCommand(null, null);

        // Assert
        result.Command.Should().BeEmpty();
        result.Aliases.Should().BeNull();
        result.Description.Should().Be("Parent command containing all scripts in a directory.");
        result.Parent.Should().BeNull();
    }

    [Test]
    public void FileScriptDirectoryCommand_ShouldProperlyInitialize_WhenDataProvided()
    {
        // Arrange
        var parentMock = new Mock<IFileScriptCommandParent>(MockBehavior.Strict);

        // Act
        var result = new FileScriptDirectoryCommand(TestName, parentMock.Object);

        // Assert
        result.Command.Should().Be(TestName);
        result.Aliases.Should().BeNull();
        result.Description.Should().Be("Parent command containing all scripts in a directory.");
        result.Parent.Should().Be(parentMock.Object);
        parentMock.VerifyAll();
    }

    [Test]
    public void GetLocation_ShouldReturnProperPath_WhenNoParentPresent([Values] bool includeRoot)
    {
        // Arrange
        var cmd = new FileScriptDirectoryCommand(TestName, null);

        // Act
        var result = cmd.GetLocation(includeRoot);

        // Assert
        result.Should().Be(cmd.Command);
    }

    [Test]
    public void GetLocation_ShouldReturnProperPath_WhenParentIsPresent([Values] bool includeRoot)
    {
        // Arrange
        const string parentLocation = "parent";
        var parentMock = new Mock<IFileScriptCommandParent>(MockBehavior.Strict);
        parentMock.Setup(x => x.GetLocation(includeRoot)).Returns(parentLocation);
        var cmd = new FileScriptDirectoryCommand(TestName, parentMock.Object);

        // Act
        var result = cmd.GetLocation(includeRoot);

        // Assert
        result.Should().Be($"{parentLocation}{cmd.Command}{Path.DirectorySeparatorChar}");
        parentMock.VerifyAll();
    }

    [Test]
    public void ExecuteParent_ShouldProperlyInvokeSubcommand([Values] bool isSuccess)
    {
        // Arrange
        var response = "hello";
        var cmd = new FileScriptDirectoryCommand(null, null);
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Command).Returns("test");
        cmdMock.Setup(x => x.Aliases).Returns<string[]>(null!);
        cmdMock.Setup(x => x.Execute(new(new[] { "test" }, 0, 0), null, out response)).Returns(isSuccess);
        cmd.RegisterCommand(cmdMock.Object);

        // Act
        var result = cmd.Execute(new(["test"], 0, 1), null, out var message);

        // Assert
        result.Should().Be(isSuccess);
        message.Should().Be(response);
        cmdMock.VerifyAll();
    }

    [Test]
    public void ExecuteParent_ShouldFail()
    {
        // Arrange
        var cmd = new FileScriptDirectoryCommand(null, null);

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Cannot execute this parent command");
    }
}
