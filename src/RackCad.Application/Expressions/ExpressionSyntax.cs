using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>The closed set of syntax nodes.</summary>
    public enum ExpressionSyntaxKind
    {
        Number = 1,
        Reference = 2,
        NamespaceReference = 3,
        Unary = 4,
        Binary = 5,
        Parenthesized = 6,
        Call = 7,
    }

    public enum SyntaxUnaryOperator
    {
        Plus = 1,
        Minus = 2,
    }

    public enum SyntaxBinaryOperator
    {
        Add = 1,
        Subtract = 2,
        Multiply = 3,
        Divide = 4,
    }

    /// <summary>
    /// The syntax model of V6 P2.1: what the user WROTE, with positions and with references in textual form. It only
    /// serves writing and diagnosing, and it is never persisted — the bound expression is.
    ///
    /// <para>
    /// Immutable, and closed: nodes are built only by the parser of this assembly. It keeps what the text says —
    /// redundant parentheses, a unary plus, the name exactly as typed, whether it was braced — because removing any
    /// of that is canonicalization, and canonicalization belongs to binding.
    /// </para>
    /// </summary>
    public abstract class ExpressionSyntax
    {
        private protected ExpressionSyntax(SourceSpan span)
        {
            Span = span;
        }

        /// <summary>The whole node, from its first to its last code unit.</summary>
        public SourceSpan Span { get; }

        public abstract ExpressionSyntaxKind Kind { get; }
    }

    /// <summary>
    /// <c>number [ "[" unit "]" ]</c>. The value is the invariant reading of the numeral; the unit is kept as its token
    /// and NOT converted — <c>100[mm]</c> still has value 100 here (P9 gives units their numeric meaning).
    /// </summary>
    public sealed class NumberSyntax : ExpressionSyntax
    {
        internal NumberSyntax(SourceSpan span, string literal, double value, SourceSpan literalSpan, string unitToken, SourceSpan? unitSpan)
            : base(span)
        {
            Literal = literal;
            Value = value;
            LiteralSpan = literalSpan;
            UnitToken = unitToken;
            UnitSpan = unitSpan;
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.Number;

        /// <summary>The numeral exactly as typed, such as <c>007</c> or <c>1.50</c>.</summary>
        public string Literal { get; }

        /// <summary>A finite double: a numeral that does not fit is rejected by the lexer.</summary>
        public double Value { get; }

        public SourceSpan LiteralSpan { get; }

        /// <summary><c>mm</c>, <c>in</c> or <c>ft</c>; null when the numeral has no unit.</summary>
        public string UnitToken { get; }

        /// <summary>From <c>[</c> to <c>]</c>; null when the numeral has no unit.</summary>
        public SourceSpan? UnitSpan { get; }

        public bool HasUnit => UnitToken != null;
    }

    /// <summary>
    /// A name as the user wrote it: a bare name (<c>Holgura General</c>) or a braced one (<c>{Holgura-Base}</c>, with
    /// <c>}}</c> already unescaped). It is never an identity (P3.5): only the binder turns it into one, against the
    /// snapshot the user sees.
    /// </summary>
    public sealed class NameSyntax
    {
        internal NameSyntax(string text, bool isBraced, SourceSpan span)
        {
            Text = text;
            IsBraced = isBraced;
            Span = span;
        }

        public string Text { get; }

        /// <summary>
        /// Braces matter to binding: <c>{MIN}</c> is a variable called MIN, while a bare <c>MIN</c> is a reserved word.
        /// </summary>
        public bool IsBraced { get; }

        /// <summary>For a braced name, including both braces.</summary>
        public SourceSpan Span { get; }
    }

    /// <summary>
    /// <c>qualifier = "#" , ( guid-d | braced-key )</c> (P3.9 with Amendment A2 §3.3; ADR-0041 D7): the identity the binder
    /// validates the name against. Its only semantic content is the KEY TEXT (A2 §3.5): no GUID value is kept, so none can
    /// resolve an identity.
    /// </summary>
    public sealed class QualifierSyntax
    {
        internal QualifierSyntax(string key, SourceSpan span)
        {
            Key = key;
            Span = span;
        }

        /// <summary>
        /// The key as typed: the 36 characters of <c>#&lt;d&gt;</c> in whatever case they were typed, or what <c>#{…}</c>
        /// encloses with every <c>}}</c> already unescaped to <c>}</c>.
        /// </summary>
        public string Key { get; }

        /// <summary>The whole lexeme: the <c>#</c> and, for the exact-key form, both braces.</summary>
        public SourceSpan Span { get; }
    }

    /// <summary>
    /// <c>reference = [ name ] qualifier | name</c>: <c>Nombre</c>, <c>{Nombre}</c>, <c>Nombre#&lt;d&gt;</c>,
    /// <c>Nombre#{clave}</c>, <c>{Nombre}#…</c> or a qualifier alone. Nothing is resolved here: <c>UnknownSymbol</c>,
    /// <c>AmbiguousName</c>, <c>ReservedName</c>, <c>NameRequired</c>, <c>QualifiedNameMismatch</c> and
    /// <c>BrokenReference</c> are binding results (P7.1).
    /// </summary>
    public sealed class ReferenceSyntax : ExpressionSyntax
    {
        internal ReferenceSyntax(NameSyntax name, QualifierSyntax qualifier)
            : base(SourceSpan.FromBounds(
                (name?.Span ?? RequireQualifier(qualifier).Span).Start,
                (qualifier?.Span ?? name.Span).End))
        {
            Name = name;
            Qualifier = qualifier;
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.Reference;

        /// <summary>Null only for a qualifier written without a name.</summary>
        public NameSyntax Name { get; }

        /// <summary>Null for an unqualified name.</summary>
        public QualifierSyntax Qualifier { get; }

        private static QualifierSyntax RequireQualifier(QualifierSyntax qualifier)
            => qualifier ?? throw new ArgumentException("A reference needs a name, a qualifier or both.");
    }

    /// <summary>
    /// <c>palabra.</c> — namespace syntax reserved for ID20 (P1.7, P25.3), such as <c>Rack.Frentes</c>. The parser
    /// recognises the form so that the binder can answer <c>UnknownNamespace</c>; no namespace is active in V6.
    /// </summary>
    public sealed class NamespaceReferenceSyntax : ExpressionSyntax
    {
        internal NamespaceReferenceSyntax(NameSyntax @namespace, SourceSpan dotSpan, NameSyntax member)
            : base(SourceSpan.FromBounds(@namespace.Span.Start, member?.Span.End ?? dotSpan.End))
        {
            Namespace = @namespace;
            DotSpan = dotSpan;
            Member = member;
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.NamespaceReference;

        /// <summary>The single word before the dot.</summary>
        public NameSyntax Namespace { get; }

        public SourceSpan DotSpan { get; }

        /// <summary>The bare name after the dot; null when nothing of the kind follows it.</summary>
        public NameSyntax Member { get; }
    }

    /// <summary><c>("+" | "-") unary</c>: binds tighter than any binary operator (P1.3).</summary>
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        internal UnaryExpressionSyntax(SyntaxUnaryOperator @operator, SourceSpan operatorSpan, ExpressionSyntax operand)
            : base(SourceSpan.FromBounds(operatorSpan.Start, operand.Span.End))
        {
            Operator = @operator;
            OperatorSpan = operatorSpan;
            Operand = operand;
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.Unary;

        public SyntaxUnaryOperator Operator { get; }

        public SourceSpan OperatorSpan { get; }

        public ExpressionSyntax Operand { get; }
    }

    /// <summary>
    /// A binary operation. Chains associate to the LEFT (P1.3): <c>1 - 2 - 3</c> is <c>(1 - 2) - 3</c>, so a flat
    /// chain of n operands is a tree n levels deep even without parentheses.
    /// </summary>
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        internal BinaryExpressionSyntax(SyntaxBinaryOperator @operator, SourceSpan operatorSpan, ExpressionSyntax left, ExpressionSyntax right)
            : base(SourceSpan.FromBounds(left.Span.Start, right.Span.End))
        {
            Operator = @operator;
            OperatorSpan = operatorSpan;
            Left = left;
            Right = right;
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.Binary;

        public SyntaxBinaryOperator Operator { get; }

        /// <summary>
        /// Where the operator was written. Adjacent spans reveal a contiguous run such as <c>Holgura-Base</c>, which
        /// the binder needs for <c>OperatorInName</c> (P7.1).
        /// </summary>
        public SourceSpan OperatorSpan { get; }

        public ExpressionSyntax Left { get; }

        public ExpressionSyntax Right { get; }
    }

    /// <summary><c>"(" expression ")"</c>, kept even when redundant.</summary>
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        internal ParenthesizedExpressionSyntax(ExpressionSyntax expression, SourceSpan span)
            : base(span)
        {
            Expression = expression;
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.Parenthesized;

        public ExpressionSyntax Expression { get; }
    }

    /// <summary>
    /// <c>word "(" [ expression { "," expression } ] ")"</c>. The function is a name as typed: whether it exists, its
    /// case-insensitive lookup and its arity are binding (P10.4, P10.5).
    /// </summary>
    public sealed class CallExpressionSyntax : ExpressionSyntax
    {
        internal CallExpressionSyntax(NameSyntax function, IEnumerable<ExpressionSyntax> arguments, SourceSpan span)
            : base(span)
        {
            Function = function;
            Arguments = new ReadOnlyCollection<ExpressionSyntax>(arguments.ToList());
        }

        public override ExpressionSyntaxKind Kind => ExpressionSyntaxKind.Call;

        public NameSyntax Function { get; }

        public IReadOnlyList<ExpressionSyntax> Arguments { get; }
    }
}
