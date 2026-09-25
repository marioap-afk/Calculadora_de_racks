using System.Text.Json;
using System.Text.RegularExpressions;

namespace I52Ctda.ControlPlane;

// V34 binding clarifications (decisions section 164). SM-LINK: one AcDbXrecord owned by the scratch NOD under
// RACKCAD_CTDA_V34_SM-LINK with exactly one kDxfText resbuf; it is a relation store, never a fixture identity.
// OPEN-SM-B: sibling membership is the SM-LINK relation plus liveness of the named members; F-REF-B stays live as
// the M target and leaves the sibling set only relationally; F-REF-C does not exist until MUT-SM appends it.
public static class SmBindingContract
{
    public const string LinkKey = "RACKCAD_CTDA_V34_SM-LINK";
    public const string LinkPrefix = "HFV30:LINK:";
    public const string StateSm0 = "HFV30:LINK:A,B";
    public const string StateSm1 = "HFV30:LINK:A,C";
    public const string BindingLinePrefix = "BINDING:SM-LINK|";
}

public sealed record SmLinkFacts(bool KeyPresent, bool IdentityMatches, bool Erased, bool IsXrecord, int ResbufCount, bool IsText, int CarriersFound, string Value)
{
    public static SmLinkFacts Valid(string value) => new(true, true, false, true, 1, true, 1, value);
}

// Raw facts captured by I52CtdaFixture::captureSm; BRead is false after a trigger because F-REF-B is never opened then.
public sealed record SmFacts(SmLinkFacts Link, bool AnchorALive, bool BRead, bool BLive, int ForeignReferenceInserts, bool CBound, bool CFound, bool CLive, bool CAtCreationPosition, string AEvidence, string CEvidence)
{
    public static SmFacts Parse(JsonElement element)
    {
        JsonElement link = element.GetProperty("link");
        return new(
            new SmLinkFacts(link.GetProperty("keyPresent").GetBoolean(), link.GetProperty("identityMatches").GetBoolean(), link.GetProperty("erased").GetBoolean(),
                link.GetProperty("isXrecord").GetBoolean(), link.GetProperty("resbufCount").GetInt32(), link.GetProperty("isText").GetBoolean(),
                link.GetProperty("carriersFound").GetInt32(), link.GetProperty("value").GetString() ?? ""),
            element.GetProperty("anchorALive").GetBoolean(), element.GetProperty("bRead").GetBoolean(), element.GetProperty("bLive").GetBoolean(),
            element.GetProperty("foreignReferenceInserts").GetInt32(), element.GetProperty("cBound").GetBoolean(), element.GetProperty("cFound").GetBoolean(),
            element.GetProperty("cLive").GetBoolean(), element.GetProperty("cAtCreationPosition").GetBoolean(),
            element.GetProperty("aEvidence").GetString() ?? "", element.GetProperty("cEvidence").GetString() ?? "");
    }
}

// The single classifier of SM facts. Structural anomalies are UNKNOWN; only a structurally valid carrier with the
// wrong post-mutation bytes is FAIL-SM; no anomaly can reach OK.
public static class SmRules
{
    public const string Ok = "OK";

    public static string? LinkStructure(SmLinkFacts link)
    {
        if (!link.KeyPresent) return "LINK-MISSING";
        if (link.Erased) return "LINK-ERASED";
        if (!link.IsXrecord || link.ResbufCount != 1 || !link.IsText) return "LINK-WRONG-TYPE";
        if (link.CarriersFound != 1) return "LINK-DUPLICATE";
        if (!link.IdentityMatches) return "LINK-IDENTITY-DRIFT";
        return null;
    }

    // Before any SM trigger: carrier = A,B, A live, B live, C absent. Anything else is UNKNOWN and the trigger must not fire.
    public static string ClassifyPrecondition(SmFacts facts)
    {
        if (LinkStructure(facts.Link) is { } structural) return "UNKNOWN:" + structural;
        if (!string.Equals(facts.Link.Value, SmBindingContract.StateSm0, StringComparison.Ordinal)) return "UNKNOWN:LINK-PRECONDITION-MISMATCH";
        if (!facts.AnchorALive) return "UNKNOWN:A-NOT-LIVE";
        if (!facts.BRead || !facts.BLive) return "UNKNOWN:B-NOT-LIVE";
        if (facts.CBound || facts.CFound || facts.ForeignReferenceInserts != 0) return "UNKNOWN:C-PRESENT";
        return Ok;
    }

    // VER-SM after MUT-SM: carrier = A,C, A live, exactly one new REF insert which is the bound C at its creation
    // position. F-REF-B is not part of this read (BRead must be false).
    public static string ClassifyAfter(SmFacts facts)
    {
        if (facts.BRead) return "UNKNOWN:B-OPENED-AFTER-TRIGGER";
        if (LinkStructure(facts.Link) is { } structural) return "UNKNOWN:" + structural;
        if (!facts.AnchorALive) return "UNKNOWN:A-NOT-LIVE";
        if (facts.ForeignReferenceInserts == 0) return "UNKNOWN:C-NONE";
        if (facts.ForeignReferenceInserts > 1) return "UNKNOWN:C-MULTIPLE";
        if (!facts.CBound || !facts.CFound) return "UNKNOWN:C-IDENTITY-DRIFT";
        if (!facts.CLive) return "UNKNOWN:C-NOT-LIVE";
        if (!facts.CAtCreationPosition) return "UNKNOWN:C-IDENTITY-DRIFT";
        if (!string.Equals(facts.Link.Value, SmBindingContract.StateSm1, StringComparison.Ordinal)) return "FAIL-SM:LINK-VALUE";
        return Ok;
    }

