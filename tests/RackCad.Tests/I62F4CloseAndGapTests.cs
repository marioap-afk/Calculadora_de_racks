#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G): C-21 (a normative change after the MaterializationClose invalidates it, on a real disposable history) and C-37 in its F4
    /// part (a manual relay needs its custodied AUTONOMY_GAP record; without it the orchestration evidence is not valid, P-21).
    /// </summary>
    public class I62F4CloseAndGapTests
    {
        [Fact]
        public void I62_C21_ANormativeOrTemplateChangeAfterTheCloseInvalidatesItAndEvidenceDoesNot()
        {
            using var g = new GitScratch();
            var repo = System.IO.Path.Combine(g.Root, "r");
            System.IO.Directory.CreateDirectory(repo);
            g.Run(repo, "init", "-q", "-b", "main");
            g.Write(repo, "docs/AUTOMATION_PLAN.md", "# Plan\n");
            g.Write(repo, "docs/initiatives/PROMPT_TEMPLATES.md", "# Plantillas\n");
            var close = g.CommitAll(repo, "MC_I62: último cambio normativo de F4");
            var git = new GitProcessHistory(repo);

            g.Write(repo, "docs/automation/evidence/I-99-F4/README.md", "evidencia\n");
            g.Write(repo, "docs/automation/state/I-99.yml", "schema: rackcad-automation-state/v1\n");
            var evidence = g.CommitAll(repo, "evidencia y estado, sin superficies");
            Assert.Empty(MaterializationClose.Invalidating(git, close, evidence));

            g.Write(repo, "docs/initiatives/PROMPT_TEMPLATES.md", "# Plantillas\n\ncambio\n");
            var template = g.CommitAll(repo, "cambio de plantilla posterior");
            Assert.Equal(new[] { "docs/initiatives/PROMPT_TEMPLATES.md" }, MaterializationClose.Invalidating(git, close, template));
            g.Write(repo, "docs/automation/agent-execution/README.md", "# README\n");
            var normative = g.CommitAll(repo, "cambio normativo posterior");
            Assert.Equal(new[] { "docs/automation/agent-execution/README.md", "docs/initiatives/PROMPT_TEMPLATES.md" },
                MaterializationClose.Invalidating(git, close, normative));
            Assert.Empty(MaterializationClose.Invalidating(git, normative, normative));
        }

        [Fact]
        public void I62_C37_AManualRelayNeedsItsCustodiedAutonomyGapRecord()
        {
            var s4 = F8Step(4);
            Assert.Equal(new[] { L1 }, AutonomyGaps.Missing(s4, new[] { L1 }));
            Assert.Empty(AutonomyGaps.Missing(s4, Array.Empty<string>()));

            var recorded = Next(s4);
            var record = Tree(recorded).PutJson("docs/automation/evidence/I-99-agent/review/L1/1/autonomy-gap.json",
                AutonomyGaps.Record(L1, 1, "OWNER", "prompt pegado a mano en la sesión del Architect", "docs/automation/evidence/I-99-agent/review/L1/1/invocation.json",
                    new string('a', 40), "transporte bloqueado"));
            Orch(recorded)["autonomy_gaps"] = L(record);
            Assert.Empty(AutonomyGaps.Missing(recorded, new[] { L1 }));
            Assert.Empty(new StateV2Validator().ValidateFile(recorded));
            Assert.Empty(new StateV2Validator().ValidatePair(s4, recorded));

            // A record that does not resolve in the tree of the point is not custody (I-S13), and it does not count.
            var dangling = Copy(recorded);
            ((YamlMap)Y.L(dangling.State, "orchestration.autonomy_gaps")[0]!)["blob"] = new string('b', 40);
            Assert.Contains(new StateV2Validator().ValidateFile(dangling), v => v.Invariant == "I-S13");
            Assert.Equal(new[] { L1 }, AutonomyGaps.Missing(dangling, new[] { L1 }));
        }
    }
}
