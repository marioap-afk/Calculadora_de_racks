using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>The direction of a point load on a member, in the neutral frame (global axes) or along a local axis of the member.</summary>
    public enum StructuralReadbackLoadAxis
    {
        GlobalX = 1,
        GlobalY = 2,
        GlobalZ = 3,
        Local1 = 4,
        Local2 = 5,
        Local3 = 6,
    }

    /// <summary>The axial rigidity behaviour of a member as read from a model.</summary>
    public enum StructuralReadbackAxialBehavior
    {
        Normal = 0,
        TensionOnly = 1,
        CompressionOnly = 2,
        Cable = 3,
    }

    /// <summary>A node as read from a model that is already built (SKELETON).</summary>
    public sealed class StructuralReadbackNode
    {
        public StructuralReadbackNode(string id, string description, StructuralPosition position, IEnumerable<bool> supportFlags)
        {
            throw new NotImplementedException();
        }

        public string Id { get; }

        public string Description { get; }

        public StructuralPosition Position { get; }

        public IReadOnlyList<bool> SupportFlags { get; }
    }

    /// <summary>A member as read from a model that is already built (SKELETON).</summary>
    public sealed class StructuralReadbackMember
    {
        public StructuralReadbackMember(
            string id,
            string description,
            string nodeIId,
            string nodeJId,
            string sectionName,
            string materialName,
            double twistDegrees,
            StructuralDirection axis1,
            StructuralDirection axis2,
            StructuralDirection axis3,
            int cardinalPoint,
            StructuralPosition offsetI,
            StructuralPosition offsetJ,
            IEnumerable<double> stiffnessFactors,
            StructuralReadbackAxialBehavior axialBehavior,
            double rotationalSpringI,
            double rotationalSpringJ,
            IEnumerable<bool> releases)
        {
            throw new NotImplementedException();
        }

        public string Id { get; }

        public string Description { get; }

        public string NodeIId { get; }

        public string NodeJId { get; }

        public string SectionName { get; }

        public string MaterialName { get; }

        public double TwistDegrees { get; }

        public StructuralDirection Axis1 { get; }

        public StructuralDirection Axis2 { get; }

        public StructuralDirection Axis3 { get; }

        public int CardinalPoint { get; }

        public StructuralPosition OffsetI { get; }

        public StructuralPosition OffsetJ { get; }

        public IReadOnlyList<double> StiffnessFactors { get; }

        public StructuralReadbackAxialBehavior AxialBehavior { get; }

        public double RotationalSpringI { get; }

        public double RotationalSpringJ { get; }

        public IReadOnlyList<bool> Releases { get; }
    }

    /// <summary>A point load as read from a model (SKELETON).</summary>
    public sealed class StructuralReadbackPointLoad
    {
        public StructuralReadbackPointLoad(string memberId, StructuralReadbackLoadAxis axis, StructuralForce value, StructuralLength distanceFromNodeI)
        {
            throw new NotImplementedException();
        }

        public string MemberId { get; }

        public StructuralReadbackLoadAxis Axis { get; }

        public StructuralForce Value { get; }

        public StructuralLength DistanceFromNodeI { get; }
    }

    /// <summary>A load case as read from a model (SKELETON).</summary>
    public sealed class StructuralReadbackLoadCase
    {
        public StructuralReadbackLoadCase(string id, string category, StructuralDirection selfWeight, IEnumerable<StructuralReadbackPointLoad> loads)
        {
            throw new NotImplementedException();
        }

        public string Id { get; }

        public string Category { get; }

        public StructuralDirection SelfWeight { get; }

        public IReadOnlyList<StructuralReadbackPointLoad> Loads { get; }
    }

    /// <summary>One term of a load combination (SKELETON).</summary>
    public sealed class StructuralReadbackCombinationTerm
    {
        public StructuralReadbackCombinationTerm(string caseId, double factor)
        {
            throw new NotImplementedException();
        }

        public string CaseId { get; }

        public double Factor { get; }
    }

    /// <summary>A load combination as read from a model (SKELETON).</summary>
    public sealed class StructuralReadbackCombination
    {
        public StructuralReadbackCombination(string id, IEnumerable<StructuralReadbackCombinationTerm> terms)
        {
            throw new NotImplementedException();
        }

        public string Id { get; }

        public IReadOnlyList<StructuralReadbackCombinationTerm> Terms { get; }
    }

    /// <summary>A whole model as read back (SKELETON).</summary>
    public sealed class StructuralModelReadback
    {
        public StructuralModelReadback(
            string adapterVersion,
            string initialUnitPair,
            bool unitsRestored,
            IEnumerable<StructuralReadbackNode> nodes,
            IEnumerable<StructuralReadbackMember> members,
            IEnumerable<StructuralReadbackLoadCase> loadCases,
            IEnumerable<StructuralReadbackCombination> combinations)
        {
            throw new NotImplementedException();
        }

        public string AdapterVersion { get; }

        public string InitialUnitPair { get; }

        public bool UnitsRestored { get; }

        public IReadOnlyList<StructuralReadbackNode> Nodes { get; }

        public IReadOnlyList<StructuralReadbackMember> Members { get; }

        public IReadOnlyList<StructuralReadbackLoadCase> LoadCases { get; }

        public IReadOnlyList<StructuralReadbackCombination> Combinations { get; }
    }

    /// <summary>The result of the read-back of a transfer (SKELETON).</summary>
    public sealed class StructuralTransferReadback
    {
        public StructuralTransferReadback(bool passed, string analysisInputFingerprint, string adapterVersion, IEnumerable<string> differences)
        {
            throw new NotImplementedException();
        }

        public bool Passed { get; }

        public string AnalysisInputFingerprint { get; }

        public string AdapterVersion { get; }

        public IReadOnlyList<string> Differences { get; }
    }

    /// <summary>What a package manifest says about the input it was built from (SKELETON).</summary>
    public sealed class StructuralPackageProvenance
    {
        public StructuralPackageProvenance(string analysisInputFingerprint, string adapterVersion)
        {
            throw new NotImplementedException();
        }

        public string AnalysisInputFingerprint { get; }

        public string AdapterVersion { get; }
    }
}
