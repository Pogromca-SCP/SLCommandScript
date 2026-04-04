using System.Collections.Generic;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a sequence expression.
/// </summary>
/// <param name="body">Expressions to execute in a sequence.</param>
public class SequenceExpr(IEnumerable<Expr> body) : Expr
{
    /// <summary>
    /// Expressions to execute in a sequence.
    /// </summary>
    public IEnumerable<Expr> Body { get; } = body;

    /// <inheritdoc />
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitSequenceExpr(this);
}
