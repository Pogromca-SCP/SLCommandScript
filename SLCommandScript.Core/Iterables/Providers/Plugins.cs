using PluginAPI.Core;
using PluginAPI.Loader;
using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Iterables.Providers;

/// <summary>
/// Provides multiple sources of plugin iterables.
/// </summary>
public static class PluginIterablesProvider
{
    /// <summary>
    /// Retrieves iterable object for all plugins.
    /// </summary>
    /// <returns>Iterable object for all plugins.</returns>
    public static IIterable AllPlugins() => new ListIterable<PluginHandler>(() => AssemblyLoader.InstalledPlugins, LoadVariables);

    /// <summary>
    /// Loads properties from plugin object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="plugin">Plugin to load properties from.</param>
    /// <exception cref="NullReferenceException">When provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string> targetVars, PluginHandler plugin)
    {
        targetVars["name"] = plugin.PluginName;
        targetVars["version"] = plugin.PluginVersion;
    }
}
