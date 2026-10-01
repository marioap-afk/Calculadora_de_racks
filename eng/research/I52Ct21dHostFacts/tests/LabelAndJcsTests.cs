using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// Label (HF-M7) and RFC 8785 vectors. The EXPECTED values come from tests/data/gen_vectors.py, a second implementation written with the
/// Python standard library only (json, hashlib); the two DESIGN-3.2 vectors are also asserted literally from design section 3.2.
/// </summary>
public class LabelAndJcsTests
{
    private static Dictionary<string, string?> Six(JsonNode inputs) =>
        ((JsonObject)inputs).ToDictionary(p => p.Key, p => (string?)p.Value!.GetValue<string>(), StringComparer.Ordinal);

    [Fact]
    public void Label_vectors_generated_independently_with_python_are_reproduced()
    {
        var cases = (JsonArray)Support.Data("label-vectors.json")["cases"]!;
        Assert.True(cases.Count >= 6);
        foreach (var c in cases)
        {
            var r = MachineLabel.Compute(Six(c!["inputs"]!));
            var id = c["id"]!.GetValue<string>();
            Assert.True(r.IsSet, id);
            Assert.Equal(c["serialization"]!.GetValue<string>(), r.Serialization);
            Assert.Equal(c["sha256"]!.GetValue<string>(), r.SerializationSha256);
            Assert.Equal(c["label"]!.GetValue<string>(), r.Label);
        }
    }

    [Fact]
    public void Design_section_3_2_synthetic_vectors_are_exact()
    {
        var six = new Dictionary<string, string?>
        {
            ["AutoCadProduct"] = "SYNTHETIC-PRODUCT", ["AutoCadProfile"] = "SYNTHETIC-PROFILE",
            ["MachineGuid"] = "00000000-0000-0000-0000-000000000000", ["OsVersionBuild"] = "0.0.0.0", ["SECURELOAD"] = "1",
            ["TRUSTEDPATHS"] = "C:\\SYNTHETIC\\modules\\...",
        };
        var a = MachineLabel.Compute(six);
        Assert.Equal("{\"AutoCadProduct\":\"SYNTHETIC-PRODUCT\",\"AutoCadProfile\":\"SYNTHETIC-PROFILE\",\"MachineGuid\":\"00000000-0000-0000-0000-000000000000\",\"OsVersionBuild\":\"0.0.0.0\",\"SECURELOAD\":\"1\",\"TRUSTEDPATHS\":\"C:\\\\SYNTHETIC\\\\modules\\\\...\"}", a.Serialization);
        Assert.Equal("47229d23a125f9bc7bb947aa8d2eef9ee05f8e2484ea82c61c5f2ba31f593780", a.SerializationSha256);
        Assert.Equal("MC-47229d23a125", a.Label);
        six["TRUSTEDPATHS"] = "";
        Assert.Equal("MC-c7edfcd1078e", MachineLabel.Compute(six).Label);
    }

    [Fact]
    public void Label_is_unset_when_any_of_the_six_is_missing_and_never_partial()
    {
        var six = new Dictionary<string, string?> { ["MachineGuid"] = "g" };
        var r = MachineLabel.Compute(six);
        Assert.False(r.IsSet);
        Assert.Equal("UNSET", r.Label);
        Assert.Equal("", r.SerializationSha256);
        foreach (var k in MachineLabel.Keys) six[k] = "v";
        Assert.True(MachineLabel.Compute(six).IsSet);
        six["SECURELOAD"] = null;
        Assert.False(MachineLabel.Compute(six).IsSet);
    }

    [Fact]
    public void A_key_outside_the_closed_list_of_six_is_refused()
    {
        var six = MachineLabel.Keys.ToDictionary(k => k, _ => (string?)"v");
        six["TRUSTEDDOMAINS"] = "x";
        Assert.Throws<ArgumentException>(() => MachineLabel.Compute(six));
    }

    [Fact]
    public void Label_does_not_depend_on_the_order_in_which_the_strings_are_supplied()
    {
        var forward = MachineLabel.Keys.ToDictionary(k => k, k => (string?)("v-" + k));
        var backward = MachineLabel.Keys.Reverse().ToDictionary(k => k, k => (string?)("v-" + k));
        Assert.Equal(MachineLabel.Compute(forward).Label, MachineLabel.Compute(backward).Label);
    }

    [Fact]
    public void Jcs_vectors_generated_independently_with_python_are_reproduced()
    {
        var cases = (JsonArray)Support.Data("jcs-vectors.json")["cases"]!;
        Assert.True(cases.Count >= 9);
        foreach (var c in cases)
        {
            var doc = Jcs.ParseStrict(c!["inputJson"]!.GetValue<string>());
            Assert.Equal(c["expected"]!.GetValue<string>(), Jcs.Serialize(doc));
        }
    }

    [Fact]
    public void Jcs_sorts_member_names_by_utf16_code_units_not_by_code_points()
    {
        // RFC 8785 3.2.3: U+1F600 is the surrogate pair D83D DE00, and D83D < E000, so the astral key sorts BEFORE U+E000.
        var obj = new JsonObject { ["\uE000"] = 1, ["\uD83D\uDE00"] = 2 };
        Assert.Equal("{\"\uD83D\uDE00\":2,\"\uE000\":1}", Jcs.Serialize(obj));
    }

    [Fact]
    public void Jcs_refuses_what_it_cannot_canonicalize_instead_of_approximating()
    {
        Assert.Throws<NotSupportedException>(() => Jcs.Serialize(Jcs.ParseStrict("{\"a\":1.5}")));
        Assert.Throws<ArgumentException>(() => Jcs.Serialize(new JsonObject { ["a"] = "\uD800" }));
        Assert.Throws<ArgumentException>(() => Jcs.Serialize(new JsonObject { ["a"] = "\uDC00x" }));
    }

