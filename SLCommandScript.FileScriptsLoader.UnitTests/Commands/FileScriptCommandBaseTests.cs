using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;
using System;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptCommandBaseTests : TestWithConfigBase
{
    private const string TestCommand = "test";

    private const string TestPath = "test.slcs";

    private static readonly string?[][] _errorPaths = [
        ["xd", "Command 'xd' was not found\nat test.slcs:1"],
        [null, "Cannot read script from file 'test.slcs'"],
        ["[", "Directive structure is invalid\nat test.slcs:1"]
    ];

    private static readonly string[] _goldPaths = [
        string.Empty,
        "help",
        "#This is a comment"
    ];

    #region Description Tests
    [Test]
    public void Description_ShouldBeSetToDefault_WhenProvidedValueIsNull()
    {
        // Act
        var result = new FileScriptCommandBase(null, null, RuntimeConfig)
        {
            Description = null!,
        };

        // Assert
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
    }

    [Test]
    public void Description_ShouldBeSetToDefault_WhenProvidedValueIsBlank()
    {
        // Act
        var result = new FileScriptCommandBase(null, null, RuntimeConfig)
        {
            Description = "     "
        };

        // Assert
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
    }

    [Test]
    public void Description_ShouldBeSetToNewValue_WhenProvidedValueIsValid()
    {
        // Arrange
        const string newDesc = "HelloThere!";

        // Act
        var result = new FileScriptCommandBase(null, null, RuntimeConfig)
        {
            Description = newDesc
        };

        // Assert
        result.Description.Should().Be(newDesc);
    }
    #endregion

    #region Constructor Tests
    [Test]
    public void FileScriptCommandBase_ShouldProperlyInitialize_WhenProvidedNullValues()
    {
        // Act
        var result = new FileScriptCommandBase(null, null, null);

        // Assert
        result.Command.Should().BeEmpty();
        result.Aliases.Should().BeNull();
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        result.SanitizeResponse.Should().BeTrue();
        result.Parent.Should().BeNull();
        result.Config.Should().NotBeNull();
    }

    [Test]
    public void FileScriptCommandBase_ShouldProperlyInitialize_WhenGoldFlow()
    {
        // Arrange
        var fileScriptParentMock = new Mock<IFileScriptCommandParent>(MockBehavior.Strict);

        // Act
        var result = new FileScriptCommandBase(TestCommand, fileScriptParentMock.Object, RuntimeConfig);

        // Assert
        result.Command.Should().Be(TestCommand);
        result.Aliases.Should().BeNull();
        result.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        result.SanitizeResponse.Should().BeTrue();
        result.Parent.Should().Be(fileScriptParentMock.Object);
        result.Config.Should().Be(RuntimeConfig);
        fileScriptParentMock.VerifyAll();
    }
    #endregion

    #region Execute Tests
    [Test]
    public void Execute_ShouldFail_WhenConcurrentExecutionsLimitIsExceeded()
    {
        // Arrange
        var cmd = new FileScriptCommandBase(null, null, new(RuntimeConfig.FileSystemHelper, RuntimeConfig.PermissionsResolver, 0));

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Script execution terminated due to exceeded concurrent executions limit");
    }

    [Test]
    public void Execute_ShouldFail_WhenReadFileThrows()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.ReadFile(TestPath)).Throws<Exception>();
        var cmd = new FileScriptCommandBase(TestCommand, null, FromFilesMock(fileSystemMock));

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Cannot read script from file 'test.slcs'");
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Execute_ShouldParentLocationBeUsed_WhenNotNull()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.ReadFile($"parent{Path.DirectorySeparatorChar}test.slcs")).Throws<Exception>();
        var scriptParentMock = new Mock<IFileScriptCommandParent>(MockBehavior.Strict);
        scriptParentMock.Setup(x => x.GetLocation(true)).Returns("parent");
        var cmd = new FileScriptCommandBase(TestCommand, scriptParentMock.Object, FromFilesMock(fileSystemMock));

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be($"Cannot read script from file 'parent{Path.DirectorySeparatorChar}test.slcs'");
        fileSystemMock.VerifyAll();
        scriptParentMock.VerifyAll();
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Execute_ShouldFail_WhenScriptFails(string? src, string expectedError)
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.ReadFile(TestPath)).Returns(src!);
        var cmd = new FileScriptCommandBase(TestCommand, null, FromFilesMock(fileSystemMock));

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be(expectedError);
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Execute_ShouldSucceed_WhenGoldFlow(string src)
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.ReadFile(TestPath)).Returns(src);
        var cmd = new FileScriptCommandBase(TestCommand, null, FromFilesMock(fileSystemMock));

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeTrue();
        message.Should().Be("Script executed successfully.");
        fileSystemMock.VerifyAll();
    }
    #endregion
}
