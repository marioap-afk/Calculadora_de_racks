using System.Globalization;
using System.Text;

namespace I52Ct21d.HostFacts.Core;

public enum RawAttributeStatus { Observed, Unknown }

/// <summary>The extracted string of one raw attribute file, or the reason it is UNKNOWN.</summary>
public sealed record RawAttributeResult(RawAttributeStatus Status, string? Text, string Reason, int? RuneCount)
{
    public static RawAttributeResult Unknown(string reason) => new(RawAttributeStatus.Unknown, null, reason, null);
}

/// <summary>
/// Offline helper of design 3.2 (transfer mechanism). The raw file holds exactly the string in UTF-8 without BOM plus ONE line
/// terminator. The helper (i) rejects invalid UTF-8, (ii) strips exactly one trailing terminator (LF or CRLF) and nothing else, and
/// (iii) compares the number of characters it decoded with the number the CAD manager recorded from <c>(strlen ...)</c> in the same
/// session; a mismatch makes the attribute UNKNOWN. No trimming, no case folding, no path normalization.
/// HOST-TO-CONFIRM: whether AutoLISP <c>strlen</c> counts characters or bytes; this helper counts Unicode scalar values and fails
/// closed (UNKNOWN) on any mismatch.
/// </summary>
public static class RawAttributeText
{
    private static readonly UTF8Encoding Strict = new(false, true);

    public static RawAttributeResult Read(byte[] bytes, int? declaredLength, bool declaredLengthRequired)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return RawAttributeResult.Unknown("BOM_PRESENT");
        string decoded;
        try { decoded = Strict.GetString(bytes); }
        catch (DecoderFallbackException) { return RawAttributeResult.Unknown("INVALID_UTF8"); }

        string text;
        if (decoded.EndsWith("\r\n", StringComparison.Ordinal)) text = decoded[..^2];
        else if (decoded.EndsWith('\n')) text = decoded[..^1];
        else return RawAttributeResult.Unknown("NO_LINE_TERMINATOR");

        var runes = 0;
        foreach (var _ in text.EnumerateRunes()) runes++;

        if (declaredLength is null)
        {
            if (declaredLengthRequired) return RawAttributeResult.Unknown("DECLARED_LENGTH_MISSING");
        }
        else if (declaredLength.Value != runes)
        {
            return RawAttributeResult.Unknown($"LENGTH_MISMATCH declared={declaredLength.Value} decoded={runes}");
        }
        return new RawAttributeResult(RawAttributeStatus.Observed, text, "", runes);
    }
}

/// <summary>The six raw files of I-1 and the offline recomputation of HF-M7 from a folder.</summary>
public static class LabelHelper
{
    /// <summary>File name of the raw file of an attribute (A1-R1..A1-R6).</summary>
    public static string RawFileName(string key) => "raw-" + key + ".txt";

    /// <summary>
    /// File name of the typed <c>(strlen ...)</c> value of an attribute (design 3.2 (iii)): decimal digits plus one line terminator. One
    /// file per attribute, so that every result file is written once (create-new) and nothing is ever appended.
    /// </summary>
    public static string DeclaredLengthFileName(string key) => "declared-length-" + key + ".txt";

    // A1-R1 and A1-R2 are read by 64-bit PowerShell; A1-R3..A1-R6 by a typed AutoLISP expression with a (strlen) cross-check.
    private static readonly HashSet<string> LengthRequired = new(StringComparer.Ordinal)
    {
        "AutoCadProduct", "AutoCadProfile", "SECURELOAD", "TRUSTEDPATHS",
    };

    public sealed record Outcome(MachineLabelResult Label, IReadOnlyDictionary<string, RawAttributeResult> Attributes);

    public static Outcome ComputeFromFolder(string folder)
    {
        var attrs = new Dictionary<string, RawAttributeResult>(StringComparer.Ordinal);
        var strings = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var key in MachineLabel.Keys)
        {
            var path = Path.Combine(folder, RawFileName(key));
            if (!File.Exists(path))
            {
                attrs[key] = RawAttributeResult.Unknown("RAW_FILE_MISSING");
                continue;
            }
            var r = RawAttributeText.Read(File.ReadAllBytes(path), ReadDeclaredLength(Path.Combine(folder, DeclaredLengthFileName(key))), LengthRequired.Contains(key));
            attrs[key] = r;
            if (r.Status == RawAttributeStatus.Observed) strings[key] = r.Text;
        }
        return new Outcome(MachineLabel.Compute(strings), attrs);
    }

    // a missing or malformed length file declares nothing: the attribute then fails closed when the length is required
    private static int? ReadDeclaredLength(string path)
    {
        if (!File.Exists(path)) return null;
        var text = File.ReadAllText(path).TrimEnd((char)13, (char)10);
        return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : null;
    }
}
