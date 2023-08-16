using PluginAPI.Enums;
using System.Collections.Generic;
using CommandSystem;
using System.Linq;
using RemoteAdmin;
using GameCore;

namespace SLCommandScript.Core.Commands;

/// <summary>
/// Provides additional utilities for commands.
/// </summary>
public static class CommandsUtils
{
    /// <summary>
    /// Defines command handlers hierarchy for command searches.
    /// </summary>
    private static readonly CommandType[] _handlersHierarchy = { CommandType.RemoteAdmin, CommandType.Console, CommandType.GameConsole };

    /// <summary>
    /// Retrieves all appropriate command handlers for specific command type.
    /// </summary>
    /// <param name="commandType">Command type to handle.</param>
    /// <returns>All valid command handlers for provided command type.</returns>
    public static IEnumerable<ICommandHandler> GetCommandHandlers(CommandType commandType) =>
        _handlersHierarchy.Where(t => (t & commandType) != 0).Select(GetCommandHandler).Where(h => h is not null);

    /// <summary>
    /// Checks if provided command is invalid.
    /// </summary>
    /// <param name="command">Command to check.</param>
    /// <returns><see langword="true" /> if command is invalid, <see langword="false" /> otherwise.</returns>
    public static bool IsCommandInvalid(ICommand command) => command is null || string.IsNullOrWhiteSpace(command.Command)
        || (command.Aliases is not null && command.Aliases.Any(string.IsNullOrWhiteSpace));

    /// <summary>
    /// Registers a command of specific type.
    /// </summary>
    /// <param name="commandType">Type of registered command.</param>
    /// <param name="command">Command to register.</param>
    /// <returns>Types of command handlers the command was registered to or <see langword="null" /> if command is invalid.</returns>
    public static CommandType? RegisterCommand(CommandType commandType, ICommand command)
    {
        var registered = IsCommandRegistered(commandType, command);
        return registered == null ? null : ManageCommand(commandType ^ registered.Value, command, true);
    }

    /// <summary>
    /// Unregisters a command of specific type.
    /// </summary>
    /// <param name="commandType">Type of command to unregister.</param>
    /// <param name="command">Command to unregister.</param>
    /// <returns>Types of command handlers the command was unregistered from or <see langword="null" /> if command is invalid.</returns>
    public static CommandType? UnregisterCommand(CommandType commandType, ICommand command)
    {
        var registered = IsCommandRegistered(commandType, command);
        return registered == null ? null : ManageCommand(registered.Value, command, false);
    }

    /// <summary>
    /// Attempts to get a command from specific handlers.
    /// </summary>
    /// <param name="commandType">Command handlers to search in.</param>
    /// <param name="commandName">Name or alias of the command to get.</param>
    /// <returns>Found command or <see langword="null" /> if nothing was found.</returns>
    public static ICommand GetCommand(CommandType commandType, string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return null;
        }

        foreach (var handler in GetCommandHandlers(commandType))
        {
            var commandFound = handler.TryGetCommand(commandName, out var command);

            if (commandFound)
            {
                return command;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if provided command is already registered in specific handlers.
    /// </summary>
    /// <param name="commandType">Handler types to check.</param>
    /// <param name="command">Command to check.</param>
    /// <returns>Types of command handlers where the command is already registered in or <see langword="null" /> if command is invalid.</returns>
    public static CommandType? IsCommandRegistered(CommandType commandType, ICommand command)
    {
        if (IsCommandInvalid(command))
        {
            return null;
        }

        CommandType result = 0;

        foreach (var handler in GetCommandHandlers(commandType))
        {
            var isFound = handler.TryGetCommand(command.Command, out var foundCommand);

            if (isFound && command.Aliases is not null)
            {
                foreach (var alias in command.Aliases)
                {
                    isFound = handler.TryGetCommand(alias, out var aliasResult);

                    if (!isFound || foundCommand != aliasResult)
                    {
                        isFound = false;
                        break;
                    }
                }
            }

            if (isFound)
            {
                result |= GetCommandType(handler);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns appropriate command handler.
    /// </summary>
    /// <param name="commandType">Type of required command handler.</param>
    /// <returns>Command handler of provided type or <see langword="null" /> if no such handler exists.</returns>
    private static ICommandHandler GetCommandHandler(CommandType commandType) => commandType switch
    {
        CommandType.RemoteAdmin => CommandProcessor.RemoteAdminCommandHandler,
        CommandType.Console => Console.singleton?.ConsoleCommandHandler,
        CommandType.GameConsole => QueryProcessor.DotCommandHandler,
        _ => null,
    };

    /// <summary>
    /// Returns appropriate command handler type.
    /// </summary>
    /// <param name="commandHandler">Command handler to get type of.</param>
    /// <returns>Type of provided command handler or 0 if provided handler is invalid.</returns>
    private static CommandType GetCommandType(ICommandHandler commandHandler) => commandHandler switch
    {
        RemoteAdminCommandHandler => CommandType.RemoteAdmin,
        GameConsoleCommandHandler => CommandType.Console,
        ClientCommandHandler => CommandType.GameConsole,
        _ => 0
    };

    /// <summary>
    /// Manages command registration or unregistration process.
    /// </summary>
    /// <param name="commandType">Type of command to manage.</param>
    /// <param name="command">Command to register/unregister.</param>
    /// <param name="doRegister">Set to <see langword="true" /> to register a command, set to <see langword="false" /> to unregister.</param>
    /// <returns>Types of affected command handlers.</returns>
    private static CommandType ManageCommand(CommandType commandType, ICommand command, bool doRegister)
    {
        CommandType result = 0;

        foreach (var handler in GetCommandHandlers(commandType))
        {
            if (doRegister)
            {
                handler.RegisterCommand(command);
            }
            else
            {
                handler.UnregisterCommand(command);
            }

            result |= GetCommandType(handler);
        }

        return result;
    }
}
