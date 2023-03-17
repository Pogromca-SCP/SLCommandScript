using CommandSystem;
using System;
using System.IO;
using SLCommandScript.Interpreter;

namespace SLCommandScript.Commands
{
    /// <summary>
    /// Command used to launch interpreted scripts
    /// </summary>
    public class FileScriptCommand : ICommand
    {
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
        public string Description => $"Executes custom script named {Command}";

        /// <summary>
        /// Contains path to script file
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Initializes the command
        /// </summary>
        /// <param name="file">Path to associated script</param>
        public FileScriptCommand(string file)
        {
            _filePath = file;
            Command = file.Substring(file.LastIndexOf('/') + 1);
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

            if (!File.Exists(_filePath))
            {
                response = "Script file does not exist or cannot be accessed.";
                return false;
            }

            return new SCLInterpreterBase().ProcessLines(File.ReadAllLines(_filePath), sender, out response);
        }
    }
}
