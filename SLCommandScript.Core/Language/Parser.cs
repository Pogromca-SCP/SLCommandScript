using NorthwoodLib.Pools;
using PluginAPI.Enums;
using SLCommandScript.Core.Commands;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Iterables;
using SLCommandScript.Core.Language.Expressions;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Parses provided tokens into expressions.
/// </summary>
public class Parser
{
    #region Fields and Properties
    /// <summary>
    /// Contains current error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Contains current commands scope.
    /// </summary>
    public CommandType Scope { get; set; } = CommandsUtils.AllScopes;

    /// <summary>
    /// <see langword="true" /> if tokens end was reached, <see langword="false" /> otherwise.
    /// </summary>
    private bool IsAtEnd => _current >= _tokens.Count;

    /// <summary>
    /// Contains a list with tokens to process.
    /// </summary>
    private IList<Token> _tokens;

    /// <summary>
    /// Contains current token index.
    /// </summary>
    private int _current;

    /// <summary>
    /// Contains current scope depth level.
    /// </summary>
    private int _depth;
    #endregion

    #region State Management
    /// <summary>
    /// Parses an expression from provided tokens list.
    /// </summary>
    /// <param name="tokens">List with tokens to process.</param>
    /// <returns>Parsed expression or <see langword="null" /> if something went wrong.</returns>
    public Expr Parse(IList<Token> tokens)
    {
        if (tokens is null)
        {
            ErrorMessage = "Provided tokens list to parse was null";
            return null;
        }

        ErrorMessage = null;
        _tokens = tokens;
        _current = 0;
        _depth = 0;
        var expr = ParseExpr();

        if (ErrorMessage is not null)
        {
            _tokens = null;
            return null;
        }

        ParseGuard();

        if (ErrorMessage is not null)
        {
            _tokens = null;
            return null;
        }

        if (!IsAtEnd)
        {
            ErrorMessage = $"An unexpected token remained after parsing (TokenType: {_tokens[_current].Type})";
            _tokens = null;
            return null;
        }

        _tokens = null;
        return expr;
    }

    /// <summary>
    /// Attempts to match specific token type.
    /// </summary>
    /// <param name="types">Token type to match.</param>
    /// <returns><see langword="true" /> if token type was matched, <see langword="false" /> otherwise.</returns>
    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            ++_current;
            return true;
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
    /// <returns>Parsed expression or <see langword="null" /> if nothing could be parsed.</returns>
    private Expr ParseExpr()
    {
        if (Match(TokenType.LeftSquare))
        {
            return Directive();
        }

        if (CheckNot(TokenType.ScopeGuard))
        {
            return Command();
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
        ++_depth;
        var expr = ParseExpr();
        var keyword = Advance();

        Expr body = keyword.Type switch
        {
            TokenType.If => If(expr),
            TokenType.Else => Else(expr),
            TokenType.Foreach => Foreach(expr),
            TokenType.DelayBy => Delay(expr),
            TokenType.ForRandom => ForRandom(expr),
            TokenType.Sequence => Sequence(expr),
            _ => null
        };

        --_depth;

        if (body is null)
        {
            ErrorMessage ??= "Directive body is invalid";
            return null;
        }

        if (!Match(TokenType.RightSquare))
        {
            ErrorMessage = "Missing closing square bracket for directive";
            return null;
        }

        return body;
    }

    /// <summary>
    /// Parses a command expression.
    /// </summary>
    /// <returns>Parsed command expression or <see langword="null" /> if something went wrong.</returns>
    private CommandExpr Command()
    {
        var cmd = CommandsUtils.GetCommand(Scope, _tokens[_current].Value);

        if (cmd is null)
        {
            ErrorMessage = $"Command '{_tokens[_current].Value}' was not found";
            return null;
        }

        var args = ListPool<string>.Shared.Rent();
        var hasVars = false;
        args.Add(_tokens[_current].Value);
        ++_current;

        while (CheckNot(TokenType.ScopeGuard) && (_depth < 1 || (_tokens[_current].Type != TokenType.RightSquare && _tokens[_current].Type < TokenType.If)))
        {
            hasVars = hasVars || (_depth > 0 && _tokens[_current].Type == TokenType.Variable);
            args.Add(_tokens[_current].Value);
            ++_current;
        }

        var argsArr = args.ToArray();
        ListPool<string>.Shared.Return(args);
        return new(cmd, argsArr, hasVars);
    }

    /// <summary>
    /// Parses and evaluates a scope guard.
    /// </summary>
    private void ScopeGuard()
    {
        CommandType scope = 0;

        while (Check(TokenType.Text))
        {
            var parsed = Enum.TryParse<CommandType>(_tokens[_current].Value, true, out var result);

            if (!parsed)
            {
                ErrorMessage = $"'{_tokens[_current].Value}' is not a valid scope name";
                return;
            }

            scope |= result;
            ++_current;
        }

        Scope = scope == 0 ? CommandsUtils.AllScopes : scope;
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
            ErrorMessage += "\nin if branch expression";
            return null;
        }

        var condition = ParseExpr();

        if (condition is null)
        {
            ErrorMessage = ErrorMessage is null ? "If condition expression is missing" : $"{ErrorMessage}\nin if condition expression";
            return null;
        }

        Expr els = null;

        if (Match(TokenType.Else))
        {
            els = ParseExpr();

            if (els is null)
            {
                ErrorMessage = ErrorMessage is null ? "Else branch expression is missing" : $"{ErrorMessage}\nin else branch expression";
                return null;
            }
        }

        return new(expr, condition, els);
    }

