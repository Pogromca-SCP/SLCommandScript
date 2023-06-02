using System.Collections.Generic;
using System;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Language.Iterables;
using PluginAPI.Enums;
using SLCommandScript.Core.Language.Expressions;
using SLCommandScript.Core.Commands;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Parses provided tokens into expressions.
/// </summary>
public class Parser
{
    /// <summary>
    /// Contains iterable objects providers used in foreach directives.
    /// </summary>
    public static Dictionary<string, Func<IIterable>> Iterables { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "player", PlayerIterablesProvider.AllPlayers },
        { "classd", PlayerIterablesProvider.AllClassDs },
        { "scientist", PlayerIterablesProvider.AllScientists },
        { "mtf", PlayerIterablesProvider.AllMTFs },
        { "chaos", PlayerIterablesProvider.AllChaos },
        { "scp", PlayerIterablesProvider.AllSCPs },
        { "human", PlayerIterablesProvider.AllHumans }
    };

    #region Fields and Properties
    /// <summary>
    /// Contains current error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// <see langword="true" /> if tokens end was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => _current >= _tokens.Count;

    /// <summary>
    /// Current token.
    /// </summary>
    private Token Peek => _tokens[_current];

    /// <summary>
    /// Previous token.
    /// </summary>
    private Token Previous => _tokens[_current - 1];

    /// <summary>
    /// Contains a list with tokens to process.
    /// </summary>
    private readonly IList<Token> _tokens;

    /// <summary>
    /// Contains current token index.
    /// </summary>
    private int _current;

    /// <summary>
    /// Contains current commands scope.
    /// </summary>
    private CommandType _scope;
    #endregion

    #region State Management
    /// <summary>
    /// Creates new parser instance.
    /// </summary>
    /// <param name="tokens">List with tokens to process.</param>
    public Parser(IList<Token> tokens)
    {
        _tokens = tokens ?? new List<Token>();
        _scope = CommandType.RemoteAdmin | CommandType.Console | CommandType.GameConsole;
        Reset();
    }

    /// <summary>
    /// Parses an expression from provided tokens list.
    /// </summary>
    /// <returns>Parsed expression or <see langword="null" /> if something went wrong.</returns>
    public Expr Parse()
    {
        var expr = ParseExpr(false);

        if (ErrorMessage is not null)
        {
            return null;
        }

        ParseGuard();

        if (ErrorMessage is not null)
        {
            return null;
        }

        if (!IsAtEnd)
        {
            ErrorMessage = "[Parser] Unexpected token";
            return null;
        }
  
        return expr;
    }

    /// <summary>
    /// Resets the parsing process.
    /// </summary>
    public void Reset()
    {
        _current = 0;
        ErrorMessage = null;
    }

    /// <summary>
    /// Attempts to match specific token types.
    /// </summary>
    /// <param name="types">Token types to match.</param>
    /// <returns><see langword="true" /> if one of token types was matched, <see langword="false" /> otherwise.</returns>
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if current token is of specific type.
    /// </summary>
    /// <param name="type">Expected token type.</param>
    /// <returns><see langword="true" /> if current token exists and has correct type, <see langword="false" /> otherwise.</returns>
    private bool Check(TokenType type) => !IsAtEnd && Peek.Type == type;

    /// <summary>
    /// Checks if current token is not of specific type.
    /// </summary>
    /// <param name="type">Unwanted token type.</param>
    /// <returns><see langword="true" /> if current token exists and has correct type, <see langword="false" /> otherwise.</returns>
    private bool CheckNot(TokenType type) => !IsAtEnd && Peek.Type != type;

    /// <summary>
    /// Retrieves current token and moves index forward.
    /// </summary>
    /// <returns>Retrieved token.</returns>
    private Token Advance()
    {
        if (!IsAtEnd)
        {
            ++_current;
        }

        return Previous;
    }
    #endregion

    #region Expressions Parsing
    /// <summary>
    /// Parses a single expression.
    /// </summary>
    /// <param name="isInner">Whether or not this expression is inside another expression.</param>
    /// <returns>Parsed expression or <see langword="null" /> if nothing could be parsed.</returns>
    private Expr ParseExpr(bool isInner)
    {
        if (Match(TokenType.LeftSquare))
        {
            return Directive();
        }

        if (CheckNot(TokenType.ScopeGuard))
        {
            return Command(isInner);
        }

        return null;
    }

    /// <summary>
    /// Parses and evaluates a guard.
    /// </summary>
    private void ParseGuard()
    {
        if (Match(TokenType.ScopeGuard))
        {
            ScopeGuard();
        }
    }

    /// <summary>
    /// Parses a directive expression.
    /// </summary>
    /// <returns>Parsed directive expression or <see langword="null" /> if something went wrong.</returns>
    private Expr.Directive Directive()
    {
        Direct body = null;
        var expr = ParseExpr(true);

        if (Match(TokenType.If))
        {
            body = If(expr);
        }

        if (Match(TokenType.Foreach))
        {
            body = Foreach(expr);
        }

        if (body is null)
        {
            ErrorMessage ??= "[Parser] Directive body is invalid";
            return null;
        }

        if (!Match(TokenType.RightSquare))
        {
            ErrorMessage = "[Parser] Missing closing square for directive";
            return null;
        }

        return new(body);
    }

    /// <summary>
    /// Parses a command expression.
    /// </summary>
    /// <param name="isInner">Whether or not this expression is inside another expression.</param>
    /// <returns>Parsed command expression or <see langword="null" /> if something went wrong.</returns>
    private Expr.Command Command(bool isInner)
    {
        var cmd = CommandsUtils.GetCommand(_scope, Peek.Value);

        if (cmd is null)
        {
            ErrorMessage = $"[Parser] Command '{cmd.Command}' was not found";
            return null;
        }

        var args = new List<string>();
        var hasVars = false;
        args.Add(Peek.Value);
        Advance();

        while (CheckNot(TokenType.ScopeGuard) && (!isInner || (Peek.Type != TokenType.RightSquare && Peek.Type < TokenType.If)))
        {
            if (Peek.Type == TokenType.Variable && isInner)
            {
                hasVars = true;
            }

            args.Add(Peek.Value);
            Advance();
        }

        return new(cmd, args.ToArray(), hasVars);
    }

    /// <summary>
    /// Parses and evaluates a scope guard.
    /// </summary>
    private void ScopeGuard()
    {
        CommandType scope = 0;

        while (Check(TokenType.Identifier))
        {
            var parsed = Enum.TryParse<CommandType>(Peek.Value, true, out var result);

            if (!parsed)
            {
                ErrorMessage = $"[Parser] '{Peek.Value}' is not a valid scope type";
                return;
            }

            scope |= result;
            Advance();
        }

        _scope = scope == 0 ? CommandType.RemoteAdmin | CommandType.Console | CommandType.GameConsole : scope;
    }

    /// <summary>
    /// Parses an if directive.
    /// </summary>
    /// <param name="expr">Expression to use as then branch expression.</param>
    /// <returns>Parsed if directive or <see langword="null" /> if something went wrong.</returns>
    private Direct.If If(Expr expr)
    {
        if (expr is null)
        {
            ErrorMessage = "[Parser] Then branch expression for if directive is invalid";
            return null;
        }

        var condition = ParseExpr(true);

        if (condition is null)
        {
            ErrorMessage = "[Parser] Condition for if directive is invalid";
            return null;
        }

        Expr els = null;

        if (Match(TokenType.Else))
        {
            els = ParseExpr(true);

            if (els is null)
            {
                ErrorMessage = "[Parser] Else branch expression for if directive is invalid";
                return null;
            }
        }

        return new(expr, condition, els);
    }

    /// <summary>
    /// Parses a foreach directive.
    /// </summary>
    /// <param name="body">Expression to use as loop body.</param>
    /// <returns>Parsed foreach directive or <see langword="null" /> if something went wrong.</returns>
    private Direct.Foreach Foreach(Expr body)
    {
        if (body is null)
        {
            ErrorMessage = "[Parser] Loop body for foreach directive is invalid";
            return null;
        }

        if (CheckNot(TokenType.Text) || !Iterables.ContainsKey(Peek.Value) || Iterables[Peek.Value] is null)
        {
            ErrorMessage = $"[Parser] '{Peek.Value}' is not a valid iterable object";
            return null;
        }

        var iter = Iterables[Peek.Value]();

        if (iter is null)
        {
            ErrorMessage = $"[Parser] '{Peek.Value}' iterable object could not be retrieved";
            return null;
        }

        Advance();
        return new(body, iter);
    }
    #endregion
}
