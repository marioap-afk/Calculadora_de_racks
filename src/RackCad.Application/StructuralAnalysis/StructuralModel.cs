using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis
{
    public enum StructuralMemberRole
    {
        Column = 1,
        Arm = 2,
    }

    public enum StructuralLoadCategory
    {
        /// <summary>The neutral category of the single reference case.</summary>
        Other = 1,
    }

    /// <summary>The only load direction V1 admits: global minus Z.</summary>
    public enum StructuralLoadDirection
    {
        GlobalNegativeZ = 1,
    }

    /// <summary>The argument checks every model type shares.</summary>
    internal static class StructuralRequire
    {
        internal static string Text(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A value is required.", name);
            }

            return value;
        }

        internal static T NotNull<T>(T value, string name)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        internal static IReadOnlyList<bool> Flags(IEnumerable<bool> flags, int count, string name)
        {
            NotNull(flags, name);
            var list = flags.ToList();

            if (list.Count != count)
            {
                throw new ArgumentException("Exactly " + count + " flags are required, not " + list.Count + ".", name);
            }

            return new ReadOnlyCollection<bool>(list);
        }

        internal static IReadOnlyList<T> Items<T>(IEnumerable<T> items, string name)
            where T : class
        {
            NotNull(items, name);
            var list = items.ToList();

            if (list.Any(item => item == null))
            {
                throw new ArgumentException("A list cannot contain null.", name);
            }

            return new ReadOnlyCollection<T>(list);
        }
    }

    public sealed class StructuralPosition
    {
        public StructuralPosition(StructuralLength x, StructuralLength y, StructuralLength z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public StructuralLength X { get; }

        public StructuralLength Y { get; }

        public StructuralLength Z { get; }
    }

    public sealed class StructuralNode
    {
        public StructuralNode(string id, StructuralPosition position)
        {
            Id = StructuralRequire.Text(id, nameof(id));
            Position = StructuralRequire.NotNull(position, nameof(position));
        }

        public string Id { get; }

        public StructuralPosition Position { get; }
    }

    /// <summary>A direction in the global frame, as three dimensionless components.</summary>
    public readonly struct StructuralDirection
    {
        public StructuralDirection(double x, double y, double z)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y) || double.IsNaN(z) || double.IsInfinity(z))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "The components of a direction must be finite.");
            }

            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }

    /// <summary>
    /// The ten LOCAL release flags of a member (end I flags first, then end J) and the assumption that justifies them.
    /// V1 admits none released: any true flag makes the export Unsupported.
    /// </summary>
    public sealed class StructuralMemberReleases
    {
        public const int FlagCount = 10;

        public StructuralMemberReleases(IEnumerable<bool> flags, string assumptionId)
        {
            Flags = StructuralRequire.Flags(flags, FlagCount, nameof(flags));
            AssumptionId = StructuralRequire.Text(assumptionId, nameof(assumptionId));
        }

        public IReadOnlyList<bool> Flags { get; }

        public string AssumptionId { get; }

        public bool AnyReleased => Flags.Any(flag => flag);

        public static StructuralMemberReleases None(string assumptionId) =>
            new StructuralMemberReleases(Enumerable.Repeat(false, FlagCount), assumptionId);
    }

    public sealed class StructuralMember
    {
        public StructuralMember(
            string id,
            string nodeIId,
            string nodeJId,
            StructuralSectionId sectionRef,
            StructuralMaterialId materialRef,
            StructuralDirection peralteAxis,
            StructuralMemberReleases releases,
            StructuralMemberRole role,
            string originPiece)
        {
            if (sectionRef.IsEmpty)
            {
                throw new ArgumentException("A member needs a section.", nameof(sectionRef));
            }

            if (materialRef.IsEmpty)
            {
                throw new ArgumentException("A member needs a material.", nameof(materialRef));
            }

            Id = StructuralRequire.Text(id, nameof(id));
            NodeIId = StructuralRequire.Text(nodeIId, nameof(nodeIId));
            NodeJId = StructuralRequire.Text(nodeJId, nameof(nodeJId));
            SectionRef = sectionRef;
            MaterialRef = materialRef;
            PeralteAxis = peralteAxis;
            Releases = StructuralRequire.NotNull(releases, nameof(releases));
            Role = role;
            OriginPiece = StructuralRequire.Text(originPiece, nameof(originPiece));
        }

        public string Id { get; }

        public string NodeIId { get; }

        public string NodeJId { get; }

        public StructuralSectionId SectionRef { get; }

        public StructuralMaterialId MaterialRef { get; }

        /// <summary>Unit vector, in the global frame, of the depth axis; perpendicular to the member axis.</summary>
        public StructuralDirection PeralteAxis { get; }

        public StructuralMemberReleases Releases { get; }

        public StructuralMemberRole Role { get; }

        public string OriginPiece { get; }
    }

    /// <summary>A section of the model: its identity and the INFORMATIVE properties of the catalogue (absent is null, never 0).</summary>
    public sealed class StructuralSection
    {
        public StructuralSection(
            StructuralSectionId id,
            StructuralArea? area,
            StructuralMomentOfInertia? ix,
            StructuralMomentOfInertia? iy,
            StructuralMomentOfInertia? j)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("A section needs an id.", nameof(id));
            }

            Id = id;
            Area = area;
            Ix = ix;
            Iy = iy;
            J = j;
        }

        public StructuralSectionId Id { get; }

        public StructuralArea? Area { get; }

        public StructuralMomentOfInertia? Ix { get; }

        public StructuralMomentOfInertia? Iy { get; }

        public StructuralMomentOfInertia? J { get; }
    }

    public sealed class StructuralMaterial
    {
        public StructuralMaterial(StructuralMaterialId id)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("A material needs an id.", nameof(id));
            }

            Id = id;
        }

        public StructuralMaterialId Id { get; }
    }

    /// <summary>The six GLOBAL support flags of a node and the assumption that justifies them.</summary>
    public sealed class StructuralSupport
    {
        public const int FlagCount = 6;

        public StructuralSupport(string nodeId, IEnumerable<bool> flags, string assumptionId)
        {
            NodeId = StructuralRequire.Text(nodeId, nameof(nodeId));
            Flags = StructuralRequire.Flags(flags, FlagCount, nameof(flags));
            AssumptionId = StructuralRequire.Text(assumptionId, nameof(assumptionId));
        }

        public string NodeId { get; }

        public IReadOnlyList<bool> Flags { get; }

        public string AssumptionId { get; }
    }

    /// <summary>The one load case of V1: <c>REF1</c>, category Other, without self weight.</summary>
    public sealed class StructuralLoadCase
    {
        public const string ReferenceCaseId = "REF1";

        private StructuralLoadCase()
        {
        }

        public static StructuralLoadCase Reference { get; } = new StructuralLoadCase();

        public string Id => ReferenceCaseId;

        public StructuralLoadCategory Category => StructuralLoadCategory.Other;

        public bool IncludesSelfWeight => false;
    }

    /// <summary>A point load on a member: global minus Z, magnitude in lbf, position as the absolute distance from node I.</summary>
    public sealed class StructuralMemberPointLoad
    {
        public StructuralMemberPointLoad(
            string id, string memberId, StructuralForce magnitude, StructuralLength distanceFromNodeI)
        {
            Id = StructuralRequire.Text(id, nameof(id));
            MemberId = StructuralRequire.Text(memberId, nameof(memberId));
            Magnitude = magnitude;
            DistanceFromNodeI = distanceFromNodeI;
        }

        public string Id { get; }

        public string MemberId { get; }

        public StructuralLoadDirection Direction => StructuralLoadDirection.GlobalNegativeZ;

        public StructuralForce Magnitude { get; }

        public StructuralLength DistanceFromNodeI { get; }
    }

    /// <summary>An explicit hypothesis of the idealisation. It never states that anything is structurally validated.</summary>
    public sealed class AnalysisAssumption
    {
        public AnalysisAssumption(string id, string statement)
        {
            Id = StructuralRequire.Text(id, nameof(id));
            Statement = StructuralRequire.Text(statement, nameof(statement));
        }

        public string Id { get; }

        public string Statement { get; }
    }

    public sealed class StructuralSourceRef
    {
        public StructuralSourceRef(string rackId, string kind, int stationIndex, string sourceFingerprint)
        {
            if (stationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stationIndex), stationIndex, "The station index cannot be negative.");
            }

            RackId = StructuralRequire.Text(rackId, nameof(rackId));
            Kind = StructuralRequire.Text(kind, nameof(kind));
            StationIndex = stationIndex;
            SourceFingerprint = StructuralRequire.Text(sourceFingerprint, nameof(sourceFingerprint));
        }

        public string RackId { get; }

        public string Kind { get; }

        public int StationIndex { get; }

        public string SourceFingerprint { get; }
    }

    public sealed class StructuralModelMetadata
    {
        public StructuralModelMetadata(
            StructuralSourceRef sourceRef,
            string generatorVersion,
            string loadFileSha256,
            string mappingRowsSha256,
            string assumptionsSha256)
        {
            SourceRef = StructuralRequire.NotNull(sourceRef, nameof(sourceRef));
            GeneratorVersion = StructuralRequire.Text(generatorVersion, nameof(generatorVersion));
            LoadFileSha256 = StructuralRequire.Text(loadFileSha256, nameof(loadFileSha256));
            MappingRowsSha256 = StructuralRequire.Text(mappingRowsSha256, nameof(mappingRowsSha256));
            AssumptionsSha256 = StructuralRequire.Text(assumptionsSha256, nameof(assumptionsSha256));
        }

        public StructuralSourceRef SourceRef { get; }

        public string GeneratorVersion { get; }

        public string LoadFileSha256 { get; }

        public string MappingRowsSha256 { get; }

        public string AssumptionsSha256 { get; }
    }

    /// <summary>
    /// The neutral analysis model (I-65 D-02): immutable, DERIVED from the design, never persisted and always regenerable.
    /// </summary>
    public sealed class StructuralModel
    {
        public StructuralModel(
            StructuralModelMetadata metadata,
            IEnumerable<StructuralNode> nodes,
            IEnumerable<StructuralMember> members,
            IEnumerable<StructuralSection> sections,
            IEnumerable<StructuralMaterial> materials,
            IEnumerable<StructuralSupport> supports,
            IEnumerable<StructuralMemberPointLoad> loads,
            IEnumerable<AnalysisAssumption> assumptions,
            IEnumerable<StructuralDiagnostic> diagnostics)
        {
            Metadata = StructuralRequire.NotNull(metadata, nameof(metadata));
            Nodes = StructuralRequire.Items(nodes, nameof(nodes));
            Members = StructuralRequire.Items(members, nameof(members));
            Sections = StructuralRequire.Items(sections, nameof(sections));
            Materials = StructuralRequire.Items(materials, nameof(materials));
            Supports = StructuralRequire.Items(supports, nameof(supports));
            Loads = StructuralRequire.Items(loads, nameof(loads));
            Assumptions = StructuralRequire.Items(assumptions, nameof(assumptions));
            Diagnostics = StructuralRequire.Items(diagnostics, nameof(diagnostics));
        }

        public StructuralModelMetadata Metadata { get; }

        public IReadOnlyList<StructuralNode> Nodes { get; }

        public IReadOnlyList<StructuralMember> Members { get; }

        public IReadOnlyList<StructuralSection> Sections { get; }

        public IReadOnlyList<StructuralMaterial> Materials { get; }

        public IReadOnlyList<StructuralSupport> Supports { get; }

        public StructuralLoadCase LoadCase => StructuralLoadCase.Reference;

        public IReadOnlyList<StructuralMemberPointLoad> Loads { get; }

        public IReadOnlyList<AnalysisAssumption> Assumptions { get; }

        /// <summary>The non-blocking diagnostics of a Valid model.</summary>
        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }
    }
}
