#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RackCad.Tests
{
    /// <summary>A skipped test with the runner's own reason.</summary>
    public sealed record TrxSkip(string Test, string Reason);

    /// <summary>The normalized reading of one TRX (P6).</summary>
    public sealed record TrxFacts(
        string Sha256,
        IReadOnlyDictionary<string, int> Counters,
        IReadOnlyDictionary<string, int> Outcomes,
        int Passed,
        int Failed,
        IReadOnlyList<TrxSkip> Skipped,
        IReadOnlyList<TrxSkip> UnexplainedNotExecuted,
        IReadOnlyList<string> UnexpectedOutcomes,
        IReadOnlyList<string> Failures,
        IReadOnlyList<string> Notes)
    {
        public string Status => Failures.Count == 0 ? "PASS" : "FAIL";
    }

    /// <summary>
    /// P6 of the F4 opening order (decisions §43): a normalized reading of a VSTest TRX for check #10 (<c>Tests</c>) of AUTOMATION_PLAN 16.9 and the
    /// Full of AGENTS. It distinguishes Passed, Failed, skipped (NotExecuted with the runner's reason) and every other state, which is unexpected and
    /// fails; a skip is never counted as passed; an empty selection, failed tests, a NotExecuted without the runner's reason and a contradiction between
    /// the counters and the results fail. The number of skips is never fixed: optionally it is cross-checked one by one against the methods whose test
    /// attribute declares <c>Skip =</c> in the source. Port of the F4 preparation prototype (<c>i62_helpers.py</c>, <c>p6_trx</c>), plus the unexpected
    /// states. Facts only: no verdict.
    /// </summary>
    public static class TrxReading
    {
        private static readonly XNamespace Ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

        private static readonly Regex SkipAttribute = new Regex(@"\[[A-Za-z]*(?:Fact|Theory)[A-Za-z]*\s*\([^\]]*\bSkip\s*=", RegexOptions.Singleline);

        private static readonly Regex Method = new Regex(@"public\s+(?:async\s+)?(?:void|Task)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(");

        public static TrxFacts Read(byte[] trx, ISet<string>? declaredSkipMethods = null)
        {
            var failures = new List<string>();
            var notes = new List<string>();
            var doc = XDocument.Parse(System.Text.Encoding.UTF8.GetString(trx).TrimStart((char)0xFEFF));
            var counters = (doc.Root?.Element(Ns + "ResultSummary")?.Element(Ns + "Counters")?.Attributes() ?? Enumerable.Empty<XAttribute>())
                .Where(a => int.TryParse(a.Value, out _)).ToDictionary(a => a.Name.LocalName, a => int.Parse(a.Value, System.Globalization.CultureInfo.InvariantCulture));
            var results = doc.Root?.Element(Ns + "Results")?.Elements(Ns + "UnitTestResult").ToList() ?? new List<XElement>();
            var outcomes = new Dictionary<string, int>(StringComparer.Ordinal);
            var skipped = new List<TrxSkip>();
            var unexplained = new List<TrxSkip>();
            var unexpected = new List<string>();
            foreach (var r in results)
            {
                var outcome = (string?)r.Attribute("outcome") ?? "(none)";
                var name = (string?)r.Attribute("testName") ?? "(unnamed)";
                outcomes[outcome] = outcomes.TryGetValue(outcome, out var c) ? c + 1 : 1;
                if (outcome == "NotExecuted")
                {
                    var reason = r.Element(Ns + "Output")?.Element(Ns + "ErrorInfo")?.Element(Ns + "Message")?.Value.Trim() ?? string.Empty;
                    (reason.Length > 0 ? skipped : unexplained).Add(new TrxSkip(name, reason));
                }
                else if (outcome != "Passed" && outcome != "Failed")
                {
                    unexpected.Add(name + ": " + outcome);
                }
            }

            int Count(string key) => counters.TryGetValue(key, out var v) ? v : 0;
            var passed = outcomes.TryGetValue("Passed", out var p) ? p : 0;
            var failed = outcomes.TryGetValue("Failed", out var f) ? f : 0;
            if (results.Count == 0 || Count("total") == 0)
            {
                failures.Add("empty selection");
            }

            if (failed > 0 || Count("failed") > 0)
            {
                failures.Add("failed tests: " + Math.Max(failed, Count("failed")));
            }

            if (unexplained.Count > 0)
            {
                failures.Add("NotExecuted without the runner's skip reason: " + unexplained.Count);
            }

            if (unexpected.Count > 0)
            {
                failures.Add("unexpected outcomes: " + string.Join(", ", unexpected.Take(5)));
            }

            if (counters.ContainsKey("passed") && Count("passed") != passed)
            {
                failures.Add("contradiction: Counters.passed = " + Count("passed") + " against " + passed + " Passed results");
            }

            if (counters.ContainsKey("total") && Count("total") != results.Count)
            {
                failures.Add("contradiction: Counters.total = " + Count("total") + " against " + results.Count + " results");
            }

            if (skipped.Count > 0 && Count("notExecuted") == 0)
            {
                notes.Add("VSTest TRX logger convention: skips as NotExecuted with Counters.notExecuted = 0 (I-63 evidence §37.4)");
            }

            if (declaredSkipMethods != null)
            {
                var names = new HashSet<string>(skipped.Select(s => s.Test.Split('.').Last()), StringComparer.Ordinal);
                var notDeclared = names.Where(n => !declaredSkipMethods.Contains(n)).OrderBy(n => n, StringComparer.Ordinal).ToList();
                var executed = declaredSkipMethods.Where(n => !names.Contains(n)).OrderBy(n => n, StringComparer.Ordinal).ToList();
                if (notDeclared.Count > 0)
                {
                    failures.Add("skips the source does not declare: " + string.Join(", ", notDeclared.Take(5)));
                }

                if (executed.Count > 0)
                {
                    notes.Add("declared with Skip but not skipped in this TRX: " + string.Join(", ", executed.Take(5)));
                }
            }

            return new TrxFacts(Convert.ToHexString(SHA256.HashData(trx)).ToLowerInvariant(), counters, outcomes, passed, failed, skipped, unexplained, unexpected,
                failures, notes);
        }

        /// <summary>The methods whose test attribute declares <c>Skip = …</c> (a declaration of the framework in the source, not a TRX count).</summary>
        public static HashSet<string> DeclaredSkipMethods(IEnumerable<string> sources)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var text in sources)
            {
                foreach (Match m in SkipAttribute.Matches(text))
                {
                    var method = Method.Match(text, m.Index + m.Length);
                    if (method.Success)
                    {
                        names.Add(method.Groups[1].Value);
                    }
                }
            }

            return names;
        }
    }
}
