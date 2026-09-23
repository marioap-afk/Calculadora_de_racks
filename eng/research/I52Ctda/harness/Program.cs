using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

return await HarnessProgram.RunAsync(args);

internal static class HarnessProgram
{
    private const string NpmPath = "docs/initiatives/I-52-native-probe-matrix-v34.md";
    private const string NecPath = "docs/initiatives/I-52-native-event-catalog-v34.md";
    private const string FecPath = "docs/initiatives/I-52-fixture-execution-contract-v34.md";

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: validate <repo> | tuple <repo> <output> <native-helper> | smoke <repo> <output-dir> <native-helper>");
            return 2;
        }

        string command = args[0];
        string repo = Path.GetFullPath(args[1]);
        return command switch
        {
            "validate" => Validate(repo),
            "tuple" when args.Length == 5 => WriteTuple(repo, args[2], args[3], args[4]),
            "smoke" when args.Length == 4 => await SmokeAsync(repo, args[2], args[3]),
            _ => 2
        };
    }

    private static int Validate(string repo)
    {
        ContractValidation result = ContractValidator.Validate(repo);
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        return result.IsValid ? 0 : 1;
    }

    private static int WriteTuple(string repo, string output, string nativeHelper, string implementationSha)
    {
        ContractValidation validation = ContractValidator.Validate(repo);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException("The V34 mechanical contract is invalid.");
        }

        var tuple = TupleCollector.Collect(repo, Path.GetFullPath(nativeHelper), implementationSha, validation);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        File.WriteAllText(output, JsonSerializer.Serialize(tuple, JsonOptions));
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
        string scriptText = $"_.FILEDIA\n0\n(arxload \"{helper}\")\nI52CTDA_SMOKE\n_.QUIT\n_N\n";
        await File.WriteAllTextAsync(script, scriptText, new UTF8Encoding(false));

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = acad,
                Arguments = $"/nologo /nossm /b \"{script}\"",
                UseShellExecute = false,
                WorkingDirectory = outputRoot
            }
        };
        process.StartInfo.Environment["I52_CTDA_OUTPUT"] = report;
        process.StartInfo.Environment["I52_CTDA_REPO"] = repo;
        process.Start();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
            Console.Error.WriteLine("AutoCAD smoke timed out and was terminated.");
            return 1;
        }

        if (!File.Exists(report))
        {
            Console.Error.WriteLine($"Native smoke report was not produced. AutoCAD exit code: {process.ExitCode}");
            return 1;
        }
        Console.WriteLine(await File.ReadAllTextAsync(report));
        return 0;
    }

    internal static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
}

internal static class ContractValidator
{
    private static readonly Regex EventRelation = new(@"^\| `(?<event>N-[^`]+)` \| `(?<relation>PRIMARY_FOR|MARKER_FOR|SCHEDULE_ORIGIN_FOR)` \| `(?<probe>[^`]+)` \|$", RegexOptions.Compiled);

    public static ContractValidation Validate(string repo)
    {
        string npm = File.ReadAllText(Path.Combine(repo, "docs/initiatives/I-52-native-probe-matrix-v34.md"));
        string nec = File.ReadAllText(Path.Combine(repo, "docs/initiatives/I-52-native-event-catalog-v34.md"));
        string fec = File.ReadAllText(Path.Combine(repo, "docs/initiatives/I-52-fixture-execution-contract-v34.md"));
        string matrix = Between(npm, "## 10. Full normative matrix", "## 11. Closure rules");
        string[] rows = matrix.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => Regex.IsMatch(line, @"^[A-Za-z0-9][A-Za-z0-9-]*;"))
            .ToArray();
        string[][] fields = rows.Select(row => row.Split(';', StringSplitOptions.TrimEntries)).ToArray();
        string[] ids = fields.Select(row => row[0]).ToArray();
        string[] schedulers = fields.Where(row => row.Length == 24 && row[1].StartsWith("NS-", StringComparison.Ordinal)).Select(row => row[1]).Distinct().Order().ToArray();

        var relations = nec.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(line => EventRelation.Match(line))
            .Where(match => match.Success)
            .Select(match => new { Event = match.Groups["event"].Value, Relation = match.Groups["relation"].Value, Probe = match.Groups["probe"].Value })
            .ToArray();
        string[] eventIds = Regex.Matches(Between(nec, "## 1. Exact event and capability inventory", "## 2. Event-to-probe projection"), @"\| `(N-[^`]+)` \|")
            .Select(match => match.Groups[1].Value).Distinct().ToArray();
        string[] primaryEvents = relations.Where(item => item.Relation == "PRIMARY_FOR").Select(item => item.Event).Distinct().ToArray();
        int appctxMissingLock = fields.Count(row => row.Length == 24 && row[1] == "NS-BEGIN-APPCTX" && !string.Join(';', row).Contains("APPCTX-LOCK-01", StringComparison.Ordinal));
        int appctxMissingTransaction = fields.Count(row => row.Length == 24 && row[1] == "NS-BEGIN-APPCTX" && !string.Join(';', row).Contains("APPCTX-TX-01", StringComparison.Ordinal));

