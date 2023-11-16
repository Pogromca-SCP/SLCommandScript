using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using CommandSystem.Commands.Shared;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using PluginAPI.Enums;
using RemoteAdmin;
using SLCommandScript.Core.Commands;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Commands;

[TestFixture]
public class CommandsUtilsTests
{
    private const string MockCommandName = "test";

    private const CommandType InvalidCommandType = CommandType.Console;

    #region Test Case Sources
    private static readonly CommandType[] _allHandlerTypes = [CommandType.RemoteAdmin, CommandType.Console,
        CommandType.GameConsole, CommandType.RemoteAdmin | CommandType.Console, CommandType.RemoteAdmin | CommandType.GameConsole,
        CommandType.GameConsole | CommandType.Console, CommandType.RemoteAdmin | CommandType.GameConsole | CommandType.Console];

    private static readonly string[] _invalidCommandNames = [null, "", " ", " \t ", "  \t  \t\t"];

    private static readonly string[] _validCommandNames = ["hello", "item list", "?.cassie"];

    private static readonly string[][] _invalidAliases = [["  "], [null, "test"], ["hello", "  \t", "   ", null]];

    private static readonly string[][] _validAliases = [null, ["string", "example"], []];

    private static readonly CommandType[] _validHandlerTypes = [CommandType.RemoteAdmin, CommandType.GameConsole, CommandType.RemoteAdmin | CommandType.GameConsole];

    private static readonly string[] _existingCommandNames = ["help", "HelP", "bc", "cassie", "BC"];

    private static readonly string[] _commandsToRegister = ["wtf", "dotheflip", "weeee"];

    private static readonly ICommand[] _exampleCommands = [new BroadcastCommand(), new CassieCommand(), new HelpCommand(ClientCommandHandler.Create())];

    private static IEnumerable<object[]> AllHandlersXInvalidCommands => JoinArrays(_allHandlerTypes, _invalidCommandNames);

    private static IEnumerable<object[]> AllHandlersXInvalidAliases => JoinArrays(_allHandlerTypes, _invalidAliases);

    private static IEnumerable<object[]> ValidHandlersXExistingCommandNames => JoinArrays(_validHandlerTypes, _existingCommandNames);

    private static IEnumerable<object[]> ValidHandlersXCommandsToRegister => JoinArrays(_validHandlerTypes, _commandsToRegister);

    private static IEnumerable<object[]> ValidHandlersXExampleCommands => JoinArrays(_validHandlerTypes, _exampleCommands);

    private static IEnumerable<object[]> JoinArrays<TFirst, TSecond>(TFirst[] first, TSecond[] second) =>
        first.SelectMany(f => second.Select(s => new object[] { f, s }));
    #endregion

    #region Helper Methods
    private static IEnumerable<ICommandHandler> GetExpectedCommandHandlers(CommandType handlerType) => handlerType switch
    {
        CommandType.RemoteAdmin => [CommandProcessor.RemoteAdminCommandHandler],
        CommandType.RemoteAdmin | CommandType.Console => [CommandProcessor.RemoteAdminCommandHandler],
        CommandType.GameConsole => [QueryProcessor.DotCommandHandler],
        CommandType.GameConsole | CommandType.Console => [QueryProcessor.DotCommandHandler],
        CommandType.RemoteAdmin | CommandType.GameConsole => [CommandProcessor.RemoteAdminCommandHandler, QueryProcessor.DotCommandHandler],
        CommandType.RemoteAdmin | CommandType.GameConsole | CommandType.Console => [CommandProcessor.RemoteAdminCommandHandler, QueryProcessor.DotCommandHandler],
        _ => []
    };

    private static CommandType GetCommandHandlerType(ICommandHandler handler) => handler switch
    {
        RemoteAdminCommandHandler => CommandType.RemoteAdmin,
        ClientCommandHandler => CommandType.GameConsole,
        _ => 0
    };

    private static CommandType JoinCommandTypes(IEnumerable<CommandType> handlerTypes)
    {
        CommandType result = 0;

        foreach (var handlerType in handlerTypes)
        {
            result |= handlerType;
        }

        return result;
    }
    #endregion

