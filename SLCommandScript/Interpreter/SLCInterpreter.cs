using System.IO;
using System;
using CommandSystem;
using System.Text;

namespace SLCommandScript
{
    public class SLCInterpreter
    {
        private readonly string _file;

        public SLCInterpreter(string filepath)
        {
            _file = string.IsNullOrWhiteSpace(filepath) ? null : filepath;
        }

        /*public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is null)
            {
                response = "Command sender is null.";
                return false;
            }

            if (_file is null)
            {
                response = "Script file path is invalid.";
                return false;
            }

            if (!File.Exists(_file))
            {
                response = "Script file does not exist or cannot be accessed.";
                return false;
            }

            var sb = new StringBuilder();

            foreach (var line in File.ReadAllLines(_file))
            {
                if (line.EndsWith(" _"))
                {
                    sb.Append(line.Substring(0, line.Length - 1));
                }
                else if (sb.Length > 0)
                {
                    sb.Append(line);
                    ProcessLine(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    ProcessLine(line);
                }
            }

            if (sb.Length > 0)
            {
                ProcessLine(sb.ToString());
            }

            response = "Script executed without issues.";
            return true;
        }*/
    }
}
