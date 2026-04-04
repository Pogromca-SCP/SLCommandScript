using AwesomeAssertions;
using CommandSystem;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Permissions;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptCommandTests : TestWithConfigBase
{
    private const string TestCommand = "test";

    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueIsNull()
    {
        // Act
        var result = new FileScriptCommand(TestCommand, null, RuntimeConfig)
        {
            Usage = null
        };

        // Assert
        result.Usage.Should().BeNull();
    }

    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueIsEmptyArray()
    {
        // Act
        var result = new FileScriptCommand(TestCommand, null, RuntimeConfig)
        {
            Usage = []
        };

        // Assert
        result.Usage.Should().BeNull();
    }

    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueHasOnlyBlankEntries()
    {
        // Act
        var result = new FileScriptCommand(TestCommand, null, RuntimeConfig)
        {
            Usage = ["", "       ", null!, "\t\t"],
        };

        // Assert
        result.Usage.Should().BeNull();
    }

    [Test]
    public void Usage_ShouldBeSetToNewValue_WhenProvidedValueIsValid()
    {
        var usage = new[] { "Option", "Args..." };

        // Act
        var result = new FileScriptCommand(TestCommand, null, RuntimeConfig)
        {
            Usage = usage
        };

        // Assert
        result.Usage.Should().BeEquivalentTo(usage);
    }

    [Test]
    public void Execute_ShouldSkipPermissionsCheck_WhenCommandSenderIsNull()
    {
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        var cmd = new FileScriptCommand(TestCommand, null, new(RuntimeConfig.FileSystemHelper, resolverMock.Object, 10))
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Cannot execute script without a command sender");
        resolverMock.VerifyAll();
    }

    [Test]
    public void Execute_ShouldFail_WhenPermissionCheckFails()
    {
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        var message = "bottom text";
        var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
        resolverMock.Setup(x => x.CheckPermission(senderMock.Object, "Noclip", out message)).Returns(true);

        var cmd = new FileScriptCommand(TestCommand, null, new(RuntimeConfig.FileSystemHelper, resolverMock.Object, 10))
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), senderMock.Object, out message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("bottom text");
        resolverMock.VerifyAll();
    }

    [Test]
    public void Execute_ShouldFail_WhenSenderIsMissingRequiredPermission()
    {
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        string? message = null;
        var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
        resolverMock.Setup(x => x.CheckPermission(senderMock.Object, "Noclip", out message)).Returns(false);

        var cmd = new FileScriptCommand(TestCommand, null, new(RuntimeConfig.FileSystemHelper, resolverMock.Object, 10))
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), senderMock.Object, out message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Missing permission: 'Noclip'. Access denied");
        resolverMock.VerifyAll();
    }

    [Test]
    public void Execute_ShouldFail_WhenNotEnoughArgumentsAreProvided()
    {
        var cmd = new FileScriptCommand(TestCommand, null, RuntimeConfig)
        {
            Arity = 1
        };

        var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);

        // Act
        var result = cmd.Execute(new(), senderMock.Object, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Missing argument: script expected 1 arguments, but sender provided 0");
    }

    [Test]
    public void Execute_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.ReadFile(".slcs")).Returns(string.Empty);
        var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
        var cmd = new FileScriptCommand(TestCommand, null, FromFilesMock(fileSystemMock));

        // Act
        var result = cmd.Execute(new(), senderMock.Object, out var message);

        // Assert
        result.Should().BeTrue();
        message.Should().Be("Script executed successfully.");
        fileSystemMock.VerifyAll();
    }
}
