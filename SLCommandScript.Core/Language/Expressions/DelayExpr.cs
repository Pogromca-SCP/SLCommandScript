using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a delay expression.
/// </summary>
/// <param name="body">Expression to execute after the delay.</param>
/// <param name="duration">Duration of the delay in milliseconds.</param>
/// <param name="name">Optional name of the delayed operation.</param>
public class DelayExpr(Expr body, int duration, string name) : Expr
{
    /// <summary>
    /// Expression to execute after the delay.
    /// </summary>
    public Expr Body { get; } = body;

    /// <summary>
    /// Duration of the delay in milliseconds.
    /// </summary>
    public int Duration { get; } = duration;

    /// <summary>
    /// Contains delay operation name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="TResult">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitDelayExpr(this);
}
