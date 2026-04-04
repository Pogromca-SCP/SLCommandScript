using SLCommandScript.Core.Iterables;

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

    /// <inheritdoc />
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitForeachExpr(this);
}
