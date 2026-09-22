using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Placement;
using RackCad.Application.Views.Preparation;
using RackCad.Application.Views.Redraw;
using RackCad.Domain.Systems.Selective;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Selective;
using RackCad.Plugin.Systems.Shared;
using RackCad.Plugin.Views;

namespace RackCad.Plugin
{
    public sealed partial class RackSelectivoCommands
    {
        private sealed class SelectiveInsertPort
            : IRackSiblingInsertPort<RackViewAddress, RackPreparedProductView<HeaderRunPlan>, SiblingRedrawUnit>
        {
            private readonly Document document;
            private readonly ObjectId selected;
            private readonly RackEmbedDocument source;
            private readonly SelectiveRackSystem system;
            private readonly string authoredJson;
            private readonly string rackId;
            private readonly string rackName;
            private readonly string requestedView;
            private readonly string baseName;
            private readonly SelectiveViewAvailabilityFacts availability;
            private RackSiblingScanSnapshot snapshot;

            internal SelectiveInsertPort(Document document, ObjectId selected, RackEmbedDocument source,
                SelectiveRackSystem system, string authoredJson, string rackId, string rackName, string requestedView)
            {
                this.document = document;
                this.selected = selected;
                this.source = source;
                this.system = system;
                this.authoredJson = authoredJson;
                this.rackId = rackId;
                this.rackName = rackName;
                this.requestedView = requestedView;
                baseName = string.IsNullOrWhiteSpace(rackName) ? null : rackName.Trim();
                var catalog = LateralHeaderDrawService.LoadCatalog();
                availability = new SelectiveViewAvailabilityFacts(
                    SelectiveDepthLayout.Count(system),
                    new SelectiveLateralBuilder().Cortes(system, catalog).Select(cut => cut.PostIndex));
            }

            public RackSiblingMembershipSnapshot ScanAndClassifyOnce()
            {
                snapshot = RackSiblingScan.Capture(document, selected, rackId, source?.Id,
                    !string.IsNullOrEmpty(source?.Id), envelope =>
                {
                    var decoded = RackCommandSupport.DecodeView(envelope);
                    return RackViewAvailability.Evaluate(decoded, availability).Status == RackViewAvailabilityStatus.VariantNotPresent;
                });
                return snapshot.Membership;
            }

            public bool TryChooseVariant(out RackViewAddress variant)
            {
                if (string.Equals(requestedView, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase))
                {
                    variant = RackViewAddress.Whole(DimensionViewKind.Planta);
                    return true;
                }

                if (string.Equals(requestedView, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase))
                {
                    var cuts = new SelectiveLateralBuilder().Cortes(system, LateralHeaderDrawService.LoadCatalog());
                    if (cuts.Count == 0) { variant = default; return false; }
                    var maximum = cuts.Max(cut => cut.PostIndex) + 1;
                    var pick = document.Editor.GetInteger(new PromptIntegerOptions("\nQue corte lateral insertar (numero de poste)?")
                    { LowerLimit = 1, UpperLimit = maximum, DefaultValue = 1, UseDefaultValue = true, AllowNone = false });
                    if (pick.Status != PromptStatus.OK || cuts.All(cut => cut.PostIndex != pick.Value - 1))
                    { variant = default; return false; }
                    variant = RackViewAddress.Post(pick.Value - 1);
                    return true;
                }

                var fondoCount = SelectiveDepthLayout.Count(system);
                var fondo = 0;
                if (fondoCount > 1)
                {
                    var pick = document.Editor.GetInteger(new PromptIntegerOptions("\nQue frontal insertar (numero de fondo)?")
                    { LowerLimit = 1, UpperLimit = fondoCount, DefaultValue = 1, UseDefaultValue = true, AllowNone = false });
                    if (pick.Status != PromptStatus.OK) { variant = default; return false; }
                    fondo = pick.Value - 1;
                }
                variant = RackViewAddress.Fondo(fondo);
                return true;
            }

            public RackInsertGateResult CheckCustomProperties(RackSiblingMembershipSnapshot membership)
                => RackSiblingCustomPropertiesGate.Evaluate(membership, snapshot.Properties);

            public RackInsertGateResult CheckAuthored(RackSiblingMembershipSnapshot membership)
                => RackInsertGateResult.Proceed(); // AUTH-13 is executed by PrepareExisting before Resolve.

            public bool TryPrepare(RackViewAddress address, RackSiblingMembershipSnapshot membership,
                out RackPreparedProductView<HeaderRunPlan> prepared, out string diagnostic)
            {
                var entries = membership.AuthoredGateMembers.Select(member =>
                    ProjectVariableScanProjection.Project(member.Fact.DefinitionKey,
                        snapshot.Envelopes.TryGetValue(member.Fact.DefinitionKey, out var envelope) ? envelope : null,
                        member.Fact.LayoutReferenceCount)).ToList();
                var comparison = new SelectiveAuthoredComparisonInput(rackId, entries);
                var curedSource = ComposeEnvelope(source, rackId, rackName,
                    source?.View, source?.Section ?? -1, authoredJson);
                var request = new RackProductViewRequest(RackViewProductOperation.InsertSibling,
                    RackCad.Domain.Systems.Shared.RackSystemKind.SelectiveRack, address, availability);
                var accepted = RackProductIntentAcceptance.Accept(
                    new ExistingRackViewIntent<SelectiveAuthoredComparisonInput>(
                        new ExistingRackContext<SelectiveAuthoredComparisonInput>(curedSource, comparison), request));
                if (!accepted.IsAccepted)
                { prepared = null; diagnostic = accepted.CauseCode; return false; }

                var catalog = LateralHeaderDrawService.LoadCatalog();
                var preparer = new RackProductPreparer<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan>(
                    RackResolvePorts.Selective<SelectivePalletDesignDocument, SelectiveRackSystem>(_ => system),
                    RackViewPreparationPorts.Selective<SelectiveRackSystem, HeaderRunPlan>(
                        (resolved, target) => BuildPlan(resolved, target, catalog),
                        target => target.Kind == DimensionViewKind.Planta && target.Variant.Kind == RackViewVariantKind.Whole
                            || target.Kind == DimensionViewKind.Frontal && target.Variant.Kind == RackViewVariantKind.Fondo
                               && target.Variant.Index < availability.FondoCount
                            || target.Kind == DimensionViewKind.Lateral && target.Variant.Kind == RackViewVariantKind.Post,
                        RackBlockRequirementExtractors.HeaderRun),
                    (resolved, target) => SelectiveViewFrameAdapter.Resolve(resolved, catalog, target),
                    (resolved, target) => ViewName(resolved, target));
                var result = preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.Selective());
                prepared = result.Product;
                diagnostic = result.IsSuccess ? null : result.CauseCode + ": " + result.Diagnostic;
                return result.IsSuccess;
            }

