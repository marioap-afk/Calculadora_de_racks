#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62, gate F1: obligations C-01 and C-03 of the frozen Proposal V14 (Anexo C). They guard the plane-(b) materialization: the role table of
    /// AUTOMATION_PLAN §16.1 carries no provider mark, and the CONFIGURATION_STATUS texts of AUTOMATION_PLAN §16 and of the agent-execution README
    /// are equal, row by row, to Proposal V14 §4.2. The oracle of C-03 is the frozen text itself, pinned by its Git blob. These are fidelity controls,
    /// not behavior tests; each content assertion is shown to be able to fail with an in-memory mutation of the real input. The I-61 guards
    /// (AgentExecutionProtocolTests) stay unchanged.
    /// </summary>
    public class PrincipalPortabilityProtocolTests
    {
        private const string FreezePath = "docs/initiatives/I-62-proposal-v14.md";
        private const string FreezeBlob = "34ad80ea1bfff144bfc5169f62920a4c904c1bfa";
        private const string FreezeStatusSection = "### 4.2 `CONFIGURATION_STATUS` y disposición (D-04)";

        private const string PlanPath = "docs/AUTOMATION_PLAN.md";
        private const string PlanRolesSection = "### 16.1 Participantes y declaraciones";
        private const string PlanStatusSection = "### 16.16 `CONFIGURATION_STATUS` y disposición (I62)";

        private const string ProtocolDir = "docs/automation/agent-execution";
        private const string ReadmeStatusSection = "## 12. Autoverificación del Principal y `CONFIGURATION_STATUS` (unidades I62)";

        private const string RolesHeader = "| Rol | Semántica | Declara | Nunca declara |";
        private const string DispositionHeader = "| Situación | Disposición |";
        private const string ExamplesHeader = "| Caso | Entrada | Agregado | Disposición |";

        // Proposal V14 §2: the five provider-neutral roles, in their frozen order.
        private static readonly string[] Roles = { "PRINCIPAL_COORDINATOR", "ARCHITECT", "EXECUTION_CONTROLLER", "WORKER", "REVIEWER" };

        // The five I61 schemas. Every other schema of the protocol directory belongs to the I62 core (Proposal V14 Anexo B.1).
        private static readonly string[] I61Schemas =
        {
            "gate-contract.schema.json",
            "delegation.schema.json",
            "worker-handoff.schema.json",
            "controller-verification.schema.json",
            "relay-record.schema.json",
        };

        // C-01 marks: these fixed patterns, plus the providers named by the adapter descriptors and the model ids of the catalog.
        private static readonly string[] ProviderPatterns =
        {
            "codex", "claude", "anthropic", "openai", "chatgpt", "gpt", "gemini", "google", "copilot", "mistral", "llama", "deepseek",
        };

        // ---------------------------------------------------------------- C-01: roles without provider

        [Fact]
        public void I62_C01_RoleTableOfSection16_1HasTheFiveNeutralRolesAndNoProviderMark()
        {
            var table = RoleTable(Read(PlanPath));

            Assert.NotNull(table);
            Assert.Equal(Roles, RolesIn(table!));
            Assert.Empty(ProviderMarksIn(string.Join("\n", table!)));
        }

        [Fact]
        public void I62_C01_CoreSchemaEnumsCarryNoProviderMark()
        {
            foreach (var file in CoreSchemaFiles())
            {
                var schema = JsonNode.Parse(File.ReadAllText(file));
                Assert.NotNull(schema);
                Assert.Empty(ProviderMarksIn(string.Join("\n", EnumValues(schema!))).Select(m => Path.GetFileName(file) + ": " + m));
            }
        }

        [Fact]
        public void I62_C01_TheMarkOraclesDetectAProviderInTheRoleTableAndInACoreEnum()
        {
            var table = RoleTable(Read(PlanPath));
            Assert.NotNull(table);

            var mutated = table!.Select(r => r.Contains("**WORKER**", StringComparison.Ordinal) ? r.Replace("escritura dentro del alcance", "escritura dentro del alcance (Codex)", StringComparison.Ordinal) : r).ToList();
            Assert.Contains("Codex", ProviderMarksIn(string.Join("\n", mutated)));

            var missingRole = table.Where(r => !r.Contains("**REVIEWER**", StringComparison.Ordinal)).ToList();
            Assert.NotEqual(Roles, RolesIn(missingRole));

            var schema = new JsonObject { ["properties"] = new JsonObject { ["Adapter"] = new JsonObject { ["enum"] = new JsonArray("WORKER", "claude-subagent") } } };
            Assert.NotEmpty(ProviderMarksIn(string.Join("\n", EnumValues(schema))));
        }

        // ---------------------------------------------------------------- C-03: faithful status tables

        [Fact]
        public void I62_C03_TheOracleIsTheFrozenProposal()
        {
            Assert.Equal(FreezeBlob, GitBlobSha1(Read(FreezePath)));
        }

        [Fact]
        public void I62_C03_StatusRulesOfSection16EqualProposalV14Section4_2RowByRow()
        {
            var freeze = SectionLines(Read(FreezePath), FreezeStatusSection);
            var plan = SectionLines(Read(PlanPath), PlanStatusSection);

            Assert.Empty(Differences(AggregationBlock(freeze), AggregationBlock(plan)));
            Assert.Empty(Differences(TableAfter(freeze, DispositionHeader), TableAfter(plan, DispositionHeader)));
        }

        [Fact]
        public void I62_C03_ReadmeExamplesEqualProposalV14Section4_2RowByRow()
        {
            var freeze = SectionLines(Read(FreezePath), FreezeStatusSection);
            var readme = SectionLines(Read(ProtocolDir + "/README.md"), ReadmeStatusSection);

            Assert.Empty(Differences(TableAfter(freeze, ExamplesHeader), TableAfter(readme, ExamplesHeader)));
        }

        [Fact]
        public void I62_C03_TheFidelityOracleDetectsMutatedRowsOrderAndOracle()
        {
            var freezeText = Read(FreezePath);
            var freeze = SectionLines(freezeText, FreezeStatusSection);
            var plan = SectionLines(Read(PlanPath), PlanStatusSection);
            var readme = SectionLines(Read(ProtocolDir + "/README.md"), ReadmeStatusSection);
            Assert.NotEmpty(plan);
            Assert.NotEmpty(readme);

            var weakened = plan.Select(l => l.Replace("nunca convierte lo no observado en MATCH", "convierte lo no observado en MATCH", StringComparison.Ordinal)).ToList();
            Assert.NotEmpty(Differences(TableAfter(freeze, DispositionHeader), TableAfter(weakened, DispositionHeader)));

            var optimistic = readme.Select(l => l.StartsWith("| E4 |", StringComparison.Ordinal) ? l.Replace("UNKNOWN", "MATCH", StringComparison.Ordinal) : l).ToList();
            Assert.NotEmpty(Differences(TableAfter(freeze, ExamplesHeader), TableAfter(optimistic, ExamplesHeader)));

            var dropped = readme.Where(l => !l.StartsWith("| E13 |", StringComparison.Ordinal)).ToList();
            Assert.NotEmpty(Differences(TableAfter(freeze, ExamplesHeader), TableAfter(dropped, ExamplesHeader)));

            var second = plan.FindIndex(l => l.StartsWith("2. ", StringComparison.Ordinal));
            Assert.True(second >= 0 && plan[second + 1].StartsWith("3. ", StringComparison.Ordinal));
            var reordered = plan.ToList();
            (reordered[second], reordered[second + 1]) = (reordered[second + 1], reordered[second]);
            Assert.NotEmpty(Differences(AggregationBlock(freeze), AggregationBlock(reordered)));

            Assert.NotEqual(FreezeBlob, GitBlobSha1(freezeText.Replace("UNKNOWN nunca → MATCH", "UNKNOWN → MATCH", StringComparison.Ordinal)));
        }

        // ================================================================ oracles

        private static List<string>? RoleTable(string plan) => TableAfter(SectionLines(plan, PlanRolesSection), RolesHeader);

        private static List<string> RolesIn(IEnumerable<string> table) =>
            table.Skip(2)
                .Select(r => r.Trim('|').Split('|')[0].Trim().Trim('*'))
                .ToList();

        private static List<string> ProviderMarksIn(string text)
        {
            var hits = new List<string>();
            foreach (var mark in ProviderMarks())
            {
                hits.AddRange(Regex.Matches(text, @"\b" + Regex.Escape(mark) + @"\b", RegexOptions.IgnoreCase).Select(m => m.Value));
            }

            return hits;
        }

        private static List<string> ProviderMarks()
        {
            var marks = new HashSet<string>(ProviderPatterns, StringComparer.OrdinalIgnoreCase);

            // Adapter descriptors are adapters/<AdapterId>.md (Proposal V14 Anexo B.2); the provider is the first segment of the id.
            var adapters = Path.Combine(RepoPath(ProtocolDir), "adapters");
            if (Directory.Exists(adapters))
            {
                foreach (var descriptor in Directory.GetFiles(adapters, "*.md"))
                {
                    marks.Add(Path.GetFileNameWithoutExtension(descriptor).Split('-')[0]);
                }
            }

            var catalogIds = Regex.Matches(Read(ProtocolDir + "/model-catalog.md"), @"^### (\S+) \(", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();
            foreach (var id in catalogIds)
            {
                marks.Add(id);
                foreach (var segment in Regex.Split(id, @"[-.]").Where(s => s.Length >= 3 && s.All(char.IsLetter)))
                {
                    marks.Add(segment);
                }
            }

            return marks.ToList();
        }

        private static IEnumerable<string> CoreSchemaFiles()
        {
            var schemas = Path.Combine(RepoPath(ProtocolDir), "schemas");
            return Directory.GetFiles(schemas, "*.json", SearchOption.AllDirectories)
                .Where(f => !I61Schemas.Contains(Path.GetRelativePath(schemas, f).Replace('\\', '/'), StringComparer.Ordinal))
                .OrderBy(f => f, StringComparer.Ordinal);
        }

        private static List<string> EnumValues(JsonNode? node)
        {
            var values = new List<string>();
            if (node is JsonObject obj)
            {
                foreach (var (key, child) in obj)
                {
                    if ((key == "enum" || key == "const") && child != null)
                    {
                        values.AddRange(child is JsonArray array ? array.Select(v => v?.ToString() ?? string.Empty) : new[] { child.ToString() });
                    }
                    else
                    {
                        values.AddRange(EnumValues(child));
                    }
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    values.AddRange(EnumValues(item));
                }
            }

            return values;
        }

        // The normative aggregation text: from «Estados exactos:» through «Las métricas opcionales …», without blank lines.
        private static List<string>? AggregationBlock(IReadOnlyList<string> section)
        {
            var start = section.ToList().FindIndex(l => l.StartsWith("Estados exactos:", StringComparison.Ordinal));
            var end = section.ToList().FindIndex(l => l.StartsWith("Las métricas opcionales", StringComparison.Ordinal));
            if (start < 0 || end < start)
            {
                return null;
            }

            return section.Skip(start).Take(end - start + 1).Select(Normalize).Where(l => l.Length > 0).ToList();
        }

        // The table whose header row is <paramref name="header"/>: header, separator and every row up to the first line that is not a row.
        private static List<string>? TableAfter(IReadOnlyList<string> section, string header)
        {
            var starts = section.Select((l, i) => (Line: l, Index: i)).Where(x => Normalize(x.Line) == Normalize(header)).Select(x => x.Index).ToList();
            if (starts.Count != 1)
            {
                return null;
            }

            return section.Skip(starts[0]).TakeWhile(l => l.TrimStart().StartsWith("|", StringComparison.Ordinal)).Select(Normalize).ToList();
        }

        private static List<string> Differences(IReadOnlyList<string>? expected, IReadOnlyList<string>? actual)
        {
            if (expected == null || expected.Count == 0)
            {
                return new List<string> { "the frozen oracle text was not found" };
            }

            if (actual == null)
            {
                return new List<string> { "the materialized text was not found" };
            }

            var differences = new List<string>();
            for (var i = 0; i < Math.Max(expected.Count, actual.Count); i++)
            {
                var e = i < expected.Count ? expected[i] : "<none>";
                var a = i < actual.Count ? actual[i] : "<none>";
                if (!string.Equals(e, a, StringComparison.Ordinal))
                {
                    differences.Add("row " + (i + 1) + ": expected «" + e + "», got «" + a + "»");
                }
            }

            return differences;
        }

        // Lines of the section whose heading line is <paramref name="heading"/>, up to the next heading of the same or a higher level. Lines inside
        // fenced blocks are never headings. Empty when the heading is absent or not unique.
        private static List<string> SectionLines(string text, string heading)
        {
            var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var headings = new List<(int Index, int Level)>();
            var inFence = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    inFence = !inFence;
                    continue;
                }

                var level = inFence ? 0 : HeadingLevel(lines[i]);
                if (level > 0)
                {
                    headings.Add((i, level));
                }
            }

            var matches = headings.Where(h => lines[h.Index].TrimEnd() == heading).ToList();
            if (matches.Count != 1)
            {
                return new List<string>();
            }

            var (start, startLevel) = matches[0];
            var next = headings.FirstOrDefault(h => h.Index > start && h.Level <= startLevel);
            var end = next == default ? lines.Length : next.Index;
            return lines.Skip(start).Take(end - start).ToList();
        }

        private static int HeadingLevel(string line)
        {
            var level = line.TakeWhile(c => c == '#').Count();
            return level is >= 1 and <= 6 && line.Length > level && line[level] == ' ' ? level : 0;
        }

        // Git blob id of the text as stored in the repository (LF line ends, UTF-8 without BOM).
        private static string GitBlobSha1(string text)
        {
            var content = Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal));
            var header = Encoding.ASCII.GetBytes("blob " + content.Length + "\0");
            return Convert.ToHexString(SHA1.HashData(header.Concat(content).ToArray())).ToLowerInvariant();
        }

        // ================================================================ input

        private static string Normalize(string text) => Regex.Replace(text, @"\s+", " ").Trim();

        private static string RepoPath(string relative) =>
            Path.Combine(I55G14FoundationConsumerTests.RepoRoot(), relative.Replace('/', Path.DirectorySeparatorChar));

        private static string Read(string relative)
        {
            var path = RepoPath(relative);
            Assert.True(File.Exists(path), relative + " must exist");
            return File.ReadAllText(path);
        }
    }
}
