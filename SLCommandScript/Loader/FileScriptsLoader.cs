using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using CommandSystem;
using PluginAPI.Enums;
using System.IO;
using SLCommandScript.Commands;
using SLCommandScript.Core.Commands;
using PluginAPI.Events;
using SLCommandScript.Events;
using PluginAPI.Core;
using SLCommandScript.Core.Reflection;
using SLCommandScript.Core.Permissions;

namespace SLCommandScript.Loader;

/// <summary>
/// Server files script loader.
/// </summary>
public class FileScriptsLoader : IScriptsLoader
{
    #region Commands Directory
    /// <summary>
    /// Monitors a directory and related scripts.
    /// </summary>
    private class CommandsDirectory : IDisposable
    {
        /// <summary>
        /// Contains all registered scripts commands from monitored directory.
        /// </summary>
        public Dictionary<string, ICommand> Commands { get; private set; }

        /// <summary>
        /// Contains handler type used for commands cleanup.
        /// </summary>
        public CommandType HandlerType { get; private set; }

        /// <summary>
        /// Permissions resolver to use.
        /// </summary>
        public IPermissionsResolver PermissionsResolver { get; private set; }

        /// <summary>
        /// File system watcher used to detect script files changes.
        /// </summary>
        public FileSystemWatcher Watcher { get; private set; }

        /// <summary>
        /// Creates new directory monitor and initializes the watcher.
        /// </summary>
        /// <param name="directory">File directory to monitor for changes.</param>
        /// <param name="handlerType">Type of handler to use.</param>
        /// <param name="resolver">Permissions resolver to use.</param>
        public CommandsDirectory(string directory, CommandType handlerType, IPermissionsResolver resolver)
        {
            Commands = new(StringComparer.OrdinalIgnoreCase);
            HandlerType = handlerType;
            PermissionsResolver = resolver;

            Watcher = new(directory)
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
                Filter = ScriptFilesFilter,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            foreach (var file in Directory.EnumerateFiles(directory, ScriptFilesFilter, SearchOption.AllDirectories))
            {
                RegisterScript(file);
            }

            Watcher.Created += (obj, args) => RegisterScript(args.FullPath);
            Watcher.Deleted += (obj, args) => UnregisterScript(args.FullPath);
            Watcher.Renamed += (obj, args) => RefreshScript(args.OldFullPath, args.FullPath);
        }

        /// <summary>
        /// Disposes the watcher and performs command cleanup.
        /// </summary>
        public void Dispose()
        {
            Watcher.Dispose();

            foreach (var command in Commands.Values)
            {
                CommandsUtils.UnregisterCommand(HandlerType, command);
            }
        }

        /// <summary>
        /// Registers a script.
        /// </summary>
        /// <param name="scriptFile">Script file to register.</param>
        private void RegisterScript(string scriptFile)
        {
            var cmd = new FileScriptCommand(scriptFile, PermissionsResolver);
            var registered = CommandsUtils.RegisterCommand(HandlerType, cmd);

            if (registered != null)
            {
                Commands[cmd.Command] = cmd;
            }
            else
            {
                PrintError($"Could not register command '{cmd.Command}'.");
            }
        }

        /// <summary>
        /// Unregisters a script.
        /// </summary>
        /// <param name="scriptFile">Script file to unregister.</param>
        private void UnregisterScript(string scriptFile)
        {
            var cmd = Commands[Path.GetFileNameWithoutExtension(scriptFile)];
            var removed = CommandsUtils.UnregisterCommand(HandlerType, cmd);

            if (removed != null)
            {
                Commands.Remove(cmd.Command);
            }
            else
            {
                PrintError($"Could not unregister command '{cmd.Command}'.");
            }
        }

        /// <summary>
        /// Refreshes script name.
        /// </summary>
        /// <param name="oldFileName">Old script file name to unregister.</param>
        /// <param name="newFileName">New script file name to register.</param>
        private void RefreshScript(string oldFileName, string newFileName)
        {
            UnregisterScript(oldFileName);
            RegisterScript(newFileName);
        }
    }
    #endregion

    #region Events Directory
    /// <summary>
    /// Monitors a directory and related scripts.
    /// </summary>
    private class EventsDirectory : IDisposable
    {
        /// <summary>
        /// Plugin object.
        /// </summary>
        public object PluginObject { get; private set; }

        /// <summary>
        /// Contains all registered scripts commands from monitored directory.
        /// </summary>
        public IDictionary<ServerEventType, ICommand> Commands { get; private set; }

        /// <summary>
        /// Permissions resolver to use.
        /// </summary>
        public IPermissionsResolver PermissionsResolver { get; private set; }

        /// <summary>
        /// File system watcher used to detect script files changes.
        /// </summary>
        public FileSystemWatcher Watcher { get; private set; }

