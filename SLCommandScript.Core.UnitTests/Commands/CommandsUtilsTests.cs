using SLCommandScript.Core.Commands;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using RemoteAdmin;
using NUnit.Framework;
using FluentAssertions;
using Moq;

namespace SLCommandScript.Core.UnitTests.Commands
{
    public class CommandsUtilsTests
    {
        #region Test Case Sources
        private static readonly CommandHandlerType[] _allHandlerTypes = { CommandHandlerType.RemoteAdmin, CommandHandlerType.ServerConsole,
            CommandHandlerType.ClientConsole };

        private static readonly string[] _invalidCommandNames = { null, "", " ", " \t ", "  \t  \t\t" };

        private static readonly string[] _validCommandNames = { "hello", "item list", "?.cassie" };

        private static readonly CommandHandlerType[] _validHandlerTypes = { CommandHandlerType.RemoteAdmin, CommandHandlerType.ClientConsole };

        private static readonly string[] _existingCommandNames = { "help", "bc", "cassie" };

        private static readonly string[][] _existingCommands = { new[] { "clearcassie", "cassieclear" }, new[] { "help" },
            new[] { "096state", "state096", "state" } };

        private static IEnumerable<object[]> _allHandlersXInvalidCommands => JoinArrays(_allHandlerTypes, _invalidCommandNames);

        private static IEnumerable<object[]> _validHandlersXExistingCommandNames => JoinArrays(_validHandlerTypes, _existingCommandNames);

        private static IEnumerable<object[]> _validHandlersXExistingCommands => JoinArrays(_validHandlerTypes, _existingCommands);

        private static IEnumerable<object[]> JoinArrays(CommandHandlerType[] first, string[] second) =>
            first.SelectMany(f => second.Select(s => new object[] { f, s }));

        private static IEnumerable<object[]> JoinArrays(CommandHandlerType[] first, string[][] second) =>
            first.SelectMany(f => second.Select(s => new object[] { f }.Concat(s).ToArray()));
        #endregion

        private static ICommandHandler GetCommandHandler(CommandHandlerType handlerType)
        {
            switch (handlerType)
            {
                case CommandHandlerType.RemoteAdmin:
                    return CommandProcessor.RemoteAdminCommandHandler;
                case CommandHandlerType.ClientConsole:
                    return QueryProcessor.DotCommandHandler;
                default:
                    return null;
            }
        }

        #region GetCommandHandler Tests
        [TestCaseSource(nameof(_allHandlerTypes))]
        public void GetCommandHandler_ShouldReturnProperHandler(CommandHandlerType handlerType)
        {
            // Arrange
            var expectedHandler = GetCommandHandler(handlerType);

            // Act
            var result = CommandsUtils.GetCommandHandler(handlerType);

            // Assert
            result.Should().Be(expectedHandler);
        }
        #endregion

        #region GetCommand Tests
        [TestCaseSource(nameof(_allHandlersXInvalidCommands))]
        public void GetCommand_ShouldReturnNull_WhenCommandNameIsInvalid(CommandHandlerType handlerType, string commandName)
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
            var result = CommandsUtils.GetCommand(CommandHandlerType.ServerConsole, commandName);

            // Assert
            result.Should().BeNull();
        }

        [TestCaseSource(nameof(_validHandlersXExistingCommandNames))]
        public void GetCommand_ShouldReturnProperResult(CommandHandlerType handlerType, string commandName)
        {
            // Arrange
            var handler = GetCommandHandler(handlerType);
            var commandExists = handler.TryGetCommand(commandName, out var foundCommand);

            // Act
            var result = CommandsUtils.GetCommand(handlerType, commandName);

            // Assert
            if (commandExists)
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
        public void IsCommandRegistered_ShouldReturnFalse_WhenCommandIsNull(CommandHandlerType handlerType)
        {
            // Act
            var result = CommandsUtils.IsCommandRegistered(handlerType, null);

            // Assert
            result.Should().BeFalse();
        }

        [TestCaseSource(nameof(_validHandlersXExistingCommands))]
        public void IsCommandRegistered_ShouldReturnProperResult(CommandHandlerType handlerType, string commandName, params string[] aliases)
        {
            System.Console.WriteLine($"{handlerType} - {commandName} - {aliases}");
            // Arrange
            if (aliases.Length == 0)
            {
                aliases = null;
            }

            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName).Verifiable();
            commandMock.Setup(x => x.Aliases).Returns(aliases);
            var handler = GetCommandHandler(handlerType);

            var expectedResult = handler.AllCommands.Any(cmd => cmd.Command.Equals(commandName) && (cmd.Aliases == aliases ||
                Enumerable.SequenceEqual(cmd.Aliases, aliases)));

            // Act
            var result = CommandsUtils.IsCommandRegistered(handlerType, commandMock.Object);

            // Assert
            result.Should().Be(expectedResult);
            commandMock.Verify();
        }
        #endregion

