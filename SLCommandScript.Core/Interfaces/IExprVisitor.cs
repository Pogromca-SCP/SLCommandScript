using SLCommandScript.Core.Language.Expressions;

namespace SLCommandScript.Core.Interfaces;

/// <summary>
/// Interface to implement in order to create an expressions visitor.
/// </summary>
/// <typeparam name="T">Type used for visit results.</typeparam>
public interface IExprVisitor<T>
{
    /// <summary>
    /// Visits a command expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    T VisitCommandExpr(CommandExpr expr);

    /// <summary>
    /// Visits a delay expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    T VisitDelayExpr(DelayExpr expr);

    /// <summary>
    /// Visits a foreach expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    T VisitForeachExpr(ForeachExpr expr);

    /// <summary>
    /// Visits a for else expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    T VisitForElseExpr(ForElseExpr expr);

    /// <summary>
    /// Visits an if expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    T VisitIfExpr(IfExpr expr);
}
