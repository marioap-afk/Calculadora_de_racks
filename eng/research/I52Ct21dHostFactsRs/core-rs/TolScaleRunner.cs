using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

public sealed class TolScaleOutcome
{
    public string Result { get; }
    public IReadOnlyList<string> Reasons { get; }
    public IReadOnlyList<WrittenFile> Written { get; }
    public JsonObject Record { get; }
    public JsonObject RunRecord { get; }

    public TolScaleOutcome(string result, IReadOnlyList<string> reasons, IReadOnlyList<WrittenFile> written, JsonObject record, JsonObject runRecord)
    {
        Result = result;
        Reasons = reasons;
        Written = written;
        Record = record;
        RunRecord = runRecord;
    }
}

/// <summary>
/// U-RS-3 / I-6: the TOL_SCALE probe of design 2.4 as host-independent orchestration. The host is reached only through
/// <see cref="ITolScaleHost"/>; the only writes are the side database (inside the host adapter), the single OP2 scratch save (authorized by the
/// <see cref="ScratchRootGuard"/>) and the three result files through the <see cref="EvidenceWriter"/>. ONE execution: nothing here retries, and the
/// preflight refuses a run whose outputs already exist. Steps S0..S8 of the design: S0 snapshots, S2 side database and target, S3 construct and
/// commit (T1), S4 read (OP1), S5 witness (inside the row analysis), S6 save, reopen, read (OP2), S8 snapshots and checks.
/// </summary>
public static class TolScaleRunner
{
    public const string Command = "CT21DHG_TOLSCALE";
    public const string RawFile = "tolscale-raw.json";
    public const string ProbeTableFile = "tolscale-probe-table.json";
    public const string RunFile = "rs-run-tolscale.json";
    public const string Op2ScratchName = "CT21D_TOLSCALE_OP2.dwg";

    public static IReadOnlyList<string> OutputNames { get; } = new[] { RawFile, ProbeTableFile, RunFile };

