using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;

// Discovery probe only: no candidate carrier or comparator implementation is introduced.
// Generic I-57 ports accept this test-local carrier but currently ignore every input.
// RED is the desired assertion failing against the current unsupported baseline, not F1 completion.
var store = new RackProjectStore();
var id = "11111111-1111-1111-1111-111111111111";
var dynamicDesign = new DynamicRackDesign { Pallet = new PalletSpecification(40, 48, 50, 1000, "kg"), PalletsDeep = 4 };
dynamicDesign.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2 });
var header = new RackFrameProjectDocument { Name = "H", Height = 132, Depth = 42,
    LeftPost = new PostDocument { PostCatalogId = "POSTE_OMEGA_3X3" },
    RightPost = new PostDocument { PostCatalogId = "POSTE_OMEGA_3X3" } }.ToConfiguration();
dynamicDesign.Modules.Add(new DynamicRackModuleDesign { ModuleId = "H1", Kind = DynamicRackModuleKind.HeaderStart, Length = 48, HeaderConfiguration = header });
var line = new CantileverLineDesign { Id = Guid.Parse(id), Name = "C", StationCount = 3,
    StationTopology = new CantileverLineStationTopologyDesign {
        ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign {
            ColumnSectionId = "AISC-W-W10X33", Base = new CantileverBaseDesign { SectionId = "AISC-W-W12X26", Length = 48 } } },
    DefaultArmTemplate = new CantileverArmTemplateDesign { Body = new CantileverArmBodyDesign { SectionId = "AISC-HSS-RECT-HSS4X4X_250", CutLength = 36 } } };
var failed = 0; var selected = 0;
Run("dynamic", store.Serialize(RackProject.ForDynamic(dynamicDesign)), "DynamicSystem", "PalletDepth", 49,
    RackAuthoredComparatorPorts.Dynamic<ProbeInput, DynamicRackSystemDocument>());
Run("pushback", store.Serialize(RackProject.ForPushBack(new PushBackDesign { Structure = dynamicDesign })), "PushBack", "RearTopeSaque", 2,
    RackAuthoredComparatorPorts.PushBack<ProbeInput, PushBackDesignDocument>());
Run("cantilever", store.Serialize(RackProject.ForCantilever(line)), "Cantilever", "Line.ColumnCentreSpacing", 50,
    RackAuthoredComparatorPorts.Cantilever<ProbeInput, CantileverLineDesign>());
Run("cabecera", store.Serialize(RackProject.ForSelective(header)), "Header", "Height", 144,
    RackAuthoredComparatorPorts.Cabecera<ProbeInput, RackFrameProjectDocument>());
Console.WriteLine($"SELECTED={selected}; EXPECTATION_FAILURES={failed}; BASELINE=UNSUPPORTED; NOT_F1_GATE_EVIDENCE");
return selected == 0 ? 2 : failed == 0 ? 0 : 1;

