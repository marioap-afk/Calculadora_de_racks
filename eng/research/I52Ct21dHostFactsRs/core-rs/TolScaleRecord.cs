using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

public sealed class TolScaleChecks
{
    public int DbmodBefore { get; }
    public int DbmodAfter { get; }
    public bool PrivateFilesUnchanged { get; }
    public bool OtherAcadProcess { get; }
    public bool ManualInput { get; }

    public TolScaleChecks(int dbmodBefore, int dbmodAfter, bool privateFilesUnchanged, bool otherAcadProcess, bool manualInput)
    {
        DbmodBefore = dbmodBefore;
        DbmodAfter = dbmodAfter;
        PrivateFilesUnchanged = privateFilesUnchanged;
        OtherAcadProcess = otherAcadProcess;
        ManualInput = manualInput;
    }
}

/// <summary>The probe-table file (<c>tolscale-probe-table.json</c>): the 147 rows with their bit patterns, so that <c>design.probeTableSha256</c> is checkable.</summary>
public static class TolScaleProbeTableFile
{
    public static JsonObject Build(IReadOnlyList<ProbeRow> rows)
    {
        var arr = new JsonArray();
        foreach (var r in rows)
            arr.Add(new JsonObject
            {
                ["id"] = r.Id, ["family"] = r.Family, ["signPattern"] = r.SignPattern, ["component"] = r.Component,
                ["k"] = r.K is { } k ? k : null, ["inScope"] = r.InScope, ["intended"] = RecordJson.Strings(r.Intended),
            });
        return new JsonObject { ["schema"] = "ct21d.tolscale-probe-table.v1", ["rows"] = arr };
    }

    public static string ToFileText(JsonObject table) => Jcs.Serialize(table) + "\n";

    public static string Sha256(JsonObject table) => Sha256Hex.OfUtf8(ToFileText(table));
}

/// <summary>Builds <c>tolscale-raw.json</c> exactly as the design 2.4 schema (<c>ct21d.tolscale.v1</c>, closed) defines it.</summary>
public static class TolScaleRecord
{
    private static JsonNode? NullableHex(double? v) => v is null ? null : JsonValue.Create(DoubleBits.Hex(v.Value));

    private static JsonNode? NullableStrings(IReadOnlyList<string>? list) => list is null ? null : RecordJson.Strings(list);

