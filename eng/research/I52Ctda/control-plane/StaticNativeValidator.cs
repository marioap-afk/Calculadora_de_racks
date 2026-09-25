using System.Text.RegularExpressions;

namespace I52Ctda.ControlPlane;

public sealed record StaticNativeValidation(int DispatchEntries, int EventMappings, int SchedulerSymbols, int SupportActionSymbols, bool BalancedBraces, IReadOnlyList<string> MissingTokens, IReadOnlyList<string> SmBindingViolations)
{
    public bool IsValid => DispatchEntries == 100 && EventMappings == 26 && SchedulerSymbols == 3 && SupportActionSymbols == 7 && BalancedBraces && MissingTokens.Count == 0 && SmBindingViolations.Count == 0;
}

public static class StaticNativeValidator
{
    public static StaticNativeValidation Validate(string repository, ContractCatalog catalog)
    {
        string native = Path.Combine(repository, "eng/research/I52Ctda/native");
        string source = string.Join('\n', Directory.GetFiles(native, "*.cpp").Concat(Directory.GetFiles(native, "*.h")).Select(File.ReadAllText));
        string dispatch = File.ReadAllText(Path.Combine(native, "ProbeDispatchTable.inc"));
        string[] schedulers = ["sendStringToExecute", "beginExecuteInCommandContext", "beginExecuteInApplicationContext"];
        string[] actions = ["veto()", "lockDocument(", "unlockDocument(", "transactionManager()", "startTransaction()", "endTransaction()", "abortTransaction()"];
        var missing = new List<string>();
        foreach (string eventId in catalog.ReachableEventIds) if (!source.Contains($"\"{eventId}\"", StringComparison.Ordinal)) missing.Add(eventId);
        foreach (string scheduler in schedulers) if (!source.Contains(scheduler, StringComparison.Ordinal)) missing.Add(scheduler);
        foreach (string action in actions) if (!source.Contains(action, StringComparison.Ordinal)) missing.Add(action);
        int opens = source.Count(c => c == '{');
        int closes = source.Count(c => c == '}');
        return new(
            Regex.Matches(dispatch, @"^I52_PROBE\(", RegexOptions.Multiline).Count,
            catalog.ReachableEventIds.Count(id => source.Contains($"\"{id}\"", StringComparison.Ordinal)),
            schedulers.Count(s => source.Contains(s, StringComparison.Ordinal)),
            actions.Count(a => source.Contains(a, StringComparison.Ordinal)),
            opens == closes,
            missing,
            SmBindingGuard.Check(File.ReadAllText(Path.Combine(native, "I52CtdaFixture.cpp")))
                .Concat(SmBindingGuard.CheckHeader(File.ReadAllText(Path.Combine(native, "I52CtdaFixture.h")))).ToArray());
    }
}
