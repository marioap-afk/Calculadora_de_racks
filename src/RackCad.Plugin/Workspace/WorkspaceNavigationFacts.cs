using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Workspace;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The read-only source of the neutral facts the navigation authority classifies (I-64 D-05, F2-T2A). It is the ONLY place
    /// of the workspace that collects those facts and the handles of the references: it consumes the integrated envelope scan
    /// (<c>RackBlockFinder.ScanEnvelopes</c>, unchanged) and, per scanned definition, lists only its DIRECT references whose
    /// owner is Model Space. It does not classify (<c>NavigationTargets</c> does), builds no inventory, persists no handle and
    /// keeps no AutoCAD object: the transaction and the document lock live in a <c>using</c> and nothing leaves them.
    /// It runs only under the document lock taken by the navigation host, on the document of the session.
    /// </summary>
    public sealed class WorkspaceNavigationFacts : INavigationFactsPort
    {
        private readonly Func<SessionId, long?> _nativeInstanceOf;

        public WorkspaceNavigationFacts(Func<SessionId, long?> nativeInstanceOf)
        {
            _nativeInstanceOf = nativeInstanceOf ?? throw new ArgumentNullException(nameof(nativeInstanceOf));
        }

        public IReadOnlyList<NavigationDefinitionFact> ReadDefinitionFacts(SessionId session) => Scan(session, includeReferences: true);

        /// <summary>
        /// The neutral facts of the browser summary (I-64 F3-T2): name, Kind, view and section of every scanned definition that has a
        /// direct reference in Model Space, with no handle of any reference. It is the same integrated scan of the same document, run
        /// once. A definition with no such reference (an erased rack stays in the table until a PURGE) is not a rack that is present,
        /// so it is omitted and its rack answers not found.
        /// </summary>
        public IReadOnlyList<BrowserRackFact> ReadBrowserFacts(SessionId session)
        {
            var scanned = Scan(session, includeReferences: true);
            var facts = new List<BrowserRackFact>(scanned.Count);
            foreach (var fact in scanned)
            {
                if (fact.ModelSpaceReferenceHandles.Count == 0) continue;
                facts.Add(new BrowserRackFact(fact.IsInterpretable, fact.EnvelopeId, fact.Kind, fact.View, fact.Name, fact.Section));
            }

            return facts;
        }

        // The one place that runs the integrated scan. Both reads list the direct Model Space references of each definition.
        private List<NavigationDefinitionFact> Scan(SessionId session, bool includeReferences)
        {
            var facts = new List<NavigationDefinitionFact>();

            var document = AcApplication.DocumentManager.MdiActiveDocument;
            var native = _nativeInstanceOf(session);
            if (document == null || !native.HasValue || document.UnmanagedObject.ToInt64() != native.Value) return facts;

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var modelSpace = SymbolUtilityServices.GetBlockModelSpaceId(document.Database);

                foreach (var scan in RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: false))
                {
                    var handles = new List<long>();
                    var record = includeReferences ? transaction.GetObject(scan.DefinitionId, OpenMode.ForRead) as BlockTableRecord : null;
                    if (record != null)
                    {
                        foreach (ObjectId referenceId in record.GetBlockReferenceIds(directOnly: true, forceValidity: false))
                        {
                            if (referenceId.IsErased) continue;
                            var reference = transaction.GetObject(referenceId, OpenMode.ForRead) as BlockReference;
                            if (reference != null && reference.OwnerId == modelSpace) handles.Add(referenceId.Handle.Value);
                        }
                    }

                    // F2T2A-KL-01: the integrated scan does not expose the raw probe identity, so ProbedId stays null. An envelope that
                    // cannot be interpreted is therefore never navigable and attribution never depends on the probe.
                    var embed = scan.Embed;
                    facts.Add(new NavigationDefinitionFact(
                        scan.DefinitionId.Handle.Value,
                        false,
                        embed != null,
                        embed?.Id,
                        ProbedId: null,
                        embed?.Kind,
                        embed?.View,
                        embed == null ? -1 : embed.Section,
                        embed?.Name ?? scan.BlockName,
                        ModelSpaceReferenceHandles: handles));
                }
            }

            return facts;
        }
    }
}
