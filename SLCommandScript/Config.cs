using System.ComponentModel;

namespace SLCommandScript;

/// <summary>
/// Contains plugin configuration.
/// </summary>
public class Config
{
    /// <summary>
    /// Scripts loader implementation to use, provided as fully a qualified type name.
    /// </summary>
    [Description("Scripts loader implementation to use, provided as a fully qualified type name")]
    public string ScriptsLoaderImplementation { get; set; } = "SLCommandScript.FileScriptsLoader.FileScriptsLoader, SLCommandScript.FileScriptsLoader";
}
