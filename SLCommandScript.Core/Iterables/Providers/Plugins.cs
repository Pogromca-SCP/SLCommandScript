using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public static IIterable AllPlugins() => new ListIterable<Plugin>(() => PluginLoader.Plugins.Keys, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all enabled plugins.
    /// </summary>
    /// <returns>Iterable object for all enabled plugins.</returns>
    public static IIterable AllEnabledPlugins() => new ListIterable<Plugin>(() => PluginLoader.EnabledPlugins, LoadVariables);

    /// <summary>
    /// Retrieves iterable object for all disabled plugins.
    /// </summary>
    /// <returns>Iterable object for all disabled plugins.</returns>
    public static IIterable AllDisabledPlugins() =>
        new ListIterable<Plugin>(() => PluginLoader.Plugins.Keys.Where(p => !PluginLoader.EnabledPlugins.Contains(p)), LoadVariables);

    /// <summary>
    /// Loads properties from plugin object and inserts them into a dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <param name="plugin">Plugin to load properties from.</param>
    /// <exception cref="NullReferenceException">When <paramref name="targetVars" /> or provided object is <see langword="null"/>.</exception>
    public static void LoadVariables(IDictionary<string, string?> targetVars, Plugin plugin)
    {
        targetVars["name"] = plugin.Name;
        targetVars["version"] = plugin.Version.ToString();
    }
}
