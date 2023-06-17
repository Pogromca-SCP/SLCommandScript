using System;
using SLCommandScript.Core.Interfaces;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents an if expression.
/// </summary>
public class IfExpr : Expr
{
    /// <summary>
    /// Expression to evaluate when condition is met.
    /// </summary>
    public Expr Then { get; }

    /// <summary>
    /// Condition to check.
    /// </summary>
    public Expr Condition { get; }

    /// <summary>
    /// Expression to evaluate when condition is not met.
    /// </summary>
    public Expr Else { get; }

    /// <summary>
    /// Creates new if expression representation.
    /// </summary>
    /// <param name="then">Expression to evaluate when condition is met.</param>
    /// <param name="condition">Condition to check.</param>
    /// <param name="els">Expression to evaluate when condition is not met.</param>
    public IfExpr(Expr then, Expr condition, Expr els)
    {
        Then = then;
        Condition = condition;
        Else = els;
    }

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitIfExpr(this);
}
