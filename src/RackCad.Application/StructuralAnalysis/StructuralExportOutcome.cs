using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        private StructuralModelResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, StructuralModel model)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Model = model;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        /// <summary>The model when, and only when, <see cref="Outcome"/> is Valid; otherwise <c>null</c>.</summary>
        public StructuralModel Model { get; }

        /// <summary>The outcome of a set of diagnostics under the precedence of CD-I65-F1-04.</summary>
        public static StructuralExportOutcome Aggregate(IEnumerable<StructuralDiagnostic> diagnostics)
        {
            var outcome = StructuralExportOutcome.Valid;

            foreach (var diagnostic in diagnostics ?? Enumerable.Empty<StructuralDiagnostic>())
            {
                if (diagnostic == null)
                {
                    throw new ArgumentException("A diagnostic cannot be null.", nameof(diagnostics));
                }

                if (diagnostic.IsBlocking && diagnostic.ImpliedOutcome > outcome)
                {
                    outcome = diagnostic.ImpliedOutcome;
                }
            }

            return outcome;
        }

        /// <summary>Builds the result; <paramref name="model"/> is kept only if no diagnostic is blocking.</summary>
        public static StructuralModelResult Create(IEnumerable<StructuralDiagnostic> diagnostics, StructuralModel model)
        {
            var list = new ReadOnlyCollection<StructuralDiagnostic>((diagnostics ?? Enumerable.Empty<StructuralDiagnostic>()).ToList());
            var outcome = Aggregate(list);

            return new StructuralModelResult(outcome, list, outcome == StructuralExportOutcome.Valid ? model : null);
        }
    }
}
