using System;

namespace SLCommandScript.FileScriptsLoader.Helpers;

/// <summary>
/// Provides helper objects.
/// </summary>
public static class HelpersProvider
{
    /// <summary>
    /// Contains file system helper object to use.
    /// </summary>
    public static IFileSystemHelper FileSystemHelper { get; set; } = null;

    /// <summary>
    /// Contains plugin helper object to use.
    /// </summary>
    public static IPluginHelper PluginHelper { get; set; } = null;

    /// <summary>
    /// Cointains currently used file system watcher helper factory.
    /// </summary>
    public static Func<string, string, bool, IFileSystemWatcherHelper> FileSystemWatcherHelperFactory { get; set; } = null;
}