        #region RegisterCommand Tests
        [TestCase(CommandHandlerType.RemoteAdmin)]
        [TestCase(CommandHandlerType.ServerConsole)]
        [TestCase(CommandHandlerType.ClientConsole)]
        public void RegisterCommand_ShouldReturnFalse_WhenCommandIsNull(CommandHandlerType handlerType)
        {
            // Act
            var result = CommandsUtils.RegisterCommand(handlerType, null);

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(CommandHandlerType.RemoteAdmin, null)]
        [TestCase(CommandHandlerType.ServerConsole, null)]
        [TestCase(CommandHandlerType.ClientConsole, null)]
        [TestCase(CommandHandlerType.RemoteAdmin, "")]
        [TestCase(CommandHandlerType.ServerConsole, "")]
        [TestCase(CommandHandlerType.ClientConsole, "")]
        [TestCase(CommandHandlerType.RemoteAdmin, " \t ")]
        [TestCase(CommandHandlerType.ServerConsole, " \t ")]
        [TestCase(CommandHandlerType.ClientConsole, " \t ")]
        public void RegisterCommand_ShouldReturnFalse_WhenCommandNameIsInvalid(CommandHandlerType handlerType, string commandName)
        {
            // Arrange
            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);

            // Act
            var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

            // Assert
            result.Should().BeFalse();
            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }

        [TestCase("test")]
        [TestCase("bruh")]
        [TestCase("?exampleCommand")]
        public void RegisterCommand_ShouldReturnFalse_WhenCommandHandlerIsNotFound(string commandName)
        {
            // Arrange
            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);

            // Act
            var result = CommandsUtils.RegisterCommand(CommandHandlerType.ServerConsole, commandMock.Object);

            // Assert
            result.Should().BeFalse();
            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }

        [TestCase(CommandHandlerType.RemoteAdmin, "funnycommand", "funny")]
        [TestCase(CommandHandlerType.ClientConsole, "funnycommand", "funny")]
        [TestCase(CommandHandlerType.RemoteAdmin, "bruh")]
        [TestCase(CommandHandlerType.ClientConsole, "bruh")]
        [TestCase(CommandHandlerType.RemoteAdmin, "testcommand", "testing", "test")]
        [TestCase(CommandHandlerType.ClientConsole, "testcommand", "testing", "test")]
        public void RegisterCommand_ShouldProperlyRegister_WhenGoldFlow(CommandHandlerType handlerType, string commandName, params string[] aliases)
        {
            // Arrange
            if (aliases.Length == 0)
            {
                aliases = null;
            }

            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);
            commandMock.Setup(x => x.Aliases).Returns(aliases);

            var handler = GetCommandHandler(handlerType);

            // Act
            var result = CommandsUtils.RegisterCommand(handlerType, commandMock.Object);

            // Assert
            result.Should().BeTrue();
            var registered = handler.TryGetCommand(commandName, out var command);
            registered.Should().BeTrue();
            command.Should().Be(commandMock.Object);

            if (!(aliases is null))
            {
                foreach (var alias in aliases)
                {
                    registered = handler.TryGetCommand(alias, out command);
                    registered.Should().BeTrue();
                    command.Should().Be(commandMock.Object);
                }
            }

            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }
        #endregion

        #region UnregisterCommand Tests
        [TestCase(CommandHandlerType.RemoteAdmin)]
        [TestCase(CommandHandlerType.ServerConsole)]
        [TestCase(CommandHandlerType.ClientConsole)]
        public void UnregisterCommand_ShouldReturnFalse_WhenCommandIsNull(CommandHandlerType handlerType)
        {
            // Act
            var result = CommandsUtils.UnregisterCommand(handlerType, null);

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(CommandHandlerType.RemoteAdmin, null)]
        [TestCase(CommandHandlerType.ServerConsole, null)]
        [TestCase(CommandHandlerType.ClientConsole, null)]
        [TestCase(CommandHandlerType.RemoteAdmin, "")]
        [TestCase(CommandHandlerType.ServerConsole, "")]
        [TestCase(CommandHandlerType.ClientConsole, "")]
        [TestCase(CommandHandlerType.RemoteAdmin, " \t ")]
        [TestCase(CommandHandlerType.ServerConsole, " \t ")]
        [TestCase(CommandHandlerType.ClientConsole, " \t ")]
        public void UnregisterCommand_ShouldReturnFalse_WhenCommandNameIsInvalid(CommandHandlerType handlerType, string commandName)
        {
            // Arrange
            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);

            // Act
            var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

            // Assert
            result.Should().BeFalse();
            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }

        [TestCase("test")]
        [TestCase("bruh")]
        [TestCase("?exampleCommand")]
        public void UnregisterCommand_ShouldReturnFalse_WhenCommandHandlerIsNotFound(string commandName)
        {
            // Arrange
            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);

            // Act
            var result = CommandsUtils.UnregisterCommand(CommandHandlerType.ServerConsole, commandMock.Object);

            // Assert
            result.Should().BeFalse();
            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }

        [TestCase(CommandHandlerType.RemoteAdmin, "funnycommand", "funny")]
        [TestCase(CommandHandlerType.ClientConsole, "funnycommand", "funny")]
        [TestCase(CommandHandlerType.RemoteAdmin, "bruh")]
        [TestCase(CommandHandlerType.ClientConsole, "bruh")]
        [TestCase(CommandHandlerType.RemoteAdmin, "testcommand", "testing", "test")]
        [TestCase(CommandHandlerType.ClientConsole, "testcommand", "testing", "test")]
        public void UnregisterCommand_ShouldProperlyRegister_WhenGoldFlow(CommandHandlerType handlerType, string commandName, params string[] aliases)
        {
            // Arrange
            if (aliases.Length == 0)
            {
                aliases = null;
            }

            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);
            commandMock.Setup(x => x.Aliases).Returns(aliases);

            var handler = GetCommandHandler(handlerType);

            // Act
            var result = CommandsUtils.UnregisterCommand(handlerType, commandMock.Object);

            // Assert
            result.Should().BeTrue();
            var registered = handler.TryGetCommand(commandName, out var command);
            registered.Should().BeFalse();

            if (!(aliases is null))
            {
                foreach (var alias in aliases)
                {
                    registered = handler.TryGetCommand(alias, out command);
                    registered.Should().BeFalse();
                }
            }

            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }
        #endregion

        #region RegisterCommandIfMissing Tests
        [TestCase(CommandHandlerType.RemoteAdmin)]
        [TestCase(CommandHandlerType.ServerConsole)]
        [TestCase(CommandHandlerType.ClientConsole)]
        public void RegisterCommandIfMissing_ShouldReturnFalse_WhenCommandIsNull(CommandHandlerType handlerType)
        {
            // Act
            var result = CommandsUtils.RegisterCommandIfMissing(handlerType, null);

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(CommandHandlerType.RemoteAdmin, null)]
        [TestCase(CommandHandlerType.ServerConsole, null)]
        [TestCase(CommandHandlerType.ClientConsole, null)]
        [TestCase(CommandHandlerType.RemoteAdmin, "")]
        [TestCase(CommandHandlerType.ServerConsole, "")]
        [TestCase(CommandHandlerType.ClientConsole, "")]
        [TestCase(CommandHandlerType.RemoteAdmin, " \t ")]
        [TestCase(CommandHandlerType.ServerConsole, " \t ")]
        [TestCase(CommandHandlerType.ClientConsole, " \t ")]
        public void RegisterCommandIfMissing_ShouldReturnFalse_WhenCommandNameIsInvalid(CommandHandlerType handlerType, string commandName)
        {
            // Arrange
            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);

            // Act
            var result = CommandsUtils.RegisterCommandIfMissing(handlerType, commandMock.Object);

            // Assert
            result.Should().BeFalse();
            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }

        [TestCase("test")]
        [TestCase("bruh")]
        [TestCase("?exampleCommand")]
        public void RegisterCommandIfMissing_ShouldReturnFalse_WhenCommandHandlerIsNotFound(string commandName)
        {
            // Arrange
            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);

            // Act
            var result = CommandsUtils.RegisterCommandIfMissing(CommandHandlerType.ServerConsole, commandMock.Object);

            // Assert
            result.Should().BeFalse();
            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }

        [TestCase(CommandHandlerType.RemoteAdmin, "funnycommand", "funny")]
        [TestCase(CommandHandlerType.ClientConsole, "funnycommand", "funny")]
        [TestCase(CommandHandlerType.RemoteAdmin, "bruh")]
        [TestCase(CommandHandlerType.ClientConsole, "bruh")]
        [TestCase(CommandHandlerType.RemoteAdmin, "testcommand", "testing", "test")]
        [TestCase(CommandHandlerType.ClientConsole, "testcommand", "testing", "test")]
        public void RegisterCommandIfMissing_ShouldProperlyRegister_WhenGoldFlow(CommandHandlerType handlerType, string commandName, params string[] aliases)
        {
            // Arrange
            if (aliases.Length == 0)
            {
                aliases = null;
            }

            var commandMock = new Mock<ICommand>(MockBehavior.Strict);
            commandMock.Setup(x => x.Command).Returns(commandName);
            commandMock.Setup(x => x.Aliases).Returns(aliases);

            var handler = GetCommandHandler(handlerType);

            // Act
            var result = CommandsUtils.RegisterCommandIfMissing(handlerType, commandMock.Object);

            // Assert
            result.Should().BeTrue();
            var registered = handler.TryGetCommand(commandName, out var command);
            registered.Should().BeTrue();
            command.Should().Be(commandMock.Object);

            if (!(aliases is null))
            {
                foreach (var alias in aliases)
                {
                    registered = handler.TryGetCommand(alias, out command);
                    registered.Should().BeTrue();
                    command.Should().Be(commandMock.Object);
                }
            }

            commandMock.VerifyAll();
            commandMock.VerifyNoOtherCalls();
        }
        #endregion
    }
}
