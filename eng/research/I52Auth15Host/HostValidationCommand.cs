using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Plugin.Drawing;

[assembly: CommandClass(typeof(I52Auth15.HostHarness.HostValidationCommand))]

namespace I52Auth15.HostHarness
{
    /// <summary>
    /// The ONE test-only command of the I-52-AUTH15 host validation. It is NETLOADed from the versioned run folder;
    /// the product Plugin is not. It writes a single evidence file next to the run folder and never prompts.
    /// </summary>
    public sealed class HostValidationCommand
    {
        [CommandMethod("I52AUTH15_HOSTVAL", CommandFlags.Session)]
        public void Run()
        {
            var runDir = Path.GetDirectoryName(typeof(HostValidationCommand).Assembly.Location);
            var outDir = Environment.GetEnvironmentVariable("I52_AUTH15_HV_OUT");

            if (string.IsNullOrEmpty(outDir))
            {
                outDir = Path.Combine(Directory.GetParent(runDir).FullName, "out");
            }

            Directory.CreateDirectory(outDir);
            var log = new HvLog(Path.Combine(outDir, "hostval.log"));
            log.Info("I52AUTH15_HOSTVAL start run=" + runDir + " out=" + outDir);

            // Nothing that touches the RackCad assemblies may be JIT-compiled before Bootstrap has loaded them from
            // the run folder, which is why the work lives in other classes.
            Bootstrap.Run(runDir, outDir, log);
        }
    }

    internal static class Bootstrap
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Run(string runDir, string outDir, HvLog log)
        {
            var problems = new List<string>();
            Assembly plugin = null;

            try
            {
                var context = AssemblyLoadContext.GetLoadContext(typeof(Bootstrap).Assembly) ?? AssemblyLoadContext.Default;

                context.Resolving += (loadContext, name) =>
                {
                    var candidate = Path.Combine(runDir, name.Name + ".dll");
                    return name.Name.StartsWith("RackCad.", StringComparison.Ordinal) && File.Exists(candidate)
                        ? loadContext.LoadFromAssemblyPath(candidate)
                        : null;
                };

                Preload(context, runDir, "RackCad.Domain", problems);
                Preload(context, runDir, "RackCad.Application", problems);
                plugin = Preload(context, runDir, "RackCad.Plugin", problems);
            }
            catch (System.Exception ex)
            {
                problems.Add("preload: " + ex.GetType().FullName + ": " + ex.Message);
            }

            try
            {
                Runner.Execute(runDir, outDir, log, plugin, problems);
            }
            catch (System.Exception ex)
            {
                log.Info("FATAL " + ex);
                problems.Add("fatal: " + ex.GetType().FullName + ": " + ex.Message);

                var doc = new EvidenceDoc();
                doc.Header["note"] = "the runner could not start; no AUTH-15 call was made";
                doc.Problems.AddRange(problems);
                doc.Write(Path.Combine(outDir, "hostval-evidence.json"));
            }

            log.Info("I52AUTH15_HOSTVAL end");
        }

        private static Assembly Preload(AssemblyLoadContext context, string runDir, string simpleName, List<string> problems)
        {
            var path = Path.Combine(runDir, simpleName + ".dll");
            var loaded = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == simpleName).ToList();
            var same = loaded.FirstOrDefault(a => string.Equals(a.Location, path, StringComparison.OrdinalIgnoreCase));

            if (same != null)
            {
                return same;
            }

            if (loaded.Count > 0)
            {
                problems.Add(simpleName + " was already loaded from elsewhere: " + string.Join("; ", loaded.Select(a => a.Location)));
                return null;
            }