            public ISiblingRedrawPort<SiblingRedrawUnit> CreateRedrawPort(RackPreparedProductView<HeaderRunPlan> prepared)
                => new AutoCadSiblingRedrawPort(document, member => PrepareMember(member, prepared));

            public bool HasOpenTopTransaction => document.Database.TransactionManager.TopTransaction != null;

            public RackInsertPlacementResult Place(RackPreparedProductView<HeaderRunPlan> prepared,
                RackSiblingRedrawPlan<SiblingRedrawUnit> deferredRedraw)
            {
                var deferredUnits = deferredRedraw == null
                    ? null
                    : deferredRedraw.RedrawUnits.Select(item => item.Unit)
                        .Concat(deferredRedraw.EraseUnits.Select(item => item.Unit)).ToList();
                var result = deferredUnits == null
                    ? RackViewPlacement.PlaceSelective(document, prepared)
                    : RackViewPlacement.PlaceSelective(document, prepared, transaction =>
                    {
                        var mutation = SiblingRedrawTransaction.ApplyInTransaction(transaction, deferredUnits);
                        if (mutation.Kind == RackSiblingMutationKind.Discarded)
                            throw new InvalidOperationException("SIBLING_REDRAW_DISCARDED: " + mutation.Diagnostic);
                    }, () => SiblingRedrawTransaction.Post(document, deferredUnits));
                if (result.Report.Items.Count > 0)
                {
                    document.Editor.WriteMessage("\nRackCad: la vista se colocara con "
                        + result.Report.Items.Count + " pieza(s) de biblioteca faltante(s) o invalidas.");
                }
                if (result.Status == RackSingleViewPlacementStatus.Placed
                    || result.Status == RackSingleViewPlacementStatus.PlacedWithReport)
                {
                    return RackInsertPlacementResult.Applied(result.Report.Items.Count == 0
                        ? null
                        : "MISSING_LIBRARY_ITEMS=" + result.Report.Items.Count);
                }
                return result.Status == RackSingleViewPlacementStatus.Cancelled
                    ? RackInsertPlacementResult.Cancelled(result.Diagnostic)
                    : RackInsertPlacementResult.Failed(result.Diagnostic);
            }

