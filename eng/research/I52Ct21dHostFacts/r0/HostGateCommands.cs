using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using I52Ct21d.HostFacts.Core;

[assembly: CommandClass(typeof(I52Ct21d.HostFacts.R0.HostGateCommands))]

namespace I52Ct21d.HostFacts.R0;

/// <summary>
/// The three R0 commands (design 5.2). They are NOT loaded and NOT run by anything in this repository: loading is a declared event of
/// a future, authorized host gate (decisions section 239 condition 2). They are ordinary (non-Session) commands, so the host holds the
/// document lock; they never take a lock, never write a system variable, never open a document and register no event or overrule.
///
/// Inputs: <c>run-designation.json</c> next to the DLL (declared by the CAD manager) and the pin sidecar of the DLL.
/// Output: new files under the evidence folder, only through the <see cref="EvidenceWriter"/>, plus a one-line status on the command
/// line (decisions section 239, R-I: a one-line status is allowed).
/// </summary>
public sealed class HostGateCommands
{
    private const string InstrumentName = "I52Ct21d.HostFacts.R0";

    /// <summary>I-2: HF-C1, HF-C3, HF-G1 (M1) from a private copy of the library file.</summary>
    [CommandMethod("CT21DHG_CENSUS_R")]
    public void CensusR() => Execute("CT21DHG_CENSUS_R",
        (d, inst, env, writer) => Runners.RunCensus(d, inst, env, new AcadSideDbReader(), new AcadSystemVariables(), writer));

    /// <summary>I-2, separate command and record: HF-T2, the scale components of the library's references.</summary>
    [CommandMethod("CT21DHG_INVENTORY")]
    public void Inventory() => Execute("CT21DHG_INVENTORY",
        (d, inst, env, writer) => Runners.RunInventory(d, inst, env, new AcadSideDbReader(), new AcadSystemVariables(), writer));

    /// <summary>I-4: HF-G3, the host type of the context-variable reads. No side database.</summary>
    [CommandMethod("CT21DHG_CTXVARS")]
    public void CtxVars() => Execute("CT21DHG_CTXVARS",
        (d, inst, env, writer) => Runners.RunCtxVars(d, inst, env, new AcadSystemVariables(), writer));

    private static void Execute(string command, Func<RunDesignation, InstrumentInfo, IRunEnvironment, EvidenceWriter, RunOutcome> run)
    {
        string status;
        try
        {
            // HOST-TO-CONFIRM: Assembly.Location is a real file path for an assembly loaded by NETLOAD in AutoCAD 2025.
            var dll = typeof(HostGateCommands).Assembly.Location;
            var instrument = SelfPin.Verify(InstrumentName, dll);
            var designationPath = Path.Combine(Path.GetDirectoryName(dll)!, RunDesignation.FileName);
            var designation = RunDesignation.Parse(File.ReadAllText(designationPath));
            var writer = new EvidenceWriter(designation.EvidenceFolder);
            var outcome = run(designation, instrument, new HostEnvironment(), writer);
            status = command + ": " + string.Join(", ", outcome.FactStatuses.Select(p => p.Key + "=" + p.Value)) +
                     (outcome.InvalidReasons.Count > 0 ? " [INVALID: " + string.Join(";", outcome.InvalidReasons) + "]" : "");
        }
        catch (System.Exception ex)
        {
            status = command + ": NOT_RECORDED (" + ex.GetType().Name + ": " + ex.Message + ")";
        }
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n" + status + "\n");
    }
}
