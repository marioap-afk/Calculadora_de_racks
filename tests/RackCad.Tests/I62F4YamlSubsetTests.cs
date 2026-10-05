#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-A), part of obligation C-18 of the frozen Proposal V14 (Anexo C): the canonical serialization of
    /// <c>rackcad-automation-state/v2</c>. The reader must accept exactly the subset the canonical writer emits and reject everything else with the
    /// line and the reason (fail closed, never normalize); the writer must be a fixed point; the HEADER mode must read every real
    /// <c>rackcad-automation-state/v1</c> file of the tree, because the classifier of Anexo E.2 consumes only <c>schema</c> and <c>claim_id</c> from
    /// them. Each rejection is shown to be load-bearing: disabling it in memory makes a case of this class fail.
    /// </summary>
    public class I62F4YamlSubsetTests
    {
        // (name, text, expected: "OK" or a substring of the rejection reason) — the cases of the measured preparation prototype f4/yaml-subset.
        public static readonly (string Name, string Text, string Expected)[] Cases =
        {
            ("valid-minimal", "schema: rackcad-automation-state/v2\nautomation_state:\n  initiative: I-99\n  attempts: 0\n  claim_id: null\n", "OK"),
            ("nested-list-map", "a:\n  items:\n    - k: 1\n      v: \"x\"\n    - k: 2\n      v: []\n  flags:\n    - true\n    - null\n", "OK"),
            ("quoted-colon", "a: \"X: Y\"\nb: 'it''s: fine'\n", "OK"),
            ("plain-colon", "a: X: Y\n", "contains ': '"),
            ("invalid-indentation", "a:\n  b:\n    c: 1\n   d: 2\n", "unexpected indentation"),
            ("duplicate-key", "a: 1\na: 2\n", "duplicate key"),
            ("anchor", "a: &x 1\nb: 2\n", "anchors, aliases and tags"),
            ("alias", "a: 1\nb: *x\n", "anchors, aliases and tags"),
            ("multiline-folded", "a: >-\n  uno\n  dos\n", "block scalars"),
            ("multiline-plain", "a: uno\n  dos\n", "multi-line scalar"),
            ("multiline-quoted", "a: \"uno\n  dos\"\n", "unterminated or multi-line"),
            ("malformed-list", "a:\n  - 1\n  b: 2\n", "mapping key where a sequence item was expected"),
            ("list-not-indented", "a:\n- 1\n", "sequence must be indented"),
            ("empty-value", "a:\nb: 1\n", "empty value without a nested block"),
            ("tab-indent", "a:\n\tb: 1\n", "tab in indentation"),
            ("flow-nonempty", "a: [1, 2]\n", "flow collections"),
            ("tag", "a: !!str 1\n", "anchors, aliases and tags"),
            ("doc-marker", "---\na: 1\n", "document markers"),
            ("ambiguous-yes", "a: yes\n", "ambiguous plain scalar"),
            ("ambiguous-octal", "a: 0123\n", "ambiguous plain scalar"),
            ("trailing-comment", "a: x # c\n", "' #'"),
            ("comment-lines", "# cabecera\na: 1\n  # sangrado\nb: \"#no\"\n", "OK"),
        };

        // (name, text, expected claim_id, or "ERR:" + substring of the rejection reason)
        public static readonly (string Name, string Text, string Expected)[] HeaderCases =
        {
            ("v1-folded-next-action", "schema: rackcad-automation-state/v1\nautomation_state:\n  initiative: I-23\n  claim_id: fade0f8a-1438-4bfb-a894-2a9671685e60\n"
                + "  next_action: >-\n    uno: dos\n    tres\n  attempts: 0\nexecution_context:\n  x: [1, 2]\n", "fade0f8a-1438-4bfb-a894-2a9671685e60"),
            ("v1-duplicate-claim-id", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: a\n  claim_id: b\n", "ERR:duplicate key"),
            ("v1-folded-claim-id", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: >-\n    abc\n", "ERR:plain scalar starts with an indicator"),
            ("v1-claim-id-continuation", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: abc\n    def\n", "ERR:continuation found"),
            ("v1-claim-id-colon", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: a: b\n", "ERR:contains ': '"),
        };

        [Fact]
        public void I62_C18_TheReaderAcceptsTheSubsetAndRejectsEveryConstructOutsideItWithItsReason()
        {
            var failures = RunCases(YamlReaderMutation.None);
            Assert.True(failures.Count == 0, string.Join("; ", failures));
        }

        [Fact]
        public void I62_C18_TheHeaderModeConsumesOnlySchemaAndTheIdentityFieldsOfAV1State()
        {
            var failures = RunHeaderCases();
            Assert.True(failures.Count == 0, string.Join("; ", failures));
        }

        [Fact]
        public void I62_C18_TheCanonicalWriterIsAFixedPointForAFullStateV2()
        {
            var state = StateV2Samples.ReviewPendingQu();
            var text = YamlSubset.Write(state);
            var back = YamlSubset.Read(text);

            Assert.True(YamlSubset.DeepEquals(state, back), "Read(Write(state)) differs from state");
            Assert.Equal(text, YamlSubset.Write(back));
            Assert.True(text.Split('\n').Length > 150, "the sample must exercise a complete state, not a fragment");
        }

        [Fact]
        public void I62_C18_TheWriterQuotesEveryStringThatWouldReadAsAnotherType()
        {
            var state = StateV2Samples.M(
                ("digits", StateV2Samples.DigitSha), ("yes", "yes"), ("null_text", "null"), ("colon", "X: Y"), ("hash", "a #b"), ("lead", "-x"),
                ("octal", "0123"), ("float", "1.5"), ("newline", "a\nb"), ("quote", "\"q\""), ("control", "a\u0001b"), ("empty", string.Empty));
            var text = YamlSubset.Write(state);
            var back = YamlSubset.Read(text);

            Assert.True(YamlSubset.DeepEquals(state, back), text);
            Assert.Contains("\"" + StateV2Samples.DigitSha + "\"", text, StringComparison.Ordinal);
            Assert.All(back, kv => Assert.IsType<string>(kv.Value));
        }

        [Fact]
        public void I62_C18_EveryRealStateFileOfTheTreeIsReadableByTheModeItsSchemaRequires()
        {
            var dir = I62Repo.FullPath("docs/automation/state");
            var files = Directory.GetFiles(dir, "*.yml").OrderBy(f => f, StringComparer.Ordinal).ToArray();
            Assert.True(files.Length > 0, "the tree must contain state files (selection > 0)");
            var problems = new List<string>();
            var v1 = 0;
            var v2 = 0;
            var other = new List<string>();
            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                try
                {
                    var header = YamlSubset.Read(text, YamlReadMode.Header);
                    var schema = header.TryGetValue("schema", out var s) ? s as string : null;
                    if (schema == "rackcad-automation-state/v2")
                    {
                        v2++;
                        YamlSubset.Read(text, YamlReadMode.Strict);
                    }
                    else if (schema == "rackcad-automation-state/v1")
                    {
                        v1++;
                        if (header["automation_state"] is not YamlMap st || st["claim_id"] is not string claim || claim.Length == 0)
                        {
                            problems.Add(Path.GetFileName(file) + ": rackcad-automation-state/v1 without a claim_id");
                        }
                    }
                    else
                    {
                        // Not a rackcad-automation-state file (e.g. a unit that keeps a free-form status file): reported, never read as a state.
                        other.Add(Path.GetFileName(file));
                    }
                }
                catch (YamlSubsetException e)
                {
                    problems.Add(Path.GetFileName(file) + ": " + e.Message);
                }
            }

            Assert.True(problems.Count == 0, string.Join("; ", problems));
            Assert.True(v1 > 0, "the tree must contain rackcad-automation-state/v1 files (selection > 0)");
            Assert.Equal(files.Length, v1 + v2 + other.Count);
        }

        [Theory]
        [InlineData(YamlReaderMutation.AcceptDuplicateKeys, "duplicate-key")]
        [InlineData(YamlReaderMutation.AcceptPlainColon, "plain-colon")]
        [InlineData(YamlReaderMutation.AcceptBlockScalars, "multiline-folded")]
        [InlineData(YamlReaderMutation.AcceptMultilinePlain, "multiline-plain")]
        public void I62_C18_DisablingARejectionInMemoryMakesItsCaseFail(YamlReaderMutation mutation, string detectingCase)
        {
            var failures = RunCases(mutation);
            Assert.Contains(failures, f => f.StartsWith(detectingCase + ":", StringComparison.Ordinal));
        }

        private static List<string> RunCases(YamlReaderMutation mutation)
        {
            var failures = new List<string>();
            foreach (var (name, text, expected) in Cases)
            {
                string got;
                try
                {
                    YamlSubset.Read(text, YamlReadMode.Strict, mutation);
                    got = "OK";
                }
                catch (YamlSubsetException e)
                {
                    got = e.Message;
                }

                var pass = expected == "OK" ? got == "OK" : got != "OK" && got.Contains(expected, StringComparison.Ordinal);
                if (!pass)
                {
                    failures.Add(name + ": expected " + expected + ", got " + got);
                }
            }

            return failures;
        }

        private static List<string> RunHeaderCases()
        {
            var failures = new List<string>();
            foreach (var (name, text, expected) in HeaderCases)
            {
                string got;
                try
                {
                    var header = YamlSubset.Read(text, YamlReadMode.Header);
                    got = ((YamlMap)header["automation_state"]!)["claim_id"] as string ?? "<null>";
                }
                catch (YamlSubsetException e)
                {
                    got = "ERR:" + e.Message;
                }

                var pass = expected.StartsWith("ERR:", StringComparison.Ordinal)
                    ? got.StartsWith("ERR:", StringComparison.Ordinal) && got.Contains(expected.Substring(4), StringComparison.Ordinal)
                    : got == expected;
                if (!pass)
                {
                    failures.Add(name + ": expected " + expected + ", got " + got);
                }
            }

            return failures;
        }
    }
}
