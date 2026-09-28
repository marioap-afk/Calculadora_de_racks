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
        // FIN-GATE-01 deadlines (300 s external, 120 s after the exit record) and the D-3 interactive-state guard.
        V35ProcessRun run = await V35ProcessRunner.RunAsync(launch.AcadExecutable, launch.Arguments, launch.OutputRoot, launch.Environment, launch.EventLog,
            TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(120));
        var scratch = new ScratchDrawingIntegrity(before, ScratchDrawingState.Capture(launch.ScratchDrawing));
        // Process-run evidence is persisted before the log is read; the log and report reads are retry-safe.
        V35SmokeVerdict verdict = await V35SmokeCompletion.CompleteAsync(launch, run, scratch);
        Console.WriteLine(JsonSerializer.Serialize(verdict));
        return verdict.Result == "PASS" ? 0 : 1;
    }
}
