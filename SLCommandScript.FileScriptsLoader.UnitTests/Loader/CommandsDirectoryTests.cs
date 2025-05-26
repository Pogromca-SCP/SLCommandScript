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
public class CommandsDirectoryTests : TestWithConfigBase
{
    private const string TestDirectory = "commandsTest";

    private const string TestParent = "parentTest";

    private const string TestCommand = "test";

    private const CommandType TestType = CommandType.RemoteAdmin;

    #region Test Case Sources
    private static readonly string[] _invalidCommands = [string.Empty, "     ", "\t \t"];

    private static readonly string[] _validCommands = ["example", "bull", "script"];

    private static readonly CommandType[] _handlerTypes = [CommandType.RemoteAdmin, CommandType.Console, CommandType.Client];

    private static readonly CommandType[] _validTypes = [CommandType.RemoteAdmin, CommandType.Client];

    private static IEnumerable<object[]> InvalidCommandsXTypes => TestArrays.CartesianJoin(_invalidCommands, _validTypes);

    private static IEnumerable<object[]> ValidCommandsXTypes => TestArrays.CartesianJoin(_validCommands, _validTypes);
    #endregion

    #region Utilities
    private static Mock<IFileSystemWatcherHelper> MakeWatcherMock()
    {
        var watcherMock = new Mock<IFileSystemWatcherHelper>(MockBehavior.Strict);
        watcherMock.Setup(x => x.Directory).Returns(TestDirectory);
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
        fileSystemMock.Setup(x => x.EnumerateDirectories(TestDirectory)).Returns(foundFolders);
        fileSystemMock.Setup(x => x.EnumerateFiles(TestDirectory, EventsDirectory.ScriptFilesFilter, SearchOption.AllDirectories)).Returns(foundScripts);
        fileSystemMock.Setup(x => x.EnumerateFiles(TestDirectory, CommandsDirectory.DescriptionFilesFilter, SearchOption.AllDirectories)).Returns(foundJsons);
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

    private readonly string _testParentPath = $"{TestDirectory}{Path.DirectorySeparatorChar}{TestParent}";

    private readonly CommandMetaData _emptyMetadata = new();

    private CommandsDirectory MakeSupressed(IFileSystemWatcherHelper watcher, CommandType handlerType, Mock<IFileSystemHelper> fileSystemMock)
    {
        var dir = new CommandsDirectory(watcher, handlerType, FromFilesMock(fileSystemMock));
        GC.SuppressFinalize(dir);
        return dir;
    }

    private string MakePath(bool withParent, string name) => $"{(withParent ? _testParentPath : TestDirectory)}{Path.DirectorySeparatorChar}{name}";

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
    #endregion

    #region Constructor Tests
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
        var folder1 = $"{TestDirectory}{Path.DirectorySeparatorChar}folder1";
        var folder2 = $"{TestDirectory}{Path.DirectorySeparatorChar}folder2";
        var globalFile = $"{TestDirectory}{Path.DirectorySeparatorChar}global";
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
    #endregion

    #region Dispose Tests
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
        var folder1 = $"{TestDirectory}{Path.DirectorySeparatorChar}folder1";
        var folder2 = $"{TestDirectory}{Path.DirectorySeparatorChar}folder2";
        var globalFile = $"{TestDirectory}{Path.DirectorySeparatorChar}global";
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
    #endregion

    #region GetLocation Tests
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
        result.Should().Be(TestDirectory);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
    #endregion

    #region Created Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Created_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(false, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(string.Empty);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, false, name);

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Created_ShouldNotRegisterParent_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryCreate(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, false, name);

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Created_ShouldNotRegisterParentToParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryCreate(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);

        // Act
        RaiseCreate(watcherMock, true, name);

        // Assert
        GetCommandHandler(TestType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Created_ShouldRegisterParent_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryCreate(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, false, name);

        // Assert
        var handler = GetCommandHandler(type);
        handler.AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().ContainKey(name);
        dir.Commands[name]!.Command.Should().Be(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();

        // Cleanup
        handler.UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Created_ShouldRegisterParentToParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryCreate(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);

        // Act
        RaiseCreate(watcherMock, true, name);

        // Assert
        GetCommandHandler(TestType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().HaveCount(1);
        testParent.AllCommands.Should().Contain(c => c.Command.Equals(name));
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Created_ShouldNotRegisterCommand_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupScriptCreate(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, false, name);

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Created_ShouldNotRegisterCommandToParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = SetupScriptCreate(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);

        // Act
        RaiseCreate(watcherMock, true, name);

        // Assert
        GetCommandHandler(TestType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Created_ShouldRegisterCommand_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupScriptCreate(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, false, name);

        // Assert
        var handler = GetCommandHandler(type);
        handler.AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().ContainKey(name);
        dir.Commands[name]!.Command.Should().Be(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();

        // Cleanup
        handler.UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Created_ShouldRegisterCommandToParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = SetupScriptCreate(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);

        // Act
        RaiseCreate(watcherMock, true, name);

        // Assert
        GetCommandHandler(TestType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().HaveCount(1);
        testParent.AllCommands.Should().Contain(c => c.Command.Equals(name));
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldNotUpdateCommand_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = SetupDescriptionCreate(false, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);

        // Act
        RaiseCreate(watcherMock, false, TestCommand);

        // Assert
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldNotUpdateCommandFromParent_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = SetupDescriptionCreate(true, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        AddParent(dir);

        // Act
        RaiseCreate(watcherMock, true, TestCommand);

        // Assert
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldNotUpdateCommand_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = SetupDescriptionCreate(false, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        AddBadCommand(dir);

        // Act
        RaiseCreate(watcherMock, false, TestCommand);

        // Assert
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldNotUpdateCommandFromParent_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = SetupDescriptionCreate(true, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        AddBadCommand(testParent);

        // Act
        RaiseCreate(watcherMock, true, TestCommand);

        // Assert
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldNotUpdateCommand_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = SetupDescriptionCreate(false, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(false, TestCommand))).Throws<Exception>();
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var cmd = AddCommand(dir);

        // Act
        RaiseCreate(watcherMock, false, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.RequiredPermissions.Should().BeNull();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldNotUpdateCommandFromParent_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = SetupDescriptionCreate(true, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(true, TestCommand))).Throws<Exception>();
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddCommand(testParent);

        // Act
        RaiseCreate(watcherMock, true, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.RequiredPermissions.Should().BeNull();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldUpdateCommand_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example",
            Usage = ["Hello", "there"],
            Arity = 4,
            RequiredPerms = ["Noclip"]
        };

        var fileSystemMock = SetupDescriptionCreate(false, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(false, TestCommand))).Returns(newData);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var cmd = AddCommand(dir);

        // Act
        RaiseCreate(watcherMock, false, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.RequiredPermissions.Should().BeEquivalentTo(newData.RequiredPerms);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Created_ShouldUpdateCommandFromParent_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example2",
            Usage = ["Sequel"],
            Arity = 9,
            RequiredPerms = ["Noclip"]
        };

        var fileSystemMock = SetupDescriptionCreate(true, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(true, TestCommand))).Returns(newData);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddCommand(testParent);

        // Act
        RaiseCreate(watcherMock, true, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.RequiredPermissions.Should().BeEquivalentTo(newData.RequiredPerms);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
    #endregion

    #region Changed Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Changed_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper([], [], []);
        fileSystemMock.Setup(x => x.GetFileExtension(MakePath(false, name))).Returns(string.Empty);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseChange(watcherMock, false, name);

        // Assert
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommand_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = SetupChange(false, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);

        // Act
        RaiseChange(watcherMock, false, TestCommand);

        // Assert
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommandFromParent_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = SetupChange(true, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);

        // Act
        RaiseChange(watcherMock, true, TestCommand);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommand_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = SetupChange(false, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        AddBadCommand(dir);

        // Act
        RaiseChange(watcherMock, false, TestCommand);

        // Assert
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommandFromParent_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = SetupChange(true, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        AddBadCommand(testParent);

        // Act
        RaiseChange(watcherMock, true, TestCommand);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommand_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = SetupChange(false, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(false, TestCommand))).Throws<Exception>();
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var cmd = AddCommand(dir);

        // Act
        RaiseChange(watcherMock, false, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.RequiredPermissions.Should().BeNull();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommandFromParent_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = SetupChange(true, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(true, TestCommand))).Throws<Exception>();
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddCommand(testParent);

        // Act
        RaiseChange(watcherMock, true, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.RequiredPermissions.Should().BeNull();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldUpdateCommand_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example",
            Usage = ["Hello", "there"],
            Arity = 4,
            RequiredPerms = ["Noclip"]
        };

        var fileSystemMock = SetupChange(false, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(false, TestCommand))).Returns(newData);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var cmd = AddCommand(dir);

        // Act
        RaiseChange(watcherMock, false, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.RequiredPermissions.Should().BeEquivalentTo(newData.RequiredPerms);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [Test]
    public void Changed_ShouldUpdateCommandFromParent_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example2",
            Usage = ["Sequel"],
            Arity = 9,
            RequiredPerms = ["ServerConsoleCommands"]
        };

        var fileSystemMock = SetupChange(true, TestCommand);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(MakePath(true, TestCommand))).Returns(newData);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddCommand(testParent);

        // Act
        RaiseChange(watcherMock, true, TestCommand);

        // Assert
        cmd.Command.Should().Be(TestCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.RequiredPermissions.Should().BeEquivalentTo(newData.RequiredPerms);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
    #endregion

    #region Deleted Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(false, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(string.Empty);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterParent_WhenCommandIsNotFound(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryDelete(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldNotUnregisterParentFromParent_WhenCommandIsNotFound(string name)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryDelete(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);

        // Act
        RaiseDelete(watcherMock, true, name);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterParent_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryDelete(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);
        var parent = AddParent(dir, name);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        dir.Commands.Should().ContainKey(parent.Command);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Deleted_ShouldNotUnregisterParentFromParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryDelete(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var parent = AddParent(testParent, $"'{name}'");

        // Act
        RaiseDelete(watcherMock, true, name);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().Contain(parent);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldUnregisterParent_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryDelete(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);
        var parent = AddParent(dir, name);
        CommandsUtils.RegisterCommand(type, parent);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldUnregisterParentFromParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryDelete(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        AddParent(testParent, name);

        // Act
        RaiseDelete(watcherMock, true, name);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterCommand_WhenCommandIsNotFound(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupScriptDelete(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldNotUnregisterCommandFromParent_WhenCommandIsNotFound(string name)
    {
        // Arrange
        var fileSystemMock = SetupScriptDelete(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        AddParent(dir);

        // Act
        RaiseDelete(watcherMock, true, name);

        // Assert
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterCommand_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupScriptDelete(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);
        var cmd = AddCommand(dir, name);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        dir.Commands.Should().ContainKey(cmd.Command);
        dir.Commands[cmd.Command].Should().Be(cmd);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Deleted_ShouldNotUnregisterCommandFromParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = SetupScriptDelete(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddCommand(testParent, $"'{name}'");

        // Act
        RaiseDelete(watcherMock, true, name);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().Contain(cmd);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldUnregisterCommand_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupScriptDelete(false, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseDelete(watcherMock, false, name);

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldUnregisterCommandFromParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = SetupScriptDelete(true, name);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        AddCommand(testParent, name);

        // Act
        RaiseDelete(watcherMock, true, name);

        // Assert
        dir.Commands.Should().HaveCount(1);
        testParent.AllCommands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
    #endregion

    #region Renamed Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Renamed_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper([], [], []);
        var path = MakePath(false, TestCommand);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(string.Empty);
        path = MakePath(false, name);
        fileSystemMock.Setup(x => x.DirectoryExists(path)).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension(path)).Returns(string.Empty);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseRename(watcherMock, false, name, TestCommand);

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Renamed_ShouldReplaceParent_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryRename(false, true, name, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseRename(watcherMock, false, name, TestCommand);

        // Assert
        var handler = GetCommandHandler(type);
        handler.AllCommands.Should().NotContain(c => c.Command.Equals(TestCommand));
        handler.AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        dir.Commands.Should().ContainKey(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();

        // Cleanup
        handler.UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Renamed_ShouldReplaceParentInsideParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = SetupDirectoryRename(true, false, name, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddParent(testParent, TestCommand);

        // Act
        RaiseRename(watcherMock, true, name, TestCommand);

        // Assert
        GetCommandHandler(TestType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        var children = testParent.AllCommands;
        children.Should().NotContain(c => c.Command.Equals(TestCommand));
        children.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Renamed_ShouldReplaceCommand_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = SetupScriptRename(false, true, name, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, type, fileSystemMock);

        // Act
        RaiseRename(watcherMock, false, name, TestCommand);

        // Assert
        var handler = GetCommandHandler(type);
        handler.AllCommands.Should().NotContain(c => c.Command.Equals(TestCommand));
        handler.AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        dir.Commands.Should().ContainKey(name);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();

        // Cleanup
        handler.UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Renamed_ShouldReplaceCommandInsideParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = SetupScriptRename(true, false, name, TestCommand);
        var watcherMock = MakeWatcherMock();
        var dir = MakeSupressed(watcherMock.Object, TestType, fileSystemMock);
        var testParent = AddParent(dir);
        var cmd = AddCommand(testParent, TestCommand);

        // Act
        RaiseRename(watcherMock, true, name, TestCommand);

        // Assert
        GetCommandHandler(TestType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        var children = testParent.AllCommands;
        children.Should().NotContain(c => c.Command.Equals(TestCommand));
        children.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        fileSystemMock.VerifyAll();
    }
    #endregion
}
