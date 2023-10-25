using SLCommandScript.Core.Interfaces;
using PluginAPI.Core;
using SLCommandScript.FileScriptsLoader.Helpers;
using System.Collections.Generic;
using SLCommandScript.FileScriptsLoader.Loader;
using SLCommandScript.Core;
using SLCommandScript.Core.Permissions;
using SLCommandScript.Core.Reflection;
using SLCommandScript.FileScriptsLoader.Commands;
using PluginAPI.Enums;
using System;

namespace SLCommandScript.FileScriptsLoader;

/// <summary>
/// Server files script loader.
/// </summary>
public class FileScriptsLoader : IScriptsLoader
{
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

    /// <summary>
    /// Creates new file system watcher.
    /// </summary>
    /// <param name="path">Path to watch.</param>
    /// <param name="filter">Files filter to use.</param>
    /// <param name="includeSubdirectories">Whether or not subdirectories should be monitored.</param>
    /// <returns>Newly created file watcher.</returns>
    private static FileSystemWatcherHelper CreateWatcher(string path, string filter, bool includeSubdirectories) => new(path, filter, includeSubdirectories);

    /// <summary>
    /// Contains all scripts directories monitors.
    /// </summary>
    private readonly List<CommandsDirectory> _registeredDirectories = new();

    /// <summary>
    /// Contains events directory monitor.
    /// </summary>
    private EventsDirectory _eventsDirectory;

    /// <summary>
    /// Initializes scripts loader and loads the scripts.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="handler">Plugin handler object.</param>
    /// <param name="loaderConfig">Scripts loader configuration to use.</param>
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

        loaderConfig ??= new();
        IPermissionsResolver permissionsResolver;

        if (string.IsNullOrWhiteSpace(loaderConfig.CustomPermissionsResolver))
        {
            PrintLog("Using default permissions resolver.");
            permissionsResolver = new VanillaPermissionsResolver();
        }
        else
        {
            permissionsResolver = CustomTypesUtils.MakeCustomTypeInstance<IPermissionsResolver>(loaderConfig.CustomPermissionsResolver, out var message);

            if (permissionsResolver is null)
            {
                PrintError(message);
                permissionsResolver = new VanillaPermissionsResolver();
            }
            else
            {
                PrintLog("Custom permissions resolver loaded successfully.");
            }
        }

        FileScriptCommandBase.PermissionsResolver = permissionsResolver;
        FileScriptCommandBase.ConcurrentExecutionsLimit = loaderConfig.ScriptExecutionsLimit;
        HelpersProvider.FileSystemHelper ??= new FileSystemHelper();
        HelpersProvider.FileSystemWatcherHelperFactory ??= CreateWatcher;
        LoadDirectory(plugin, $"{handler.PluginDirectoryPath}/scripts/events/", loaderConfig.EnableScriptEventHandlers ? CommandType.Console : 0);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/ra/", loaderConfig.AllowedScriptCommandTypes & CommandType.RemoteAdmin);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/server/", loaderConfig.AllowedScriptCommandTypes & CommandType.Console);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/client/", loaderConfig.AllowedScriptCommandTypes & CommandType.GameConsole);
    }

    /// <summary>
    /// Releases resources.
    /// </summary>
    ~FileScriptsLoader() => PerformCleanup();

    /// <summary>
    /// Releases resources.
    /// </summary>
    public void Dispose()
    {
        PerformCleanup();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Unloads scripts and helpers.
    /// </summary>
    protected void PerformCleanup()
    {
        foreach (var dir in _registeredDirectories)
        {
            dir.Dispose();
        }

        _registeredDirectories.Clear();
        _eventsDirectory?.Dispose();
        _eventsDirectory = null;
        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        FileScriptCommandBase.PermissionsResolver = null;
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
    }

    /// <summary>
    /// Loads all scripts from directory.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="directory">Directory to load.</param>
    /// <param name="handlerType">Handler to use for commands registration.</param>
    private void LoadDirectory(object plugin, string directory, CommandType handlerType)
    {
        if (handlerType == 0)
        {
            return;
        }

        if (!HelpersProvider.FileSystemHelper.DirectoryExists(directory))
        {
            HelpersProvider.FileSystemHelper.CreateDirectory(directory);
        }

        if (plugin is null)
        {
            _registeredDirectories.Add(new CommandsDirectory(HelpersProvider.FileSystemWatcherHelperFactory(directory, "*.*", true), handlerType));
        }
        else
        {
            _eventsDirectory = new EventsDirectory(plugin, HelpersProvider.FileSystemWatcherHelperFactory(directory, EventsDirectory.ScriptFilesFilter, false));
        }
    }
}
