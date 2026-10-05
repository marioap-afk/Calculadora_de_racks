#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>P5: a capability observation compared with the accepted one (README §13.3). The state is never ACCEPTED: acceptance is not a helper's.</summary>
    public sealed record ObservationComparison(IReadOnlyList<string> Changed, bool Inheritable, string State);

    /// <summary>
    /// P5 of the F4 opening order (decisions §43), only what the Freeze authorizes: the invalidation of a capability observation by its frozen
    /// invalidators (AUTOMATION_PLAN 16.18; <c>preflight/v1</c> <c>Invalidators</c>; README §13.3), STOP when a declared fingerprint changes during a
    /// cession (P-01, P-11; 16.19) and fingerprints that carry only non-secret metadata (a hash and names, never values; README §13.1 rule 6, C4 and
    /// §13.4). A new observation is only an observation: nothing here accepts a new baseline, reconfigures or resets (README §13.3), and OD-2 is
    /// untouched. The helper works on given values; it never reads a real configuration file, credential or secret.
    /// </summary>
    public static class ConfigurationFacts
    {
        /// <summary>The invalidators of <c>preflight/v1</c>, in schema order. The binding and the branch SHA are deliberately absent (README §13.3).</summary>
        public static readonly string[] Invalidators =
            { "HostInstanceHash", "AdapterVersion", "BinaryPathHash", "AuthState", "FingerprintSha256", "CatalogEntryBlob", "RoutingBlob" };

        /// <summary>The secret patterns of README §13.4 (id, regular expression).</summary>
        public static readonly (string Id, Regex Pattern)[] SecretPatterns =
        {
            ("PEM", new Regex("-----BEGIN [A-Z ]*PRIVATE KEY-----")),
            ("JWT", new Regex(@"eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]*")),
            ("SK", new Regex(@"\bsk-[A-Za-z0-9_-]{16,}")),
            ("GH", new Regex(@"\b(gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})")),
            ("AWS", new Regex(@"\bAKIA[0-9A-Z]{16}\b")),
            ("SLACK", new Regex(@"\bxox[abposr]-[A-Za-z0-9-]{10,}")),
            ("GAPI", new Regex(@"\bAIza[0-9A-Za-z_-]{35}\b")),
            ("BEARER", new Regex(@"(?i)\bbearer\s+[A-Za-z0-9._~+/-]{16,}=*")),
            ("KV", new Regex(@"(?i)\b(password|passwd|secret|token|api[_-]?key|access[_-]?key|client[_-]?secret)\b\s*[:=]\s*\S+")),
            ("URLCRED", new Regex(@"[a-z][a-z0-9+.-]*://[^/\s:@]+:[^/\s@]+@")),
        };

        private static readonly Regex Sha256 = new Regex("^[0-9a-f]{64}$");

        /// <summary>
        /// README §13.3: an accepted observation stops being valid when any invalidator differs from the current observation (its requirements become
        /// UNKNOWN and a new observation is needed). An unobserved host instance (<c>HostInstanceState</c> UNOBSERVED) forbids inheriting an observation
        /// between sessions (16.18). The result is STILL_VALID or INVALIDATED, never an acceptance of the new values.
        /// </summary>
        public static ObservationComparison Compare(JsonObject accepted, JsonObject observed)
        {
            var changed = Invalidators.Where(k => !JsonNode.DeepEquals(accepted["Invalidators"]?[k], observed["Invalidators"]?[k])).ToList();
            var inheritable = J.S(accepted, "Host.HostInstanceState") == "OBSERVED" && J.S(observed, "Host.HostInstanceState") == "OBSERVED";
            return new ObservationComparison(changed, inheritable, changed.Count == 0 && inheritable ? "STILL_VALID" : "INVALIDATED");
        }

        /// <summary>
        /// P-01 / P-11: the fingerprint declared at the exit and at the entry of a cession. Equal → MATCH; changed, or unknown at either end → STOP.
        /// A STOP is resolved by the Coordinator, never by adopting the new fingerprint here.
        /// </summary>
        public static string CessionGate(string? exitFingerprint, string? entryFingerprint)
        {
            return exitFingerprint != null && entryFingerprint != null && exitFingerprint != "UNKNOWN" && exitFingerprint == entryFingerprint ? "MATCH" : "STOP";
        }

        /// <summary>The ids of the secret patterns found in a text.</summary>
        public static List<string> SecretHits(string text) => SecretPatterns.Where(p => p.Pattern.IsMatch(text)).Select(p => p.Id).ToList();

        /// <summary>
        /// The non-secret rule of a <c>preflight/v1</c> <c>Fingerprint</c> (README §13.1 rule 6, C4 and §13.4): CONFIG_FILE carries a SHA-256 and
        /// names only; NONE carries the Coordinator's acceptance and no hash; UNVERIFIED carries neither hash, names nor decision. A name never carries a
        /// value (<c>=</c>), a secret pattern, or an unsanitized path or drive (<c>\</c>, <c>/</c>, <c>:</c>) outside a <c>&lt;redactado&gt;</c> section.
        /// </summary>
        public static List<string> FingerprintProblems(JsonObject fingerprint)
        {
            var problems = new List<string>();
            var kind = J.S(fingerprint, "Kind");
            var sha = J.S(fingerprint, "Sha256");
            var names = (fingerprint["KeyNames"] as JsonArray)?.Select(n => n?.GetValue<string>() ?? string.Empty).ToList() ?? new List<string>();
            var decision = fingerprint["AcceptanceDecisionRef"];
            switch (kind)
            {
                case "CONFIG_FILE":
                    if (sha == null || !Sha256.IsMatch(sha))
                    {
                        problems.Add("CONFIG_FILE without a SHA-256");
                    }

                    if (decision != null)
                    {
                        problems.Add("CONFIG_FILE with an acceptance decision");
                    }

                    break;
                case "NONE":
                    if (sha != null || names.Count > 0 || decision == null)
                    {
                        problems.Add("NONE must carry the Coordinator's acceptance and no hash or names");
                    }

                    break;
                case "UNVERIFIED":
                    if (sha != null || names.Count > 0 || decision != null)
                    {
                        problems.Add("UNVERIFIED carries no hash, names or decision");
                    }

                    break;
                default:
                    problems.Add("unknown fingerprint kind " + kind);
                    break;
            }

            foreach (var name in names)
            {
                if (name.Contains('=', StringComparison.Ordinal))
                {
                    problems.Add("a key name carries a value: " + name.Substring(0, name.IndexOf('=', StringComparison.Ordinal)) + "=…");
                }

                if (SecretHits(name).Count > 0)
                {
                    problems.Add("a key name matches a secret pattern (" + string.Join(",", SecretHits(name)) + ")");
                }

                if (name.IndexOfAny(new[] { '\\', '/', ':' }) >= 0 && !name.Contains("<redactado>", StringComparison.Ordinal))
                {
                    problems.Add("a key name keeps a path or a drive unsanitized");
                }
            }

            return problems;
        }
    }
}
