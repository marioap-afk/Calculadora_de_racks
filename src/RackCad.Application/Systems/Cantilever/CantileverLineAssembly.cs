using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// A resolved Cantilever line: its stations, its intervals, and the one height they all share.
    ///
    /// It is IMMUTABLE, like the station before it. Nothing here recomputes on read and nothing can be edited
    /// after the fact: the editor changes the DESIGN and asks for a new assembly. An assembly with setters
    /// would let a caller move a station and leave every separator derived from the old spacing (ADR-0026,
    /// D18 — the same rule, one level up).
    /// </summary>
    public sealed class CantileverLineAssembly
    {
        private CantileverLineAssembly(
            Guid id,
            string name,
            int stationCount,
            double columnCentreSpacing,
            double columnHeight,
            double largestStationMinimumHeight,
            IReadOnlyList<CantileverLineStationPlacement> stations,
            IReadOnlyList<CantileverIntervalAssembly> intervals,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            Id = id;
            Name = name;
            StationCount = stationCount;
            ColumnCentreSpacing = columnCentreSpacing;
            ColumnHeight = columnHeight;
            LargestStationMinimumHeight = largestStationMinimumHeight;
            Stations = stations;
            Intervals = intervals;
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// The line's identity: ONE GUID for the whole line.
        ///
        /// Not one per station. A station is not independently editable, insertable or duplicable — the three
        /// things a GUID exists for in this product — and per-station identity would mean a line whose stations
        /// could drift apart in the drawing while the design said they were one rack.
        /// </summary>
        public Guid Id { get; }

        public string Name { get; }

        public int StationCount { get; }

        public double ColumnCentreSpacing { get; }

        /// <summary>The height every column of this line has.</summary>
        public double ColumnHeight { get; }

        /// <summary>The largest minimum any single station needed, which is what set the common height.</summary>
        public double LargestStationMinimumHeight { get; }

        public IReadOnlyList<CantileverLineStationPlacement> Stations { get; }

        public IReadOnlyList<CantileverIntervalAssembly> Intervals { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        public int IntervalCount => Intervals.Count;

        /// <summary>Every separator of the line, walked interval by interval so none is counted twice.</summary>
        public IReadOnlyList<CantileverSeparatorPlan> Separators =>
            Intervals.SelectMany(i => i.Separators).ToList();

        public IReadOnlyList<CantileverBracedPanelPlan> BracedPanels =>
            Intervals.SelectMany(i => i.BracedPanels).ToList();

        public IReadOnlyList<CantileverBracePlan> Braces =>
            Intervals.SelectMany(i => i.Braces).ToList();

        public IReadOnlyList<CantileverColdRolledAdapterPlan> Adapters =>
            Intervals.SelectMany(i => i.Adapters).ToList();

        public IReadOnlyList<CantileverSeparatorColumnPlatePlan> SeparatorColumnPlates =>
            Intervals.SelectMany(i => i.ColumnPlates).ToList();

        /// <summary>Every arm of every station, with the station it belongs to.</summary>
        public IReadOnlyList<(int StationIndex, CantileverArmAssembly Arm)> Arms =>
            Stations.SelectMany(s => s.Station.Arms.Select(a => (s.Index, a))).ToList();

        /// <summary>
        /// Every piece id of the line, station pieces already scoped.
        ///
        /// It exists so the uniqueness of the whole line is a thing that can be ASKED, and therefore tested.
        /// Station pieces are numbered by the I-37C authority, which does not know it is inside a line, so
        /// without the scope the same id appears once per station.
        /// </summary>
        public IReadOnlyList<CantileverPieceId> AllPieceIds =>
            Stations
                .SelectMany(s => StationPieceIds(s.Station).Select(s.ScopedId))
                .Concat(Intervals.SelectMany(IntervalPieceIds))
                .ToList();

        public static CantileverLineAssembly Create(
            CantileverLineDesign design,
            double columnHeight,
            double largestStationMinimumHeight,
            IReadOnlyList<CantileverLineStationPlacement> stations,
            IReadOnlyList<CantileverIntervalAssembly> intervals,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            return new CantileverLineAssembly(
                design.Id,
                design.Name,
                design.StationCount,
                design.ColumnCentreSpacing,
                columnHeight,
                largestStationMinimumHeight,
                stations ?? Array.Empty<CantileverLineStationPlacement>(),
                intervals ?? Array.Empty<CantileverIntervalAssembly>(),
                diagnostics ?? Array.Empty<CantileverDiagnostic>());
        }

        /// <summary>
        /// A line that could not be resolved. It carries its diagnostics and NOTHING else — no stations, no
        /// intervals, no height — so a caller that ignores <see cref="IsBlocked"/> reads emptiness rather than
        /// a half-built line it might draw.
        /// </summary>
        public static CantileverLineAssembly Blocked(
            CantileverLineDesign design, IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            return new CantileverLineAssembly(
                design.Id,
                design.Name,
                design.StationCount,
                design.ColumnCentreSpacing,
                0.0,
                0.0,
                Array.Empty<CantileverLineStationPlacement>(),
                Array.Empty<CantileverIntervalAssembly>(),
                diagnostics ?? Array.Empty<CantileverDiagnostic>());
        }

        /// <summary>
        /// The line's extent: every station's, translated, unioned with every interval's pieces.
        ///
        /// Nullable, like the station's, and for the same reason: a blocked line has nothing placed, and a box
        /// of zeros would be indistinguishable from a real line sitting at the origin.
        /// </summary>
        public CantileverEnvelope3D? Envelope()
        {
            var boxes = new List<CantileverEnvelope3D>();

            foreach (var placement in Stations)
            {
                if (placement.Station.Envelope != null)
                {
                    boxes.Add(placement.Station.Envelope.Value.Translated(placement.Offset));
                }
            }

            foreach (var interval in Intervals)
            {
                foreach (var plate in interval.Plates)
                {
                    boxes.Add(plate.Envelope());
                }

                foreach (var separator in interval.Separators)
                {
                    boxes.Add(CantileverEnvelope3D.FromPoints(
                        new[] { separator.Member.Start, separator.Member.End }));
                }

                foreach (var brace in interval.Braces)
                {
                    boxes.Add(CantileverEnvelope3D.FromPoints(new[] { brace.LowerEnd, brace.UpperEnd }));
                }
            }

            if (boxes.Count == 0)
            {
                return null;
            }

            var box = boxes[0];

            for (var i = 1; i < boxes.Count; i++)
            {
                box = box.Union(boxes[i]);
            }

            return box;
        }

        /// <summary>
        /// The line's physical signature: everything that would change the drawing, and nothing else.
        ///
        /// The GUID and the name are NOT in it. Two lines with the same geometry and different names are the
        /// same physical rack, and a signature that disagreed would report a change every time somebody
        /// renamed one (ADR-0026, D21).
        /// </summary>
        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "LINE;n={0};pitch={1:0.######};h={2:0.######};{3};{4}",
            StationCount,
            ColumnCentreSpacing,
            ColumnHeight,
            string.Join("|", Stations.Select(s => s.Signature())),
            string.Join("|", Intervals.Select(i => i.Signature())));

        private static IEnumerable<CantileverPieceId> StationPieceIds(CantileverStationAssembly station) =>
            station.Members.Select(m => m.Id)
                .Concat(station.Plates.Select(p => p.Id))
                .Concat(station.Gussets.Select(g => g.Id))
                .Concat(station.Punches.Select(p => p.Id));

        private static IEnumerable<CantileverPieceId> IntervalPieceIds(CantileverIntervalAssembly interval) =>
            interval.Members.Select(m => m.Id)
                .Concat(interval.Plates.Select(p => p.Id))
                .Concat(interval.Adapters.Select(a => a.Id))
                .Concat(interval.Punches.Select(p => p.Id));

        public override string ToString() =>
            "CantileverLine " + (Name ?? "(sin nombre)") + " n=" + StationCount +
            (IsBlocked ? " BLOCKED" : " h=" + ColumnHeight.ToString("0.##", CultureInfo.InvariantCulture));
    }
}
