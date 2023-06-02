using System.ComponentModel;

namespace SLCommandScript;

/// <summary>
/// Contains plugin configuration
/// </summary>
public class Config
{
    /// <summary>
    /// Custom scripts loader implementation to use, leave empty if not needed
    /// </summary>
    [Description("Custom scripts loader implementation to use, leave empty if not needed")]
    public string CustomScriptsLoader { get; set; } = string.Empty;
}
