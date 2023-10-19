using NUnit.Framework;
using PluginAPI.Enums;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SLCommandScript.FileScriptsLoader.Helpers;
using System.IO;
using SLCommandScript.FileScriptsLoader.Loader;
using FluentAssertions;
using SLCommandScript.FileScriptsLoader.Events;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Loader;

[TestFixture]
public class EventsDirectoryTests
{
    #region Tests Utils
    private const string _testDirectory = "eventsTest";

    private static readonly string[] _invalidEvents = { string.Empty, "hello", "there" };

    private static readonly string[] _validEvents = { $"{ServerEventType.PlayerDeath}", $"On{ServerEventType.Scp079CameraChanged}", $"on{ServerEventType.PlaceBlood}" };

    private static readonly ServerEventType[] _eventTypes = { ServerEventType.PlayerDeath, ServerEventType.Scp079CameraChanged, ServerEventType.PlaceBlood };

    private static IEnumerable<object[]> InvalidEvents = JoinArrays(_invalidEvents, _eventTypes);

    private static IEnumerable<object[]> ValidEvents = JoinArrays(_validEvents, _eventTypes);

    private static IEnumerable<object[]> JoinArrays(string[] names, ServerEventType[] types) => names.Select((n, index) => new object[] { n, types[index] });

    private static Mock<IFileSystemWatcherHelper> MakeWatcherMock()
    {
        var watcherMock = new Mock<IFileSystemWatcherHelper>(MockBehavior.Strict);
        watcherMock.Setup(x => x.Directory).Returns(_testDirectory);
        watcherMock.SetupAdd(x => x.Created += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Deleted += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Renamed += It.IsAny<RenamedEventHandler>());
        return watcherMock;
    }

