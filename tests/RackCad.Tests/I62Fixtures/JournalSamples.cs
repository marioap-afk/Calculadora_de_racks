#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>Synthetic <c>relay-record/v2</c> journals chained by <c>PrevRelaySha256</c> (B.8.2). Test data only.</summary>
    public static class JournalSamples
    {
        public static JsonObject Record(string phase, string outcome, string status, int open, JsonObject? acceptance = null, string disposition = "NONE",
            long window = 1) => new JsonObject
        {
            ["Schema"] = "rackcad-relay-record/v2", ["TaskId"] = "T-01", ["RunId"] = "R20261003T0" + phase.Length + "0101Z-ab12", ["Attempt"] = 0, ["Phase"] = phase,
            ["WindowSeq"] = window, ["PrevRelaySha256"] = null,
            ["Exit"] = new JsonObject { ["DelegationStatus"] = status, ["OpenDelegations"] = open },
            ["Outcome"] = new JsonObject { ["Kind"] = outcome, ["Acceptance"] = acceptance },
            ["Disposition"] = disposition,
        };

        public static JsonObject Pass(int failing = 0) =>
            new JsonObject(Enumerable.Range(1, 8).Select(i => new KeyValuePair<string, JsonNode?>("A" + i, i == failing ? "fail" : "pass")));

        /// <summary>Serializes the records, chaining each one to the bytes of the previous.</summary>
        public static List<byte[]> Chain(params JsonObject[] records)
        {
            var bytes = new List<byte[]>();
            foreach (var r in records)
            {
                r["PrevRelaySha256"] = bytes.Count == 0 ? null : DelegationJournal.Sha256(bytes[^1]);
                bytes.Add(Encoding.UTF8.GetBytes(r.ToJsonString() + "\n"));
            }

            return bytes;
        }

        public static JsonObject Planning(JsonObject? acceptance, long window = 1) => Record("PLANNING", "COMPLETED", "PLANNED", 0, acceptance, window: window);

        public static JsonObject Work(string outcome = "COMPLETED", string disposition = "NONE", long window = 1) =>
            Record("WORK", outcome, "ACCEPTED_OPEN", 1, disposition: disposition, window: window);

        public static JsonObject Verification(long window = 1) => Record("VERIFICATION", "COMPLETED", "ACCEPTED_OPEN", 1, window: window);
    }
}
