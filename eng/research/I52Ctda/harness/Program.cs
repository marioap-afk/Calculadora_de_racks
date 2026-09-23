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
        if (args.Length < 2) { Console.Error.WriteLine("Usage: validate|inventory|static-native|headers|generate|tuple|smoke <repo> [arguments]"); return 2; }
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
            "smoke" when args.Length == 4 => await SmokeAsync(repo, args[2], args[3]),
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

    private static async Task<int> SmokeAsync(string repo, string outputDirectory, string nativeHelper)
    {
        string outputRoot = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(outputRoot);
        string report = Path.Combine(outputRoot, "native-smoke.json");
        string script = Path.Combine(outputRoot, "native-smoke.scr");
        string acad = @"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe";
        string helper = Path.GetFullPath(nativeHelper).Replace('\\', '/');
        await File.WriteAllTextAsync(script, $"_.FILEDIA\n0\n(arxload \"{helper}\")\nI52CTDA_SMOKE\n_.QUIT\n_N\n", new UTF8Encoding(false));
        using var process = new Process { StartInfo = new ProcessStartInfo(acad, $"/nologo /nossm /b \"{script}\"") { UseShellExecute = false, WorkingDirectory = outputRoot } };
        process.StartInfo.Environment["I52_CTDA_OUTPUT"] = report;
        process.StartInfo.Environment["I52_CTDA_REPO"] = repo;
        process.Start();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            return 1;
        }
        return File.Exists(report) ? 0 : 1;
    }
}
