namespace SLCommandScript.Core.Language;

/// <summary>
/// Defines SLC Script token types.
/// </summary>
public enum TokenType : byte
{
    /// <summary>
    /// Empty token type for technical purposes.
    /// </summary>
    None,

    /// <summary>
    /// [
    /// </summary>
    LeftSquare,

    /// <summary>
    /// ]
    /// </summary>
    RightSquare,

    /// <summary>
    /// #?
    /// </summary>
    ScopeGuard,

    /// <summary>
    /// $(variable_name)
    /// </summary>
    Variable,

    /// <summary>
    /// Represents any text value not matching other token types.
    /// </summary>
    Text,

    /// <summary>
    /// -?[0-9]*
    /// </summary>
    Number,

    /// <summary>
    /// NUMBER%
    /// </summary>
    Percentage,

    /// <summary>
    /// if
    /// </summary>
    If,

    /// <summary>
    /// else
    /// </summary>
    Else,

    /// <summary>
    /// foreach
    /// </summary>
    Foreach,

    /// <summary>
    /// delayby
    /// </summary>
    DelayBy,

    /// <summary>
    /// forrandom
    /// </summary>
    ForRandom,

    /// <summary>
    /// |
    /// </summary>
    Sequence
}
