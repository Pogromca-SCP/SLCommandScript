using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using SLCommandScript.Core;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Loader;
using PluginAPI.Enums;
using SLCommandScript.Core.Reflection;

namespace SLCommandScript;

/// <summary>
/// Defines plugin functionality.
/// </summary>
public class Plugin
{
    public const string PluginName = "SLCommandScript";
    public const string PluginVersion = "0.2.3";
    public const string PluginDescription = "Simple, commands based scripting language.";
    public const string PluginAuthor = "Adam Szerszenowicz";

    /// <summary>
    /// Plugin prefix to use in logs.
    /// </summary>
    private const string PluginPrefix = "SLCommandScript: ";

    /// <summary>
    /// Prints an info message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    private static void PrintLog(string message) => Log.Info(message, PluginPrefix);

    /// <summary>
    /// Prints an error message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    private static void PrintError(string message) => Log.Error(message, PluginPrefix);

    /// <summary>
    /// Stores plugin configuration.
    /// </summary>
    [PluginConfig("pluginConfig")]
    public Config PluginConfig;

    /// <summary>
    /// Stores scripts loader configuration.
    /// </summary>
    [PluginConfig("scriptsLoaderConfig")]
    public ScriptsLoaderConfig ScriptsLoaderConfig;

    /// <summary>
    /// Stores a reference to scripts loader.
    /// </summary>
    private IScriptsLoader _scriptsLoader;

    /// <summary>
    /// Loads and initializes the plugin.
    /// </summary>
    [PluginPriority(LoadPriority.Lowest)]
    [PluginEntryPoint(PluginName, PluginVersion, PluginDescription, PluginAuthor)]
    void LoadPlugin()
    {
        PrintLog("Plugin load started...");
        Init();
        PrintLog("Plugin is loaded.");
    }

    /// <summary>
    /// Reloads the plugin.
    /// </summary>
    [PluginReload]
    void ReloadPlugin()
    {
        PrintLog("Plugin reload started...");
        Init();
        PrintLog("Plugin reloaded.");
    }

    /// <summary>
    /// Unloads the plugin.
    /// </summary>
    [PluginUnload]
    void UnloadPlugin()
    {
        PrintLog("Plugin unload started...");
        _scriptsLoader?.Dispose();
        _scriptsLoader = null;
        PluginConfig = null;
        PrintLog("Plugin is unloaded.");
    }

    /// <summary>
    /// Plugin components initialization.
    /// </summary>
    private void Init()
    {
        ReloadConfigs();
        _scriptsLoader?.Dispose();
        _scriptsLoader = LoadScriptsLoader();
        _scriptsLoader.InitScriptsLoader(this, ScriptsLoaderConfig);
    }

    /// <summary>
    /// Reloads plugin config values.
    /// </summary>
    private void ReloadConfigs()
    {
        var handler = PluginHandler.Get(this);

        if (handler is null)
        {
            return;
        }

        handler.LoadConfig(this, nameof(PluginConfig));
        handler.LoadConfig(this, nameof(ScriptsLoaderConfig));
    }

    /// <summary>
    /// Loads scripts loader.
    /// </summary>
    /// <returns>Loaded scripts loader instance.</returns>
    private IScriptsLoader LoadScriptsLoader()
    {
        if (string.IsNullOrWhiteSpace(PluginConfig.CustomScriptsLoader))
        {
            PrintLog("Using default scripts loader.");
            return new FileScriptsLoader();
        }

        var loader = CustomTypesUtils.MakeCustomTypeInstance<IScriptsLoader>(PluginConfig.CustomScriptsLoader, out var message);

        if (loader is null)
        {
            PrintError(message);
            return new FileScriptsLoader();
        }

        PrintLog("Custom scripts loader loaded successfully.");
        return loader;
    }
}