    [Fact]
    public void Strict_parse_refuses_duplicate_members()
    {
        Assert.ThrowsAny<Exception>(() => Jcs.ParseStrict("{\"a\":1,\"a\":2}"));
    }

    // ---- the raw attribute files (design 3.2 transfer mechanism) --------------------------------------------------------
    private static byte[] Utf8(string s) => new UTF8Encoding(false).GetBytes(s);

    [Fact]
    public void Raw_file_strips_exactly_one_terminator_and_nothing_else()
    {
        Assert.Equal("abc", RawAttributeText.Read(Utf8("abc\n"), 3, true).Text);
        Assert.Equal("abc", RawAttributeText.Read(Utf8("abc\r\n"), 3, true).Text);
        Assert.Equal("abc\n", RawAttributeText.Read(Utf8("abc\n\n"), 4, true).Text); // the second LF belongs to the string
        Assert.Equal(" abc ", RawAttributeText.Read(Utf8(" abc \n"), 5, true).Text); // no trimming
        Assert.Equal("", RawAttributeText.Read(Utf8("\n"), 0, true).Text); // an empty string is a valid value (TRUSTEDPATHS)
    }

    [Fact]
    public void Raw_file_without_terminator_bom_or_valid_utf8_is_unknown()
    {
        Assert.Equal("NO_LINE_TERMINATOR", RawAttributeText.Read(Utf8("abc"), 3, true).Reason);
        Assert.Equal("BOM_PRESENT", RawAttributeText.Read(new byte[] { 0xEF, 0xBB, 0xBF, 0x61, 0x0A }, 1, true).Reason);
        Assert.Equal("INVALID_UTF8", RawAttributeText.Read(new byte[] { 0x61, 0xFF, 0x0A }, 2, true).Reason);
        Assert.Equal("INVALID_UTF8", RawAttributeText.Read(new byte[] { 0xC3, 0x28, 0x0A }, 2, true).Reason); // truncated sequence
    }

    [Fact]
    public void Declared_strlen_must_match_the_decoded_character_count()
    {
        Assert.Equal(RawAttributeStatus.Observed, RawAttributeText.Read(Utf8("Mar\u00eda\n"), 5, true).Status);
        Assert.StartsWith("LENGTH_MISMATCH", RawAttributeText.Read(Utf8("Mar\u00eda\n"), 6, true).Reason); // 6 would be a byte count
        Assert.Equal(RawAttributeStatus.Observed, RawAttributeText.Read(Utf8("\U0001F600\n"), 1, true).Status); // one scalar value
        Assert.Equal("DECLARED_LENGTH_MISSING", RawAttributeText.Read(Utf8("abc\n"), null, true).Reason);
        Assert.Equal(RawAttributeStatus.Observed, RawAttributeText.Read(Utf8("abc\n"), null, false).Status);
    }

    [Fact]
    public void Label_helper_recomputes_the_design_vector_from_six_raw_files()
    {
        using var t = new TempDir();
        var vector = new Dictionary<string, string>
        {
            ["AutoCadProduct"] = "SYNTHETIC-PRODUCT", ["AutoCadProfile"] = "SYNTHETIC-PROFILE",
            ["MachineGuid"] = "00000000-0000-0000-0000-000000000000", ["OsVersionBuild"] = "0.0.0.0", ["SECURELOAD"] = "1",
            ["TRUSTEDPATHS"] = "C:\\SYNTHETIC\\modules\\...",
        };
        foreach (var (k, v) in vector) File.WriteAllBytes(Path.Combine(t.Path, LabelHelper.RawFileName(k)), Utf8(v + "\n"));
        void Declare(string key, int n) => File.WriteAllText(Path.Combine(t.Path, LabelHelper.DeclaredLengthFileName(key)), n + "\n");
        Declare("AutoCadProduct", 17);
        Declare("AutoCadProfile", 17);
        Declare("SECURELOAD", 1);
        Declare("TRUSTEDPATHS", vector["TRUSTEDPATHS"].Length);
        var outcome = LabelHelper.ComputeFromFolder(t.Path);
        Assert.True(outcome.Label.IsSet);
        Assert.Equal("MC-47229d23a125", outcome.Label.Label);

        // a wrong declared length makes that attribute UNKNOWN and the label UNSET (never partial)
        File.WriteAllText(Path.Combine(t.Path, LabelHelper.DeclaredLengthFileName("AutoCadProduct")), "99\n");
        var bad = LabelHelper.ComputeFromFolder(t.Path);
        Assert.False(bad.Label.IsSet);
        Assert.Equal(RawAttributeStatus.Unknown, bad.Attributes["AutoCadProduct"].Status);

        // a missing declared length of an AutoCAD read likewise
        Declare("AutoCadProduct", 17);
        File.Delete(Path.Combine(t.Path, LabelHelper.DeclaredLengthFileName("SECURELOAD")));
        Assert.Equal("DECLARED_LENGTH_MISSING", LabelHelper.ComputeFromFolder(t.Path).Attributes["SECURELOAD"].Reason);
        Declare("SECURELOAD", 1);

        // a missing raw file likewise
        File.Delete(Path.Combine(t.Path, LabelHelper.RawFileName("MachineGuid")));
        Assert.Equal("RAW_FILE_MISSING", LabelHelper.ComputeFromFolder(t.Path).Attributes["MachineGuid"].Reason);
    }
}
