namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Represents an if expression.
/// </summary>
/// <param name="then">Expression to evaluate when condition is met.</param>
/// <param name="condition">Condition to check.</param>
/// <param name="els">Expression to evaluate when condition is not met.</param>
public class IfExpr(Expr? then, Expr condition, Expr? els) : Expr
{
    /// <summary>
    /// Expression to evaluate when condition is met.
    /// </summary>
    public Expr? Then { get; } = then;

    /// <summary>
    /// Condition to check.
    /// </summary>
    public Expr Condition { get; } = condition;

    /// <summary>
    /// Expression to evaluate when condition is not met.
    /// </summary>
    public Expr? Else { get; } = els;

    /// <inheritdoc />
    public override TResult Accept<TResult>(IExprVisitor<TResult> visitor) => visitor.VisitIfExpr(this);
}
