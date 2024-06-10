using CommandSystem;
using NorthwoodLib.Pools;
using SLCommandScript.Core;
using SLCommandScript.Core.Interfaces;
using System;

namespace SLCommandScript.Commands;

/// <summary>
/// Main command provided by the plugin.
/// </summary>
public class HelperCommands : ParentCommand, IUsageProvider
{
    /// <summary>
    /// Creates an info line.
    /// </summary>
    /// <param name="name">Item name.</param>
    /// <param name="version">Item version.</param>
    /// <param name="author">Item author.</param>
    /// <returns>Created info line.</returns>
    private static string MakeInfoLine(string name, string version, string author) => $"{name} v{version} @{author}";

    /// <summary>
    /// Contains command name.
    /// </summary>
    public override string Command { get; } = "slcshelper";

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public override string[] Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public override string Description { get; } = "Provides helper subcommands for SLCommandScript. Displays environment info if no valid subcommand is selected.";

    /// <summary>
    /// Defines command usage prompts.
    /// </summary>
    public string[] Usage { get; } = ["iterables/syntax", "Args..."];

    /// <summary>
    /// Stores a reference to currently used loader.
    /// </summary>
    private readonly IScriptsLoader _loader;

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="loader">Currently used loader.</param>
    public HelperCommands(IScriptsLoader loader)
    {
        _loader = loader;
        LoadGeneratedCommands();
    }

    /// <summary>
    /// Loads subcommands.
    /// </summary>
    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new IterablesCommand());
        RegisterCommand(new SyntaxCommand());
    }

    /// <summary>
    /// Executes the parent command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true"/> if command executed successfully, <see langword="false"/> otherwise.</returns>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var sb = StringBuilderPool.Shared.Rent("Current SLCommandScript environment state:\n");
        sb.AppendLine(MakeInfoLine(Plugin.PluginName, Plugin.PluginVersion, Plugin.PluginAuthor));
        sb.AppendLine(MakeInfoLine(Constants.Name, Constants.Version, Constants.Author));
        sb.Append(_loader is null ? "No Scripts Loader currently in use" : MakeInfoLine(_loader.LoaderName, _loader.LoaderVersion, _loader.LoaderAuthor));
        response = StringBuilderPool.Shared.ToStringReturn(sb);
        return true;
    }
}
