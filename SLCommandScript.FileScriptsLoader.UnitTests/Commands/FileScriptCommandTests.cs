using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Permissions;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptCommandTests
{
    [TearDown]
    public void TearDown()
    {
        HelpersProvider.FileSystemHelper = null;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
    }

    #region Usage Tests
    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueIsNull()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new FileScriptCommand(null, null)
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
        var result = new FileScriptCommand(null, null)
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
        var result = new FileScriptCommand(null, null)
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
        var result = new FileScriptCommand(null, null)
        {
            Usage = usage
        };

        // Assert
        result.Usage.Should().BeEquivalentTo(usage);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Execute Tests
    [Test]
    public void Execute_ShouldFail_WhenPermissionCheckFails()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        var message = "bottom text";
        resolverMock.Setup(x => x.CheckPermission(null, "Noclip", out message)).Returns(true);
        FileScriptCommandBase.PermissionsResolver = resolverMock.Object;

        var cmd = new FileScriptCommand(null, null)
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), null, out message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("bottom text");
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
        resolverMock.VerifyAll();
        resolverMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Execute_ShouldFail_WhenSenderIsMissingRequiredPermission()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        string message = null;
        resolverMock.Setup(x => x.CheckPermission(null, "Noclip", out message)).Returns(false);
        FileScriptCommandBase.PermissionsResolver = resolverMock.Object;

        var cmd = new FileScriptCommand(null, null)
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), null, out message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Missing permission: 'Noclip'. Access denied");
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
        resolverMock.VerifyAll();
        resolverMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Execute_ShouldFail_WhenNotEnoughArgumentsAreProvided()
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(null)).Returns("test");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        var cmd = new FileScriptCommand(null, null)
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
        fileSystemMock.Setup(x => x.ReadFile(string.Empty)).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 1;
        var cmd = new FileScriptCommand(null, null);

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
