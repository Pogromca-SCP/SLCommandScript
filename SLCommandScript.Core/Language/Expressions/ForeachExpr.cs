using SLCommandScript.Core.Iterables;
using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a foreach expression.
/// </summary>
/// <param name="body">Expression to use as loop body.</param>
/// <param name="iterable">Iterable object to loop over.</param>
public class ForeachExpr(Expr body, IIterable iterable) : Expr
{
    /// <summary>
    /// Expression to use as loop body.
    /// </summary>
    public Expr Body { get; } = body;

    /// <summary>
    /// Iterable object to loop over.
    /// </summary>
    public IIterable Iterable { get; } = iterable;

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="TResult">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitForeachExpr(this);
}
