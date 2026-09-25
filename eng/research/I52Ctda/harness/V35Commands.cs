using System.Text.Json;
using System.Text.Json.Serialization;
using I52Ctda.ControlPlane.V35;

// V35 (R3) harness commands. Every command verifies the V35 freeze before it reads a plan.
internal static class V35Commands
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
    private static readonly string[] Commands = ["freeze-v35", "generate-v35", "check-generated-v35", "plan", "conformance-v35", "probe", "smoke-v35", "result-v35", "tuple-v35"];
    private const string Acad = @"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe";

    public static bool Handles(string command) => Commands.Contains(command);

    public static async Task<int> RunAsync(string command, string repo, string[] rest)
    {
        switch (command)
        {
            case "conformance-v35":
            {
                V35ConformanceReport report = V35Conformance.Check(repo, V35Authority.Load(repo));
                Console.WriteLine(JsonSerializer.Serialize(report, Json));
                return report.Holds ? 0 : 1;
            }
            case "probe" when rest.Length is 4 or 5:
            {
                // probe <repo> <out> <I52CtdaNative.arx> <scratch.dwg> <ProbeId> [profile]; not executed in the build gate.
                V35Authority authority = V35Authority.Load(repo);
                V35ProbeLaunch launch;
                try { launch = V35ProbeLaunch.Create(authority, Acad, rest[1], rest[2], rest[0], rest[3], rest.Length == 5 ? rest[4] : null); }
                catch (Exception e) when (e is ArgumentException or FileNotFoundException) { Console.Error.WriteLine(e.Message); return 2; }
                (int code, V35Result? result) = await V35ProbeHarness.RunAsync(authority, launch);
                Console.WriteLine(JsonSerializer.Serialize(new { result?.ProbeId, result?.Result, result?.ResultClass, result?.Step, result?.UnknownsHolding, result?.FailsHolding }, Json));
                return code;
            }
            case "tuple-v35" when rest.Length == 2:
                return V35TupleCommand.Run(repo, rest[0], rest[1]);
            case "smoke-v35" when rest.Length is 3 or 4:
                return await V35SmokeCommand.RunAsync(Acad, rest[1], rest[2], rest[0], rest.Length == 4 ? rest[3] : null);
            case "result-v35" when rest.Length == 2:
            {
                // Offline classification of a recorded log without process evidence (always evidence-incomplete).
                V35Authority authority = V35Authority.Load(repo);
                if (!authority.ByProbe.TryGetValue(rest[0], out V35RowPlan? plan)) { Console.Error.WriteLine($"Unknown ProbeId '{rest[0]}': rejected."); return 2; }
                V35Result result = new V35ResultEngine(authority).Evaluate(plan, V35RunEvidence.Load(rest[1], null, null));
                Console.WriteLine(JsonSerializer.Serialize(result, Json));
                return 0;
            }
        }
        return await Legacy(command, repo, rest);
    }

    private static Task<int> Legacy(string command, string repo, string[] rest)
    {
        switch (command)
        {
            case "freeze-v35":
            {
                V35FreezeVerification result = V35Freeze.Verify(repo);
                Console.WriteLine(JsonSerializer.Serialize(result, Json));
                return Task.FromResult(result.Holds ? 0 : 1);
            }
            case "generate-v35":
            {
                V35Authority authority = V35Authority.Load(repo);
                V35PlanCompiler.Write(repo, V35PlanCompiler.Generate(authority, repo));
                Console.WriteLine($"generated {authority.Rows.Count} plans");
                return Task.FromResult(0);
            }
            case "check-generated-v35":
            {
                V35Authority authority = V35Authority.Load(repo);
                IReadOnlyList<string> drift = V35PlanCompiler.Drift(repo, V35PlanCompiler.Generate(authority, repo));
                Console.WriteLine(JsonSerializer.Serialize(new { plans = authority.Rows.Count, drift }, Json));
                return Task.FromResult(drift.Count == 0 ? 0 : 1);
            }
            case "plan" when rest.Length == 1:
            {
                V35Authority authority = V35Authority.Load(repo);
                if (!authority.ByProbe.TryGetValue(rest[0], out V35RowPlan? plan)) { Console.Error.WriteLine($"Unknown ProbeId '{rest[0]}': rejected."); return Task.FromResult(2); }
                Console.WriteLine(JsonSerializer.Serialize(new { plan, derivedPlan = V35PlanCompiler.DerivedPlan(authority, plan) }, Json));
                return Task.FromResult(0);
            }
            default:
                return Task.FromResult(2);
        }
    }
}
