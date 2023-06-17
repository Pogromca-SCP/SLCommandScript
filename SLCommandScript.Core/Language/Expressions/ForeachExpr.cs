using SLCommandScript.Core.Interfaces;
using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a foreach expression.
/// </summary>
public class ForeachExpr : Expr
{
    /// <summary>
    /// Expression to use as loop body.
    /// </summary>
    public Expr Body { get; }

    /// <summary>
    /// Iterable object to loop over.
    /// </summary>
    public IIterable Iterable { get; }

    /// <summary>
    /// Creates new foreach expression representation.
    /// </summary>
    /// <param name="body">Expression to use as loop body.</param>
    /// <param name="iterable">Iterable object to loop over.</param>
    public ForeachExpr(Expr body, IIterable iterable)
    {
        Body = body;
        Iterable = iterable;
    }

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitForeachExpr(this);
}
