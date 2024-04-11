using CommandSystem;
using SLCommandScript.Core.Permissions;
using System;
using System.Linq;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Script command used to launch interpreted scripts.
/// </summary>
/// <param name="location">Root location where the used path starts from.</param>
/// <param name="path">Path to associated script.</param>
public class FileScriptCommand(string location, string path) : FileScriptCommandBase(location, path), IUsageProvider
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
    /// Contains permission names required to run the command.
    /// </summary>
    public string[] RequiredPermissions { get; set; } = null;

    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    private string[] _usage = null;

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (RequiredPermissions is not null && RequiredPermissions.Length > 0)
        {
            var resolver = PermissionsResolver ?? new VanillaPermissionsResolver();

            foreach (var perm in RequiredPermissions)
            {
                var hasPerm = resolver.CheckPermission(sender, perm, out response);

                if (response is not null)
                {
                    return false;
                }

                if (!hasPerm)
                {
                    response = $"Missing permission: '{perm}'. Access denied";
                    return false;
                }
            }
        }

        if (arguments.Count < Arity)
        {
            response = $"Missing argument: script expected {Arity} arguments, but sender provided {arguments.Count}";
            return false;
        }

        return base.Execute(arguments, sender, out response);
    }
}
