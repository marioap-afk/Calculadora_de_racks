#nullable enable
using System;
using System.Collections.Generic;
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

    /// <summary>Reads artifacts of a tree by StateRef, failing closed when the reference does not resolve to exactly its blob.</summary>
    public static class StateTreeReader
    {
        public static bool Resolves(IStateTree tree, object? stateRef) =>
            StateV2Shape.IsStateRef(stateRef) && tree.TryRead((string)((YamlMap)stateRef!)["path"]!, out var bytes)
            && I62Repo.GitBlobSha1(bytes) == (string)((YamlMap)stateRef!)["blob"]!;

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
