using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;

[assembly: CommandClass(typeof(I52Ct21d.HostFacts.Rs.RsGateCommands))]

namespace I52Ct21d.HostFacts.Rs;

/// <summary>
/// The three RS commands (design 5.2: I-3, I-5, I-6). They are NOT loaded and NOT run by anything in this repository: loading is a declared event of
/// a future, authorized host gate. They are ordinary (non-Session) commands: no lock on the active document is taken (none is needed, no document
/// database is touched), no system variable is written, no document is opened, no event or overrule is registered. The side databases are
/// disposable and are created by <see cref="SideDbWriter"/> only.
///
/// ONE execution per command per session: a second invocation in the same process is refused before any side effect (no automatic retry;
/// PARAM-04 = 3 is a ceiling of capacity per slot, not a permission to repeat). Inputs: <c>run-designation-rs.json</c> next to the DLL (declared by
/// the CAD manager) and the pin sidecars of the DLLs. Output: new files under the evidence root through the <see cref="EvidenceWriter"/>, one new
/// scratch DWG through the <see cref="ScratchSaver"/> (U-RS-2 and U-RS-3), and a one-line status on the command line.
/// </summary>
public sealed class RsGateCommands
{
    private const string InstrumentName = "I52Ct21d.HostFacts.Rs";

    private static readonly HashSet<string> ExecutedInThisSession = new(StringComparer.Ordinal);

    /// <summary>U-RS-1 / I-3: HF-C2, the dynamic properties of the library's dynamic blocks, read from references created in a scratch side database.</summary>
    [CommandMethod("CT21DHG_CENSUS_DYN")]
    public void CensusDyn() => Execute(CensusDynName);

    /// <summary>U-RS-2 / I-5: HF-C4 and HF-C5, the dimension write-back probe in a scratch side database (with the OP2 save of that side database).</summary>
    [CommandMethod("CT21DHG_DIMWRITEBACK")]
    public void DimWriteBack() => Execute(DimWriteBackName);

    /// <summary>U-RS-3 / I-6: HF-T1, the TOL_SCALE probe of design 2.4 (147 rows, OP1 and OP2), single execution.</summary>
    [CommandMethod("CT21DHG_TOLSCALE")]
    public void TolScale() => Execute(TolScaleName);

    private const string CensusDynName = "CT21DHG_CENSUS_DYN";
    private const string DimWriteBackName = "CT21DHG_DIMWRITEBACK";
    private const string TolScaleName = "CT21DHG_TOLSCALE";

    // The host factories are NAMED methods (not lambdas) so that the callers pinned by the scan have stable names.
    private static IDynCensusHost MakeCensusHost(ScratchRootGuard guard, SideDbLedger ledger) =>
        new DynCensusHostAdapter(new SideDbWriter(ledger, guard), new SideDbReader());

    private static IWriteBackHost MakeWriteBackHost(ScratchRootGuard guard, SideDbLedger ledger) =>
        new WriteBackHostAdapter(new SideDbWriter(ledger, guard), new SideDbReader(), new ScratchSaver(guard));

    private static ITolScaleHost MakeTolScaleHost(ScratchRootGuard guard, SideDbLedger ledger) =>
        new TolScaleHostAdapter(new SideDbWriter(ledger, guard), new SideDbReader(), new ScratchSaver(guard));

    private static string Dispatch(string command, RsDesignation d, InstrumentInfo instrument, EvidenceWriter writer)
    {
        var env = new RsEnvironment();
        var vars = new RsSystemVariables();
        switch (command)
        {
            case CensusDynName: return DynCensusRunner.Run(d, instrument, env, vars, MakeCensusHost, writer).Status;
            case DimWriteBackName: return WriteBackRunner.Run(d, instrument, env, vars, MakeWriteBackHost, writer).Status;
            case TolScaleName: return TolScaleRunner.Run(d, instrument, env, vars, MakeTolScaleHost, writer).Result;
            default: throw new ArgumentException("unknown RS command " + command);
        }
    }

    private static void Execute(string command)
    {
        string status;
        if (!ExecutedInThisSession.Add(command))
        {
            status = command + ": REFUSED (NO_AUTOMATIC_RETRY: already executed in this session; a new attempt needs the Coordinator's authorization)";
        }
        else
        {
            try
            {
                // HOST-TO-CONFIRM: Assembly.Location is a real file path for an assembly loaded by NETLOAD in AutoCAD 2025.
                var dll = typeof(RsGateCommands).Assembly.Location;
                var instrument = Pins(dll);
                RsDesignation designation;
                EvidenceWriter writer;
                try
                {
                    designation = RsDesignation.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(dll)!, RsDesignation.FileName)));
                    writer = new EvidenceWriter(designation.EvidenceRoot);
                }
                catch (System.Exception ex) when (ex is System.IO.IOException or FormatException or ArgumentException or System.Text.Json.JsonException)
                {
                    // the designation is missing or invalid: nothing was created, so this is a refusal and not an execution (the command may be typed again)
                    ExecutedInThisSession.Remove(command);
                    throw new RsRefusedException(new[] { "DESIGNATION_UNUSABLE:" + ex.GetType().Name + ":" + ex.Message });
                }
                status = command + ": " + Dispatch(command, designation, instrument, writer);
            }
            catch (RsRefusedException ex)
            {
                // a refusal happens BEFORE any side effect: it is not an execution, so the command may be typed again once the CAD manager repaired the input
                ExecutedInThisSession.Remove(command);
                status = command + ": REFUSED (" + string.Join(";", ex.Reasons) + ")";
            }
            catch (System.Exception ex)
            {
                status = command + ": NOT_RECORDED (" + ex.GetType().Name + ": " + ex.Message + ")";
            }
        }
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n" + status + "\n");
    }

    /// <summary>The self-pin of the RS DLL and of the two libraries it depends on (the R0 core and the RS core): the first pin that is not a MATCH is reported.</summary>
    private static InstrumentInfo Pins(string dll)
    {
        var own = SelfPin.Verify(InstrumentName, dll);
        if (own.SelfPinStatus != InstrumentInfo.PinMatch) return own;
        foreach (var dependency in new[] { typeof(EvidenceWriter).Assembly.Location, typeof(RsDesignation).Assembly.Location })
        {
            var pin = SelfPin.Verify(Path.GetFileNameWithoutExtension(dependency), dependency);
            if (pin.SelfPinStatus != InstrumentInfo.PinMatch) return new InstrumentInfo(own.Name, own.Sha256, "DEPENDENCY_" + pin.Name + "_" + pin.SelfPinStatus);
        }
        return own;
    }
}
