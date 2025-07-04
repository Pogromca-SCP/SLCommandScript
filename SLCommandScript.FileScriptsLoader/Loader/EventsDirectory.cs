using LabApi.Features.Console;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Events;
using SLCommandScript.FileScriptsLoader.Helpers;
using System;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.Loader;

/// <summary>
/// Monitors a directory and related scripts.
/// </summary>
public class EventsDirectory : IDisposable, IFileScriptCommandParent
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
    /// Contains used event handler.
    /// </summary>
    public FileScriptsEventHandler Handler { get; }

    /// <summary>
    /// File system watcher used to detect script files changes.
    /// </summary>
    public IFileSystemWatcherHelper? Watcher { get; }

    /// <summary>
    /// Contains commands configuration to apply.
    /// </summary>
    public RuntimeConfig Config { get; }

    /// <summary>
    /// Creates new directory monitor and initializes the watcher.
    /// </summary>
    /// <param name="watcher">File system watcher to use.</param>
    /// <param name="config">Runtime configuration to use by event scripts.</param>
    public EventsDirectory(IFileSystemWatcherHelper? watcher, RuntimeConfig? config)
    {
        Handler = new();
        Watcher = watcher;
        Config = config ?? new(null, null, 10);

        if (Watcher is null)
        {
            return;
        }

        foreach (var file in Config.FileSystemHelper.EnumerateFiles(Watcher.Directory, ScriptFilesFilter, SearchOption.TopDirectoryOnly))
        {
            RegisterEvent(file);
        }

        Watcher.Created += (obj, args) => RegisterEvent(args.FullPath);
        Watcher.Deleted += (obj, args) => UnregisterEvent(args.FullPath);
        Watcher.Renamed += (obj, args) => RefreshEvent(args.OldFullPath, args.FullPath);
        Watcher.Error += (obj, args) => Logger.Error($"An events watcher error has occured: {args.GetException().Message}");
        Watcher.RegisterEvents(Handler);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public string GetLocation(bool includeRoot = false) => Watcher is not null && includeRoot ? Watcher.Directory : string.Empty;

    /// <summary>
    /// Disposes the watcher and unregisters events.
    /// </summary>
    /// <param name="disposing">Whether or not this method is invoked from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || Watcher is null)
        {
            return;
        }

        Watcher.UnregisterEvents(Handler);
        Watcher.Dispose();
    }

    /// <summary>
    /// Registers an event.
    /// </summary>
    /// <param name="scriptFile">Event script file to register.</param>
    private void RegisterEvent(string scriptFile)
    {
        var cmd = new FileScriptCommandBase(Config.FileSystemHelper.GetFileNameWithoutExtension(scriptFile), this, Config);
        var name = cmd.Command;

        if (name.Length > EventHandlerPrefix.Length && name.StartsWith(EventHandlerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(EventHandlerPrefix.Length);
        }

        var parsed = Enum.TryParse<EventType>(name, true, out var result);

        if (parsed)
        {
            Handler.EventScripts[result] = cmd;
            Logger.Info($"Registered event handler for '{result}' event.");
        }
        else
        {
            Logger.Warn($"Could not register event handler for '{name}' event.");
        }
    }

    /// <summary>
    /// Unregisters an event.
    /// </summary>
    /// <param name="scriptFile">Event script file to unregister.</param>
    private void UnregisterEvent(string scriptFile)
    {
        var name = Config.FileSystemHelper.GetFileNameWithoutExtension(scriptFile);

        if (name is null)
        {
            return;
        }

        if (name.Length > EventHandlerPrefix.Length && name.StartsWith(EventHandlerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(EventHandlerPrefix.Length);
        }

        var parsed = Enum.TryParse<EventType>(name, true, out var result);

        if (parsed)
        {
            Handler.EventScripts.Remove(result);
            Logger.Info($"Unregistered event handler for '{result}' event.");
        }
        else
        {
            Logger.Warn($"Could not unregister event handler for '{name}' event.");
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
