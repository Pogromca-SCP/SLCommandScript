using CommandSystem;
using System;
using SLCommandScript.Core.Commands;

namespace SLCommandScript.Commands.ScopeCommands
{
    /// <summary>
    /// Server console scope command
    /// </summary>
    public class ServerConsoleScopeCommand : HandlerCommandBase, ICommand
    {
        /// <summary>
        /// Contains command name
        /// </summary>
        public string Command { get; } = "serverconsole";

        /// <summary>
        /// Defines command aliases
        /// </summary>
        public string[] Aliases { get; } = new[] { "server" };

        /// <summary>
        /// Contains command description
        /// </summary>
        public string Description { get; } = "Runs provided command if its registered in server console.";

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">Command arguments provided by sender</param>
        /// <param name="sender">Command sender</param>
        /// <param name="response">Response to display in sender's console</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) =>
            RunCommandInHandler(CommandHandlerType.ServerConsole, arguments, sender, out response);
    }
}
