using PluginAPI.Events;

namespace SLCommandScript.FileScriptsLoader.Helpers;

/// <summary>
/// Interface encapsulating plugin interactions for easier testing.
/// </summary>
public interface IPluginHelper
{
    /// <summary>
    /// Registers an event handler.
    /// </summary>
    /// <param name="plugin">Plugin to register to.</param>
    /// <param name="eventHandler">Event handler to register.</param>
    void RegisterEvents(object plugin, object eventHandler);

    /// <summary>
    /// Unegisters an event handler.
    /// </summary>
    /// <param name="plugin">Plugin to unregister from.</param>
    /// <param name="eventHandler">Event handler to unregister.</param>
    void UnregisterEvents(object plugin, object eventHandler);
}

/// <summary>
/// Handles plugin interactions.
/// </summary>
public class PluginHelper : IPluginHelper
{
    /// <summary>
    /// Registers an event handler.
    /// </summary>
    /// <param name="plugin">Plugin to register to.</param>
    /// <param name="eventHandler">Event handler to register.</param>
    public void RegisterEvents(object plugin, object eventHandler) => EventManager.RegisterEvents(plugin, eventHandler);

    /// <summary>
    /// Unegisters an event handler.
    /// </summary>
    /// <param name="plugin">Plugin to unregister from.</param>
    /// <param name="eventHandler">Event handler to unregister.</param>
    public void UnregisterEvents(object plugin, object eventHandler) => EventManager.UnregisterEvents(plugin, eventHandler);
}
