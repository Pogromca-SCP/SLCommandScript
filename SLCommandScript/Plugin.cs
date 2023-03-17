using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using SLCommandScript.Loader;
using PluginAPI.Enums;
using System;

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
        /// Stores plugin configuration
        /// </summary>
        [PluginConfig]
        public Config PluginConfig;

        /// <summary>
        /// Stores a reference to scripts loader
        /// </summary>
        private IScriptsLoader _scriptsLoader;

        /// <summary>
        /// Loads and initializes the plugin
        /// </summary>
        [PluginPriority(LoadPriority.Highest)]
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
            _scriptsLoader?.UnloadScripts();
            _scriptsLoader = null;
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
            _scriptsLoader?.UnloadScripts();
            InitScriptsLoader();
            _scriptsLoader.LoadScripts();
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
        private void InitScriptsLoader()
        {
            if (string.IsNullOrWhiteSpace(PluginConfig.CustomScriptsLoader))
            {
                _scriptsLoader = new FileScriptsLoader();
                return;
            }

            Type loaderType = null;

            try
            {
                loaderType = Type.GetType(PluginConfig.CustomScriptsLoader);
            }
            catch (Exception) {}

            if (loaderType is null || !loaderType.IsSubclassOf(typeof(IScriptsLoader)))
            {
                PrintError("Custom scripts loader is invalid.");
                _scriptsLoader = new FileScriptsLoader();
                return;
            }

            try
            {
                _scriptsLoader = (IScriptsLoader) Activator.CreateInstance(loaderType);
            }
            catch (Exception)
            {
                PrintError("Custom scripts loader could not be loaded.");
                _scriptsLoader = new FileScriptsLoader();
            }
        }
    }
}
