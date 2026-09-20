using System;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    public sealed class SharedViewFoundationF5Tests
    {
        [Theory]
        [InlineData("selective")]
        [InlineData("dynamic")]
        [InlineData("pushback")]
        [InlineData("cantilever")]
        [InlineData("cabecera")]
        [InlineData("cama")]
        public void AUTH09_KIND_PORT_DELEGATES_TO_THE_EXISTING_RESOLVER_EXACTLY_ONCE(string kind)
        {
            var calls = 0;
            var expected = new Marker("resolved");
            var port = Port(kind, input =>
            {
                calls++;
                Assert.Equal("authored", input.Value);
                return expected;
            });

            var result = port.Resolve(new Marker("authored"));

            Assert.True(result.IsSuccess);
            Assert.Equal(kind, result.Kind);
            Assert.Same(expected, result.Resolved);
            Assert.Equal(1, calls);
        }

        [Fact]
        public void AUTH09_FAILURES_ARE_TYPED_AND_DIAGNOSTICS_ARE_PRESERVED()
        {
            var invalid = RackResolvePorts.Dynamic<Marker, Marker>(input => input).Resolve(null);
            Assert.Equal(RackResolveFailure.UnreadableInput, invalid.Failure);

            var blocked = RackResolvePorts.Selective<Marker, Marker>(
                input => new Marker("effective"),
                resolved => "variable faltante").Resolve(new Marker("authored"));
            Assert.Equal(RackResolveFailure.Blocked, blocked.Failure);
            Assert.Equal("variable faltante", blocked.Diagnostic);

            var failed = RackResolvePorts.PushBack<Marker, Marker>(
                input => throw new InvalidOperationException("resolver fallo")).Resolve(new Marker("authored"));
            Assert.Equal(RackResolveFailure.ResolutionFailure, failed.Failure);
            Assert.Equal("resolver fallo", failed.Diagnostic);

            var unsupported = RackResolveResult<Marker>.Unsupported("future", "kind no soportado");
            Assert.Equal(RackResolveFailure.UnsupportedKind, unsupported.Failure);
        }

        [Fact]
        public void AUTH09_SELECTIVE_ADAPTER_HAS_ONE_EFFECTIVE_RESOLVE_AND_NO_FALLBACK()
        {
            var calls = 0;
            var port = RackResolvePorts.Selective<Marker, Marker>(input =>
            {
                calls++;
                throw new InvalidOperationException("referencia rota");
            });

            var result = port.Resolve(new Marker("authored"));

            Assert.Equal(RackResolveFailure.ResolutionFailure, result.Failure);
            Assert.Equal(1, calls);
        }

        [Theory]
        [InlineData("selective")]
        [InlineData("dynamic")]
        [InlineData("pushback")]
        [InlineData("cantilever")]
        [InlineData("cabecera")]
        [InlineData("cama")]
        public void AUTH10_KIND_ADAPTER_CALLS_THE_REAL_BUILDER_ONCE_AND_PRESERVES_THE_TYPED_PAYLOAD(string kind)
        {
            var calls = 0;
            var expected = new HeaderRunPlan(Array.Empty<HeaderGroup>(), Array.Empty<HeaderBlockInstance>());
            var port = Preparation(kind, (resolved, address) =>
            {
                calls++;
                Assert.Equal("resolved", resolved.Value);
                Assert.Equal(DimensionViewKind.Frontal, address.Kind);
                return expected;
            });
            var address = RackViewAddress.Whole(DimensionViewKind.Frontal);
            var frame = Frame();

            var result = port.Prepare(new Marker("resolved"), address, frame, "Vista Rack");

            Assert.True(result.IsSuccess);
            Assert.Equal(1, calls);
            Assert.Equal(kind, result.Prepared.Kind);
            Assert.Equal(address, result.Prepared.Address);
            Assert.Equal(frame.Frame.Center, result.Prepared.Frame.Center);
            Assert.Equal("Vista Rack", result.Prepared.BaseName);
            Assert.Same(expected, result.Prepared.Payload);
        }

        [Fact]
        public void AUTH10_UNSUPPORTED_FRAME_AND_BUILDER_FAILURE_FAIL_CLOSED_WITHOUT_SECOND_CALL()
        {
            var calls = 0;
            var port = RackViewPreparationPorts.Dynamic<Marker, HeaderRunPlan>(
                (resolved, address) =>
                {
                    calls++;
                    throw new InvalidOperationException("builder fallo");
                },
                address => address.Kind == DimensionViewKind.Frontal);

            var unsupported = port.Prepare(
                new Marker("resolved"),
                RackViewAddress.Whole(DimensionViewKind.Planta),
                Frame(),
                "Vista");
            Assert.Equal(RackViewPreparationFailure.UnsupportedAddress, unsupported.Failure);
            Assert.Equal(0, calls);

            var invalidFrame = port.Prepare(
                new Marker("resolved"),
                RackViewAddress.Whole(DimensionViewKind.Frontal),
                RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry),
                "Vista");
            Assert.Equal(RackViewPreparationFailure.FrameUnavailable, invalidFrame.Failure);
            Assert.Equal(0, calls);

            var failed = port.Prepare(
                new Marker("resolved"),
                RackViewAddress.Whole(DimensionViewKind.Frontal),
                Frame(),
                "Vista");
            Assert.Equal(RackViewPreparationFailure.BuilderFailure, failed.Failure);
            Assert.Equal("builder fallo", failed.Diagnostic);
            Assert.Equal(1, calls);
        }

        [Fact]
        public void AUTH10_PAYLOAD_CONTRACT_IS_GENERIC_AND_HAS_NO_OBJECT_JSON_OR_REQUIREMENTS_PLACEHOLDER()
        {
            var prepared = typeof(RackPreparedView<>);
            Assert.True(prepared.IsGenericTypeDefinition);
            Assert.DoesNotContain(prepared.GetProperties(), property => property.PropertyType == typeof(object));
            Assert.DoesNotContain(prepared.GetProperties(), property =>
                property.Name.Contains("Json", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("Requirement", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void AUTH11_BASE_NAMES_MATCH_THE_LEGACY_STRINGS_FOR_ALL_KINDS()
        {
            var selective = new SelectiveRackSystem { Height = 120 };
            selective.Bays.Add(new SelectiveBay());
            var dynamic = new DynamicRackSystem { PalletsDeep = 8 };
            var pushBack = new PushBackSystem();
            pushBack.Structure.Fronts.Add(new DynamicRackFront());
            var bed = new FlowBedConfiguration { BedType = FlowBedType.Dynamic, LaneDepth = 48 };

            Assert.Equal("Rack A", RackViewBaseName.SelectiveFrontal(selective, " Rack:A "));
            Assert.Equal("Selectivo planta - 1 frentes", RackViewBaseName.SelectivePlanta(selective, null));
            Assert.Equal("Sistema dinamico - 8 fondos - L0", RackViewBaseName.DynamicLateral(dynamic, null));
            Assert.Equal("Rack - frontal entrada-salida", RackViewBaseName.PushBackFrontal(
                pushBack, "Rack", RackPushBackEnd.EntradaSalida, RackPushBackSide.A));
            Assert.Equal("Cabecera planta", RackViewBaseName.CabeceraPlanta(null));
            Assert.Equal("Cama dinamica - fondo 48", RackViewBaseName.Cama(bed, null));
            Assert.Equal(
                "RACKCAD_CANTILEVER_Rack_-_frontal_FRONTAL",
                RackViewBaseName.CantileverGenerated(
                    CantileverViewKind.Frontal, -1, "Rack - frontal"));
        }

        [Fact]
        public void AUTH11_HAS_NO_COLLISION_OR_LIBRARY_REQUIREMENT_AUTHORITY()
        {
            var methods = typeof(RackViewBaseName).GetMethods()
                .Where(method => method.DeclaringType == typeof(RackViewBaseName))
                .Select(method => method.Name)
                .ToArray();
            Assert.DoesNotContain(methods, name => name.Contains("Unique", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(methods, name => name.Contains("Collision", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(methods, name => name.Contains("Requirement", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(methods, name => name.Contains("Library", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void AUTH11_LINKED_VIEW_NAMES_MATCH_LEGACY_SUFFIXES_AND_PRESERVE_ABSENCE()
        {
            Assert.Equal("Rack A", RackViewBaseName.LinkedBase(" Rack:A "));
            Assert.Equal("Rack - planta", RackViewBaseName.LinkedPlanta("Rack"));
            Assert.Equal("Rack - lateral 2", RackViewBaseName.LinkedLateral("Rack", 1));
            Assert.Equal("Rack - frontal salida", RackViewBaseName.LinkedFlowEnd("Rack", RackFlowEnd.Exit));
            Assert.Equal(
                "Rack - frontal posterior B",
                RackViewBaseName.LinkedPushBackCut(
                    "Rack",
                    RackPushBackEnd.Posterior,
                    RackPushBackSide.B,
                    includeSide: true));
            Assert.Equal(
                "Rack - frente F2",
                RackViewBaseName.LinkedSelectiveFrontal("Rack", fondo: 1, fondoCount: 2));
            Assert.Null(RackViewBaseName.LinkedBase(null));
            Assert.Null(RackViewBaseName.LinkedPlanta(" "));
            Assert.Null(RackViewBaseName.LinkedLateral(null, 0));
            Assert.Null(RackViewBaseName.LinkedFlowEnd(null, RackFlowEnd.Entrance));
            Assert.Null(RackViewBaseName.LinkedPushBackCut(null, RackPushBackEnd.Posterior, RackPushBackSide.A, true));
            Assert.Null(RackViewBaseName.LinkedSelectiveFrontal(null, 0, 2));
        }

        [Fact]
        public void AUTH09_11_PUBLIC_CONTRACTS_HAVE_NO_AUTOCAD_TYPES()
        {
            var contractTypes = typeof(RackResolveResult<>).Assembly.GetExportedTypes()
                .Where(type => type.Namespace == "RackCad.Application.Systems.Shared")
                .Where(type => type.Name.StartsWith("RackResolve", StringComparison.Ordinal)
                    || type.Name.StartsWith("IRackResolve", StringComparison.Ordinal)
                    || type.Name.StartsWith("RackPrepared", StringComparison.Ordinal)
                    || type.Name.StartsWith("RackViewPreparation", StringComparison.Ordinal)
                    || type.Name.StartsWith("IRackViewPreparation", StringComparison.Ordinal)
                    || type.Name == nameof(RackViewBaseName))
                .ToArray();

            Assert.NotEmpty(contractTypes);
            var exposedTypes = contractTypes
                .SelectMany(type => type.GetProperties().Select(property => property.PropertyType)
                    .Concat(type.GetMethods().Select(method => method.ReturnType))
                    .Concat(type.GetMethods().SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType))))
                .ToArray();
            Assert.DoesNotContain(exposedTypes, TypeGraphContainsAutoCad);
        }

        private static IRackResolvePort<Marker, Marker> Port(string kind, Func<Marker, Marker> resolve)
        {
            switch (kind)
            {
                case "selective": return RackResolvePorts.Selective(resolve);
                case "dynamic": return RackResolvePorts.Dynamic(resolve);
                case "pushback": return RackResolvePorts.PushBack(resolve);
                case "cantilever": return RackResolvePorts.Cantilever(resolve);
                case "cabecera": return RackResolvePorts.Cabecera(resolve);
                case "cama": return RackResolvePorts.Cama(resolve);
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static IRackViewPreparationPort<Marker, HeaderRunPlan> Preparation(
            string kind,
            Func<Marker, RackViewAddress, HeaderRunPlan> builder)
        {
            bool Supports(RackViewAddress address) => address.Kind == DimensionViewKind.Frontal;
            switch (kind)
            {
                case "selective": return RackViewPreparationPorts.Selective(builder, Supports);
                case "dynamic": return RackViewPreparationPorts.Dynamic(builder, Supports);
                case "pushback": return RackViewPreparationPorts.PushBack(builder, Supports);
                case "cantilever": return RackViewPreparationPorts.Cantilever(builder, Supports);
                case "cabecera": return RackViewPreparationPorts.Cabecera(builder, Supports);
                case "cama": return RackViewPreparationPorts.Cama(builder, Supports);
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static RackViewFrameResult Frame()
            => RackViewFrame.TryCreate(
                RackViewAxisMap.RunHeight,
                RackPhysicalPoint.Zero,
                0,
                10,
                RackFrameEndpointConvention.PhysicalRunAxes,
                RackPhysicalVector.Zero);

        private static bool TypeGraphContainsAutoCad(Type type)
        {
            if (type.FullName != null && type.FullName.StartsWith("Autodesk.AutoCAD.", StringComparison.Ordinal))
            {
                return true;
            }

            if (type.IsArray || type.IsByRef || type.IsPointer)
            {
                return TypeGraphContainsAutoCad(type.GetElementType());
            }

            return type.IsGenericType && type.GetGenericArguments().Any(TypeGraphContainsAutoCad);
        }

        private sealed class Marker
        {
            internal Marker(string value) => Value = value;
            internal string Value { get; }
        }
    }
}
