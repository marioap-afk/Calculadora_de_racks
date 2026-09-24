using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class I59F4ConformanceTests
    {
        private static readonly string[] FoundationFields =
        {
            "Name:",
            "Status:",
            "Authority:",
            "Persistence:",
            "Mutation contract:",
            "Extension point:",
            "Decision source:",
            "Protecting tests:",
            "Known limitations:",
            "Last changed by:"
        };

        [Fact]
        public void CT59_16_PRODUCT_SCOPE_EXPOSES_NO_PERSISTENCE_BOM_NAMING_OR_CONSUMER_POLICY()
        {
            var types = new[]
            {
                typeof(RackSourcePlacementInput),
                typeof(RackSourceTransformClassifier),
                typeof(RackSourceTransformFactsV2),
                typeof(LibraryPieceRequirement),
                typeof(RackHeaderBlockRequirementRoleClassifier),
                typeof(LibraryPieceAvailabilityFlowV2),
                typeof(ExternalLibraryAvailabilityFacts)
            };

            var referencedNamespaces = types
                .SelectMany(ReferencedTypes)
                .Select(type => type.Namespace ?? string.Empty)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            Assert.DoesNotContain(referencedNamespaces, value => value.Contains(".Persistence", StringComparison.Ordinal));
            Assert.DoesNotContain(referencedNamespaces, value => value.Contains(".Bom", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(referencedNamespaces, value => value.Contains(".Naming", StringComparison.Ordinal));

            var applicationSource = File.ReadAllText(Path.Combine(
                RepositoryRoot(), "src", "RackCad.Application", "Systems", "Shared", "RackSourceTransformFactsV2.cs"));
            Assert.DoesNotContain("Rigid", applicationSource);
            Assert.DoesNotContain("Orthographic", applicationSource);
            Assert.DoesNotContain("Accepted", applicationSource);
            Assert.DoesNotContain("Remedy", applicationSource);
        }

        [Fact]
        public void CT59_17_FOUNDATIONS_DRAFT_HAS_EXACTLY_THE_TEN_FIELDS_AND_REQUIRED_LIMITATIONS()
        {
            var path = Path.Combine(
                RepositoryRoot(), "docs", "automation", "evidence", "I-59-f4",
                "FOUNDATIONS-auth08-auth12-draft.md");
            var lines = File.ReadAllLines(path);
            var actualFields = lines
                .Select(line => FoundationFields.SingleOrDefault(field => line.StartsWith(field, StringComparison.Ordinal)))
                .Where(field => field != null)
                .ToArray();

            Assert.Equal(FoundationFields, actualFields);
            Assert.Equal(FoundationFields.Length, actualFields.Distinct(StringComparer.Ordinal).Count());

            var draft = string.Join(Environment.NewLine, lines);
            Assert.Contains("Status: STABLE", draft);
            Assert.Contains("consumer policy queda fuera", draft);
            Assert.Contains("I-55 G14 no queda desbloqueado", draft);
            Assert.Contains("AUTH-15 queda fuera", draft);
            Assert.Contains("No existe persistencia I-59", draft);
            Assert.Contains("V1 sigue coexistiendo", draft);
            Assert.Contains("Presencia en el dibujo activo y presencia en la biblioteca externa son hechos distintos", draft);
            Assert.Contains("Final Candidate re-verification = REQUIRED before publication", draft);
        }

        [Fact]
        public void F4_I55_G14_CAN_CONSUME_ALL_FROZEN_TRANSFORM_FACTS_WITHOUT_RECONSTRUCTION()
        {
            var input = new RackSourcePlacementInput(
                11, 12, 13,
                -Math.PI,
                -2, 3, -4,
                0, 0, 1,
                21, 22, 23);
            var result = RackSourceTransformClassifier.Classify(
                input, new RackSourceTransformTolerance(1e-9, 1e-9, 1e-9));
            var facts = result.Facts;

            Assert.True(result.HasFacts);
            Assert.Equal(11, facts.ReferencePosition.X);
            Assert.Equal(21, facts.DefinitionOrigin.X);
            Assert.Equal(Math.PI, facts.RotationRadians, 12);
            Assert.Equal((-2.0, 3.0, -4.0), (facts.ScaleX, facts.ScaleY, facts.ScaleZ));
            Assert.Equal((RackScaleSign.Negative, RackScaleSign.Positive, RackScaleSign.Negative),
                (facts.SignX, facts.SignY, facts.SignZ));
            Assert.Equal(-6, facts.ReferenceBasisDeterminantXY);
            Assert.True(facts.ReferenceBasisIsReflectionXY);
            Assert.False(facts.IsUniformScale);
            Assert.False(facts.IsUnitScale);
            Assert.False(facts.HasPlanarHalfTurnSign);
            Assert.True(facts.IsNegativeZ);
            Assert.Equal(1, facts.Normal.Z);
            Assert.True(facts.NormalIsWorldZ);
            Assert.Equal(1e-9, facts.Tolerance.Scale);
        }

        [Fact]
        public void F4_I55_G14_CAN_CONSUME_TRACEABLE_REQUIREMENTS_AND_EXTERNAL_LIBRARY_FACTS()
        {
            var requirement = new LibraryPieceRequirement(
                "piece-17", RackViewAddress.Fondo(3), RequirementRole.Required, " ");
            var external = ExternalLibraryAvailabilityFacts.Observe(
                "BLOCK-A", LibraryAvailability.FileMissing, ExternalLibraryBlockObservation.NotObserved);

            Assert.Equal("piece-17", requirement.PieceId);
            Assert.Equal(RackViewAddress.Fondo(3), requirement.ViewAddress);
            Assert.Equal(RequirementRole.Required, requirement.Role);
            Assert.Equal(" ", requirement.LibraryKey);
            Assert.Equal(RequirementKeyState.KeyMissing, requirement.KeyState);
            Assert.Equal(LibraryAvailability.FileMissing, external.LibraryAvailability);
            Assert.Equal(LibraryBlockPresence.Unknown, external.BlockPresence);
        }

        private static IEnumerable<Type> ReferencedTypes(Type type)
        {
            yield return type;
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                yield return property.PropertyType;
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                yield return method.ReturnType;
                foreach (var parameter in method.GetParameters())
                {
                    yield return parameter.ParameterType;
                }
            }
        }

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }
}
