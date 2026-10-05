#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// A disposable custody scenario for the reproducible controls of C-15/C-16: a bare <c>origin</c>, the holder's clone and, on demand, clean clones
    /// (<c>--no-local</c>, so unreachable objects are not copied). Durable points are written from the synthetic points of the Core guards with their
    /// symbolic SHAs replaced by real commits; every StateRef is recomputed from the bytes actually committed. Test data only.
    /// </summary>
    public sealed class CustodyRepo : IDisposable
    {
        public const string StatePath = "docs/automation/state/I-99.yml";

        public CustodyRepo()
        {
            G = new GitScratch();
            Origin = Path.Combine(G.Root, "origin.git");
            Directory.CreateDirectory(Origin);
            G.Run(Origin, "init", "-q", "--bare", "-b", "main");
            Holder = Path.Combine(G.Root, "holder");
            G.Run(G.Root, "clone", "-q", Origin, Holder);
            G.Write(Holder, "README.md", "fixture\n");
            Main0 = G.CommitAll(Holder, "main 0");
            G.Run(Holder, "push", "-q", "-u", "origin", "main");
            G.Run(Holder, "checkout", "-q", "-b", "feature");
        }

        public GitScratch G { get; }

        public string Origin { get; }

        public string Holder { get; }

        public string Main0 { get; }

        /// <summary>Symbolic SHA → real commit, applied to the text of every written file and of the state.</summary>
        public Dictionary<string, string> Subst { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        public string Git(string repo, params string[] args) => G.Run(repo, args);

        public string Head(string repo) => G.Run(repo, "rev-parse", "HEAD");

        private string Apply(string text) => Subst.Aggregate(text, (t, kv) => t.Replace(kv.Key, kv.Value, StringComparison.Ordinal));

        /// <summary>
        /// Writes a durable point: the tree files of <paramref name="p"/> (substituted), the StateRefs of its state recomputed from what was written, and
        /// the canonical YAML of the state; commits it and, unless told otherwise, pushes it. Returns the commit.
        /// </summary>
        public string WritePoint(string repo, StatePoint p, string message, bool push = true, IEnumerable<string>? onlyFiles = null)
        {
            var only = onlyFiles == null ? null : new HashSet<string>(onlyFiles, StringComparer.Ordinal);
            var written = new Dictionary<string, string>(StringComparer.Ordinal);
            if (p.Tree is InMemoryStateTree tree)
            {
                foreach (var (path, bytes) in tree.Files)
                {
                    if (only != null && !only.Contains(path))
                    {
                        continue;
                    }

                    var text = Apply(Encoding.UTF8.GetString(bytes));
                    G.Write(repo, path, text);
                    written[path] = I62Repo.GitBlobSha1(text);
                }
            }

            var state = (YamlMap)YamlSubset.DeepClone(p.State)!;
            foreach (var (_, stateRef) in Y.StateRefs(state, "$").ToList())
            {
                if (written.TryGetValue(Y.S(stateRef, "path")!, out var blob))
                {
                    stateRef["blob"] = blob;
                }
            }

            G.Write(repo, StatePath, Apply(YamlSubset.Write(state)));
            G.Run(repo, "add", "-A");
            G.Run(repo, "commit", "-q", "-m", message);
            if (push)
            {
                G.Run(repo, "push", "-q", "origin", "HEAD");
            }

            return Head(repo);
        }

        /// <summary>The durable point of a commit: its state, read in STRICT mode, and the tree of that commit.</summary>
        public StatePoint Read(string repo, string commit) =>
            new StatePoint(YamlSubset.Read(G.Run(repo, "show", commit + ":" + StatePath) + "\n"), new GitCommitStateTree(repo, commit));

        /// <summary>A commit of the Worker (or of anyone) touching the given files, without changing the state.</summary>
        public string Commit(string repo, string message, params (string Path, string Text)[] files)
        {
            foreach (var (path, text) in files)
            {
                G.Write(repo, path, text);
            }

            return G.CommitAll(repo, message);
        }

        public string CleanClone(string name, string branch = "feature")
        {
            var dir = Path.Combine(G.Root, name);
            G.Run(G.Root, "clone", "-q", "--no-local", "-c", "core.autocrlf=false", "--single-branch", "--branch", branch, Origin, dir);
            return dir;
        }

        /// <summary>
        /// The RebaseMap of a real rebase (AUTOMATION_PLAN 16.25; README §17.3): the commits of <c>mainBefore..branchBefore</c> paired in order with those of
        /// <c>mainAfter..branchAfter</c>, with their recomputed patch ids, and one StateFields entry per given field.
        /// </summary>
        public JsonObject Map(string repo, string runId, string mainBefore, string mainAfter, string branchBefore, string branchAfter,
            IEnumerable<(string Field, string Original, string Image)> fields)
        {
            var git = new GitProcessHistory(repo);
            var before = git.Range(mainBefore, branchBefore);
            var after = git.Range(mainAfter, branchAfter);
            return new JsonObject
            {
                ["RunId"] = runId, ["TaskId"] = "SESSION", ["MainBeforeSha"] = mainBefore, ["MainAfterSha"] = mainAfter, ["BranchBeforeSha"] = branchBefore,
                ["BranchAfterSha"] = branchAfter,
                ["Commits"] = new JsonArray(before.Zip(after, (o, i) => (JsonNode)new JsonObject
                    { ["OriginalSha"] = o, ["ImageSha"] = i, ["PatchId"] = git.PatchId(i), ["PatchIdEqual"] = git.PatchId(o) == git.PatchId(i) }).ToArray()),
                ["StateFields"] = new JsonArray(fields.Select(f => (JsonNode)new JsonObject { ["Field"] = f.Field, ["OriginalSha"] = f.Original, ["ImageSha"] = f.Image }).ToArray()),
                ["CiRuns"] = new JsonArray(),
                ["Unmapped"] = new JsonArray(),
            };
        }

        public void Dispose() => G.Dispose();
    }
}
