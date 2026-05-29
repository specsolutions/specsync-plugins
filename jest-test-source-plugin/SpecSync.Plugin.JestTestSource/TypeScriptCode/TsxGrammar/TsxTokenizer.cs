namespace SpecSync.Plugin.JestTestSource.TypeScriptCode.TsxGrammar;

public enum TsxTokenKind
{
    Unknown,
    EndOfFile,

    Whitespace,
    NewLine,

    LineComment,
    BlockComment,

    Identifier,
    Keyword,
    Number,

    StringLiteral,
    TemplateLiteral,

    OpenParen,
    CloseParen,
    OpenBrace,
    CloseBrace,
    OpenBracket,
    CloseBracket,

    Dot,
    Comma,
    Colon,
    Semicolon,
    Question,

    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    Equals,
    LessThan,
    GreaterThan,
    Bang,
    Ampersand,
    Pipe,
    Caret,
    Tilde,

    Arrow,
    EqualsEquals,
    EqualsEqualsEquals,
    BangEquals,
    BangEqualsEquals,
    AmpersandAmpersand,
    PipePipe,

    OtherPunctuation
}

public struct TsxToken(TsxTokenKind kind, int start, int length, string text, int line, int column)
{
    public TsxTokenKind Kind { get; } = kind;
    public int Start { get; } = start;
    public int Length { get; } = length;
    public int End => Start + Length;
    public string Text { get; } = text;
    public int Line { get; } = line;
    public int Column { get; } = column;
    private TsxToken[]? _commentTokens = null;
    public IReadOnlyList<TsxToken> CommentTokens => _commentTokens ?? [];

    public void AddCommentTokens(IEnumerable<TsxToken> tokens)
    {
        _commentTokens ??= tokens.ToArray();
    }

    public override string ToString()
    {
        return $"{Kind} [{Line},{Column}] '{Text}'";
    }
}

