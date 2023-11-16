namespace SLCommandScript.Core.Language;

/// <summary>
/// Represents a single token in SLC Script.
/// </summary>
/// <param name="type">Token type to set.</param>
/// <param name="value">Value related to this token.</param>
/// <param name="line">Line number where the token was found.</param>
public readonly struct Token(TokenType type, string value, int line)
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
    /// Contains line number where the token is located.
    /// </summary>
    public int Line { get; } = line;

    /// <summary>
    /// Creates new token structure.
    /// </summary>
    public Token() : this(TokenType.None, null, 0) {}

    /// <summary>
    /// Converts this token into a human readable string.
    /// </summary>
    /// <returns>Human readable representation of this token.</returns>
    public override string ToString() => $"{Type} '{Value}' at line {Line}";
}
