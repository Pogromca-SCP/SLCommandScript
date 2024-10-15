using System.Collections.Generic;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Represents an argument processing result.
/// </summary>
/// <param name="source">Argument source code.</param>
/// <param name="tokens">Tokens produced from argument.</param>
/// <param name="depth">Argument final scope depth level.</param>
public readonly struct ArgResult(string source, IList<Token> tokens, int depth)
{
    /// <summary>
    /// Contains argument source code.
    /// </summary>
    public string Source { get; } = source;

    /// <summary>
    /// Contains tokens produced from argument.
    /// </summary>
    public IList<Token> Tokens { get; } = tokens;

    /// <summary>
    /// Contains argument final scope depth level.
    /// </summary>
    public int Depth { get; } = depth;

    /// <summary>
    /// Creates new argument processing result.
    /// </summary>
    public ArgResult() : this(string.Empty, [], 0) {}
}
