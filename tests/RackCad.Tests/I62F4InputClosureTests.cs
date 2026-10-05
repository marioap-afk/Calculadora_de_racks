#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-41: the effective input closure and the runtime identity (<see cref="InputClosure"/>; AUTOMATION_PLAN 16.24) on the
    /// REAL authorities of this tree — (a) the closure computed over the real AGENTS.md (an automatic instruction of a Codex adapter): its three initial
    /// reads and the unit's Context Pack become transitive inputs, `git log` is allowed, `dotnet test` without an exemption stops the launch and, with
    /// one, is omitted and leaves only a publication health signal; (b) a read outside the closure; (c) a run without reads; (d) a declared identity
    /// different from the observed one; (e) a declared identity UNKNOWN; (f) a change of the AGENTS.md blob after the calculation.
    /// </summary>
    public class I62F4InputClosureTests
    {
        private sealed class Overlay : IStateTree
        {
            private readonly IStateTree under;
            private readonly Dictionary<string, byte[]> files = new Dictionary<string, byte[]>(StringComparer.Ordinal);

            public Overlay(IStateTree under) => this.under = under;

            public Overlay With(string path, string text)
            {
                files[path] = Encoding.UTF8.GetBytes(text);
                return this;
            }

            public bool TryRead(string path, out byte[] content) => files.TryGetValue(path, out content!) || under.TryRead(path, out content);
        }

        private static readonly string[] Canonical = { "docs/initiatives/I-62-A-1.md" };
        private static readonly (string, string)[] Agents = { ("AGENTS.md", "codex-cli") };
        private static readonly string[] Packs = { "docs/context-packs/documentation-governance.md" };

        private static (JsonObject Closure, ClosureDecision Decision) Compute(IStateTree? tree = null, IReadOnlyDictionary<string, string>? exemptions = null) =>
            InputClosure.Compute(tree ?? new WorkingTreeStateTree(), new string('0', 40), Canonical, Agents, Packs, readOnly: true, exemptions);

        private static List<string> Transitive(JsonObject closure) =>
            ((JsonArray)closure["AllowedTransitiveInputs"]!).Select(t => J.S((JsonObject)t!, "Path")!).ToList();

        [Fact]
        public void I62_C41_a_TheClosureOverTheRealAgentsMdHasItsInitialReadsAndTheContextPackAndStopsWithoutAnExemption()
        {
            var (closure, decision) = Compute();
            var transitive = Transitive(closure);
            foreach (var read in new[] { "docs/HANDOFF.md", "README.md", "docs/ARCHITECTURE.md", "docs/context-packs/README.md", Packs[0], "docs/WORKFLOW.md", "docs/FOUNDATIONS.md" })
            {
                Assert.Contains(read, transitive);
            }

            Assert.True((int)closure["FixedPointIterations"]! >= 2);
            var actions = ((JsonArray)closure["AllowedActions"]!).Select(a => (J.S((JsonObject)a!, "Action"), J.S((JsonObject)a!, "Class"))).ToList();
            Assert.Contains(("git log --oneline -10", "ACTION_COMPATIBLE"), actions);
            Assert.DoesNotContain(actions, a => a.Item1 == "dotnet test");
            Assert.False(decision.Launchable);
            Assert.Contains("dotnet test", decision.Stop, StringComparison.Ordinal);
            Assert.Contains(((JsonArray)closure["Obligations"]!).OfType<JsonObject>(), o => J.S(o, "Class") == "CONDITIONAL_NOT_TRIGGERED");

            // The record is a rackcad-input-closure/v1 instance.
            var schema = JsonNode.Parse(I62Repo.ReadText("docs/automation/agent-execution/schemas/input-closure.v1.schema.json"))!;
            Assert.Empty(MiniJsonSchema.Validate(schema, closure));
        }

        [Fact]
        public void I62_C41_a_WithAnExemptionTheIncompatibleActionIsOmittedAndOnlyAHealthSignalRemains()
        {
            var (closure, decision) = Compute(exemptions: new Dictionary<string, string> { ["dotnet test"] = "docs/automation/decisions/I-99.md#exención-dotnet-test" });
            Assert.True(decision.Launchable);
            Assert.Null(decision.Stop);
            Assert.Contains(((JsonArray)closure["AllowedActions"]!).OfType<JsonObject>(), a => J.S(a, "Action") == "dotnet test" && J.S(a, "Class") == "EXEMPTED");
            Assert.Single(decision.HealthSignals);
            Assert.Contains("no es evidencia local, de gate, de Candidato ni de cierre", decision.HealthSignals[0], StringComparison.Ordinal);
        }

        [Fact]
        public void I62_C41_b_c_AReadOutsideTheClosureInvalidatesTheContextAndARunWithoutReadsLeavesItUnknown()
        {
            var (closure, _) = Compute();
            bool Own(string p) => p.StartsWith("scratchpad/", StringComparison.Ordinal);
            Assert.Equal("FAITHFUL_CONTEXT", InputClosure.AuditReads(closure, new[] { Canonical[0], "docs/HANDOFF.md", "scratchpad/notas.md" }, Own).Context);
            var outside = InputClosure.AuditReads(closure, new[] { Canonical[0], "docs/automation/decisions/I-62.md" }, Own);
            Assert.Equal("INVALID_REVIEW_CONTEXT", outside.Context);
            Assert.Equal(new[] { "docs/automation/decisions/I-62.md" }, outside.Outside);
            Assert.Equal("UNKNOWN", InputClosure.AuditReads(closure, Array.Empty<string>(), Own).Context);
        }

        [Fact]
        public void I62_C41_d_e_TheObservedIdentityPrevailsAndADifferentDeclaredOneIsAContradiction()
        {
            Assert.Equal(("CONTRADICTION_S04", "codex-cli/gpt"), InputClosure.CompareIdentity("claude-opus", "codex-cli/gpt"));
            Assert.Equal(("NO_CONTRADICTION", "codex-cli/gpt"), InputClosure.CompareIdentity("UNKNOWN", "codex-cli/gpt"));
            Assert.Equal(("NO_CONTRADICTION", "codex-cli/gpt"), InputClosure.CompareIdentity(null, "codex-cli/gpt"));
            Assert.Equal(("CONSISTENT", "codex-cli/gpt"), InputClosure.CompareIdentity("codex-cli/gpt", "codex-cli/gpt"));
        }

        [Fact]
        public void I62_C41_f_AChangedAgentsMdBlobForcesTheRecalculationBeforeLaunching()
        {
            var (closure, _) = Compute();
            Assert.Empty(InputClosure.Stale(closure, new WorkingTreeStateTree()));
            var changed = new Overlay(new WorkingTreeStateTree()).With("AGENTS.md", I62Repo.ReadText("AGENTS.md").Replace("\r\n", "\n", StringComparison.Ordinal)
                + "\n## Nueva sección\n\n1. [docs/ROADMAP.md](docs/ROADMAP.md)\n");
            Assert.Equal(new[] { "AGENTS.md" }, InputClosure.Stale(closure, changed));
            var (recomputed, _) = Compute(changed);
            Assert.NotEqual(J.S((JsonObject)((JsonArray)closure["AutomaticInstructions"]!)[0]!, "Blob"), J.S((JsonObject)((JsonArray)recomputed["AutomaticInstructions"]!)[0]!, "Blob"));
        }
    }
}
