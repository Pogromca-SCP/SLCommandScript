using CommandSystem;
using System;
using SLCommandScript.Core.Interfaces;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a command expression.
/// </summary>
public class CommandExpr : Expr
{
    /// <summary>
    /// Command to execute.
    /// </summary>
    public ICommand Cmd { get; }

    /// <summary>
    /// Command arguments to use.
    /// </summary>
    public string[] Arguments { get; }

    /// <summary>
    /// Whether or not this expression contains variables.
    /// </summary>
    public bool HasVariables { get; }

    /// <summary>
    /// Creates new command expression representation.
    /// </summary>
    /// <param name="cmd">Command to execute.</param>
    /// <param name="args">Command arguments to use.</param>
    /// <param name="hasVariables">Whether or not this expression contains variables.</param>
    public CommandExpr(ICommand cmd, string[] args, bool hasVariables)
    {
        Cmd = cmd;
        Arguments = args;
        HasVariables = hasVariables;
    }

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitCommandExpr(this);
}
