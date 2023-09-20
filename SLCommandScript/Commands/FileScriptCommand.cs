using CommandSystem;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace SLCommandScript.Commands;

/// <summary>
/// Script command used to launch interpreted scripts.
/// </summary>
public class FileScriptCommand : FileScriptCommandBase, IUsageProvider, IHelpProvider
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
            }

            var usage = value.Where(i => !string.IsNullOrWhiteSpace(i));
            _usage = usage.Count() > 0 ? usage.ToArray() : null;
        }
    }

    /// <summary>
    /// Contains expected amount of arguments.
    /// </summary>
    public byte Arity { get; set; }

    /// <summary>
    /// Text to display when help for command is requested.
    /// </summary>
    public string Help { get; set; }

    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    private string[] _usage;

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="file">Path to associated script.</param>
    public FileScriptCommand(string file) : base(file)
    {
        Arity = 0;
        Help = null;
        _usage = null;
    }

    /// <summary>
    /// Generates message for help command.
    /// </summary>
    /// <param name="arguments">Arguments provided by sender.</param>
    /// <returns>Generated help message.</returns>
    public string GetHelp(ArraySegment<string> arguments) => string.IsNullOrWhiteSpace(Help) ? Description : new Regex("\\$\\(([1-9][0-9]*)\\)").Replace(Help, m =>
    {
        var index = int.Parse(m.Groups[1].Value);
        return index > arguments.Count ? m.Value : arguments.At(index - 1);
    });

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
