using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>Why an EXPLICITLY named cell produced no target (I-43). Only cells the caller spelled out are
    /// reported: a frente that is simply shorter than the anchor level under a Nivel scope is the ordinary ragged
    /// rule, not an omission, and reporting it would drown the real ones.</summary>
    public enum SelectiveTargetOmissionReason
    {
        /// <summary>Its fondo is not a fondo of this rack.</summary>
        FondoOutOfRange,

        /// <summary>Its fondo exists but was not among the target fondos of this operation.</summary>
        FondoNotTargeted,

        /// <summary>Its fondo is targeted, but that frente or that level does not exist there.</summary>
        CellOutOfRange
    }

    /// <summary>One explicitly named cell that produced no target, and why.</summary>
    public readonly struct SelectiveTargetOmission
    {
        public SelectiveTargetOmission(SelectiveCellAddress address, SelectiveTargetOmissionReason reason)
        {
            Address = address;
            Reason = reason;
        }

        public SelectiveCellAddress Address { get; }

        public SelectiveTargetOmissionReason Reason { get; }
    }

    /// <summary>
    /// The COMPLETE result of resolving <c>fondos objetivo x alcance interno</c> into selective cells (I-43): every
    /// address the operation will touch, the fondos it reaches, and everything it had to leave out.
    /// <para>
    /// A plan exists so an operation can be resolved ENTIRELY before anything mutates. It names no cell object and
    /// holds no value: it is a list of addresses over a <see cref="SelectiveTopology"/> snapshot, so a caller can
    /// inspect it, count it, refuse it or report it, and only then write — and, having written, recompute ONCE per
    /// fondo in <see cref="Fondos"/> instead of once per cell.
    /// </para>
    /// <para>
    /// A plan is not persisted and does not survive the operation, exactly like the target set it came from.
    /// </para>
    /// </summary>
    public sealed class SelectiveTargetPlan
    {
        internal SelectiveTargetPlan(
            SelectiveApplyScope scope,
            SelectiveCellAddress anchor,
            bool anchorMissing,
            IReadOnlyList<SelectiveCellAddress> targets,
            IReadOnlyList<int> omittedFondos,
            IReadOnlyList<SelectiveTargetOmission> omittedCells)
        {
            Scope = scope;
            Anchor = anchor;
            AnchorMissing = anchorMissing;
            Targets = targets;
            Fondos = targets.Select(target => target.FondoIndex).Distinct().OrderBy(index => index).ToArray();
            OmittedFondos = omittedFondos;
            OmittedCells = omittedCells;
        }

        /// <summary>The inner scope this plan resolved.</summary>
        public SelectiveApplyScope Scope { get; }

        /// <summary>The cell whose frente/nivel coordinates the scope read. Meaningless for <c>All</c>/<c>Selected</c>.</summary>
        public SelectiveCellAddress Anchor { get; }

        /// <summary>True when the scope needed an anchor cell and that cell does not exist. <see cref="Targets"/> is
        /// then empty: the operation is refused whole, never re-aimed at a neighbouring cell.</summary>
        public bool AnchorMissing { get; }

        /// <summary>Every cell to write, in canonical order — fondo, then frente, then nivel, ascending — and
        /// distinct. Every one of them EXISTS in the topology the plan was resolved against.</summary>
        public IReadOnlyList<SelectiveCellAddress> Targets { get; }

        /// <summary>The fondos the plan actually reaches, ascending. One recompute each, AFTER the whole plan is
        /// applied — never one per target.</summary>
        public IReadOnlyList<int> Fondos { get; }

        /// <summary>Requested target fondos this rack does not have, ascending. Omitted, never created.</summary>
        public IReadOnlyList<int> OmittedFondos { get; }

        /// <summary>Explicitly named cells that produced no target, with the reason (scope <c>Selected</c> only).</summary>
        public IReadOnlyList<SelectiveTargetOmission> OmittedCells { get; }

        public int Count => Targets.Count;

        public bool IsEmpty => Targets.Count == 0;

        public bool Contains(SelectiveCellAddress address) => Targets.Contains(address);

        /// <summary>One Spanish sentence for a status line: how much this plan reaches, or why it reaches nothing.</summary>
        public string Describe()
        {
            if (AnchorMissing)
            {
                return "La celda de origen no existe: no se aplico nada.";
            }

            if (IsEmpty)
            {
                return "Ningun fondo destino tiene celdas en el alcance: no se aplico nada.";
            }

            var text = string.Format(
                CultureInfo.CurrentCulture,
                Count == 1 ? "{0} celda en {1}." : "{0} celdas en {1}.",
                Count,
                Fondos.Count == 1
                    ? "el fondo " + Fondos[0].ToString(CultureInfo.CurrentCulture)
                    : "los fondos " + string.Join(", ", Fondos.Select(index => index.ToString(CultureInfo.CurrentCulture))));

            if (OmittedFondos.Count > 0)
            {
                text += string.Format(
                    CultureInfo.CurrentCulture,
                    OmittedFondos.Count == 1 ? " Se omitio el fondo {0}: no existe." : " Se omitieron los fondos {0}: no existen.",
                    string.Join(", ", OmittedFondos.Select(index => index.ToString(CultureInfo.CurrentCulture))));
            }

            if (OmittedCells.Count > 0)
            {
                text += string.Format(
                    CultureInfo.CurrentCulture,
                    OmittedCells.Count == 1 ? " Se omitio {0} celda seleccionada." : " Se omitieron {0} celdas seleccionadas.",
                    OmittedCells.Count);
            }

            return text;
        }

        internal static readonly IReadOnlyList<SelectiveCellAddress> NoTargets = Array.Empty<SelectiveCellAddress>();
        internal static readonly IReadOnlyList<SelectiveTargetOmission> NoOmittedCells = Array.Empty<SelectiveTargetOmission>();
    }
}
