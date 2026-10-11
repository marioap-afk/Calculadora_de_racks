using System;
using System.Collections.Generic;
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

    public sealed class StructuralPosition
    {
        public StructuralPosition(StructuralLength x, StructuralLength y, StructuralLength z)
        {
            throw new NotImplementedException();
        }

        public StructuralLength X => throw new NotImplementedException();

        public StructuralLength Y => throw new NotImplementedException();

        public StructuralLength Z => throw new NotImplementedException();
    }

    public sealed class StructuralNode
    {
        public StructuralNode(string id, StructuralPosition position)
        {
            throw new NotImplementedException();
        }

        public string Id => throw new NotImplementedException();

        public StructuralPosition Position => throw new NotImplementedException();
    }

    /// <summary>A direction in the global frame, as three dimensionless components.</summary>
    public readonly struct StructuralDirection
    {
        public StructuralDirection(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public double X => throw new NotImplementedException();

        public double Y => throw new NotImplementedException();

        public double Z => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public IReadOnlyList<bool> Flags => throw new NotImplementedException();

        public string AssumptionId => throw new NotImplementedException();

        public bool AnyReleased => throw new NotImplementedException();

        public static StructuralMemberReleases None(string assumptionId) => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public string Id => throw new NotImplementedException();

        public string NodeIId => throw new NotImplementedException();

        public string NodeJId => throw new NotImplementedException();

        public StructuralSectionId SectionRef => throw new NotImplementedException();

        public StructuralMaterialId MaterialRef => throw new NotImplementedException();

        /// <summary>Unit vector, in the global frame, of the depth axis; perpendicular to the member axis.</summary>
        public StructuralDirection PeralteAxis => throw new NotImplementedException();

        public StructuralMemberReleases Releases => throw new NotImplementedException();

        public StructuralMemberRole Role => throw new NotImplementedException();

        public string OriginPiece => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public StructuralSectionId Id => throw new NotImplementedException();

        public StructuralArea? Area => throw new NotImplementedException();

        public StructuralMomentOfInertia? Ix => throw new NotImplementedException();

        public StructuralMomentOfInertia? Iy => throw new NotImplementedException();

        public StructuralMomentOfInertia? J => throw new NotImplementedException();
    }

    public sealed class StructuralMaterial
    {
        public StructuralMaterial(StructuralMaterialId id)
        {
            throw new NotImplementedException();
        }

        public StructuralMaterialId Id => throw new NotImplementedException();
    }

    /// <summary>The six GLOBAL support flags of a node and the assumption that justifies them.</summary>
    public sealed class StructuralSupport
    {
        public const int FlagCount = 6;

        public StructuralSupport(string nodeId, IEnumerable<bool> flags, string assumptionId)
        {
            throw new NotImplementedException();
        }

        public string NodeId => throw new NotImplementedException();

        public IReadOnlyList<bool> Flags => throw new NotImplementedException();

        public string AssumptionId => throw new NotImplementedException();
    }

    /// <summary>The one load case of V1: <c>REF1</c>, category Other, without self weight.</summary>
    public sealed class StructuralLoadCase
    {
        public const string ReferenceCaseId = "REF1";

        public static StructuralLoadCase Reference => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public StructuralLoadCategory Category => throw new NotImplementedException();

        public bool IncludesSelfWeight => throw new NotImplementedException();
    }

    /// <summary>A point load on a member: global minus Z, magnitude in lbf, position as the absolute distance from node I.</summary>
    public sealed class StructuralMemberPointLoad
    {
        public StructuralMemberPointLoad(
            string id, string memberId, StructuralForce magnitude, StructuralLength distanceFromNodeI)
        {
            throw new NotImplementedException();
        }

        public string Id => throw new NotImplementedException();

        public string MemberId => throw new NotImplementedException();

        public StructuralLoadDirection Direction => throw new NotImplementedException();

        public StructuralForce Magnitude => throw new NotImplementedException();

        public StructuralLength DistanceFromNodeI => throw new NotImplementedException();
    }

    /// <summary>An explicit hypothesis of the idealisation. It never states that anything is structurally validated.</summary>
    public sealed class AnalysisAssumption
    {
        public AnalysisAssumption(string id, string statement)
        {
            throw new NotImplementedException();
        }

        public string Id => throw new NotImplementedException();

        public string Statement => throw new NotImplementedException();
    }

    public sealed class StructuralSourceRef
    {
        public StructuralSourceRef(string rackId, string kind, int stationIndex, string sourceFingerprint)
        {
            throw new NotImplementedException();
        }

        public string RackId => throw new NotImplementedException();

        public string Kind => throw new NotImplementedException();

        public int StationIndex => throw new NotImplementedException();

        public string SourceFingerprint => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public StructuralSourceRef SourceRef => throw new NotImplementedException();

        public string GeneratorVersion => throw new NotImplementedException();

        public string LoadFileSha256 => throw new NotImplementedException();

        public string MappingRowsSha256 => throw new NotImplementedException();

        public string AssumptionsSha256 => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public StructuralModelMetadata Metadata => throw new NotImplementedException();

        public IReadOnlyList<StructuralNode> Nodes => throw new NotImplementedException();

        public IReadOnlyList<StructuralMember> Members => throw new NotImplementedException();

        public IReadOnlyList<StructuralSection> Sections => throw new NotImplementedException();

        public IReadOnlyList<StructuralMaterial> Materials => throw new NotImplementedException();

        public IReadOnlyList<StructuralSupport> Supports => throw new NotImplementedException();

        public StructuralLoadCase LoadCase => throw new NotImplementedException();

        public IReadOnlyList<StructuralMemberPointLoad> Loads => throw new NotImplementedException();

        public IReadOnlyList<AnalysisAssumption> Assumptions => throw new NotImplementedException();

        /// <summary>The non-blocking diagnostics of a Valid model.</summary>
        public IReadOnlyList<StructuralDiagnostic> Diagnostics => throw new NotImplementedException();
    }
}
