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
}
