using System;
using RackCad.Application.Units;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The NORMATIVE limits of a bound tree (I-49, Proposal V6 P1.9 and P7.5; ADR-0040 D8) and the binder's diagnostic
    /// cap (P7.4). One instance, <see cref="Normative"/>, carried by every operation context.
    ///
    /// <para>
    /// These are contract, not implementation values: <see cref="MaxBoundExpressionDepth"/> decides which persisted
    /// payloads are readable, so changing it needs review, the worst-case test of G8 and an ADR. Depth is the maximum
    /// number of nodes on any root→leaf path, counting both ends, so associativity counts: a flat sum of n operands is n
    /// deep. The parser guards (<see cref="ExpressionParser.MaxSyntacticTokens"/>,
    /// <see cref="ExpressionParser.MaxSyntacticNesting"/>) are something else and never replace these (Amendment A1.5).
    /// </para>
    /// </summary>
    public sealed class ExpressionLimits
    {
        /// <summary>Nodes per tree.</summary>
        public const int MaxNodeCount = 256;

        /// <summary>Depth of the <c>BoundExpression</c>: nodes on the longest root→leaf path, both ends included.</summary>
        public const int MaxBoundExpressionDepth = 24;

        /// <summary>Arguments per call, whatever the function (its arity is a separate rule).</summary>
        public const int MaxArgumentCount = 16;

        /// <summary>The binder returns at most this many diagnostics, the first ones in deterministic order (P7.4).</summary>
        public const int MaxBinderDiagnostics = 20;

        private ExpressionLimits()
        {
        }

        public static ExpressionLimits Normative { get; } = new ExpressionLimits();

        public int NodeCount => MaxNodeCount;

        public int BoundExpressionDepth => MaxBoundExpressionDepth;

        public int ArgumentCount => MaxArgumentCount;

        public int BinderDiagnostics => MaxBinderDiagnostics;
    }

    /// <summary>
    /// The immutable context of ONE operation (P6.1–P6.3; ADR-0040 D1): the symbol table of one snapshot, the single
    /// productive <see cref="FunctionRegistry"/>, the neutral units authority and the normative limits.
    ///
    /// <para>
    /// No ambient state: no culture, no clock, no randomness, no environment, no static caches, no AutoCAD and no file
    /// system (P6.2). A semantic error of one symbol does not invalidate the context (P6.8).
    /// </para>
    /// <para>
    /// Construction is an internal seam. In G6 only synthetic contexts exist, built by the core and by tests (P6.5); the
    /// Project Variables adapter of G8 will build them from the accredited registry (P6.4). Plugin and UI cannot build one.
    /// </para>
    /// </summary>
    public sealed class ExpressionContext
    {
        private ExpressionContext(SymbolTable symbols)
        {
            Symbols = symbols;
        }

        public SymbolTable Symbols { get; }

        public FunctionRegistry Functions => FunctionRegistry.Productive;

        public LengthUnits Units => LengthUnits.Authority;

        public ExpressionLimits Limits => ExpressionLimits.Normative;

        internal static ExpressionContext Create(SymbolTable symbols)
            => new ExpressionContext(symbols ?? throw new ArgumentNullException(nameof(symbols)));
    }
}
