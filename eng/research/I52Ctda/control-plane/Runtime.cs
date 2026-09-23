using System.Diagnostics;
using System.Security.Cryptography;

namespace I52Ctda.ControlPlane;

public enum CleanupObligation
{
    TAppInactive,
    TCmdInactive,
    DocumentUnlocked,
    ReactorsRemoved,
    SendDrained,
    AppCtxDrained,
    CmdCtxDrained,
    ReentrancyGuardsReset,
    ScratchProcessExited,
    FixtureAccounted
}

public sealed class CleanupStateMachine
{
    private readonly Dictionary<CleanupObligation, bool> obligations = Enum.GetValues<CleanupObligation>().ToDictionary(x => x, _ => false);
    public void Satisfy(CleanupObligation obligation) => obligations[obligation] = true;
    public CleanupRecord Snapshot() => new(obligations.ToDictionary(k => k.Key.ToString(), v => v.Value), obligations.Values.All(v => v));
}

public sealed record ChildProcessResult(ProcessIdentity Identity, int ExitCode, bool TimedOut, string StandardOutput, string StandardError);

public static class ScratchProcessController
{
    public static async Task<ChildProcessResult> RunAsync(string executable, string arguments, string workingDirectory, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var start = new ProcessStartInfo(executable, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };
        using var process = new Process { StartInfo = start };
        DateTimeOffset started = DateTimeOffset.UtcNow;
        process.Start();
        var identity = new ProcessIdentity(process.Id, started, Sha256(executable), $"{executable} {arguments}");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        bool timedOut = false;
        try { await process.WaitForExitAsync(timeoutCts.Token); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
        }
        return new(identity, process.ExitCode, timedOut, await stdout, await stderr);
    }

    public static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}

public static class NativeSourceGenerator
{
    public static void WriteDispatchTable(ContractCatalog catalog, string path)
    {
        catalog.AssertComplete();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(false));
        writer.WriteLine("// Generated from NPM-V34. One concrete entry per ProbeId; no wildcard dispatch.");
        foreach (ImplementationDescriptor item in catalog.Implementations.Values.OrderBy(i => i.ProbeId, StringComparer.Ordinal))
            writer.WriteLine($"I52_PROBE(\"{item.ProbeId}\", {item.HandlerId}, \"{item.PrimaryAuthorityId}\", \"{item.ScheduleOriginEventId}\")");
    }
}
