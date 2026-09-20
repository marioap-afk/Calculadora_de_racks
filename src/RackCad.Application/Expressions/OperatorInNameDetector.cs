using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// <c>OperatorInName</c> (P7.1; §4.2, rule 12; ADR-0040 D6): a contiguous run, without braces or spaces, with
    /// operators, that equals the name of a symbol —<c>Holgura-Base</c> when a variable has that name— is never read as
    /// an operation.
    ///
    /// <para>
    /// It works on what the G5 parser already produced, never on a second lexer: the syntax tree gives, in text order,
    /// the operand pieces a run can be made of —a bare name of one word, a numeral, a call name— and the operators between
    /// them, each with its exact position, so adjacency is a fact of the spans. A run matches a name when the name, split at
    /// its operator characters, is that same sequence: every operand piece equal ignoring case ordinally, every operator
    /// the same character. Because a valid operand piece never contains an operator character, the split is exact.
    /// </para>
    /// <para>
    /// A matching run starts and ends with an operand. A run that starts with an operator is a negation or a sign (<c>-a</c>),
    /// and every negation the formatter writes has that shape, so the canonical text of a legal tree never trips this rule
    /// (P2.5); the formatter also writes a space on each side of a binary operator.
    /// </para>
    /// <para>
    /// Cost is linear: names are split once per table, operand texts are interned once per text, and each name is searched
    /// over the runs with Knuth–Morris–Pratt, so nothing is quadratic in the length of a name or of a run (Amendment A1 §5).
    /// </para>
    /// </summary>
    internal static class OperatorInNameDetector
    {
        private static readonly char[] OperatorCharacters = { '+', '-', '*', '/' };

        /// <summary>A display name that contains operators, as its sequence of operand texts and operator characters.</summary>
        internal sealed class NamePattern
        {
            internal NamePattern(IReadOnlyList<SymbolEntry> entries, IReadOnlyList<string> pieces)
            {
                Entries = entries;
                Pieces = pieces;
            }

            /// <summary>Every symbol with this name, ignoring case: all of them are what the run would mean.</summary>
            internal IReadOnlyList<SymbolEntry> Entries { get; }

            /// <summary>Operand texts and one-character operator strings, in order.</summary>
            internal IReadOnlyList<string> Pieces { get; }
        }

        internal readonly struct Run
        {
            internal Run(SourceSpan span, IReadOnlyList<SymbolId> symbols)
            {
                Span = span;
                Symbols = symbols;
            }

            internal SourceSpan Span { get; }

            internal IReadOnlyList<SymbolId> Symbols { get; }
        }

        private readonly struct Atom
        {
            internal Atom(SourceSpan span, string text, bool isOperator)
            {
                Span = span;
                Text = text;
                IsOperator = isOperator;
            }

            internal SourceSpan Span { get; }

            internal string Text { get; }

            internal bool IsOperator { get; }
        }

        /// <summary>Splits, once per table, every display name that could be misread as an operation.</summary>
        internal static IReadOnlyList<NamePattern> PatternsOf(IReadOnlyDictionary<string, IReadOnlyList<SymbolEntry>> byName)
        {
            var patterns = new List<NamePattern>();

            foreach (var pair in byName.OrderBy(pair => pair.Value[0].Id))
            {
                var name = pair.Key;

                if (name.Length < 3
                    || name.IndexOfAny(OperatorCharacters) < 0
                    || IsOperator(name[0])
                    || IsOperator(name[name.Length - 1]))
                {
                    continue;
                }

                var pieces = new List<string>();
                var start = 0;

                for (var index = 0; index < name.Length; index++)
                {
                    if (!IsOperator(name[index]))
                    {
                        continue;
                    }

                    if (index > start)
                    {
                        pieces.Add(name.Substring(start, index - start));
                    }

                    pieces.Add(name[index].ToString());
                    start = index + 1;
                }

                pieces.Add(name.Substring(start));
                patterns.Add(new NamePattern(pair.Value, new ReadOnlyCollection<string>(pieces)));
            }

            return new ReadOnlyCollection<NamePattern>(patterns);
        }

        /// <summary>Every distinct run that equals a name, in the order the names and runs were searched.</summary>
        internal static IReadOnlyList<Run> Find(ExpressionSyntax root, SymbolTable symbols)
        {
            var patterns = symbols.OperatorNames;

            if (patterns.Count == 0)
            {
                return Array.Empty<Run>();
            }

            var atoms = AtomsOf(root);

            if (atoms.Count < 3)
            {
                return Array.Empty<Run>();
            }

            var interned = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var sequence = new int[atoms.Count];

            for (var index = 0; index < atoms.Count; index++)
            {
                sequence[index] = atoms[index].IsOperator ? OperatorCode(atoms[index].Text[0]) : Intern(interned, atoms[index].Text);
            }

            var runs = new List<(int First, int Last)>();
            var runStart = 0;

            for (var index = 1; index <= atoms.Count; index++)
            {
                if (index == atoms.Count || atoms[index].Span.Start != atoms[index - 1].Span.End)
                {
                    if (index - runStart >= 3)
                    {
                        runs.Add((runStart, index - 1));
                    }

                    runStart = index;
                }
            }

            var found = new Dictionary<SourceSpan, IReadOnlyList<SymbolId>>();
            var order = new List<SourceSpan>();

            foreach (var pattern in patterns)
            {
                var codes = CodesOf(pattern, interned);

                if (codes == null)
                {
                    continue;
                }

                var failure = FailureFunction(codes);

                foreach (var (first, last) in runs)
                {
                    if (last - first + 1 < codes.Length)
                    {
                        continue;
                    }

                    var matched = 0;

                    for (var index = first; index <= last; index++)
                    {
                        while (matched > 0 && sequence[index] != codes[matched])
                        {
                            matched = failure[matched - 1];
                        }

                        if (sequence[index] == codes[matched])
                        {
                            matched++;
                        }

                        if (matched == codes.Length)
                        {
                            var span = SourceSpan.FromBounds(atoms[index - codes.Length + 1].Span.Start, atoms[index].Span.End);

                            if (!found.ContainsKey(span))
                            {
                                found.Add(span, new ReadOnlyCollection<SymbolId>(pattern.Entries.Select(entry => entry.Id).ToList()));
                                order.Add(span);
                            }

                            matched = failure[matched - 1];
                        }
                    }
                }
            }

            return order.Select(span => new Run(span, found[span])).ToList();
        }

        /// <summary>
        /// The operand pieces and operators of the syntax, in text order, walked with an explicit stack. A braced name, a
        /// name of several words, a qualifier, a unit or any punctuation is not a piece: the gap it leaves between spans ends
        /// the run.
        /// </summary>
        private static List<Atom> AtomsOf(ExpressionSyntax root)
        {
            var atoms = new List<Atom>();
            var pending = new Stack<object>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                var item = pending.Pop();

                if (item is Atom atom)
                {
                    atoms.Add(atom);
                    continue;
                }

                switch (item)
                {
                    case NumberSyntax number:
                        atoms.Add(new Atom(number.LiteralSpan, number.Literal, false));
                        break;

                    case ReferenceSyntax reference:
                        if (reference.Name != null && !reference.Name.IsBraced && reference.Name.Text.IndexOf(' ') < 0)
                        {
                            atoms.Add(new Atom(reference.Name.Span, reference.Name.Text, false));
                        }

                        break;

                    case UnaryExpressionSyntax unary:
                        pending.Push(unary.Operand);
                        pending.Push(new Atom(unary.OperatorSpan, unary.Operator == SyntaxUnaryOperator.Plus ? "+" : "-", true));
                        break;

                    case BinaryExpressionSyntax binary:
                        pending.Push(binary.Right);
                        pending.Push(new Atom(binary.OperatorSpan, SymbolOf(binary.Operator), true));
                        pending.Push(binary.Left);
                        break;

                    case ParenthesizedExpressionSyntax parenthesized:
                        pending.Push(parenthesized.Expression);
                        break;

                    case CallExpressionSyntax call:
                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);
                        }

                        atoms.Add(new Atom(call.Function.Span, call.Function.Text, false));
                        break;
                }
            }

            return atoms;
        }

        private static int[] CodesOf(NamePattern pattern, Dictionary<string, int> interned)
        {
            var codes = new int[pattern.Pieces.Count];

            for (var index = 0; index < codes.Length; index++)
            {
                var piece = pattern.Pieces[index];

                if (piece.Length == 1 && IsOperator(piece[0]))
                {
                    codes[index] = OperatorCode(piece[0]);
                }
                else if (interned.TryGetValue(piece, out var code))
                {
                    codes[index] = code;
                }
                else
                {
                    // An operand of the name appears nowhere in the text: the name cannot match.
                    return null;
                }
            }

            return codes;
        }

        private static int[] FailureFunction(int[] codes)
        {
            var failure = new int[codes.Length];
            var matched = 0;

            for (var index = 1; index < codes.Length; index++)
            {
                while (matched > 0 && codes[index] != codes[matched])
                {
                    matched = failure[matched - 1];
                }

                if (codes[index] == codes[matched])
                {
                    matched++;
                }

                failure[index] = matched;
            }

            return failure;
        }

        private static int Intern(Dictionary<string, int> interned, string text)
        {
            if (!interned.TryGetValue(text, out var code))
            {
                code = interned.Count;
                interned.Add(text, code);
            }

            return code;
        }

        private static bool IsOperator(char character) => character == '+' || character == '-' || character == '*' || character == '/';

        private static int OperatorCode(char character)
        {
            switch (character)
            {
                case '+': return -1;
                case '-': return -2;
                case '*': return -3;
                case '/': return -4;
                default: throw new ArgumentOutOfRangeException(nameof(character), character, "Not an operator.");
            }
        }

        private static string SymbolOf(SyntaxBinaryOperator syntaxOperator)
        {
            switch (syntaxOperator)
            {
                case SyntaxBinaryOperator.Add: return "+";
                case SyntaxBinaryOperator.Subtract: return "-";
                case SyntaxBinaryOperator.Multiply: return "*";
                case SyntaxBinaryOperator.Divide: return "/";
                default: throw new InvalidOperationException("Unknown syntax operator " + (int)syntaxOperator + ".");
            }
        }
    }
}
