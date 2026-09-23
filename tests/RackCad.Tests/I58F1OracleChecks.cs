using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.RackFrames;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O=RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;
namespace RackCad.Tests;

public sealed class I58F1OracleChecks
{
    public static IEnumerable<object[]> Singles()=>I58F1Matrix.Build().Where(c=>c.Expected==O.Single).Select(c=>new object[]{c});
    [Theory]
    [MemberData(nameof(Singles))]
    public void Oracle_fixture_consistency_NOT_AUTH13_GREEN(I58F1Case c)
    {
        // No comparator and no synthetic Single. Verify the declared equivalent fixtures support the
        // typed future oracle, including all legacy/excluded values, before recording admissible RED.
        foreach(var sibling in c.Input.Siblings) {
            var e=Store.Deserialize(c.OriginalDesign);var a=Store.Deserialize(sibling.RawDesign);
            switch(c.Kind) {
                case "dynamic":I58F1DomainOracle.Values(Expected(e.DynamicDesign),Expected(a.DynamicDesign));break;
                case "pushback":I58F1DomainOracle.Values(Expected(e.PushBackDesign),Expected(a.PushBackDesign));break;
                case "cantilever":I58F1DomainOracle.Values(e.CantileverLineDesign,a.CantileverLineDesign);break;
                case "cabecera":I58F1DomainOracle.Values(e.Header,a.Header);break;
            }
        }
    }
    [Theory]
    [InlineData("dynamic")][InlineData("pushback")][InlineData("cantilever")][InlineData("cabecera")]
    public void Typed_oracle_and_seam_selfcheck_NOT_AUTH13_GREEN(string kind)
    {
        var p=Store.Deserialize(Rich(kind));var q=Store.Deserialize(Rich(kind));var e=Store.Deserialize(Rich(kind));
        switch(kind) {
            case "dynamic": {
                var a=Expected(p.DynamicDesign);var b=Expected(q.DynamicDesign);Expected(e.DynamicDesign);
                I58F1DomainOracle.Values(a,b);I58F1DomainOracle.Separate(a,b);DynamicSeam(a);I58F1DomainOracle.Values(e.DynamicDesign,a);
                I58F1DomainOracle.Mutate(a);I58F1DomainOracle.Values(e.DynamicDesign,b);break;
            }
            case "pushback": {
                var a=Expected(p.PushBackDesign);var b=Expected(q.PushBackDesign);Expected(e.PushBackDesign);
                I58F1DomainOracle.Values(a,b);I58F1DomainOracle.Separate(a,b);PushBackSeam(a);I58F1DomainOracle.Values(e.PushBackDesign,a);
                I58F1DomainOracle.Mutate(a);I58F1DomainOracle.Values(e.PushBackDesign,b);break;
            }
            case "cantilever": {
                var a=p.CantileverLineDesign;var b=q.CantileverLineDesign;
                I58F1DomainOracle.Values(a,b);I58F1DomainOracle.Separate(a,b);CantileverSeam(a);I58F1DomainOracle.Values(e.CantileverLineDesign,a);
                I58F1DomainOracle.Mutate(a);I58F1DomainOracle.Values(e.CantileverLineDesign,b);break;
            }
            case "cabecera": {
                var a=p.Header;var b=q.Header;
                I58F1DomainOracle.Values(a,b);I58F1DomainOracle.Separate(a,b);HeaderSeam(a);I58F1DomainOracle.Values(e.Header,a);
                I58F1DomainOracle.Mutate(a);I58F1DomainOracle.Values(e.Header,b);break;
            }
        }
    }
    [Theory]
    [InlineData(false,7)][InlineData(false,9)][InlineData(true,7)][InlineData(true,9)]
    public void V2_header_null_sabotage_breaks_real_resolution_parity(bool push,double peralte)
    {
        var original=Dynamic(0,new double?[]{peralte});var bad=Copy(original);bad.Modules[0].HeaderConfiguration=null;
        Assert.Contains("HEADER_PRESENCE",RetentionViolations(original,bad));
        if(push){Assert.Equal(peralte,PushBackPeraltes(new PushBackDesign{Structure=original}).Rack);Assert.Equal(3,PushBackPeraltes(new PushBackDesign{Structure=bad}).Rack);}
        else {Assert.Equal(peralte,DynamicPeralte(original));Assert.Equal(3,DynamicPeralte(bad));}
    }
    [Theory]
    [InlineData("global")][InlineData("provenance")][InlineData("module")][InlineData("payload-moved")]
    public void Materialization_sabotage_is_observable_beyond_internal_equality(string attack)
    {
        var e=Dynamic();var a=Copy(e);string expected;
        switch(attack) {
            case "global":a.PostPeralte=7;expected="GLOBAL_PROMOTED";Assert.Equal(DynamicPeralte(e),DynamicPeralte(a));break;
            case "provenance":a.Modules[0].UseCalculatedHeaderConfiguration=false;expected="PROVENANCE";break;
            case "module":a.Modules[0].ModuleId="neighbor";expected="MODULE_ORDER";break;
            default:var h=a.Modules[0].HeaderConfiguration;a.Modules[0].HeaderConfiguration=a.Modules[1].HeaderConfiguration;a.Modules[1].HeaderConfiguration=h;expected="HEADER_PERALTE_AT_MODULE";break;
        }
        Assert.Contains(expected,RetentionViolations(e,a));
    }
    [Theory]
    [InlineData(0,true)][InlineData(0,false)][InlineData(8,true)][InlineData(8,false)]
    public void Composite_fixture_resolves_rack_and_local_A_B(double global,bool calcA)
    {
        var original=Composite(global,calcA);var projected=Expected(Copy(original));
        var values=PushBackPeraltes(original);Assert.Equal(global>0?8:7,values.Rack);Assert.Equal(global>0?8:7,values.A);Assert.Equal(global>0?8:9,values.B);
        PushBackParity(original,projected);Assert.Empty(RetentionViolations(original.Structure,projected.Structure));
        var bad=Copy(original);foreach(var m in bad.Structure.Modules)if(m.IsHeader)m.HeaderConfiguration=null;
        if(global==0)Assert.NotEqual(values,PushBackPeraltes(bad));
    }
    [Theory]
    [InlineData("dynamic")][InlineData("pushback")]
    public void Ordered_first_positive_and_conservative_payload_are_distinct_obligations(string kind)
    {
        foreach(var (seq,wanted) in new[]{(new double?[]{7,9},7d),(new double?[]{9,7},9d),(new double?[]{0,7},7d),(new double?[]{7,0},7d)}) {
            var d=Dynamic(0,seq);double actual=kind=="dynamic"?DynamicPeralte(d):PushBackPeraltes(new PushBackDesign{Structure=d}).Rack;Assert.Equal(wanted,actual);
        }
        var c=I58F1Matrix.Build().Single(x=>x.Id==kind+"/conservative-second-positive");Assert.Equal(O.Divergent,c.Expected);
        Assert.Equal(DynamicPeralte(Dynamic()),DynamicPeralte(Dynamic(0,new double?[]{7,0})));
    }
    [Theory]
    [InlineData("dynamic")][InlineData("pushback")][InlineData("cantilever")][InlineData("cabecera")]
    public void Effective_context_is_outside_the_comparator_carrier(string kind)
    {
        string raw=Rich(kind);var a=Store.Deserialize(raw);var b=Store.Deserialize(raw);
        int externalAuthorityCalls=0;
        // Two explicit effective contexts applied to independent work snapshots outside AUTH-13.
        // These are test contexts, never authored materialization or a proposed comparator algorithm.
        switch(kind) {
            case "dynamic":a.DynamicDesign.PostPeralte=8;b.DynamicDesign.PostPeralte=10;externalAuthorityCalls+=2;
                Assert.NotEqual(DynamicPeralte(a.DynamicDesign),DynamicPeralte(b.DynamicDesign));break;
            case "pushback":a.PushBackDesign.Structure.PostPeralte=8;b.PushBackDesign.Structure.PostPeralte=10;externalAuthorityCalls+=2;
                Assert.NotEqual(PushBackPeraltes(a.PushBackDesign),PushBackPeraltes(b.PushBackDesign));break;
            case "cantilever":a.CantileverLineDesign.ColumnCentreSpacing=80;b.CantileverLineDesign.ColumnCentreSpacing=120;
                var assembler=new RackCad.Application.Systems.Cantilever.CantileverLineEditorAssembler(new RackCad.Application.StructuralSections.CsvStructuralSectionCatalogProvider(RackCad.Application.Catalogs.CatalogDirectory.Resolve()).Load());
                var ra=assembler.Build(a.CantileverLineDesign);var rb=assembler.Build(b.CantileverLineDesign);externalAuthorityCalls+=2;
                Assert.True(ra.IsValid,ra.Error);Assert.True(rb.IsValid,rb.Error);Assert.NotEqual(ra.Design.ColumnCentreSpacing,rb.Design.ColumnCentreSpacing);break;
            case "cabecera":a.Header.Horizontals[1].Elevation=100;b.Header.Horizontals[1].Elevation=120;
                var builder=new RackCad.Application.RackFrames.BracingPanelMemberBuilder();builder.RefreshPhysicalModel(a.Header);builder.RefreshPhysicalModel(b.Header);externalAuthorityCalls+=2;
                Assert.NotEqual(a.Header.BracingPanels[0].EndElevation,b.Header.BracingPanels[0].EndElevation);break;
        }
        var c=I58F1Matrix.Build().Single(x=>x.Id==kind+"/external-context");
        AssertBaseline(c);Assert.Equal(2,externalAuthorityCalls);
        // Unsupported baseline only. Future outcome, typed fidelity and authority count are in AssertFuture.
    }
    [Fact]
    public void CT58_22_arbitrary_marker_and_Cama_remain_unsupported()
    {
        var marker=new object();Assert.Equal(O.Unreadable,RackAuthoredComparatorPorts.Cama<object,object>().Compare(marker).Outcome);
        Assert.Null(RackAuthoredComparatorPorts.Cama<object,object>().Compare(marker).Authored);
        foreach(var port in new[]{RackAuthoredComparatorPorts.Dynamic<object,object>(),RackAuthoredComparatorPorts.PushBack<object,object>(),RackAuthoredComparatorPorts.Cantilever<object,object>(),RackAuthoredComparatorPorts.Cabecera<object,object>()}) {
            var r=port.Compare(marker);Assert.Equal(O.Unreadable,r.Outcome);Assert.Null(r.Authored);
        }
        // Selective outcomes and no-first/no-majority are exercised unmodified in inherited F6/authority tests.
    }
}
