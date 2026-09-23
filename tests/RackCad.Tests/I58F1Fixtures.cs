using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using RackCad.Application.Systems.Shared;

namespace RackCad.Tests;

// F-03 test-only carrier. Raw strings are originals, never reconstructed by a comparison adapter.
// External context is opaque fixture data; no resolver/catalog delegate is available to the comparator.
public sealed record I58F1Sibling(string SourceIdentity, string RawEnvelope, string RawDesign,
    string View, int Section, double PlacementX, string ExternalContext);
public sealed record I58F1Input(string RackId, IReadOnlyList<I58F1Sibling> Siblings, bool IsComplete, string CompletenessEvidence);
public sealed record I58F1Case(string Id, string Kind, string[] Rows, RackAuthoredComparisonOutcome Expected,
    I58F1Input Input, string OriginalDesign, string Stimulus)
{
    public override string ToString() => Id;
}

internal static class I58F1Fixtures
{
    public const string Outer = "11111111-1111-1111-1111-111111111111";
    public const string Inner = "22222222-2222-2222-2222-222222222222";
    public const string Other = "33333333-3333-3333-3333-333333333333";
    public static readonly string[] Kinds = { "dynamic", "pushback", "cantilever", "cabecera" };
    public static readonly RackProjectStore Store = new RackProjectStore();
    public static RackFrameConfiguration Header(double p = 7) => new RackFrameProjectDocument {
        Name="Cabecera F1", Units="in", Height=132, Depth=48, PostPeralte=p,
        StandardBaselineId="F1-header", StandardBaselineVersion="1",
        LeftPost=new PostDocument {PostCatalogId="POSTE_OMEGA_3X3",Description="L"},
        RightPost=new PostDocument {PostCatalogId="POSTE_OMEGA_3X3",Description="R"},
        LeftBasePlate=new PlateDocument {PlateCatalogId="PLACA",ConnectionPointId="L",PeralteOverride=null},
        RightBasePlate=new PlateDocument {PlateCatalogId="PLACA",ConnectionPointId="R",PeralteOverride=3},
        Horizontals=new List<HorizontalDocument> {
            new HorizontalDocument {Id="h1",Number=1,Elevation=12,ProfileId="H",Quantity=1},
            new HorizontalDocument {Id="h2",Number=2,Elevation=120,ProfileId="H",Quantity=1}},
        Panels=new List<PanelDocument> {new PanelDocument {PanelId="p1",Number=1,LowerHorizontalId="h1",UpperHorizontalId="h2",DiagonalProfileId="D"}}
    }.ToConfiguration();
    public static DynamicRackDesign Dynamic(double global=0, double?[] peraltes=null, bool[] flags=null)
    {
        peraltes ??= new double?[]{7,9}; flags ??= new[]{true,true};
        var d=new DynamicRackDesign {Pallet=new PalletSpecification(40,48,50,1000,"kg"),PalletsDeep=4,PostPeralte=global};
        d.Fronts.Add(new DynamicRackFrontDesign {PalletCount=2,LoadLevels=2,PalletsDeep=4,DepthStartPosition=1});
        d.Fronts.Add(new DynamicRackFrontDesign {PalletCount=1,LoadLevels=2,PalletsDeep=4,DepthStartPosition=1});
        for(int i=0;i<peraltes.Length;i++) d.Modules.Add(new DynamicRackModuleDesign {
            ModuleId="H"+(i+1),Kind=i==0?DynamicRackModuleKind.HeaderStart:DynamicRackModuleKind.HeaderEnd,
            Length=48,UseCalculatedHeaderConfiguration=flags[i%flags.Length],HeaderConfiguration=peraltes[i].HasValue?Header(peraltes[i].Value):null,Notes="module"+i});
        return d;
    }
    public static CantileverLineDesign Cantilever()
    {
        var d=new CantileverLineDesign {Id=Guid.Parse(Inner),Name="C F1",StationCount=3,ColumnCentreSpacing=96,
            StationTopology=new CantileverLineStationTopologyDesign {LevelCount=2,RequestedClearHeight=24,
                ColumnBaseTemplate=new CantileverStationColumnBaseTemplateDesign {ColumnSectionId="AISC-W-W10X33",Base=new CantileverBaseDesign {SectionId="AISC-W-W12X26",Length=48}}},
            DefaultArmTemplate=new CantileverArmTemplateDesign {Body=new CantileverArmBodyDesign {SectionId="AISC-HSS-RECT-HSS4X4X_250",CutLength=36}}};
        d.ArmCellOverrides.Add(new CantileverArmCellOverride {StationIndex=0,LevelIndex=0,Side=CantileverArmSide.PositiveY,Arm=d.DefaultArmTemplate.DeepCopy()});
        d.Bracing.AdvancedPanelSegments.Add(new CantileverPanelSegmentDesign {StartElevation=12,EndElevation=36});
        return d;
    }
    public static PushBackDesign Composite(double global=0, bool calculatedA=true)
    {
        var d=new PushBackDesign {
            Structure=new DynamicRackDesign {Pallet=new PalletSpecification(42,48,60,1000,"kg"),PalletsDeep=5,LoadLevels=3,FirstLevelHeight=4,BeamDepth=4},
            SideB=new PushBackSideDesign {IsPresent=true,LoadLevels=2,FirstLevelHeight=4},
            Composite=new PushBackCompositeDesign {DefaultTopology=PushBackCellTopology.Encontradas}};
        for(int i=0;i<2;i++) {
            d.Structure.Fronts.Add(new DynamicRackFrontDesign {PalletCount=1,LoadLevels=3,PalletsDeep=5,DepthStartPosition=1});
            d.Fronts.Add(new PushBackFrontConfig {DefaultPalletsDeep=5});
            d.SideB.Fronts.Add(new DynamicRackFrontDesign {PalletCount=1,LoadLevels=2,PalletsDeep=4,DepthStartPosition=1});
            d.SideB.FrontConfigs.Add(new PushBackFrontConfig {DefaultPalletsDeep=4});
        }
        // Resolver is fixture setup ONLY; never called by a comparator. Restore authored global afterwards.
        var cat=JsonRackCatalogProvider.FromBaseDirectory().Load();
        var seed=new DynamicRackSystemResolver(cat).Snapshot(new PushBackResolver(cat).Resolve(d).Structure,3,4,4,null);
        foreach(var m in seed.Modules) {
            if(m.IsHeader) {bool b=m.ModuleId.StartsWith("B:",StringComparison.Ordinal);m.HeaderConfiguration=Header(b?9:7);m.UseCalculatedHeaderConfiguration=b?!calculatedA:calculatedA;}
            d.Structure.Modules.Add(m);
        }
        d.Structure.PostPeralte=global;
        return d;
    }
    public static string Serialize(string kind) => kind switch {
        "dynamic"=>Store.Serialize(RackProject.ForDynamic(Dynamic())),
        "pushback"=>Store.Serialize(RackProject.ForPushBack(Composite())),
        "cantilever"=>Store.Serialize(RackProject.ForCantilever(Cantilever())),
        "cabecera"=>Store.Serialize(RackProject.ForSelective(Header())),
        _=>throw new ArgumentOutOfRangeException(nameof(kind))};
    public static string Root(string kind) => kind switch {"dynamic"=>"DynamicSystem","pushback"=>"PushBack","cantilever"=>"Cantilever",_=>"Header"};
    public static string Structure(string kind) => kind=="dynamic"?"DynamicSystem":"PushBack.Structure";
    public static I58F1Sibling Sibling(string kind,string design,string source="handle-A",string outer=Outer,string view="frontal",int section=-1)
    {
        var envelope=new RackEmbedDocument {Id=outer,Kind=kind,Name="Rack F1",View=view,Section=section,Design=design};
        return new I58F1Sibling(source,new RackEmbedStore().Serialize(envelope),design,view,section,0,"catalog-A/variables-A/effective-A");
    }
    public static I58F1Input Input(params I58F1Sibling[] siblings)=>new I58F1Input(Outer,siblings,true,"complete scan of all source handles");
    public static I58F1Case Pair(string id,string kind,string[] rows,RackAuthoredComparisonOutcome expected,string a,string b,string stimulus=null)
        =>new I58F1Case(id,kind,rows,expected,Input(Sibling(kind,a),Sibling(kind,b,"handle-B")),a,stimulus??id);
    // Explicit fixture editing, not a reader or equality algorithm. Paths are listed individually in the matrix.
    public static JsonNode At(JsonNode node,string path) {
        if(string.IsNullOrEmpty(path))return node;
        foreach(var part in path.Split('.')) node=node is JsonArray array?array[int.Parse(part)]:node[part];
        return node;
    }
    public static string Set(string raw,string path,string json) {
        var node=JsonNode.Parse(raw);int split=path.LastIndexOf('.');var parent=At(node,split<0?"":path.Substring(0,split));string key=path.Substring(split+1);
        if(parent is JsonArray arr)arr[int.Parse(key)]=JsonNode.Parse(json);else parent[key]=JsonNode.Parse(json);
        return node.ToJsonString();
    }
    public static string Remove(string raw,string path) {
        var node=JsonNode.Parse(raw);int split=path.LastIndexOf('.');((JsonObject)At(node,split<0?"":path.Substring(0,split))).Remove(path.Substring(split+1));return node.ToJsonString();
    }
    public static string Reverse(string raw,string path) {
        var node=JsonNode.Parse(raw);var arr=(JsonArray)At(node,path);var values=arr.Select(x=>x?.DeepClone()).Reverse().ToArray();arr.Clear();foreach(var v in values)arr.Add(v);return node.ToJsonString();
    }
    public static string Rich(string kind)
    {
        string raw=Serialize(kind);
        if(kind is "dynamic" or "pushback") {
            string s=Structure(kind);
            raw=Set(raw,s+".Fronts.0.Levels","[{\"ClearHeight\":60,\"PalletHeight\":50,\"BeamLengthOverride\":null}]");
            raw=Set(raw,s+".SafetySelections","[{\"ElementId\":\"tope\",\"Quantity\":1,\"Side\":1,\"AuthoredSide\":1,\"PostSides\":[{\"PostIndex\":0,\"Side\":1}],\"TopeOffCells\":[{\"Frente\":0,\"Level\":0}],\"BotaBPosts\":[{\"PostIndex\":0,\"Placement\":1}]}]");
            foreach(var (field,value) in new[]{("DesviadorOffCells","[{\"Frente\":0,\"Level\":0}]"),("GuiaEntradaOffCells","[{\"Frente\":0,\"Level\":0}]"),("ParrillaOffCells","[{\"Frente\":0,\"Level\":0}]"),("DefensaPosts","[{\"PostIndex\":0,\"ExitLength\":12,\"EntranceLength\":14,\"ExitAuto\":false,\"EntranceAuto\":true}]"),("BotaPosts","[{\"PostIndex\":0,\"Placement\":1}]")})
                raw=Set(raw,s+".SafetySelections.0."+field,value);
            string header=JsonSerializer.Serialize(RackFrameProjectDocument.FromConfiguration(Header()));
            raw=Set(raw,s+".HeaderLineOverrides","[{\"PostIndex\":0,\"ModuleId\":\"H1\",\"Header\":"+header+"}]");
            raw=Set(raw,s+".DerivedPostLineOverrides","[{\"PostIndex\":0,\"Height\":120}]");
        }
        if(kind=="pushback") {
            raw=Set(raw,"PushBack.Fronts.0.HighEndBeamPeraltes","[null,4]");
            raw=Set(raw,"PushBack.Fronts.0.PalletsDeepOverrides","[null,4]");
            raw=Set(raw,"PushBack.Fronts.0.DrawPallets","[false,true]");
            raw=Set(raw,"PushBack.RearTopeOffCells","[{\"Frente\":0,\"Level\":0}]");
            raw=Set(raw,"PushBack.Composite.Topologies","[{\"Frente\":0,\"Level\":0,\"Topology\":\"Encontradas\",\"Direction\":\"AToB\",\"CorridaDepth\":null}]");
        }
        return raw;
    }
}
