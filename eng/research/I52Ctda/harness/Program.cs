using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using I52Ctda.ControlPlane;

return await HarnessProgram.RunAsync(args);

internal static class HarnessProgram
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private static JsonSerializerOptions CreateJsonOptions() { var options = new JsonSerializerOptions { WriteIndented = true }; options.Converters.Add(new JsonStringEnumConverter()); return options; }

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: validate|inventory|static-native|headers|generate|tuple <repo> [arguments] | smoke <repo> <output-dir> <helper.arx> <scratch.dwg> [profile]"); return 2; }
        string command = args[0];
        string repo = Path.GetFullPath(args[1]);
        ContractCatalog catalog = ContractCatalog.Load(repo);
        return command switch
        {
            "validate" => Validate(catalog),
            "inventory" => Inventory(catalog),
            "static-native" => StaticNative(repo, catalog),
            "headers" => Headers(repo),
            "generate" when args.Length == 4 => Generate(catalog, repo, args[2], args[3]),
            "tuple" when args.Length == 4 => WriteTuple(repo, args[2], args[3]),
            "smoke" when args.Length is 5 or 6 => await SmokeAsync(repo, args[2], args[3], args[4], args.Length == 6 ? args[5] : null),
            _ => 2
        };
    }

    private static int Headers(string repo)
    {
        HeaderAuthorityValidation result = HeaderAuthorityValidator.Validate(repo, @"D:\Downloads\CDROM1");
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        return result.IsValid ? 0 : 1;
    }

    private static int StaticNative(string repo, ContractCatalog catalog)
    {
        StaticNativeValidation result = StaticNativeValidator.Validate(repo, catalog);
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        return result.IsValid ? 0 : 1;
    }

    private static int Validate(ContractCatalog catalog)
    {
        catalog.AssertComplete();
        FixtureSpecification.Validate();
        var result = new
        {
            ProbeCount = catalog.Probes.Count,
            UniqueProbeIds = catalog.Probes.Select(p => p.ProbeId).Distinct(StringComparer.Ordinal).Count(),
            ImplementationDescriptors = catalog.Implementations.Count,
            StubCount = catalog.Implementations.Values.Count(i => i.Status == ImplementationStatus.Stub),
            ReachableEventIds = catalog.ReachableEventIds.Count,
            PrimaryHandlers = catalog.Implementations.Values.Where(i => i.EventId is not null).Select(i => i.EventId).Distinct().Count(),
            SchedulerIds = catalog.Implementations.Values.Where(i => i.SchedulerId is not null).Select(i => i.SchedulerId).Distinct().Count(),
            FixtureObjects = FixtureSpecification.Objects.Count,
            IsValid = true
        };
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        return 0;
    }

    private static int Inventory(ContractCatalog catalog)
    {
        var result = new
        {
            contracted = catalog.Probes.Count,
            registered = catalog.Implementations.Count,
            readyForNativeLink = catalog.Implementations.Values.Count(i => i.Status == ImplementationStatus.ReadyForNativeLink),
            runtimeOnlyPending = catalog.Implementations.Values.Count(i => i.Status == ImplementationStatus.RuntimeOnlyPending),
            stubs = catalog.Implementations.Values.Count(i => i.Status == ImplementationStatus.Stub),
            callbackHandlers = catalog.ReachableEventIds.Count,
            schedulers = catalog.Implementations.Values.Where(i => i.SchedulerId is not null).Select(i => i.SchedulerId).Distinct().Count(),
            supportActions = 7,
            fixture = "FIXTURE_SPEC_READY / RUNTIME_MATERIALIZATION_PENDING"
        };
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        return 0;
    }

    private static int Generate(ContractCatalog catalog, string repo, string traceOutput, string nativeOutput)
    {
        catalog.AssertComplete();
        string trace = Path.GetFullPath(Path.Combine(repo, traceOutput));
        Directory.CreateDirectory(Path.GetDirectoryName(trace)!);
        File.WriteAllText(trace, JsonSerializer.Serialize(catalog.Implementations.Values.OrderBy(i => i.ProbeId), JsonOptions), new UTF8Encoding(false));
        NativeSourceGenerator.WriteDispatchTable(catalog, Path.GetFullPath(Path.Combine(repo, nativeOutput)));
        return 0;
    }

    private static int WriteTuple(string repo, string output, string implementationSha)
    {
        TupleIdentity tuple = ExactTupleCollector.Collect(repo, null, implementationSha);
        string target = Path.GetFullPath(output);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllText(target, JsonSerializer.Serialize(tuple, JsonOptions), new UTF8Encoding(false));
        return 0;
    }

    // Smoke only: one dedicated AutoCAD process on an explicit scratch DWG; no governed ProbeId is dispatched.
    // The event log is left to the helper's derivation rule so the fallback itself is exercised.
    private static async Task<int> SmokeAsync(string repo, string outputDirectory, string nativeHelper, string scratchDrawing, string? profile)
    {
        const string acad = @"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe";
        SmokeLaunchPlan plan;
        try { plan = SmokeLaunchPlan.Create(acad, nativeHelper, scratchDrawing, outputDirectory, profile); }
        catch (Exception e) when (e is ArgumentException or FileNotFoundException) { Console.Error.WriteLine(e.Message); return 2; }
        Directory.CreateDirectory(plan.OutputRoot);
        if (File.Exists(plan.ReportPath) || File.Exists(plan.EventLogPath)) { Console.Error.WriteLine("Smoke output directory already holds a report or event log."); return 2; }
        await File.WriteAllTextAsync(plan.ScriptPath, plan.Script, new UTF8Encoding(false));

        ScratchDrawingState drawingBefore = ScratchDrawingState.Capture(plan.ScratchDrawing);
        if (drawingBefore.BackupExists) { Console.Error.WriteLine("A fresh scratch DWG is required: its .bak sibling already exists."); return 2; }
        int[] acadBefore = Process.GetProcessesByName("acad").Select(p => p.Id).ToArray();
        var environment = new Dictionary<string, string?>
        {
            ["I52_CTDA_OUTPUT"] = plan.ReportPath,
            ["I52_CTDA_REPO"] = repo,
            ["I52_CTDA_EVENT_LOG"] = null
        };
        SmokeProcessRun run = await SmokeProcessRunner.RunAsync(plan.AcadExecutable, plan.Arguments, plan.OutputRoot, environment, TimeSpan.FromMinutes(5));

        NativeSmokeReport? report = File.Exists(plan.ReportPath) ? NativeSmokeReport.Parse(await File.ReadAllTextAsync(plan.ReportPath)) : null;
        EventLogSummary? log = File.Exists(plan.EventLogPath) ? EventLogSummary.Read(await File.ReadAllLinesAsync(plan.EventLogPath)) : null;
        // Recomputed only after the exact PID is gone, so AutoCAD can no longer write the scratch DWG or its .bak.
        var scratch = new ScratchDrawingIntegrity(drawingBefore, ScratchDrawingState.Capture(plan.ScratchDrawing));
        SmokeVerdict verdict = SmokeEvaluator.Evaluate(report, log, run, scratch);
        var result = new
        {
            schemaVersion = 1,
            commandIdentity = SmokeContract.CommandIdentity,
            result = verdict.Result,
            failures = verdict.Failures,
            process = new { run.ProcessId, run.StartedAtUtc, run.ExitedAtUtc, run.ExitCode, run.TimedOut, run.ProcessGone, run.Executable, executableSha256 = ScratchProcessController.Sha256(plan.AcadExecutable), run.Arguments, preexistingAcadProcessIds = acadBefore, newProcess = !acadBefore.Contains(run.ProcessId) },
            nativeHelper = new { path = plan.NativeHelper, sha256 = ScratchProcessController.Sha256(plan.NativeHelper) },
            scratchDrawing = new { before = scratch.Before, after = scratch.After, unchanged = scratch.Unchanged, backupCreated = scratch.BackupCreated },
            report = plan.ReportPath,
            eventLog = plan.EventLogPath,
            eventLogSummary = log,
            fixtureSnapshotSha256 = report?.SnapshotSha256,
            nativeReport = report,
            governedProbesExecuted = report?.GovernedProbesDispatched
        };
        await File.WriteAllTextAsync(Path.Combine(plan.OutputRoot, "smoke-result.json"), JsonSerializer.Serialize(result, JsonOptions), new UTF8Encoding(false));
        Console.WriteLine(JsonSerializer.Serialize(new { verdict.Result, verdict.Failures }, JsonOptions));
        return verdict.Pass ? 0 : 1;
    }
}
