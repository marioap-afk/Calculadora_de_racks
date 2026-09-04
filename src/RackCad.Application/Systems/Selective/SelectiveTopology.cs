using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The SHAPE of a selective rack as the target resolver needs it (I-43): how many fondos there are, how many
    /// frentes each fondo has, and how many levels each of those frentes has. Nothing else — no cells, no values, no
    /// catalog. A topology is a read-only snapshot, so a plan resolved against it cannot be invalidated by the very
    /// mutation it describes.
    /// <para>
    /// The Selectivo is RAGGED on both inner axes and the resolver must never pretend otherwise: fondo 0 defines the
    /// master frente grid but an extra fondo may be SHORTER (a corner layout keeps its own frente count), each frente
    /// keeps its own level count, and a persisted design may carry a frente with ZERO levels (a building column the
    /// resolver, planta and BOM already honor). A frente with zero levels HAS no cell: it is a frente that exists and
    /// holds nothing, and no scope may invent a level 0 for it.
    /// </para>
    /// <para>
    /// This is where the reuse of the Dinamico/Push Back infrastructure stops being possible.
    /// <c>DynamicRackCellScopeResolver</c> takes a flat <c>IReadOnlyList&lt;int&gt;</c> of level counts and applies
    /// <c>Math.Max(1, ...)</c> to it, so a zero-level front is treated as having one level, and it CLAMPS the source
    /// level index into range. Both are correct for the Dinamico, whose fronts always carry at least one level and
    /// which resolves inside ONE grid; both are wrong here, where clamping across fondos of different depth would
    /// silently retarget a level and the <c>Math.Max(1, ...)</c> would create a cell that does not exist.
    /// </para>
    /// </summary>
    public sealed class SelectiveTopology
    {
        private readonly int[][] levelCounts;

        private SelectiveTopology(int[][] normalized) => levelCounts = normalized;

        /// <summary>An empty rack: no fondo, hence no frente and no cell. Every scope resolves to nothing against it.</summary>
        public static SelectiveTopology Empty { get; } = new SelectiveTopology(Array.Empty<int[]>());

        /// <summary>How many fondos the rack has. Fondo indices run <c>[0, FondoCount)</c>.</summary>
        public int FondoCount => levelCounts.Length;

        /// <summary>True when <paramref name="fondoIndex"/> names a fondo of this rack.</summary>
        public bool HasFondo(int fondoIndex) => fondoIndex >= 0 && fondoIndex < levelCounts.Length;

        /// <summary>How many frentes fondo <paramref name="fondoIndex"/> has; 0 when the fondo does not exist.</summary>
        public int FrontCount(int fondoIndex) => HasFondo(fondoIndex) ? levelCounts[fondoIndex].Length : 0;

        /// <summary>True when the pair <c>(fondoIndex, frontIndex)</c> names a frente. It may still hold no cell.</summary>
        public bool HasFront(int fondoIndex, int frontIndex)
            => frontIndex >= 0 && frontIndex < FrontCount(fondoIndex);

        /// <summary>How many levels that frente has; 0 both when the frente does not exist and when it is empty.</summary>
        public int LevelCount(int fondoIndex, int frontIndex)
            => HasFront(fondoIndex, frontIndex) ? levelCounts[fondoIndex][frontIndex] : 0;

        /// <summary>True when the three-axis address names a cell that EXISTS. The only existence test the resolver uses.</summary>
        public bool HasCell(SelectiveCellAddress address)
            => address.LevelIndex >= 0 && address.LevelIndex < LevelCount(address.FondoIndex, address.FrontIndex);

        /// <summary>Build a topology from raw per-fondo level counts: <c>counts[fondo][frente] = niveles</c>. A null
        /// fondo row is a fondo with no frente; a negative count is normalized to 0 (an empty frente). That is
        /// normalization of malformed INPUT, not clamping of an index — no address is ever moved.</summary>
        public static SelectiveTopology FromLevelCounts(IEnumerable<IEnumerable<int>> perFondoLevelCounts)
        {
            if (perFondoLevelCounts == null) return Empty;
            var rows = perFondoLevelCounts
                .Select(fondo => (fondo ?? Enumerable.Empty<int>()).Select(count => Math.Max(0, count)).ToArray())
                .ToArray();
            return rows.Length == 0 ? Empty : new SelectiveTopology(rows);
        }

        /// <summary>
        /// The topology of a live selective editor state.
        /// <para>
        /// It honors the state's own staleness invariant: while a fondo is being edited its slot in
        /// <c>FondoMatrices</c> is STALE, because the live working matrix (<c>Bays</c>) IS that fondo's copy until
        /// <c>SaveWorkingToSelected</c> commits it. So the selected fondo is read from the working matrix and every
        /// other fondo from its slot — the same rule <c>SelectiveEditorState.MaxFrenteCount</c> already applies.
        /// Reading the stale slot instead would resolve targets against a shape the user has already changed.
        /// </para>
        /// <para>
        /// A state with no slot at all is one fondo whose content is the working matrix, mirroring what
        /// <c>SelectiveEditorState.BuildDesign</c> commits before reading fondo 0.
        /// </para>
        /// </summary>
        public static SelectiveTopology From(SelectiveEditorState state)
        {
            if (state == null) return Empty;

            var working = state.Bays.Select(column => column.Count).ToArray();
            if (state.FondoMatrices.Count == 0)
            {
                return working.Length == 0 ? Empty : new SelectiveTopology(new[] { working });
            }

            var rows = new int[state.FondoMatrices.Count][];
            for (var fondo = 0; fondo < rows.Length; fondo++)
            {
                rows[fondo] = fondo == state.SelectedFondo
                    ? working
                    : state.FondoMatrices[fondo].Bays.Select(column => column.Count).ToArray();
            }

            return new SelectiveTopology(rows);
        }
    }
}
