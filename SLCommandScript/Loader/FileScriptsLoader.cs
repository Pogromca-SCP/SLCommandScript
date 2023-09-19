using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using PluginAPI.Enums;
using System.IO;
using SLCommandScript.Commands;
using SLCommandScript.Core.Commands;
using SLCommandScript.Events;
using PluginAPI.Events;
using PluginAPI.Core;
using SLCommandScript.Core;
using SLCommandScript.Core.Permissions;
using SLCommandScript.Core.Reflection;

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
        public Dictionary<string, FileScriptCommand> Commands { get; private set; }

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
        /// File system watcher used to detect description files changes.
        /// </summary>
        public FileSystemWatcher JSONWatcher { get; private set; }

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
            Watcher = CreateWatcher(directory, ScriptFilesFilter);

            foreach (var file in Directory.EnumerateFiles(directory, ScriptFilesFilter, SearchOption.AllDirectories))
            {
                RegisterScript(file);
            }

            Watcher.Created += (obj, args) => RegisterScript(args.FullPath);
            Watcher.Deleted += (obj, args) => UnregisterScript(args.FullPath);
            Watcher.Renamed += (obj, args) => RefreshScript(args.OldFullPath, args.FullPath);
            JSONWatcher = CreateWatcher(directory, DescriptionFilesFilter);

            foreach (var file in Directory.EnumerateFiles(directory, DescriptionFilesFilter, SearchOption.AllDirectories))
            {
                UpdateScriptDescription(file);
            }

            JSONWatcher.Created += (obj, args) => UpdateScriptDescription(args.FullPath);
            JSONWatcher.Changed += (obj, args) => UpdateScriptDescription(args.FullPath);
            JSONWatcher.Deleted += (obj, args) => ClearScriptDescription(args.FullPath);
            JSONWatcher.Renamed += (obj, args) => RefreshDescription(args.OldFullPath, args.FullPath);
        }

        /// <summary>
        /// Disposes the watcher and performs command cleanup.
        /// </summary>
        public void Dispose()
        {
            Watcher.Dispose();
            JSONWatcher.Dispose();

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
                PrintLog($"Registered command '{cmd.Command}' for {HandlerType}.");
            }
            else
            {
                PrintError($"Could not register command '{cmd.Command}' for {HandlerType}.");
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
                PrintLog($"Unregistered command '{cmd.Command}' from {HandlerType}.");
            }
            else
            {
                PrintError($"Could not unregister command '{cmd.Command}' from {HandlerType}.");
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

        /// <summary>
        /// Refreshes commands descriptions.
        /// </summary>
        /// <param name="oldDescName">Old description file to clear.</param>
        /// <param name="newDescName">New description file name to update.</param>
        private void RefreshDescription(string oldDescName, string newDescName)
        {
            ClearScriptDescription(oldDescName);
            UpdateScriptDescription(newDescName);
        }

        /// <summary>
        /// Updates script command description info.
        /// </summary>
        /// <param name="descFile">Description file name to use.</param>
        private void UpdateScriptDescription(string descFile)
        {
            var name = Path.GetFileNameWithoutExtension(descFile);

            if (!Commands.ContainsKey(name))
            {
                PrintError($"Could not update description for command '{name}' in {HandlerType}.");
                return;
            }

            var cmd = Commands[name];
            CommandMetaData desc;
            
            try
            {
                desc = JsonSerialize.FromFile<CommandMetaData>(descFile);
            }
            catch (Exception ex)
            {
                PrintError($"An error has occured during '{cmd.Command}' in {HandlerType} description deserialization: {ex.Message}");
                return;
            }

            cmd.Description = desc.Description;
            cmd.Usage = desc.Usage;
            cmd.Arity = desc.Arity;
            cmd.Help = desc.Help;
            PrintLog($"Description update for '{cmd.Command}' command in {HandlerType} finished successfully.");
        }

        /// <summary>
        /// Clears script command description info.
        /// </summary>
        /// <param name="descFile">Description file name to use.</param>
        private void ClearScriptDescription(string descFile)
        {
            var name = Path.GetFileNameWithoutExtension(descFile);

            if (!Commands.ContainsKey(name))
            {
                PrintError($"Could not clear description for command '{name}' in {HandlerType}.");
                return;
            }

            var cmd = Commands[name];
            cmd.Description = FileScriptCommandBase.DefaultDescription;
            cmd.Usage = null;
            cmd.Arity = 0;
            cmd.Help = null;
            PrintLog($"Description clear for '{cmd.Command}' command in {HandlerType} finished successfully.");
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
        /// Contains used event handler.
        /// </summary>
        public FileScriptsEventHandler Handler { get; private set; }

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
            Handler = new();
            PermissionsResolver = resolver;
            Watcher = CreateWatcher(directory, ScriptFilesFilter);

            foreach (var file in Directory.EnumerateFiles(directory, ScriptFilesFilter, SearchOption.AllDirectories))
            {
                RegisterEvent(file);
            }

            Watcher.Created += (obj, args) => RegisterEvent(args.FullPath);
            Watcher.Deleted += (obj, args) => UnregisterEvent(args.FullPath);
            Watcher.Renamed += (obj, args) => RefreshEvent(args.OldFullPath, args.FullPath);
            EventManager.RegisterEvents(PluginObject, Handler);
        }

        /// <summary>
        /// Disposes the watcher and unregisters events.
        /// </summary>
        public void Dispose()
        {
            Watcher.Dispose();
            EventManager.UnregisterEvents(PluginObject, Handler);
        }

        /// <summary>
        /// Registers an event.
        /// </summary>
        /// <param name="scriptFile">Event script file to register.</param>
        private void RegisterEvent(string scriptFile)
        {
            var cmd = new FileScriptCommandBase(scriptFile, PermissionsResolver);
            var name = cmd.Command;

            if (name.Length > 2 && name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(2);
            }

            var parsed = Enum.TryParse<ServerEventType>(name, true, out var result);

            if (parsed)
            {
                Handler.EventScripts[result] = cmd;
                PrintLog($"Registered event handler for '{result}' event.");
            }
            else
            {
                PrintError($"Could not register event handler for '{name}' event.");
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
                Handler.EventScripts.Remove(result);
                PrintLog($"Unregistered event handler for '{result}' event.");
            }
            else
            {
                PrintError($"Could not unregister event handler for '{name}' event.");
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
    private const string ScriptFilesFilter = "*.slcs";

    /// <summary>
    /// Defines description files extension filter.
    /// </summary>
    private const string DescriptionFilesFilter = "*.json";

    /// <summary>
    /// Prefix string to use in logs.
    /// </summary>
    private const string LoaderPrefix = "FileScriptsLoader: ";

    /// <summary>
    /// Prints a message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    private static void PrintLog(string message) => Log.Info(message, LoaderPrefix);

    /// <summary>
    /// Prints an error message to server log.
    /// </summary>
    /// <param name="message">Message to print.</param>
    private static void PrintError(string message) => Log.Error(message, LoaderPrefix);

    /// <summary>
    /// Creates new file system watcher.
    /// </summary>
    /// <param name="path">Path to watch.</param>
    /// <param name="filter">Files filter to use.</param>
    /// <returns>Newly created file watcher.</returns>
    private static FileSystemWatcher CreateWatcher(string path, string filter) => new(path)
    {
        NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
        Filter = filter,
        IncludeSubdirectories = true,
        EnableRaisingEvents = true
    };

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
    /// <param name="loaderConfig">Scripts loader configuration to use.</param>
    public void InitScriptsLoader(object plugin, ScriptsLoaderConfig loaderConfig)
    {
        loaderConfig ??= new();
        var handler = PluginHandler.Get(plugin);

        if (handler is null)
        {
            PrintError("Cannot load plugin directory path.");
            return;
        }

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
        
        LoadDirectory(plugin, $"{handler.PluginDirectoryPath}/scripts/events/", loaderConfig.EnableScriptEventHandlers ? CommandType.Console : 0, permissionsResolver);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/ra/", loaderConfig.AllowedScriptCommandTypes & CommandType.RemoteAdmin, permissionsResolver);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/server/", loaderConfig.AllowedScriptCommandTypes & CommandType.Console, permissionsResolver);
        LoadDirectory(null, $"{handler.PluginDirectoryPath}/scripts/client/", loaderConfig.AllowedScriptCommandTypes & CommandType.GameConsole, permissionsResolver);
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
    private void LoadDirectory(object plugin, string directory, CommandType handlerType, IPermissionsResolver resolver)
    {
        if (handlerType == 0)
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (plugin is null)
        {
            _registeredDirectories.Add(new CommandsDirectory(directory, handlerType, resolver));
        }
        else
        {
            _eventsDirectory = new EventsDirectory(plugin, directory, resolver);
        }
    }
}