void Run<T>(string kind, string serialized, string payload, string field, double changedValue, IRackAuthoredComparatorPort<ProbeInput,T> port)
{
    // Verify the fixture goes through the actual store and actual reopen path, before any comparison.
    var reopened = store.Serialize(store.Deserialize(serialized));
    Console.WriteLine($"FIXTURE {kind}: real store save/reopen OK");
    RackEmbedDocument E(string design) => new() { Kind = kind, Id = id, Name = kind, View = "frontal", Design = design };
    var a = E(serialized); var same = E(serialized); same.View = "planta";
    var root = JsonNode.Parse(serialized); var target = root[payload]; var segments = field.Split('.');
    foreach (var part in segments[..^1]) target = target[part];
    target[segments[^1]] = changedValue;
    var different = E(root.ToJsonString());
    void Check(string scenario, RackAuthoredComparisonOutcome expected, params RackEmbedDocument[] siblings)
    {
        selected++;
        var input = new ProbeInput(id, siblings, scenario == "placement" ? 100 : 0, scenario == "effective-context" ? "catalog-B" : "catalog-A");
        var result = port.Compare(input);
        var nullOk = result.Outcome == RackAuthoredComparisonOutcome.Single ? result.Authored != null : result.Authored == null;
        var ok = result.Outcome == expected && nullOk;
        if (!ok) failed++;
        Console.WriteLine($"{(ok ? "PASS" : "RED")} {kind}/{scenario} expected={expected} actual={result.Outcome} authoredNull={result.Authored == null}");
    }
    Check("view", RackAuthoredComparisonOutcome.Single, a, same);
    Check("placement", RackAuthoredComparisonOutcome.Single, a, same);
    Check("effective-context", RackAuthoredComparisonOutcome.Single, a, same);
    Check("authored-mutation", RackAuthoredComparisonOutcome.Divergent, a, different);
    Check("malformed-sibling", RackAuthoredComparisonOutcome.Unreadable, a, E("{"));
    var future = JsonNode.Parse(serialized); future["SchemaVersion"] = "99.0";
    Check("future-major", RackAuthoredComparisonOutcome.Unreadable, a, E(future.ToJsonString()));
    future["SchemaVersion"] = "2.99";
    Check("future-minor", RackAuthoredComparisonOutcome.Unreadable, a, E(future.ToJsonString()));
    future["SchemaVersion"] = "bad";
    Check("malformed-version", RackAuthoredComparisonOutcome.Unreadable, a, E(future.ToJsonString()));
    Check("permuted", RackAuthoredComparisonOutcome.Single, same, a);
    Check("first-different", RackAuthoredComparisonOutcome.Divergent, different, a, same);
    Check("majority-same", RackAuthoredComparisonOutcome.Divergent, a, same, different);
    Check("save-reopen", RackAuthoredComparisonOutcome.Single, a, E(reopened));
    var wrong = E(serialized); wrong.Kind = "cama";
    Check("wrong-kind", RackAuthoredComparisonOutcome.Unreadable, a, wrong);
    var unknown = JsonNode.Parse(serialized); unknown[payload]["FutureAuthored"] = 1;
    Check("unknown-payload", RackAuthoredComparisonOutcome.Unreadable, a, E(unknown.ToJsonString()));
    var nested = JsonNode.Parse(serialized);
    var nestedObject = kind switch { "dynamic" => nested[payload]["Fronts"][0], "pushback" => nested[payload]["Structure"], "cantilever" => nested[payload]["Line"]["StationTopology"], _ => nested[payload]["LeftPost"] };
    nestedObject["FutureNestedIntent"] = 1;
    Check("unknown-nested", RackAuthoredComparisonOutcome.Unreadable, a, E(nested.ToJsonString()));
    var reopenedUnknown = JsonNode.Parse(store.Serialize(store.Deserialize(unknown.ToJsonString())));
    var reopenedNested = JsonNode.Parse(store.Serialize(store.Deserialize(nested.ToJsonString())));
    var observedNested = kind switch { "dynamic" => reopenedNested[payload]["Fronts"][0], "pushback" => reopenedNested[payload]["Structure"], "cantilever" => reopenedNested[payload]["Line"]["StationTopology"], _ => reopenedNested[payload]["LeftPost"] };
    Console.WriteLine($"OBSERVED {kind}: payloadUnknownPreserved={reopenedUnknown[payload]["FutureAuthored"] != null}; nestedUnknownPreserved={observedNested["FutureNestedIntent"] != null}");
    if (kind == "cantilever")
        Console.WriteLine($"OBSERVED cantilever: IntervalCountSerialized={JsonNode.Parse(serialized)[payload]["Line"]["IntervalCount"] != null}");
    Check("unreadable-precedence", RackAuthoredComparisonOutcome.Unreadable, a, different, E("{"));
    Check("unreadable-precedence-permuted", RackAuthoredComparisonOutcome.Unreadable, E("{"), different, a);
}
sealed record ProbeInput(string RackId, IReadOnlyList<RackEmbedDocument> Siblings, double PlacementX, string EffectiveContext);
