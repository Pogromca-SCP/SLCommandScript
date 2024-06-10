using PluginAPI.Core;
using PluginAPI.Enums;
using SLCommandScript.Core;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Permissions;
using SLCommandScript.Core.Reflection;
using SLCommandScript.FileScriptsLoader.Commands;
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
    public const string ProjectVersion = "0.6.1";

    /// <summary>
    /// Contains project author.
    /// </summary>
    public const string ProjectAuthor = "Adam Szerszenowicz";

    /// <summary>
    /// Prefix string to use in logs.
    /// </summary>
    private const string LoaderPrefix = "FileScriptsLoader: ";

    /// <summary>
    /// Contains a reference to initialized instance.
    /// </summary>
    private static FileScriptsLoader _instance = null;

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

    /// <inheritdoc />
    public string LoaderName => ProjectName;

    /// <inheritdoc />
    public string LoaderVersion => ProjectVersion;

    /// <inheritdoc />
    public string LoaderAuthor => ProjectAuthor;

    /// <summary>
    /// Contains all scripts directories monitors.
    /// </summary>
    private readonly List<CommandsDirectory> _registeredDirectories = [];

    /// <summary>
    /// Contains events directory monitor.
    /// </summary>
    private EventsDirectory _eventsDirectory;

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

        if (_instance is not null)
        {
            PrintError("Only one instance of FileScriptsLoader can be initialized.");
            return;
        }

        _instance = this;
        PrintLog("Initializing scripts loader...");
        loaderConfig ??= new();
        FileScriptCommandBase.PermissionsResolver = LoadPermissionsResolver(loaderConfig.CustomPermissionsResolver);
        FileScriptCommandBase.ConcurrentExecutionsLimit = loaderConfig.ScriptExecutionsLimit;
        HelpersProvider.FileSystemHelper ??= new FileSystemHelper();
        HelpersProvider.FileSystemWatcherHelperFactory ??= CreateWatcher;
        LoadDirectory(plugin, $"{handler.PluginDirectoryPath}/scripts/events/", loaderConfig.EnableScriptEventHandlers ? CommandType.Console : 0);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/ra/", loaderConfig.AllowedScriptCommandTypes & CommandType.RemoteAdmin);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/server/", loaderConfig.AllowedScriptCommandTypes & CommandType.Console);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/client/", loaderConfig.AllowedScriptCommandTypes & CommandType.GameConsole);
        PrintLog("Scripts loader is initialized.");
    }

    /// <summary>
    /// Releases resources.
    /// </summary>
    ~FileScriptsLoader() => PerformCleanup();

    /// <inheritdoc />
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
        PrintLog("Disabling scripts loader...");

        foreach (var dir in _registeredDirectories)
        {
            dir.Dispose();
        }

        _registeredDirectories.Clear();
        _eventsDirectory?.Dispose();
        _eventsDirectory = null;

        if (!ReferenceEquals(_instance, this))
        {
            PrintLog("Scripts loader is disabled but static helpers are still active.");
            return;
        }

        FileScriptCommandBase.ConcurrentExecutionsLimit = 0;
        FileScriptCommandBase.PermissionsResolver = null;
        HelpersProvider.FileSystemHelper = null;
        HelpersProvider.FileSystemWatcherHelperFactory = null;
        _instance = null;
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
            PrintLog($"Initializing commands directory for {handlerType}...");
            _registeredDirectories.Add(new CommandsDirectory(HelpersProvider.FileSystemWatcherHelperFactory(directory, null, true), handlerType));
        }
        else
        {
            PrintLog("Initializing events directory...");
            _eventsDirectory = new EventsDirectory(plugin, HelpersProvider.FileSystemWatcherHelperFactory(directory, EventsDirectory.ScriptFilesFilter, false));
        }
    }
}
