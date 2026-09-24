using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-59 F1 executable characterization. These tests describe neutral facts only; product acceptance,
    /// projection policy and AutoCAD extraction remain outside this fixture.
    /// </summary>
    public sealed class I59F1CharacterizationTests
    {
        [Theory]
        [MemberData(nameof(TransformCases))]
        public void AUTH08_TRANSFORM_MATRIX_REMAINS_OBSERVABLE(
            string caseId,
            Transform2D transform,
            double expectedAngle,
            double expectedDeterminant,
            bool expectedReflection,
            double expectedScale)
        {
            var result = RackTransformFacts.Describe(transform, 1e-9);

            Assert.True(result.IsSupported, caseId + " must remain a neutral observable fact, not a policy rejection.");
            Assert.Equal(expectedAngle, result.Facts.RotationRadians, 10);
            Assert.Equal(expectedDeterminant, result.Facts.Determinant, 10);
            Assert.Equal(expectedReflection, result.Facts.IsReflection);
            Assert.Equal(expectedScale, result.Facts.UniformScale, 10);
        }

        public static IEnumerable<object[]> TransformCases()
        {
            yield return new object[] { "identity", Transform2D.Identity, 0.0, 1.0, false, 1.0 };
            yield return new object[]
            {
                "rotation-translation-uniform",
                Transform2D.Scale(2).Then(Transform2D.RotationDegrees(90)).Then(Transform2D.Translation(5, -3)),
                Math.PI / 2,
                4.0,
                false,
                2.0,
            };
            yield return new object[]
            {
                "unit-half-turn",
                new Transform2D(-1, 0, 0, -1, 7, 11),
                Math.PI,
                1.0,
                false,
                1.0,
            };
            yield return new object[]
            {
                "unit-reflection",
                Transform2D.MirrorAboutY,
                Math.PI,
                -1.0,
                true,
                1.0,
            };
            yield return new object[]
            {
                "non-uniform",
                new Transform2D(2, 0, 0, 3, 0, 0),
                0.0,
                6.0,
                false,
                Math.Sqrt(6),
            };
        }

        [Fact]
        public void AUTH08_ANGLE_MATRIX_CANONICALIZES_NEGATIVE_PI_TO_POSITIVE_PI()
        {
            var result = RackTransformFacts.Describe(Transform2D.Rotation(-Math.PI), 1e-9);

            Assert.True(result.IsSupported);
            Assert.Equal(Math.PI, result.Facts.RotationRadians, 12);
        }

        [Theory]
        [InlineData("Unit")]
        [InlineData("UnitHalfTurn")]
        [InlineData("UnitReflectionXY")]
        [InlineData("UnitNegativeZ")]
        [InlineData("NonUnitUniform")]
        [InlineData("NonUniform")]
        public void AUTH08_SCALE_CLASSIFICATION_MATRIX_IS_PUBLIC_AND_TYPED(string expectedClass)
        {
            var enumNames = SharedPublicTypes()
                .Where(type => type.IsEnum && type.Name.Contains("Scale", StringComparison.OrdinalIgnoreCase))
                .SelectMany(Enum.GetNames)
                .ToArray();

            Assert.Contains(expectedClass, enumNames, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData("position-xyz", "Position", "Translation", true)]
        [InlineData("per-axis-scale", "ScaleFactors", "ScaleX", true)]
        [InlineData("normal", "Normal", null, true)]
        [InlineData("normal-is-world-z", "NormalIsWorldZ", "IsWorldZ", false)]
        [InlineData("source-origin", "Origin", null, true)]
        public void AUTH08_PLACEMENT_NORMAL_AND_ORIGIN_MATRIX_HAS_A_NEUTRAL_PUBLIC_CARRIER(
            string caseId,
            string primaryName,
            string alternateName,
            bool requiresThreeCoordinates)
        {
            var properties = SharedPublicTypes().SelectMany(type => type.GetProperties()).ToArray();
            var property = properties.FirstOrDefault(candidate =>
                candidate.Name.Equals(primaryName, StringComparison.OrdinalIgnoreCase)
                || (alternateName != null && candidate.Name.Equals(alternateName, StringComparison.OrdinalIgnoreCase)));

            Assert.True(property != null, caseId + " is not exposed by the current AUTH-08 boundary.");
            if (requiresThreeCoordinates)
            {
                Assert.True(HasCoordinates(property.PropertyType, "X", "Y", "Z"),
                    caseId + " must preserve all three source coordinates.");
            }
        }

        [Theory]
        [InlineData("Required")]
        [InlineData("OptionalVisual")]
        [InlineData("NotApplicable")]
        public void AUTH12_REQUIREMENT_ROLE_MATRIX_IS_PUBLIC_AND_TYPED(string expectedRole)
        {
            var enumNames = SharedPublicTypes()
                .Where(type => type.IsEnum && type.Name.Contains("Role", StringComparison.OrdinalIgnoreCase))
                .SelectMany(Enum.GetNames)
                .ToArray();

            Assert.Contains(expectedRole, enumNames, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData("PieceId", null)]
        [InlineData("ViewAddress", "View")]
        [InlineData("Role", null)]
        [InlineData("LibraryKey", "Key")]
        public void AUTH12_REQUIREMENT_TRACEABILITY_MATRIX_PRESERVES_EACH_FACT(
            string primaryName,
            string alternateName)
        {
            var properties = typeof(LibraryBlockRequirement).GetProperties();

            Assert.Contains(properties, property =>
                property.Name.Equals(primaryName, StringComparison.OrdinalIgnoreCase)
                || (alternateName != null && property.Name.Equals(alternateName, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void AUTH12_EQUAL_KEYS_FROM_DIFFERENT_PIECES_ARE_NOT_COLLAPSED()
        {
            var plan = new HeaderRunPlan(
                Array.Empty<HeaderGroup>(),
                new[]
                {
                    Piece(HeaderBlockRole.Beam, "beam-left", "VIGA.100", "frontal"),
                    Piece(HeaderBlockRole.Beam, "beam-right", "VIGA.100", "lateral"),
                });

            var requirements = RackBlockRequirementExtractors.HeaderRun.Extract(plan);

            Assert.Equal(2, requirements.Count);
        }

        [Fact]
        public void AUTH12_REQUIRED_BLANK_KEY_REMAINS_AN_OBSERVABLE_STRUCTURAL_FACT()
        {
            var plan = new HeaderRunPlan(
                Array.Empty<HeaderGroup>(),
                new[] { Piece(HeaderBlockRole.Beam, "beam-without-key", " ", "frontal") });

            var requirement = Assert.Single(RackBlockRequirementExtractors.HeaderRun.Extract(plan));

            Assert.Equal("beam-without-key", ReadString(requirement, "PieceId"));
            Assert.True(string.IsNullOrWhiteSpace(ReadString(requirement, "LibraryKey", "Key")));
            Assert.Equal("Required", ReadValue(requirement, "Role")?.ToString());
        }

        [Theory]
        [InlineData(HeaderBlockRole.Beam, "Required")]
        [InlineData(HeaderBlockRole.Pallet, "OptionalVisual")]
        [InlineData(HeaderBlockRole.Annotation, "NotApplicable")]
        [InlineData(HeaderBlockRole.Dimension, "NotApplicable")]
        public void AUTH12_SOURCE_ROLE_MATRIX_IS_TRACEABLE_PER_INSTANCE(
            HeaderBlockRole sourceRole,
            string expectedRequirementRole)
        {
            var plan = new HeaderRunPlan(
                Array.Empty<HeaderGroup>(),
                new[] { Piece(sourceRole, sourceRole.ToString(), "BLOCK." + sourceRole, "lateral") });

            var requirement = Assert.Single(RackBlockRequirementExtractors.HeaderRun.Extract(plan));

            Assert.Equal(expectedRequirementRole, ReadValue(requirement, "Role")?.ToString());
        }

        [Theory]
        [InlineData("Ok")]
        [InlineData("FileMissing")]
        [InlineData("Unknown")]
        public void AUTH12_LIBRARY_AVAILABILITY_MATRIX_REMAINS_DISTINCT(string expectedAvailability)
            => Assert.Contains(expectedAvailability, Enum.GetNames(typeof(LibraryBlockAvailability)), StringComparer.Ordinal);

        [Theory]
        [InlineData("block-absent", "Missing|BlockMissing")]
        [InlineData("library-unavailable", "FileMissing|LibraryUnavailable")]
        [InlineData("availability-unknown", "Unknown")]
        public void AUTH12_AVAILABILITY_TRACEABILITY_DOES_NOT_COLLAPSE_FAILURE_CAUSES(
            string caseId,
            string acceptedAvailabilityNames)
        {
            var names = Enum.GetNames(typeof(LibraryBlockAvailability));
            Assert.Contains(acceptedAvailabilityNames.Split('|'), candidate => names.Contains(candidate, StringComparer.Ordinal));
            Assert.False(string.IsNullOrWhiteSpace(caseId));
        }

        private static HeaderBlockInstance Piece(
            HeaderBlockRole role,
            string pieceId,
            string key,
            string view)
            => new HeaderBlockInstance
            {
                Role = role,
                PieceId = pieceId,
                BlockName = key,
                View = view,
                Insertion = new Point2D(0, 0),
                ConnectionAnchor = new Point2D(0, 0),
            };

        private static Type[] SharedPublicTypes()
            => typeof(RackTransformFacts).Assembly.GetExportedTypes()
                .Where(type => type.Namespace == typeof(RackTransformFacts).Namespace)
                .ToArray();

        private static bool HasCoordinates(Type type, params string[] names)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .ToArray();
            return names.All(name => properties.Contains(name, StringComparer.OrdinalIgnoreCase));
        }

        private static object ReadValue(object instance, params string[] names)
        {
            var property = instance.GetType().GetProperties()
                .FirstOrDefault(candidate => names.Contains(candidate.Name, StringComparer.OrdinalIgnoreCase));
            Assert.True(property != null, "Missing traceability member: " + string.Join(" or ", names));
            return property.GetValue(instance);
        }

        private static string ReadString(object instance, params string[] names)
            => ReadValue(instance, names) as string;
    }
}
