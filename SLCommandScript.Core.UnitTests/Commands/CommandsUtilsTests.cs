using AwesomeAssertions;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using CommandSystem.Commands.Shared;
using LabApi.Features.Wrappers;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Commands;
using SLCommandScript.TestUtils;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Commands;

[TestFixture]
public class CommandsUtilsTests
{
    private const string MockCommandName = "test";

    private const CommandType InvalidCommandType = 0;

    private static readonly CommandType[] _allHandlerTypes = [CommandType.RemoteAdmin, CommandType.Console,
        CommandType.Client, CommandType.RemoteAdmin | CommandType.Console, CommandType.RemoteAdmin | CommandType.Client,
        CommandType.Client | CommandType.Console, CommandType.RemoteAdmin | CommandType.Client | CommandType.Console];

    private static readonly string?[] _invalidCommandNames = [null, "", " ", " \t ", "  \t  \t\t"];

    private static readonly string[] _validCommandNames = ["hello", "item list", "?.cassie"];

    private static readonly string?[][] _invalidAliases = [["  "], [null, "test"], ["hello", "  \t", "   ", null]];

    private static readonly string[]?[] _validAliases = [null, ["string", "example"], []];

    private static readonly string[] _existingCommandNames = ["help", "HelP", "bc", "cassie", "BC"];

    private static readonly string[] _commandsToRegister = ["wtf", "dotheflip", "weeee"];

    private static readonly ICommand[] _exampleCommands = [new BroadcastCommand(), new CassieCommand(), new HelpCommand(ClientCommandHandler.Create())];

    private static IEnumerable<object?[]> AllHandlersXInvalidCommands => TestArrays.CartesianJoin(_allHandlerTypes, _invalidCommandNames);

    private static IEnumerable<object?[]> AllHandlersXInvalidAliases => TestArrays.CartesianJoin(_allHandlerTypes, _invalidAliases);

    private static IEnumerable<object[]> ValidHandlersXExistingCommandNames => TestArrays.CartesianJoin(_allHandlerTypes, _existingCommandNames);

    private static IEnumerable<object[]> ValidHandlersXCommandsToRegister => TestArrays.CartesianJoin(_allHandlerTypes, _commandsToRegister);

    private static IEnumerable<object[]> ValidHandlersXExampleCommands => TestArrays.CartesianJoin(_allHandlerTypes, _exampleCommands);

    private static IEnumerable<ICommandHandler> GetExpectedCommandHandlers(CommandType handlerType) => handlerType switch
    {
        CommandType.RemoteAdmin => [Server.RemoteAdminCommandHandler],
        CommandType.Console => [Server.GameConsoleCommandHandler],
        CommandType.RemoteAdmin | CommandType.Console => [Server.RemoteAdminCommandHandler, Server.GameConsoleCommandHandler],
        CommandType.Client => [Server.ClientCommandHandler],
        CommandType.Client | CommandType.Console => [Server.GameConsoleCommandHandler, Server.ClientCommandHandler],
        CommandType.RemoteAdmin | CommandType.Client => [Server.RemoteAdminCommandHandler, Server.ClientCommandHandler],
        CommandType.RemoteAdmin | CommandType.Client | CommandType.Console => [Server.RemoteAdminCommandHandler, Server.GameConsoleCommandHandler,
            Server.ClientCommandHandler],
        _ => [],
    };

