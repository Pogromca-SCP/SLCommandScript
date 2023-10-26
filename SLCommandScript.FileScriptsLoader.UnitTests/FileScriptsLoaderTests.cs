using NUnit.Framework;
using System;
using PluginAPI.Loader.Features;
using SLCommandScript.FileScriptsLoader.Helpers;
using SLCommandScript.FileScriptsLoader.Commands;
using FluentAssertions;
using SLCommandScript.Core;
using SLCommandScript.Core.Permissions;
using Moq;
using PluginAPI.Enums;
using PluginAPI.Core.Attributes;
using SLCommandScript.Core.Interfaces;
using CommandSystem;

namespace SLCommandScript.FileScriptsLoader.UnitTests;

[TestFixture]
public class FileScriptsLoaderTests
{
    private static readonly PluginDirectory _testDirectory = new("./");

    private static readonly Type[] _emptyTypesArray = new Type[0];

    private static readonly Func<string, string, bool, IFileSystemWatcherHelper> _testWatcherFactory = (directory, filter, allowSubdirectories) => null;

    #region InitScriptsLoader Tests
    [Test]
    public void InitScriptsLoader_ShouldNotInitialize_WhenProvidedPluginObjectIsNull()
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        var loader = new FileScriptsLoader();

        // Act
        loader.InitScriptsLoader(null, null, null);

