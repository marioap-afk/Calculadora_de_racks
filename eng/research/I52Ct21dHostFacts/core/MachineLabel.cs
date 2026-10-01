using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

/// <summary>Result of the PARAM-05 label computation (HF-M7; BA-10 V8 section 2.2).</summary>
public sealed record MachineLabelResult(bool IsSet, string Label, string SerializationSha256, string Serialization);

/// <summary>
/// The machine-class label of BA-10 V8 section 2.2: the RFC 8785 serialization, in UTF-8, of a JSON object with EXACTLY the six
/// keys below, every value a JSON string holding the exact text that the observation read; label = <c>MC-</c> + the first 12
/// lowercase hexadecimal digits of the SHA-256 of those bytes. Computed only when all six are present; otherwise
/// <c>UNSET</c>, never partial (design 3.4, HF-M7).
/// </summary>
public static class MachineLabel
{
    public static readonly IReadOnlyList<string> Keys = new[]
    {
        "AutoCadProduct", "AutoCadProfile", "MachineGuid", "OsVersionBuild", "SECURELOAD", "TRUSTEDPATHS",
    };

    public const string Unset = "UNSET";

    /// <summary>Throws when a key outside the closed list is supplied. A missing key gives <see cref="Unset"/>.</summary>
    public static MachineLabelResult Compute(IReadOnlyDictionary<string, string?> strings)
    {
        foreach (var key in strings.Keys)
            if (!Keys.Contains(key, StringComparer.Ordinal))
                throw new ArgumentException("key outside the closed list of six: " + key);

        foreach (var key in Keys)
            if (!strings.TryGetValue(key, out var v) || v is null)
                return new MachineLabelResult(false, Unset, "", "");

        var obj = new JsonObject();
        foreach (var key in Keys) obj[key] = strings[key]!;
        var serialization = Jcs.Serialize(obj);
        var sha = Sha256Hex.Of(Jcs.SerializeUtf8(obj));
        return new MachineLabelResult(true, "MC-" + sha[..12], sha, serialization);
    }
}
