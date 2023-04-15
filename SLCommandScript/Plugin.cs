using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using SLCommandScript.Core.Loader;
using System;
using SLCommandScript.Loader;
using SLCommandScript.Commands;
using PluginAPI.Enums;
using SLCommandScript.Core.Commands;

namespace SLCommandScript
{

    /// <summary>
    /// Defines plugin functionality
    /// </summary>
    public class Plugin
    {
        /// <summary>
        /// Plugin prefix to use in logs
        /// </summary>
        private const string PluginPrefix = "SLCommandScript: ";

        /// <summary>
        /// Stores plugin's singleton reference
        /// </summary>
        public static Plugin Singleton { get; private set; }

        /// <summary>
        /// Prints an info message to server log
        /// </summary>
        /// <param name="message">Message to print</param>
        public static void PrintLog(string message) => Log.Info(message, PluginPrefix);

        /// <summary>
        /// Prints an error message to server log
        /// </summary>
        /// <param name="message">Message to print</param>
        public static void PrintError(string message) => Log.Error(message, PluginPrefix);

        /// <summary>
        /// Creates custom scripts loader instance
        /// </summary>
        /// <param name="loaderType">Type of custom scripts loader to instantiate</param>
        /// <returns>Custom scripts loader instance or default scripts loader instance if something goes wrong</returns>
        private static IScriptsLoader ActivateLoaderInstance(Type loaderType)
        {
            try
            {
                return (IScriptsLoader) Activator.CreateInstance(loaderType);
            }
            catch (Exception ex)
            {
                PrintError($"An error has occured during custom scripts loader instance creation: {ex.Message}");
                return new FileScriptsLoader();
            }
        }

        /// <summary>
        /// Stores plugin configuration
        /// </summary>
        [PluginConfig]
        public Config PluginConfig;

        /// <summary>
        /// Stores a reference to scripts loader
        /// </summary>
        private IScriptsLoader _scriptsLoader;

        /// <summary>
        /// Stores flow control commands
        /// </summary>
        private FlowCommand _flowCommand;

        /// <summary>
        /// Stores scope control commands
        /// </summary>
        private ScopeCommand _scopeCommand;

        /// <summary>
        /// Loads and initializes the plugin
        /// </summary>
        [PluginPriority(LoadPriority.Lowest)]
        [PluginEntryPoint("SLCommandScript", "1.0.0", "Simple commands based scripting language for SCP: Secret Laboratory", "Adam Szerszenowicz")]
        void LoadPlugin()
        {
            PrintLog("Plugin load started...");
            Init();
            PrintLog("Plugin is loaded.");
        }

        /// <summary>
        /// Reloads the plugin
        /// </summary>
        [PluginReload]
        void ReloadPlugin()
        {
            PrintLog("Plugin reload started...");
            Init();
            PrintLog("Plugin reloaded.");
        }

        /// <summary>
        /// Unloads the plugin
        /// </summary>
        [PluginUnload]
        void UnloadPlugin()
        {
            PrintLog("Plugin unload started...");
            _scriptsLoader?.Dispose();
            _scriptsLoader = null;
            ClearCommands();
            _flowCommand = null;
            _scopeCommand = null;
            PluginConfig = null;
            Singleton = null;
            PrintLog("Plugin is unloaded.");
        }

        /// <summary>
        /// Plugin components initialization
        /// </summary>
        private void Init()
        {
            Singleton = this;
            ReloadConfig();
            _scriptsLoader?.Dispose();
            ClearCommands();
            InitCommands();
            _scriptsLoader = InitScriptsLoader();
            _scriptsLoader.InitScriptsLoader();
        }

        /// <summary>
        /// Reloads plugin config values
        /// </summary>
        private void ReloadConfig()
        {
            if (PluginConfig is null)
            {
                PluginConfig = new Config();
            }

            var handler = PluginHandler.Get(this);
            handler?.LoadConfig(this, nameof(PluginConfig));
        }

        /// <summary>
        /// Initializes scripts loader
        /// </summary>
        private IScriptsLoader InitScriptsLoader()
        {
            if (string.IsNullOrWhiteSpace(PluginConfig.CustomScriptsLoader))
            {
                return new FileScriptsLoader();
            }

            var loaderType = GetCustomScriptsLoader();

            if (loaderType is null)
            {
                return new FileScriptsLoader();
            }

            if (!typeof(IScriptsLoader).IsAssignableFrom(loaderType))
            {
                PrintError("Custom scripts loader does not implement required interface.");
                return new FileScriptsLoader();
            }

            return ActivateLoaderInstance(loaderType);
        }

        /// <summary>
        /// Retrieves custom scripts loader type
        /// </summary>
        /// <returns>Custom scripts loader type or null if nothing was found</returns>
        private Type GetCustomScriptsLoader()
        {
            try
            {
                return Type.GetType(PluginConfig.CustomScriptsLoader);
            }
            catch (Exception ex)
            {
                PrintError($"An error has occured during custom scripts loader type search: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Initializes built-in commands
        /// </summary>
        private void InitCommands()
        {
            if (_flowCommand is null)
            {
                _flowCommand = new FlowCommand();
            }

            if (_scopeCommand is null)
            {
                _scopeCommand = new ScopeCommand();
            }

            CommandsUtils.RegisterCommand(CommandHandlerType.RemoteAdmin, _flowCommand);
            CommandsUtils.RegisterCommand(CommandHandlerType.ServerConsole, _flowCommand);
            CommandsUtils.RegisterCommand(CommandHandlerType.ClientConsole, _flowCommand);
            CommandsUtils.RegisterCommand(CommandHandlerType.RemoteAdmin, _scopeCommand);
        }

        /// <summary>
        /// Clears built-in commands
        /// </summary>
        private void ClearCommands()
        {
            CommandsUtils.UnregisterCommand(CommandHandlerType.RemoteAdmin, _flowCommand);
            CommandsUtils.UnregisterCommand(CommandHandlerType.ServerConsole, _flowCommand);
            CommandsUtils.UnregisterCommand(CommandHandlerType.ClientConsole, _flowCommand);
            CommandsUtils.UnregisterCommand(CommandHandlerType.RemoteAdmin, _scopeCommand);
        }
    }
}
