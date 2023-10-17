using CommandSystem;
using System;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Special parent command used for directories.
/// </summary>
public class FileScriptDirectoryCommand : ParentCommand
{
    /// <summary>
    /// Contains command name.
    /// </summary>
    public override string Command { get; }

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public override string[] Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public override string Description { get; }

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="name">Name to use.</param>
    public FileScriptDirectoryCommand(string name)
    {
        Command = name;
        Description = "Parent command containing all scripts in a directory.";
    }

    /// <summary>
    /// Loads subcommands.
    /// </summary>
    public override void LoadGeneratedCommands() {}

    /// <summary>
    /// Executes the parent command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = "Cannot execute this parent command";
        return false;
    }
}
