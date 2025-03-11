using CommandSystem;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Commands;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptDirectoryCommandTests
{
    #region Constructor Tests
    [TestCase("hello", "hello")]
    [TestCase(null, null)]
    [TestCase("example/", "")]
    [TestCase("/example", "example")]
    [TestCase("example/test", "test")]
    [TestCase("example/multiple//test", "test")]
    public void FileScriptDirectoryCommand_ShouldProperlyInitialize(string path, string name)
    {
        // Act
        var result = new FileScriptDirectoryCommand(path);

        // Assert
        result.Command.Should().Be(name);
        result.Aliases.Should().BeNull();
        result.Description.Should().Be("Parent command containing all scripts in a directory.");
        result.Path.Should().Be(path);
    }
    #endregion

    #region Execute Tests
    [Test]
    public void ExecuteParent_ShouldProperlyInvokeSubcommand([Values] bool isSuccess)
    {
        // Arrange
        var response = "hello";
        var cmd = new FileScriptDirectoryCommand(null);
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Command).Returns("test");
        cmdMock.Setup(x => x.Aliases).Returns<string[]>(null);
        cmdMock.Setup(x => x.Execute(new(new[] { "test" }, 0, 0), null, out response)).Returns(isSuccess);
        cmd.RegisterCommand(cmdMock.Object);

        // Act
        var result = cmd.Execute(new(["test"], 0, 1), null, out var message);

        // Assert
        result.Should().Be(isSuccess);
        message.Should().Be(response);
        cmdMock.VerifyAll();
        cmdMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ExecuteParent_ShouldFail()
    {
        // Arrange
        var cmd = new FileScriptDirectoryCommand(null);

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Cannot execute this parent command");
    }
    #endregion
}
