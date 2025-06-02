using CommandSystem;
using System;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Special parent command used for directories.
/// </summary>
/// <param name="name">Name of the command.</param>
/// <param name="parent">Parent which stores this command.</param>
public class FileScriptDirectoryCommand(string? name, IFileScriptCommandParent? parent) : ParentCommand, IFileScriptCommandParent
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public override string Command { get; } = name ?? string.Empty;

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public override string[]? Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public override string Description { get; } = "Parent command containing all scripts in a directory.";

    /// <summary>
    /// Contains parent object which stores this command.
    /// </summary>
    public IFileScriptCommandParent? Parent { get; } = parent;

    /// <summary>
    /// Loads subcommands.
    /// </summary>
    public override void LoadGeneratedCommands() {}

    /// <inheritdoc />
    public string GetLocation(bool includeRoot = false) => Parent is null ? Command : $"{Parent.GetLocation(includeRoot)}{Command}/";

    /// <summary>
    /// Executes the parent command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    protected override bool ExecuteParent(ArraySegment<string?> arguments, ICommandSender? sender, out string response)
    {
        response = "Cannot execute this parent command";
        return false;
    }
}
