using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Permissions;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class FileScriptCommandTests : TestWithConfigBase
{
    #region Usage Tests
    [Test]
    public void Usage_ShouldBeSetToNull_WhenProvidedValueIsNull()
    {
        // Act
        var result = new FileScriptCommand(null, null, RuntimeConfig)
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
        var result = new FileScriptCommand(null, null, RuntimeConfig)
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
        var result = new FileScriptCommand(null, null, RuntimeConfig)
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
        var result = new FileScriptCommand(null, null, RuntimeConfig)
        {
            Usage = usage
        };

        // Assert
        result.Usage.Should().BeEquivalentTo(usage);
    }
    #endregion

    #region Execute Tests
    [Test]
    public void Execute_ShouldFail_WhenPermissionCheckFails()
    {
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        var message = "bottom text";
        resolverMock.Setup(x => x.CheckPermission(null, "Noclip", out message)).Returns(true);

        var cmd = new FileScriptCommand(null, null, new(RuntimeConfig.FileSystemHelper, resolverMock.Object, 10))
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), null, out message);

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
        resolverMock.Setup(x => x.CheckPermission(null, "Noclip", out message)).Returns(false);

        var cmd = new FileScriptCommand(null, null, new(RuntimeConfig.FileSystemHelper, resolverMock.Object, 10))
        {
            RequiredPermissions = ["Noclip"]
        };

        // Act
        var result = cmd.Execute(new(), null, out message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Missing permission: 'Noclip'. Access denied");
        resolverMock.VerifyAll();
    }

    [Test]
    public void Execute_ShouldFail_WhenNotEnoughArgumentsAreProvided()
    {
        var cmd = new FileScriptCommand(null, null, RuntimeConfig)
        {
            Arity = 1
        };

        // Act
        var result = cmd.Execute(new(), null, out var message);

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
        var cmd = new FileScriptCommand(null, null, FromFilesMock(fileSystemMock));

        // Act
        var result = cmd.Execute(new(), null, out var message);

        // Assert
        result.Should().BeTrue();
        message.Should().Be("Script executed successfully.");
        fileSystemMock.VerifyAll();
    }
    #endregion
}