        /// <summary>
        /// Creates new directory monitor and initializes the watcher.
        /// </summary>
        /// <param name="plugin">Plugin object.</param>
        /// <param name="directory">File directory to monitor for changes.</param>
        /// <param name="resolver">Permissions resolver to use.</param>
        public EventsDirectory(object plugin, string directory, IPermissionsResolver resolver)
        {
            PluginObject = plugin;
            Commands = FileScriptsEventHandlers.EventScripts;
            PermissionsResolver = resolver;

            Watcher = new(directory)
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
                Filter = ScriptFilesFilter,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            foreach (var file in Directory.EnumerateFiles(directory, ScriptFilesFilter, SearchOption.AllDirectories))
            {
                RegisterEvent(file);
            }

            Watcher.Created += (obj, args) => RegisterEvent(args.FullPath);
            Watcher.Deleted += (obj, args) => UnregisterEvent(args.FullPath);
            Watcher.Renamed += (obj, args) => RefreshEvent(args.OldFullPath, args.FullPath);
            EventManager.RegisterEvents<FileScriptsEventHandlers>(PluginObject);
        }

        /// <summary>
        /// Disposes the watcher and unregisters events.
        /// </summary>
        public void Dispose()
        {
            Watcher.Dispose();
            EventManager.UnregisterEvents<FileScriptsEventHandlers>(PluginObject);
            Commands.Clear();
        }

        /// <summary>
        /// Registers an event.
        /// </summary>
        /// <param name="scriptFile">Event script file to register.</param>
        private void RegisterEvent(string scriptFile)
        {
            var cmd = new FileScriptCommand(scriptFile, PermissionsResolver);
            var name = cmd.Command;

            if (name.Length > 2 && name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(2);
            }

            var parsed = Enum.TryParse<ServerEventType>(name, true, out var result);

            if (parsed)
            {
                Commands[result] = cmd;
            }
        }

        /// <summary>
        /// Unregisters an event.
        /// </summary>
        /// <param name="scriptFile">Event script file to unregister.</param>
        private void UnregisterEvent(string scriptFile)
        {
            var name = Path.GetFileNameWithoutExtension(scriptFile);
            
            if (name.Length > 2 && name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(2);
            }

            var parsed = Enum.TryParse<ServerEventType>(name, true, out var result);

            if (parsed)
            {
                Commands.Remove(result);
            }
        }

        /// <summary>
        /// Refreshes event script name.
        /// </summary>
        /// <param name="oldFileName">Old script file name to unregister.</param>
        /// <param name="newFileName">New script file name to register.</param>
        private void RefreshEvent(string oldFileName, string newFileName)
        {
            UnregisterEvent(oldFileName);
            RegisterEvent(newFileName);
        }
    }
    #endregion

    /// <summary>
    /// Defines script files extension filter.
    /// </summary>
    private const string ScriptFilesFilter = "*.slc";

    /// <summary>
    /// Prints an error message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    private static void PrintError(string message) => Log.Error(message, "FileScriptsLoader: ");

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
    /// <param name="permsResolver">Custom permissions resolver to use.</param>
    /// <param name="eventsEnabled">Tells if custom event handlers are enabled.</param>
    public void InitScriptsLoader(object plugin, string permsResolver, bool eventsEnabled)
    {
        var handler = PluginHandler.Get(plugin);

        if (handler is null)
        {
            PrintError("Cannot load plugin directory path.");
            return;
        }

        var permissionsResolver = CustomTypesUtils.MakeCustomTypeInstance<IPermissionsResolver>(permsResolver, out var message);

        if (permissionsResolver is null)
        {
            PrintError(message);
            permissionsResolver = new VanillaPermissionsResolver();
        }
        
        if (eventsEnabled)
        {
            LoadDirectory(plugin, $"{handler.PluginDirectoryPath}/scripts/events/", null, permissionsResolver);
        }

        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/ra/", CommandType.RemoteAdmin, permissionsResolver);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/server/", CommandType.Console, permissionsResolver);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/client/", CommandType.GameConsole, permissionsResolver);
    }

    /// <summary>
    /// Unloads scripts and releases unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var dir in _registeredDirectories)
        {
            dir.Dispose();
        }

        _registeredDirectories.Clear();
        _eventsDirectory?.Dispose();
        _eventsDirectory = null;
    }

    /// <summary>
    /// Loads all scripts from directory.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="directory">Directory to load.</param>
    /// <param name="handlerType">Handler to use for commands registration.</param>
    /// <param name="resolver">Permissions resolver to use.</param>
    private void LoadDirectory(object plugin, string directory, CommandType? handlerType, IPermissionsResolver resolver)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (handlerType == null)
        {
            _eventsDirectory = new EventsDirectory(plugin, directory, resolver);
            return;
        }

        _registeredDirectories.Add(new CommandsDirectory(directory, handlerType.Value, resolver));
    }
}
