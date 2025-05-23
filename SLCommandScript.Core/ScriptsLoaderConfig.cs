using SLCommandScript.Core.Commands;
using System.ComponentModel;

namespace SLCommandScript.Core;

/// <summary>
/// Contains scripts loader configuration.
/// </summary>
public class ScriptsLoaderConfig
{
    /// <summary>
    /// Custom permissions resolver implementation to use, leave empty if not needed.
    /// </summary>
    [Description("Custom permissions resolver implementation to use, leave empty if not needed")]
    public string? CustomPermissionsResolver { get; set; } = null;

    /// <summary>
    /// Set to <see langword="false" /> in order to disable event handling with scripts.
    /// </summary>
    [Description("Set to false in order to disable event handling with scripts")]
    public bool EnableScriptEventHandlers { get; set; } = true;

    /// <summary>
    /// Defines allowed script command types (Console, GameConsole or RemoteAdmin), set to 0 to disable all script commands.
    /// </summary>
    [Description("Defines allowed script command types (Console, GameConsole or RemoteAdmin), set to 0 to disable all script commands")]
    public CommandType AllowedScriptCommandTypes { get; set; } = CommandsUtils.AllScopes;

    /// <summary>
    /// Defines a maximum amount of concurrent executions a single script can have, use it to set max recursion depth.
    /// </summary>
    [Description("Defines a maximum amount of concurrent executions a single script can have, use it to set max recursion depth")]
    public int ScriptExecutionsLimit { get; set; } = 10;
}
