using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Commands;

/// <summary>
/// Helper command with syntax examples.
/// </summary>
public class SyntaxCommand : ICommand
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "syntax";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["tip"];

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Helper command with syntax examples. Provide expression name to view syntax examples.";

    /// <summary>
    /// Contains syntax tips.
    /// </summary>
    public Dictionary<string, string> Tips { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "perm", "Permissions guard:\n#! <permission_names...>" },
        { "scope", "Scope guard:\n#? <scope_names...>" },
        { "cmd", "Command expression:\n<command_name> <arguments...>" },
        { "if", "If expression:\n[ <expression> if <expression> ]\n[ <expression> if <expression> else <expression> ]" },
        { "foreach", "Foreach expression:\n[ <expression> foreach <iterable_name> ]" },
        { "delay", "Delay expression:\n[ <expression> delayby <time_in_ms> ]\n[ <expression> delayby <time_in_ms> <name_to_use_for_error_log> ]" },
        { "forrandom", "Forrandom expression:\n[ <expression> forrandom <iterable_name> ]\n[ <expression> forrandom <iterable_name> <number_limit> ]\n"
            + "[ <expression> forrandom <iterable_name> else <expression> ]\n[ <expression> forrandom <iterable_name> <number_limit> else <expression> ]" }
    };

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true"/> if command executed successfully, <see langword="false"/> otherwise.</returns>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count > 0)
        {
            var key = arguments.At(0);

            if (!Tips.ContainsKey(key))
            {
                response = $"No syntax tips found for '{key}'";
                return false;
            }

            response = Tips[key];
            return true;
        }

        var sb = StringBuilderPool.Shared.Rent("Available expression tips:\n");

        foreach (var name in Tips.Keys)
        {
            sb.AppendLine(name);
        }

        response = StringBuilderPool.Shared.ToStringReturn(sb);
        return true;
    }
}
