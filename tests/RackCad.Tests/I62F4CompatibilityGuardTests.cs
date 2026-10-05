#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-F), C-20a: the compatibility adoption of Proposal V14 Anexo E is well formed without history — the WORKFLOW entry point,
    /// AUTOMATION_PLAN §16.13, the pointers of §16 and §16.3, the clause map and its schema (see <see cref="CompatibilityGuard"/>). The positive reads
    /// the working tree; each mutation of the C-20a row (entry point deleted or repeated, a map entry repeated, a pointer deleted, an extra ENTRY) and
    /// the other rules of the guard are applied to an in-memory overlay of the same tree and must be detected.
    /// </summary>
    public class I62F4CompatibilityGuardTests
    {
        // AUTOMATION_PLAN §16.3 as I-61 left it: docs/AUTOMATION_PLAN.md blob 07b97bd3 at origin/main bb0d5522 (the base of I-62 F4).
        private const string I61Section163Path = "tests/RackCad.Tests/I62Fixtures/i61-automation-plan-16.3.md";
        private const string I61Section163Blob = "19e195c82af9031a2ddca675db5d25f043d2aae0";

        private static string I61Section163() => I62Repo.ReadText(I61Section163Path);

        private static List<string> Ids(IEnumerable<string> violations) => violations.Select(x => x.Substring(0, x.IndexOf(':', StringComparison.Ordinal))).Distinct().ToList();

        private static OverlaySurfaceTree Overlay() => new OverlaySurfaceTree(new WorkingSurfaceTree());

        private static string Text(string path) => new WorkingSurfaceTree().Text(path)!;

        private static JsonObject Map() => (JsonObject)JsonNode.Parse(Text(CompatibilityGuard.MapPath))!;

        private static List<string> Check(ISurfaceTree tree) => CompatibilityGuard.Check(tree, I61Section163());

        [Fact]
        public void I62_C20a_TheI61Section163FixtureIsTheTextPinnedWithItsSourceBlob()
        {
            Assert.Equal(I61Section163Blob, I62Repo.GitBlobSha1(I61Section163()));
            Assert.StartsWith(CompatibilityGuard.Section163 + "\n", I61Section163(), StringComparison.Ordinal);
        }

        [Fact]
        public void I62_C20a_TheEntryPointResolverPointersAndClauseMapAreWellFormed()
        {
            Assert.Empty(Check(new WorkingSurfaceTree()));
        }

        [Fact]
        public void I62_C20a_MutationTheEntryPointDeletedOrRepeatedIsDetected()
        {
            var workflow = Text(CompatibilityGuard.Workflow);
            var lines = MarkdownSections.RawLines(workflow, CompatibilityGuard.EntryHeading)!;
            var section = string.Join("\n", lines);
            Assert.Contains("ENTRY-ONCE", Ids(Check(Overlay().With(CompatibilityGuard.Workflow, workflow.Replace(section, string.Empty, StringComparison.Ordinal)))));
            Assert.Contains("ENTRY-ONCE", Ids(Check(Overlay().With(CompatibilityGuard.Workflow, workflow.TrimEnd('\n') + "\n\n" + section + "\n"))));

            // The entry point names another heading than §16.13.
            var renamed = workflow.Replace("`" + CompatibilityGuard.ResolverHeading + "`", "`### 16.13 Compatibilidad inexistente`", StringComparison.Ordinal);
            Assert.Contains("ENTRY-NAMES-16.13", Ids(Check(Overlay().With(CompatibilityGuard.Workflow, renamed))));
        }

        [Fact]
        public void I62_C20a_MutationAPointerDeletedOrSection163ChangedIsDetected()
        {
            var plan = Text(CompatibilityGuard.Plan);
            Assert.Contains("POINTER-16", Ids(Check(Overlay().With(CompatibilityGuard.Plan, plan.Replace(CompatibilityGuard.Pointer + " ", string.Empty, StringComparison.Ordinal)))));
            Assert.Contains("POINTER-16.3", Ids(Check(Overlay().With(CompatibilityGuard.Plan, plan.Replace(" " + CompatibilityGuard.Pointer, string.Empty, StringComparison.Ordinal)))));
            var changed = plan.Replace("Si algo de esto no es verificable → STOP (S-12).", "Si algo de esto no es verificable → continuar.", StringComparison.Ordinal);
            Assert.NotEqual(plan, changed);
            Assert.Contains("16.3-LITERAL", Ids(Check(Overlay().With(CompatibilityGuard.Plan, changed))));
        }

        [Fact]
        public void I62_C20a_MutationSection1613AbsentRepeatedOrWithoutTheClosedListIsDetected()
        {
            var plan = Text(CompatibilityGuard.Plan);
            var section = string.Join("\n", MarkdownSections.RawLines(plan, CompatibilityGuard.ResolverHeading)!);
            Assert.Contains("16.13-ONCE", Ids(Check(Overlay().With(CompatibilityGuard.Plan, plan.Replace(section, string.Empty, StringComparison.Ordinal)))));
            Assert.Contains("16.13-ONCE", Ids(Check(Overlay().With(CompatibilityGuard.Plan, plan.TrimEnd('\n') + "\n\n" + section + "\n"))));
            Assert.Contains("SURFACES-16.13", Ids(Check(Overlay().With(CompatibilityGuard.Plan, plan.Replace("`docs/adr/`, ", string.Empty, StringComparison.Ordinal)))));
        }

        [Fact]
        public void I62_C20a_MutationARepeatedMapEntryOrFileAndAnExtraEntryAreDetected()
        {
            var dupEntry = Map();
            ((JsonArray)dupEntry["Entries"]!).Add(((JsonArray)dupEntry["Entries"]!)[0]!.DeepClone());
            Assert.Contains("ENTRIES-UNIQUE", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, dupEntry.ToJsonString()))));

            var dupFile = Map();
            ((JsonArray)dupFile["Files"]!).Add(((JsonArray)dupFile["Files"]!)[0]!.DeepClone());
            Assert.Contains("FILES-UNIQUE", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, dupFile.ToJsonString()))));

            var extra = Map();
            ((JsonArray)extra["Entries"]!).Add(new JsonObject
                { ["Path"] = CompatibilityGuard.Plan, ["Section"] = "## 3. Limites de seguridad", ["Level"] = 2, ["Kind"] = "ENTRY" });
            Assert.Contains("ENTRY-CLOSED", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, extra.ToJsonString()))));

            var extraFile = Map();
            var adr = ((JsonArray)extraFile["Files"]!).Select(f => (JsonObject)f!).First(f => (string)f["FileKind"]! == "ADDED");
            adr["FileKind"] = "ENTRY";
            Assert.Contains("ENTRY-FILE-CLOSED", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, extraFile.ToJsonString()))));
        }

        [Fact]
        public void I62_C20a_TheMapMustValidateAgainstItsSchemaAndKeepTheClosedSurfaces()
        {
            var unknown = Map();
            unknown["Extra"] = true;
            Assert.Contains("MAP-SCHEMA", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, unknown.ToJsonString()))));

            var kind = Map();
            ((JsonObject)((JsonArray)kind["Files"]!)[0]!)["FileKind"] = "REMOVED";
            Assert.Contains("MAP-SCHEMA", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, kind.ToJsonString()))));

            var surfaces = Map();
            ((JsonArray)surfaces["Surfaces"]!).Add("docs/ROADMAP.md");
            Assert.Contains("SURFACES-MAP", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, surfaces.ToJsonString()))));

            Assert.Contains("MAP-SCHEMA", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, null))));
            Assert.Contains("MAP-SCHEMA", Ids(Check(Overlay().With(CompatibilityGuard.SchemaPath, null))));
        }

        [Fact]
        public void I62_C20a_ASurfaceChangedWithoutRegeneratingTheMapIsDetectedByItsEffBlob()
        {
            var plan = Text(CompatibilityGuard.Plan);
            Assert.Contains("EFFBLOB", Ids(Check(Overlay().With(CompatibilityGuard.Plan, plan + "\nTexto añadido sin regenerar el mapa.\n"))));

            var baseBlob = Map();
            var modified = ((JsonArray)baseBlob["Files"]!).Select(f => (JsonObject)f!).First(f => (string)f["FileKind"]! == "MODIFIED");
            modified["BaseBlob"] = null;
            Assert.Contains("BASEBLOB-NULL", Ids(Check(Overlay().With(CompatibilityGuard.MapPath, baseBlob.ToJsonString()))));
        }

        [Fact]
        public void I62_C20a_HeadingsAreUniquePerFileInTheSurfaces()
        {
            const string path = "docs/automation/agent-execution/routing.md";
            var routing = Text(path);
            var first = MarkdownSections.Of(routing).First(s => s.Level == 2);
            var lines = MarkdownSections.Lines(routing);
            var repeated = routing.TrimEnd('\n') + "\n\n" + lines[first.Line] + "\n\nRepetida.\n";
            Assert.Contains("HEADINGS-UNIQUE", Ids(Check(Overlay().With(path, repeated))));

            // A heading inside a code fence is not a heading.
            Assert.Empty(MarkdownSections.DuplicateHeadings("## A\n\n```text\n## A\n```\n"));
        }

        [Fact]
        public void I62_C20a_TheSchemaSubsetFailsClosedOnAnUnsupportedKeyword()
        {
            var schema = JsonNode.Parse("{\"type\": \"object\", \"oneOf\": []}")!;
            Assert.Contains(MiniJsonSchema.Validate(schema, new JsonObject()), p => p.Contains("unsupported keyword oneOf", StringComparison.Ordinal));
            var strict = JsonNode.Parse("{\"type\": \"object\", \"additionalProperties\": false, \"required\": [\"A\"], \"properties\": {\"A\": {\"type\": [\"string\", \"null\"], "
                                        + "\"pattern\": \"^[0-9a-f]{40}$\"}}}")!;
            Assert.Empty(MiniJsonSchema.Validate(strict, JsonNode.Parse("{\"A\": null}")));
            Assert.Empty(MiniJsonSchema.Validate(strict, JsonNode.Parse("{\"A\": \"" + new string('a', 40) + "\"}")));
            Assert.NotEmpty(MiniJsonSchema.Validate(strict, JsonNode.Parse("{\"A\": \"x\"}")));
            Assert.NotEmpty(MiniJsonSchema.Validate(strict, JsonNode.Parse("{}")));
            Assert.NotEmpty(MiniJsonSchema.Validate(strict, JsonNode.Parse("{\"A\": null, \"B\": 1}")));
        }
    }
}
