#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// The tree of the commit that holds a durable point: what a <c>StateRef</c> of that state may point to (I-S13: «todo StateRef existe en el árbol
    /// del commit y su blob coincide»). The validator reads custodied artifacts only through this view, never from the working copy.
    /// </summary>
    public interface IStateTree
    {
        bool TryRead(string path, out byte[] content);

        /// <summary>The Git blob id of a path when the tree knows it without reading the content (a committed tree), or null to hash the content.</summary>
        string? KnownBlob(string path) => null;
    }

    /// <summary>A tree held in memory: synthetic points of the Core guards and of the reproducible controls.</summary>
    public sealed class InMemoryStateTree : IStateTree
    {
        private readonly Dictionary<string, byte[]> files = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        public bool TryRead(string path, out byte[] content)
        {
            if (files.TryGetValue(path, out var bytes))
            {
                content = bytes;
                return true;
            }

            content = Array.Empty<byte>();
            return false;
        }

        /// <summary>Stores the text (UTF-8, LF) and returns its StateRef with the Git blob id of the stored bytes.</summary>
        public YamlMap Put(string path, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal));
            files[path] = bytes;
            var map = new YamlMap();
            map.Add("path", path);
            map.Add("blob", I62Repo.GitBlobSha1(bytes));
            return map;
        }

        public YamlMap PutJson(string path, JsonNode json) => Put(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) + "\n");

        public void Remove(string path) => files.Remove(path);

        /// <summary>Every file of the tree (path and bytes), in ordinal path order.</summary>
        public IEnumerable<KeyValuePair<string, byte[]>> Files
        {
            get
            {
                var keys = new List<string>(files.Keys);
                keys.Sort(StringComparer.Ordinal);
                foreach (var k in keys)
                {
                    yield return new KeyValuePair<string, byte[]>(k, files[k]);
                }
            }
        }

        public InMemoryStateTree Clone()
        {
            var c = new InMemoryStateTree();
            foreach (var kv in files)
            {
                c.files[kv.Key] = kv.Value;
            }

            return c;
        }
    }

    /// <summary>The working tree of the repository (the guard over the real state files reads their references here).</summary>
    public sealed class WorkingTreeStateTree : IStateTree
    {
        public bool TryRead(string path, out byte[] content)
        {
            var full = I62Repo.FullPath(path);
            if (path.Contains("..", StringComparison.Ordinal) || !System.IO.File.Exists(full))
            {
                content = Array.Empty<byte>();
                return false;
            }

            // The repository stores LF; a CRLF checkout must not change the blob id the state cites.
            content = Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(full).Replace("\r\n", "\n", StringComparison.Ordinal));
            return true;
        }
    }

    /// <summary>The tree of one commit of a Git repository, as stored (the durable point a StateRef is resolved in; reproducible controls and clean clones).</summary>
    public sealed class GitCommitStateTree : IStateTree
    {
        private readonly string repo;
        private readonly string commit;
        private readonly Dictionary<string, byte[]?> read = new Dictionary<string, byte[]?>(StringComparer.Ordinal);
        private Dictionary<string, string>? blobs;

        public GitCommitStateTree(string repo, string commit)
        {
            this.repo = repo;
            this.commit = commit;
        }

        /// <summary>The blob ids of the whole tree, read once with <c>git ls-tree -r</c> (absent path → null).</summary>
        public string? KnownBlob(string path)
        {
            if (blobs == null)
            {
                blobs = new Dictionary<string, string>(StringComparer.Ordinal);
                var listing = Git(out var ok, "ls-tree", "-r", "-z", commit);
                foreach (var entry in ok ? System.Text.Encoding.UTF8.GetString(listing).Split((char)0, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>())
                {
                    var tab = entry.IndexOf('\t', StringComparison.Ordinal);
                    var meta = entry.Substring(0, tab).Split(' ');
                    if (meta.Length == 3 && meta[1] == "blob")
                    {
                        blobs[entry.Substring(tab + 1)] = meta[2];
                    }
                }
            }

            return blobs.TryGetValue(path, out var b) ? b : null;
        }

        public bool TryRead(string path, out byte[] content)
        {
            if (!read.TryGetValue(path, out var bytes))
            {
                bytes = Load(path);
                read[path] = bytes;
            }

            content = bytes ?? Array.Empty<byte>();
            return bytes != null;
        }

        private byte[]? Load(string path)
        {
            if (path.Contains("..", StringComparison.Ordinal))
            {
                return null;
            }

            var content = Git(out var ok, "cat-file", "-p", commit + ":" + path);
            return ok ? content : null;
        }

        private byte[] Git(out bool ok, params string[] args)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            foreach (var a in new[] { "-C", repo, "-c", "core.autocrlf=false" }.Concat(args))
            {
                psi.ArgumentList.Add(a);
            }

            using var p = System.Diagnostics.Process.Start(psi)!;
            using var ms = new System.IO.MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(ms);
            p.StandardError.ReadToEnd();
            p.WaitForExit();
            ok = p.ExitCode == 0;
            return ms.ToArray();
        }
    }

    /// <summary>Reads artifacts of a tree by StateRef, failing closed when the reference does not resolve to exactly its blob.</summary>
    public static class StateTreeReader
    {
        public static bool Resolves(IStateTree tree, object? stateRef)
        {
            if (!StateV2Shape.IsStateRef(stateRef))
            {
                return false;
            }

            var path = (string)((YamlMap)stateRef!)["path"]!;
            var blob = (string)((YamlMap)stateRef)["blob"]!;
            var known = tree.KnownBlob(path);
            if (known != null)
            {
                return known == blob;
            }

            return tree.TryRead(path, out var bytes) && I62Repo.GitBlobSha1(bytes) == blob;
        }

        public static string? Text(IStateTree tree, object? stateRef) =>
            Resolves(tree, stateRef) && tree.TryRead((string)((YamlMap)stateRef!)["path"]!, out var bytes) ? Encoding.UTF8.GetString(bytes) : null;

        public static JsonObject? Json(IStateTree tree, object? stateRef)
        {
            var text = Text(tree, stateRef);
            if (text == null)
            {
                return null;
            }

            try
            {
                return JsonNode.Parse(text) as JsonObject;
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
        }
    }
}
