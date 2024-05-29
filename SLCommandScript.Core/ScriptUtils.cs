using CommandSystem;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Language;
using System;

namespace SLCommandScript.Core;

/// <summary>
/// Provides additional utilities for scripts.
/// </summary>
public static class ScriptUtils
{
    /// <summary>
    /// Executes a custom commands script.
    /// </summary>
    /// <param name="source">Script to execute.</param>
    /// <param name="arguments">Script arguments to use.</param>
    /// <param name="sender">Script sender.</param>
    /// <param name="permissionsResolver">Optional custom permissions resolver to use.</param>
    /// <returns>Error message if something goes wrong, <see langword="null" /> otherwise. Line number provided alongside.</returns>
    public static (string Message, int Line) Execute(string source, ArraySegment<string> arguments, ICommandSender sender, IPermissionsResolver permissionsResolver = null)
    {
        var lexer = Lexer.Rent(source, arguments, sender, permissionsResolver);
        var parser = new Parser();
        var interpreter = new Interpreter(sender);

        while (!lexer.IsAtEnd)
        {
            var tokens = lexer.ScanNextLine();

            if (lexer.ErrorMessage is not null)
            {
                return (lexer.ErrorMessage, ReturnLexer(lexer));
            }

            var expr = parser.Parse(tokens);

            if (parser.ErrorMessage is not null)
            {
                return (parser.ErrorMessage, ReturnLexer(lexer));
            }

            if (expr is not null)
            {
                var result = expr.Accept(interpreter);

                if (!result)
                {
                    return (interpreter.ErrorMessage, ReturnLexer(lexer));
                }
            }
        }

        return (null, ReturnLexer(lexer));
    }

    /// <summary>
    /// Returns a lexer to the pool and loads it's current line number.
    /// </summary>
    /// <param name="lexer">Lexer to return.</param>
    /// <returns>Line number from returned lexer.</returns>
    private static int ReturnLexer(Lexer lexer)
    {
        var line = lexer.Line;
        Lexer.Return(lexer);
        return line;
    }
}
