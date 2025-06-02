namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Interface to implement in order to create an expressions visitor.
/// </summary>
/// <typeparam name="TResult">Type used for visit results.</typeparam>
public interface IExprVisitor<out TResult>
{
    /// <summary>
    /// Visits a command expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    TResult VisitCommandExpr(CommandExpr? expr);

    /// <summary>
    /// Visits a delay expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    TResult VisitDelayExpr(DelayExpr? expr);

    /// <summary>
    /// Visits a foreach expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    TResult VisitForeachExpr(ForeachExpr? expr);

    /// <summary>
    /// Visits a for else expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    TResult VisitForElseExpr(ForElseExpr? expr);

    /// <summary>
    /// Visits an if expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    TResult VisitIfExpr(IfExpr? expr);

    /// <summary>
    /// Visits a sequence expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    TResult VisitSequenceExpr(SequenceExpr? expr);
}
