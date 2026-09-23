using System.Text.RegularExpressions;

namespace I52Ctda.ControlPlane;

public sealed record HeaderAuthorityValidation(int ReachableCallbacksVerified, int SchedulerSignaturesVerified, int SupportSignaturesVerified, IReadOnlyList<string> Missing)
{
    public bool IsValid => ReachableCallbacksVerified == 26 && SchedulerSignaturesVerified == 3 && SupportSignaturesVerified == 7 && Missing.Count == 0;
}

public static class HeaderAuthorityValidator
{
    private static readonly Regex InventoryRow = new(@"^\| `(?<id>N-[^`]+)` \| `(?<class>[^`]+)` \| `(?<callback>[^`]+)` \| `(?<header>[^`]+)` \|", RegexOptions.Multiline | RegexOptions.Compiled);

    public static HeaderAuthorityValidation Validate(string repository, string sdkRoot)
    {
        string nec = File.ReadAllText(Path.Combine(repository, "docs/initiatives/I-52-native-event-catalog-v34.md"));
        var missing = new List<string>();
        int callbacks = 0;
        foreach (Match row in InventoryRow.Matches(nec).Where(m => m.Groups["id"].Value != "N-ED-FAIL"))
        {
            string headerPath = Path.Combine(sdkRoot, "inc", row.Groups["header"].Value);
            string callback = row.Groups["callback"].Value;
            if (!File.Exists(headerPath) || !Regex.IsMatch(File.ReadAllText(headerPath), $@"virtual\s+void\s+{Regex.Escape(callback)}\s*\(")) missing.Add(row.Groups["id"].Value);
            else callbacks++;
        }

        string acdocman = File.ReadAllText(Path.Combine(sdkRoot, "inc", "acdocman.h"));
        string[] schedulers =
        [
            @"virtual\s+Acad::ErrorStatus\s+sendStringToExecute\s*\(AcApDocument\*",
            @"Acad::ErrorStatus\s+beginExecuteInCommandContext\s*\(void\s*\(\*procAddr\)\(void\s*\*\)",
            @"Acad::ErrorStatus\s+beginExecuteInApplicationContext\s*\(void\s*\(\*procAddr\)\(void\s*\*\)"
        ];
        int schedulerCount = Count(acdocman, schedulers, "scheduler", missing);

        string dbtrans = File.ReadAllText(Path.Combine(sdkRoot, "inc", "dbtrans.h"));
        string[] supportPatterns =
        [
            @"Acad::ErrorStatus\s+veto\s*\(\)", @"Acad::ErrorStatus\s+lockDocument\s*\(AcApDocument\*",
            @"Acad::ErrorStatus\s+unlockDocument\s*\(AcApDocument\*", @"AcTransactionManager\*\s+transactionManager\s*\(\)\s*const",
            @"AcTransaction\*\s+startTransaction\s*\(\)", @"Acad::ErrorStatus\s+endTransaction\s*\(\)", @"Acad::ErrorStatus\s+abortTransaction\s*\(\)"
        ];
        int supportCount = Count(acdocman + "\n" + dbtrans, supportPatterns, "support", missing);
        return new(callbacks, schedulerCount, supportCount, missing);
    }

    private static int Count(string text, IEnumerable<string> patterns, string prefix, ICollection<string> missing)
    {
        int count = 0;
        int index = 0;
        foreach (string pattern in patterns)
        {
            index++;
            if (Regex.IsMatch(text, pattern, RegexOptions.Multiline)) count++; else missing.Add($"{prefix}-{index}");
        }
        return count;
    }
}
