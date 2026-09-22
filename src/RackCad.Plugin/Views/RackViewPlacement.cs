using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Application.Views.Preparation;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Drawing.Cantilever;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Views
{
    /// <summary>AutoCAD adapters for one prepared ID17/ID18 view. ID19 never calls this permissive seam.</summary>
    internal static class RackViewPlacement
    {
        internal static RackSingleViewPlacementResult<ObjectId> PlaceSelective(Document document, RackPreparedProductView<HeaderRunPlan> product)
            => PlaceHeader(document, product, RackEmbedDocument.KindSelective);
        internal static RackSingleViewPlacementResult<ObjectId> PlaceSelective(
            Document document,
            RackPreparedProductView<HeaderRunPlan> product,
            Action<Transaction> beforeCommit,
            Action afterCommit)
            => PlaceHeader(document, product, RackEmbedDocument.KindSelective, beforeCommit, afterCommit);
        internal static RackSingleViewPlacementResult<ObjectId> PlaceDynamic(Document document, RackPreparedProductView<HeaderRunPlan> product)
            => PlaceHeader(document, product, RackEmbedDocument.KindDynamic);
        internal static RackSingleViewPlacementResult<ObjectId> PlacePushBack(Document document, RackPreparedProductView<HeaderRunPlan> product)
            => PlaceHeader(document, product, RackEmbedDocument.KindPushBack);
        internal static RackSingleViewPlacementResult<ObjectId> PlaceHeader(Document document, RackPreparedProductView<HeaderRunPlan> product)
            => PlaceHeader(document, product, RackEmbedDocument.KindCabecera);
        internal static RackSingleViewPlacementResult<ObjectId> PlaceFlowBed(Document document, RackPreparedProductView<HeaderRunPlan> product)
            => PlaceHeader(document, product, RackEmbedDocument.KindCama);
        internal static RackSingleViewPlacementResult<ObjectId> PlaceCantilever(Document document, RackPreparedProductView<CantileverViewPlan> product)
            => RackSingleViewPlacement.Place(product, new NoRequirements(),
                new CantileverMaterializer(document, RackEmbedDocument.KindCantilever));

        private static RackSingleViewPlacementResult<ObjectId> PlaceHeader(
            Document document, RackPreparedProductView<HeaderRunPlan> product, string kind,
            Action<Transaction> beforeCommit = null,
            Action afterCommit = null)
            => RackSingleViewPlacement.Place(product, new HeaderRequirements(document),
                new HeaderMaterializer(document, kind, beforeCommit, afterCommit));

        private sealed class HeaderRequirements : IRackSingleViewRequirementEvaluator<HeaderRunPlan>
        {
            private readonly Document document;
            internal HeaderRequirements(Document document) => this.document = document;

            public RackRequirementReport Evaluate(RackPreparedProductView<HeaderRunPlan> product)
            {
                var requirements = product.Prepared.BlockRequirements;
                if (requirements.Count == 0)
                    return RackSingleViewRequirementReporting.ForHeaderPlan(
                        product.Prepared.Payload, Array.Empty<LibraryBlockAvailabilityFact>(), false);

                LibraryBlockAvailabilityFlowResult observed;
                using (document.LockDocument())
                {
                    observed = LibraryBlockAvailabilityFlow.Observe(
                        requirements, true, new AutoCadLibraryBlockQuery(document.Database), new Importer(document.Database));
                }

                var unavailable = !File.Exists(BlockLibraryImporter.LibraryPath);
                return RackSingleViewRequirementReporting.ForHeaderPlan(
                    product.Prepared.Payload, observed.Facts, unavailable);
            }

            private sealed class Importer : ILibraryBlockImporter
            {
                private readonly Database database;
                internal Importer(Database database) => this.database = database;
                public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
                    => new LibraryBlockImportResult(true, BlockLibraryImporter.EnsureRequirements(database, requirements));
            }
        }

        private sealed class NoRequirements : IRackSingleViewRequirementEvaluator<CantileverViewPlan>
        {
            public RackRequirementReport Evaluate(RackPreparedProductView<CantileverViewPlan> product)
                => RackRequirementReport.Empty;
        }

        private sealed class HeaderMaterializer : IRackSingleViewMaterializer<HeaderRunPlan, ObjectId, ObjectId>
        {
            private readonly Document document;
            private readonly Action<Transaction> beforeCommit;
            private readonly Action afterCommit;
            internal HeaderMaterializer(Document document, string kind,
                Action<Transaction> beforeCommit, Action afterCommit)
            { this.document = document; ExpectedKind = kind; this.beforeCommit = beforeCommit; this.afterCommit = afterCommit; }
            public string ExpectedKind { get; }
            public bool CanPlace(RackPreparedProductView<HeaderRunPlan> product, out string diagnostic)
            { diagnostic = document == null ? "NO_ACTIVE_DOCUMENT" : null; return document != null; }
            public RackSingleViewDefinition<ObjectId> Create(RackPreparedProductView<HeaderRunPlan> product)
            {
                var payload = new RackEmbedStore().Serialize(product.Envelope);
                var result = SystemBlockWriter.CreatePreparedBlock(document, new LateralHeaderDrawer(),
                    product.Prepared.Payload, product.Prepared.BaseName, payload);
                return new RackSingleViewDefinition<ObjectId>(result.DefinitionId, result.BlockName);
            }
            public RackSingleViewReference<ObjectId> Place(RackSingleViewDefinition<ObjectId> definition)
            {
                var id = beforeCommit == null
                    ? BlockPlacement.PlaceDefinitionWithoutCleanup(document, definition.Handle)
                    : BlockPlacement.PlaceDefinitionWithoutCleanup(document, definition.Handle, beforeCommit);
                return id.IsNull ? RackSingleViewReference<ObjectId>.Cancelled() : RackSingleViewReference<ObjectId>.Placed(id);
            }
            public RackSingleViewCleanupResult Cleanup(RackSingleViewDefinition<ObjectId> definition)
                => BlockPlacement.TryCleanupDefinition(document, definition.Handle);
            public void Complete(RackPreparedProductView<HeaderRunPlan> product, ObjectId reference)
            {
                if (afterCommit == null) SystemBlockWriter.ApplyRegen(document, true);
                else afterCommit();
            }
        }

        private sealed class CantileverMaterializer : IRackSingleViewMaterializer<CantileverViewPlan, ObjectId, ObjectId>
        {
            private readonly Document document;
            internal CantileverMaterializer(Document document, string kind) { this.document = document; ExpectedKind = kind; }
            public string ExpectedKind { get; }
            public bool CanPlace(RackPreparedProductView<CantileverViewPlan> product, out string diagnostic)
            { diagnostic = document == null ? "NO_ACTIVE_DOCUMENT" : null; return document != null; }
            public RackSingleViewDefinition<ObjectId> Create(RackPreparedProductView<CantileverViewPlan> product)
            {
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var id = CantileverViewMaterializer.CreateBlockDefinitionNamed(document.Database, transaction,
                        product.Prepared.Payload, product.Prepared.BaseName, out var name);
                    RackBlockData.Write(transaction, id, new RackEmbedStore().Serialize(product.Envelope));
                    transaction.Commit();
                    return new RackSingleViewDefinition<ObjectId>(id, name);
                }
            }
            public RackSingleViewReference<ObjectId> Place(RackSingleViewDefinition<ObjectId> definition)
            {
                var id = BlockPlacement.PlaceDefinitionWithoutCleanup(document, definition.Handle);
                return id.IsNull ? RackSingleViewReference<ObjectId>.Cancelled() : RackSingleViewReference<ObjectId>.Placed(id);
            }
            public RackSingleViewCleanupResult Cleanup(RackSingleViewDefinition<ObjectId> definition)
                => BlockPlacement.TryCleanupDefinition(document, definition.Handle);
            public void Complete(RackPreparedProductView<CantileverViewPlan> product, ObjectId reference)
                => SystemBlockWriter.ApplyRegen(document, true);
        }
    }
}
