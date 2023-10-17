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

    /// <summary>
    /// Contains watched directory.
    /// </summary>
    public string Directory => _watcher.Path;

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
    public event FileSystemEventHandler Created;

    /// <summary>
    /// Event invoked on file change.
    /// </summary>
    public event FileSystemEventHandler Changed;

    /// <summary>
    /// Event invoked on file renaming.
    /// </summary>
    public event RenamedEventHandler Renamed;

    /// <summary>
    /// Event invoked on file deletion.
    /// </summary>
    public event FileSystemEventHandler Deleted;
}
