using System.Security.Cryptography;
using System.Text;

namespace I52Ct21d.HostFacts.Tools.Scan;

/// <summary>One line of privileged-callers.txt: a caller method that may call a method of a privileged Core type.</summary>
public sealed class CallerEntry
{
    public string Caller { get; }

    public string Callee { get; }

    public string Justification { get; }

    public int Line { get; }

    public CallerEntry(string caller, string callee, string justification, int line)
    {
        Caller = caller;
        Callee = callee;
        Justification = justification;
        Line = line;
    }

    public string Key => Caller + "|" + Callee;
}

/// <summary>One line of expected-commands.txt: an AutoCAD command registration the R0 assembly is expected to carry.</summary>
public sealed class CommandEntry
{
    public string Kind { get; }

    public string Owner { get; }

    public string Args { get; }

    public string Justification { get; }

    public int Line { get; }

    public CommandEntry(string kind, string owner, string args, string justification, int line)
    {
        Kind = kind;
        Owner = owner;
        Args = args;
        Justification = justification;
        Line = line;
    }

    public string Key => Kind + "|" + Owner + "|" + Args;
}

/// <summary>
/// The three checked-in lists that pin the PRIVILEGED surface of the scan (layer 3), beside <c>allowed-apis.txt</c>:
///
///  * <c>privileged-surface.txt</c>: the exact set of methods (name + signature + accessibility) of each privileged Core type
///    (<c>EvidenceWriter</c>, <c>EvidenceSealer</c>), nested types included, one per line, no justification (it is a pin, not an approval).
///  * <c>privileged-callers.txt</c>: <c>CALLER|CALLEE|WHY</c>, CALLER = <c>Assembly::Full.Type.Name::Method</c> (nested types with '/'),
///    CALLEE = <c>Full.Type.Name::Method</c> of a privileged top-level type. Every call, from ANY assembly including Core itself, to a
///    method of a privileged type that is not in this list is a violation (calls from the privileged type to itself are not listed).
///  * <c>expected-commands.txt</c>: <c>KIND|OWNER|ARGS|WHY</c>, KIND = CommandMethod or CommandClass, OWNER = <c>Assembly::Type::Method</c>
///    (or <c>Assembly::&lt;assembly&gt;</c>), ARGS = the decoded fixed arguments of the attribute joined by ','. A registration not in it is a
///    violation; one in it that the assemblies no longer carry is reported as stale when all three assemblies are scanned.
///
/// Format of every file: UTF-8, LF, blank lines and lines starting with '#' are ignored.
/// </summary>
public sealed class ScanPolicy
{
    public Dictionary<string, IReadOnlyList<string>> Surface { get; }

    public IReadOnlyList<CallerEntry> Callers { get; }

    public IReadOnlyList<CommandEntry> Commands { get; }

    public string SurfaceSha256 { get; }

    public string CallersSha256 { get; }

    public string CommandsSha256 { get; }

    /// <summary>Entries a scan matched. Mutable on purpose, like <see cref="ApiAllowlist.Used"/>.</summary>
    public HashSet<CallerEntry> UsedCallers { get; } = new();

    public HashSet<CommandEntry> UsedCommands { get; } = new();

    private readonly Dictionary<string, CallerEntry> _callers;
    private readonly Dictionary<string, CommandEntry> _commands;

    private ScanPolicy(Dictionary<string, IReadOnlyList<string>> surface, List<CallerEntry> callers, List<CommandEntry> commands, string s, string c, string e)
    {
        Surface = surface;
        Callers = callers;
        Commands = commands;
        SurfaceSha256 = s;
        CallersSha256 = c;
        CommandsSha256 = e;
        _callers = callers.ToDictionary(x => x.Key, StringComparer.Ordinal);
        _commands = commands.ToDictionary(x => x.Key, StringComparer.Ordinal);
    }

    public static ScanPolicy LoadFiles(string surfacePath, string callersPath, string commandsPath) =>
        Parse(File.ReadAllBytes(surfacePath), File.ReadAllBytes(callersPath), File.ReadAllBytes(commandsPath));

    public static ScanPolicy Parse(string surface, string callers, string commands)
    {
        var enc = new UTF8Encoding(false);
        return Parse(enc.GetBytes(surface), enc.GetBytes(callers), enc.GetBytes(commands));
    }

