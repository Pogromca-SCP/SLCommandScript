using CommandSystem;
using RemoteAdmin;
using GameCore;

namespace SLCommandScript.Commands
{
    /// <summary>
    /// Provides additional utilities for commands
    /// </summary>
    public static class CommandsUtils
    {
        /// <summary>
        /// Defines command handlers hierarchy for command searches
        /// </summary>
        private static readonly CommandHandlerType[] _hanldersHierarchy = { CommandHandlerType.RemoteAdmin, CommandHandlerType.ServerConsole,
            CommandHandlerType.ClientConsole };

        /// <summary>
        /// Attempts to find a command
        /// </summary>
        /// <param name="commandName">Name or alias of the command to find</param>
        /// <returns>Found command or null if nothing was found</returns>
        public static ICommand FindCommand(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return null;
            }

            foreach (var handlerType in _hanldersHierarchy)
            {
                var command = GetCommand(handlerType, commandName);
                
                if (!(command is null))
                {
                    return command;
                }
            }

            return null;
        }

        /// <summary>
        /// Registers a command into specific handler
        /// </summary>
        /// <param name="handlerType">Command handler to register into</param>
        /// <param name="command">Command to register</param>
        /// <returns>True if command was registered, false otherwise</returns>
        public static bool RegisterCommand(CommandHandlerType handlerType, ICommand command) => ManageCommand(handlerType, command, true);

        /// <summary>
        /// Registers a command into specific handler only if its not already registered
        /// </summary>
        /// <param name="handlerType">Command handler to register into</param>
        /// <param name="command">Command to register</param>
        /// <returns>True if command was registered, false otherwise</returns>
        public static bool RegisterCommandIfMissing(CommandHandlerType handlerType, ICommand command)
        {
            if (command is null || string.IsNullOrEmpty(command.Command))
            {
                return false;
            }

            if (IsCommandRegistered(handlerType, command))
            {
                return false;
            }

            return RegisterCommand(handlerType, command);
        }

        /// <summary>
        /// Unregisters a command from specific handler
        /// </summary>
        /// <param name="handlerType">Command handler to unregister from</param>
        /// <param name="command">Command to unregister</param>
        /// <returns>True if command was unregistered, false otherwise</returns>
        public static bool UnregisterCommand(CommandHandlerType handlerType, ICommand command) => ManageCommand(handlerType, command, false);

        /// <summary>
        /// Returns appropriate command handler
        /// </summary>
        /// <param name="handlerType">Type of required command handler</param>
        /// <returns>Command hanlder for provided context or null if no such hanlder exists</returns>
        private static ICommandHandler GetCommandHandler(CommandHandlerType handlerType)
        {
            switch (handlerType)
            {
                case CommandHandlerType.RemoteAdmin:
                    return CommandProcessor.RemoteAdminCommandHandler;
                case CommandHandlerType.ServerConsole:
                    return Console.singleton?.ConsoleCommandHandler;
                case CommandHandlerType.ClientConsole:
                    return QueryProcessor.DotCommandHandler;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Manages command registration for specific handler
        /// </summary>
        /// <param name="handlerType">Command handler to manage</param>
        /// <param name="command">Command to register/unregister</param>
        /// <param name="doRegister">Set to true to register a command, set to false to unregister</param>
        /// <returns>True if command management succeded, false otherwise</returns>
        private static bool ManageCommand(CommandHandlerType handlerType, ICommand command, bool doRegister)
        {
            if (command is null || string.IsNullOrWhiteSpace(command.Command))
            {
                return false;
            }

            var handler = GetCommandHandler(handlerType);

            if (handler is null)
            {
                return false;
            }
            
            if (doRegister)
            {
                handler.RegisterCommand(command);
                return true;
            }

            handler.UnregisterCommand(command);
            return true;
        }

        /// <summary>
        /// Attempts to get a command from specific handler
        /// </summary>
        /// <param name="handlerType">Command hanlder to search in</param>
        /// <param name="commandName">Name or alias of the command to get</param>
        /// <returns>Found command or null if nothing was found</returns>
        private static ICommand GetCommand(CommandHandlerType handlerType, string commandName)
        {
            var handler = GetCommandHandler(handlerType);

            if (handler is null)
            {
                return null;
            }

            var commandFound = handler.TryGetCommand(commandName, out var command);
            return commandFound ? command : null;
        }

        /// <summary>
        /// Checks if provided command is already registered in specific handler
        /// </summary>
        /// <param name="handlerType">Handler type to check</param>
        /// <param name="command">Command to check</param>
        /// <returns>True if command is registered already, false otherwise</returns>
        private static bool IsCommandRegistered(CommandHandlerType handlerType, ICommand command)
        {
            var foundCommand = GetCommand(handlerType, command.Command);

            if (!(foundCommand is null))
            {
                return true;
            }

            if (command.Aliases is null || command.Aliases.Length < 1)
            {
                return false;
            }

            foreach (var alias in command.Aliases)
            {
                foundCommand = GetCommand(handlerType, alias);

                if (!(foundCommand is null))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
