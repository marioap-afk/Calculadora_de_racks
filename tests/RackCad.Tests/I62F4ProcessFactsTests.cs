#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-17: the classification of a process snapshot by README §3.2 and AUTOMATION_PLAN 16.4 (<see cref="ProcessFacts"/>), on
    /// synthetic snapshots so that it runs on any runner. The reproducible control with real processes of this host is
    /// <c>docs/automation/evidence/I-62-F4/c17/c17-processes.ps1</c>.
    /// </summary>
    public class I62F4ProcessFactsTests
    {
        private const string Worktree = @"C:\Users\x\.codex\worktrees\unidad";
        private static readonly DateTime T0 = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

        private static ProcessRow P(int pid, int parent, string name, string? line, int minute = 0) => new ProcessRow(pid, parent, name, line, T0.AddMinutes(minute));

        private static readonly List<ProcessRow> Snapshot = new List<ProcessRow>
        {
            P(80, 1, "claude.exe", "claude.exe --session"),
            P(90, 80, "pwsh.exe", "pwsh -NoProfile"),
            P(100, 90, "dotnet.exe", "dotnet test"),
            P(200, 1, "pwsh.exe", "pwsh -Command Start-Sleep 60 # C:/Users/x/.codex/worktrees/unidad"),
            P(210, 1, "git.exe", @"git -C C:\Users\X\.CODEX\worktrees\unidad status"),
            P(300, 1, "codex.exe", "codex.exe app-server"),
            P(310, 1, "codex-windows-sandbox-service.exe", null),
            P(400, 1, "dotnet.exe", "dotnet exec VBCSCompiler.dll"),
            P(410, 1, "MSBuild.exe", @"MSBuild.exe /nodemode:1 C:\Users\x\.codex\worktrees\unidad\a.csproj"),
            P(500, 1, "node.exe", null),
            P(600, 1, "explorer.exe", "explorer.exe"),
            P(610, 1, "svchost.exe", null),
        };

        [Fact]
        public void I62_C17_EachProcessGetsTheClassOfReadme32()
        {
            var classes = ProcessFacts.Classify(Snapshot, 100, Worktree).ToDictionary(x => x.Process.Pid, x => x.Classification);
            Assert.Equal("own-tree", classes[100]);
            Assert.Equal("own-tree", classes[90]);
            Assert.Equal("own-tree", classes[80]);
            Assert.Equal("participant", classes[200]);
            Assert.Equal("participant", classes[210]);
            Assert.Equal("owner-app", classes[300]);
            Assert.Equal("nominal-exclusion", classes[310]);
            Assert.Equal("build-server", classes[400]);
            Assert.Equal("participant", classes[410]);
            Assert.Equal("unattributable", classes[500]);
            Assert.False(classes.ContainsKey(600));
            Assert.False(classes.ContainsKey(610));
        }

        [Fact]
        public void I62_C17_AForeignParticipantOrAnUnattributableProcessIsAStopAndTheLaunchedTreeIsNot()
        {
            var classes = ProcessFacts.Classify(Snapshot, 100, Worktree);
            var causes = ProcessFacts.StopCauses(classes, launchedTree: new[] { 200 });
            Assert.Contains(causes, c => c.Contains("participant git.exe (210)", StringComparison.Ordinal));
            Assert.Contains(causes, c => c.Contains("participant MSBuild.exe (410)", StringComparison.Ordinal));
            Assert.Contains(causes, c => c.Contains("unattributable node.exe (500)", StringComparison.Ordinal));
            Assert.DoesNotContain(causes, c => c.Contains("(200)", StringComparison.Ordinal));
            Assert.Empty(ProcessFacts.StopCauses(ProcessFacts.Classify(Snapshot.Where(p => p.Pid is not (210 or 410 or 500)).ToList(), 100, Worktree), new[] { 200 }));
        }

        [Fact]
        public void I62_C17_AtTheEntryOfAWorkerSubagentANewSessionDescendantOrALiveOrphanIsAStop()
        {
            var cession = T0.AddMinutes(10);
            // 150 existed at the Exit; 160 is new; 170's parent is gone; 180's parent PID 600 was reused by a process created after it.
            var entry = Snapshot.Where(p => p.Pid != 600).Concat(new[]
            {
                P(150, 80, "conhost.exe", "conhost.exe", minute: 5),
                P(160, 80, "cmd.exe", "cmd.exe /c pause", minute: 12),
                P(170, 999, "pwsh.exe", "pwsh -Command Start-Sleep 99", minute: 15),
                P(180, 600, "git.exe", "git status", minute: 20),
                P(600, 1, "explorer.exe", "explorer.exe", minute: 30),
            }).ToList();
            var worker = ProcessFacts.WorkerClasses(entry, sessionPid: 80, selfPid: 100, Worktree, cession);
            var byPid = worker.ToDictionary(x => x.Process.Pid, x => x.Classification);
            Assert.Equal("session-descendant", byPid[150]);
            Assert.Equal("session-descendant", byPid[160]);
            Assert.Equal("orphan", byPid[170]);
            Assert.Equal("orphan", byPid[180]);
            var causes = ProcessFacts.StopCauses(Array.Empty<ClassifiedProcess>(), Array.Empty<int>(), worker, exitDescendants: new[] { 150 });
            Assert.Contains(causes, c => c.Contains("session-descendant cmd.exe (160)", StringComparison.Ordinal));
            Assert.Contains(causes, c => c.Contains("orphan pwsh.exe (170)", StringComparison.Ordinal));
            Assert.Contains(causes, c => c.Contains("orphan git.exe (180)", StringComparison.Ordinal));
            Assert.DoesNotContain(causes, c => c.Contains("(150)", StringComparison.Ordinal));
        }
    }
}
