using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// The direction of a point load on a member as read from a model that is already built: along a GLOBAL axis of the neutral frame, or
    /// along a LOCAL axis of the member. The value that goes with it is signed along the positive direction of the axis.
    /// </summary>
    public enum StructuralReadbackLoadAxis
    {
        GlobalX = 1,
        GlobalY = 2,
        GlobalZ = 3,
        Local1 = 4,
        Local2 = 5,
        Local3 = 6,
    }

    /// <summary>The axial rigidity behaviour of a member as read from a model that is already built.</summary>
    public enum StructuralReadbackAxialBehavior
    {
        Normal = 0,
        TensionOnly = 1,
        CompressionOnly = 2,
        Cable = 3,
    }

    /// <summary>The argument checks the types of the readback share.</summary>
    internal static class StructuralReadbackRequire
    {
        internal static double Finite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "A value read from a model must be finite.");
            }

            return value;
        }

        internal static IReadOnlyList<double> Numbers(IEnumerable<double> values, int count, string name)
        {
            StructuralRequire.NotNull(values, name);
            var list = values.ToList();

            if (list.Count != count)
            {
                throw new ArgumentException("Exactly " + count + " values are required, not " + list.Count + ".", name);
            }

            foreach (var value in list)
            {
                Finite(value, name);
            }

            return new ReadOnlyCollection<double>(list);
        }

        internal static T Defined<T>(T value, string name)
            where T : struct, Enum
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Undeclared value.");
            }

            return value;
        }
    }

    /// <summary>A node as read from a model that is already built. The identity is the one the reading gives it, meaningful within one readback.</summary>
    public sealed class StructuralReadbackNode
    {
        public StructuralReadbackNode(string id, string description, StructuralPosition position, IEnumerable<bool> supportFlags)
        {
            Id = StructuralRequire.Text(id, nameof(id));
            Description = description ?? string.Empty;
            Position = StructuralRequire.NotNull(position, nameof(position));
            SupportFlags = StructuralRequire.Flags(supportFlags, StructuralSupport.FlagCount, nameof(supportFlags));
        }

        public string Id { get; }

        /// <summary>The label the model gives the node; empty when it has none.</summary>
        public string Description { get; }

        /// <summary>The position in the neutral frame, in inches.</summary>
        public StructuralPosition Position { get; }

        /// <summary>The six GLOBAL support flags in the neutral order Tx, Ty, Tz, Rx, Ry, Rz.</summary>
        public IReadOnlyList<bool> SupportFlags { get; }
    }

    /// <summary>A member as read from a model that is already built.</summary>
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
            if (cardinalPoint < 0 || cardinalPoint > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(cardinalPoint), cardinalPoint, "The cardinal point is 0 (centroid) or 1 to 9 (grid).");
            }

            Id = StructuralRequire.Text(id, nameof(id));
            Description = description ?? string.Empty;
            NodeIId = StructuralRequire.Text(nodeIId, nameof(nodeIId));
            NodeJId = StructuralRequire.Text(nodeJId, nameof(nodeJId));
            SectionName = StructuralRequire.Text(sectionName, nameof(sectionName));
            MaterialName = StructuralRequire.Text(materialName, nameof(materialName));
            TwistDegrees = StructuralReadbackRequire.Finite(twistDegrees, nameof(twistDegrees));
            Axis1 = axis1;
            Axis2 = axis2;
            Axis3 = axis3;
            CardinalPoint = cardinalPoint;
            OffsetI = StructuralRequire.NotNull(offsetI, nameof(offsetI));
            OffsetJ = StructuralRequire.NotNull(offsetJ, nameof(offsetJ));
            StiffnessFactors = StructuralReadbackRequire.Numbers(stiffnessFactors, 4, nameof(stiffnessFactors));
            AxialBehavior = StructuralReadbackRequire.Defined(axialBehavior, nameof(axialBehavior));
            RotationalSpringI = StructuralReadbackRequire.Finite(rotationalSpringI, nameof(rotationalSpringI));
            RotationalSpringJ = StructuralReadbackRequire.Finite(rotationalSpringJ, nameof(rotationalSpringJ));
            Releases = StructuralRequire.Flags(releases, StructuralMemberReleases.FlagCount, nameof(releases));
        }

        public string Id { get; }

        public string Description { get; }

        public string NodeIId { get; }

        public string NodeJId { get; }

        /// <summary>The name of the section as the model reports it.</summary>
        public string SectionName { get; }

        /// <summary>The name of the material as the model reports it.</summary>
        public string MaterialName { get; }

        /// <summary>The cross-section rotation about the member axis, in degrees, as the model reports it.</summary>
        public double TwistDegrees { get; }

        /// <summary>The local axis along the member (node I to node J), in the neutral frame.</summary>
        public StructuralDirection Axis1 { get; }

        /// <summary>The local axis 2, in the neutral frame.</summary>
        public StructuralDirection Axis2 { get; }

        /// <summary>The local axis 3, in the neutral frame.</summary>
        public StructuralDirection Axis3 { get; }

        /// <summary>0 for the centroid, 1 to 9 for the nine positions of the section grid.</summary>
        public int CardinalPoint { get; }

        /// <summary>The rigid end offset at node I, in the neutral frame.</summary>
        public StructuralPosition OffsetI { get; }

        /// <summary>The rigid end offset at node J, in the neutral frame.</summary>
        public StructuralPosition OffsetJ { get; }

        /// <summary>The four stiffness adjustment factors, as the model reports them.</summary>
        public IReadOnlyList<double> StiffnessFactors { get; }

        public StructuralReadbackAxialBehavior AxialBehavior { get; }

        /// <summary>The rotational spring at node I, in pound-force inches per radian.</summary>
        public double RotationalSpringI { get; }

        /// <summary>The rotational spring at node J, in pound-force inches per radian.</summary>
        public double RotationalSpringJ { get; }

        /// <summary>The ten LOCAL release flags (end I first, then end J, then the two of the element).</summary>
        public IReadOnlyList<bool> Releases { get; }
    }

    /// <summary>A point load on a member as read from a model: a direction, a typed force and the absolute distance from node I.</summary>
    public sealed class StructuralReadbackPointLoad
    {
        public StructuralReadbackPointLoad(string memberId, StructuralReadbackLoadAxis axis, StructuralForce value, StructuralLength distanceFromNodeI)
        {
            MemberId = StructuralRequire.Text(memberId, nameof(memberId));
            Axis = StructuralReadbackRequire.Defined(axis, nameof(axis));
            Value = value;
            DistanceFromNodeI = distanceFromNodeI;
        }

        public string MemberId { get; }

        public StructuralReadbackLoadAxis Axis { get; }

        /// <summary>The force, signed along the positive direction of <see cref="Axis"/>.</summary>
        public StructuralForce Value { get; }

        public StructuralLength DistanceFromNodeI { get; }
    }

    /// <summary>A load case as read from a model, with its point loads in the order the model keeps them.</summary>
    public sealed class StructuralReadbackLoadCase
    {
        public StructuralReadbackLoadCase(string id, string category, StructuralDirection selfWeight, IEnumerable<StructuralReadbackPointLoad> loads)
        {
            Id = StructuralRequire.Text(id, nameof(id));
            Category = StructuralRequire.Text(category, nameof(category));
            SelfWeight = selfWeight;
            Loads = StructuralRequire.Items(loads, nameof(loads));
        }

        public string Id { get; }

        /// <summary>The category as text, as the model reports it.</summary>
        public string Category { get; }

        /// <summary>The self weight multipliers in the neutral frame; all zero when the case has none.</summary>
        public StructuralDirection SelfWeight { get; }

        public IReadOnlyList<StructuralReadbackPointLoad> Loads { get; }
    }

    /// <summary>One term of a load combination: a load case and its factor.</summary>
    public sealed class StructuralReadbackCombinationTerm
    {
        public StructuralReadbackCombinationTerm(string caseId, double factor)
        {
            CaseId = StructuralRequire.Text(caseId, nameof(caseId));
            Factor = StructuralReadbackRequire.Finite(factor, nameof(factor));
        }

        public string CaseId { get; }

        public double Factor { get; }
    }

    /// <summary>A load combination as read from a model.</summary>
    public sealed class StructuralReadbackCombination
    {
        public StructuralReadbackCombination(string id, IEnumerable<StructuralReadbackCombinationTerm> terms)
        {
            Id = StructuralRequire.Text(id, nameof(id));
            Terms = StructuralRequire.Items(terms, nameof(terms));
        }

        public string Id { get; }

        public IReadOnlyList<StructuralReadbackCombinationTerm> Terms { get; }
    }

    /// <summary>
    /// A whole model as read back: what a dump of a model that is already built says, in the neutral frame and in typed inches and
    /// pounds-force. Immutable and free of I/O; the comparator consumes it.
    /// </summary>
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
            AdapterVersion = StructuralRequire.Text(adapterVersion, nameof(adapterVersion));
            InitialUnitPair = StructuralRequire.Text(initialUnitPair, nameof(initialUnitPair));
            UnitsRestored = unitsRestored;
            Nodes = StructuralRequire.Items(nodes, nameof(nodes));
            Members = StructuralRequire.Items(members, nameof(members));
            LoadCases = StructuralRequire.Items(loadCases, nameof(loadCases));
            Combinations = StructuralRequire.Items(combinations, nameof(combinations));
        }

        /// <summary>The version of the adapter that produced the reading.</summary>
        public string AdapterVersion { get; }

        /// <summary>The pair of units the model had before the reading (it was read in inches and pounds-force whatever it was).</summary>
        public string InitialUnitPair { get; }

        /// <summary>True when the reading had to change the pair of units and put the initial pair back.</summary>
        public bool UnitsRestored { get; }

        public IReadOnlyList<StructuralReadbackNode> Nodes { get; }

        public IReadOnlyList<StructuralReadbackMember> Members { get; }

        public IReadOnlyList<StructuralReadbackLoadCase> LoadCases { get; }

        public IReadOnlyList<StructuralReadbackCombination> Combinations { get; }
    }

    /// <summary>
    /// The result of the read-back that a build of a model leaves behind. It is approved ONLY when the build says so and no difference was
    /// found; a failure, including the rejections of its gates, is never approved.
    /// </summary>
    public sealed class StructuralTransferReadback
    {
        public StructuralTransferReadback(bool passed, string analysisInputFingerprint, string adapterVersion, IEnumerable<string> differences)
        {
            Differences = StructuralRequire.Items(differences, nameof(differences));

            if (passed)
            {
                StructuralRequire.Text(analysisInputFingerprint, nameof(analysisInputFingerprint));
                StructuralRequire.Text(adapterVersion, nameof(adapterVersion));

                if (Differences.Count > 0)
                {
                    throw new ArgumentException("An approved transfer cannot carry differences.", nameof(differences));
                }
            }

            Passed = passed;
            AnalysisInputFingerprint = analysisInputFingerprint;
            AdapterVersion = adapterVersion;
        }

        public bool Passed { get; }

        /// <summary>The fingerprint of the input the build repeats; <c>null</c> when a failure could not even read it.</summary>
        public string AnalysisInputFingerprint { get; }

        /// <summary>The version of the adapter that the build records; <c>null</c> when a failure could not even read it.</summary>
        public string AdapterVersion { get; }

        public IReadOnlyList<string> Differences { get; }
    }

    /// <summary>What the manifest of a package says about the input it was built from.</summary>
    public sealed class StructuralPackageProvenance
    {
        public StructuralPackageProvenance(string analysisInputFingerprint, string adapterVersion)
        {
            AnalysisInputFingerprint = StructuralRequire.Text(analysisInputFingerprint, nameof(analysisInputFingerprint));
            AdapterVersion = StructuralRequire.Text(adapterVersion, nameof(adapterVersion));
        }

        public string AnalysisInputFingerprint { get; }

        public string AdapterVersion { get; }
    }
}
