using CommandSystem;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Permissions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Language;

/// <summary>
/// Performs tokenization of provided source code.
/// </summary>
public class Lexer
{
    #region Static Elements
    /// <summary>
    /// Contains language keywords associated with appropriate token types.
    /// </summary>
    private static readonly Dictionary<string, TokenType> _keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "foreach", TokenType.Foreach },
        { "delayby", TokenType.DelayBy },
        { "forrandom", TokenType.ForRandom },
        { "|", TokenType.Sequence }
    };

    /// <summary>
    /// Checks if provided character is a whitespace.
    /// </summary>
    /// <param name="ch">Character to check.</param>
    /// <returns><see langword="true" /> if character is a whitespace, <see langword="false" /> otherwise.</returns>
    public static bool IsWhiteSpace(char ch) => ch == ' ' || ch == '\n' || ch == '\t' || ch == '\r' || ch == '\0';

    /// <summary>
    /// Checks if provided character is a digit.
    /// </summary>
    /// <param name="ch">Character to check.</param>
    /// <returns><see langword="true" /> if character is a digit, <see langword="false" /> otherwise.</returns>
    public static bool IsDigit(char ch) => ch >= '0' && ch <= '9';

    /// <summary>
    /// Checks if provided character is a special character.
    /// </summary>
    /// <param name="ch">Character to check.</param>
    /// <returns><see langword="true" /> if character is a special character, <see langword="false" /> otherwise.</returns>
    public static bool IsSpecialCharacter(char ch) => ch == '[' || ch == ']' || ch == '#';

    /// <summary>
    /// Checks if provided string is a keyword.
    /// </summary>
    /// <param name="str">String to check.</param>
    /// <returns><see langword="true" /> if string is a keyword, <see langword="false" /> otherwise.</returns>
    public static bool IsKeyword(string str) => str is not null && _keywords.ContainsKey(str);

    /// <summary>
    /// Checks if provided token type is atomic.
    /// </summary>
    /// <param name="type">Token type to check.</param>
    /// <returns><see langword="true" /> if token type is atomic, <see langword="false" /> otherwise.</returns>
    private static bool IsAtomic(TokenType type) => type == TokenType.LeftSquare || type == TokenType.RightSquare;

    /// <summary>
    /// Merges numeric values.
    /// </summary>
    /// <param name="currentValue">Current numeric value.</param>
    /// <param name="tokenType">Merged token type.</param>
    /// <param name="nextToken">Token to merge.</param>
    /// <returns>Merged numeric value.</returns>
    private static int MergeNumbers(int currentValue, TokenType tokenType, Token nextToken) => tokenType switch
    {
        TokenType.Number => nextToken.Value.Length * 10 * currentValue + nextToken.NumericValue,
        TokenType.Percentage => nextToken.Value.Length > 0 ? (nextToken.Value.Length - 1) * 10 * currentValue + nextToken.NumericValue : currentValue,
        _ => 0
    };

    /// <summary>
    /// Merges token types.
    /// </summary>
    /// <param name="tokenType">Current token type.</param>
    /// <param name="nextToken">Token to merge.</param>
    /// <returns>Merged token type.</returns>
    private static TokenType MergeTypes(TokenType tokenType, Token nextToken) => tokenType switch
    {
        TokenType.Number => nextToken.Value.Length == 1 && nextToken.Value[0] == '%' ? TokenType.Percentage : nextToken.Type,
        TokenType.Variable => TokenType.Variable,
        _ => nextToken.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text
    };
    #endregion

    #region Fields and Properties
    /// <summary>
    /// Contains tokenized source code.
    /// </summary>
    public string Source { get; private set; }

    /// <summary>
    /// Contains script arguments.
    /// </summary>
    public ArraySegment<string> Arguments { get; private set; }

    /// <summary>
    /// Contains command sender for permissions guards evaluation.
    /// </summary>
    public ICommandSender Sender { get; private set; }

    /// <summary>
    /// Contains permissions resolved used for permissions guards evaluation.
    /// </summary>
    public IPermissionsResolver PermissionsResolver { get; private set; }

    /// <summary>
    /// Contains current line number.
    /// </summary>
    public int Line { get; private set; }

    /// <summary>
    /// Contains current error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// <see langword="true" /> if end of source code was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd => _current >= Source.Length;

    /// <summary>
    /// Current character getter with bounds checking.
    /// </summary>
    private char Current => IsAtEnd ? '\0' : Source[_current];

    /// <summary>
    /// Tells whether or not the lexer can continue reading the same line.
    /// </summary>
    private bool CanRead => Source[_current] != '\n' || Source[_current - 1] == '\\' || (Source[_current - 1] == '\r' && Source[_current - 2] == '\\');

    /// <summary>
    /// Tells whether or not the current character is not a line extension.
    /// </summary>
    private bool IsNotLineExtend => Source[_current] != '\\' || (_current + 1 < Source.Length && Source[_current + 1] != '\n' && Source[_current + 1] != '\r');

    /// <summary>
    /// <see langword="true" /> if its top level tokenizer, <see langword="false" /> otherwise.
    /// </summary>
    private bool IsTopLevel => _argResults is not null;

    /// <summary>
    /// Tells whether or not the command sender has missing permission.
    /// </summary>
    private bool _hasMissingPerms;

    /// <summary>
    /// Contains a list to output found tokens into.
    /// </summary>
    private readonly List<Token> _tokens;

    /// <summary>
    /// Caches provided arguments processing results.
    /// </summary>
    private readonly Dictionary<int, ArgResult> _argResults;

    /// <summary>
    /// Contains lexer instance used for arguments processing.
    /// </summary>
    private Lexer _argLexer;

    /// <summary>
    /// Contains current token start index.
    /// </summary>
    private int _start;

    /// <summary>
    /// Contains current character index.
    /// </summary>
    private int _current;

    /// <summary>
    /// Contains a prefix to use in arguments merging.
    /// </summary>
    private string _prefix;

    /// <summary>
    /// Contains current numeric value.
    /// </summary>
    private int _numericValue;
    #endregion

    #region State Management
    /// <summary>
    /// Creates new lexer instance.
    /// </summary>
    /// <param name="source">Source code to tokenize.</param>
    /// <param name="arguments">Script arguments to inject.</param>
    /// <param name="sender">Command sender to use.</param>
    /// <param name="resolver">Permissions resolver to use.</param>
    public Lexer(string source, ArraySegment<string> arguments, ICommandSender sender, IPermissionsResolver resolver = null)
    {
        _tokens = [];
        _argResults = [];
        _argLexer = null;
        Reset(source, arguments, sender, resolver);
    }

    /// <summary>
    /// Creates new lexer instance.
    /// </summary>
    private Lexer()
    {
        Source = string.Empty;
        Arguments = new();
        Sender = null;
        PermissionsResolver = null;
        _tokens = [];
        _argResults = null;
        _argLexer = null;
        Reset();
    }

    /// <summary>
    /// Tokenizes next source code line.
    /// </summary>
    /// <returns>List with processed tokens.</returns>
    public IList<Token> ScanNextLine()
    {
        _tokens.Clear();

        if (ErrorMessage is not null || IsAtEnd)
        {
            return _tokens;
        }

        ++Line;
        var canRead = true;

        while (!IsAtEnd && canRead && ErrorMessage is null)
        {
            _start = _current;
            canRead = ScanToken();
        }

        return _tokens;
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    public void Reset()
    {
        Line = 0;
        _hasMissingPerms = false;
        _start = 0;
        _current = 0;
        ErrorMessage = null;
        _prefix = string.Empty;
        _numericValue = 0;
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    public void Reset(string source)
    {
        Source = source ?? string.Empty;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="arguments">New script arguments to inject.</param>
    public void Reset(ArraySegment<string> arguments)
    {
        Arguments = arguments;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(ICommandSender sender)
    {
        Sender = sender;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(IPermissionsResolver resolver)
    {
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="arguments">New script arguments to inject.</param>
    public void Reset(string source, ArraySegment<string> arguments)
    {
        Source = source ?? string.Empty;
        Arguments = arguments;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(string source, ICommandSender sender)
    {
        Source = source ?? string.Empty;
        Sender = sender;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(string source, IPermissionsResolver resolver)
    {
        Source = source ?? string.Empty;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="arguments">New script arguments to inject.</param>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(ArraySegment<string> arguments, ICommandSender sender)
    {
        Arguments = arguments;
        Sender = sender;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="arguments">New script arguments to inject.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(ArraySegment<string> arguments, IPermissionsResolver resolver)
    {
        Arguments = arguments;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="sender">New command sender to use.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(ICommandSender sender, IPermissionsResolver resolver)
    {
        Sender = sender;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="arguments">New script arguments to inject.</param>
    /// <param name="sender">New command sender to use.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(ArraySegment<string> arguments, ICommandSender sender, IPermissionsResolver resolver)
    {
        Arguments = arguments;
        Sender = sender;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="sender">New command sender to use.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(string source, ICommandSender sender, IPermissionsResolver resolver)
    {
        Source = source ?? string.Empty;
        Sender = sender;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="arguments">New script arguments to inject.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(string source, ArraySegment<string> arguments, IPermissionsResolver resolver)
    {
        Source = source ?? string.Empty;
        Arguments = arguments;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="arguments">New script arguments to inject.</param>
    /// <param name="sender">New command sender to use.</param>
    public void Reset(string source, ArraySegment<string> arguments, ICommandSender sender)
    {
        Source = source ?? string.Empty;
        Arguments = arguments;
        Sender = sender;
        Reset();
    }

    /// <summary>
    /// Resets the tokenization process.
    /// </summary>
    /// <param name="source">New source code to tokenize.</param>
    /// <param name="arguments">New script arguments to inject.</param>
    /// <param name="sender">New command sender to use.</param>
    /// <param name="resolver">New permissions resolver to use.</param>
    public void Reset(string source, ArraySegment<string> arguments, ICommandSender sender, IPermissionsResolver resolver)
    {
        Source = source ?? string.Empty;
        Arguments = arguments;
        Sender = sender;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        Reset();
    }

    /// <summary>
    /// Scans a beggining of next token.
    /// </summary>
    /// <returns><see langword="true" /> if reading can continue, <see langword="false" /> otherwise.</returns>
    private bool ScanToken()
    {
        var ch = Advance();

        switch (ch)
        {
            case '[':
                Directive(TokenType.LeftSquare);
                break;
            case ']':
                Directive(TokenType.RightSquare);
                break;
            case '#':
                Guard();
                break;
            case '\\':
                LineExtend();
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                return !IsTopLevel;
            default:
                Text(true);
                break;
        }

        return true;
    }

    /// <summary>
    /// Retrieves current character and moves the index forward.
    /// </summary>
    /// <returns>Currently parsed character.</returns>
    private char Advance() => Source[_current++];

    /// <summary>
    /// Adds new token to tokens list using current token as value.
    /// </summary>
    /// <param name="type">Type of token to add.</param>
    private void AddToken(TokenType type) => AddToken(type, Source.Substring(_start, _current - _start));

    /// <summary>
    /// Adds new token to tokens list using specific value.
    /// </summary>
    /// <param name="type">Type of token to add.</param>
    /// <param name="text">Token value to assign.</param>
    /// <param name="value">Numeric token value to assign.</param>
    private void AddToken(TokenType type, string text, int value = 0) => _tokens.Add(new(type, text, value));

    /// <summary>
    /// Merges current token with prefix if it exists.
    /// </summary>
    /// <param name="current">Index number to use as token end.</param>
    /// <returns>Current string token with attached prefix.</returns>
    private string GetTextWithPrefix(int current)
    {
        string text;

        if (_prefix.Length > 0)
        {
            text = _prefix + Source.Substring(_start, current - _start);
            _prefix = string.Empty;
        }
        else
        {
            text = Source.Substring(_start, current - _start);
        }

        return text;
    }

    /// <summary>
    /// Checks if current character matches the expected one.
    /// </summary>
    /// <param name="expected">Character to compare with.</param>
    /// <returns><see langword="true" /> if expected character was matched, <see langword="false" /> otherwise.</returns>
    private bool Match(char expected)
    {
        if (IsAtEnd)
        {
            return false;
        }

        if (Source[_current] != expected)
        {
            return false;
        }

        ++_current;
        return true;
    }
    #endregion

    #region Tokens Processing
    /// <summary>
    /// Processes directive start or end.
    /// </summary>
    /// <param name="type">Type of token to use.</param>
    private void Directive(TokenType type)
    {
        if (_hasMissingPerms)
        {
            SkipUntilGuard();
        }
        else
        {
            AddToken(type);
        }
    }

    /// <summary>
    /// Processes comments, permissions guards or scope guards.
    /// </summary>
    private void Guard()
    {
        if (!IsTopLevel)
        {
            Text(false);
            return;
        }

        var action = Skip;

        if (Match('!'))
        {
            _hasMissingPerms = false;
            action = Permission;
        }
        else if (Match('?'))
        {
            AddToken(TokenType.ScopeGuard);
            action = Identifier;
        }

        while (!IsAtEnd && CanRead)
        {
            if (!IsWhiteSpace(Source[_current]) && IsNotLineExtend)
            {
                action();

                if (ErrorMessage is not null)
                {
                    return;
                }
            }
            else
            {
                if (Source[_current] == '\n')
                {
                    ++Line;
                }

                ++_current;
            }
        }
    }

    /// <summary>
    /// Processes line extensions.
    /// </summary>
    private void LineExtend()
    {
        if (!IsWhiteSpace(Current))
        {
            if (IsSpecialCharacter(Source[_current]))
            {
                ++_current;
            }

            ++_start;
            Text(false);
            return;
        }

        if (IsTopLevel)
        {
            Match('\r');

            if (Match('\n'))
            {
                ++Line;
            }
        }
    }

    /// <summary>
    /// Processes text based tokens.
    /// </summary>
    /// <param name="enableKeywords">Whether or not the keywords are enabled.</param>
    private void Text(bool enableKeywords)
    {
        if (_hasMissingPerms)
        {
            SkipUntilGuard();
            return;
        }

        if (enableKeywords)
        {
            --_current;
        }

        var type = _start == _current && IsDigit(Current) ? TokenType.Number : TokenType.Text;
        
        while (!IsWhiteSpace(Current) && !IsSpecialCharacter(Source[_current]))
        {
            type = type switch
            {
                TokenType.Number => ProcessNumeric(true),
                TokenType.Percentage => ProcessNumeric(false),
                _ => ProcessText(type)
            };

            if (ErrorMessage is not null || type == TokenType.None)
            {
                return;
            }
        }

        var text = GetTextWithPrefix(_current);
        AddToken(enableKeywords && type == TokenType.Text && _keywords.ContainsKey(text) ? _keywords[text] : type, text, _numericValue);
        _numericValue = 0;
    }

    /// <summary>
    /// Skips current token.
    /// </summary>
    private void Skip()
    {
        do
        {
            ++_current;
        }
        while (!IsWhiteSpace(Current));
    }

    /// <summary>
    /// Processes current token as an identifier.
    /// </summary>
    private void Identifier()
    {
        _start = _current;
        Skip();
        AddToken(TokenType.Text);
    }

    /// <summary>
    /// Processes current token as a permission.
    /// </summary>
    private void Permission()
    {
        _start = _current;
        Skip();

        if (!_hasMissingPerms)
        {
            _hasMissingPerms = !PermissionsResolver.CheckPermission(Sender, Source.Substring(_start, _current - _start), out var message);
            ErrorMessage = message;
        }
    }

    /// <summary>
    /// Skips current token until a comment start is found.
    /// </summary>
    private void SkipUntilGuard()
    {
        while (!IsWhiteSpace(Current) && (Source[_current] != '#' || Source[_current - 1] == '\\'))
        {
            ++_current;
        }
    }

    /// <summary>
    /// Processes numeric value.
    /// </summary>
    /// <param name="isPureNumber">Whether or not the value is a pure number.</param>
    /// <returns>Token type after processing.</returns>
    private TokenType ProcessNumeric(bool isPureNumber)
    {
        if (isPureNumber && IsDigit(Source[_current]))
        {
            _numericValue *= 10;
            _numericValue += Source[_current] - '0';
            ++_current;
            return TokenType.Number;
        }

        ++_current;

        switch (Source[_current - 1])
        {
            case '$':
                if (Match('(') && !Match(')'))
                {
                    var type = Argument(_current - 2, isPureNumber ? TokenType.Number : TokenType.Percentage);

                    if (type != TokenType.Number && type != TokenType.Percentage)
                    {
                        _numericValue = 0;
                    }

                    return type;
                }

                _numericValue = 0;
                return TokenType.Text;
            case '\\':
                if (IsSpecialCharacter(Current))
                {
                    _prefix = GetTextWithPrefix(_current - 1);
                    _start = _current++;
                }

                _numericValue = 0;
                return TokenType.Text;
            case '%':
                if (isPureNumber)
                {
                    return TokenType.Percentage;
                }

                _numericValue = 0;
                return TokenType.Text;
            default:
                _numericValue = 0;
                return TokenType.Text;
        }
    }

    /// <summary>
    /// Processes text value.
    /// </summary>
    /// <param name="type">Current token type.</param>
    /// <returns>Token type after processing.</returns>
    private TokenType ProcessText(TokenType type)
    {
        ++_current;

        switch (Source[_current - 1])
        {
            case '$':
                if (Match('(') && !Match(')'))
                {
                    return Argument(_current - 2, type);
                }

                return type;
            case '\\':
                if (IsSpecialCharacter(Current))
                {
                    _prefix = GetTextWithPrefix(_current - 1);
                    _start = _current++;
                }

                return type;
            default:
                return type;
        }
    }
    #endregion

    #region Arguments Processing
    /// <summary>
    /// Processes a potential variable.
    /// </summary>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType Argument(int startedAt, TokenType type)
    {
        if (IsTopLevel)
        {
            var argNum = 0;

            while (IsDigit(Current))
            {
                argNum *= 10;
                argNum += Source[_current] - '0';
                ++_current;
            }

            if (Match(')'))
            {
                return ProcessArgument(argNum, startedAt, type);
            }
        }

        while (!IsWhiteSpace(Current) && Source[_current] != ')')
        {
            ++_current;
        }

        if (Match(')'))
        {
            return TokenType.Variable;
        }

        return type == TokenType.Variable ? TokenType.Variable : TokenType.Text;
    }

    /// <summary>
    /// Processes a script argument.
    /// </summary>
    /// <param name="argNum">Number of processed argument.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType ProcessArgument(int argNum, int startedAt, TokenType type)
    {
        ValidateArgs(argNum);

        if (ErrorMessage is not null)
        {
            return TokenType.None;
        }

        if (_argResults.ContainsKey(argNum))
        {
            return InjectArg(_argResults[argNum], startedAt, type);
        }

        _argLexer ??= new();
        _argLexer.Reset(Arguments.Array[Arguments.Offset + argNum - 1]);
        _argLexer.ScanNextLine();
        var result = new ArgResult(_argLexer.Source, [.._argLexer._tokens]);
        _argResults[argNum] = result;

        if (_argLexer.ErrorMessage is not null)
        {
            ErrorMessage = $"{_argLexer.ErrorMessage}\nat $({argNum})";
            return TokenType.None;
        }

        return InjectArg(result, startedAt, type);
    }

    /// <summary>
    /// Checks if provided arguments are valid.
    /// </summary>
    /// <param name="argNum">Number of processed argument.</param>
    private void ValidateArgs(int argNum)
    {
        if (Arguments.Array is null)
        {
            ErrorMessage = $"Invalid argument $({argNum}), provided arguments array is null";
            return;
        }

        if (Arguments.Offset < 1)
        {
            ErrorMessage = $"Invalid argument $({argNum}), provided arguments array has incorrect offset ({Arguments.Offset})";
            return;
        }

        if (argNum > Arguments.Count)
        {
            ErrorMessage = $"Missing argument $({argNum}), sender provided only {Arguments.Count} arguments";
        }
    }

    /// <summary>
    /// Injects argument content in place of current variable.
    /// </summary>
    /// <param name="result">Result of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType InjectArg(ArgResult result, int startedAt, TokenType type) => result.Tokens.Count switch
    {
        0 => InjectNoTokensArg(result.Source.Length < 1, startedAt, type),
        1 => Inject1TokenArg(result, startedAt, type),
        2 => Inject2TokensArg(result, startedAt, type),
        _ => InjectNTokensArg(result, startedAt, type)
    };
    #endregion

    #region Arguments Injection
    /// <summary>
    /// Injects empty argument in place of currect variable.
    /// </summary>
    /// <param name="isEmpty">Whether or not the argument is empty.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType InjectNoTokensArg(bool isEmpty, int startedAt, TokenType type)
    {
        if (_start == startedAt)
        {
            return TokenType.None;
        }

        if (isEmpty && !IsWhiteSpace(Current))
        {
            _prefix = GetTextWithPrefix(startedAt);
            _start = _current;
            return type;
        }

        AddToken(type, GetTextWithPrefix(startedAt), _numericValue);
        return TokenType.None;
    }

    /// <summary>
    /// Injects arguments content in place of current variable.
    /// </summary>
    /// <param name="result">Result of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType Inject1TokenArg(ArgResult result, int startedAt, TokenType type)
    {
        var token = result.Tokens[0];
        var isEnd = IsWhiteSpace(Current) || IsAtomic(token.Type) || IsWhiteSpace(result.Source[result.Source.Length - 1]);

        if (_start != startedAt)
        {
            if (IsWhiteSpace(result.Source[0]) || IsAtomic(token.Type))
            {
                AddToken(type, GetTextWithPrefix(startedAt), _numericValue);
            }
            else if (isEnd)
            {
                var newType = MergeTypes(type, token);
                AddToken(newType, GetTextWithPrefix(startedAt) + token.Value, MergeNumbers(_numericValue, newType, token));
                return TokenType.None;
            }
            else
            {
                var newType = MergeTypes(type, token);
                _prefix = GetTextWithPrefix(startedAt) + token.Value;
                _numericValue = MergeNumbers(_numericValue, newType, token);
                _start = _current;
                return newType;
            }
        }

        if (isEnd)
        {
            AddToken(token.Type, token.Value, token.NumericValue);
            return TokenType.None;
        }

        _prefix = token.Value;
        _numericValue = token.NumericValue;
        _start = _current;
        return token.Type;
    }

    /// <summary>
    /// Injects arguments content in place of current variable.
    /// </summary>
    /// <param name="result">Result of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType Inject2TokensArg(ArgResult result, int startedAt, TokenType type)
    {
        var token = result.Tokens[0];

        if (_start != startedAt)
        {
            if (IsWhiteSpace(result.Source[0]) || IsAtomic(token.Type))
            {
                AddToken(type, GetTextWithPrefix(startedAt), _numericValue);
                AddToken(token.Type, token.Value, token.NumericValue);
            }
            else
            {
                var newType = MergeTypes(type, token);
                AddToken(newType, GetTextWithPrefix(startedAt) + token.Value, MergeNumbers(_numericValue, newType, token));
            }
        }
        else
        {
            AddToken(token.Type, token.Value, token.NumericValue);
        }

        var lastToken = result.Tokens[1];

        if (IsWhiteSpace(Current) || IsAtomic(lastToken.Type) || IsWhiteSpace(result.Source[result.Source.Length - 1]))
        {
            AddToken(lastToken.Type, lastToken.Value, lastToken.NumericValue);
            return TokenType.None;
        }

        _prefix = lastToken.Value;
        _numericValue = lastToken.NumericValue;
        _start = _current;
        return lastToken.Type;
    }

    /// <summary>
    /// Injects arguments content in place of current variable.
    /// </summary>
    /// <param name="result">Result of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="type">Current token type.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType InjectNTokensArg(ArgResult result, int startedAt, TokenType type)
    {
        IEnumerable<Token> tokens = result.Tokens;
        var isEnd = IsWhiteSpace(Current) || IsAtomic(result.Tokens[result.Tokens.Count - 1].Type) || IsWhiteSpace(result.Source[result.Source.Length - 1]);

        if (!isEnd)
        {
            tokens = tokens.Take(result.Tokens.Count - 1);
        }

        if (_start != startedAt)
        {
            if (IsWhiteSpace(result.Source[0]) || IsAtomic(result.Tokens[0].Type))
            {
                AddToken(type, GetTextWithPrefix(startedAt), _numericValue);
            }
            else
            {
                var token = result.Tokens[0];
                var newType = MergeTypes(type, token);
                AddToken(newType, GetTextWithPrefix(startedAt) + token.Value, MergeNumbers(_numericValue, newType, token));
                tokens = tokens.Skip(1);
            }
        }

        foreach (var token in tokens)
        {
            AddToken(token.Type, token.Value, token.NumericValue);
        }

        if (isEnd)
        {
            return TokenType.None;
        }

        var lastToken = result.Tokens[result.Tokens.Count - 1];
        _prefix = lastToken.Value;
        _numericValue = lastToken.NumericValue;
        _start = _current;
        return lastToken.Type;
    }
    #endregion
}
