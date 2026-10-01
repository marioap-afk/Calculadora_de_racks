using System.Globalization;
using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

/// <summary>Shared pieces of every record: tuple bindings (design 3.1), the volatile part and the content hash.</summary>
public static class RecordJson
{
    public const string Gate = "HOST_GATE_BA11_3_3";

    public static JsonArray Arr(IEnumerable<JsonNode?> items)
    {
        var a = new JsonArray();
        foreach (var i in items) a.Add(i);
        return a;
    }

    public static JsonArray Strings(IEnumerable<string> items) => Arr(items.Select(s => (JsonNode?)JsonValue.Create(s)));

    /// <summary>
    /// TB-C, TB-B, TB-S, TB-L, TB-R. <c>pid</c> and <c>processStartUtc</c> are in <c>volatile</c>, not in TB-S, so that the hashed
    /// part holds no value that changes with the process (design 5.1, "no timestamps inside the hashed part").
    /// </summary>
    public static JsonObject Tuple(RunDesignation d, InstrumentInfo instrument, string? privateBefore, string? privateAfter, bool privateCopyApplies)
    {
        var tb = new JsonObject();
        if (privateCopyApplies)
        {
            tb["privateCopyPath"] = d.PrivateCopyPath;
            if (privateBefore is not null) tb["privateCopySha256Before"] = privateBefore;
            if (privateAfter is not null) tb["privateCopySha256After"] = privateAfter;
            tb["libraryPath"] = d.LibraryPath;
            tb["libraryFileSha256"] = d.LibraryFileSha256;
        }
        else
        {
            tb["notApplicable"] = true;
        }
        return new JsonObject
        {
            ["TB-C"] = new JsonObject { ["machineClassLabel"] = d.MachineClassLabel },
            ["TB-B"] = new JsonObject
            {
                ["buildTupleDigest"] = d.BuildTupleDigest,
                ["packageManifestSha256"] = d.PackageManifestSha256,
                ["declaredSetSha256"] = d.DeclaredSetSha256,
                ["instrument"] = new JsonObject { ["name"] = instrument.Name, ["sha256"] = instrument.Sha256, ["selfPinStatus"] = instrument.SelfPinStatus },
            },
            ["TB-S"] = new JsonObject { ["sessionId"] = d.SessionId, ["runId"] = d.RunId, ["attempt"] = d.Attempt },
            ["TB-L"] = tb,
            ["TB-R"] = new JsonObject { ["designBlob"] = d.DesignBlob, ["ba05Blob"] = d.Ba05Blob },
        };
    }

    public static JsonObject Volatile(IRunEnvironment env) => new()
    {
        ["capturedUtc"] = env.UtcNow.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
        ["pid"] = env.ProcessId,
        ["processStartUtc"] = env.ProcessStartUtc,
    };

    /// <summary>SHA-256 of the canonical (RFC 8785) serialization of the record WITHOUT <c>volatile</c> and <c>contentSha256</c>.</summary>
    public static string ContentSha256(JsonObject record)
    {
        var clone = (JsonObject)record.DeepClone();
        clone.Remove("volatile");
        clone.Remove("contentSha256");
        return Sha256Hex.Of(Jcs.SerializeUtf8(clone));
    }

    /// <summary>Adds <c>volatile</c> and <c>contentSha256</c> (in that order of meaning: the hash excludes both).</summary>
    public static JsonObject Seal(JsonObject record, IRunEnvironment env)
    {
        record["volatile"] = Volatile(env);
        record["contentSha256"] = ContentSha256(record);
        return record;
    }

    /// <summary>Serializes a sealed record as the bytes of a result file: canonical JSON plus one LF.</summary>
    public static string ToFileText(JsonObject record) => Jcs.Serialize(record) + "\n";

    public static void ValidateOrThrow(string schemaFile, JsonObject record)
    {
        var errors = JsonSchemaLite.LoadEmbedded(schemaFile).Validate(record);
        if (errors.Count > 0)
            throw new InvalidOperationException(schemaFile + " validation failed: " + string.Join("; ", errors.Take(10)));
    }
}

/// <summary>The common host-fact record <c>ct21d.hostfact.v1</c> (design 3.1).</summary>
public static class HostFactRecord
{
    public static JsonObject Build(
        string factId,
        JsonObject tuple,
        JsonObject observation,
        string status,
        string reason,
        IEnumerable<WrittenFile> rawRefs,
        IRunEnvironment env)
    {
        var record = new JsonObject
        {
            ["schema"] = "ct21d.hostfact.v1",
            ["factId"] = factId,
            ["gate"] = RecordJson.Gate,
            ["governing"] = false,
            ["tuple"] = tuple.DeepClone(),
            ["observation"] = observation,
            ["status"] = status,
            ["reason"] = reason,
            ["rawRefs"] = RecordJson.Arr(rawRefs.Select(r => (JsonNode?)new JsonObject { ["path"] = r.Name, ["sha256"] = r.Sha256 })),
        };
        RecordJson.Seal(record, env);
        RecordJson.ValidateOrThrow("ct21d.hostfact.v1.json", record);
        return record;
    }
}
