using System;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public class SharedPhysicalFactsTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";

        [Fact]
        public void AUTH06_SNAPSHOT_PRESERVES_PHYSICAL_IDENTITY_ADDRESS_COUNT_AND_ORIGIN()
        {
            var selection = RackPhysicalSelection.Classify(
                new[]
                {
                    new RackPhysicalReferenceSnapshot(
                        "REF-1", true, RackPhysicalSpace.ModelSpace, false, "DEF-1", new Point2D(125, -40)),
                },
                new[] { Definition("DEF-1", Envelope(RackA), directReferenceCount: 3) },
                Known);

            var member = Assert.Single(selection.SelectedMembers);
            Assert.Equal("REF-1", member.PhysicalKey);
            Assert.Equal("DEF-1", member.DefinitionKey);
            Assert.Equal(new Point2D(125, -40), member.Origin);
            Assert.Equal(3, member.DirectReferenceCount);
            Assert.Equal(RackA, member.RackId);
            Assert.True(member.HasAddress);
            Assert.Equal(RackViewAddress.FlowEnd(RackFlowEnd.Exit), member.Address);
            Assert.Equal("frontal", member.ViewSyntax.View);
        }

        [Fact]
        public void AUTH06_FAIL_CLOSED_STATES_REMAIN_DISTINCT_AND_NEVER_SYNTHESIZE_IDENTITY()
        {
            var selection = RackPhysicalSelection.Classify(
                new[]
                {
                    new RackPhysicalReferenceSnapshot("NONBLOCK", false, true, null),
                    new RackPhysicalReferenceSnapshot("PAPER", true, RackPhysicalSpace.PaperSpace, false, null, new Point2D(0, 0)),
                    new RackPhysicalReferenceSnapshot("XREF", true, RackPhysicalSpace.ModelSpace, true, "DX", new Point2D(0, 0)),
                    new RackPhysicalReferenceSnapshot("MISSING", true, true, "DM"),
                    new RackPhysicalReferenceSnapshot("UNREADABLE", true, true, "DU"),
                    new RackPhysicalReferenceSnapshot("FOREIGN", true, true, "DF"),
                    new RackPhysicalReferenceSnapshot("UNKNOWN", true, true, "DK"),
                    new RackPhysicalReferenceSnapshot("NOID", true, true, "DI"),
                },
                new[]
                {
                    Definition("DU", "{not-json"),
                    Definition("DF", Envelope(RackA, "cabecera")),
                    Definition("DK", Envelope(RackA, "future-rack")),
                    Definition("DI", Envelope(null)),
                },
                kind => kind == "cabecera"
                    ? RackPhysicalKindDisposition.Foreign
                    : Known(kind));

            Assert.Contains(selection.Members, x => x.PhysicalKey == "NONBLOCK" && x.Disposition == RackPhysicalMemberDisposition.NotBlock);
            Assert.Contains(selection.Members, x => x.PhysicalKey == "PAPER" && x.Disposition == RackPhysicalMemberDisposition.WrongSpace);
            Assert.Contains(selection.Members, x => x.PhysicalKey == "XREF" && x.Disposition == RackPhysicalMemberDisposition.Xref);
            Assert.Contains(selection.Members, x => x.PhysicalKey == "MISSING" && x.Disposition == RackPhysicalMemberDisposition.MissingDefinition);
            Assert.Contains(selection.Members, x => x.PhysicalKey == "UNREADABLE" && x.Disposition == RackPhysicalMemberDisposition.Unreadable);
            Assert.Contains(selection.Members, x => x.PhysicalKey == "FOREIGN" && x.Disposition == RackPhysicalMemberDisposition.Foreign);
            Assert.Contains(selection.Members, x => x.PhysicalKey == "UNKNOWN" && x.Disposition == RackPhysicalMemberDisposition.UnknownKind);

            var missingIdentity = Assert.Single(selection.SelectedMembers, x => x.PhysicalKey == "NOID");
            Assert.Equal(RackPhysicalIdentityFact.Missing, missingIdentity.Identity);
            Assert.Null(missingIdentity.RackId);
        }

        [Fact]
        public void AUTH07_DEDUPES_IN_STABLE_ORDER_AND_GROUPS_ONLY_OBSERVED_RACK_IDS()
        {
            var selection = RackPhysicalSelection.Classify(
                new[]
                {
                    new RackPhysicalReferenceSnapshot("R2", true, true, "D2"),
                    new RackPhysicalReferenceSnapshot("R1", true, true, "D1"),
                    new RackPhysicalReferenceSnapshot("R2", true, true, "D2"),
                    new RackPhysicalReferenceSnapshot("R3", true, true, "D3"),
                },
                new[]
                {
                    Definition("D1", Envelope(RackA)),
                    Definition("D2", Envelope(RackA.ToUpperInvariant())),
                    Definition("D3", Envelope(null)),
                },
                Known);

            Assert.Equal(new[] { "R2", "R1", "R3" }, selection.Members.Select(x => x.PhysicalKey));
            Assert.Equal(1, selection.DuplicatePhysicalMembers);
            var group = Assert.Single(selection.RackGroups);
            Assert.Equal(RackA, group.RackId, ignoreCase: true);
            Assert.Equal(new[] { "R2", "R1" }, group.Members.Select(x => x.PhysicalKey));
            Assert.DoesNotContain(group.Members, x => x.PhysicalKey == "R3");
        }

        [Fact]
        public void AUTH07_CONTRACT_HAS_NO_COPY_OR_GENERATED_IDENTITY_POLICY()
        {
            var publicNames = typeof(RackPhysicalSelection).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(RackPhysicalSelection).Namespace
                    && (type.Name.StartsWith("RackPhysical", StringComparison.Ordinal)
                        || type.Name.StartsWith("RackTransform", StringComparison.Ordinal)))
                .SelectMany(type => type.GetMembers())
                .Select(member => member.Name)
                .ToArray();

            Assert.DoesNotContain(publicNames, name => name.Contains("NewRackId", StringComparison.Ordinal));
            Assert.DoesNotContain(publicNames, name => name.Contains("CopyCount", StringComparison.Ordinal));
            Assert.DoesNotContain(publicNames, name => name.Contains("CopyName", StringComparison.Ordinal));
            Assert.DoesNotContain(publicNames, name => name.Contains("Restamp", StringComparison.Ordinal));
        }

        [Fact]
        public void AUTH06_APPLICATION_CONTRACT_EXPOSES_NO_AUTOCAD_RUNTIME_TYPES()
        {
            var boundaryTypes = typeof(RackPhysicalSelection).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(RackPhysicalSelection).Namespace
                    && type.Name.StartsWith("RackPhysical", StringComparison.Ordinal))
                .SelectMany(type => type.GetMembers())
                .SelectMany(member => member switch
                {
                    System.Reflection.PropertyInfo property => new[] { property.PropertyType },
                    System.Reflection.MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType)
                        .Append(method.ReturnType),
                    System.Reflection.ConstructorInfo constructor => constructor.GetParameters()
                        .Select(parameter => parameter.ParameterType),
                    _ => Array.Empty<Type>(),
                })
                .Select(type => type.FullName ?? type.Name)
                .ToArray();

            Assert.DoesNotContain(boundaryTypes, name => name.Contains("Autodesk.AutoCAD", StringComparison.Ordinal));
            Assert.DoesNotContain(boundaryTypes, name => name.EndsWith(".ObjectId", StringComparison.Ordinal));
            Assert.DoesNotContain(boundaryTypes, name => name.EndsWith(".Database", StringComparison.Ordinal));
            Assert.DoesNotContain(boundaryTypes, name => name.EndsWith(".Transaction", StringComparison.Ordinal));
            Assert.DoesNotContain(boundaryTypes, name => name.EndsWith(".BlockReference", StringComparison.Ordinal));
        }

        private static RackPhysicalDefinitionSnapshot Definition(
            string key, string payload, int directReferenceCount = 1)
            => new RackPhysicalDefinitionSnapshot(key, payload, "BLOCK_" + key, directReferenceCount);

        private static string Envelope(string id, string kind = RackEmbedDocument.KindDynamic)
            => new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = kind,
                Id = id,
                Name = "Rack",
                View = RackEmbedDocument.ViewFrontal,
                Section = 0,
                Design = "{}",
            });

        private static RackPhysicalKindDisposition Known(string kind)
            => new[]
            {
                RackEmbedDocument.KindSelective,
                RackEmbedDocument.KindDynamic,
                RackEmbedDocument.KindPushBack,
                RackEmbedDocument.KindCantilever,
                RackEmbedDocument.KindCabecera,
                RackEmbedDocument.KindCama,
            }.Contains(kind, StringComparer.OrdinalIgnoreCase)
                ? RackPhysicalKindDisposition.Known
                : RackPhysicalKindDisposition.Unknown;
    }

    public class RackTransformFactsTests
    {
        [Fact]
        public void AUTH08_REUSES_TRANSFORM2D_AND_REPORTS_IDENTITY_TRANSLATION_ROTATION_SCALE()
        {
            var transform = Transform2D.Scale(2)
                .Then(Transform2D.RotationDegrees(90))
                .Then(Transform2D.Translation(5, -3));

            var result = RackTransformFacts.Describe(transform, tolerance: 1e-9);

            Assert.True(result.IsSupported);
            Assert.Equal(new Vector2D(5, -3), result.Facts.Translation);
            Assert.Equal(Math.PI / 2, result.Facts.RotationRadians, 10);
            Assert.Equal(4, result.Facts.Determinant, 10);
            Assert.Equal(2, result.Facts.UniformScale, 10);
            Assert.False(result.Facts.IsReflection);
        }

        [Fact]
        public void AUTH08_REPORTS_REFLECTION_AS_FACT_WITHOUT_ACCEPTANCE_POLICY()
        {
            var result = RackTransformFacts.Describe(Transform2D.MirrorAboutY, 1e-9);

            Assert.True(result.IsSupported);
            Assert.Equal(-1, result.Facts.Determinant, 10);
            Assert.True(result.Facts.IsReflection);
            Assert.Equal(1, result.Facts.UniformScale, 10);
        }

        [Theory]
        [InlineData(2, 0, 0, 3, RackTransformFailure.NonUniformScale)]
        [InlineData(1, 1, 0, 1, RackTransformFailure.NonOrthogonal)]
        [InlineData(0, 0, 0, 0, RackTransformFailure.Degenerate)]
        public void AUTH08_UNSUPPORTED_MATRICES_FAIL_CLOSED(
            double m11, double m12, double m21, double m22, RackTransformFailure expected)
        {
            var result = RackTransformFacts.Describe(new Transform2D(m11, m12, m21, m22, 4, 5), 1e-9);

            Assert.False(result.IsSupported);
            Assert.Equal(expected, result.Failure);
        }

        [Fact]
        public void AUTH08_CALLER_TOLERANCE_CONTROLS_DECOMPOSITION()
        {
            var almostUniform = new Transform2D(1, 0, 0, 1.00001, 0, 0);

            Assert.False(RackTransformFacts.Describe(almostUniform, 1e-8).IsSupported);
            Assert.True(RackTransformFacts.Describe(almostUniform, 1e-3).IsSupported);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void AUTH08_INVALID_TOLERANCE_IS_TYPED(double tolerance)
        {
            var result = RackTransformFacts.Describe(Transform2D.Identity, tolerance);

            Assert.False(result.IsSupported);
            Assert.Equal(RackTransformFailure.InvalidTolerance, result.Failure);
        }
    }
}
