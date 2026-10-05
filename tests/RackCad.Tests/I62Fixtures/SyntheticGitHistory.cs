#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RackCad.Tests
{
    /// <summary>
    /// A synthetic commit graph for the history class (parents, files per commit with inheritance, changed paths and patch ids). It lets the Core guards
    /// exercise I-H01, I-H02, I-P03, I-P08 and ResolveBranchRef without depending on the repository's own (shallow) history. Test data only.
    /// </summary>
    public sealed class SyntheticGitHistory : IGitHistory
    {
        private readonly Dictionary<string, string?> parent = new Dictionary<string, string?>(StringComparer.Ordinal);
        private readonly Dictionary<string, Dictionary<string, string>> files = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> patchIds = new Dictionary<string, string>(StringComparer.Ordinal);

        public SyntheticGitHistory Commit(string sha, string? parentSha, string? patchId = null, params (string Path, string Blob)[] changed)
        {
            parent[sha] = parentSha;
            files[sha] = changed.ToDictionary(c => c.Path, c => c.Blob, StringComparer.Ordinal);
            if (patchId != null)
            {
                patchIds[sha] = patchId;
            }

            return this;
        }

        public bool Exists(string commit) => parent.ContainsKey(commit);

        public bool IsAncestor(string ancestor, string descendant)
        {
            if (!Exists(ancestor) || !Exists(descendant))
            {
                return false;
            }

            for (var c = descendant; c != null; c = parent[c])
            {
                if (c == ancestor)
                {
                    return true;
                }
            }

            return false;
        }

        public string? BlobAt(string commit, string path)
        {
            for (var c = Exists(commit) ? commit : null; c != null; c = parent[c])
            {
                if (files[c].TryGetValue(path, out var blob))
                {
                    return blob;
                }
            }

            return null;
        }

        public IReadOnlyList<string> Range(string from, string to)
        {
            var result = new List<string>();
            for (var c = Exists(to) ? to : null; c != null && !(Exists(from) && IsAncestor(c, from)); c = parent[c])
            {
                result.Add(c);
            }

            result.Reverse();
            return result;
        }

        public IReadOnlyList<string> ChangedPaths(string commit) => Exists(commit) ? files[commit].Keys.ToList() : new List<string>();

        public string? PatchId(string commit) => patchIds.TryGetValue(commit, out var p) ? p : null;
    }

    /// <summary>A disposable Git repository driven by the Git CLI (temporary directory; deleted by <see cref="Dispose"/>).</summary>
    public sealed class GitScratch : IDisposable
    {
        public GitScratch()
        {
            Root = Path.Combine(Path.GetTempPath(), "i62-f4-" + Guid.NewGuid().ToString("N").Substring(0, 12));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string Run(string repo, params string[] args)
        {
            var psi = new ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, StandardOutputEncoding = Encoding.UTF8 };
            psi.ArgumentList.Add("-C");
            psi.ArgumentList.Add(repo);
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("core.autocrlf=false");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("commit.gpgsign=false");
            foreach (var a in args)
            {
                psi.ArgumentList.Add(a);
            }

            foreach (var (k, v) in new[] { ("GIT_AUTHOR_NAME", "i62"), ("GIT_AUTHOR_EMAIL", "i62@example.invalid"), ("GIT_COMMITTER_NAME", "i62"),
                ("GIT_COMMITTER_EMAIL", "i62@example.invalid"), ("GIT_CONFIG_NOSYSTEM", "1") })
            {
                psi.Environment[k] = v;
            }

            psi.Environment["GIT_CONFIG_GLOBAL"] = OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
            using var p = Process.Start(psi)!;
            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new InvalidOperationException("git " + string.Join(" ", args) + ": " + stderr);
            }

            return stdout.Trim();
        }

        public void Write(string repo, string relative, string text)
        {
            var full = Path.Combine(repo, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, text.Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
        }

        public string CommitAll(string repo, string message)
        {
            Run(repo, "add", "-A");
            Run(repo, "commit", "-q", "-m", message);
            return Run(repo, "rev-parse", "HEAD");
        }

        public void Dispose()
        {
            try
            {
                foreach (var f in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(f, FileAttributes.Normal);
                }

                Directory.Delete(Root, recursive: true);
            }
            catch (IOException)
            {
                // a temporary directory left behind does not change any result
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
