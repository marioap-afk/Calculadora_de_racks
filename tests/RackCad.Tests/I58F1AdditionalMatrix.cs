using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Dynamic;
using static RackCad.Tests.I58F1Fixtures;
using O=RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;
namespace RackCad.Tests;
internal static partial class I58F1Matrix
{
    private static void ExtraCases(List<I58F1Case> cases)
    {
        foreach(string k in Kinds) {
            string a=Rich(k);var b=Sibling(k,a,"B");
            cases.Add(new I58F1Case(k+"/envelope-name",k,new[]{"CT58-04"},O.Divergent,Input(Sibling(k,a),b with {RawEnvelope=Set(b.RawEnvelope,"Name","\"other name\"")}),a,"outer authored name"));
            cases.Add(new I58F1Case(k+"/design-disagrees-with-envelope",k,new[]{"CT58-19"},O.Unreadable,Input(Sibling(k,a),b with {RawDesign="{}"}),a,"original raw Design contradicts original envelope"));
            foreach(var (left,right) in new[]{(true,true),(true,false),(false,true)}) {
                string x=left?"{":a,y=right?"{":a;
                cases.Add(Pair(k+"/malformed-all-positions-"+left+right,k,new[]{"CT58-05"},O.Unreadable,x,y));
            }
            foreach(string v in new[]{"\"1.0\"","\"2.0\"","null"})cases.Add(Pair(k+"/known-wrapper-"+v,k,new[]{"CT58-20","CT58-27"},O.Single,a,Set(a,"SchemaVersion",v)));
            cases.Add(Pair(k+"/known-wrapper-absent",k,new[]{"CT58-20"},O.Single,a,Remove(a,"SchemaVersion")));
            string enumPath=k switch {"dynamic"=>"DynamicSystem.Modules.0.Kind","pushback"=>"PushBack.Structure.Modules.0.Kind","cantilever"=>"Cantilever.Line.StationTopology.FaceMode",_=>"Header.Panels.0.Arrangement"};
            cases.Add(Pair(k+"/future-enum",k,new[]{"CT58-18"},O.Unreadable,a,Set(a,enumPath,"999")));
            foreach(string legacy in new[]{"null","absent"}) {
                var legacyEnvelope=b with {RawEnvelope=legacy=="null"?Set(b.RawEnvelope,"SchemaVersion","null"):Remove(b.RawEnvelope,"SchemaVersion")};
                cases.Add(new I58F1Case(k+"/legacy-envelope-"+legacy,k,new[]{"CT58-20","CT58-27"},O.Single,Input(Sibling(k,a),legacyEnvelope),a,"known legacy envelope"));
                if(k!="dynamic")cases.Add(Pair(k+"/legacy-payload-"+legacy,k,new[]{"CT58-20","CT58-27"},O.Single,a,legacy=="null"?Set(a,Root(k)+".SchemaVersion","null"):Remove(a,Root(k)+".SchemaVersion")));
            }
            cases.Add(new I58F1Case(k+"/empty-proof",k,new[]{"CT58-19"},O.Unreadable,Input(Sibling(k,a)) with {CompletenessEvidence=""},a,"completeness flag without accreditation"));
            cases.Add(new I58F1Case(k+"/empty-rack-id",k,new[]{"CT58-19"},O.Unreadable,Input(Sibling(k,a)) with {RackId=""},a,"missing rack identity"));
            // Known calculated subtree remains subject to every raw gate before F-08 exclusion.
            if(k is "dynamic" or "pushback") {
                string p=Structure(k);
                cases.Add(Pair(k+"/line-header-schema",k,new[]{"CT58-27","MM-D09e"},O.Unreadable,a,Set(a,p+".HeaderLineOverrides.0.Header.SchemaVersion","\"1.1\"")));
                foreach(double global in new[]{0d,8d})foreach(bool calc in new[]{true,false}) {
                    string current=Set(Set(a,p+".PostPeralte",global.ToString()),p+".Modules.0.UseCalculatedHeaderConfiguration",calc?"true":"false");
                    foreach(string version in new[]{"\"1.1\"","\"99.0\"","{}"})cases.Add(Pair(k+"/header-schema-g"+global+"-"+calc+version,k,new[]{"CT58-27","CT58-32","MM-D09e"},O.Unreadable,current,Set(current,p+".Modules.0.Header.SchemaVersion",version)));
                }
                foreach(string value in new[]{"0","7"})cases.Add(Pair(k+"/header-absence-vs-"+value,k,new[]{"CT58-29","MM-D09b","MM-D09c"},O.Divergent,Set(a,p+".Modules.0.Header","null"),Set(a,p+".Modules.0.Header.PostPeralte",value)));
                string headerMissing=Remove(a,p+".Modules.0.Header");
                cases.Add(Pair(k+"/legacy-calculated-absent-header",k,new[]{"CT58-20"},O.Single,Set(headerMissing,p+".Modules.0.UseCalculatedHeaderConfiguration","true"),Remove(headerMissing,p+".Modules.0.UseCalculatedHeaderConfiguration")));
                string headerCustom=Set(a,p+".Modules.0.UseCalculatedHeaderConfiguration","false");
                cases.Add(Pair(k+"/legacy-custom-present-header",k,new[]{"CT58-20"},O.Single,headerCustom,Remove(headerCustom,p+".Modules.0.UseCalculatedHeaderConfiguration")));
            }
        }
        // Add/remove/reorder variants, with positional holes distinguished from removal.
        foreach(string k in new[]{"dynamic","pushback"}) {
            string a=Rich(k),p=Structure(k);
            cases.Add(Pair(k+"/front-count",k,new[]{"MM-D02"},O.Divergent,a,Set(a,p+".Fronts","["+At(System.Text.Json.Nodes.JsonNode.Parse(a),p+".Fronts.0").ToJsonString()+"]")));
        }
        string pb=Rich("pushback");string holes=Set(pb,"PushBack.SideB.Fronts.0","null");
        cases.Add(Pair("pushback/sideB-hole-position","pushback",new[]{"MM-P04","CT58-26"},O.Divergent,holes,Reverse(holes,"PushBack.SideB.Fronts")));
        foreach(string path in new[]{"Cantilever.Line.ArmCellOverrides","Header.Horizontals"}) {
            string k=path.StartsWith("Header")?"cabecera":"cantilever",a=Rich(k),row=k=="cabecera"?"MM-H04":"MM-C04";
            var array=(System.Text.Json.Nodes.JsonArray)At(System.Text.Json.Nodes.JsonNode.Parse(a),path);array.Add(array[0].DeepClone());
            string expanded=Set(a,path,array.ToJsonString());
            cases.Add(Pair(k+"/added-item",k,new[]{row},O.Divergent,a,expanded));
            cases.Add(Pair(k+"/removed-item",k,new[]{row},O.Divergent,a,Set(a,path,"[]")));
            if(k=="cantilever") {expanded=Set(expanded,path+".1.StationIndex","1");cases.Add(Pair(k+"/reordered-overrides",k,new[]{row},O.Divergent,expanded,Reverse(expanded,path)));}
        }
        foreach(string path in new[]{"ColumnBottomPlate.Thickness","Base.FrontPlate.Thickness","Base.RearPlate.Thickness","Base.Gusset.Thickness"}) {
            string a=Rich("cantilever");cases.Add(Pair("cantilever/plate-"+path,"cantilever",new[]{"MM-C07"},O.Divergent,a,Set(a,"Cantilever.Line.StationTopology.ColumnBaseTemplate."+path,"0.75")));
        }
        // Composite matrix exercises local A/B fallback separately, not just the rack-wide first positive.
        foreach(double global in new[]{0d,8d})foreach(var flags in new[]{new[]{true,true},new[]{true,false},new[]{false,true},new[]{false,false}})
        foreach(var values in new[]{new double?[]{null,0},new double?[]{7,9},new double?[]{9,7},new double?[]{0,7},new double?[]{7,0}}) {
            var d=Composite(global);foreach(var m in d.Structure.Modules.Where(m=>m.IsHeader)) {
                int side=m.ModuleId.StartsWith("B:",StringComparison.Ordinal)?1:0;m.UseCalculatedHeaderConfiguration=flags[side];m.HeaderConfiguration=values[side].HasValue?Header(values[side].Value):null;
            }
            bool bad=d.Structure.Modules.Any(m=>m.IsHeader&&!m.UseCalculatedHeaderConfiguration&&m.HeaderConfiguration==null);
            string a=Store.Serialize(RackProject.ForPushBack(d));
            // Preserve deliberately contradictory custom+null RAW after the writer's legacy repair.
            if(bad)for(int i=0;i<d.Structure.Modules.Count;i++)if(d.Structure.Modules[i].IsHeader&&!d.Structure.Modules[i].UseCalculatedHeaderConfiguration&&d.Structure.Modules[i].HeaderConfiguration==null)
                a=Set(a,"PushBack.Structure.Modules."+i+".UseCalculatedHeaderConfiguration","false");
            string id=$"pushback/composite-full-{global}-{flags[0]}-{flags[1]}-{values[0]}-{values[1]}";
            var c=Pair(id,"pushback",new[]{"CT58-29","CT58-30","CT58-31","CT58-32"},bad?O.Unreadable:O.Single,a,a);cases.Add(c);
            cases.Add(c with {Id=id+"-siblings-reversed",Input=c.Input with {Siblings=c.Input.Siblings.Reverse().ToArray()}});
            if(!bad) {
                cases.Add(Pair(id+"-modules-reversed","pushback",new[]{"CT58-30"},O.Divergent,a,Reverse(a,"PushBack.Structure.Modules")));
                if(global==0)foreach(string legacy in new[]{"null","absent"})cases.Add(Pair(id+"-global-"+legacy,"pushback",new[]{"CT58-29"},O.Single,a,legacy=="null"?Set(a,"PushBack.Structure.PostPeralte","null"):Remove(a,"PushBack.Structure.PostPeralte")));
            }
        }
    }
}
