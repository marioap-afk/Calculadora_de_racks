using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// The aggregate result of building the neutral model (I-65 D-08, CD-I65-F1-04). The numeric order IS the precedence:
    /// <c>Unavailable</c> &gt; <c>Invalid</c> &gt; <c>Unsupported</c> &gt; <c>Valid</c>.
    /// </summary>
    public enum StructuralExportOutcome
    {
        Valid = 1,
        Unsupported = 2,
        Invalid = 3,
        Unavailable = 4,
    }

    /// <summary>
    /// A result that only exposes the model when the outcome is <see cref="StructuralExportOutcome.Valid"/>: no partial and
    /// no complete model ever escapes a blocking diagnostic (INV-05).
    /// </summary>
    public sealed class StructuralModelResult
    {
        private StructuralModelResult()
        {
        }

        public StructuralExportOutcome Outcome => throw new NotImplementedException();

        public IReadOnlyList<StructuralDiagnostic> Diagnostics => throw new NotImplementedException();

        /// <summary>The model when, and only when, <see cref="Outcome"/> is Valid; otherwise <c>null</c>.</summary>
        public StructuralModel Model => throw new NotImplementedException();

        /// <summary>The outcome of a set of diagnostics under the precedence of CD-I65-F1-04.</summary>
        public static StructuralExportOutcome Aggregate(IEnumerable<StructuralDiagnostic> diagnostics) =>
            throw new NotImplementedException();

        /// <summary>Builds the result; <paramref name="model"/> is kept only if no diagnostic is blocking.</summary>
        public static StructuralModelResult Create(IEnumerable<StructuralDiagnostic> diagnostics, StructuralModel model) =>
            throw new NotImplementedException();
    }
}
