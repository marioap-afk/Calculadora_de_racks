using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace I52Auth15.HostHarness
{
    internal static class Outcome
    {
        public const string Pass = "PASS";
        public const string Fail = "FAIL";
        public const string Unknown = "UNKNOWN";
        public const string NotRun = "NOT_RUN";
    }

    internal sealed class Assertion
    {
        public string Name { get; set; }

        public string Expected { get; set; }

        public string Observed { get; set; }

        public string Result { get; set; }
    }

    /// <summary>One HV case: its assertions and the verdict they imply. A case never invents a PASS.</summary>
    internal sealed class CaseRecord
    {
        public CaseRecord(string id, string family, string expected)
        {
            Id = id;
            Family = family;
            Expected = expected;
        }

        public string Id { get; }

        public string Family { get; }

        public string Expected { get; }

        public string Observed { get; set; }

        public List<Assertion> Assertions { get; } = new List<Assertion>();

        public List<string> Notes { get; } = new List<string>();

        /// <summary>Every object/state that survived a caller's abort in this case, as "label: added KEY = handle". A leak is a
        /// FAIL and is never reclassified: the exact keys and handles are the evidence.</summary>
        public List<string> Leaks { get; } = new List<string>();

        /// <summary>An unexpected exception inside the case body (harness or binding), never an AUTH-15 result.</summary>
        public string Exception { get; set; }

        /// <summary>A characterization that did not come out as ruled: NEW DEVIATION, the run STOPS.</summary>
        public bool Deviation { get; set; }

        public string Result { get; private set; } = Outcome.NotRun;

        public bool Check(string name, string expected, string observed, bool ok)
        {
            Assertions.Add(new Assertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Result = ok ? Outcome.Pass : Outcome.Fail,
            });
            return ok;
        }

        public bool Check(string name, bool expected, bool observed) =>
            Check(name, expected.ToString(), observed.ToString(), expected == observed);

        public bool Equal(string name, string expected, string observed) =>
            Check(name, expected ?? "<null>", observed ?? "<null>", string.Equals(expected, observed, StringComparison.Ordinal));

        public void Unknown(string name, string expected, string why) =>
            Assertions.Add(new Assertion { Name = name, Expected = expected, Observed = why, Result = Outcome.Unknown });

        public void Finish()
        {
            if (Assertions.Any(a => a.Result == Outcome.Fail))
            {
                Result = Outcome.Fail;
            }
            else if (Exception != null || Assertions.Count == 0 || Assertions.Any(a => a.Result == Outcome.Unknown))
            {
                Result = Outcome.Unknown;
            }
            else
            {
                Result = Outcome.Pass;
            }
        }
    }

    /// <summary>
    /// What run.scr recorded about FILEDIA in outiledia.txt (before / during / after). FILEDIA is the Owner's preference and
    /// must never be left changed: the run is only valid if it was restored to EXACTLY the captured original, before the
    /// harness command started, and is still that value at the end. Pure, so it is testable without AutoCAD.
    /// </summary>
    internal sealed class FilediaRecord
    {
        public int? Before { get; private set; }

        public int? During { get; private set; }

        public int? After { get; private set; }

        /// <summary>Strict parse of lines "before=N", "during=N", "after=N" (each at most once, nothing else).</summary>
        public static FilediaRecord Parse(string text, out string error)
        {
            var record = new FilediaRecord();
            error = null;

            foreach (var raw in (text ?? string.Empty).Split('\n'))
            {
                var line = raw.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                var at = line.IndexOf('=');
                int value;

                if (at <= 0 || !int.TryParse(line.Substring(at + 1), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value))
                {
                    error = "unparseable line: " + line;
                    return record;
                }

                switch (line.Substring(0, at))
                {
                    case "before" when record.Before == null:
                        record.Before = value;
                        break;
                    case "during" when record.During == null:
                        record.During = value;
                        break;
                    case "after" when record.After == null:
                        record.After = value;
                        break;
                    default:
                        error = "unknown or repeated key: " + line;
                        return record;
                }
            }

            return record;
        }

        /// <summary>Null when the state is valid; otherwise the reason the run is INVALID.</summary>
        public string Validate(int? liveNow)
        {
            if (Before == null || During == null || After == null)
            {
                return "filedia.txt does not hold before, during and after";
            }

            if (During.Value != 0)
            {
                return "FILEDIA during NETLOAD was " + During.Value + ", expected 0";
            }

            if (After.Value != Before.Value)
            {
                return "FILEDIA after NETLOAD (" + After.Value + ") differs from the original (" + Before.Value + ")";
            }

            if (liveNow == null || liveNow.Value != Before.Value)
            {
                return "live FILEDIA (" + (liveNow == null ? "unreadable" : liveNow.Value.ToString()) + ") differs from the original (" + Before.Value + ")";
            }

            return null;
        }
    }

    internal sealed class HvLog
    {
        private readonly string _path;

        public HvLog(string path)
        {
            _path = path;
        }

        public void Info(string message)
        {
            try
            {
                File.AppendAllText(
                    _path,
                    DateTime.UtcNow.ToString("o") + " " + message + Environment.NewLine,
                    new UTF8Encoding(false));
            }
            catch (System.Exception)
            {
                // The log is a courtesy; the evidence file is the record.
            }
        }
    }

    /// <summary>The evidence document (schema I52-AUTH15-HV/1), assembled once and written once at the end.</summary>
    internal sealed class EvidenceDoc
    {
        public const string Schema = "I52-AUTH15-HV/1";

        public Dictionary<string, object> Header { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> Package { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> Host { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> Binding { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> Fixtures { get; } = new Dictionary<string, object>();

        public List<CaseRecord> Cases { get; } = new List<CaseRecord>();

        /// <summary>Observations that are not part of HV-00..HV-14 and never change the verdict.</summary>
        public List<Dictionary<string, object>> Characterizations { get; } = new List<Dictionary<string, object>>();

        public List<Dictionary<string, object>> NotExercisable { get; } = new List<Dictionary<string, object>>();

        public List<string> Problems { get; } = new List<string>();

        public bool Completed { get; set; }

        public string StoppedBy { get; set; }

        /// <summary>"in-progress" after each completed case, "final" once the run ended.</summary>
        public string State { get; set; } = "in-progress";

        public string LastCase { get; set; }

        public string Verdict()
        {
            var expectedIds = Enumerable.Range(0, 15).Select(i => "HV-" + i.ToString("D2")).ToList();

            if (Cases.Any(c => c.Result == Outcome.Fail))
            {
                return Outcome.Fail;
            }

            var complete = expectedIds.All(id => Cases.Any(c => c.Id == id && c.Result == Outcome.Pass));

            return complete && Completed && Problems.Count == 0 ? Outcome.Pass : Outcome.Unknown;
        }

        public void Write(string path)
        {
            var root = new Dictionary<string, object>
            {
                ["schema"] = Schema,
            };

            foreach (var pair in Header)
            {
                root[pair.Key] = pair.Value;
            }

            root["package"] = Package;
            root["host"] = Host;
            root["binding"] = Binding;
            root["fixtures"] = Fixtures;
            root["cases"] = Cases;
            root["characterizations"] = Characterizations;
            root["notExercisable"] = NotExercisable;
            root["leaks"] = Cases.SelectMany(c => c.Leaks.Select(l => c.Id + " " + l)).ToList();
            root["problems"] = Problems;
            root["state"] = State;
            root["lastCase"] = LastCase;
            root["completed"] = Completed;
            root["stoppedBy"] = StoppedBy;
            root["verdict"] = Verdict();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = null,
            };

            // Temp file + replace: a crash mid-write can never leave a truncated evidence file, and the previous complete
            // snapshot (written after the last finished case) survives.
            var temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(root, options), new UTF8Encoding(false));
            File.Move(temp, path, true);
        }
    }
}
