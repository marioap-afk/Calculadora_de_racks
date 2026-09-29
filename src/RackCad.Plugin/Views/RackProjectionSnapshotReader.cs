using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// The ONE read of the drawing for RACKPROYECTAR (ACQUIRE + SNAPSHOT, D-08f). It copies what the pure plan decides on
    /// and nothing else: the physical facts of each selected reference (AUTH-07, AUTH-08 V2), the whole-drawing scan of the
    /// rack definitions read once, the catalog and the project-variable register. It classifies, accepts and rejects
    /// nothing itself: every verdict belongs to the Foundation authorities and the pure plan, and the drawing is not read
    /// again to make a semantic decision after the points.
    /// </summary>
    internal static class RackProjectionSnapshotReader
    {
        /// <summary>Shared scale, angle and normal tolerance of the source placement facts (X-7).</summary>
        private static readonly RackSourceTransformTolerance Tolerance =
            new RackSourceTransformTolerance(GeometryTolerance.Length, GeometryTolerance.Angle, GeometryTolerance.Length);

        internal static RackProjectionSnapshot Read(
            Document document, IReadOnlyList<ObjectId> selectedIds, DimensionViewKind targetKind)
        {
            try
            {
                // SCP: the plane must be parallel to the universal one before any point is asked (D-08f).
                var z = document.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis;
                if (Math.Abs(z.X) > GeometryTolerance.Angle || Math.Abs(z.Y) > GeometryTolerance.Angle || z.Z <= 0.0)
                {
                    return RackProjectionSnapshot.Unavailable(
                        RackProjectionSnapshotFailure.ReadFailed,
                        "el plano XY del SCP actual no es paralelo al universal");
                }

                var references = new List<RackPhysicalReferenceSnapshot>();
                var definitions = new List<RackPhysicalDefinitionSnapshot>();
                var sourceFacts = new Dictionary<string, RackSourceTransformFactsResult>(StringComparer.Ordinal);
                var setAside = new List<string>();
                RackCad.Application.Persistence.ProjectVariablesReadResult variables = null;

                InDocumentTransaction.Run(document, transaction =>
                {
                    var database = document.Database;
                    var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(database);
                    var payloads = new Dictionary<string, string>(StringComparer.Ordinal);
                    variables = ProjectVariablesRegistry.Read(transaction, database);

                    foreach (var id in selectedIds)
                    {
                        var key = id.Handle.ToString();

                        if (!(transaction.GetObject(id, OpenMode.ForRead) is BlockReference reference))
                        {
                            references.Add(new RackPhysicalReferenceSnapshot(key, false, false, null));
                            continue;
                        }

                        if (reference.OwnerId != modelSpaceId)
                        {
                            references.Add(new RackPhysicalReferenceSnapshot(key, true, false, null));
                            continue;
                        }

                        var definitionId = reference.BlockTableRecord;
                        var definitionKey = definitionId.Handle.ToString();
                        var definition = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
                        var isExternalReference = definition.IsFromExternalReference;

                        references.Add(new RackPhysicalReferenceSnapshot(
                            key,
                            true,
                            RackPhysicalSpace.ModelSpace,
                            isExternalReference,
                            definitionKey,
                            new Point2D(reference.Position.X, reference.Position.Y)));

                        // A definition referenced several times (linked cells) is read once.
                        if (!payloads.TryGetValue(definitionKey, out var payload))
                        {
                            payload = RackBlockData.Read(transaction, definitionId);
                            payloads[definitionKey] = payload;
                            definitions.Add(new RackPhysicalDefinitionSnapshot(definitionKey, payload, definition.Name));
                        }

                        if (isExternalReference || string.IsNullOrEmpty(payload))
                        {
                            continue;
                        }

                        // D-08f: a reference the placement contract cannot describe fails closed; no facts are produced for it,
                        // so the pure plan lists it as unsupported before any point.
                        if (reference is MInsertBlock
                            || reference.IsDynamicBlock
                            || definition.IsAnonymous
                            || definition.Annotative == AnnotativeStates.True)
                        {
                            setAside.Add("RackCad: la referencia " + key
                                + " es un arreglo MINSERT, un bloque dinamico, anonimo o anotativo con datos de RackCad: no se puede proyectar.");
                            continue;
                        }

                        sourceFacts[key] = RackSourceTransformClassifier.Classify(
                            RackSourcePlacementCaptureAdapter.Capture(reference, definition), Tolerance);
                    }
                });

                var selection = RackProjectionSelectionFilter.Build(references, definitions, ClassifyKind);
                var notices = new List<string>(selection.Notices);
                notices.AddRange(setAside);
                if (selection.Selection.Members.Count == 0)
                {
                    return RackProjectionSnapshot.Unavailable(
                        RackProjectionSnapshotFailure.NoRackMembers, null, notices);
                }

                var scan = RackSiblingScan.CaptureDrawing(document);
                var facts = new RackProjectionDrawingFacts(
                    scan,
                    LateralHeaderDrawService.LoadCatalog(),
                    variables,
                    () => StructuralSectionCatalogAccess.TryLoad(null, out var catalogue) ? catalogue : null);

                var sessions = new Dictionary<string, RackProjectionKindSession>(StringComparer.OrdinalIgnoreCase);
                RackProjectionKindSession SessionOf(string rackId)
                {
                    if (!sessions.TryGetValue(rackId, out var session))
                    {
                        var group = selection.Selection.RackGroups.First(
                            item => string.Equals(item.RackId, rackId, StringComparison.OrdinalIgnoreCase));
                        var members = group.Members.OrderBy(member => member.PhysicalKey, StringComparer.Ordinal).ToList();
                        session = RackProjectionKindSession.Create(
                            members[0].Kind,
                            rackId,
                            members.Select(member => member.DefinitionKey).Distinct(StringComparer.Ordinal).ToList(),
                            facts);
                        sessions.Add(rackId, session);
                    }

                    return session;
                }

                var services = new RackProjectionServices
                {
                    Authored = rackId => SessionOf(rackId).Authored(),
                    Properties = rackId => SessionOf(rackId).Properties(),
                    Resolve = rackId => SessionOf(rackId).Resolve(),
                    EditPreflight = rackId => SessionOf(rackId).EditPreflight(),
                    Frame = (rackId, address) => SessionOf(rackId).Frame(address),
                    Prepare = (rackId, address) => SessionOf(rackId).Prepare(address),
                };

                return RackProjectionSnapshot.Available(
                    new RackProjectionRequest(selection.Selection, targetKind, sourceFacts, services), notices);
            }
            catch (System.Exception ex)
            {
                return RackProjectionSnapshot.Unavailable(RackProjectionSnapshotFailure.ReadFailed, ex.Message);
            }
        }

        /// <summary>D-08g: the kind is known when the handler registry resolves it, never from the variant of RACKLAYOUT.</summary>
        private static RackPhysicalKindDisposition ClassifyKind(string kind)
            => KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out _)
                ? RackPhysicalKindDisposition.Known
                : RackPhysicalKindDisposition.Unknown;
    }
}
