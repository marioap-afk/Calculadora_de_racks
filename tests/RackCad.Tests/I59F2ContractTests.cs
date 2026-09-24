using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class I59F2ContractTests
    {
        private const string Shared = "RackCad.Application.Systems.Shared.";

        [Fact]
        public void CT59_01_02_RAW_INPUT_PUBLISHES_FINITE_XYZ_NORMAL_SCALES_AND_DISTINCT_ORIGIN()
        {
            var result = Classify(Input(11, 12, 13, 0.25, 2, 3, 4, 0, 1, 0, 21, 22, 23), Tolerance());

            Assert.Equal("Facts", Name(Property(result, "Outcome")));
            var facts = Property(result, "Facts");
            AssertPoint(Property(facts, "ReferencePosition"), 11, 12, 13);
            AssertPoint(Property(facts, "DefinitionOrigin"), 21, 22, 23);
            AssertVector(Property(facts, "Normal"), 0, 1, 0);
            Assert.Equal(2, Number(facts, "ScaleX"));
            Assert.Equal(3, Number(facts, "ScaleY"));
            Assert.Equal(4, Number(facts, "ScaleZ"));
        }

        [Theory]
        [InlineData(-Math.PI, Math.PI)]
        [InlineData(Math.PI, Math.PI)]
        [InlineData(-3 * Math.PI, Math.PI)]
        [InlineData(3 * Math.PI, Math.PI)]
        [InlineData(0.25, 0.25)]
        public void CT59_03_ROTATION_IS_CANONICAL(double source, double expected)
        {
            var facts = Facts(Classify(Input(rotation: source), Tolerance()));
            Assert.Equal(expected, Number(facts, "RotationRadians"), 12);
            Assert.True(Number(facts, "RotationRadians") > -Math.PI);
            Assert.True(Number(facts, "RotationRadians") <= Math.PI);
        }

        [Theory]
        [InlineData(-0.010001, 0.010001, "Negative", "Positive", true)]
        [InlineData(0.010001, 0.010001, "Positive", "Positive", false)]
        [InlineData(-0.010001, -0.010001, "Negative", "Negative", false)]
        [InlineData(-1, 1, "Negative", "Positive", true)]
        [InlineData(-1, -1, "Negative", "Negative", false)]
        public void CT59_04_05_SIGNS_AND_REFLECTION_USE_AXIS_TOLERANCE_ONLY(
            double x, double y, string signX, string signY, bool reflection)
        {
            var facts = Facts(Classify(Input(scaleX: x, scaleY: y), Tolerance(scale: 0.01)));

            Assert.Equal(signX, Name(Property(facts, "SignX")));
            Assert.Equal(signY, Name(Property(facts, "SignY")));
            Assert.Equal(x * y, Number(facts, "ReferenceBasisDeterminantXY"), 15);
            Assert.Equal(reflection, Boolean(facts, "ReferenceBasisIsReflectionXY"));
        }

        [Theory]
        [InlineData(0.01, 1)]
        [InlineData(-0.01, 1)]
        [InlineData(0.009999, -1)]
        [InlineData(1, 0.01)]
        public void CT59_05_AXIS_ON_OR_WITHIN_TOLERANCE_IS_DEGENERATE_WITHOUT_REFLECTION(
            double x, double y)
        {
            var result = Classify(Input(scaleX: x, scaleY: y), Tolerance(scale: 0.01));
            Assert.Equal("Degenerate", Name(Property(result, "Outcome")));
            Assert.False(Boolean(result, "HasFacts"));
        }

        [Fact]
        public void CT59_04_06_NON_UNIFORM_AND_INDEPENDENT_FACTS_ARE_PRESERVED()
        {
            var facts = Facts(Classify(Input(scaleX: -2, scaleY: -3, scaleZ: -4), Tolerance()));

            Assert.False(Boolean(facts, "IsUniformScale"));
            Assert.False(Boolean(facts, "IsUnitScale"));
            Assert.True(Boolean(facts, "HasPlanarHalfTurnSign"));
            Assert.False(Boolean(facts, "ReferenceBasisIsReflectionXY"));
            Assert.True(Boolean(facts, "IsNegativeZ"));
            Assert.Equal(6, Number(facts, "ReferenceBasisDeterminantXY"));
        }

        [Fact]
        public void CT59_06_UNIT_REFLECTION_HALF_TURN_AND_NEGATIVE_Z_ARE_INDEPENDENT()
        {
            var reflection = Facts(Classify(Input(scaleX: -1, scaleY: 1, scaleZ: 1), Tolerance()));
            var halfTurn = Facts(Classify(Input(scaleX: -1, scaleY: -1, scaleZ: 1), Tolerance()));
            var negativeZ = Facts(Classify(Input(scaleX: 1, scaleY: 1, scaleZ: -1), Tolerance()));

            Assert.True(Boolean(reflection, "IsUnitScale"));
            Assert.True(Boolean(reflection, "ReferenceBasisIsReflectionXY"));
            Assert.False(Boolean(reflection, "HasPlanarHalfTurnSign"));
            Assert.True(Boolean(halfTurn, "IsUnitScale"));
            Assert.True(Boolean(halfTurn, "HasPlanarHalfTurnSign"));
            Assert.False(Boolean(halfTurn, "ReferenceBasisIsReflectionXY"));
            Assert.True(Boolean(negativeZ, "IsNegativeZ"));
            Assert.False(Boolean(negativeZ, "HasPlanarHalfTurnSign"));
        }

        [Theory]
        [InlineData(0, 0, 1, 0.0001, true)]
        [InlineData(0, 0.00005, 1, 0.0001, true)]
        [InlineData(0, 0.00011, 1, 0.0001, false)]
        public void CT59_07_NORMAL_IS_PRESERVED_AND_WORLD_Z_USES_EXPLICIT_TOLERANCE(
            double x, double y, double z, double tolerance, bool expected)
        {
            var facts = Facts(Classify(Input(normalX: x, normalY: y, normalZ: z), Tolerance(normal: tolerance)));
            AssertVector(Property(facts, "Normal"), x, y, z);
            Assert.Equal(expected, Boolean(facts, "NormalIsWorldZ"));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CT59_08_NON_FINITE_RAW_INPUT_IS_VISIBLE(double value)
        {
            var result = Classify(Input(positionX: value), Tolerance());
            Assert.Equal("NonFinite", Name(Property(result, "Outcome")));
            Assert.False(Boolean(result, "HasFacts"));
        }

        [Fact]
        public void CT59_08_ZERO_NORMAL_IS_DEGENERATE()
        {
            var result = Classify(Input(normalX: 0, normalY: 0, normalZ: 0), Tolerance());
            Assert.Equal("Degenerate", Name(Property(result, "Outcome")));
            Assert.False(Boolean(result, "HasFacts"));
        }

        [Theory]
        [InlineData(-0.1, 0, 0)]
        [InlineData(0, -0.1, 0)]
        [InlineData(0, 0, -0.1)]
        [InlineData(double.NaN, 0, 0)]
        public void CT59_08_INVALID_TOLERANCE_IS_VISIBLE(double scale, double angle, double normal)
        {
            var result = Classify(Input(), Tolerance(scale, angle, normal));
            Assert.Equal("InvalidTolerance", Name(Property(result, "Outcome")));
            Assert.False(Boolean(result, "HasFacts"));
        }

        [Fact]
        public void CT59_05_V2_EXPOSES_NO_COMPOSITE_PLANAR_TRANSFORM()
        {
            var factsType = RequireType(Shared + "RackSourceTransformFactsV2");
            Assert.Null(factsType.GetProperty("PlanarTransform", BindingFlags.Public | BindingFlags.Instance));
            Assert.Null(factsType.GetProperty("Transform", BindingFlags.Public | BindingFlags.Instance));
        }

        public static IEnumerable<object[]> CurrentRoles()
        {
            var notApplicable = new[] { HeaderBlockRole.Annotation, HeaderBlockRole.Dimension };
            foreach (HeaderBlockRole role in Enum.GetValues(typeof(HeaderBlockRole)))
            {
                var expected = notApplicable.Contains(role)
                    ? "NotApplicable"
                    : role == HeaderBlockRole.Pallet ? "OptionalVisual" : "Required";
                yield return new object[] { role, expected };
            }
        }

        [Theory]
        [MemberData(nameof(CurrentRoles))]
        public void CT59_11_ALL_CURRENT_SOURCE_ROLES_HAVE_THE_FROZEN_TOTAL_MAPPING(
            HeaderBlockRole source, string expected)
        {
            var result = Call(Shared + "HeaderBlockRequirementRoleClassifier", "Classify", source);
            Assert.Equal("Classified", Name(Property(result, "Outcome")));
            Assert.Equal(expected, Name(Property(result, "Role")));
        }

        [Fact]
        public void CT59_11_UNKNOWN_SOURCE_ROLE_FAILS_VISIBLY()
        {
            var result = Call(Shared + "HeaderBlockRequirementRoleClassifier", "Classify", (HeaderBlockRole)999);
            Assert.Equal("UnknownSourceRole", Name(Property(result, "Outcome")));
            Assert.False(Boolean(result, "HasRole"));
        }

        [Fact]
        public void CT59_09_10_REQUIREMENTS_KEEP_INSTANCE_ID_ADDRESS_ROLE_AND_ORIGINAL_KEY()
        {
            var address = RackViewAddress.Fondo(7);
            var plan = Plan(
                Piece("piece-a", "SAME", HeaderBlockRole.Beam, "misleading-view"),
                Piece("piece-b", "SAME", HeaderBlockRole.Pallet, "other-view"),
                Piece("note", null, HeaderBlockRole.Annotation, "third-view"));

            var result = Extract(plan, address);
            Assert.Equal("Extracted", Name(Property(result, "Outcome")));
            var requirements = Items(Property(result, "Requirements"));
            Assert.Equal(3, requirements.Length);
            AssertRequirement(requirements[0], "piece-a", address, "Required", "SAME", "Present");
            AssertRequirement(requirements[1], "piece-b", address, "OptionalVisual", "SAME", "Present");
            AssertRequirement(requirements[2], "note", address, "NotApplicable", null, "KeyMissing");
        }

        [Fact]
        public void CT59_10_PREPARATION_V2_INJECTS_THE_AUTHORITATIVE_TYPED_ADDRESS()
        {
            var address = RackViewAddress.Fondo(4);
            var adapter = RackViewPreparationPorts.Cabecera<Marker, HeaderRunPlan>(
                (_, __) => Plan(Piece("piece", "KEY", HeaderBlockRole.Beam, "not-the-address")),
                candidate => candidate.Kind == DimensionViewKind.Frontal,
                RackBlockRequirementExtractors.HeaderRun);
            var extractorType = RequireType(Shared + "HeaderPieceRequirementExtractorV2");
            var extractor = extractorType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            Assert.NotNull(extractor);
            var method = adapter.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(candidate => candidate.Name == "PrepareV2" && candidate.GetParameters().Length == 5);

            var result = method.Invoke(adapter, new[] { new Marker(), (object)address, Frame(), "generated-name", extractor });

            Assert.True(Boolean(result, "IsSuccess"));
            var prepared = Property(result, "Prepared");
            Assert.Equal(address, (RackViewAddress)Property(prepared, "Address"));
            var requirement = Assert.Single(Items(Property(prepared, "PieceRequirements")));
            Assert.Equal(address, (RackViewAddress)Property(requirement, "ViewAddress"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CT59_12_REQUIRED_BLANK_KEY_STAYS_TRACEABLE(string key)
        {
            var result = Extract(Plan(Piece("piece", key, HeaderBlockRole.Beam)), RackViewAddress.Whole(DimensionViewKind.Frontal));
            var requirement = Assert.Single(Items(Property(result, "Requirements")));
            AssertRequirement(requirement, "piece", RackViewAddress.Whole(DimensionViewKind.Frontal), "Required", key, "KeyMissing");

            var projected = Items(Call(Shared + "LibraryPieceKeyProjection", "Project", Property(result, "Requirements")));
            Assert.Empty(projected);
        }

        [Fact]
        public void CT59_09_REPEATED_KEY_PROJECTS_ONCE_WITHOUT_COLLAPSING_REQUIREMENTS()
        {
            var address = RackViewAddress.Post(3);
            var result = Extract(Plan(
                Piece("a", "Same.Key", HeaderBlockRole.Post),
                Piece("b", "same.key", HeaderBlockRole.Beam)), address);

            Assert.Equal(2, Items(Property(result, "Requirements")).Length);
            var keys = Items(Call(Shared + "LibraryPieceKeyProjection", "Project", Property(result, "Requirements")));
            Assert.Single(keys);
            Assert.Equal("Same.Key", Text(keys[0], "Key"));
        }

        [Fact]
        public void CT59_11_UNKNOWN_ROLE_STOPS_EXTRACTION_WITHOUT_PARTIAL_REQUIREMENTS()
        {
            var result = Extract(Plan(
                Piece("valid", "A", HeaderBlockRole.Beam),
                Piece("future", "B", (HeaderBlockRole)999)), RackViewAddress.Whole(DimensionViewKind.Frontal));

            Assert.Equal("UnknownSourceRole", Name(Property(result, "Outcome")));
            Assert.Empty(Items(Property(result, "Requirements")));
        }

        [Fact]
        public void CT59_12_13_14_FINAL_QUERY_REATTACHES_TO_EACH_INSTANCE_AND_IMPORT_STAYS_SEPARATE()
        {
            var extracted = Extract(Plan(
                Piece("a", "SAME", HeaderBlockRole.Beam),
                Piece("b", "same", HeaderBlockRole.Pallet),
                Piece("blank", " ", HeaderBlockRole.Beam)), RackViewAddress.Fondo(1));
            var events = new List<string>();
            var query = QueryProxy((keys, observationType) =>
            {
                events.Add("query:" + string.Join(",", keys));
                return ObservationList(observationType, Observation("SAME", "Ok", "Present"));
            });
            var importer = new RecordingImporter(events);

            var flow = Call(Shared + "LibraryPieceAvailabilityFlowV2", "Observe",
                Property(extracted, "Requirements"), true, query, importer);

            Assert.Equal(new[] { "import:SAME", "query:SAME" }, events);
            var facts = Items(Property(flow, "Facts"));
            Assert.Equal(3, facts.Length);
            Assert.Equal(new[] { "a", "b", "blank" }, facts.Select(f => Text(Property(f, "Requirement"), "PieceId")));
            Assert.Equal(new[] { "Ok", "Ok", "Unknown" }, facts.Select(f => Name(Property(f, "LibraryAvailability"))));
            Assert.Equal(new[] { "Present", "Present", "Unknown" }, facts.Select(f => Name(Property(f, "BlockPresence"))));
        }

        [Theory]
        [InlineData("FileMissing", "Unknown")]
        [InlineData("Unknown", "Unknown")]
        [InlineData("Ok", "BlockMissing")]
        public void CT59_13_AVAILABILITY_AXES_REMAIN_DISTINCT(string availability, string presence)
        {
            var extracted = Extract(Plan(Piece("a", "KEY", HeaderBlockRole.Beam)), RackViewAddress.Fondo(0));
            var query = QueryProxy((_, observationType) =>
                ObservationList(observationType, Observation("KEY", availability, presence)));

            var flow = Call(Shared + "LibraryPieceAvailabilityFlowV2", "Observe",
                Property(extracted, "Requirements"), false, query, new RecordingImporter(new List<string>()));
            var fact = Assert.Single(Items(Property(flow, "Facts")));
            Assert.Equal(availability, Name(Property(fact, "LibraryAvailability")));
            Assert.Equal(presence, Name(Property(fact, "BlockPresence")));
        }

        private static object Input(
            double positionX = 1, double positionY = 2, double positionZ = 3, double rotation = 0,
            double scaleX = 1, double scaleY = 1, double scaleZ = 1,
            double normalX = 0, double normalY = 0, double normalZ = 1,
            double originX = 0, double originY = 0, double originZ = 0)
            => New(Shared + "RackSourcePlacementInput", positionX, positionY, positionZ, rotation,
                scaleX, scaleY, scaleZ, normalX, normalY, normalZ, originX, originY, originZ);

        private static object Tolerance(double scale = 1e-6, double angle = 1e-6, double normal = 1e-6)
            => New(Shared + "RackSourceTransformTolerance", scale, angle, normal);

        private static object Classify(object input, object tolerance)
            => Call(Shared + "RackSourceTransformClassifier", "Classify", input, tolerance);

        private static object Facts(object result)
        {
            Assert.Equal("Facts", Name(Property(result, "Outcome")));
            Assert.True(Boolean(result, "HasFacts"));
            return Property(result, "Facts");
        }

        private static object Extract(HeaderRunPlan plan, RackViewAddress address)
            => Call(Shared + "HeaderPieceRequirementExtractorV2", "Extract", plan, address);

        private static HeaderRunPlan Plan(params HeaderBlockInstance[] pieces)
            => new HeaderRunPlan(Array.Empty<HeaderGroup>(), pieces);

        private static HeaderBlockInstance Piece(string id, string key, HeaderBlockRole role, string view = null)
            => new HeaderBlockInstance { PieceId = id, BlockName = key, Role = role, View = view };

        private static RackViewFrameResult Frame()
            => RackViewFrame.TryCreate(
                RackViewAxisMap.RunHeight,
                RackPhysicalPoint.Zero,
                0,
                10,
                RackFrameEndpointConvention.PhysicalRunAxes,
                RackPhysicalVector.Zero);

        private static void AssertRequirement(
            object requirement, string pieceId, RackViewAddress address, string role, string key, string keyState)
        {
            Assert.Equal(pieceId, Text(requirement, "PieceId"));
            Assert.Equal(address, (RackViewAddress)Property(requirement, "ViewAddress"));
            Assert.Equal(role, Name(Property(requirement, "Role")));
            Assert.Equal(key, Text(requirement, "LibraryKey"));
            Assert.Equal(keyState, Name(Property(requirement, "KeyState")));
        }

        private static object QueryProxy(Func<string[], Type, object> handler)
        {
            var interfaceType = RequireType(Shared + "ILibraryPieceAvailabilityQuery");
            var proxy = DispatchProxy.Create(interfaceType, typeof(AvailabilityQueryProxy));
            ((AvailabilityQueryProxy)proxy).Handler = args =>
            {
                var keys = Items(args[0]).Select(item => Text(item, "Key")).ToArray();
                return handler(keys, RequireType(Shared + "LibraryKeyAvailabilityObservation"));
            };
            return proxy;
        }

        private static object Observation(string key, string availability, string presence)
            => New(Shared + "LibraryKeyAvailabilityObservation", key,
                EnumValue(Shared + "LibraryAvailability", availability),
                EnumValue(Shared + "LibraryBlockPresence", presence));

        private static object ObservationList(Type observationType, params object[] observations)
        {
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(observationType));
            foreach (var observation in observations) list.Add(observation);
            return list;
        }

        private static object New(string typeName, params object[] args)
            => Activator.CreateInstance(RequireType(typeName), args);

        private static object Call(string typeName, string methodName, params object[] args)
        {
            var methods = RequireType(typeName).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name == methodName && method.GetParameters().Length == args.Length)
                .ToArray();
            var method = Assert.Single(methods);
            return method.Invoke(null, args);
        }

        private static Type RequireType(string fullName)
        {
            var type = typeof(RackTransformFacts).Assembly.GetType(fullName, false);
            Assert.NotNull(type);
            return type;
        }

        private static object Property(object target, string name)
        {
            Assert.NotNull(target);
            var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property);
            return property.GetValue(target);
        }

        private static object[] Items(object value) => ((IEnumerable)value).Cast<object>().ToArray();
        private static string Name(object value) => value?.ToString();
        private static string Text(object target, string name) => (string)Property(target, name);
        private static double Number(object target, string name) => (double)Property(target, name);
        private static bool Boolean(object target, string name) => (bool)Property(target, name);
        private static object EnumValue(string typeName, string name) => Enum.Parse(RequireType(typeName), name);

        private static void AssertPoint(object point, double x, double y, double z)
        {
            Assert.Equal(x, Number(point, "X"));
            Assert.Equal(y, Number(point, "Y"));
            Assert.Equal(z, Number(point, "Z"));
        }

        private static void AssertVector(object vector, double x, double y, double z) => AssertPoint(vector, x, y, z);

        public sealed class AvailabilityQueryProxy : DispatchProxy
        {
            public Func<object[], object> Handler { get; set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args) => Handler(args);
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

        private sealed class Marker
        {
        }
    }
}
