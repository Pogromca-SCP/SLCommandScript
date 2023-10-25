using System;
using SLCommandScript.FileScriptsLoader.Commands;
using System.Collections.Generic;
using CommandSystem;
using PluginAPI.Enums;
using SLCommandScript.FileScriptsLoader.Helpers;
using SLCommandScript.Core.Commands;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.Loader;

/// <summary>
/// Monitors a directory and related scripts.
/// </summary>
public class CommandsDirectory : IDisposable
{
    /// <summary>
    /// Contains script files extension.
    /// </summary>
    public const string ScriptFileExtension = ".slcs";

    /// <summary>
    /// Contains description files extension.
    /// </summary>
    public const string ScriptDescriptionExtension = ".json";

    /// <summary>
    /// Defines description files extension filter.
    /// </summary>
    public const string DescriptionFilesFilter = "*.json";

    /// <summary>
    /// Updates command description.
    /// </summary>
    /// <param name="cmd">Command to update.</param>
    /// <param name="data">New description values to set.</param>
    private static void UpdateCommandDesc(FileScriptCommand cmd, CommandMetaData data)
    {
        cmd.Description = data.Description;
        cmd.Usage = data.Usage;
        cmd.Arity = data.Arity;
        cmd.Help = data.Help;
    }

    /// <summary>
    /// Contains all subdirectories from monitored directory.
    /// </summary>
    public Dictionary<string, FileScriptDirectoryCommand> Directories { get; }

    /// <summary>
    /// Contains all registered scripts commands from monitored directory.
    /// </summary>
    public Dictionary<string, ICommand> Commands { get; }

    /// <summary>
    /// Contains handler type used for root commands.
    /// </summary>
    public CommandType HandlerType { get; }

    /// <summary>
    /// File system watcher used to detect script files changes.
    /// </summary>
    public IFileSystemWatcherHelper Watcher { get; }

    /// <summary>
    /// Creates new directory monitor and initializes the watcher.
    /// </summary>
    /// <param name="watcher">File system watcher to use.</param>
    /// <param name="handlerType">Type of handler to use.</param>
    public CommandsDirectory(IFileSystemWatcherHelper watcher, CommandType handlerType)
    {
        Directories = new(StringComparer.OrdinalIgnoreCase);
        Commands = new(StringComparer.OrdinalIgnoreCase);
        HandlerType = handlerType;
        Watcher = watcher;

        if (Watcher is null)
        {
            return;
        }

        LoadInitialFiles();
        Watcher.Created += (obj, args) => RegisterFile(args.FullPath);
        Watcher.Changed += (obj, args) => RefreshDescription(args.FullPath);
        Watcher.Deleted += (obj, args) => UnregisterFile(args.FullPath);
        Watcher.Renamed += (obj, args) => RefreshFile(args.OldFullPath, args.FullPath);
    }

    /// <summary>
    /// Releases resources.
    /// </summary>
    ~CommandsDirectory() => DisposeAndUnregisterCommands();

    /// <summary>
    /// Releases resources.
    /// </summary>
    public void Dispose()
    {
        DisposeAndUnregisterCommands();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the watcher and performs command cleanup.
    /// </summary>
    protected void DisposeAndUnregisterCommands()
    {
        Watcher?.Dispose();

        foreach (var command in Commands.Values)
        {
            CommandsUtils.UnregisterCommand(HandlerType, command);
        }
    }

    /// <summary>
    /// Processes and formats directory path.
    /// </summary>
    /// <param name="path">Path to process.</param>
    /// <returns>Processed path.</returns>
    private string ProcessDirectoryPath(string path) =>
        path.Length > Watcher.Directory.Length ? path.Substring(Watcher.Directory.Length).Replace('\\', '/').TrimStart('/') : string.Empty;

    /// <summary>
    /// Loads initial files and directories.
    /// </summary>
    private void LoadInitialFiles()
    {
        foreach (var path in HelpersProvider.FileSystemHelper.EnumerateDirectories(Watcher.Directory))
        {
            var cmd = new FileScriptDirectoryCommand(ProcessDirectoryPath(path));

            if (RegisterCommand(path, cmd))
            {
                Directories[cmd.Path] = cmd;
            }
        }

        foreach (var path in HelpersProvider.FileSystemHelper.EnumerateFiles(Watcher.Directory, EventsDirectory.ScriptFilesFilter, SearchOption.AllDirectories))
        {
            RegisterCommand(path, new FileScriptCommand(path));
        }

        foreach (var path in HelpersProvider.FileSystemHelper.EnumerateFiles(Watcher.Directory, DescriptionFilesFilter, SearchOption.AllDirectories))
        {
            UpdateScriptDescription(path);
        }
    }

    /// <summary>
    /// Registers a file.
    /// </summary>
    /// <param name="path">File path to register.</param>
    private void RegisterFile(string path)
    {
        if (HelpersProvider.FileSystemHelper.DirectoryExists(path))
        {
            var cmd = new FileScriptDirectoryCommand(ProcessDirectoryPath(path));
            
            if (RegisterCommand(path, cmd))
            {
                Directories[cmd.Path] = cmd;
            }

            return;
        }

        var ext = HelpersProvider.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            RegisterCommand(path, new FileScriptCommand(path));
            return;
        }

        if (ext.Equals(ScriptDescriptionExtension, StringComparison.OrdinalIgnoreCase))
        {
            UpdateScriptDescription(path);
        }
    }

