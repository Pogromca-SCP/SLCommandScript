using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a sequence expression.
/// </summary>
/// <param name="body">Expressions to execute in a sequence.</param>
public class SequenceExpr(IEnumerable<Expr?>? body) : Expr
{
    /// <summary>
    /// Expressions to execute in a sequence.
    /// </summary>
    public IEnumerable<Expr?>? Body { get; } = body;

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="TResult">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitSequenceExpr(this);
}
