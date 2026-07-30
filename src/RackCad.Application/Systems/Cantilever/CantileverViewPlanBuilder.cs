using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>Which of the three views a plan is.</summary>
    public enum CantileverViewKind
    {
        /// <summary>Looking along +Y: the line runs across the picture and the columns are up. Whole line.</summary>
        Frontal = 0,

        /// <summary>Looking along −X: one station's depth is across the picture and its column is up.</summary>
        Lateral = 1,

        /// <summary>Looking down: the line runs across and the depth goes up the picture. Whole line.</summary>
        Planta = 2
    }

    /// <summary>What a projected piece IS, so a consumer can put it on the right layer without guessing.</summary>
    public enum CantileverViewPieceKind
    {
        Column = 0,
        Base = 1,
        Arm = 2,
        Plate = 3,
        Gusset = 4,
        Separator = 5,
        Brace = 6,
        ColdRolledAdapter = 7,
        Punch = 8
    }

    /// <summary>
    /// One closed or open outline of a view, in the view's own 2D plane.
    ///
    /// It carries POINTS and a role, and nothing that would let a consumer reach back into a section's
    /// dimensions — the same discipline <c>StructuralSectionPlanBuilder</c> imposes, one level up.
    /// </summary>
    public sealed class CantileverViewCurve
    {
        internal CantileverViewCurve(
            CantileverViewPieceKind kind,
            CantileverPieceId pieceId,
            IReadOnlyList<Point2D> points,
            bool isClosed,
            double? circleDiameter = null)
        {
            Kind = kind;
            PieceId = pieceId;
            Points = points;
            IsClosed = isClosed;
            CircleDiameter = circleDiameter;
        }

        public CantileverViewPieceKind Kind { get; }

        /// <summary>Which piece drew it, station scope included. Empty for nothing: every curve has an owner.</summary>
        public CantileverPieceId PieceId { get; }

        public IReadOnlyList<Point2D> Points { get; }

        public bool IsClosed { get; }

        /// <summary>
        /// When set, this curve is a CIRCLE of that diameter centred on its single point — not a polyline.
        ///
        /// A hole drilled along the camera is a circle, and a circle is what the drawing must carry: a polygon
        /// approximation would put a shape on the paper that the punch does not have, and whoever measured it
        /// would measure the polygon. It is still a neutral plan: a centre and a diameter name no AutoCAD type
        /// (ADR-0028, D3). A hole seen EDGE-ON gets no diameter here — it is drawn as its trace, see
        /// <see cref="CantileverViewPlanBuilder"/>.
        /// </summary>
        public double? CircleDiameter { get; }

        public bool IsCircle => CircleDiameter.HasValue;

        public override string ToString() =>
            Kind + " " + PieceId.Value +
            (IsCircle
                ? " (circulo d=" + CircleDiameter.Value.ToString("0.###", CultureInfo.InvariantCulture) + ")"
                : " (" + Points.Count + " puntos)");
    }

    /// <summary>
    /// A pure, deterministic plan of ONE view of a resolved Cantilever line.
    ///
    /// It contains no AutoCAD type, no block name, no layer, no colour and no transaction. Domain and
    /// Application do not draw (ADR-0028, D3); the Plugin turns these outlines into entities and is the only
    /// place that knows what a <c>Polyline</c> is.
    /// </summary>
    public sealed class CantileverViewPlan
    {
        internal CantileverViewPlan(
            CantileverViewKind view,
            int stationIndex,
            IReadOnlyList<CantileverViewCurve> curves,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            View = view;
            StationIndex = stationIndex;
            Curves = curves;
            Diagnostics = diagnostics;
        }

        public CantileverViewKind View { get; }

        /// <summary>
        /// The station this view is of, or −1 for a whole-line view.
        ///
        /// It is the number that gets stamped on <c>RackEmbedDocument.Section</c>: the frontal and the planta
        /// carry −1 because they are the whole line, and a lateral carries its station's index so an edit can
        /// redraw the right block in place (ADR-0028, D4).
        /// </summary>
        public int StationIndex { get; }

        public IReadOnlyList<CantileverViewCurve> Curves { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsEmpty => Curves.Count == 0;

        /// <summary>The plan's own extent, for a preview or a zoom.</summary>
        public Bounds2D Bounds => Curves.Count == 0
            ? new Bounds2D(0.0, 0.0, 0.0, 0.0)
            : Bounds2D.FromPoints(Curves.SelectMany(c => c.Points));

        public IEnumerable<CantileverViewCurve> Of(CantileverViewPieceKind kind) =>
            Curves.Where(c => c.Kind == kind);

        /// <summary>
        /// What this view WOULD draw, as text.
        ///
        /// Deterministic, so a test can assert that the same line always produces the same view and that a view
        /// is not accidentally sensitive to the order pieces happen to be enumerated in.
        /// </summary>
        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "{0};s={1};n={2};{3}",
            View, StationIndex, Curves.Count,
            string.Join("|", Curves.Select(c => string.Format(
                CultureInfo.InvariantCulture,
                "{0}:{1}:{2}:{3}{4}",
                c.Kind, c.PieceId.Value, c.Points.Count, c.IsClosed ? "C" : "O",
                c.IsCircle ? ":d=" + c.CircleDiameter.Value.ToString("0.####", CultureInfo.InvariantCulture) : string.Empty))));

        public override string ToString() =>
            View + (StationIndex < 0 ? " (linea)" : " (estacion " + (StationIndex + 1) + ")") +
            " " + Curves.Count + " curvas";
    }

    /// <summary>
    /// THE authority that turns a resolved line into the three views.
    ///
    /// Every structural member goes through <see cref="StructuralSectionPlanBuilder"/>, which owns the whole
    /// projection pipeline — mirror, rotation, lift to both ends of the run, place through the frame, project,
    /// flatten. Projecting a member here by hand would be a second implementation of that pipeline, and the two
    /// would disagree the first time a member was mirrored.
    ///
    /// The flat pieces are projected directly, because they already ARE outlines in world coordinates and there
    /// is no section to run through the pipeline. The projection they use is the SAME camera the members use,
    /// obtained from the same viewpoint, so a plate cannot come out in a different orientation from the profile
    /// it is bolted to.
    /// </summary>
    public static class CantileverViewPlanBuilder
    {
        /// <summary>
        /// The camera of each view.
        ///
        /// Written as explicit directions rather than reusing <c>SectionViewpoint.LongitudinalX</c> and its
        /// siblings: those are named for a SECTION's own axes and put the run across the picture, which is right
        /// for looking at one profile and wrong for a rack, where the columns must be vertical.
        /// </summary>
        public static SectionViewpoint Viewpoint(CantileverViewKind view)
        {
            switch (view)
            {
                case CantileverViewKind.Frontal:
                    // Looking towards +Y with Z up: the line's X runs across the picture, columns stand up.
                    return SectionViewpoint.Custom(Vector3D.UnitY, Vector3D.UnitZ);

                case CantileverViewKind.Lateral:
                    // Looking towards −X with Z up: the arms' depth runs across the picture.
                    return SectionViewpoint.Custom(new Vector3D(-1.0, 0.0, 0.0), Vector3D.UnitZ);

                case CantileverViewKind.Planta:
                    // Looking down, with +Y up the picture: the line runs across and the arms reach up it.
                    return SectionViewpoint.Custom(new Vector3D(0.0, 0.0, -1.0), Vector3D.UnitY);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(view), view, "La vista '" + view + "' no tiene camara declarada.");
            }
        }

        /// <summary>The section value a view stamps on its embed envelope.</summary>
        public static int SectionFor(CantileverViewKind view, int stationIndex) =>
            view == CantileverViewKind.Lateral ? stationIndex : -1;

        /// <summary>
        /// Builds one view.
        ///
        /// <paramref name="stationIndex"/> is read ONLY by the lateral. A frontal or a planta of "station 2" is
        /// not a thing — they are views of the line — and silently accepting an index for them would let a caller
        /// believe it had asked for something narrower than it got.
        /// </summary>
        public static CantileverViewPlan Build(
            CantileverLineAssembly line,
            CantileverViewKind view,
            StructuralSectionGeometryFactory geometryFactory,
            int stationIndex = 0)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (geometryFactory == null)
            {
                throw new ArgumentNullException(nameof(geometryFactory));
            }

            var diagnostics = new List<CantileverDiagnostic>();
            var curves = new List<CantileverViewCurve>();

            if (line.IsBlocked)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.LineStationBlocked,
                    "La linea no se resolvio; no hay nada que dibujar."));

                return new CantileverViewPlan(
                    view, SectionFor(view, stationIndex), curves, diagnostics);
            }

            var viewpoint = Viewpoint(view);
            var options = new SectionRepresentationOptions { Viewpoint = viewpoint };

            var stations = view == CantileverViewKind.Lateral
                ? OneStation(line, stationIndex, diagnostics)
                : line.Stations;

            foreach (var placement in stations)
            {
                AddStation(curves, placement, viewpoint, options, geometryFactory);
            }

            // A lateral looks at ONE station, so the bracing between stations is not part of it: what it would
            // show is a separator seen end-on, which is a line the length of a flange and tells the reader
            // nothing. The frontal is the view the bracing is FOR.
            if (view != CantileverViewKind.Lateral)
            {
                foreach (var interval in line.Intervals)
                {
                    AddInterval(curves, interval, viewpoint, options, geometryFactory);
                }
            }

            return new CantileverViewPlan(
                view, SectionFor(view, stationIndex), curves, diagnostics);
        }

        /// <summary>Every view of a line, with exactly one lateral: the one the caller asked for.</summary>
        public static IReadOnlyList<CantileverViewPlan> BuildInitialSet(
            CantileverLineAssembly line,
            StructuralSectionGeometryFactory geometryFactory,
            int lateralStationIndex = 0)
        {
            // ONE frontal, ONE planta and ONE lateral. Not N laterals: a line of twelve stations would drop
            // twelve blocks nobody asked for, and the lateral of a station is reachable by changing the index
            // (ADR-0028, D4).
            return new[]
            {
                Build(line, CantileverViewKind.Frontal, geometryFactory),
                Build(line, CantileverViewKind.Planta, geometryFactory),
                Build(line, CantileverViewKind.Lateral, geometryFactory, lateralStationIndex)
            };
        }

        private static IReadOnlyList<CantileverLineStationPlacement> OneStation(
            CantileverLineAssembly line, int stationIndex, ICollection<CantileverDiagnostic> diagnostics)
        {
            if (stationIndex >= 0 && stationIndex < line.Stations.Count)
            {
                return new[] { line.Stations[stationIndex] };
            }

            diagnostics.Add(CantileverDiagnostic.Blocking(
                CantileverDiagnostics.LineStationBlocked,
                "La linea no tiene una estacion " + (stationIndex + 1) + "; tiene " + line.Stations.Count + "."));

            return Array.Empty<CantileverLineStationPlacement>();
        }

        private static void AddStation(
            ICollection<CantileverViewCurve> curves,
            CantileverLineStationPlacement placement,
            SectionViewpoint viewpoint,
            SectionRepresentationOptions options,
            StructuralSectionGeometryFactory geometryFactory)
        {
            var station = placement.Station;
            var offset = placement.Offset;

            foreach (var member in station.Members)
            {
                AddMember(
                    curves, placement.ScopedId(member.Id), RoleOf(member.Role),
                    member, offset, options, geometryFactory);
            }

            foreach (var plate in station.Plates)
            {
                AddOutline(
                    curves, CantileverViewPieceKind.Plate, placement.ScopedId(plate.Id),
                    plate.Outline, offset, viewpoint, true);
            }

            foreach (var gusset in station.Gussets)
            {
                AddOutline(
                    curves, CantileverViewPieceKind.Gusset, placement.ScopedId(gusset.Id),
                    gusset.Vertices, offset, viewpoint, true);
            }

            // The holes. They were resolved from the first day and never asked for, which is why a column came
            // out as a bare outline: the drawing showed a profile where the product has a punched column
            // (motivo 5 del rechazo de la ronda 1). Nothing is recomputed here — every centre and every
            // diameter comes from the PunchPlan the station already resolved.
            foreach (var punch in station.Punches)
            {
                AddPunch(curves, placement.ScopedId(punch.Id), punch, offset, viewpoint);
            }
        }

        private static void AddInterval(
            ICollection<CantileverViewCurve> curves,
            CantileverIntervalAssembly interval,
            SectionViewpoint viewpoint,
            SectionRepresentationOptions options,
            StructuralSectionGeometryFactory geometryFactory)
        {
            var zero = new Vector3D(0.0, 0.0, 0.0);

            foreach (var plate in interval.ColumnPlates)
            {
                AddOutline(
                    curves, CantileverViewPieceKind.Plate, plate.Plate.Id,
                    plate.Plate.Outline, zero, viewpoint, true);

                // Its ONE centred hole. A separator plate without its hole is a 3 × 3 square that says nothing
                // about where the separator bolts, and that is what the frontal was showing.
                AddPunch(curves, plate.Punch.Id, plate.Punch, zero, viewpoint);
            }

            foreach (var separator in interval.Separators)
            {
                AddMember(
                    curves, separator.Member.Id, CantileverViewPieceKind.Separator,
                    separator.Member, zero, options, geometryFactory);

                foreach (var punch in separator.Punches)
                {
                    AddPunch(curves, punch.Id, punch, zero, viewpoint);
                }
            }

            foreach (var brace in interval.Braces)
            {
                if (brace.Member != null)
                {
                    AddMember(
                        curves, brace.Member.Id, CantileverViewPieceKind.Brace,
                        brace.Member, zero, options, geometryFactory);
                    continue;
                }

                // A cold-rolled rod has no catalogued section, so there is no contour to project: it is drawn as
                // its AXIS. Giving it a fictional circular section would put a shape on the drawing that no
                // catalogue row backs (ADR-0027, D7).
                foreach (var adapter in brace.Adapters)
                {
                    AddOutline(
                        curves, CantileverViewPieceKind.ColdRolledAdapter, adapter.Id,
                        AdapterOutline(adapter), zero, viewpoint, true);
                }

                AddOutline(
                    curves, CantileverViewPieceKind.Brace,
                    CantileverPieceId.Create(
                        CantileverPieceTokens.IntervalOwnerOf(brace.IntervalIndex),
                        CantileverPieceTokens.BraceToken(brace.PanelIndex, brace.Diagonal)),
                    new[] { brace.LowerEnd, brace.UpperEnd }, zero, viewpoint, false);
            }
        }

        /// <summary>
        /// The adapter as a square in the bracing plane, centred on its separator hole.
        ///
        /// It is a REPRESENTATION and says so: an angle cut to two inches, seen from the front, is a 2 × 2
        /// square, and the MVP draws that square rather than modelling the heel, the two legs and their fillet.
        /// Anything finer would need a contour authority for a fabricated part, which is not a thing this system
        /// has and not a thing I-37D may invent.
        /// </summary>
        private static IReadOnlyList<Point3D> AdapterOutline(CantileverColdRolledAdapterPlan adapter)
        {
            var half = adapter.CutLength / 2.0;
            var centre = adapter.Origin;

            return new[]
            {
                new Point3D(centre.X - half, centre.Y, centre.Z - half),
                new Point3D(centre.X + half, centre.Y, centre.Z - half),
                new Point3D(centre.X + half, centre.Y, centre.Z + half),
                new Point3D(centre.X - half, centre.Y, centre.Z + half)
            };
        }

        private static void AddMember(
            ICollection<CantileverViewCurve> curves,
            CantileverPieceId id,
            CantileverViewPieceKind kind,
            CantileverStructuralMemberPlan member,
            Vector3D offset,
            SectionRepresentationOptions options,
            StructuralSectionGeometryFactory geometryFactory)
        {
            var geometry = geometryFactory.Get(member.Placement.SectionId, options.Detail);

            // The line's translation goes through the FRAME AUTHORITY, which is where the reasoning for applying
            // it to the frame rather than to the projected points is written down.
            var placement = CantileverLineFrameResolver.Translated(member.Placement, offset);
            var plan = StructuralSectionPlanBuilder.Build(geometry, placement, options);

            foreach (var curve in plan.Curves)
            {
                curves.Add(new CantileverViewCurve(kind, id, curve.Points, curve.IsClosed));
            }
        }

        /// <summary>
        /// How much of a hole's own axis survives the projection before it is drawn as a circle.
        ///
        /// Zero would mean «only a perfectly axial hole is a circle», which no real camera gives; one would mean
        /// «every hole is a circle», which would draw a full circle where the reader can only see an edge. A
        /// fifth of the diameter is the line between the two, and it is a VIEWING convention: it changes what is
        /// drawn, never what was resolved.
        /// </summary>
        private const double CircleForeshorteningLimit = 0.2;

        /// <summary>
        /// One hole, drawn as what the camera can actually see of it.
        ///
        /// Looking DOWN the drilling axis, a hole is a circle of its real diameter, and that is what gets
        /// emitted — a real circle, not a polygon that approximates one. Seen EDGE-ON it is not a circle at all,
        /// and drawing one would put a shape on the paper the piece does not have; it is drawn as its TRACE
        /// instead: a segment as long as the diameter, across the hole, exactly where the material is pierced.
        /// That is the same discipline the cold-rolled rod follows when it is drawn as its axis (ADR-0027, D7):
        /// draw what is there, declare the convention, and never invent the rest.
        ///
        /// A hole is never omitted for being small. That was motivo 5 of the round-1 rejection.
        /// </summary>
        private static void AddPunch(
            ICollection<CantileverViewCurve> curves,
            CantileverPieceId id,
            CantileverPunchPlan punch,
            Vector3D offset,
            SectionViewpoint viewpoint)
        {
            if (!(punch.Diameter > 0.0))
            {
                return; // a hole with no diameter is not a hole; the resolver already reported it
            }

            var centre = punch.Centre + offset;
            var alongAxis = centre + punch.Direction * punch.Diameter;

            var projectedCentre = viewpoint.Project(centre);
            var projectedTip = viewpoint.Project(alongAxis);

            var dx = projectedTip.X - projectedCentre.X;
            var dy = projectedTip.Y - projectedCentre.Y;
            var foreshortening = Math.Sqrt(dx * dx + dy * dy) / punch.Diameter;

            if (foreshortening <= CircleForeshorteningLimit)
            {
                curves.Add(new CantileverViewCurve(
                    CantileverViewPieceKind.Punch, id, new[] { projectedCentre }, false, punch.Diameter));
                return;
            }

            // Edge-on: the trace is PERPENDICULAR to the projected axis, so it reads as the mouth of the hole
            // and not as a piece of the axis itself.
            var length = Math.Sqrt(dx * dx + dy * dy);
            var nx = -dy / length;
            var ny = dx / length;
            var half = punch.Diameter / 2.0;

            curves.Add(new CantileverViewCurve(
                CantileverViewPieceKind.Punch,
                id,
                new[]
                {
                    new Point2D(projectedCentre.X - nx * half, projectedCentre.Y - ny * half),
                    new Point2D(projectedCentre.X + nx * half, projectedCentre.Y + ny * half)
                },
                false));
        }

        private static void AddOutline(
            ICollection<CantileverViewCurve> curves,
            CantileverViewPieceKind kind,
            CantileverPieceId id,
            IReadOnlyList<Point3D> outline,
            Vector3D offset,
            SectionViewpoint viewpoint,
            bool isClosed)
        {
            if (outline == null || outline.Count == 0)
            {
                return;
            }

            var projected = outline
                .Select(p => viewpoint.Project(p + offset))
                .ToList();

            curves.Add(new CantileverViewCurve(kind, id, projected, isClosed));
        }

        private static CantileverViewPieceKind RoleOf(CantileverMemberRole role)
        {
            switch (role)
            {
                case CantileverMemberRole.Column:
                    return CantileverViewPieceKind.Column;
                case CantileverMemberRole.Base:
                    return CantileverViewPieceKind.Base;
                case CantileverMemberRole.Arm:
                    return CantileverViewPieceKind.Arm;
                case CantileverMemberRole.Separator:
                    return CantileverViewPieceKind.Separator;
                case CantileverMemberRole.Brace:
                    return CantileverViewPieceKind.Brace;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role), role, "El rol '" + role + "' no tiene tipo de pieza de vista.");
            }
        }
    }
}
