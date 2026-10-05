#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// The Git facts the history class needs (I-P03, I-P08, I-H01, I-H02; A-1 D2-3, D2-10..D2-12): ancestry, blobs at a commit, the commits a rebase
    /// rewrote and patch identities. Only the reproducible controls and the guards over synthetic histories use it; the Core guards never read the
    /// repository's own (shallow) history.
    /// </summary>
    public interface IGitHistory
    {
        bool Exists(string commit);

        /// <summary>True only if both commits exist and <paramref name="ancestor"/> is an ancestor of (or equal to) <paramref name="descendant"/>.</summary>
        bool IsAncestor(string ancestor, string descendant);

        /// <summary>The blob of <paramref name="path"/> at <paramref name="commit"/>, or null.</summary>
        string? BlobAt(string commit, string path);

        /// <summary>The commits of <c>from..to</c> (reachable from <paramref name="to"/>, not from <paramref name="from"/>), oldest first.</summary>
        IReadOnlyList<string> Range(string from, string to);

        /// <summary>Paths a commit changes against its first parent.</summary>
        IReadOnlyList<string> ChangedPaths(string commit);

        /// <summary>The stable patch id of a commit (<c>git patch-id --stable</c>), or null.</summary>
        string? PatchId(string commit);

        /// <summary>The type of the object a full id names (<c>git cat-file -t</c>), or null when it does not exist.</summary>
        string? ObjectType(string sha) => Exists(sha) ? "commit" : null;

        /// <summary>The rows of <c>git diff --no-renames --raw from to</c>, or null when the comparison could not run (never an empty pass).</summary>
        IReadOnlyList<GitDiffRow>? DiffRaw(string from, string to) => null;
    }

    /// <summary>One row of a raw diff: the status letter, both modes and the path (a rename is a delete plus an add; no rename detection).</summary>
    public sealed record GitDiffRow(string Status, string OldMode, string NewMode, string Path);

    /// <summary>The Git CLI over a local repository (used by the reproducible controls on disposable repositories and clean clones).</summary>
    public sealed class GitProcessHistory : IGitHistory
    {
        private readonly string repo;

        public GitProcessHistory(string repo)
        {
            this.repo = repo;
        }

        public bool Exists(string commit) => Run(out _, "cat-file", "-e", commit + "^{commit}") == 0;

        public bool IsAncestor(string ancestor, string descendant) =>
            Exists(ancestor) && Exists(descendant) && Run(out _, "merge-base", "--is-ancestor", ancestor, descendant) == 0;

        public string? BlobAt(string commit, string path) => Run(out var o, "rev-parse", commit + ":" + path) == 0 ? o.Trim() : null;

        public IReadOnlyList<string> Range(string from, string to) =>
            Run(out var o, "rev-list", "--reverse", from + ".." + to) == 0 ? o.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList() : new List<string>();

        public IReadOnlyList<string> ChangedPaths(string commit) =>
            Run(out var o, "diff-tree", "--no-commit-id", "--name-only", "-r", "--root", commit) == 0
                ? o.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()
                : new List<string>();

        public string? PatchId(string commit)
        {
            if (Run(out var show, "show", commit) != 0)
            {
                return null;
            }

            var psi = Start("patch-id", "--stable");
            psi.RedirectStandardInput = true;
            using var p = Process.Start(psi)!;
            p.StandardInput.Write(show);
            p.StandardInput.Close();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var first = output.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return p.ExitCode == 0 ? first : null;
        }

        public string? ObjectType(string sha) => Run(out var o, "cat-file", "-t", sha) == 0 ? o.Trim() : null;

        public IReadOnlyList<GitDiffRow>? DiffRaw(string from, string to)
        {
            if (Run(out var o, "diff", "--no-renames", "--raw", "-z", from, to) != 0)
            {
                return null;
            }

            var parts = o.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            var rows = new List<GitDiffRow>();
            for (var i = 0; i + 1 < parts.Length; i += 2)
            {
                var meta = parts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                rows.Add(new GitDiffRow(meta[^1], meta[0].TrimStart(':'), meta[1], parts[i + 1]));
            }

            return rows;
        }

        private ProcessStartInfo Start(params string[] args)
        {
            var psi = new ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, StandardOutputEncoding = Encoding.UTF8 };
            psi.ArgumentList.Add("-C");
            psi.ArgumentList.Add(repo);
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("core.autocrlf=false");
            foreach (var a in args)
            {
                psi.ArgumentList.Add(a);
            }

            return psi;
        }

        private int Run(out string stdout, params string[] args)
        {
            using var p = Process.Start(Start(args))!;
            stdout = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode;
        }
    }

    /// <summary>One custodied RebaseMap (B.8.7, with the field names of <c>relay-record/v2</c>).</summary>
    public sealed class RebaseMapDoc
    {
        public RebaseMapDoc(JsonObject json)
        {
            Json = json;
            MainBefore = J.S(json, "MainBeforeSha") ?? string.Empty;
            MainAfter = J.S(json, "MainAfterSha") ?? string.Empty;
            BranchBefore = J.S(json, "BranchBeforeSha") ?? string.Empty;
            BranchAfter = J.S(json, "BranchAfterSha");
            Commits = (json["Commits"] as JsonArray)?.OfType<JsonObject>()
                .Select(c => new RebaseCommit(J.S(c, "OriginalSha") ?? string.Empty, J.S(c, "ImageSha"), J.S(c, "PatchId"),
                    c["PatchIdEqual"] is JsonValue v && v.TryGetValue<bool>(out var b) && b))
                .ToList() ?? new List<RebaseCommit>();
        }

        public JsonObject Json { get; }

        public string MainBefore { get; }

        public string MainAfter { get; }

        public string BranchBefore { get; }

        public string? BranchAfter { get; }

        public IReadOnlyList<RebaseCommit> Commits { get; }

        public RebaseCommit? Entry(string original) => Commits.FirstOrDefault(c => c.Original == original);
    }

    public sealed record RebaseCommit(string Original, string? Image, string? PatchId, bool PatchIdEqual);

    /// <summary>The result of ResolveBranchRef (A-1 D2-10): the resolved commit, or why the reference stays UNRESOLVED.</summary>
    public sealed record ResolveResult(string? Commit, string Reason)
    {
        public bool Resolved => Commit != null;
    }

    /// <summary>
    /// The custodied chain of RebaseMaps of a unit and what A-1 builds on it: ResolveBranchRef (D2-10), EquivalentReviewedObject (D2-12) and the
    /// publication check of a map (B.8.7, plus the Coordinator's A62-A1U-O1: an entry is creditable only when its OriginalSha was really rewritten by
    /// that rebase, so a fabricated composite entry X → X'' never replaces a missing step).
    /// </summary>
    public static class RebaseChain
    {
        /// <summary>The ordered maps of <c>custody.rebase_history[]</c>, read by StateRef from the tree of the point; null if any does not resolve.</summary>
        public static List<RebaseMapDoc>? History(StatePoint point)
        {
            var maps = new List<RebaseMapDoc>();
            foreach (var r in Y.L(point.State, "custody.rebase_history"))
            {
                var json = StateTreeReader.Json(point.Tree, r);
                if (json == null)
                {
                    return null;
                }

                maps.Add(new RebaseMapDoc(json));
            }

            return maps;
        }

        /// <summary>
        /// D2-10 step 2: the image of <paramref name="commit"/> through the chain, or null when unproven. A step is followed only through a map entry
        /// with PatchIdEqual; a later map that does not rewrite the commit requires it to be an ancestor of that map's MainBeforeSha (needs
        /// <paramref name="git"/>; without Git the step is unproven, which fails closed).
        /// </summary>
        public static string? Image(string commit, IReadOnlyList<RebaseMapDoc> history, IGitHistory? git, out string reason)
        {
            var first = -1;
            for (var i = 0; i < history.Count; i++)
            {
                if (history[i].Entry(commit) != null)
                {
                    first = i;
                    break;
                }
            }

            if (first < 0)
            {
                reason = commit.Substring(0, Math.Min(8, commit.Length)) + " is not an OriginalSha of any map of the history";
                return null;
            }

            var start = history[first].Entry(commit)!;
            if (!start.PatchIdEqual || start.Image == null)
            {
                reason = "the map entry of " + Short(commit) + " has no image with PatchIdEqual";
                return null;
            }

            if (!Creditable(history[first], commit, git))
            {
                reason = "map " + first + " credits " + Short(commit) + ", which its rebase did not rewrite (A62-A1U-O1)";
                return null;
            }

            var c = start.Image;
            for (var j = first + 1; j < history.Count; j++)
            {
                var e = history[j].Entry(c);
                if (e != null)
                {
                    if (!e.PatchIdEqual || e.Image == null)
                    {
                        reason = "the map entry of " + Short(c) + " has no image with PatchIdEqual";
                        return null;
                    }

                    if (!Creditable(history[j], c, git))
                    {
                        reason = "map " + j + " credits " + Short(c) + ", which its rebase did not rewrite (A62-A1U-O1)";
                        return null;
                    }

                    c = e.Image;
                }
                else if (git == null || !git.IsAncestor(c, history[j].MainBefore))
                {
                    reason = "a step of the chain is missing for " + Short(c) + " (not rewritten by map " + j + " and not an ancestor of its MainBeforeSha)";
                    return null;
                }
            }

            reason = string.Empty;
            return c;
        }

        /// <summary>
        /// ResolveBranchRef(ref, H, HEAD) of A-1 D2-10, with <paramref name="history"/> = the ordered maps of the point that consumes the reference
        /// (for a reconciliation, the candidate history of n, which already ends with the new map; D2-2 (cont.)) and <paramref name="head"/> = the tip
        /// that point consumes. Never guessed: no path, patch similarity, tree or SHA substitution outside the maps.
        /// </summary>
        public static ResolveResult Resolve(string commit, string path, string blob, IReadOnlyList<RebaseMapDoc> history, string head, IGitHistory git)
        {
            if (git.IsAncestor(commit, head) && git.BlobAt(commit, path) == blob)
            {
                return new ResolveResult(commit, "ancestor of HEAD with its blob");
            }

            var image = Image(commit, history, git, out var reason);
            if (image == null)
            {
                return new ResolveResult(null, reason);
            }

            if (!git.IsAncestor(image, head))
            {
                return new ResolveResult(null, Short(image) + " is not an ancestor of HEAD");
            }

            if (git.BlobAt(image, path) != blob)
            {
                return new ResolveResult(null, "the blob of " + path + " at " + Short(image) + " is not " + Short(blob));
            }

            // D2-10 relies on the map's PatchId; another machine recomputes it on the image (§8.8) and a mismatch is UNRESOLVED.
            var last = history.Select(m => m.Commits.FirstOrDefault(c => c.Image == image)).LastOrDefault(e => e != null);
            if (last?.PatchId != null && git.PatchId(image) is string recomputed && recomputed != last.PatchId)
            {
                return new ResolveResult(null, "the PatchId recomputed on " + Short(image) + " differs from the map");
            }

            return new ResolveResult(image, "image through the custodied chain");
        }

        /// <summary>
        /// EquivalentReviewedObject(A, B, H) of A-1 D2-12: same path and blob and, besides, the same commit or an image relation proven by the chain.
        /// </summary>
        public static bool Equivalent(YamlMap? a, YamlMap? b, IReadOnlyList<RebaseMapDoc> history, IGitHistory? git = null)
        {
            if (a == null || b == null || Y.S(a, "path") != Y.S(b, "path") || Y.S(a, "blob") != Y.S(b, "blob"))
            {
                return false;
            }

            var ca = Y.S(a, "commit")!;
            var cb = Y.S(b, "commit")!;
            return ca == cb || Image(ca, history, git, out _) == cb || Image(cb, history, git, out _) == ca;
        }

        /// <summary>
        /// The check the reconciling host makes before publishing a map (B.8.7 and §8.8 step 4, with A62-A1U-O1), where the pre-rebase objects exist:
        /// every OriginalSha is a commit that this rebase rewrote (MainBefore..BranchBefore), every image is in MainAfter..BranchAfter, both sides have
        /// the same length and order, the patch identity is recomputed and equal, nothing is duplicated and Unmapped is empty. A composite entry X → X''
        /// whose X this rebase did not rewrite is rejected here, so it can never stand in for a missing earlier map.
        /// </summary>
        public static List<string> PublicationProblems(RebaseMapDoc map, IGitHistory git)
        {
            var problems = new List<string>();
            var rewritten = git.Range(map.MainBefore, map.BranchBefore);
            var images = map.BranchAfter == null ? new List<string>() : git.Range(map.MainAfter, map.BranchAfter);
            var originals = map.Commits.Select(c => c.Original).ToList();
            foreach (var c in map.Commits)
            {
                if (!rewritten.Contains(c.Original))
                {
                    problems.Add("OriginalSha " + Short(c.Original) + " was not rewritten by this rebase (A62-A1U-O1)");
                }

                if (c.Image == null || !images.Contains(c.Image))
                {
                    problems.Add("ImageSha of " + Short(c.Original) + " is not in the published branch");
                }
                else if (!c.PatchIdEqual || git.PatchId(c.Original) is not string po || po != git.PatchId(c.Image) || po != c.PatchId)
                {
                    problems.Add("patch identity of " + Short(c.Original) + " is not proven equal");
                }
            }

            if (originals.Distinct().Count() != originals.Count)
            {
                problems.Add("duplicated OriginalSha");
            }

            if (!rewritten.SequenceEqual(originals))
            {
                problems.Add("Commits[] is not exactly the rewritten range MainBefore..BranchBefore, in order");
            }

            if (map.Json["Unmapped"] is JsonArray unmapped && unmapped.Count > 0)
            {
                problems.Add("Unmapped is not empty");
            }

            return problems;
        }

        /// <summary>
        /// A62-A1U-O1: an entry is creditable only when its OriginalSha was really rewritten by that map's rebase (MainBefore..BranchBefore). Where the
        /// pre-rebase objects exist (the reconciling host) this is checked; in a clean successor clone they may be unreachable, and the map is credited as
        /// custodied, because the reconciling host could not have published it otherwise (§8.8 step 4, <see cref="PublicationProblems"/>).
        /// </summary>
        private static bool Creditable(RebaseMapDoc map, string original, IGitHistory? git)
        {
            if (git == null || !git.Exists(map.BranchBefore) || !git.Exists(map.MainBefore))
            {
                return true;
            }

            return git.Range(map.MainBefore, map.BranchBefore).Contains(original);
        }

        private static string Short(string sha) => sha.Length > 8 ? sha.Substring(0, 8) : sha;
    }
}