    /// <summary>
    /// Parses an if expression without then branch.
    /// </summary>
    /// <param name="expr">Expression to use as condition expression.</param>
    /// <returns>Parsed if expression or <see langword="null" /> if something went wrong.</returns>
    private IfExpr Else(Expr expr)
    {
        if (expr is null)
        {
            ErrorMessage += "\nin if condition expression";
            return null;
        }

        var els = ParseExpr();

        if (els is null)
        {
            ErrorMessage = ErrorMessage is null ? "Else branch expression is missing" : $"{ErrorMessage}\nin else branch expression";
            return null;
        }

        return new(null, expr, els);
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
            ErrorMessage += "\nin foreach loop body expression";
            return null;
        }

        var iter = GetIterable();

        if (iter is null)
        {
            return null;
        }

        ++_current;
        return new(body, iter);
    }

    /// <summary>
    /// Parses a for random expression.
    /// </summary>
    /// <param name="body">Expression to use as loop body.</param>
    /// <returns>Parsed expression or <see langword="null" /> if something went wrong.</returns>
    private Expr ForRandom(Expr body)
    {
        if (body is null)
        {
            ErrorMessage += "\nin for random loop body expression";
            return null;
        }

        var iter = GetIterable();

        if (iter is null)
        {
            return null;
        }

        ++_current;
        var limit = new RandomSettings(1);

        if (Check(TokenType.Text))
        {
            limit = _tokens[_current].Value.Length > 0 && _tokens[_current].Value[_tokens[_current].Value.Length - 1] == '%' ? new(ParsePercent()) : new(ParseNumber());

            if (!limit.IsValid)
            {
                if (limit.IsEmpty)
                {
                    ErrorMessage = "Limit of random elements must be greater than 0";
                }

                return null;
            }

            ++_current;
        }

        if (!Match(TokenType.Else))
        {
            iter.Randomize(limit);
            return new ForeachExpr(body, iter);
        }
        
        var els = ParseExpr();
        
        if (els is null)
        {
            ErrorMessage = ErrorMessage is null ? "For random loop else expression is missing" : $"{ErrorMessage}\nin for random loop else expression";
            return null;
        }

        iter.Randomize();
        return new ForElseExpr(body, iter, els, limit);
    }

    /// <summary>
    /// Parses a delay expression.
    /// </summary>
    /// <param name="body">Expression to execute after the delay.</param>
    /// <returns>Parsed delay expression or <see langword="null" /> if something went wrong.</returns>
    private DelayExpr Delay(Expr body)
    {
        if (body is null)
        {
            ErrorMessage += "\nin delay body expression";
            return null;
        }

        if (!Check(TokenType.Text))
        {
            ErrorMessage = "Delay duration is missing";
            return null;
        }

        var duration = ParseNumber();

        if (duration < 0)
        {
            return null;
        }

        ++_current;
        string name = null;

        if (!IsAtEnd && _tokens[_current].Type > TokenType.ScopeGuard)
        {
            name = _tokens[_current].Value;
            ++_current;
        }

        return new(body, duration, name);
    }

    /// <summary>
    /// Parses a sequence expression.
    /// </summary>
    /// <param name="initial">First expression to execute in the sequence.</param>
    /// <returns>Parsed sequence expression or <see langword="null" /> if something went wrong.</returns>
    private SequenceExpr Sequence(Expr initial)
    {
        if (initial is null)
        {
            ErrorMessage += "\nin sequence expression 1";
            return null;
        }

        var body = new List<Expr>()
        {
            initial
        };

        do
        {
            var expr = ParseExpr();
            body.Add(expr);

            if (expr is null)
            {
                ErrorMessage = ErrorMessage is null ? $"Sequence expression {body.Count} is missing" : $"{ErrorMessage}\nin sequence expression {body.Count}";
                return null;
            }
        }
        while (Match(TokenType.Sequence));

        return new(body);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Attempts to retrieve an iterable object from current token.
    /// </summary>
    /// <returns>Retrieved iterable object or <see langword="null" /> if something went wrong.</returns>
    private IIterable GetIterable()
    {
        if (IsAtEnd || _tokens[_current].Type < TokenType.Variable)
        {
            ErrorMessage = "Iterable object name is missing";
            return null;
        }

        if (!IterablesUtils.Providers.ContainsKey(_tokens[_current].Value))
        {
            ErrorMessage = $"'{_tokens[_current].Value}' is not a valid iterable object name";
            return null;
        }

        var provider = IterablesUtils.Providers[_tokens[_current].Value];

        if (provider is null)
        {
            ErrorMessage = $"Provider for '{_tokens[_current].Value}' iterable object is null";
            return null;
        }

        var iter = provider();

        if (iter is null)
        {
            ErrorMessage = $"Provider for '{_tokens[_current].Value}' iterable object returned null";
            return null;
        }

        return iter;
    }

    /// <summary>
    /// Attempts to parse a percent from current token.
    /// </summary>
    /// <returns>Parsed percent or -1 if something went wrong.</returns>
    private float ParsePercent()
    {
        var result = 0;
        var end = _tokens[_current].Value.Length - 1;

        for (var i = 0; i < end; ++i)
        {
            var ch = _tokens[_current].Value[i];

            if (ch < '0' || ch > '9')
            {
                ErrorMessage = $"Expected '{_tokens[_current].Value}' to be a percentage";
                return -1;
            }

            result *= 10;
            result += ch - '0';
        }

        return result / 100.0f;
    }

    /// <summary>
    /// Attempts to parse a number from current token.
    /// </summary>
    /// <returns>Parsed number or -1 if something went wrong.</returns>
    private int ParseNumber()
    {
        var result = 0;

        foreach (var ch in _tokens[_current].Value)
        {
            if (ch < '0' || ch > '9')
            {
                ErrorMessage = $"Expected '{_tokens[_current].Value}' to be a number";
                return -1;
            }

            result *= 10;
            result += ch - '0';
        }

        return result;
    }
    #endregion
}
