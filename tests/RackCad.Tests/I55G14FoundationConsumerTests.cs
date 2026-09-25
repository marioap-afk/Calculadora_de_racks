using System;
using System.IO;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14: evidence that I-55 consumes the integrated Foundation (AUTH-08 V2 and AUTH-12 V2 of I-59) instead of
    /// rebuilding it. These are source guards because the product must not grow a private copy of a shared fact.
    /// </summary>
    public class I55G14FoundationConsumerTests
    {
        [Fact]
        public void AUTH08_V2_FACTS_TRAVEL_INTO_THE_PRODUCT_WITHOUT_BEING_REBUILT()
        {
            var classified = G14.Facts(x: 10, y: 20, rotation: 0.5, originX: 3, originY: -2);
            var acceptance = RackProjectionSourceTransform.Accept(classified);

            Assert.True(acceptance.IsAccepted);
            Assert.Same(classified.Facts, acceptance.Facts);
            Assert.Equal(classified.Facts.RotationRadians, acceptance.Facts.RotationRadians, 12);
            Assert.Equal(classified.Facts.DefinitionOrigin.X, acceptance.Facts.DefinitionOrigin.X, 12);
            Assert.Equal(classified.Facts.ReferencePosition.Y, acceptance.Facts.ReferencePosition.Y, 12);
            Assert.True(acceptance.Facts.NormalIsWorldZ);
        }

        [Fact]
        public void AUTH12_V2_FACTS_TRAVEL_INTO_THE_DIAGNOSTIC_WITHOUT_BEING_REBUILT()
        {
            var fact = G14.Piece(
                "P-1",
                RequirementRole.Required,
                "RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL",
                LibraryAvailability.FileMissing,
                LibraryBlockPresence.BlockMissing);

            Assert.Equal("P-1", fact.Requirement.PieceId);
            Assert.Equal(RequirementKeyState.Present, fact.Requirement.KeyState);
            Assert.Equal("RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL", fact.Requirement.LibraryKey);
            Assert.Equal(LibraryAvailability.FileMissing, fact.LibraryAvailability);
            Assert.Equal(LibraryBlockPresence.BlockMissing, fact.BlockPresence);
        }

        [Fact]
        public void G14_DOES_NOT_CLONE_AUTH08_CLASSIFICATION_OR_ANGLE_CANONICALISATION()
        {
            foreach (var file in ProductFiles())
            {
                var source = File.ReadAllText(file);
                Assert.DoesNotContain("Math.Atan2", source);
                Assert.DoesNotContain("NormalizePi", source);
                Assert.DoesNotContain("new RackSourcePlacementSnapshot", source);
                Assert.DoesNotContain("RackScaleSign.ZeroWithinTolerance", source);
                Assert.DoesNotContain("ScaleFactors.X", source);
            }
        }

        [Fact]
        public void G14_DOES_NOT_CLONE_AUTH12_REQUIREMENT_EXTRACTION_OR_LIBRARY_QUERIES()
        {
            foreach (var file in ProductFiles())
            {
                var source = File.ReadAllText(file);
                Assert.DoesNotContain("new LibraryPieceRequirement(", source);
                Assert.DoesNotContain("LibraryPieceAvailabilityFlowV2.Observe", source);
                Assert.DoesNotContain("RackHeaderPieceRequirementExtractorV2", source);
                Assert.DoesNotContain("ILibraryPieceAvailabilityQuery", source);
                Assert.DoesNotContain("blocks-library", source);
            }
        }

        [Fact]
        public void G14_CONSUMES_THE_FOUNDATION_TYPES_BY_NAME()
        {
            var placement = typeof(RackSourcePlacementAcceptance);
            Assert.Equal(
                typeof(RackSourceTransformFactsV2),
                placement.GetProperty(nameof(RackSourcePlacementAcceptance.Facts)).PropertyType);

            var preparation = typeof(RackProjectionTargetPreparation);
            Assert.Equal(
                typeof(PieceRequirementExtractionOutcome),
                preparation.GetProperty(nameof(RackProjectionTargetPreparation.RequirementOutcome)).PropertyType);
        }

        internal static string[] ProductFiles()
        {
            var root = RepoRoot();
            return Directory
                .GetFiles(Path.Combine(root, "src", "RackCad.Application", "Views"), "*.cs", SearchOption.AllDirectories)
                .ToArray();
        }

        internal static string RepoRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            return directory.FullName;
        }
    }

    /// <summary>
    /// G14: the authored gate and the custom properties gate stay independent authorities; neither one reads the
    /// other's subject.
    /// </summary>
    public class RackSiblingGateIndependenceGuardTests
    {
        [Fact]
        public void THE_AUTHORED_GATE_DOES_NOT_READ_CUSTOM_PROPERTIES()
        {
            var source = File.ReadAllText(Path.Combine(
                I55G14FoundationConsumerTests.RepoRoot(),
                "src", "RackCad.Application", "Views", "Placement", "RackSiblingAuthoredGate.cs"));

            Assert.DoesNotContain("CustomProperties", source);
            Assert.DoesNotContain("ProjectVariables", source);
            Assert.Contains("RackAuthoredComparisonOutcome", source);
        }

        [Fact]
        public void THE_CUSTOM_PROPERTIES_GATE_DOES_NOT_READ_AUTHORED_COMPARATORS()
        {
            var source = File.ReadAllText(Path.Combine(
                I55G14FoundationConsumerTests.RepoRoot(),
                "src", "RackCad.Application", "Views", "Insertion", "RackSiblingCustomPropertiesGate.cs"));

            Assert.DoesNotContain("RackAuthoredComparator", source);
            Assert.DoesNotContain("IsSameAuthority", source);
        }

        [Fact]
        public void THE_PLAN_KEEPS_BOTH_GATES_AS_SEPARATE_STATES()
        {
            var states = Enum.GetNames(typeof(RackProjectionFailureCode));

            Assert.Contains("AuthoredDivergent", states);
            Assert.Contains("PropertiesDivergent", states);
            Assert.Contains("AuthoredUnreadable", states);
            Assert.Contains("PropertiesUnreadable", states);
        }
    }
}
