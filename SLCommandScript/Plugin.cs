using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Core;
using System.IO;
using SLCommandScript.Commands;
using System.Collections.Generic;

namespace SLCommandScript
{

    /// <summary>
    /// Defines plugin functionality
    /// </summary>
    public class Plugin
    {
        public static string ScriptsPath => $"{_singleton.PluginDirectoryPath}/scripts/";
        private static PluginHandler _singleton;
        private readonly List<ScriptCommand> _commands = new List<ScriptCommand>();

        /// <summary>
        /// Loads and initializes the plugin
        /// </summary>
        [PluginPriority(LoadPriority.Highest)]
        [PluginEntryPoint("SLCommandScript", "1.0.0", "Simple commands based scripting language for SCP: Secret Laboratory", "Adam Szerszenowicz")]
        void LoadPlugin()
        {
            Log.Info("Plugin is loaded.", "SLCommandScript: ");
            _singleton = PluginHandler.Get(this);

            foreach (var file in Directory.GetFiles(ScriptsPath))
            {
                var name = file.Substring(file.LastIndexOf('/') + 1);
                var tmp = new ScriptCommand(name, CommandContextType.RemoteAdmin);
                _commands.Add(tmp);
                tmp = new ScriptCommand(name, CommandContextType.ServerConsole);
                _commands.Add(tmp);
                tmp = new ScriptCommand(name, CommandContextType.ClientConsole);
                _commands.Add(tmp);
            }

            foreach (var cmd in _commands)
            {
                CommandsUtils.RegisterCommand(cmd.ContextType, cmd);
            }
        }

        [PluginUnload]
        void UnloadPlugin()
        {
            foreach (var cmd in _commands)
            {
                CommandsUtils.UnregisterCommand(cmd.ContextType, cmd);
            }
        }
    }
}
