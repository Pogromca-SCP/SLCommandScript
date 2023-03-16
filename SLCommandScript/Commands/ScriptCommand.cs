using CommandSystem;
using System;
using System.IO;
using SLCommandScript.Interpreter;
using SLCommandScript;

namespace SLCommandScript.Commands
{
    /// <summary>
    /// Command used to launch interpreted scripts
    /// </summary>
    public class ScriptCommand : ICommand
    {
        /// <summary>
        /// Contains command context
        /// </summary>
        public CommandContextType ContextType { get; private set; }

        /// <summary>
        /// Contains command name
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Defines command aliases
        /// </summary>
        public string[] Aliases => null;

        /// <summary>
        /// Contains command description
        /// </summary>
        public string Description => $"Executes custom script named {Command}.scl";

        /// <summary>
        /// Initializes the command
        /// </summary>
        /// <param name="name">Name of associated script</param>
        public ScriptCommand(string name, CommandContextType context)
        {
            Command = name;
            ContextType = context;
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">Command arguments provided by sender</param>
        /// <param name="sender">Command sender</param>
        /// <param name="response">Response to display in sender's console</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is null)
            {
                response = "Command sender is null.";
                return false;
            }

            if (!File.Exists($"{Plugin.ScriptsPath}{Command}"))
            {
                response = "Script file does not exist or cannot be accessed.";
                return false;
            }

            return new SCLInterpreterBase().ProcessLines(File.ReadAllLines($"{Plugin.ScriptsPath}{Command}"), sender, ContextType, out response);
        }
    }
}
