using NUnit.Framework;
using PluginAPI.Enums;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SLCommandScript.FileScriptsLoader.Helpers;
using System.IO;
using CommandSystem;
using RemoteAdmin;
using SLCommandScript.FileScriptsLoader.Loader;
using FluentAssertions;
using SLCommandScript.FileScriptsLoader.Commands;
using System;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Loader;

[TestFixture]
public class CommandsDirectoryTests
{
    #region Constants
    private const string _testDirectory = "commandsTest";

    private const string _testParent = "parentTest";

    private const string _testCommand = "test";

    private CommandType _testType = CommandType.RemoteAdmin;
    #endregion

    #region Test Case Sources
    private static readonly string[] _invalidCommands = { string.Empty, "     ", "\t \t" };

    private static readonly string[] _validCommands = { "example", "bull", "script" };

    private static readonly CommandType[] _handlerTypes = { CommandType.RemoteAdmin, CommandType.Console, CommandType.GameConsole };

    private static readonly CommandType[] _validTypes = { CommandType.RemoteAdmin, CommandType.GameConsole };

    private static IEnumerable<object[]> InvalidCommandsXTypes => JoinArrays(_invalidCommands, _validTypes);

    private static IEnumerable<object[]> ValidCommandsXTypes => JoinArrays(_validCommands, _validTypes);

    private static IEnumerable<object[]> JoinArrays(string[] first, CommandType[] second) =>
        first.SelectMany(f => second.Select(s => new object[] { f, s }));
    #endregion

    #region Helper Methods
    private static Mock<IFileSystemWatcherHelper> MakeWatcherMock()
    {
        var watcherMock = new Mock<IFileSystemWatcherHelper>(MockBehavior.Strict);
        watcherMock.Setup(x => x.Directory).Returns(_testDirectory);
        watcherMock.SetupAdd(x => x.Created += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Changed += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Deleted += It.IsAny<FileSystemEventHandler>());
        watcherMock.SetupAdd(x => x.Renamed += It.IsAny<RenamedEventHandler>());
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
        CommandType.GameConsole => QueryProcessor.DotCommandHandler,
        _ => null
    };
    #endregion

    #region Constructor Tests
    [TestCaseSource(nameof(_handlerTypes))]
    public void CommandsDirectory_ShouldNotInitialize_WhenProvidedWatcherIsNull(CommandType type)
    {
        // Arrange
        HelpersProvider.FileSystemHelper = null;

        // Act
        var result = new CommandsDirectory(null, type);

        // Assert
        result.Directories.Should().BeEmpty();
        result.Commands.Should().BeEmpty();
        result.HandlerType.Should().Be(type);
        result.Watcher.Should().BeNull();
    }

    [TestCaseSource(nameof(_handlerTypes))]
    public void CommandsDirectory_ShouldProperlyInitialize_WhenNoFilesExist(CommandType type)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new CommandsDirectory(watcherMock.Object, type);

