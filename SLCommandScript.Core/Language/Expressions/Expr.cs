using CommandSystem;
using System;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Base class for expression representations.
/// </summary>
public abstract class Expr
{
    /// <summary>
    /// Interface to implement in order to create an expressions visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit results.</typeparam>
    public interface IVisitor<T>
    {
        /// <summary>
        /// Visits a command expression.
        /// </summary>
        /// <param name="expr">Expression to visit.</param>
        /// <returns>Result value of the visit.</returns>
        T VisitCommandExpr(Command expr);

        /// <summary>
        /// Visits a directive expression.
        /// </summary>
        /// <param name="expr">Expression to visit.</param>
        /// <returns>Result value of the visit.</returns>
        T VisitDirectiveExpr(Directive expr);
    }

    /// <summary>
    /// Represents a command expression.
    /// </summary>
    public class Command : Expr
    {
        /// <summary>
        /// Command to execute.
        /// </summary>
        public ICommand Cmd { get; }

        /// <summary>
        /// Command arguments to use.
        /// </summary>
        public string[] Arguments { get; }

        /// <summary>
        /// Whether or not this expression contains variables.
        /// </summary>
        public bool HasVariables { get; }

        /// <summary>
        /// Creates new command expression representation.
        /// </summary>
        /// <param name="cmd">Command to execute.</param>
        /// <param name="args">Command arguments to use.</param>
        /// <param name="hasVariables">Whether or not this expression contains variables.</param>
        public Command(ICommand cmd, string[] args, bool hasVariables)
        {
            Cmd = cmd;
            Arguments = args;
            HasVariables = hasVariables;
        }

        /// <summary>
        /// Accepts a visit from an expression visitor.
        /// </summary>
        /// <typeparam name="T">Type used for visit result.</typeparam>
        /// <param name="visitor">Visitor to accept.</param>
        /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
        /// <returns>Result of accepted visit.</returns>
        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCommandExpr(this);
    }

    /// <summary>
    /// Represents a directive expression.
    /// </summary>
    public class Directive : Expr
    {
        /// <summary>
        /// Directive body to use.
        /// </summary>
        public Direct Body { get; }

        /// <summary>
        /// Creates new directive expression representation.
        /// </summary>
        /// <param name="body">Directive body to use.</param>
        public Directive(Direct body)
        {
            Body = body;
        }

        /// <summary>
        /// Accepts a visit from an expression visitor.
        /// </summary>
        /// <typeparam name="T">Type used for visit result.</typeparam>
        /// <param name="visitor">Visitor to accept.</param>
        /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
        /// <returns>Result of accepted visit.</returns>
        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitDirectiveExpr(this);
    }

    /// <summary>
    /// Accepts a visit from an expression visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <returns>Result of accepted visit.</returns>
    public abstract T Accept<T>(IVisitor<T> visitor);
}
