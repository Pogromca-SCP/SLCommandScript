using System.ComponentModel;

namespace SLCommandScript;

/// <summary>
/// Contains plugin configuration.
/// </summary>
public class Config
{
    /// <summary>
    /// Custom scripts loader implementation to use, leave empty if not needed.
    /// </summary>
    [Description("Custom scripts loader implementation to use, leave empty if not needed")]
    public string CustomScriptsLoader { get; set; } = null;

    /// <summary>
    /// Custom permissions resolver implementation to use, leave empty if not needed.
    /// </summary>
    [Description("Custom permissions resolver implementation to use, leave empty if not needed")]
    public string CustomPermissionsResolver { get; set; } = null;

    /// <summary>
    /// Set to false in order to disable event handling with scripts.
    /// </summary>
    [Description("Set to false in order to disable event handling with scripts")]
    public bool EnableScriptEventHandlers { get; set; } = true;
}
