using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using SLCommandScript.Commands;
using PluginAPI.Enums;
using System.IO;
using SLCommandScript.Core.Commands;
using PluginAPI.Core;

namespace SLCommandScript.Loader;

/// <summary>
/// Server files script loader.
/// </summary>
public class FileScriptsLoader : IScriptsLoader
{
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
        /// File system watcher used to detect script files changes.
        /// </summary>
        public FileSystemWatcher Watcher { get; private set; }

        /// <summary>
        /// Creates new directory monitor and initializes the watcher.
        /// </summary>
        /// <param name="directory">File directory to monitor for changes.</param>
        /// <param name="handlerType">Type of handler to use.</param>
        public CommandsDirectory(string directory, CommandType handlerType)
        {
            Commands = new(StringComparer.OrdinalIgnoreCase);
            HandlerType = handlerType;

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
            var cmd = new FileScriptCommand(scriptFile);
            var registered = CommandsUtils.RegisterCommand(HandlerType, cmd);

            if (registered != null)
            {
                Commands[cmd.Command] = cmd;
            }
            else
            {
                Plugin.PrintError($"Could not register command '{cmd.Command}'.");
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
                Plugin.PrintError($"Could not unregister command '{cmd.Command}'.");
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

    /// <summary>
    /// Defines script files extension filter.
    /// </summary>
    private const string ScriptFilesFilter = "*.slc";

    /// <summary>
    /// Contains all scripts directories monitors.
    /// </summary>
    private readonly Dictionary<CommandType, CommandsDirectory> _registeredDirectories = new();

    /// <summary>
    /// Initializes scripts loader and loads the scripts.
    /// </summary>
    public void InitScriptsLoader()
    {
        var handler = PluginHandler.Get(Plugin.Singleton);

        if (handler is null)
        {
            Plugin.PrintError("Cannot load plugin directory path.");
            return;
        }

        LoadDirectory($"{handler.PluginDirectoryPath}/scripts/ra/", CommandType.RemoteAdmin);
        LoadDirectory($"{handler.PluginDirectoryPath}/scripts/server/", CommandType.Console);
        LoadDirectory($"{handler.PluginDirectoryPath}/scripts/client/", CommandType.GameConsole);
    }

    /// <summary>
    /// Unloads scripts and releases unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var dir in _registeredDirectories.Values)
        {
            dir.Dispose();
        }

        _registeredDirectories.Clear();
    }

    /// <summary>
    /// Loads all scripts from directory.
    /// </summary>
    /// <param name="directory">Directory to load.</param>
    /// <param name="handlerType">Handler to use for commands registration.</param>
    private void LoadDirectory(string directory, CommandType handlerType)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var dir = new CommandsDirectory(directory, handlerType);
        _registeredDirectories[handlerType] = dir;
    }
}
