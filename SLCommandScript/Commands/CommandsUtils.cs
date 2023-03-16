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
        /// Attempts to find a command in specific context
        /// </summary>
        /// <param name="context">Command context from which the command should be available</param>
        /// <param name="commandName">Name or alias of the command to find</param>
        /// <returns>Found command or null if nothing was found</returns>
        public static ICommand FindCommand(CommandContextType context, string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return null;
            }

            var handler = GetCommandHandler(context);

            if (handler is null)
            {
                return null;
            }

            handler.TryGetCommand(commandName, out var result);
            return result;
        }

        /// <summary>
        /// Registers a command into specific context
        /// </summary>
        /// <param name="context">Command context to register into</param>
        /// <param name="command">Command to register</param>
        /// <returns>True if command and handler were valid, false otherwise</returns>
        public static bool RegisterCommand(CommandContextType context, ICommand command) => ManageCommand(context, command, true);

        /// <summary>
        /// Unregisters a command from specific context
        /// </summary>
        /// <param name="context">Command context to unregister from</param>
        /// <param name="command">Command to unregister</param>
        /// <returns>True if command and handler were valid, false otherwise</returns>
        public static bool UnregisterCommand(CommandContextType context, ICommand command) => ManageCommand(context, command, false);

        /// <summary>
        /// Returns appropriate command handler for specific context
        /// </summary>
        /// <param name="context">Command context which requires the handler</param>
        /// <returns>Command hanlder for provided context or null if no such hanlder exists</returns>
        private static ICommandHandler GetCommandHandler(CommandContextType context)
        {
            switch (context)
            {
                case CommandContextType.RemoteAdmin:
                    return CommandProcessor.RemoteAdminCommandHandler;
                case CommandContextType.ServerConsole:
                    return Console.singleton?.ConsoleCommandHandler;
                case CommandContextType.ClientConsole:
                    return QueryProcessor.DotCommandHandler;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Manages command registration in specific context
        /// </summary>
        /// <param name="context">Command context to manage</param>
        /// <param name="command">Command to register/unregister</param>
        /// <param name="doRegister">Set to true to register a command, set to false to unregister</param>
        /// <returns>True if command and handler were valid, false otherwise</returns>
        private static bool ManageCommand(CommandContextType context, ICommand command, bool doRegister)
        {
            if (command is null || string.IsNullOrWhiteSpace(command.Command))
            {
                return false;
            }

            var handler = GetCommandHandler(context);

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