    private static Mock<IFileSystemHelper> MakeFilesHelper(string[] foundFiles)
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.EnumerateFiles(_testDirectory, EventsDirectory.ScriptFilesFilter, SearchOption.TopDirectoryOnly)).Returns(foundFiles);
        return fileSystemMock;
    }
    #endregion

    #region Constructor Tests
    [TestCase(null)]
    [TestCase(3)]
    public void EventsDirectory_ShouldNotInitialize_WhenProvidedWatcherIsNull(object plugin)
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.PluginHelper = null;

        // Act
        var result = new EventsDirectory(plugin, null);

        // Assert
        result.PluginObject.Should().Be(plugin);
        result.Handler.EventScripts.Should().BeEmpty();
        result.Watcher.Should().BeNull();
    }

    [Test]
    public void EventsDirectory_ShouldNotRegisterEvents_WhenProvidedPluginObjectIsNull()
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;

        // Act
        var result = new EventsDirectory(null, watcherMock.Object);

        // Assert
        result.PluginObject.Should().BeNull();
        result.Handler.EventScripts.Should().BeEmpty();
        result.Watcher.Should().Be(watcherMock.Object);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCase(3)]
    [TestCase(7)]
    public void EventsDirectory_ShouldProperlyInitialize_WhenNoFilesExist(object plugin)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var pluginMock = new Mock<IPluginHelper>(MockBehavior.Strict);
        pluginMock.Setup(x => x.RegisterEvents(plugin, It.IsAny<FileScriptsEventHandler>()));
        HelpersProvider.PluginHelper = pluginMock.Object;

        // Act
        var result = new EventsDirectory(plugin, watcherMock.Object);

        // Assert
        result.PluginObject.Should().Be(plugin);
        result.Handler.EventScripts.Should().BeEmpty();
        result.Watcher.Should().Be(watcherMock.Object);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
        pluginMock.VerifyAll();
        pluginMock.VerifyNoOtherCalls();
    }

    [TestCase(-1)]
    [TestCase(4)]
    public void EventsDirectory_ShouldProperlyInitialize_WhenFilesExist(object plugin)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new[] { "panabe", "xd", "bad" });
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("panabe")).Returns(ServerEventType.ItemSpawned.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("xd")).Returns(ServerEventType.MapGenerated.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("bad")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var pluginMock = new Mock<IPluginHelper>(MockBehavior.Strict);
        pluginMock.Setup(x => x.RegisterEvents(plugin, It.IsAny<FileScriptsEventHandler>()));
        HelpersProvider.PluginHelper = pluginMock.Object;

        // Act
        var result = new EventsDirectory(plugin, watcherMock.Object);

        // Assert
        result.PluginObject.Should().Be(plugin);
        result.Handler.EventScripts.Should().HaveCount(2);
        result.Watcher.Should().Be(watcherMock.Object);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
        pluginMock.VerifyAll();
        pluginMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Dispose Tests
    [Test]
    public void Dispose_ShouldDoNothing_WhenPropertiesAreNull()
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, null);

        // Act
        dir.Dispose();

        // Assert
        dir.PluginObject.Should().BeNull();
        dir.Handler.EventScripts.Should().BeEmpty();
        dir.Watcher.Should().BeNull();
    }

    [TestCase(-10)]
    [TestCase(-3)]
    public void Dispose_ShouldUnregisterEvents_WhenWatcherIsNull(object plugin)
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;
        var pluginMock = new Mock<IPluginHelper>(MockBehavior.Strict);
        pluginMock.Setup(x => x.UnregisterEvents(plugin, It.IsAny<FileScriptsEventHandler>()));
        HelpersProvider.PluginHelper = pluginMock.Object;
        var dir = new EventsDirectory(plugin, null);

        // Act
        dir.Dispose();

        // Assert
        dir.PluginObject.Should().Be(plugin);
        dir.Handler.EventScripts.Should().BeEmpty();
        dir.Watcher.Should().BeNull();
        pluginMock.VerifyAll();
        pluginMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Dispose_ShouldDisposeWatcher_WhenPluginObjectIsNull()
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        watcherMock.Setup(x => x.Dispose());
        var fileSystemMock = MakeFilesHelper(new string[0]);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);

        // Act
        dir.Dispose();

        // Assert
        dir.PluginObject.Should().BeNull();
        dir.Handler.EventScripts.Should().BeEmpty();
        dir.Watcher.Should().Be(watcherMock.Object);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCase(0)]
    [TestCase(22)]
    public void Dispose_ShouldCleanupResources_WhenGoldFlow(object plugin)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        watcherMock.Setup(x => x.Dispose());
        var fileSystemMock = MakeFilesHelper(new[] { "panabe", "xd", "bad" });
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("panabe")).Returns(ServerEventType.ItemSpawned.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("xd")).Returns(ServerEventType.MapGenerated.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("bad")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var pluginMock = new Mock<IPluginHelper>(MockBehavior.Strict);
        pluginMock.Setup(x => x.RegisterEvents(plugin, It.IsAny<FileScriptsEventHandler>()));
        pluginMock.Setup(x => x.UnregisterEvents(plugin, It.IsAny<FileScriptsEventHandler>()));
        HelpersProvider.PluginHelper = pluginMock.Object;
        var dir = new EventsDirectory(plugin, watcherMock.Object);

        // Act
        dir.Dispose();

        // Assert
        dir.PluginObject.Should().Be(plugin);
        dir.Handler.EventScripts.Should().HaveCount(2);
        dir.Watcher.Should().Be(watcherMock.Object);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
        pluginMock.VerifyAll();
        pluginMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Created Tests
    [TestCaseSource(nameof(_invalidEvents))]
    public void Created_ShouldNotRegisterEvent_WhenEventNameIsInvalid(string name)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Created_ShouldRegisterEvent_WhenEventNameIsValid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Command.Should().Be(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Created_ShouldReplaceEvent_WhenEventIsAlreadyRegistered(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Command.Should().Be(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Deleted Tests
    [TestCaseSource(nameof(InvalidEvents))]
    public void Deleted_ShouldNotUnregisterEvent_WhenEventNameIsInvalid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Should().BeNull();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Deleted_ShouldUnregisterEvent_WhenEventNameIsValid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validEvents))]
    public void Deleted_ShouldNotUnregisterEvent_WhenEventIsNotRegistered(string name)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Renamed Tests
    [TestCaseSource(nameof(InvalidEvents))]
    public void Renamed_ShouldDoNothing_WhenEventNameIsInvalid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, name));

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Should().BeNull();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Renamed_ShouldReplaceEvent_WhenEventNameIsValid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, name));

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Command.Should().Be(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Renamed_ShouldUnregisterEvent_WhenNewEventNameIsInvalid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\XD{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, $"XD{name}", name));

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Renamed_ShouldAddNewEvent_WhenOldEventNameIsInvalid(string name, ServerEventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\XD{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        HelpersProvider.PluginHelper = null;
        var dir = new EventsDirectory(null, watcherMock.Object);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, $"XD{name}"));

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Command.Should().Be(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion
}