    private static CommandType GetCommandHandlerType(ICommandHandler handler) => handler switch
    {
        RemoteAdminCommandHandler => CommandType.RemoteAdmin,
        GameConsoleCommandHandler => CommandType.Console,
        ClientCommandHandler => CommandType.Client,
        _ => 0,
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

    private static Mock<ICommand> MockCommand() => new(MockBehavior.Strict);

    private static Mock<ICommand> MockCommand(string? name)
    {
        var mock = MockCommand();
        mock.Setup(x => x.Command).Returns(name!);
        return mock;
    }

    private static Mock<ICommand> MockCommand(string? name, string?[]? aliases)
    {
        var mock = MockCommand(name);
        mock.Setup(x => x.Aliases).Returns(aliases!);
        return mock;
    }

    private static ICommandHandler MakeHandler() => ClientCommandHandler.Create();

    private IEnumerable<ICommand> _originalRemoteAdmin = null!;

    private IEnumerable<ICommand> _originalConsole = null!;

    private IEnumerable<ICommand> _originalClient = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _originalRemoteAdmin = TestCommandHandlers.CopyCommands(Server.RemoteAdminCommandHandler);
        _originalConsole = TestCommandHandlers.CopyCommands(Server.GameConsoleCommandHandler);
        _originalClient = TestCommandHandlers.CopyCommands(Server.ClientCommandHandler);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestCommandHandlers.SetCommands(Server.RemoteAdminCommandHandler, _originalRemoteAdmin);
        TestCommandHandlers.SetCommands(Server.GameConsoleCommandHandler, _originalConsole);
        TestCommandHandlers.SetCommands(Server.ClientCommandHandler, _originalClient);
    }

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

    [Test]
    public void IsCommandInvalid_ShouldReturnTrue_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.IsCommandInvalid(null);

