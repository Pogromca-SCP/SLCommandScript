using System;

namespace SLCommandScript.FileScriptsLoader.Helpers;

/// <summary>
/// Interface encapsulating file system watcher for easier testing.
/// </summary>
public interface IFileSystemWatcher : IDisposable
{
    /// <summary>
    /// Contains watched directory.
    /// </summary>
    string Directory { get; }

    /// <summary>
    /// Event invoked on file creation.
    /// </summary>
    event System.IO.FileSystemEventHandler Created;

    /// <summary>
    /// Event invoked on file change.
    /// </summary>
    event System.IO.FileSystemEventHandler Changed;

    /// <summary>
    /// Event invoked on file renaming.
    /// </summary>
    event System.IO.RenamedEventHandler Renamed;

    /// <summary>
    /// Event invoked on file deletion.
    /// </summary>
    event System.IO.FileSystemEventHandler Deleted;
}

/// <summary>
/// Handles file system watching.
/// </summary>
public class FileSystemWatcher : IFileSystemWatcher
{
    /// <summary>
    /// Contains wrapped file watcher.
    /// </summary>
    private readonly System.IO.FileSystemWatcher _watcher;

    /// <summary>
    /// Contains watched directory.
    /// </summary>
    public string Directory => _watcher.Path;

    /// <summary>
    /// Initializes new file system watcher.
    /// </summary>
    /// <param name="watcher">Watcher to wrap.</param>
    public FileSystemWatcher(System.IO.FileSystemWatcher watcher)
    {
        _watcher = watcher;
        _watcher.Created += Created;
        _watcher.Changed += Changed;
        _watcher.Renamed += Renamed;
        _watcher.Deleted += Deleted;
    }

    /// <summary>
    /// Unbinds events and releases unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _watcher.Created -= Created;
        _watcher.Changed -= Changed;
        _watcher.Renamed -= Renamed;
        _watcher.Deleted -= Deleted;
        _watcher.Dispose();
    }

    /// <summary>
    /// Event invoked on file creation.
    /// </summary>
    public event System.IO.FileSystemEventHandler Created;

    /// <summary>
    /// Event invoked on file change.
    /// </summary>
    public event System.IO.FileSystemEventHandler Changed;

    /// <summary>
    /// Event invoked on file renaming.
    /// </summary>
    public event System.IO.RenamedEventHandler Renamed;

    /// <summary>
    /// Event invoked on file deletion.
    /// </summary>
    public event System.IO.FileSystemEventHandler Deleted;
}
