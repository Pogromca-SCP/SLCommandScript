using CommandSystem;
using System;
using System.Linq;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Script command used to launch interpreted scripts.
/// </summary>
/// <param name="name">Name of the command.</param>
/// <param name="parent">Parent which stores this command.</param>
/// <param name="config">Configuration to use.</param>
public class FileScriptCommand(string? name, IFileScriptCommandParent? parent, RuntimeConfig? config) : FileScriptCommandBase(name, parent, config), IUsageProvider
{
    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    public string[]? Usage
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
            _usage = usage.Count() > 0 ? [..usage] : null;
        }
    }

    /// <summary>
    /// Contains expected amount of arguments.
    /// </summary>
    public byte Arity { get; set; } = 0;

    /// <summary>
    /// Contains permission names required to run the command.
    /// </summary>
    public string?[]? RequiredPermissions { get; set; } = null;

    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    private string[]? _usage = null;

    /// <inheritdoc />
    public override bool Execute(ArraySegment<string?> arguments, ICommandSender? sender, out string response)
    {
        if (RequiredPermissions is not null && RequiredPermissions.Length > 0)
        {
            var resolver = Config.PermissionsResolver;

            foreach (var perm in RequiredPermissions)
            {
                var hasPerm = resolver.CheckPermission(sender, perm, out var error);

                if (error is not null)
                {
                    response = error;
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
