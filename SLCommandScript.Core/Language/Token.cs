namespace SLCommandScript.Core.Language;

/// <summary>
/// Represents a single token in SLC Script.
/// </summary>
public readonly struct Token
{
    /// <summary>
    /// Contains type of the token.
    /// </summary>
    public TokenType Type { get; }

    /// <summary>
    /// Contains a value assigned to this token.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Contains line number where the token is located.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Creates new token structure.
    /// </summary>
    public Token() : this(TokenType.None, null, 0) {}

    /// <summary>
    /// Creates new token structure.
    /// </summary>
    /// <param name="type">Token type to set.</param>
    /// <param name="lexeme">Value related to this token.</param>
    /// <param name="line">Line number where the token was found.</param>
    public Token(TokenType type, string value, int line)
    {
        Type = type;
        Value = value ?? string.Empty;
        Line = line;
    }

    /// <summary>
    /// Converts this token into a human readable string.
    /// </summary>
    /// <returns>Human readable representation of this token.</returns>
    public override string ToString() => $"{Type} '{Value}' at line {Line}";
}
