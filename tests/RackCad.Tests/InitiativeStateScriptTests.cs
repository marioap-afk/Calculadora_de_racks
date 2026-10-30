using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 E1 (A-8 §2 (g), T-01..T-16): <c>eng/automation/initiative-state.ps1</c> over the v1 state of AUTOMATION_PLAN §8. Each case builds its own
    /// temporary Git repository with a local bare <c>origin</c>, so nothing depends on this repository's history (CI checks out a shallow SHA).
    /// The verdicts are informative; what is pinned here is that they follow the state and Git, that a missing, incomplete or invalid state stops
    /// with a readable reason, and that only <c>update</c> writes, and only the lines it names.
    /// </summary>
    public class InitiativeStateScriptTests
    {
        private const string Branch = "feature/e1-prueba";
        private const string Claim = "5b661a17-0000-4000-8000-000000000001";

        private static string ScriptPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            var path = Path.Combine(dir.FullName, "eng", "automation", "initiative-state.ps1");
            Assert.True(File.Exists(path), "No existe eng/automation/initiative-state.ps1.");
            return path;
        }

        private static string State(string initiative, string branch, string state, string gate, string evidence, string claim = Claim,
            string extraInside = "", string extraAfter = "") =>
            "schema: rackcad-automation-state/v1\n" +
            "automation_state:\n" +
            $"  initiative: {initiative}\n" +
            $"  branch: {branch}\n" +
            $"  claim_id: {claim}\n" +
            "  current_phase: G1\n" +
            $"  state: {state}\n" +
            $"  gate: {gate}\n" +
            "  attempts: 0\n" +
            "  next_action: \"Continuar con G1\"\n" +
            $"  last_evidence_commit: {evidence}\n" +
            extraInside +
            extraAfter;

        /// <summary>Publishes a state file whose <c>last_evidence_commit</c> is the commit before it, as AUTOMATION_PLAN §8 asks.</summary>
        private static Repo Ready(string state, string gate, string fileName = "I-90.yml", string initiative = "I-90", string extraInside = "",
            string extraAfter = "")
        {
            var repo = new Repo();
            repo.WriteState(fileName, State(initiative, Branch, state, gate, repo.Head, extraInside: extraInside, extraAfter: extraAfter));
            repo.CommitAndPush("estado");
            return repo;
        }

        private static Result Run(Repo repo, params string[] args)
        {
            var psi = new ProcessStartInfo("pwsh")
            {
                WorkingDirectory = repo.Work,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            foreach (var a in new[] { "-NoProfile", "-NonInteractive", "-File", ScriptPath() }.Concat(args).Append("-Json"))
            {
                psi.ArgumentList.Add(a);
            }

            using var p = Process.Start(psi);
            var stdout = p.StandardOutput.ReadToEndAsync();
            var stderr = p.StandardError.ReadToEndAsync();
            Assert.True(p.WaitForExit(120_000), "pwsh no termino en 120 s.");
            var text = stdout.Result;
            JsonElement json;
            try
            {
                json = JsonDocument.Parse(text).RootElement.Clone();
            }
            catch (JsonException)
            {
                throw new Xunit.Sdk.XunitException($"Salida no JSON (codigo {p.ExitCode}): {text}\n{stderr.Result}");
            }

            return new Result(p.ExitCode, json);
        }

        private sealed record Result(int Code, JsonElement Json)
        {
            public string Text(string name) => Json.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

            public List<string> Reasons => Json.GetProperty("Reasons").EnumerateArray().Select(r => r.GetString()).ToList();

            public bool Flag(string name) => Json.GetProperty(name).GetBoolean();
        }

        private static void AssertStop(Result r, string reasonFragment)
        {
            Assert.Equal("STOP", r.Text("Verdict"));
            Assert.NotEqual(0, r.Code);
            Assert.Contains(r.Reasons, x => x.Contains(reasonFragment, StringComparison.Ordinal));
        }

        [Fact]
        public void T01_ImplementingWithoutGateContinues()
        {
            using var repo = Ready("implementing", "none");
            var r = Run(repo, "resume");
            Assert.Equal(0, r.Code);
            Assert.Equal("CONTINUE", r.Text("Verdict"));
            Assert.Equal("Continuar con G1", r.Text("NextAction"));
            Assert.Empty(r.Reasons);
        }

        [Fact]
        public void T02_OpenGateWaitsAndNamesIt()
        {
            using var repo = Ready("implementing", "owner-decision");
            var r = Run(repo, "resume");
            Assert.Equal(0, r.Code);
            Assert.Equal("WAIT owner-decision", r.Text("Verdict"));
        }

        [Fact]
        public void T03_WaitingStateWaitsAndNamesIt()
        {
            using var repo = Ready("waiting", "none");
            Assert.Equal("WAIT waiting", Run(repo, "resume").Text("Verdict"));
        }

        [Fact]
        public void T04_CompletedIsDone()
        {
            using var repo = Ready("completed", "none");
            Assert.Equal("DONE", Run(repo, "resume").Text("Verdict"));
        }

        [Fact]
        public void T05_NoStateForTheBranchStops()
        {
            using var repo = new Repo();
            repo.WriteState("I-90.yml", State("I-90", "otra/rama", "implementing", "none", repo.Head));
            repo.CommitAndPush("estado de otra rama");
            AssertStop(Run(repo, "resume"), "branch: " + Branch);
        }

        [Fact]
        public void T06_TwoStatesForTheBranchStop()
        {
            using var repo = new Repo();
            repo.WriteState("I-90.yml", State("I-90", Branch, "implementing", "none", repo.Head));
            repo.WriteState("I-91.yml", State("I-91", Branch, "implementing", "none", repo.Head));
            repo.CommitAndPush("dos estados");
            AssertStop(Run(repo, "resume"), "varios archivos");
        }

        [Fact]
        public void T07_StateAndGateOutsideTheVocabularyStopWithBothReasonsInOrder()
        {
            using var repo = Ready("in-progress", "READY");
            var r = Run(repo, "resume");
            AssertStop(r, "state fuera del vocabulario");
            var state = r.Reasons.FindIndex(x => x.StartsWith("state fuera del vocabulario", StringComparison.Ordinal));
            var gate = r.Reasons.FindIndex(x => x.StartsWith("gate fuera del vocabulario", StringComparison.Ordinal));
            Assert.True(state >= 0 && gate > state, string.Join(" | ", r.Reasons));
        }

        [Fact]
        public void T08_EmptyFieldStops()
        {
            using var repo = new Repo();
            repo.WriteState("I-90.yml", State("I-90", Branch, "implementing", "none", repo.Head, claim: string.Empty));
            repo.CommitAndPush("sin claim");
            AssertStop(Run(repo, "resume"), "claim_id");
        }

        [Fact]
        public void T08_PlainValueThatYamlRejectsStops()
        {
            using var repo = new Repo();
            var body = State("I-90", Branch, "implementing", "none", repo.Head).Replace("  next_action: \"Continuar con G1\"", "  next_action: Owner: decidir");
            repo.WriteState("I-90.yml", body);
            repo.CommitAndPush("valor sin comillas");
            AssertStop(Run(repo, "resume"), "YAML no admite");
        }

        [Fact]
        public void T09_InitiativeDifferentFromTheFileNameStops()
        {
            using var repo = Ready("implementing", "none", fileName: "I-90.yml", initiative: "I-91");
            AssertStop(Run(repo, "resume"), "distinto del nombre del archivo");
        }

        [Fact]
        public void T10_UnpublishedHeadStops()
        {
            using var repo = Ready("implementing", "none");
            File.WriteAllText(Path.Combine(repo.Work, "nota.txt"), "local\n");
            repo.Commit("sin publicar");
            AssertStop(Run(repo, "resume"), "distinto de origin/" + Branch);
        }

        [Fact]
        public void T11_EvidenceThatIsNotAnAncestorStops()
        {
            using var repo = new Repo();
            repo.Git("checkout", "-q", "-b", "lateral");
            File.WriteAllText(Path.Combine(repo.Work, "lateral.txt"), "x\n");
            var side = repo.Commit("lateral");
            repo.Git("checkout", "-q", Branch);
            repo.WriteState("I-90.yml", State("I-90", Branch, "implementing", "none", side));
            repo.CommitAndPush("estado con evidencia lateral");
            AssertStop(Run(repo, "resume"), "no es antecesor de HEAD");
        }

        [Fact]
        public void T12_ChangesWithoutCommitStop()
        {
            using var repo = Ready("implementing", "none");
            File.WriteAllText(Path.Combine(repo.Work, "nuevo.txt"), "x\n");
            AssertStop(Run(repo, "resume"), "cambios sin commit");
        }

        [Theory]
        [InlineData("implementing", 0)]
        [InlineData("in-progress", 1)]
        public void T13_ResumeAndCheckWriteNothing(string state, int checkCode)
        {
            using var repo = Ready(state, "none");
            var before = repo.Snapshot();
            Run(repo, "resume");
            var check = Run(repo, "check");
            Assert.Equal(checkCode, check.Code);
            Assert.Equal(checkCode == 0, check.Flag("Valid"));
            Assert.Equal(before, repo.Snapshot());
        }

        [Fact]
        public void T14_UpdateRewritesOnlyItsLinesAndResumeReadsThem()
        {
            using var repo = Ready("implementing", "none", extraInside: "  notes: \"se conserva\"\n",
                extraAfter: "execution_context:\n  mode: manual\n  worktree: e1-prueba\n");
            var path = repo.StatePath("I-90.yml");
            var before = File.ReadAllText(path).Replace("\r\n", "\n").Split('\n');
            var evidence = repo.Head;
            const string next = "Owner: revisar el cierre de G1 (sección §2) #1 \"ya\"";

            var u = Run(repo, "update", "-NextAction", next, "-State", "waiting", "-Gate", "owner-decision");
            Assert.Equal(0, u.Code);
            Assert.True(u.Flag("Written"));

            var after = File.ReadAllText(path).Replace("\r\n", "\n").Split('\n');
            Assert.Equal(before.Length, after.Length);
            var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).Select(i => after[i].Split(':')[0].Trim()).ToList();
            Assert.Equal(new[] { "state", "gate", "next_action", "last_evidence_commit" }, changed);

            repo.CommitAndPush("estado al terminar la tarea");
            var r = Run(repo, "resume");
            Assert.Equal("WAIT owner-decision", r.Text("Verdict"));
            Assert.Equal(next, r.Text("NextAction"));
            Assert.Equal(evidence, r.Text("LastEvidenceCommit"));
            Assert.Empty(r.Reasons);
        }

        [Theory]
        [InlineData("vocabulario", "state fuera del vocabulario")]
        [InlineData("multilinea", "mas de una linea")]
        [InlineData("sin-publicar", "HEAD no publicado")]
        public void T15_UpdateRefusesWithoutWriting(string kind, string reason)
        {
            using var repo = Ready("implementing", "none");
            var args = new List<string> { "update", "-NextAction", "Seguir" };
            if (kind == "vocabulario")
            {
                args.AddRange(new[] { "-State", "in-progress" });
            }
            else if (kind == "multilinea")
            {
                args[2] = "Seguir\ncon otra linea";
            }
            else
            {
                File.WriteAllText(Path.Combine(repo.Work, "nota.txt"), "local\n");
                repo.Commit("sin publicar");
            }

            var before = repo.Snapshot();
            var r = Run(repo, args.ToArray());
            Assert.NotEqual(0, r.Code);
            Assert.False(r.Flag("Written"));
            Assert.Contains(r.Reasons, x => x.Contains(reason, StringComparison.Ordinal));
            Assert.Equal(before, repo.Snapshot());
        }

        [Fact]
        public void T16_InitCreatesAStateThatCheckAccepts()
        {
            using var repo = new Repo();
            var u = Run(repo, "update", "-Init", "-Initiative", "I-90", "-ClaimId", Claim, "-Phase", "G0", "-State", "claimed", "-Gate", "none",
                "-NextAction", "Abrir G0");
            Assert.Equal(0, u.Code);
            Assert.True(File.Exists(repo.StatePath("I-90.yml")));

            repo.CommitAndPush("estado inicial");
            var check = Run(repo, "check");
            Assert.Equal(0, check.Code);
            Assert.True(check.Flag("Valid"));
            Assert.Equal("CONTINUE", Run(repo, "resume").Text("Verdict"));
        }

        [Fact]
        public void T16_InitRefusesWhenTheStateAlreadyExists()
        {
            using var repo = Ready("implementing", "none");
            var before = repo.Snapshot();
            var u = Run(repo, "update", "-Init", "-Initiative", "I-90", "-ClaimId", Claim, "-Phase", "G0", "-State", "claimed", "-Gate", "none",
                "-NextAction", "Abrir G0");
            Assert.NotEqual(0, u.Code);
            Assert.False(u.Flag("Written"));
            Assert.Contains(u.Reasons, x => x.Contains("ya existe", StringComparison.Ordinal));
            Assert.Equal(before, repo.Snapshot());
        }

        /// <summary>A work repository on <see cref="Branch"/> with one published commit and a local bare <c>origin</c>.</summary>
        private sealed class Repo : IDisposable
        {
            private readonly string root;

            public Repo()
            {
                root = Path.Combine(Path.GetTempPath(), "rackcad-e1-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                Origin = Path.Combine(root, "origin.git");
                Work = Path.Combine(root, "work");
                RunGit(root, "init", "-q", "--bare", Origin);
                RunGit(root, "-c", "init.defaultBranch=main", "init", "-q", Work);
                Git("checkout", "-q", "-b", Branch);
                Git("remote", "add", "origin", Origin);
                File.WriteAllText(Path.Combine(Work, "README.md"), "e1\n");
                CommitAndPush("inicio");
            }

            public string Origin { get; }

            public string Work { get; }

            public string Head => Git("rev-parse", "HEAD");

            public string StatePath(string fileName) => Path.Combine(Work, "docs", "automation", "state", fileName);

            public void WriteState(string fileName, string body)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(StatePath(fileName)));
                File.WriteAllText(StatePath(fileName), body);
            }

            public string Git(params string[] args) => RunGit(Work, args);

            public string Commit(string message)
            {
                Git("add", "-A");
                Git("commit", "-q", "-m", message);
                return Head;
            }

            public void CommitAndPush(string message)
            {
                Commit(message);
                Git("push", "-q", "origin", "HEAD");
            }

            /// <summary>HEAD, the status and every file's bytes of the work tree (without .git).</summary>
            public string Snapshot()
            {
                var files = Directory.EnumerateFiles(Work, "*", SearchOption.AllDirectories)
                    .Where(f => !f.Substring(Work.Length).TrimStart(Path.DirectorySeparatorChar).StartsWith(".git", StringComparison.Ordinal))
                    .OrderBy(f => f, StringComparer.Ordinal)
                    .Select(f => f.Substring(Work.Length) + "=" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(f))));
                return Head + "|" + Git("status", "--porcelain") + "|" + string.Join(";", files);
            }

            public void Dispose()
            {
                try
                {
                    foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(f, FileAttributes.Normal);
                    }

                    Directory.Delete(root, true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            private static string RunGit(string cwd, params string[] args)
            {
                var psi = new ProcessStartInfo("git")
                {
                    WorkingDirectory = cwd,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                foreach (var a in new[] { "-c", "user.name=E1", "-c", "user.email=e1@example.invalid", "-c", "commit.gpgsign=false" }.Concat(args))
                {
                    psi.ArgumentList.Add(a);
                }

                psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
                using var p = Process.Start(psi);
                var stdout = p.StandardOutput.ReadToEndAsync();
                var stderr = p.StandardError.ReadToEndAsync();
                p.WaitForExit();
                Assert.True(p.ExitCode == 0, $"git {string.Join(" ", args)}: {stderr.Result}");
                return stdout.Result.Trim();
            }
        }
    }
}
