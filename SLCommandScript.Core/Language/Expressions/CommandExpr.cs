using CommandSystem;
using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a command expression.
/// </summary>
/// <param name="cmd">Command to execute.</param>
/// <param name="args">Command arguments to use.</param>
/// <param name="hasVariables">Whether or not this expression contains variables.</param>
public class CommandExpr(ICommand cmd, string[] args, bool hasVariables) : Expr
{
    /// <summary>
    /// Command to execute.
    /// </summary>
    public ICommand Cmd { get; } = cmd;

    /// <summary>
    /// Command arguments to use.
    /// </summary>
    public string[] Arguments { get; } = args;

    /// <summary>
    /// Whether or not this expression contains variables.
    /// </summary>
    public bool HasVariables { get; } = hasVariables;

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="TResult">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitCommandExpr(this);
}
