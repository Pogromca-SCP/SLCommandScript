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
        var lexer = new Lexer(source, arguments, sender, permissionsResolver);
        var parser = new Parser();
        var interpreter = new Interpreter(sender);

        while (!lexer.IsAtEnd)
        {
            var tokens = lexer.ScanNextLine();

            if (lexer.ErrorMessage is not null)
            {
                return (lexer.ErrorMessage, lexer.Line);
            }

            var expr = parser.Parse(tokens);

            if (parser.ErrorMessage is not null)
            {
                return (parser.ErrorMessage, lexer.Line);
            }

            if (expr is not null)
            {
                var result = expr.Accept(interpreter);

                if (!result)
                {
                    return (interpreter.ErrorMessage, lexer.Line);
                }
            }
        }

        return (null, lexer.Line);
    }
}
