using System;
using System.Collections.Generic;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// How far a bulk edit reaches over a <see cref="SelectionMatrixModel"/> (I-34 / PB-007). Deliberately the same
    /// grammar the design editors already offer as "Aplicar a:" — <c>SelectiveApplyScope</c> in the Selectivo and
    /// <c>DynamicRackCellScope</c> in Dinámico/Push Back — so the safety grids do not teach the user a second idiom.
    /// <para>
    /// The axes are named by the ADOPTER, not by this enum: <see cref="Column"/> is a "Frente" in the tope/guía grids
    /// and a "Poste" in the desviador one. That is why the captions live in <see cref="SelectionMatrixScopeLabels"/>
    /// and never in a switch over a system kind — this whole layer is agnostic to <c>RackSystemKind</c>.
    /// </para>
    /// </summary>
    public enum SelectionMatrixScope
    {
        /// <summary>Only the primary cell.</summary>
        Cell,

        /// <summary>Every column at the primary cell's row (one LEVEL across the whole grid).</summary>
        Row,

        /// <summary>Every row of the primary cell's column (one FRENTE — or POSTE — across all its levels).</summary>
        Column,

        /// <summary>Every present cell in the grid. The only scope that needs NO primary cell.</summary>
        All
    }

    /// <summary>
    /// Payload for <see cref="SelectionMatrixModel.ScopeApplied"/>: the ONE aggregated notification a bulk edit raises,
    /// carrying EXACTLY the cells whose state changed. Not one event per cell (which would make an observer such as the
    /// desviador's live clearance note recompute N times) and not a blunt "something changed" (which would force a full
    /// repaint). An operation that changes nothing raises no event at all.
    /// </summary>
    public sealed class SelectionMatrixScopeAppliedEventArgs : EventArgs
    {
        public SelectionMatrixScopeAppliedEventArgs(
            SelectionMatrixScope scope, IReadOnlyList<SelectionMatrixCell> cells, bool isSelected)
        {
            Scope = scope;
            Cells = cells ?? Array.Empty<SelectionMatrixCell>();
            IsSelected = isSelected;
        }

        /// <summary>The scope the adopter asked for.</summary>
        public SelectionMatrixScope Scope { get; }

        /// <summary>The cells that actually changed — never empty (no change ⇒ no event), never absent cells.</summary>
        public IReadOnlyList<SelectionMatrixCell> Cells { get; }

        /// <summary>The state they were all set to: true = Activar, false = Desactivar.</summary>
        public bool IsSelected { get; }
    }
}
