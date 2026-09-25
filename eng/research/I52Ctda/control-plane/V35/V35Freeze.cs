using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace I52Ctda.ControlPlane.V35;

// Canonical serialization of the V35 approval hashes (oracle_predicates.canon == validate-v35-catalog.ps1 Canon):
// keys sorted ordinally, no whitespace, integers as digits, strings escaped with \uXXXX outside 0x20..0x7E.
public static class V35Canon
{
    public static string Serialize(JsonNode? node)
    {
        var text = new StringBuilder();
        Write(node, text);
        return text.ToString();
    }

    public static string Sha256(JsonNode? node) => Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(Serialize(node))));

    public static string Sha256OfStrings(IReadOnlyDictionary<string, string> map)
    {
        var node = new JsonObject();
        foreach ((string key, string value) in map) node[key] = value;
        return Sha256(node);
    }

    private static void Write(JsonNode? node, StringBuilder text)
    {
        switch (node)
        {
            case null: text.Append("null"); return;
            case JsonObject obj:
                text.Append('{');
                bool first = true;
                foreach (string key in obj.Select(p => p.Key).OrderBy(k => k, StringComparer.Ordinal))
                {
                    if (!first) text.Append(',');
                    first = false;
                    WriteString(key, text);
                    text.Append(':');
                    Write(obj[key], text);
                }
                text.Append('}');
                return;
            case JsonArray array:
                text.Append('[');
                for (int i = 0; i < array.Count; i++)
                {
                    if (i > 0) text.Append(',');
                    Write(array[i], text);
                }
                text.Append(']');
                return;
            case JsonValue value:
                if (value.TryGetValue(out bool b)) { text.Append(b ? "true" : "false"); return; }
                if (value.TryGetValue(out string? s)) { WriteString(s!, text); return; }
                if (value.TryGetValue(out long l)) { text.Append(l.ToString(System.Globalization.CultureInfo.InvariantCulture)); return; }
                throw new InvalidDataException($"Canonical serialization supports only strings, booleans, integers and null: {value.ToJsonString()}");
        }
    }

    private static void WriteString(string value, StringBuilder text)
    {
        text.Append('"');
        foreach (char ch in value)
        {
            if (ch is '"' or '\\') text.Append('\\').Append(ch);
            else if (ch >= 0x20 && ch <= 0x7E) text.Append(ch);
            else text.Append("\\u").Append(((int)ch).ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
        }
        text.Append('"');
    }
}

public sealed record V35FreezeVerification(string ExpectedPackageHash, string ManifestPackageHash, string ComputedPackageHash, int Blobs, IReadOnlyList<string> Mismatches)
{
    public bool Holds => Mismatches.Count == 0 && ComputedPackageHash == ExpectedPackageHash && ManifestPackageHash == ExpectedPackageHash && Blobs == V35Freeze.PackageBlobCount;
}

// The governing V35 freeze (decision 172). Every consumer of V35 authority verifies it before reading a plan:
// the package hash is recomputed from the manifest and every bound file is compared with its frozen git blob id.
public static class V35Freeze
{
    public const string PackageHash = "43DCE809AA5B124E73B67E2B8B76EB78DC921FCE0BE961B2906BC21BE9D3B6DF";
    public const string FreezeSha = "86089886f37da05c2de5dcf9e237044a3deeada3";
    public const string Revision = "V35-A2";
    public const int PackageBlobCount = 24;
    public const string ManifestPath = "docs/automation/evidence/I-52-v35-freeze-manifest.json";

    public static V35FreezeVerification Verify(string repository)
    {
        var mismatches = new List<string>();
        JsonObject manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(repository, ManifestPath)))!.AsObject();
        string manifestHash = manifest["V35_FREEZE_PACKAGE_HASH"]!.GetValue<string>();
        JsonObject package = manifest["package"]!.AsObject();
        string computed = V35Canon.Sha256(package);
        int blobs = 0;
        foreach ((string group, JsonNode? entries) in package["blobs"]!.AsObject())
        {
            foreach ((string path, JsonNode? blob) in entries!.AsObject())
            {
                blobs++;
                string file = Path.Combine(repository, path);
                if (!File.Exists(file)) { mismatches.Add($"{group}:{path}: missing"); continue; }
                string actual = GitBlobId(file);
                if (actual != blob!.GetValue<string>()) mismatches.Add($"{group}:{path}: blob {actual} differs from frozen {blob.GetValue<string>()}");
            }
        }
        if (package["revision"]?.GetValue<string>() != Revision) mismatches.Add("package revision is not " + Revision);
        return new(PackageHash, manifestHash, computed, blobs, mismatches);
    }

    public static void AssertHolds(string repository)
    {
        V35FreezeVerification result = Verify(repository);
        if (!result.Holds)
            throw new InvalidDataException("V35 freeze does not hold; STOP (normative V35 drift): " + string.Join("; ", result.Mismatches.DefaultIfEmpty(result.ComputedPackageHash)));
    }

    // git blob id of the file as git stores it with core.autocrlf: CRLF is normalized to LF before hashing.
    public static string GitBlobId(string path)
    {
        byte[] raw = File.ReadAllBytes(path);
        var body = new List<byte>(raw.Length);
        for (int i = 0; i < raw.Length; i++)
        {
            if (raw[i] == (byte)'\r' && i + 1 < raw.Length && raw[i + 1] == (byte)'\n') continue;
            body.Add(raw[i]);
        }
        byte[] header = Encoding.ASCII.GetBytes($"blob {body.Count}\0");
        return Convert.ToHexString(SHA1.HashData(header.Concat(body).ToArray())).ToLowerInvariant();
    }
}
