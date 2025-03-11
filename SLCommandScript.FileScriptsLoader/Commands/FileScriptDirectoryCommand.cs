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
    /// Contains shortened directory path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="path">Path to use.</param>
    public FileScriptDirectoryCommand(string path)
    {
        Path = path;
        var index = path?.LastIndexOf('/') ?? -1;
        Command = index < 0 ? path : path.Substring(index + 1);
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
