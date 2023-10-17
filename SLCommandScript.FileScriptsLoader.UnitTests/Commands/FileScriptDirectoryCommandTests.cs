using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Commands;
using FluentAssertions;
using Moq;
using CommandSystem;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptDirectoryCommandTests
{
    #region Constructor Tests
    [TestCase("hello")]
    [TestCase(null)]
    [TestCase("wnebgosg")]
    public void FileScriptDirectoryCommand_ShouldProperlyInitialize(string name)
    {
        // Act
        var result = new FileScriptDirectoryCommand(name);

        // Assert
        result.Command.Should().Be(name);
        result.Aliases.Should().BeNull();
        result.Description.Should().Be("Parent command containing all scripts in a directory.");
    }
    #endregion

    #region Execute Tests
    [TestCase(true)]
    [TestCase(false)]
    public void ExecuteParent_ShouldProperlyInvokeSubcommand(bool isSuccess)
    {
        // Arrange
        var response = "hello";
        var cmd = new FileScriptDirectoryCommand(null);
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Command).Returns("test");
        cmdMock.Setup(x => x.Aliases).Returns((string[]) null);
        cmdMock.Setup(x => x.Execute(new(new[] { "test" }, 0, 0), null, out response)).Returns(isSuccess);
        cmd.RegisterCommand(cmdMock.Object);

        // Act
        var result = cmd.Execute(new(new[] { "test" }, 0, 1), null, out var message);

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
