using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// One station placed on a line: its resolved assembly, its index, and where it sits.
    ///
    /// The station assembly itself is resolved at the ORIGIN by the I-37C authority, exactly as a standalone
    /// station is. Its position on the line lives here. Re-resolving each station in its own world position
    /// would have meant giving the station resolver a datum offset — a second authority for the one thing
    /// <c>CantileverColumnBaseDatum</c> already fixes — and every station's signature would then differ from
    /// its neighbours' for no structural reason.
    /// </summary>
    public sealed class CantileverLineStationPlacement
    {
        internal CantileverLineStationPlacement(
            int index, double originX, CantileverStationAssembly station)
        {
            Index = index;
            OriginX = originX;
            Station = station;
        }

        public int Index { get; }

        /// <summary>World X of this station's column centre-line.</summary>
        public double OriginX { get; }

        public CantileverStationAssembly Station { get; }

        /// <summary>The translation from the station's own coordinates to the line's.</summary>
        public Vector3D Offset => new Vector3D(OriginX, 0.0, 0.0);

        /// <summary>A piece of this station, keyed so it cannot collide with the same piece of another.</summary>
        public CantileverPieceId ScopedId(CantileverPieceId id) => id.WithStationScope(Index);

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture, "stn{0}@{1:0.######}[{2}]", Index, OriginX, Station.Signature());

        public override string ToString() => "Station " + Index + " at x=" +
            OriginX.ToString("0.##", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// THE authority that turns a <see cref="CantileverLineDesign"/> into a resolved line.
    ///
    /// Its one hard problem is the COMMON HEIGHT. Every station of a line has the same column height — a line
    /// whose columns differed would not be one rack — but the height a station needs depends on its own arms,
    /// and arms vary per cell. Two orders were rejected. Resolving each station at its own height and drawing
    /// them at different heights contradicts the product. Resolving station 0 and imposing its height on the
    /// rest would silently truncate a station whose arms need more, because the station authority snaps UP to
    /// a punch and cannot snap down.
    ///
    /// So it runs in two passes: measure what every station needs, take the largest, then re-resolve them all
    /// at that one height. The second pass is a real re-resolution and not a patch — the height changes which
    /// punch every level lands on, and adjusting a number afterwards would leave the punches where the first
    /// pass put them (ADR-0027, D2).
    /// </summary>
    public static class CantileverLineResolver
    {
        public static CantileverLineAssembly Resolve(
            CantileverLineDesign design,
            StructuralSectionCatalog catalog,
            StructuralSectionGeometryFactory geometryFactory,
            ICantileverColumnBaseSectionPolicy columnBasePolicy,
            ICantileverArmSectionPolicy armPolicy)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (geometryFactory == null)
            {
                throw new ArgumentNullException(nameof(geometryFactory));
            }

            var diagnostics = new List<CantileverDiagnostic>();

            if (design.StationCount < CantileverLineDefaults.MinimumStationCount)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.LineNeedsTwoStations,
                    "Una linea necesita al menos " + CantileverLineDefaults.MinimumStationCount +
                    " estaciones; el diseno declara " + design.StationCount + "."));
            }

            if (!(design.ColumnCentreSpacing > 0.0))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.LineSpacingNotPositive,
                    "La separacion entre centros de columna debe ser positiva."));
            }

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return CantileverLineAssembly.Blocked(design, diagnostics);
            }

            var resolver = new CantileverStationResolver(
                catalog, geometryFactory, columnBasePolicy, armPolicy);

            // ---- Pass one: what does each station need? -------------------------------------------------
            var probes = new List<CantileverStationAssembly>(design.StationCount);

            for (var i = 0; i < design.StationCount; i++)
            {
                probes.Add(resolver.Resolve(design.ToStationDesign(i)));
            }

            var blockedProbe = probes.FindIndex(p => p.IsBlocked);

            if (blockedProbe >= 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.LineStationBlocked,
                    "La estacion " + (blockedProbe + 1) + " de la linea no se pudo resolver."));
                diagnostics.AddRange(probes[blockedProbe].Diagnostics.Where(d => d.IsBlocking));
                return CantileverLineAssembly.Blocked(design, diagnostics);
            }

            var commonHeight = probes.Max(p => p.ResolvedColumnHeight);
            var largestMinimum = probes.Max(p => p.MinimumColumnHeight);

            var manual = design.StationTopology.ColumnHeight != null &&
                design.StationTopology.ColumnHeight.Mode == CantileverStationColumnHeightMode.Manual;

            if (manual && commonHeight < largestMinimum)
            {
                // Unreachable while the station authority refuses a manual height under its own minimum; kept
                // because "the tallest station wins" is only true if every station's own answer was valid.
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.LineManualHeightBelowMinimum,
                    "La altura manual de la linea es menor que la minima que necesita su estacion mas alta."));
                return CantileverLineAssembly.Blocked(design, diagnostics);
            }

            for (var i = 0; i < probes.Count; i++)
            {
                if (probes[i].ResolvedColumnHeight < commonHeight)
                {
                    diagnostics.Add(CantileverDiagnostic.Warning(
                        CantileverDiagnostics.LineCommonHeightMovedALevel,
                        "La estacion " + (i + 1) + " sola resolvia a " +
                        probes[i].ResolvedColumnHeight.ToString("0.###", CultureInfo.InvariantCulture) +
                        " pulgadas y en la linea sube a " +
                        commonHeight.ToString("0.###", CultureInfo.InvariantCulture) +
                        "; sus niveles pueden caer en otro troquel."));
                }
            }

            // ---- Pass two: every station at the one height ----------------------------------------------
            var placements = new List<CantileverLineStationPlacement>(design.StationCount);

            for (var i = 0; i < design.StationCount; i++)
            {
                var station = resolver.Resolve(design.ToStationDesignAtHeight(i, commonHeight));

                if (station.IsBlocked)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.LineStationBlocked,
                        "La estacion " + (i + 1) + " no se pudo resolver a la altura comun de " +
                        commonHeight.ToString("0.###", CultureInfo.InvariantCulture) + " pulgadas."));
                    diagnostics.AddRange(station.Diagnostics.Where(d => d.IsBlocking));
                    return CantileverLineAssembly.Blocked(design, diagnostics);
                }

                placements.Add(new CantileverLineStationPlacement(
                    i, design.StationOriginX(i), station));
            }

            RequireOneHeight(placements, commonHeight, diagnostics);

            // ---- The intervals ---------------------------------------------------------------------------
            var attachmentGeometry = MeasureAttachment(placements[0].Station, geometryFactory);
            var intervals = new List<CantileverIntervalAssembly>(design.IntervalCount);

            for (var i = 0; i < design.IntervalCount; i++)
            {
                var attachment = new CantileverBracingAttachment(
                    placements[i].OriginX + attachmentGeometry.HalfWidthX,
                    placements[i + 1].OriginX - attachmentGeometry.HalfWidthX,
                    attachmentGeometry.BracingFaceY,
                    attachmentGeometry.OutwardSign);

                var interval = CantileverIntervalResolver.Resolve(
                    i, design.Bracing, attachment, commonHeight, manual, catalog, geometryFactory);

                diagnostics.AddRange(interval.Diagnostics);
                intervals.Add(interval);
            }

            return CantileverLineAssembly.Create(
                design, commonHeight, largestMinimum, placements, intervals, diagnostics);
        }

        /// <summary>
        /// A full pass over the resolved stations confirming they really do share one height.
        ///
        /// The second pass asked for the common height; this checks it was GRANTED. The station authority snaps
        /// its height up to a punch, so a station whose levels land differently could come back taller than
        /// asked, and a line drawn from those assemblies would have columns of different heights while every
        /// number in the design said otherwise.
        /// </summary>
        private static void RequireOneHeight(
            IReadOnlyList<CantileverLineStationPlacement> placements,
            double commonHeight,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            foreach (var placement in placements)
            {
                var height = placement.Station.ResolvedColumnHeight;

                if (Math.Abs(height - commonHeight) <= GeometryTolerance.Length)
                {
                    continue;
                }

                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.LineCommonHeightMovedALevel,
                    "La estacion " + (placement.Index + 1) + " resolvio a " +
                    height.ToString("0.######", CultureInfo.InvariantCulture) +
                    " pulgadas y no a la altura comun " +
                    commonHeight.ToString("0.######", CultureInfo.InvariantCulture) + "."));
            }
        }

        /// <summary>
        /// Measures the column once: how far its faces are from its centre-line, and which side the bracing
        /// goes on.
        ///
        /// Every station of a line shares one column template, so this is measured on station 0 and reused. A
        /// per-station measurement would invite a line whose stations had different columns, which the design
        /// has no way to express.
        /// </summary>
        private static ColumnAttachmentGeometry MeasureAttachment(
            CantileverStationAssembly station, StructuralSectionGeometryFactory geometryFactory)
        {
            var column = station.ColumnBase.Column;
            var geometry = geometryFactory.Get(column.Placement.SectionId, SectionDetailLevel.Tabulated);
            var box = CantileverPrismExtent.CrossSection(column.Placement, geometry);

            // The bracing goes on the column face the loads do NOT come from: on a single station, the side
            // opposite its arms. A double station has arms on both sides and therefore no back at all, so the
            // rule is declared rather than derived — +Y — and the MVP does not check whether a separator and
            // an arm at nearby elevations interfere.
            var outward = station.FaceMode == CantileverStationFaceMode.Single &&
                station.SingleSide == CantileverArmSide.PositiveY
                    ? -1.0
                    : 1.0;

            return new ColumnAttachmentGeometry(
                Math.Max(Math.Abs(box.MinX), Math.Abs(box.MaxX)),
                outward > 0.0 ? box.MaxY : box.MinY,
                outward);
        }

        private readonly struct ColumnAttachmentGeometry
        {
            public ColumnAttachmentGeometry(double halfWidthX, double bracingFaceY, double outwardSign)
            {
                HalfWidthX = halfWidthX;
                BracingFaceY = bracingFaceY;
                OutwardSign = outwardSign;
            }

            public double HalfWidthX { get; }

            public double BracingFaceY { get; }

            public double OutwardSign { get; }
        }
    }
}
