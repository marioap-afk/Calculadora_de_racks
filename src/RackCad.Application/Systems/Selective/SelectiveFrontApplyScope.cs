using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// How far a FRENTE-wide edit reaches inside each target fondo (I-43, ID14).
    /// <para>
    /// It is deliberately NOT <see cref="SelectiveApplyScope"/>. That enum addresses CELLS of a frente x nivel
    /// matrix, and this property has no nivel axis at all: the elevation of the floor larguero belongs to a frente.
    /// Reusing the cell scopes here would mean answering what "Nivel" or "Seleccionadas" mean for something that has
    /// no level, and the honest answer is that they mean nothing — so only the two scopes a frente really has exist,
    /// the same reasoning <c>RackModuleHeaderScope</c> already applies to a module.
    /// </para>
    /// </summary>
    public enum SelectiveFrontApplyScope
    {
        /// <summary>Only the frente the editor is on, in every target fondo that has it.</summary>
        Front,

        /// <summary>Every frente of every target fondo.</summary>
        All
    }

    /// <summary>
    /// What a frente-wide edit over the target fondos actually reached: the frentes written, as
    /// <c>(fondo, frente)</c>, and the target fondos that do not have the requested frente.
    /// <para>
    /// The omissions are reported rather than swallowed, for the same reason as everywhere else in I-43: a fondo with
    /// fewer frentes is a legitimate rack, and the user has to be told that their edit did not land there instead of
    /// believing it did.
    /// </para>
    /// </summary>
    public sealed class SelectiveFrontApplyResult
    {
        public SelectiveFrontApplyResult(
            SelectiveFrontApplyScope scope,
            IReadOnlyList<(int FondoIndex, int FrontIndex)> applied,
            IReadOnlyList<int> omittedFondos)
        {
            Scope = scope;
            Applied = applied;
            OmittedFondos = omittedFondos;
            Fondos = applied.Select(target => target.FondoIndex).Distinct().OrderBy(index => index).ToArray();
        }

        public SelectiveFrontApplyScope Scope { get; }

        /// <summary>Every frente written, in canonical order — fondo, then frente, both ascending.</summary>
        public IReadOnlyList<(int FondoIndex, int FrontIndex)> Applied { get; }

        /// <summary>The fondos this edit affected, ascending. Not a list of recomputes: one edit, one recompute.</summary>
        public IReadOnlyList<int> Fondos { get; }

        /// <summary>Target fondos that do not have the requested frente. Nothing was created for them.</summary>
        public IReadOnlyList<int> OmittedFondos { get; }

        public int Count => Applied.Count;

        public bool IsEmpty => Applied.Count == 0;

        /// <summary>One Spanish sentence for a status line, numbering fondos and frentes as the editor shows them.</summary>
        public string Describe(bool restore)
        {
            if (IsEmpty)
            {
                return "Ningún fondo destino tiene ese frente: no se cambió nada.";
            }

            var text = string.Format(
                CultureInfo.InvariantCulture,
                Count == 1 ? "{0} la elevación en {1} frente" : "{0} la elevación en {1} frentes",
                restore ? "Restablecida" : "Aplicada",
                Count);

            text += Fondos.Count == 1
                ? " del fondo " + (Fondos[0] + 1).ToString(CultureInfo.InvariantCulture) + "."
                : " de los fondos " + string.Join(", ", Fondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture))) + ".";

            if (OmittedFondos.Count > 0)
            {
                text += string.Format(
                    CultureInfo.InvariantCulture,
                    OmittedFondos.Count == 1
                        ? " Se omitió el fondo {0}: no tiene ese frente."
                        : " Se omitieron los fondos {0}: no tienen ese frente.",
                    string.Join(", ", OmittedFondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture))));
            }

            return text;
        }
    }
}
