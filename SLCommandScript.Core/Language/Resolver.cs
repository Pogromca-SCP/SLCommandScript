using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Language.Expressions;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Helper class for expressions resolving.
/// </summary>
public class Resolver : IExprVisitor<object>
{
    /// <summary>
    /// Current depth level of variable scopes.
    /// </summary>
    private int _scopeDepth = 0;

    /// <summary>
    /// Visits a command expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public object VisitCommandExpr(CommandExpr expr)
    {
        if (expr is not null && expr.HasVariables)
        {
            expr.HasVariables = _scopeDepth > 0;
        }

        return null;
    }

    /// <summary>
    /// Visits a delay expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public object VisitDelayExpr(DelayExpr expr)
    {
        expr?.Body?.Accept(this);
        return null;
    }

    /// <summary>
    /// Visits a foreach expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public object VisitForeachExpr(ForeachExpr expr)
    {
        if (expr is not null && expr.Body is not null)
        {
            ++_scopeDepth;
            expr.Body.Accept(this);
            --_scopeDepth;
        }

        return null;
    }

    /// <summary>
    /// Visits an if expression.
    /// </summary>
    /// <param name="expr">Expression to visit.</param>
    /// <returns>Result value of the visit.</returns>
    public object VisitIfExpr(IfExpr expr)
    {
        if (expr is not null)
        {
            expr.Then?.Accept(this);
            expr.Condition?.Accept(this);
            expr.Else?.Accept(this);
        }

        return null;
    }
}
