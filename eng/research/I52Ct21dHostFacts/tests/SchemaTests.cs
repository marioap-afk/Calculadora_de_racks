using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

public class SchemaTests
{
    private static JsonObject Obj(string json) => (JsonObject)Jcs.ParseStrict(json);

    [Theory]
    [InlineData("ct21d.designation.v1.json")]
    [InlineData("ct21d.hostfact.v1.json")]
    [InlineData("ct21d.census.v1.json")]
    [InlineData("ct21d.inventory.v1.json")]
    [InlineData("ct21d.ctxvars.v1.json")]
    [InlineData("ct21d.machine-label.v1.json")]
    public void Every_embedded_schema_loads_and_uses_only_supported_keywords(string file)
    {
        Assert.NotNull(JsonSchemaLite.LoadEmbedded(file));
    }

    [Fact]
    public void A_keyword_outside_the_supported_subset_makes_the_schema_unusable_instead_of_being_ignored()
    {
        Assert.Throws<SchemaUnsupportedException>(() => new JsonSchemaLite(Obj("{\"type\":\"object\",\"patternProperties\":{}}")));
        Assert.Throws<SchemaUnsupportedException>(() => new JsonSchemaLite(Obj("{\"properties\":{\"a\":{\"uniqueItems\":true}}}")));
    }

    [Fact]
    public void The_validator_enforces_the_keywords_it_supports()
    {
        var schema = new JsonSchemaLite(Obj(@"{
          ""type"":""object"",""additionalProperties"":false,""required"":[""a"",""b""],
          ""properties"":{
            ""a"":{""const"":""x""},
            ""b"":{""type"":""integer"",""minimum"":1,""maximum"":3},
            ""c"":{""type"":""string"",""pattern"":""^[0-9]+$"",""minLength"":2,""maxLength"":3},
            ""d"":{""enum"":[""p"",""q""]},
            ""e"":{""type"":""array"",""minItems"":1,""maxItems"":2,""items"":{""type"":""boolean""}},
            ""f"":{""oneOf"":[{""type"":""string""},{""type"":""null""}]}
          },
          ""allOf"":[{""if"":{""properties"":{""d"":{""const"":""p""}},""required"":[""d""]},""then"":{""required"":[""c""]}}]
        }"));
        Assert.Empty(schema.Validate(Obj("{\"a\":\"x\",\"b\":2,\"c\":\"12\",\"d\":\"p\",\"e\":[true],\"f\":null}")));
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"y\",\"b\":2}")));                       // const
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\"}")));                               // required
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":0}")));                       // minimum
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":4}")));                       // maximum
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1.5}")));                     // integer
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"c\":\"ab\"}")));          // pattern
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"c\":\"1\"}")));           // minLength
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"c\":\"1234\"}")));        // maxLength / pattern
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"d\":\"z\"}")));           // enum
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"e\":[]}")));              // minItems
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"e\":[true,true,true]}"))); // maxItems
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"e\":[1]}")));             // items
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"f\":1}")));               // oneOf
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"zzz\":1}")));             // additionalProperties false
        Assert.NotEmpty(schema.Validate(Obj("{\"a\":\"x\",\"b\":1,\"d\":\"p\"}")));           // if/then: d = p requires c
    }

    private static JsonObject SampleHostFact()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        var tuple = RecordJson.Tuple(d, new InstrumentInfo("I", new string('a', 64), "MATCH"), null, null, false);
        return HostFactRecord.Build("HF-G3", tuple, new JsonObject { ["k"] = 1 }, "OBSERVED", "", Array.Empty<WrittenFile>(), new FakeEnv());
    }

    [Fact]
    public void A_built_host_fact_validates_and_each_mutation_is_rejected()
    {
        var schema = JsonSchemaLite.LoadEmbedded("ct21d.hostfact.v1.json");
        var good = SampleHostFact();
        Assert.Empty(schema.Validate(good));

        JsonObject Mutate(Action<JsonObject> m) { var c = (JsonObject)good.DeepClone(); m(c); return c; }
        Assert.NotEmpty(schema.Validate(Mutate(o => o["governing"] = true)));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["status"] = "PASS")));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["factId"] = "G3")));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["gate"] = "OTHER")));
        Assert.NotEmpty(schema.Validate(Mutate(o => o.Remove("volatile"))));
        Assert.NotEmpty(schema.Validate(Mutate(o => o.Remove("contentSha256"))));
        Assert.NotEmpty(schema.Validate(Mutate(o => ((JsonObject)o["tuple"]!).Remove("TB-S"))));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["extra"] = 1)));
        Assert.NotEmpty(schema.Validate(Mutate(o => ((JsonObject)((JsonObject)o["tuple"]!)["TB-C"]!)["machineClassLabel"] = "MC-XYZ")));
        Assert.NotEmpty(schema.Validate(Mutate(o => ((JsonObject)((JsonObject)o["tuple"]!)["TB-B"]!)["buildTupleDigest"] = "ABC")));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["rawRefs"] = new JsonArray(new JsonObject { ["path"] = "..\\x", ["sha256"] = new string('0', 64) }))));
    }

    [Fact]
    public void Content_hash_excludes_volatile_and_changes_with_anything_else()
    {
        var a = SampleHostFact();
        var b = (JsonObject)a.DeepClone();
        b["volatile"] = new JsonObject { ["capturedUtc"] = "2030-01-01T00:00:00.000Z", ["pid"] = 1, ["processStartUtc"] = "x" };
        Assert.Equal(a["contentSha256"]!.GetValue<string>(), RecordJson.ContentSha256(b));
        b["status"] = "UNKNOWN";
        Assert.NotEqual(a["contentSha256"]!.GetValue<string>(), RecordJson.ContentSha256(b));
    }

    [Fact]
    public void The_designation_must_validate()
    {
        using var t = new TempDir();
        var d = RunnersTestsSupport.Designation(t.Path);
        Assert.Equal(d.RunId, RunDesignation.Parse(RunnersTestsSupport.DesignationJson(d)).RunId);
        var bad = RunnersTestsSupport.DesignationJson(d).Replace("\"attempt\":1", "\"attempt\":0");
        Assert.Throws<FormatException>(() => RunDesignation.Parse(bad));
        Assert.Throws<FormatException>(() => RunDesignation.Parse(RunnersTestsSupport.DesignationJson(d).Replace("ct21d.designation.v1", "x")));
    }
}
