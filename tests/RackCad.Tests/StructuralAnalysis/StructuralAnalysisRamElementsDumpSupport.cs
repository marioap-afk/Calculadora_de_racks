using System;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the I-65 F4-T3 tests: a small VALID dump written as the dump script would write it, in the pair LB_IN and in the
    /// frame of the analysis program (Y up), plus the rendered script of a fixed folder. The values are fixture values, not engineering
    /// values. A test changes a fresh copy, never a shared one.
    /// </summary>
    internal static class StructuralAnalysisRamElementsDumpSupport
    {
        public const string Folder = "C:\\RackCadDumps\\Fixed-Folder";
        public const string Version = "1.0.0-fixture";

        public static string Script(string version = Version, string folder = Folder) =>
            RamDumpScriptTemplate.Render(RamDumpPayload.Create(version, folder));

        public static JsonArray Numbers(params double[] values)
        {
            var array = new JsonArray();

            foreach (var value in values)
            {
                array.Add(JsonValue.Create(value));
            }

            return array;
        }

        public static JsonArray Bools(params bool[] values)
        {
            var array = new JsonArray();

            foreach (var value in values)
            {
                array.Add(JsonValue.Create(value));
            }

            return array;
        }

        public static JsonObject Node(int id, string description, double x, double y, double z, params bool[] flags) =>
            new JsonObject
            {
                ["id"] = id,
                ["description"] = description,
                ["coordinates"] = Numbers(x, y, z),
                ["restraints"] = Bools(flags),
            };

        public static JsonObject Member(
            int id, string description, int nodeI, int nodeJ, double[] l1, double[] l2, double[] l3, double rotation = 0.0) =>
            new JsonObject
            {
                ["id"] = id,
                ["description"] = description,
                ["nodeI"] = nodeI,
                ["nodeJ"] = nodeJ,
                ["section"] = "W 12X26",
                ["material"] = "A992 Gr50",
                ["rotation"] = rotation,
                ["l1"] = Numbers(l1),
                ["l2"] = Numbers(l2),
                ["l3"] = Numbers(l3),
                ["cardinalPoint"] = 0,
                ["offsetI"] = Numbers(0, 0, 0),
                ["offsetJ"] = Numbers(0, 0, 0),
                ["stiffnessFactors"] = Numbers(1, 1, 1, 1),
                ["axialBehavior"] = 0,
                ["springI"] = 0.0,
                ["springJ"] = 0.0,
                ["releases"] = Bools(new bool[10]),
            };

        public static JsonObject Load(string direction, double value, double distance, bool isPercentage) =>
            new JsonObject
            {
                ["direction"] = direction,
                ["value"] = value,
                ["distance"] = distance,
                ["isPercentage"] = isPercentage,
            };

        public static JsonObject Case(string id, string category, params JsonObject[] memberLoads) =>
            new JsonObject
            {
                ["id"] = id,
                ["category"] = category,
                ["selfWeight"] = Numbers(0, 0, 0),
                ["memberLoads"] = new JsonArray(memberLoads.Cast<JsonNode>().ToArray()),
            };

        public static JsonObject MemberLoads(int member, params JsonObject[] loads) =>
            new JsonObject
            {
                ["member"] = member,
                ["loads"] = new JsonArray(loads.Cast<JsonNode>().ToArray()),
            };

        public static JsonObject Combination(string id, params (string Case, double Factor)[] terms) =>
            new JsonObject
            {
                ["id"] = id,
                ["terms"] = new JsonArray(terms.Select(t => (JsonNode)new JsonObject { ["case"] = t.Case, ["factor"] = t.Factor }).ToArray()),
            };

        /// <summary>
        /// A vertical column of 100 in (Y up in the program), fixed at its base, one case with ONE load of -1000 lbf at 25 percent, no
        /// combination. Program ids 11 and 12 for the nodes and 21 for the member.
        /// </summary>
        public static JsonObject Valid() =>
            new JsonObject
            {
                ["schema"] = RamDumpReader.Schema,
                ["status"] = RamDumpScriptTemplate.StatusOk,
                ["adapterVersion"] = Version,
                ["initialUnits"] = "LB_IN",
                ["readUnits"] = "LB_IN",
                ["unitsRestored"] = false,
                ["nodes"] = new JsonArray(
                    Node(11, "N1", 0, 0, 0, true, true, true, true, true, true),
                    Node(12, "N2", 0, 100, 0, true, false, false, false, true, true)),
                ["members"] = new JsonArray(
                    Member(21, "M1", 11, 12, new[] { 0.0, 1.0, 0.0 }, new[] { -1.0, 0.0, 0.0 }, new[] { 0.0, 0.0, 1.0 })),
                ["loadCases"] = new JsonArray(
                    Case("REF1", "OTHER", MemberLoads(21, Load("Y", -1000, 25, true)))),
                ["combinations"] = new JsonArray(),
                ["errors"] = new JsonArray(),
            };

        public static RamDumpReadResult Read(JsonObject dump) => RamDumpReader.Read(dump.ToJsonString());

        public static JsonObject ObjectAt(this JsonNode array, int index) => (JsonObject)((JsonArray)array)[index];

        public static string Codes(RamDumpReadResult result) =>
            string.Join(", ", result.Diagnostics.Select(d => d.Code + "(" + d.Subject + ")"));
    }
}