    #region GetCommandHandlers Tests
    [TestCaseSource(nameof(_allHandlerTypes))]
    public void GetCommandHandlers_ShouldReturnProperHandlers(CommandType handlerType)
    {
        // Arrange
        var expectedHandlers = GetExpectedCommandHandlers(handlerType);

        // Act
        var result = CommandsUtils.GetCommandHandlers(handlerType);

        // Assert
        result.Should().BeEquivalentTo(expectedHandlers);
    }
    #endregion

    #region IsCommandInvalid Tests
    [Test]
    public void IsCommandInvalid_ShouldReturnTrue_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.IsCommandInvalid(null);

        // Assert
        result.Should().BeTrue();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void IsCommandInvalid_ShouldReturnTrue_WhenCommandHasInvalidName(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.IsCommandInvalid(commandMock.Object);

        // Assert
        result.Should().BeTrue();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void IsCommandInvalid_ShouldReturnTrue_WhenCommandHasInvalidAlias(string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.IsCommandInvalid(commandMock.Object);

        // Assert
        result.Should().BeTrue();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validAliases))]
    public void IsCommandInvalid_ShouldReturnFalse_WhenCommandIsValid(string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.IsCommandInvalid(commandMock.Object);

        // Assert
        result.Should().BeFalse();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion

    #region GetCommand Tests
    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void GetCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string commandName)
    {
        // Act
        var result = CommandsUtils.GetCommand(handlerType, commandName);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void GetCommand_ShouldReturnNull_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Act
        var result = CommandsUtils.GetCommand(InvalidCommandType, commandName);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(ValidHandlersXExistingCommandNames))]
    public void GetCommand_ShouldReturnProperResult_WhenGoldFlow(CommandType handlerType, string commandName)
    {
        // Arrange
        var handler = GetExpectedCommandHandlers(handlerType).FirstOrDefault(h => h.TryGetCommand(commandName, out _));
        ICommand foundCommand = null;
        var commandExists = handler?.TryGetCommand(commandName, out foundCommand);

        // Act
        var result = CommandsUtils.GetCommand(handlerType, commandName);

        // Assert
        if (commandExists.HasValue && commandExists.Value)
        {
            result.Should().Be(foundCommand);
        }
        else
        {
            result.Should().BeNull();
        }
    }
    #endregion

    #region IsCommandRegistered Tests
    [TestCaseSource(nameof(_allHandlerTypes))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandIsNull(CommandType handlerType)
    {
        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidAliases))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandHasInvalidAlias(CommandType handlerType, string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void IsCommandRegistered_ShouldReturnZero_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);
        commandMock.Setup(x => x.Aliases).Returns((string[]) null);

        // Act
        var result = CommandsUtils.IsCommandRegistered(InvalidCommandType, commandMock.Object);

        // Assert
        result.Should().Be(0);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidHandlersXExampleCommands))]
    public void IsCommandRegistered_ShouldReturnProperResult_WhenGoldFlow(CommandType handlerType, ICommand command)
    {
        // Arrange
        var handlersTypes = GetExpectedCommandHandlers(handlerType).Where(h => h.TryGetCommand(command.Command, out _)).Select(GetCommandHandlerType);
        var expectedResult = JoinCommandTypes(handlersTypes);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, command);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandHandlerIsNull()
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);

        // Act
        var result = CommandsUtils.IsCommandRegistered(null, commandMock.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.IsCommandRegistered(ClientCommandHandler.Create(), null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandNameIsInvalid(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.IsCommandRegistered(ClientCommandHandler.Create(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandHasInvalidAlias(string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.IsCommandRegistered(ClientCommandHandler.Create(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_exampleCommands))]
    public void IsCommandRegistered_ShouldReturnProperResult_WhenGoldFlow(ICommand command)
    {
        // Arrange
        var handler = ClientCommandHandler.Create();
        var expectedResult = handler.TryGetCommand(command.Command, out _);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handler, command);

        // Assert
        result.Should().Be(expectedResult);
    }
    #endregion

    #region RegisterCommand Tests
    [TestCaseSource(nameof(_allHandlerTypes))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandIsNull(CommandType handlerType)
    {
        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidAliases))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(CommandType handlerType, string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void RegisterCommand_ShouldReturnZero_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);
        commandMock.Setup(x => x.Aliases).Returns((string[]) null);

        // Act
        var result = CommandsUtils.RegisterCommand(InvalidCommandType, commandMock.Object);

        // Assert
        result.Should().Be(0);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidHandlersXCommandsToRegister))]
    public void RegisterCommand_ShouldProperlyRegister_WhenGoldFlow(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);
        commandMock.Setup(x => x.Aliases).Returns((string[]) null);
        var handlers = GetExpectedCommandHandlers(handlerType);
        var expectedResult = JoinCommandTypes(handlers.Where(h => !h.TryGetCommand(commandName, out _)).Select(GetCommandHandlerType));

        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().Be(expectedResult);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
        handlers.Select(h => h.TryGetCommand(commandName, out _)).Should().NotContain(false);
    }

    [Test]
    public void RegisterCommand_ShouldReturnNull_WhenCommandHandlerIsNull()
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);

        // Act
        var result = CommandsUtils.RegisterCommand(null, commandMock.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void RegisterCommand_ShouldReturnNull_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.RegisterCommand(ClientCommandHandler.Create(), null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.RegisterCommand(ClientCommandHandler.Create(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.RegisterCommand(ClientCommandHandler.Create(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_exampleCommands))]
    public void RegisterCommand_ShouldProperlyRegister_WhenGoldFlow(ICommand command)
    {
        // Arrange
        var handler = ClientCommandHandler.Create();
        var expectedResult = !handler.TryGetCommand(command.Command, out _);

        // Act
        var result = CommandsUtils.RegisterCommand(handler, command);

        // Assert
        result.Should().Be(expectedResult);
        handler.TryGetCommand(command.Command, out _).Should().BeTrue();
    }
    #endregion

    #region UnregisterCommand Tests
    [TestCaseSource(nameof(_allHandlerTypes))]
    public void UnegisterCommand_ShouldReturnNull_WhenCommandIsNull(CommandType handlerType)
    {
        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidAliases))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(CommandType handlerType, string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void UnregisterCommand_ShouldReturnZero_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);
        commandMock.Setup(x => x.Aliases).Returns((string[]) null);

        // Act
        var result = CommandsUtils.UnregisterCommand(InvalidCommandType, commandMock.Object);

        // Assert
        result.Should().Be(0);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(ValidHandlersXCommandsToRegister))]
    public void UnregisterCommand_ShouldProperlyUnregister_WhenGoldFlow(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);
        commandMock.Setup(x => x.Aliases).Returns((string[]) null);
        var handlers = GetExpectedCommandHandlers(handlerType);
        var expectedResult = JoinCommandTypes(handlers.Where(h => h.TryGetCommand(commandName, out _)).Select(GetCommandHandlerType));

        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().Be(expectedResult);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
        handlers.Select(h => h.TryGetCommand(commandName, out _)).Should().NotContain(true);
    }

    [Test]
    public void UnegisterCommand_ShouldReturnNull_WhenCommandHandlerIsNull()
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);

        // Act
        var result = CommandsUtils.UnregisterCommand(null, commandMock.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void UnegisterCommand_ShouldReturnNull_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.UnregisterCommand(ClientCommandHandler.Create(), null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(string commandName)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(commandName);

        // Act
        var result = CommandsUtils.UnregisterCommand(ClientCommandHandler.Create(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(string[] aliases)
    {
        // Arrange
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Command).Returns(MockCommandName);
        commandMock.Setup(x => x.Aliases).Returns(aliases);

        // Act
        var result = CommandsUtils.UnregisterCommand(ClientCommandHandler.Create(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_exampleCommands))]
    public void UnregisterCommand_ShouldProperlyUnregister_WhenGoldFlow(ICommand command)
    {
        // Arrange
        var handler = ClientCommandHandler.Create();
        var expectedResult = handler.TryGetCommand(command.Command, out _);

        // Act
        var result = CommandsUtils.UnregisterCommand(handler, command);

        // Assert
        result.Should().Be(expectedResult);
        handler.TryGetCommand(command.Command, out _).Should().BeFalse();
    }
    #endregion
}
