using System;
using System.Collections.Generic;
using SLCommandScript.Commands;
using System.IO;
using PluginAPI.Core;

namespace SLCommandScript.Loader
{
    /// <summary>
    /// Server files script loader
    /// </summary>
    public class FileScriptsLoader : IScriptsLoader
    {
        /// <summary>
        /// Monitors a directory and related scripts
        /// </summary>
        private class CommandsDirectory : IDisposable
        {
            /// <summary>
            /// Contains all registered scripts commands from monitored directory
            /// </summary>
            public Dictionary<string, FileScriptCommand> Commands { get; private set; }

            /// <summary>
            /// Contains handler type used for commands cleanup
            /// </summary>
            public CommandHandlerType HandlerType { get; private set; }

            /// <summary>
            /// File system watcher used to detect script files changes
            /// </summary>
            public FileSystemWatcher Watcher { get; private set; }

            /// <summary>
            /// Creates new directory monitor and initializes the watcher
            /// </summary>
            /// <param name="directory">File directory to monitor for changes</param>
            /// <param name="handlerType">Type of handler to use</param>
            public CommandsDirectory(string directory, CommandHandlerType handlerType)
            {
                Commands = new Dictionary<string, FileScriptCommand>();
                HandlerType = handlerType;
                Watcher = new FileSystemWatcher(directory);
                Watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName;
                Watcher.Filter = ScriptFileFilter;
                Watcher.IncludeSubdirectories = true;
                Watcher.EnableRaisingEvents = true;
            }

            /// <summary>
            /// Disposes the watcher and performs command cleanup
            /// </summary>
            public void Dispose()
            {
                Watcher.Dispose();

                foreach (var command in Commands.Values)
                {
                    CommandsUtils.UnregisterCommand(HandlerType, command);
                }
            }
        }

        /// <summary>
        /// Defines script files extension filter
        /// </summary>
        private const string ScriptFileFilter = "*.slc";

        /// <summary>
        /// Contains all scripts directories monitors
        /// </summary>
        private readonly Dictionary<CommandHandlerType, CommandsDirectory> _registeredDirectories =
            new Dictionary<CommandHandlerType, CommandsDirectory>();

        /// <summary>
        /// Initializes scripts loader and loads the scripts
        /// </summary>
        public void InitScriptsLoader()
        {
            var handler = PluginHandler.Get(Plugin.Singleton);

            if (handler is null)
            {
                Plugin.PrintError("Cannot load plugin directory path.");
                return;
            }

            LoadDirectory($"{handler.PluginDirectoryPath}/scripts/ra/", CommandHandlerType.RemoteAdmin);
            LoadDirectory($"{handler.PluginDirectoryPath}/scripts/server/", CommandHandlerType.ServerConsole);
            LoadDirectory($"{handler.PluginDirectoryPath}/scripts/client/", CommandHandlerType.ClientConsole);
        }

        /// <summary>
        /// Unloads scripts and releases unmanaged resources
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
        /// Loads all scripts from directory
        /// </summary>
        /// <param name="directory">Directory to load</param>
        /// <param name="handlerType">Handler to use for commands registration</param>
        private void LoadDirectory(string directory, CommandHandlerType handlerType)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var dir = new CommandsDirectory(directory, handlerType);
            _registeredDirectories[handlerType] = dir;

            foreach (var file in Directory.EnumerateFiles(directory, ScriptFileFilter, SearchOption.AllDirectories))
            {
                RegisterScript(file, handlerType);
            }

            dir.Watcher.Created += (obj, args) => RegisterScript(args.FullPath, handlerType);
            dir.Watcher.Deleted += (obj, args) => UnregisterScript(args.FullPath, handlerType);
            dir.Watcher.Renamed += (obj, args) => RefreshScript(args.OldFullPath, args.FullPath, handlerType);
        }

        /// <summary>
        /// Registers a script
        /// </summary>
        /// <param name="scriptFile">Script file to register</param>
        /// <param name="handlerType">Type of handler to register into</param>
        private void RegisterScript(string scriptFile, CommandHandlerType handlerType)
        {
            var dir = _registeredDirectories[handlerType];
            var cmd = new FileScriptCommand(scriptFile);
            var registered = CommandsUtils.RegisterCommandIfMissing(handlerType, cmd);

            if (registered)
            {
                dir.Commands[cmd.Command] = cmd;
            }
            else
            {
                Plugin.PrintError($"Could not register command '{cmd.Command}'.");
            }
        }

        /// <summary>
        /// Unregisters a script
        /// </summary>
        /// <param name="scriptFile">Script file to unregister</param>
        /// <param name="handlerType">Type of handler to unregister from</param>
        private void UnregisterScript(string scriptFile, CommandHandlerType handlerType)
        {
            var dir = _registeredDirectories[handlerType];
            var cmd = dir.Commands[FileScriptCommand.FilePathToCommandName(scriptFile)];
            var removed = CommandsUtils.UnregisterCommand(handlerType, cmd);

            if (removed)
            {
                dir.Commands.Remove(cmd.Command);
            }
            else
            {
                Plugin.PrintError($"Could not unregister command '{cmd.Command}'.");
            }
        }

        /// <summary>
        /// Refreshes script name
        /// </summary>
        /// <param name="oldFileName">Old script file name to unregister</param>
        /// <param name="newFileName">New script file name to register</param>
        /// <param name="handlerType">Type of handler to refresh</param>
        private void RefreshScript(string oldFileName, string newFileName, CommandHandlerType handlerType)
        {
            UnregisterScript(oldFileName, handlerType);
            RegisterScript(newFileName, handlerType);
        }
    }
}