        // Assert
        HelpersProvider.FileSystemHelper.Should().BeNull();
        HelpersProvider.FileSystemWatcherHelperFactory.Should().BeNull();
        FileScriptCommandBase.PermissionsResolver.Should().BeNull();
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(0);
    }

    [Test]
    public void InitScriptsLoader_ShouldNotInitialize_WhenProvidedPluginHandlerIsNull()
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        var loader = new FileScriptsLoader();

        // Act
        loader.InitScriptsLoader(new(), null, null);

        // Assert
        HelpersProvider.FileSystemHelper.Should().BeNull();
        HelpersProvider.FileSystemWatcherHelperFactory.Should().BeNull();
        FileScriptCommandBase.PermissionsResolver.Should().BeNull();
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(0);
    }

    [TestCase(-9)]
    [TestCase(9)]
    [TestCase(1)]
    public void InitScriptsLoader_ShouldNotInitialize_WhenAnInstanceIsAlreadyInitialized(int execsLimit)
    {
        // Arrange
        var config = new ScriptsLoaderConfig()
        {
            CustomPermissionsResolver = null,
            ScriptExecutionsLimit = execsLimit,
            AllowedScriptCommandTypes = 0,
            EnableScriptEventHandlers = false
        };

        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        using var loader = new FileScriptsLoader();
        var plugin = new TestPlugin();
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), config);
        var fileSystemHelper = HelpersProvider.FileSystemHelper;
        var fileSystemWatcherFactory = HelpersProvider.FileSystemWatcherHelperFactory;
        var permissionsResolver = FileScriptCommandBase.PermissionsResolver;
        var executionsLimit = FileScriptCommandBase.ConcurrentExecutionsLimit;
        var secondLoader = new FileScriptsLoader();
        config.ScriptExecutionsLimit *= 2;

        // Act
        secondLoader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), config);

        // Assert
        HelpersProvider.FileSystemHelper.Should().Be(fileSystemHelper);
        HelpersProvider.FileSystemWatcherHelperFactory.Should().Be(fileSystemWatcherFactory);
        FileScriptCommandBase.PermissionsResolver.Should().Be(permissionsResolver);
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(executionsLimit);
    }

    [TestCase(3)]
    [TestCase(7)]
    [TestCase(0)]
    public void InitScriptsLoader_ShouldInitialize_WhenNoDirectoriesAreEnabled(int execsLimit)
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        using var loader = new FileScriptsLoader();
        var plugin = new TestPlugin();

        // Act
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), new()
        {
            CustomPermissionsResolver = "xd",
            ScriptExecutionsLimit = execsLimit,
            AllowedScriptCommandTypes = 0,
            EnableScriptEventHandlers = false
        });

        // Assert
        HelpersProvider.FileSystemHelper.Should().NotBeNull();
        HelpersProvider.FileSystemWatcherHelperFactory.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.GetType().Should().Be(typeof(VanillaPermissionsResolver));
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(execsLimit);
    }

    [TestCase(2)]
    [TestCase(1)]
    [TestCase(-8)]
    public void InitScriptsLoader_ShouldLoadCustomPermissionsLoader(int execsLimit)
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        using var loader = new FileScriptsLoader();
        var plugin = new TestPlugin();

        // Act
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), new()
        {
            CustomPermissionsResolver = typeof(CustomResolver).AssemblyQualifiedName,
            ScriptExecutionsLimit = execsLimit,
            AllowedScriptCommandTypes = 0,
            EnableScriptEventHandlers = false
        });

        // Assert
        HelpersProvider.FileSystemHelper.Should().NotBeNull();
        HelpersProvider.FileSystemWatcherHelperFactory.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.GetType().Should().Be(typeof(CustomResolver));
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(execsLimit);
    }

    [Test]
    public void InitScriptsLoader_ShouldInitialize_WhenEventsAreEnabled()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.FileSystemWatcherHelperFactory = _testWatcherFactory;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        using var loader = new FileScriptsLoader();
        var plugin = new TestPlugin();

        // Act
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), new()
        {
            CustomPermissionsResolver = null,
            ScriptExecutionsLimit = 0,
            AllowedScriptCommandTypes = 0,
            EnableScriptEventHandlers = true
        });

        // Assert
        HelpersProvider.FileSystemHelper.Should().Be(fileSystemMock.Object);
        HelpersProvider.FileSystemWatcherHelperFactory.Should().Be(_testWatcherFactory);
        FileScriptCommandBase.PermissionsResolver.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.GetType().Should().Be(typeof(VanillaPermissionsResolver));
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(0);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCase(CommandType.RemoteAdmin)]
    [TestCase(CommandType.Console)]
    [TestCase(CommandType.GameConsole)]
    [TestCase(CommandType.RemoteAdmin | CommandType.Console)]
    public void InitScriptsLoader_ShouldInitialize_WhenCommandsAreEnabled(CommandType type)
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.FileSystemWatcherHelperFactory = _testWatcherFactory;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        using var loader = new FileScriptsLoader();
        var plugin = new TestPlugin();

        // Act
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), new()
        {
            CustomPermissionsResolver = null,
            ScriptExecutionsLimit = 0,
            AllowedScriptCommandTypes = type,
            EnableScriptEventHandlers = false
        });

        // Assert
        HelpersProvider.FileSystemHelper.Should().Be(fileSystemMock.Object);
        HelpersProvider.FileSystemWatcherHelperFactory.Should().Be(_testWatcherFactory);
        FileScriptCommandBase.PermissionsResolver.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.GetType().Should().Be(typeof(VanillaPermissionsResolver));
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(0);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void InitScriptsLoader_ShouldInitialize_WhenLoaderConfigIsNull()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);
        fileSystemMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.FileSystemWatcherHelperFactory = _testWatcherFactory;
        FileScriptCommandBase.PermissionsResolver = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        using var loader = new FileScriptsLoader();
        var plugin = new TestPlugin();

        // Act
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), null);

        // Assert
        HelpersProvider.FileSystemHelper.Should().Be(fileSystemMock.Object);
        HelpersProvider.FileSystemWatcherHelperFactory.Should().Be(_testWatcherFactory);
        FileScriptCommandBase.PermissionsResolver.Should().NotBeNull();
        FileScriptCommandBase.PermissionsResolver.GetType().Should().Be(typeof(VanillaPermissionsResolver));
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(10);
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Dispose Tests
    [Test]
    public void Dispose_ShouldProperlyCleanupResources()
    {
        // Arrange
        var plugin = new TestPlugin();
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.FileSystemWatcherHelperFactory = _testWatcherFactory;
        var loader = new FileScriptsLoader();
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), null);

        // Act
        loader.Dispose();

        // Assert
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(0);
        FileScriptCommandBase.PermissionsResolver.Should().BeNull();
        HelpersProvider.FileSystemHelper.Should().BeNull();
        HelpersProvider.FileSystemWatcherHelperFactory.Should().BeNull();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Dispose_ShouldNotCleanupHelpers_WhenAnInstanceIsStillActive()
    {
        // Arrange
        var plugin = new TestPlugin();
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.FileSystemWatcherHelperFactory = _testWatcherFactory;
        using var loader = new FileScriptsLoader();
        loader.InitScriptsLoader(plugin, new(_testDirectory, plugin, plugin.GetType(), _emptyTypesArray), null);
        var secondLoader = new FileScriptsLoader();

        // Act
        secondLoader.Dispose();

        // Assert
        FileScriptCommandBase.ConcurrentExecutionsLimit.Should().Be(10);
        FileScriptCommandBase.PermissionsResolver.Should().NotBeNull();
        HelpersProvider.FileSystemHelper.Should().NotBeNull();
        HelpersProvider.FileSystemWatcherHelperFactory.Should().NotBeNull();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion
}

public class TestPlugin
{
    [PluginEntryPoint("TestPlugin", "1.0.0", "Plugin for testing purposes only", "Test")]
    void Load() {}
}

public class CustomResolver : IPermissionsResolver
{
    public bool CheckPermission(ICommandSender sender, string permission, out string message)
    {
        message = null;
        return false;
    }
}
