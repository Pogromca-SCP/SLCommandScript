using System.Collections.Generic;
using System;
using SLCommandScript.Core.Interfaces;
using PluginAPI.Enums;
using SLCommandScript.Core.Language.Expressions;
using SLCommandScript.Core.Commands;
using SLCommandScript.Core.Iterables;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Parses provided tokens into expressions.
/// </summary>
public class Parser
{
    /// <summary>
    /// Defines an universal scope value.
    /// </summary>
    public const CommandType AllScopes = CommandType.RemoteAdmin | CommandType.Console | CommandType.GameConsole;

    /// <summary>
    /// Contains iterable objects providers used in foreach expressions.
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
    /// Contains current commands scope.
    /// </summary>
    public CommandType Scope { get; private set; }

    /// <summary>
    /// <see langword="true" /> if tokens end was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => _current >= _tokens.Count;

    /// <summary>
    /// Contains a list with tokens to process.
    /// </summary>
    private readonly IList<Token> _tokens;

    /// <summary>
    /// Contains current token index.
    /// </summary>
    private int _current;
    #endregion

    #region State Management
    /// <summary>
    /// Creates new parser instance.
    /// </summary>
    /// <param name="tokens">List with tokens to process.</param>
    /// <param name="scope">Initial commands scope to use.</param>
    public Parser(IList<Token> tokens, CommandType scope = AllScopes)
    {
        _tokens = tokens ?? new List<Token>();
        Reset(scope);
    }

    /// <summary>
    /// Parses an expression from provided tokens list.
    /// </summary>
    /// <returns>Parsed expression or <see langword="null" /> if something went wrong.</returns>
    public Expr Parse()
    {
        if (ErrorMessage is not null)
        {
            return null;
        }

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
            ErrorMessage = "[Parser] An unexpected token was found after parsing";
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
    /// Resets the parsing process and changes used commands scope.
    /// </summary>
    /// <param name="newScope">New commands scope to use.</param>
    public void Reset(CommandType newScope)
    {
        Scope = newScope;
        Reset();
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
    private bool Check(TokenType type) => !IsAtEnd && _tokens[_current].Type == type;

    /// <summary>
    /// Checks if current token is not of specific type.
    /// </summary>
    /// <param name="type">Unwanted token type.</param>
    /// <returns><see langword="true" /> if current token exists and has correct type, <see langword="false" /> otherwise.</returns>
    private bool CheckNot(TokenType type) => !IsAtEnd && _tokens[_current].Type != type;

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

        return _tokens[_current - 1];
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
    private Expr Directive()
    {
        Expr body = null;
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
            ErrorMessage ??= "[Parser] Directive body is null";
            return null;
        }

        if (!Match(TokenType.RightSquare))
        {
            ErrorMessage = "[Parser] Missing closing square for directive";
            return null;
        }

        return body;
    }

    /// <summary>
    /// Parses a command expression.
    /// </summary>
    /// <param name="isInner">Whether or not this expression is inside another expression.</param>
    /// <returns>Parsed command expression or <see langword="null" /> if something went wrong.</returns>
    private CommandExpr Command(bool isInner)
    {
        var cmd = CommandsUtils.GetCommand(Scope, _tokens[_current].Value);

        if (cmd is null)
        {
            ErrorMessage = $"[Parser] Command '{_tokens[_current].Value}' was not found";
            return null;
        }

        var args = new List<string>();
        var hasVars = false;
        args.Add(_tokens[_current].Value);
        Advance();

        while (CheckNot(TokenType.ScopeGuard) && (!isInner || (_tokens[_current].Type != TokenType.RightSquare && _tokens[_current].Type < TokenType.If)))
        {
            if (_tokens[_current].Type == TokenType.Variable && isInner)
            {
                hasVars = true;
            }

            args.Add(_tokens[_current].Value);
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
            var parsed = Enum.TryParse<CommandType>(_tokens[_current].Value, true, out var result);

            if (!parsed)
            {
                ErrorMessage = $"[Parser] '{_tokens[_current].Value}' is not a valid scope type";
                return;
            }

            scope |= result;
            Advance();
        }

        Scope = scope == 0 ? AllScopes : scope;
    }

    /// <summary>
    /// Parses an if expression.
    /// </summary>
    /// <param name="expr">Expression to use as then branch expression.</param>
    /// <returns>Parsed if expression or <see langword="null" /> if something went wrong.</returns>
    private IfExpr If(Expr expr)
    {
        if (expr is null)
        {
            ErrorMessage = "[Parser] Then branch expression for if directive is null";
            return null;
        }

        var condition = ParseExpr(true);

        if (condition is null)
        {
            ErrorMessage = "[Parser] Condition for if directive is null";
            return null;
        }

        Expr els = null;

        if (Match(TokenType.Else))
        {
            els = ParseExpr(true);

            if (els is null)
            {
                ErrorMessage = "[Parser] Else branch expression for if directive is null";
                return null;
            }
        }

        return new(expr, condition, els);
    }

    /// <summary>
    /// Parses a foreach expression.
    /// </summary>
    /// <param name="body">Expression to use as loop body.</param>
    /// <returns>Parsed foreach expression or <see langword="null" /> if something went wrong.</returns>
    private ForeachExpr Foreach(Expr body)
    {
        if (body is null)
        {
            ErrorMessage = "[Parser] Loop body for foreach directive is null";
            return null;
        }

        if (CheckNot(TokenType.Text) || !Iterables.ContainsKey(_tokens[_current].Value))
        {
            ErrorMessage = $"[Parser] '{_tokens[_current].Value}' is not a valid iterable object";
            return null;
        }

        var provider = Iterables[_tokens[_current].Value];

        if (provider is null)
        {
            ErrorMessage = $"[Parser] Provider for '{_tokens[_current].Value}' iterable object is null";
            return null;
        }

        var iter = provider();

        if (iter is null)
        {
            ErrorMessage = $"[Parser] Provider for '{_tokens[_current].Value}' iterable object returned null";
            return null;
        }

        Advance();
        return new(body, iter);
    }
    #endregion
}
