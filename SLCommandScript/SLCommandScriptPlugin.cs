using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using SLCommandScript.Commands;
using SLCommandScript.Core;
using SLCommandScript.Core.Commands;
using SLCommandScript.Core.Reflection;
using System;

namespace SLCommandScript;

/// <summary>
/// Defines plugin functionality.
/// </summary>
public class SLCommandScriptPlugin : Plugin
{
    /// <summary>
    /// Contains plugin name to display.
    /// </summary>
    public const string PluginName = "SLCommandScript";

    /// <summary>
    /// Contains current plugin version.
    /// </summary>
    public const string PluginVersion = "2.0.0";

    /// <summary>
    /// Contains plugin description.
    /// </summary>
    public const string PluginDescription = "Simple, commands based scripting language.";

    /// <summary>
    /// Contains plugin author.
    /// </summary>
    public const string PluginAuthor = "Adam Szerszenowicz";

    /// <inheritdoc />
    public override string Name { get; } = PluginName;

    /// <inheritdoc />
    public override string Description { get; } = PluginDescription;

    /// <inheritdoc />
    public override string Author { get; } = PluginAuthor;

    /// <inheritdoc />
    public override Version Version { get; } = new(PluginVersion);

    /// <inheritdoc />
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    /// <inheritdoc />
    public override LoadPriority Priority => LoadPriority.Lowest;

    /// <summary>
    /// Stores plugin configuration.
    /// </summary>
    private Config _pluginConfig = null!;

    /// <summary>
    /// Stores scripts loader configuration.
    /// </summary>
    private ScriptsLoaderConfig _scriptsLoaderConfig = null!;

    /// <summary>
    /// Stores a reference to scripts loader.
    /// </summary>
    private IScriptsLoader? _scriptsLoader;

    /// <summary>
    /// Stores a reference to helper commands.
    /// </summary>
    private HelperCommands? _helperCommands;

    /// <inheritdoc />
    public override void Enable()
    {
        if (_scriptsLoader is not null)
        {
            return;
        }

        if (_pluginConfig is null || _scriptsLoaderConfig is null)
        {
            LoadConfigs();
        }

        Logger.Info("Enabling SLCS...");
        _scriptsLoader = LoadScriptsLoader();

        if (_scriptsLoader is null)
        {
            Logger.Error("Could not load scripts loader, SLCS is disabled.");
            return;
        }

        RegisterHelperCommands();
        _scriptsLoader.InitScriptsLoader(this, _scriptsLoaderConfig);
        Logger.Info("SLCS is enabled.");
    }

    /// <inheritdoc />
    public override void Disable()
    {
        if (_scriptsLoader is null)
        {
            return;
        }

        Logger.Info("Disabling SLCS...");
        _scriptsLoader.Dispose();
        _scriptsLoader = null;
        UnregisterHelperCommands();
        Logger.Info("SLCS is disabled.");
    }

    /// <inheritdoc />
    public override void LoadConfigs()
    {
        if (!this.TryLoadConfig("pluginConfig.yml", out Config? pluginConfig))
        {
            Logger.Warn("Failed to load plugin config, using default values.");
            _pluginConfig = new();
        }

        _pluginConfig = pluginConfig!;

        if (!this.TryLoadConfig("scriptsLoaderConfig.yml", out ScriptsLoaderConfig? scriptsConfig))
        {
            Logger.Warn("Failed to load scripts loader config, using default values.");
            _scriptsLoaderConfig = new();
        }

        _scriptsLoaderConfig = scriptsConfig!;
    }

    /// <summary>
    /// Loads scripts loader.
    /// </summary>
    /// <returns>Loaded scripts loader instance or <see langword="null" /> if something went wrong.</returns>
    private IScriptsLoader? LoadScriptsLoader()
    {
        var implementation = _pluginConfig.ScriptsLoaderImplementation;

        if (string.IsNullOrWhiteSpace(implementation))
        {
            Logger.Error("Scripts loader implementation name is blank.");
            return null;
        }

        var loader = CustomTypesUtils.MakeCustomTypeInstance<IScriptsLoader>(implementation, out var message);

        if (loader is null)
        {
            Logger.Error(message!);
            return null;
        }

        Logger.Info("Scripts loader loaded successfully.");
        return loader;
    }

    /// <summary>
    /// Registers helper commands.
    /// </summary>
    private void RegisterHelperCommands()
    {
        if (!_pluginConfig.EnableHelperCommands || _scriptsLoaderConfig.AllowedScriptCommandTypes == 0)
        {
            return;
        }
        
        _helperCommands ??= new(_scriptsLoader);
        var targetTypes = _scriptsLoaderConfig.AllowedScriptCommandTypes;
        var registered = CommandsUtils.RegisterCommand(targetTypes, _helperCommands);

        if (registered != targetTypes)
        {
            Logger.Warn($"Could not register helper commands for {targetTypes ^ (registered ?? 0)}");
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

        var targetTypes = _scriptsLoaderConfig.AllowedScriptCommandTypes;
        var unregistered = CommandsUtils.UnregisterCommand(targetTypes, _helperCommands);

        if (unregistered != targetTypes)
        {
            Logger.Warn($"Could not unregister helper commands from {targetTypes ^ (unregistered ?? 0)}");
        }
        else
        {
            _helperCommands = null;
        }
    }
}
