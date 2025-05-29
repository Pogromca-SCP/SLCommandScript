using CommandSystem;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RemoteAdmin;
using SLCommandScript.Core.Commands;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;
using SLCommandScript.FileScriptsLoader.Loader;
using SLCommandScript.TestUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Loader;

[TestFixture]
public partial class CommandsDirectoryTests : TestWithConfigBase
{
    private const string TestDirectory = "commandsTest";

    private const string TestParent = "parentTest";

    private const string TestCommand = "test";

    private const CommandType TestType = CommandType.RemoteAdmin;

    private static readonly string[] _invalidCommands = [string.Empty, "     ", "\t \t"];

    private static readonly string[] _validCommands = ["example", "bull", "script"];

    private static readonly CommandType[] _handlerTypes = [CommandType.RemoteAdmin, CommandType.Console, CommandType.Client];

    private static readonly CommandType[] _validTypes = [CommandType.RemoteAdmin, CommandType.Client];

    private static IEnumerable<object[]> InvalidCommandsXTypes => TestArrays.CartesianJoin(_invalidCommands, _validTypes);

    private static IEnumerable<object[]> ValidCommandsXTypes => TestArrays.CartesianJoin(_validCommands, _validTypes);

    private static Mock<IFileSystemWatcherHelper> MakeWatcherMock()
    {
        var watcherMock = new Mock<IFileSystemWatcherHelper>(MockBehavior.Strict);
        watcherMock.Setup(x => x.Directory).Returns(_testDirectory);
        watcherMock.SetupAdd(x => x.Created += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Changed += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Deleted += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Renamed += It.IsAny<RenamedEventHandler>());
        watcherMock.SetupAdd(x => x.Error += It.IsAny<ErrorEventHandler>());
        return watcherMock;
    }

