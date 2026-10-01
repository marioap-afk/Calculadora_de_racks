using System.Reflection.Metadata;
using I52Ct21d.HostFacts.Tools.Scan;

namespace I52Ct21d.HostFacts.Rs.Tools;

public sealed class RsScanInput
{
    public ScanRole Role { get; }
    public string Path { get; }

    public RsScanInput(ScanRole role, string path)
    {
        Role = role;
        Path = path;
    }
}

public sealed class RsScanAssembly
{
    public string Name { get; }
    public IReadOnlyList<Violation> Violations { get; }
    public int WaivedFindings { get; }
    public int BodiesExamined { get; }
    public int PrivilegedCallSites { get; }
    public int ImportRowsChecked { get; }
    public RsLayerObservations Observed { get; }

    public RsScanAssembly(string name, IReadOnlyList<Violation> violations, int waivedFindings, int bodiesExamined, int privilegedCallSites, int importRowsChecked, RsLayerObservations observed)
    {
        Name = name;
        Violations = violations;
        WaivedFindings = waivedFindings;
        BodiesExamined = bodiesExamined;
        PrivilegedCallSites = privilegedCallSites;
        ImportRowsChecked = importRowsChecked;
        Observed = observed;
    }
}

public sealed class RsScanResult
{
    public IReadOnlyList<RsScanAssembly> Assemblies { get; }
    public IReadOnlyList<Violation> ListProblems { get; }

    public bool Clean => ListProblems.Count == 0 && Assemblies.All(a => a.Violations.Count == 0);

    public RsScanResult(IReadOnlyList<RsScanAssembly> assemblies, IReadOnlyList<Violation> listProblems)
    {
        Assemblies = assemblies;
        ListProblems = listProblems;
    }
}

/// <summary>
/// The RS scan: the R0 scan (<see cref="ForbiddenApiScan"/>, deny-by-default imports + deny rules) configured for the RS assemblies, then the scoped
/// waivers of <c>rs-waivers.txt</c>, then the RS layer (<see cref="RsLayer"/>), then the stale-entry checks of the checked-in lists. The R0 scan is not
/// edited: the only RS-specific configuration is the side-database type and assembly (<c>SideDbWriter</c> of the RS DLL).
/// </summary>
public static class RsScan
{
    public static ScanConfig ConfigFor(ScanRole role, ApiAllowlist allowlist, ScanPolicy policy) =>
        new ScanConfig(role, "I52Ct21d.HostFacts.Core.EvidenceWriter", RsLayer.SideDbWriterType, "I52Ct21d.HostFacts.Core", RsLayer.RsAssembly)
        {
            Allowlist = allowlist,
            Policy = policy,
        };

    public static RsScanAssembly ScanOne(byte[] image, string displayName, ScanRole role, ApiAllowlist allowlist, RsWaivers waivers, ScanPolicy policy)
    {
        var outcome = ForbiddenApiScan.ScanDetailed(image, displayName, ConfigFor(role, allowlist, policy));
        string assemblyName;
        using (var pe = new System.Reflection.PortableExecutable.PEReader(System.Collections.Immutable.ImmutableArray.Create(image)))
        {
            var metadata = pe.GetMetadataReader();
            assemblyName = metadata.GetString(metadata.GetAssemblyDefinition().Name);
        }
        var kept = new List<Violation>();
        var waived = 0;
        foreach (var v in outcome.Violations)
        {
            if (waivers.Find(assemblyName, v) is not null) waived++;
            else kept.Add(v);
        }
        var layer = RsLayer.Check(image, displayName, policy);
        kept.AddRange(layer.Violations);
        var ordered = kept.OrderBy(v => v.Type, StringComparer.Ordinal).ThenBy(v => v.Method, StringComparer.Ordinal).ThenBy(v => v.IlOffset)
            .ThenBy(v => v.Rule, StringComparer.Ordinal).ToList();
        return new RsScanAssembly(displayName, ordered, waived, layer.BodiesExamined, layer.PrivilegedCallSites, outcome.ImportRowsChecked, layer.Observed);
    }

    public static RsScanResult Run(IReadOnlyList<RsScanInput> inputs, ApiAllowlist allowlist, RsWaivers waivers, ScanPolicy policy)
    {
        var results = new List<RsScanAssembly>();
        foreach (var input in inputs)
            results.Add(ScanOne(File.ReadAllBytes(input.Path), System.IO.Path.GetFileName(input.Path), input.Role, allowlist, waivers, policy));

        // stale entries are reported only when the three assemblies were scanned (otherwise the others' entries look unused)
        var stale = new List<Violation>();
        var roles = new HashSet<ScanRole>(inputs.Select(i => i.Role));
        if (roles.Count == 3)
        {
            foreach (var e in waivers.Unused())
                stale.Add(new Violation("rs-waivers.txt", "<list>", "", -1, "R-RS-WAIVER-UNUSED", "line " + e.Line + ": " + e.Key + " waives nothing"));
            foreach (var e in policy.UnusedCallers())
                stale.Add(new Violation("privileged-callers-rs.txt", "<list>", "", -1, "R-PRIVILEGED-CALLERS-UNUSED", "line " + e.Line + ": " + e.Key + " is listed but no scanned assembly makes that call"));
            foreach (var e in policy.UnusedCommands())
                stale.Add(new Violation("expected-commands-rs.txt", "<list>", "", -1, "R-COMMAND-UNUSED", "line " + e.Line + ": " + e.Key + " is listed but no scanned assembly registers it"));
            foreach (var e in allowlist.Unused())
                stale.Add(new Violation("allowed-apis-rs.txt", "<list>", "", -1, "R-ALLOWLIST-UNUSED", "line " + e.Line + ": " + e.Key + " is approved but no scanned assembly uses it"));
        }
        return new RsScanResult(results, stale);
    }

    public static string ToText(RsScanResult result)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var a in result.Assemblies)
        {
            sb.Append(a.Name).Append(": ").Append(a.Violations.Count == 0 ? "CLEAN" : a.Violations.Count + " violation(s)")
              .Append(" (import rows ").Append(a.ImportRowsChecked).Append(", waived findings ").Append(a.WaivedFindings)
              .Append(", bodies ").Append(a.BodiesExamined).Append(", privileged call sites ").Append(a.PrivilegedCallSites).Append(")\n");
            foreach (var v in a.Violations) sb.Append("  ").Append(v).Append('\n');
        }
        sb.Append("lists: ").Append(result.ListProblems.Count == 0 ? "CLEAN" : result.ListProblems.Count + " problem(s)").Append('\n');
        foreach (var v in result.ListProblems) sb.Append("  ").Append(v).Append('\n');
        return sb.ToString();
    }
}
