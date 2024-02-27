namespace SLCommandScript.Core.Language;

/// <summary>
/// Represents a single token in SLC Script.
/// </summary>
/// <param name="type">Token type to set.</param>
/// <param name="value">Value related to this token.</param>
/// <param name="numeric">Numeric value related to this token.</param>
public readonly struct Token(TokenType type, string value, int numeric = 0)
{
    /// <summary>
    /// Contains type of the token.
    /// </summary>
    public TokenType Type { get; } = type;

    /// <summary>
    /// Contains a value assigned to this token.
    /// </summary>
    public string Value { get; } = value ?? string.Empty;

    /// <summary>
    /// Contains a numeric value assigned to this token.
    /// </summary>
    public int NumericValue { get; } = numeric;

    /// <summary>
    /// Creates new token structure.
    /// </summary>
    public Token() : this(TokenType.None, null, 0) {}
}