            return context.LoadFromAssemblyPath(path);
        }
    }

    internal sealed class SysVarRead
    {
        public string Name { get; set; }

        public bool Ok { get; set; }

        public string Text { get; set; }

        public string Error { get; set; }

        public int? AsInt() =>
            Ok && int.TryParse(Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : (int?)null;
    }

    /// <summary>
    /// The ONE place the harness reads an AutoCAD system variable (HC-6). It only reads names in <see cref="SysVarCatalog"/> (each
    /// verified valid in AutoCAD 2025) and never throws: a failed read comes back as data, so the caller decides what it means.
    /// A missing value is NEVER replaced by a guessed default. RUN-1 was lost to GetSystemVariable("PROFILENAME"), which raises
    /// eInvalidInput; the profile variable is CPROFILE.
    /// </summary>
    internal static class SysVar
    {
        public static SysVarRead Read(string name)
        {
            if (!SysVarCatalog.IsAudited(name))
            {
                return new SysVarRead { Name = name, Ok = false, Error = "not in the audited SysVarCatalog: a new system variable needs review before the harness may read it" };
            }

            try
            {
                var value = Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable(name);

                return new SysVarRead
                {
                    Name = name,
                    Ok = value != null,
                    Text = value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture),
                    Error = value == null ? "AutoCAD returned null" : null,
                };
            }
            catch (System.Exception ex)
            {
                return new SysVarRead { Name = name, Ok = false, Error = ex.GetType().Name + ": " + ex.Message };
            }
        }

        public static int? ReadInt(string name) => Read(name).AsInt();
    }

    internal enum DbKind
    {
        Side,
        Document,
    }

    internal interface IDbScope : IDisposable
    {
        Database Db { get; }
    }

    /// <summary>
    /// The PRODUCTION condition: a document database. A second document is opened from a copy of the launcher's untouched blank
    /// template (never the anchor scratch drawing), locked for the case, and closed with DISCARD afterwards, so a rollback that does
    /// not roll back can neither contaminate the next case nor write anything anywhere. Any failure to lock, close or restore the
    /// active document is recorded in <see cref="Ctx.DocErrors"/> and makes the environment untrustworthy.
    /// </summary>
    internal sealed class DocScope : IDbScope
    {
        private static int _counter;
        private readonly Ctx _c;
        private readonly Database _previousWorking;
        private Document _document;
        private DocumentLock _lock;
        private bool _disposed;

        public DocScope(Ctx c, string label, bool pieceBlock)
        {
            _c = c;
            var template = Path.Combine(c.OutDir, "blank-template.dwg");

            if (!File.Exists(template))
            {
                throw new InvalidOperationException("the launcher did not provide out/blank-template.dwg: DOCUMENT-AUTHORITY is unavailable");
            }

            var directory = Path.Combine(c.OutDir, "doc-cases");
            Directory.CreateDirectory(directory);
            var safe = new string(label.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
            var file = Path.Combine(directory, safe + "-" + (++_counter).ToString("D2", CultureInfo.InvariantCulture) + ".dwg");
            File.Copy(template, file, false);

            try
            {
                _previousWorking = HostApplicationServices.WorkingDatabase;
            }
            catch (System.Exception)
            {
                _previousWorking = null;
            }

            try
            {
                var documents = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                _document = documents.Open(file, false);
                documents.MdiActiveDocument = _document;
                _lock = _document.LockDocument();
                Db = _document.Database;
                HostApplicationServices.WorkingDatabase = Db;

                if (pieceBlock)
                {
                    Fixtures.CreateBlock(Db, Fixtures.PieceBlock);
                }
            }
            catch (System.Exception)
            {
                Dispose();
                throw;
            }
        }

        public Database Db { get; private set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _lock?.Dispose();
            }
            catch (System.Exception ex)
            {
                _c.DocErrors.Add("unlock: " + ex.GetType().Name + ": " + ex.Message);
            }

            try
            {
                _document?.CloseAndDiscard();
            }
            catch (System.Exception ex)
            {
                _c.DocErrors.Add("close and discard: " + ex.GetType().Name + ": " + ex.Message);
            }

            try
            {
                if (_c.AnchorDocument != null)
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = _c.AnchorDocument;
                }

                if (_previousWorking != null)
                {
                    HostApplicationServices.WorkingDatabase = _previousWorking;
                }
            }
            catch (System.Exception ex)
            {
                _c.DocErrors.Add("restore active document: " + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }

    internal sealed class Scope : IDbScope
    {
        private readonly Database _previous;

        public Scope(bool pieceBlock = true)
        {
            try
            {
                _previous = HostApplicationServices.WorkingDatabase;
            }
            catch (System.Exception)
            {
                _previous = null;
            }

            Db = new Database(true, true);

            try
            {
                HostApplicationServices.WorkingDatabase = Db;

                if (pieceBlock)
                {
                    Fixtures.CreateBlock(Db, Fixtures.PieceBlock);
                }
            }
            catch (System.Exception)
            {
                Dispose();
                throw;
            }
        }

        public Database Db { get; }

        public void Dispose()
        {
            try
            {
                if (_previous != null)
                {
                    HostApplicationServices.WorkingDatabase = _previous;
                }
            }
            catch (System.Exception)
            {
            }

            try
            {
                Db.Dispose();
            }
            catch (System.Exception)
            {
            }
        }
    }

    internal sealed class Ctx
    {
        public string RunDir { get; set; }

        public string OutDir { get; set; }

        public HvLog Log { get; set; }

        public Auth15Binding Bind { get; set; }

        public EvidenceDoc Doc { get; set; }

        public LateralHeaderDrawer Drawer { get; } = new LateralHeaderDrawer();

        public CantileverViewPlan CantPlan { get; set; }

        public string CantPlanError { get; set; }

        public Dictionary<string, string> Sums { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>dll name -> sha256 as declared by TRANSFER-METADATA.json (a second, independent record next to SHA256SUMS).</summary>
        public Dictionary<string, string> MetaDlls { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public FilediaRecord Filedia { get; set; }

        public IntPtr InitialWorking { get; set; }

        /// <summary>The database kind the CURRENT case body runs on (the runner sets it per variant).</summary>
        public DbKind Kind { get; set; } = DbKind.Side;

        /// <summary>True while a SIDE-DB-CHARACTERIZATION variant runs: it must not feed the shared HV-11/13/14 logs.</summary>
        public bool Characterizing { get; set; }

        public string CurrentCase { get; set; } = "case";

        public Document AnchorDocument { get; set; }

        public int InitialDocCount { get; set; }

        public List<string> DocErrors { get; } = new List<string>();

        public bool DocumentAuthorityAvailable { get; set; }

        public string DocumentAuthorityError { get; set; }

        /// <summary>A scope on the database kind of the current variant (document = authority, side = characterization).</summary>
        public IDbScope NewScope(bool pieceBlock = true)
        {
            if (Kind == DbKind.Document)
            {
                if (!DocumentAuthorityAvailable)
                {
                    throw new InvalidOperationException("DOCUMENT-AUTHORITY is unavailable: " + (DocumentAuthorityError ?? "not probed"));
                }

                return new DocScope(this, CurrentCase, pieceBlock);
            }

            return new Scope(pieceBlock);
        }

        public Assembly PluginAssembly { get; set; }

        public List<(string Case, string What, bool Unchanged)> Immutability { get; } = new List<(string, string, bool)>();

        public List<(string Case, bool Unchanged)> Placement { get; } = new List<(string, bool)>();

        public List<(string Case, string What, bool Live)> CallerOwned { get; } = new List<(string, string, bool)>();

        public RackEmbedDocument EnvH(int n) => Fixtures.Envelope(RackEmbedDocument.KindDynamic, "HV HeaderRun " + n, RackEmbedDocument.ViewFrontal, n);

        public RackEmbedDocument EnvC(int n) => Fixtures.Envelope(RackEmbedDocument.KindCantilever, "HV Cantilever " + n, RackEmbedDocument.ViewFrontal, n);

        public CreationResult H(Database db, Transaction tr, string name, RackEmbedDocument env, HeaderRunPlan plan = null)
            => Bind.HeaderRun(db, tr, Drawer, plan ?? Fixtures.HeaderRun(), name, env);

        public CreationResult C(Database db, Transaction tr, string name, RackEmbedDocument env)
            => Bind.Cantilever(db, tr, CantPlan, name, env);

        /// <summary>A successful-path header-run call whose plan and envelope are fingerprinted before and after.</summary>
        public CreationResult HRec(string caseId, Database db, Transaction tr, string name, RackEmbedDocument env, HeaderRunPlan plan = null)
        {
            plan = plan ?? Fixtures.HeaderRun();
            var planBefore = Fixtures.Fingerprint(plan);
            var envBefore = Fixtures.Fingerprint(env);
            var result = Bind.HeaderRun(db, tr, Drawer, plan, name, env);
            if (!Characterizing)
            {
                Immutability.Add((caseId, "HeaderRun " + name, planBefore == Fixtures.Fingerprint(plan) && envBefore == Fixtures.Fingerprint(env)));
            }

            NoteLive(caseId, "HeaderRun " + name, db, tr);
            return result;
        }

        public CreationResult CRec(string caseId, Database db, Transaction tr, string name, RackEmbedDocument env)
        {
            var planBefore = Fixtures.Fingerprint(CantPlan);
            var envBefore = Fixtures.Fingerprint(env);
            var result = Bind.Cantilever(db, tr, CantPlan, name, env);
            if (!Characterizing)
            {
                Immutability.Add((caseId, "Cantilever " + name, planBefore == Fixtures.Fingerprint(CantPlan) && envBefore == Fixtures.Fingerprint(env)));
            }

            NoteLive(caseId, "Cantilever " + name, db, tr);
            return result;
        }

        private void NoteLive(string caseId, string what, Database db, Transaction tr)
        {
            var live = false;

            try
            {
                var top = db.TransactionManager.TopTransaction;
                live = !tr.IsDisposed && top != null && top.UnmanagedObject == tr.UnmanagedObject;
            }
            catch (System.Exception)
            {
                live = false;
            }

            if (!Characterizing)
            {
                CallerOwned.Add((caseId, what, live));
            }

        }
    }

    internal sealed class DefInfo
    {
        public string Name;
        public bool IsLayout;
        public bool IsAnonymous;
        public bool IsDynamic;
        public bool Erased;
        public int BlockRefs;
        public int Texts;
        public int Dimensions;
        public int Polylines;
        public int Circles;
        public int Others;
        public List<string> RefTargets = new List<string>();
        public string Raw;
        public int Chunks;
        public int MaxChunk;
        public int ReferencesToIt;
        public int Entities => BlockRefs + Texts + Dimensions + Polylines + Circles + Others;
    }

    internal static class Runner
    {
        private const string Unit = "I-52-AUTH15";

        // ================================================================ execution

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Execute(string runDir, string outDir, HvLog log, Assembly plugin, List<string> preProblems)
        {
            var doc = new EvidenceDoc();
            var ctx = new Ctx { RunDir = runDir, OutDir = outDir, Log = log, Doc = doc, PluginAssembly = plugin };
            var started = DateTime.UtcNow;
            var stopped = false;

            doc.Problems.AddRange(preProblems);

            try
            {
                ReadPackage(ctx);
                doc.Host["startUtc"] = started.ToString("o");

                // HC-1: FILEDIA is the Owner's preference. The run is only valid if run.scr restored EXACTLY the captured original
                // before this command started. Otherwise NO case runs.
                var filediaProblem = CheckFilediaAtStart(ctx);

                if (filediaProblem != null)
                {
                    doc.Problems.Add("FILEDIA: " + filediaProblem);
                    doc.StoppedBy = "FILEDIA not restored: INVALID RUN";
                    doc.StopKind = StopKind.Filedia;
                    stopped = true;
                    log.Info("FILEDIA INVALID: " + filediaProblem);
                }

                // HC-7: everything immutable about this run that can be recorded WITHOUT touching a fallible AutoCAD API is persisted
                // now, before HV-00. "expected" package hashes are package facts; loaded assembly facts are HV-00's, later.
                RecordEarlyIdentity(ctx);
                SafeWrite(doc, outDir, log);

                BuildFixtures(ctx);

                if (plugin != null)
                {
                    ctx.Bind = Auth15Binding.Bind(plugin, doc.Problems);
                }

                try
                {
                    ctx.InitialWorking = HostApplicationServices.WorkingDatabase?.UnmanagedObject ?? IntPtr.Zero;
                }
                catch (System.Exception)
                {
                    ctx.InitialWorking = IntPtr.Zero;
                }

                var entries = new List<CaseEntry>
                {
                    new CaseEntry("HV-00", "BIND", "the versioned Plugin/Application are the only ones loaded; both AUTH-15 overloads resolve; B-1 fact recorded", Hv00, false),
                    new CaseEntry("HV-01", "HeaderRun", "success: definition, nested content, exact envelope, no placement, caller-owned transaction; caller abort leaves nothing", Hv01, true),
                    new CaseEntry("HV-02", "Cantilever", "success: definition, role layers, exact envelope, no placement; caller abort leaves nothing", Hv02, true),
                    new CaseEntry("HV-03", "Both", "caller abort leaves SNAP identical to before", Hv03, true),
                    new CaseEntry("HV-04", "Both", "caller commit persists both definitions and their envelopes across SaveAs/reopen; no references", Hv04, false),
                    new CaseEntry("HV-05", "Both", "name collision: HeaderRun _1 policy, Cantilever _2 policy; pre-existing definitions unchanged", Hv05, true),
                    new CaseEntry("HV-06", "Both", "TransactionMismatch (a..e), no write", Hv06, false),
                    new CaseEntry("HV-07", "Both", "InvalidPlan, no write", Hv07, false),
                    new CaseEntry("HV-08", "Both", "InvalidBlockName pre-write; HeaderRun \"<>\" characterization = WriteFailed (effective name empty) + clean rollback", Hv08, true),
                    new CaseEntry("HV-09", "Both", "InvalidEnvelope, no write", Hv09, false),
                    new CaseEntry("HV-10", "HeaderRun", "MissingLibraryBlocks with exact distinct representatives; envelope absent; rollback clean", Hv10, true),
                    new CaseEntry("HV-11", "Both", "plans and envelopes unchanged by successful calls", Hv11, false),
                    new CaseEntry("HV-12", "Both", "no batch memory across independent calls in one transaction", Hv12, true),
                    new CaseEntry("HV-13", "Both", "no reference placement in model space or any layout", Hv13, true),
                    new CaseEntry("HV-14", "Both", "no internal commit; transaction stays caller-owned", Hv14, true),
                };

                try
                {
                    ctx.AnchorDocument = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    ctx.InitialDocCount = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Count;
                }
                catch (System.Exception)
                {
                    ctx.InitialDocCount = 0;
                }

                SafeWrite(doc, outDir, log);

                foreach (var entry in entries)
                {
                    if (stopped)
                    {
                        doc.Cases.Add(new CaseRecord(entry.Id, entry.Family, entry.Expected));
                        continue;
                    }

                    // Rollback-sensitive cases are AUTHORITATIVE on the DOCUMENT database (the production condition); the same body
                    // is then run again on a side database as CHARACTERIZATION only.
                    var record = RunOne(ctx, log, entry, entry.RollbackSensitive ? DbKind.Document : DbKind.Side, false);
                    doc.Cases.Add(record);
                    doc.LastCase = record.Id;

                    if (record.Deviation)
                    {
                        doc.Deviations.Add(record.Id);
                    }

                    SafeWrite(doc, outDir, log);
                    string why;

                    if (entry.Id == "HV-00")
                    {
                        if (record.Result != Outcome.Pass)
                        {
                            stopped = true;
                            doc.StopKind = StopKind.Hv00;
                            doc.StoppedBy = "HV-00 is not PASS: no AUTH-15 call was made";
                        }
                        else
                        {
                            ProbeDocumentAuthority(ctx);
                            SafeWrite(doc, outDir, log);

                            if (!RunControls(ctx, log, doc, outDir, out why))
                            {
                                stopped = true;
                                doc.StopKind = StopKind.Exception;
                                doc.StoppedBy = why;
                                doc.Problems.Add(why);
                            }
                        }
                    }
                    else if (record.StopRun)
                    {
                        stopped = true;
                        doc.StopKind = StopKind.Deviation;
                        doc.StoppedBy = entry.Id + " asked to stop: continuing would make later evidence untrustworthy";
                    }

                    if (!stopped && !EnvironmentTrustworthy(ctx, out why))
                    {
                        stopped = true;
                        doc.StopKind = StopKind.Exception;
                        doc.StoppedBy = why;
                        doc.Problems.Add(why);
                    }

                    if (!stopped && entry.RollbackSensitive)
                    {
                        var side = RunOne(ctx, log, entry, DbKind.Side, true);
                        doc.SideCharacterizations.Add(side);
                        SafeWrite(doc, outDir, log);

                        if (!EnvironmentTrustworthy(ctx, out why))
                        {
                            stopped = true;
                            doc.StopKind = StopKind.Exception;
                            doc.StoppedBy = why;
                            doc.Problems.Add(why);
                        }
                    }
                }

                doc.Completed = !stopped;

                try
                {
                    var now = HostApplicationServices.WorkingDatabase?.UnmanagedObject ?? IntPtr.Zero;

                    if (now != ctx.InitialWorking)
                    {
                        doc.Problems.Add("WorkingDatabase was not restored to the document database");
                    }
                }
                catch (System.Exception ex)
                {
                    doc.Problems.Add("WorkingDatabase check: " + ex.Message);
                }
            }
            catch (System.Exception ex)
            {
                log.Info("RUNNER EXCEPTION " + ex);
                doc.Problems.Add("runner: " + ex.GetType().FullName + ": " + ex.Message);
                doc.StopKind = StopKind.Exception;
            }
            finally
            {
                var liveEnd = ReadLiveFiledia();
                doc.Host["filediaLiveAtHarnessEnd"] = liveEnd;

                if (ctx.Filedia != null && ctx.Filedia.Before != null && liveEnd != ctx.Filedia.Before)
                {
                    doc.Problems.Add("FILEDIA at the end of the run (" + (liveEnd == null ? "unreadable" : liveEnd.ToString()) + ") differs from the original (" + ctx.Filedia.Before + "): INVALID RUN");
                }

                doc.NotExercisable.Add(NotExercisable(
                    "EnvelopeWriteFailed",
                    "needs a read-back that differs from the written envelope; that cannot be provoked without fault injection into RackBlockData/AutoCAD, which this harness must not add"));
                doc.NotExercisable.Add(NotExercisable(
                    "InvalidEnvelope (serialization failure)",
                    "needs RackEmbedStore.Serialize to throw on an otherwise valid envelope; not reachable without fault injection. The null / blank Id, Kind, Name variants ARE exercised by HV-09"));
                doc.Host["endUtc"] = DateTime.UtcNow.ToString("o");
                doc.Host["startUtc"] = started.ToString("o");
                doc.Host["exitCode"] = "not known to the harness: see launcher-record.json";
                doc.State = "final";
                doc.Write(Path.Combine(outDir, "hostval-evidence.json"));
                log.Info("evidence written; verdict " + doc.Verdict());
            }
        }

        private sealed class CaseEntry
        {
            public CaseEntry(string id, string family, string expected, Action<Ctx, CaseRecord> body, bool rollbackSensitive)
            {
                Id = id;
                Family = family;
                Expected = expected;
                Body = body;
                RollbackSensitive = rollbackSensitive;
            }

            public string Id { get; }

            public string Family { get; }

            public string Expected { get; }

            public Action<Ctx, CaseRecord> Body { get; }

            public bool RollbackSensitive { get; }
        }

        private static CaseRecord RunOne(Ctx ctx, HvLog log, CaseEntry entry, DbKind kind, bool characterizing)
        {
            ctx.Kind = kind;
            ctx.Characterizing = characterizing;
            ctx.CurrentCase = entry.Id + (characterizing ? "-side" : string.Empty);

            var label = characterizing ? "SIDE-DB-CHARACTERIZATION" : (entry.RollbackSensitive ? "DOCUMENT-AUTHORITY" : "SIDE-DB");
            var record = new CaseRecord(entry.Id, entry.Family, entry.Expected) { DbKind = label };
            log.Info(entry.Id + " begin [" + label + "]");
            CurrentRecord = record;

            try
            {
                entry.Body(ctx, record);
            }
            catch (System.Exception ex)
            {
                record.Exception = ex.GetType().FullName + ": " + ex.Message;
                log.Info(entry.Id + " EXCEPTION " + ex);
            }
            finally
            {
                CurrentRecord = null;
            }

            record.Finish();
            log.Info(entry.Id + " [" + label + "] " + record.Result + (record.Deviation ? " DEVIATION" : string.Empty));
            return record;
        }

        /// <summary>Whether continuing would still produce trustworthy evidence. Only a working database that was NOT restored stops the
        /// run (later scopes would restore to the wrong place). A changed open-document count or a document that could not be closed is
        /// recorded as a Problem, which blocks a PASS, but does not stop the independent cases that follow.</summary>
        private static bool EnvironmentTrustworthy(Ctx ctx, out string why)
        {
            why = null;

            try
            {
                var now = HostApplicationServices.WorkingDatabase?.UnmanagedObject ?? IntPtr.Zero;

                if (now != ctx.InitialWorking)
                {
                    why = "WorkingDatabase was not restored to the anchor document's database after " + ctx.CurrentCase;
                }

                if (why == null && ctx.InitialDocCount > 0 && Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Count != ctx.InitialDocCount)
                {
                    NoteProblem(ctx, "the open-document count changed after " + ctx.CurrentCase);
                }

                if (why == null && ctx.DocErrors.Count > 0)
                {
                    NoteProblem(ctx, "document authority error(s) up to " + ctx.CurrentCase + ": " + string.Join("; ", ctx.DocErrors));
                }
            }
            catch (System.Exception ex)
            {
                why = "environment check failed after " + ctx.CurrentCase + ": " + ex.GetType().Name + ": " + ex.Message;
            }

            return why == null;
        }

        private static void NoteProblem(Ctx ctx, string problem)
        {
            if (!ctx.Doc.Problems.Contains(problem))
            {
                ctx.Doc.Problems.Add(problem);
            }
        }

        /// <summary>Opens and closes ONE throw-away document from the blank template, so a missing document authority is known (and
        /// recorded) before any case needs it. The authority cases are UNKNOWN, never silently side-database, if it is unavailable.</summary>
        private static void ProbeDocumentAuthority(Ctx ctx)
        {
            var d = ctx.Doc.DocumentAuthority;
            ctx.Kind = DbKind.Document;
            ctx.Characterizing = false;
            ctx.CurrentCase = "probe";
            ctx.DocumentAuthorityAvailable = true;

            try
            {
                using (new DocScope(ctx, "probe", false))
                {
                }

                d["available"] = true;
            }
            catch (System.Exception ex)
            {
                ctx.DocumentAuthorityAvailable = false;
                ctx.DocumentAuthorityError = ex.GetType().Name + ": " + ex.Message;
                d["available"] = false;
                d["error"] = ctx.DocumentAuthorityError;
            }

            d["documentsAtStart"] = ctx.InitialDocCount;
        }

        private static void SafeWrite(EvidenceDoc doc, string outDir, HvLog log)
        {
            try
            {
                doc.Write(Path.Combine(outDir, "hostval-evidence.json"));
            }
            catch (System.Exception ex)
            {
                log.Info("progressive evidence write failed: " + ex.Message);
            }
        }

        private static int? ReadLiveFiledia() => SysVar.ReadInt("FILEDIA");

        /// <summary>Null when run.scr restored the original FILEDIA exactly and it is still that value; otherwise the reason.</summary>
        private static string CheckFilediaAtStart(Ctx c)
        {
            var host = c.Doc.Host;
            var live = ReadLiveFiledia();
            host["filediaLiveAtHarnessStart"] = live;
            var path = Path.Combine(c.OutDir, "filedia.txt");

            if (!File.Exists(path))
            {
                host["filediaBefore"] = null;
                host["filediaDuringNetload"] = null;
                host["filediaAfter"] = null;
                return "filedia.txt was not written by run.scr";
            }

            var record = FilediaRecord.Parse(File.ReadAllText(path), out var error);
            c.Filedia = record;
            host["filediaBefore"] = record.Before;
            host["filediaDuringNetload"] = record.During;
            host["filediaAfter"] = record.After;
            return error ?? record.Validate(live);
        }

        private static string ShortName(string assembly) =>
            assembly == "I52Auth15.HostHarness" ? "Harness" : assembly.Substring("RackCad.".Length);

        private static void RecordEarlyIdentity(Ctx c)
        {
            var host = c.Doc.Host;
            var package = c.Doc.Package;
            var errors = new List<string>();

            void Attempt(string what, Action action)
            {
                try
                {
                    action();
                }
                catch (System.Exception ex)
                {
                    errors.Add(what + ": " + ex.GetType().Name + ": " + ex.Message);
                }
            }

            host["runFolder"] = c.RunDir;

            Attempt("process identity", () =>
            {
                using (var process = Process.GetCurrentProcess())
                {
                    host["pid"] = process.Id;
                    host["processName"] = process.ProcessName;
                    host["processStartUtc"] = process.StartTime.ToUniversalTime().ToString("o");
                }
            });

            Attempt("acad executable", () =>
            {
                using (var process = Process.GetCurrentProcess())
                {
                    var main = process.MainModule;
                    host["acadExecutable"] = main.FileName;
                    host["acadFileVersion"] = main.FileVersionInfo.FileVersion;
                    host["acadSha256"] = Sha256File(main.FileName);
                }
            });

            Attempt("harness DLL", () =>
            {
                var location = typeof(HostValidationCommand).Assembly.Location;
                host["harnessDllPath"] = location;
                host["harnessDllSha256"] = Sha256File(location);
            });

            // EXPECTED hashes: what the package says these assemblies must be (SHA256SUMS, cross-checked against TRANSFER-METADATA).
            foreach (var name in new[] { "RackCad.Plugin", "RackCad.Application", "RackCad.Domain", "I52Auth15.HostHarness" })
            {
                var file = name + ".dll";
                c.Sums.TryGetValue("run/" + file, out var fromSums);
                c.MetaDlls.TryGetValue(file, out var fromMeta);
                package["expected" + ShortName(name) + "Sha256"] = fromSums;

                if (string.IsNullOrEmpty(fromSums) || !string.Equals(fromSums, fromMeta, StringComparison.OrdinalIgnoreCase))
                {
                    c.Doc.Problems.Add("expected hash of " + file + " is missing or differs between SHA256SUMS and TRANSFER-METADATA");
                }
            }

            var scratch = Path.Combine(c.OutDir, "scratch.dwg");
            host["scratchDocumentPath"] = scratch;
            Attempt("scratch hash", () => host["scratchSha256AtHarnessStart"] = Sha256File(scratch));
            host["scratchSha256DeclaredByLauncher"] = Environment.GetEnvironmentVariable("I52_AUTH15_HV_SCRATCH_SHA256");
            host["ownerNoTouchDeclaredByLauncher"] = Environment.GetEnvironmentVariable("I52_AUTH15_HV_OWNER_NOTOUCH") == "1";
            host["earlyIdentityErrors"] = errors;

            if (errors.Count > 0)
            {
                c.Doc.Problems.Add("early identity could not be recorded: " + string.Join("; ", errors));
            }
        }

        private static Dictionary<string, object> NotExercisable(string what, string why) =>
            new Dictionary<string, object> { ["surface"] = what, ["reason"] = why, ["result"] = "NOT_EXERCISABLE_WITHOUT_FAULT_INJECTION" };

        // ================================================================ package metadata

        private static void ReadPackage(Ctx c)
        {
            var doc = c.Doc;
            var root = Directory.GetParent(c.RunDir).FullName;
            var metaPath = Path.Combine(root, "TRANSFER-METADATA.json");
            var sumsPath = Path.Combine(root, "SHA256SUMS");

            doc.Package["packageRoot"] = root;
            doc.Package["runDirectory"] = c.RunDir;
            doc.Package["outDirectory"] = c.OutDir;

            if (File.Exists(sumsPath))
            {
                foreach (var line in File.ReadAllLines(sumsPath))
                {
                    var at = line.IndexOf("  ", StringComparison.Ordinal);

                    if (at > 0)
                    {
                        c.Sums[line.Substring(at + 2).Trim()] = line.Substring(0, at).Trim();
                    }
                }

                doc.Package["sha256SumsDigest"] = Sha256File(sumsPath);
            }
            else
            {
                doc.Problems.Add("SHA256SUMS not found next to the run folder");
            }

            var mismatches = new List<string>();

            foreach (var pair in c.Sums.Where(p => p.Key.StartsWith("run/", StringComparison.OrdinalIgnoreCase)))
            {
                var file = Path.Combine(root, pair.Key.Replace('/', Path.DirectorySeparatorChar));

                if (!File.Exists(file) || !string.Equals(Sha256File(file), pair.Value, StringComparison.OrdinalIgnoreCase))
                {
                    mismatches.Add(pair.Key);
                }
            }

            doc.Package["runFilesVerified"] = c.Sums.Count(p => p.Key.StartsWith("run/", StringComparison.OrdinalIgnoreCase));
            doc.Package["runFileMismatches"] = mismatches;

            if (mismatches.Count > 0)
            {
                doc.Problems.Add("run\\ files differ from SHA256SUMS: " + string.Join(", ", mismatches));
            }

            if (!File.Exists(metaPath))
            {
                doc.Problems.Add("TRANSFER-METADATA.json not found next to the run folder");
                return;
            }

            using (var meta = JsonDocument.Parse(File.ReadAllText(metaPath)))
            {
                var m = meta.RootElement;

                string S(string name) => m.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

                doc.Header["unit"] = Unit;
                doc.Header["implementationSha"] = S("implementationSha");
                doc.Header["harnessSha"] = S("harnessSha");
                doc.Header["srcTree"] = S("implementationSrcTree");
                doc.Header["testsTree"] = S("implementationTestsTree");
                doc.Header["harnessSrcTree"] = S("harnessSrcTree");
                doc.Header["harnessTestsTree"] = S("harnessTestsTree");

                var equal = m.TryGetProperty("treesEqual", out var te) && te.ValueKind == JsonValueKind.True
                            && S("implementationSrcTree") == S("harnessSrcTree")
                            && S("implementationTestsTree") == S("harnessTestsTree")
                            && !string.IsNullOrEmpty(S("implementationSrcTree"));

                doc.Header["treesEqual"] = equal;

                if (!equal)
                {
                    doc.Problems.Add("treesEqual is not true in TRANSFER-METADATA.json");
                }

                doc.Package["metadataSha256"] = Sha256File(metaPath);

                if (m.TryGetProperty("dlls", out var dlls) && dlls.ValueKind == JsonValueKind.Object)
                {
                    foreach (var dll in dlls.EnumerateObject())
                    {
                        if (dll.Value.TryGetProperty("sha256", out var sha) && sha.ValueKind == JsonValueKind.String)
                        {
                            c.MetaDlls[dll.Name] = sha.GetString();
                        }
                    }
                }
                else
                {
                    doc.Problems.Add("TRANSFER-METADATA.json has no dlls section");
                }
            }
        }

        private static string Sha256File(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                return Convert.ToHexString(SHA256.HashData(stream));
            }
        }

        private static void BuildFixtures(Ctx c)
        {
            var doc = c.Doc;
            var catalogs = Path.Combine(c.RunDir, "catalogs");

            try
            {
                c.CantPlan = Fixtures.CantileverFrontal(catalogs);
                doc.Fixtures["cantileverPlanFingerprint"] = Fixtures.Fingerprint(c.CantPlan);
                doc.Fixtures["cantileverPlanCurves"] = c.CantPlan.Curves.Count;
                doc.Fixtures["cantileverPlanSignature"] = Fixtures.Sha256Hex(c.CantPlan.Signature());
            }
            catch (System.Exception ex)
            {
                c.CantPlanError = ex.GetType().FullName + ": " + ex.Message;
                doc.Fixtures["cantileverPlanError"] = c.CantPlanError;
            }

            doc.Fixtures["headerRunPlanFingerprint"] = Fixtures.Fingerprint(Fixtures.HeaderRun());
            doc.Fixtures["headerRunMissingPlanFingerprint"] = Fixtures.Fingerprint(Fixtures.HeaderRunWithMissing(out _));
            doc.Fixtures["headerRunEnvelopeFingerprint(n=1)"] = Fixtures.Fingerprint(c.EnvH(1));
            doc.Fixtures["cantileverEnvelopeFingerprint(n=2)"] = c.CantPlan == null ? null : Fixtures.Fingerprint(c.EnvC(2));
            doc.Fixtures["hostModel"] =
                "every case runs on harness-owned side databases (new Database(true,true)), one per case, with HostApplicationServices.WorkingDatabase "
                + "set to it for the case and restored after; the scratch document is only the WorkingDatabase anchor and is never written";
        }

        // ================================================================ helpers

        /// <summary>The record of the case currently running, so transaction ends can be attributed and can fail it.</summary>
        internal static CaseRecord CurrentRecord;

        /// <summary>
        /// Ends a caller's transaction by ABORTING it and RECORDS what happened: whether Abort and Dispose were attempted and
        /// succeeded (with the exception text if not), whether the transaction is disposed afterwards, and the active-transaction
        /// count and top-transaction identity before and after. RUN-2's leaks could not be attributed because the previous helper
        /// swallowed every exception; now a failed Abort or Dispose FAILS the current case and the rollback is never assumed.
        /// </summary>
        private static void End(Transaction transaction, string label = null) => EndCore(transaction, label, abort: true);

        /// <summary>Ends a transaction by DISPOSING it without an explicit Abort (what a forgotten Commit does). Same recording.</summary>
        private static void EndDisposeOnly(Transaction transaction, string label = null) => EndCore(transaction, label, abort: false);

        /// <summary>After a Commit: only the dispose is recorded; there is no rollback to attribute.</summary>
        private static void EndAfterCommit(Transaction transaction, string label = null)
        {
            var outcome = EndCore(transaction, label ?? "after commit", abort: false, expectRollback: false);
        }

        private static TxOutcome EndCore(Transaction transaction, string label, bool abort, bool expectRollback = true)
        {
            if (transaction == null)
            {
                return null;
            }

            var outcome = new TxOutcome { Label = label ?? "tx" };
            Autodesk.AutoCAD.DatabaseServices.TransactionManager manager = null;

            try
            {
                manager = transaction.TransactionManager;
                outcome.ActiveBefore = manager.NumberOfActiveTransactions;
            }
            catch (System.Exception ex)
            {
                outcome.AbortError = "could not read the transaction manager: " + ex.GetType().Name + ": " + ex.Message;
            }

            try
            {
                outcome.IsDisposedBefore = transaction.IsDisposed;
            }
            catch (System.Exception)
            {
                outcome.IsDisposedBefore = false;
            }

            if (abort && !outcome.IsDisposedBefore)
            {
                outcome.AbortAttempted = true;

                try
                {
                    transaction.Abort();
                    outcome.AbortSucceeded = true;
                }
                catch (System.Exception ex)
                {
                    outcome.AbortError = ex.GetType().Name + ": " + ex.Message;
                }
            }

            outcome.DisposeAttempted = true;

            try
            {
                transaction.Dispose();
                outcome.DisposeSucceeded = true;
            }
            catch (System.Exception ex)
            {
                outcome.DisposeError = ex.GetType().Name + ": " + ex.Message;
            }

            try
            {
                outcome.IsDisposedAfter = transaction.IsDisposed;
            }
            catch (System.Exception)
            {
                outcome.IsDisposedAfter = false;
            }

            try
            {
                if (manager != null)
                {
                    outcome.ActiveAfter = manager.NumberOfActiveTransactions;
                    var top = manager.TopTransaction;
                    outcome.TopAfter = top == null ? "null" : top.UnmanagedObject.ToString();
                }
            }
            catch (System.Exception ex)
            {
                outcome.TopAfter = "unreadable: " + ex.GetType().Name;
            }

            var record = CurrentRecord;

            if (record != null)
            {
                record.Tx.Add(outcome);

                foreach (var failure in outcome.Failures(abortExpected: abort && expectRollback))
                {
                    record.Check("transaction end [" + outcome.Label + "]: " + failure, "Abort/Dispose succeed and the transaction is disposed", failure, false);
                }
            }

            return outcome;
        }

        private static bool Same(Transaction a, Transaction b) =>
            a != null && b != null && !a.IsDisposed && !b.IsDisposed && a.UnmanagedObject == b.UnmanagedObject;

        private static string Joined(List<string> lines) => lines.Count == 0 ? "identical" : string.Join("; ", lines);

        /// <summary>
        /// The post-abort comparison. ANY difference from the baseline is a leak and a FAIL: nothing is normalized, excused or
        /// reclassified (anonymous *D dimension blocks, nested definitions, layers, dictionaries, table entries all count).
        /// Every leaked key and handle is listed, in the assertion and in the case's Leaks.
        /// </summary>
        private static void AbortClean(CaseRecord r, string label, Snapshot baseline, Snapshot after)
        {
            var leaks = baseline.Diff(after);

            foreach (var leak in leaks)
            {
                r.Leaks.Add(label + ": " + leak);
            }

            r.Check(label + ": no leak (SNAP identical to before)", "identical", Joined(leaks), leaks.Count == 0);
        }

        private static bool NeedCantilever(Ctx c, CaseRecord r, string what)
        {
            if (c.CantPlan != null)
            {
                return true;
            }

            r.Unknown(what, "Cantilever fixture plan available", c.CantPlanError ?? "not built");
            return false;
        }

        private static string Raw(Ctx c, Transaction tr, ObjectId id, out int chunks, out int maxChunk)
        {
            chunks = 0;
            maxChunk = 0;

            var owner = (DBObject)tr.GetObject(id, OpenMode.ForRead);

            if (owner.ExtensionDictionary.IsNull)
            {
                return null;
            }

            var dictionary = (DBDictionary)tr.GetObject(owner.ExtensionDictionary, OpenMode.ForRead);

            if (!dictionary.Contains(c.Bind.DictKey))
            {
                return null;
            }

            var record = (Xrecord)tr.GetObject(dictionary.GetAt(c.Bind.DictKey), OpenMode.ForRead);

            if (record.Data == null)
            {
                return null;
            }

            var builder = new StringBuilder();

            foreach (TypedValue value in record.Data)
            {
                if (value.TypeCode == (short)DxfCode.Text)
                {
                    var text = (string)value.Value;
                    chunks++;
                    maxChunk = Math.Max(maxChunk, text.Length);
                    builder.Append(text);
                }
            }

            return builder.ToString();
        }

        private static DefInfo Inspect(Ctx c, Transaction tr, ObjectId id)
        {
            var info = new DefInfo();
            var record = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
            info.Name = record.Name;
            info.IsLayout = record.IsLayout;
            info.IsAnonymous = record.IsAnonymous;
            info.IsDynamic = record.IsDynamicBlock;
            info.Erased = record.IsErased;

            foreach (ObjectId entityId in record)
            {
                var entity = tr.GetObject(entityId, OpenMode.ForRead);

                if (entity is BlockReference reference)
                {
                    info.BlockRefs++;
                    info.RefTargets.Add(((BlockTableRecord)tr.GetObject(reference.BlockTableRecord, OpenMode.ForRead)).Name);
                }
                else if (entity is DBText)
                {
                    info.Texts++;
                }
                else if (entity is Dimension)
                {
                    info.Dimensions++;
                }
                else if (entity is Polyline)
                {
                    info.Polylines++;
                }
                else if (entity is Circle)
                {
                    info.Circles++;
                }
                else
                {
                    info.Others++;
                }
            }

            info.Raw = Raw(c, tr, id, out info.Chunks, out info.MaxChunk);
            info.ReferencesToIt = ReferenceOwners(record.Database, tr, id).Count;
            return info;
        }

        /// <summary>
        /// The owner (definition, model space or layout) of every BlockReference that points at <paramref name="target"/>,
        /// found by SCANNING every block table record. <c>BlockTableRecord.GetBlockReferenceIds</c> is deliberately not used: a
        /// reference created inside a transaction that has not committed yet may not be registered in that list, which
        /// would make a "no references" or "two references" assertion depend on AutoCAD's bookkeeping instead of on the data.
        /// </summary>
        private static List<ObjectId> ReferenceOwners(Database db, Transaction tr, ObjectId target)
        {
            var owners = new List<ObjectId>();
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var referenceClass = RXObject.GetClass(typeof(BlockReference));

            foreach (ObjectId ownerId in blockTable)
            {
                var owner = (BlockTableRecord)tr.GetObject(ownerId, OpenMode.ForRead);

                foreach (ObjectId entityId in owner)
                {
                    if (!entityId.ObjectClass.IsDerivedFrom(referenceClass))
                    {
                        continue;
                    }

                    if (((BlockReference)tr.GetObject(entityId, OpenMode.ForRead)).BlockTableRecord == target)
                    {
                        owners.Add(ownerId);
                    }
                }
            }

            return owners;
        }

        private static void Negative(
            CaseRecord r, string label, string expectedFailure, Database viewDb, Transaction viewTr, Func<CreationResult> call)
        {
            var before = Snapshot.Take(viewDb, viewTr);
            var result = call();
            var ok = result.Thrown == null && !result.IsSuccess && result.Failure == expectedFailure
                     && result.DefinitionId.IsNull && result.BlockName == null;

            r.Check(label + ": typed failure", expectedFailure + ", DefinitionId Null, BlockName null, no exception", result.Describe(), ok);

            var diff = before.Diff(Snapshot.Take(viewDb, viewTr));
            r.Check(label + ": no write", "SNAP identical", Joined(diff), diff.Count == 0);
        }

        private static void PlacementCheck(Ctx c, CaseRecord r, string label, Snapshot before, Snapshot after)
        {
            var diff = before.Diff(after, Snapshot.IsPlacementKey);
            r.Check(label + ": model space and every layout unchanged", "identical", Joined(diff), diff.Count == 0);
            if (!c.Characterizing)
            {
                c.Placement.Add((r.Id + " " + label, diff.Count == 0));
            }

        }

        private static void CallerOwnedCheck(CaseRecord r, string label, Database db, Transaction tr, int activeExpected)
        {
            r.Check(label + ": transaction not disposed", true, !tr.IsDisposed);
            var top = db.TransactionManager.TopTransaction;
            r.Check(label + ": still the top transaction (native identity)", true, top != null && top.UnmanagedObject == tr.UnmanagedObject);
            r.Check(
                label + ": active transaction count unchanged", activeExpected.ToString(),
                db.TransactionManager.NumberOfActiveTransactions.ToString(),
                db.TransactionManager.NumberOfActiveTransactions == activeExpected);
        }

        private static void RequireRead(CaseRecord r, SysVarRead read)
        {
            if (read.Ok && !string.IsNullOrEmpty(read.Text))
            {
                r.Check(read.Name + " recorded", "non-empty", read.Text, true);
            }
            else
            {
                r.Unknown(read.Name + " readable", "a value", read.Error ?? "empty");
            }
        }

        // ================================================================ ROLLBACK CONTROLS (RB-xx)
        //
        // No AUTH-15 here unless the control says so. Each control does the same thing: snapshot, write inside ONE caller transaction,
        // snapshot inside (sanity: something was written), end the transaction (recorded, never swallowed), snapshot again and enumerate
        // EVERY difference. A control result is a RAW OUTCOME: nothing is reinterpreted and nothing here decides who is to blame; the
        // controls never change the HV verdict. They exist to attribute F-1 (RUN-2: everything survived the caller's abort).

        private sealed class ControlEntry
        {
            public ControlEntry(string id, string family, string expected, Action<Ctx, CaseRecord> body, params DbKind[] kinds)
            {
                Id = id;
                Family = family;
                Expected = expected;
                Body = body;
                Kinds = kinds;
            }

            public string Id { get; }

            public string Family { get; }

            public string Expected { get; }

            public Action<Ctx, CaseRecord> Body { get; }

            public DbKind[] Kinds { get; }
        }

        private static bool RunControls(Ctx ctx, HvLog log, EvidenceDoc doc, string outDir, out string why)
        {
            why = null;
            var both = new[] { DbKind.Side, DbKind.Document };
            var controls = new List<ControlEntry>
            {
                new ControlEntry("RB-01", "primitive writes, side database", "block + layer + entity + extension dictionary/Xrecord created then aborted: nothing survives", Rb01, DbKind.Side),
                new ControlEntry("RB-01V", "primitive writes, dispose WITHOUT Abort", "the same, but the transaction is only disposed (a forgotten Commit): nothing survives", Rb01V, both),
                new ControlEntry("RB-01D", "primitive writes, DOCUMENT database", "the primitive control on a document under LockDocument: the production condition", Rb01, DbKind.Document),
                new ControlEntry("RB-02a", "LateralHeaderDrawer.CreateSystemBlock, no dimension/annotation", "the family creator alone, no AUTH-15, then abort", Rb02a, both),
                new ControlEntry("RB-02b", "LateralHeaderDrawer.CreateSystemBlock, with dimension", "the family creator alone with a dimension, no AUTH-15, then abort", Rb02b, both),
                new ControlEntry("RB-02c", "CantileverViewMaterializer.CreateBlockDefinitionNamed", "the Cantilever creator alone, no AUTH-15, then abort", Rb02c, both),
                new ControlEntry("RB-03", "hand-made definition + RackBlockData.Write", "the envelope writer alone on a hand-made definition, then abort", Rb03, both),
                new ControlEntry("RB-05", "dimension + RecomputeDimensionBlock only", "native dimension residue (Defpoints, *D) in isolation, then abort", Rb05, both),
            };

            foreach (var control in controls)
            {
                foreach (var kind in control.Kinds)
                {
                    ctx.Kind = kind;
                    ctx.Characterizing = false;
                    ctx.CurrentCase = control.Id;
                    var label = kind == DbKind.Document ? "DOCUMENT-AUTHORITY" : "SIDE-DB";
                    var record = new CaseRecord(control.Id, control.Family, control.Expected) { DbKind = label };
                    log.Info(control.Id + " begin [" + label + "]");
                    CurrentRecord = record;

                    try
                    {
                        control.Body(ctx, record);
                    }
                    catch (System.Exception ex)
                    {
                        record.Exception = ex.GetType().FullName + ": " + ex.Message;
                        log.Info(control.Id + " EXCEPTION " + ex);
                    }
                    finally
                    {
                        CurrentRecord = null;
                    }

                    record.Finish();
                    log.Info(control.Id + " [" + label + "] " + record.Result);
                    doc.Controls.Add(record);
                    SafeWrite(doc, outDir, log);

                    if (!EnvironmentTrustworthy(ctx, out why))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void RollbackControl(Ctx c, CaseRecord r, Action<Database, Transaction> writes, bool piece, bool abort)
        {
            using (var scope = c.NewScope(piece))
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    writes(db, tr);
                    var inside = baseline.Diff(Snapshot.Take(db, tr));
                    r.Check("sanity: the control wrote something inside the transaction", "> 0 differences", inside.Count + " differences", inside.Count > 0);
                }
                finally
                {
                    if (abort)
                    {
                        End(tr, r.Id + " abort");
                    }
                    else
                    {
                        EndDisposeOnly(tr, r.Id + " dispose without abort");
                    }
                }

                AbortClean(r, r.Id + " after the caller's rollback", baseline, Snapshot.Now(db));
            }
        }

        private static void PrimitiveWrites(Database db, Transaction tr)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var block = new BlockTableRecord { Name = "CTRL_RB_BLOCK", Origin = Point3d.Origin };
            blockTable.Add(block);
            tr.AddNewlyCreatedDBObject(block, true);

            var line = new Line(Point3d.Origin, new Point3d(10.0, 0.0, 0.0));
            block.AppendEntity(line);
            tr.AddNewlyCreatedDBObject(line, true);

            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
            var layer = new LayerTableRecord { Name = "CTRL_RB_LAYER" };
            layers.Add(layer);
            tr.AddNewlyCreatedDBObject(layer, true);

            block.CreateExtensionDictionary();
            var dictionary = (DBDictionary)tr.GetObject(block.ExtensionDictionary, OpenMode.ForWrite);
            var record = new Xrecord { Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, "CTRL")) };
            dictionary.SetAt("CTRL_RB_KEY", record);
            tr.AddNewlyCreatedDBObject(record, true);
        }

        private static void Rb01(Ctx c, CaseRecord r) => RollbackControl(c, r, PrimitiveWrites, false, true);

        private static void Rb01V(Ctx c, CaseRecord r) => RollbackControl(c, r, PrimitiveWrites, false, false);

        private static void Rb02a(Ctx c, CaseRecord r) =>
            RollbackControl(c, r, (db, tr) => c.Drawer.CreateSystemBlock(db, tr, Fixtures.HeaderRunPlain(), "CTRL_RB02A"), true, true);

        private static void Rb02b(Ctx c, CaseRecord r) =>
            RollbackControl(c, r, (db, tr) => c.Drawer.CreateSystemBlock(db, tr, Fixtures.HeaderRun(), "CTRL_RB02B"), true, true);

        private static void Rb02c(Ctx c, CaseRecord r)
        {
            if (c.CantPlan == null || c.Bind == null || c.Bind.MaterializerNamed == null)
            {
                r.Unknown("Cantilever creator reachable", "the fixture plan and the internal creator", c.CantPlanError ?? "the internal creator was not found");
                return;
            }

            RollbackControl(c, r, (db, tr) => c.Bind.CallMaterializer(db, tr, c.CantPlan, "CTRL_RB02C", out _), true, true);
        }

        private static void Rb03(Ctx c, CaseRecord r)
        {
            if (c.Bind == null || c.Bind.BlockDataWrite == null)
            {
                r.Unknown("RackBlockData.Write reachable", "the internal writer", "the internal writer was not found");
                return;
            }

            RollbackControl(c, r, (db, tr) =>
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
                var block = new BlockTableRecord { Name = "CTRL_RB03", Origin = Point3d.Origin };
                var id = blockTable.Add(block);
                tr.AddNewlyCreatedDBObject(block, true);
                c.Bind.CallEnvelopeWrite(tr, id, new RackEmbedStore().Serialize(c.EnvH(3)));
            }, false, true);
        }

        private static void Rb05(Ctx c, CaseRecord r) =>
            RollbackControl(c, r, (db, tr) =>
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
                var block = new BlockTableRecord { Name = "CTRL_RB05", Origin = Point3d.Origin };
                blockTable.Add(block);
                tr.AddNewlyCreatedDBObject(block, true);

                var dimension = new RotatedDimension(0.0, Point3d.Origin, new Point3d(48.0, 0.0, 0.0), new Point3d(24.0, -6.0, 0.0), string.Empty, db.Dimstyle);
                block.AppendEntity(dimension);
                tr.AddNewlyCreatedDBObject(dimension, true);
                dimension.RecomputeDimensionBlock(true);
            }, false, true);

        // ================================================================ HV-00

        private static void Hv00(Ctx c, CaseRecord r)
        {
            // Prerequisite: an open scratch document anchors HostApplicationServices.WorkingDatabase.
            Database working = null;

            try
            {
                working = HostApplicationServices.WorkingDatabase;
            }
            catch (System.Exception)
            {
            }

            var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            r.Check(
                "ENVIRONMENT_PREREQUISITE: a scratch document is open (WorkingDatabase anchor)", "WorkingDatabase != null and an active document",
                "WorkingDatabase " + (working == null ? "null" : "set") + ", document " + (document == null ? "none" : document.Name),
                working != null && document != null);

            // Identity facts read through the ONE audited helper (ACADVER, CPROFILE). A failed read is recorded with its error and makes
            // this case UNKNOWN; it never throws out of the harness and never becomes a guessed default.
            var acad = SysVar.Read("ACADVER");
            var profile = SysVar.Read("CPROFILE");
            c.Doc.Host["acadVersion"] = acad.Ok ? acad.Text : null;
            c.Doc.Host["acadVersionReadError"] = acad.Error;
            c.Doc.Host["profile"] = profile.Ok ? profile.Text : null;
            c.Doc.Host["profileReadError"] = profile.Error;
            RequireRead(r, acad);
            RequireRead(r, profile);
            c.Doc.Host["os"] = Environment.OSVersion.VersionString;
            c.Doc.Host["scratchDocument"] = document?.Name;

            // One assembly each, loaded from the versioned run folder, byte-identical to the package.
            var runDir = Path.GetFullPath(c.RunDir).TrimEnd('\\');
            var loadedList = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var simple in new[] { "RackCad.Plugin", "RackCad.Application", "RackCad.Domain" })
            {
                var matches = loadedList.Where(a => a.GetName().Name == simple).ToList();
                r.Check(simple + ": exactly one loaded", "1", matches.Count.ToString(), matches.Count == 1);

                if (matches.Count != 1)
                {
                    continue;
                }

                var location = matches[0].Location;
                var inRun = string.Equals(Path.GetDirectoryName(Path.GetFullPath(location)), runDir, StringComparison.OrdinalIgnoreCase);
                r.Check(simple + ": Location inside run\\", runDir, location, inRun);

                var sha = inRun ? Sha256File(location) : null;
                c.Sums.TryGetValue("run/" + simple + ".dll", out var expectedSha);
                c.MetaDlls.TryGetValue(simple + ".dll", out var metaSha);
                r.Check(simple + ": SHA-256 equals SHA256SUMS", expectedSha ?? "<missing in SHA256SUMS>", sha ?? "<not in run>",
                    sha != null && string.Equals(sha, expectedSha, StringComparison.OrdinalIgnoreCase));
                r.Check(simple + ": SHA-256 equals TRANSFER-METADATA", metaSha ?? "<missing in TRANSFER-METADATA>", sha ?? "<not in run>",
                    sha != null && string.Equals(sha, metaSha, StringComparison.OrdinalIgnoreCase));

                // LOADED facts (observed here), kept apart from the EXPECTED package hashes recorded before HV-00.
                c.Doc.Binding["loaded" + ShortName(simple) + "Path"] = location;
                c.Doc.Binding["loaded" + ShortName(simple) + "Sha256"] = sha;

                if (simple == "RackCad.Application")
                {

                    // Application identity against BOTH expectations: the harness's own compile-time reference, and the Plugin's
                    // dependency on it (its declared reference, and the parameter types of the AUTH-15 methods, checked in the binding).
                    r.Check(
                        "harness expectation: the harness compiled against this same Application assembly", true,
                        ReferenceEquals(typeof(HeaderRunPlan).Assembly, matches[0]));

                    var reference = c.PluginAssembly?.GetReferencedAssemblies().FirstOrDefault(n => n.Name == "RackCad.Application");
                    r.Check(
                        "Plugin expectation: the Plugin's own reference to RackCad.Application matches this assembly (name and version)",
                        matches[0].GetName().Version?.ToString() ?? "<none>", reference?.Version?.ToString() ?? "<no reference>",
                        reference != null && reference.Version == matches[0].GetName().Version);
                }
            }

            var harness = typeof(HostValidationCommand).Assembly;
            var harnessSha = Sha256File(harness.Location);
            c.Doc.Binding["loadedHarnessPath"] = harness.Location;
            c.Doc.Binding["loadedHarnessSha256"] = harnessSha;
            c.Sums.TryGetValue("run/I52Auth15.HostHarness.dll", out var harnessSums);
            c.MetaDlls.TryGetValue("I52Auth15.HostHarness.dll", out var harnessMeta);
            r.Check("the harness DLL's SHA-256 equals SHA256SUMS and TRANSFER-METADATA", "both equal " + (harnessSums ?? "<missing>"), harnessSha + " / " + (harnessMeta ?? "<missing>"),
                string.Equals(harnessSha, harnessSums, StringComparison.OrdinalIgnoreCase)
                && string.Equals(harnessSha, harnessMeta, StringComparison.OrdinalIgnoreCase));
            r.Check(
                "the harness itself was loaded from run\\", runDir, harness.Location,
                string.Equals(Path.GetDirectoryName(Path.GetFullPath(harness.Location)), runDir, StringComparison.OrdinalIgnoreCase));

            r.Check(
                "the product Plugin is not the loaded extension: only the harness command is under test",
                "no second RackCad.Plugin", string.Join("; ", loadedList.Where(a => a.GetName().Name == "RackCad.Plugin").Select(a => a.Location)),
                loadedList.Count(a => a.GetName().Name == "RackCad.Plugin") == 1);

            // Both AUTH-15 overloads resolve by exact parameter types.
            if (c.Bind == null)
            {
                r.Check("AUTH-15 surface bound", "both overloads + result + RackBlockData.Read", "no Plugin assembly", false);
                return;
            }

            foreach (var finding in c.Bind.Findings)
            {
                r.Check("binding exactness: " + finding.Name, "true", finding.Ok + (finding.Detail.Length == 0 ? string.Empty : " (" + finding.Detail + ")"), finding.Ok);
            }

            if (!c.Bind.Ready)
            {
                r.Check("AUTH-15 surface bound", "both overloads + result + RackBlockData.Read", "not bound", false);
                return;
            }

            r.Check("CreateInTransaction overloads on RackDefinitionCreator", "2", c.Bind.OverloadCount.ToString(), c.Bind.OverloadCount == 2);
            r.Check("HeaderRun overload resolved", true, c.Bind.HeaderRunOverload != null);
            r.Check("Cantilever overload resolved", true, c.Bind.CantileverOverload != null);
            c.Doc.Binding["overloads"] = c.Bind.OverloadCount;
            c.Doc.Binding["dictKey"] = c.Bind.DictKey;

            // The B-1 fact, recorded on a real transaction: wrapper identity is unusable, native identity is exact.
            using (var scope = new Scope(false))
            {
                var db = scope.Db;
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var top = db.TransactionManager.TopTransaction;
                    var again = db.TransactionManager.TopTransaction;
                    var referenceEquals = ReferenceEquals(top, tr);
                    var native = top != null && top.UnmanagedObject == tr.UnmanagedObject;

                    r.Check("recorded: ReferenceEquals(TopTransaction, tr)", "False (B-1 premise)", referenceEquals.ToString(), true);
                    r.Check("recorded: two reads of TopTransaction are the same wrapper", "False (a new wrapper per read)", ReferenceEquals(top, again).ToString(), true);
                    r.Check("TopTransaction.UnmanagedObject == tr.UnmanagedObject", true, native);
                    c.Doc.Binding["referenceEqualsTopTransaction"] = referenceEquals;
                    c.Doc.Binding["nativeIdentityEqual"] = native;
                }
                finally
                {
                    End(tr);
                }
            }
        }

        // ================================================================ HV-01

        private static void Hv01(Ctx c, CaseRecord r)
        {
            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var env = c.EnvH(1);
                var expectedRaw = new RackEmbedStore().Serialize(env);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var active = db.TransactionManager.NumberOfActiveTransactions;
                    var result = c.HRec(r.Id, db, tr, "AUTH15HV_HR", env);
                    r.Observed = result.Describe();

                    if (!r.Check("call succeeded", "IsSuccess=True, Failure=None, no exception", result.Describe(),
                            result.Thrown == null && result.IsSuccess && result.Failure == "None"))
                    {
                        return;
                    }

                    r.Check("DefinitionId is valid and not erased", true, !result.DefinitionId.IsNull && !result.DefinitionId.IsErased);
                    var info = Inspect(c, tr, result.DefinitionId);
                    r.Equal("BlockName equals the definition's name", info.Name, result.BlockName);
                    r.Equal("requested name was free, so the actual name is the requested one", "AUTH15HV_HR", result.BlockName);
                    r.Check("a top-level, non-anonymous, non-layout definition", "true/false/false", $"{!info.IsLayout}/{info.IsAnonymous}/{info.IsLayout}", !info.IsLayout && !info.IsAnonymous);
                    r.Equal("raw envelope on the definition is exactly RackEmbedStore.Serialize(envelope)", expectedRaw, info.Raw);
                    r.Check("the envelope spans several 255-character chunks", "chunks >= 2 and each <= 255", $"{info.Chunks} chunks, max {info.MaxChunk}", info.Chunks >= 2 && info.MaxChunk <= 255);
                    r.Equal("RackBlockData.Read returns the same envelope", expectedRaw, c.Bind.ReadBack(tr, result.DefinitionId));

                    var headerRefs = info.RefTargets.Count(t => t.StartsWith(Fixtures.HeaderName, StringComparison.Ordinal));
                    r.Check("nested header definition referenced at both placements", "2", headerRefs.ToString(), headerRefs == 2);
                    r.Check("the loose piece block is referenced once", "1", info.RefTargets.Count(t => t == Fixtures.PieceBlock).ToString(), info.RefTargets.Count(t => t == Fixtures.PieceBlock) == 1);
                    r.Check("the annotation (DBText) and the dimension are content of the definition", "1 text, 1 dimension", $"{info.Texts} text, {info.Dimensions} dimension", info.Texts == 1 && info.Dimensions == 1);

                    var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var nestedName = info.RefTargets.FirstOrDefault(t => t.StartsWith(Fixtures.HeaderName, StringComparison.Ordinal));
                    var nestedOk = nestedName != null && blockTable.Has(nestedName);
                    r.Check("the nested definition exists in the block table", true, nestedOk);

                    if (nestedOk)
                    {
                        var nested = Inspect(c, tr, blockTable[nestedName]);
                        r.Check("the nested definition carries no envelope", "null", nested.Raw ?? "null", nested.Raw == null);
                        var nestedOwners = ReferenceOwners(db, tr, blockTable[nestedName]);
                        r.Check("every reference to the nested definition lives inside the system definition", "2 refs, all owned by the new definition",
                            $"{nestedOwners.Count} refs, {nestedOwners.Distinct().Count()} owner(s)",
                            nestedOwners.Count == 2 && nestedOwners.All(o => o == result.DefinitionId));
                    }

                    var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    r.Check("annotation and dimension layers exist", true, layers.Has("RACKCAD_ANOTACIONES") && layers.Has("RACKCAD_COTAS"));
                    r.Check("no reference to the new definition anywhere", "0", info.ReferencesToIt.ToString(), info.ReferencesToIt == 0);

                    PlacementCheck(c, r, "HeaderRun", baseline, Snapshot.Take(db, tr));
                    CallerOwnedCheck(r, "after the call", db, tr, active);
                }
                finally
                {
                    End(tr);
                }

                var after = Snapshot.Now(db);
                AbortClean(r, "caller abort afterwards", baseline, after);
            }
        }

        // ================================================================ HV-02

        private static void Hv02(Ctx c, CaseRecord r)
        {
            if (!NeedCantilever(c, r, "Cantilever fixture"))
            {
                return;
            }

            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var env = c.EnvC(2);
                var expectedRaw = new RackEmbedStore().Serialize(env);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var active = db.TransactionManager.NumberOfActiveTransactions;
                    var result = c.CRec(r.Id, db, tr, "AUTH15HV_CANT", env);
                    r.Observed = result.Describe();

                    if (!r.Check("call succeeded", "IsSuccess=True, Failure=None, no exception", result.Describe(),
                            result.Thrown == null && result.IsSuccess && result.Failure == "None"))
                    {
                        return;
                    }

                    r.Check("DefinitionId is valid and not erased", true, !result.DefinitionId.IsNull && !result.DefinitionId.IsErased);
                    var info = Inspect(c, tr, result.DefinitionId);
                    r.Equal("BlockName equals the definition's name", info.Name, result.BlockName);
                    r.Equal("requested name was free, so the actual name is the requested one", "AUTH15HV_CANT", result.BlockName);
                    r.Check("a top-level, non-anonymous, non-layout definition", "true", (!info.IsLayout && !info.IsAnonymous).ToString(), !info.IsLayout && !info.IsAnonymous);
                    r.Equal("raw envelope on the definition is exactly RackEmbedStore.Serialize(envelope)", expectedRaw, info.Raw);
                    r.Check("the envelope spans several 255-character chunks", "chunks >= 2 and each <= 255", $"{info.Chunks} chunks, max {info.MaxChunk}", info.Chunks >= 2 && info.MaxChunk <= 255);
                    r.Equal("RackBlockData.Read returns the same envelope", expectedRaw, c.Bind.ReadBack(tr, result.DefinitionId));

                    var expectedCircles = c.CantPlan.Curves.Count(x => x.IsCircle && x.Points != null && x.Points.Count == 1 && x.CircleDiameter.Value > 0.0);
                    var expectedPolylines = c.CantPlan.Curves.Count(x => !x.IsCircle && x.Points != null && x.Points.Count >= 2);
                    r.Check("entities equal what the plan can draw", $"{expectedPolylines} polylines + {expectedCircles} circles",
                        $"{info.Polylines} polylines + {info.Circles} circles (+{info.Others} other)",
                        info.Polylines == expectedPolylines && info.Circles == expectedCircles && info.Others == 0 && info.Entities == expectedPolylines + expectedCircles && info.Entities > 0);
                    r.Check("no nested definition: the family draws no references", "0", info.BlockRefs.ToString(), info.BlockRefs == 0);

                    var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    var missingLayers = Enum.GetValues(typeof(CantileverVisualRole)).Cast<CantileverVisualRole>()
                        .Select(CantileverVisualRoles.LayerNameOf).Where(name => !layers.Has(name)).ToList();
                    r.Check("all role layers are present", "none missing", missingLayers.Count == 0 ? "none missing" : string.Join(",", missingLayers), missingLayers.Count == 0);

                    var record = (BlockTableRecord)tr.GetObject(result.DefinitionId, OpenMode.ForRead);
                    var offLayer = record.Cast<ObjectId>().Select(id => (Entity)tr.GetObject(id, OpenMode.ForRead))
                        .Where(e => !e.Layer.StartsWith(CantileverVisualRoles.LayerPrefix, StringComparison.Ordinal)).Select(e => e.Layer).Distinct().ToList();
                    r.Check("every entity is on a role layer", "none off-role", offLayer.Count == 0 ? "none off-role" : string.Join(",", offLayer), offLayer.Count == 0);
                    r.Check("no reference to the new definition anywhere", "0", info.ReferencesToIt.ToString(), info.ReferencesToIt == 0);

                    PlacementCheck(c, r, "Cantilever", baseline, Snapshot.Take(db, tr));
                    CallerOwnedCheck(r, "after the call", db, tr, active);
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "caller abort afterwards", baseline, Snapshot.Now(db));
            }
        }

        // ================================================================ HV-03

        private static void Hv03(Ctx c, CaseRecord r)
        {
            if (!NeedCantilever(c, r, "Cantilever fixture"))
            {
                return;
            }

            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();
                var names = new List<string>();
                var ids = new List<ObjectId>();

                try
                {
                    var header = c.HRec(r.Id, db, tr, "AUTH15HV_RB_H", c.EnvH(31));
                    var cantilever = c.CRec(r.Id, db, tr, "AUTH15HV_RB_C", c.EnvC(32));
                    r.Observed = header.Describe() + " | " + cantilever.Describe();
                    r.Check("both calls succeeded inside ONE caller transaction", "both IsSuccess", r.Observed, header.IsSuccess && cantilever.IsSuccess);

                    if (!(header.IsSuccess && cantilever.IsSuccess))
                    {
                        return;
                    }

                    names.Add(header.BlockName);
                    names.Add(cantilever.BlockName);
                    ids.Add(header.DefinitionId);
                    ids.Add(cantilever.DefinitionId);

                    var mid = Snapshot.Take(db, tr);
                    PlacementCheck(c, r, "both families in one transaction", baseline, mid);
                    var written = baseline.Diff(mid, null, 200);
                    r.Check("sanity: the calls did write inside the transaction", "> 0 differences", written.Count + " differences", written.Count > 0);
                    r.Check("sanity: definitions, nested definition, layers and an anonymous dimension block were created",
                        "BT: + LT: + extension data",
                        $"BT+ {written.Count(x => x.StartsWith("added BT:"))}, LT+ {written.Count(x => x.StartsWith("added LT:"))}",
                        written.Any(x => x.StartsWith("added BT:")) && written.Any(x => x.StartsWith("added LT:")));
                }
                finally
                {
                    End(tr); // the caller aborts
                }

                var after = Snapshot.Now(db);
                AbortClean(r, "after the abort", baseline, after);

                using (var verify = db.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)verify.GetObject(db.BlockTableId, OpenMode.ForRead);
                    r.Check("neither top-level name survives", "absent", string.Join(",", names.Where(blockTable.Has)) is var left && left.Length == 0 ? "absent" : left, names.All(n => !blockTable.Has(n)));
                    r.Check("the nested header definition does not survive", "absent", blockTable.Has(Fixtures.HeaderName) ? "present" : "absent", !blockTable.Has(Fixtures.HeaderName));

                    var layers = (LayerTable)verify.GetObject(db.LayerTableId, OpenMode.ForRead);
                    r.Check("no layer AUTH-15 or the family drawers created survives", "absent",
                        (layers.Has("RACKCAD_ANOTACIONES") || layers.Has("RACKCAD_COTAS") ? "annotation/dimension layer present" : "absent"),
                        !layers.Has("RACKCAD_ANOTACIONES") && !layers.Has("RACKCAD_COTAS"));
                    var anyRole = Enum.GetValues(typeof(CantileverVisualRole)).Cast<CantileverVisualRole>().Select(CantileverVisualRoles.LayerNameOf).Where(layers.Has).ToList();
                    r.Check("no Cantilever role layer survives", "absent", anyRole.Count == 0 ? "absent" : string.Join(",", anyRole), anyRole.Count == 0);
                    verify.Abort();
                }

                var openable = new List<string>();

                foreach (var id in ids)
                {
                    try
                    {
                        using (var probe = db.TransactionManager.StartTransaction())
                        {
                            probe.GetObject(id, OpenMode.ForRead, false);
                            openable.Add(id.Handle.ToString() + " still opens");
                            probe.Abort();
                        }
                    }
                    catch (System.Exception)
                    {
                        // Expected: the object no longer exists.
                    }
                }

                r.Check("the returned ObjectIds no longer open", "none opens", openable.Count == 0 ? "none opens" : string.Join(",", openable), openable.Count == 0);
            }
        }

        // ================================================================ HV-04

        private static void Hv04(Ctx c, CaseRecord r)
        {
            if (!NeedCantilever(c, r, "Cantilever fixture"))
            {
                return;
            }

            var file = Path.Combine(c.OutDir, "hv04.dwg");

            if (File.Exists(file))
            {
                r.Check("the output file does not pre-exist", "absent", "present", false);
                return;
            }

            var envH = c.EnvH(41);
            var envC = c.EnvC(42);
            var rawH = new RackEmbedStore().Serialize(envH);
            var rawC = new RackEmbedStore().Serialize(envC);
            string nameH;
            string nameC;
            string nestedName;

            using (var scope = new Scope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();
                var committed = false;

                try
                {
                    var header = c.HRec(r.Id, db, tr, "AUTH15HV_CM_H", envH);
                    var cantilever = c.CRec(r.Id, db, tr, "AUTH15HV_CM_C", envC);
                    r.Observed = header.Describe() + " | " + cantilever.Describe();

                    if (!r.Check("both calls succeeded", "both IsSuccess", r.Observed, header.IsSuccess && cantilever.IsSuccess))
                    {
                        return;
                    }

                    nameH = header.BlockName;
                    nameC = cantilever.BlockName;
                    nestedName = Inspect(c, tr, header.DefinitionId).RefTargets.FirstOrDefault(t => t.StartsWith(Fixtures.HeaderName, StringComparison.Ordinal));
                    tr.Commit(); // the CALLER commits
                    committed = true;
                }
                finally
                {
                    if (committed)
                    {
                        EndAfterCommit(tr, "HV-04 after the caller's commit");
                    }
                    else
                    {
                        End(tr);
                    }
                }

                PlacementCheck(c, r, "after commit", baseline, Snapshot.Now(db));
                db.SaveAs(file, DwgVersion.Current);
            }

            r.Check("the drawing was saved", "file exists", File.Exists(file) ? "exists" : "missing", File.Exists(file));

            using (var reopened = new Database(false, true))
            {
                reopened.ReadDwgFile(file, FileShare.Read, true, string.Empty);
                reopened.CloseInput(true);

                using (var tr = reopened.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)tr.GetObject(reopened.BlockTableId, OpenMode.ForRead);

                    foreach (var (name, raw, kind) in new[] { (nameH, rawH, "dynamic"), (nameC, rawC, "cantilever") })
                    {
                        r.Check(name + ": definition exists after reopen", true, blockTable.Has(name));

                        if (!blockTable.Has(name))
                        {
                            continue;
                        }

                        var info = Inspect(c, tr, blockTable[name]);
                        r.Equal(name + ": raw envelope exact after reopen", raw, info.Raw);
                        var parsed = new RackEmbedStore().Deserialize(info.Raw);
                        r.Check(name + ": Id/Kind/Name deserialize", "id/kind/name of the composed envelope",
                            parsed == null ? "null" : parsed.Id + "/" + parsed.Kind + "/" + parsed.Name,
                            parsed != null && parsed.Kind == kind && parsed.Id.StartsWith("a15a15a1-", StringComparison.Ordinal) && parsed.Name.StartsWith("HV ", StringComparison.Ordinal));
                        r.Check(name + ": no references to it", "0", info.ReferencesToIt.ToString(), info.ReferencesToIt == 0);
                    }

                    r.Check("the nested header definition persisted", true, nestedName != null && blockTable.Has(nestedName));

                    var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    r.Check("model space is empty after reopen", "0 entities", modelSpace.Cast<ObjectId>().Count() + " entities", !modelSpace.Cast<ObjectId>().Any());
                    tr.Abort();
                }
            }
        }

        // ================================================================ HV-05

        private static void Hv05(Ctx c, CaseRecord r)
        {
            if (!NeedCantilever(c, r, "Cantilever fixture"))
            {
                return;
            }

            using (var scope = c.NewScope())
            {
                var db = scope.Db;

                foreach (var name in new[] { "AUTH15HV_HR", Fixtures.HeaderName, "AUTH15HV_CANT" })
                {
                    Fixtures.CreateBlock(db, name);
                }

                var pre = new Dictionary<string, string>();
                var baseline = Snapshot.Now(db);

                foreach (var name in new[] { "AUTH15HV_HR", Fixtures.HeaderName, "AUTH15HV_CANT" })
                {
                    pre[name] = baseline.Items["BTC:" + name];
                }

                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var header = c.HRec(r.Id, db, tr, "AUTH15HV_HR", c.EnvH(51));
                    var cantilever = c.CRec(r.Id, db, tr, "AUTH15HV_CANT", c.EnvC(52));
                    r.Observed = header.Describe() + " | " + cantilever.Describe();

                    if (!r.Check("both calls succeeded", "both IsSuccess", r.Observed, header.IsSuccess && cantilever.IsSuccess))
                    {
                        return;
                    }

                    r.Equal("HeaderRun applies its own _1 policy to the system definition", "AUTH15HV_HR_1", header.BlockName);
                    r.Equal("Cantilever applies its own _2 policy", "AUTH15HV_CANT_2", cantilever.BlockName);

                    var infoH = Inspect(c, tr, header.DefinitionId);
                    r.Equal("returned HeaderRun name equals the definition's name", infoH.Name, header.BlockName);
                    r.Check("the nested header definition took the _1 policy too", "AUTH15HV_HDR_1",
                        string.Join(",", infoH.RefTargets.Distinct().OrderBy(x => x)), infoH.RefTargets.Contains("AUTH15HV_HDR_1") && !infoH.RefTargets.Contains(Fixtures.HeaderName));
                    r.Equal("returned Cantilever name equals the definition's name", Inspect(c, tr, cantilever.DefinitionId).Name, cantilever.BlockName);

                    var now = Snapshot.Take(db, tr);
                    PlacementCheck(c, r, "collision case", baseline, now);
                    foreach (var pair in pre)
                    {
                        r.Equal("pre-existing definition " + pair.Key + " unchanged (entities, no new extension dictionary)", pair.Value, now.Items["BTC:" + pair.Key]);
                    }
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "caller abort afterwards", baseline, Snapshot.Now(db));
            }
        }

        // ================================================================ HV-06

        private static void Hv06(Ctx c, CaseRecord r)
        {
            var families = new List<(string Name, Func<Database, Transaction, CreationResult> Call)>
            {
                ("HeaderRun", (d, t) => c.H(d, t, "AUTH15HV_M", c.EnvH(61))),
            };

            if (NeedCantilever(c, r, "Cantilever fixture"))
            {
                families.Add(("Cantilever", (d, t) => c.C(d, t, "AUTH15HV_M", c.EnvC(62))));
            }

            using (var scope = new Scope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);

                foreach (var (family, call) in families)
                {
                    // a) null transaction
                    var view = db.TransactionManager.StartTransaction();
                    try
                    {
                        Negative(r, family + " a) null transaction", "TransactionMismatch", db, view, () => call(db, null));
                    }
                    finally
                    {
                        End(view);
                    }

                    // b) a transaction that belongs to ANOTHER database, while this one has its own top transaction
                    using (var other = new Database(true, true))
                    {
                        var own = db.TransactionManager.StartTransaction();
                        var foreign = other.TransactionManager.StartTransaction();
                        try
                        {
                            var otherBefore = Snapshot.Take(other, foreign);
                            Negative(r, family + " b) foreign-database transaction", "TransactionMismatch", db, own, () => call(db, foreign));
                            var otherDiff = otherBefore.Diff(Snapshot.Take(other, foreign));
                            r.Check(family + " b) the foreign database was not written either", "identical", Joined(otherDiff), otherDiff.Count == 0);
                        }
                        finally
                        {
                            End(foreign);
                            End(own);
                        }
                    }

                    // c) the outer transaction while a nested one is the top
                    var outer = db.TransactionManager.StartTransaction();
                    var nested = db.TransactionManager.StartTransaction();
                    try
                    {
                        var top = db.TransactionManager.TopTransaction;
                        r.Check(family + " c) setup: the nested transaction is the top, the outer is not", true,
                            top != null && top.UnmanagedObject == nested.UnmanagedObject && top.UnmanagedObject != outer.UnmanagedObject);
                        Negative(r, family + " c) outer while nested is top", "TransactionMismatch", db, nested, () => call(db, outer));
                    }
                    finally
                    {
                        End(nested);
                        End(outer);
                    }

                    // d) a disposed transaction
                    var disposed = db.TransactionManager.StartTransaction();
                    End(disposed);
                    var viewD = db.TransactionManager.StartTransaction();
                    try
                    {
                        r.Check(family + " d) setup: the transaction is disposed", true, disposed.IsDisposed);
                        Negative(r, family + " d) disposed transaction", "TransactionMismatch", db, viewD, () => call(db, disposed));
                    }
                    finally
                    {
                        End(viewD);
                    }

                    // e) a disposed database
                    var dead = new Database(true, true);
                    dead.Dispose();
                    var viewE = db.TransactionManager.StartTransaction();
                    try
                    {
                        r.Check(family + " e) setup: the database is disposed", true, dead.IsDisposed);
                        Negative(r, family + " e) disposed database", "TransactionMismatch", db, viewE, () => call(dead, viewE));
                    }
                    finally
                    {
                        End(viewE);
                    }
                }

                // Characterizations that are not part of the ruled set: recorded, never counted in the verdict.
                Characterize(c, "null database", () =>
                {
                    var t = db.TransactionManager.StartTransaction();
                    try
                    {
                        var result = c.H(null, t, "AUTH15HV_M", c.EnvH(63));
                        return (result.Describe(), result.Thrown == null && !result.IsSuccess && result.Failure == "TransactionMismatch");
                    }
                    finally
                    {
                        End(t);
                    }
                }, "expected TransactionMismatch");

                Characterize(c, "OpenCloseTransaction (conservatively unsupported)", () =>
                {
                    var t = db.TransactionManager.StartOpenCloseTransaction();
                    try
                    {
                        var result = c.H(db, t, "AUTH15HV_M", c.EnvH(64));
                        return (result.Describe(), result.Thrown == null && !result.IsSuccess && result.Failure == "TransactionMismatch");
                    }
                    finally
                    {
                        End(t);
                    }
                }, "expected TransactionMismatch");

                AbortClean(r, "after every sub-case", baseline, Snapshot.Now(db));
                r.Observed = "see assertions";
            }
        }

        private static void Characterize(Ctx c, string what, Func<(string Observed, bool Matches)> action, string expectation)
        {
            string observed;
            bool matches;

            try
            {
                (observed, matches) = action();
            }
            catch (System.Exception ex)
            {
                observed = "harness exception " + ex.GetType().Name + ": " + ex.Message;
                matches = false;
            }

            c.Doc.Characterizations.Add(new Dictionary<string, object>
            {
                ["what"] = what,
                ["expectation"] = expectation,
                ["observed"] = observed,
                ["matchesExpectation"] = matches,
                ["countedInVerdict"] = false,
            });
        }

        // ================================================================ HV-07

        private static void Hv07(Ctx c, CaseRecord r)
        {
            using (var scope = new Scope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    Negative(r, "HeaderRun null plan", "InvalidPlan", db, tr, () => c.Bind.HeaderRun(db, tr, c.Drawer, null, "AUTH15HV_P", c.EnvH(71)));
                    Negative(r, "HeaderRun null drawer", "InvalidPlan", db, tr, () => c.Bind.HeaderRun(db, tr, null, Fixtures.HeaderRun(), "AUTH15HV_P", c.EnvH(72)));
                    Negative(r, "Cantilever null plan", "InvalidPlan", db, tr, () => c.Bind.Cantilever(db, tr, null, "AUTH15HV_P", c.EnvC(73)));
                    r.Observed = "see assertions";
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "after the abort", baseline, Snapshot.Now(db));
            }
        }

        // ================================================================ HV-08

        private static void Hv08(Ctx c, CaseRecord r)
        {
            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    foreach (var (label, value) in new[] { ("null", (string)null), ("empty", string.Empty), ("whitespace", "   ") })
                    {
                        Negative(r, "HeaderRun name " + label, "InvalidBlockName", db, tr, () => c.H(db, tr, value, c.EnvH(81)));

                        if (c.CantPlan != null)
                        {
                            Negative(r, "Cantilever name " + label, "InvalidBlockName", db, tr, () => c.C(db, tr, value, c.EnvC(82)));
                        }
                        else
                        {
                            r.Unknown("Cantilever name " + label, "InvalidBlockName", c.CantPlanError ?? "no fixture");
                        }
                    }
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "after the abort", baseline, Snapshot.Now(db));

                // CHARACTERIZATION (c): a non-empty name the family policy makes invalid. Ruled: WriteFailed and a clean rollback.
                var tr2 = db.TransactionManager.StartTransaction();
                CreationResult result;

                try
                {
                    result = c.H(db, tr2, "<>", c.EnvH(83));
                    string partial;

                    try
                    {
                        partial = baseline.Diff(Snapshot.Take(db, tr2), null, 8).Count + " differences inside the transaction (partial writes are the caller's rollback)";
                    }
                    catch (System.Exception ex)
                    {
                        partial = "snapshot inside the transaction failed: " + ex.Message;
                    }

                    r.Notes.Add("HeaderRun \"<>\": " + result.Describe() + "; " + partial);
                }
                finally
                {
                    End(tr2);
                }

                var expected = result.Thrown == null && !result.IsSuccess && result.Failure == "WriteFailed";
                r.Check("CHARACTERIZATION HeaderRun \"<>\" surfaces as WriteFailed", "WriteFailed, no exception", result.Describe(), expected);

                if (!expected)
                {
                    r.Deviation = true;
                    r.Notes.Add("NEW DEVIATION: HeaderRun \"<>\" did not come out as ruled. It is recorded and the case is FAIL; the run continues (HV-09..HV-14 are independent).");
                }

                AbortClean(r, "CHARACTERIZATION HeaderRun <> after the caller's abort", baseline, Snapshot.Now(db));

                r.Observed = result.Describe();
            }
        }

        // ================================================================ HV-09

        private static void Hv09(Ctx c, CaseRecord r)
        {
            using (var scope = new Scope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var variants = new List<(string Label, Func<RackEmbedDocument> Make)>
                    {
                        ("null envelope", () => null),
                        ("empty Id", () => { var e = c.EnvH(91); e.Id = string.Empty; return e; }),
                        ("whitespace Id", () => { var e = c.EnvH(91); e.Id = "  "; return e; }),
                        ("null Id", () => { var e = c.EnvH(91); e.Id = null; return e; }),
                        ("empty Kind", () => { var e = c.EnvH(91); e.Kind = string.Empty; return e; }),
                        ("null Kind", () => { var e = c.EnvH(91); e.Kind = null; return e; }),
                        ("empty Name", () => { var e = c.EnvH(91); e.Name = string.Empty; return e; }),
                        ("null Name", () => { var e = c.EnvH(91); e.Name = null; return e; }),
                    };

                    foreach (var (label, make) in variants)
                    {
                        Negative(r, "HeaderRun " + label, "InvalidEnvelope", db, tr, () => c.H(db, tr, "AUTH15HV_E", make()));

                        if (c.CantPlan != null)
                        {
                            Negative(r, "Cantilever " + label, "InvalidEnvelope", db, tr, () => c.C(db, tr, "AUTH15HV_E", make()));
                        }
                        else
                        {
                            r.Unknown("Cantilever " + label, "InvalidEnvelope", c.CantPlanError ?? "no fixture");
                        }
                    }

                    r.Observed = "see assertions";
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "after the abort", baseline, Snapshot.Now(db));
            }
        }

        // ================================================================ HV-10

        private static void Hv10(Ctx c, CaseRecord r)
        {
            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var plan = Fixtures.HeaderRunWithMissing(out var expectedRepresentatives);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var result = c.Bind.HeaderRun(db, tr, c.Drawer, plan, "AUTH15HV_MISS", c.EnvH(101));
                    r.Observed = result.Describe();

                    r.Check("typed MissingLibraryBlocks", "MissingLibraryBlocks, DefinitionId Null, BlockName null, no exception", result.Describe(),
                        result.Thrown == null && !result.IsSuccess && result.Failure == "MissingLibraryBlocks" && result.DefinitionId.IsNull && result.BlockName == null);

                    var missing = result.MissingInstances ?? new List<HeaderBlockInstance>();
                    r.Check("MissingInstances: one representative per distinct (BlockName|View), not per occurrence", "2",
                        missing.Count.ToString(), missing.Count == 2);
                    r.Check("the representatives are the FIRST instance of each key, in first-seen order", "frontal#1, lateral",
                        string.Join(", ", missing.Select(m => m.BlockName + "|" + m.View + "@" + m.Insertion.X)),
                        missing.Count == 2 && ReferenceEquals(missing[0], expectedRepresentatives[0]) && ReferenceEquals(missing[1], expectedRepresentatives[1]));
                    r.Check("the keys are exactly AUTH15HV_ABSENT|frontal and AUTH15HV_ABSENT|lateral", "2 keys",
                        string.Join(", ", missing.Select(m => m.BlockName + "|" + m.View).OrderBy(x => x)),
                        missing.Select(m => m.BlockName + "|" + m.View).OrderBy(x => x).SequenceEqual(new[] { "AUTH15HV_ABSENT|frontal", "AUTH15HV_ABSENT|lateral" }));

                    var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var systemExists = blockTable.Has("AUTH15HV_MISS");
                    r.Check("the system definition exists inside the caller's transaction (post-write failure)", true, systemExists);

                    if (systemExists)
                    {
                        var info = Inspect(c, tr, blockTable["AUTH15HV_MISS"]);
                        r.Check("the envelope is ABSENT from it", "null", info.Raw ?? "null", info.Raw == null);
                        r.Check("the present piece was drawn, the absent ones omitted and reported", "1 reference to the piece",
                            info.RefTargets.Count(t => t == Fixtures.PieceBlock) + " reference(s), " + info.BlockRefs + " total",
                            info.RefTargets.Count(t => t == Fixtures.PieceBlock) == 1 && info.BlockRefs == 1);
                    }
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "after the caller's abort", baseline, Snapshot.Now(db));
            }
        }

        // ================================================================ HV-11

        private static void Hv11(Ctx c, CaseRecord r)
        {
            using (var scope = new Scope())
            {
                var db = scope.Db;
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var envelope = c.EnvH(111);
                    var before = (envelope.Id, envelope.Name, envelope.Kind, envelope.View, envelope.Section, envelope.Design);
                    var result = c.HRec(r.Id, db, tr, "AUTH15HV_IM", envelope);
                    r.Check("HeaderRun call succeeded", "IsSuccess", result.Describe(), result.Thrown == null && result.IsSuccess);
                    r.Check("envelope Id/Name/Kind/View/Section/Design unchanged", "same",
                        (envelope.Id, envelope.Name, envelope.Kind, envelope.View, envelope.Section, envelope.Design) == before ? "same" : "changed",
                        (envelope.Id, envelope.Name, envelope.Kind, envelope.View, envelope.Section, envelope.Design) == before);

                    if (c.CantPlan != null)
                    {
                        var envC = c.EnvC(112);
                        var idBefore = envC.Id;
                        var resultC = c.CRec(r.Id, db, tr, "AUTH15HV_IM_C", envC);
                        r.Check("Cantilever call succeeded", "IsSuccess", resultC.Describe(), resultC.Thrown == null && resultC.IsSuccess);
                        r.Check("Cantilever envelope Id unchanged", idBefore, envC.Id, idBefore == envC.Id);
                    }
                    else
                    {
                        r.Unknown("Cantilever immutability", "unchanged", c.CantPlanError ?? "no fixture");
                    }
                }
                finally
                {
                    End(tr);
                }
            }

            var changed = c.Immutability.Where(x => !x.Unchanged).Select(x => x.Case + " " + x.What).ToList();
            r.Check("across EVERY successful call of the run, plan and envelope fingerprints were identical before and after",
                $"0 changed of {c.Immutability.Count}", $"{changed.Count} changed" + (changed.Count == 0 ? string.Empty : ": " + string.Join(", ", changed)),
                c.Immutability.Count >= 8 && changed.Count == 0);
            r.Observed = c.Immutability.Count + " successful calls fingerprinted";
        }

        // ================================================================ HV-12

        private static void Hv12(Ctx c, CaseRecord r)
        {
            if (!NeedCantilever(c, r, "Cantilever fixture"))
            {
                return;
            }

            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var e1 = c.EnvH(121);
                    var e2 = c.EnvC(122);
                    var e3 = c.EnvH(123);
                    var e4 = c.EnvC(124);
                    var a1 = c.HRec(r.Id, db, tr, "AUTH15HV_B1", e1);
                    var afterA1 = Snapshot.Take(db, tr);

                    // A failure between two successes, of two different kinds.
                    var mismatch = c.Bind.HeaderRun(db, null, c.Drawer, Fixtures.HeaderRun(), "AUTH15HV_B1", c.EnvH(129));
                    var blank = c.Bind.Cantilever(db, tr, c.CantPlan, "   ", c.EnvC(128));
                    r.Check("the failures in between are typed", "TransactionMismatch, InvalidBlockName", mismatch.Failure + ", " + blank.Failure,
                        mismatch.Failure == "TransactionMismatch" && blank.Failure == "InvalidBlockName");
                    var diffBetween = afterA1.Diff(Snapshot.Take(db, tr));
                    r.Check("the failures left no trace", "identical", Joined(diffBetween), diffBetween.Count == 0);

                    var c1 = c.CRec(r.Id, db, tr, "AUTH15HV_B2", e2);
                    var a2 = c.HRec(r.Id, db, tr, "AUTH15HV_B1", e3, Fixtures.HeaderRun(90.0));
                    var c2 = c.CRec(r.Id, db, tr, "AUTH15HV_B2", e4);
                    r.Observed = string.Join(" | ", new[] { a1, c1, a2, c2 }.Select(x => x.Describe()));

                    if (!r.Check("all four independent calls succeeded", "4 x IsSuccess", r.Observed, new[] { a1, c1, a2, c2 }.All(x => x.Thrown == null && x.IsSuccess)))
                    {
                        return;
                    }

                    r.Check("names follow each family's policy, call by call", "B1, B2, B1_1, B2_2",
                        string.Join(", ", new[] { a1, c1, a2, c2 }.Select(x => x.BlockName)),
                        a1.BlockName == "AUTH15HV_B1" && c1.BlockName == "AUTH15HV_B2" && a2.BlockName == "AUTH15HV_B1_1" && c2.BlockName == "AUTH15HV_B2_2");

                    var pairs = new[] { (a1, e1), (c1, e2), (a2, e3), (c2, e4) };

                    foreach (var (result, envelope) in pairs)
                    {
                        var expected = new RackEmbedStore().Serialize(envelope);
                        r.Equal(result.BlockName + " carries its OWN envelope", expected, Inspect(c, tr, result.DefinitionId).Raw);
                    }

                    r.Check("four distinct definitions", "4", pairs.Select(p => p.Item1.DefinitionId).Distinct().Count().ToString(), pairs.Select(p => p.Item1.DefinitionId).Distinct().Count() == 4);
                    PlacementCheck(c, r, "four independent calls", baseline, Snapshot.Take(db, tr));
                }
                finally
                {
                    End(tr);
                }

                AbortClean(r, "after the abort", baseline, Snapshot.Now(db));
            }
        }

        // ================================================================ HV-13

        private static void Hv13(Ctx c, CaseRecord r)
        {
            r.Check("placement observations were collected", ">= 6", c.Placement.Count.ToString(), c.Placement.Count >= 6);

            foreach (var (label, unchanged) in c.Placement)
            {
                r.Check(label, "model space and every layout unchanged", unchanged ? "unchanged" : "CHANGED", unchanged);
            }

            // The strongest form, run once more here: both families, all their content, and the layouts compared by entity handle.
            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    c.HRec(r.Id, db, tr, "AUTH15HV_PL_H", c.EnvH(131));

                    if (c.CantPlan != null)
                    {
                        c.CRec(r.Id, db, tr, "AUTH15HV_PL_C", c.EnvC(132));
                    }

                    var mid = Snapshot.Take(db, tr);
                    var spaces = mid.Items.Keys.Count(k => k.EndsWith("#count", StringComparison.Ordinal));
                    r.Check("the snapshot covers model space and the paper-space layouts", ">= 3 spaces", spaces.ToString(), spaces >= 3);
                    PlacementCheck(c, r, "both families in one transaction", baseline, mid);
                }
                finally
                {
                    End(tr);
                }
            }

            r.Observed = c.Placement.Count + " observations";
        }

        // ================================================================ HV-14

        private static void Hv14(Ctx c, CaseRecord r)
        {
            using (var scope = c.NewScope())
            {
                var db = scope.Db;
                var baseline = Snapshot.Now(db);
                var activeBaseline = db.TransactionManager.NumberOfActiveTransactions; // whatever was active with no caller transaction
                var tr = db.TransactionManager.StartTransaction();

                try
                {
                    var active = db.TransactionManager.NumberOfActiveTransactions;
                    var result = c.HRec(r.Id, db, tr, "AUTH15HV_NC", c.EnvH(141));
                    r.Check("call succeeded", "IsSuccess", result.Describe(), result.Thrown == null && result.IsSuccess);
                    CallerOwnedCheck(r, "after the call", db, tr, active);
                }
                finally
                {
                    End(tr);
                }

                r.Check("no transaction is left active after the caller's abort (back to the count before the caller started one)", activeBaseline.ToString(),
                    db.TransactionManager.NumberOfActiveTransactions.ToString(), db.TransactionManager.NumberOfActiveTransactions == activeBaseline);

                AbortClean(r, "nothing survived the caller's abort", baseline, Snapshot.Now(db));
            }

            var notLive = c.CallerOwned.Where(x => !x.Live).Select(x => x.Case + " " + x.What).ToList();
            r.Check("across EVERY successful call the caller's transaction stayed live and top",
                $"0 not live of {c.CallerOwned.Count}", $"{notLive.Count} not live" + (notLive.Count == 0 ? string.Empty : ": " + string.Join(", ", notLive)),
                c.CallerOwned.Count >= 8 && notLive.Count == 0);
            r.Observed = c.CallerOwned.Count + " successful calls observed";
        }
    }
}
