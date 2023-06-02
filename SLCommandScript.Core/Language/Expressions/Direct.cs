using System;
using SLCommandScript.Core.Interfaces;

namespace SLCommandScript.Core.Language.Expressions;

/// <summary>
/// Base class for directive representations.
/// </summary>
public abstract class Direct
{
    /// <summary>
    /// Interface to implement in order to create a directives visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit results.</typeparam>
    public interface IVisitor<T>
    {
        /// <summary>
        /// Visits an if directive.
        /// </summary>
        /// <param name="direct">Directive to visit.</param>
        /// <returns>Result value of the visit.</returns>
        T VisitIfDirect(If direct);

        /// <summary>
        /// Visits a foreach directive.
        /// </summary>
        /// <param name="direct">Directive to visit.</param>
        /// <returns>Result value of the visit.</returns>
        T VisitForeachDirect(Foreach direct);
    }

    /// <summary>
    /// Represents an if directive.
    /// </summary>
    public class If : Direct
    {
        /// <summary>
        /// Expression to evaluate when condition is met.
        /// </summary>
        public Expr Then { get; }

        /// <summary>
        /// Condition to check.
        /// </summary>
        public Expr Condition { get; }

        /// <summary>
        /// Expression to evaluate when condition is not met.
        /// </summary>
        public Expr Else { get; }

        /// <summary>
        /// Creates new if directive representation.
        /// </summary>
        /// <param name="then">Expression to evaluate when condition is met.</param>
        /// <param name="condition">Condition to check.</param>
        /// <param name="els">Expression to evaluate when condition is not met.</param>
        public If(Expr then, Expr condition, Expr els)
        {
            Then = then;
            Condition = condition;
            Else = els;
        }

        /// <summary>
        /// Accepts a visit from a directive visitor.
        /// </summary>
        /// <typeparam name="T">Type used for visit result.</typeparam>
        /// <param name="visitor">Visitor to accept.</param>
        /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
        /// <returns>Result of accepted visit.</returns>
        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfDirect(this);
    }

    /// <summary>
    /// Represents a foreach directive.
    /// </summary>
    public class Foreach : Direct
    {
        /// <summary>
        /// Expression to use as loop body.
        /// </summary>
        public Expr Body { get; }

        /// <summary>
        /// Iterable object to loop over.
        /// </summary>
        public IIterable Iterable { get; }

        /// <summary>
        /// Creates new foreach directive representation.
        /// </summary>
        /// <param name="body">Expression to use as loop body.</param>
        /// <param name="iterable">Iterable object to loop over.</param>
        public Foreach(Expr body, IIterable iterable)
        {
            Body = body;
            Iterable = iterable;
        }

        /// <summary>
        /// Accepts a visit from a directive visitor.
        /// </summary>
        /// <typeparam name="T">Type used for visit result.</typeparam>
        /// <param name="visitor">Visitor to accept.</param>
        /// <exception cref="NullReferenceException">When provided visitor is <see langword="null" />.</exception>
        /// <returns>Result of accepted visit.</returns>
        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitForeachDirect(this);
    }

    /// <summary>
    /// Accepts a visit from a directive visitor.
    /// </summary>
    /// <typeparam name="T">Type used for visit result.</typeparam>
    /// <param name="visitor">Visitor to accept.</param>
    /// <returns>Result of accepted visit.</returns>
    public abstract T Accept<T>(IVisitor<T> visitor);
}
