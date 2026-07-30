using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>One resolved level of a station: its layout plan and the arms I-37B actually built for it.</summary>
    public sealed class CantileverStationResolvedLevel
    {
        internal CantileverStationResolvedLevel(
            CantileverStationLevelPlan plan, IReadOnlyList<CantileverArmAssembly> arms)
        {
            Plan = plan;
            Arms = arms;
        }

        /// <summary>What the layout predicted: index, metrics per side and the clears.</summary>
        public CantileverStationLevelPlan Plan { get; }

        /// <summary>
        /// The resolved arms, one per active side, in the same order as <c>Plan.Cells</c>.
        ///
        /// These are I-37B's own results, not re-derived. The layout's prediction is checked against them
        /// before a station is built, which is what makes the two-pass resolution safe (ADR-0026, D5).
        /// </summary>
        public IReadOnlyList<CantileverArmAssembly> Arms { get; }

        public int LevelIndex => Plan.LevelIndex;

        public int LowerPunchIndex => Plan.LowerPunchIndex;

        public CantileverArmAssembly Arm(CantileverArmSide side) =>
            Arms.FirstOrDefault(a => a.Side == side);
    }

    /// <summary>
    /// A resolved Cantilever STATION: one column, one or two bases, and the arms of every level.
    ///
    /// Immutable and deterministic. A BLOCKED station keeps its diagnostics and produces NO geometry and no
    /// BOM — a partially resolved station that still quoted would quote something nobody can build.
    ///
    /// What it deliberately does NOT contain: a longitudinal position, an index inside a run, spacers, braces
    /// or any reference to a neighbouring station. Those belong to the run, which is a later initiative;
    /// carrying them now would mean removing them later (ADR-0026, D7).
    /// </summary>
    public sealed class CantileverStationAssembly
    {
        private CantileverStationAssembly(
            CantileverStationFaceMode faceMode,
            CantileverArmSide? singleSide,
            CantileverStationColumnBaseAssembly columnBase,
            IReadOnlyList<CantileverStationResolvedLevel> levels,
            double minimumColumnHeight,
            double resolvedColumnHeight,
            double requestedClearHeight,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            FaceMode = faceMode;
            SingleSide = singleSide;
            ColumnBase = columnBase;
            Levels = levels;
            MinimumColumnHeight = minimumColumnHeight;
            ResolvedColumnHeight = resolvedColumnHeight;
            RequestedClearHeight = requestedClearHeight;
            Diagnostics = diagnostics;
        }

        public CantileverStationFaceMode FaceMode { get; }

        /// <summary>The active side of a SINGLE station; null for a double one, where both are active.</summary>
        public CantileverArmSide? SingleSide { get; }

        /// <summary>The one column with its one or two bases. Null when the station is blocked.</summary>
        public CantileverStationColumnBaseAssembly ColumnBase { get; }

        public IReadOnlyList<CantileverStationResolvedLevel> Levels { get; }

        /// <summary>The shortest column the levels admit.</summary>
        public double MinimumColumnHeight { get; }

        /// <summary>The height actually built with: the minimum, or a valid manual value above it.</summary>
        public double ResolvedColumnHeight { get; }

        public double RequestedClearHeight { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        /// <summary>The sides that carry a base and arms.</summary>
        public IReadOnlyList<CantileverArmSide> ActiveSides =>
            ColumnBase == null
                ? Array.Empty<CantileverArmSide>()
                : ColumnBase.Sides.Select(s => s.Side).ToList();

        /// <summary>Every arm of the station, level by level and side by side.</summary>
        public IReadOnlyList<CantileverArmAssembly> Arms =>
            Levels.SelectMany(l => l.Arms).ToList();

        public IReadOnlyList<CantileverStructuralMemberPlan> Members =>
            ColumnBase == null
                ? Array.Empty<CantileverStructuralMemberPlan>()
                : ColumnBase.Members.Concat(Arms.SelectMany(a => a.Members)).ToList();

        public IReadOnlyList<CantileverPlatePlan> Plates =>
            ColumnBase == null
                ? Array.Empty<CantileverPlatePlan>()
                : ColumnBase.Plates.Concat(Arms.SelectMany(a => a.Plates)).ToList();

        public IReadOnlyList<CantileverGussetPlan> Gussets =>
            ColumnBase == null ? Array.Empty<CantileverGussetPlan>() : ColumnBase.Gussets;

        public IReadOnlyList<CantileverPunchPlan> Punches =>
            ColumnBase == null
                ? Array.Empty<CantileverPunchPlan>()
                : ColumnBase.AllPunches.Concat(Arms.SelectMany(a => a.MountingPunches)).ToList();

        /// <summary>The real clear below each level, per side. Level 0 has none.</summary>
        public IReadOnlyList<IReadOnlyDictionary<CantileverArmSide, double>> ActualClears =>
            Levels.Select(l => l.Plan.ClearBySide).ToList();

        public CantileverEnvelope3D? Envelope
        {
            get
            {
                if (ColumnBase == null)
                {
                    return null;
                }

                var envelope = ColumnBase.Envelope();

                foreach (var arm in Arms)
                {
                    if (arm.Envelope != null)
                    {
                        envelope = envelope.Union(arm.Envelope.Value);
                    }
                }

                return envelope;
            }
        }

        internal static CantileverStationAssembly Blocked(
            CantileverStationFaceMode faceMode,
            CantileverArmSide? singleSide,
            IEnumerable<CantileverDiagnostic> diagnostics) =>
            new CantileverStationAssembly(
                faceMode, singleSide, null, Array.Empty<CantileverStationResolvedLevel>(),
                0.0, 0.0, 0.0, diagnostics.ToList());

        internal static CantileverStationAssembly Create(
            CantileverStationFaceMode faceMode,
            CantileverArmSide? singleSide,
            CantileverStationColumnBaseAssembly columnBase,
            IReadOnlyList<CantileverStationResolvedLevel> levels,
            double minimumColumnHeight,
            double resolvedColumnHeight,
            double requestedClearHeight,
            IEnumerable<CantileverDiagnostic> diagnostics) =>
            new CantileverStationAssembly(
                faceMode, singleSide, columnBase, levels,
                minimumColumnHeight, resolvedColumnHeight, requestedClearHeight, diagnostics.ToList());

        /// <summary>Deterministic fingerprint. Blocked stations say so instead of pretending to have geometry.</summary>
        public string Signature()
        {
            if (IsBlocked)
            {
                return "BLOCKED:" + string.Join(",", Diagnostics.Where(d => d.IsBlocking).Select(d => d.Code));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0};h={1:0.######}/min={2:0.######};clear={3:0.######};{4};levels={5}",
                FaceMode,
                ResolvedColumnHeight,
                MinimumColumnHeight,
                RequestedClearHeight,
                ColumnBase.Signature(),
                string.Join("+", Levels.Select(l => l.Plan.Signature())));
        }

        public override string ToString() =>
            "Station " + FaceMode + " levels=" + Levels.Count +
            (IsBlocked ? " BLOCKED" : " h=" + ResolvedColumnHeight.ToString("0.##", CultureInfo.InvariantCulture));
    }
}
