using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class SharedViewFoundationF6Tests
    {
        private static readonly string Root = FindRepositoryRoot();

        [Fact]
        public void AUTH12_HEADER_PLAN_EXTRACTS_REAL_KEYS_DEDUPLICATES_AND_PRESERVES_DOTS()
        {
            var plan = HeaderPlan("VIGA.100", "poste-a", "VIGA.100", " ");

            var requirements = RackBlockRequirementExtractors.HeaderRun.Extract(plan);

            Assert.Equal(new[] { "VIGA.100", "poste-a" }, requirements.Select(item => item.Key));
        }

        [Fact]
        public void AUTH12_EMPTY_AND_CANTILEVER_GEOMETRY_PRODUCE_ZERO_REQUIREMENTS()
        {
            Assert.Empty(RackBlockRequirementExtractors.HeaderRun.Extract(HeaderPlan()));
            var cantilever = new CantileverViewPlan(
                CantileverViewKind.Frontal,
                -1,
                Array.Empty<CantileverViewCurve>(),
                Array.Empty<CantileverDiagnostic>());
            Assert.Empty(RackBlockRequirementExtractors.Cantilever.Extract(cantilever));
        }

        [Fact]
        public void AUTH12_TYPED_EXTRACTION_MATCHES_THE_LEGACY_IMPORT_KEY_SET_AND_ORDER()
        {
            var plan = new HeaderRunPlan(
                new[] { new HeaderGroup("Cabecera", new[] { Piece("VIGA.100") }, new[] { new HeaderPlacement(0, false) }) },
                new[] { Piece("POSTE-A"), Piece("viga.100") });
            var legacy = plan.LooseInstances.Select(item => item.BlockName)
                .Concat(plan.Headers.SelectMany(group => group.Instances).Select(item => item.BlockName))
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Assert.Equal(legacy, RackBlockRequirementExtractors.HeaderRun.Extract(plan).Select(item => item.Key));
        }

        [Fact]
        public void AUTH12_REQUIREMENT_KEY_AND_VIEW_BASE_NAME_ARE_INDEPENDENT()
        {
            var prepared = RackViewPreparationPorts.Cabecera<Marker, HeaderRunPlan>(
                (_, __) => HeaderPlan("VIGA.100", "POSTE-A"),
                address => address.Kind == DimensionViewKind.Frontal,
                RackBlockRequirementExtractors.HeaderRun)
                .Prepare(new Marker(), RackViewAddress.Whole(DimensionViewKind.Frontal), Frame(), "Vista Rack");

            Assert.True(prepared.IsSuccess);
            Assert.Equal("Vista Rack", prepared.Prepared.BaseName);
            Assert.Equal(new[] { "VIGA.100", "POSTE-A" },
                prepared.Prepared.BlockRequirements.Select(item => item.Key));
            Assert.DoesNotContain(prepared.Prepared.BlockRequirements, item => item.Key == prepared.Prepared.BaseName);
        }

        [Fact]
        public void AUTH12_COLLISION_NAMING_CANNOT_CHANGE_REQUIREMENTS()
        {
            var requirements = RackBlockRequirementExtractors.HeaderRun.Extract(HeaderPlan("VIGA.100", "POSTE-A"));
            var before = requirements.Select(item => item.Key).ToArray();

            Assert.Equal("Vista Rack_1", UniqueGeneratedName("Vista Rack", new HashSet<string> { "Vista Rack" }));
            Assert.Equal(before, requirements.Select(item => item.Key));
        }

        [Fact]
        public void AUTH12_IMPORT_ALLOWED_ENSURES_BEFORE_FINAL_QUERY_AND_MISSING_IS_NOT_STICKY()
        {
            var events = new List<string>();
            var available = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var query = new FakeQuery(available, events);
            var importer = new FakeImporter(available, events, imports: true);
            var requirements = RackBlockRequirementExtractors.HeaderRun.Extract(HeaderPlan("VIGA.100"));

            var result = LibraryBlockAvailabilityFlow.Observe(requirements, true, query, importer);

            Assert.Equal(new[] { "import:VIGA.100", "query:VIGA.100" }, events);
            Assert.Equal(LibraryBlockAvailability.Found, Assert.Single(result.Facts).Availability);
        }

        [Fact]
        public void AUTH12_IMPORT_FORBIDDEN_USES_QUERY_AS_FINAL_WITHOUT_SIDE_EFFECT()
        {
            var events = new List<string>();
            var available = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var importer = new FakeImporter(available, events, imports: true);

            var result = LibraryBlockAvailabilityFlow.Observe(
                RackBlockRequirementExtractors.HeaderRun.Extract(HeaderPlan("VIGA.100")),
                false,
                new FakeQuery(available, events),
                importer);

            Assert.Equal(new[] { "query:VIGA.100" }, events);
            Assert.False(importer.WasCalled);
            Assert.Equal(LibraryBlockAvailability.Missing, Assert.Single(result.Facts).Availability);
        }

        [Fact]
        public void AUTH12_FAILED_IMPORT_IS_MISSING_AFTER_FINAL_QUERY_WITHOUT_FALSE_SUCCESS()
        {
            var events = new List<string>();
            var available = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var result = LibraryBlockAvailabilityFlow.Observe(
                RackBlockRequirementExtractors.HeaderRun.Extract(HeaderPlan("VIGA.100")),
                true,
                new FakeQuery(available, events),
                new FakeImporter(available, events, imports: false));

            Assert.Equal(new[] { "import:VIGA.100", "query:VIGA.100" }, events);
            Assert.Equal(LibraryBlockAvailability.Missing, Assert.Single(result.Facts).Availability);
        }

        [Fact]
        public void AUTH12_QUERY_PORT_IS_PURE_AND_PLUGIN_IMPORTER_STAYS_SEPARATE()
        {
            var query = Source("src/RackCad.Plugin/Drawing/AutoCadLibraryBlockQuery.cs");
            Assert.Contains("blockTable.Has(requirement.Key)", query);
            Assert.DoesNotContain("BlockLibraryImporter", query);
            Assert.DoesNotContain("WblockCloneObjects", query);
            Assert.DoesNotContain("SanitizeBlockName", query);
            Assert.DoesNotContain("ReadDwgFile", query);

            var importer = Source("src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs");
            Assert.Contains("WblockCloneObjects", importer);
            Assert.Contains("EnsureRequirements", importer);
        }

        [Fact]
        public void AUTH12_ZERO_REQUIREMENTS_DO_NOT_QUERY_OR_IMPORT_BASE_NAME()
        {
            var events = new List<string>();
            var available = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = LibraryBlockAvailabilityFlow.Observe(
                Array.Empty<LibraryBlockRequirement>(),
                true,
                new FakeQuery(available, events),
                new FakeImporter(available, events, imports: true));

            Assert.Empty(result.Facts);
            Assert.Empty(events);
        }

        [Theory]
        [InlineData("dynamic")]
        [InlineData("pushback")]
        [InlineData("cantilever")]
        [InlineData("cabecera")]
        [InlineData("cama")]
        public void AUTH13_UNSUPPORTED_KIND_COMPARATORS_FAIL_CLOSED_AS_UNREADABLE(string kind)
        {
            var result = UnsupportedComparator(kind).Compare(new Marker());
            Assert.Equal(RackAuthoredComparisonOutcome.Unreadable, result.Outcome);
            Assert.Null(result.Authored);
        }

        [Fact]
        public void AUTH13_SELECTIVE_REUSES_AUTHORED_AUTHORITY_FOR_SINGLE_DIVERGENT_AND_UNREADABLE()
        {
            var comparator = RackAuthoredComparatorPorts.Selective();
            var a = new SelectivePalletDesignDocument { Id = "rack", Name = "A", PostId = "P" };
            var same = new SelectivePalletDesignDocument { Id = "rack", Name = "A", PostId = "P" };
            var different = new SelectivePalletDesignDocument { Id = "rack", Name = "B", PostId = "P" };

            var single = comparator.Compare(new SelectiveAuthoredComparisonInput("rack", new[]
            {
                ProjectVariableScanEntry.Selective("D1", "rack", a),
                ProjectVariableScanEntry.Selective("D2", "rack", same),
            }));
            var divergent = comparator.Compare(new SelectiveAuthoredComparisonInput("rack", new[]
            {
                ProjectVariableScanEntry.Selective("D1", "rack", a),
                ProjectVariableScanEntry.Selective("D2", "rack", different),
            }));
            var unreadable = comparator.Compare(new SelectiveAuthoredComparisonInput("rack", new[]
            {
                ProjectVariableScanEntry.Selective("D1", "rack", a),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", "rack"),
            }));

            Assert.Equal(RackAuthoredComparisonOutcome.Single, single.Outcome);
            Assert.Same(a, single.Authored);
            Assert.Equal(RackAuthoredComparisonOutcome.Divergent, divergent.Outcome);
            Assert.Null(divergent.Authored);
            Assert.Equal(RackAuthoredComparisonOutcome.Unreadable, unreadable.Outcome);
            Assert.Null(unreadable.Authored);
        }

        [Fact]
        public void AUTH13_SHARED_COMPARATOR_HAS_NO_JSON_OR_SIBLING_SELECTION_FALLBACK()
        {
            var source = Source("src/RackCad.Application/Systems/Shared/RackAuthoredComparator.cs");
            Assert.DoesNotContain("Json", source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("First(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Majority", source, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("SelectiveAuthoredAuthority.Resolve", source);
        }

        [Fact]
        public void AUTH12_13_PUBLIC_CONTRACTS_HAVE_NO_AUTOCAD_OBJECT_OR_REFLECTION_ESCAPE()
        {
            var contracts = typeof(LibraryBlockRequirement).Assembly.GetExportedTypes()
                .Where(type => type.Namespace == "RackCad.Application.Systems.Shared")
                .Where(type => type.Name.Contains("LibraryBlock", StringComparison.Ordinal)
                    || type.Name.Contains("RackAuthored", StringComparison.Ordinal))
                .ToArray();
            Assert.NotEmpty(contracts);
            Assert.DoesNotContain(contracts.SelectMany(type => type.GetProperties()),
                property => property.PropertyType == typeof(object));
            Assert.DoesNotContain(contracts.SelectMany(type => type.GetMethods()),
                method => method.Name.Contains("Json", StringComparison.OrdinalIgnoreCase)
                    || method.Name.Contains("Reflect", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(contracts.SelectMany(type => type.GetProperties().Select(property => property.PropertyType)),
                TypeGraphContainsAutoCad);
        }

        private static HeaderRunPlan HeaderPlan(params string[] keys)
        {
            var instances = keys.Select(Piece).ToArray();
            return new HeaderRunPlan(Array.Empty<HeaderGroup>(), instances);
        }

        private static HeaderBlockInstance Piece(string key) => new HeaderBlockInstance
        {
            Role = HeaderBlockRole.Beam,
            PieceId = key,
            BlockName = key,
            View = "lateral",
            Insertion = new Point2D(0, 0),
            ConnectionAnchor = new Point2D(0, 0),
        };

        private static IRackAuthoredComparatorPort<Marker, Marker> UnsupportedComparator(string kind)
        {
            switch (kind)
            {
                case "dynamic": return RackAuthoredComparatorPorts.Dynamic<Marker, Marker>();
                case "pushback": return RackAuthoredComparatorPorts.PushBack<Marker, Marker>();
                case "cantilever": return RackAuthoredComparatorPorts.Cantilever<Marker, Marker>();
                case "cabecera": return RackAuthoredComparatorPorts.Cabecera<Marker, Marker>();
                case "cama": return RackAuthoredComparatorPorts.Cama<Marker, Marker>();
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

        private static string UniqueGeneratedName(string baseName, ISet<string> names)
        {
            var sanitized = BlockNaming.SanitizeBlockName(baseName);
            if (!names.Contains(sanitized)) return sanitized;
            for (var suffix = 1; ; suffix++)
            {
                var candidate = sanitized + "_" + suffix;
                if (!names.Contains(candidate)) return candidate;
            }
        }

        private static string Source(string relativePath)
            => File.ReadAllText(Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("RackCad.sln was not found.");
        }

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

        private sealed class Marker { }

        private sealed class FakeQuery : ILibraryBlockQuery
        {
            private readonly ISet<string> available;
            private readonly ICollection<string> events;

            internal FakeQuery(ISet<string> available, ICollection<string> events)
            {
                this.available = available;
                this.events = events;
            }

            public IReadOnlyList<LibraryBlockAvailabilityFact> Query(IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                foreach (var requirement in requirements) events.Add("query:" + requirement.Key);
                return requirements.Select(requirement => new LibraryBlockAvailabilityFact(
                    requirement,
                    available.Contains(requirement.Key) ? LibraryBlockAvailability.Found : LibraryBlockAvailability.Missing)).ToArray();
            }
        }

        private sealed class FakeImporter : ILibraryBlockImporter
        {
            private readonly ISet<string> available;
            private readonly ICollection<string> events;
            private readonly bool imports;

            internal FakeImporter(ISet<string> available, ICollection<string> events, bool imports)
            {
                this.available = available;
                this.events = events;
                this.imports = imports;
            }

            internal bool WasCalled { get; private set; }

            public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                WasCalled = true;
                foreach (var requirement in requirements)
                {
                    events.Add("import:" + requirement.Key);
                    if (imports) available.Add(requirement.Key);
                }

                return new LibraryBlockImportResult(true, imports ? requirements.Count : 0);
            }
        }
    }
}
