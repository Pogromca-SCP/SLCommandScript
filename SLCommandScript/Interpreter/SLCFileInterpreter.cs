using SLCommandScript.Core.Interpreter;
using System;
using CommandSystem;
using System.IO;

namespace SLCommandScript.Interpreter
{
    /// <summary>
    /// Basic SLCommands file interpreter
    /// </summary>
    public class SLCFileInterpreter : SLCInterpreterBase
    {
        /// <summary>
        /// Contains a path to script file
        /// </summary>
        private readonly string _file;

        /// <summary>
        /// Creates new interpreter instance
        /// </summary>
        /// <param name="filepath">Path to script file</param>
        public SLCFileInterpreter(string filepath)
        {
            _file = filepath;
        }

        /// <summary>
        /// Executes the script file
        /// </summary>
        /// <param name="args">Provided script arguments</param>
        /// <param name="sender">Script sender</param>
        /// <param name="response">Response text to display</param>
        /// <returns>True if command script successfully, false otherwise</returns>
        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!File.Exists(_file))
            {
                response = "Script file does not exist or cannot be accessed.";
                return false;
            }

            return ProcessMultipleLines(File.ReadAllLines(_file), args, sender, out response);
        }
    }
}