        // Assert
        result.Should().BeTrue();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void IsCommandInvalid_ShouldReturnTrue_WhenCommandHasInvalidName(string? commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.IsCommandInvalid(commandMock.Object);

        // Assert
        result.Should().BeTrue();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void IsCommandInvalid_ShouldReturnTrue_WhenCommandHasInvalidAlias(string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.IsCommandInvalid(commandMock.Object);

        // Assert
        result.Should().BeTrue();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validAliases))]
    public void IsCommandInvalid_ShouldReturnFalse_WhenCommandIsValid(string[]? aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.IsCommandInvalid(commandMock.Object);

        // Assert
        result.Should().BeFalse();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void GetCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string? commandName)
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
        ICommand? foundCommand = null;
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

    [TestCaseSource(nameof(_allHandlerTypes))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandIsNull(CommandType handlerType)
    {
        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string? commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidAliases))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandHasInvalidAlias(CommandType handlerType, string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void IsCommandRegistered_ShouldReturnZero_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName, null);

        // Act
        var result = CommandsUtils.IsCommandRegistered(InvalidCommandType, commandMock.Object);

        // Assert
        result.Should().Be(0);
        commandMock.VerifyAll();
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
        var commandMock = MockCommand();

        // Act
        var result = CommandsUtils.IsCommandRegistered(null, commandMock.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.IsCommandRegistered(MakeHandler(), null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandNameIsInvalid(string? commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.IsCommandRegistered(MakeHandler(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void IsCommandRegistered_ShouldReturnNull_WhenCommandHasInvalidAlias(string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.IsCommandRegistered(MakeHandler(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_exampleCommands))]
    public void IsCommandRegistered_ShouldReturnProperResult_WhenGoldFlow(ICommand command)
    {
        // Arrange
        var handler = MakeHandler();
        var expectedResult = handler.TryGetCommand(command.Command, out _);

        // Act
        var result = CommandsUtils.IsCommandRegistered(handler, command);

        // Assert
        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(_allHandlerTypes))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandIsNull(CommandType handlerType)
    {
        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string? commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidAliases))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(CommandType handlerType, string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void RegisterCommand_ShouldReturnZero_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName, null);

        // Act
        var result = CommandsUtils.RegisterCommand(InvalidCommandType, commandMock.Object);

        // Assert
        result.Should().Be(0);
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidHandlersXCommandsToRegister))]
    public void RegisterCommand_ShouldProperlyRegister_WhenGoldFlow(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName, null);
        var handlers = GetExpectedCommandHandlers(handlerType);
        var expectedResult = JoinCommandTypes(handlers.Where(h => !h.TryGetCommand(commandName, out _)).Select(GetCommandHandlerType));

        // Act
        var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().Be(expectedResult);
        commandMock.VerifyAll();
        handlers.Select(h => h.TryGetCommand(commandName, out _)).Should().NotContain(false);
    }

    [Test]
    public void RegisterCommand_ShouldReturnNull_WhenCommandHandlerIsNull()
    {
        // Arrange
        var commandMock = MockCommand();

        // Act
        var result = CommandsUtils.RegisterCommand(null, commandMock.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void RegisterCommand_ShouldReturnNull_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.RegisterCommand(MakeHandler(), null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(string commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.RegisterCommand(MakeHandler(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void RegisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.RegisterCommand(MakeHandler(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_exampleCommands))]
    public void RegisterCommand_ShouldProperlyRegister_WhenGoldFlow(ICommand command)
    {
        // Arrange
        var handler = MakeHandler();
        var expectedResult = !handler.TryGetCommand(command.Command, out _);

        // Act
        var result = CommandsUtils.RegisterCommand(handler, command);

        // Assert
        result.Should().Be(expectedResult);
        handler.TryGetCommand(command.Command, out _).Should().BeTrue();
    }

    [TestCaseSource(nameof(_allHandlerTypes))]
    public void UnegisterCommand_ShouldReturnNull_WhenCommandIsNull(CommandType handlerType)
    {
        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidCommands))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandType handlerType, string? commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(AllHandlersXInvalidAliases))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(CommandType handlerType, string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_validCommandNames))]
    public void UnregisterCommand_ShouldReturnZero_WhenCommandHandlerIsNotFound(string commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName, null);

        // Act
        var result = CommandsUtils.UnregisterCommand(InvalidCommandType, commandMock.Object);

        // Assert
        result.Should().Be(0);
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(ValidHandlersXCommandsToRegister))]
    public void UnregisterCommand_ShouldProperlyUnregister_WhenGoldFlow(CommandType handlerType, string commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName, null);
        var handlers = GetExpectedCommandHandlers(handlerType);
        var expectedResult = JoinCommandTypes(handlers.Where(h => h.TryGetCommand(commandName, out _)).Select(GetCommandHandlerType));

        // Act
        var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

        // Assert
        result.Should().Be(expectedResult);
        commandMock.VerifyAll();
        handlers.Select(h => h.TryGetCommand(commandName, out _)).Should().NotContain(true);
    }

    [Test]
    public void UnegisterCommand_ShouldReturnNull_WhenCommandHandlerIsNull()
    {
        // Arrange
        var commandMock = MockCommand();

        // Act
        var result = CommandsUtils.UnregisterCommand(null, commandMock.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void UnegisterCommand_ShouldReturnNull_WhenCommandIsNull()
    {
        // Act
        var result = CommandsUtils.UnregisterCommand(MakeHandler(), null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_invalidCommandNames))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandNameIsInvalid(string? commandName)
    {
        // Arrange
        var commandMock = MockCommand(commandName);

        // Act
        var result = CommandsUtils.UnregisterCommand(MakeHandler(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_invalidAliases))]
    public void UnregisterCommand_ShouldReturnNull_WhenCommandHasInvalidAlias(string?[] aliases)
    {
        // Arrange
        var commandMock = MockCommand(MockCommandName, aliases);

        // Act
        var result = CommandsUtils.UnregisterCommand(MakeHandler(), commandMock.Object);

        // Assert
        result.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_exampleCommands))]
    public void UnregisterCommand_ShouldProperlyUnregister_WhenGoldFlow(ICommand command)
    {
        // Arrange
        var handler = MakeHandler();
        var expectedResult = handler.TryGetCommand(command.Command, out _);

        // Act
        var result = CommandsUtils.UnregisterCommand(handler, command);

        // Assert
        result.Should().Be(expectedResult);
        handler.TryGetCommand(command.Command, out _).Should().BeFalse();
    }
}
