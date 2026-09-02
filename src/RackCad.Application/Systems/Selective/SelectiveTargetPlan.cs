using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
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
            IReadOnlyList<SelectiveCellAddress> omittedCells)
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

        /// <summary>
        /// The projected INSTANCES that found no cell: for scope <c>Selected</c>, each
        /// <c>(fondo objetivo, posicion seleccionada)</c> pair whose coordinates do not exist in that fondo. There is
        /// one entry per fondo, not one per position, because a position that exists in fondo 1 and not in fondo 3 is
        /// applied once and omitted once — omitting only that instance is the whole point of a projection.
        /// <para>
        /// A single reason covers every entry — that coordinate does not exist in that fondo — so none is stored. The
        /// other four scopes derive their coordinates from the topology itself and can never name a missing cell, so
        /// this list is empty for them; a frente merely shorter than the anchor level is the ordinary ragged rule and
        /// not an omission.
        /// </para>
        /// </summary>
        public IReadOnlyList<SelectiveCellAddress> OmittedCells { get; }

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
                    OmittedCells.Count == 1
                        ? " Se omitio 1 posicion que no existe en su fondo destino."
                        : " Se omitieron {0} posiciones que no existen en su fondo destino.",
                    OmittedCells.Count);
            }

            return text;
        }

        internal static readonly IReadOnlyList<SelectiveCellAddress> NoCells = Array.Empty<SelectiveCellAddress>();
    }
}
