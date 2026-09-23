using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;

namespace RackCad.Tests;

// Pure consumer composition of existing AUTH-09/10. No new production orchestration or authority.
public sealed class I58F3SeamTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var mode in new[] { "dynamic", "dynamic-positive", "dynamic-custom", "pushback-simple", "pushback-composite", "cantilever", "cabecera" })
        foreach (var outcome in new[] { "Single", "Divergent", "Unreadable", "MixedUnreadable" })
        foreach (bool reverse in new[] { false, true })
            yield return new object[] { mode, outcome, reverse };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Real_authorities_preserve_authored_across_the_complete_seam(string mode, string outcome, bool reverse)
    {
        var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
        if (mode.StartsWith("dynamic", StringComparison.Ordinal))
        {
            var original = Dynamic(mode == "dynamic-positive" ? 5 : 0,
                flags: mode == "dynamic-custom" ? new[] { false, false } : null);
            var expectedPeralte = DynamicPeralte(original);
            var resolver = new DynamicRackSystemResolver(catalog);
            Exercise(InputFor("dynamic", Store.Serialize(RackProject.ForDynamic(original)), outcome, reverse), outcome,
                RackAuthoredComparatorPorts.Dynamic(), Copy, RackResolvePorts.Dynamic<DynamicRackDesign, DynamicRackSystem>,
                d => resolver.Resolve(d).System,
                RackViewPreparationPorts.Dynamic<DynamicRackSystem, HeaderRunPlan>,
                r => new DynamicSystemFrontalBuilder().BuildPlan(r, catalog, DynamicRackEnd.Exit),
                RackBlockRequirementExtractors.HeaderRun, I58F1DomainOracle.Values, I58F1DomainOracle.Separate,
                I58F1DomainOracle.Mutate, r => r.PostPeralte = 991,
                a => Assert.Empty(RetentionViolations(original, a)),
                r => Assert.Equal(expectedPeralte, r.PostPeralte), AssertPlan);
        }
        else if (mode.StartsWith("pushback", StringComparison.Ordinal))
        {
            var original = mode == "pushback-composite" ? Composite() : new PushBackDesign { Structure = Dynamic() };
            var expected = PushBackPeraltes(original);
            var resolver = new PushBackResolver(catalog);
            Exercise(InputFor("pushback", Store.Serialize(RackProject.ForPushBack(original)), outcome, reverse), outcome,
                RackAuthoredComparatorPorts.PushBack(), Copy, RackResolvePorts.PushBack<PushBackDesign, PushBackSystem>,
                resolver.Resolve,
                RackViewPreparationPorts.PushBack<PushBackSystem, HeaderRunPlan>,
                r => new PushBackSystemFrontalBuilder().BuildPlan(r, catalog, PushBackFrontalEnd.EntradaSalida),
                RackBlockRequirementExtractors.HeaderRun, I58F1DomainOracle.Values, I58F1DomainOracle.Separate,
                I58F1DomainOracle.Mutate, r => r.Structure.PostPeralte = 992,
                a => Assert.Empty(RetentionViolations(original.Structure, a.Structure)),
                r => {
                    Assert.Equal(expected, (r.Structure.PostPeralte, r.Composite?.SideA?.Local?.Structure.PostPeralte,
                        r.Composite?.SideB?.Local?.Structure.PostPeralte));
                    Assert.Equal(7, r.Structure.PostPeralte);
                    if (mode == "pushback-composite") {
                        Assert.Equal(7, r.Composite.SideA.Local.Structure.PostPeralte);
                        Assert.Equal(9, r.Composite.SideB.Local.Structure.PostPeralte);
                    }
                }, AssertPlan);
        }
        else if (mode == "cantilever")
        {
            var original = Cantilever();
            var assembler = new CantileverLineEditorAssembler(new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());
            Exercise(InputFor(mode, Store.Serialize(RackProject.ForCantilever(original)), outcome, reverse), outcome,
                RackAuthoredComparatorPorts.Cantilever(), d => d.DeepCopy(),
                RackResolvePorts.Cantilever<CantileverLineDesign, CantileverLineEditorComputation>, d => assembler.Build(d),
                RackViewPreparationPorts.Cantilever<CantileverLineEditorComputation, CantileverViewPlan>,
                r => r.Views.First(), RackBlockRequirementExtractors.Cantilever,
                I58F1DomainOracle.Values, I58F1DomainOracle.Separate, I58F1DomainOracle.Mutate,
                r => I58F1DomainOracle.Mutate(r.Design),
                a => { Assert.Equal(Guid.Parse(Inner), a.Id); Assert.NotEqual(Guid.Parse(Outer), a.Id); },
                r => { Assert.True(r.IsValid, r.Error); Assert.Equal(original.Id, r.Design.Id); Assert.NotEmpty(r.Views); },
                Assert.NotNull);
        }
        else
        {
            var original = Header();
            Exercise(InputFor(mode, Store.Serialize(RackProject.ForSelective(original)), outcome, reverse), outcome,
                RackAuthoredComparatorPorts.Cabecera(), Copy, RackResolvePorts.Cabecera<RackFrameConfiguration, RackFrameConfiguration>,
                d => { new BracingPanelMemberBuilder().RefreshPhysicalModel(d); return d; },
                RackViewPreparationPorts.Cabecera<RackFrameConfiguration, HeaderRunPlan>,
                r => new HeaderRunPlan(Array.Empty<HeaderGroup>(), new PlantaHeaderLayoutBuilder().Build(r, catalog)),
                RackBlockRequirementExtractors.HeaderRun, I58F1DomainOracle.Values, I58F1DomainOracle.Separate,
                I58F1DomainOracle.Mutate, r => r.Members.Clear(),
                a => { Assert.Empty(a.Members); Assert.All(a.BracingPanels, p => Assert.Empty(p.Members)); },
                r => Assert.NotEmpty(r.Members), AssertPlan, DimensionViewKind.Planta);
        }
    }

    private static RackAuthoredInput InputFor(string kind, string raw, string outcome, bool reverse)
    {
        var a = Sibling(kind, raw);
        var b = Sibling(kind, raw, "handle-B", view: "lateral");
        var bad = Sibling(kind, "{", "handle-X");
        if (outcome is "Divergent" or "MixedUnreadable")
            b = b with { RawEnvelope = Set(b.RawEnvelope, "Name", "\"different authored rack name\"") };
        var siblings = outcome switch {
            "Unreadable" => new[] { a, bad },
            "MixedUnreadable" => new[] { a, b, bad },
            _ => new[] { a, b }
        };
        if (reverse) Array.Reverse(siblings);
        return ProductInput(Input(siblings), kind);
    }

    private static void Exercise<TAuthored, TResolved, TPlan>(RackAuthoredInput input, string wanted,
        IRackAuthoredComparatorPort<RackAuthoredInput, TAuthored> comparator, Func<TAuthored, TAuthored> copy,
        Func<Func<TAuthored, TResolved>, Func<TResolved, string>, RackResolveAdapter<TAuthored, TResolved>> resolveFactory,
        Func<TAuthored, TResolved> authority,
        Func<Func<TResolved, RackViewAddress, TPlan>, Func<RackViewAddress, bool>, IRackBlockRequirementExtractor<TPlan>, RackViewPreparationAdapter<TResolved, TPlan>> preparationFactory,
        Func<TResolved, TPlan> builder, IRackBlockRequirementExtractor<TPlan> extractor,
        Action<TAuthored, TAuthored> values, Action<TAuthored, TAuthored> separate,
        Action<TAuthored> mutateWorking, Action<TResolved> mutateEffective,
        Action<TAuthored> authoredInvariant, Action<TResolved> resolvedInvariant, Action<TPlan> planInvariant,
        DimensionViewKind view = DimensionViewKind.Frontal) where TAuthored : class where TResolved : class where TPlan : class
    {
        int authorityCalls = 0, resolveCalls = 0, prepareCalls = 0, builderCalls = 0;
        var baseline = comparator.Compare(input);
        var comparison = comparator.Compare(input);
        TAuthored working = null;
        TResolved effective = null;
        TPlan built = null;
        // Counters surround REAL invocations, not mocked answers. Every adapter re-entry is observable.
        var resolve = resolveFactory(d => { authorityCalls++; return authority(d); }, null);
        var prepare = preparationFactory((r, address) => {
            builderCalls++;
            Assert.Same(effective, r);
            Assert.Equal(view, address.Kind);
            built = builder(r);
            return built;
        }, _ => true, extractor);

        var expected = wanted == "Single" ? O.Single : wanted == "Divergent" ? O.Divergent : O.Unreadable;
        Assert.Equal(expected, comparison.Outcome);
        if (comparison.Outcome == O.Single) // F3_FAILURE_GATE: controls must detect bypass.
        {
            working = copy(comparison.Authored); // F3_WORKING_COPY: never hand authored to a mutating authority.
            separate(comparison.Authored, working);
            values(comparison.Authored, working);
            authoredInvariant(comparison.Authored);
            resolveCalls++;
            var resolved = resolve.Resolve(working);
            Assert.True(resolved.IsSuccess, resolved.Diagnostic);
            effective = resolved.Resolved;
            resolvedInvariant(effective);
            values(baseline.Authored, comparison.Authored);
            authoredInvariant(comparison.Authored);
            prepareCalls++;
            var prepared = prepare.Prepare(effective, RackViewAddress.Whole(view),
                RackViewFrame.TryCreate(RackViewAxisMap.RunHeight, RackPhysicalPoint.Zero, 0, 10,
                    RackFrameEndpointConvention.PhysicalRunAxes, RackPhysicalVector.Zero), "I58_F3");
            Assert.True(prepared.IsSuccess, prepared.Diagnostic);
            Assert.Same(built, prepared.Prepared.Payload);
            planInvariant(prepared.Prepared.Payload);
            values(baseline.Authored, comparison.Authored);
            authoredInvariant(comparison.Authored);
            mutateWorking(working);
            mutateEffective(effective);
            values(baseline.Authored, comparison.Authored);
            authoredInvariant(comparison.Authored);
            var again = comparator.Compare(input);
            values(comparison.Authored, again.Authored);
            separate(comparison.Authored, again.Authored);
        }
        else
        {
            Assert.Null(comparison.Authored);
            Assert.Null(working);
            Assert.Null(effective);
            Assert.Null(built);
        }
        int wantedCalls = expected == O.Single ? 1 : 0;
        Assert.Equal(wantedCalls, resolveCalls);
        Assert.Equal(wantedCalls, authorityCalls);
        Assert.Equal(wantedCalls, prepareCalls);
        Assert.Equal(wantedCalls, builderCalls);
    }

    private static void AssertPlan(HeaderRunPlan plan)
    {
        Assert.NotNull(plan);
        Assert.True(plan.Headers.Count + plan.LooseInstances.Count > 0, "Real preparation must produce a populated plan.");
    }
}