    // Smoke snapshot: exactly the seven identity lines (F-REF-C ABSENT) plus one resolved SM-LINK binding line.
    public static string? SnapshotShape(string snapshot)
    {
        string[] lines = snapshot.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string[] identities = lines.Where(l => l.StartsWith("F-", StringComparison.Ordinal)).ToArray();
        string[] bindings = lines.Where(l => l.StartsWith(SmBindingContract.BindingLinePrefix, StringComparison.Ordinal)).ToArray();
        if (identities.Length != FixtureSpecification.Objects.Count || lines.Length != identities.Length + bindings.Length) return "SNAPSHOT_IDENTITY_LINES";
        if (identities.Any(l => !l.EndsWith("|RESOLVED", StringComparison.Ordinal))) return "SNAPSHOT_IDENTITY_UNRESOLVED";
        if (!identities.Any(l => l.StartsWith("F-REF-C|NULL|ABSENT|", StringComparison.Ordinal))) return "SNAPSHOT_C_NOT_ABSENT";
        if (bindings.Length != 1) return "SNAPSHOT_BINDING_LINES";
        if (!Regex.IsMatch(bindings[0], @"^BINDING:SM-LINK\|[0-9A-F]+\|PRESENT\|STATE-SM-0\|RESOLVED$")) return "SNAPSHOT_BINDING_INVALID";
        return null;
    }
}

// Source-level guards for native SM code that cannot run without AutoCAD. They inspect the exact function bodies.
public static class SmBindingGuard
{
    public static IReadOnlyList<string> Check(string fixtureSource)
    {
        var violations = new List<string>();
        string mixed = Body(fixtureSource, "I52CtdaFixture::mutateMixed");
        string all = Body(fixtureSource, "I52CtdaFixture::mutateAll");
        string materialize = Body(fixtureSource, "I52CtdaFixture::materialize");
        if (mixed.Length == 0 || all.Length == 0 || materialize.Length == 0) { violations.Add("SM functions not found"); return violations; }
        if (mixed.Contains("erase(", StringComparison.Ordinal)) violations.Add("MUT-SM erases an object");
        if (mixed.Contains("materialReference", StringComparison.Ordinal)) violations.Add("MUT-SM references F-REF-B");
        if (Regex.IsMatch(mixed, @"openErased|,\s*true\s*\)")) violations.Add("MUT-SM opens erased objects");
        if (mixed.Contains("semanticXrecord", StringComparison.Ordinal) || mixed.Contains("mutateSemantic", StringComparison.Ordinal) || mixed.Contains("kStateS", StringComparison.Ordinal)) violations.Add("MUT-SM writes F-XR");
        if (!mixed.Contains("kLinkSm1", StringComparison.Ordinal) || !mixed.Contains("kSiblingCCreationPosition", StringComparison.Ordinal)) violations.Add("MUT-SM write set is not {SM-LINK, new C}");
        if (Regex.Matches(mixed, @"appendReference\(").Count != 1) violations.Add("MUT-SM must append exactly one reference");
        if (Regex.Matches(all, @"mutateSemantic\(\)").Count != 1 || Regex.Matches(all, @"mutateMaterial\(\)").Count != 1 || Regex.Matches(all, @"mutateMixed\(\)").Count != 1) violations.Add("MUT-ALL must run MUT-S, MUT-M and MUT-SM exactly once");
        if (materialize.Contains("siblingC", StringComparison.Ordinal) || materialize.Contains("kSiblingCCreationPosition", StringComparison.Ordinal)) violations.Add("bootstrap precreates F-REF-C");
        if (!materialize.Contains("kLinkKey", StringComparison.Ordinal) || !materialize.Contains("kLinkSm0", StringComparison.Ordinal)) violations.Add("bootstrap does not create the SM-LINK carrier at STATE-SM-0");
        if (!Regex.IsMatch(fixtureSource, @"kLinkKey\s*=\s*L""" + Regex.Escape(SmBindingContract.LinkKey) + @"""")) violations.Add("SM-LINK key differs from the binding");
        return violations;
    }

    public static IReadOnlyList<string> CheckHeader(string fixtureHeader)
    {
        var violations = new List<string>();
        // fixture-v35.json: eight persistent identities (F-TRIGGER-XR added by V35); R-SM-LINK is still not one.
        if (!Regex.IsMatch(fixtureHeader, @"kDeclaredIdentities\s*=\s*8;")) violations.Add("declared identity count is not 8");
        return violations;
    }

    // Text of the brace-balanced body that follows the definition "<name>(".
    private static string Body(string source, string name)
    {
        int at = source.IndexOf(name + "(", StringComparison.Ordinal);
        if (at < 0) return "";
        int open = source.IndexOf('{', at);
        if (open < 0) return "";
        int depth = 0;
        for (int i = open; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}' && --depth == 0) return source.Substring(open, i - open + 1);
        }
        return "";
    }
}
