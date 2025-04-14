using PluginAPI.Events;
using System;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.Helpers;

/// <summary>
/// Interface encapsulating file system watcher for easier testing.
/// </summary>
public interface IFileSystemWatcherHelper : IDisposable
{
    /// <summary>
    /// Contains watched directory.
    /// </summary>
    string Directory { get; }

    /// <summary>
    /// Event invoked on file creation.
    /// </summary>
    event FileSystemEventHandler Created;

    /// <summary>
    /// Event invoked on file change.
    /// </summary>
    event FileSystemEventHandler Changed;

    /// <summary>
    /// Event invoked on file renaming.
    /// </summary>
    event RenamedEventHandler Renamed;

    /// <summary>
    /// Event invoked on file deletion.
    /// </summary>
    event FileSystemEventHandler Deleted;

    /// <summary>
    /// Event invoked on error.
    /// </summary>
    event ErrorEventHandler Error;

    /// <summary>
    /// Registers an event handler.
    /// </summary>
    /// <param name="plugin">Plugin object responsible for event handler.</param>
    /// <param name="eventHandler">Event handler to register.</param>
    void RegisterEvents(object plugin, object eventHandler);

    /// <summary>
    /// Unregisters an event handler.
    /// </summary>
    /// <param name="plugin">Plugin object responsible for event handler.</param>
    /// <param name="eventHandler">Event handler to unregister.</param>
    void UnregisterEvents(object plugin, object eventHandler);
}

/// <summary>
/// Handles file system watching.
/// </summary>
public class FileSystemWatcherHelper : IFileSystemWatcherHelper
{
    /// <summary>
    /// Contains wrapped file watcher.
    /// </summary>
    private readonly FileSystemWatcher _watcher;

    /// <inheritdoc />
    public string Directory => _watcher.Path;

    /// <inheritdoc />
    public event FileSystemEventHandler Created { add => _watcher.Created += value; remove => _watcher.Created -= value; }

    /// <inheritdoc />
    public event FileSystemEventHandler Changed { add => _watcher.Changed += value; remove => _watcher.Changed -= value; }

    /// <inheritdoc />
    public event RenamedEventHandler Renamed { add => _watcher.Renamed += value; remove => _watcher.Renamed -= value; }

    /// <inheritdoc />
    public event FileSystemEventHandler Deleted { add => _watcher.Deleted += value; remove => _watcher.Deleted -= value; }

    /// <inheritdoc />
    public event ErrorEventHandler Error { add => _watcher.Error += value; remove => _watcher.Error -= value; }

    /// <summary>
    /// Initializes new file system watcher.
    /// </summary>
    /// <param name="path">Path to watch.</param>
    /// <param name="filter">Files filter to use.</param>
    /// <param name="includeSubdirectories">Whether or not subdirectories should be monitored.</param>
    public FileSystemWatcherHelper(string path, string filter, bool includeSubdirectories)
    {
        _watcher = new(path)
        {
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
            Filter = filter,
            IncludeSubdirectories = includeSubdirectories,
            EnableRaisingEvents = true
        };
    }

    /// <summary>
    /// Releases resources.
    /// </summary>
    ~FileSystemWatcherHelper() => DisposeWatcher();

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeWatcher();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void RegisterEvents(object plugin, object eventHandler) => EventManager.RegisterEvents(plugin, eventHandler);

    /// <inheritdoc />
    public void UnregisterEvents(object plugin, object eventHandler) => EventManager.UnregisterEvents(plugin, eventHandler);

    /// <summary>
    /// Disposes wrapped watcher.
    /// </summary>
    protected void DisposeWatcher()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
}
