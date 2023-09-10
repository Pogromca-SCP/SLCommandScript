using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PluginAPI.Core;
using CommandSystem;
using SLCommandScript.Core.Language.Expressions;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Evaluates and executes provided expressions.
/// </summary>
public class Interpreter : IExprVisitor<bool>
{
    /// <summary>
    /// Represents variables scope.
    /// </summary>
    private class Scope : Dictionary<string, string>
    {
        /// <summary>
        /// Contains reference to higher variable scope.
        /// </summary>
        public Scope Next;

        /// <summary>
        /// Creates new variables scope.
        /// </summary>
        /// <param name="next">Higher variables scope to copy values from.</param>
        public Scope(Scope next) : base(StringComparer.OrdinalIgnoreCase)
        {
            Next = next;

            if (next is not null)
            {
                foreach (var ent in next)
                {
                    Add(ent.Key, ent.Value);
                }
            }
        }
    }

    /// <summary>
    /// Contains regular expression for variables.
    /// </summary>
    private static readonly Regex _variablePattern = new("\\$\\(([a-zA-Z]+)\\)");

    /// <summary>
    /// Executes delay expression.
    /// </summary>
    /// <param name="interp">Interpreter instance to use.</param>
    /// <param name="expr">Expression to execute.</param>
    private static async void ExecuteDelayExprAsync(Interpreter interp, DelayExpr expr)
    {
        await Task.Delay(expr.Duration);
        var result = expr.Body.Accept(interp);

        if (!result)
        {
            Log.Error(interp.ErrorMessage, expr.Name is null ? "Async script: " : $"Async script ('{expr.Name}'): ");
        }
    }

    #region Fields and Properties
    /// <summary>
    /// Contains used command sender.
    /// </summary>
    public ICommandSender Sender { get; private set; }

    /// <summary>
    /// Contains current error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Contains current variable values.
    /// </summary>
    private Scope _variables;
    #endregion

    #region State Management
    /// <summary>
    /// Creates new interpreter instance.
    /// </summary>
    /// <param name="sender">Command sender to use for commands.</param>
    public Interpreter(ICommandSender sender)
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
        _variables = src._variables is null ? null : new(src._variables);

        if (_variables is not null)
        {
            _variables.Next = null;
        }
    }

    /// <summary>
    /// Resets the interpretation process.
    /// </summary>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(ICommandSender sender)
    {
        Sender = sender;
        ErrorMessage = null;
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

        var args = expr.HasVariables && expr.Arguments.Length > 1 ? InjectArguments(expr.Arguments) : expr.Arguments;
        var result = expr.Cmd.Execute(new ArraySegment<string>(args, 1, args.Length - 1), Sender, out var message);
        ErrorMessage = result ? null : message;
        return result;
    }

    /// <summary>
    /// Visits a delay expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public bool VisitDelayExpr(DelayExpr expr)
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

        ExecuteDelayExprAsync(new(this), expr);
        ErrorMessage = null;
        return true;
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

        _variables = new(_variables);

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
        ErrorMessage = null;
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
            ErrorMessage = "Provided if expression is null";
            return false;
        }

        if (expr.Then is null)
        {
            ErrorMessage = "If expression then branch is null";
            return false;
        }

        if (expr.Condition is null)
        {
            ErrorMessage = "If expression condition is null";
            return false;
        }

        var cond = expr.Condition.Accept(this);

        if (cond)
        {
            return expr.Then.Accept(this);
        }
        else if (expr.Else is not null)
        {
            return expr.Else.Accept(this);
        }
        else
        {
            ErrorMessage = null;
            return true;
        }
    }

    /// <summary>
    /// Injects appropriate values in place of variables.
    /// </summary>
    /// <param name="args">Original arguments values.</param>
    /// <returns>Arguments with injected variables values.</returns>
    private string[] InjectArguments(string[] args)
    {
        if (_variables is null)
        {
            return args;
        }

        var results = new string[args.Length];
        results[0] = args[0];

        for (var index = 1; index < args.Length; ++index)
        {
            var arg = args[index];

            if (arg is not null)
            {
                arg = _variablePattern.Replace(arg, m => _variables.ContainsKey(m.Groups[1].Value) ? _variables[m.Groups[1].Value] : m.Value);
            }

            results[index] = arg;
        }

        return results;
    }
    #endregion
}
