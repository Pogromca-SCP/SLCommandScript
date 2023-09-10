using SLCommandScript.Core.Interfaces;
using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents a delay expression.
/// </summary>
public class DelayExpr : Expr
{
    /// <summary>
    /// Expression to execute after the delay.
    /// </summary>
    public Expr Body { get; }

    /// <summary>
    /// Duration of the delay in milliseconds.
    /// </summary>
    public int Duration { get; }

    /// <summary>
    /// Contains delay operation name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates new delay expression representation.
    /// </summary>
    /// <param name="body">Expression to execute after the delay.</param>
    /// <param name="duration">Duration of the delay in milliseconds.</param>
    /// <param name="name">Optional name of the delayed operation.</param>
    public DelayExpr(Expr body, int duration, string name)
    {
        Body = body;
        Duration = duration;
        Name = name;
    }

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
    /// <returns>Result of accepted visit.</returns>
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitDelayExpr(this);
}
