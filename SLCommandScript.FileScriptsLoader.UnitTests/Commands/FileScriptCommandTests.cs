using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptCommandTests
{
    #region Usage Tests
    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueIsNull()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommand(null)
        {
            Usage = null
        };

        // Assert
        result.Usage.Should().BeNull();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueIsEmptyArray()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommand(null)
        {
            Usage = []
        };

        // Assert
        result.Usage.Should().BeNull();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueHasOnlyBlankEntries()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommand(null)
        {
            Usage = ["", "       ", null, "\t\t"]
        };

        // Assert
        result.Usage.Should().BeNull();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Usage_ShouldBeSetToNewValue_WhenProvidedValueIsValid()
    {
        var usage = new[] { "Option", "Args..." };
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommand(null)
        {
            Usage = usage
        };

        // Assert
        result.Usage.Should().BeEquivalentTo(usage);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region GetHelp Tests
    [Test]
    public void GetHelp_ShouldReturnDescription_WhenHelpIsNull()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        var cmd = new FileScriptCommand(null)
        {
            Help = null
        };

        // Act
        var result = cmd.GetHelp(new());

        // Assert
        result.Should().Be(FileScriptCommandBase.DefaultDescription);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetHelp_ShouldReturnDescription_WhenHelpIsBlank()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        var cmd = new FileScriptCommand(null)
        {
            Help = "      "
        };

        // Act
        var result = cmd.GetHelp(new());

        // Assert
        result.Should().Be(FileScriptCommandBase.DefaultDescription);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetHelp_ShouldReturnHelp_WhenHelpIsValid()
    {
        const string help = "I don't know what I'm supposed to do.";
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        var cmd = new FileScriptCommand(null)
        {
            Help = help
        };

        // Act
        var result = cmd.GetHelp(new());

        // Assert
        result.Should().Be(help);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Execute Tests
    [Test]
    public void Execute_ShouldFail_WhenNotEnoughArgumentsAreProvided()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        var cmd = new FileScriptCommand(null)
        {
            Arity = 1
        };

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Missing argument: script expected 1 arguments, but sender provided 0");
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        fileSystemMock.Setup(x => x.ReadFile(null)).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 1;
        var cmd = new FileScriptCommand(null);

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeTrue();
        message.Should().Be("Script executed successfully.");
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion
}
