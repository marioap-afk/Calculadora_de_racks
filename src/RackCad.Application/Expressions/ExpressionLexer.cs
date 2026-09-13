using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The one lexer that recognises units, names and qualifiers (P1.10). It reads the whole text, diagnoses every
    /// malformed lexeme, then runs the context checks that need the previous token; the parser only ever sees a
    /// token stream that produced no diagnostic.
    ///
    /// <para>
    /// Deliberately strict character classes, all culture-independent:
    /// <list type="bullet">
    /// <item><description>insignificant whitespace is exactly space, tab, carriage return and line feed — a
    /// non-breaking space is an unexpected character, not a separator;</description></item>
    /// <item><description>a digit is ASCII <c>0</c>–<c>9</c>;</description></item>
    /// <item><description>a letter is a Unicode letter (categories Lu, Ll, Lt, Lm, Lo), with no normalization: a
    /// decomposed accent is not a letter (P7.2);</description></item>
    /// <item><description>inside a bare name only a single U+0020 joins two words (P1.2).</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// A malformed lexeme is taken whole and reported once: a numeral with glued letters, a pasted identity after
    /// <c>#</c>, a bracket that encloses no unit. No context check reasons from a token that already carries a
    /// diagnostic, so an error is not reported again through the errors it would cause.
    /// </para>
    /// <para>
    /// The unit tokens are the syntactic table of P1.6, compared ordinally. Converting a unit is not the lexer's job:
    /// the numeric meaning belongs to the neutral units authority (P9), which does not exist in G5.
    /// </para>
    /// </summary>
    internal sealed class ExpressionLexer
    {
        private static readonly string[] UnitTokens = { "mm", "in", "ft" };

        private readonly string _text;
        private readonly List<ExpressionToken> _tokens = new List<ExpressionToken>();
        private readonly List<ExpressionDiagnostic> _diagnostics = new List<ExpressionDiagnostic>();
        private int _position;
        private bool _whitespaceBefore;

        private ExpressionLexer(string text)
        {
            _text = text;
        }

        internal sealed class Result
        {
            internal Result(IReadOnlyList<ExpressionToken> tokens, IReadOnlyList<ExpressionDiagnostic> diagnostics)
            {
                Tokens = tokens;
                Diagnostics = diagnostics;
            }

            /// <summary>The tokens, always ending with <see cref="ExpressionTokenKind.EndOfText"/>.</summary>
            internal IReadOnlyList<ExpressionToken> Tokens { get; }

            internal IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }
        }

        internal static Result Tokenize(string text)
        {
            var lexer = new ExpressionLexer(text);
            lexer.Scan();
            lexer.CheckContext();
            lexer.Add(ExpressionTokenKind.EndOfText, text.Length, 0, null, 0, Guid.Empty, true);
            return new Result(lexer._tokens, lexer._diagnostics);
        }

        internal static bool IsInsignificantWhitespace(char character)
            => character == ' ' || character == '\t' || character == '\r' || character == '\n';

        // ================================================================ scanning

        private void Scan()
        {
            while (_position < _text.Length)
            {
                var character = _text[_position];

                if (IsInsignificantWhitespace(character))
                {
                    _position++;
                    _whitespaceBefore = true;
                    continue;
                }

                if (IsAsciiDigit(character))
                {
                    ScanNumeral();
                }
                else if (character == '.')
                {
                    ScanDot();
                }
                else if (WordStartLength(_position) > 0)
                {
                    ScanBareName();
                }
                else
                {
                    switch (character)
                    {
                        case '{': ScanBracedName(); break;
                        case '#': ScanQualifier(); break;
                        case '[': ScanUnitSuffix(); break;
                        case ',': ScanComma(); break;
                        case '+': Punctuation(ExpressionTokenKind.Plus); break;
                        case '-': Punctuation(ExpressionTokenKind.Minus); break;
                        case '*': Punctuation(ExpressionTokenKind.Star); break;
                        case '/': Punctuation(ExpressionTokenKind.Slash); break;
                        case '(': Punctuation(ExpressionTokenKind.LeftParenthesis); break;
                        case ')': Punctuation(ExpressionTokenKind.RightParenthesis); break;
                        default: ScanUnexpectedCharacter(); break;
                    }
                }
            }
        }

        /// <summary>
        /// A numeral: <c>digit {digit} ["." digit {digit}]</c> (P1.4). The lexeme is taken as far as it could still be
        /// read as one number —digits, points, a comma between digits, glued letters, an exponent sign, feet and inch
        /// marks— and then judged as a whole, so the rest of the text is never misread around a malformed numeral.
        /// </summary>
        private void ScanNumeral()
        {
            var start = _position;

            while (_position < _text.Length)
            {
                var character = _text[_position];

                if (IsAsciiDigit(character) || character == '.' || character == '\'' || character == '"')
                {
                    _position++;
                    continue;
                }

                if (character == ',' && IsAsciiDigitAt(_position - 1) && IsAsciiDigitAt(_position + 1))
                {
                    _position++;
                    continue;
                }

                if ((character == '+' || character == '-') && IsAsciiDigitAt(_position + 1))
                {
                    var previous = _text[_position - 1];
                    if (previous == 'e' || previous == 'E' || (character == '-' && previous == '\''))
                    {
                        // "1e-3" and "10'-6\"" are one lexeme; "1-3" is a subtraction.
                        _position++;
                        continue;
                    }
                }

                var wordPart = WordPartLength(_position);
                if (wordPart == 0)
                {
                    break;
                }

                _position += wordPart;
            }

            ClassifyNumeral(start);
        }

        /// <summary>
        /// One verdict per numeral lexeme, the most specific first: a comma between digits is ambiguous (P1.5); feet or
        /// inch marks, or one of the unit tokens glued to a well-formed numeral, are unsupported unit syntax (P1.6);
        /// anything else that is not exactly a finite invariant numeral is an invalid number (P1.4).
        /// </summary>
        private void ClassifyNumeral(int start)
        {
            var lexeme = _text.Substring(start, _position - start);
            var numeralLength = WellFormedNumeralLength(lexeme);

            if (numeralLength == lexeme.Length)
            {
                var value = double.Parse(lexeme, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                if (!double.IsInfinity(value) && !double.IsNaN(value))
                {
                    Add(ExpressionTokenKind.Number, start, lexeme.Length, lexeme, value, Guid.Empty, true);
                    return;
                }

                // Too long for a finite double. P1.4 excludes NaN and infinity from numerals, and that holds for what
                // a numeral denotes as much as for how it is spelled.
                Invalid(ExpressionTokenKind.Number, start, ExpressionDiagnosticCode.InvalidNumber);
                return;
            }

            if (lexeme.IndexOf(',') >= 0)
            {
                Invalid(ExpressionTokenKind.Number, start, ExpressionDiagnosticCode.AmbiguousDecimalComma);
                return;
            }

            if (lexeme.IndexOf('\'') >= 0 || lexeme.IndexOf('"') >= 0
                || (numeralLength > 0 && IsUnitWord(lexeme.Substring(numeralLength))))
            {
                Invalid(ExpressionTokenKind.Number, start, ExpressionDiagnosticCode.UnitSyntaxNotSupported);
                return;
            }

            Invalid(ExpressionTokenKind.Number, start, ExpressionDiagnosticCode.InvalidNumber);
        }

        /// <summary>
        /// A point that does not belong to a numeral. Right after a word it is the namespace dot of P1.7
        /// (<c>Rack.Frentes</c>), which the parser recognises so that the binder can reject it; before a digit and
        /// after anything else it starts a numeral with no integer part (<c>.5</c>), which is invalid.
        /// </summary>
        private void ScanDot()
        {
            if (IsAsciiDigitAt(_position + 1) && !IsWordPartBefore(_position))
            {
                ScanNumeral();
                return;
            }

            Punctuation(ExpressionTokenKind.Dot);
        }

        /// <summary>
        /// The argument separator — unless an ASCII digit is right before it and another right after. That comma is
        /// <see cref="ExpressionDiagnosticCode.AmbiguousDecimalComma"/> whatever the digits belong to (P1.5): after a
        /// numeral the numeral already took it, so here the first digit ends a name or a qualifier, as in
        /// <c>MAX(A1,5)</c>.
        /// </summary>
        private void ScanComma()
        {
            if (!IsAsciiDigitAt(_position - 1) || !IsAsciiDigitAt(_position + 1))
            {
                Punctuation(ExpressionTokenKind.Comma);
                return;
            }

            var digitsBefore = _position;
            while (IsAsciiDigitAt(digitsBefore - 1))
            {
                digitsBefore--;
            }

            var commaStart = _position;
            _position++;

            while (_position < _text.Length)
            {
                var character = _text[_position];

                if (IsAsciiDigit(character) || ((character == ',' || character == '.') && IsAsciiDigitAt(_position + 1)))
                {
                    _position++;
                    continue;
                }

                break;
            }

            Report(ExpressionDiagnosticCode.AmbiguousDecimalComma, digitsBefore, _position - digitsBefore);
            Add(ExpressionTokenKind.Invalid, commaStart, _position - commaStart, null, 0, Guid.Empty, false);
        }

        /// <summary><c>word {" " word}</c>: exactly one space joins two words; anything else ends the name.</summary>
        private void ScanBareName()
        {
            var start = _position;
            SkipWord();

            while (Current == ' ' && _position + 1 < _text.Length && WordStartLength(_position + 1) > 0)
            {
                _position++;
                SkipWord();
            }

            Add(ExpressionTokenKind.Name, start, _position - start, _text.Substring(start, _position - start), 0, Guid.Empty, true);
        }

        /// <summary><c>"{" {char - "}" | "}}"} "}"</c>: every character is part of the name, and <c>}}</c> is one <c>}</c>.</summary>
        private void ScanBracedName()
        {
            var start = _position;
            var name = new StringBuilder();
            _position++;

            while (_position < _text.Length)
            {
                var character = _text[_position];

                if (character == '}')
                {
                    if (_position + 1 < _text.Length && _text[_position + 1] == '}')
                    {
                        name.Append('}');
                        _position += 2;
                        continue;
                    }

                    _position++;
                    Add(ExpressionTokenKind.BracedName, start, _position - start, name.ToString(), 0, Guid.Empty, true);
                    return;
                }

                name.Append(character);
                _position++;
            }

            Invalid(ExpressionTokenKind.BracedName, start, ExpressionDiagnosticCode.UnterminatedName);
        }

        /// <summary>
        /// <c>"#" guid</c> in full D form, hexadecimal in any case (P3.9). A fragment is never parsed: anything but
        /// exactly 36 D-format characters glued to the <c>#</c>, and not glued to a further letter, digit or underscore,
        /// is <see cref="ExpressionDiagnosticCode.InvalidQualifier"/>. A hyphen after a complete GUID is an operator.
        /// </summary>
        private void ScanQualifier()
        {
            var start = _position;
            _position++;

            if (IsDFormatGuidAt(_position) && (_position + 36 == _text.Length || WordPartLength(_position + 36) == 0))
            {
                var guid = _text.Substring(_position, 36);
                _position += 36;
                Add(ExpressionTokenKind.Qualifier, start, _position - start, guid, 0, Guid.ParseExact(guid, "D"), true);
                return;
            }

            // Whatever was meant as the identity — a fragment, a GUID after a space, a GUID pasted in braces or in
            // parentheses — belongs to this one diagnostic instead of being read as numerals and names.
            var probe = _position;
            while (probe < _text.Length && IsInsignificantWhitespace(_text[probe]))
            {
                probe++;
            }

            var closer = CharAt(probe) == '{' ? '}' : CharAt(probe) == '(' ? ')' : '\0';
            var runStart = closer == '\0' ? probe : probe + 1;
            var runEnd = runStart;

            while (runEnd < _text.Length && (_text[runEnd] == '-' || WordPartLength(runEnd) > 0))
            {
                runEnd += Math.Max(1, WordPartLength(runEnd));
            }

            if (runEnd > runStart)
            {
                if (closer == '\0')
                {
                    _position = runEnd;
                }
                else if (CharAt(runEnd) == closer)
                {
                    _position = runEnd + 1;
                }
            }

            Invalid(ExpressionTokenKind.Qualifier, start, ExpressionDiagnosticCode.InvalidQualifier);
        }

        /// <summary>
        /// <c>"[" unit "]"</c>. Brackets are reserved for units (P1.6), so what they enclose is read as a unit and
        /// judged in the context checks. A unit is a word: when the word is not followed by <c>]</c>, the bracket still
        /// counts as a unit if it closes before anything structural (<c>100[1/2]</c> is an unknown unit); otherwise it
        /// was not closed where it should have been, and the text goes on as what it is (<c>100[mm + 2</c>).
        /// </summary>
        private void ScanUnitSuffix()
        {
            var start = _position;
            _position++;
            var contentStart = _position;

            while (_position < _text.Length
                   && (IsInsignificantWhitespace(_text[_position]) || WordPartLength(_position) > 0))
            {
                _position += Math.Max(1, WordPartLength(_position));
            }

            var close = CharAt(_position) == ']' ? _position : ClosingBracketBeforeStructure(_position);

            if (close >= 0)
            {
                var content = _text.Substring(contentStart, close - contentStart).Trim(' ', '\t', '\r', '\n');
                _position = close + 1;
                Add(ExpressionTokenKind.UnitSuffix, start, _position - start, content, 0, Guid.Empty, true);
                return;
            }

            // Unclosed. If the character here is itself unexpected, its own diagnostic is the one to read.
            if (_position >= _text.Length || StartsToken(_text[_position]))
            {
                Report(ExpressionDiagnosticCode.UnexpectedToken, _position, _position < _text.Length ? 1 : 0);
            }

            Add(ExpressionTokenKind.UnitSuffix, start, _position - start, null, 0, Guid.Empty, false);
        }

        private void ScanUnexpectedCharacter()
        {
            var start = _position;
            var length = IsSurrogatePairAt(_position) ? 2 : 1;
            _position += length;
            Report(ExpressionDiagnosticCode.UnexpectedCharacter, start, length);
            Add(ExpressionTokenKind.Invalid, start, length, null, 0, Guid.Empty, false);
        }

        private void Punctuation(ExpressionTokenKind kind)
        {
            Add(kind, _position, 1, null, 0, Guid.Empty, true);
            _position++;
        }

        // ================================================================ context checks

        /// <summary>
        /// The lexical rules that depend on the previous token. A token that carries a diagnostic, or that a context
        /// diagnostic already covers, is not reasoned from again.
        /// </summary>
        private void CheckContext()
        {
            var covered = new bool[_tokens.Count];

            for (var index = 0; index < _tokens.Count; index++)
            {
                covered[index] = !_tokens[index].IsValid;
            }

            for (var index = 0; index < _tokens.Count; index++)
            {
                if (covered[index] || (index > 0 && covered[index - 1]))
                {
                    continue;
                }

                var token = _tokens[index];
                var afterNumber = index > 0 && _tokens[index - 1].Kind == ExpressionTokenKind.Number;

                switch (token.Kind)
                {
                    case ExpressionTokenKind.UnitSuffix:
                        if (!afterNumber)
                        {
                            // After a reference, a call, a parenthesis, an operator, another unit or at the start.
                            Report(ExpressionDiagnosticCode.UnitNotAllowedHere, token.Span);
                            covered[index] = true;
                        }
                        else if (!IsUnitToken(token.Text))
                        {
                            Report(ExpressionDiagnosticCode.UnknownUnit, token.Span);
                            covered[index] = true;
                        }

                        break;

                    case ExpressionTokenKind.Name:
                        var firstWord = FirstWord(token.Text);
                        if (afterNumber && token.PrecededByWhitespace && IsUnitWord(firstWord))
                        {
                            // "100 mm": a unit written as a word after the numeral.
                            Report(
                                ExpressionDiagnosticCode.UnitSyntaxNotSupported,
                                SourceSpan.FromBounds(_tokens[index - 1].Span.Start, token.Span.Start + firstWord.Length));
                            covered[index - 1] = true;
                            covered[index] = true;
                        }

                        break;

                    case ExpressionTokenKind.Number:
                        if (afterNumber && token.PrecededByWhitespace
                            && index + 2 < _tokens.Count
                            && _tokens[index + 1].Kind == ExpressionTokenKind.Slash && !covered[index + 1]
                            && _tokens[index + 2].Kind == ExpressionTokenKind.Number && !covered[index + 2])
                        {
                            // "1 1/8": a mixed number, not a sum or a product.
                            Report(
                                ExpressionDiagnosticCode.UnitSyntaxNotSupported,
                                SourceSpan.FromBounds(_tokens[index - 1].Span.Start, _tokens[index + 2].Span.End));

                            for (var coveredIndex = index - 1; coveredIndex <= index + 2; coveredIndex++)
                            {
                                covered[coveredIndex] = true;
                            }
                        }

                        break;
                }
            }
        }

        // ================================================================ lexeme helpers

        /// <summary>Length of the longest prefix of <paramref name="lexeme"/> of the form <c>digits ["." digits]</c>.</summary>
        private static int WellFormedNumeralLength(string lexeme)
        {
            var index = 0;

            while (index < lexeme.Length && IsAsciiDigit(lexeme[index]))
            {
                index++;
            }

            if (index == 0)
            {
                return 0;
            }

            if (index + 1 < lexeme.Length && lexeme[index] == '.' && IsAsciiDigit(lexeme[index + 1]))
            {
                index += 2;

                while (index < lexeme.Length && IsAsciiDigit(lexeme[index]))
                {
                    index++;
                }
            }

            return index;
        }

        /// <summary>
        /// Where a bracket closes, looking past what cannot be a unit but not past anything that structures the
        /// expression: another bracket, a brace, a qualifier, a parenthesis or an argument separator. -1 if it does not.
        /// </summary>
        private int ClosingBracketBeforeStructure(int from)
        {
            for (var index = from; index < _text.Length; index++)
            {
                switch (_text[index])
                {
                    case ']':
                        return index;

                    case '[':
                    case '{':
                    case '#':
                    case '(':
                    case ')':
                    case ',':
                        return -1;
                }
            }

            return -1;
        }

        /// <summary>Whether a character begins some token of the grammar, as opposed to being an unexpected character.</summary>
        private static bool StartsToken(char character)
        {
            switch (character)
            {
                case '.':
                case '{':
                case '#':
                case '[':
                case '+':
                case '-':
                case '*':
                case '/':
                case '(':
                case ')':
                case ',':
                    return true;

                default:
                    return false;
            }
        }

        private void SkipWord()
        {
            _position += WordStartLength(_position);

            while (_position < _text.Length)
            {
                var length = WordPartLength(_position);

                if (length == 0)
                {
                    return;
                }

                _position += length;
            }
        }

        // ================================================================ characters

        private char Current => CharAt(_position);

        private char CharAt(int index) => index >= 0 && index < _text.Length ? _text[index] : '\0';

        private static bool IsAsciiDigit(char character) => character >= '0' && character <= '9';

        private bool IsAsciiDigitAt(int index) => index >= 0 && index < _text.Length && IsAsciiDigit(_text[index]);

        private static bool IsAsciiHexDigit(char character)
            => IsAsciiDigit(character) || (character >= 'a' && character <= 'f') || (character >= 'A' && character <= 'F');

        private bool IsSurrogatePairAt(int index)
            => index >= 0 && index + 1 < _text.Length && char.IsHighSurrogate(_text[index]) && char.IsLowSurrogate(_text[index + 1]);

        /// <summary>Code units of the letter at <paramref name="index"/> (two for a surrogate pair), or zero.</summary>
        private int LetterLength(int index)
        {
            if (index < 0 || index >= _text.Length)
            {
                return 0;
            }

            switch (CharUnicodeInfo.GetUnicodeCategory(_text, index))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                    return IsSurrogatePairAt(index) ? 2 : 1;

                default:
                    return 0;
            }
        }

        /// <summary><c>letter | "_"</c>.</summary>
        private int WordStartLength(int index)
            => CharAt(index) == '_' ? 1 : LetterLength(index);

        /// <summary><c>letter | digit | "_"</c>.</summary>
        private int WordPartLength(int index)
        {
            if (index < 0 || index >= _text.Length)
            {
                return 0;
            }

            return _text[index] == '_' || IsAsciiDigit(_text[index]) ? 1 : LetterLength(index);
        }

        /// <summary>Whether the character just before <paramref name="index"/> is a word character, surrogate pairs included.</summary>
        private bool IsWordPartBefore(int index)
        {
            if (index == 0)
            {
                return false;
            }

            if (index >= 2 && IsSurrogatePairAt(index - 2))
            {
                return WordPartLength(index - 2) == 2;
            }

            return WordPartLength(index - 1) == 1;
        }

        private bool IsDFormatGuidAt(int index)
        {
            if (index + 36 > _text.Length)
            {
                return false;
            }

            for (var offset = 0; offset < 36; offset++)
            {
                var character = _text[index + offset];
                var isHyphenPosition = offset == 8 || offset == 13 || offset == 18 || offset == 23;

                if (isHyphenPosition ? character != '-' : !IsAsciiHexDigit(character))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsUnitToken(string text)
            => Array.IndexOf(UnitTokens, text) >= 0;

        /// <summary>A unit token written without brackets, in any case: the notation that P1.6 does not support.</summary>
        private static bool IsUnitWord(string text)
        {
            foreach (var unit in UnitTokens)
            {
                if (string.Equals(unit, text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string FirstWord(string bareName)
        {
            var space = bareName.IndexOf(' ');
            return space < 0 ? bareName : bareName.Substring(0, space);
        }

        // ================================================================ output

        private void Invalid(ExpressionTokenKind kind, int start, ExpressionDiagnosticCode code)
        {
            Report(code, start, _position - start);
            Add(kind, start, _position - start, null, 0, Guid.Empty, false);
        }

        private void Report(ExpressionDiagnosticCode code, int start, int length)
            => Report(code, new SourceSpan(start, length));

        private void Report(ExpressionDiagnosticCode code, SourceSpan span)
            => _diagnostics.Add(new ExpressionDiagnostic(code, span));

        private void Add(ExpressionTokenKind kind, int start, int length, string text, double value, Guid id, bool isValid)
        {
            _tokens.Add(new ExpressionToken(kind, new SourceSpan(start, length), text, value, id, _whitespaceBefore, isValid));
            _whitespaceBefore = false;
        }
    }
}
