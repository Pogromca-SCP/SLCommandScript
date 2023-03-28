using System;
using CommandSystem;
using System.Collections.Generic;
using System.Text;
using SLCommandScript.Commands;
using System.Text.RegularExpressions;

namespace SLCommandScript.Interpreter
{
    /// <summary>
    /// Base class for SLCommands interpreters
    /// </summary>
    public abstract class SLCInterpreterBase
    {
        /// <summary>
        /// Response message to display on script sender is null error
        /// </summary>
        private const string NullSenderError = "Script sender is null.";

        /// <summary>
        /// Response message to display on invalid arguments error
        /// </summary>
        private const string InvalidArgsError = "Provided arguments array segment is invalid (array is null or offset is too small).";

        /// <summary>
        /// Response message to display on success
        /// </summary>
        private const string SuccessResponse = "Script executed successfully.";

        /// <summary>
        /// Maximum amount of split results
        /// </summary>
        private const int MaxSplitResults = 512;

        /// <summary>
        /// String separators to use for splits
        /// </summary>
        private static readonly char[] _splitChars = { ' ' };

        /// <summary>
        /// Contains arguments for current script execution
        /// </summary>
        private ArraySegment<string> _arguments;

        /// <summary>
        /// Holds reference to script sender
        /// </summary>
        private ICommandSender _sender;

        /// <summary>
        /// Holds a result message to display
        /// </summary>
        private string _result;

        /// <summary>
        /// Tells whether or not the sender has missing permissions to execute the script
        /// </summary>
        private bool _missingPerms;

        /// <summary>
        /// Executes multiple lines of SLC script
        /// </summary>
        /// <param name="lines">Lines to evaluate and execute</param>
        /// <param name="arguments">Provided script arguments</param>
        /// <param name="sender">Script sender</param>
        /// <param name="response">Response text to display</param>
        /// <returns>True if script executed successfully, false otherwise</returns>
        public bool ProcessMultipleLines(IEnumerable<string> lines, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is null)
            {
                response = NullSenderError;
                return false;
            }

            if (arguments.Array is null || arguments.Offset < 1)
            {
                response = InvalidArgsError;
                return false;
            }

            if (lines is null)
            {
                response = SuccessResponse;
                return true;
            }

            _arguments = arguments;
            _sender = sender;
            _missingPerms = false;
            ProcessLines(lines);
            response = _result ?? SuccessResponse;
            var ret = _result is null;
            _arguments = new ArraySegment<string>();
            _sender = null;
            _result = null;
            return ret;
        }

        /// <summary>
        /// Executes a single line of SLC script
        /// </summary>
        /// <param name="line">Line to evaluate and execute</param>
        /// <param name="arguments">Provided script arguments</param>
        /// <param name="sender">Script sender</param>
        /// <param name="response">Response text to display</param>
        /// <returns>True if script executed successfully, false otherwise</returns>
        public bool ProcessSingleLine(string line, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is null)
            {
                response = NullSenderError;
                return false;
            }

            if (arguments.Array is null || arguments.Offset < 1)
            {
                response = InvalidArgsError;
                return false;
            }

            _arguments = arguments;
            _sender = sender;
            _missingPerms = false;
            ProcessLine(line);
            response = _result ?? SuccessResponse;
            var ret = _result is null;
            _arguments = new ArraySegment<string>();
            _sender = null;
            _result = null;
            return ret;
        }

        /// <summary>
        /// Evaluates script lines
        /// </summary>
        /// <param name="lines">Script lines to process</param>
        private void ProcessLines(IEnumerable<string> lines)
        {
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.EndsWith(" \\"))
                {
                    sb.Append(line.Substring(0, line.Length - 1));
                }
                else if (sb.Length > 0)
                {
                    ProcessLine(sb.Append(line).ToString());
                    sb.Clear();
                }
                else
                {
                    ProcessLine(line);
                }

                if (!(_result is null))
                {
                    return;
                }
            }

            if (sb.Length > 0)
            {
                ProcessLine(sb.ToString());
            }
        }

        /// <summary>
        /// Evaluates the script line
        /// </summary>
        /// <param name="line">Script line to process</param>
        private void ProcessLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (!line.Contains("#"))
            {
                ProcessCommand(line);
                return;
            }

            var index = line.IndexOf('#');
            ProcessCommand(line.Substring(0, index));

            if (line.Length > index + 2 && line[index + 1] == '!')
            {
                ProcessPermissionsDef(line.Substring(index + 2));
            }
        }

        /// <summary>
        /// Parses and executes provided command
        /// </summary>
        /// <param name="command">Command to execute</param>
        private void ProcessCommand(string command)
        {
            if (_missingPerms || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            var query = InsertArguments(command.Trim())?.Split(_splitChars, MaxSplitResults, StringSplitOptions.RemoveEmptyEntries);

            if (query is null || query.Length < 1)
            {
                return;
            }

            var cmd = CommandsUtils.FindCommand(query[0]);

            if (cmd is null)
            {
                _result = $"Command '{query[0]}' not found.";
                return;
            }

            var success = cmd.Execute(new ArraySegment<string>(query, 1, query.Length - 1), _sender, out var response);

            if (!success)
            {
                _result = response;
            }
        }

        /// <summary>
        /// Inserts script arguments into a command
        /// </summary>
        /// <param name="command">Command to insert arguments into</param>
        /// <returns>Command with injected arguments or null if something went wrong</returns>
        private string InsertArguments(string command)
        {
            if (!command.Contains("$"))
            {
                return command;
            }

            var regex = new Regex("\\$([0-9]+)");
            var matches = regex.Matches(command);

            if (matches.Count < 1)
            {
                return command;
            }

            for (var index = 0; index < matches.Count; ++index)
            {
                var argNum = int.Parse(matches[index].Groups[1].Value);

                if (argNum > _arguments.Count)
                {
                    _result = $"({_arguments.Array[_arguments.Offset - 1]}) Missing argument ${argNum}, sender provided only {_arguments.Count} arguments.";
                    return null;
                }
            }

            return regex.Replace(command, m => _arguments.Array[_arguments.Offset + int.Parse(m.Groups[1].Value) - 1]);
        }

        /// <summary>
        /// Parses permissions definitions and checks if the sender has all of them
        /// </summary>
        /// <param name="perms">Required permissions definition</param>
        private void ProcessPermissionsDef(string perms)
        {
            _missingPerms = false;

            if (string.IsNullOrWhiteSpace(perms))
            {
                return;
            }

            foreach (var perm in perms.Trim().Split(_splitChars, MaxSplitResults, StringSplitOptions.RemoveEmptyEntries))
            {
                var parsed = Enum.TryParse<PlayerPermissions>(perm, true, out var result);

                if (parsed && !_sender.CheckPermission(result))
                {
                    _missingPerms = true;
                    return;
                }
            }
        }
    }
}
