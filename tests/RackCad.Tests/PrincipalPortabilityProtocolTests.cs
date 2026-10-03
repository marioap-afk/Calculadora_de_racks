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

        // Proposal V14 §17 F2 and Anexo B.1: the core schemas that F2 publishes. The adapter facts schemas under schemas/adapters/ are the provider boundary
        // (Anexo B.3), not the core.
        private static readonly string[] F2CoreSchemas = { "preflight.v1.schema.json", "relay-record.v2.schema.json", "controller-verification.v2.schema.json" };

        // Anexo B.3: AdapterFacts.Facts is the only open object of the core.
        private const string FactsPath = "$.AdapterFacts.Facts";

        // Proposal V14 §17 F3 and Anexo B.1: the core schemas that F3 publishes.
        private static readonly string[] F3CoreSchemas =
        {
            "binding.v1.schema.json", "gate-contract.v2.schema.json", "delegation.v2.schema.json", "role-invocation.v1.schema.json",
            "input-closure.v1.schema.json", "input-fidelity.v1.schema.json", "architect-review-result.v1.schema.json", "reviewer-result.v1.schema.json",
            "normative-dependency-manifest.v1.schema.json",
        };

        // Proposal V14 §20.7: the closed set of output contracts.
        private static readonly string[] OutputContracts =
        {
            "rackcad-architect-review-result/v1", "rackcad-reviewer-result/v1", "rackcad-delegation/v2", "rackcad-controller-verification/v2",
            "rackcad-worker-handoff/v1",
        };

        // Anexo B.11: the I-62 manifest of the frozen surfaces (path fixed in F3).
        private const string ManifestPath = "docs/initiatives/I-62-normative-dependency-manifest.json";

        // Proposal V14 §17 F3 and §3.1: where F3 materializes binding, independence, the checks and the role contracts (AUTOMATION_PLAN), their procedures
        // (README), the binding algorithm (routing) and the OD-6 alternative 1 predicate (LIFECYCLE).
        private static readonly (string Path, string Heading)[] F3NormativeSections =
        {
            (PlanPath, "### 16.20 Binding y aceptación (I62)"),
            (PlanPath, "### 16.21 Independencia por riesgo (I62)"),
            (PlanPath, "### 16.22 Aceptación del paquete y comprobaciones de la verificación (I62)"),
            (PlanPath, "### 16.23 Orquestación de roles (I62): invocación de rol y contratos de salida"),
            (PlanPath, "### 16.24 Orquestación de roles (I62): cierre de insumos, identidad del revisor y fidelidad"),
            ("docs/automation/agent-execution/README.md", "## 14. Binding, aceptación, independencia y comprobaciones (unidades I62)"),
            ("docs/automation/agent-execution/README.md", "## 15. Invocación de rol, cierre de insumos y fidelidad (unidades I62)"),
            ("docs/automation/agent-execution/README.md", "## 16. Validación de los resultados por rol (unidades I62)"),
            ("docs/automation/agent-execution/routing.md", "## 9. Binding por capacidad (unidades I62)"),
            ("docs/INITIATIVE_LIFECYCLE.md", "## 5. Participacion del Architect"),
        };
        private const string FreezeCommit = "4c617e82b32b6c810b68d75fc19472efed22b393";

        // Controlling edges that Proposal V14 states explicitly; the manifest must declare each one (source unit → target unit).
        private static readonly (string Source, string Target)[] ExpectedManifestEdges =
        {
            ("C-03", "§4.2"),
            ("P-15", "E.4"),
            ("I-S15", "I-H01"),
            ("T19", "§8.8"),
            ("C-11", "P-10"),
        };

        // Proposal V14 §7: the five adapters and the nine operations of the adapter contract, in their frozen order.
        private static readonly string[] Adapters = { "claude-desktop-session", "claude-subagent", "claude-cli", "codex-cli", "codex-desktop-session" };

        private static readonly string[] AdapterOperations =
        {
            "describir", "observar", "renderizar", "invocar", "observar el resultado", "cancelar", "confirmar la terminación", "clasificar procesos",
            "declarar la huella",
        };

        private static readonly string[] OperationStates = { "DISPONIBLE", "NO APLICA", "UNVERIFIED" };

        // C-10 (i): property names that would carry a credential. KeyNames carries names, never values, and is not one of them.
        private static readonly string[] CredentialFieldPatterns =
        {
            "password", "passwd", "secret", "token", "apikey", "api_key", "credential", "bearer", "cookie", "privatekey", "private_key", "accesskey",
        };

        // C-19: the five I61 schemas and the I-61 guard class, pinned by their Git blobs (LF, as stored).
        private static readonly (string Path, string Blob)[] PinnedI61Files =
        {
            ("docs/automation/agent-execution/schemas/gate-contract.schema.json", "56ced893586abc4a571d842f631e435342906f6a"),
            ("docs/automation/agent-execution/schemas/delegation.schema.json", "62c026764a3b917c9b4ffeb6df049247ea613869"),
            ("docs/automation/agent-execution/schemas/worker-handoff.schema.json", "cc3c9cf76b2247f3312e99bd447554326cc3700a"),
            ("docs/automation/agent-execution/schemas/controller-verification.schema.json", "72747225daa2a83ba139ec1ffacb57d8943cf0af"),
            ("docs/automation/agent-execution/schemas/relay-record.schema.json", "2deba19606d058fd82b389687934b0c3dfef8c06"),
            ("tests/RackCad.Tests/AgentExecutionProtocolTests.cs", "c4af8853b466fa15039fdf0cfda7e4cfda0679ae"),
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

        // ---------------------------------------------------------------- C-05: neutral and strict core schemas

        [Fact]
        public void I62_C05_CoreSchemasAreStrictExceptFactsAndUseExactHashPatterns()
        {
            foreach (var file in F2CoreSchemas)
            {
                var schema = CoreSchema(file);
                Assert.Empty(StrictnessProblems(schema).Select(p => file + ": " + p));
                Assert.Empty(UnwalkedKeywords(schema).Select(k => file + ": uses " + k));
                Assert.Empty(HashPatternProblems(schema).Select(p => file + ": " + p));
            }
        }

        [Fact]
        public void I62_C05_CoreSchemaEnumsCarryNoProviderMark()
        {
            foreach (var file in F2CoreSchemas)
            {
                Assert.Empty(ProviderMarksIn(string.Join("\n", EnumValues(CoreSchema(file)))).Select(m => file + ": " + m));
            }
        }

        [Fact]
        public void I62_C05_AdapterFactsRequiresAStrictSchemaRefAndFactsIsTheOnlyOpenObject()
        {
            Assert.Empty(SchemaRefProblems(CoreSchema("preflight.v1.schema.json")));
        }

        [Fact]
        public void I62_C05_TheCoreOraclesDetectAMarkAnOpenObjectALooseHashAndAMissingSchemaRef()
        {
            var marked = CoreSchema("relay-record.v2.schema.json");
            ((JsonArray)marked["properties"]!["Participant"]!["properties"]!["Transport"]!["enum"]!).Add("codex-cli");
            Assert.NotEmpty(ProviderMarksIn(string.Join("\n", EnumValues(marked))));

            var open = CoreSchema("preflight.v1.schema.json");
            ((JsonObject)open["properties"]!["Host"]!).Remove("additionalProperties");
            Assert.NotEmpty(StrictnessProblems(open));

            var openFingerprint = CoreSchema("relay-record.v2.schema.json");
            ((JsonObject)openFingerprint["properties"]!["Exit"]!["properties"]!["Fingerprint"]!)["additionalProperties"] = true;
            Assert.NotEmpty(StrictnessProblems(openFingerprint));

            var loose = CoreSchema("controller-verification.v2.schema.json");
            loose["properties"]!["VerifiedSha"]!["pattern"] = "^[0-9a-f]{7,40}$";
            Assert.NotEmpty(HashPatternProblems(loose));

            var noRef = CoreSchema("preflight.v1.schema.json");
            var factsRequired = (JsonArray)noRef["properties"]!["AdapterFacts"]!["required"]!;
            factsRequired.Remove(factsRequired.First(n => (string)n! == "SchemaRef"));
            Assert.NotEmpty(SchemaRefProblems(noRef));

            var closedFacts = CoreSchema("preflight.v1.schema.json");
            ((JsonObject)closedFacts["properties"]!["AdapterFacts"]!["properties"]!["Facts"]!)["properties"] = new JsonObject { ["Extra"] = new JsonObject { ["type"] = "string" } };
            Assert.NotEmpty(SchemaRefProblems(closedFacts));
        }

        // ---------------------------------------------------------------- C-09: complete descriptors

        [Fact]
        public void I62_C09_EachFrozenAdapterHasADescriptorWithTheNineOperationsAndAStrictFactsSchema()
        {
            Assert.Equal(Adapters.OrderBy(a => a, StringComparer.Ordinal), DescriptorIds().OrderBy(a => a, StringComparer.Ordinal));
            foreach (var adapter in Adapters)
            {
                var descriptor = Read(ProtocolDir + "/adapters/" + adapter + ".md");
                Assert.Empty(DescriptorProblems(adapter, descriptor).Select(p => adapter + ": " + p));
                Assert.Empty(FactsSchemaProblems(adapter, descriptor).Select(p => adapter + ": " + p));
            }
        }

        [Fact]
        public void I62_C09_TheDescriptorOracleDetectsAMissingOperationAnInvalidStateAndABrokenFactsSchema()
        {
            var adapter = Adapters[3];
            var descriptor = Read(ProtocolDir + "/adapters/" + adapter + ".md");
            var lines = descriptor.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

            var missing = string.Join("\n", lines.Where(l => !l.StartsWith("| 6 | cancelar |", StringComparison.Ordinal)));
            Assert.NotEmpty(DescriptorProblems(adapter, missing));

            var invalid = string.Join("\n", lines.Select(l => l.StartsWith("| 9 | declarar la huella |", StringComparison.Ordinal) ? l.Replace("| DISPONIBLE |", "| QUIZÁ |", StringComparison.Ordinal) : l));
            Assert.NotEmpty(DescriptorProblems(adapter, invalid));

            var unschema = string.Join("\n", lines.Select(l => l.Replace(".facts.v1.schema.json", ".facts.v9.schema.json", StringComparison.Ordinal)));
            Assert.NotEmpty(FactsSchemaProblems(adapter, unschema));
        }

        // ---------------------------------------------------------------- C-10 (i): no credential field

        [Fact]
        public void I62_C10_NoI62SchemaDeclaresACredentialField()
        {
            var schemas = I62SchemaFiles().ToList();
            Assert.True(schemas.Count >= F2CoreSchemas.Length + Adapters.Length, "the F2 schemas must exist");
            foreach (var file in schemas)
            {
                Assert.Empty(CredentialFields(JsonNode.Parse(File.ReadAllText(file))).Select(f => Path.GetFileName(file) + ": " + f));
            }
        }

        [Fact]
        public void I62_C10_TheCredentialOracleDetectsAnInjectedField()
        {
            var schema = CoreSchema("preflight.v1.schema.json");
            ((JsonObject)schema["properties"]!["Fingerprint"]!["properties"]!)["ApiKey"] = new JsonObject { ["type"] = "string" };
            Assert.NotEmpty(CredentialFields(schema));
        }

        // ---------------------------------------------------------------- C-19: /v1 intact

        [Fact]
        public void I62_C19_TheFiveI61SchemasAndTheI61GuardClassKeepTheirBlobs()
        {
            foreach (var (path, blob) in PinnedI61Files)
            {
                Assert.True(GitBlobSha1(Read(path)) == blob, path + " changed: the I61 contracts and their tests are pinned");
            }
        }

        // ---------------------------------------------------------------- F3: the I62 contracts (Anexo B.1, B.5, B.7, B.9, B.10, B.11)

        [Fact]
        public void I62_F3_CoreSchemasAreStrictNeutralAndUseExactHashPatterns()
        {
            foreach (var file in F3CoreSchemas)
            {
                var schema = CoreSchema(file);
                Assert.Empty(StrictnessProblems(schema).Select(p => file + ": " + p));
                Assert.Empty(UnwalkedKeywords(schema).Select(k => file + ": uses " + k));
                Assert.Empty(HashPatternProblems(schema).Select(p => file + ": " + p));
                Assert.Empty(ProviderMarksIn(string.Join("\n", EnumValues(schema))).Select(m => file + ": " + m));
                Assert.Empty(CredentialFields(schema).Select(f => file + ": " + f));
            }
        }

        [Fact]
        public void I62_F3_OutputContractsAreTheClosedSetAndResultsFixTheirRoleAndAction()
        {
            Assert.Empty(OutputContractProblems(
                CoreSchema("role-invocation.v1.schema.json"), CoreSchema("architect-review-result.v1.schema.json"), CoreSchema("reviewer-result.v1.schema.json")));
        }

        [Fact]
        public void I62_F3_TheContractOraclesDetectAVerdictInTheReviewerResultAndAnExtraOutputContract()
        {
            var invocation = CoreSchema("role-invocation.v1.schema.json");
            var architect = CoreSchema("architect-review-result.v1.schema.json");
            var reviewer = CoreSchema("reviewer-result.v1.schema.json");

            var withVerdict = CoreSchema("reviewer-result.v1.schema.json");
            ((JsonObject)withVerdict["properties"]!)["Verdict"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("AGREED") };
            Assert.NotEmpty(OutputContractProblems(invocation, architect, withVerdict));

            var extra = CoreSchema("role-invocation.v1.schema.json");
            ((JsonArray)extra["properties"]!["OutputContract"]!["enum"]!).Add("rackcad-reviewer-result/v2");
            Assert.NotEmpty(OutputContractProblems(extra, architect, reviewer));

            var looseCommit = CoreSchema("binding.v1.schema.json");
            looseCommit["properties"]!["PreflightRef"]!["properties"]!["Location"]!["properties"]!["Commit"]!["pattern"] = "^[0-9a-f]+$";
            Assert.NotEmpty(HashPatternProblems(looseCommit));
        }

        // ---------------------------------------------------------------- B.11: the I-62 normative dependency manifest

        [Fact]
        public void I62_B11_TheManifestIsClosedUniqueAndAnchoredInTheFrozenProposal()
        {
            Assert.Empty(ManifestProblems(Manifest(), Read(FreezePath)));
        }

        [Fact]
        public void I62_B11_TheManifestDeclaresTheExplicitControllingEdgesOfTheProposal()
        {
            Assert.Empty(MissingEdges(Manifest(), ExpectedManifestEdges));
        }

        [Fact]
        public void I62_B11_TheManifestOracleDetectsMissingEntryDanglingTargetDuplicateUnitAmbiguityAndAnUndeclaredDependency()
        {
            var freeze = Read(FreezePath);
            var entries = (JsonArray)Manifest()["Entries"]!;
            Assert.True(entries.Count > 0);

            // Missing entry: drop the entry of a unit that some entry depends on.
            var missing = Manifest();
            var target = FirstUnitTarget(missing);
            var targetKey = UnitKey(target);
            var missingEntries = (JsonArray)missing["Entries"]!;
            missingEntries.Remove(missingEntries.First(e => UnitKey(e!["Source"]!) == targetKey));
            Assert.NotEmpty(ManifestProblems(missing, freeze));

            // Dangling target: a dependency on a unit of the frozen Proposal under an anchor that does not exist there.
            var dangling = Manifest();
            var danglingTarget = FirstUnitTarget(dangling);
            danglingTarget["UnitId"] = "§99.9#p1";
            danglingTarget["Anchor"] = "§99.9";
            Assert.NotEmpty(ManifestProblems(dangling, freeze));

            // Duplicate unit: the same source twice.
            var duplicate = Manifest();
            var duplicateEntries = (JsonArray)duplicate["Entries"]!;
            duplicateEntries.Add(duplicateEntries[0]!.DeepClone());
            Assert.NotEmpty(ManifestProblems(duplicate, freeze));

            // Ambiguous reference: an anchor that matches two headings of the document.
            Assert.NotEmpty(ManifestProblems(Manifest(), freeze + "\n## 4. Perfil duplicado\n"));

            // Undeclared controlling dependency: the edge C-03 → §4.2 removed.
            var undeclared = Manifest();
            foreach (var entry in ((JsonArray)undeclared["Entries"]!).Where(e => (string?)e!["Source"]!["UnitId"] == ExpectedManifestEdges[0].Source))
            {
                var depends = (JsonArray)entry!["DependsOn"]!;
                foreach (var edge in depends.Where(d => (string?)d!["Target"]!["Unit"]?["UnitId"] == ExpectedManifestEdges[0].Target).ToList())
                {
                    depends.Remove(edge);
                }
            }

            Assert.NotEmpty(MissingEdges(undeclared, ExpectedManifestEdges));
        }

        // ---------------------------------------------------------------- F3: the materialized texts (plane b, inactive)

        [Fact]
        public void I62_F3_TheNormativeTextsAreMaterializedOnceAndCarryNoProviderMark()
        {
            foreach (var (path, heading) in F3NormativeSections)
            {
                var section = SectionLines(Read(path), heading);
                Assert.True(section.Count > 1, path + ": «" + heading + "» must exist exactly once");
                Assert.Empty(ProviderMarksIn(string.Join("\n", section)).Select(m => heading + ": " + m));
            }

            // Mutations: a duplicated heading is not «exactly once», and a provider name inside a section is a mark.
            var (planPath, planHeading) = F3NormativeSections[0];
            var plan = Read(planPath);
            Assert.Empty(SectionLines(plan + "\n" + planHeading + "\n", planHeading));
            Assert.NotEmpty(ProviderMarksIn(string.Join("\n", SectionLines(plan, planHeading)) + "\nCodex"));
        }

        // ================================================================ oracles

        private static List<string>? RoleTable(string plan) => TableAfter(SectionLines(plan, PlanRolesSection), RolesHeader);

        // Proposal V14 §20.7: the invocation names exactly the five output contracts; each review result fixes its role and action; the reviewer result
        // has no verdict (B.10.2).
        private static List<string> OutputContractProblems(JsonNode invocation, JsonNode architect, JsonNode reviewer)
        {
            var problems = new List<string>();
            var outputs = (invocation["properties"]?["OutputContract"]?["enum"] as JsonArray)?.Select(n => (string)n!).ToList() ?? new List<string>();
            if (outputs.Count != OutputContracts.Length || OutputContracts.Any(o => !outputs.Contains(o)))
            {
                problems.Add("OutputContract must be exactly the closed set of §20.7");
            }

            foreach (var (schema, role, action) in new[] { (architect, "ARCHITECT", "REVIEW_DESIGN"), (reviewer, "REVIEWER", "REVIEW_CHANGE") })
            {
                var roles = (schema["properties"]?["RequestedRole"]?["enum"] as JsonArray)?.Select(n => (string)n!).ToArray() ?? Array.Empty<string>();
                var actions = (schema["properties"]?["Action"]?["enum"] as JsonArray)?.Select(n => (string)n!).ToArray() ?? Array.Empty<string>();
                if (!roles.SequenceEqual(new[] { role }) || !actions.SequenceEqual(new[] { action }))
                {
                    problems.Add((string?)schema["title"] + " must fix " + role + " / " + action);
                }
            }

            var verdicts = (architect["properties"]?["Verdict"]?["enum"] as JsonArray)?.Select(n => (string)n!).ToArray() ?? Array.Empty<string>();
            if (!verdicts.SequenceEqual(new[] { "AGREED", "CHANGES REQUIRED", "BLOCKED — OWNER DECISION" }))
            {
                problems.Add("the architect result must use the LIFECYCLE verdicts");
            }

            if ((reviewer["properties"] as JsonObject)?.ContainsKey("Verdict") ?? true)
            {
                problems.Add("the reviewer result must not admit a Verdict");
            }

            return problems;
        }

        private static JsonNode Manifest()
        {
            var node = JsonNode.Parse(Read(ManifestPath));
            Assert.NotNull(node);
            return node!;
        }

        private static string UnitKey(JsonNode unit) => (string?)unit["Document"] + "|" + (string?)unit["UnitId"];

        private static JsonNode FirstUnitTarget(JsonNode manifest) =>
            ((JsonArray)manifest["Entries"]!).SelectMany(e => ((JsonArray)e!["DependsOn"]!).Select(d => d!["Target"]!))
                .First(t => (string?)t["Kind"] == "UNIT" && (string?)t["Unit"]!["Document"] == FreezePath)["Unit"]!;

        // Anexo B.11 validation that needs no Git history: unique sources, closure under DependsOn, one shape per target kind, proposed targets taken
        // from §3.1, and units of the frozen Proposal pinned to its revision and anchored in exactly one of its headings.
        private static List<string> ManifestProblems(JsonNode manifest, string freeze)
        {
            var problems = new List<string>();
            if ((string?)manifest["Schema"] != "rackcad-normative-dependency-manifest/v1")
            {
                problems.Add("Schema must be rackcad-normative-dependency-manifest/v1");
            }

            var scope = (manifest["Scope"] as JsonArray)?.Select(n => (string)n!).ToHashSet(StringComparer.Ordinal) ?? new HashSet<string>();
            var entries = (manifest["Entries"] as JsonArray)?.OfType<JsonNode>().ToList() ?? new List<JsonNode>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (!keys.Add(UnitKey(entry["Source"]!)))
                {
                    problems.Add("duplicate unit " + UnitKey(entry["Source"]!));
                }
            }

            var anchors = ProposalAnchors(freeze);
            var proposed = ProposedTargets(freeze);
            foreach (var entry in entries)
            {
                problems.AddRange(UnitRefProblems(entry["Source"]!, scope, anchors, "source"));
                foreach (var target in (entry["DependsOn"] as JsonArray)?.OfType<JsonNode>().Select(d => d["Target"]!) ?? Enumerable.Empty<JsonNode>())
                {
                    var (unit, proposal, whole) = (target["Unit"], target["Proposed"], target["WholeDocument"]);
                    switch ((string?)target["Kind"])
                    {
                        case "UNIT" when unit != null && proposal == null && whole == null:
                            problems.AddRange(UnitRefProblems(unit, scope, anchors, "target"));
                            if (!keys.Contains(UnitKey(unit)))
                            {
                                problems.Add("missing entry for " + UnitKey(unit));
                            }

                            break;
                        case "PROPOSED" when proposal != null && unit == null && whole == null:
                            if (!proposed.Contains((string?)proposal["FutureDocument"] + "|" + Normalize((string?)proposal["FutureAnchor"] ?? string.Empty)))
                            {
                                problems.Add("proposed target outside §3.1: " + (string?)proposal["FutureAnchor"]);
                            }

                            break;
                        case "WHOLE_DOCUMENT" when whole != null && unit == null && proposal == null:
                            var bounded = (string?)whole["Class"] == "BOUNDED_ENTRY_SET" && ((whole["EntrySet"] as JsonArray)?.Count ?? 0) > 0;
                            var composite = (string?)whole["Class"] == "COMPOSITE_RULE" && whole["CompositeRuleRef"] != null;
                            if (!bounded && !composite)
                            {
                                problems.Add("a whole document needs an entry set or a composite rule: " + (string?)whole["Document"]);
                            }

                            break;
                        default:
                            problems.Add("target with an inconsistent kind: " + (string?)target["Kind"]);
                            break;
                    }
                }
            }

            return problems;
        }

        private static IEnumerable<string> UnitRefProblems(JsonNode unit, HashSet<string> scope, Dictionary<string, int> anchors, string role)
        {
            var document = (string?)unit["Document"];
            if (document == null || !scope.Contains(document))
            {
                yield return role + " document outside Scope: " + document;
            }

            if (document == FreezePath)
            {
                if ((string?)unit["Revision"]?["Blob"] != FreezeBlob || (string?)unit["Revision"]?["Commit"] != FreezeCommit)
                {
                    yield return role + " is not pinned to the Freeze: " + (string?)unit["UnitId"];
                }

                var anchor = (string?)unit["Anchor"] ?? string.Empty;
                anchors.TryGetValue(anchor, out var count);
                if (count == 0)
                {
                    yield return "dangling " + role + ": " + anchor + " is not a heading of the Proposal";
                }
                else if (count > 1)
                {
                    yield return "ambiguous " + role + ": " + anchor + " matches " + count + " headings";
                }
            }
        }

        private static List<string> MissingEdges(JsonNode manifest, IEnumerable<(string Source, string Target)> expected)
        {
            var edges = ((JsonArray)manifest["Entries"]!)
                .Where(e => (string?)e!["Source"]!["Document"] == FreezePath)
                .SelectMany(e => ((JsonArray)e!["DependsOn"]!).Select(d => ((string?)e["Source"]!["UnitId"], (string?)d!["Target"]!["Unit"]?["UnitId"])))
                .ToHashSet();
            return expected.Where(x => !edges.Contains((x.Source, x.Target))).Select(x => x.Source + " → " + x.Target + " is not declared").ToList();
        }

        // Section anchors of the Proposal: «§N», «§N.M», «Anexo X» and «X.N…», from its headings outside fenced blocks.
        private static Dictionary<string, int> ProposalAnchors(string text)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            var inFence = false;
            foreach (var line in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
            {
                if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    inFence = !inFence;
                    continue;
                }

                var m = inFence ? Match.Empty : Regex.Match(line, @"^#{2,4} (?:Anexo ([A-G]) —|([0-9]+(?:\.[0-9]+)*)\.? |([A-G](?:\.[0-9]+)+) )");
                if (m.Success)
                {
                    var anchor = m.Groups[1].Success ? "Anexo " + m.Groups[1].Value : m.Groups[2].Success ? "§" + m.Groups[2].Value : m.Groups[3].Value;
                    counts[anchor] = counts.TryGetValue(anchor, out var c) ? c + 1 : 1;
                }
            }

            return counts;
        }

        // Proposal V14 §3.1: «FutureDocument|FutureAnchor» of each proposed normative target.
        private static HashSet<string> ProposedTargets(string freeze)
        {
            var section = SectionLines(freeze, "### 3.1 Destinos normativos propuestos (`ProposedNormativeTarget`; A62-V11-01)");
            var rows = TableAfter(section, "| `FutureDocument` | `FutureAnchor` | `DesignSource` | `State` |") ?? new List<string>();
            return rows.Skip(2).Select(r => r.Trim('|').Split('|').Select(c => c.Trim()).ToArray())
                .Select(c => c[0].Trim('`') + "|" + Normalize(c[1])).ToHashSet(StringComparer.Ordinal);
        }

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

        // The I62 core: every schema of the protocol directory except the five I61 ones and the adapter facts schemas (Anexo B.3).
        private static IEnumerable<string> CoreSchemaFiles() =>
            I62SchemaFiles().Where(f => !f.Replace('\\', '/').Contains("/schemas/adapters/", StringComparison.Ordinal));

        private static IEnumerable<string> I62SchemaFiles()
        {
            var schemas = Path.Combine(RepoPath(ProtocolDir), "schemas");
            return Directory.GetFiles(schemas, "*.json", SearchOption.AllDirectories)
                .Where(f => !I61Schemas.Contains(Path.GetRelativePath(schemas, f).Replace('\\', '/'), StringComparer.Ordinal))
                .OrderBy(f => f, StringComparer.Ordinal);
        }

        private static JsonNode CoreSchema(string file)
        {
            var node = JsonNode.Parse(Read(ProtocolDir + "/schemas/" + file));
            Assert.NotNull(node);
            return node!;
        }

        // Every object is closed (additionalProperties false) and requires every property, except the declared open point AdapterFacts.Facts.
        private static List<string> StrictnessProblems(JsonNode schema)
        {
            var problems = new List<string>();
            Strictness(schema, "$", problems);
            return problems;
        }

        private static void Strictness(JsonNode? node, string path, List<string> problems)
        {
            if (node is not JsonObject obj || path == FactsPath)
            {
                return;
            }

            if (TypesOf(obj).Contains("object") || obj["properties"] is JsonObject)
            {
                if (obj["additionalProperties"] is not JsonValue additional || !additional.TryGetValue<bool>(out var allowed) || allowed)
                {
                    problems.Add(path + ": additionalProperties must be false");
                }

                var properties = obj["properties"] as JsonObject;
                var names = properties?.Select(p => p.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
                var required = (obj["required"] as JsonArray)?.Select(n => (string)n!).OrderBy(k => k, StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
                if (!names.SequenceEqual(required))
                {
                    problems.Add(path + ": required must list every property");
                }

                foreach (var property in properties ?? new JsonObject())
                {
                    Strictness(property.Value, path + "." + property.Key, problems);
                }
            }

            Strictness(obj["items"], path + "[]", problems);
        }

        private static readonly string[] UnwalkedSchemaKeywords =
            { "$defs", "definitions", "$ref", "anyOf", "oneOf", "allOf", "not", "if", "then", "else", "prefixItems", "patternProperties" };

        private static List<string> UnwalkedKeywords(JsonNode? node)
        {
            var found = new List<string>();
            if (node is JsonObject obj)
            {
                foreach (var (key, child) in obj)
                {
                    if (key == "properties" && child is JsonObject properties)
                    {
                        found.AddRange(properties.SelectMany(p => UnwalkedKeywords(p.Value)));
                        continue;
                    }

                    if (UnwalkedSchemaKeywords.Contains(key, StringComparer.Ordinal))
                    {
                        found.Add(key);
                    }

                    found.AddRange(UnwalkedKeywords(child));
                }
            }
            else if (node is JsonArray array)
            {
                found.AddRange(array.SelectMany(UnwalkedKeywords));
            }

            return found;
        }

        // Anexo B.1: a Git id (…Sha, …Blob, AuthorityRevision) is exactly 40 lowercase hex; a SHA-256 (…Sha256, …Hash) is 64 lowercase hex,
        // optionally with explicit literal states such as UNKNOWN.
        private static List<string> HashPatternProblems(JsonNode schema)
        {
            var problems = new List<string>();
            foreach (var (name, path, node) in NamedProperties(schema, "$"))
            {
                var pattern = node["pattern"] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
                if (name.EndsWith("Sha256", StringComparison.Ordinal) || name.EndsWith("Hash", StringComparison.Ordinal))
                {
                    if (pattern == null || !pattern.Contains("[0-9a-f]{64}", StringComparison.Ordinal) || pattern.Contains("A-F", StringComparison.Ordinal)
                        || !pattern.StartsWith("^", StringComparison.Ordinal) || !pattern.EndsWith("$", StringComparison.Ordinal))
                    {
                        problems.Add(path + ": a SHA-256 must use 64 lowercase hex");
                    }
                }
                else if (name.EndsWith("Sha", StringComparison.Ordinal) || name.EndsWith("Blob", StringComparison.Ordinal) || name == "AuthorityRevision"
                         || name == "Commit" || name == "commit" || name == "blob")
                {
                    if (pattern != "^[0-9a-f]{40}$")
                    {
                        problems.Add(path + ": a Git id must use exactly ^[0-9a-f]{40}$");
                    }
                }
            }

            return problems;
        }

        private static IEnumerable<(string Name, string Path, JsonObject Node)> NamedProperties(JsonNode? node, string path)
        {
            if (node is not JsonObject obj)
            {
                yield break;
            }

            foreach (var (key, child) in obj["properties"] as JsonObject ?? new JsonObject())
            {
                if (child is JsonObject childObject)
                {
                    yield return (key, path + "." + key, childObject);
                    foreach (var nested in NamedProperties(childObject, path + "." + key))
                    {
                        yield return nested;
                    }
                }
            }

            foreach (var nested in NamedProperties(obj["items"], path + "[]"))
            {
                yield return nested;
            }
        }

        // Anexo B.3: AdapterFacts = {SchemaRef (strict), Facts: {"type": "object"}}, both required.
        private static List<string> SchemaRefProblems(JsonNode preflight)
        {
            var problems = new List<string>();
            var facts = preflight["properties"]?["AdapterFacts"] as JsonObject;
            if (facts == null)
            {
                return new List<string> { "AdapterFacts is missing" };
            }

            var required = (facts["required"] as JsonArray)?.Select(n => (string)n!).ToList() ?? new List<string>();
            if (!required.Contains("SchemaRef") || !required.Contains("Facts"))
            {
                problems.Add("AdapterFacts must require SchemaRef and Facts");
            }

            var schemaRef = facts["properties"]?["SchemaRef"] as JsonObject;
            var refNames = (schemaRef?["properties"] as JsonObject)?.Select(p => p.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
            if (!refNames.SequenceEqual(new[] { "Blob", "Path", "SchemaId" }) || StrictnessProblems(schemaRef ?? new JsonObject()).Count > 0)
            {
                problems.Add("SchemaRef must be the strict object {SchemaId, Path, Blob}");
            }

            var open = facts["properties"]?["Facts"] as JsonObject;
            if (open == null || (string?)open["type"] != "object" || open.Any(p => p.Key != "type" && p.Key != "description"))
            {
                problems.Add("Facts must be exactly the open object {\"type\": \"object\"}");
            }

            return problems;
        }

        private static IEnumerable<string> DescriptorIds() =>
            Directory.Exists(Path.Combine(RepoPath(ProtocolDir), "adapters"))
                ? Directory.GetFiles(Path.Combine(RepoPath(ProtocolDir), "adapters"), "*.md").Select(f => Path.GetFileNameWithoutExtension(f))
                : Enumerable.Empty<string>();

        // A descriptor names its AdapterId and declares the nine operations, in order, each DISPONIBLE, NO APLICA or UNVERIFIED (Proposal V14 §7).
        private static List<string> DescriptorProblems(string adapter, string descriptor)
        {
            var problems = new List<string>();
            var lines = descriptor.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
            var id = Regex.Match(descriptor, @"\*\*AdapterId:\*\* `([^`]+)`");
            if (!id.Success || id.Groups[1].Value != adapter)
            {
                problems.Add("the descriptor must declare **AdapterId:** `" + adapter + "`");
            }

            var table = TableAfter(lines, "| # | Operación | Estado | Evidencia |");
            var rows = table?.Skip(2).Select(r => r.Trim('|').Split('|').Select(c => c.Trim()).ToArray()).ToList() ?? new List<string[]>();
            for (var i = 0; i < AdapterOperations.Length; i++)
            {
                var row = rows.FirstOrDefault(r => r.Length >= 3 && r[0] == (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (row == null || row[1] != AdapterOperations[i])
                {
                    problems.Add("operation " + (i + 1) + " («" + AdapterOperations[i] + "») is missing");
                }
                else if (!OperationStates.Contains(row[2], StringComparer.Ordinal))
                {
                    problems.Add("operation " + (i + 1) + " has an invalid state «" + row[2] + "»");
                }
            }

            if (rows.Count != AdapterOperations.Length)
            {
                problems.Add("the operation table must have exactly nine rows");
            }

            return problems;
        }

        // The facts schema lives at the path derived from the id and the version the descriptor declares (Anexo B.2), is strict and fixes its SchemaId.
        private static List<string> FactsSchemaProblems(string adapter, string descriptor)
        {
            var refs = Regex.Matches(descriptor, @"schemas/adapters/" + Regex.Escape(adapter) + @"\.facts\.v([0-9]+)\.schema\.json").Select(m => m.Groups[1].Value).Distinct().ToList();
            if (refs.Count != 1)
            {
                return new List<string> { "the descriptor must declare exactly one facts schema version" };
            }

            var relative = ProtocolDir + "/schemas/adapters/" + adapter + ".facts.v" + refs[0] + ".schema.json";
            if (!File.Exists(RepoPath(relative)))
            {
                return new List<string> { relative + " does not exist" };
            }

            var schema = JsonNode.Parse(File.ReadAllText(RepoPath(relative)))!;
            var problems = StrictnessProblems(schema).Select(p => "facts schema: " + p).ToList();
            problems.AddRange(UnwalkedKeywords(schema).Select(k => "facts schema uses " + k));
            var schemaId = schema["properties"]?["SchemaId"]?["enum"] as JsonArray;
            if (schemaId == null || schemaId.Count != 1 || (string?)schemaId[0] != "rackcad-adapter-" + adapter + "-facts/v" + refs[0])
            {
                problems.Add("facts schema must fix SchemaId = rackcad-adapter-" + adapter + "-facts/v" + refs[0]);
            }

            return problems;
        }

        private static List<string> CredentialFields(JsonNode? node)
        {
            var found = new List<string>();
            if (node is JsonObject obj)
            {
                foreach (var (key, child) in obj)
                {
                    if (key == "properties" && child is JsonObject properties)
                    {
                        foreach (var (name, property) in properties)
                        {
                            if (CredentialFieldPatterns.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase)))
                            {
                                found.Add(name);
                            }

                            found.AddRange(CredentialFields(property));
                        }

                        continue;
                    }

                    found.AddRange(CredentialFields(child));
                }
            }
            else if (node is JsonArray array)
            {
                found.AddRange(array.SelectMany(CredentialFields));
            }

            return found;
        }

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
