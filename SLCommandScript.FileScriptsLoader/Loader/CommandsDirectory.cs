using CommandSystem;
using PluginAPI.Enums;
using SLCommandScript.Core.Commands;
using SLCommandScript.FileScriptsLoader.Commands;
using SLCommandScript.FileScriptsLoader.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.Loader;

/// <summary>
/// Monitors a directory and related scripts.
/// </summary>
public class CommandsDirectory : IDisposable, IFileScriptCommandParent
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
        cmd.RequiredPermissions = data.RequiredPerms;
    }

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
    /// Contains commands configuration to apply.
    /// </summary>
    public RuntimeConfig Config { get; }

    /// <summary>
    /// Creates new directory monitor and initializes the watcher.
    /// </summary>
    /// <param name="watcher">File system watcher to use.</param>
    /// <param name="handlerType">Type of handler to use.</param>
    /// <param name="config">Runtime configuration to use by event scripts.</param>
    public CommandsDirectory(IFileSystemWatcherHelper watcher, CommandType handlerType, RuntimeConfig config)
    {
        Commands = new(StringComparer.OrdinalIgnoreCase);
        HandlerType = handlerType;
        Watcher = watcher;
        Config = config ?? new(null, null, 10);

        if (Watcher is null)
        {
            return;
        }

        LoadInitialFiles();
        Watcher.Created += (obj, args) => RegisterFile(args.FullPath);
        Watcher.Changed += (obj, args) => RefreshDescription(args.FullPath);
        Watcher.Deleted += (obj, args) => UnregisterFile(args.FullPath);
        Watcher.Renamed += (obj, args) => RefreshFile(args.OldFullPath, args.FullPath);
        Watcher.Error += (obj, args) => FileScriptsLoader.PrintError($"A {HandlerType} commands watcher error has occured: {args.GetException().Message}");
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
    /// Disposes the watcher and performs command cleanup.
    /// </summary>
    /// <param name="disposing">Whether or not this method is invoked from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Watcher?.Dispose();

        foreach (var command in Commands.Values)
        {
            CommandsUtils.UnregisterCommand(HandlerType, command);
        }
    }

    /// <summary>
    /// Loads initial files and directories.
    /// </summary>
    private void LoadInitialFiles()
    {
        foreach (var path in Config.FileSystemHelper.EnumerateDirectories(Watcher.Directory))
        {
            RegisterCommand(new FileScriptDirectoryCommand(Config.FileSystemHelper.GetDirectory(path), GetCommand<IFileScriptCommandParent>(path) ?? this));
        }

        foreach (var path in Config.FileSystemHelper.EnumerateFiles(Watcher.Directory, EventsDirectory.ScriptFilesFilter, SearchOption.AllDirectories))
        {
            RegisterCommand(new FileScriptCommand(Config.FileSystemHelper.GetFileNameWithoutExtension(path), GetCommand<IFileScriptCommandParent>(path) ?? this, Config));
        }

        foreach (var path in Config.FileSystemHelper.EnumerateFiles(Watcher.Directory, DescriptionFilesFilter, SearchOption.AllDirectories))
        {
            UpdateScriptDescription(path);
        }
    }

    /// <summary>
    /// Retrieves command as a specific type.
    /// </summary>
    /// <typeparam name="T">Type of returned value.</typeparam>
    /// <param name="path">Path to get command from.</param>
    /// <returns>Reference to command or <see langword="null" /> if nothing was found.</returns>
    private T GetCommand<T>(string path) where T : class
    {
        var processedPath = path.Substring(Watcher.Directory.Length + 1);

        if (processedPath.Length < 1)
        {
            return null;
        }

        var names = processedPath.Split(Path.DirectorySeparatorChar);
        var index = 0;
        var found = Commands.TryGetValue(names[index++], out var foundCommand);

        while (found && index < names.Length)
        {
            if (foundCommand is not ICommandHandler commandHandler)
            {
                return foundCommand as T;
            }

            found = commandHandler.TryGetCommand(names[index++], out var tmp);

            if (found)
            {
                foundCommand = tmp;
            }
        }

        return foundCommand as T;
    }

    /// <summary>
    /// Registers a file.
    /// </summary>
    /// <param name="path">File path to register.</param>
    private void RegisterFile(string path)
    {
        if (Config.FileSystemHelper.DirectoryExists(path))
        {
            RegisterCommand(new FileScriptDirectoryCommand(Config.FileSystemHelper.GetDirectory(path), GetCommand<IFileScriptCommandParent>(path) ?? this));
            return;
        }

        var ext = Config.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            RegisterCommand(new FileScriptCommand(Config.FileSystemHelper.GetFileNameWithoutExtension(path), GetCommand<IFileScriptCommandParent>(path) ?? this, Config));
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
        var ext = Config.FileSystemHelper.GetFileExtension(path);

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
        if (Config.FileSystemHelper.DirectoryExists(path))
        {
            UnregisterCommand(path);
            return;
        }

        var ext = Config.FileSystemHelper.GetFileExtension(path);

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
    /// <param name="cmd">Command to register.</param>
    /// <returns><see langword="true" /> if registered without issues, <see langword="false" /> otherwise.</returns>
    private bool RegisterCommand(FileScriptDirectoryCommand cmd)
    {
        var registered = cmd.Parent switch
        {
            FileScriptDirectoryCommand parent => CommandsUtils.RegisterCommand(parent, cmd) == true ? HandlerType : null,
            CommandsDirectory parent => CommandsUtils.RegisterCommand(parent.HandlerType, cmd),
            _ => null
        };

        if (registered != HandlerType)
        {
            FileScriptsLoader.PrintError($"Could not register command '{cmd.Command}' for {HandlerType}.");
            return false;
        }

        if (this == cmd.Parent)
        {
            Commands[cmd.Command] = cmd;
        }

        FileScriptsLoader.PrintLog($"Registered command '{cmd.Command}' for {HandlerType}.");
        return true;
    }

    /// <summary>
    /// Registers a script command.
    /// </summary>
    /// <param name="cmd">Command to register.</param>
    /// <returns><see langword="true" /> if registered without issues, <see langword="false" /> otherwise.</returns>
    private bool RegisterCommand(FileScriptCommandBase cmd)
    {
        var registered = cmd.Parent switch
        {
            FileScriptDirectoryCommand parent => CommandsUtils.RegisterCommand(parent, cmd) == true ? HandlerType : null,
            CommandsDirectory parent => CommandsUtils.RegisterCommand(parent.HandlerType, cmd),
            _ => null
        };

        if (registered != HandlerType)
        {
            FileScriptsLoader.PrintError($"Could not register command '{cmd.Command}' for {HandlerType}.");
            return false;
        }

        if (this == cmd.Parent)
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
        var cmd = GetCommand<FileScriptCommand>(path);

        if (cmd is null)
        {
            FileScriptsLoader.PrintError($"Could not update description for undescriptable command in {HandlerType}.");
            return false;
        }

        CommandMetaData desc;

        try
        {
            desc = Config.FileSystemHelper.ReadMetadataFromJson(path);
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
        var cmd = GetCommand<ICommand>(path);

        return cmd switch
        {
            FileScriptDirectoryCommand dir => UnregisterCommand(dir),
            FileScriptCommandBase script => UnregisterCommand(script),
            _ => null,
        };
    }

    /// <summary>
    /// Unregisters a script command.
    /// </summary>
    /// <param name="cmd">Command to unregister.</param>
    /// <returns>Unregistered command if no issues occured, <see langword="null" /> otherwise.</returns>
    private FileScriptCommandBase UnregisterCommand(FileScriptCommandBase cmd)
    {
        var removed = cmd.Parent switch
        {
            FileScriptDirectoryCommand parent => CommandsUtils.UnregisterCommand(parent, cmd) == true ? HandlerType : null,
            CommandsDirectory parent => CommandsUtils.UnregisterCommand(parent.HandlerType, cmd),
            _ => null
        };

        if (removed != HandlerType)
        {
            FileScriptsLoader.PrintError($"Could not unregister command '{cmd.Command}' from {HandlerType}.");
            return null;
        }

        if (this == cmd.Parent)
        {
            Commands.Remove(cmd.Command);
        }

        FileScriptsLoader.PrintLog($"Unregistered command '{cmd.Command}' from {HandlerType}.");
        return cmd;
    }

    /// <summary>
    /// Unregisters a script command.
    /// </summary>
    /// <param name="cmd">Command to unregister.</param>
    /// <returns>Unregistered command if no issues occured, <see langword="null" /> otherwise.</returns>
    private FileScriptDirectoryCommand UnregisterCommand(FileScriptDirectoryCommand cmd)
    {
        var removed = cmd.Parent switch
        {
            FileScriptDirectoryCommand parent => CommandsUtils.UnregisterCommand(parent, cmd) == true ? HandlerType : null,
            CommandsDirectory parent => CommandsUtils.UnregisterCommand(parent.HandlerType, cmd),
            _ => null
        };

        if (removed != HandlerType)
        {
            FileScriptsLoader.PrintError($"Could not unregister command '{cmd.Command}' from {HandlerType}.");
            return null;
        }

        if (this == cmd.Parent)
        {
            Commands.Remove(cmd.Command);
        }

        FileScriptsLoader.PrintLog($"Unregistered command '{cmd.Command}' from {HandlerType}.");
        return cmd;
    }
}
