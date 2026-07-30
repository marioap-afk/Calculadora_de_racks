using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// How far one edit of the line's arm matrix reaches.
    ///
    /// A line's matrix has three axes, so the station's three scopes are not enough: the useful edits are "this
    /// whole station", "this level down the whole line" and "this side down the whole line". Each is a real
    /// operation a user asks for, and each is expressed ONCE here rather than as a loop written again at every
    /// call site.
    /// </summary>
    public enum CantileverLineApplyScope
    {
        /// <summary>One cell: one station, one level, one side.</summary>
        Cell = 0,

        /// <summary>Every cell of one station.</summary>
        Station = 1,

        /// <summary>One level, both sides, every station.</summary>
        Level = 2,

        /// <summary>One side, every level, every station.</summary>
        Side = 3,

        /// <summary>Every active cell of the line.</summary>
        Line = 4
    }

    /// <summary>One active cell of a line's arm matrix.</summary>
    public readonly struct CantileverLineCell : IEquatable<CantileverLineCell>
    {
        public CantileverLineCell(int stationIndex, int levelIndex, CantileverArmSide side)
        {
            StationIndex = stationIndex;
            LevelIndex = levelIndex;
            Side = side;
        }

        public int StationIndex { get; }

        public int LevelIndex { get; }

        public CantileverArmSide Side { get; }

        /// <summary>The station-local cell this one projects onto.</summary>
        public CantileverStationCell ToStationCell() => new CantileverStationCell(LevelIndex, Side);

        public bool Equals(CantileverLineCell other) =>
            StationIndex == other.StationIndex && LevelIndex == other.LevelIndex && Side == other.Side;

        public override bool Equals(object obj) => obj is CantileverLineCell other && Equals(other);

        public override int GetHashCode() =>
            ((StationIndex * 397) ^ LevelIndex) * 397 ^ (int)Side;

        public override string ToString() =>
            "S" + (StationIndex + 1) + ":L" + (LevelIndex + 1) + ":" + Side;
    }

    /// <summary>What one matrix operation reached and what it actually moved.</summary>
    public sealed class CantileverLineMatrixChange
    {
        internal CantileverLineMatrixChange(
            CantileverLineApplyScope scope,
            IReadOnlyList<CantileverLineCell> inScope,
            IReadOnlyList<CantileverLineCell> changed)
        {
            Scope = scope;
            InScope = inScope;
            Changed = changed;
        }

        public CantileverLineApplyScope Scope { get; }

        /// <summary>Every cell the operation covered, whether or not it moved.</summary>
        public IReadOnlyList<CantileverLineCell> InScope { get; }

        /// <summary>The cells whose stored override really changed.</summary>
        public IReadOnlyList<CantileverLineCell> Changed { get; }

        public int Count => InScope.Count;

        /// <summary>
        /// Whether the design is byte-identical to what it was.
        ///
        /// The editor asks this before regenerating: ONE notification and ONE regeneration per matrix
        /// operation, and none at all when nothing moved.
        /// </summary>
        public bool IsNoOp => Changed.Count == 0;

        public override string ToString() =>
            Scope + " " + Changed.Count + "/" + InScope.Count;
    }

    /// <summary>
    /// The read and write face of a line's arm matrix: <c>station × level × side</c>.
    ///
    /// It is the same shape as <see cref="CantileverStationArmMatrix"/> one axis up, and it delegates the
    /// meaning of a cell to the design's own <c>EffectiveArm</c> rather than restating the fallback. It also
    /// keeps the station matrix's two rules, because both were about correctness and not about station-ness:
    /// an override structurally equal to the default is NOT stored, and clearing writes null instead of copying
    /// the default in (ADR-0026, D3).
    /// </summary>
    public sealed class CantileverLineArmMatrix
    {
        private readonly CantileverLineDesign _design;

        public CantileverLineArmMatrix(CantileverLineDesign design)
        {
            _design = design ?? throw new ArgumentNullException(nameof(design));
        }

        public int StationCount => _design.StationCount;

        public int LevelCount => _design.StationTopology?.LevelCount ?? 0;

        /// <summary>The sides that have cells. One for a single line, two for a double one.</summary>
        public IReadOnlyList<CantileverArmSide> ActiveSides =>
            _design.TryActiveSides(out var sides) ? sides : Array.Empty<CantileverArmSide>();

        /// <summary>
        /// Every active cell, in a deterministic order: station, then level, then side.
        ///
        /// The order is fixed because it is what a scope iterates and what a test compares. An order that
        /// depended on a dictionary's enumeration would make an identical edit produce a different change list
        /// from one run to the next.
        /// </summary>
        public IReadOnlyList<CantileverLineCell> Cells
        {
            get
            {
                var cells = new List<CantileverLineCell>();

                for (var station = 0; station < StationCount; station++)
                {
                    for (var level = 0; level < LevelCount; level++)
                    {
                        foreach (var side in ActiveSides)
                        {
                            cells.Add(new CantileverLineCell(station, level, side));
                        }
                    }
                }

                return cells;
            }
        }

        public bool IsActive(CantileverLineCell cell) =>
            cell.StationIndex >= 0 && cell.StationIndex < StationCount &&
            cell.LevelIndex >= 0 && cell.LevelIndex < LevelCount &&
            ActiveSides.Contains(cell.Side);

        /// <summary>The template in force in a cell: its override, or the line default.</summary>
        public CantileverArmTemplateDesign Effective(CantileverLineCell cell) =>
            IsActive(cell)
                ? _design.EffectiveArm(cell.StationIndex, cell.LevelIndex, cell.Side)
                : null;

        public bool HasOverride(CantileverLineCell cell) =>
            IsActive(cell) &&
            _design.OverrideFor(cell.StationIndex, cell.LevelIndex, cell.Side) != null;

        public int OverrideCount => Cells.Count(HasOverride);

        /// <summary>The cells one scope reaches, anchored on a cell.</summary>
        public IReadOnlyList<CantileverLineCell> InScope(
            CantileverLineApplyScope scope, CantileverLineCell anchor)
        {
            switch (scope)
            {
                case CantileverLineApplyScope.Cell:
                    return IsActive(anchor)
                        ? new[] { anchor }
                        : Array.Empty<CantileverLineCell>();

                case CantileverLineApplyScope.Station:
                    return Cells.Where(c => c.StationIndex == anchor.StationIndex).ToList();

                case CantileverLineApplyScope.Level:
                    return Cells.Where(c => c.LevelIndex == anchor.LevelIndex).ToList();

                case CantileverLineApplyScope.Side:
                    return Cells.Where(c => c.Side == anchor.Side).ToList();

                case CantileverLineApplyScope.Line:
                    return Cells;

                default:
                    // A scope added without a rule here would otherwise reach nothing and look like a no-op.
                    throw new ArgumentOutOfRangeException(
                        nameof(scope), scope, "El alcance '" + scope + "' no tiene regla.");
            }
        }

        /// <summary>Writes the template as the override of every cell in scope.</summary>
        public CantileverLineMatrixChange Apply(
            CantileverLineApplyScope scope,
            CantileverLineCell anchor,
            CantileverArmTemplateDesign template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var followsDefault =
                CantileverArmTemplateComparer.AreEqual(template, _design.DefaultArmTemplate);

            return Write(scope, anchor, followsDefault ? null : template);
        }

        /// <summary>Clears the override of every cell in scope, so they follow the line default again.</summary>
        public CantileverLineMatrixChange Restore(
            CantileverLineApplyScope scope, CantileverLineCell anchor) =>
            Write(scope, anchor, null);

        /// <summary>
        /// The one writer. It compares structurally before storing, so an edit that changes nothing leaves the
        /// design untouched and reports a no-op.
        /// </summary>
        private CantileverLineMatrixChange Write(
            CantileverLineApplyScope scope,
            CantileverLineCell anchor,
            CantileverArmTemplateDesign wanted)
        {
            var inScope = InScope(scope, anchor);
            var changed = new List<CantileverLineCell>();

            _design.ArmCellOverrides ??= new List<CantileverArmCellOverride>();

            foreach (var cell in inScope)
            {
                var existing = _design.OverrideFor(cell.StationIndex, cell.LevelIndex, cell.Side);

                if (CantileverArmTemplateComparer.AreEqual(existing, wanted))
                {
                    continue;
                }

                RemoveOverride(cell);

                if (wanted != null)
                {
                    // A deep copy PER CELL. One shared instance across cells would make a later edit of one of
                    // them silently change the rest, which is the aliasing bug the station matrix documents.
                    _design.ArmCellOverrides.Add(new CantileverArmCellOverride
                    {
                        StationIndex = cell.StationIndex,
                        LevelIndex = cell.LevelIndex,
                        Side = cell.Side,
                        Arm = wanted.DeepCopy()
                    });
                }

                changed.Add(cell);
            }

            return new CantileverLineMatrixChange(scope, inScope, changed);
        }

        private void RemoveOverride(CantileverLineCell cell) =>
            _design.ArmCellOverrides.RemoveAll(o =>
                o != null &&
                o.StationIndex == cell.StationIndex &&
                o.LevelIndex == cell.LevelIndex &&
                o.Side == cell.Side);
    }
}
