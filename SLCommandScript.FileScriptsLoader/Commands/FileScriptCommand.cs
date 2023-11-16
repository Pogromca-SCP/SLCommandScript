using CommandSystem;
using System;
using System.Linq;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Script command used to launch interpreted scripts.
/// </summary>
/// <param name="file">Path to associated script.</param>
public class FileScriptCommand(string file) : FileScriptCommandBase(file), IUsageProvider, IHelpProvider
{
    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    public string[] Usage
    {
        get => _usage;
        set
        {
            if (value is null || value.Length < 1)
            {
                _usage = null;
                return;
            }

            var usage = value.Where(i => !string.IsNullOrWhiteSpace(i));
            _usage = usage.Count() > 0 ? usage.ToArray() : null;
        }
    }

    /// <summary>
    /// Contains expected amount of arguments.
    /// </summary>
    public byte Arity { get; set; } = 0;

    /// <summary>
    /// Text to display when help for command is requested.
    /// </summary>
    public string Help { get; set; } = null;

    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    private string[] _usage = null;

    /// <summary>
    /// Generates message for help command.
    /// </summary>
    /// <param name="arguments">Arguments provided by sender.</param>
    /// <returns>Generated help message.</returns>
    public string GetHelp(ArraySegment<string> arguments) => string.IsNullOrWhiteSpace(Help) ? Description : Help;

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < Arity)
        {
            response = $"Missing argument: script expected {Arity} arguments, but sender provided {arguments.Count}";
            return false;
        }

        return base.Execute(arguments, sender, out response);
    }
}
