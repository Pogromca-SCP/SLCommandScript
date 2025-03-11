using SLCommandScript.Core.Iterables;
using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a for else expression.
/// </summary>
/// <param name="then">Expression to use as a primary loop body.</param>
/// <param name="iterable">Iterable object to loop over.</param>
/// <param name="els">Expression to use as a secondary loop body.</param>
/// <param name="limit">Limit of iterations for primary expression.</param>
public class ForElseExpr(Expr then, IIterable iterable, Expr els, IterableSettings limit) : Expr
{
    /// <summary>
    /// Expression to use as a primary loop body.
    /// </summary>
    public Expr Then { get; } = then;

    /// <summary>
    /// Iterable object to loop over.
    /// </summary>
    public IIterable Iterable { get; } = iterable;

    /// <summary>
    /// Expression to use as a secondary loop body.
    /// </summary>
    public Expr Else { get; } = els;

    /// <summary>
    /// Limit of iterations for primary expression.
    /// </summary>
    public IterableSettings Limit { get; } = limit;

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="TResult">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitForElseExpr(this);
}
