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
    /// Without a request, or for a request of another drawing, they do nothing. RACKLAYOUT and RACKRELLENAR keep their classic windows.
    /// <para>
    /// T6 rl (AL-S14; answer of the Owner, 2026-11-27: «La página «Almacén»»): the shortcuts RLY and RR open the page, RLY on «Rejilla» and RR
    /// on «Rellenar un área», with the rack selected in AutoCAD before them as the seed; without one, the page takes the seed it takes by hand.
    /// The gesture is the shortcut the user typed, so the command reads the drawing INSIDE itself, with the read of RACKALMACENLEER (no undo
    /// marker, the selection kept), and hands the reading to the page: no second command leaves the window (the lesson of the fourth batch,
    /// as RL and RB). They write nothing and do not warn about the units: the warning stays at «Colocar» (AL-S10).
    /// </para>
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

        // «Colocar» keeps the selection made before it (correction 4 of the review): when the drawing changed, the reading it hands back is
        // made as the page reads, and the seed is the planta selected in AutoCAD (AL-S2). It has an undo marker: one «U» undoes it.
        private const CommandFlags WarehousePlaceFlags = CommandFlags.UsePickSet | CommandFlags.Redraw;

        /// <summary>«Colocar» of the page «Almacén»: reads again, compares, and places the grid or the fill (one command, one «U»).</summary>
        [CommandMethod("RACKALMACENCOLOCAR", WarehousePlaceFlags)]
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
            var grid = request.Mode == WarehouseLayoutMode.Grid;
            WarehouseReading fresh;
            WarehousePlacePlan plan;
            int placed;
            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, false, WarehouseOtherDrawingText, 0, null));
                    return;
                }

                // The planta of the request, and only it (its handle): what the page showed is what is compared and placed. The grid does
                // not read the site (its mark does not use it) and neither reads the layers (correction 3 of the review).
                fresh = ReadWarehouse(document, request.RequestId, request.SeedRackId, request.Layer, request.SeedHandle,
                    useSelection: false, onlyPreferred: true, out var seed, out var embed, readSite: !grid, readLayers: false);
                plan = WarehouseLayoutRules.PlanPlace(request, fresh);
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

                // The halves of RACKLAYOUT and RACKRELLENAR, which commit their one transaction; the regeneration comes after, apart.
                placed = grid
                    ? PlaceGrid(document, plan.Assessment.Grid.Plan, seed, embed, request.Values.Independent, regen: false)
                    : PlaceFill(document, plan.Assessment.Fill.Result, seed, regen: false);
            }
            catch (System.Exception ex)
            {
                // Nothing was committed: a failure before or inside the transaction of the placement leaves the drawing as it was.
                RackCommandSupport.Report(ex);
                door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, false, "No se pudo colocar: " + ex.Message, 0, null));
                return;
            }

            // From here the racks ARE in the drawing (correction 1 of the review): whatever fails now, the answer says they were placed, so
            // the page never offers to place them again on top. The regeneration and the reading after it have their own try.
            var independent = grid && request.Values.Independent;
            var message = WarehouseLayoutRules.PlacedText(request.Mode, placed, plan.Assessment.Grid, independent);
            try
            {
                document.Editor.Regen();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackLayoutCommands.RackAlmacenColocar (regenerar tras colocar)", ex);
            }

            WarehouseReading after;
            try
            {
                after = AfterPlacing(document, request, fresh, placed, independent);
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackLayoutCommands.RackAlmacenColocar (leer tras colocar)", ex);
                after = WarehouseReading.Failed(request.RequestId, request.SeedRackId, "No se pudo volver a leer el dibujo después de colocar: " + ex.Message);
            }

            door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, true, message, placed, after));
            try
            {
                document.Editor.WriteMessage("\n" + WarehouseLayoutRules.CommandLineText(request.Mode, placed, plan.Assessment.Grid, independent));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackLayoutCommands.RackAlmacenColocar (línea de órdenes)", ex);
            }
        }

        /// <summary>
        /// The reading after placing, without a second pass over the block table (correction 3 of the review): the seed is the one read
        /// just before (placing does not touch it), with its plantas counted again by what was placed (a linked copy is one more planta of
        /// the rack; an independent one is a rack of its own); the site of the fill is the one read before, unless the references went to
        /// its layer (the current layer: then it is read again); the grid reads the site once, here; and the layers, which may now have
        /// RACKCAD_LAYOUT.
        /// </summary>
        private static WarehouseReading AfterPlacing(Document document, WarehousePlaceRequest request, WarehouseReading fresh, int placed, bool independent)
        {
            var layers = ReadLayerNames(document, out var current);
            var site = fresh.Site;
            if (site == null || string.Equals(current, site.Layer, StringComparison.OrdinalIgnoreCase)) site = ReadSiteFacts(document, request.Layer);
            return fresh with
            {
                PlantaCount = fresh.PlantaCount + (independent ? 0 : placed),
                Site = site,
                Layers = layers,
                CurrentLayer = current,
            };
        }

        /// <summary>
        /// ONE reading for the page: the seed (<paramref name="onlyPreferred"/>: only the planta of that handle, for «Colocar»), its site and
        /// the layers. <paramref name="seed"/> and <paramref name="embed"/> are what <c>PlaceGrid</c> and <c>PlaceFill</c> need. Without
        /// <paramref name="readSite"/> or <paramref name="readLayers"/>, those passes are not made (correction 3 of the review).
        /// </summary>
        private static WarehouseReading ReadWarehouse(
            Document document, long requestId, string rackId, string layer, string preferredHandle, bool useSelection, bool onlyPreferred,
            out PlantaSeed seed, out RackEmbedDocument embed, bool readSite = true, bool readLayers = true)
        {
            var facts = ReadSeedFacts(document, rackId, preferredHandle, useSelection, onlyPreferred, out seed, out embed, out var plantas,
                out var name, out var problem);
            var site = readSite ? ReadSiteFacts(document, layer) : null;
            string current = null;
            IReadOnlyList<string> layers = readLayers ? ReadLayerNames(document, out current) : Array.Empty<string>();
            return new WarehouseReading(requestId, null, rackId, name, plantas, facts, problem, site, layers, current);
        }

        // The site of a layer, as RACKRELLENAR reads it (ReadSiteFromLayer, its two passes over the model space).
        private static WarehouseSiteFacts ReadSiteFacts(Document document, string layer)
        {
            var layerName = string.IsNullOrWhiteSpace(layer) ? WarehouseLayoutRules.DefaultSiteLayer : layer.Trim();
            if (ReadSiteFromLayer(document, layerName, 0.0, out var boundary, out var obstacles, out var ignored, out var siteProblem))
            {
                return new WarehouseSiteFacts(layerName, null, boundary,
                    obstacles.Select(o => new WarehouseObstacleFact(o.X, o.Y, o.Width, o.Depth, o.Label)).ToList(), ignored);
            }

            return new WarehouseSiteFacts(layerName, WarehouseLayoutRules.Sentence(siteProblem), null, Array.Empty<WarehouseObstacleFact>(), 0);
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

        // The layers of the drawing for the list «Capa del sitio» (question 8): the drawing's own, by name; and the current one (CLAYER),
        // where the references are placed (correction 2 of the review).
        private static List<string> ReadLayerNames(Document document, out string current)
        {
            var names = new List<string>();
            current = null;
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(document.Database.Clayer, OpenMode.ForRead) is LayerTableRecord active) current = active.Name;
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

        // ------------------------------------------------------------------ T6 rl: RLY and RR open the page «Almacén» (AL-S14)

        // The read of the shortcut keeps no undo step and keeps the selection made before it (the seed, AL-S2), as RL and RB.
        private const CommandFlags WarehouseShortcutFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw;

        /// <summary>T6 rl: RLY opens «Almacén» › «Rejilla» of the window. RACKLAYOUT keeps its classic window.</summary>
        [CommandMethod("RLY", WarehouseShortcutFlags)] public void AliasRackLayout() => OpenWarehousePage(WarehouseLayoutMode.Grid);

        /// <summary>T6 rl: RR opens «Almacén» › «Rellenar un área» of the window. RACKRELLENAR keeps its classic window.</summary>
        [CommandMethod("RR", WarehouseShortcutFlags)] public void AliasRackRellenar() => OpenWarehousePage(WarehouseLayoutMode.Fill);

        // The rack selected before the shortcut designates the seed; the window shows the page and says what to read (during the call); the
        // command reads it here and hands the reading over. No command is handed to AutoCAD: the read is this command's own.
        private static void OpenWarehousePage(WarehouseLayoutMode mode)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null) return;

                var designated = WarehouseLayoutRules.DesignatedSeed(SelectedRacks(document), mode, out var note);
                Workspace.WorkspacePanelCommand.ShowRackCadWindow();
                var door = RackCadRackDoorPort.Current;
                var ask = new WarehousePageAsk(mode, designated, note);
                door?.AskWarehousePage(ask);
                if (door == null || !ask.Taken)
                {
                    document.Editor.WriteMessage("\nRackCad: no se pudo abrir la página «Almacén» de la ventana.");
                    return;
                }

                WarehouseReading reading = null;
                if (ask.Read != null)
                {
                    try
                    {
                        reading = ReadWarehouse(document, 0, ask.Read.SeedRackId, ask.Read.Layer, ask.Read.PreferredSeedHandle,
                            useSelection: true, onlyPreferred: false, out _, out _);
                    }
                    catch (System.Exception ex)
                    {
                        RackLog.Exception("RackLayoutCommands.OpenWarehousePage", ex);
                        reading = WarehouseReading.Failed(0, ask.Read.SeedRackId, "No se pudo leer el dibujo: " + ex.Message);
                    }

                    door.AnswerWarehouseShortcut(ask, reading);
                }

                document.Editor.WriteMessage("\n" + WarehouseLayoutRules.ShortcutCommandLineText(mode, reading, ask.Note ?? note));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        // The racks of the selection made in AutoCAD before the shortcut (UsePickSet): each reference of a rack block, by the data of its
        // definition, as RACKLAYOUT reads the rack it picks; with whether it is the planta. Empty when nothing of a rack is selected, or when
        // the selection cannot be read (then the page takes the seed it takes by hand).
        internal static List<WarehouseSelectedRack> SelectedRacks(Document document) // RACKPROPIEDADES uses it too (RackCadWindowShortcuts)
        {
            var racks = new List<WarehouseSelectedRack>();
            var ids = SelectedIds(document.Editor);
            if (ids.Count == 0) return racks;

            try
            {
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var store = new RackEmbedStore();
                    var embeds = new Dictionary<ObjectId, RackEmbedDocument>();
                    foreach (var id in ids)
                    {
                        if (id.IsNull || id.IsErased) continue;
                        if (!(transaction.GetObject(id, OpenMode.ForRead) is BlockReference reference)) continue;
                        var definitionId = reference.BlockTableRecord;
                        if (!embeds.TryGetValue(definitionId, out var embed))
                        {
                            embed = store.Deserialize(Systems.Shared.RackBlockData.Read(transaction, definitionId));
                            embeds[definitionId] = embed;
                        }

                        if (embed == null || string.IsNullOrWhiteSpace(embed.Id)) continue;
                        racks.Add(new WarehouseSelectedRack(embed.Id.Trim(), embed.Name, RackCommandSupport.IsPlantaView(embed)));
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("La selección previa para RLY / RR", ex);
                racks.Clear();
            }

            return racks;
        }
    }
}
