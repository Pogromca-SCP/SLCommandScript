﻿using CommandSystem;
using SLCommandScript.Core.Interfaces;
using System.Collections.Concurrent;
using SLCommandScript.Core.Language;
using System;
using System.IO;
using System.Threading;

using PluginAPI.Core;

namespace SLCommandScript.Commands;

/// <summary>
/// Base class for script executing commands.
/// </summary>
public class FileScriptCommandBase : ICommand
{
    /// <summary>
    /// Default command description to use.
    /// </summary>
    public const string DefaultDescription = "Custom script command.";

    private const string DebugPrefix = "Scripts cache: ";

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
    private static readonly ConcurrentDictionary<string, string> _loadedScripts = new();

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
    public FileScriptCommandBase(string file)
    {
        Command = Path.GetFileNameWithoutExtension(file);
        _file = file;
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

        var lexer = Lexer.Rent(LoadSource(), arguments, sender, PermissionsResolver);
        response = Interpret(lexer);
        var line = lexer.Line;
        Lexer.Return(lexer);

        if (Interlocked.Decrement(ref _calls) < 1)
        {
            var message = _loadedScripts.TryRemove(_file, out _) ? "Unloaded" : "Failed to unload";
            Log.Debug($"{message} script - {Command}.slcs", DebugPrefix);
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
        if (_loadedScripts.ContainsKey(_file))
        {
            return _loadedScripts[_file];
        }

        var src = File.ReadAllText(_file);
        Log.Debug($"Loaded script - {Command}.slcs", DebugPrefix);
        _loadedScripts[_file] = src;
        return src;
    }
}
