using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.ProjectVariables;

namespace RackCad.UI
{
    /// <summary>
    /// What the window TELLS the user before a repair (I-48 G4B.1).
    ///
    /// <para>
    /// It is a separate, pure function of the projection because the disclosure has to be verifiable on its
    /// own. The operation is rack-scoped: repairing from a selected row removes EVERY repairable broken
    /// binding of that rack. Wording that spoke of "esta propiedad" collected a consent narrower than the
    /// action — the user confirmed one row and two disappeared.
    /// </para>
    /// <para>
    /// It DERIVES nothing. The complete set, whether the rack can be repaired and why it cannot all arrive
    /// decided in <see cref="BrokenBindingRow"/>; this composes a sentence out of them. In particular it never
    /// rebuilds the set by grouping the other rows: that would be a second answer to "what does this repair
    /// touch", and the two answers would eventually disagree.
    /// </para>
    /// </summary>
    internal static class ProjectVariableRepairText
    {
        /// <summary>
        /// The disclosure for the selected row. Names the RACK, the size of the complete set, and every
        /// binding that will be removed with the literal that governs afterwards.
        /// </summary>
        internal static string Describe(BrokenBindingRow broken)
        {
            if (broken == null)
            {
                return string.Empty;
            }

            if (!broken.RackCanRepair)
            {
                // A diagnostic, not an offer: promising the stored literal here would promise a change that
                // cannot be applied at all while the rack carries a fatal state.
                return "Este rack no se puede reparar. " + (broken.RackBlockingReason ?? "Estado no interpretable.")
                       + " La fila se muestra solo como diagnóstico.";
            }

            var batch = broken.RepairBatch;

            return "Reparar actúa sobre el RACK " + batch.RackId + " completo: se retirarán TODOS los vínculos "
                   + "rotos reparables que tiene (" + batch.Count.ToString(CultureInfo.InvariantCulture) + "), "
                   + "no solo la fila seleccionada. " + Enumerate(batch)
                   + " No hay valor efectivo que materializar: gobernarán esos literales almacenados, así que la "
                   + "geometría puede cambiar.";
        }

        /// <summary>
        /// The racks that cannot be repaired at all because their views do not agree on a single authored
        /// state. Shown as a diagnostic: they contribute no repair row, and none of them is actionable.
        /// </summary>
        internal static string DescribeUnresolvable(IReadOnlyList<RackWithoutAuthority> racks)
        {
            if (racks == null || racks.Count == 0)
            {
                return string.Empty;
            }

            var text = new System.Text.StringBuilder();

            text.Append(racks.Count == 1
                ? "Un rack no se puede reparar todavía porque sus vistas no coinciden: "
                : racks.Count.ToString(CultureInfo.InvariantCulture) +
                  " racks no se pueden reparar todavía porque sus vistas no coinciden: ");

            for (var i = 0; i < racks.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(" · ");
                }

                text.Append(racks[i].Reason);
            }

            return text.ToString();
        }

        /// <summary>Every binding of the batch, with the three data a decision needs.</summary>
        private static string Enumerate(RackRepairBatch batch)
        {
            var text = new System.Text.StringBuilder();

            for (var i = 0; i < batch.Bindings.Count; i++)
            {
                var binding = batch.Bindings[i];

                text.Append(i == 0 ? "Se quitarán: " : "; ");
                text.Append('\'');
                text.Append(binding.PropertyId);
                text.Append("' -> '");
                text.Append(binding.VariableId);
                text.Append("' (literal almacenado ");
                text.Append(binding.StoredLiteral.ToString("0.###", CultureInfo.InvariantCulture));
                text.Append(')');
            }

            text.Append('.');
            return text.ToString();
        }
    }
}
