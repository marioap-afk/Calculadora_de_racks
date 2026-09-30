using System;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.RackProductPrepareTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-60 amendment A-1 (docs/automation/decisions/I-60.md §5): after I-55, the RACKCAD menu, the library «como nuevo», the first-view
    /// freedom (ID17) and the multi-view queue (ID18) create a NEW rack through the batch factories of <c>RackViewBatchProducts</c>, whose
    /// session holds ONE <c>NewRackCreationContext</c> for every initial view. The name is resolved once, at the menu case, before the factory;
    /// the factories, the session, RACKEDITAR (ExistingRack) and RACKPROYECTAR never name a rack.
    /// </summary>
    public class I60BatchCreationNamingTests
    {
        // ================================================================ A1-1 / A1-2: the menu batch cases

        [Theory]
        [InlineData("case SelectiveInsertionRequest selective:", "RackViewBatchProducts.SelectiveNew(", "RackSystemKind.SelectiveRack", "selective.System.Name = ")]
        [InlineData("case DynamicInsertionRequest dynamic:", "RackViewBatchProducts.Dynamic(", "RackSystemKind.PalletFlow", "dynamic.System.Name = ")]
        [InlineData("case PushBackInsertionRequest pushBack:", "RackViewBatchProducts.PushBack(", "RackSystemKind.PushBack", "pushBack.System.Name = ")]
        [InlineData("case CantileverInsertionRequest cantilever:", "RackViewBatchProducts.Cantilever(", "RackSystemKind.Cantilever", "cantilever.Design.Name = ")]
        [InlineData("case HeaderInsertionRequest header:", "RackViewBatchProducts.Header(", "RackSystemKind.Selective", "header.Configuration.Name = ")]
        public void I60_A1_EachMenuBatchCaseResolvesOneNameBeforeItsFactory(string caseLabel, string factory, string kind, string copy)
        {
            var body = Case(caseLabel);

            var resolve = body.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal);
            var call = body.IndexOf(factory, StringComparison.Ordinal);
            Assert.True(resolve >= 0, caseLabel + " must resolve the name of the new rack");
            Assert.True(call > resolve, caseLabel + ": resolve before " + factory);
            Assert.Contains(kind, body.Substring(resolve, call - resolve));
            Assert.Equal(1, Count(body, "RackNewRackName.Resolve("));

            // The persisted/runtime copy the factory reads is set before the factory is called.
            var copyAt = body.IndexOf(copy, StringComparison.Ordinal);
            Assert.True(copyAt >= 0 && copyAt < call, caseLabel + ": " + copy + " before " + factory);
        }

        [Theory]
        [InlineData("case SelectiveInsertionRequest selective:", "selective.RackName")]
        [InlineData("case DynamicInsertionRequest dynamic:", "dynamic.RackName")]
        [InlineData("case PushBackInsertionRequest pushBack:", "pushBack.RackName")]
        [InlineData("case CantileverInsertionRequest cantilever:", "cantilever.RackName")]
        public void I60_A1_TheFactoryReceivesTheResolvedNameNotTheRequestedOne(string caseLabel, string requested)
        {
            var body = Case(caseLabel);
            var call = body.Substring(body.IndexOf("RackViewBatchProducts.", StringComparison.Ordinal));
            call = call.Substring(0, call.IndexOf(';'));

            Assert.DoesNotContain(requested, call);
        }

        [Fact]
        public void I60_A1_TheHeaderFactoryReadsTheResolvedConfigurationName()
        {
            var body = Case("case HeaderInsertionRequest header:");
            var assign = body.IndexOf("header.Configuration.Name = ", StringComparison.Ordinal);
            var call = body.IndexOf("RackViewBatchProducts.Header(", StringComparison.Ordinal);

            Assert.True(assign >= 0 && assign < call);
            Assert.Contains("RackNewRackName.Resolve(", body.Substring(assign, call - assign));
        }

        [Fact]
        public void I60_A1_TheCantileverNameIsResolvedAfterTheGeometryGateAndBeforeItsFactory()
        {
            var body = Case("case CantileverInsertionRequest cantilever:");
            var gate = body.IndexOf("TryGeometryFactory(", StringComparison.Ordinal);
            var resolve = body.IndexOf("RackNewRackName.Resolve(", StringComparison.Ordinal);
            var call = body.IndexOf("RackViewBatchProducts.Cantilever(", StringComparison.Ordinal);

            Assert.True(gate >= 0 && gate < resolve && resolve < call);
        }

        [Fact]
        public void I60_A1_TheHeaderRackNameArgumentIsTheAssignedConfigurationName()
        {
            var body = Case("case HeaderInsertionRequest header:");
            var call = body.Substring(body.IndexOf("RackViewBatchProducts.Header(", StringComparison.Ordinal));
            call = call.Substring(0, call.IndexOf(';'));

            // The block names (HeaderPlan: Configuration.Name; HeaderName: rackName) must come from the same assigned value.
            Assert.Matches(@"header\.RackId,\s*(header\.Configuration\?\.Name|header\.Configuration\.Name|\w+Name)\s*,", call);
        }

        // ================================================================ A1-3: never an existing rack, never a projection

        [Theory]
        [InlineData("src/RackCad.Plugin/Views/RackViewBatchProducts.cs")]
        [InlineData("src/RackCad.Plugin/Views/RackViewBatchProductSession.cs")]
        [InlineData("src/RackCad.Plugin/Views/RackViewBatchExecution.cs")]
        [InlineData("src/RackCad.Plugin/Views/RackViewBatchDriver.cs")]
        [InlineData("src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs")]
        [InlineData("src/RackCad.Plugin/RackSelectivoInsertIntegration.cs")]
        [InlineData("src/RackCad.Plugin/RackProyectarCommands.cs")]
        public void I60_A1_NoExistingRackOrProjectionPathNamesARack(string file)
        {
            Assert.DoesNotContain("RackNewRackName", Code(file));
        }

        [Fact]
        public void I60_A1_NoProjectionFileNamesARack()
        {
            var projection = Directory.GetFiles(Path.Combine(Root(), "src", "RackCad.Plugin", "Views"), "RackProjection*.cs");
            Assert.NotEmpty(projection);
            Assert.All(projection, file => Assert.DoesNotContain("RackNewRackName", File.ReadAllText(file)));
        }

        [Fact]
        public void I60_A1_RackEditarNeverNamesARack()
        {
            var body = Body(Code("src/RackCad.Plugin/RackMenuCommands.cs"), "public void RackEditar(");

            Assert.DoesNotContain("RackNewRackName", body);
        }

        // ================================================================ A1-4 / A1-5: one name for every initial view; legacy stays unnamed

        [Fact]
        public void I60_A1_OneNewRackGetsOneNameSharedByEveryInitialView()
        {
            var name = RackLogicalNameAllocator.ForNewRack(RackSystemKind.SelectiveRack, "  ", new[] { "Selectivo 1", "Selectivo 2", "Pasillo A" });
            Assert.Equal("Selectivo 3", name);

            var ids = new CountingIdFactory();
            var context = new NewRackCreationContext<Authored>(new Authored(), name, "{}", ids);
            var preparer = Preparer(RackSystemKind.SelectiveRack, Success(RackSystemKind.SelectiveRack));
            var facts = new SelectiveViewAvailabilityFacts(1, new[] { 0 });

            var views = new[]
            {
                (RackViewAddress.Fondo(0), RackViewProductOperation.CreateFirst),
                (RackViewAddress.Post(0), RackViewProductOperation.Batch),
                (RackViewAddress.Whole(DimensionViewKind.Planta), RackViewProductOperation.Batch),
            };
            var products = views.Select(view =>
            {
                var accepted = Accepted(NewIntent(RackSystemKind.SelectiveRack, view.Item1, facts, ids, view.Item2, context));
                var result = preparer.Prepare(accepted);
                Assert.True(result.IsSuccess, view.Item1 + ": " + result.CauseCode + " " + result.Diagnostic);
                return result.Product;
            }).ToList();

            Assert.All(products, product => Assert.Equal("Selectivo 3", product.Envelope.Name));
            Assert.Single(products.Select(product => product.RackId).Distinct());
            Assert.Equal(1, ids.Calls);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void I60_A1_AnExistingUnnamedRackInsertedInABatchStaysUnnamed(string name)
        {
            var syntax = RackViewCodec.Encode(RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta));
            var source = RackEmbedComposer.Compose(null, syntax.Kind, "rack-legacy", name, syntax.View, syntax.Section, "{\"design\":1}");
            source.Name = name;
            var preparer = Preparer(RackSystemKind.SelectiveRack, Success(RackSystemKind.SelectiveRack));
            var request = new RackProductViewRequest(
                RackViewProductOperation.Batch, RackSystemKind.SelectiveRack, RackViewAddress.Post(0),
                new SelectiveViewAvailabilityFacts(1, new[] { 0 }));
            var accepted = RackProductIntentAcceptance.Accept(
                new ExistingRackViewIntent<object>(new ExistingRackContext<object>(source, new object()), request));
            Assert.True(accepted.IsAccepted, accepted.CauseCode);

            var result = preparer.PrepareExisting(accepted.Intent, new Comparator(source.Kind));

            Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
            Assert.True(string.IsNullOrWhiteSpace(result.Product.Envelope.Name), "a legacy unnamed rack must never be auto-named");
            Assert.Equal("rack-legacy", result.Product.RackId);
        }

        // ================================================================ helpers

        private static CountingResolve<Authored, Resolved> Success(RackSystemKind systemKind)
            => new CountingResolve<Authored, Resolved>(Kind(systemKind),
                _ => RackResolveResult<Resolved>.Success(Kind(systemKind), new Resolved()));

        private sealed class Comparator : IRackAuthoredComparatorPort<object, Authored>
        {
            internal Comparator(string kind) => Kind = kind;
            public string Kind { get; }
            public RackAuthoredComparisonResult<Authored> Compare(object input)
                => RackAuthoredComparisonResult<Authored>.Single(new Authored());
        }

        private static string Case(string label)
        {
            var menu = Body(Code("src/RackCad.Plugin/RackMenuCommands.cs"), "public void RackCad(");
            var at = menu.IndexOf(label, StringComparison.Ordinal);
            Assert.True(at >= 0, "case not found: " + label);
            var body = menu.Substring(at);
            return body.Substring(0, body.IndexOf("break;", StringComparison.Ordinal));
        }

        private static int Count(string text, string token)
        {
            var count = 0;
            for (var at = text.IndexOf(token, StringComparison.Ordinal); at >= 0; at = text.IndexOf(token, at + 1, StringComparison.Ordinal)) count++;
            return count;
        }

        private static string Root()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir.FullName;
        }

        private static string Code(string relative)
            => string.Join("\n", File.ReadAllLines(Path.Combine(Root(), relative.Replace('/', Path.DirectorySeparatorChar)))
                .Select(line => line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)) : line));

        private static string Body(string source, string signature)
        {
            var at = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(at >= 0, "signature not found: " + signature);
            var open = source.IndexOf('{', at);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(at, i - at + 1);
            }

            return source.Substring(at);
        }
    }
}
