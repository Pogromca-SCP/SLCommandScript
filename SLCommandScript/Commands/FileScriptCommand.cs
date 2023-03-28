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
        /// Converts a file path into a usable command name
        /// </summary>
        /// <param name="path">File path to convert</param>
        /// <returns>Usable command name or null if path is null</returns>
        public static string FilePathToCommandName(string path)
        {
            if (path is null)
            {
                return null;
            }

            var startIndex = path.LastIndexOf('/') + 1;

            if (startIndex < 1)
            {
                startIndex = path.LastIndexOf('\\') + 1;
            }

            var endIndex = path.LastIndexOf('.');
            return endIndex <= startIndex ? path.Substring(startIndex) : path.Substring(startIndex, endIndex - startIndex);
        }

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
            Command = FilePathToCommandName(file);
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
