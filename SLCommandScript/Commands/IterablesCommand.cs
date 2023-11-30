using CommandSystem;
using NorthwoodLib.Pools;
using SLCommandScript.Core.Language;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Commands;

/// <summary>
/// Helper command for iterables discovery.
/// </summary>
public class IterablesCommand : ICommand
{
    /// <summary>
    /// Attempts to retrieve available variables from iterable object.
    /// </summary>
    /// <param name="iterableName">Name of the iterable to analyze.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true"/> if attempt finished successfully, <see langword="false"/> otherwise.</returns>
    private static bool GetVariables(string iterableName, out string response)
    {
        if (!Parser.Iterables.ContainsKey(iterableName))
        {
            response = $"'{iterableName}' was not found in available iterables";
            return false;
        }

        var source = Parser.Iterables[iterableName];

        if (source is null)
        {
            response = $"'{iterableName}' is null";
            return false;
        }

        var iterable = source();

        if (iterable is null)
        {
            response = $"'{iterableName}' returned null";
            return false;
        }

        var vars = new Dictionary<string, string>();
        iterable.LoadNext(vars);

        if (vars.Count < 1)
        {
            response = $"No variables available in '{iterableName}'. Perhaps it did not contain any elements";
            return false;
        }

        var sb = StringBuilderPool.Shared.Rent($"Variables available in '{iterableName}':\n");

        foreach (var key in vars.Keys)
        {
            sb.AppendLine(key);
        }

        response = StringBuilderPool.Shared.ToStringReturn(sb);
        return true;
    }

    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "iterables";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["iter", "for"];

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; } = "Helper command for iterables discovery. Provide iterable name to check available variables.";

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
            return GetVariables(arguments.At(0), out response);
        }

        var sb = StringBuilderPool.Shared.Rent("Currently available iterables:\n");

        foreach (var iterableName in Parser.Iterables.Keys)
        {
            sb.AppendLine(iterableName);
        }

        response = StringBuilderPool.Shared.ToStringReturn(sb);
        return true;
    }
}
