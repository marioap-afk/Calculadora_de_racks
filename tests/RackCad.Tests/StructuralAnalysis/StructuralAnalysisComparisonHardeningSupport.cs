using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using D = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the I-65 F4-T6 tests (comparator hardening): the values a read-back carries, a dump of an independent model of the
    /// same column, and a real package with its read-back written into a folder of the temporary area. The values are fixture values, not
    /// engineering values; a test changes a fresh copy, never a shared one.
    /// </summary>
    internal static class StructuralAnalysisComparisonHardeningSupport
    {
        public static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        /// <summary>The values the build would have read from the model of the sample dump <c>D.Valid()</c>, with the keys the build writes.</summary>
        public static JsonObject SampleValues() =>
            new JsonObject
            {
                ["nodes"] = new JsonArray(
                    SampleNode("N1", 11, 0, 0, 0, true, true, true, true, true, true),
                    SampleNode("N2", 12, 0, 100, 0, true, false, false, false, true, true)),
                ["members"] = new JsonArray(SampleMember("M1", 21)),
                ["loads"] = new JsonArray(
                    new JsonObject
                    {
                        ["id"] = "P1",
                        ["member"] = "M1",
                        ["index"] = 0,
                        ["direction"] = "Y",
                        ["value"] = -1000.0,
                        ["percent"] = 25.0,
                        ["isPercentage"] = true,
                    }),
                ["loadCase"] = new JsonObject { ["id"] = "REF1", ["category"] = "OTHER", ["selfWeight"] = D.Numbers(0, 0, 0) },
            };

        public static JsonObject SampleNode(string id, int ramId, double x, double y, double z, params bool[] flags) =>
            new JsonObject { ["id"] = id, ["ramId"] = ramId, ["coordinates"] = D.Numbers(x, y, z), ["restraints"] = D.Bools(flags) };

        public static JsonObject SampleMember(string id, int ramId) =>
            new JsonObject
            {
                ["id"] = id,
                ["ramId"] = ramId,
                ["section"] = "W 12X26",
                ["material"] = "A992 Gr50",
                ["rotation"] = 0.0,
                ["cardinalPoint"] = 0,
                ["offsetI"] = D.Numbers(0, 0, 0),
                ["offsetJ"] = D.Numbers(0, 0, 0),
                ["stiffnessFactors"] = D.Numbers(1, 1, 1, 1),
                ["axialBehavior"] = 0,
                ["springI"] = 0.0,
                ["springJ"] = 0.0,
                ["releases"] = D.Bools(new bool[10]),
                ["l1"] = D.Numbers(0, 1, 0),
                ["l2"] = D.Numbers(-1, 0, 0),
                ["l3"] = D.Numbers(0, 0, 1),
                ["description"] = id,
            };

        /// <summary>
        /// The dump of an INDEPENDENT model of the same column: other ids, other order and other descriptions (the same bytes would be
        /// rejected, A65-F4-03). It agrees with <c>D.Valid()</c> through its geometry only.
        /// </summary>
        public static JsonObject ManualDump()
        {
            var dump = D.Valid();
            dump["nodes"] = new JsonArray(
                D.Node(32, "B", 0, 100, 0, true, false, false, false, true, true),
                D.Node(31, "A", 0, 0, 0, true, true, true, true, true, true));
            dump["members"] = new JsonArray(D.Member(41, "X1", 31, 32, new[] { 0.0, 1.0, 0.0 }, new[] { -1.0, 0.0, 0.0 }, new[] { 0.0, 0.0, 1.0 }));
            dump["loadCases"] = new JsonArray(D.Case("REF1", "OTHER", D.MemberLoads(41, D.Load("Y", -1000, 25, true))));
            return dump;
        }

        /// <summary>A readback document as the build writes it (the full shape), around the given values.</summary>
        public static JsonObject Readback(string status, string fingerprint, string[] differences, JsonObject values) =>
            new JsonObject
            {
                ["schema"] = RamReadbackReader.Schema,
                ["status"] = status ?? RamReadbackReader.StatusPassed,
                ["adapterVersion"] = I.AdapterVersion,
                ["analysisInputFingerprint"] = fingerprint,
                ["differences"] = new JsonArray((differences ?? new string[0]).Select(d => (JsonNode)d).ToArray()),
                ["values"] = values,
            };

        /// <summary>Writes a real package (manifest, script, loads) and a read-back into a NEW folder; returns the fingerprint of the package.</summary>
        public static string WritePackage(string folder, string status = null, string fingerprint = null, string[] differences = null, JsonObject values = null)
        {
            var package = I.Package();
            Directory.CreateDirectory(folder);

            foreach (var file in package.Files)
            {
                File.WriteAllBytes(Path.Combine(folder, file.Name), file.GetContent());
            }

            var readback = Readback(status, fingerprint ?? package.AnalysisInputFingerprint, differences, values ?? SampleValues());
            File.WriteAllText(Path.Combine(folder, RamBuildManifest.ReadbackFileName), readback.ToJsonString(), new UTF8Encoding(false));
            return package.AnalysisInputFingerprint;
        }

        public static string WriteDump(string path, JsonObject dump)
        {
            File.WriteAllText(path, dump.ToJsonString(), new UTF8Encoding(false));
            return path;
        }
    }
}
