using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using SLCommandScript.Commands;
using SLCommandScript.Core;
using SLCommandScript.Core.Commands;
using SLCommandScript.Core.Reflection;

namespace SLCommandScript;

/// <summary>
/// Defines plugin functionality.
/// </summary>
public class Plugin
{
    /// <summary>
    /// Contains plugin name to display.
    /// </summary>
    public const string PluginName = "SLCommandScript";

    /// <summary>
    /// Contains current plugin version.
    /// </summary>
    public const string PluginVersion = "1.0.0";

    /// <summary>
    /// Contains plugin description.
    /// </summary>
    public const string PluginDescription = "Simple, commands based scripting language.";

    /// <summary>
    /// Contains plugin author.
    /// </summary>
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
    [PluginConfig("pluginConfig.yml")]
    public Config PluginConfig;

    /// <summary>
    /// Stores scripts loader configuration.
    /// </summary>
    [PluginConfig("scriptsLoaderConfig.yml")]
    public ScriptsLoaderConfig ScriptsLoaderConfig;

    /// <summary>
    /// Stores a reference to scripts loader.
    /// </summary>
    private IScriptsLoader _scriptsLoader;

    /// <summary>
    /// Stores a reference to helper commands.
    /// </summary>
    private HelperCommands _helperCommands;

    /// <summary>
    /// Loads and initializes the plugin.
    /// </summary>
    [PluginPriority(LoadPriority.Lowest)]
    [PluginEntryPoint(PluginName, PluginVersion, PluginDescription, PluginAuthor)]
    private void LoadPlugin()
    {
        PrintLog("Plugin load started...");
        Init();
        PrintLog("Plugin is loaded.");
    }

    /// <summary>
    /// Reloads the plugin.
    /// </summary>
    [PluginReload]
    private void ReloadPlugin()
    {
        PrintLog("Plugin reload started...");
        Init();
        PrintLog("Plugin reloaded.");
    }

    /// <summary>
    /// Unloads the plugin.
    /// </summary>
    [PluginUnload]
    private void UnloadPlugin()
    {
        PrintLog("Plugin unload started...");
        _scriptsLoader?.Dispose();
        _scriptsLoader = null;
        UnregisterHelperCommands();
        _helperCommands = null;
        ScriptsLoaderConfig = null;
        PluginConfig = null;
        PrintLog("Plugin is unloaded.");
    }

    /// <summary>
    /// Plugin components initialization.
    /// </summary>
    private void Init()
    {
        _scriptsLoader?.Dispose();
        UnregisterHelperCommands();
        var handler = PluginHandler.Get(this);

        if (handler is null)
        {
            PrintError("Plugin handler not found.");
            return;
        }

        handler.LoadConfig(this, nameof(PluginConfig));
        handler.LoadConfig(this, nameof(ScriptsLoaderConfig));
        _scriptsLoader = LoadScriptsLoader();
        RegisterHelperCommands();
        _scriptsLoader?.InitScriptsLoader(this, handler, ScriptsLoaderConfig);
    }

    /// <summary>
    /// Loads scripts loader.
    /// </summary>
    /// <returns>Loaded scripts loader instance or <see langword="null" /> if something went wrong.</returns>
    private IScriptsLoader LoadScriptsLoader()
    {
        if (string.IsNullOrWhiteSpace(PluginConfig.ScriptsLoaderImplementation))
        {
            PrintError("Scripts loader implementation name is blank.");
            return null;
        }

        var loader = CustomTypesUtils.MakeCustomTypeInstance<IScriptsLoader>(PluginConfig.ScriptsLoaderImplementation, out var message);

        if (loader is null)
        {
            PrintError(message);
            return null;
        }

        PrintLog("Scripts loader loaded successfully.");
        return loader;
    }

    /// <summary>
    /// Registers helper commands.
    /// </summary>
    private void RegisterHelperCommands()
    {
        if (!PluginConfig.EnableHelperCommands || ScriptsLoaderConfig.AllowedScriptCommandTypes == 0)
        {
            return;
        }
        
        _helperCommands = new(_scriptsLoader);
        var registered = CommandsUtils.RegisterCommand(ScriptsLoaderConfig.AllowedScriptCommandTypes, _helperCommands);

        if (registered != ScriptsLoaderConfig.AllowedScriptCommandTypes)
        {
            PrintError($"Could not register helper commands for {ScriptsLoaderConfig.AllowedScriptCommandTypes ^ (registered ?? 0)}");
        }
    }

    /// <summary>
    /// Unregisters helper commands.
    /// </summary>
    private void UnregisterHelperCommands()
    {
        if (_helperCommands is null)
        {
            return;
        }

        var unregistered = CommandsUtils.UnregisterCommand(ScriptsLoaderConfig.AllowedScriptCommandTypes, _helperCommands);

        if (unregistered != ScriptsLoaderConfig.AllowedScriptCommandTypes)
        {
            PrintError($"Could not unregister helper commands from {ScriptsLoaderConfig.AllowedScriptCommandTypes ^ (unregistered ?? 0)}");
        }
    }
}
