using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// What identifies the station being exported, as PRIMITIVE values (CD-I65-F1-03 and CD-I65-F1-10): the rack id of the
    /// envelope, the kind, the station index and the exact bytes of the persisted design.
    /// </summary>
    public sealed class StructuralScope
    {
        private readonly byte[] _designBytes;

        public StructuralScope(string rackId, string kind, int stationIndex, byte[] designBytes)
        {
            if (stationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stationIndex), stationIndex, "The station index cannot be negative.");
            }

            RackId = StructuralRequire.Text(rackId, nameof(rackId));
            Kind = StructuralRequire.Text(kind, nameof(kind));
            StationIndex = stationIndex;
            _designBytes = (byte[])StructuralRequire.NotNull(designBytes, nameof(designBytes)).Clone();
        }

        public string RackId { get; }

        public string Kind { get; }

        public int StationIndex { get; }

        /// <summary>A copy of the exact bytes of the persisted design.</summary>
        public byte[] GetDesignBytes() => (byte[])_designBytes.Clone();
    }

    /// <summary>A member as a source describes it: everything except the material, which only the profile assigns.</summary>
    public sealed class StructuralSourceMember
    {
        public StructuralSourceMember(
            string id,
            string nodeIId,
            string nodeJId,
            StructuralSectionId sectionId,
            StructuralDirection peralteAxis,
            StructuralMemberRole role,
            string originPiece,
            StructuralMemberReleases releases)
        {
            if (sectionId.IsEmpty)
            {
                throw new ArgumentException("A member needs a section.", nameof(sectionId));
            }

            Id = StructuralRequire.Text(id, nameof(id));
            NodeIId = StructuralRequire.Text(nodeIId, nameof(nodeIId));
            NodeJId = StructuralRequire.Text(nodeJId, nameof(nodeJId));
            SectionId = sectionId;
            PeralteAxis = peralteAxis;
            Role = role;
            OriginPiece = StructuralRequire.Text(originPiece, nameof(originPiece));
            Releases = StructuralRequire.NotNull(releases, nameof(releases));
        }

        public string Id { get; }

        public string NodeIId { get; }

        public string NodeJId { get; }

        public StructuralSectionId SectionId { get; }

        public StructuralDirection PeralteAxis { get; }

        public StructuralMemberRole Role { get; }

        public string OriginPiece { get; }

        public StructuralMemberReleases Releases { get; }
    }

    /// <summary>What a source returns: the idealised structure with the loads already placed, or diagnostics.</summary>
    public sealed class StructuralSourceDescription
    {
        public StructuralSourceDescription(
            IEnumerable<StructuralNode> nodes,
            IEnumerable<StructuralSourceMember> members,
            IEnumerable<StructuralSupport> supports,
            IEnumerable<AnalysisAssumption> assumptions,
            IEnumerable<StructuralMemberPointLoad> loads,
            IEnumerable<StructuralDiagnostic> diagnostics)
        {
            Nodes = StructuralRequire.Items(nodes, nameof(nodes));
            Members = StructuralRequire.Items(members, nameof(members));
            Supports = StructuralRequire.Items(supports, nameof(supports));
            Assumptions = StructuralRequire.Items(assumptions, nameof(assumptions));
            Loads = StructuralRequire.Items(loads, nameof(loads));
            Diagnostics = StructuralRequire.Items(diagnostics, nameof(diagnostics));
        }

        public IReadOnlyList<StructuralNode> Nodes { get; }

        public IReadOnlyList<StructuralSourceMember> Members { get; }

        public IReadOnlyList<StructuralSupport> Supports { get; }

        public IReadOnlyList<AnalysisAssumption> Assumptions { get; }

        public IReadOnlyList<StructuralMemberPointLoad> Loads { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }
    }

    /// <summary>
    /// The neutral source of the idealised structure (I-65 D-13). It resolves the target of each load (level and side) and
    /// places it. In F1-T1 only a test double implements it; the Cantilever source arrives with F1-T2.
    /// </summary>
    public interface IStructuralModelSource
    {
        StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads);
    }

    /// <summary>
    /// The pure construction of the neutral model (I-65 D-13). There is no analysis, no result reading, no file writing
    /// and nothing of AutoCAD here: a source, a scope, the loads, the explicit material profile and the catalogue go in;
    /// a result comes out that exposes the model only when it is Valid.
    /// </summary>
    public static class StructuralModelBuilder
    {
        public const string GeneratorVersion = "rackcad-structural-model/1";

        public const int MaxPointLoadsPerMember = 24;

        private const double Tolerance = 1e-9;

        public static StructuralModelResult BuildStructuralModel(
            IStructuralModelSource source,
            StructuralScope scope,
            StructuralLoadFileReadResult loads,
            IStructuralMaterialProfile profile,
            StructuralSectionCatalog catalog)
        {
            StructuralRequire.NotNull(source, nameof(source));
            StructuralRequire.NotNull(scope, nameof(scope));
            StructuralRequire.NotNull(loads, nameof(loads));
            StructuralRequire.NotNull(profile, nameof(profile));
            StructuralRequire.NotNull(catalog, nameof(catalog));

            if (loads.Outcome != StructuralExportOutcome.Valid || loads.LoadFile == null)
            {
                return StructuralModelResult.Create(loads.Diagnostics, null);
            }

            StructuralSourceDescription description;

            try
            {
                description = source.Describe(scope, loads.LoadFile);
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception)
            {
                // A65-F1-09: a source that fails is not a model and not a crash; the caller gets the visible finding.
                description = null;
            }

            if (description == null)
            {
                return StructuralModelResult.Create(
                    new[] { StructuralDiagnostic.Blocking(StructuralDiagnosticCodes.SourceUnavailable, StructuralExportOutcome.Unavailable, "source") },
                    null);
            }

            var diagnostics = new List<StructuralDiagnostic>(description.Diagnostics);

            CheckPopulation(description, diagnostics);

            var nodes = IndexNodes(description, diagnostics);
            var members = CheckMembers(description, nodes, diagnostics);

            CheckSupports(description, nodes, diagnostics);
            CheckLoads(description, members, diagnostics);

            var rows = CheckSections(description, profile, catalog, diagnostics, out var sections);

            if (StructuralModelResult.Aggregate(diagnostics) != StructuralExportOutcome.Valid)
            {
                return StructuralModelResult.Create(diagnostics, null);
            }

            return StructuralModelResult.Create(diagnostics, Assemble(scope, loads.LoadFile, description, rows, sections, diagnostics));
        }

        // ---------------------------------------------------------------- structure

        /// <summary>
        /// A65-F1-09: what a source may not leave out or contradict. An empty model, two supports on one node and a reference to an
        /// assumption that is not declared are findings of the description, never a model. A source that already refused with a
        /// blocking finding carries nothing else, so the emptiness is not reported on top of it.
        /// </summary>
        private static void CheckPopulation(StructuralSourceDescription description, List<StructuralDiagnostic> diagnostics)
        {
            if (!description.Diagnostics.Any(d => d.IsBlocking))
            {
                if (description.Nodes.Count == 0)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ModelNoNodes, StructuralExportOutcome.Invalid, "model");
                }

                if (description.Members.Count == 0)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ModelNoMembers, StructuralExportOutcome.Invalid, "model");
                }
            }

            var supported = new HashSet<string>(StringComparer.Ordinal);
            var repeated = new HashSet<string>(StringComparer.Ordinal);

            foreach (var support in description.Supports)
            {
                if (!supported.Add(support.NodeId) && repeated.Add(support.NodeId))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.SupportDuplicateNode, StructuralExportOutcome.Invalid, support.NodeId);
                }
            }

            var declared = new HashSet<string>(description.Assumptions.Select(a => a.Id), StringComparer.Ordinal);
            var missing = new HashSet<string>(StringComparer.Ordinal);
            var referenced = description.Supports.Select(s => s.AssumptionId)
                .Concat(description.Members.Select(m => m.Releases.AssumptionId));

            foreach (var id in referenced)
            {
                if (!declared.Contains(id) && missing.Add(id))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.AssumptionUndeclared, StructuralExportOutcome.Invalid, id);
                }
            }
        }

        private static Dictionary<string, StructuralNode> IndexNodes(StructuralSourceDescription description, List<StructuralDiagnostic> diagnostics)
        {
            var nodes = new Dictionary<string, StructuralNode>(StringComparer.Ordinal);

            foreach (var node in description.Nodes)
            {
                if (!nodes.ContainsKey(node.Id))
                {
                    nodes.Add(node.Id, node);
                }
                else
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ModelDuplicateId, StructuralExportOutcome.Invalid, node.Id);
                }
            }

            return nodes;
        }

        private static Dictionary<string, double> CheckMembers(
            StructuralSourceDescription description, Dictionary<string, StructuralNode> nodes, List<StructuralDiagnostic> diagnostics)
        {
            var lengths = new Dictionary<string, double>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in description.Members)
            {
                if (!seen.Add(member.Id))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ModelDuplicateId, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                if (member.Releases.AnyReleased)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ReleaseUnsupported, StructuralExportOutcome.Unsupported, member.Id);
                }

                if (!nodes.TryGetValue(member.NodeIId, out var nodeI) || !nodes.TryGetValue(member.NodeJId, out var nodeJ))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                var dx = nodeJ.Position.X.Inches - nodeI.Position.X.Inches;
                var dy = nodeJ.Position.Y.Inches - nodeI.Position.Y.Inches;
                var dz = nodeJ.Position.Z.Inches - nodeI.Position.Z.Inches;
                var length = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));

                if (!(length > 0) || double.IsInfinity(length))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.MemberGeometry, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                var axis = member.PeralteAxis;
                var norm = Math.Sqrt((axis.X * axis.X) + (axis.Y * axis.Y) + (axis.Z * axis.Z));
                var dot = ((axis.X * dx) + (axis.Y * dy) + (axis.Z * dz)) / length;

                if (Math.Abs(norm - 1) > Tolerance || Math.Abs(dot) > Tolerance)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.MemberGeometry, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                lengths.Add(member.Id, length);
            }

            return lengths;
        }

        private static void CheckSupports(
            StructuralSourceDescription description, Dictionary<string, StructuralNode> nodes, List<StructuralDiagnostic> diagnostics)
        {
            foreach (var support in description.Supports.Where(s => !nodes.ContainsKey(s.NodeId)))
            {
                Blocking(diagnostics, StructuralDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, support.NodeId);
            }
        }

        private static void CheckLoads(
            StructuralSourceDescription description, Dictionary<string, double> lengths, List<StructuralDiagnostic> diagnostics)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var perMember = new Dictionary<string, int>(StringComparer.Ordinal);
            var memberIds = new HashSet<string>(description.Members.Select(m => m.Id), StringComparer.Ordinal);

            foreach (var load in description.Loads)
            {
                if (!seen.Add(load.Id))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.ModelDuplicateId, StructuralExportOutcome.Invalid, load.Id);
                    continue;
                }

                if (!memberIds.Contains(load.MemberId))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.LoadTargetMember, StructuralExportOutcome.Invalid, load.Id);
                    continue;
                }

                perMember[load.MemberId] = perMember.TryGetValue(load.MemberId, out var count) ? count + 1 : 1;

                if (lengths.TryGetValue(load.MemberId, out var length)
                    && !(load.DistanceFromNodeI.Inches >= 0 && load.DistanceFromNodeI.Inches <= length + Tolerance))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.LoadPositionRange, StructuralExportOutcome.Invalid, load.Id);
                }
            }

            foreach (var pair in perMember.Where(p => p.Value > MaxPointLoadsPerMember).OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                Blocking(diagnostics, StructuralDiagnosticCodes.LoadCountExceeded, StructuralExportOutcome.Unsupported, pair.Key);
            }
        }

        // ---------------------------------------------------------------- sections and materials

        private static Dictionary<StructuralSectionId, StructuralMaterialRow> CheckSections(
            StructuralSourceDescription description,
            IStructuralMaterialProfile profile,
            StructuralSectionCatalog catalog,
            List<StructuralDiagnostic> diagnostics,
            out List<StructuralSection> sections)
        {
            var rows = new Dictionary<StructuralSectionId, StructuralMaterialRow>();
            sections = new List<StructuralSection>();

            var ids = description.Members
                .Select(m => m.SectionId)
                .Distinct()
                .OrderBy(id => id.Value, StringComparer.Ordinal);

            foreach (var id in ids)
            {
                if (!catalog.TryGetById(id, out var definition))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.SectionUnknown, StructuralExportOutcome.Invalid, id.Value);
                    continue;
                }

                if (!IsSupportedFamily(definition.Family))
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.SectionFamilyUnsupported, StructuralExportOutcome.Unsupported, id.Value);
                }

                if (definition.NativeUnitSystem != StructuralSectionUnitSystem.UsCustomary)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.SectionUnitSystem, StructuralExportOutcome.Unsupported, id.Value);
                }

                if (!profile.TryGetRow(id, out var row) || row == null)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.MaterialRowMissing, StructuralExportOutcome.Unsupported, id.Value);
                }
                else if (!row.IsVerified)
                {
                    Blocking(diagnostics, StructuralDiagnosticCodes.MaterialRowUnverified, StructuralExportOutcome.Unsupported, id.Value);
                }
                else
                {
                    rows.Add(id, row);
                }

                sections.Add(DescribeSection(id, definition.Properties ?? StructuralSectionProperties.Empty, diagnostics));
            }

            return rows;
        }

        /// <summary>
        /// IDL-09 as an ALLOW-LIST (A65-F1-03): the wide flange, the rectangular hollow section and the American Standard beam.
        /// Anything else is refused, including a family added to the catalogue later and a value the enum does not define.
        /// </summary>
        private static bool IsSupportedFamily(StructuralSectionFamily family) =>
            family == StructuralSectionFamily.W
            || family == StructuralSectionFamily.HssRectangular
            || family == StructuralSectionFamily.S;

        private static StructuralSection DescribeSection(
            StructuralSectionId id, StructuralSectionProperties properties, List<StructuralDiagnostic> diagnostics)
        {
            var area = Present(properties.Area);
            var ix = Present(properties.Ix);
            var iy = Present(properties.Iy);
            var j = Present(properties.J);

            Absent(diagnostics, id, "Area", area);
            Absent(diagnostics, id, "Ix", ix);
            Absent(diagnostics, id, "Iy", iy);
            Absent(diagnostics, id, "J", j);

            return new StructuralSection(
                id,
                area.HasValue ? StructuralArea.FromSquareInches(area.Value) : (StructuralArea?)null,
                ix.HasValue ? StructuralMomentOfInertia.FromInchesToTheFourth(ix.Value) : (StructuralMomentOfInertia?)null,
                iy.HasValue ? StructuralMomentOfInertia.FromInchesToTheFourth(iy.Value) : (StructuralMomentOfInertia?)null,
                j.HasValue ? StructuralMomentOfInertia.FromInchesToTheFourth(j.Value) : (StructuralMomentOfInertia?)null);
        }

        private static double? Present(double? value) =>
            value.HasValue && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value) ? value : null;

        private static void Absent(List<StructuralDiagnostic> diagnostics, StructuralSectionId id, string property, double? value)
        {
            if (!value.HasValue)
            {
                diagnostics.Add(StructuralDiagnostic.NonBlocking(StructuralDiagnosticCodes.SectionPropertyAbsent, id.Value + "#" + property));
            }
        }

        // ---------------------------------------------------------------- the model

        private static StructuralModel Assemble(
            StructuralScope scope,
            StructuralLoadFile loadFile,
            StructuralSourceDescription description,
            Dictionary<StructuralSectionId, StructuralMaterialRow> rows,
            List<StructuralSection> sections,
            List<StructuralDiagnostic> diagnostics)
        {
            var members = description.Members.Select(m => new StructuralMember(
                m.Id, m.NodeIId, m.NodeJId, m.SectionId, rows[m.SectionId].MaterialId, m.PeralteAxis, m.Releases, m.Role, m.OriginPiece));

            var materials = rows.Values
                .Select(row => row.MaterialId)
                .Distinct()
                .OrderBy(id => id.Value, StringComparer.Ordinal)
                .Select(id => new StructuralMaterial(id));

            var metadata = new StructuralModelMetadata(
                new StructuralSourceRef(
                    scope.RackId,
                    scope.Kind,
                    scope.StationIndex,
                    StructuralFingerprints.Source(scope.GetDesignBytes(), scope.RackId, scope.Kind, scope.StationIndex)),
                GeneratorVersion,
                loadFile.Sha256,
                StructuralFingerprints.MappingRows(rows.Values),
                StructuralFingerprints.Assumptions(description.Assumptions));

            return new StructuralModel(
                metadata,
                description.Nodes,
                members,
                sections,
                materials,
                description.Supports,
                description.Loads,
                description.Assumptions,
                diagnostics.Where(d => !d.IsBlocking));
        }

        private static void Blocking(List<StructuralDiagnostic> diagnostics, string code, StructuralExportOutcome outcome, string subject) =>
            diagnostics.Add(StructuralDiagnostic.Blocking(code, outcome, subject));
    }
}
