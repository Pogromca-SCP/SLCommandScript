using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Commands;
using SLCommandScript.FileScriptsLoader.Commands;
using System;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Loader;

[TestFixture]
public partial class CommandsDirectoryTests : TestWithConfigBase
{
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
}
