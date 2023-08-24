using CommandSystem;
using System.Linq;
using SLCommandScript.Core.Interfaces;
using System;

namespace SLCommandScript.Commands;

/// <summary>
/// Script command used to launch interpreted scripts.
/// </summary>
public class FileScriptCommand : FileScriptCommandBase, IUsageProvider
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
    /// Describes command arguments usage.
    /// </summary>
    private string[] _usage;

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="file">Path to associated script.</param>
    /// <param name="resolver">Permissions resolver to use.</param>
    public FileScriptCommand(string file, IPermissionsResolver resolver) : base(file, resolver)
    {
        Arity = 0;
        _usage = null;
    }

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