            private RackSiblingUnitPreparation<SiblingRedrawUnit> PrepareMember(
                RackSiblingMember member, RackPreparedProductView<HeaderRunPlan> prepared)
            {
                var key = member.Fact.DefinitionKey;
                var id = snapshot.Definitions[key];
                if (member.Kind == RackSiblingMembershipKind.Erase)
                    return RackSiblingUnitPreparation<SiblingRedrawUnit>.Prepared(
                        new SiblingRedrawUnit(key, transaction => EraseDefinition(transaction, id)), snapshot.Layers[key]);

                if (!snapshot.Envelopes.TryGetValue(key, out var envelope))
                    return RackSiblingUnitPreparation<SiblingRedrawUnit>.Failed("ENVELOPE_UNREADABLE");
                var payloadEnvelope = ComposeEnvelope(envelope, rackId, rackName,
                    envelope.View, envelope.Section, authoredJson);
                var payload = new RackEmbedStore().Serialize(payloadEnvelope);
                var decoded = RackCommandSupport.DecodeView(envelope);
                if (!decoded.HasAddress)
                    return RackSiblingUnitPreparation<SiblingRedrawUnit>.Failed("VIEW_ADDRESS_UNREADABLE");

                PreparedViewRedraw redraw;
                string targetName;
                if (decoded.Address.Kind == DimensionViewKind.Planta)
                {
                    redraw = new SelectivePlantaDrawService().PrepareRedraw(document.Database, id, system, payload);
                    targetName = RackViewBaseName.LinkedPlanta(baseName);
                }
                else if (decoded.Address.Kind == DimensionViewKind.Lateral)
                {
                    var cut = new SelectiveLateralBuilder().Cortes(system, LateralHeaderDrawService.LoadCatalog())
                        .FirstOrDefault(item => item.PostIndex == decoded.Address.Variant.Index);
                    if (cut == null) return RackSiblingUnitPreparation<SiblingRedrawUnit>.Failed("VIEW_VARIANT_NOT_PRESENT");
                    redraw = new LateralHeaderDrawService().PrepareRedraw(document.Database, id, cut.Cabecera, payload, cut.Largueros);
                    targetName = RackViewBaseName.LinkedLateral(baseName, cut.PostIndex);
                }
                else
                {
                    var fondo = decoded.Address.Variant.Kind == RackViewVariantKind.Fondo ? decoded.Address.Variant.Index : 0;
                    var view = SelectiveDepthLayout.FondoSystemView(system, fondo);
                    view.Name = rackName;
                    redraw = new SelectiveFrontalDrawService().PrepareRedraw(document.Database, id, view, payload);
                    targetName = RackViewBaseName.LinkedSelectiveFrontal(baseName, fondo, SelectiveDepthLayout.Count(system));
                }

                IReadOnlyCollection<ObjectId> stale = Array.Empty<ObjectId>();
                return RackSiblingUnitPreparation<SiblingRedrawUnit>.Prepared(new SiblingRedrawUnit(key, transaction =>
                {
                    LateralHeaderDrawOutcome outcome;
                    if (decoded.Address.Kind == DimensionViewKind.Planta)
                        outcome = new SelectivePlantaDrawService().RedrawInTransaction(document.Database, transaction, redraw, out stale);
                    else if (decoded.Address.Kind == DimensionViewKind.Lateral)
                        outcome = new LateralHeaderDrawService().RedrawInTransaction(document.Database, transaction, redraw, out stale);
                    else
                        outcome = new SelectiveFrontalDrawService().RedrawInTransaction(document.Database, transaction, redraw, out stale);
                    return outcome == null ? RackSiblingMutationResult.Discarded(key, "REDRAW_RETURNED_NULL") : RackSiblingMutationResult.Committed();
                }, () =>
                {
                    SystemBlockWriter.PurgeAfterCommit(document.Database, stale);
                    RackBlockRenamer.SyncName(document, id, targetName);
                }), snapshot.Layers[key]);
            }

