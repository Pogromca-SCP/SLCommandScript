using CommandSystem;
using SLCommandScript.Interpreter;
using System.IO;
using System;

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
        public string Description => $"Executes custom script from {Command}.scl.";

        /// <summary>
        /// Contains script interpreter
        /// </summary>
        private readonly SLCFileInterpreter _interpreter;

        /// <summary>
        /// Initializes the command
        /// </summary>
        /// <param name="file">Path to associated script</param>
        public FileScriptCommand(string file)
        {
            if (!File.Exists(file))
            {
                _interpreter = null;
                Command = null;
                return;
            }

            _interpreter = new SLCFileInterpreter(file);
            var startIndex = file.LastIndexOf('/') + 1;
            var endIndex = file.LastIndexOf('.');
            Command = endIndex < 0 ? file.Substring(startIndex) : file.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">Command arguments provided by sender</param>
        /// <param name="sender">Command sender</param>
        /// <param name="response">Response to display in sender's console</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) => _interpreter.Execute(arguments, sender, out response);
    }
}
