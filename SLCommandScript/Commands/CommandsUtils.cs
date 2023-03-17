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

            ICommand command = null;
            var handler = GetCommandHandler(CommandHandlerType.RemoteAdmin);
            handler?.TryGetCommand(commandName, out command);

            if (command is null)
            {
                //
            }
        }

        /// <summary>
        /// Registers a command into specific handler
        /// </summary>
        /// <param name="handlerType">Command handler to register into</param>
        /// <param name="command">Command to register</param>
        /// <returns>True if command and handler were valid, false otherwise</returns>
        public static bool RegisterCommand(CommandHandlerType handlerType, ICommand command) => ManageCommand(handlerType, command, true);

        /// <summary>
        /// Unregisters a command from specific handler
        /// </summary>
        /// <param name="handlerType">Command handler to unregister from</param>
        /// <param name="command">Command to unregister</param>
        /// <returns>True if command and handler were valid, false otherwise</returns>
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
        /// <returns>True if command and handler were valid, false otherwise</returns>
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
    }
}
