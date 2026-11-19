using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The criterion that blocks the OUTPUT of the Push Back editor (insert, update, library save), taken out of the
    /// window so every host applies the same one: the diagnostics of the declared INTENT plus those of the resolved
    /// system, as ONE list with its severity. It is not the bill-of-materials gate: that one answers a different question.
    /// </summary>
    public static class PushBackEditorOutputGate
    {
        /// <summary>The diagnostics in force: those of the intent (invalid inputs, kept and declared without turning into
        /// another value) followed by those of the resolved system.</summary>
        public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(
            PushBackCompositeEditorState composite, PushBackSystem system)
            => composite.IntentDiagnostics()
                .Concat(PushBackCompositeDiagnostics.Evaluate(system))
                .ToList();

        /// <summary>True when any diagnostic in force blocks the output.</summary>
        public static bool IsBlocked(PushBackCompositeEditorState composite, PushBackSystem system)
            => Diagnostics(composite, system).Any(diagnostic => diagnostic.IsBlocking);

        /// <summary>The status message: the first diagnostic that blocks, else the first warning, else
        /// <paramref name="fallback"/>.</summary>
        public static string StatusOr(PushBackCompositeEditorState composite, PushBackSystem system, string fallback)
        {
            var diagnostics = Diagnostics(composite, system);
            var blocking = diagnostics.FirstOrDefault(diagnostic => diagnostic.IsBlocking);
            if (blocking != null)
            {
                return blocking.Message;
            }

            var warning = diagnostics.FirstOrDefault(
                diagnostic => diagnostic.Severity == PushBackCompositeSeverity.Warning);
            return warning != null ? warning.Message : fallback;
        }
    }
}
