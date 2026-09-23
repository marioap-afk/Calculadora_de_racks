using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using static RackCad.Tests.I58F1Fixtures;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;
namespace RackCad.Tests;

internal static partial class I58F1Matrix
{
    public static IReadOnlyList<I58F1Case> Build()
    {
        var cases=new List<I58F1Case>();
        foreach(string kind in Kinds) {
            string a=Rich(kind), root=Root(kind);
            string scalar=kind switch {"dynamic"=>root+".PalletDepth","pushback"=>root+".RearTopeSaque","cantilever"=>root+".Line.ColumnCentreSpacing",_=>root+".Height"};
            string b=Set(a,scalar,kind=="pushback"?"2":"144");
            void Add(string id,O outcome,string left,string right,params string[] rows)=>cases.Add(Pair(kind+"/"+id,kind,rows,outcome,left,right));
            void InputCase(string id,O outcome,I58F1Input input,params string[] rows)=>cases.Add(new I58F1Case(kind+"/"+id,kind,rows,outcome,input,a,id));
            Add("equal",O.Single,a,a,"CT58-01","CT58-21","CT58-25","CT58-26","CT58-28");
            foreach(string view in new[]{"frontal","lateral","planta"})
                InputCase("view-"+view,O.Single,Input(Sibling(kind,a),Sibling(kind,a,"B",Outer,view,2)),"CT58-01");
            InputCase("placement",O.Single,Input(Sibling(kind,a),Sibling(kind,a,"B") with {PlacementX=123}),"CT58-02");
            InputCase("external-context",O.Single,Input(Sibling(kind,a),Sibling(kind,a,"B") with {ExternalContext="catalog-B/variables-B/effective-B"}),"CT58-03");
            Add("scalar-divergence",O.Divergent,a,b,"CT58-04","CT58-14");
            Add("malformed-design",O.Unreadable,a,"{","CT58-05");
            InputCase("malformed-envelope",O.Unreadable,Input(Sibling(kind,a),Sibling(kind,a,"B") with {RawEnvelope="{"}),"CT58-05");
            InputCase("null-input",O.Unreadable,null,"CT58-05","CT58-19");
            InputCase("null-sibling",O.Unreadable,Input(Sibling(kind,a),null),"CT58-05","CT58-19");
            InputCase("empty",O.Unreadable,Input(),"CT58-19");
            InputCase("incomplete",O.Unreadable,Input(Sibling(kind,a)) with {IsComplete=false,CompletenessEvidence=null},"CT58-19");
            InputCase("mixed-outer",O.Unreadable,Input(Sibling(kind,a),Sibling(kind,a,"B",Other)),"CT58-19","CT58-24");
            InputCase("duplicate-source",O.Unreadable,Input(Sibling(kind,a),Sibling(kind,b)),"CT58-19");
            InputCase("first-different",O.Divergent,Input(Sibling(kind,b,"B"),Sibling(kind,a),Sibling(kind,a,"C")),"CT58-10");
            InputCase("majority",O.Divergent,Input(Sibling(kind,a),Sibling(kind,a,"B"),Sibling(kind,a,"C"),Sibling(kind,b,"D")),"CT58-11");
            foreach(var family in new[]{new[]{a,a},new[]{a,b},new[]{a,b,"{"}}) {
                int n=0;foreach(var permutation in Permutations(family))
                    InputCase("perm-"+family.Length+"-"+(family[1]==a?"eq":"diff")+"-"+n++,family.Contains("{")?O.Unreadable:family[1]==a?O.Single:O.Divergent,
                        Input(permutation.Select((v,i)=>Sibling(kind,v,"source-"+i)).ToArray()),"CT58-09",family.Contains("{")?"CT58-17":"CT58-14");
            }
            Add("save-reopen",O.Single,a,Store.Serialize(Store.Deserialize(a)),"CT58-12");
            var wrong=Sibling(kind,a,"B");wrong=wrong with {RawEnvelope=Set(wrong.RawEnvelope,"Kind","\"cama\"")};
            InputCase("wrong-kind",O.Unreadable,Input(Sibling(kind,a),wrong),"CT58-13");
            Add("cross-payload",O.Unreadable,a,Serialize(kind=="cabecera"?"dynamic":"cabecera"),"CT58-13");
            var layers=new List<string>{"SchemaVersion"};
            if(kind!="dynamic")layers.Add(root+".SchemaVersion");
            if(kind is "dynamic" or "pushback")layers.Add(Structure(kind)+".Modules.0.Header.SchemaVersion");
            foreach(string layer in layers) {
                foreach(var (version,row) in new[]{("\"99.0\"","CT58-06"),("\"2.99\"","CT58-07"),("\"bad\"","CT58-08"),("3","CT58-08"),("{}","CT58-08"),("\"999999999999999.0\"","CT58-08"),("\"+1.0\"","CT58-08"),("\"1.0.0\"","CT58-08")})
                    Add("schema-"+layer+"-"+version,O.Unreadable,a,Set(a,layer,version),row,"CT58-27");
            }
            foreach(string version in new[]{"\"99.0\"","\"1.1\"","\"bad\"","1","{}","\"+1.0\"","\"1.0.0\""}) {
                var bad=Sibling(kind,a,"B");bad=bad with {RawEnvelope=Set(bad.RawEnvelope,"SchemaVersion",version)};
                InputCase("envelope-schema-"+version,O.Unreadable,Input(Sibling(kind,a),bad),version=="\"99.0\""?"CT58-06":version=="\"1.1\""?"CT58-07":"CT58-08","CT58-27");
            }
            foreach(string value in new[]{"null","{}","[]"}) {
                Add("unknown-wrapper-"+value,O.Unreadable,a,Set(a,"Future",value),"CT58-15","CT58-27");
                Add("unknown-payload-"+value,O.Unreadable,a,Set(a,root+".Future",value),"CT58-15","CT58-27");
                var bad=Sibling(kind,a,"B");bad=bad with {RawEnvelope=Set(bad.RawEnvelope,"Future",value)};
                InputCase("unknown-envelope-"+value,O.Unreadable,Input(Sibling(kind,a),bad),"CT58-15","CT58-27");
            }
            foreach(string path in Nested(kind)) Add("unknown-"+path,O.Unreadable,a,Set(a,path+".FutureNested","1"),"CT58-16","CT58-27");
            Add("duplicate-member",O.Unreadable,a,a.Insert(1,"\"SchemaVersion\":\"2.0\","),"CT58-18");
            Add("case-collision",O.Unreadable,a,a.Insert(1,"\"schemaversion\":\"2.0\","),"CT58-18");
            string nullable=kind switch {"dynamic"=>"DynamicSystem.PostPeralte","pushback"=>"PushBack.Structure.PostPeralte","cantilever"=>"Cantilever.Line.Name",_=>"Header.PostPeralte"};
            string legacyBase=kind is "dynamic" or "pushback" or "cabecera"?Set(a,nullable,"0"):Set(a,nullable,"null");
            Add("legacy-null",O.Single,legacyBase,Set(legacyBase,nullable,"null"),"CT58-20");
            Add("legacy-absent",O.Single,legacyBase,Remove(legacyBase,nullable),"CT58-20");
            if(kind=="cantilever") {
                foreach(string label in new[]{"A-new","B-library","C-reopen"}) Add("identity-"+label,O.Single,a,a,"CT58-23");
                string restamped=Set(a,"Cantilever.Line.Id","\""+Outer+"\"");Add("identity-D-restamp",O.Single,restamped,restamped,"CT58-23");
                Add("inner-different",O.Divergent,a,Set(a,"Cantilever.Line.Id","\""+Other+"\""),"CT58-24","MM-C01");
                foreach(string id in new[]{"\"00000000-0000-0000-0000-000000000000\"","\"bad\"","null"})Add("inner-invalid-"+id,O.Unreadable,a,Set(a,"Cantilever.Line.Id",id),"CT58-24");
                Add("inner-absent",O.Unreadable,a,Remove(a,"Cantilever.Line.Id"),"CT58-24");
            }
            AddMutations(cases,kind,a);
        }
        Fallback(cases);
        ExtraCases(cases);
        return cases;
    }
    private static IEnumerable<string[]> Permutations(string[] items)
    {
        if(items.Length==0){yield return Array.Empty<string>();yield break;}
        for(int i=0;i<items.Length;i++)foreach(var tail in Permutations(items.Where((_,j)=>j!=i).ToArray()))yield return new[]{items[i]}.Concat(tail).ToArray();
    }
    private static IEnumerable<string> Nested(string k)
    {
        if(k is "dynamic" or "pushback") {
            string s=Structure(k);
            foreach(string p in new[]{"Fronts.0","Fronts.0.Levels.0","Modules.0","Modules.0.Header","Modules.0.Header.LeftPost","Modules.0.Header.RightBasePlate","Modules.0.Header.Horizontals.0","Modules.0.Header.Panels.0","SafetySelections.0","SafetySelections.0.PostSides.0","SafetySelections.0.DesviadorOffCells.0","SafetySelections.0.DefensaPosts.0","SafetySelections.0.GuiaEntradaOffCells.0","SafetySelections.0.ParrillaOffCells.0","SafetySelections.0.BotaPosts.0","SafetySelections.0.TopeOffCells.0","SafetySelections.0.BotaBPosts.0","HeaderLineOverrides.0","HeaderLineOverrides.0.Header","DerivedPostLineOverrides.0"})yield return s+"."+p;
            if(k=="pushback")foreach(string p in new[]{"PushBack.Structure","PushBack.Fronts.0","PushBack.SideB","PushBack.SideB.Fronts.0","PushBack.SideB.FrontConfigs.0","PushBack.Composite","PushBack.Composite.Topologies.0","PushBack.RearTopeOffCells.0"})yield return p;
        } else if(k=="cantilever") {
            foreach(string p in new[]{"Line","Line.StationTopology","Line.StationTopology.ColumnHeight","Line.StationTopology.ColumnBaseTemplate","Line.StationTopology.ColumnBaseTemplate.Base","Line.StationTopology.ColumnBaseTemplate.ColumnBottomPlate","Line.StationTopology.ColumnBaseTemplate.Base.FrontPlate","Line.StationTopology.ColumnBaseTemplate.Base.RearPlate","Line.StationTopology.ColumnBaseTemplate.Base.Gusset","Line.StationTopology.ColumnBaseTemplate.Connection","Line.StationTopology.ColumnBaseTemplate.Connection.Punches","Line.DefaultArmTemplate","Line.DefaultArmTemplate.Body","Line.DefaultArmTemplate.MountingPlate","Line.DefaultArmTemplate.EndPlate","Line.ArmCellOverrides.0","Line.ArmCellOverrides.0.Arm","Line.ArmCellOverrides.0.Arm.Body","Line.ArmCellOverrides.0.Arm.MountingPlate","Line.ArmCellOverrides.0.Arm.EndPlate","Line.Bracing","Line.Bracing.ColdRolled","Line.Bracing.AdvancedPanelSegments.0","Line.PlantaVisibility"})yield return "Cantilever."+p;
        } else foreach(string p in new[]{"LeftPost","RightPost","LeftBasePlate","RightBasePlate","Horizontals.0","Panels.0"})yield return "Header."+p;
    }
    private static void Fallback(List<I58F1Case> cases)
    {
        foreach(string kind in new[]{"dynamic","pushback"}) {
            string Encode(RackCad.Domain.Systems.Dynamic.DynamicRackDesign d)=>kind=="dynamic"?Store.Serialize(RackProject.ForDynamic(d)):Store.Serialize(RackProject.ForPushBack(new RackCad.Domain.Systems.PushBack.PushBackDesign{Structure=d}));
            foreach(double global in new[]{0d,8d})foreach(bool calc in new[]{true,false})foreach(double? p in new double?[]{null,0,7,9}) {
                string a=Encode(Dynamic(global,new[]{p},new[]{calc}));bool invalid=!calc&&!p.HasValue;
                var c=Pair($"{kind}/fallback-g{global}-{calc}-{p?.ToString()??"null"}",kind,new[]{"CT58-29","CT58-31","CT58-32"},invalid?O.Unreadable:O.Single,a,a);
                cases.Add(c);cases.Add(c with {Id=c.Id+"-siblings-reversed",Input=c.Input with {Siblings=c.Input.Siblings.Reverse().ToArray()}});
                if(global==0&&!invalid)foreach(string legacy in new[]{"absent","null"})cases.Add(c with {Id=c.Id+"-legacy-"+legacy,Input=Input(Sibling(kind,a),Sibling(kind,legacy=="absent"?Remove(a,Structure(kind)+".PostPeralte"):Set(a,Structure(kind)+".PostPeralte","null"),"B"))});
            }
            foreach(double global in new[]{0d,8d})foreach(var flags in new[]{new[]{true,true},new[]{true,false},new[]{false,true},new[]{false,false}})foreach(var values in new[]{new double?[]{7,9},new double?[]{9,7},new double?[]{0,7},new double?[]{7,0}}) {
                string a=Encode(Dynamic(global,values,flags));string label=$"{kind}/ordered-{global}-{flags[0]}-{flags[1]}-{values[0]}-{values[1]}";
                cases.Add(Pair(label,kind,new[]{"CT58-29","CT58-30","CT58-31"},O.Single,a,a));
                cases.Add(Pair(label+"-module-permutation",kind,new[]{"CT58-30","MM-D09b"},O.Divergent,a,Reverse(a,Structure(kind)+".Modules")));
            }
            string x=Encode(Dynamic());string y=Encode(Dynamic(0,new double?[]{7,0},new[]{true,true}));
            cases.Add(Pair(kind+"/conservative-second-positive",kind,new[]{"CT58-30","CT58-32","MM-D09c"},O.Divergent,x,y));
        }
        foreach(double global in new[]{0d,8d})foreach(bool calcA in new[]{true,false}) {
            string a=Store.Serialize(RackProject.ForPushBack(Composite(global,calcA)));
            cases.Add(Pair($"pushback/composite-{global}-{calcA}","pushback",new[]{"CT58-29","CT58-30","CT58-31","CT58-32"},O.Single,a,a));
        }
    }
    // Explicit authored mutations; no reflection, property discovery, JSON equality, or source-only oracle.
    private static void AddMutations(List<I58F1Case> cases,string k,string a)
    {
        void M(string row,string path,string value,O expected=O.Divergent) {
            // Fixture stimulus sanity only; this never determines authored outcome.
            if(expected==O.Divergent && At(JsonNode.Parse(a),path)?.ToJsonString()==JsonNode.Parse(value)?.ToJsonString())
                throw new InvalidOperationException("No-op fixture mutation: "+k+"/"+path);
            cases.Add(Pair(k+"/"+row+"/"+path+"/"+value,k,new[]{row},expected,a,Set(a,path,value),path+" -> "+value));
        }
        void R(string row,string path)=>cases.Add(Pair(k+"/"+row+"/reverse-"+path,k,new[]{row},O.Divergent,a,Reverse(a,path)));
        if(k is "dynamic" or "pushback") {
            string s=Structure(k);void D(string row,string path,string v,O e=O.Divergent)=>M(row,s+"."+path,v,e);
            D("MM-D01","PalletDepth","49");D("MM-D01","PalletWeight","1100");D("MM-D01","PalletWeightUnit","\"lb\"");D("MM-D01","BeamDepth","5");D("MM-D01","InOutBeamCatalogId","\"other-beam\"");
            D("MM-D02","Fronts.0.IsActive","false");D("MM-D02","Fronts.0.Levels.0.ClearHeight","61");R("MM-D02",s+".Fronts");
            D("MM-D03","Modules.0.Length","49");D("MM-D03","Modules.0.IsManualOverride","true");D("MM-D03","Modules.0.Notes","\"changed\"");
            D("MM-D04","HeaderLineOverrides.0.Header.LeftPost.ReinforcementHeight","60");D("MM-D04","DerivedPostLineOverrides.0.Height","121");
            D("MM-D05","Fronts.0.BeamLengthOverride","100");D("MM-D05","FirstLevelDatum","0");D("MM-D05","DimensionViews","0");
            D("MM-D06","SafetySelections.0.AuthoredSide","2");D("MM-D06","SafetySelections.0.PostSides.0.PostIndex","1");D("MM-D06","SafetySelections.0.TopeOffCells.0.Level","1");D("MM-D06","SafetySelections.0.BotaBPosts.0.PostIndex","1");D("MM-D06","SafetySelections.0.BotaPieceId","\"boot\"");
            D("MM-D07","Fronts.0.Bfr","999",O.Single);D("MM-D08","PostPeralte","-1",O.Unreadable);
            D("MM-D09b","Modules.0.Header.PostPeralte","9");D("MM-D09c","Modules.0.Header.Name","\"conservative\"");
            foreach(double global in new[]{0d,8d}) {
                string current=Set(a,s+".PostPeralte",global.ToString(System.Globalization.CultureInfo.InvariantCulture));
                current=Set(current,s+".Modules.0.UseCalculatedHeaderConfiguration","true");
                foreach(var (field,value) in new[]{("PostPeralte","9"),("Height","144"),("Name","\"rebuilt\""),("LeftPost.PostCatalogId","\"other\"")})
                    cases.Add(Pair(k+"/calculated-"+global+"-"+field,k,new[]{global>0?"MM-D09a":"MM-D09c","CT58-32"},global>0?O.Single:O.Divergent,current,Set(current,s+".Modules.0.Header."+field,value)));
                string custom=Set(current,s+".Modules.0.UseCalculatedHeaderConfiguration","false");
                cases.Add(Pair(k+"/custom-"+global,k,new[]{"MM-D09d"},O.Divergent,custom,Set(custom,s+".Modules.0.Header.Name","\"custom mutation\"")));
                cases.Add(Pair(k+"/custom-null-"+global,k,new[]{"MM-D09d"},O.Unreadable,custom,Set(custom,s+".Modules.0.Header","null")));
                cases.Add(Pair(k+"/unknown-calculated-"+global,k,new[]{"MM-D09e"},O.Unreadable,current,Set(current,s+".Modules.0.Header.LeftPost.Future","1")));
            }
            D("MM-D10","SafetySelections.0.Side","2",O.Single);D("MM-D10","SafetySelections.0.DerivedAisles","[{\"PostIndex\":0,\"Side\":1}]",O.Unreadable);
        }
        if(k=="pushback") {
            M("MM-P01","PushBack.Structure.PalletFront","43");M("MM-P02","PushBack.Fronts.0.HighEndBeamPeraltes","[4,null]");M("MM-P02","PushBack.Fronts.0.PalletsDeepOverrides","[4,null]");M("MM-P02","PushBack.Fronts.0.DefaultPalletsDeep","6");
            M("MM-P03","PushBack.Fronts.0.DrawPallets.0","true");
            string none=Remove(a,"PushBack.Fronts.0.DrawPallets");cases.Add(Pair(k+"/MM-P03/null-list",k,new[]{"MM-P03"},O.Single,none,Set(none,"PushBack.Fronts.0.DrawPallets","[null,null]")));
            M("MM-P04","PushBack.SideB","null");M("MM-P04","PushBack.SideB.Fronts.0","null");M("MM-P04","PushBack.SideB.FrontConfigs.0.DefaultPalletsDeep","5");
            M("MM-P05","PushBack.Composite.Gap","6");M("MM-P05","PushBack.Composite.CentralSeparator","true");M("MM-P05","PushBack.Composite.StructureOverrideA","6");M("MM-P05","PushBack.Composite.StructureOverrideB","6");M("MM-P05","PushBack.Composite.Topologies.0.Direction","\"BToA\"");M("MM-P05","PushBack.Composite.Topologies.0.CorridaDepth","4");
            M("MM-P06","PushBack.Composite.AbsentSlotsA","[0]");M("MM-P06","PushBack.Composite.AbsentSlotsB","[1]");M("MM-P06","PushBack.RearTopeOffCells.0.Level","1");M("MM-P06","PushBack.DefensePieceId","\"defense\"");M("MM-P06","PushBack.RearTopePieceId","\"tope\"");
            M("MM-P07","PushBack.Composite.DefaultTopology","\"FutureTopology\"",O.Unreadable);M("MM-P07","PushBack.Composite.Topologies.0.Direction","\"FutureDirection\"",O.Unreadable);M("MM-P08","PushBack.SideB.Structure","{}",O.Unreadable);M("MM-P08","PushBack.Composite.Topologies.0.Future","0",O.Unreadable);
        }
        if(k=="cantilever") {
            void C(string row,string path,string v,O e=O.Divergent)=>M(row,"Cantilever.Line."+path,v,e);
            C("MM-C01","Name","\"changed\"");C("MM-C01","StationCount","4");C("MM-C01","ColumnCentreSpacing","97");
            C("MM-C02","StationTopology.FaceMode","1");C("MM-C02","StationTopology.SingleSide","1");C("MM-C02","StationTopology.ColumnHeight.Mode","1");C("MM-C02","StationTopology.ColumnHeight.ManualHeight","180");
            C("MM-C03","DefaultArmTemplate.Body.CutLength","37");C("MM-C03","DefaultArmTemplate.Body.SectionId","\"other-section\"");C("MM-C03","DefaultArmTemplate.Body.Arrangement","1");cases.Add(Pair(k+"/MM-C03/nullable-mounting-offset",k,new[]{"MM-C03"},O.Divergent,Set(a,"Cantilever.Line.DefaultArmTemplate.MountingPlate.VerticalEndOffset","null"),a));
            C("MM-C04","ArmCellOverrides","[]");C("MM-C04","ArmCellOverrides.0.StationIndex","1");C("MM-C04","ArmCellOverrides.0.Side","1");C("MM-C04","ArmCellOverrides.0.Arm.EndPlate.Thickness","0.75");
            C("MM-C05","Bracing.AdvancedPanelSegments.0.EndElevation","40");C("MM-C05","Bracing.AdvancedPanelSegments.0.BracingMode","1");C("MM-C05","Bracing.ManualPanelCount","2");
            C("MM-C06","PlantaVisibility.ShowArms","true");C("MM-C06","PlantaVisibility.ShowBraces","true");
            C("MM-C07","StationTopology.ColumnBaseTemplate.BaseFollowsColumn","false");C("MM-C07","StationTopology.ColumnBaseTemplate.Connection.Punches.Diameter","0.8");
            C("MM-C08","StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset","2",O.Single);C("MM-C08","StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset","5",O.Single);C("MM-C08","IntervalCount","99",O.Single);C("MM-C08","StationTopology.ColumnBaseTemplate.Connection.Punches.FutureMargin","2",O.Unreadable);
        }
        if(k=="cabecera") {
            void H(string row,string path,string v,O e=O.Divergent)=>M(row,"Header."+path,v,e);
            foreach(var (field,value) in new[]{("Height","144"),("Depth","49"),("Units","\"mm\""),("Name","\"changed\""),("StandardBaselineId","\"other\""),("StandardBaselineVersion","\"2\"")})H("MM-H01",field,value);
            H("MM-H02","LeftPost.PostCatalogId","\"other\"");H("MM-H02","RightPost.Description","\"changed\"");H("MM-H02","LeftPost.ReinforcementHeight","60");H("MM-H02","LeftBasePlate.ConnectionPointId","\"other\"");H("MM-H03","LeftBasePlate.PeralteOverride","7");
            H("MM-H04","Horizontals.0.Elevation","13");H("MM-H04","Horizontals.0.ProfileId","\"other\"");H("MM-H04","Horizontals.0.State","1");H("MM-H04","Horizontals.0.Notes","\"note\"");R("MM-H04","Header.Horizontals");
            H("MM-H05","Panels.0.LowerHorizontalId","\"h2\"");H("MM-H05","Panels.0.UpperHorizontalId","\"h1\"");H("MM-H05","Panels.0.Arrangement","1");H("MM-H05","Panels.0.DiagonalDirection","1");H("MM-H05","Panels.0.IsException","true");
            H("MM-H06","PasoTroquel","4");cases.Add(Pair(k+"/MM-H06/legacy-default",k,new[]{"MM-H06","CT58-20"},O.Single,a,Set(a,"Header.PasoTroquel","null")));
            cases.Add(Pair(k+"/MM-H07/external-members",k,new[]{"MM-H07"},O.Single,a,a) with {Input=Input(Sibling(k,a),Sibling(k,a,"B") with {ExternalContext="Members/Exceptions changed outside authored"})});
            H("MM-H07","Members","[]",O.Unreadable);H("MM-H08","LeftPost.Future","1",O.Unreadable);H("MM-H08","Panels.0.Future","1",O.Unreadable);H("MM-H08","SchemaVersion","\"1.1\"",O.Unreadable);
        }
    }
}
