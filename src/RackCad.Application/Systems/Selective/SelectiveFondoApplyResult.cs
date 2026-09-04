using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// What a FONDO-wide edit over the target fondos reached (I-43, gate 7): which fondos took the value and which
    /// target indices the rack does not have.
    /// <para>
    /// A fondo-wide property has no inner scope. Its authority is the fondo itself, so the only axis is
    /// <c>TargetFondos</c> — presenting a "Celda" or "Nivel" reach for it would be a scope that means nothing.
    /// </para>
    /// </summary>
    public sealed class SelectiveFondoApplyResult
    {
        public SelectiveFondoApplyResult(IReadOnlyList<int> appliedFondos, IReadOnlyList<int> omittedFondos)
        {
            AppliedFondos = appliedFondos;
            OmittedFondos = omittedFondos;
        }

        /// <summary>The fondos written, ascending.</summary>
        public IReadOnlyList<int> AppliedFondos { get; }

        /// <summary>Target indices this rack does not have. Nothing was created for them.</summary>
        public IReadOnlyList<int> OmittedFondos { get; }

        public bool Applied => AppliedFondos.Count > 0;

        /// <summary>One Spanish sentence for a status line, with fondos numbered as the editor shows them.</summary>
        public string Describe(string what, bool restore)
        {
            if (!Applied) return "Ningún fondo destino existe: no se cambió nada.";

            var text = string.Format(
                CultureInfo.InvariantCulture,
                AppliedFondos.Count == 1 ? "{0} {1} en el fondo {2}." : "{0} {1} en los fondos {2}.",
                restore ? "Restablecido" : "Aplicado",
                what,
                string.Join(", ", AppliedFondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture))));

            if (OmittedFondos.Count > 0)
            {
                text += string.Format(
                    CultureInfo.InvariantCulture,
                    " Se omitieron los fondos {0}: no existen.",
                    string.Join(", ", OmittedFondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture))));
            }

            return text;
        }
    }
}
