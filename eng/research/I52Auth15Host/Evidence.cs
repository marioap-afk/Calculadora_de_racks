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

    /// <summary>Why the run stopped early (machine-readable; the launcher classifies from it).</summary>
    internal static class StopKind
    {
        public const string None = "none";

        /// <summary>A case asked to stop because continuing would make later evidence untrustworthy.</summary>
        public const string Deviation = "deviation";

        /// <summary>HV-00 was not PASS: no AUTH-15 call was made.</summary>
        public const string Hv00 = "hv00";

        /// <summary>FILEDIA was not restored exactly.</summary>
        public const string Filedia = "filedia";

        /// <summary>The runner or the environment became untrustworthy.</summary>
        public const string Exception = "exception";
    }

    /// <summary>
    /// What ending a caller's transaction actually did. RUN-2's leaks could not be attributed because the old helper swallowed
    /// every exception from Abort and Dispose: this records each outcome instead, and turns a failure into a FAIL (a case cannot
    /// carry on as if the rollback had happened). Pure, so the failure rules are testable offline.
    /// </summary>
    internal sealed class TxOutcome
    {
        public string Label { get; set; }

        public bool IsDisposedBefore { get; set; }

        public bool AbortAttempted { get; set; }

        public bool AbortSucceeded { get; set; }

        public string AbortError { get; set; }

        public bool DisposeAttempted { get; set; }

        public bool DisposeSucceeded { get; set; }

        public string DisposeError { get; set; }

        public bool IsDisposedAfter { get; set; }

        public int ActiveBefore { get; set; } = -1;

        public int ActiveAfter { get; set; } = -1;

        /// <summary>The native identity of the top transaction after the end, or "null".</summary>
        public string TopAfter { get; set; }

        /// <summary>The native identity of the transaction that was ended (read before it was ended), or null if it could not be read.</summary>
        public string EndedId { get; set; }

        /// <summary>The reasons this end cannot be taken as a successful rollback (empty = it can).</summary>
        /// <param name="abortExpected">The path expects the caller's Abort to be what rolls back.</param>
        /// <param name="strictActive">Rollback-sensitive cases and controls: the active-transaction bookkeeping must be provable, RELATIVE to the
        /// count read just before the end (never an absolute zero): ending one transaction takes the count down by exactly one and the top
        /// transaction afterwards must be consistent with that count. A metric that cannot be read is a failure (fail-closed), not "good".</param>
        public System.Collections.Generic.List<string> Failures(bool abortExpected, bool strictActive = false)
        {
            var failures = new System.Collections.Generic.List<string>();

            if (abortExpected && IsDisposedBefore)
            {
                // Something disposed the caller's transaction before the caller's Abort: there was nothing left to roll back, so this
                // is NOT a clean rollback and it is never reinterpreted as "already rolled back".
                failures.Add("the transaction was already disposed BEFORE the caller's Abort: not a clean rollback");
            }

            if (abortExpected && !IsDisposedBefore && (!AbortAttempted || !AbortSucceeded))
            {
                failures.Add("Abort " + (AbortAttempted ? "threw: " + AbortError : "was not attempted"));
            }

            if (DisposeAttempted && !DisposeSucceeded)
            {
                failures.Add("Dispose threw: " + DisposeError);
            }

            if (!IsDisposedAfter)
            {
                failures.Add("the transaction is not disposed after the end");
            }

            if (strictActive)
            {
                if (ActiveBefore < 0 || ActiveAfter < 0)
                {
                    failures.Add("the active-transaction count could not be read (fail-closed)");
                }
                else if (ActiveAfter != ActiveBefore - 1)
                {
                    failures.Add("active transactions went " + ActiveBefore + " -> " + ActiveAfter + " (expected " + ActiveBefore + " -> " + (ActiveBefore - 1) + ")");
                }

                if (string.IsNullOrEmpty(TopAfter) || TopAfter.StartsWith("unreadable", StringComparison.Ordinal))
                {
                    failures.Add("the top transaction after the end could not be read (fail-closed)");
                }
                else
                {
                    if (ActiveAfter == 0 && TopAfter != "null")
                    {
                        failures.Add("no transaction is active but the top transaction is " + TopAfter);
                    }

                    if (ActiveAfter > 0 && TopAfter == "null")
                    {
                        failures.Add(ActiveAfter + " transaction(s) still active but there is no top transaction");
                    }

                    if (!string.IsNullOrEmpty(EndedId) && TopAfter == EndedId)
                    {
                        failures.Add("the ended transaction is still the top transaction");
                    }
                }

                if (string.IsNullOrEmpty(EndedId))
                {
                    failures.Add("the identity of the ended transaction could not be read (fail-closed)");
                }
            }

            return failures;
        }
    }

    /// <summary>
    /// The rollback controls that GOVERN the verdict. The exact expected set is 14 records (id + database kind): a PASS needs every one of them
    /// present exactly once and PASS, with no control leak and with DOCUMENT-AUTHORITY available. A control leak or FAIL is a FAIL; a missing,
    /// duplicated, unexpected or UNKNOWN control can never PASS. A SIDE-DB record never stands in for a missing DOCUMENT-AUTHORITY one (they are
    /// distinct keys). Pure, so the launcher's independent recomputation and the offline mutation tests have one definition to mirror.
    /// </summary>
    internal static class ControlSet
    {
        public const string Side = "SIDE-DB";

        public const string Document = "DOCUMENT-AUTHORITY";

        /// <summary>The governing (id, database kind) pairs, 14 in all.</summary>
        public static readonly (string Id, string DbKind)[] Expected =
        {
            ("RB-01", Side),
            ("RB-01V", Side),
            ("RB-01V", Document),
            ("RB-01D", Document),
            ("RB-02a", Side),
            ("RB-02a", Document),
            ("RB-02b", Side),
            ("RB-02b", Document),
            ("RB-02c", Side),
            ("RB-02c", Document),
            ("RB-03", Side),
            ("RB-03", Document),
            ("RB-05", Side),
            ("RB-05", Document),
        };

        /// <summary>The rollback-sensitive HV cases: their governing run is the DOCUMENT one; a side-database run never satisfies them.</summary>
        public static readonly string[] RollbackSensitiveCases = { "HV-01", "HV-02", "HV-03", "HV-05", "HV-08", "HV-10", "HV-12", "HV-13", "HV-14" };

        /// <summary>Why the control evidence cannot back a PASS (empty = it can). Includes documentAuthority.available.</summary>
        public static System.Collections.Generic.List<string> Problems(
            System.Collections.Generic.IEnumerable<CaseRecord> controls,
            System.Collections.Generic.IDictionary<string, object> documentAuthority)
        {
            var problems = new System.Collections.Generic.List<string>();
            var records = controls.ToList();

            foreach (var (id, kind) in Expected)
            {
                var matching = records.Where(r => r.Id == id && r.DbKind == kind).ToList();

                if (matching.Count == 0)
                {
                    problems.Add("control " + id + " [" + kind + "] is missing");
                }
                else if (matching.Count > 1)
                {
                    problems.Add("control " + id + " [" + kind + "] is duplicated (" + matching.Count + " records)");
                }
                else if (matching[0].Result != Outcome.Pass)
                {
                    problems.Add("control " + id + " [" + kind + "] is " + matching[0].Result);
                }
            }

            foreach (var record in records.Where(r => !Expected.Any(e => e.Id == r.Id && e.DbKind == r.DbKind)))
            {
                problems.Add("unexpected control record " + record.Id + " [" + record.DbKind + "]");
            }

            foreach (var record in records.Where(r => r.Leaks.Count > 0))
            {
                problems.Add("control " + record.Id + " [" + record.DbKind + "] leaked " + record.Leaks.Count + " item(s)");
            }

            object available;

            if (documentAuthority == null || !documentAuthority.TryGetValue("available", out available) || !(available is bool) || !(bool)available)
            {
                problems.Add("documentAuthority.available is not true");
            }

            return problems;
        }

        /// <summary>The rollback-sensitive cases that are not labelled DOCUMENT-AUTHORITY (or are missing).</summary>
        public static System.Collections.Generic.List<string> CaseLabelProblems(System.Collections.Generic.IEnumerable<CaseRecord> cases)
        {
            var problems = new System.Collections.Generic.List<string>();
            var list = cases.ToList();

            foreach (var id in RollbackSensitiveCases)
            {
                var matching = list.Where(c => c.Id == id).ToList();

                if (matching.Count != 1 || matching[0].DbKind != Document)
                {
                    problems.Add(id + " is not a single " + Document + " record" + (matching.Count == 1 ? " (labelled " + matching[0].DbKind + ")" : " (" + matching.Count + " records)"));
                }
            }

            return problems;
        }
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

        /// <summary>Which database the case ran on: DOCUMENT-AUTHORITY (the production condition), SIDE-DB-CHARACTERIZATION, or SIDE-DB
        /// (cases and controls whose subject does not depend on the database kind).</summary>
        public string DbKind { get; set; }

        /// <summary>Every transaction end this case performed, with its Abort/Dispose outcome.</summary>
        public System.Collections.Generic.List<TxOutcome> Tx { get; } = new System.Collections.Generic.List<TxOutcome>();

        /// <summary>The case asks the runner to stop because continuing would make later evidence untrustworthy.</summary>
        public bool StopRun { get; set; }

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

        /// <summary>Rollback controls (RB-xx): raw outcomes, PASS/FAIL each, never reinterpreted and never part of the HV verdict.</summary>
        public List<CaseRecord> Controls { get; } = new List<CaseRecord>();

        /// <summary>Side-database runs of the rollback-sensitive HV cases. Characterization only: they never change the verdict.</summary>
        public List<CaseRecord> SideCharacterizations { get; } = new List<CaseRecord>();

        public Dictionary<string, object> DocumentAuthority { get; } = new Dictionary<string, object>();

        /// <summary>Ids of the cases that recorded a NEW DEVIATION. The run continues after one unless it asks to stop.</summary>
        public List<string> Deviations { get; } = new List<string>();

        public string StopKind { get; set; } = HostHarness.StopKind.None;

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

            // A FAIL anywhere governs: a failing HV case OR a failing control (a control is a raw outcome, but a control that leaks
            // means the caller-rollback premise does not hold on that path, so the campaign cannot be a PASS).
            if (Cases.Any(c => c.Result == Outcome.Fail) || Controls.Any(c => c.Result == Outcome.Fail))
            {
                return Outcome.Fail;
            }

            var complete = expectedIds.All(id => Cases.Count(c => c.Id == id && c.Result == Outcome.Pass) == 1) && Cases.Count == expectedIds.Count;
            var controlsGovern = ControlSet.Problems(Controls, DocumentAuthority).Count == 0 && ControlSet.CaseLabelProblems(Cases).Count == 0;

            return complete && controlsGovern && Completed && Problems.Count == 0 ? Outcome.Pass : Outcome.Unknown;
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
            root["controls"] = Controls;
            root["controlLeaks"] = Controls.SelectMany(c => c.Leaks.Select(l => c.Id + " [" + c.DbKind + "] " + l)).ToList();
            root["sideCharacterizations"] = SideCharacterizations;
            root["sideCharacterizationLeaks"] = SideCharacterizations.SelectMany(c => c.Leaks.Select(l => c.Id + " [" + c.DbKind + "] " + l)).ToList();
            root["documentAuthority"] = DocumentAuthority;
            root["deviations"] = Deviations;
            root["stopKind"] = StopKind;
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
