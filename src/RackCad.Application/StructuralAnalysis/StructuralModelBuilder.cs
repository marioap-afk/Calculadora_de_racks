using System;
using System.Collections.Generic;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// What identifies the station being exported, as PRIMITIVE values (CD-I65-F1-03 and CD-I65-F1-10): the rack id of the
    /// envelope, the kind, the station index and the exact bytes of the persisted design.
    /// </summary>
    public sealed class StructuralScope
    {
        public StructuralScope(string rackId, string kind, int stationIndex, byte[] designBytes)
        {
            throw new NotImplementedException();
        }

        public string RackId => throw new NotImplementedException();

        public string Kind => throw new NotImplementedException();

        public int StationIndex => throw new NotImplementedException();

        public byte[] GetDesignBytes() => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public string Id => throw new NotImplementedException();

        public string NodeIId => throw new NotImplementedException();

        public string NodeJId => throw new NotImplementedException();

        public StructuralSectionId SectionId => throw new NotImplementedException();

        public StructuralDirection PeralteAxis => throw new NotImplementedException();

        public StructuralMemberRole Role => throw new NotImplementedException();

        public string OriginPiece => throw new NotImplementedException();

        public StructuralMemberReleases Releases => throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public IReadOnlyList<StructuralNode> Nodes => throw new NotImplementedException();

        public IReadOnlyList<StructuralSourceMember> Members => throw new NotImplementedException();

        public IReadOnlyList<StructuralSupport> Supports => throw new NotImplementedException();

        public IReadOnlyList<AnalysisAssumption> Assumptions => throw new NotImplementedException();

        public IReadOnlyList<StructuralMemberPointLoad> Loads => throw new NotImplementedException();

        public IReadOnlyList<StructuralDiagnostic> Diagnostics => throw new NotImplementedException();
    }

    /// <summary>
    /// The neutral source of the idealised structure (I-65 D-13). It resolves the target of each load (level and side) and
    /// places it. In F1-T1 only a test double implements it; the Cantilever source arrives with F1-T2.
    /// </summary>
    public interface IStructuralModelSource
    {
        StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads);
    }

    /// <summary>The pure construction of the neutral model (I-65 D-13). There is no analysis and no result reading here.</summary>
    public static class StructuralModelBuilder
    {
        public const string GeneratorVersion = "rackcad-structural-model/1";

        public const int MaxPointLoadsPerMember = 24;

        public static StructuralModelResult BuildStructuralModel(
            IStructuralModelSource source,
            StructuralScope scope,
            StructuralLoadFileReadResult loads,
            IStructuralMaterialProfile profile,
            StructuralSectionCatalog catalog)
        {
            throw new NotImplementedException();
        }
    }
}
