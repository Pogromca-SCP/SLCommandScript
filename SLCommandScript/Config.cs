using System.ComponentModel;

namespace SLCommandScript;

/// <summary>
/// Contains plugin configuration.
/// </summary>
public class Config
{
    /// <summary>
    /// Scripts loader implementation to use, provided as a fully qualified type name.
    /// </summary>
    [Description("Scripts loader implementation to use, provided as a fully qualified type name")]
    public string ScriptsLoaderImplementation { get; set; } = "SLCommandScript.FileScriptsLoader.FileScriptsLoader, SLCommandScript.FileScriptsLoader";

    /// <summary>
    /// Tells whether or not helper commands should be registered in consoles.
    /// </summary>
    [Description("Tells whether or not helper commands should be registered in consoles")]
    public bool EnableHelperCommands { get; set; } = true;
}
