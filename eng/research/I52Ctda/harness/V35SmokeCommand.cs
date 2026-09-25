using System.Text;
using System.Text.Json;
using I52Ctda.ControlPlane;
using I52Ctda.ControlPlane.V35;

// smoke-v35 <repo> <out> <I52CtdaNative.arx> <scratch.dwg> [profile]: the zero-ProbeId R3 host smoke (next gate).
internal static class V35SmokeCommand
{
    public static async Task<int> RunAsync(string acad, string nativeHelper, string scratchDrawing, string outputRoot, string? profile)
    {
        V35SmokeLaunch launch;
        try { launch = V35SmokeLaunch.Create(acad, nativeHelper, scratchDrawing, outputRoot, profile); }
        catch (FileNotFoundException e) { Console.Error.WriteLine(e.Message); return 2; }
        Directory.CreateDirectory(launch.OutputRoot);
        if (File.Exists(launch.ReportPath) || File.Exists(launch.EventLog)) { Console.Error.WriteLine("Use a fresh output directory."); return 2; }
        ScratchDrawingState before = ScratchDrawingState.Capture(launch.ScratchDrawing);
        if (before.BackupExists) { Console.Error.WriteLine("A fresh scratch DWG is required."); return 2; }
        await File.WriteAllTextAsync(launch.ScriptPath, launch.Script, new UTF8Encoding(false));
        SmokeProcessRun run = await SmokeProcessRunner.RunAsync(launch.AcadExecutable, launch.Arguments, launch.OutputRoot, launch.Environment, TimeSpan.FromMinutes(5));
        var scratch = new ScratchDrawingIntegrity(before, ScratchDrawingState.Capture(launch.ScratchDrawing));
        JsonElement? report = File.Exists(launch.ReportPath) ? JsonDocument.Parse(await File.ReadAllTextAsync(launch.ReportPath)).RootElement.Clone() : null;
        var records = V35RunEvidence.Load(launch.EventLog, null, null).Records;
        V35SmokeVerdict verdict = V35SmokeEvaluator.Evaluate(report, records, run.ProcessId, run.ProcessGone, run.TimedOut, scratch);
        var result = new
        {
            schemaVersion = 1, stage = "R3_SMOKE", verdict.Result, verdict.Failures, run,
            modules = new { native = V35Files.Sha256(launch.NativeHelper), payload = V35Files.Sha256(launch.PayloadArx), managed = V35Files.Sha256(launch.ManagedObserver) },
            scratch = new { scratch.Before, scratch.After, scratch.Unchanged, scratch.BackupCreated }, records = records.Count, governedProbesExecuted = 0,
        };
        await File.WriteAllTextAsync(Path.Combine(launch.OutputRoot, "r3-smoke-result.json"), JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
        Console.WriteLine(JsonSerializer.Serialize(verdict));
        return verdict.Result == "PASS" ? 0 : 1;
    }
}