    public static JsonObject Build(
        RsDesignation d, InstrumentInfo instrument, IRunEnvironment env, IReadOnlyList<TolScaleRowResult> rows, ProbeStatisticsResult stats,
        TolScaleChecks checks, string result, string probeTableSha256)
    {
        var rowArray = new JsonArray();
        foreach (var r in rows)
        {
            var o = new JsonObject
            {
                ["id"] = r.Row.Id,
                ["family"] = r.Row.Family,
                ["signPattern"] = r.Row.SignPattern,
                ["component"] = r.Row.Component,
                ["k"] = r.Row.K is { } k ? k : null,
                ["inScope"] = r.Row.InScope,
                ["intended"] = RecordJson.Strings(r.Row.Intended),
                ["op1"] = NullableStrings(r.Op1),
                ["op2"] = NullableStrings(r.Op2),
                ["rotationHex"] = r.RotationHex,
                ["normal"] = NullableStrings(r.NormalHex),
                ["deltaOp1"] = NullableStrings(r.DeltaOp1),
                ["deltaOp2"] = NullableStrings(r.DeltaOp2),
                ["deltaExact"] = r.DeltaExact,
                ["bitIdentical"] = r.BitIdentical,
                ["witness"] = r.Witness,
                ["status"] = r.Observed ? "OBSERVED" : "UNKNOWN",
                ["reason"] = r.Reason,
            };
            rowArray.Add(o);
        }

        var record = new JsonObject
        {
            ["schema"] = "ct21d.tolscale.v1",
            ["governing"] = false,
            ["gate"] = RecordJson.Gate,
            ["tuple"] = new JsonObject
            {
                ["machineClassLabel"] = d.MachineClassLabel,
                ["buildTupleDigest"] = d.BuildTupleDigest,
                ["sessionId"] = d.SessionId,
                ["runId"] = d.RunId,
                ["attempt"] = d.Attempt,
                ["instrument"] = new JsonObject
                {
                    ["name"] = instrument.Name,
                    ["sha256"] = instrument.Sha256,
                    ["packageManifestSha256"] = d.PackageManifestSha256,
                    ["declaredSetSha256"] = d.DeclaredSetSha256,
                },
            },
            ["design"] = new JsonObject { ["designBlob"] = d.DesignBlob, ["ba05Blob"] = d.Ba05Blob, ["probeTableSha256"] = probeTableSha256 },
            ["rows"] = rowArray,
            ["summary"] = new JsonObject
            {
                ["rowsExpected"] = 147,
                ["rowsObserved"] = rows.Count(r => r.Observed),
                ["inScopeRows"] = 135,
                ["DA_op1"] = NullableHex(stats.DaOp1),
                ["DA_op2"] = NullableHex(stats.DaOp2),
                ["DA"] = NullableHex(stats.Da),
                ["DA_char"] = NullableHex(stats.DaChar),
                ["Kpres"] = stats.Kpres,
                ["bitLevelSignedZeroRows"] = stats.BitLevelSignedZeroRows,
            },
            ["checks"] = new JsonObject
            {
                ["dbmodBefore"] = checks.DbmodBefore,
                ["dbmodAfter"] = checks.DbmodAfter,
                ["privateFilesUnchanged"] = checks.PrivateFilesUnchanged,
                ["otherAcadProcess"] = checks.OtherAcadProcess,
                ["manualInput"] = checks.ManualInput,
            },
            ["result"] = result,
            ["decisionEligible"] = result == TolScaleResult.Pass,
            ["volatile"] = new JsonObject
            {
                ["capturedUtc"] = env.UtcNow.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture),
                ["pid"] = env.ProcessId,
            },
        };
        RsSchemas.ValidateOrThrow("ct21d.tolscale.v1.json", record);
        return record;
    }

    /// <summary>SHA-256 of the canonical record WITHOUT <c>volatile</c> (the design: "the file's contentSha256 excludes volatile"). The closed schema has no field for it, so it is reported beside the record.</summary>
    public static string ContentSha256(JsonObject record)
    {
        var clone = (JsonObject)record.DeepClone();
        clone.Remove("volatile");
        return Sha256Hex.Of(Jcs.SerializeUtf8(clone));
    }
}

