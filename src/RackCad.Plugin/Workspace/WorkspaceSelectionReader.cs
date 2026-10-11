using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.Workspace;
using RackCad.Plugin.Systems.Shared;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The fine selection reader of the workspace panel (I-64 D-06, P-04, P-08). It is the ONLY place of the workspace that
    /// reads the drawing, and the host core calls it ONLY from the drain: in application context, at rest, with no RackCad
    /// modal open. It reads the implicit selection of the active document in read-only mode, prefilters by class without
    /// opening anything, opens each selected block reference and its definition in read mode inside one short transaction
    /// that lives in a <c>using</c> with the document locked in a <c>using</c>, and reads the envelope of the definition with
    /// the integrated reader (<c>RackBlockData.Read</c> and <c>RackEmbedStore.Deserialize</c>; no codec of its own).
    ///
    /// The work is proportional to the selected references: a definition read is remembered per session and the hints
    /// invalidate it. Nested, external and paper-space references count as "not a rack"; an envelope that cannot be read, or
    /// that a newer build wrote, is a diagnostic fact. It never commits, never opens for write, never stamps or invents a
    /// rack Id, never changes the selection, the view or PICKFIRST, and keeps no AutoCAD object between calls.
    /// </summary>
    public sealed class WorkspaceSelectionReader : IWorkspaceSelectionReader
    {
        private readonly WorkspaceDefinitionCache _cache = new WorkspaceDefinitionCache();

        public WorkspaceSelectionRead Read(SessionId session, long nativeInstanceId)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null || document.UnmanagedObject.ToInt64() != nativeInstanceId) return WorkspaceSelectionRead.Failed;

            // PICKFIRST = 0: there is no implied selection. It is reflected as state; the variable is never changed.
            if (!IsPickFirstOn()) return new WorkspaceSelectionRead(Array.Empty<DefinitionFact>(), 0, false);

            var selected = document.Editor.SelectImplied();
            if (selected.Status != PromptStatus.OK || selected.Value == null)
                return new WorkspaceSelectionRead(Array.Empty<DefinitionFact>(), 0);

            var ids = selected.Value.GetObjectIds();
            var facts = new List<DefinitionFact>();
            var notRack = 0;

            try
            {
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var modelSpace = SymbolUtilityServices.GetBlockModelSpaceId(document.Database);
                    var blockReferenceClass = RXObject.GetClass(typeof(BlockReference));

                    foreach (var id in ids)
                    {
                        // Prefilter by class: anything that is not a block reference is not opened.
                        if (!id.ObjectClass.IsDerivedFrom(blockReferenceClass))
                        {
                            notRack++;
                            continue;
                        }

                        var reference = transaction.GetObject(id, OpenMode.ForRead) as BlockReference;
                        if (reference == null || reference.OwnerId != modelSpace)
                        {
                            // Nested in a block or in paper space: counted as "not a rack" (D-06, V1 exclusions).
                            notRack++;
                            continue;
                        }

                        var definitionId = reference.BlockTableRecord;
                        var key = definitionId.Handle.Value;
                        if (!_cache.TryGet(session, key, out var cached))
                        {
                            cached = ReadDefinition(transaction, definitionId);
                            _cache.Store(session, key, cached);
                        }

                        if (cached.IsRack) facts.Add(cached.Fact);
                        else notRack++;
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex) when (ex.ErrorStatus == ErrorStatus.LockViolation)
            {
                // The document cannot be locked right now: nothing was read, the context stays as it was.
                return WorkspaceSelectionRead.Failed;
            }

            Debug.Assert(document.Database.TransactionManager.TopTransaction == null, "The selection read left a transaction open.");
            return new WorkspaceSelectionRead(facts, notRack);
        }

        /// <summary>Forgets what was learned about the definitions of that session. It reads nothing.</summary>
        public void Invalidate(SessionId session) => _cache.Invalidate(session);

        private static CachedDefinition ReadDefinition(Transaction transaction, ObjectId definitionId)
        {
            var definition = transaction.GetObject(definitionId, OpenMode.ForRead) as BlockTableRecord;
            if (definition == null || definition.IsFromExternalReference || definition.IsFromOverlayReference)
                return new CachedDefinition(false, default);

            var json = RackBlockData.Read(transaction, definitionId);
            if (string.IsNullOrWhiteSpace(json)) return new CachedDefinition(false, default);

            // A readable envelope gives its Id as written (possibly empty); an unreadable or future one is a diagnostic fact.
            var envelope = new RackEmbedStore().Deserialize(json);
            return envelope == null
                ? new CachedDefinition(true, new DefinitionFact(false, null))
                : new CachedDefinition(true, new DefinitionFact(true, envelope.Id));
        }

        private static bool IsPickFirstOn()
        {
            var value = AcApplication.GetSystemVariable("PICKFIRST");
            return value == null || Convert.ToInt32(value, CultureInfo.InvariantCulture) != 0;
        }
    }
}
