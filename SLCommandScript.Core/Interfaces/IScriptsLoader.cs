using System;
using PluginAPI.Enums;

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
    /// <param name="permsResolver">Custom permissions resolver to use.</param>
    /// <param name="eventsEnabled">Tells if custom event handlers are enabled.</param>
    /// <param name="enabledScopes">Tells which console scopes are enabled.</param>
    void InitScriptsLoader(object plugin, string permsResolver, bool eventsEnabled, CommandType enabledScopes);
}