    /// <summary>
    /// Refreshes commands descriptions.
    /// </summary>
    /// <param name="path">File path to update.</param>
    private void RefreshDescription(string path)
    {
        var ext = HelpersProvider.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptDescriptionExtension, StringComparison.OrdinalIgnoreCase))
        {
            UpdateScriptDescription(path);
        }
    }

    /// <summary>
    /// Unregisters a file.
    /// </summary>
    /// <param name="path">File path to unregister.</param>
    private void UnregisterFile(string path)
    {
        if (HelpersProvider.FileSystemHelper.DirectoryExists(path))
        {
            var cmd = UnregisterCommand(path) as FileScriptDirectoryCommand;

            if (cmd is not null)
            {
                Directories.Remove(cmd.Path);
            }

            return;
        }

        var ext = HelpersProvider.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            UnregisterCommand(path);
        }
    }

    /// <summary>
    /// Refreshes a file.
    /// </summary>
    /// <param name="oldPath">Old file path.</param>
    /// <param name="newPath">New file path.</param>
    private void RefreshFile(string oldPath, string newPath)
    {
        UnregisterFile(oldPath);
        RegisterFile(newPath);
    }

    /// <summary>
    /// Registers a script command.
    /// </summary>
    /// <param name="path">Script command file to register.</param>
    /// <param name="cmd">Command to register.</param>
    /// <returns><see langword="true" /> if registered without issues, <see langword="false" /> otherwise.</returns>
    private bool RegisterCommand(string path, ICommand cmd)
    {
        var dir = ProcessDirectoryPath(HelpersProvider.FileSystemHelper.GetDirectory(path));
        var hasParent = Directories.ContainsKey(dir);
        var registered = hasParent ? (CommandsUtils.RegisterCommand(Directories[dir], cmd) == true ? HandlerType : null) : CommandsUtils.RegisterCommand(HandlerType, cmd);

        if (registered != HandlerType)
        {
            FileScriptsLoader.PrintError($"Could not register command '{cmd.Command}' for {HandlerType}.");
            return false;
        }

        if (!hasParent)
        {
            Commands[cmd.Command] = cmd;
        }

        FileScriptsLoader.PrintLog($"Registered command '{cmd.Command}' for {HandlerType}.");
        return true;
    }

    /// <summary>
    /// Updates script command description info.
    /// </summary>
    /// <param name="path">Script description file to update.</param>
    /// <returns><see langword="true" /> if updated without issues, <see langword="false" /> otherwise.</returns>
    private bool UpdateScriptDescription(string path)
    {
        var dir = ProcessDirectoryPath(HelpersProvider.FileSystemHelper.GetDirectory(path));
        var hasParent = Directories.ContainsKey(dir);
        var name = HelpersProvider.FileSystemHelper.GetFileNameWithoutExtension(path);
        ICommand foundCommand;

        if (hasParent)
        {
            Directories[dir].TryGetCommand(name, out foundCommand);
        }
        else
        {
            Commands.TryGetValue(name, out foundCommand);
        }

        var cmd = foundCommand as FileScriptCommand;

        if (cmd is null)
        {
            FileScriptsLoader.PrintError($"Could not update description for command '{name}' in {HandlerType}.");
            return false;
        }

        CommandMetaData desc;

        try
        {
            desc = HelpersProvider.FileSystemHelper.ReadMetadataFromJson(path);
        }
        catch (Exception ex)
        {
            FileScriptsLoader.PrintError($"An error has occured during '{cmd.Command}' in {HandlerType} description deserialization: {ex.Message}");
            return false;
        }

        UpdateCommandDesc(cmd, desc);
        FileScriptsLoader.PrintLog($"Description update for '{cmd.Command}' command in {HandlerType} finished successfully.");
        return true;
    }

    /// <summary>
    /// Unregisters a script command.
    /// </summary>
    /// <param name="path">Script command file to unregister.</param>
    /// <returns>Unregistered command if no issues occured, <see langword="null" /> otherwise.</returns>
    private ICommand UnregisterCommand(string path)
    {
        var dir = ProcessDirectoryPath(HelpersProvider.FileSystemHelper.GetDirectory(path));
        var hasParent = Directories.ContainsKey(dir);
        var name = HelpersProvider.FileSystemHelper.GetFileNameWithoutExtension(path);
        ICommand cmd;

        if (hasParent)
        {
            Directories[dir].TryGetCommand(name, out cmd);
        }
        else
        {
            Commands.TryGetValue(name, out cmd);
        }

        var removed = hasParent ? (CommandsUtils.UnregisterCommand(Directories[dir], cmd) == true ? HandlerType : null) : CommandsUtils.UnregisterCommand(HandlerType, cmd);

        if (removed != HandlerType)
        {
            FileScriptsLoader.PrintError($"Could not unregister command '{cmd?.Command}' from {HandlerType}.");
            return null;
        }

        if (!hasParent)
        {
            Commands.Remove(cmd.Command);
        }

        FileScriptsLoader.PrintLog($"Unregistered command '{cmd.Command}' from {HandlerType}.");
        return cmd;
    }
}
