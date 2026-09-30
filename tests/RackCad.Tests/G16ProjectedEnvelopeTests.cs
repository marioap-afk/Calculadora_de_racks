using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 corrective round (OV-ID19-01). RACKPROYECTAR failed to write its first definition with AUTH-15's
    /// <c>InvalidEnvelope: sobre ausente o sin Id/Kind/Name</c>.
    ///
    /// <para>
    /// Which precondition: the envelope is never null and its Id and Kind come from the accepted intent (an intent without an Id or
    /// with another kind is refused before this point), so the only field the projected envelope can lack is <b>Name</b>. The
    /// preparer composes it from <c>source.Name</c> — the client-facing name of the rack — and a rack may legally carry none
    /// (the editors do not require one; the list shows «(sin nombre)»). The block definition BaseName is a DIFFERENT authority
    /// (AUTH-11, with its own descriptive fallback) and was never blank, which is why the definition name never showed the problem.
    /// </para>
    ///
    /// <para>
    /// Resolution (Owner product decision, after C16-05 was revoked): a blank logical Name is a VALID state and RACKPROYECTAR projects it
    /// as it is. AUTH-15 was corrected by I-52-AUTH15-C1 (integrated in main, <c>integration/I-52-AUTH15-C1</c>): it requires Id and
    /// Kind, never Name. No synthetic name is invented anywhere; the projected envelope keeps the rack unnamed.
    /// </para>
    /// </summary>
    public class G16ProjectedEnvelopeTests
    {
        private const string RackA = "731e88d1-a2a0-4b2e-9cab-b96212d2711e";

        private const string CustomProps = "{\"a\":1}";

        public static IEnumerable<object[]> SupportedTargets()
        {
            foreach (var kind in new[]
            {
                RackSystemKind.SelectiveRack, RackSystemKind.PalletFlow, RackSystemKind.PushBack,
                RackSystemKind.Cantilever, RackSystemKind.Selective,
            })
            {
                foreach (var address in G16Targets(kind))
                {
                    yield return new object[] { kind, address };
                }
            }
        }

        private static IEnumerable<RackViewAddress> G16Targets(RackSystemKind kind)
        {
            yield return RackViewAddress.Whole(DimensionViewKind.Planta);
            switch (kind)
            {
                case RackSystemKind.SelectiveRack:
                    yield return RackViewAddress.Fondo(0);
                    yield return RackViewAddress.Post(0);
                    break;
                case RackSystemKind.PalletFlow:
                    yield return RackViewAddress.FlowEnd(RackFlowEnd.Exit);
                    yield return RackViewAddress.FlowEnd(RackFlowEnd.Entrance);
                    yield return RackViewAddress.Post(0);
                    break;
                case RackSystemKind.PushBack:
                    yield return RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A);
                    yield return RackViewAddress.Post(0);
                    break;
                case RackSystemKind.Cantilever:
                    yield return RackViewAddress.Whole(DimensionViewKind.Frontal);
                    yield return RackViewAddress.Station(0);
                    break;
                case RackSystemKind.Selective:
                    yield return RackViewAddress.Whole(DimensionViewKind.Lateral);
                    break;
            }
        }

        // ================================================================ the cause, demonstrated

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void G16_ID19_ABlankNamedSourceProducesAProjectedEnvelopeThatAuth15Accepts(string sourceName)
        {
            var product = Prepare(RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta), sourceName, composeName: false);

            // AUTH-15-C1 (integrated): the precondition is Id and Kind only; a blank logical Name is valid and travels as it is.
            Assert.False(string.IsNullOrWhiteSpace(product.Envelope.Id));
            Assert.False(string.IsNullOrWhiteSpace(product.Envelope.Kind));
            Assert.True(string.IsNullOrWhiteSpace(product.Envelope.Name));
            Assert.True(Auth15Accepts(product.Envelope));
        }

        [Fact]
        public void G16_ID19_TheAuth15PreconditionIsExactlyIdAndKind()
        {
            var creator = string.Join("\n", File.ReadAllLines(Path.Combine(Root(), "src", "RackCad.Plugin", "Systems", "Shared", "RackDefinitionCreator.cs"))
                .Select(line => line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)) : line));

            Assert.Contains("envelope == null", creator);
            Assert.Contains("string.IsNullOrWhiteSpace(envelope.Id)", creator);
            Assert.Contains("string.IsNullOrWhiteSpace(envelope.Kind)", creator);
            // I-52-AUTH15-C1: the logical Name is optional and AUTH-15 never reads it.
            Assert.DoesNotContain("envelope.Name", creator);
        }

        // ================================================================ same rack, same logical name state

        [Fact]
        public void G16_ID19_ABlankNamedRackIsNeverGivenASyntheticName()
        {
            // A legacy unnamed rack stays unnamed in the projected sibling, and AUTH-15-C1 accepts that envelope.
            var product = Prepare(RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta), "  ", composeName: true);

            Assert.True(string.IsNullOrWhiteSpace(product.Envelope.Name));
            Assert.True(Auth15Accepts(product.Envelope));
        }

        [Fact]
        public void G16_ID19_ANamedRackKeepsItsOwnNameTrimmedAndUnchanged()
        {
            var product = Prepare(RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta), "Rack A", composeName: true);

            Assert.Equal("Rack A", product.Envelope.Name);
        }

        [Fact]
        public void G16_ID19_ABlankViewTakesTheNameOfASiblingViewOfTheSameRack()
        {
            Assert.Equal("Rack A", RackProjectionEnvelopeName.LogicalName(null, new[] { "", "  ", "Rack A", "Otro" }));
            Assert.Null(RackProjectionEnvelopeName.LogicalName("", new[] { null, " " }));
            Assert.Equal("Propio", RackProjectionEnvelopeName.LogicalName(" Propio ", new[] { "Rack A" }));
        }

        [Fact]
        public void G16_ID19_TheSourceEnvelopeIsNeverRepairedInPlace()
        {
            var source = Source(RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta), "");

            var copy = RackProjectionEnvelopeName.WithLogicalName(source, new[] { "", "Rack A" });

            Assert.NotSame(source, copy);
            Assert.Equal(string.Empty, source.Name);
            Assert.Equal("Rack A", copy.Name);
            Assert.Equal(source.Id, copy.Id);
            Assert.Equal(source.Design, copy.Design);
        }

        [Fact]
        public void G16_ID19_ANamedSourceIsNotCopiedNeedlessly()
        {
            var source = Source(RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta), "Rack A");

            Assert.Same(source, RackProjectionEnvelopeName.WithLogicalName(source, new[] { "Rack A" }));
        }

        // ================================================================ every system, every supported target class

        [Theory]
        [MemberData(nameof(SupportedTargets))]
        public void G16_ID19_EveryProjectedEnvelopeSatisfiesAuth15ForABlankNamedRackWithoutInventingAName(RackSystemKind kind, RackViewAddress target)
        {
            // Legacy unnamed racks are projectable: same RackId, kind, view, design and custom properties; the name stays blank.
            var product = Prepare(kind, target, "", composeName: true);

            AssertProjected(product.Envelope, kind, target);
            Assert.True(string.IsNullOrWhiteSpace(product.Envelope.Name));
        }

        [Theory]
        [MemberData(nameof(SupportedTargets))]
        public void G16_ID19_EveryProjectedEnvelopeSatisfiesAuth15ForANamedRack(RackSystemKind kind, RackViewAddress target)
        {
            var product = Prepare(kind, target, "Rack A", composeName: true);

            AssertProjected(product.Envelope, kind, target);
            Assert.Equal("Rack A", product.Envelope.Name);
        }

        // ================================================================ session wiring (Plugin source)

        [Fact]
        public void G16_ID19_TheProjectionSessionComposesFromTheLogicalNameAndNamesBlocksFromTheRawName()
        {
            var sessions = Code("src/RackCad.Plugin/Views/RackProjectionKindSessions.cs");

            Assert.Contains("RackProjectionEnvelopeName.WithLogicalName(", sessions);
            // BaseName and the plan grouping keep reading the source name AS IS: they are another authority with its own fallback.
            Assert.Matches(new System.Text.RegularExpressions.Regex(@"baseName\(system, address, rawSourceName\)"), sessions);
            Assert.DoesNotContain("baseName(system, address, source.Name)", sessions);
        }

        [Fact]
        public void G16_ID19_AFailedDefinitionReportsTheEnvelopeFieldsItWasGiven()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            foreach (var field in new[] { "view.Envelope.Id", "view.Envelope.Kind", "view.Envelope.Name", "view.Envelope.View", "view.Envelope.Section" })
            {
                Assert.Contains(field, scope);
            }
        }

        [Fact]
        public void G16_ID19_Auth15AndTheGlobalEnvelopeCompositionAreUntouched()
        {
            var composition = Code("src/RackCad.Application/Views/Preparation/RackViewEnvelopeComposition.cs");

            Assert.Contains("RackEmbedComposer.Compose(", composition);
            Assert.DoesNotContain("IsNullOrWhiteSpace(rackName)", composition);
        }

        // ================================================================ helpers

        private static void AssertProjected(RackEmbedDocument envelope, RackSystemKind kind, RackViewAddress target)
        {
            var syntax = RackViewCodec.Encode(kind, target);

            Assert.True(Auth15Accepts(envelope));
            Assert.Equal(RackA, envelope.Id);
            Assert.Equal(syntax.Kind, envelope.Kind);
            Assert.Equal(syntax.View, envelope.View);
            Assert.Equal(syntax.Section, envelope.Section);
            Assert.Equal("{\"design\":1}", envelope.Design);
            Assert.Equal(CustomProps, envelope.CustomProperties?.GetRawText());
            Assert.True(RackViewCodec.Decode(envelope.Kind, envelope.View, envelope.Section).HasAddress);
        }

        /// <summary>The integrated AUTH-15-C1 precondition (pinned to its source above): envelope, Id and Kind; never Name.</summary>
        private static bool Auth15Accepts(RackEmbedDocument envelope)
            => envelope != null
                && !string.IsNullOrWhiteSpace(envelope.Id)
                && !string.IsNullOrWhiteSpace(envelope.Kind);

        private static RackEmbedDocument Source(RackSystemKind kind, RackViewAddress address, string name)
        {
            var syntax = RackViewCodec.Encode(kind, address);
            var source = RackEmbedComposer.Compose(null, syntax.Kind, RackA, name, syntax.View, syntax.Section, "{\"design\":1}");
            source.Name = name;
            using (var document = JsonDocument.Parse(CustomProps))
            {
                source.CustomProperties = document.RootElement.Clone();
            }

            return source;
        }

        /// <summary>The real preparer of the G7 product path, fed the way the G15 session feeds it.</summary>
        private static RackPreparedProductView<RackProductPrepareTestSupport.Payload> Prepare(
            RackSystemKind kind, RackViewAddress target, string sourceName, bool composeName)
        {
            var source = Source(kind, RackViewAddress.Whole(DimensionViewKind.Planta), sourceName);
            var fromSession = composeName
                ? RackProjectionEnvelopeName.WithLogicalName(source, new[] { source.Name })
                : source;
            var resolve = new RackProductPrepareTestSupport.CountingResolve<RackProductPrepareTestSupport.Authored,
                RackProductPrepareTestSupport.Resolved>(
                fromSession.Kind,
                _ => RackResolveResult<RackProductPrepareTestSupport.Resolved>.Success(
                    fromSession.Kind, new RackProductPrepareTestSupport.Resolved()));
            var preparer = RackProductPrepareTestSupport.Preparer(kind, resolve);
            var request = new RackProductViewRequest(RackViewProductOperation.GroupProjection, kind, target, G15.Facts(kind));
            var accepted = RackProductIntentAcceptance.Accept(
                new ExistingRackViewIntent<object>(new ExistingRackContext<object>(fromSession, new object()), request));
            Assert.True(accepted.IsAccepted, accepted.CauseCode);

            var result = preparer.PrepareExisting(accepted.Intent, new Comparator(fromSession.Kind));
            Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
            return result.Product;
        }

        private sealed class Comparator : IRackAuthoredComparatorPort<object, RackProductPrepareTestSupport.Authored>
        {
            internal Comparator(string kind) => Kind = kind;
            public string Kind { get; }
            public RackAuthoredComparisonResult<RackProductPrepareTestSupport.Authored> Compare(object input)
                => RackAuthoredComparisonResult<RackProductPrepareTestSupport.Authored>.Single(
                    new RackProductPrepareTestSupport.Authored());
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
    }
}
