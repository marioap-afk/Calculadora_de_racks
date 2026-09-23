using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;

// Diagnostic only: existing resolvers plus a test-local proposed materialization transform.
// There is no sibling comparator, new product authority, or AUTH-13 GREEN in this program.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
var dynamicResolver = new DynamicRackSystemResolver(catalog);
var pushResolver = new PushBackResolver(catalog);
var store = new RackProjectStore();
int selected = 0, assertions = 0, negativeControls = 0;
void Check(bool condition, string label) { assertions++; if (!condition) throw new Exception(label); }
string Val(double? n) => n?.ToString(CultureInfo.InvariantCulture) ?? "null";
RackFrameConfiguration Header(double p) => new RackFrameProjectDocument {
    Name="H", Height=132, Depth=48, PostPeralte=p,
    LeftPost=new PostDocument { PostCatalogId="POSTE_OMEGA_3X3" },
    RightPost=new PostDocument { PostCatalogId="POSTE_OMEGA_3X3" }
}.ToConfiguration();
DynamicRackDesign Make(double global, double?[] values, bool[] calculated) {
    var d = new DynamicRackDesign { Pallet=new PalletSpecification(40,48,50,1000,"kg"), PalletsDeep=4, PostPeralte=global };
    d.Fronts.Add(new DynamicRackFrontDesign { PalletCount=2 });
    for (int i=0;i<values.Length;i++) d.Modules.Add(new DynamicRackModuleDesign {
        ModuleId=$"H{i+1}", Kind=i==0 ? DynamicRackModuleKind.HeaderStart : DynamicRackModuleKind.HeaderEnd,
        Length=48, UseCalculatedHeaderConfiguration=calculated[i], HeaderConfiguration=values[i].HasValue ? Header(values[i].Value) : null
    });
    return d;
}
DynamicRackDesign Copy(DynamicRackDesign source) {
    var copy=DynamicRackSystemDocument.From(source).ToDesign();
    // Keep deliberately malformed custom+null for observation of CURRENT resolver; V3 rejects it.
    for(int i=0;i<source.Modules.Count;i++) copy.Modules[i].UseCalculatedHeaderConfiguration=source.Modules[i].UseCalculatedHeaderConfiguration;
    return copy;
}
void Proposed(DynamicRackDesign output) {
    // Full typed persisted header retained at global zero; no invented stub or promoted global.
    if (output.PostPeralte>0) foreach(var m in output.Modules)
        if(m.IsHeader && m.UseCalculatedHeaderConfiguration) m.HeaderConfiguration=null;
}
void Run(string label, DynamicRackDesign original, bool push) {
    selected++;
    var baseline = push ? pushResolver.Resolve(new PushBackDesign {Structure=Copy(original)}).Structure : dynamicResolver.Resolve(Copy(original)).System;
    var output=Copy(original); Proposed(output);
    var actual = push ? pushResolver.Resolve(new PushBackDesign {Structure=Copy(output)}).Structure : dynamicResolver.Resolve(Copy(output)).System;
    Check(actual.PostPeralte==baseline.PostPeralte,label+" parity");
    Check(output.PostPeralte==original.PostPeralte,label+" no global promotion");
    var expected=original.PostPeralte>0 ? original.PostPeralte : original.Modules.FirstOrDefault(m=>m.IsHeader && m.HeaderConfiguration?.PostPeralte>0)?.HeaderConfiguration.PostPeralte;
    if(expected.HasValue) Check(baseline.PostPeralte==expected.Value,label+" first positive");
    for(int i=0;i<output.Modules.Count;i++) {
        var m=output.Modules[i]; var source=original.Modules[i];
        Check(m.ModuleId==source.ModuleId && m.UseCalculatedHeaderConfiguration==source.UseCalculatedHeaderConfiguration,label+" module/provenance");
        if(original.PostPeralte==0 || !m.UseCalculatedHeaderConfiguration)
            Check(m.HeaderConfiguration?.PostPeralte==source.HeaderConfiguration?.PostPeralte,label+" retained fallback");
        if(m.HeaderConfiguration!=null) {
            Check(!ReferenceEquals(m.HeaderConfiguration,source.HeaderConfiguration),label+" isolation");
            double before=source.HeaderConfiguration.PostPeralte;
            m.HeaderConfiguration.PostPeralte=123; Check(source.HeaderConfiguration.PostPeralte==before,label+" mutation isolation");
            m.HeaderConfiguration.PostPeralte=before;
        }
        // Check the reconstructed physical header, beyond rack PostPeralte, for eligible positive-global exclusions.
        var a=actual.Modules[i].AssociatedFrameConfiguration; var b=baseline.Modules[i].AssociatedFrameConfiguration;
        if(a!=null && b!=null) Check(a.Height==b.Height && a.Depth==b.Depth && a.PostPeralte==b.PostPeralte && a.Horizontals.Count==b.Horizontals.Count && a.Panels.Count==b.Panels.Count,label+" reconstructed header");
    }
    bool accredited=!original.Modules.Any(m=>!m.UseCalculatedHeaderConfiguration && m.HeaderConfiguration==null);
    if(accredited) {
        var reopened=push ? store.Deserialize(store.Serialize(RackProject.ForPushBack(new PushBackDesign {Structure=output}))).PushBackDesign.Structure : store.Deserialize(store.Serialize(RackProject.ForDynamic(output))).DynamicDesign;
        var resolved=push ? pushResolver.Resolve(new PushBackDesign {Structure=reopened}).Structure : dynamicResolver.Resolve(reopened).System;
        Check(resolved.PostPeralte==baseline.PostPeralte,label+" store roundtrip parity");
        Check(reopened.PostPeralte==original.PostPeralte,label+" store inherited global");
    }
    Console.WriteLine($"{(push?"PushBack":"Dynamic")}|{label}|global={original.PostPeralte}|headers={string.Join(",",original.Modules.Select(m=>$"{(m.UseCalculatedHeaderConfiguration?"calc":"custom")}:{Val(m.HeaderConfiguration?.PostPeralte)}"))}|resolved={baseline.PostPeralte}|projected={actual.PostPeralte}|accredited={accredited}");
}
foreach(bool push in new[]{false,true}) {
    foreach(double global in new[]{0d,8d}) foreach(bool calc in new[]{true,false}) foreach(double? p in new double?[]{null,0,7,9})
        Run("single",Make(global,new[]{p},new[]{calc}),push);
    foreach(double global in new[]{0d,8d}) foreach(var values in new[]{new double?[]{7,9},new double?[]{9,7},new double?[]{0,7},new double?[]{7,0}})
        foreach(var flags in new[]{new[]{true,true},new[]{false,false},new[]{true,false},new[]{false,true}})
            Run("ordered",Make(global,values,flags),push);
    foreach(string legacy in new[]{"absent","null"}) {
        var raw=JsonSerializer.SerializeToNode(DynamicRackSystemDocument.From(Make(0,new double?[]{7},new[]{true}))).AsObject();
        if(legacy=="absent")raw.Remove("PostPeralte"); else raw["PostPeralte"]=null;
        var d=raw.Deserialize<DynamicRackSystemDocument>().ToDesign(); Check(d.PostPeralte==0,"legacy global"); Run("legacy-"+legacy,d,push);
    }
    foreach(double p in new[]{7d,9d}) {
        var input=Make(0,new double?[]{p},new[]{true}); var sabotaged=Copy(input); sabotaged.Modules[0].HeaderConfiguration=null;
        double before=push ? pushResolver.Resolve(new PushBackDesign{Structure=input}).Structure.PostPeralte : dynamicResolver.Resolve(input).System.PostPeralte;
        double after=push ? pushResolver.Resolve(new PushBackDesign{Structure=sabotaged}).Structure.PostPeralte : dynamicResolver.Resolve(sabotaged).System.PostPeralte;
        selected++; Check(before!=after,"V2 sabotage must break parity"); negativeControls++;
        Console.WriteLine($"{(push?"PushBack":"Dynamic")}|V2-sabotage|before={before}|after={after}|PARITY_REJECTED");
    }
}
PushBackDesign Composite() {
    var d=new PushBackDesign {
        Structure=new DynamicRackDesign {Pallet=new PalletSpecification(42,48,60,1000,"kg"),PalletsDeep=5,LoadLevels=3,FirstLevelHeight=4,BeamDepth=4},
        SideB=new PushBackSideDesign {IsPresent=true,LoadLevels=2,FirstLevelHeight=4},
        Composite=new PushBackCompositeDesign {DefaultTopology=PushBackCellTopology.Encontradas}
    };
    d.Structure.Fronts.Add(new DynamicRackFrontDesign {PalletCount=1,LoadLevels=3,PalletsDeep=5,DepthStartPosition=1});
    d.Fronts.Add(new PushBackFrontConfig {DefaultPalletsDeep=5});
    d.SideB.Fronts.Add(new DynamicRackFrontDesign {PalletCount=1,LoadLevels=2,PalletsDeep=4,DepthStartPosition=1});
    d.SideB.FrontConfigs.Add(new PushBackFrontConfig {DefaultPalletsDeep=4});
    // Seed the real composite module topology, then change only the diagnostic authored header/global inputs.
    var seed=dynamicResolver.Snapshot(pushResolver.Resolve(d).Structure,3,4,4,d.Structure.HeaderPostCatalogId);
    foreach(var m in seed.Modules)d.Structure.Modules.Add(m);
    return d;
}
foreach(double global in new[]{0d,8d}) foreach(bool calc in new[]{true,false}) foreach(var values in new[]{new double?[]{null,0},new double?[]{7,9},new double?[]{9,7},new double?[]{0,7}}) {
    selected++; var d=Composite(); d.Structure.PostPeralte=global; int h=0;
    foreach(var m in d.Structure.Modules.Where(m=>m.IsHeader)) { m.UseCalculatedHeaderConfiguration=calc; var p=values[h++%2]; m.HeaderConfiguration=p.HasValue?Header(p.Value):null; }
    var before=pushResolver.Resolve(d);
    var output=PushBackDesignDocument.FromDomain(d).ToDomain(); Proposed(output.Structure);
    var after=pushResolver.Resolve(output);
    Check(before.Structure.PostPeralte==after.Structure.PostPeralte,"composite global parity");
    Check(before.Composite.SideA.Local.Structure.PostPeralte==after.Composite.SideA.Local.Structure.PostPeralte,"side A parity");
    Check(before.Composite.SideB.Local.Structure.PostPeralte==after.Composite.SideB.Local.Structure.PostPeralte,"side B parity");
    Check(output.Structure.PostPeralte==global,"composite no promotion");
    Console.WriteLine($"PushBackComposite|global={global}|calc={calc}|headers={string.Join(",",d.Structure.Modules.Where(m=>m.IsHeader).Select(m=>$"{m.ModuleId}:{Val(m.HeaderConfiguration?.PostPeralte)}"))}|resolved={before.Structure.PostPeralte}|A={before.Composite.SideA.Local.Structure.PostPeralte}|B={before.Composite.SideB.Local.Structure.PostPeralte}|projected={after.Structure.PostPeralte}");
}
Check(selected>0,"nonzero selection");
Console.WriteLine($"SELECTED={selected}; ASSERTIONS={assertions}; NEGATIVE_CONTROLS={negativeControls}; DIAGNOSTIC_PASS; AUTH13_GREEN=NO");
