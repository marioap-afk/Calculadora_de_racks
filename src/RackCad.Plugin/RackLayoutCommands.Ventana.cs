using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// T5 al (I-64, prototype «almacen», questions 1 to 11 accepted): the door of the page «Almacén» of the RackCad window, for RACKLAYOUT and
    /// RACKRELLENAR without a modal. The page computes the grid and the fill with the same pure rules (<see cref="WarehouseLayoutRules"/>)
    /// and shows them before anything is written; these two commands only read and place.
    /// <list type="bullet">
    /// <item><b>RACKALMACENLEER</b> only reads (no undo marker, no history, the selection kept): the planta of the seed rack — the one
    /// selected in AutoCAD, else the one the page read before, else the only one (question 3) —, measured as RACKLAYOUT measures it
    /// (<c>SeedOf</c>), the count of its plantas (question 7), the site of the layer as RACKRELLENAR reads it (<c>ReadSiteFromLayer</c>)
    /// and the layers of the drawing (question 8).</item>
    /// <item><b>RACKALMACENCOLOCAR</b> reads the planta (and the site) AGAIN, places nothing if they changed since the page read them and
    /// gives the page the new reading (question 5); otherwise it plans with the same rules over what it read now and places with the SAME
    /// halves as RACKLAYOUT and RACKRELLENAR (<c>PlaceGrid</c>, <c>PlaceFill</c>: one transaction, one regen). It is one command, so one «U»
    /// undoes it all. Its answer carries the reading made after placing, in the same command (F-2 of the fourth batch).</item>
    /// </list>
    /// Without a request, or for a request of another drawing, they do nothing. RACKLAYOUT, RLY, RACKRELLENAR and RR keep their classic
    /// windows (question 10, until the Owner validates the page in AutoCAD).
    /// </summary>
    public sealed partial class RackLayoutCommands
    {
        private const string WarehouseOtherDrawingText =
            "La orden corrió en otro dibujo que el de la ventana: no se hizo nada. Vuelve a ese dibujo y pulsa «Volver a leer».";

        // The read of the page keeps no undo step and keeps the selection made before it (the seed is the planta selected, question 3).
        private const CommandFlags WarehouseReadFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoHistory;

        /// <summary>The read of the page «Almacén»: the planta of the seed, its site and the layers (no undo marker, no history).</summary>
        [CommandMethod("RACKALMACENLEER", WarehouseReadFlags)]
        public void RackAlmacenLeer()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingWarehouseRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKALMACENLEER la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            // The window counts what runs from here to the answer as its own read, never as a change of the drawing.
            door?.Started(pending.RequestId);
            var request = pending.Request;
            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerWarehouseRead(WarehouseReading.Failed(request.RequestId, request.SeedRackId, WarehouseOtherDrawingText));
                    return;
                }

                var reading = ReadWarehouse(document, request.RequestId, request.SeedRackId, request.Layer, request.PreferredSeedHandle,
                    useSelection: true, onlyPreferred: false, out _, out _);
                door?.AnswerWarehouseRead(reading);
                document.Editor.WriteMessage("\n" + WarehouseLayoutRules.ReadCommandLineText(reading));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackLayoutCommands.RackAlmacenLeer", ex);
                door?.AnswerWarehouseRead(WarehouseReading.Failed(request.RequestId, request.SeedRackId, "No se pudo leer el dibujo: " + ex.Message));
            }
        }

        /// <summary>«Colocar» of the page «Almacén»: reads again, compares, and places the grid or the fill (one command, one «U»).</summary>
        [CommandMethod("RACKALMACENCOLOCAR")]
        public void RackAlmacenColocar()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingWarehousePlace();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKALMACENCOLOCAR la usa la ventana RackCad: no hay nada pendiente de colocar.");
                return;
            }

            door?.Started(pending.RequestId);
            var request = pending.Request;
            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, false, WarehouseOtherDrawingText, 0, null));
                    return;
                }

                // The planta of the request, and only it (its handle): what the page showed is what is compared and placed.
                var fresh = ReadWarehouse(document, request.RequestId, request.SeedRackId, request.Layer, request.SeedHandle,
                    useSelection: false, onlyPreferred: true, out var seed, out var embed);
                var plan = WarehouseLayoutRules.PlanPlace(request, fresh);
                if (!plan.CanPlace)
                {
                    // The drawing changed: the page is given the new reading, made as the page reads (question 5).
                    var reading = plan.DrawingChanged
                        ? ReadWarehouse(document, request.RequestId, request.SeedRackId, request.Layer, request.SeedHandle,
                            useSelection: true, onlyPreferred: false, out _, out _)
                        : fresh;
                    door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, false, plan.Reason, 0, reading));
                    document.Editor.WriteMessage("\nRackCad: " + plan.Reason);
                    return;
                }

                // I-05: an insertion path warns once, before the first modification of the drawing.
                RackUnitsGuard.WarnIfNotInches(document);

                var grid = request.Mode == WarehouseLayoutMode.Grid;
                var independent = grid && request.Values.Independent;
                var placed = grid
                    ? PlaceGrid(document, plan.Assessment.Grid.Plan, seed, embed, independent)
                    : PlaceFill(document, plan.Assessment.Fill.Result, seed);

                // The reading after placing, in this same command: the page follows without a second command.
                var after = ReadWarehouse(document, request.RequestId, request.SeedRackId, request.Layer, request.SeedHandle,
                    useSelection: false, onlyPreferred: false, out _, out _);
                door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, true,
                    WarehouseLayoutRules.PlacedText(request.Mode, placed, plan.Assessment.Grid, independent), placed, after));
                document.Editor.WriteMessage("\n" + WarehouseLayoutRules.CommandLineText(request.Mode, placed, plan.Assessment.Grid, independent));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, false, "No se pudo colocar: " + ex.Message, 0, null));
            }
        }

        /// <summary>
        /// ONE reading for the page: the seed (<paramref name="onlyPreferred"/>: only the planta of that handle, for «Colocar»), its site and
        /// the layers. <paramref name="seed"/> and <paramref name="embed"/> are what <c>PlaceGrid</c> and <c>PlaceFill</c> need.
        /// </summary>
        private static WarehouseReading ReadWarehouse(
            Document document, long requestId, string rackId, string layer, string preferredHandle, bool useSelection, bool onlyPreferred,
            out PlantaSeed seed, out RackEmbedDocument embed)
        {
            var facts = ReadSeedFacts(document, rackId, preferredHandle, useSelection, onlyPreferred, out seed, out embed, out var plantas,
                out var name, out var problem);

            var layerName = string.IsNullOrWhiteSpace(layer) ? WarehouseLayoutRules.DefaultSiteLayer : layer.Trim();
            WarehouseSiteFacts site;
            if (ReadSiteFromLayer(document, layerName, 0.0, out var boundary, out var obstacles, out var ignored, out var siteProblem))
            {
                site = new WarehouseSiteFacts(layerName, null, boundary,
                    obstacles.Select(o => new WarehouseObstacleFact(o.X, o.Y, o.Width, o.Depth, o.Label)).ToList(), ignored);
            }
            else
            {
                site = new WarehouseSiteFacts(layerName, WarehouseLayoutRules.Sentence(siteProblem), null, Array.Empty<WarehouseObstacleFact>(), 0);
            }

            return new WarehouseReading(requestId, null, rackId, name, plantas, facts, problem, site, ReadLayerNames(document));
        }

        // The seed: the model-space references of the plantas of the rack; the one selected in AutoCAD, else the one of the handle, else the
        // only one. Measured with SeedOf, the measure of RACKLAYOUT.
        private static WarehouseSeedFacts ReadSeedFacts(
            Document document, string rackId, string preferredHandle, bool useSelection, bool onlyPreferred,
            out PlantaSeed seed, out RackEmbedDocument embed, out int plantaCount, out string name, out string problem)
        {
            seed = default;
            embed = null;
            plantaCount = 0;
            name = null;
            problem = null;

            var blocks = RackCommandSupport.FindRackBlocks(document, rackId);
            if (blocks.Count == 0)
            {
                problem = "Ese rack ya no está en el dibujo: pulsa «Volver a leer la lista».";
                return null;
            }

            name = blocks.Select(b => b.Embed?.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))?.Trim();
            var who = string.IsNullOrWhiteSpace(name) ? "Este rack" : name;
            var plantas = blocks.Where(IsPlantaViewBlock).ToList();
            if (plantas.Count == 0)
            {
                problem = who + " no tiene vista en planta: insértala con «Insertar vista…» (o RACKEDITAR) para usarlo como semilla.";
                return null;
            }

            var selected = useSelection ? SelectedIds(document.Editor) : new HashSet<ObjectId>();
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(document.Database);
                var references = new List<BlockReference>();
                foreach (var planta in plantas)
                {
                    var record = (BlockTableRecord)transaction.GetObject(planta.BlockId, OpenMode.ForRead);
                    foreach (ObjectId referenceId in record.GetBlockReferenceIds(directOnly: true, forceValidity: false))
                    {
                        if (referenceId.IsErased) continue;
                        if (transaction.GetObject(referenceId, OpenMode.ForRead) is BlockReference reference && reference.OwnerId == modelSpaceId)
                            references.Add(reference);
                    }
                }

                plantaCount = references.Count;
                BlockReference chosen = null;
                var wasSelected = false;
                var handle = (preferredHandle ?? string.Empty).Trim();
                if (references.Count == 0)
                {
                    problem = "La vista en planta no tiene copias insertadas, o no se pudo medir su tamaño.";
                }
                else if (onlyPreferred)
                {
                    chosen = references.FirstOrDefault(r => string.Equals(r.Handle.ToString(), handle, StringComparison.OrdinalIgnoreCase));
                    if (chosen == null) problem = "La planta semilla de " + who + " ya no está en el dibujo.";
                }
                else
                {
                    var picked = references.Where(r => selected.Contains(r.ObjectId)).ToList();
                    if (picked.Count == 1)
                    {
                        chosen = picked[0];
                        wasSelected = true;
                    }
                    else if (picked.Count > 1)
                    {
                        problem = "Hay " + picked.Count + " plantas de " + who + " seleccionadas en AutoCAD: deja seleccionada solo la que sirve de semilla y pulsa «Volver a leer».";
                    }
                    else
                    {
                        chosen = handle.Length == 0 ? null : references.FirstOrDefault(r => string.Equals(r.Handle.ToString(), handle, StringComparison.OrdinalIgnoreCase));
                        if (chosen == null && references.Count == 1) chosen = references[0];
                        if (chosen == null)
                            problem = "Hay " + references.Count + " plantas de " + who + " en el dibujo y ninguna está seleccionada: selecciona en AutoCAD la que sirve de semilla y pulsa «Volver a leer».";
                    }
                }

                WarehouseSeedFacts facts = null;
                if (chosen != null)
                {
                    try
                    {
                        seed = SeedOf(transaction, chosen);
                    }
                    catch (System.Exception ex)
                    {
                        RackLog.Exception("Medir la planta semilla para la ventana (RACKALMACENLEER)", ex);
                        seed = default;
                    }

                    if (seed.FootprintX > 0.0 && seed.FootprintY > 0.0)
                    {
                        var definitionId = seed.DefinitionId;
                        embed = plantas.FirstOrDefault(p => p.BlockId == definitionId).Embed ?? plantas[0].Embed;
                        var inner = new RackEmbedStore().Deserialize(seed.Payload);
                        var kind = embed?.Kind ?? inner?.Kind;
                        facts = new WarehouseSeedFacts(
                            rackId, name, kind, chosen.Handle.ToString(), definitionId.Handle.ToString(),
                            seed.OriginX, seed.OriginY, seed.FootprintX, seed.FootprintY, seed.OffsetX, seed.OffsetY,
                            seed.Rotation, seed.Scale.X, seed.Scale.Y, seed.Scale.Z,
                            WarehouseLayoutRules.HashOf(seed.Payload), inner != null,
                            KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out _), wasSelected);
                    }
                    else
                    {
                        problem = "La vista en planta no tiene copias insertadas, o no se pudo medir su tamaño.";
                    }
                }

                transaction.Commit();
                return facts;
            }
        }

        // The selection made in AutoCAD before the command (UsePickSet); empty when there is none.
        private static HashSet<ObjectId> SelectedIds(Editor editor)
        {
            var ids = new HashSet<ObjectId>();
            try
            {
                var result = editor.SelectImplied();
                if (result.Status == PromptStatus.OK && result.Value != null)
                {
                    foreach (var id in result.Value.GetObjectIds()) ids.Add(id);
                }
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("La selección previa para la ventana (RACKALMACENLEER)", ex);
            }

            return ids;
        }

        // The layers of the drawing for the list «Capa del sitio» (question 8): the drawing's own, by name.
        private static List<string> ReadLayerNames(Document document)
        {
            var names = new List<string>();
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var table = (LayerTable)transaction.GetObject(document.Database.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId id in table)
                {
                    if (id.IsErased) continue;
                    if (transaction.GetObject(id, OpenMode.ForRead) is LayerTableRecord layer && !layer.IsDependent) names.Add(layer.Name);
                }

                transaction.Commit();
            }

            names.Sort(StringComparer.CurrentCultureIgnoreCase);
            return names;
        }
    }
}
