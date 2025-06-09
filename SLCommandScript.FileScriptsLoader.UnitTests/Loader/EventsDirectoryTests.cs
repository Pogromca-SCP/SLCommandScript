using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Events;
using SLCommandScript.FileScriptsLoader.Helpers;
using SLCommandScript.FileScriptsLoader.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Loader;

[TestFixture]
public class EventsDirectoryTests : TestWithConfigBase
{
    private const string TestDirectory = "eventsTest";

    private static readonly string[] _invalidEvents = [string.Empty, "hello", "there"];

    private static readonly string[] _validEvents = [$"{EventType.PlayerDeath}", $"On{EventType.MapGenerate}", $"on{EventType.PlaceBlood}"];

    private static readonly EventType[] _eventTypes = [EventType.PlayerDeath, EventType.MapGenerate, EventType.PlaceBlood];

    private static IEnumerable<object[]> InvalidEvents => JoinArrays(_invalidEvents, _eventTypes);

    private static IEnumerable<object[]> ValidEvents => JoinArrays(_validEvents, _eventTypes);

    private static IEnumerable<object[]> JoinArrays(string[] names, EventType[] types) => names.Select((n, index) => new object[] { n, types[index] });

    private static Mock<IFileSystemWatcherHelper> MakeWatcherMock()
    {
        var watcherMock = new Mock<IFileSystemWatcherHelper>(MockBehavior.Strict);
        watcherMock.Setup(x => x.Directory).Returns(TestDirectory);
        watcherMock.SetupAdd(x => x.Created += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Deleted += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Renamed += It.IsAny<RenamedEventHandler>());
        watcherMock.SetupAdd(x => x.Error += It.IsAny<ErrorEventHandler>());
        watcherMock.Setup(x => x.RegisterEvents(It.IsAny<FileScriptsEventHandler>()));
        return watcherMock;
    }

