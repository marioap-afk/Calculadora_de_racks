using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// Where a cabecera configuration is applied inside the rack's longitudinal module sequence (I-40, PBH-02).
    /// <para>
    /// It is deliberately NOT one of the existing scope types. <c>SelectiveApplyScope</c>,
    /// <c>DynamicRackCellScope</c> and <c>SelectionMatrixScope</c> all address a CELL of a frente x nivel (or
    /// poste x nivel) matrix, and a module has neither axis: the modules are ONE longitudinal sequence shared by
    /// every frente and every poste (I-35). Reusing a matrix scope here would mean inventing a per-frente or
    /// per-poste module that the model does not have. What IS reused is the shape of the interaction —
    /// «Aplicar a:» plus a validate-all-then-apply operation— not the type.
    /// </para>
    /// </summary>
    public enum RackModuleHeaderScope
    {
        /// <summary>Only the module the user has selected.</summary>
        Module,

        /// <summary>Every cabecera the caller declared applicable. The caller owns applicability because physical
        /// presence (I-33) only exists on a RESOLVED system, which a staged session does not hold.</summary>
        AllApplicableHeaders
    }

    /// <summary>
    /// The outcome of applying one cabecera configuration to one or more modules. The operation is ATOMIC: either
    /// every target took an independent copy, or the staged state was not touched at all (I-40).
    /// </summary>
    public sealed class RackModuleHeaderApplyResult
    {
        private RackModuleHeaderApplyResult(bool applied, IReadOnlyList<string> appliedModuleIds, string rejectionReason)
        {
            Applied = applied;
            AppliedModuleIds = appliedModuleIds;
            RejectionReason = rejectionReason;
        }

        /// <summary>True when every target was written. False means NOTHING was written.</summary>
        public bool Applied { get; }

        /// <summary>The modules that received an independent copy, in the order they were validated. Empty on a
        /// rejection — there is no partial application to report.</summary>
        public IReadOnlyList<string> AppliedModuleIds { get; }

        /// <summary>Why nothing was applied; null on success. It names the offending module so the surface can say
        /// what blocked instead of failing quietly.</summary>
        public string RejectionReason { get; }

        internal static RackModuleHeaderApplyResult Success(IReadOnlyList<string> appliedModuleIds)
            => new RackModuleHeaderApplyResult(true, appliedModuleIds, null);

        internal static RackModuleHeaderApplyResult Rejected(string reason)
            => new RackModuleHeaderApplyResult(false, Array.Empty<string>(), reason);

        /// <summary>One Spanish sentence for a status line: what was applied, or why nothing was.</summary>
        public string Describe()
        {
            if (!Applied)
            {
                return RejectionReason ?? "No se aplico ninguna configuracion de cabecera.";
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                AppliedModuleIds.Count == 1
                    ? "Configuracion aplicada a {0} cabecera ({1}). Queda pendiente de Confirmar."
                    : "Configuracion aplicada a {0} cabeceras ({1}). Queda pendiente de Confirmar.",
                AppliedModuleIds.Count,
                string.Join(", ", AppliedModuleIds));
        }
    }
}
