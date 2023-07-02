using CommandSystem;
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
    /// Contains currently loaded scripts.
    /// </summary>
    private static readonly Dictionary<string, string> _loadedScripts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Contains currently used interpreter.
    /// </summary>
    private static Interpreter _interpreter = null;

    /// <summary>
    /// Tells how many scripts are currently nested.
    /// </summary>
    private static int _nestLevel = 0;

    /// <summary>
    /// Contains command name.
    /// </summary>
    public string Command { get; private set; }

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
        if (!File.Exists(file))
        {
            Command = null;
            _file = null;
            return;
        }

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
        if (_file is null)
        {
            response = "Script file does not exist.";
            return false;
        }

        if (_nestLevel == 0)
        {
            _interpreter = new(sender);
        }

        ++_nestLevel;
        string src;

        if (_loadedScripts.ContainsKey(Command))
        {
            src = _loadedScripts[Command];
        }
        else
        {
            src = File.ReadAllText(_file);
            _loadedScripts[Command] = src;
        }

        response = Interpret(new Lexer(src, arguments, sender));
        --_nestLevel;

        if (_nestLevel == 0)
        {
            _interpreter = null;
            _loadedScripts.Clear();
        }

        var result = response is null;
        response ??= "Script executed successfully.";
        return result;
    }

    /// <summary>
    /// Setups an interpretation process for the script.
    /// </summary>
    /// <param name="lexer">Lexer used for tokenization.</param>
    /// <returns>Error message if something goes wrong, <see langword="null" /> otherwise.</returns>
    private string Interpret(Lexer lexer)
    {
        var tokens = lexer.ScanNextLine();

        if (lexer.ErrorMessage is not null)
        {
            return lexer.ErrorMessage;
        }

        var parser = new Parser(tokens);
        var expr = parser.Parse();

        if (parser.ErrorMessage is not null)
        {
            return parser.ErrorMessage;
        }

        var result = expr.Accept(_interpreter);

        if (!result)
        {
            return _interpreter.ErrorMessage;
        }

        while (!lexer.IsAtEnd)
        {
            var message = ParseLoop(lexer, parser);

            if (message is not null)
            {
                return message;
            }
        }

        return null;
    }

    /// <summary>
    /// Performs a single parse loop step.
    /// </summary>
    /// <param name="lexer">Lexer used for tokenization.</param>
    /// <param name="parser">Parser used for parsing.</param>
    /// <returns>Error message if something goes wrong, <see langword="null" /> otherwise.</returns>
    private string ParseLoop(Lexer lexer, Parser parser)
    {
        lexer.ScanNextLine();

        if (lexer.ErrorMessage is not null)
        {
            return lexer.ErrorMessage;
        }

        var expr = parser.Parse();

        if (parser.ErrorMessage is not null)
        {
            return parser.ErrorMessage;
        }

        var result = expr.Accept(_interpreter);
        return result ? null : _interpreter.ErrorMessage;
    }
}
