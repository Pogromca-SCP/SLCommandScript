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
    If,
    Else,
    Foreach,
    DelayBy,
    ForRandom,
    Sequence
}
