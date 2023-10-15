using System;
using System.Collections.Generic;
using SLCommandScript.FileScriptsLoader.Commands;
using PluginAPI.Enums;
using SLCommandScript.FileScriptsLoader.Helpers;
using System.IO;
using SLCommandScript.Core.Commands;

namespace SLCommandScript.FileScriptsLoader.Loader;

/// <summary>
/// Monitors a directory and related scripts.
/// </summary>
public class CommandsDirectory : IDisposable
{
    /// <summary>
    /// Contains script files extension.
    /// </summary>
    public const string ScriptFileExtension = "slcs";

    /// <summary>
    /// Contains description files extension.
    /// </summary>
    public const string ScriptDescriptionExtension = "json";

    /// <summary>
    /// Contains all registered scripts commands from monitored directory.
    /// </summary>
    public Dictionary<string, FileScriptCommand> Commands { get; }

    /// <summary>
    /// Contains handler type used for root commands.
    /// </summary>
    public CommandType HandlerType { get; }

    /// <summary>
    /// File system watcher used to detect script files changes.
    /// </summary>
    public IFileSystemWatcher Watcher { get; }

    /// <summary>
    /// Creates new directory monitor and initializes the watcher.
    /// </summary>
    /// <param name="watcher">File system watcher to use.</param>
    /// <param name="handlerType">Type of handler to use.</param>
    public CommandsDirectory(IFileSystemWatcher watcher, CommandType handlerType)
    {
        Commands = new(StringComparer.OrdinalIgnoreCase);
        HandlerType = handlerType;
        Watcher = watcher;

        if (Watcher is null)
        {
            return;
        }

        foreach (var file in FileScriptCommandBase.FileSystemHelper.EnumeratePath(Watcher.Directory, SearchOption.AllDirectories))
        {
            RegisterFile(file);
        }

        Watcher.Created += (obj, args) => RegisterFile(args.FullPath);
        Watcher.Changed += (obj, args) => RefreshDescription(args.FullPath);
        Watcher.Deleted += (obj, args) => UnregisterFile(args.FullPath);
        Watcher.Renamed += (obj, args) => RefreshFile(args.OldFullPath, args.FullPath);
    }

    /// <summary>
    /// Disposes the watcher and performs command cleanup.
    /// </summary>
    public void Dispose()
    {
        Watcher?.Dispose();

        foreach (var command in Commands.Values)
        {
            CommandsUtils.UnregisterCommand(HandlerType, command);
        }
    }

    #region Create
    /// <summary>
    /// Registers a file.
    /// </summary>
    /// <param name="path">File path to register.</param>
    private void RegisterFile(string path)
    {
        var ext = FileScriptCommandBase.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            RegisterScript(path);
            return;
        }

        if (ext.Equals(ScriptDescriptionExtension, StringComparison.OrdinalIgnoreCase))
        {
            UpdateScriptDescription(path);
        }
    }

    /// <summary>
    /// Registers a script.
    /// </summary>
    /// <param name="scriptFile">Script command file to register.</param>
    private void RegisterScript(string scriptFile)
    {
        var cmd = new FileScriptCommand(scriptFile);
        var registered = CommandsUtils.RegisterCommand(HandlerType, cmd);

        if (registered != null)
        {
            Commands[cmd.Command] = cmd;
            FileScriptsLoader.PrintLog($"Registered command '{cmd.Command}' for {HandlerType}.");
        }
        else
        {
            FileScriptsLoader.PrintError($"Could not register command '{cmd.Command}' for {HandlerType}.");
        }
    }

    /// <summary>
    /// Updates script command description info.
    /// </summary>
    /// <param name="descFile">Description file name to use.</param>
    private void UpdateScriptDescription(string descFile)
    {
        var name = FileScriptCommandBase.FileSystemHelper.GetFileNameWithoutExtension(descFile);

        if (!Commands.ContainsKey(name))
        {
            FileScriptsLoader.PrintError($"Could not update description for command '{name}' in {HandlerType}.");
            return;
        }

        var cmd = Commands[name];
        CommandMetaData desc;

        try
        {
            desc = FileScriptCommandBase.FileSystemHelper.ReadMetadataFromJson(descFile);
        }
        catch (Exception ex)
        {
            FileScriptsLoader.PrintError($"An error has occured during '{cmd.Command}' in {HandlerType} description deserialization: {ex.Message}");
            return;
        }

        cmd.Description = desc.Description;
        cmd.Usage = desc.Usage;
        cmd.Arity = desc.Arity;
        cmd.Help = desc.Help;
        FileScriptsLoader.PrintLog($"Description update for '{cmd.Command}' command in {HandlerType} finished successfully.");
    }
    #endregion

    #region Update
    /// <summary>
    /// Refreshes commands descriptions.
    /// </summary>
    /// <param name="path">File path to update.</param>
    private void RefreshDescription(string path)
    {
        var ext = FileScriptCommandBase.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptDescriptionExtension, StringComparison.OrdinalIgnoreCase))
        {
            UpdateScriptDescription(path);
        }
    }
    #endregion

    #region Delete
    /// <summary>
    /// Unregisters a file.
    /// </summary>
    /// <param name="path">File path to unregister.</param>
    private void UnregisterFile(string path)
    {
        var ext = FileScriptCommandBase.FileSystemHelper.GetFileExtension(path);

        if (ext.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            UnregisterScript(path);
        }
    }

    /// <summary>
    /// Unregisters a script.
    /// </summary>
    /// <param name="scriptFile">Script file to unregister.</param>
    private void UnregisterScript(string scriptFile)
    {
        var name = FileScriptCommandBase.FileSystemHelper.GetFileNameWithoutExtension(scriptFile);
        var cmd = Commands[name];
        var removed = CommandsUtils.UnregisterCommand(HandlerType, cmd);

        if (removed != null)
        {
            Commands.Remove(cmd.Command);
            FileScriptsLoader.PrintLog($"Unregistered command '{cmd.Command}' from {HandlerType}.");
        }
        else
        {
            FileScriptsLoader.PrintError($"Could not unregister command '{cmd.Command}' from {HandlerType}.");
        }
    }
    #endregion

    #region Rename
    /// <summary>
    /// Refreshes a file.
    /// </summary>
    /// <param name="oldPath">Old file path.</param>
    /// <param name="newPath">New file path.</param>
    private void RefreshFile(string oldPath, string newPath)
    {
        var oldExt = FileScriptCommandBase.FileSystemHelper.GetFileExtension(oldPath);

        if (oldExt.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            UnregisterScript(oldPath);
        }

        var newExt = FileScriptCommandBase.FileSystemHelper.GetFileExtension(newPath);

        if (newExt.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            RegisterScript(newPath);
            return;
        }

        if (newExt.Equals(ScriptDescriptionExtension, StringComparison.OrdinalIgnoreCase))
        {
            UpdateScriptDescription(newPath);
        }
    }
    #endregion
}
