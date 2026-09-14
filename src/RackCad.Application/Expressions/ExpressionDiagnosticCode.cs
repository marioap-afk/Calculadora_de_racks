namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The closed catalogue of expression diagnostics (I-49, Proposal V6 P15.3), in catalogue order.
    ///
    /// <para>
    /// The numeric value IS the order used to sort diagnostics that share a position (P15.7), so members are only ever
    /// appended and never renumbered. G5 declared the "syntax and limits" class, the only one a lexer and a parser can
    /// produce; G6 appends the binding, semantic and boundary-contract classes. Which component may produce a code is part
    /// of the contract: the parser never decides a binding question, and the core never produces <c>Cycle</c> or
    /// <c>DependencyFailed</c> before the graph exists (G7), nor <c>OutOfRange</c>, which belongs to adapters and consumers.
    /// </para>
    /// <para>
    /// These are codes, not messages. The Spanish text a user reads is produced by one text layer outside the core
    /// (P15.4).
    /// </para>
    /// </summary>
    public enum ExpressionDiagnosticCode
    {
        // ================================================================ syntax and limits (lexer, parser; limits also after binding)

        /// <summary>The text is empty or only whitespace.</summary>
        EmptyExpression = 1,

        /// <summary>A character that belongs to no token of the grammar, such as <c>=</c>, <c>@</c> or a non-breaking space.</summary>
        UnexpectedCharacter = 2,

        /// <summary>A well-formed token where the grammar does not allow it, including a missing token at the end.</summary>
        UnexpectedToken = 3,

        /// <summary>A parenthesis that is never closed, or a closing one with nothing open.</summary>
        UnbalancedParenthesis = 4,

        /// <summary>A braced name whose closing brace never arrives (an escaped <c>}}</c> does not close it).</summary>
        UnterminatedName = 5,

        /// <summary>A numeral outside the invariant form: <c>.5</c>, <c>5.</c>, an exponent, glued letters or a non-finite value.</summary>
        InvalidNumber = 6,

        /// <summary>
        /// A comma with a digit right before it and another right after, whatever those digits belong to: never read as
        /// a decimal point nor as an argument separator (P1.5). A separator is written with a space: <c>MAX(A1, 5)</c>.
        /// </summary>
        AmbiguousDecimalComma = 7,

        /// <summary>A bracketed suffix after a numeral whose token is not exactly <c>mm</c>, <c>in</c> or <c>ft</c>.</summary>
        UnknownUnit = 8,

        /// <summary>A bracketed unit suffix that does not follow a numeral: after a reference, a call or a parenthesis.</summary>
        UnitNotAllowedHere = 9,

        /// <summary>A unit written any other way: <c>100 mm</c>, <c>100mm</c>, <c>12"</c>, <c>10'6"</c> or <c>1 1/8</c>.</summary>
        UnitSyntaxNotSupported = 10,

        /// <summary>A <c>#</c> not followed by a complete D-format GUID: a fragment is never parsed (P1.7, P3.9).</summary>
        InvalidQualifier = 11,

        /// <summary>A parser guard or a normative limit was exceeded; <see cref="ExpressionDiagnostic.Limit"/> says which.</summary>
        LimitExceeded = 12,

        // ================================================================ binding (binder, while writing)

        /// <summary>An unqualified name that matches no symbol exactly; a partial name never binds (P7.1).</summary>
        UnknownSymbol = 13,

        /// <summary>An unqualified name that matches two or more symbols; the diagnostic carries every candidate.</summary>
        AmbiguousName = 14,

        /// <summary><c>word.</c>: namespace syntax, reserved for ID20, with no active namespace (P1.7, P25.3).</summary>
        UnknownNamespace = 15,

        /// <summary>A call whose name is not in the function registry.</summary>
        UnknownFunction = 16,

        /// <summary>A reference to a symbol whose scope the consumer cannot see: a Project definition reading a Rack symbol (P5.3).</summary>
        ScopeViolation = 17,

        /// <summary>A bare name equal to a function name, <c>Rack</c> or <c>Project</c>; it has to be written with braces.</summary>
        ReservedName = 18,

        /// <summary><c>#guid</c> without a name, for a present id: that form only displays broken references and never binds.</summary>
        NameRequired = 19,

        /// <summary>A qualified reference whose name is not the current name of the id: the id rules, the name is only validated.</summary>
        QualifiedNameMismatch = 20,

        /// <summary>A contiguous run with operators and no braces or spaces equal to a symbol name: never read as an operation.</summary>
        OperatorInName = 21,

        // ================================================================ semantic (while writing and when evaluating persisted trees)

        /// <summary>A reference to an id absent from the snapshot: it can be shown, never committed (P4.5).</summary>
        BrokenReference = 22,

        /// <summary>A member of a dependency cycle. Declared here; produced by the graph of G7.</summary>
        Cycle = 23,

        /// <summary>A dependency failed, so this symbol is not evaluated. Declared here; produced by G7.</summary>
        DependencyFailed = 24,

        /// <summary>A known function with an arity outside its range (P10.4).</summary>
        InvalidArguments = 25,

        /// <summary>A division whose divisor is zero, positive or negative, including <c>0 / 0</c> (P14.4).</summary>
        DivisionByZero = 26,

        /// <summary>An intermediate or final value that is not finite (P14.4).</summary>
        NonFiniteResult = 27,

        /// <summary>A persisted form that is not canonical (P2.8). Declared here; read and reported from G8 on.</summary>
        NonCanonicalForm = 28,

        // ================================================================ boundary contract (adapters and consumers, never the core)

        /// <summary>The root value breaks the contract of its variable type or the domain of its property (P5.7).</summary>
        OutOfRange = 29,
    }
}
