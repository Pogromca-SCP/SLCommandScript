using NUnit.Framework;
using Moq;
using SLCommandScript.FileScriptsLoader.Helpers;
using SLCommandScript.FileScriptsLoader.Commands;
using FluentAssertions;
using System;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptCommandBaseTests
{
    #region Error Flow Test Case Sources
    private static string[][] _errorPaths = {
        new[] { "xd", "Command 'xd' was not found\nat test.slcs:1" },
        new[] { null, "Cannot read script from file 'test.slcs'" },
        new[] { "[", "Directive body is invalid\nat test.slcs:1" }
    };
    #endregion

    #region Gold Flow Test Case Sources
    private static string[] _goldPaths = {
        string.Empty,
        "help",
        "#This is a comment"
    };
    #endregion

    #region Description Tests
    [Test]
    public void Description_ShouldBeSetToDefault_WhenProvidedValueIsNull()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommandBase(null)
        {
            Description = null
        };

        // Assert
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Description_ShouldBeSetToDefault_WhenProvidedValueIsBlank()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommandBase(null)
        {
            Description = "     "
        };

        // Assert
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Description_ShouldBeSetToNewValue_WhenProvidedValueIsValid()
    {
        // Arrange
        const string newDesc = "HelloThere!";
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommandBase(null)
        {
            Description = newDesc
        };

        // Assert
        result.Description.Should().Be(newDesc);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Constructor Tests
    [Test]
    public void FileScriptCommandBase_ShouldThrow_WhenGetFileNameWithoutExtensionThrows()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Throws(new Exception());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var action = () => new FileScriptCommandBase(null);

        // Assert
        action.Should().Throw<Exception>();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void FileScriptCommandBase_ShouldProperlyInitialize_WhenGoldFlow()
    {
        // Arrange
        const string testName = "test";
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns(testName);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommandBase(null);

        // Assert
        result.Command.Should().Be(testName);
        result.Aliases.Should().BeNull();
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Execute Tests
    [Test]
    public void Execute_ShouldFail_WhenConcurrentExecutionsLimitIsExceeded()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        var cmd = new FileScriptCommandBase(null);

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Script execution terminated due to exceeded concurrent executions limit");
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Execute_ShouldFail_WhenReadFileThrows()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        fileSystemMock.Setup(x => x.ReadFile(null)).Throws(new Exception());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 1;
        var cmd = new FileScriptCommandBase(null);

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Cannot read script from file 'test.slcs'");
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Execute_ShouldFail_WhenScriptFails(string src, string expectedError)
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        fileSystemMock.Setup(x => x.ReadFile(null)).Returns(src);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 1;
        var cmd = new FileScriptCommandBase(null);

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be(expectedError);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Execute_ShouldSucceed_WhenGoldFlow(string src)
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        fileSystemMock.Setup(x => x.ReadFile(null)).Returns(src);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 1;
        var cmd = new FileScriptCommandBase(null);

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