        // Assert
        result.Directories.Should().BeEmpty();
        result.Commands.Should().BeEmpty();
        result.HandlerType.Should().Be(type);
        result.Watcher.Should().Be(watcherMock.Object);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validTypes))]
    public void CommandsDirectory_ShouldProperlyInitialize_WhenFilesExist(CommandType type)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        var fileSystemMock = MakeFilesHelper(new[] { "folder1", "folder2", "bad" }, new[] { "global", "inner", "bad" }, new[] { "global", "inner", "bad" });
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("folder1")).Returns("folder1");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("folder2")).Returns("folder2");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("global")).Returns("global");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("inner")).Returns("inner");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("bad")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("folder1")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("folder2")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("global")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("inner")).Returns("folder1");
        fileSystemMock.Setup(x => x.GetDirectory("bad")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(It.IsAny<string>())).Returns(new CommandMetaData());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;

        // Act
        var result = new CommandsDirectory(watcherMock.Object, type);

        // Assert
        result.Directories.Should().HaveCount(2);
        result.Commands.Should().HaveCount(3);
        result.HandlerType.Should().Be(type);
        result.Watcher.Should().Be(watcherMock.Object);
        var commands = GetCommandHandler(type).AllCommands;
        commands.Should().Contain(c => c.Command.Equals("folder1"));
        commands.Should().Contain(c => c.Command.Equals("folder2"));
        commands.Should().Contain(c => c.Command.Equals("global"));
        commands.Should().NotContain(c => c.Command.Equals("inner"));
        commands.Should().NotContain(c => c.Command.Equals(string.Empty));
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Dispose Tests
    [TestCaseSource(nameof(_validTypes))]
    public void Dispose_ShouldUnregisterCommands_WhenWatcherIsNull(CommandType type)
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystemHelper>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("global")).Returns("global");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("inner")).Returns("inner");
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var f1 = new FileScriptDirectoryCommand("folder1");
        f1.RegisterCommand(new FileScriptCommand("inner"));
        var dir = new CommandsDirectory(null, type);
        dir.Commands.Add(f1.Command, f1);
        dir.Directories.Add(f1.Command, f1);
        var f2 = new FileScriptDirectoryCommand("folder2");
        dir.Commands.Add(f2.Command, f2);
        dir.Directories.Add(f2.Command, f2);
        var cmd = new FileScriptCommand("global");
        GetCommandHandler(type).RegisterCommand(cmd);
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        dir.Dispose();

        // Assert
        dir.Directories.Should().HaveCount(2);
        dir.Commands.Should().HaveCount(3);
        dir.HandlerType.Should().Be(type);
        dir.Watcher.Should().BeNull();
        var commands = GetCommandHandler(type).AllCommands;
        commands.Should().NotContain(c => c.Command.Equals("folder1"));
        commands.Should().NotContain(c => c.Command.Equals("folder2"));
        commands.Should().NotContain(c => c.Command.Equals("global"));
        commands.Should().NotContain(c => c.Command.Equals("inner"));
        commands.Should().NotContain(c => c.Command.Equals(string.Empty));
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validTypes))]
    public void Dispose_ShouldCleanupResources_WhenGoldFlow(CommandType type)
    {
        // Arrange
        var watcherMock = MakeWatcherMock();
        watcherMock.Setup(x => x.Dispose());
        var fileSystemMock = MakeFilesHelper(new[] { "folder1", "folder2", "bad" }, new[] { "global", "inner", "bad" }, new[] { "global", "inner", "bad" });
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("folder1")).Returns("folder1");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("folder2")).Returns("folder2");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("global")).Returns("global");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("inner")).Returns("inner");
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension("bad")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("folder1")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("folder2")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("global")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.GetDirectory("inner")).Returns("folder1");
        fileSystemMock.Setup(x => x.GetDirectory("bad")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson(It.IsAny<string>())).Returns(new CommandMetaData());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        dir.Dispose();

        // Assert
        dir.Directories.Should().HaveCount(2);
        dir.Commands.Should().HaveCount(3);
        dir.HandlerType.Should().Be(type);
        dir.Watcher.Should().Be(watcherMock.Object);
        var commands = GetCommandHandler(type).AllCommands;
        commands.Should().NotContain(c => c.Command.Equals("folder1"));
        commands.Should().NotContain(c => c.Command.Equals("folder2"));
        commands.Should().NotContain(c => c.Command.Equals("global"));
        commands.Should().NotContain(c => c.Command.Equals("inner"));
        commands.Should().NotContain(c => c.Command.Equals(string.Empty));
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Created Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Created_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Created_ShouldNotRegisterParent_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Created_ShouldNotRegisterParentToParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(_testType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Created_ShouldRegisterParent_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Directories.Should().ContainKey($"{_testDirectory}\\{name}");
        dir.Directories[$"{_testDirectory}\\{name}"].Command.Should().Be(name);
        dir.Commands.Should().ContainKey(name);
        dir.Commands[name].Command.Should().Be(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();

        // Cleanup
        GetCommandHandler(type).UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Created_ShouldRegisterParentToParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(_testType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().HaveCount(2);
        dir.Directories.Should().ContainKey($"{_testDirectory}\\{name}");
        dir.Directories[_testParent].AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Created_ShouldNotRegisterCommand_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Created_ShouldNotRegisterCommandToParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(_testType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Created_ShouldRegisterCommand_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().ContainKey(name);
        dir.Commands[name].Command.Should().Be(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();

        // Cleanup
        GetCommandHandler(type).UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Created_ShouldRegisterCommandToParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, name));

        // Assert
        GetCommandHandler(_testType).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldNotUpdateCommand_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldNotUpdateCommandFromParent_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldNotUpdateCommand_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Commands.Add(_testCommand, new FileScriptDirectoryCommand(_testCommand));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldNotUpdateCommandFromParent_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand(_testCommand));

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().HaveCount(1);
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldNotUpdateCommand_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Throws(new Exception());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.Help.Should().BeNull();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldNotUpdateCommandFromParent_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Throws(new Exception());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Directories[_testParent].RegisterCommand(cmd);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.Help.Should().BeNull();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldUpdateCommand_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example",
            Usage = new[] { "Hello", "there" },
            Arity = 4,
            Help = "HEEEEELP!!!"
        };

        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Returns(newData);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.Help.Should().Be(newData.Help);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Created_ShouldUpdateCommandFromParent_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example2",
            Usage = new[] { "Sequel" },
            Arity = 9,
            Help = "AAAAAA!!!"
        };

        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Returns(newData);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Directories[_testParent].RegisterCommand(cmd);

        // Act
        watcherMock.Raise(x => x.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.Help.Should().Be(newData.Help);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Changed Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Changed_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, name));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommand_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommandFromParent_WhenCommandIsNotFound()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommand_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Commands.Add(_testCommand, new FileScriptDirectoryCommand(_testCommand));

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().HaveCount(1);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommandFromParent_WhenCommandHasIncorrectType()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand(_testCommand));

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().HaveCount(1);
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommand_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Throws(new Exception());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.Help.Should().BeNull();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldNotUpdateCommandFromParent_WhenJsonSerializerThrows()
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Throws(new Exception());
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Directories[_testParent].RegisterCommand(cmd);

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(FileScriptCommandBase.DefaultDescription);
        cmd.Usage.Should().BeNull();
        cmd.Arity.Should().Be(0);
        cmd.Help.Should().BeNull();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldUpdateCommand_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example",
            Usage = new[] { "Hello", "there" },
            Arity = 4,
            Help = "HEEEEELP!!!"
        };

        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Returns(newData);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.Help.Should().Be(newData.Help);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Changed_ShouldUpdateCommandFromParent_WhenCommandIsFound()
    {
        // Arrange
        var newData = new CommandMetaData()
        {
            Description = "example2",
            Usage = new[] { "Sequel" },
            Arity = 9,
            Help = "AAAAAA!!!"
        };

        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptDescriptionExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        fileSystemMock.Setup(x => x.ReadMetadataFromJson($"{_testDirectory}\\{_testCommand}")).Returns(newData);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        var cmd = new FileScriptCommand($"{_testDirectory}\\{_testCommand}");
        dir.Directories[_testParent].RegisterCommand(cmd);

        // Act
        watcherMock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, _testDirectory, _testCommand));

        // Assert
        cmd.Command.Should().Be(_testCommand);
        cmd.Aliases.Should().BeNull();
        cmd.Description.Should().Be(newData.Description);
        cmd.Usage.Should().BeEquivalentTo(newData.Usage);
        cmd.Arity.Should().Be(newData.Arity);
        cmd.Help.Should().Be(newData.Help);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Deleted Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterParent_WhenCommandIsNotFound(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldNotUnregisterParentFromParent_WhenCommandIsNotFound(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterParent_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);
        var cmd = new FileScriptDirectoryCommand(name);
        dir.Directories.Add($"{_testDirectory}\\{name}", cmd);
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().ContainKey($"{_testDirectory}\\{name}");
        dir.Directories[$"{_testDirectory}\\{name}"].Should().Be(cmd);
        dir.Commands.Should().ContainKey(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Deleted_ShouldNotUnregisterParentFromParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand($"'{name}'"));

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().Contain(c => c.Command.Equals($"'{name}'"));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldUnregisterParent_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new[] { $"{_testDirectory}\\{name}" }, new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldUnregisterParentFromParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand(name));

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterCommand_WhenCommandIsNotFound(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldNotUnregisterCommandFromParent_WhenCommandIsNotFound(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    public void Deleted_ShouldNotUnregisterCommand_WhenCommandNameIsInvalid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);
        var cmd = new FileScriptDirectoryCommand(name);
        dir.Commands.Add(cmd.Command, cmd);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().ContainKey(cmd.Command);
        dir.Commands[cmd.Command].Should().Be(cmd);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidCommands))]
    public void Deleted_ShouldNotUnregisterCommandFromParent_WhenCommandNameIsInvalid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand($"'{name}'"));

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().Contain(c => c.Command.Equals($"'{name}'"));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Deleted_ShouldUnregisterCommand_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new[] { $"{_testDirectory}\\{name}" }, new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Deleted_ShouldUnregisterCommandToParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand(name));

        // Act
        watcherMock.Raise(x => x.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _testDirectory, name));

        // Assert
        dir.Directories.Should().HaveCount(1);
        dir.Directories[_testParent].AllCommands.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion

    #region Renamed Tests
    [TestCaseSource(nameof(InvalidCommandsXTypes))]
    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Renamed_ShouldDoNothing_WhenFileTypeIsNotSupported(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, _testCommand));

        // Assert
        GetCommandHandler(type).AllCommands.Should().NotContain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Renamed_ShouldReplaceParent_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new[] { $"{_testDirectory}\\{_testCommand}" }, new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(true);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, _testCommand));

        // Assert
        var handler = GetCommandHandler(type);
        handler.AllCommands.Should().NotContain(c => c.Command.Equals(_testCommand));
        handler.AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Directories.Should().HaveCount(1);
        dir.Directories.Should().ContainKey($"{_testDirectory}\\{name}");
        dir.Commands.Should().HaveCount(1);
        dir.Commands.Should().ContainKey(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();

        // Cleanup
        handler.UnregisterCommand(dir.Directories[$"{_testDirectory}\\{name}"]);
    }

    [TestCaseSource(nameof(ValidCommandsXTypes))]
    public void Renamed_ShouldReplaceCommand_WhenCommandNameIsValid(string name, CommandType type)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new[] { $"{_testDirectory}\\{_testCommand}" }, new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(string.Empty);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(string.Empty);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, type);

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, _testCommand));

        // Assert
        var handler = GetCommandHandler(type);
        handler.AllCommands.Should().NotContain(c => c.Command.Equals(_testCommand));
        handler.AllCommands.Should().Contain(c => c.Command.Equals(name));
        dir.Directories.Should().BeEmpty();
        dir.Commands.Should().HaveCount(1);
        dir.Commands.Should().ContainKey(name);
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();

        // Cleanup
        handler.UnregisterCommand(dir.Commands[name]);
    }

    [TestCaseSource(nameof(_validCommands))]
    public void Renamed_ShouldReplaceCommandInsideParent_WhenCommandNameIsValid(string name)
    {
        // Arrange
        var fileSystemMock = MakeFilesHelper(new string[0], new string[0], new string[0]);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{_testCommand}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{_testCommand}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{_testCommand}")).Returns(_testCommand);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{_testCommand}")).Returns(_testParent);
        fileSystemMock.Setup(x => x.DirectoryExists($"{_testDirectory}\\{name}")).Returns(false);
        fileSystemMock.Setup(x => x.GetFileExtension($"{_testDirectory}\\{name}")).Returns(CommandsDirectory.ScriptFileExtension);
        fileSystemMock.Setup(x => x.GetFileNameWithoutExtension($"{_testDirectory}\\{name}")).Returns(name);
        fileSystemMock.Setup(x => x.GetDirectory($"{_testDirectory}\\{name}")).Returns(_testParent);
        HelpersProvider.FileSystemHelper = fileSystemMock.Object;
        var watcherMock = MakeWatcherMock();
        var dir = new CommandsDirectory(watcherMock.Object, _testType);
        dir.Directories.Add(_testParent, new(_testParent));
        dir.Directories[_testParent].RegisterCommand(new FileScriptDirectoryCommand(_testCommand));

        // Act
        watcherMock.Raise(x => x.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, _testDirectory, name, _testCommand));

        // Assert
        dir.Directories.Should().HaveCount(1);
        var children = dir.Directories[_testParent].AllCommands;
        children.Should().NotContain(c => c.Command.Equals(_testCommand));
        children.Should().Contain(c => c.Command.Equals(name));
        dir.Commands.Should().BeEmpty();
        watcherMock.VerifyAll();
        watcherMock.VerifyNoOtherCalls();
        fileSystemMock.VerifyAll();
        fileSystemMock.VerifyNoOtherCalls();
    }
    #endregion
}
