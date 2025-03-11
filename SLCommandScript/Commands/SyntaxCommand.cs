using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Commands;

/// <summary>
/// Helper command with syntax rules.
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
    public string[] Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Helper command with syntax rules. Provide expression/guard name to view its syntax rules.";

    /// <summary>
    /// Tells whether or not command response should be sanitized.
    /// </summary>
    public bool SanitizeResponse => true;

    /// <summary>
    /// Contains syntax rules.
    /// </summary>
    public Dictionary<string, string> Rules { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "perm", "Permissions guard:\n#! <permission_names...>\n(guards cannot be placed inside expressions)" },
        { "scope", "Scope guard:\n#? <scope_names...>\n(guards cannot be placed inside expressions)" },
        { "cmd", "Command expression:\n<command_name> <arguments...>" },
        { "if", "If expression:\n[<expression> if <expression>]\n[<expression> else <expression>]\n[<expression> if <expression> else <expression>]" },
        { "foreach", "Foreach expression:\n[<expression> foreach <iterable_name_or_numbers_range>]" },
        { "delay", "Delay expression:\n[<expression> delayby <time_in_ms>]\n[<expression> delayby <time_in_ms> <name_to_use_for_error_log>]" },
        { "forrandom", "Forrandom expression:\n[<expression> forrandom <iterable_name_or_numbers_range>]\n" +
            "[<expression> forrandom <iterable_name_or_numbers_range> <limit_number_or_percentage>]\n" +
            "[<expression> forrandom <iterable_name_or_numbers_range> else <expression>]\n" +
            "[<expression> forrandom <iterable_name_or_numbers_range> <limit_number_or_percentage> else <expression>]" },
        { "seq", "Sequence expression:\n[<expressions_separated_with_|...>]" }
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

            if (!Rules.ContainsKey(key))
            {
                response = $"No syntax rules found for '{key}'";
                return false;
            }

            response = Rules[key];
            return true;
        }

        var sb = StringBuilderPool.Shared.Rent("Available expression/guard types:\n");

        foreach (var name in Rules.Keys)
        {
            sb.AppendLine(name);
        }

        response = StringBuilderPool.Shared.ToStringReturn(sb);
        return true;
    }
}