    public static TolScaleOutcome Run(
        RsDesignation d,
        InstrumentInfo instrument,
        IRunEnvironment env,
        ISystemVariables vars,
        Func<ScratchRootGuard, SideDbLedger, ITolScaleHost> hostFactory,
        EvidenceWriter writer)
    {
        // ---- S0 (refusals happen BEFORE any side effect) -------------------------------------------------------------------
        var refusal = new List<string>();
        if (!d.IsTolscaleRunId) refusal.Add("RUN_ID_IS_NOT_AN_H4_RUN");
        var pre = RsPreflight.Check(d, instrument, OutputNames);
        refusal.AddRange(pre.Problems);
        if (!PathRegions.Same(writer.Root, d.EvidenceRoot)) refusal.Add("WRITER_ROOT_IS_NOT_THE_DESIGNATED_EVIDENCE_ROOT");
        if (refusal.Count > 0) throw new RsRefusedException(refusal);

        var guard = new ScratchRootGuard(d.ScratchRoot, d.PrivateCopyPath);
        var sideDbs = new SideDbLedger();
        var invalid = new List<string>();
        var before = SysSnapshot.Take(vars);
        var privateBefore = Sha256Hex.OfFile(d.PrivateCopyPath);
        var libraryBefore = Sha256Hex.OfFile(d.LibraryPath);

        var rows = ProbeTable.Generate();
        IReadOnlyList<RowConstruction> constructions = Array.Empty<RowConstruction>();
        IReadOnlyList<ReferenceReading> op1 = Array.Empty<ReferenceReading>();
        IReadOnlyList<ReferenceReading> op2 = Array.Empty<ReferenceReading>();

        try
        {
            var host = hostFactory(guard, sideDbs);
            using var db = host.CreateSideDatabase();                                  // S2
            constructions = db.AppendReferences(rows);                                   // S3 (T1, committed)
            op1 = db.ReadReferences();                                                   // S4 (T2, OP1)
            var target = guard.AuthorizeNew(Op2ScratchName, "TOLSCALE_OP2");              // S6
            var saved = db.SaveToScratch(target);
            var source = guard.AuthorizeReadBack(saved);
            using var reopened = db.Reopen(source);
            op2 = reopened.ReadReferences();                                             // S6 (T3, OP2)
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            invalid.Add("INSTRUMENT_ERROR:" + ex.GetType().Name + ":" + ex.Message);
        }

        // ---- S8 ------------------------------------------------------------------------------------------------------------
        var after = SysSnapshot.Take(vars);
        invalid.AddRange(SysSnapshot.Differences(before, after));
        var dbmodBefore = SysSnapshot.Dbmod(before);
        var dbmodAfter = SysSnapshot.Dbmod(after);
        if (dbmodBefore is null || dbmodAfter is null) invalid.Add("DBMOD_UNREADABLE");
        var privateAfter = Sha256Hex.OfFile(d.PrivateCopyPath);
        var libraryAfter = Sha256Hex.OfFile(d.LibraryPath);
        var filesUnchanged = privateBefore == privateAfter && libraryBefore == libraryAfter && privateBefore == d.PrivateCopySha256 && libraryBefore == d.LibraryFileSha256;
        if (!filesUnchanged) invalid.Add("PRIVATE_FILES_CHANGED");
        if (!sideDbs.AllDisposed) invalid.Add("SIDE_DATABASE_NOT_DISPOSED");
        var declared = new List<ScratchFileEntry>(pre.PriorScratch);
        declared.AddRange(guard.Ledger);
        var verification = ScratchVerifier.Verify(d.ScratchRoot, declared);
        if (!verification.Ok)
            invalid.Add("SCRATCH_VERIFICATION_FAILED:undeclared=" + verification.Undeclared.Count + ",missing=" + verification.Missing.Count +
                        ",hashMismatches=" + verification.HashMismatches.Count + ",reparsePoints=" + verification.ReparsePoints.Count);

        try
        {
            var results = TolScaleAnalyzer.AnalyzeRows(rows, constructions, op1, op2);
            var stats = TolScaleAnalyzer.Statistics(results);
            var table = TolScaleProbeTableFile.Build(rows);
            var checks = new TolScaleChecks(dbmodBefore ?? -1, dbmodAfter ?? -1, filesUnchanged, d.HostChecks.OtherAcadProcess, !d.HostChecks.LoadRouteScripted);

            TolScaleClassification Classify()
            {
                var facts = new TolScaleFacts(invalid, results.Count(r => r.RejectedByHost), results.Count(r => r.ContradictsDesign),
                    results.Count(r => !r.Observed), stats.Complete);
                return TolScaleClassifier.Classify(facts);
            }

            var cls = Classify();
            var record = TolScaleRecord.Build(d, instrument, env, results, stats, checks, cls.Result, TolScaleProbeTableFile.Sha256(table));
            // V6: the independent offline recomputation must equal what the instrument computed; a mismatch is INVALID evidence
            var recomputation = TolScaleOfflineVerifier.Verify(Jcs.Serialize(record));
            if (recomputation.Count > 0)
            {
                invalid.Add("OFFLINE_RECOMPUTATION_MISMATCH:" + string.Join("|", recomputation));
                cls = Classify();
                record = TolScaleRecord.Build(d, instrument, env, results, stats, checks, cls.Result, TolScaleProbeTableFile.Sha256(table));
            }

            var reasons = cls.Reasons.ToList();
            var runRecord = RsRunRecord.Build(Command, "U-RS-3", d, instrument, env, sideDbs, guard, verification, pre.PriorScratch, before, after, invalid, cls.Result,
                new Dictionary<string, string> { [RawFile] = TolScaleRecord.ContentSha256(record) });

            // ---- the result files, create-new, through the single writer ----------------------------------------------------------------
            writer.WriteNewText(ProbeTableFile, TolScaleProbeTableFile.ToFileText(table));
            writer.WriteNewText(RawFile, Jcs.Serialize(record) + "\n");
            writer.WriteNewText(RunFile, RecordJson.ToFileText(runRecord));
            return new TolScaleOutcome(cls.Result, reasons, writer.Ledger.ToList(), record, runRecord);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not RsRefusedException)
        {
            var declaration = RsFailSafe.TryBuild(Command, "U-RS-3", d, instrument, env, sideDbs, guard, verification, pre.PriorScratch, before, after, invalid, ex);
            if (declaration is not null)
            {
                try { writer.WriteNewText(RunFile, declaration); }
                catch (Exception wex) when (wex is IOException or EvidenceWriterException) { } // the record exists already (create-new): nothing is overwritten
            }
            throw;
        }
    }
}
