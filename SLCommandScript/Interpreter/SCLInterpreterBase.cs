using System.Collections.Generic;
using CommandSystem;
using SLCommandScript.Commands;
using System;

namespace SLCommandScript.Interpreter
{
    public class SCLInterpreterBase
    {
        private ICommandSender _sender;

        private CommandContextType _contextType;

        private string _result;

        private bool _missingPerms;

        private bool _missingContext;

        public bool ProcessLines(IEnumerable<string> lines, ICommandSender sender, CommandContextType context, out string response)
        {
            if (sender is null)
            {
                response = "Script sender is null.";
                return false;
            }

            _sender = sender;
            _contextType = context;
            _missingPerms = false;
            _missingContext = false;

            foreach (var line in lines)
            {
                ProcessLine(line);

                if (!(_result is null))
                {
                    response = _result;
                    _sender = null;
                    _result = null;
                    return false;
                }
            }

            response = _result ?? "Script executed successfully.";
            var res = _result is null;
            _sender = null;
            _result = null;
            return res;
        }

        public bool ProcessSingleLine(string line, ICommandSender sender, CommandContextType context, out string response)
        {
            if (sender is null)
            {
                response = "Script sender is null.";
                return false;
            }

            _sender = sender;
            _contextType = context;
            _missingPerms = false;
            _missingContext = false;
            ProcessLine(line);
            response = _result ?? "Script executed successfully.";
            var res = _result is null;
            _sender = null;
            _result = null;
            return res;
        }

        private void ProcessLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (line.Contains("#"))
            {
                var index = line.IndexOf("#");
                ProcessCommand(line.Substring(0, index));
                ProcessComment(line.Substring(index + 1));
                return;
            }

            ProcessCommand(line);
        }

        private void ProcessCommand(string command)
        {
            if (_missingPerms || _missingContext || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            var query = command.Split(' ');
            var cmd = CommandsUtils.FindCommand(_contextType, query[0]);

            if (cmd is null)
            {
                _result = $"Command '{query[0]}' not found.";
                return;
            }

            var tmp = cmd.Execute(new ArraySegment<string>(query, 1, query.Length - 1), _sender, out var response);

            if (!tmp)
            {
                _result = response;
            }
        }

        private void ProcessComment(string comment)
        {
            if (comment.StartsWith("!"))
            {
                ProcessPermissionsDef(comment.Substring(1));
            }

            if (comment.StartsWith("?"))
            {
                ProcessHandlersDef(comment.Substring(1));
            }
        }

        private void ProcessPermissionsDef(string perms)
        {
            _missingPerms = false;

            if (string.IsNullOrWhiteSpace(perms))
            {
                return;
            }

            foreach (var perm in perms.Split(' '))
            {
                var tmp = Enum.TryParse<PlayerPermissions>(perm, true, out var result);

                if (tmp && !_sender.CheckPermission(result))
                {
                    _missingPerms = true;
                }
            }
        }

        private void ProcessHandlersDef(string handlers)
        {
            if (string.IsNullOrWhiteSpace(handlers))
            {
                _missingContext = false;
                return;
            }

            _missingContext = true;

            foreach (var con in handlers.Split(' '))
            {
                var tmp = Enum.TryParse<CommandContextType>(con, true, out var result);

                if (tmp && _contextType == result)
                {
                    _missingContext = false;
                    return;
                }
            }
        }
    }
}