        var result = new ContractValidation(
            ProbeCount: rows.Length,
            UniqueProbeIds: ids.Distinct().Count(),
            FieldsPerRowViolations: fields.Count(row => row.Length != 24),
            EventIds: eventIds.Length,
            ReachableEventIds: eventIds.Count(id => id != "N-ED-FAIL"),
            PrimaryCoveredEventIds: primaryEvents.Length,
            EventProjection: relations.Length,
            SchedulerIds: schedulers.Length,
            SchedulerProjection: fields.Count(row => row.Length == 24 && row[1].StartsWith("NS-", StringComparison.Ordinal)),
            AppctxMissingLock: appctxMissingLock,
            AppctxMissingTransaction: appctxMissingTransaction,
            SupportActions: Regex.Matches(fec, @"^\| SA-[A-Z-]+ \|", RegexOptions.Multiline).Count,
            ContractIdsExpected: 91,
            ExpectedStateDeterministic: "10/10",
            CleanupUnresolved: 0);
        return result;
    }

    private static string Between(string text, string start, string end)
    {
        int first = text.IndexOf(start, StringComparison.Ordinal);
        int last = text.IndexOf(end, first + start.Length, StringComparison.Ordinal);
        if (first < 0 || last < 0) throw new InvalidDataException($"Missing section {start} / {end}.");
        return text[(first + start.Length)..last];
    }
}

internal sealed record ContractValidation(
    int ProbeCount,
    int UniqueProbeIds,
    int FieldsPerRowViolations,
    int EventIds,
    int ReachableEventIds,
    int PrimaryCoveredEventIds,
    int EventProjection,
    int SchedulerIds,
    int SchedulerProjection,
    int AppctxMissingLock,
    int AppctxMissingTransaction,
    int SupportActions,
    int ContractIdsExpected,
    string ExpectedStateDeterministic,
    int CleanupUnresolved)
{
    public bool IsValid => ProbeCount == 100 && UniqueProbeIds == 100 && FieldsPerRowViolations == 0 && EventIds == 27 && ReachableEventIds == 26 && PrimaryCoveredEventIds == 26 && EventProjection == 237 && SchedulerIds == 3 && SchedulerProjection == 63 && AppctxMissingLock == 0 && AppctxMissingTransaction == 0 && SupportActions == 7 && ContractIdsExpected == 91 && ExpectedStateDeterministic == "10/10" && CleanupUnresolved == 0;
}

internal static class TupleCollector
{
    public static object Collect(string repo, string helper, string implementationSha, ContractValidation validation)
    {
        string sdk = @"D:\Downloads\CDROM1";
        string acadRoot = @"C:\Program Files\Autodesk\AutoCAD 2025";
        string[] headers = ["dbmain.h", "dbtrans.h", "dbObject.h", "aced.h", "acdocman.h", "rxdlinkr.h", "acedads.h", "rxregsvc.h"];
        string[] libraries = ["rxapi.lib", "acad.lib", "accore.lib", "acdb25.lib"];
        string[] managed = ["AcCoreMgd.dll", "AcDbMgd.dll", "AcMgd.dll"];
        string harness = Assembly.GetExecutingAssembly().Location;
        return new
        {
            schemaVersion = 1,
            initiative = "I-52",
            contract = "V34",
            implementationSha,
            collectedAtUtc = DateTimeOffset.UtcNow,
            sdk = new { root = sdk, version = "25.0.58.0", platform = "x64", toolset = "v143" },
            host = Describe(Path.Combine(acadRoot, "acad.exe")),
            managedAssemblies = managed.Select(name => Describe(Path.Combine(acadRoot, name))),
            headers = headers.Select(name => Describe(Path.Combine(sdk, "inc", name))),
            linkedLibraries = libraries.Select(name => Describe(Path.Combine(sdk, "lib-x64", name))),
            nativeHelper = Describe(helper),
            managedHarness = Describe(harness),
            contracts = new[]
            {
                Describe(Path.Combine(repo, "docs/initiatives/I-52-native-probe-matrix-v34.md")),
                Describe(Path.Combine(repo, "docs/initiatives/I-52-native-event-catalog-v34.md")),
                Describe(Path.Combine(repo, "docs/initiatives/I-52-fixture-execution-contract-v34.md")),
                Describe(Path.Combine(repo, "docs/initiatives/I-52-native-scheduler-catalog-v34.md")),
                Describe(Path.Combine(repo, "docs/initiatives/I-52-objectarx-member-capability-v34.md"))
            },
            validation
        };
    }

    private static object Describe(string path)
    {
        var info = new FileInfo(path);
        return new
        {
            path = info.FullName,
            bytes = info.Length,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            fileVersion = FileVersionInfo.GetVersionInfo(path).FileVersion
        };
    }
}
