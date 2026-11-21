#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4C1-HOST (D-T4C-14, D-T4C-20 rows 19 and 22; INV-F4T4C-03, 04, 06, 13, 14, 15 and 19): "NO SECOND WRITE PATH". The guards of the whole
// product for the integration of the update of the pilot, each with its negative over the REAL text: the purity of the runner, the guard of
// the adapter, G-1 to G-7, G-9 and G-10, the census of Task and async, I-01, M-01, the callers of EnterError and the handler of the command
// that is not the update. The Core test project cannot reference the Plugin (it needs AutoCAD), so the product is read as text.
public class FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests
{
    private const string AdapterPath = "src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs";
    private const string HostRunPath = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";

    private static IReadOnlyList<(string Path, string Code)> Product() => Src.AllProduct();

    private static IReadOnlyList<(string Path, string Code)> With(IReadOnlyList<(string Path, string Code)> product, string path, Func<string, string> change) =>
        product.Select(f => f.Path == path ? (f.Path, change(f.Code)) : f).ToList();

    private static IReadOnlyList<(string Path, string Code)> Append(IReadOnlyList<(string Path, string Code)> product, string path, string text) =>
        With(product, path, code => code + "\n" + text);

    private static string CodeOf(string path) => Src.Code(path);

    // ================================================================== G-4: one caller of the update of T4B

    // The code without the text of its strings: the context that the core gives to its log is not an identifier.
    private static string WithoutStrings(string code) => Regex.Replace(code, "\"(?:[^\"\\\\\\r\\n]|\\\\.)*\"", "\"\"");

    private static IEnumerable<string> G4Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        // M-07: the IDENTIFIER, as a whole word and outside the text of a string, so that a group of methods (a delegate that is the method itself)
        // is seen too: it appears in the core twice (its declaration and its one call) and nowhere else.
        var callers = product.Where(f => Regex.IsMatch(WithoutStrings(f.Code), @"\bTryUpdatePilot\b")).Select(f => f.Path).ToArray();
        if (!callers.SequenceEqual(new[] { CorePath })) yield return "the callers are in " + string.Join(", ", callers);

        var core = product.Single(f => f.Path == CorePath).Code;
        var run = Src.BodyOf(core, @"private\s+sealed\s+class\s+PilotUpdateRun\b");
        var execute = Src.BodyOf(run, @"public\s+void\s+Execute\s*\(");
        if (Src.Count(core, @"\.TryUpdatePilot\s*\(") != 1 || Src.Count(execute, @"\.TryUpdatePilot\s*\(") != 1) yield return "TryUpdatePilot is not called exactly once, from Execute of the run of the core";
        if (Src.Count(WithoutStrings(core), @"\bTryUpdatePilot\b") != 2) yield return "the identifier TryUpdatePilot appears other than as its declaration and its one call";

        var runCallers = product.Where(f => Regex.IsMatch(f.Code, @"PushBackPilotUpdateRun\.ExecuteFull\s*\(")).Select(f => f.Path).ToArray();
        if (!runCallers.SequenceEqual(new[] { CorePath })) yield return "PushBackPilotUpdateRun.ExecuteFull is named in " + string.Join(", ", runCallers);
    }

    [Fact]
    public void G4OnlyTheExecuteOfTheRunOfTheCoreCallsTheUpdateAndOnlyTheCoreCallsTheRunOfT4B()
    {
        Assert.Empty(G4Violations(Product()));
        // I-64 F4B PB-T4b (the self-test of IG:510-517 over the full run): a caller of the run outside the core, a caller of the update
        // outside the core and a second call in the core trip the guard.
        Assert.NotEmpty(G4Violations(Append(Product(), HostRunPath, "var o = PushBackPilotUpdateRun.ExecuteFull(draft, request, port);")));
        Assert.NotEmpty(G4Violations(Append(Product(), ControllerPath, "var u = _host.TryUpdatePilot(request, port);")));
        Assert.NotEmpty(G4Violations(Append(Product(), AdapterPath, "var u = core.TryUpdatePilot(request, port);")));
        Assert.NotEmpty(G4Violations(With(Product(), CorePath, code => code.Replace("public EditUpdateOutcome TryUpdatePilot(", "public EditUpdateOutcome Other() { return TryUpdatePilot(null!, null!); }\n        public EditUpdateOutcome TryUpdatePilot(", StringComparison.Ordinal))));
    }

    [Fact]
    public void G4TripsOnAGroupOfMethodsOfTheUpdateOutsideTheCoreAndInsideItAndNotOnTheNameInsideAString()
    {
        var product = Product();

        Assert.Empty(G4Violations(product));
        // M-07: no parenthesis at all, so the old expression of the guard did not see these.
        Assert.NotEmpty(G4Violations(Append(product, ControllerPath, "Func<EditIntentRequest, IPushBackPilotUpdatePort, EditUpdateOutcome> f = core.TryUpdatePilot;")));
        Assert.NotEmpty(G4Violations(Append(product, AdapterPath, "var f = (Delegate)host.TryUpdatePilot;")));
        Assert.NotEmpty(G4Violations(Append(product, CorePath, "Func<EditIntentRequest, IPushBackPilotUpdatePort, EditUpdateOutcome> f = TryUpdatePilot;")));
        // The name of the method that the log of the core carries in a string is not an identifier: the guard does not count it.
        Assert.Contains("\"WorkspaceHostCore.TryUpdatePilot\"", CodeOf(CorePath));
        Assert.Empty(G4Violations(Append(product, CorePath, "var context = \"WorkspaceHostCore.TryUpdatePilot\";")));
    }
}
