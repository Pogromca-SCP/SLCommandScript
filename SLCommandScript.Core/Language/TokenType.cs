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
    Identifier,
    Variable,
    Text,
    If,
    Else,
    Foreach,
    DelayBy
}
