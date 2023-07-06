using System.Collections.Generic;
using System;
using CommandSystem;
using SLCommandScript.Core.Interfaces;
using SLCommandScript.Core.Permissions;
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
        { "foreach", TokenType.Foreach }
    };

    /// <summary>
    /// Checks if provided character is a whitespace.
    /// </summary>
    /// <param name="ch">Character to check.</param>
    /// <returns><see langword="true" /> if character is a whitespace, <see langword="false" /> otherwise.</returns>
    private static bool IsWhiteSpace(char ch) => ch == '\0' || char.IsWhiteSpace(ch);

    /// <summary>
    /// Checks if provided character is an alpha char.
    /// </summary>
    /// <param name="ch">Character to check.</param>
    /// <returns><see langword="true" /> if character is alpha, <see langword="false" /> otherwise.</returns>
    private static bool IsAlpha(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

    /// <summary>
    /// Checks if provided character is a digit.
    /// </summary>
    /// <param name="ch">Character to check.</param>
    /// <returns><see langword="true" /> if character is a digit, <see langword="false" /> otherwise.</returns>
    private static bool IsDigit(char ch) => ch >= '0' && ch <= '9';
    #endregion

    #region Fields and Properties
    /// <summary>
    /// Contains tokenized source code.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Contains script arguments.
    /// </summary>
    public ArraySegment<string> Arguments
    {
        get => _arguments;
        private set
        {
            _argLexers.Clear();
            _arguments = value;
        }
    }

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
    /// Contains a list with processed tokens.
    /// </summary>
    public IList<Token> Tokens => _tokens;

    /// <summary>
    /// Current character getter with bounds checking.
    /// </summary>
    private char Current => IsAtEnd ? '\0' : Source[_current];

    /// <summary>
    /// Tells whether or not the lexer can continue reading the same line.
    /// </summary>
    private bool CanRead => Source[_current] != '\n' || Source[_current - 1] == '\\' || (Source[_current - 1] == '\r' && Source[_current - 2] == '\\');

    /// <summary>
    /// <see langword="true" /> if its top level tokenizer, <see langword="false" /> otherwise.
    /// </summary>
    private bool IsTopLevel => _argLexers is not null;

    /// <summary>
    /// Contains script arguments.
    /// </summary>
    private ArraySegment<string> _arguments;

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
    private readonly Dictionary<int, Lexer> _argLexers;

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
    #endregion

    #region State Management
    /// <summary>
    /// Creates new lexer instance.
    /// </summary>
    /// <param name="source">Source code to tokenize.</param>
    /// <param name="arguments">Script arguments to inject.</param>
    /// <param name="">Command sender to use for permissions guards evaluation.</param>
    /// <param name="">Permissions resolver to use for permissions guards evaluation.</param>
    public Lexer(string source, ArraySegment<string> arguments, ICommandSender sender, IPermissionsResolver resolver = null)
    {
        Source = source ?? string.Empty;
        _arguments = arguments;
        Sender = sender;
        PermissionsResolver = resolver ?? new VanillaPermissionsResolver();
        _tokens = new();
        _argLexers = new();
        Reset();
    }

    /// <summary>
    /// Creates new lexer instance.
    /// </summary>
    /// <param name="source">Source code to tokenize.</param>
    /// <param name="line">Line number to use.</param>
    private Lexer(string source)
    {
        Source = source ?? string.Empty;
        _arguments = new();
        Sender = null;
        PermissionsResolver = null;
        _tokens = new();
        _argLexers = null;
        Reset();
    }

    /// <summary>
    /// Tokenizes next source code line.
    /// </summary>
    /// <returns>List of all tokens found in processed line.</returns>
    public IList<Token> ScanNextLine()
    {
        _tokens.Clear();
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
                Text();
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
    private void AddToken(TokenType type, string text) => _tokens.Add(new(type, text, Line));

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
        if (IsWhiteSpace(Current) && !_hasMissingPerms)
        {
            AddToken(type);
        }
        else
        {
            Text();
        }
    }

    /// <summary>
    /// Processes comments, permissions guards or scope guards.
    /// </summary>
    private void Guard()
    {
        if (!IsTopLevel)
        {
            Text();
            return;
        }

        var skip = true;

        if (Match('!'))
        {
            PermissionsGuard();
            return;
        }
        else if (Match('?'))
        {
            AddToken(TokenType.ScopeGuard, null);
            skip = false;
        }

        while (!IsAtEnd && CanRead)
        {
            if (Source[_current] == '\n')
            {
                ++Line;
            }

            if (skip || !IsAlpha(Source[_current]))
            {
                Advance();
            }
            else
            {
                _start = _current;
                Identifier();
            }
        }
    }

    /// <summary>
    /// Processes permissions guards.
    /// </summary>
    private void PermissionsGuard()
    {
        _hasMissingPerms = false;

        while (!IsAtEnd && CanRead)
        {
            if (Source[_current] == '\n')
            {
                ++Line;
            }
            else if (!_hasMissingPerms && IsAlpha(Source[_current]))
            {
                _start = _current;

                do
                {
                    Advance();
                }
                while (IsAlpha(Current));

                _hasMissingPerms = !PermissionsResolver.CheckPermission(Sender, Source.Substring(_start, _current - _start), out var message);

                if (message is not null)
                {
                    ErrorMessage = message;
                    return;
                }
            }

            Advance();
        }
    }

    /// <summary>
    /// Processes line extensions.
    /// </summary>
    private void LineExtend()
    {
        if (IsTopLevel)
        {
            var matched = Match('\r');

            if (Match('\n'))
            {
                ++Line;
                return;
            }

            if (matched)
            {
                --_current;
            }
        }

        Text();
    }

    /// <summary>
    /// Processes text based tokens.
    /// </summary>
    private void Text()
    {
        if (_hasMissingPerms)
        {
            Skip();
            return;
        }

        var type = TokenType.Text;
        
        while (!IsWhiteSpace(Current))
        {
            if (Source[_current - 1] == '$' && Match('(') && !Match(')'))
            {
                type = Argument(_current - 2, type == TokenType.Variable);
            }
            else
            {
                Advance();
            }

            if (ErrorMessage is not null || type == TokenType.None)
            {
                return;
            }
        }
        
        var text = GetTextWithPrefix(_current);
        AddToken(type == TokenType.Text && _keywords.ContainsKey(text) ? _keywords[text] : type, text);
    }

    /// <summary>
    /// Processes identifiers.
    /// </summary>
    private void Identifier()
    {
        while (IsAlpha(Current))
        {
            Advance();
        }

        AddToken(TokenType.Identifier);
    }

    /// <summary>
    /// Skips current token.
    /// </summary>
    private void Skip()
    {
        while (!IsWhiteSpace(Current))
        {
            Advance();
        }
    }
    #endregion

    #region Arguments Processing
    /// <summary>
    /// Processes a potential variable.
    /// </summary>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType Argument(int startedAt, bool isVarText)
    {
        if (IsTopLevel)
        {
            var argNum = 0;

            while (IsDigit(Current))
            {
                argNum *= 10;
                argNum += Source[_current] - '0';
                Advance();
            }

            if (Match(')'))
            {
                return ProcessArgument(argNum, startedAt, isVarText);
            }
        }

        while (IsAlpha(Current))
        {
            Advance();
        }

        if (Match(')'))
        {
            return TokenType.Variable;
        }

        return isVarText ? TokenType.Variable : TokenType.Text;
    }

    /// <summary>
    /// Processes a script argument.
    /// </summary>
    /// <param name="argNum">Number of processed argument.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType ProcessArgument(int argNum, int startedAt, bool isVarText)
    {
        ValidateArgs(argNum);

        if (ErrorMessage is not null)
        {
            return TokenType.Text;
        }

        if (_argLexers.ContainsKey(argNum))
        {
            return InjectArg(_argLexers[argNum], startedAt, isVarText);
        }

        var lexer = new Lexer(_arguments.Array[_arguments.Offset + argNum - 1]);
        _argLexers[argNum] = lexer;
        lexer.ScanNextLine();

        if (lexer.ErrorMessage is not null)
        {
            ErrorMessage = $"{lexer.ErrorMessage}\nat $({argNum})";
            return TokenType.Text;
        }

        return InjectArg(lexer, startedAt, isVarText);
    }

    /// <summary>
    /// Checks if provided arguments are valid.
    /// </summary>
    /// <param name="argNum">Number of processed argument.</param>
    private void ValidateArgs(int argNum)
    {
        if (_arguments.Array is null)
        {
            ErrorMessage = $"Invalid argument $({argNum}), provided arguments array is null";
            return;
        }

        if (_arguments.Offset < 1)
        {
            ErrorMessage = $"Invalid argument $({argNum}), provided arguments array has incorrect offset ({_arguments.Offset})";
            return;
        }

        if (argNum > _arguments.Count)
        {
            ErrorMessage = $"Missing argument $({argNum}), sender provided only {_arguments.Count} arguments";
        }
    }

    /// <summary>
    /// Injects argument content in place of current variable.
    /// </summary>
    /// <param name="lexer">Lexer of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType InjectArg(Lexer lexer, int startedAt, bool isVarText) => lexer._tokens.Count switch
    {
        0 => InjectNoTokensArg(lexer.Source.Length < 1, startedAt, isVarText),
        1 => Inject1TokenArg(lexer, startedAt, isVarText),
        2 => Inject2TokensArg(lexer, startedAt, isVarText),
        _ => InjectNTokensArg(lexer, startedAt, isVarText)
    };
    #endregion

    #region Arguments Injection
    /// <summary>
    /// Injects empty argument in place of currect variable.
    /// </summary>
    /// <param name="isEmpty">Whether or not the argument is empty.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType InjectNoTokensArg(bool isEmpty, int startedAt, bool isVarText)
    {
        if (_start == startedAt)
        {
            return TokenType.None;
        }

        if (isEmpty && !IsWhiteSpace(Current))
        {
            _prefix = GetTextWithPrefix(startedAt);
            _start = _current;
            return isVarText ? TokenType.Variable : TokenType.Text;
        }

        AddToken(isVarText ? TokenType.Variable : TokenType.Text, GetTextWithPrefix(startedAt));
        return TokenType.None;
    }

    /// <summary>
    /// Injects arguments content in place of current variable.
    /// </summary>
    /// <param name="lexer">Lexer of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType Inject1TokenArg(Lexer lexer, int startedAt, bool isVarText)
    {
        var token = lexer._tokens[0];
        var isEnd = IsWhiteSpace(Current) || IsWhiteSpace(lexer.Source[lexer.Source.Length - 1]);

        if (_start != startedAt)
        {
            var type = isVarText || token.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text;

            if (IsWhiteSpace(lexer.Source[0]))
            {
                AddToken(isVarText ? TokenType.Variable : TokenType.Text, GetTextWithPrefix(startedAt));
            }
            else if (isEnd)
            {
                AddToken(type, GetTextWithPrefix(startedAt) + token.Value);
                return TokenType.None;
            }
            else
            {
                _prefix = GetTextWithPrefix(startedAt) + token.Value;
                _start = _current;
                return type;
            }
        }

        if (isEnd)
        {
            AddToken(token.Type, token.Value);
            return TokenType.None;
        }

        _prefix = token.Value;
        _start = _current;
        return token.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text;
    }

    /// <summary>
    /// Injects arguments content in place of current variable.
    /// </summary>
    /// <param name="lexer">Lexer of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType Inject2TokensArg(Lexer lexer, int startedAt, bool isVarText)
    {
        var token = lexer._tokens[0];

        if (_start != startedAt)
        {
            if (IsWhiteSpace(lexer.Source[0]))
            {
                AddToken(isVarText ? TokenType.Variable : TokenType.Text, GetTextWithPrefix(startedAt));
                AddToken(token.Type, token.Value);
            }
            else
            {
                var type = isVarText || token.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text;
                AddToken(type, GetTextWithPrefix(startedAt) + token.Value);
            }
        }
        else
        {
            AddToken(token.Type, token.Value);
        }

        var lastToken = lexer._tokens[1];

        if (IsWhiteSpace(Current) || IsWhiteSpace(lexer.Source[lexer.Source.Length - 1]))
        {
            AddToken(lastToken.Type, lastToken.Value);
            return TokenType.None;
        }

        _prefix = lastToken.Value;
        _start = _current;
        return lastToken.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text;
    }

    /// <summary>
    /// Injects arguments content in place of current variable.
    /// </summary>
    /// <param name="lexer">Lexer of argument to inject.</param>
    /// <param name="startedAt">Start index of processed variable.</param>
    /// <param name="isVarText">Whether or not current token contains other variables.</param>
    /// <returns>Type of token to use in remaining token processing.</returns>
    private TokenType InjectNTokensArg(Lexer lexer, int startedAt, bool isVarText)
    {
        IEnumerable<Token> tokens = lexer._tokens;
        var isEnd = IsWhiteSpace(Current) || IsWhiteSpace(lexer.Source[lexer.Source.Length - 1]);

        if (!isEnd)
        {
            tokens = tokens.Take(lexer._tokens.Count - 1);
        }

        if (_start != startedAt)
        {
            if (IsWhiteSpace(lexer.Source[0]))
            {
                AddToken(isVarText ? TokenType.Variable : TokenType.Text, GetTextWithPrefix(startedAt));
            }
            else
            {
                var token = lexer._tokens[0];
                var type = isVarText || token.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text;
                AddToken(type, GetTextWithPrefix(startedAt) + token.Value);
                tokens = tokens.Skip(1);
            }
        }

        foreach (var token in tokens)
        {
            AddToken(token.Type, token.Value);
        }

        if (isEnd)
        {
            return TokenType.None;
        }

        var lastToken = lexer._tokens[lexer._tokens.Count - 1];
        _prefix = lastToken.Value;
        _start = _current;
        return lastToken.Type == TokenType.Variable ? TokenType.Variable : TokenType.Text;
    }
    #endregion
}
