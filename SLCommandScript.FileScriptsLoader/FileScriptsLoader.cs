using PluginAPI.Core;
using PluginAPI.Enums;
using SLCommandScript.Core;
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
    public const string ProjectVersion = "2.0.0";

    /// <summary>
    /// Contains project author.
    /// </summary>
    public const string ProjectAuthor = "Adam Szerszenowicz";

    /// <summary>
    /// Prefix string to use in logs.
    /// </summary>
    private const string LoaderPrefix = "FileScriptsLoader: ";

    /// <summary>
    /// Prints a message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintLog(string message) => Log.Info(message, LoaderPrefix);

    /// <summary>
    /// Prints an error message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintError(string message) => Log.Error(message, LoaderPrefix);

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
    private EventsDirectory _eventsDirectory = null;

    /// <inheritdoc />
    public void InitScriptsLoader(object plugin, PluginHandler handler, ScriptsLoaderConfig loaderConfig)
    {
        if (plugin is null)
        {
            PrintError("Provided plugin object is null.");
            return;
        }

        if (handler is null)
        {
            PrintError("Provided plugin handler is null.");
            return;
        }

        PrintLog("Initializing scripts loader...");
        loaderConfig ??= new();
        var runtimeConfig = new RuntimeConfig(new FileSystemHelper(), LoadPermissionsResolver(loaderConfig.CustomPermissionsResolver), loaderConfig.ScriptExecutionsLimit);
        LoadDirectory(plugin, $"{handler.PluginDirectoryPath}/scripts/events/", loaderConfig.EnableScriptEventHandlers ? CommandType.Console : 0, runtimeConfig);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/ra/", loaderConfig.AllowedScriptCommandTypes & CommandType.RemoteAdmin, runtimeConfig);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/server/", loaderConfig.AllowedScriptCommandTypes & CommandType.Console, runtimeConfig);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/client/", loaderConfig.AllowedScriptCommandTypes & CommandType.GameConsole, runtimeConfig);
        PrintLog("Scripts loader is initialized.");
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

        PrintLog("Disabling scripts loader...");

        foreach (var dir in _registeredDirectories)
        {
            dir.Dispose();
        }

        _registeredDirectories.Clear();
        _eventsDirectory?.Dispose();
        _eventsDirectory = null;
        PrintLog("Scripts loader is disabled.");
    }

    /// <summary>
    /// Loads permissions resolver instance to use.
    /// </summary>
    /// <param name="resolverToUse">Custom type of resolver to use.</param>
    /// <returns>Loaded permissions resolver instance.</returns>
    private IPermissionsResolver LoadPermissionsResolver(string resolverToUse)
    {
        if (string.IsNullOrWhiteSpace(resolverToUse))
        {
            PrintLog("Using default permissions resolver.");
            return new VanillaPermissionsResolver();
        }

        var permissionsResolver = CustomTypesUtils.MakeCustomTypeInstance<IPermissionsResolver>(resolverToUse, out var message);

        if (permissionsResolver is null)
        {
            PrintError(message);
            return new VanillaPermissionsResolver();
        }
        else
        {
            PrintLog("Custom permissions resolver loaded successfully.");
            return permissionsResolver;
        }
    }

    /// <summary>
    /// Loads all scripts from directory.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="directory">Directory to load.</param>
    /// <param name="handlerType">Handler to use for commands registration.</param>
    /// <param name="config">Configuration to apply.</param>
    private void LoadDirectory(object plugin, string directory, CommandType handlerType, RuntimeConfig config)
    {
        if (handlerType == 0)
        {
            return;
        }

        if (!config.FileSystemHelper.DirectoryExists(directory))
        {
            config.FileSystemHelper.CreateDirectory(directory);
        }

        if (plugin is null)
        {
            PrintLog($"Initializing commands directory for {handlerType}...");
            _registeredDirectories.Add(new CommandsDirectory(new FileSystemWatcherHelper(directory, null, true), handlerType, config));
        }
        else
        {
            PrintLog("Initializing events directory...");
            _eventsDirectory = new EventsDirectory(plugin, new FileSystemWatcherHelper(directory, EventsDirectory.ScriptFilesFilter, false), config);
        }
    }
}