public sealed class TsxTokenizer(string? text)
{
    private readonly string _text = text ?? string.Empty;
    private int _pos;
    private int _line = 1;
    private int _column = 1;

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "const", "let", "var", "function", "return", "if", "else", "for", "while",
        "do", "switch", "case", "default", "break", "continue", "class", "extends",
        "implements", "interface", "type", "enum", "import", "export", "from",
        "new", "try", "catch", "finally", "throw", "async", "await", "yield",
        "true", "false", "null", "undefined", "this", "super"
    };

    public IEnumerable<TsxToken> Tokenize(bool includeWhitespace = false, bool includeCommentsAsSeparateTokens = false)
    {
        List<TsxToken> comments = new();
        while (true)
        {
            TsxToken token = NextToken(includeWhitespace);

            if (!includeCommentsAsSeparateTokens &&
                token.Kind is TsxTokenKind.LineComment or TsxTokenKind.BlockComment)
            {
                comments.Add(token);
                continue;
            }

            if (includeWhitespace || (token.Kind != TsxTokenKind.Whitespace && token.Kind != TsxTokenKind.NewLine))
            {
                if (comments.Any())
                {
                    token.AddCommentTokens(comments);
                    comments.Clear();
                }

                yield return token;
            }

            if (token.Kind == TsxTokenKind.EndOfFile)
                yield break;
        }
    }

    public TsxToken NextToken(bool includeWhitespace)
    {
        if (_pos >= _text.Length)
            return NewToken(TsxTokenKind.EndOfFile, _pos, 0, string.Empty, _line, _column);

        char c = Current;
        int start = _pos;
        int line = _line;
        int col = _column;

        if (c == '\r' || c == '\n')
            return ReadNewLine();

        if (char.IsWhiteSpace(c))
        {
            TsxToken token = ReadWhitespace();
            if (!includeWhitespace)
                return NextToken(false);
            return token;
        }

        if (c == '/')
        {
            if (Peek(1) == '/')
                return ReadLineComment();

            if (Peek(1) == '*')
                return ReadBlockComment();

            Advance();
            return NewToken(TsxTokenKind.Slash, start, 1, "/", line, col);
        }

        if (c == '\'' || c == '"')
            return ReadStringLiteral(c);

        if (c == '`')
            return ReadTemplateLiteral();

        if (IsIdentifierStart(c))
            return ReadIdentifierOrKeyword();

        if (char.IsDigit(c))
            return ReadNumber();

        if (c == '=' && Peek(1) == '>')
            return ReadTwoChar(TsxTokenKind.Arrow);

        if (c == '=' && Peek(1) == '=' && Peek(2) == '=')
            return ReadThreeChar(TsxTokenKind.EqualsEqualsEquals);

        if (c == '!' && Peek(1) == '=' && Peek(2) == '=')
            return ReadThreeChar(TsxTokenKind.BangEqualsEquals);

        if (c == '=' && Peek(1) == '=')
            return ReadTwoChar(TsxTokenKind.EqualsEquals);

        if (c == '!' && Peek(1) == '=')
            return ReadTwoChar(TsxTokenKind.BangEquals);

        if (c == '&' && Peek(1) == '&')
            return ReadTwoChar(TsxTokenKind.AmpersandAmpersand);

        if (c == '|' && Peek(1) == '|')
            return ReadTwoChar(TsxTokenKind.PipePipe);

        Advance();

        switch (c)
        {
            case '(':
                return NewToken(TsxTokenKind.OpenParen, start, 1, "(", line, col);
            case ')':
                return NewToken(TsxTokenKind.CloseParen, start, 1, ")", line, col);
            case '{':
                return NewToken(TsxTokenKind.OpenBrace, start, 1, "{", line, col);
            case '}':
                return NewToken(TsxTokenKind.CloseBrace, start, 1, "}", line, col);
            case '[':
                return NewToken(TsxTokenKind.OpenBracket, start, 1, "[", line, col);
            case ']':
                return NewToken(TsxTokenKind.CloseBracket, start, 1, "]", line, col);
            case '.':
                return NewToken(TsxTokenKind.Dot, start, 1, ".", line, col);
            case ',':
                return NewToken(TsxTokenKind.Comma, start, 1, ",", line, col);
            case ':':
                return NewToken(TsxTokenKind.Colon, start, 1, ":", line, col);
            case ';':
                return NewToken(TsxTokenKind.Semicolon, start, 1, ";", line, col);
            case '?':
                return NewToken(TsxTokenKind.Question, start, 1, "?", line, col);
            case '+':
                return NewToken(TsxTokenKind.Plus, start, 1, "+", line, col);
            case '-':
                return NewToken(TsxTokenKind.Minus, start, 1, "-", line, col);
            case '*':
                return NewToken(TsxTokenKind.Star, start, 1, "*", line, col);
            case '%':
                return NewToken(TsxTokenKind.Percent, start, 1, "%", line, col);
            case '=':
                return NewToken(TsxTokenKind.Equals, start, 1, "=", line, col);
            case '<':
                return NewToken(TsxTokenKind.LessThan, start, 1, "<", line, col);
            case '>':
                return NewToken(TsxTokenKind.GreaterThan, start, 1, ">", line, col);
            case '!':
                return NewToken(TsxTokenKind.Bang, start, 1, "!", line, col);
            case '&':
                return NewToken(TsxTokenKind.Ampersand, start, 1, "&", line, col);
            case '|':
                return NewToken(TsxTokenKind.Pipe, start, 1, "|", line, col);
            case '^':
                return NewToken(TsxTokenKind.Caret, start, 1, "^", line, col);
            case '~':
                return NewToken(TsxTokenKind.Tilde, start, 1, "~", line, col);
            default:
                return NewToken(TsxTokenKind.OtherPunctuation, start, 1, c.ToString(), line, col);
        }
    }

    private TsxToken ReadWhitespace()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        while (_pos < _text.Length)
        {
            char c = Current;
            if (c == '\r' || c == '\n' || !char.IsWhiteSpace(c))
                break;
            Advance();
        }

        return NewToken(TsxTokenKind.Whitespace, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadNewLine()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        if (Current == '\r')
        {
            Advance();
            if (_pos < _text.Length && Current == '\n')
                Advance();
        }
        else
        {
            Advance();
        }

        return NewToken(TsxTokenKind.NewLine, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadLineComment()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        Advance();
        Advance();

        while (_pos < _text.Length)
        {
            char c = Current;
            if (c == '\r' || c == '\n')
                break;
            Advance();
        }

        return NewToken(TsxTokenKind.LineComment, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadBlockComment()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        Advance();
        Advance();

        while (_pos < _text.Length)
        {
            if (Current == '*' && Peek(1) == '/')
            {
                Advance();
                Advance();
                break;
            }

            Advance();
        }

        return NewToken(TsxTokenKind.BlockComment, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadStringLiteral(char quote)
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        Advance();

        while (_pos < _text.Length)
        {
            char c = Current;

            if (c == '\\')
            {
                Advance();
                if (_pos < _text.Length)
                    Advance();
                continue;
            }

            if (c == quote)
            {
                Advance();
                break;
            }

            if (c == '\r' || c == '\n')
                break;

            Advance();
        }

        return NewToken(TsxTokenKind.StringLiteral, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadTemplateLiteral()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        Advance();

        while (_pos < _text.Length)
        {
            char c = Current;

            if (c == '\\')
            {
                Advance();
                if (_pos < _text.Length)
                    Advance();
                continue;
            }

            if (c == '`')
            {
                Advance();
                break;
            }

            Advance();
        }

        return NewToken(TsxTokenKind.TemplateLiteral, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadIdentifierOrKeyword()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        Advance();
        while (_pos < _text.Length && IsIdentifierPart(Current))
            Advance();

        string text = _text.Substring(start, _pos - start);
        TsxTokenKind kind = Keywords.Contains(text) ? TsxTokenKind.Keyword : TsxTokenKind.Identifier;
        return NewToken(kind, start, _pos - start, text, line, col);
    }

    private TsxToken ReadNumber()
    {
        int start = _pos;
        int line = _line;
        int col = _column;

        while (_pos < _text.Length && char.IsDigit(Current))
            Advance();

        if (_pos < _text.Length && Current == '.')
        {
            Advance();
            while (_pos < _text.Length && char.IsDigit(Current))
                Advance();
        }

        return NewToken(TsxTokenKind.Number, start, _pos - start, _text.Substring(start, _pos - start), line, col);
    }

    private TsxToken ReadTwoChar(TsxTokenKind kind)
    {
        int start = _pos;
        int line = _line;
        int col = _column;
        Advance();
        Advance();
        return NewToken(kind, start, 2, _text.Substring(start, 2), line, col);
    }

    private TsxToken ReadThreeChar(TsxTokenKind kind)
    {
        int start = _pos;
        int line = _line;
        int col = _column;
        Advance();
        Advance();
        Advance();
        return NewToken(kind, start, 3, _text.Substring(start, 3), line, col);
    }

    private bool IsIdentifierStart(char c)
    {
        return char.IsLetter(c) || c == '_' || c == '$';
    }

    private bool IsIdentifierPart(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '$';
    }

    private char Current
    {
        get { return _text[_pos]; }
    }

    private char Peek(int offset)
    {
        int index = _pos + offset;
        return index >= 0 && index < _text.Length ? _text[index] : '\0';
    }

    private void Advance()
    {
        if (_pos >= _text.Length)
            return;

        char c = _text[_pos];
        _pos++;

        if (c == '\r')
        {
            if (_pos < _text.Length && _text[_pos] == '\n')
                _pos++;

            _line++;
            _column = 1;
            return;
        }

        if (c == '\n')
        {
            _line++;
            _column = 1;
            return;
        }

        _column++;
    }

    private static TsxToken NewToken(TsxTokenKind kind, int start, int length, string text, int line, int column)
    {
        return new TsxToken(kind, start, length, text, line, column);
    }
}