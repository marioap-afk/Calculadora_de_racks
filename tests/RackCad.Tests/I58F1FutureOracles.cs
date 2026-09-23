using System;
using System.Linq;
using System.Collections.Generic;
using RackCad.Application;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Drawing;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using O=RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;
namespace RackCad.Tests;

// F1 executable oracles, now bound to the four concrete production ports by F2.
// Expected snapshots remain independent test data, never a comparator/reader implementation.
internal static partial class I58F1Oracles
{
    internal static RackAuthoredInput ProductInput(I58F1Input input, string kind)
        => input == null ? null : new RackAuthoredInput(input.RackId,
            input.Siblings?.Select(s => s == null ? null : new RackAuthoredSibling(s.SourceIdentity, kind, s.RawEnvelope, s.RawDesign)).ToArray(),
            input.IsComplete, input.CompletenessEvidence);

    public static void AssertFuture(I58F1Case c)
    {
        switch(c.Kind) {
            case "dynamic":Future(c,RackAuthoredComparatorPorts.Dynamic(),
                raw=>Expected(Store.Deserialize(raw).DynamicDesign),I58F1DomainOracle.Values,I58F1DomainOracle.Separate,I58F1DomainOracle.Mutate,
                (raw,a)=>DynamicParity(Store.Deserialize(raw).DynamicDesign,a),DynamicSeam);break;
            case "pushback":Future(c,RackAuthoredComparatorPorts.PushBack(),
                raw=>Expected(Store.Deserialize(raw).PushBackDesign),I58F1DomainOracle.Values,I58F1DomainOracle.Separate,I58F1DomainOracle.Mutate,
                (raw,a)=>PushBackParity(Store.Deserialize(raw).PushBackDesign,a),PushBackSeam);break;
            case "cantilever":Future(c,RackAuthoredComparatorPorts.Cantilever(),
                raw=>Store.Deserialize(raw).CantileverLineDesign,I58F1DomainOracle.Values,I58F1DomainOracle.Separate,I58F1DomainOracle.Mutate,
                (_,__)=>{},CantileverSeam);break;
            case "cabecera":Future(c,RackAuthoredComparatorPorts.Cabecera(),
                raw=>Store.Deserialize(raw).Header,I58F1DomainOracle.Values,I58F1DomainOracle.Separate,I58F1DomainOracle.Mutate,
                (_,__)=>{},HeaderSeam);break;
            default:throw new ArgumentException(c.Kind);
        }
    }
    private static void Future<T>(I58F1Case c,IRackAuthoredComparatorPort<RackAuthoredInput,T> port,Func<string,T> expected,
        Action<T,T> values,Action<T,T> separate,Action<T> mutate,Action<string,T> parity,Action<T> seam) where T:class
    {
        var result=port.Compare(ProductInput(c.Input, c.Kind));
        Assert.Equal(c.Expected,result.Outcome); // FUTURE_OUTCOME: the only admissible F1 RED cause.
        if(c.Expected!=O.Single){Assert.Null(result.Authored);return;}
        Assert.NotNull(result.Authored);
        foreach(var sibling in c.Input.Siblings){values(expected(sibling.RawDesign),result.Authored);parity(sibling.RawDesign,result.Authored);}
        var again=port.Compare(ProductInput(c.Input, c.Kind));Assert.Equal(O.Single,again.Outcome);
        var permuted=port.Compare(ProductInput(c.Input with {Siblings=c.Input.Siblings.Reverse().ToArray()}, c.Kind));Assert.Equal(O.Single,permuted.Outcome);
        values(result.Authored,again.Authored);values(result.Authored,permuted.Authored);
        separate(result.Authored,again.Authored);separate(result.Authored,permuted.Authored);
        // CT25 uses resolvable fixtures; fidelity mutants may deliberately carry catalog-independent
        // inactive intent. Their CT26/28 oracle must not impose extra resolver/catalog admission rules.
        if(c.Rows.Contains("CT58-25") || c.Rows.Contains("CT58-29"))seam(result.Authored);
        values(expected(c.Input.Siblings[0].RawDesign),result.Authored);
        mutate(result.Authored); // Every mutable subtree in the explicit domain inventory.
        var after=port.Compare(ProductInput(c.Input, c.Kind));Assert.Equal(O.Single,after.Outcome);
        foreach(var sibling in c.Input.Siblings){values(expected(sibling.RawDesign),again.Authored);values(expected(sibling.RawDesign),after.Authored);}
    }
    public static DynamicRackDesign Expected(DynamicRackDesign d)
    {
        if(d.PostPeralte>0)foreach(var m in d.Modules)if(m.IsHeader&&m.UseCalculatedHeaderConfiguration)m.HeaderConfiguration=null;
        foreach(var s in d.SafetySelections){s.Side=s.AuthoredSide??s.Side;s.AuthoredSide=s.Side;}
        return d;
    }
    public static PushBackDesign Expected(PushBackDesign d)
    {
        Expected(d.Structure);
        foreach(var f in d.Fronts.Concat(d.SideB?.FrontConfigs ?? Array.Empty<PushBackFrontConfig>())) {
            if(f==null)continue;
            if(!f.DrawPallets.Any(x=>x==true))f.DrawPallets.Clear();
            else for(int i=0;i<f.DrawPallets.Count;i++)f.DrawPallets[i]=f.DrawPallets[i]??false;
        }
        return d;
    }
    public static DynamicRackDesign Copy(DynamicRackDesign d)=>DynamicRackSystemDocument.From(d).ToDesign();
    public static PushBackDesign Copy(PushBackDesign d)=>PushBackDesignDocument.FromDomain(d).ToDomain();
    public static RackFrameConfiguration Copy(RackFrameConfiguration d)=>RackFrameProjectDocument.FromConfiguration(d).ToConfiguration();
    public static double DynamicPeralte(DynamicRackDesign d)=>new DynamicRackSystemResolver(JsonRackCatalogProvider.FromBaseDirectory().Load()).Resolve(Copy(d)).System.PostPeralte;
    public static (double Rack,double? A,double? B) PushBackPeraltes(PushBackDesign d)
    {
        var r=new PushBackResolver(JsonRackCatalogProvider.FromBaseDirectory().Load()).Resolve(Copy(d));
        return (r.Structure.PostPeralte,r.Composite?.SideA?.Local?.Structure.PostPeralte,r.Composite?.SideB?.Local?.Structure.PostPeralte);
    }
    public static void DynamicParity(DynamicRackDesign e,DynamicRackDesign a)=>Assert.Equal(DynamicPeralte(e),DynamicPeralte(a));
    public static void PushBackParity(PushBackDesign e,PushBackDesign a)=>Assert.Equal(PushBackPeraltes(e),PushBackPeraltes(a));
    // Named violations power sabotage self-checks without accepting an exception or replacing product evidence.
    public static IReadOnlyList<string> RetentionViolations(DynamicRackDesign e,DynamicRackDesign a)
    {
        var v=new List<string>();if(e.PostPeralte!=a.PostPeralte)v.Add("GLOBAL_PROMOTED");
        if(e.Modules.Count!=a.Modules.Count){v.Add("MODULE_COUNT");return v;}
        for(int i=0;i<e.Modules.Count;i++) {
            var x=e.Modules[i];var y=a.Modules[i];
            if(x.ModuleId!=y.ModuleId||x.Kind!=y.Kind)v.Add("MODULE_ORDER");
            if(x.UseCalculatedHeaderConfiguration!=y.UseCalculatedHeaderConfiguration)v.Add("PROVENANCE");
            var wanted=e.PostPeralte>0&&x.IsHeader&&x.UseCalculatedHeaderConfiguration?null:x.HeaderConfiguration;
            if((wanted is null)!=(y.HeaderConfiguration is null))v.Add("HEADER_PRESENCE");
            else if(wanted!=null&&wanted.PostPeralte!=y.HeaderConfiguration.PostPeralte)v.Add("HEADER_PERALTE_AT_MODULE");
        }
        return v;
    }
    public static void DynamicSeam(DynamicRackDesign authored)
    {
        int calls=0;var copy=Copy(authored);var resolver=new DynamicRackSystemResolver(JsonRackCatalogProvider.FromBaseDirectory().Load());
        var result=RackResolvePorts.Dynamic<DynamicRackDesign,DynamicRackSystem>(d=>{calls++;return resolver.Resolve(d).System;}).Resolve(copy);
        Assert.True(result.IsSuccess,result.Diagnostic);Assert.Equal(1,calls);
        var prep=RackViewPreparationPorts.Dynamic<DynamicRackSystem,HeaderRunPlan>((r,_)=>{Assert.Same(result.Resolved,r);return EmptyPlan();},_=>true,RackBlockRequirementExtractors.HeaderRun);
        Assert.True(prep.Prepare(result.Resolved,RackViewAddress.Whole(DimensionViewKind.Frontal),Frame(),"F1").IsSuccess);
        Assert.Equal(1,calls);I58F1DomainOracle.Mutate(copy);
    }
    public static void PushBackSeam(PushBackDesign authored)
    {
        int calls=0;var copy=Copy(authored);var resolver=new PushBackResolver(JsonRackCatalogProvider.FromBaseDirectory().Load());
        var result=RackResolvePorts.PushBack<PushBackDesign,PushBackSystem>(d=>{calls++;return resolver.Resolve(d);}).Resolve(copy);
        Assert.True(result.IsSuccess,result.Diagnostic);Assert.Equal(1,calls);
        var prep=RackViewPreparationPorts.PushBack<PushBackSystem,HeaderRunPlan>((r,_)=>{Assert.Same(result.Resolved,r);return EmptyPlan();},_=>true,RackBlockRequirementExtractors.HeaderRun);
        Assert.True(prep.Prepare(result.Resolved,RackViewAddress.Whole(DimensionViewKind.Frontal),Frame(),"F1").IsSuccess);
        Assert.Equal(1,calls);I58F1DomainOracle.Mutate(copy);
    }
    public static void CantileverSeam(CantileverLineDesign authored)
    {
        int calls=0;var copy=authored.DeepCopy();var assembler=new CantileverLineEditorAssembler(new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());
        var result=RackResolvePorts.Cantilever<CantileverLineDesign,CantileverLineEditorComputation>(d=>{calls++;return assembler.Build(d);}).Resolve(copy);
        Assert.True(result.IsSuccess,result.Diagnostic);Assert.Equal(1,calls);Assert.True(result.Resolved.IsValid,result.Resolved.Error);
        var prep=RackViewPreparationPorts.Cantilever<CantileverLineEditorComputation,CantileverViewPlan>((r,_)=>{Assert.Same(result.Resolved,r);return r.Views.First();},_=>true,RackBlockRequirementExtractors.Cantilever);
        Assert.True(prep.Prepare(result.Resolved,RackViewAddress.Whole(DimensionViewKind.Frontal),Frame(),"F1").IsSuccess);
        Assert.Equal(1,calls);I58F1DomainOracle.Mutate(copy);I58F1DomainOracle.Mutate(result.Resolved.Design);
    }
    public static void HeaderSeam(RackFrameConfiguration authored)
    {
        int calls=0;var copy=Copy(authored);
        var result=RackResolvePorts.Cabecera<RackFrameConfiguration,RackFrameConfiguration>(d=>{calls++;new BracingPanelMemberBuilder().RefreshPhysicalModel(d);return d;}).Resolve(copy);
        Assert.True(result.IsSuccess,result.Diagnostic);Assert.Equal(1,calls);
        var prep=RackViewPreparationPorts.Cabecera<RackFrameConfiguration,HeaderRunPlan>((r,_)=>{Assert.Same(result.Resolved,r);return EmptyPlan();},_=>true,RackBlockRequirementExtractors.HeaderRun);
        Assert.True(prep.Prepare(result.Resolved,RackViewAddress.Whole(DimensionViewKind.Frontal),Frame(),"F1").IsSuccess);
        Assert.Equal(1,calls);I58F1DomainOracle.Mutate(copy);
    }
    private static HeaderRunPlan EmptyPlan()=>new HeaderRunPlan(Array.Empty<HeaderGroup>(),Array.Empty<HeaderBlockInstance>());
    private static RackViewFrameResult Frame()=>RackViewFrame.TryCreate(RackViewAxisMap.RunHeight,RackPhysicalPoint.Zero,0,10,RackFrameEndpointConvention.PhysicalRunAxes,RackPhysicalVector.Zero);
}
