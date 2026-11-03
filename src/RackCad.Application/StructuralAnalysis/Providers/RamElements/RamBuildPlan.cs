using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The six restraint flags of a node, in the names of the analysis program.</summary>
    public readonly struct RamRestraints
    {
        public RamRestraints(bool dx, bool dy, bool dz, bool rx, bool ry, bool rz)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
            Rx = rx;
            Ry = ry;
            Rz = rz;
        }

        public bool Dx { get; }

        public bool Dy { get; }

        public bool Dz { get; }

        public bool Rx { get; }

        public bool Ry { get; }

        public bool Rz { get; }
    }

    public sealed class RamNodePlan
    {
        public RamNodePlan(string id, RamVector position, RamRestraints restraints)
        {
            Id = id;
            Position = position;
            Restraints = restraints;
        }

        public string Id { get; }

        /// <summary>The position in inches, in the coordinate system of the analysis program.</summary>
        public RamVector Position { get; }

        public RamRestraints Restraints { get; }
    }

    public sealed class RamMemberPlan
    {
        public RamMemberPlan(
            string id,
            string nodeIId,
            string nodeJId,
            string sectionName,
            string materialName,
            double thetaDegrees,
            RamAxesFrame localAxes,
            IReadOnlyList<bool> releases)
        {
            Id = id;
            NodeIId = nodeIId;
            NodeJId = nodeJId;
            SectionName = sectionName;
            MaterialName = materialName;
            ThetaDegrees = thetaDegrees;
            LocalAxes = localAxes;
            Releases = releases;
        }

        public string Id { get; }

        public string NodeIId { get; }

        public string NodeJId { get; }

        public string SectionName { get; }

        public string MaterialName { get; }

        public double ThetaDegrees { get; }

        /// <summary>The local axes the member is expected to have once the twist is applied.</summary>
        public RamAxesFrame LocalAxes { get; }

        /// <summary>The ten LOCAL release flags, untransformed; all false in V1.</summary>
        public IReadOnlyList<bool> Releases { get; }
    }

    public sealed class RamLoadPlan
    {
        public RamLoadPlan(string id, string memberId, string direction, double valuePoundsForce, double positionPercent)
        {
            Id = id;
            MemberId = memberId;
            Direction = direction;
            ValuePoundsForce = valuePoundsForce;
            PositionPercent = positionPercent;
        }

        public string Id { get; }

        public string MemberId { get; }

        /// <summary>The direction in the names of the analysis program; always <c>GLOBAL_Y</c> in V1.</summary>
        public string Direction { get; }

        /// <summary>The signed value, in pounds-force.</summary>
        public double ValuePoundsForce { get; }

        /// <summary>The position as a percentage of the length of the member from node I.</summary>
        public double PositionPercent { get; }
    }

    /// <summary>The deterministic translation of a neutral model: everything the build of the analysis program needs.</summary>
    public sealed class RamBuildPlan
    {
        public RamBuildPlan(
            string units,
            string loadCaseId,
            string loadCaseCategory,
            IReadOnlyList<RamNodePlan> nodes,
            IReadOnlyList<RamMemberPlan> members,
            IReadOnlyList<RamLoadPlan> loads,
            IReadOnlyList<RamSectionMapRow> usedRows)
        {
            Units = units;
            LoadCaseId = loadCaseId;
            LoadCaseCategory = loadCaseCategory;
            Nodes = nodes;
            Members = members;
            Loads = loads;
            UsedRows = usedRows;
        }

        public string Units { get; }

        public string LoadCaseId { get; }

        /// <summary>The category as text; never a member of the enum of the analysis program.</summary>
        public string LoadCaseCategory { get; }

        public IReadOnlyList<RamNodePlan> Nodes { get; }

        public IReadOnlyList<RamMemberPlan> Members { get; }

        public IReadOnlyList<RamLoadPlan> Loads { get; }

        /// <summary>The provider rows the model uses, in ordinal order of section id.</summary>
        public IReadOnlyList<RamSectionMapRow> UsedRows { get; }
    }

    /// <summary>The result of a translation: the plan when, and only when, the outcome is Valid. There is never a partial plan.</summary>
    public sealed class RamTranslationResult
    {
        internal RamTranslationResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, RamBuildPlan plan)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Plan = plan;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public RamBuildPlan Plan { get; }
    }
}
