using CommandSystem;
using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;
using SLCommandScript.Core.Language;
using System;
using System.IO;

namespace SLCommandScript.Commands;

/// <summary>
/// Command used to launch interpreted scripts.
/// </summary>
public class FileScriptCommand : ICommand
{
    /// <summary>
    /// Contains permissions resolver type to use.
    /// </summary>
    public static IPermissionsResolver PermissionsResolver { get; set; } = null;

    /// <summary>
    /// Contains currently loaded scripts.
    /// </summary>
    private static readonly Dictionary<string, string> _loadedScripts = new();

    /// <summary>
    /// Contains scripts stack.
    /// </summary>
    private static readonly Stack<string> _scriptsStack = new();

    /// <summary>
    /// Contains currently used interpreter.
    /// </summary>
    private static Interpreter _interpreter = null;

    /// <summary>
    /// Setups an interpretation process for the script.
    /// </summary>
    /// <param name="lexer">Lexer used for tokenization.</param>
    /// <returns>Error message if something goes wrong, <see langword="null" /> otherwise.</returns>
    private static string Interpret(Lexer lexer)
    {
        var parser = new Parser(lexer.Tokens);

        while (!lexer.IsAtEnd)
        {
            lexer.ScanNextLine();

            if (lexer.ErrorMessage is not null)
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
                var result = expr.Accept(_interpreter);

                if (!result)
                {
                    return _interpreter.ErrorMessage;
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
    public string Description => $"Executes custom script from {Command}.slc file.";

    /// <summary>
    /// Holds full path to script file.
    /// </summary>
    private readonly string _file;

    /// <summary>
    /// Initializes the command.
    /// </summary>
    /// <param name="file">Path to associated script.</param>
    public FileScriptCommand(string file)
    {
        Command = Path.GetFileNameWithoutExtension(file);
        _file = file;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="arguments">Command arguments provided by sender.</param>
    /// <param name="sender">Command sender.</param>
    /// <param name="response">Response to display in sender's console.</param>
    /// <returns><see langword="true" /> if command executed successfully, <see langword="false" /> otherwise.</returns>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        _interpreter ??= new(sender);
        _scriptsStack.Push(_file);
        var line = 0;

        using (var lexer = new Lexer(LoadSource(), arguments, sender, PermissionsResolver))
        {
            response = Interpret(lexer);
            line = lexer.Line;
        }

        _scriptsStack.Pop();

        if (!_scriptsStack.Contains(_file))
        {
            _loadedScripts.Remove(_file);
        }

        if (_scriptsStack.Count < 1)
        {
            _interpreter = null;
        }

        var result = response is null;
        response = result ? "Script executed successfully." : $"{response}\nat {Command}:{line}";
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
        _loadedScripts[_file] = src;
        return src;
    }
}