    public static ScanPolicy Parse(byte[] surfaceBytes, byte[] callersBytes, byte[] commandsBytes)
    {
        var surface = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var seenSurface = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (line, no) in Lines(surfaceBytes, "privileged-surface"))
        {
            var sep = line.IndexOf("::", StringComparison.Ordinal);
            if (sep <= 0 || sep + 2 >= line.Length || !line.Contains('(') || !line.Contains(")->"))
                throw new FormatException($"privileged-surface line {no}: expected Full.Type::Name(params)->ret [flags]");
            if (line.Contains('|')) throw new FormatException($"privileged-surface line {no}: '|' is not allowed");
            if (!seenSurface.Add(line)) throw new FormatException($"privileged-surface line {no}: duplicate entry");
            var top = line[..sep].Split('/')[0];
            if (!surface.TryGetValue(top, out var list)) surface[top] = list = new List<string>();
            list.Add(line);
        }

        var callers = new List<CallerEntry>();
        var seenCallers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (line, no) in Lines(callersBytes, "privileged-callers"))
        {
            var parts = line.Split('|');
            if (parts.Length != 3) throw new FormatException($"privileged-callers line {no}: expected CALLER|CALLEE|WHY");
            var caller = parts[0].Trim();
            var callee = parts[1].Trim();
            var why = parts[2].Trim();
            if (CountSeparators(caller) != 2) throw new FormatException($"privileged-callers line {no}: CALLER must be Assembly::Full.Type.Name::Method");
            if (CountSeparators(callee) != 1) throw new FormatException($"privileged-callers line {no}: CALLEE must be Full.Type.Name::Method");
            if (why.Length < 8) throw new FormatException($"privileged-callers line {no}: a justification of at least 8 characters is required");
            var entry = new CallerEntry(caller, callee, why, no);
            if (!seenCallers.Add(entry.Key)) throw new FormatException($"privileged-callers line {no}: duplicate entry");
            callers.Add(entry);
        }

        var commands = new List<CommandEntry>();
        var seenCommands = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (line, no) in Lines(commandsBytes, "expected-commands"))
        {
            var parts = line.Split('|');
            if (parts.Length != 4) throw new FormatException($"expected-commands line {no}: expected KIND|OWNER|ARGS|WHY");
            var kind = parts[0].Trim();
            if (kind is not ("CommandMethod" or "CommandClass")) throw new FormatException($"expected-commands line {no}: KIND must be CommandMethod or CommandClass");
            var why = parts[3].Trim();
            if (why.Length < 8) throw new FormatException($"expected-commands line {no}: a justification of at least 8 characters is required");
            var entry = new CommandEntry(kind, parts[1].Trim(), parts[2].Trim(), why, no);
            if (!seenCommands.Add(entry.Key)) throw new FormatException($"expected-commands line {no}: duplicate entry");
            commands.Add(entry);
        }

        return new ScanPolicy(
            surface.ToDictionary(p => p.Key, p => (IReadOnlyList<string>)p.Value.OrderBy(x => x, StringComparer.Ordinal).ToList(), StringComparer.Ordinal),
            callers, commands, Sha(surfaceBytes), Sha(callersBytes), Sha(commandsBytes));
    }

    private static string Sha(byte[] b) => Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();

    private static int CountSeparators(string s)
    {
        var n = 0;
        for (var i = s.IndexOf("::", StringComparison.Ordinal); i >= 0; i = s.IndexOf("::", i + 2, StringComparison.Ordinal)) n++;
        return n;
    }

    private static List<(string Line, int No)> Lines(byte[] bytes, string what)
    {
        var result = new List<(string Line, int No)>();
        var text = new UTF8Encoding(false, true).GetString(bytes);
        if (text.Contains('\r')) throw new FormatException(what + ": the file must use LF line endings");
        var no = 0;
        foreach (var raw in text.Split('\n'))
        {
            no++;
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            result.Add((line, no));
        }
        return result;
    }

    /// <summary>True (and marks the entry used) when <paramref name="caller"/> may call <paramref name="callee"/>.</summary>
    public bool AllowsCaller(string caller, string callee)
    {
        if (!_callers.TryGetValue(caller + "|" + callee, out var e)) return false;
        UsedCallers.Add(e);
        return true;
    }

    public bool ExpectsCommand(string kind, string owner, string args)
    {
        if (!_commands.TryGetValue(kind + "|" + owner + "|" + args, out var e)) return false;
        UsedCommands.Add(e);
        return true;
    }

    public IReadOnlyList<CallerEntry> UnusedCallers() => Callers.Where(c => !UsedCallers.Contains(c)).ToList();

    public IReadOnlyList<CommandEntry> UnusedCommands() => Commands.Where(c => !UsedCommands.Contains(c)).ToList();
}
