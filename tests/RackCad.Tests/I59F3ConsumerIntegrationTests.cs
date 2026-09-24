using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class I59F3ConsumerIntegrationTests
    {
        [Fact]
        public void F3_AUTH08_V1_REMAINS_SOURCE_AND_BEHAVIOR_COMPATIBLE()
        {
            var nonUniform = RackTransformFacts.Describe(new Transform2D(2, 0, 0, 3, 5, 7), 1e-9);
            var negativePi = RackTransformFacts.Describe(Transform2D.Rotation(-Math.PI), 1e-9);

            Assert.False(nonUniform.IsSupported);
            Assert.Equal(RackTransformFailure.NonUniformScale, nonUniform.Failure);
            Assert.True(negativePi.IsSupported);
            Assert.Equal(-Math.PI, negativePi.Facts.RotationRadians, 12);
        }

        [Fact]
        public void F3_PREPARATION_V1_REMAINS_UNCHANGED_WHILE_V2_PRESERVES_INSTANCE_IDENTITY()
        {
            var address = RackViewAddress.Fondo(2);
            var plan = Plan(
                Piece("beam-a", "SAME", HeaderBlockRole.Beam, "misleading-frontal"),
                Piece("beam-b", "same", HeaderBlockRole.Beam, "misleading-lateral"),
                Piece("beam-blank", " ", HeaderBlockRole.Beam, "misleading-planta"));
            var port = RackViewPreparationPorts.Cabecera<Marker, HeaderRunPlan>(
                (_, __) => plan,
                candidate => candidate.Equals(address),
                RackBlockRequirementExtractors.HeaderRun);

            var v1 = port.Prepare(new Marker(), address, Frame(), "generated-name");
            var v2 = port.PrepareV2(
                new Marker(), address, Frame(), "generated-name", RackHeaderPieceRequirementExtractorV2.Instance);

            Assert.True(v1.IsSuccess);
            Assert.Same(plan, v1.Prepared.Payload);
            Assert.Equal(new[] { "SAME" }, v1.Prepared.BlockRequirements.Select(item => item.Key));
            Assert.True(v2.IsSuccess);
            Assert.Same(plan, v2.Prepared.Payload);
            Assert.Equal(v1.Prepared.BlockRequirements.Select(item => item.Key),
                v2.Prepared.BlockRequirements.Select(item => item.Key));
            Assert.Equal(new[] { "beam-a", "beam-b", "beam-blank" },
                v2.Prepared.PieceRequirements.Select(item => item.PieceId));
            Assert.All(v2.Prepared.PieceRequirements, item => Assert.Equal(address, item.ViewAddress));
            Assert.Equal(RequirementKeyState.KeyMissing, v2.Prepared.PieceRequirements[2].KeyState);
        }

        [Fact]
        public void F3_V1_QUERY_IMPORT_AND_FINAL_QUERY_AUTHORITY_REMAIN_UNCHANGED()
        {
            var events = new List<string>();
            var query = new SequencedV1Query(events, LibraryBlockAvailability.Found);
            var requirements = new[] { new LibraryBlockRequirement("BLOCK-A") };

            var result = LibraryBlockAvailabilityFlow.Observe(
                requirements, true, query, new RecordingImporter(events));

            Assert.Equal(new[] { "import:BLOCK-A", "query:BLOCK-A" }, events);
            Assert.Equal(LibraryBlockAvailability.Found, Assert.Single(result.Facts).Availability);
        }

        [Fact]
        public void F3_V2_BRIDGE_QUERIES_ONE_IO_KEY_AND_REATTACHES_MULTIPLE_IDENTITIES()
        {
            var events = new List<string>();
            var requirements = new[]
            {
                new LibraryPieceRequirement("piece-a", RackViewAddress.Fondo(0), RequirementRole.Required, "SAME"),
                new LibraryPieceRequirement("piece-b", RackViewAddress.Fondo(1), RequirementRole.OptionalVisual, "same"),
            };
            var bridge = CreateBridge(
                new SequencedV1Query(events, LibraryBlockAvailability.Found),
                LibraryAvailability.Ok);

            var result = LibraryPieceAvailabilityFlowV2.Observe(
                requirements, false, bridge, new RecordingImporter(events));

            Assert.Equal(new[] { "query:SAME" }, events);
            Assert.Equal(new[] { "piece-a", "piece-b" },
                result.Facts.Select(item => item.Requirement.PieceId));
            Assert.All(result.Facts, item =>
            {
                Assert.Equal(LibraryAvailability.Ok, item.LibraryAvailability);
                Assert.Equal(LibraryBlockPresence.Present, item.BlockPresence);
            });
        }

        [Fact]
        public void F3_V2_BRIDGE_REQUIRES_EXPLICIT_LIBRARY_EVIDENCE_BEFORE_DECLARING_BLOCK_MISSING()
        {
            var requirement = new[] { new LibraryBlockRequirement("BLOCK-A") };
            var missing = new SequencedV1Query(new List<string>(), LibraryBlockAvailability.Missing);

            var knownLibrary = CreateBridge(missing, LibraryAvailability.Ok).Query(requirement);
            var unknownLibrary = CreateBridge(missing, LibraryAvailability.Unknown).Query(requirement);
            var missingLibrary = CreateBridge(missing, LibraryAvailability.FileMissing).Query(requirement);

            Assert.Equal(LibraryBlockPresence.BlockMissing, Assert.Single(knownLibrary).BlockPresence);
            Assert.Equal(LibraryBlockPresence.Unknown, Assert.Single(unknownLibrary).BlockPresence);
            Assert.Equal(LibraryBlockPresence.Unknown, Assert.Single(missingLibrary).BlockPresence);
        }

        [Fact]
        public void F3_I52_COMPATIBLE_TRANSFORM_FIXTURE_KEEPS_V1_REFLECTION_FACTS()
        {
            var result = RackTransformFacts.Describe(Transform2D.MirrorAboutY, 1e-9);

            Assert.True(result.IsSupported);
            Assert.True(result.Facts.IsReflection);
            Assert.Equal(-1, result.Facts.Determinant, 12);
            Assert.Equal(1, result.Facts.UniformScale, 12);
        }

        [Fact]
        public void F3_I55_G8_G9_G12_COMPATIBLE_FIXTURE_KEEPS_V1_MISSING_AFTER_IMPORT()
        {
            var events = new List<string>();
            var requirements = new[] { new LibraryBlockRequirement("MISSING") };

            var result = LibraryBlockAvailabilityFlow.Observe(
                requirements,
                true,
                new SequencedV1Query(events, LibraryBlockAvailability.Missing),
                new RecordingImporter(events));

            Assert.Equal(new[] { "import:MISSING", "query:MISSING" }, events);
            Assert.Equal(LibraryBlockAvailability.Missing, Assert.Single(result.Facts).Availability);
        }

        [Fact]
        public void F3_PLUGIN_EXPOSES_THE_NEUTRAL_CAPTURE_SEAM_WITHOUT_CONSUMER_POLICY()
        {
            var source = File.ReadAllText(Path.Combine(
                RepositoryRoot(), "src", "RackCad.Plugin", "Drawing", "RackSourcePlacementCaptureAdapter.cs"));

            Assert.Contains("RackSourcePlacementInput Capture", source);
            Assert.Contains("reference.Position", source);
            Assert.Contains("definition.Origin", source);
            Assert.DoesNotContain("RackSourceTransformClassifier", source);
            Assert.DoesNotContain("RackTransformFacts", source);
            Assert.DoesNotContain("Accepted", source);
            Assert.DoesNotContain("Remedy", source);
        }

        private static ILibraryPieceAvailabilityQuery CreateBridge(
            ILibraryBlockQuery query,
            LibraryAvailability libraryAvailability)
        {
            var type = typeof(LibraryPieceRequirement).Assembly.GetType(
                "RackCad.Application.Systems.Shared.LibraryPieceAvailabilityQueryV1Adapter",
                throwOnError: false);
            Assert.NotNull(type);
            var instance = Activator.CreateInstance(type, query, libraryAvailability);
            return Assert.IsAssignableFrom<ILibraryPieceAvailabilityQuery>(instance);
        }

        private static HeaderRunPlan Plan(params HeaderBlockInstance[] pieces)
            => new HeaderRunPlan(Array.Empty<HeaderGroup>(), pieces);

        private static HeaderBlockInstance Piece(string id, string key, HeaderBlockRole role, string view)
            => new HeaderBlockInstance
            {
                PieceId = id,
                BlockName = key,
                Role = role,
                View = view,
                Insertion = new Point2D(0, 0),
                ConnectionAnchor = new Point2D(0, 0),
            };

        private static RackViewFrameResult Frame()
            => RackViewFrame.TryCreate(
                RackViewAxisMap.RunHeight,
                RackPhysicalPoint.Zero,
                0,
                10,
                RackFrameEndpointConvention.PhysicalRunAxes,
                RackPhysicalVector.Zero);

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }

        private sealed class Marker
        {
        }

        private sealed class SequencedV1Query : ILibraryBlockQuery
        {
            private readonly List<string> events;
            private readonly LibraryBlockAvailability availability;

            public SequencedV1Query(List<string> events, LibraryBlockAvailability availability)
            {
                this.events = events;
                this.availability = availability;
            }

            public IReadOnlyList<LibraryBlockAvailabilityFact> Query(
                IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                events.Add("query:" + string.Join(",", requirements.Select(item => item.Key)));
                return requirements
                    .Select(item => new LibraryBlockAvailabilityFact(item, availability))
                    .ToArray();
            }
        }

        private sealed class RecordingImporter : ILibraryBlockImporter
        {
            private readonly List<string> events;

            public RecordingImporter(List<string> events) => this.events = events;

            public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                events.Add("import:" + string.Join(",", requirements.Select(item => item.Key)));
                return new LibraryBlockImportResult(true, requirements.Count);
            }
        }
    }
}
