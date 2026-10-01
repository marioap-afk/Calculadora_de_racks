using System.Globalization;
using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

public sealed record InventorySummary(int Count, int Observed, int Unknown, int DistinctTriples, double? Imax, string Status);

/// <summary>
/// HF-T2 (design 2.4, step S7 and 3.4): the scale components of the existing <c>AcDbBlockReference</c>s of the private library copy,
/// as binary64 bit patterns, with <c>I_max</c> = the largest |value - nearest member of {+1, -1}| over the components. No block
/// extents are read (the field is struck). Informative evidence only; it decides no tolerance.
/// </summary>
public static class InventoryBuilder
{
    public const string BlockReferenceClass = "AcDbBlockReference";

    public static double NearestMemberDistance(double v) => Math.Min(Math.Abs(v - 1.0), Math.Abs(v + 1.0));

    public static string NearestMember(double v) => Math.Abs(v - 1.0) <= Math.Abs(v + 1.0) ? "+1" : "-1";

    public static (JsonObject Body, InventorySummary Summary) Build(
        IReadOnlyList<BlockRecordInfo> blocks, string libraryFileSha256, string libraryPath, InstrumentInfo instrument)
    {
        var refs = new List<JsonNode?>();
        var triples = new HashSet<string>(StringComparer.Ordinal);
        double? imax = null;
        int index = 0, observed = 0, unknown = 0;

        foreach (var b in blocks)
        {
            for (var ei = 0; ei < b.Entities.Count; ei++)
            {
                var e = b.Entities[ei];
                // exact class only (BA-05 2.3.1: never a subclass test); an entity whose class could not be read is not known to be a reference
                if (!string.Equals(e.RxClassName, BlockReferenceClass, StringComparison.Ordinal)) continue;

                string status;
                string reason;
                JsonNode? scale = null;
                JsonNode? nearest = null;
                if (e.ReadError is not null) { status = "UNKNOWN"; reason = e.ReadError; }
                else if (e.Scale is null || e.Scale.Count != 3) { status = "UNKNOWN"; reason = "SCALE_NOT_READ"; }
                else if (e.Scale.Any(v => !double.IsFinite(v))) { status = "UNKNOWN"; reason = "NON_FINITE"; }
                else
                {
                    status = "OBSERVED";
                    reason = "";
                    var hex = e.Scale.Select(DoubleBits.Hex).ToList();
                    scale = RecordJson.Strings(hex);
                    nearest = RecordJson.Strings(e.Scale.Select(NearestMember));
                    triples.Add(string.Join("|", hex));
                    foreach (var v in e.Scale)
                    {
                        var d = NearestMemberDistance(v);
                        if (imax is null || d > imax) imax = d;
                    }
                }
                if (status == "OBSERVED") observed++; else unknown++;
                refs.Add(new JsonObject
                {
                    ["index"] = index++,
                    ["containerBlock"] = b.Name,
                    ["entityIndex"] = ei,
                    ["referencedBlock"] = e.ReferencedBlockName ?? "",
                    ["scale"] = scale,
                    ["nearestMember"] = nearest,
                    ["status"] = status,
                    ["reason"] = reason,
                });
            }
        }

        var count = observed + unknown;
        var summaryStatus = count == 0 ? "NOT_OBSERVED" : unknown > 0 ? "UNKNOWN" : "OBSERVED";
        var body = new JsonObject
        {
            ["schema"] = "ct21d.inventory.v1",
            ["governing"] = false,
            ["gate"] = RecordJson.Gate,
            ["identity"] = new JsonObject
            {
                ["libraryFileSha256"] = libraryFileSha256,
                ["libraryPath"] = libraryPath,
                ["instrumentBuild"] = new JsonObject { ["name"] = instrument.Name, ["sha256"] = instrument.Sha256 },
            },
            ["references"] = RecordJson.Arr(refs),
            ["summary"] = new JsonObject
            {
                ["count"] = count,
                ["observed"] = observed,
                ["unknown"] = unknown,
                ["distinctTriples"] = triples.Count,
                ["ImaxHex"] = imax is null ? null : DoubleBits.Hex(imax.Value),
                // a decimal rendering for reading only (design 2.4: "plus a decimal rendering for reading only")
                ["ImaxDecimalForReadingOnly"] = imax?.ToString("R", CultureInfo.InvariantCulture),
                ["status"] = summaryStatus,
            },
        };
        return (body, new InventorySummary(count, observed, unknown, triples.Count, imax, summaryStatus));
    }
}

/// <summary>
/// HF-G3 (I-4): the host type of the value returned by <c>Application.GetSystemVariable(name)</c> for the ten variables of
/// BA-05 V5 section 2.1.3, plus any extra name (recorded the same way, with no expectation: V5:140). The TYPE is the fact; the
/// value is context.
/// </summary>
public static class CtxVarsBuilder
{
    public static (JsonArray Vars, string FactStatus, string Reason) Build(IReadOnlyList<SysVarReading> readings)
    {
        var nodes = new List<JsonNode?>();
        var anyUnknown = false;
        var anyDiffers = false;
        foreach (var r in readings)
        {
            var expected = ContextVariableTable.Find(r.Name);
            string status;
            string reason = "";
            bool? matches = null;
            if (r.Error is not null || r.HostTypeFullName is null)
            {
                status = "UNKNOWN";
                reason = r.Error ?? "NO_VALUE";
                anyUnknown = true;
            }
            else if (expected is null)
            {
                status = "OBSERVED"; // no expectation: the host type is recorded
            }
            else
            {
                matches = string.Equals(r.HostTypeFullName, expected.ExpectedHostType, StringComparison.Ordinal);
                status = matches.Value ? "OBSERVED" : "OBSERVED_DIFFERS";
                if (!matches.Value) anyDiffers = true;
            }
            nodes.Add(new JsonObject
            {
                ["name"] = r.Name,
                ["hostType"] = r.HostTypeFullName,
                ["valueText"] = r.ValueText,
                ["valueBitsHex"] = r.ValueBitsHex,
                ["expectedType"] = expected?.ExpectedHostType,
                ["matches"] = matches,
                ["status"] = status,
                ["reason"] = reason,
            });
        }
        var fact = anyUnknown ? "UNKNOWN" : anyDiffers ? "OBSERVED_DIFFERS" : "OBSERVED";
        return (RecordJson.Arr(nodes), fact, anyUnknown ? "AT_LEAST_ONE_VARIABLE_UNREADABLE" : anyDiffers ? "AT_LEAST_ONE_TYPE_DIFFERS" : "");
    }
}