/// <summary>
/// The OFFLINE recomputation of design 2.4 ("recomputed offline by the pure core library; a mismatch is INVALID evidence"). It is a SECOND,
/// independent implementation of the statistics, written from the definitions of design 2.3 over the rows of the record only (it shares no
/// code with <see cref="ProbeStatistics"/>), plus the structural checks that the closed schema cannot express.
/// </summary>
public static class TolScaleOfflineVerifier
{
    public static IReadOnlyList<string> Verify(string json)
    {
        var problems = new List<string>();
        JsonNode node;
        try { node = Jcs.ParseStrict(json); }
        catch (Exception ex) when (ex is FormatException or System.Text.Json.JsonException) { return new[] { "NOT_JSON:" + ex.Message }; }
        var errors = RsSchemas.Load("ct21d.tolscale.v1.json").Validate(node);
        if (errors.Count > 0) return errors.Take(10).Select(e => "SCHEMA:" + e).ToList();

        var o = (JsonObject)node;
        var rows = (JsonArray)o["rows"]!;
        var summary = (JsonObject)o["summary"]!;

        double? daOp1 = 0.0, daOp2 = 0.0, daChar = 0.0, daPf1 = 0.0; // daPf1: the PF-1 in-scope rows only (design 2.3 states the equivalence 'for these rows')
        var observed = 0;
        var signedZero = 0;
        var levelOk = new SortedDictionary<int, bool>();
        foreach (var level in ProbeTable.LadderLevels) levelOk[level] = true;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var inScope = 0;
        foreach (var n in rows)
        {
            var row = (JsonObject)n!;
            if (!ids.Add(row["id"]!.GetValue<string>())) problems.Add("DUPLICATE_ROW_ID:" + row["id"]!.GetValue<string>());
            var isIn = row["inScope"]!.GetValue<bool>();
            if (isIn) inScope++;
            var family = row["family"]!.GetValue<string>();
            int? k = row["k"] is JsonNode kn ? kn.GetValue<int>() : null;
            var status = row["status"]!.GetValue<string>();
            var intended = row["intended"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
            if (status != "OBSERVED")
            {
                if (isIn) { daOp1 = null; daOp2 = null; if (family == "PF1") daPf1 = null; } else daChar = null;
                if (family == "PF1" && isIn && k is { } kk) levelOk[kk] = false;
                continue;
            }
            observed++;
            var op1 = row["op1"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
            var op2 = row["op2"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
            var dmax1 = 0.0;
            var dmax2 = 0.0;
            for (var i = 0; i < 3; i++)
            {
                var a = BitConverter.UInt64BitsToDouble(Convert.ToUInt64(intended[i], 16));
                var b1 = BitConverter.UInt64BitsToDouble(Convert.ToUInt64(op1[i], 16));
                var b2 = BitConverter.UInt64BitsToDouble(Convert.ToUInt64(op2[i], 16));
                if (Math.Abs(b1 - a) > dmax1) dmax1 = Math.Abs(b1 - a);
                if (Math.Abs(b2 - a) > dmax2) dmax2 = Math.Abs(b2 - a);
            }
            var identical = intended.SequenceEqual(op1) && intended.SequenceEqual(op2);
            if (row["bitIdentical"]!.GetValue<bool>() != identical) problems.Add("ROW_BITIDENTICAL_MISMATCH:" + row["id"]!.GetValue<string>());
            if (!identical)
            {
                var sameNumbers = true;
                for (var i = 0; i < 3; i++)
                {
                    var a = BitConverter.UInt64BitsToDouble(Convert.ToUInt64(intended[i], 16));
                    var b1 = BitConverter.UInt64BitsToDouble(Convert.ToUInt64(op1[i], 16));
                    var b2 = BitConverter.UInt64BitsToDouble(Convert.ToUInt64(op2[i], 16));
                    if (a != b1 || a != b2) sameNumbers = false;
                }
                if (sameNumbers) signedZero++;
            }
            if (isIn)
            {
                if (daOp1 is not null && dmax1 > daOp1) daOp1 = dmax1;
                if (daOp2 is not null && dmax2 > daOp2) daOp2 = dmax2;
                if (family == "PF1" && daPf1 is not null) daPf1 = Math.Max(daPf1.Value, Math.Max(dmax1, dmax2));
            }
            else if (daChar is not null)
            {
                daChar = Math.Max(daChar.Value, Math.Max(dmax1, dmax2));
            }
            if (family == "PF1" && isIn && k is { } level && !identical) levelOk[level] = false;
        }

        var kpres = 0;
        foreach (var pair in levelOk) // ascending k: coarse to fine
        {
            if (!pair.Value) break;
            kpres = pair.Key;
        }
        double? da = daOp1 is null || daOp2 is null ? null : Math.Max(daOp1.Value, daOp2.Value);

        if (rows.Count != 147) problems.Add("ROW_COUNT");
        if (inScope != 135) problems.Add("IN_SCOPE_COUNT=" + inScope);
        void Compare(string field, double? expected)
        {
            var actual = summary[field] is JsonNode s ? s.GetValue<string>() : null;
            var exp = expected is null ? null : DoubleBits.Hex(expected.Value);
            if (!string.Equals(actual, exp, StringComparison.Ordinal)) problems.Add("SUMMARY_MISMATCH:" + field);
        }
        Compare("DA_op1", daOp1);
        Compare("DA_op2", daOp2);
        Compare("DA", da);
        Compare("DA_char", daChar);
        if (summary["Kpres"]!.GetValue<int>() != kpres) problems.Add("SUMMARY_MISMATCH:Kpres");
        if (summary["rowsObserved"]!.GetValue<int>() != observed) problems.Add("SUMMARY_MISMATCH:rowsObserved");
        if (summary["bitLevelSignedZeroRows"]!.GetValue<int>() != signedZero) problems.Add("SUMMARY_MISMATCH:bitLevelSignedZeroRows");
        if (daPf1 is not null && (daPf1 == 0.0) != (kpres == 52)) problems.Add("EQUIVALENCE_DA_ZERO_IFF_KPRES_52_FAILS");
        var result = o["result"]!.GetValue<string>();
        if (result == TolScaleResult.Pass && observed != 147) problems.Add("PASS_WITH_UNOBSERVED_ROWS");
        if (o["decisionEligible"]!.GetValue<bool>() != (result == TolScaleResult.Pass)) problems.Add("DECISION_ELIGIBLE_MISMATCH");
        return problems;
    }
}
