using LabApi.Features.Console;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using SLCommandScript.Core;
using SLCommandScript.Core.Commands;
using SLCommandScript.Core.Permissions;
using SLCommandScript.Core.Reflection;
using SLCommandScript.FileScriptsLoader.Helpers;
using SLCommandScript.FileScriptsLoader.Loader;
using System;
using System.Collections.Generic;

namespace SLCommandScript.FileScriptsLoader;

/// <summary>
/// Server files script loader.
/// </summary>
public class FileScriptsLoader : IScriptsLoader
{
    /// <summary>
    /// Contains project name to display.
    /// </summary>
    public const string ProjectName = "SLCommandScript.FileScriptsLoader";

    /// <summary>
    /// Contains current project version.
    /// </summary>
    public const string ProjectVersion = "2.4.0";

    /// <summary>
    /// Contains project author.
    /// </summary>
    public const string ProjectAuthor = "Adam Szerszenowicz";

    /// <inheritdoc />
    public string LoaderName => ProjectName;

    /// <inheritdoc />
    public string LoaderVersion => ProjectVersion;

    /// <inheritdoc />
    public string LoaderAuthor => ProjectAuthor;

    /// <summary>
    /// Contains all scripts directories monitors.
    /// </summary>
    private readonly List<CommandsDirectory> _registeredDirectories = new(3);

    /// <summary>
    /// Contains events directory monitor.
    /// </summary>
    private EventsDirectory? _eventsDirectory = null;

    /// <inheritdoc />
    public void InitScriptsLoader(Plugin? plugin, ScriptsLoaderConfig? loaderConfig)
    {
        if (plugin is null)
        {
            Logger.Error("Provided plugin object is null.");
            return;
        }

        Logger.Info("Initializing scripts loader...");
        loaderConfig ??= new();
        var runtimeConfig = new RuntimeConfig(new FileSystemHelper(), LoadPermissionsResolver(loaderConfig.CustomPermissionsResolver), loaderConfig.ScriptExecutionsLimit);
        var directory = plugin.GetConfigDirectory();
        LoadDirectory(false, $"{directory.FullName}/scripts/events/", loaderConfig.EnableScriptEventHandlers ? CommandType.Console : 0, runtimeConfig);
        LoadDirectory(true, $"{directory.FullName}/scripts/ra/", loaderConfig.AllowedScriptCommandTypes & CommandType.RemoteAdmin, runtimeConfig);
        LoadDirectory(true, $"{directory.FullName}/scripts/server/", loaderConfig.AllowedScriptCommandTypes & CommandType.Console, runtimeConfig);
        LoadDirectory(true, $"{directory.FullName}/scripts/client/", loaderConfig.AllowedScriptCommandTypes & CommandType.Client, runtimeConfig);
        Logger.Info("Scripts loader is initialized.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Unloads scripts and helpers.
    /// </summary>
    /// <param name="disposing">Whether or not this method is invoked from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Logger.Info("Disabling scripts loader...");

        foreach (var dir in _registeredDirectories)
        {
            dir.Dispose();
        }

        _registeredDirectories.Clear();
        _eventsDirectory?.Dispose();
        _eventsDirectory = null;
        Logger.Info("Scripts loader is disabled.");
    }

    /// <summary>
    /// Loads permissions resolver instance to use.
    /// </summary>
    /// <param name="resolverToUse">Custom type of resolver to use.</param>
    /// <returns>Loaded permissions resolver instance.</returns>
    private IPermissionsResolver LoadPermissionsResolver(string? resolverToUse)
    {
        if (string.IsNullOrWhiteSpace(resolverToUse))
        {
            Logger.Info("Using default permissions resolver.");
            return new VanillaPermissionsResolver();
        }

        var permissionsResolver = CustomTypesUtils.MakeCustomTypeInstance<IPermissionsResolver>(resolverToUse, out var message);

        if (permissionsResolver is null)
        {
            Logger.Warn($"Failed to load custom permissions resolver: {message}. Using default resolver.");
            return new VanillaPermissionsResolver();
        }
        else
        {
            Logger.Info("Custom permissions resolver loaded successfully.");
            return permissionsResolver;
        }
    }

    /// <summary>
    /// Loads all scripts from directory.
    /// </summary>
    /// <param name="isCommand">Determines the directory be for commands or events.</param>
    /// <param name="directory">Directory to load.</param>
    /// <param name="handlerType">Handler to use for commands registration.</param>
    /// <param name="config">Configuration to apply.</param>
    private void LoadDirectory(bool isCommand, string directory, CommandType handlerType, RuntimeConfig config)
    {
        if (handlerType == 0)
        {
            return;
        }

        if (!config.FileSystemHelper.DirectoryExists(directory))
        {
            config.FileSystemHelper.CreateDirectory(directory);
        }

        if (isCommand)
        {
            Logger.Info($"Initializing commands directory for {handlerType}...");
            _registeredDirectories.Add(new CommandsDirectory(new FileSystemWatcherHelper(directory, null, true), handlerType, config));
        }
        else
        {
            Logger.Info("Initializing events directory...");
            _eventsDirectory = new EventsDirectory(new FileSystemWatcherHelper(directory, EventsDirectory.ScriptFilesFilter, false), config);
        }
    }
}
