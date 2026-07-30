using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// How far a station arm edit reaches.
    ///
    /// The scopes are OPERATIONS, not persisted layers. Applying one writes cell overrides and nothing else,
    /// so there is never a global value and a per-cell value disagreeing about the same arm — the failure mode
    /// PB-014 hit when the same datum lived in four places at once (ADR-0026, D3).
    /// </summary>
    public enum CantileverStationApplyScope
    {
        /// <summary>One cell: the given level on the given side.</summary>
        Cell,

        /// <summary>
        /// A whole level. On a single station that is one cell; on a double one it is both sides — which is
        /// what "the levels are shared" means when you edit them.
        /// </summary>
        Level,

        /// <summary>Every active cell of the station.</summary>
        Station
    }

    /// <summary>One active cell of the matrix: a level, a side and the template in force there.</summary>
    public readonly struct CantileverStationCell : IEquatable<CantileverStationCell>
    {
        public CantileverStationCell(int levelIndex, CantileverArmSide side)
        {
            LevelIndex = levelIndex;
            Side = side;
        }

        public int LevelIndex { get; }

        public CantileverArmSide Side { get; }

        public bool Equals(CantileverStationCell other) =>
            LevelIndex == other.LevelIndex && Side == other.Side;

        public override bool Equals(object obj) => obj is CantileverStationCell other && Equals(other);

        public override int GetHashCode() => (LevelIndex * 397) ^ (int)Side;

        public override string ToString() => "L" + (LevelIndex + 1) + ":" + Side;
    }

    /// <summary>
    /// The result of one scope operation: which cells it touched, and nothing more.
    ///
    /// ONE aggregate result and not N notifications. A window that refreshed per cell would do O(cells) work
    /// for an edit the user experiences as single, which is the cost I-15 measured and removed from the main
    /// menu.
    /// </summary>
    public sealed class CantileverStationMatrixChange
    {
        internal CantileverStationMatrixChange(
            CantileverStationApplyScope scope, IReadOnlyList<CantileverStationCell> touched)
        {
            Scope = scope;
            Touched = touched;
        }

        public CantileverStationApplyScope Scope { get; }

        /// <summary>The cells whose effective arm may have changed, in deterministic order.</summary>
        public IReadOnlyList<CantileverStationCell> Touched { get; }

        public int Count => Touched.Count;

        public override string ToString() => Scope + " → " + Count + " celda(s)";
    }

    /// <summary>
    /// THE pure matrix of a station's arms: the ACTIVE cells of <c>level × side</c>, and the scope operations
    /// that write or clear their overrides.
    ///
    /// It is a view over a <see cref="CantileverStationDesign"/> and holds no state of its own. Everything it
    /// reports comes from the design, and everything it changes is a cell override on the design — so there is
    /// no second copy of the arm configuration to keep in step (ADR-0026, D3).
    ///
    /// The inactive side of a single station does NOT appear. Not as an empty cell, not as a disabled one: it
    /// is not a cell. That is what stops a "apply to level" on a single station from writing an override
    /// nobody will ever resolve.
    ///
    /// No operation changes the level count, the requested clear, the computed indices or the face mode. Those
    /// are topology, and a cell edit is not a topology edit.
    /// </summary>
    public sealed class CantileverStationArmMatrix
    {
        private readonly CantileverStationDesign _design;

        public CantileverStationArmMatrix(CantileverStationDesign design)
        {
            _design = design ?? throw new ArgumentNullException(nameof(design));
        }

        public CantileverStationFaceMode FaceMode => _design.FaceMode;

        public int LevelCount => _design.LevelCount;

        /// <summary>The sides that have cells. One for a single station, two for a double one.</summary>
        public IReadOnlyList<CantileverArmSide> ActiveSides => _design.ActiveSides();

        /// <summary>
        /// Every active cell, level-outer and side-inner, ascending.
        ///
        /// Deterministic order so a caller can compare two matrices, or refresh in a stable sequence, without
        /// sorting first.
        /// </summary>
        public IReadOnlyList<CantileverStationCell> Cells
        {
            get
            {
                var sides = ActiveSides;
                var cells = new List<CantileverStationCell>(LevelCount * sides.Count);

                for (var level = 0; level < LevelCount; level++)
                {
                    foreach (var side in sides.OrderBy(s => s))
                    {
                        cells.Add(new CantileverStationCell(level, side));
                    }
                }

                return cells;
            }
        }

        /// <summary>Whether a cell exists. False for the inactive side and for an out-of-range level.</summary>
        public bool IsActive(CantileverStationCell cell) =>
            cell.LevelIndex >= 0 &&
            cell.LevelIndex < LevelCount &&
            ActiveSides.Contains(cell.Side);

        /// <summary>The template in force in a cell: its override, or the station default.</summary>
        public CantileverArmTemplateDesign Effective(CantileverStationCell cell) =>
            IsActive(cell) ? _design.EffectiveArm(cell.LevelIndex, cell.Side) : null;

        /// <summary>Whether a cell carries an override at all. False means it follows the default.</summary>
        public bool HasOverride(CantileverStationCell cell) =>
            IsActive(cell) && _design.Levels[cell.LevelIndex]?.OverrideFor(cell.Side) != null;

        /// <summary>How many cells currently differ from the default.</summary>
        public int OverrideCount => Cells.Count(HasOverride);

        /// <summary>
        /// Writes <paramref name="template"/> as the override of every cell in scope.
        ///
        /// It stores a DEEP COPY per cell. Sharing one instance across cells would make a later edit of one
        /// cell silently change the others, which is the same aliasing bug a shared default would cause.
        /// </summary>
        public CantileverStationMatrixChange Apply(
            CantileverStationApplyScope scope,
            CantileverStationCell anchor,
            CantileverArmTemplateDesign template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            return Write(scope, anchor, cell => template.DeepCopy());
        }

        /// <summary>
        /// Clears the override of every cell in scope, so they follow the station default again.
        ///
        /// Restoring writes NULL rather than copying the default in. A cell holding a copy of the default is a
        /// cell that stops following it the next time the default changes, and the user would have no way to
        /// tell the two states apart (ADR-0026, D3).
        /// </summary>
        public CantileverStationMatrixChange Restore(
            CantileverStationApplyScope scope, CantileverStationCell anchor) =>
            Write(scope, anchor, cell => null);

        private CantileverStationMatrixChange Write(
            CantileverStationApplyScope scope,
            CantileverStationCell anchor,
            Func<CantileverStationCell, CantileverArmTemplateDesign> value)
        {
            var touched = new List<CantileverStationCell>();

            foreach (var cell in InScope(scope, anchor))
            {
                _design.Levels[cell.LevelIndex].SetOverride(cell.Side, value(cell));
                touched.Add(cell);
            }

            return new CantileverStationMatrixChange(scope, touched);
        }

        /// <summary>
        /// The cells a scope reaches from an anchor.
        ///
        /// Exposed because "what would this affect" is a question the UI asks before applying, and answering it
        /// twice — once to preview and once to apply — is how a preview stops matching its own action.
        /// </summary>
        public IReadOnlyList<CantileverStationCell> InScope(
            CantileverStationApplyScope scope, CantileverStationCell anchor)
        {
            switch (scope)
            {
                case CantileverStationApplyScope.Cell:
                    return IsActive(anchor)
                        ? new[] { anchor }
                        : Array.Empty<CantileverStationCell>();

                case CantileverStationApplyScope.Level:
                    // On a single station this is ONE cell, because the opposite side does not exist. On a
                    // double one it is both, because the level is shared.
                    return Cells.Where(c => c.LevelIndex == anchor.LevelIndex).ToList();

                case CantileverStationApplyScope.Station:
                    return Cells;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(scope), scope,
                        "El alcance '" + scope + "' no tiene regla. Anadir un alcance exige escribirla aqui.");
            }
        }
    }
}
