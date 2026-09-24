using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-59 F1 characterization of the concrete AUTH-08/AUTH-12 contracts that exist before the proposed
    /// V2 boundary. Future API obligations remain documentary RED until their exact carrier is frozen.
    /// </summary>
    public sealed class I59F1CharacterizationTests
    {
        [Theory]
        [MemberData(nameof(SupportedTransformCases))]
        public void AUTH08_CURRENT_SUPPORTED_MATRIX_EXPOSES_TYPED_FACTS(
            string caseId,
            Transform2D transform,
            double expectedAngle,
            double expectedDeterminant,
            bool expectedReflection,
            double expectedScale)
        {
            var result = RackTransformFacts.Describe(transform, 1e-9);

            Assert.True(result.IsSupported, caseId);
            Assert.Equal(RackTransformFailure.None, result.Failure);
            Assert.Equal(expectedAngle, result.Facts.RotationRadians, 10);
            Assert.Equal(expectedDeterminant, result.Facts.Determinant, 10);
            Assert.Equal(expectedReflection, result.Facts.IsReflection);
            Assert.Equal(expectedScale, result.Facts.UniformScale, 10);
        }

        public static IEnumerable<object[]> SupportedTransformCases()
        {
            yield return new object[] { "identity", Transform2D.Identity, 0.0, 1.0, false, 1.0 };
            yield return new object[]
            {
                "rotation-translation-uniform",
                Transform2D.Scale(2).Then(Transform2D.RotationDegrees(90)).Then(Transform2D.Translation(5, -3)),
                Math.PI / 2, 4.0, false, 2.0,
            };
            yield return new object[]
            {
                "half-turn-matrix", new Transform2D(-1, 0, 0, -1, 7, 11), Math.PI, 1.0, false, 1.0,
            };
            yield return new object[]
            {
                "reflection-matrix", Transform2D.MirrorAboutY, Math.PI, -1.0, true, 1.0,
            };
        }

        [Fact]
        public void AUTH08_CURRENT_NON_UNIFORM_INPUT_PRESERVES_AXIS_MATRIX_BUT_CLASSIFIER_RETURNS_TYPED_FAILURE()
        {
            var transform = new Transform2D(2, 0, 0, 3, 13, 17);

            var result = RackTransformFacts.Describe(transform, 1e-9);

            Assert.Equal(2.0, transform.M11);
            Assert.Equal(3.0, transform.M22);
            Assert.Equal(6.0, transform.Determinant);
            Assert.False(result.IsSupported);
            Assert.Equal(RackTransformFailure.NonUniformScale, result.Failure);
        }

        [Fact]
        public void AUTH08_CURRENT_NEGATIVE_PI_REMAINS_NEGATIVE_AND_IS_A_MEASURED_V2_GAP()
        {
            var result = RackTransformFacts.Describe(Transform2D.Rotation(-Math.PI), 1e-9);

            Assert.True(result.IsSupported);
            Assert.Equal(-Math.PI, result.Facts.RotationRadians, 12);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(-1.0)]
        public void AUTH08_CURRENT_INVALID_TOLERANCE_RETURNS_TYPED_FAILURE(double tolerance)
        {
            var result = RackTransformFacts.Describe(Transform2D.Identity, tolerance);

            Assert.False(result.IsSupported);
            Assert.Equal(RackTransformFailure.InvalidTolerance, result.Failure);
        }

        [Fact]
        public void AUTH12_CURRENT_EXTRACTOR_SKIPS_BLANKS_AND_DEDUPLICATES_KEYS_CASE_INSENSITIVELY()
        {
            var plan = new HeaderRunPlan(
                Array.Empty<HeaderGroup>(),
                new[]
                {
                    Piece(HeaderBlockRole.Beam, "beam-left", "VIGA.100", "frontal"),
                    Piece(HeaderBlockRole.Beam, "beam-right", "viga.100", "lateral"),
                    Piece(HeaderBlockRole.Beam, "beam-blank", " ", "frontal"),
                    Piece(HeaderBlockRole.Pallet, "pallet", "PALLET.100", "frontal"),
                });

            var requirements = RackBlockRequirementExtractors.HeaderRun.Extract(plan);

            Assert.Equal(new[] { "VIGA.100", "PALLET.100" }, requirements.Select(item => item.Key));
        }

        [Fact]
        public void AUTH12_CURRENT_REQUIREMENT_REJECTS_BLANK_AND_EQUALITY_IS_KEY_ONLY()
        {
            Assert.Throws<ArgumentException>(() => new LibraryBlockRequirement(" "));
            Assert.Equal(new LibraryBlockRequirement("VIGA.100"), new LibraryBlockRequirement("viga.100"));
        }

        [Fact]
        public void AUTH12_CURRENT_SOURCE_INSTANCES_PRESERVE_PIECE_VIEW_ROLE_AND_BLANK_KEY_BEFORE_EXTRACTION()
        {
            var instance = Piece(HeaderBlockRole.Dimension, "dimension-01", " ", "lateral");

            Assert.Equal(HeaderBlockRole.Dimension, instance.Role);
            Assert.Equal("dimension-01", instance.PieceId);
            Assert.Equal("lateral", instance.View);
            Assert.True(string.IsNullOrWhiteSpace(instance.BlockName));
        }

        [Fact]
        public void AUTH12_CURRENT_AVAILABILITY_DISTINGUISHES_FOUND_AND_MISSING_ON_THE_CONCRETE_CONTRACT()
        {
            var requirement = new LibraryBlockRequirement("VIGA.100");

            var found = new LibraryBlockAvailabilityFact(requirement, LibraryBlockAvailability.Found);
            var missing = new LibraryBlockAvailabilityFact(requirement, LibraryBlockAvailability.Missing);

            Assert.Same(requirement, found.Requirement);
            Assert.Equal(LibraryBlockAvailability.Found, found.Availability);
            Assert.Same(requirement, missing.Requirement);
            Assert.Equal(LibraryBlockAvailability.Missing, missing.Availability);
        }

        [Fact]
        public void AUTH12_CURRENT_FLOW_IMPORTS_BEFORE_THE_FINAL_QUERY()
        {
            var calls = new List<string>();
            var requirements = new[] { new LibraryBlockRequirement("VIGA.100") };
            var result = LibraryBlockAvailabilityFlow.Observe(
                requirements, true, new RecordingQuery(calls), new RecordingImporter(calls));

            Assert.Equal(new[] { "import", "query" }, calls);
            Assert.True(result.Import.WasAttempted);
            Assert.Equal(LibraryBlockAvailability.Found, Assert.Single(result.Facts).Availability);
        }

        private static HeaderBlockInstance Piece(
            HeaderBlockRole role, string pieceId, string key, string view)
            => new HeaderBlockInstance
            {
                Role = role,
                PieceId = pieceId,
                BlockName = key,
                View = view,
                Insertion = new Point2D(0, 0),
                ConnectionAnchor = new Point2D(0, 0),
            };

        private sealed class RecordingQuery : ILibraryBlockQuery
        {
            private readonly List<string> calls;

            public RecordingQuery(List<string> calls) => this.calls = calls;

            public IReadOnlyList<LibraryBlockAvailabilityFact> Query(
                IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                calls.Add("query");
                return requirements
                    .Select(item => new LibraryBlockAvailabilityFact(item, LibraryBlockAvailability.Found))
                    .ToArray();
            }
        }

        private sealed class RecordingImporter : ILibraryBlockImporter
        {
            private readonly List<string> calls;

            public RecordingImporter(List<string> calls) => this.calls = calls;

            public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                calls.Add("import");
                return new LibraryBlockImportResult(true, requirements.Count);
            }
        }
    }
}
