using CommandSystem;
using System;
using System.Text;

namespace SLCommandScript.Commands
{
    /// <summary>
    /// Provides flow control commands
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]
    public class FlowCommand : ParentCommand, IUsageProvider
    {
        /// <summary>
        /// Contains command name
        /// </summary>
        public override string Command { get; } = "flow";

        /// <summary>
        /// Defines command aliases
        /// </summary>
        public override string[] Aliases { get; } = null;

        /// <summary>
        /// Contains command description
        /// </summary>
        public override string Description { get; } = "Provides flow control utilities for commands.";

        /// <summary>
        /// Defines command usage prompts
        /// </summary>
        public string[] Usage { get; } = new[] { "Operation", "Operation Arguments" };

        /// <summary>
        /// Initializes the command
        /// </summary>
        public FlowCommand() => LoadGeneratedCommands();

        /// <summary>
        /// Loads subcommands
        /// </summary>
        public override void LoadGeneratedCommands()
        {
            //
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
            if (sender is null)
            {
                response = "Command sender is null.";
                return false;
            }

            var sb = new StringBuilder("Available flow control operations:\n");

            foreach (var command in AllCommands)
            {
                sb.AppendLine($" - {command.Command} {command.Description}");
            }

            response = sb.ToString();
            return true;
        }
    }
}
