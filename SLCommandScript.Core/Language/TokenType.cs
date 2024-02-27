namespace SLCommandScript.Core.Language;

/// <summary>
/// Defines SLC Script token types.
/// </summary>
public enum TokenType : byte
{
    None,
    LeftSquare,
    RightSquare,
    ScopeGuard,
    Variable,
    Text,
    Number,
    Percentage,
    If,
    Else,
    Foreach,
    DelayBy,
    ForRandom,
    Sequence
}
