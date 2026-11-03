using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The Cantilever source of the neutral analysis model (I-65 D-05, D-10, D-10b and D-13; F1-T2).
    ///
    /// <para>
    /// It receives the design ALREADY deserialised and the catalogue: reading the envelope and decoding bytes belong to the
    /// neutral layer (CD-I65-F1-03). It resolves the line with the one resolver of the system and the production policies, takes
    /// the station of the scope in the station's OWN coordinates (the datum, before the line offset), and idealises it.
    /// </para>
    /// <para>
    /// The idealisation (IDL-01..08) is a pure function of the resolved PLACEMENTS: the analytic axis of a member is the
    /// centroidal axis of its placement, never the geometry of a block. The column is one member split at the joint of every arm;
    /// the arm goes from that joint (the prolonged arm axis meets the column axis in the plane of the station) to the end of its
    /// physical length. The base, the plates, the spacers, the bracing and the gussets are not modelled and do not block the
    /// station (CD-I65-F1-14).
    /// </para>
    /// </summary>
    public sealed class CantileverStructuralModelSource : IStructuralModelSource
    {
        /// <summary>Joints closer than this are the same node (IDL-08), in inches.</summary>
        private const double FusionTolerance = 0.001;

        /// <summary>The least distance between two nodes of the column (IDL-08), in inches.</summary>
        private const double MinimumColumnSeparation = 0.05;

        /// <summary>The most an axis may lie away from the plane of the station (IDL-08), in inches.</summary>
        private const double PlaneTolerance = 0.01;

        /// <summary>The most an axis may lean out of the plane of the station, and the least two axes may differ by (IDL-08), in degrees.</summary>
        private const double AngularToleranceDegrees = 0.01;

        /// <summary>The steepest arm the first version admits (IDL-10), in degrees from the horizontal.</summary>
        private const double MaximumArmInclinationDegrees = 10.0;

        /// <summary>The canonical kind of the scope this source answers to; the entry translates the kind persisted in the envelope to it.</summary>
        internal const string KindName = "Cantilever";

        private const string LineSubject = "line";
        private const string SideToken = "PositiveY";
        private const string FootId = "COL.FOOT";
        private const string TopId = "COL.TOP";

        private const string FootSupport = "SUP-01";
        private const string PlanarSupport = "SUP-03";
        private const string ContinuousConnection = "CON-01";

        private readonly CantileverLineDesign _design;
        private readonly StructuralSectionCatalog _catalog;

        public CantileverStructuralModelSource(CantileverLineDesign design, StructuralSectionCatalog catalog)
        {
            _design = design ?? throw new ArgumentNullException(nameof(design));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (loads == null)
            {
                throw new ArgumentNullException(nameof(loads));
            }

            if (!string.Equals(scope.Kind, KindName, StringComparison.Ordinal))
            {
                return Refuse(Blocking(StructuralDiagnosticCodes.SourceKindMismatch, StructuralExportOutcome.Invalid, scope.Kind));
            }

            var stationSubject = "station-" + scope.StationIndex;

            if (scope.StationIndex >= _design.StationCount)
            {
                return Refuse(Blocking(StructuralDiagnosticCodes.SourceStationRange, StructuralExportOutcome.Invalid, stationSubject));
            }

            var line = CantileverLineResolver.Resolve(
                _design,
                _catalog,
                new StructuralSectionGeometryFactory(_catalog),
                CantileverCataloguePolicies.ColumnBase(_catalog),
                CantileverCataloguePolicies.Arm(_catalog));

            // A65-F1-11: a block of the whole line names the line; only a station that is blocked itself names "station-N".
            if (line.IsBlocked)
            {
                return Refuse(Blocking(StructuralDiagnosticCodes.SourceLineBlocked, StructuralExportOutcome.Unsupported, LineSubject));
            }

            if (scope.StationIndex >= line.Stations.Count || line.Stations[scope.StationIndex].Station.IsBlocked)
            {
                return Refuse(Blocking(StructuralDiagnosticCodes.SourceStationBlocked, StructuralExportOutcome.Unsupported, stationSubject));
            }

            var placement = line.Stations[scope.StationIndex];
            var station = placement.Station;
            var diagnostics = new List<StructuralDiagnostic>();

            // A65-F1-12: every structural piece the resolver produces for the station is either modelled or one of the pieces
            // CD-I65-F1-14 leaves out; a piece that is neither makes the station Unsupported.
            diagnostics.AddRange(InventoryDiagnostics(
                station.Members.Select(m => new KeyValuePair<string, CantileverMemberRole>(placement.ScopedId(m.Id).Value, m.Role))));

            // When the inventory already refused the station, the face and the arrangement are not looked at.
            if (diagnostics.Count == 0 && station.FaceMode == CantileverStationFaceMode.Double)
            {
                diagnostics.Add(Blocking(StructuralDiagnosticCodes.StationFaceUnsupported, StructuralExportOutcome.Unsupported, stationSubject));
            }
            else if (diagnostics.Count == 0 && station.SingleSide != CantileverArmSide.PositiveY)
            {
                diagnostics.Add(Blocking(StructuralDiagnosticCodes.StationSideUnsupported, StructuralExportOutcome.Unsupported, stationSubject));
            }

            var arms = new List<StationPiece>();

            if (diagnostics.Count == 0)
            {
                for (var index = 0; index < station.Levels.Count; index++)
                {
                    var arm = station.Levels[index].Arm(CantileverArmSide.PositiveY);

                    if (arm == null
                        || arm.Body == null
                        || arm.Body.Arrangement != CantileverArmBodyArrangement.Single
                        || arm.Body.Members.Count != 1)
                    {
                        diagnostics.Add(Blocking(
                            StructuralDiagnosticCodes.ArmArrangementUnsupported, StructuralExportOutcome.Unsupported, ArmId(index + 1)));
                        continue;
                    }

                    var member = arm.Body.Members[0];
                    arms.Add(new StationPiece(placement.ScopedId(member.Id).Value, member.Placement));
                }
            }

            if (diagnostics.Count > 0)
            {
                return Refuse(diagnostics.ToArray());
            }

            var column = station.ColumnBase.Column;

            return Idealise(new StationPiece(placement.ScopedId(column.Id).Value, column.Placement), arms, loads);
        }

        /// <summary>One resolved piece of the station as the idealisation sees it: its scoped id and its placement.</summary>
        internal sealed class StationPiece
        {
            public StationPiece(string pieceId, PrismaticSectionInstance placement)
            {
                PieceId = pieceId ?? throw new ArgumentNullException(nameof(pieceId));
                Placement = placement ?? throw new ArgumentNullException(nameof(placement));
            }

            public string PieceId { get; }

            public PrismaticSectionInstance Placement { get; }
        }

        /// <summary>
        /// The pure idealisation of one station of a single face: the column and the arms of the positive side, level by level
        /// (the first arm is level 1), plus the loads of the file. Every blocking finding comes back as a diagnostic and the
        /// description then carries nothing else.
        /// </summary>
        internal static StructuralSourceDescription Idealise(
            StationPiece column, IReadOnlyList<StationPiece> arms, StructuralLoadFile loads)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (arms == null)
            {
                throw new ArgumentNullException(nameof(arms));
            }

            if (loads == null)
            {
                throw new ArgumentNullException(nameof(loads));
            }

            var diagnostics = new List<StructuralDiagnostic>();

            CheckAxis(column.PieceId, column.Placement, diagnostics);

            for (var index = 0; index < arms.Count; index++)
            {
                CheckAxis(ArmId(index + 1), arms[index].Placement, diagnostics);
                CheckInclination(ArmId(index + 1), arms[index].Placement, diagnostics);
            }

            if (diagnostics.Count > 0)
            {
                return Refuse(diagnostics.ToArray());
            }

            var columnStart = column.Placement.Frame.Origin;
            var columnEnd = column.Placement.Frame.ToWorld(new Point3D(0.0, 0.0, column.Placement.Length));
            var columnLength = Math.Sqrt(Math.Pow(columnEnd.Y - columnStart.Y, 2) + Math.Pow(columnEnd.Z - columnStart.Z, 2));
            var columnY = (columnEnd.Y - columnStart.Y) / columnLength;
            var columnZ = (columnEnd.Z - columnStart.Z) / columnLength;

            // IDL-04: the joint of every arm is where its prolonged axis meets the column axis, in the Y-Z plane.
            var joints = new List<Joint>();
            var parallel = Math.Sin(AngularToleranceDegrees * Math.PI / 180.0);

            for (var index = 0; index < arms.Count; index++)
            {
                var frame = arms[index].Placement.Frame;
                var armNorm = Math.Sqrt((frame.AxisZ.Y * frame.AxisZ.Y) + (frame.AxisZ.Z * frame.AxisZ.Z));
                var armY = frame.AxisZ.Y / armNorm;
                var armZ = frame.AxisZ.Z / armNorm;
                var cross = (columnY * armZ) - (columnZ * armY);

                if (Math.Abs(cross) < parallel)
                {
                    diagnostics.Add(Blocking(StructuralDiagnosticCodes.AxesParallel, StructuralExportOutcome.Unsupported, ArmId(index + 1)));
                    continue;
                }

                var offsetY = frame.Origin.Y - columnStart.Y;
                var offsetZ = frame.Origin.Z - columnStart.Z;
                var along = ((offsetY * armZ) - (offsetZ * armY)) / cross;

                if (along < -FusionTolerance || along > columnLength + FusionTolerance)
                {
                    diagnostics.Add(Blocking(StructuralDiagnosticCodes.NodeOutsideColumn, StructuralExportOutcome.Invalid, ArmId(index + 1)));
                    continue;
                }

                joints.Add(new Joint(index + 1, along));
            }

            if (diagnostics.Count > 0)
            {
                return Refuse(diagnostics.ToArray());
            }

            // IDL-02, IDL-03 and IDL-08: the column nodes from the foot up, fusing what is closer than the fusion tolerance and
            // refusing (never fusing silently) what is closer than the minimum separation.
            var columnNodes = new List<ColumnNode> { new ColumnNode(FootId, 0.0, columnStart.Y, columnStart.Z) };
            var jointNode = new Dictionary<int, string>();

            foreach (var joint in joints.OrderBy(j => j.Along).ThenBy(j => j.Level))
            {
                if (columnLength - joint.Along < FusionTolerance)
                {
                    jointNode[joint.Level] = TopId;
                    continue;
                }

                var last = columnNodes[columnNodes.Count - 1];

                if (joint.Along - last.Along < FusionTolerance)
                {
                    jointNode[joint.Level] = last.Id;
                    continue;
                }

                var id = ArmId(joint.Level) + ".J";
                columnNodes.Add(new ColumnNode(id, joint.Along, columnStart.Y + (joint.Along * columnY), columnStart.Z + (joint.Along * columnZ)));
                jointNode[joint.Level] = id;
            }

            columnNodes.Add(new ColumnNode(TopId, columnLength, columnEnd.Y, columnEnd.Z));

            for (var index = 1; index < columnNodes.Count; index++)
            {
                if (columnNodes[index].Along - columnNodes[index - 1].Along < MinimumColumnSeparation)
                {
                    diagnostics.Add(Blocking(StructuralDiagnosticCodes.NodeSeparation, StructuralExportOutcome.Invalid, columnNodes[index].Id));
                }
            }

            if (diagnostics.Count > 0)
            {
                return Refuse(diagnostics.ToArray());
            }

            var positions = columnNodes.ToDictionary(n => n.Id, n => new Position(n.Y, n.Z), StringComparer.Ordinal);
            var nodes = columnNodes.Select(n => Node(n.Id, n.Y, n.Z)).ToList();
            var tips = new List<ColumnNode>();

            for (var index = 0; index < arms.Count; index++)
            {
                var frame = arms[index].Placement.Frame;
                var end = frame.ToWorld(new Point3D(0.0, 0.0, arms[index].Placement.Length));
                var tip = new ColumnNode(ArmId(index + 1) + ".TIP", 0.0, end.Y, end.Z);

                tips.Add(tip);
                positions[tip.Id] = new Position(tip.Y, tip.Z);
                nodes.Add(Node(tip.Id, tip.Y, tip.Z));
            }

            var members = new List<StructuralSourceMember>();
            var columnPeralte = PeralteAxis(column.Placement);

            for (var index = 1; index < columnNodes.Count; index++)
            {
                var start = columnNodes[index - 1];
                var finish = columnNodes[index];

                if (!TryPerpendicular(columnPeralte, finish.Y - start.Y, finish.Z - start.Z, out var columnDepth))
                {
                    diagnostics.Add(Blocking(
                        StructuralDiagnosticCodes.PeralteReorthogonalisation, StructuralExportOutcome.Unsupported, "COL." + index));
                    continue;
                }

                members.Add(new StructuralSourceMember(
                    "COL." + index,
                    start.Id,
                    finish.Id,
                    column.Placement.SectionId,
                    columnDepth,
                    StructuralMemberRole.Column,
                    column.PieceId,
                    StructuralMemberReleases.None(ContinuousConnection)));
            }

            var rigid = new Dictionary<int, double>();

            for (var index = 0; index < arms.Count; index++)
            {
                var level = index + 1;
                var piece = arms[index];
                var start = positions[jointNode[level]];
                var tip = tips[index];
                var armNorm = Math.Sqrt((piece.Placement.Frame.AxisZ.Y * piece.Placement.Frame.AxisZ.Y) + (piece.Placement.Frame.AxisZ.Z * piece.Placement.Frame.AxisZ.Z));

                // IDL-06: the rigid distance from the joint to the physical start of the arm, along its axis.
                rigid[level] = ((piece.Placement.Frame.Origin.Y - start.Y) * piece.Placement.Frame.AxisZ.Y / armNorm)
                    + ((piece.Placement.Frame.Origin.Z - start.Z) * piece.Placement.Frame.AxisZ.Z / armNorm);

                if (!TryPerpendicular(PeralteAxis(piece.Placement), tip.Y - start.Y, tip.Z - start.Z, out var armDepth))
                {
                    diagnostics.Add(Blocking(
                        StructuralDiagnosticCodes.PeralteReorthogonalisation, StructuralExportOutcome.Unsupported, ArmId(level)));
                    continue;
                }

                members.Add(new StructuralSourceMember(
                    ArmId(level),
                    jointNode[level],
                    tip.Id,
                    piece.Placement.SectionId,
                    armDepth,
                    StructuralMemberRole.Arm,
                    piece.PieceId,
                    StructuralMemberReleases.None(ContinuousConnection)));
            }

            // D-10: the foot is fixed; every other node restrains Tx, Ry and Rz (flags in the order Tx, Ty, Tz, Rx, Ry, Rz).
            var supports = nodes
                .Select(node => node.Id == FootId
                    ? new StructuralSupport(node.Id, new[] { true, true, true, true, true, true }, FootSupport)
                    : new StructuralSupport(node.Id, new[] { true, false, false, false, true, true }, PlanarSupport))
                .ToList();

            // D-10b: the loads come ONLY from the file; the position is the rigid distance plus the fraction of the physical length.
            var pointLoads = new List<StructuralMemberPointLoad>();

            foreach (var entry in loads.Loads)
            {
                if (entry.Side != StructuralLoadSide.PositiveY || entry.Level < 1 || entry.Level > arms.Count)
                {
                    diagnostics.Add(Blocking(StructuralDiagnosticCodes.LoadTargetUnknown, StructuralExportOutcome.Invalid, entry.Id));
                    continue;
                }

                var distance = rigid[entry.Level] + (entry.PositionFraction * arms[entry.Level - 1].Placement.Length);

                pointLoads.Add(new StructuralMemberPointLoad(
                    entry.Id, ArmId(entry.Level), entry.Magnitude, StructuralLength.FromInches(distance)));
            }

            if (diagnostics.Count > 0)
            {
                return Refuse(diagnostics.ToArray());
            }

            return new StructuralSourceDescription(nodes, members, supports, Assumptions(), pointLoads, new StructuralDiagnostic[0]);
        }

        /// <summary>
        /// The unit vector, in the global frame, of the depth axis of a placement: the local depth direction (0, 1) carried
        /// through the section transform (the mirror, then the rotation) and then through the frame (INV-07). It never looks at
        /// the geometry of a block.
        /// </summary>
        internal static StructuralDirection PeralteAxis(PrismaticSectionInstance placement)
        {
            if (placement == null)
            {
                throw new ArgumentNullException(nameof(placement));
            }

            var local = placement.SectionTransform().Apply(new Vector2D(0.0, 1.0));
            var world = (placement.Frame.AxisX * local.X) + (placement.Frame.AxisY * local.Y);

            return new StructuralDirection(world.X + 0.0, world.Y + 0.0, world.Z + 0.0);
        }

        /// <summary>What the idealisation does with one piece the resolver produces for a station (A65-F1-12, CD-I65-F1-14).</summary>
        internal enum StationPieceDisposition
        {
            Modelled = 1,
            NotModelled = 2,
            Unclassified = 3,
        }

        /// <summary>
        /// The pure classification of a station piece by its role (A65-F1-12, CD-I65-F1-14). The column and the body of the arm are
        /// modelled. The base, the separators, the bracing and the end angles of a cold-rolled tie are the pieces CD-I65-F1-14
        /// leaves out; the plates and the gussets are not members and are always left out. Any other role, including one defined
        /// in the future, is unclassified and refuses the station: nothing structural is ignored without a decision.
        /// </summary>
        internal static StationPieceDisposition Classify(CantileverMemberRole role)
        {
            switch (role)
            {
                case CantileverMemberRole.Column:
                case CantileverMemberRole.Arm:
                    return StationPieceDisposition.Modelled;

                case CantileverMemberRole.Base:
                case CantileverMemberRole.Separator:
                case CantileverMemberRole.Brace:
                case CantileverMemberRole.ColdRolledAdapter:
                    return StationPieceDisposition.NotModelled;

                default:
                    return StationPieceDisposition.Unclassified;
            }
        }

        /// <summary>One blocking finding per piece (id and role) that is neither modelled nor left out by CD-I65-F1-14.</summary>
        internal static IReadOnlyList<StructuralDiagnostic> InventoryDiagnostics(IEnumerable<KeyValuePair<string, CantileverMemberRole>> pieces)
        {
            if (pieces == null)
            {
                throw new ArgumentNullException(nameof(pieces));
            }

            return pieces
                .Where(piece => Classify(piece.Value) == StationPieceDisposition.Unclassified)
                .Select(piece => Blocking(
                    StructuralDiagnosticCodes.StationPieceUnclassified, StructuralExportOutcome.Unsupported, piece.Key))
                .ToList();
        }

        /// <summary>
        /// The depth axis made exactly perpendicular to the analytic axis of the member, which runs between two nodes in the plane
        /// (A65-F1-10). The component that is removed is bounded by the angular tolerance of IDL-08: inside it only a
        /// rounding-sized component goes away and the result is the one it always was; a larger one is not corrected in silence
        /// and the caller refuses the station. A member with no length is left as it is, for the model builder to refuse.
        /// </summary>
        internal static bool TryPerpendicular(StructuralDirection depth, double memberY, double memberZ, out StructuralDirection result)
        {
            var length = Math.Sqrt((memberY * memberY) + (memberZ * memberZ));

            if (!(length > 0))
            {
                result = depth;
                return true;
            }

            var unitY = memberY / length;
            var unitZ = memberZ / length;
            var along = (depth.Y * unitY) + (depth.Z * unitZ);

            if (Math.Abs(along) > Math.Sin(AngularToleranceDegrees * Math.PI / 180.0))
            {
                result = depth;
                return false;
            }

            var x = depth.X;
            var y = depth.Y - (along * unitY);
            var z = depth.Z - (along * unitZ);
            var norm = Math.Sqrt((x * x) + (y * y) + (z * z));

            result = norm > 1e-6 ? new StructuralDirection((x / norm) + 0.0, (y / norm) + 0.0, (z / norm) + 0.0) : depth;
            return true;
        }

        private static string ArmId(int level) => "ARM." + level + "." + SideToken;

        private static StructuralSourceDescription Refuse(params StructuralDiagnostic[] diagnostics) =>
            new StructuralSourceDescription(
                new StructuralNode[0],
                new StructuralSourceMember[0],
                new StructuralSupport[0],
                new AnalysisAssumption[0],
                new StructuralMemberPointLoad[0],
                diagnostics);

        private static StructuralDiagnostic Blocking(string code, StructuralExportOutcome outcome, string subject) =>
            StructuralDiagnostic.Blocking(code, outcome, subject);

        private static StructuralNode Node(string id, double y, double z) =>
            new StructuralNode(
                id,
                new StructuralPosition(StructuralLength.FromInches(0.0), StructuralLength.FromInches(y + 0.0), StructuralLength.FromInches(z + 0.0)));

        /// <summary>IDL-08: an axis lies in the plane of the station (x = 0) within the position and the angular tolerance.</summary>
        private static void CheckAxis(string subject, PrismaticSectionInstance placement, List<StructuralDiagnostic> diagnostics)
        {
            var start = placement.Frame.Origin;
            var end = placement.Frame.ToWorld(new Point3D(0.0, 0.0, placement.Length));

            if (Math.Max(Math.Abs(start.X), Math.Abs(end.X)) > PlaneTolerance)
            {
                diagnostics.Add(Blocking(StructuralDiagnosticCodes.NodePlaneDeviation, StructuralExportOutcome.Unsupported, subject));
            }

            var leaning = Math.Asin(Math.Min(1.0, Math.Abs(placement.Frame.AxisZ.X) / placement.Frame.AxisZ.Length)) * 180.0 / Math.PI;

            if (leaning > AngularToleranceDegrees)
            {
                diagnostics.Add(Blocking(StructuralDiagnosticCodes.AxisAngleDeviation, StructuralExportOutcome.Unsupported, subject));
            }
        }

        /// <summary>IDL-10: an arm steeper than the limit from the horizontal is outside the first version.</summary>
        private static void CheckInclination(string subject, PrismaticSectionInstance placement, List<StructuralDiagnostic> diagnostics)
        {
            var axis = placement.Frame.AxisZ;
            var degrees = Math.Atan2(Math.Abs(axis.Z), Math.Abs(axis.Y)) * 180.0 / Math.PI;

            if (degrees > MaximumArmInclinationDegrees)
            {
                diagnostics.Add(Blocking(StructuralDiagnosticCodes.ArmSlopeUnsupported, StructuralExportOutcome.Unsupported, subject));
            }
        }

        private static IReadOnlyList<AnalysisAssumption> Assumptions() => new[]
        {
            new AnalysisAssumption(FootSupport, "COL.FOOT is fixed in its six degrees of freedom. Declared limitation: no anchor, base plate or slab stiffness is represented."),
            new AnalysisAssumption("SUP-02", "The base is not modelled; the idealisation starts at the column foot. Declared limitation: the real base bears on the floor."),
            new AnalysisAssumption(PlanarSupport, "Plane analysis: every node other than the foot restrains translation along X and rotation about Y and Z. Declared limitation: no out-of-plane buckling and no bracing effect are represented."),
            new AnalysisAssumption(ContinuousConnection, "The arm-to-column connection is continuous: no member end is released. Declared limitation: the real connection is semi-rigid."),
            new AnalysisAssumption("IDL-NO-OFFSETS", "No rigid offsets: each analytical arm runs from the column axis to its tip and includes half the column and the plate. Declared limitation: the arm flexibility is overestimated, and no conclusion about real behaviour is drawn from V1."),
            new AnalysisAssumption("ASM-PROP-01", "Section and material properties used by the analysis are those of the analysis program database, selected by the section and material names of the verified mapping rows. Declared limitation: database editions and material values may differ from the usual ones; they are compared against the catalogue and against the expected values with their cited source."),
            new AnalysisAssumption("LOD-01", "A single load case REF1 carries the loads of the load file; there is no self weight and there are no combinations. Declared limitation: the loads are not derived from pallets, impact, seismic action or tributary areas."),
        };

        private readonly struct Joint
        {
            public Joint(int level, double along)
            {
                Level = level;
                Along = along;
            }

            public int Level { get; }

            public double Along { get; }
        }

        private readonly struct Position
        {
            public Position(double y, double z)
            {
                Y = y;
                Z = z;
            }

            public double Y { get; }

            public double Z { get; }
        }

        private sealed class ColumnNode
        {
            public ColumnNode(string id, double along, double y, double z)
            {
                Id = id;
                Along = along;
                Y = y;
                Z = z;
            }

            public string Id { get; }

            public double Along { get; }

            public double Y { get; }

            public double Z { get; }
        }
    }
}
