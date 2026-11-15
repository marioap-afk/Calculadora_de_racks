#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Redraw;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.PushBack;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// I-64 F4-T4B (D-T4B-03) — the writer of the update of the bounded Push Back pilot. It only does PRIMITIVE operations of the drawing,
    /// inside scopes that live for ONE run, and it only returns FACTS: the origin, the survivors, the locked layers, the names that the
    /// views must carry and every warning are decided by the pure run of Application. It never takes the lock of the document: the one lock
    /// around PREPARE, MUTATE and POST is the one of the adapter that runs it. It never erases a view.
    ///
    /// This class is the port of ONE run (I-64 F4-T4C1-HOST, D-T4C-07): it is created only by the target of the adapter, with the exact document
    /// that the adapter locked, and it can begin once. The target retires it on every exit. The objects of AutoCAD live only in the two scope
    /// classes below, which the run always releases.
    /// </summary>
    internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort
    {
        private readonly Document _document;
        private bool _spent;

        internal RackPilotUpdateWriter(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (_spent) throw new InvalidOperationException("The writer of the run was already used.");
            _spent = true;
            return new RackPilotUpdateScope(_document, request);
        }

        internal void Retire() => _spent = true;

        // The facts of one scan as values. The locked layers are listed for every definition that touches one; the run only looks at the
        // members of the plan.
        internal static PushBackPilotScanValues ValuesOf(
            RackSiblingDrawingScan scan, RackCatalog? catalog, IReadOnlyDictionary<string, bool>? preparedBlockMatches)
        {
            var locked = scan.Layers
                .Where(pair => pair.Value.Any(layer => layer.IsLocked))
                .Select(pair => pair.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToList();
            return new PushBackPilotScanValues(true, scan.Facts, scan.RawEnvelopes, locked, catalog, preparedBlockMatches);
        }
    }

    /// <summary>
    /// The scope of one run: PREPARE (the read and the import), the start of MUTATE and POST. It holds the document, the definitions that
    /// the read found, what the import prepared and the definitions that the writes left without a use. Releasing it forgets all of them.
    /// </summary>
    internal sealed class RackPilotUpdateScope : IPushBackPilotUpdateScope
    {
        private readonly Document? _document;
        private readonly bool _documentMatches;
        private readonly Dictionary<string, ObjectId> _definitions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, (PreparedViewRedraw Prepared, DimensionViewKind Kind)> _prepared = new(StringComparer.Ordinal);
        private readonly HashSet<ObjectId> _staleDefinitions = new();
        private RackCatalog? _catalog;
        private bool _disposed;

        internal RackPilotUpdateScope(Document? document, PushBackPilotUpdateRequest request)
        {
            _document = document;
            _documentMatches = document != null && document.UnmanagedObject.ToInt64() == request.AuthorizedDocumentNativeId;
        }

        public PushBackPilotScanValues ReadForPrepare()
        {
            EnsureOpen();
            if (!_documentMatches) return new PushBackPilotScanValues(false, Array.Empty<RackSiblingScanFact>(), new Dictionary<string, string>());

            // The catalog is read outside the ownership of any scope of mutation, and the drawing is read by the same traversal as every
            // other path, which takes its own lock and its own short scope.
            _catalog = LateralHeaderDrawService.LoadCatalog() ?? throw new InvalidOperationException("El catálogo no está disponible.");
            var scan = RackSiblingScan.CaptureDrawing(_document!);
            _definitions.Clear();
            foreach (var pair in scan.Definitions) _definitions[pair.Key] = pair.Value;
            return RackPilotUpdateWriter.ValuesOf(scan, _catalog, null);
        }

        // IMPORT: every PrepareRedraw of the facades imports what its plan references, before any scope of mutation exists.
        public PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan)
        {
            EnsureOpen();
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (!_documentMatches || _catalog == null) return PushBackPilotImportResult.Failed("PrepareUnits se llamó sin una lectura previa.");

            var database = _document!.Database;
            var store = new RackEmbedStore();
            _prepared.Clear();
            foreach (var member in plan.Members)
            {
                if (!_definitions.TryGetValue(member.Key, out var blockId))
                    return PushBackPilotImportResult.Failed("La definición " + member.Key + " ya no está en el dibujo.");

                var embed = store.Deserialize(member.EnvelopeText)
                    ?? throw new InvalidOperationException("El sobre de la vista " + member.Key + " no se puede leer.");
                var address = RackViewCodec.Decode(embed.Kind, embed.View, embed.Section).Address;
                var system = PushBackPilotSystemRebuild.Build(member.DesignText, _catalog);
                PreparedViewRedraw prepared;
                switch (address.Kind)
                {
                    case DimensionViewKind.Planta:
                        prepared = new PushBackPlantaDrawService().PrepareRedraw(database, blockId, system, member.EnvelopeText);
                        break;
                    case DimensionViewKind.Lateral:
                        prepared = new PushBackSystemDrawService().PrepareRedraw(database, blockId, system, member.EnvelopeText, address.Variant.Index);
                        break;
                    default:
                        prepared = new PushBackFrontalDrawService().PrepareRedraw(
                            database,
                            blockId,
                            system,
                            address.Variant.PushBackEnd == RackPushBackEnd.Posterior ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida,
                            member.EnvelopeText,
                            address.Variant.PushBackSide == RackPushBackSide.B ? PushBackSide.B : PushBackSide.A);
                        break;
                }

                _prepared[member.Key] = (prepared, address.Kind);
            }

            return PushBackPilotImportResult.Succeeded();
        }

        // The ONE owned scope of MUTATE, only with no scope of anybody else open. A rejection is a value, never an exception.
        public PushBackPilotMutationStart BeginMutation()
        {
            EnsureOpen();
            if (!_documentMatches || _prepared.Count == 0) throw new InvalidOperationException("BeginMutation se llamó sin una importación previa.");

            var database = _document!.Database;
            if (database.TransactionManager.TopTransaction != null)
                return PushBackPilotMutationStart.Rejected(PushBackPilotUpdateRun.ForeignScopeOpenCause);

            var transaction = database.TransactionManager.StartTransaction();
            return PushBackPilotMutationStart.Started(new RackPilotMutationScope(database, transaction, _prepared, _staleDefinitions));
        }

        // POST: the tidy-up, in the order of the classic editor, and the facts of the drawing afterwards. It never decides a warning.
        public PushBackPilotPostValues Post(PushBackPilotPostRequest request)
        {
            EnsureOpen();
            if (request == null) throw new ArgumentNullException(nameof(request));

            var document = _document!;
            var database = document.Database;

            // 1: no scope of anybody else may be open, or nothing else is done.
            if (database.TransactionManager.TopTransaction != null)
                return new PushBackPilotPostValues(true, null, null, null);

            // 2: the purge, ONCE, over the union of what every unit left without a use, after the seal returned and the scope of MUTATE
            // was released. A failure of the purge only goes to the log: the write is already confirmed.
            SystemBlockWriter.PurgeAfterCommit(database, _staleDefinitions.ToList());

            // 3: the names that the views must carry, as the rule of the run computed them. A key without one is not renamed.
            foreach (var pair in request.DesiredNames)
            {
                if (_definitions.TryGetValue(pair.Key, out var blockId)) RackBlockRenamer.SyncName(document, blockId, pair.Value);
            }

            // 4: the name that each definition has NOW, read by a scope of its own that only reads.
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            using (var reading = database.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (var key in _prepared.Keys)
                {
                    if (_definitions.TryGetValue(key, out var blockId) && reading.GetObject(blockId, OpenMode.ForRead) is BlockTableRecord definition)
                        names[key] = definition.Name;
                }
            }

            // 5: one regeneration; a failure is a fact, and the run decides what it means.
            string? regenFailure = null;
            try
            {
                SystemBlockWriter.ApplyRegen(document, true);
            }
            catch (System.Exception ex)
            {
                regenFailure = ex.GetType().Name + ": " + ex.Message;
            }

            // 6: the re-read of POST, AFTER the purge, so the comparison sees the state that stays.
            var scan = RackSiblingScan.CaptureDrawing(document);
            return new PushBackPilotPostValues(false, RackPilotUpdateWriter.ValuesOf(scan, null, null), names, regenFailure);
        }

        public void Dispose()
        {
            _disposed = true;
            _definitions.Clear();
            _prepared.Clear();
            _staleDefinitions.Clear();
        }

        private void EnsureOpen()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RackPilotUpdateScope));
        }
    }

    /// <summary>
    /// The ONE owned scope of MUTATE. It owns the semantic scope of the drawing that it starts with, and releasing it without a seal is an
    /// abort. It never purges, renames, regenerates or erases: that is POST, which runs after the run released this scope.
    /// </summary>
    internal sealed class RackPilotMutationScope : IPushBackPilotMutationScope
    {
        private const string UnitVariable = "RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT";
        private const string SealVariable = "RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL";

        private readonly Database _database;
        private readonly Transaction _transaction;
        private readonly IReadOnlyDictionary<string, (PreparedViewRedraw Prepared, DimensionViewKind Kind)> _prepared;
        private readonly HashSet<ObjectId> _staleDefinitions;
        private int _units;
        private bool _disposed;

        internal RackPilotMutationScope(
            Database database,
            Transaction transaction,
            IReadOnlyDictionary<string, (PreparedViewRedraw Prepared, DimensionViewKind Kind)> prepared,
            HashSet<ObjectId> staleDefinitions)
        {
            _database = database;
            _transaction = transaction;
            _prepared = prepared;
            _staleDefinitions = staleDefinitions;
        }

        // The re-read inside the owned scope: the traversal of the nucleus, with no lock, no scope of its own and no confirmation.
        public PushBackPilotScanValues Reread()
        {
            EnsureOwned();
            var scan = RackSiblingScan.TraverseCore(_transaction, _database, ObjectId.Null, null);
            var matches = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var pair in _prepared)
                matches[pair.Key] = scan.Definitions.TryGetValue(pair.Key, out var found) && found == pair.Value.Prepared.BlockId;
            return RackPilotUpdateWriter.ValuesOf(scan, null, matches);
        }

        public PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)
        {
            EnsureOwned();
            if (!_prepared.TryGetValue(memberKey, out var unit))
                throw new InvalidOperationException("La vista " + memberKey + " no se preparó.");
            if (!string.Equals(envelopeText, unit.Prepared.PayloadJson, StringComparison.Ordinal))
                throw new InvalidOperationException("El texto de la vista " + memberKey + " no es el del plan.");

            _units++;
            FailUnitIfRequested(_units);

            IReadOnlyCollection<ObjectId> stale;
            LateralHeaderDrawOutcome outcome;
            switch (unit.Kind)
            {
                case DimensionViewKind.Planta:
                    outcome = new PushBackPlantaDrawService().RedrawInTransaction(_database, _transaction, unit.Prepared, out stale);
                    break;
                case DimensionViewKind.Lateral:
                    outcome = new PushBackSystemDrawService().RedrawInTransaction(_database, _transaction, unit.Prepared, out stale);
                    break;
                default:
                    outcome = new PushBackFrontalDrawService().RedrawInTransaction(_database, _transaction, unit.Prepared, out stale);
                    break;
            }

            if (stale != null)
            {
                foreach (var id in stale) _staleDefinitions.Add(id);
            }

            var missing = outcome.MissingInstances
                .Select(instance => string.IsNullOrEmpty(instance.BlockName) ? instance.PieceId : instance.BlockName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            return new PushBackPilotUnitResult(missing);
        }

        public void Seal()
        {
            EnsureOwned();
            FailSealIfRequested("before");
            _transaction.Commit();
            FailSealIfRequested("after");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Without a seal this is the abort.
            _transaction.Dispose();
        }

        // The scope that was started must still be the top one of the drawing: compared by its native identity, never by reference.
        private void EnsureOwned()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RackPilotMutationScope));
            var top = _database.TransactionManager.TopTransaction;
            if (top == null || top.UnmanagedObject != _transaction.UnmanagedObject)
                throw new InvalidOperationException("El ámbito de la escritura ya no es el último abierto del dibujo.");
        }

        // The points of injection of Debug builds: a unit that throws, and a seal that throws before or after the confirmation.
        [Conditional("DEBUG")]
        private static void FailUnitIfRequested(int oneBasedUnit)
        {
            if (int.TryParse(DebugVariable(UnitVariable), out var requested) && requested == oneBasedUnit)
                throw new InvalidOperationException("DEBUG pilot update fault at unit " + oneBasedUnit + ".");
        }

        [Conditional("DEBUG")]
        private static void FailSealIfRequested(string moment)
        {
            if (string.Equals(DebugVariable(SealVariable), moment, StringComparison.Ordinal))
                throw new InvalidOperationException("DEBUG pilot update fault " + moment + " the seal.");
        }

        private static string? DebugVariable(string name)
        {
#if DEBUG
            return Environment.GetEnvironmentVariable(name);
#else
            return null;
#endif
        }
    }
}
