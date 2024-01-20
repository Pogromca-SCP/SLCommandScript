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

        if (iterable.IsAtEnd)
        {
            response = $"'{iterableName}' has no elements";
            return false;
        }

        var vars = new Dictionary<string, string>();
        iterable.LoadNext(vars);

        if (vars.Count < 1)
        {
            response = $"No variables available in '{iterableName}'";
            return true;
        }

        response = GetDictionaryKeys(vars, $"Variables available in '{iterableName}':\n");
        return true;
    }

    /// <summary>
    /// Retrieves dictionary keys as a human readable list.
    /// </summary>
    /// <typeparam name="T">Type of dictionary values.</typeparam>
    /// <param name="dictionary">Dictionary to get keys from.</param>
    /// <param name="initialText">Text to use at the beggining of the list.</param>
    /// <returns>Dictionary keys in a list.</returns>
    private static string GetDictionaryKeys<T>(IDictionary<string, T> dictionary, string initialText)
    {
        var sb = StringBuilderPool.Shared.Rent(initialText);

        foreach (var key in dictionary.Keys)
        {
            sb.AppendLine(key);
        }

        return StringBuilderPool.Shared.ToStringReturn(sb);
    }

    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = "iterables";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases { get; } = ["iter"];

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

        response = GetDictionaryKeys(Parser.Iterables, "Currently available iterables:\n");
        return true;
    }
}
