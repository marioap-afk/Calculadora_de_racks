#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-61, obligations OBL-01..OBL-06 and the structural part of OBL-11 (frozen Proposal V9 §13). The protocol artifacts under
    /// docs/automation/agent-execution/ and PROMPT_TEMPLATES §G are the product of the initiative, so these tests guard their static
    /// boundaries. Each content assertion is shown to be able to fail with an in-memory mutation of the real input.
    /// </summary>
    public class AgentExecutionProtocolTests
    {
        private const string ProtocolDir = "docs/automation/agent-execution";
        private const string Sha40 = "^[0-9a-f]{40}$";

        private static readonly string[] SchemaFiles =
        {
            "gate-contract.schema.json",
            "delegation.schema.json",
            "worker-handoff.schema.json",
            "controller-verification.schema.json",
            "relay-record.schema.json",
        };

        // Proposal V9 §8.4.
        private static readonly string[] GateTerms =
        {
            "GATE PASS", "GATE_PASS", "Candidato", "Candidate", "FINAL_CANDIDATE_SHA", "gate cerrado", "gate closed", "cierre del gate",
            "rama integrada", "integrada en main", "integrated into main", "merged into main", "lista para integrar", "ready to merge",
            "Owner Validation APROBADA", "Owner Validation APPROVED",
        };

        // Proposal V9 §8.3: the fourteen mandatory checks, in their fixed order.
        private static readonly string[] MandatoryChecks =
        {
            "Termination", "Handoff", "Authority", "Contract", "Identity", "Remote", "Scope", "CleanTree", "Ci", "Tests", "Trailer",
            "Routing", "FreeText", "Denials",
        };

        private static readonly string[] Profiles =
        {
            "ROUTINE_IMPLEMENTATION", "DEBUGGING", "LONG_HORIZON_IMPLEMENTATION", "ARCHITECTURE_REVIEW", "CHARACTERIZATION",
            "DOCUMENTATION", "CONTROLLER_PLANNING", "CONTROLLER_VERIFICATION",
        };

        // INITIATIVE_LIFECYCLE §10: the eight fields of every order, as labels of the base delegation contract.
        private static readonly string[] LifecycleFields =
        {
            "Objetivo:", "Alcance:", "Invariantes del Freeze:", "Rutas y archivos calientes:", "Evidencia requerida:", "No-touch:",
            "Condiciones de parada:", "Informe esperado:",
        };

        // Proposal V9 §13: frozen witness phrases and their sources.
        private static readonly (string Source, string Phrase)[] Witnesses =
        {
            ("AGENTS.md", "La `ref` se consulta en la corrida; no se infiere del `head_sha`."),
            ("AGENTS.md", "0 pruebas seleccionadas = FALLO"),
            ("docs/WORKFLOW.md", "la sesión saliente deja commit + push +"),
            ("docs/WORKFLOW.md", "si el trunk avanzó, **rebase** sobre su punta antes de escribir una"),
            ("docs/AUTOMATION_PLAN.md", "Un intento es una secuencia de diagnostico, correccion acotada, validacion local, commit y push."),
            ("docs/AUTOMATION_PLAN.md", "El ejecutor se detiene y deja informe cuando ocurra cualquiera de estas condiciones"),
            ("docs/INITIATIVE_LIFECYCLE.md", "No copia rutinariamente historia, WORKFLOW completo, ADR completos"),
        };

        private const string SubordinationClause =
            "Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. " +
            "Ante un conflicto manda la autoridad del dominio y el conflicto se eleva como STOP.";

        private static readonly string[] CatalogKeys =
        {
            "Fuente:", "Fecha de verificación:", "Tipo de fuente:", "Nivel:", "Effort:", "Consumo cubierto:", "Retiro anunciado:",
            "Estado local:",
        };

        // ---------------------------------------------------------------- OBL-01 (INV-01): no self-approval

        [Fact]
        public void I61_OBL01_WorkerAndControllerStatesAreExactlyTheMandatedOnes()
        {
            var handoff = Schema("worker-handoff.schema.json");
            var verification = Schema("controller-verification.schema.json");

            Assert.Equal(
                new[] { "IMPLEMENTATION_COMPLETE", "PARTIAL", "BLOCKED" },
                EnumOf(handoff["properties"]!["WorkerStatus"]!));
            Assert.Equal(
                new[] { "EXECUTION_VERIFIED", "EXECUTION_REWORK_REQUIRED", "EXECUTION_BLOCKED" },
                EnumOf(verification["properties"]!["Classification"]!));
        }

        [Fact]
        public void I61_OBL01_NoSchemaEnumCarriesAGateTerm()
        {
            foreach (var file in SchemaFiles)
            {
                Assert.Empty(GateTermsInEnums(Schema(file)).Select(t => file + ": " + t));
            }
        }

        [Fact]
        public void I61_OBL01_TheGateTermOracleDetectsAMutatedEnum()
        {
            var handoff = Schema("worker-handoff.schema.json");
            ((JsonArray)handoff["properties"]!["WorkerStatus"]!["enum"]!).Add("GATE_PASS");

            Assert.NotEmpty(GateTermsInEnums(handoff));
        }

        // ---------------------------------------------------------------- OBL-02 (INV-09): strict schemas

        [Fact]
        public void I61_OBL02_EverySchemaIsStrictRecursively()
        {
            foreach (var file in SchemaFiles)
            {
                Assert.Empty(StrictnessProblems(Schema(file)).Select(p => file + ": " + p));
            }
        }

        [Fact]
        public void I61_OBL02_TheStrictnessOracleDetectsMutationsInNestedObjects()
        {
            var missingRequired = Schema("delegation.schema.json");
            var ownerRequired = (JsonArray)missingRequired["properties"]!["Owner"]!["required"]!;
            ownerRequired.RemoveAt(ownerRequired.Count - 1);
            Assert.NotEmpty(StrictnessProblems(missingRequired));

            var openObject = Schema("delegation.schema.json");
            ((JsonObject)openObject["properties"]!["Dimensions"]!).Remove("additionalProperties");
            Assert.NotEmpty(StrictnessProblems(openObject));

            var openItems = Schema("delegation.schema.json");
            ((JsonObject)openItems["properties"]!["RequiredTests"]!["items"]!)["additionalProperties"] = true;
            Assert.NotEmpty(StrictnessProblems(openItems));
        }

        // ---------------------------------------------------------------- OBL-03 (INV-02): exact-SHA identity

        [Fact]
        public void I61_OBL03_EveryShaFieldUsesExactlyTheFortyHexPattern()
        {
            foreach (var file in SchemaFiles)
            {
                Assert.Empty(ShaProblems(Schema(file)).Select(p => file + ": " + p));
            }

            Assert.Contains("BaseSha", ShaFieldNames(Schema("delegation.schema.json")));
            Assert.Contains("MainSha", ShaFieldNames(Schema("delegation.schema.json")));
            Assert.Contains("AuthorityRevision", ShaFieldNames(Schema("delegation.schema.json")));
            Assert.Contains("ChainBaseSha", ShaFieldNames(Schema("delegation.schema.json")));
            Assert.Contains("CurrentSha", ShaFieldNames(Schema("worker-handoff.schema.json")));
            Assert.Contains("RedSha", ShaFieldNames(Schema("worker-handoff.schema.json")));
            Assert.Contains("VerifiedSha", ShaFieldNames(Schema("controller-verification.schema.json")));
        }

        [Fact]
        public void I61_OBL03_TheShaOracleDetectsAWeakenedPattern()
        {
            var noPattern = Schema("delegation.schema.json");
            ((JsonObject)noPattern["properties"]!["BaseSha"]!).Remove("pattern");
            Assert.NotEmpty(ShaProblems(noPattern));

            var shortSha = Schema("worker-handoff.schema.json");
            shortSha["properties"]!["CurrentSha"]!["pattern"] = "^[0-9a-f]{7,40}$";
            Assert.NotEmpty(ShaProblems(shortSha));
        }

        // ---------------------------------------------------------------- OBL-04 (INV-05): transient traffic outside Git

        [Fact]
        public void I61_OBL04_TransientTrafficIsIgnoredAndOperationalPathsLiveUnderArtifacts()
        {
            Assert.Empty(GitignoreProblems(Read(".gitignore")));
            Assert.Empty(OperationalPathProblems(Read(ProtocolDir + "/README.md")));

            var pattern = (string)Schema("delegation.schema.json")["properties"]!["ExpectedHandoffPath"]!["pattern"]!;
            Assert.StartsWith("^artifacts/orchestration/", pattern);
        }

        [Fact]
        public void I61_OBL04_TheTransientOraclesDetectNegationsAndStrayPaths()
        {
            Assert.NotEmpty(GitignoreProblems(Read(".gitignore") + "\n!artifacts/orchestration/\n"));

            var readme = Read(ProtocolDir + "/README.md");
            Assert.NotEmpty(OperationalPathProblems(readme + "\n```text operativo\n.agent/x\n```\n"));
            Assert.NotEmpty(OperationalPathProblems(readme + "\n```text operativo\ntmp/out.json\n```\n"));
        }

        // ---------------------------------------------------------------- OBL-05 (INV-06): stable routing apart from the catalog

        [Fact]
        public void I61_OBL05_RoutingNamesNoModelAndTheCatalogIsNonNormativeAndComplete()
        {
            var catalog = Read(ProtocolDir + "/model-catalog.md");
            var models = CatalogModelIds(catalog);

            Assert.NotEmpty(models);
            Assert.Empty(ModelIdsIn(Read(ProtocolDir + "/routing.md"), models));
            Assert.Contains("NO NORMATIVO", catalog);
            Assert.Empty(CatalogEntryProblems(catalog));
        }

        [Fact]
        public void I61_OBL05_TheSeparationOraclesDetectInjectedIdsAndMissingKeys()
        {
            var catalog = Read(ProtocolDir + "/model-catalog.md");
            var models = CatalogModelIds(catalog);
            var routing = Read(ProtocolDir + "/routing.md");

            Assert.NotEmpty(ModelIdsIn(routing + "\nUsar " + models.First() + " por defecto.\n", models));
            Assert.NotEmpty(ModelIdsIn(routing + "\nUsar gpt-9-nova por defecto.\n", models));
            Assert.NotEmpty(ModelIdsIn(routing + "\nUsar claude-zeta-9 por defecto.\n", models));

            var firstKey = catalog.IndexOf("Consumo cubierto:", StringComparison.Ordinal);
            Assert.True(firstKey >= 0);
            Assert.NotEmpty(CatalogEntryProblems(catalog.Remove(firstKey, "Consumo cubierto:".Length).Insert(firstKey, "Consumo:")));
        }

        // ---------------------------------------------------------------- OBL-06 (INV-07): self-locating composition

        [Fact]
        public void I61_OBL06_WitnessPhrasesStillExistInTheirSources()
        {
            foreach (var (source, phrase) in Witnesses)
            {
                Assert.True(
                    Normalize(Read(source)).Contains(Normalize(phrase), StringComparison.Ordinal),
                    "stale witness set: '" + phrase + "' is no longer in " + source);
            }
        }

        [Fact]
        public void I61_OBL06_SectionGComposesBaseContractAndEightShortProfiles()
        {
            Assert.Empty(SectionGProblems(SectionG(Read("docs/initiatives/PROMPT_TEMPLATES.md"))));
        }

        [Fact]
        public void I61_OBL06_ProtocolTextsCopyNoNormAndCarryTheSubordinationClause()
        {
            Assert.Empty(WitnessesIn(SectionG(Read("docs/initiatives/PROMPT_TEMPLATES.md"))));

            foreach (var file in new[] { "README.md", "routing.md", "prompting-guide.md" })
            {
                Assert.Empty(WitnessesIn(Read(ProtocolDir + "/" + file)).Select(w => file + ": " + w));
            }

            foreach (var file in new[] { "README.md", "routing.md", "model-catalog.md", "prompting-guide.md" })
            {
                Assert.True(HasSubordinationClause(Read(ProtocolDir + "/" + file)), file + " must carry the subordination clause");
            }
        }

        [Fact]
        public void I61_OBL06_TheCompositionOraclesDetectMutations()
        {
            var sectionG = SectionG(Read("docs/initiatives/PROMPT_TEMPLATES.md"));

            Assert.NotEmpty(WitnessesIn(sectionG + "\n" + Witnesses[0].Phrase + "\n"));

            var longProfile = LongerProfile(sectionG, Profiles[0], 26);
            Assert.NotEmpty(SectionGProblems(longProfile));

            Assert.NotEmpty(SectionGProblems(sectionG.Replace(LifecycleFields[0], "Meta:", StringComparison.Ordinal)));

            var readme = Read(ProtocolDir + "/README.md");
            Assert.False(HasSubordinationClause(Normalize(readme).Replace(Normalize(SubordinationClause), string.Empty, StringComparison.Ordinal)));
        }

        // ---------------------------------------------------------------- OBL-11 (structural): VERIFIED needs fourteen named checks

        [Fact]
        public void I61_OBL11_VerificationChecksAreExactlyTheFourteenRequiredIds()
        {
            Assert.Empty(CheckProblems(Schema("controller-verification.schema.json")));
        }

        [Fact]
        public void I61_OBL11_TheChecksOracleDetectsAMissingCheck()
        {
            var verification = Schema("controller-verification.schema.json");
            var checks = (JsonObject)verification["properties"]!["Checks"]!;
            ((JsonObject)checks["properties"]!).Remove("Scope");
            ((JsonArray)checks["required"]!).Remove(((JsonArray)checks["required"]!).First(n => (string)n! == "Scope"));

            Assert.NotEmpty(CheckProblems(verification));
        }

        // ================================================================ oracles

        private static List<string> GateTermsInEnums(JsonNode schema)
        {
            var hits = new List<string>();
            foreach (var value in AllEnumValues(schema))
            {
                var normalized = value.Replace('_', ' ');
                foreach (var term in GateTerms)
                {
                    if (normalized.IndexOf(term.Replace('_', ' '), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hits.Add(value + " ~ " + term);
                    }
                }
            }

            return hits;
        }

        private static IEnumerable<string> AllEnumValues(JsonNode? node)
        {
            if (node is JsonObject obj)
            {
                foreach (var pair in obj)
                {
                    if (pair.Key == "enum" && pair.Value is JsonArray values)
                    {
                        foreach (var value in values.OfType<JsonValue>())
                        {
                            if (value.TryGetValue<string>(out var text))
                            {
                                yield return text;
                            }
                        }
                    }
                    else
                    {
                        foreach (var nested in AllEnumValues(pair.Value))
                        {
                            yield return nested;
                        }
                    }
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    foreach (var nested in AllEnumValues(item))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private static List<string> StrictnessProblems(JsonNode schema)
        {
            var problems = new List<string>();
            Strictness(schema, "$", problems);
            return problems;
        }

        private static void Strictness(JsonNode? node, string path, List<string> problems)
        {
            if (node is not JsonObject obj)
            {
                return;
            }

            if (TypesOf(obj).Contains("object"))
            {
                if (obj["additionalProperties"] is not JsonValue additional || !additional.TryGetValue<bool>(out var allowed) || allowed)
                {
                    problems.Add(path + ": additionalProperties must be false");
                }

                var properties = obj["properties"] as JsonObject;
                var names = properties?.Select(p => p.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
                var required = (obj["required"] as JsonArray)?.Select(n => (string)n!).OrderBy(k => k, StringComparer.Ordinal).ToArray()
                    ?? Array.Empty<string>();
                if (!names.SequenceEqual(required))
                {
                    problems.Add(path + ": required must list every property");
                }

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        Strictness(property.Value, path + "." + property.Key, problems);
                    }
                }
            }

            Strictness(obj["items"], path + "[]", problems);
        }

        private static List<string> ShaProblems(JsonNode schema)
        {
            var problems = new List<string>();
            foreach (var (name, path, node) in ShaFields(schema, "$"))
            {
                if (!TypesOf((JsonObject)node).Contains("string"))
                {
                    problems.Add(path + ": a SHA field must be a string");
                }

                if (node["pattern"] is not JsonValue pattern || !pattern.TryGetValue<string>(out var text) || text != Sha40)
                {
                    problems.Add(path + ": a SHA field must use exactly " + Sha40);
                }
            }

            return problems;
        }

        private static List<string> ShaFieldNames(JsonNode schema) => ShaFields(schema, "$").Select(f => f.Name).ToList();

        private static IEnumerable<(string Name, string Path, JsonNode Node)> ShaFields(JsonNode? node, string path)
        {
            if (node is not JsonObject obj)
            {
                yield break;
            }

            if (obj["properties"] is JsonObject properties)
            {
                foreach (var property in properties)
                {
                    if (property.Value is JsonObject && (property.Key.EndsWith("Sha", StringComparison.Ordinal) || property.Key == "AuthorityRevision"))
                    {
                        yield return (property.Key, path + "." + property.Key, property.Value);
                    }

                    foreach (var nested in ShaFields(property.Value, path + "." + property.Key))
                    {
                        yield return nested;
                    }
                }
            }

            foreach (var nested in ShaFields(obj["items"], path + "[]"))
            {
                yield return nested;
            }
        }

        private static List<string> GitignoreProblems(string gitignore)
        {
            var lines = gitignore.Split('\n').Select(l => l.Trim()).ToList();
            var problems = new List<string>();
            var index = lines.IndexOf("artifacts/");
            if (index < 0)
            {
                problems.Add(".gitignore must ignore artifacts/");
            }

            problems.AddRange(lines.Where(l => l.StartsWith("!", StringComparison.Ordinal) && l.Contains("artifacts", StringComparison.Ordinal))
                .Select(l => ".gitignore negates artifacts: " + l));
            return problems;
        }

        private static List<string> OperationalPathProblems(string readme)
        {
            var problems = new List<string>();
            var blocks = OperationalBlocks(readme);
            if (blocks.Count == 0)
            {
                problems.Add("README.md must declare its operational paths in ```text operativo blocks");
            }

            foreach (var line in blocks.SelectMany(b => b))
            {
                if (!line.StartsWith("artifacts/orchestration/", StringComparison.Ordinal))
                {
                    problems.Add("operational path outside artifacts/orchestration/: " + line);
                }

                if (line.Contains(".agent/", StringComparison.Ordinal))
                {
                    problems.Add("operational path under .agent/: " + line);
                }
            }

            return problems;
        }

        private static List<List<string>> OperationalBlocks(string text)
        {
            var blocks = new List<List<string>>();
            List<string>? current = null;
            foreach (var raw in text.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (current == null && line.Trim() == "```text operativo")
                {
                    current = new List<string>();
                }
                else if (current != null && line.Trim() == "```")
                {
                    blocks.Add(current);
                    current = null;
                }
                else if (current != null && line.Trim().Length > 0)
                {
                    current.Add(line.Trim());
                }
            }

            return blocks;
        }

        private static List<string> CatalogModelIds(string catalog) =>
            Regex.Matches(catalog, @"^### (\S+) \(", RegexOptions.Multiline).Select(m => m.Groups[1].Value).Distinct().ToList();

        private static List<string> ModelIdsIn(string text, IEnumerable<string> catalogIds)
        {
            var hits = catalogIds.Where(id => text.Contains(id, StringComparison.OrdinalIgnoreCase)).ToList();
            hits.AddRange(Regex.Matches(text, @"\b(gpt-[0-9][\w.-]*|claude-[a-z][\w.-]*|o[0-9]-[\w.-]+|gemini-[\w.-]+)", RegexOptions.IgnoreCase)
                .Select(m => m.Value));
            return hits;
        }

        private static List<string> CatalogEntryProblems(string catalog)
        {
            var problems = new List<string>();
            var headings = Regex.Matches(catalog, @"^### (\S+) \(", RegexOptions.Multiline).ToList();
            for (var i = 0; i < headings.Count; i++)
            {
                var start = headings[i].Index;
                var end = i + 1 < headings.Count ? headings[i + 1].Index : NextHeading(catalog, start, "## ");
                var entry = catalog.Substring(start, end - start);
                foreach (var key in CatalogKeys)
                {
                    if (!entry.Contains(key, StringComparison.Ordinal))
                    {
                        problems.Add(headings[i].Groups[1].Value + " lacks " + key);
                    }
                }
            }

            return problems;
        }

        private static int NextHeading(string text, int from, string prefix)
        {
            var next = text.IndexOf("\n" + prefix, from + 1, StringComparison.Ordinal);
            return next < 0 ? text.Length : next;
        }

        private static string SectionG(string promptTemplates)
        {
            var start = promptTemplates.IndexOf("\n## G. ", StringComparison.Ordinal);
            Assert.True(start >= 0, "PROMPT_TEMPLATES.md must contain section G");
            var end = promptTemplates.IndexOf("\n## ", start + 1, StringComparison.Ordinal);
            return promptTemplates.Substring(start, (end < 0 ? promptTemplates.Length : end) - start);
        }

        private static List<string> SectionGProblems(string sectionG)
        {
            var problems = new List<string>();
            var baseContract = FencedBlockAfter(sectionG, "\n### G.1 ");
            if (baseContract == null)
            {
                problems.Add("section G must open with the base delegation contract (### G.1) in a fenced block");
            }
            else
            {
                if (baseContract.Count > 40)
                {
                    problems.Add("base contract has " + baseContract.Count + " lines (max 40)");
                }

                var text = string.Join("\n", baseContract);
                problems.AddRange(LifecycleFields.Where(f => !text.Contains(f, StringComparison.Ordinal)).Select(f => "base contract lacks " + f));
            }

            foreach (var profile in Profiles)
            {
                var block = FencedBlockAfter(sectionG, "\n#### " + profile);
                if (block == null)
                {
                    problems.Add("missing profile " + profile);
                }
                else if (block.Count > 25)
                {
                    problems.Add(profile + " has " + block.Count + " lines (max 25)");
                }
            }

            return problems;
        }

        private static List<string>? FencedBlockAfter(string text, string heading)
        {
            var at = text.IndexOf(heading, StringComparison.Ordinal);
            if (at < 0)
            {
                return null;
            }

            var open = text.IndexOf("\n```", at + heading.Length, StringComparison.Ordinal);
            var nextHeading = text.IndexOf("\n#", at + heading.Length, StringComparison.Ordinal);
            if (open < 0 || (nextHeading >= 0 && nextHeading < open))
            {
                return null;
            }

            var bodyStart = text.IndexOf('\n', open + 1) + 1;
            var close = text.IndexOf("\n```", bodyStart - 1, StringComparison.Ordinal);
            if (close < 0)
            {
                return null;
            }

            return text.Substring(bodyStart, Math.Max(0, close - bodyStart)).Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        }

        private static string LongerProfile(string sectionG, string profile, int lines)
        {
            var heading = "\n#### " + profile;
            var at = sectionG.IndexOf(heading, StringComparison.Ordinal);
            var open = sectionG.IndexOf("\n```", at, StringComparison.Ordinal);
            var bodyStart = sectionG.IndexOf('\n', open + 1) + 1;
            var filler = string.Concat(Enumerable.Repeat("relleno\n", lines));
            return sectionG.Insert(bodyStart, filler);
        }

        private static List<string> WitnessesIn(string text)
        {
            var normalized = Normalize(text);
            return Witnesses.Where(w => normalized.Contains(Normalize(w.Phrase), StringComparison.Ordinal)).Select(w => w.Phrase).ToList();
        }

        private static bool HasSubordinationClause(string text) =>
            Normalize(text).Contains(Normalize(SubordinationClause), StringComparison.Ordinal);

        private static List<string> CheckProblems(JsonNode verification)
        {
            var problems = new List<string>();
            var checks = verification["properties"]?["Checks"] as JsonObject;
            if (checks == null)
            {
                return new List<string> { "Checks must be an object" };
            }

            var names = (checks["properties"] as JsonObject)?.Select(p => p.Key).ToArray() ?? Array.Empty<string>();
            var required = (checks["required"] as JsonArray)?.Select(n => (string)n!).ToArray() ?? Array.Empty<string>();
            if (!names.OrderBy(n => n, StringComparer.Ordinal).SequenceEqual(MandatoryChecks.OrderBy(n => n, StringComparer.Ordinal)))
            {
                problems.Add("Checks must have exactly the fourteen mandatory ids");
            }

            if (!required.OrderBy(n => n, StringComparer.Ordinal).SequenceEqual(MandatoryChecks.OrderBy(n => n, StringComparer.Ordinal)))
            {
                problems.Add("every mandatory check must be required");
            }

            foreach (var redCheck in new[] { "Ci", "Tests" })
            {
                if (checks["properties"]?[redCheck]?["properties"]?["RedPart"] == null)
                {
                    problems.Add(redCheck + " must carry RedPart");
                }
            }

            return problems;
        }

        // ================================================================ input

        private static JsonNode Schema(string file)
        {
            var node = JsonNode.Parse(Read(ProtocolDir + "/schemas/" + file));
            Assert.NotNull(node);
            return node!;
        }

        private static string[] EnumOf(JsonNode property) => ((JsonArray)property["enum"]!).Select(n => (string)n!).ToArray();

        private static HashSet<string> TypesOf(JsonObject obj)
        {
            var types = new HashSet<string>(StringComparer.Ordinal);
            if (obj["type"] is JsonValue single && single.TryGetValue<string>(out var one))
            {
                types.Add(one);
            }
            else if (obj["type"] is JsonArray many)
            {
                foreach (var value in many)
                {
                    types.Add((string)value!);
                }
            }

            return types;
        }

        private static string Normalize(string text) => Regex.Replace(text, @"\s+", " ");

        private static string Read(string relative)
        {
            var path = Path.Combine(I55G14FoundationConsumerTests.RepoRoot(), relative.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), relative + " must exist");
            return File.ReadAllText(path);
        }
    }
}