            private HeaderRunPlan BuildPlan(SelectiveRackSystem resolved, RackViewAddress address, RackCad.Application.Catalogs.RackCatalog catalog)
            {
                if (address.Kind == DimensionViewKind.Planta)
                    return new SelectivePlantaBuilder().BuildPlan(resolved, catalog);
                if (address.Kind == DimensionViewKind.Frontal)
                    return new SelectiveFrontalBuilder().BuildPlan(
                        SelectiveDepthLayout.FondoSystemView(resolved, address.Variant.Index), catalog);
                var cut = new SelectiveLateralBuilder().Cortes(resolved, catalog)
                    .First(item => item.PostIndex == address.Variant.Index);
                var parameters = LateralHeaderParametersFactory.FromConfiguration(cut.Cabecera);
                var layout = new LateralHeaderLayoutBuilder().Build(cut.Cabecera, parameters, catalog);
                return HeaderInstanceGrouper.Group(layout.Instances.Concat(cut.Largueros).ToList(), ViewName(resolved, address));
            }

            private string ViewName(SelectiveRackSystem resolved, RackViewAddress address)
            {
                if (address.Kind == DimensionViewKind.Planta) return RackViewBaseName.SelectivePlanta(resolved, rackName);
                if (address.Kind == DimensionViewKind.Lateral) return RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                    ?? "Selectivo lateral " + (address.Variant.Index + 1);
                return RackViewBaseName.LinkedSelectiveFrontal(baseName, address.Variant.Index, SelectiveDepthLayout.Count(resolved))
                    ?? RackViewBaseName.SelectiveFrontal(resolved, rackName);
            }

            private static RackSiblingMutationResult EraseDefinition(Transaction transaction, ObjectId definitionId)
            {
                var definition = transaction.GetObject(definitionId, OpenMode.ForWrite) as BlockTableRecord;
                if (definition == null) return RackSiblingMutationResult.Discarded(definitionId.ToString(), "DEFINITION_NOT_FOUND");
                foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
                    (transaction.GetObject(referenceId, OpenMode.ForWrite) as BlockReference)?.Erase();
                definition.Erase();
                return RackSiblingMutationResult.Committed();
            }

            private static RackEmbedDocument ComposeEnvelope(
                RackEmbedDocument source, string rackId, string rackName, string view, int section, string design)
                => RackEmbedComposer.Compose(source, RackEmbedDocument.KindSelective, rackId, rackName,
                    view, section, design);
        }
    }
}
