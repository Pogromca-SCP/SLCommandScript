using CommandSystem;
using System.Collections.Concurrent;
using SLCommandScript.Core.Language;
using SLCommandScript.Core.Interfaces;
using System;
using System.IO;
using System.Threading;

using PluginAPI.Core;

namespace SLCommandScript.Commands;

/// <summary>
/// Base class for script executing commands.
/// </summary>
public class ScriptCommandBase : ICommand
{
    /// <summary>
    /// Default command description to use.
    /// </summary>
    public const string DefaultDescription = "Custom script command.";

    private const string DebugPrefix = "Scripts cache: ";

    /// <summary>
    /// Contains currently loaded scripts.
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> _loadedScripts = new();

    /// <summary>
    /// Setups an interpretation process for the script.
    /// </summary>
    /// <param name="lexer">Lexer used for tokenization.</param>
    /// <returns>Error message if something goes wrong, <see langword="null" /> otherwise.</returns>
    private static string Interpret(Lexer lexer)
    {
        var parser = new Parser(lexer.Tokens);
        var resolver = new Resolver();
        var interpreter = new Interpreter(lexer.Sender);

        while (!lexer.IsAtEnd)
        {
            var result = lexer.ScanNextLine();

            if (!result)
            {
                return lexer.ErrorMessage;
            }

            parser.Reset();
            var expr = parser.Parse();

            if (parser.ErrorMessage is not null)
            {
                return parser.ErrorMessage;
            }

            if (expr is not null)
            {
                expr.Accept(resolver);
                result = expr.Accept(interpreter);

                if (!result)
                {
                    return interpreter.ErrorMessage;
                }
            }
        }

        return null;
    }

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
    /// Holds full path to script file.
    /// </summary>
    private readonly string _file;

    /// <summary>
    /// Contains permissions resolver type to use.
    /// </summary>
    private readonly IPermissionsResolver _resolver;

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
    /// <param name="file">Path to associated script.</param>
    /// <param name="resolver">Permissions resolver to use.</param>
    public ScriptCommandBase(string file, IPermissionsResolver resolver)
    {
        Command = Path.GetFileNameWithoutExtension(file);
        _file = file;
        _resolver = resolver;
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
        Interlocked.Increment(ref _calls);
        var line = 0;

        using (var lexer = new Lexer(LoadSource(), arguments, sender, _resolver))
        {
            response = Interpret(lexer);
            line = lexer.Line;
        }

        if (Interlocked.Decrement(ref _calls) < 1)
        {
            var message = _loadedScripts.TryRemove(_file, out _) ? "Unloaded" : "Failed to unload";
            Log.Debug($"{message} script - {Command}.slc", DebugPrefix);
        }

        var result = response is null;
        response = result ? "Script executed successfully." : $"{response}\nat {Command}.slc:{line}";
        return result;
    }

    /// <summary>
    /// Loads script source code.
    /// </summary>
    /// <returns>Loaded source code string.</returns>
    private string LoadSource()
    {
        if (_loadedScripts.ContainsKey(_file))
        {
            return _loadedScripts[_file];
        }

        var src = File.ReadAllText(_file);
        Log.Debug($"Loaded script - {Command}.slc", DebugPrefix);
        _loadedScripts[_file] = src;
        return src;
    }
}
