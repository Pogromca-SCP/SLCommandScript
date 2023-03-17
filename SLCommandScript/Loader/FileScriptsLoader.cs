using System.Collections.Generic;
using SLCommandScript.Commands;
using PluginAPI.Core;
using System.IO;

namespace SLCommandScript.Loader
{
    /// <summary>
    /// Server files script loader
    /// </summary>
    public class FileScriptsLoader : IScriptsLoader
    {
        /// <summary>
        /// Contains all registered commands
        /// </summary>
        private readonly Dictionary<CommandHandlerType, List<FileScriptCommand>> _registeredCommands =
            new Dictionary<CommandHandlerType, List<FileScriptCommand>>();

        /// <summary>
        /// Loads scripts
        /// </summary>
        public void LoadScripts()
        {
            var handler = PluginHandler.Get(Plugin.Singleton);

            if (handler is null)
            {
                Plugin.PrintError("Cannot load server directory path.");
                return;
            }

            LoadDirectory($"{handler.PluginDirectoryPath}/scripts/ra/", CommandHandlerType.RemoteAdmin);
            LoadDirectory($"{handler.PluginDirectoryPath}/scripts/server/", CommandHandlerType.ServerConsole);
            LoadDirectory($"{handler.PluginDirectoryPath}/scripts/client/", CommandHandlerType.ClientConsole);
        }

        /// <summary>
        /// Unloads scripts
        /// </summary>
        public void UnloadScripts()
        {
            foreach (var handlerType in _registeredCommands.Keys)
            {
                foreach (var cmd in _registeredCommands[handlerType])
                {
                    CommandsUtils.UnregisterCommand(handlerType, cmd);
                }
            }

            _registeredCommands.Clear();
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
                return;
            }

            if (!_registeredCommands.ContainsKey(handlerType))
            {
                _registeredCommands[handlerType] = new List<FileScriptCommand>();
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                var cmd = new FileScriptCommand(file);
                var tmp = CommandsUtils.RegisterCommand(handlerType, cmd);

                if (tmp)
                {
                    _registeredCommands[handlerType].Add(cmd);
                }
            }
        }
    }
}
