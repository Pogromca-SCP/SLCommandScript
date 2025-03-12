namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Base class for expression representations.
/// </summary>
public abstract class Expr
{
    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="TResult">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <returns>Result of accepted visit.</returns>
    public abstract TResult Accept<TResult>(IExprVisitor<TResult> visitor);
}
