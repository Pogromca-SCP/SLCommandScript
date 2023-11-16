using PluginAPI.Core;
using System;

namespace SLCommandScript.Core.Interfaces;

/// <summary>
/// Interface to implement in order to create a custom scripts loader.
/// </summary>
public interface IScriptsLoader : IDisposable
{
    /// <summary>
    /// Initializes the scripts loader.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="handler">Plugin handler object.</param>
    /// <param name="loaderConfig">Scripts loader configuration to use.</param>
    void InitScriptsLoader(object plugin, PluginHandler handler, ScriptsLoaderConfig loaderConfig);
}
