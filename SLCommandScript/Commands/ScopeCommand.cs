using CommandSystem;
using System;
using SLCommandScript.Commands.ScopeCommands;
using System.Text;
using SLCommandScript.Core.Commands;

namespace SLCommandScript.Commands
{
    /// <summary>
    /// Provides handlers control commands
    /// </summary>
    public class ScopeCommand : ParentCommand, IUsageProvider
    {
        /// <summary>
        /// Contains command name
        /// </summary>
        public override string Command { get; } = "scope";

        /// <summary>
        /// Defines command aliases
        /// </summary>
        public override string[] Aliases => null;

        /// <summary>
        /// Contains command description
        /// </summary>
        public override string Description { get; } = "Runs provided command if its registered in specific command handler.";

        /// <summary>
        /// Defines command usage prompts
        /// </summary>
        public string[] Usage { get; } = new[] { "Command Handler", "Command" };

        /// <summary>
        /// Initializes the command
        /// </summary>
        public ScopeCommand() => LoadGeneratedCommands();

        /// <summary>
        /// Loads subcommands
        /// </summary>
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new RemoteAdminScopeCommand());
            RegisterCommand(new ServerConsoleScopeCommand());
            RegisterCommand(new ClientConsoleScopeCommand());
        }

        /// <summary>
        /// Executes the parent command
        /// </summary>
        /// <param name="arguments">Command arguments provided by sender</param>
        /// <param name="sender">Command sender</param>
        /// <param name="response">Response to display in sender's console</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var sb = new StringBuilder("Available command handlers:\n");

            foreach (var command in AllCommands)
            {
                sb.AppendLine($" - {command.Command} <color=grey>Aliases: {command.Aliases}</color>");
            }

            response = sb.ToString();
            return true;
        }
    }

    /// <summary>
    /// Base class for handler commands
    /// </summary>
    public abstract class HandlerCommandBase : IUsageProvider
    {
        /// <summary>
        /// Defines command usage prompts
        /// </summary>
        public string[] Usage { get; } = new[] { "Command" };

        /// <summary>
        /// Executes the command from specific handler
        /// </summary>
        /// <param name="handlerType">Type of command handler to get command from</param>
        /// <param name="arguments">Command arguments provided by sender</param>
        /// <param name="sender">Command sender</param>
        /// <param name="response">Response to display in sender's console</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        protected bool RunCommandInHandler(CommandHandlerType handlerType, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = $"To execute this command provide at least 1 argument!\nUsage: {this.DisplayCommandUsage()}";
                return false;
            }

            var cmdName = arguments.Array[arguments.Offset];
            var command = CommandsUtils.GetCommand(handlerType, cmdName);

            if (command is null)
            {
                response = $"Command '<color=green>{cmdName}</color>' not found.";
                return false;
            }

            return command.Execute(new ArraySegment<string>(arguments.Array, arguments.Offset + 1, arguments.Count - 1), sender, out response);
        }
    }
}