    private static Mock<IFileSystemHelper> MakeFilesHelper(string[] foundFiles)
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.EnumerateFiles(TestDirectory, EventsDirectory.ScriptFilesFilter, SearchOption.TopDirectoryOnly)).Returns(foundFiles);
        return fileSystemMock;
    }

    private static Mock<IFileSystemHelper> MakeFilesHelper(string name)
    {
        var fileSystemMock = MakeFilesHelper([]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{TestDirectory}{Path.DirectorySeparatorChar}{name}")).Returns(name);
        return fileSystemMock;
    }

    private static EventsDirectory MakeSupressed(IFileSystemWatcherHelper? watcher, RuntimeConfig? config)
    {
        var dir = new EventsDirectory(watcher, config);
        GC.SuppressFinalize(dir);
        return dir;
    }

    private static void RaiseCreate(Mock<IFileSystemWatcherHelper> watcherMock, string name) =>
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, TestDirectory, name));

    private static void RaiseDelete(Mock<IFileSystemWatcherHelper> watcherMock, string name) =>
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, TestDirectory, name));

    private static void RaiseRename(Mock<IFileSystemWatcherHelper> watcherMock, string name, string oldName) =>
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, TestDirectory, name, oldName));

    private EventsDirectory MakeSupressed(IFileSystemWatcherHelper watcher, Mock<IFileSystemHelper> fileSystemMock)
    {
        var dir = new EventsDirectory(watcher, FromFilesMock(fileSystemMock));
        GC.SuppressFinalize(dir);
        return dir;
    }

    [Test]
    public void EventsDirectory_ShouldNotInitialize_WhenProvidedWatcherIsNull()
    {
        // Act
        var result = EventsDirectoryTests.MakeSupressed(null, null);

        // Assert
        result.Handler.EventScripts.Should().BeEmpty();
        result.Watcher.Should().BeNull();
        result.Config.Should().NotBeNull();
    }

    [Test]
    public void EventsDirectory_ShouldProperlyInitialize_WhenNoFilesExist()
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([]);
        var config = FromFilesMock(fileSystemMock);

        // Act
        var result = MakeSupressed(watcherMock.Object, config);

        // Assert
        result.Handler.EventScripts.Should().BeEmpty();
        result.Watcher.Should().Be(watcherMock.Object);
        result.Config.Should().Be(config);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void EventsDirectory_ShouldProperlyInitialize_WhenFilesExist()
    {
        // Arrange
        var path1 = $"{TestDirectory}/panabe";
        var path2 = $"{TestDirectory}/xd";
        var path3 = "bad";
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([path1, path2, path3]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path1)).Returns(EventType.ItemSpawn.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path2)).Returns(EventType.MapGenerate.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path3)).Returns(string.Empty);
        var config = FromFilesMock(fileSystemMock);

        // Act
        var result = MakeSupressed(watcherMock.Object, config);

        // Assert
        result.Handler.EventScripts.Should().HaveCount(2);
        result.Watcher.Should().Be(watcherMock.Object);
        result.Config.Should().Be(config);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Dispose_ShouldDoNothing_WhenPropertiesAreNull()
    {
        // Arrange
        var dir = EventsDirectoryTests.MakeSupressed(null, null);

        // Act
        dir.Dispose();

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        dir.Watcher.Should().BeNull();
        dir.Config.Should().NotBeNull();
    }

    [Test]
    public void Dispose_ShouldDoNothing_WhenWatcherIsNull()
    {
        // Arrange
        var dir = MakeSupressed(null, RuntimeConfig);

        // Act
        dir.Dispose();

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        dir.Watcher.Should().BeNull();
        dir.Config.Should().Be(RuntimeConfig);
    }

    [Test]
    public void Dispose_ShouldCleanupResources_WhenGoldFlow()
    {
        // Arrange
        var path1 = $"{TestDirectory}/panabe";
        var path2 = $"{TestDirectory}/xd";
        var path3 = "bad";
        var watcherMock = MakeWatcherMock();
        watcherMock.Setup(x => x.UnregisterEvents(It.IsAny<FileScriptsEventHandler>()));
        watcherMock.Setup(x => x.Dispose());
        var fileSystemMock = MakeFilesHelper([path1, path2, path3]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path1)).Returns(EventType.ItemSpawn.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path2)).Returns(EventType.MapGenerate.ToString());
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path3)).Returns(string.Empty);
        var config = FromFilesMock(fileSystemMock);
        var dir = MakeSupressed(watcherMock.Object, config);

        // Act
        dir.Dispose();

        // Assert
        dir.Handler.EventScripts.Should().HaveCount(2);
        dir.Watcher.Should().Be(watcherMock.Object);
        dir.Config.Should().Be(config);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void GetLocation_ShouldReturnEmptyString_WhenWatcherIsNull([Values] bool includeRoot)
    {
        // Arrange
        var dir = MakeSupressed(null, RuntimeConfig);

        // Act
        var result = dir.GetLocation(includeRoot);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetLocation_ShouldReturnEmptyString_WhenIncludeRootIsFalse()
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([]);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);

        // Act
        var result = dir.GetLocation();

        // Assert
        result.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void GetLocation_ShouldReturnEmptyString_WhenIncludeRootIsTrue()
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([]);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);

        // Act
        var result = dir.GetLocation(true);

        // Assert
        result.Should().Be(TestDirectory);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidEvents))]
    public void Created_ShouldNotRegisterEvent_WhenEventNameIsInvalid(string name)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, name);

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Created_ShouldRegisterEvent_WhenEventNameIsValid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, name);

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key]!.Command.Should().Be(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Created_ShouldReplaceEvent_WhenEventIsAlreadyRegistered(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        RaiseCreate(watcherMock, name);

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key]!.Command.Should().Be(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(InvalidEvents))]
    public void Deleted_ShouldNotUnregisterEvent_WhenEventNameIsInvalid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        RaiseDelete(watcherMock, name);

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Should().BeNull();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Deleted_ShouldUnregisterEvent_WhenEventNameIsValid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        RaiseDelete(watcherMock, name);

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validEvents))]
    public void Deleted_ShouldNotUnregisterEvent_WhenEventIsNotRegistered(string name)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);

        // Act
        RaiseDelete(watcherMock, name);

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(InvalidEvents))]
    public void Renamed_ShouldDoNothing_WhenEventNameIsInvalid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        RaiseRename(watcherMock, name, name);

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key].Should().BeNull();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Renamed_ShouldReplaceEvent_WhenEventNameIsValid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(name);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        RaiseRename(watcherMock, name, name);

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key]!.Command.Should().Be(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Renamed_ShouldUnregisterEvent_WhenNewEventNameIsInvalid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{TestDirectory}{Path.DirectorySeparatorChar}{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{TestDirectory}{Path.DirectorySeparatorChar}XD{name}")).Returns(string.Empty);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);
        dir.Handler.EventScripts.Add(key, null);

        // Act
        RaiseRename(watcherMock, $"XD{name}", name);

        // Assert
        dir.Handler.EventScripts.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidEvents))]
    public void Renamed_ShouldAddNewEvent_WhenOldEventNameIsInvalid(string name, EventType key)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{TestDirectory}{Path.DirectorySeparatorChar}{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{TestDirectory}{Path.DirectorySeparatorChar}XD{name}")).Returns(string.Empty);
        var dir = MakeSupressed(watcherMock.Object, fileSystemMock);

        // Act
        RaiseRename(watcherMock, name, $"XD{name}");

        // Assert
        dir.Handler.EventScripts.Should().ContainKey(key);
        dir.Handler.EventScripts[key]!.Command.Should().Be(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
}
