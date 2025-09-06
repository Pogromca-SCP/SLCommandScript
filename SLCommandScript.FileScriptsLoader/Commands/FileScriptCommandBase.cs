using CommandSystem;
using SLCommandScript.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Base class for script executing commands.
/// </summary>
/// <param name="name">Name of the command.</param>
/// <param name="parent">Parent which stores this command.</param>
/// <param name="config">Configuration to use.</param>
public class FileScriptCommandBase(string? name, IFileScriptCommandParent? parent, RuntimeConfig? config) : ICommand
{
    /// <summary>
    /// Default command description to use.
    /// </summary>
    public const string DefaultDescription = "Custom script command.";

    /// <summary>
    /// Contains currently loaded scripts.
    /// </summary>
    private static readonly ConcurrentDictionary<FileScriptCommandBase, string> _loadedScripts = new();

    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; } = name ?? string.Empty;

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[]? Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get => _desc; set => _desc = string.IsNullOrWhiteSpace(value) ? DefaultDescription : value; }

    /// <summary>
    /// Contains parent object which stores this command.
    /// </summary>
    public IFileScriptCommandParent? Parent { get; } = parent;

    /// <summary>
    /// Stores used configuration.
    /// </summary>
    public RuntimeConfig Config { get; } = config ?? new(null, null, 10);

    /// <summary>
    /// Contains command description.
    /// </summary>
    private string _desc = DefaultDescription;

    /// <summary>
    /// Contains script calls counter.
    /// </summary>
    private int _calls = 0;

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    public virtual bool Execute(ArraySegment<string?> arguments, ICommandSender? sender, out string response)
    {
        if (Interlocked.Increment(ref _calls) > Config.ScriptExecutionsLimit)
        {
            Interlocked.Decrement(ref _calls);
            response = "Script execution terminated due to exceeded concurrent executions limit";
            return false;
        }

        var file = $"{Command}.slcs";
        var path = Parent is null ? file : $"{Parent.GetLocation(true)}{file}";
        var src = LoadSource(path);

        if (src is null)
        {
            Interlocked.Decrement(ref _calls);
            response = $"Cannot read script from file '{path}'";
            return false;
        }

        (var error, var line) = ScriptUtils.Execute(src, arguments, sender, Config.PermissionsResolver);

        if (Interlocked.Decrement(ref _calls) < 1)
        {
            _loadedScripts.TryRemove(this, out _);
        }

        var result = error is null;
        response = result ? "Script executed successfully." : $"{error}\nat {Command}.slcs:{line}";
        return result;
    }

    /// <summary>
    /// Loads script source code.
    /// </summary>
    /// <param name="path">Path to read script from.</param>
    /// <returns>Loaded source code string.</returns>
    private string? LoadSource(string path)
    {
        if (!_loadedScripts.TryGetValue(this, out var src))
        {
            try
            {
                src = Config.FileSystemHelper.ReadFile(path);
            }
            catch (Exception)
            {
                return null;
            }

            _loadedScripts.TryAdd(this, src);
        }

        return src;
    }
}