    private static Mock<IFileSystemHelper> MakeFilesHelper(string[] foundFolders, string[] foundScripts, string[] foundJsons)
    {
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.EnumerateDirectories(_testDirectory)).Returns(foundFolders);
        fileSystemMock.Setup(x => x.EnumerateFiles(_testDirectory, EventsDirectory.ScriptFilesFilter, SearchOption.AllDirectories)).Returns(foundScripts);
        fileSystemMock.Setup(x => x.EnumerateFiles(_testDirectory, CommandsDirectory.DescriptionFilesFilter, SearchOption.AllDirectories)).Returns(foundJsons);
        return fileSystemMock;
    }

    private static ICommandHandler GetCommandHandler(CommandType type) => type switch
    {
        CommandType.RemoteAdmin => CommandProcessor.RemoteAdminCommandHandler,
        CommandType.Client => QueryProcessor.DotCommandHandler,
        _ => null!,
    };

    private static CommandsDirectory MakeSupressed(IFileSystemWatcherHelper? watcher, CommandType handlerType, RuntimeConfig? config)
    {
        var dir = new CommandsDirectory(watcher, handlerType, config);
        GC.SuppressFinalize(dir);
        return dir;
    }

    private static FileScriptDirectoryCommand AddParent(CommandsDirectory dir, string name = TestParent)
    {
        var parent = new FileScriptDirectoryCommand(name, dir);
        dir.Commands.Add(parent.Command, parent);
        return parent;
    }

    private static FileScriptDirectoryCommand AddParent(FileScriptDirectoryCommand parent, string name = TestParent)
    {
        var cmd = new FileScriptDirectoryCommand(name, parent);
        parent.RegisterCommand(cmd);
        return cmd;
    }

    private static FileScriptDirectoryCommand AddBadCommand(CommandsDirectory dir)
    {
        var cmd = new FileScriptDirectoryCommand(TestCommand, dir);
        dir.Commands.Add(cmd.Command, cmd);
        return cmd;
    }

    private static FileScriptDirectoryCommand AddBadCommand(FileScriptDirectoryCommand parent)
    {
        var cmd = new FileScriptDirectoryCommand(TestCommand, parent);
        parent.RegisterCommand(cmd);
        return cmd;
    }

    private static readonly string _testDirectory = $"{TestDirectory}{Path.DirectorySeparatorChar}";

    private readonly string _testParentPath = $"{_testDirectory}{TestParent}";

    private readonly CommandMetaData _emptyMetadata = new();

    private CommandsDirectory MakeSupressed(IFileSystemWatcherHelper watcher, CommandType handlerType, Mock<IFileSystemHelper> fileSystemMock)
    {
        var dir = new CommandsDirectory(watcher, handlerType, FromFilesMock(fileSystemMock));
        GC.SuppressFinalize(dir);
        return dir;
    }

    private string MakePath(bool withParent, string name) => $"{(withParent ? $"{_testParentPath}{Path.DirectorySeparatorChar}" : _testDirectory)}{name}";

    private Mock<IFileSystemHelper> SetupDirectoryCreate(bool withParent, string name)
    {
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(true);
        fileSystemMock.Setup(x => x.GetDirectory(path)).Returns(name);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupScriptCreate(bool withParent, string name)
    {
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path)).Returns(name);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupDescriptionCreate(bool withParent, string name)
    {
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(CommandsDirectory.ScriptDescriptionExtension);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupChange(bool withParent, string name)
    {
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(CommandsDirectory.ScriptDescriptionExtension);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupDirectoryDelete(bool withParent, string name)
    {
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(true);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupScriptDelete(bool withParent, string name)
    {
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(CommandsDirectory.ScriptFileExtension);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupDirectoryRename(bool withParent, bool withInit, string name, string oldName)
    {
        var path = MakePath(withParent, oldName);
        var fileSystemMock = MakeFilesHelper(withInit ? [path] : [], [], []);

        if (withInit)
        {
            fileSystemMock.Setup(x => x.GetDirectory(path)).Returns(oldName);
        }

        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(true);
        path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(true);
        fileSystemMock.Setup(x => x.GetDirectory(path)).Returns(name);
        return fileSystemMock;
    }

    private Mock<IFileSystemHelper> SetupScriptRename(bool withParent, bool withInit, string name, string oldName)
    {
        var path = MakePath(withParent, oldName);
        var fileSystemMock = MakeFilesHelper([], withInit ? [path] : [], []);

        if (withInit)
        {
            fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path)).Returns(oldName);
        }

        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(CommandsDirectory.ScriptFileExtension);
        path = MakePath(withParent, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(path)).Returns(name);
        return fileSystemMock;
    }

    private FileScriptCommand AddCommand(CommandsDirectory dir, string name = TestCommand)
    {
        var cmd = new FileScriptCommand(name, dir, RuntimeConfig);
        dir.Commands.Add(cmd.Command, cmd);
        return cmd;
    }

    private FileScriptCommand AddCommand(FileScriptDirectoryCommand parent, string name = TestCommand)
    {
        var cmd = new FileScriptCommand(name, parent, RuntimeConfig);
        parent.RegisterCommand(cmd);
        return cmd;
    }

    private void RaiseCreate(Mock<IFileSystemWatcherHelper> watcherMock, bool withParent, string name) =>
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, withParent ? _testParentPath : TestDirectory, name));

    private void RaiseChange(Mock<IFileSystemWatcherHelper> watcherMock, bool withParent, string name) =>
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, withParent ? _testParentPath : TestDirectory, name));

    private void RaiseDelete(Mock<IFileSystemWatcherHelper> watcherMock, bool withParent, string name) =>
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, withParent ? _testParentPath : TestDirectory, name));

    private void RaiseRename(Mock<IFileSystemWatcherHelper> watcherMock, bool withParent, string name, string oldName) =>
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, withParent ? _testParentPath : TestDirectory, name, oldName));

    [TestCaseSource(nameof(_handlerTypes))]
    public void CommandsDirectory_ShouldNotInitialize_WhenProvidedWatcherIsNull(CommandType type)
    {
        // Act
        var result = CommandsDirectoryTests.MakeSupressed(null, type, null);

        // Assert
        result.Commands.Should().BeEmpty();
        result.HandlerType.Should().Be(type);
        result.Watcher.Should().BeNull();
        result.Config.Should().NotBeNull();
    }

    [TestCaseSource(nameof(_handlerTypes))]
    public void CommandsDirectory_ShouldProperlyInitialize_WhenNoFilesExist(CommandType type)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper([], [], []);
        var config = FromFilesMock(fileSystemMock);

        // Act
        var result = MakeSupressed(watcherMock.Object, type, config);

        // Assert
        result.Commands.Should().BeEmpty();
        result.HandlerType.Should().Be(type);
        result.Watcher.Should().Be(watcherMock.Object);
        result.Config.Should().Be(config);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validTypes))]
    public void CommandsDirectory_ShouldProperlyInitialize_WhenFilesExist(CommandType type)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var folder1 = $"{_testDirectory}folder1";
        var folder2 = $"{_testDirectory}folder2";
        var globalFile = $"{_testDirectory}global";
        var innerFile = $"{folder1}{Path.DirectorySeparatorChar}inner";
        var fileSystemMock = MakeFilesHelper([folder1, folder2], [globalFile, innerFile], [globalFile, innerFile]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(globalFile)).Returns("global");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(innerFile)).Returns("inner");
        fileSystemMock.Setup(x => x.GetDirectory(folder1)).Returns("folder1");
        fileSystemMock.Setup(x => x.GetDirectory(folder2)).Returns("folder2");
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(It.IsAny<string>())).Returns(_emptyMetadata);
        var config = FromFilesMock(fileSystemMock);

        // Act
        var result = MakeSupressed(watcherMock.Object, type, config);

        // Assert
        result.Commands.Should().HaveCount(3);
        result.HandlerType.Should().Be(type);
        result.Watcher.Should().Be(watcherMock.Object);
        result.Config.Should().Be(config);
        var handler = GetCommandHandler(type);
        var commands = handler.AllCommands;
        commands.Should().Contain(c => c.Command.Equals("folder1"));
        commands.Should().Contain(c => c.Command.Equals("folder2"));
        commands.Should().Contain(c => c.Command.Equals("global"));
        commands.Should().NotContain(c => c.Command.Equals("inner"));
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();

        // Cleanup
        foreach (var cmd in result.Commands.Values)
        {
            CommandsUtils.UnregisterCommand(type, cmd);
        }
    }

    [TestCaseSource(nameof(_validTypes))]
    public void Dispose_ShouldUnregisterCommands_WhenWatcherIsNull(CommandType type)
    {
        // Arrange
        var dir = MakeSupressed(null, type, RuntimeConfig);
        var handler = GetCommandHandler(type);
        var f1 = new FileScriptDirectoryCommand("folder1", dir);
        f1.RegisterCommand(new FileScriptCommand("inner", f1, RuntimeConfig));
        handler.RegisterCommand(f1);
        dir.Commands.Add(f1.Command, f1);
        var f2 = new FileScriptDirectoryCommand("folder2", dir);
        handler.RegisterCommand(f2);
        dir.Commands.Add(f2.Command, f2);
        var cmd = new FileScriptCommand("global", dir, RuntimeConfig);
        handler.RegisterCommand(cmd);
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        dir.Dispose();

        // Assert
        dir.Commands.Should().HaveCount(3);
        dir.HandlerType.Should().Be(type);
        dir.Watcher.Should().BeNull();
        dir.Config.Should().Be(RuntimeConfig);
        var commands = handler.AllCommands;
        commands.Should().NotContain(c => c.Command.Equals("folder1"));
        commands.Should().NotContain(c => c.Command.Equals("folder2"));
        commands.Should().NotContain(c => c.Command.Equals("global"));
        commands.Should().NotContain(c => c.Command.Equals("inner"));
    }

    [TestCaseSource(nameof(_validTypes))]
    public void Dispose_ShouldCleanupResources_WhenGoldFlow(CommandType type)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        watcherMock.Setup(x => x.Dispose());
        var folder1 = $"{_testDirectory}folder1";
        var folder2 = $"{_testDirectory}folder2";
        var globalFile = $"{_testDirectory}global";
        var innerFile = $"{folder1}{Path.DirectorySeparatorChar}inner";
        var fileSystemMock = MakeFilesHelper([folder1, folder2], [globalFile, innerFile], [globalFile, innerFile]);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(globalFile)).Returns("global");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension(innerFile)).Returns("inner");
        fileSystemMock.Setup(x => x.GetDirectory(folder1)).Returns("folder1");
        fileSystemMock.Setup(x => x.GetDirectory(folder2)).Returns("folder2");
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(It.IsAny<string>())).Returns(_emptyMetadata);
        var config = FromFilesMock(fileSystemMock);
        var dir = MakeSupressed(watcherMock.Object, type, config);

        // Act
        dir.Dispose();

        // Assert
        dir.Commands.Should().HaveCount(3);
        dir.HandlerType.Should().Be(type);
        dir.Watcher.Should().Be(watcherMock.Object);
        dir.Config.Should().Be(config);
        var commands = GetCommandHandler(type).AllCommands;
        commands.Should().NotContain(c => c.Command.Equals("folder1"));
        commands.Should().NotContain(c => c.Command.Equals("folder2"));
        commands.Should().NotContain(c => c.Command.Equals("global"));
        commands.Should().NotContain(c => c.Command.Equals("inner"));
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void GetLocation_ShouldReturnEmptyString_WhenWatcherIsNull([Values] bool includeRoot)
    {
        // Arrange
        var dir = MakeSupressed(null, TestType, RuntimeConfig);

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
        var fileSystemMock = MakeFilesHelper([], [], []);
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);

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
        var fileSystemMock = MakeFilesHelper([], [], []);
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);

        // Act
        var result = dir.GetLocation(true);

        // Assert
        result.Should().Be(_testDirectory);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
}
