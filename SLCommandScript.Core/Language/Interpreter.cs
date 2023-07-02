﻿using SLCommandScript.Core.Interfaces;
using System.Text.RegularExpressions;
using CommandSystem;
using System.Collections.Generic;
using SLCommandScript.Core.Language.Expressions;
using System;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Evaluates and executes provided expressions.
/// </summary>
public class Interpreter : IExprVisitor<bool>
{
    /// <summary>
    /// Contains regular expression for variables.
    /// </summary>
    private static readonly Regex _variablePattern = new("\\$\\(([a-zA-Z]+)\\)");

    #region Fields and Properties
    /// <summary>
    /// Contains used command sender.
    /// </summary>
    public ICommandSender Sender { get; private set; }

    /// <summary>
    /// Contains current error message.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Contains current variable values.
    /// </summary>
    private readonly Dictionary<string, string> _variables;
    #endregion

    #region State Management
    /// <summary>
    /// Creates new interpreter instance.
    /// </summary>
    /// <param name="sender">Command sender to use for commands.</param>
    public Interpreter(ICommandSender sender)
    {
        Reset(sender);
        ErrorMessage = null;
        _variables = new();
    }

    /// <summary>
    /// Resets the interpretation process.
    /// </summary>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(ICommandSender sender)
    {
        Sender = sender;
    }
    #endregion

    #region Expressions Processing
    /// <summary>
    /// Visits a command expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public bool VisitCommandExpr(CommandExpr expr)
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

        var args = expr.HasVariables && expr.Arguments.Length > 1 ? InjectArguments(expr.Arguments) : ClearArguments(expr.Arguments);
        var result = expr.Cmd.Execute(new ArraySegment<string>(args, 1, args.Length), Sender, out var message);

        if (!result)
        {
            ErrorMessage = message;
        }

        return result;
    }

    /// <summary>
    /// Visits a foreach expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public bool VisitForeachExpr(ForeachExpr expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided foreach directive is null";
            return false;
        }

        if (expr.Body is null)
        {
            ErrorMessage = "Foreach directive body is null";
            return false;
        }

        if (expr.Iterable is null)
        {
            ErrorMessage = "Foreach directive iterable object is null";
            return false;
        }

        while (expr.Iterable.LoadNext(_variables))
        {
            var result = expr.Body.Accept(this);

            if (!result)
            {
                _variables.Clear();
                return false;
            }
        }

        _variables.Clear();
        return true;
    }

    /// <summary>
    /// Visits an if expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public bool VisitIfExpr(IfExpr expr)
    {
        if (expr is null)
        {
            ErrorMessage = "Provided if directive is null";
            return false;
        }

        if (expr.Then is null)
        {
            ErrorMessage = "If directive then branch is null";
            return false;
        }

        if (expr.Condition is null)
        {
            ErrorMessage = "If directive condition is null";
            return false;
        }

        if (expr.Condition.Accept(this))
        {
            return expr.Then.Accept(this);
        }
        else if (expr.Else is not null)
        {
            return expr.Else.Accept(this);
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Sanitizes arguments values.
    /// </summary>
    /// <param name="args">Original arguments values.</param>
    /// <returns>Arguments with sanitized values.</returns>
    private string[] ClearArguments(string[] args)
    {
        var results = new string[args.Length];

        for (var index = 0; index < args.Length; ++index)
        {
            results[index] = args[index] ?? string.Empty;
        }

        return results;
    }

    /// <summary>
    /// Injects appropriate values in place of variables.
    /// </summary>
    /// <param name="args">Original arguments values.</param>
    /// <returns>Arguments with injected variables values.</returns>
    private string[] InjectArguments(string[] args)
    {
        var results = new string[args.Length];
        results[0] = args[0] ?? string.Empty;

        for (var index = 1; index < args.Length; ++index)
        {
            var arg = args[index];

            if (arg is null)
            {
                results[index] = string.Empty;
            }
            else
            {
                results[index] = _variablePattern.Replace(arg, m => _variables.ContainsKey(m.Groups[0].Value) ? _variables[m.Groups[0].Value] : m.Value);
            }
        }

        return results;
    }
    #endregion
}
