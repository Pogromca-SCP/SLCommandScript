using System;
using SLCommandScript.FileScriptsLoader.Events;
using SLCommandScript.FileScriptsLoader.Helpers;
using System.IO;
using PluginAPI.Events;
using SLCommandScript.FileScriptsLoader.Commands;
using PluginAPI.Enums;

namespace SLCommandScript.FileScriptsLoader.Loader;

/// <summary>
/// Monitors a directory and related scripts.
/// </summary>
public class EventsDirectory : IDisposable
{
    /// <summary>
    /// Defines script files extension filter.
    /// </summary>
    public const string ScriptFilesFilter = "*.slcs";

    /// <summary>
    /// Contains event handler prefix string.
    /// </summary>
    public const string EventHandlerPrefix = "on";

    /// <summary>
    /// Contains plugin object.
    /// </summary>
    public object PluginObject { get; }

    /// <summary>
    /// Contains used event handler.
    /// </summary>
    public FileScriptsEventHandler Handler { get; }

    /// <summary>
    /// File system watcher used to detect script files changes.
    /// </summary>
    public IFileSystemWatcherHelper Watcher { get; }

    /// <summary>
    /// Creates new directory monitor and initializes the watcher.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="watcher">File system watcher to use.</param>
    public EventsDirectory(object plugin, IFileSystemWatcherHelper watcher)
    {
        PluginObject = plugin;
        Handler = new();
        Watcher = watcher;

        if (Watcher is null)
        {
            return;
        }

        foreach (var file in HelpersProvider.FileSystemHelper.EnumerateFiles(Watcher.Directory, ScriptFilesFilter, SearchOption.TopDirectoryOnly))
        {
            RegisterEvent(file);
        }

        Watcher.Created += (obj, args) => RegisterEvent(args.FullPath);
        Watcher.Deleted += (obj, args) => UnregisterEvent(args.FullPath);
        Watcher.Renamed += (obj, args) => RefreshEvent(args.OldFullPath, args.FullPath);

        if (PluginObject is null)
        {
            return;
        }

        EventManager.RegisterEvents(PluginObject, Handler);
    }

    /// <summary>
    /// Disposes the watcher and unregisters events.
    /// </summary>
    public void Dispose()
    {
        Watcher?.Dispose();

        if (PluginObject is null)
        {
            return;
        }

        EventManager.UnregisterEvents(PluginObject, Handler);
    }

    /// <summary>
    /// Registers an event.
    /// </summary>
    /// <param name="scriptFile">Event script file to register.</param>
    private void RegisterEvent(string scriptFile)
    {
        var cmd = new FileScriptCommandBase(scriptFile);
        var name = cmd.Command;

        if (name.Length > EventHandlerPrefix.Length && name.StartsWith(EventHandlerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(EventHandlerPrefix.Length);
        }

        var parsed = Enum.TryParse<ServerEventType>(name, true, out var result);

        if (parsed)
        {
            Handler.EventScripts[result] = cmd;
            FileScriptsLoader.PrintLog($"Registered event handler for '{result}' event.");
        }
        else
        {
            FileScriptsLoader.PrintError($"Could not register event handler for '{name}' event.");
        }
    }

    /// <summary>
    /// Unregisters an event.
    /// </summary>
    /// <param name="scriptFile">Event script file to unregister.</param>
    private void UnregisterEvent(string scriptFile)
    {
        var name = HelpersProvider.FileSystemHelper.GetFileNameWithoutExtension(scriptFile);

        if (name.Length > EventHandlerPrefix.Length && name.StartsWith(EventHandlerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(EventHandlerPrefix.Length);
        }

        var parsed = Enum.TryParse<ServerEventType>(name, true, out var result);

        if (parsed)
        {
            Handler.EventScripts.Remove(result);
            FileScriptsLoader.PrintLog($"Unregistered event handler for '{result}' event.");
        }
        else
        {
            FileScriptsLoader.PrintError($"Could not unregister event handler for '{name}' event.");
        }
    }

    /// <summary>
    /// Refreshes event script name.
    /// </summary>
    /// <param name="oldFileName">Old script file name to unregister.</param>
    /// <param name="newFileName">New script file name to register.</param>
    private void RefreshEvent(string oldFileName, string newFileName)
    {
        UnregisterEvent(oldFileName);
        RegisterEvent(newFileName);
    }
}
