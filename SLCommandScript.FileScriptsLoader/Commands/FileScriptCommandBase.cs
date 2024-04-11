using CommandSystem;
using SLCommandScript.Core;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.FileScriptsLoader.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Base class for script executing commands.
/// </summary>
public class FileScriptCommandBase : ICommand
{
    /// <summary>
    /// Default command description to use.
    /// </summary>
    public const string DefaultDescription = "Custom script command.";

    /// <summary>
    /// Contains permissions resolver object to use.
    /// </summary>
    public static IPermissionsResolver PermissionsResolver { get; set; } = null;

    /// <summary>
    /// Contains a maximum amount of concurrent executions a single script can have.
    /// </summary>
    public static int ConcurrentExecutionsLimit { get; set; } = 0;

    /// <summary>
    /// Contains currently loaded scripts.
    /// </summary>
    private static readonly ConcurrentDictionary<FileScriptCommandBase, string> _loadedScripts = new();

    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Defines command aliases.
    /// </summary>
    public string[] Aliases => null;

    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get => _desc; set => _desc = string.IsNullOrWhiteSpace(value) ? DefaultDescription : value; }

    /// <summary>
    /// Root location where the shortened file path starts from.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Contains shortened file path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Contains command description.
    /// </summary>
    private string _desc;

    /// <summary>
    /// Contains script calls counter.
    /// </summary>
    private int _calls;

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="location">Root location where the used path starts from.</param>
    /// <param name="path">Path to use.</param>
    public FileScriptCommandBase(string location, string path)
    {
        Location = location;
        Path = path;
        var index = path?.LastIndexOf('/') ?? -1;
        Command = HelpersProvider.FileSystemHelper.GetFileNameWithoutExtension(index < 0 ? path : path.Substring(index + 1));
        _desc = DefaultDescription;
        _calls = 0;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    public virtual bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (Interlocked.Increment(ref _calls) > ConcurrentExecutionsLimit)
        {
            Interlocked.Decrement(ref _calls);
            response = "Script execution terminated due to exceeded concurrent executions limit";
            return false;
        }

        var src = LoadSource();

        if (src is null)
        {
            Interlocked.Decrement(ref _calls);
            response = $"Cannot read script from file '{Path}'";
            return false;
        }

        (response, var line) = ScriptUtils.Execute(src, arguments, sender, PermissionsResolver);

        if (Interlocked.Decrement(ref _calls) < 1)
        {
            _loadedScripts.TryRemove(this, out _);
        }

        var result = response is null;
        response = result ? "Script executed successfully." : $"{response}\nat {Command}.slcs:{line}";
        return result;
    }

    /// <summary>
    /// Loads script source code.
    /// </summary>
    /// <returns>Loaded source code string.</returns>
    private string LoadSource()
    {
        if (!_loadedScripts.TryGetValue(this, out var src))
        {
            try
            {
                src = HelpersProvider.FileSystemHelper.ReadFile($"{Location}{Path}");
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
