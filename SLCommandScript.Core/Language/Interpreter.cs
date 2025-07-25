using CommandSystem;
using LabApi.Features.Console;
using MEC;
using SLCommandScript.Core.Language.Expressions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Evaluates and executes provided expressions.
/// </summary>
public class Interpreter : IExprVisitor<bool>
{
    /// <summary>
    /// Represents variables scope.
    /// </summary>
    private class Scope : Dictionary<string, string?>
    {
        /// <summary>
        /// Contains reference to higher variable scope.
        /// </summary>
        public Scope? Next;

        /// <summary>
        /// Creates new variables scope.
        /// </summary>
        /// <param name="next">Higher variables scope to copy values from.</param>
        /// <param name="saveParent">Whether or not the scope should remember its parent.</param>
        public Scope(Scope? next, bool saveParent) : base(StringComparer.OrdinalIgnoreCase)
        {
            Next = saveParent ? next : null;

            if (next is not null)
            {
                foreach (var ent in next)
                {
                    Add(saveParent ? $"^{ent.Key}" : ent.Key, ent.Value);
                }
            }
        }
    }

    /// <summary>
    /// Contains regular expression for variables.
    /// </summary>
    private static readonly Regex _variablePattern = new("\\$\\(([^)\\s]+)\\)");

    /// <summary>
    /// Contains used command sender.
    /// </summary>
    public ICommandSender? Sender { get; private set; }

    /// <summary>
    /// Contains current error message.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Contains current variable values.
    /// </summary>
    private Scope? _variables;

    /// <summary>
    /// Creates new interpreter instance.
    /// </summary>
    /// <param name="sender">Command sender to use for commands.</param>
    public Interpreter(ICommandSender? sender)
    {
        Reset(sender);
        _variables = null;
    }

    /// <summary>
    /// Creates new interpreter instance.
    /// </summary>
    /// <param name="src">Interpreter to copy data from.</param>
    private Interpreter(Interpreter src)
    {
        Reset(src.Sender);
        _variables = src._variables is null ? null : new(src._variables, false);
    }

    /// <summary>
    /// Resets the interpretation process.
    /// </summary>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(ICommandSender? sender)
    {
        Sender = sender;
        ErrorMessage = null;
    }

    /// <inheritdoc />
    public bool VisitCommandExpr(CommandExpr? expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided command expression is null";
            return false;
        }

        if (expr.Cmd is null)
        {
            ErrorMessage = "Cannot execute a null command";
            return false;
        }

        if (expr.Arguments is null)
        {
            ErrorMessage = "Provided command arguments array is null";
            return false;
        }

        if (expr.Arguments.Length < 1)
        {
            ErrorMessage = "Provided command arguments array is empty";
            return false;
        }

        var args = _variables is not null && expr.HasVariables && expr.Arguments.Length > 1 ? InjectArguments(expr.Arguments) : expr.Arguments;
        var result = expr.Cmd.Execute(new(args, 1, args.Length - 1), Sender, out var message);
        
        if (!result)
        {
            ErrorMessage = message;
        }

        return result;
    }

    /// <inheritdoc />
    public bool VisitDelayExpr(DelayExpr? expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided delay expression is null";
            return false;
        }

        if (expr.Body is null)
        {
            ErrorMessage = "Delay expression body is null";
            return false;
        }

        if (expr.Duration < 1)
        {
            return expr.Body.Accept(this);
        }

        var innerInterp = new Interpreter(this);

        Timing.CallDelayed(expr.Duration / 1000, () =>
        {
            var result = expr.Body.Accept(innerInterp);

            if (!result)
            {
                Logger.Error(expr.Name is null ? innerInterp.ErrorMessage! : $"[{expr.Name}] {innerInterp.ErrorMessage}");
            }
        });

        return true;
    }

    /// <inheritdoc />
    public bool VisitForeachExpr(ForeachExpr? expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided foreach expression is null";
            return false;
        }

        if (expr.Body is null)
        {
            ErrorMessage = "Foreach expression body is null";
            return false;
        }

        if (expr.Iterable is null)
        {
            ErrorMessage = "Foreach expression iterable object is null";
            return false;
        }

        _variables = new(_variables, true);

        while (expr.Iterable.LoadNext(_variables))
        {
            var result = expr.Body.Accept(this);

            if (!result)
            {
                _variables = _variables.Next;
                return false;
            }
        }

        _variables = _variables.Next;
        return true;
    }

    /// <inheritdoc />
    public bool VisitForElseExpr(ForElseExpr? expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided forelse expression is null";
            return false;
        }

        if (expr.Then is null)
        {
            ErrorMessage = "Forelse primary expression body is null";
            return false;
        }

        if (expr.Iterable is null)
        {
            ErrorMessage = "Forelse expression iterable object is null";
            return false;
        }

        if (expr.Else is null)
        {
            ErrorMessage = "Forelse secondary expression body is null";
            return false;
        }

        _variables = new(_variables, true);
        var count = 0;
        int? limit = null;

        while (expr.Iterable.LoadNext(_variables))
        {
            limit ??= expr.Limit.IsPrecise ? expr.Limit.Amount : (int) (expr.Limit.Percent * expr.Iterable.Count);
            var result = count < limit ? expr.Then.Accept(this) : expr.Else.Accept(this);
            ++count;

            if (!result)
            {
                _variables = _variables.Next;
                return false;
            }
        }

        _variables = _variables.Next;
        return true;
    }

    /// <inheritdoc />
    public bool VisitIfExpr(IfExpr? expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided if expression is null";
            return false;
        }

        if (expr.Condition is null)
        {
            ErrorMessage = "If expression condition is null";
            return false;
        }

        if (expr.Then is null && expr.Else is null)
        {
            ErrorMessage = "If expression branches are null";
            return false;
        }

        var cond = expr.Condition.Accept(this);
        ErrorMessage = null;

        if (cond)
        {
            return expr.Then is null || expr.Then.Accept(this);
        }
        else
        {
            return expr.Else is null || expr.Else.Accept(this);
        }
    }

    /// <inheritdoc />
    public bool VisitSequenceExpr(SequenceExpr? expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided sequence expression is null";
            return false;
        }

        if (expr.Body is null)
        {
            ErrorMessage = "Sequence expression body is null";
            return false;
        }

        foreach (var exp in expr.Body)
        {
            if (exp is not null && !exp.Accept(this))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Injects appropriate values in place of variables.
    /// </summary>
    /// <param name="args">Original arguments values.</param>
    /// <returns>Arguments with injected variables values.</returns>
    private string?[] InjectArguments(string?[] args)
    {
        var results = new string?[args.Length];
        results[0] = args[0];

        for (var index = 1; index < args.Length; ++index)
        {
            var arg = args[index];

            if (arg is not null)
            {
                arg = _variablePattern.Replace(arg, m => _variables!.ContainsKey(m.Groups[1].Value) ? _variables[m.Groups[1].Value] : m.Value);
            }

            results[index] = arg;
        }

        return results;
    }
}
