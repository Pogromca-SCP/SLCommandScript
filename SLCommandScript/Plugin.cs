using PluginAPI.Core;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Loader;
using System;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace SLCommandScript;

/// <summary>
/// Defines plugin functionality.
/// </summary>
public class Plugin
{
    public const string PluginName = "SLCommandScript";
    public const string PluginVersion = "0.2.1";
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
    /// Creates custom scripts loader instance.
    /// </summary>
    /// <param name="loaderType">Type of custom scripts loader to instantiate.</param>
    /// <returns>Custom scripts loader instance or default scripts loader instance if something goes wrong.</returns>
    private static IScriptsLoader ActivateLoaderInstance(Type loaderType)
    {
        try
        {
            return (IScriptsLoader) Activator.CreateInstance(loaderType);
        }
        catch (Exception ex)
        {
            PrintError($"An error has occured during custom scripts loader instance creation: {ex.Message}.");
            return new FileScriptsLoader();
        }
    }

    /// <summary>
    /// Stores plugin configuration.
    /// </summary>
    [PluginConfig]
    public Config PluginConfig;

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
        ReloadConfig();
        _scriptsLoader?.Dispose();
        _scriptsLoader = LoadScriptsLoader();
        _scriptsLoader.InitScriptsLoader(this, PluginConfig.CustomPermissionsResolver, PluginConfig.EnableScriptEventHandlers);
    }

    /// <summary>
    /// Reloads plugin config values.
    /// </summary>
    private void ReloadConfig()
    {
        var handler = PluginHandler.Get(this);
        handler?.LoadConfig(this, nameof(PluginConfig));
    }

    /// <summary>
    /// Loads scripts loader.
    /// </summary>
    /// <returns>Loaded scripts loader instance.</returns>
    private IScriptsLoader LoadScriptsLoader()
    {
        if (string.IsNullOrWhiteSpace(PluginConfig.CustomScriptsLoader))
        {
            return new FileScriptsLoader();
        }

        var loaderType = GetCustomScriptsLoader();

        if (loaderType is null)
        {
            return new FileScriptsLoader();
        }

        if (!typeof(IScriptsLoader).IsAssignableFrom(loaderType))
        {
            PrintError("Custom scripts loader does not implement required interface.");
            return new FileScriptsLoader();
        }

        return ActivateLoaderInstance(loaderType);
    }

    /// <summary>
    /// Retrieves custom scripts loader type.
    /// </summary>
    /// <returns>Custom scripts loader type or <see langword="null" /> if nothing was found.</returns>
    private Type GetCustomScriptsLoader()
    {
        try
        {
            return Type.GetType(PluginConfig.CustomScriptsLoader);
        }
        catch (Exception ex)
        {
            PrintError($"An error has occured during custom scripts loader type search: {ex.Message}.");
            return null;
        }
    }
}
