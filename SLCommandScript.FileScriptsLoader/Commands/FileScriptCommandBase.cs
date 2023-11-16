using CommandSystem;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Language;
using SLCommandScript.FileScriptsLoader.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Base class for script executing commands.
/// </summary>
/// <param name="file">Path to associated script.</param>
public class FileScriptCommandBase(string file) : ICommand
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
    /// Setups an interpretation process for the script.
    /// </summary>
    /// <param name="lexer">Lexer used for tokenization.</param>
    /// <returns>Error message if something goes wrong, <see langword="null" /> otherwise.</returns>
    private static string Interpret(Lexer lexer)
    {
        var parser = new Parser();
        var interpreter = new Interpreter(lexer.Sender);

        while (!lexer.IsAtEnd)
        {
            var tokens = lexer.ScanNextLine();

            if (lexer.ErrorMessage is not null)
            {
                return lexer.ErrorMessage;
            }

            var expr = parser.Parse(tokens);

            if (parser.ErrorMessage is not null)
            {
                return parser.ErrorMessage;
            }

            if (expr is not null)
            {
                var result = expr.Accept(interpreter);

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
    public string Command { get; } = HelpersProvider.FileSystemHelper.GetFileNameWithoutExtension(file);

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
    private readonly string _file = file;

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
            response = $"Cannot read script from file '{Command}.slcs'";
            return false;
        }

        var lexer = Lexer.Rent(src, arguments, sender, PermissionsResolver);
        response = Interpret(lexer);
        var line = lexer.Line;
        Lexer.Return(lexer);

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
                src = HelpersProvider.FileSystemHelper.ReadFile(_file);
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
