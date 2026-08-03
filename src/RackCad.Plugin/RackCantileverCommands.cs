using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Drawing.Cantilever;
using RackCad.Plugin.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Cantilever;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// Cantilever (I-37D) system command + its draw/edit/payload helpers, plus the RCT alias.
    ///
    /// It opens the pure Cantilever editor and draws whatever <see cref="CantileverInsertionRequest"/> the
    /// window's session produced. No geometry lives here: the window already resolved the line, and every outline
    /// comes from <see cref="CantileverViewPlanBuilder"/>. The RACKEDITAR multi-view round-trip is
    /// <see cref="EditCantilever"/>; <c>CantileverKindHandler</c> only forwards to it.
    /// </summary>
    public sealed class RackCantileverCommands
    {
        /// <summary>What the jig asks, so the prompt names the thing being placed and not a cabecera.</summary>
        private const string InsertionPrompt = "\nPunto de insercion de la vista Cantilever: ";

        [CommandMethod("RCT")]
        public void AliasRct() => RackCantilever(); // RACKCANTILEVER

        /// <summary>Opens the Cantilever editor and, if the user asked to insert, draws the requested view.</summary>
        [CommandMethod("RACKCANTILEVER")]
        public void RackCantilever()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null)
            {
                return;
            }

            try
            {
                var window = new RackCantileverWindow(canInsertInAutoCad: true);
                AcApplication.ShowModalWindow(window);

                // A stand-alone component travels through the same seam and is drawn here, after the modal
                // closed, because its point prompt needs the editor free.
                if (window.ComponentInsertion != null)
                {
                    DrawCantileverComponent(window.ComponentInsertion);
                    return;
                }

                if (!(window.InsertionRequest is CantileverInsertionRequest request))
                {
                    return; // cancelled / closed: do NOT modify the DWG
                }

                // I-05: warn once if the drawing is not in inches, right before the first block is drawn.
                RackUnitsGuard.WarnIfNotInches(document);
                DrawCantileverView(
                    request.View,
                    request.Section,
                    request.Line,
                    request.Design,
                    request.RackId,
                    request.RackName,
                    source: null,
                    innerSource: request.SourceProject);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>
        /// Builds the requested linked Cantilever view and runs its placement jig. An unknown view or an
        /// out-of-range station fails VISIBLY and draws nothing — it never falls back to another view or to
        /// station one.
        /// </summary>
        internal static void DrawCantileverView(
            string view,
            int section,
            CantileverLineAssembly line,
            CantileverLineDesign design,
            string id,
            string rackName,
            RackEmbedDocument source = null,
            RackProject innerSource = null)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || line == null || design == null)
            {
                return;
            }

            var editor = document.Editor;

            if (!TryViewKind(view, out var kind))
            {
                editor.WriteMessage("\nRackCad: vista Cantilever desconocida (" + (view ?? "null") + "); no se dibuja.");
                return;
            }

            if (kind == CantileverViewKind.Lateral && (section < 0 || section >= line.Stations.Count))
            {
                editor.WriteMessage("\nRackCad: la linea no tiene la estacion "
                                    + (section + 1).ToString(CultureInfo.InvariantCulture) + "; tiene "
                                    + line.Stations.Count.ToString(CultureInfo.InvariantCulture) + ". No se dibuja.");
                return;
            }

            if (!TryGeometryFactory(editor, out var factory))
            {
                return;
            }

            var plan = CantileverViewPlanBuilder.Build(line, kind, factory, section < 0 ? 0 : section);

            if (plan.IsEmpty)
            {
                editor.WriteMessage("\nRackCad: la vista Cantilever no contiene nada que dibujar.");
                return;
            }

            var payload = BuildCantileverPayload(design, id, rackName, view, section, source, innerSource);
            var baseName = string.IsNullOrWhiteSpace(rackName) ? null : rackName.Trim();

            try
            {
                ObjectId definitionId;
                string blockName;

                using (document.LockDocument())
                {
                    using (var transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        definitionId = CantileverViewMaterializer.CreateBlockDefinition(
                            document.Database, transaction, plan, ViewBlockName(baseName, kind, section), out blockName);

                        // The payload goes on the DEFINITION, like every other system: every reference and every
                        // copy of this view then shares the same rack data.
                        RackBlockData.Write(transaction, definitionId, payload);
                        transaction.Commit();
                    }
                }

                var placedId = BlockPlacement.PlaceDefinition(document, definitionId, InsertionPrompt);

                if (placedId.IsNull)
                {
                    editor.WriteMessage("\nRackCad: insercion cancelada; no se dejo nada en el dibujo.");
                    return;
                }

                SystemBlockWriter.ApplyRegen(document, regen: true);
                editor.WriteMessage("\nRackCad: vista " + kind.ToString().ToLowerInvariant()
                                    + " de la linea Cantilever insertada como bloque '" + blockName + "' ("
                                    + plan.Curves.Count.ToString(CultureInfo.InvariantCulture) + " contornos).");
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudo dibujar la vista Cantilever. " + ex.Message);
            }
        }

        /// <summary>
        /// RACKEDITAR round-trip for a Cantilever line: reopen the editor with the picked block's data, then redraw
        /// EVERY linked view in place (and optionally insert one more).
        ///
        /// It reuses the window/session as the sole recompute authority — it never re-resolves a line the window
        /// already produced — preserves the picked GUID (an edit never mints a new one), and runs the FULL
        /// preflight (envelope kind + GUID, inner project I-11, view descriptor) BEFORE touching any geometry.
        /// </summary>
        internal static void EditCantilever(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            RackProject project;

            try
            {
                project = new RackProjectStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos de la linea Cantilever. " + ex.Message);
                return;
            }

            if (project?.CantileverLineDesign == null)
            {
                editor.WriteMessage("\nRackCad: datos de linea Cantilever invalidos.");
                return;
            }

            var window = new RackCantileverWindow(canInsertInAutoCad: true);
            window.LoadExisting(project.CantileverLineDesign, embed.Id, embed.Name, project);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            // Use ONLY what the window/session produced; do NOT re-resolve a line the window already gave us.
            var design = window.DesignToInsert;
            var line = window.LineToInsert;

            if (design == null || line == null)
            {
                editor.WriteMessage("\nRackCad: la ventana Cantilever no entrego una linea valida; no se modifico nada.");
                return;
            }

            // Identity: keep the picked envelope's GUID; the window GUID is only a fallback. NEVER mint one on edit.
            var id = string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id;
            var name = string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName;
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

            var blocks = RackCommandSupport.FindRackBlocks(document, id);

            if (blocks.Count == 0)
            {
                blocks.Add((blockId, embed));
            }

            // --- PREFLIGHT: everything below runs BEFORE the first redefinition / rename / erase / regen ---

            foreach (var viewBlock in blocks)
            {
                if (viewBlock.Embed == null
                    || !string.Equals(viewBlock.Embed.Kind, RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(viewBlock.Embed.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de esta linea no es Cantilever o pertenece a otro sistema (posible corrupcion). No se modifico ningun bloque.");
                    return;
                }
            }

            // Inner-project preflight (I-11): an incompatible-MAJOR or wrong-kind inner design aborts the whole
            // edit before any geometry change.
            var preflight = RackCommandSupport.PreflightInnerSources(blocks, RackSystemKind.Cantilever, project);

            if (preflight.Aborted)
            {
                editor.WriteMessage("\nRackCad: " + preflight.ErrorMessage);
                return;
            }

            foreach (var viewBlock in blocks)
            {
                if (!IsValidCantileverDescriptor(viewBlock.Embed))
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de esta linea tiene un descriptor de vista invalido (posible corrupcion). No se modifico ningun bloque.");
                    return;
                }
            }

            if (!TryGeometryFactory(editor, out var factory))
            {
                return;
            }

            // I-05: a NEW linked view is inserted only in the "!UpdateOnly" branch; warn once BEFORE any block is
            // (re)drawn. A pure update must NOT warn.
            if (!window.UpdateOnly)
            {
                RackUnitsGuard.WarnIfNotInches(document);
            }

            // --- Multiview redraw. The line is NEVER recomputed here; only projected, once per view. ---
            var updated = new Dictionary<CantileverViewKind, int>
            {
                { CantileverViewKind.Frontal, 0 },
                { CantileverViewKind.Lateral, 0 },
                { CantileverViewKind.Planta, 0 }
            };

            var staleViewBlocks = new List<ObjectId>();

            foreach (var viewBlock in blocks)
            {
                if (!TryViewKind(viewBlock.Embed.View, out var kind))
                {
                    continue; // the descriptor preflight already rejected anything unknown
                }

                var station = kind == CantileverViewKind.Lateral ? viewBlock.Embed.Section : -1;

                // A lateral of a station the line no longer has becomes STALE. It is never redrawn at another
                // index: that would silently show a different station under the same block.
                if (kind == CantileverViewKind.Lateral && station >= line.Stations.Count)
                {
                    staleViewBlocks.Add(viewBlock.BlockId);
                    continue;
                }

                var plan = CantileverViewPlanBuilder.Build(line, kind, factory, station < 0 ? 0 : station);
                var payload = BuildCantileverPayload(
                    design, id, name, viewBlock.Embed.View, viewBlock.Embed.Section,
                    viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);

                try
                {
                    using (document.LockDocument())
                    using (var transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        CantileverViewMaterializer.RedefineBlock(
                            document.Database, transaction, viewBlock.BlockId, plan);
                        RackBlockData.Write(transaction, viewBlock.BlockId, payload);
                        transaction.Commit();
                    }
                }
                catch (System.Exception ex)
                {
                    editor.WriteMessage("\nRackCad: no se pudo redibujar una vista Cantilever. " + ex.Message);
                    continue;
                }

                RackBlockRenamer.SyncName(document, viewBlock.BlockId, ViewBlockName(baseName, kind, station));
                updated[kind] = updated[kind] + 1;
            }

            // Stale laterals: erase ONLY when some views survive (never delete a line's last remaining link).
            var survivors = blocks.Count - staleViewBlocks.Count;
            var erasedPhantoms = staleViewBlocks.Count > 0 && survivors > 0
                ? RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)
                : 0;

            if (staleViewBlocks.Count > 0 && survivors == 0)
            {
                editor.WriteMessage("\nRackCad: todas las vistas Cantilever quedaron obsoletas; no se elimino el ultimo vinculo de la linea.");
            }

            // SINGLE authority for "the drawing changed in place": erasing a stale lateral IS a change. Exactly one
            // regen for the whole batch; a NEW view inserted below runs its own.
            var changedInPlace =
                updated[CantileverViewKind.Frontal] + updated[CantileverViewKind.Lateral] +
                updated[CantileverViewKind.Planta] + erasedPhantoms > 0;

            if (changedInPlace)
            {
                document.Editor.Regen();
            }

            if (!window.UpdateOnly)
            {
                // The NEW view inherits the picked envelope AND the inner wrapper (I-11): same GUID, current name,
                // unknown envelope metadata + unknown/non-degraded inner-project version.
                DrawCantileverView(
                    window.InsertView, window.InsertSection, line, design, id, name,
                    source: embed, innerSource: project);
                return;
            }

            editor.WriteMessage(changedInPlace
                ? "\nRackCad: linea Cantilever actualizada; vistas redibujadas (frontal x"
                  + updated[CantileverViewKind.Frontal].ToString(CultureInfo.InvariantCulture) + ", lateral x"
                  + updated[CantileverViewKind.Lateral].ToString(CultureInfo.InvariantCulture) + ", planta x"
                  + updated[CantileverViewKind.Planta].ToString(CultureInfo.InvariantCulture)
                  + "; laterales obsoletas eliminadas x"
                  + erasedPhantoms.ToString(CultureInfo.InvariantCulture) + ")."
                : "\nRackCad: no se pudo actualizar la linea Cantilever.");
        }


        /// <summary>
        /// Draws ONE Cantilever component on its own, as a NON-EDITABLE block.
        ///
        /// It follows the <c>RACKSECCION</c> precedent exactly, and the differences from a line insertion are the
        /// point of it:
        ///
        /// <list type="bullet">
        /// <item>the point is asked FIRST, so a cancelled prompt creates nothing at all — there is no definition to
        /// clean up afterwards and no phantom block for any scan to find;</item>
        /// <item><b>no payload is written.</b> No <c>RackBlockData</c>, no envelope, no <c>KindCantilever</c>: this
        /// piece is not a rack, so <c>RACKLISTA</c> will not list it and <c>RACKEDITAR</c> will not offer to edit
        /// it. Promising a round-trip that does not exist would be worse than not offering the insertion;</item>
        /// <item>the block NAME carries the component's OWN id — never the line's — plus its kind, designation and
        /// view, which is what identifies the piece as drawn by RackCad;</item>
        /// <item>the views are laid out left to right from the picked point, so two of them do not land on top of
        /// each other.</item>
        /// </list>
        ///
        /// It materialises the plans the PREVIEW drew. There is no second projection here.
        /// </summary>
        internal static void DrawCantileverComponent(CantileverComponentInsertionRequest request)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || request == null)
            {
                return;
            }

            var editor = document.Editor;
            var options = new PromptPointOptions("\nPunto de insercion del componente Cantilever: ")
            {
                AllowNone = false
            };

            var pick = editor.GetPoint(options);

            if (pick.Status != PromptStatus.OK)
            {
                editor.WriteMessage("\nRackCad: insercion cancelada; no se dejo nada en el dibujo.");
                return;
            }

            // I-05: the plans are in inches and nothing converts them.
            RackUnitsGuard.WarnIfNotInches(document);

            try
            {
                var names = new List<string>();
                var offsetX = 0.0;

                using (document.LockDocument())
                {
                    using (var transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var plan in request.Views)
                        {
                            var definitionId = CantileverViewMaterializer.CreateBlockDefinitionNamed(
                                document.Database, transaction, plan, request.BlockName(plan), out var blockName);

                            CantileverViewMaterializer.InsertReference(
                                document.Database, transaction, definitionId,
                                new Point3d(pick.Value.X + offsetX, pick.Value.Y, pick.Value.Z));

                            names.Add(blockName);

                            // A gap of a tenth of the width, so the views read as separate drawings of one piece.
                            offsetX += plan.Bounds.Width * 1.1 + ViewGapInches;
                        }

                        transaction.Commit();
                    }

                    SystemBlockWriter.ApplyRegen(document, regen: true);
                }

                editor.WriteMessage("\nRackCad: " + request.Describe() + ". Bloques: " + string.Join(", ", names) +
                                    ". Es una pieza SUELTA: no es una linea Cantilever, no aparece en RACKLISTA y " +
                                    "RACKEDITAR no la abre.");
            }
            catch (System.Exception ex)
            {
                // Nothing was committed, so nothing partial reached the drawing.
                editor.WriteMessage("\nRackCad: no se pudo dibujar el componente Cantilever. " + ex.Message);
            }
        }

        /// <summary>The gap between two views of the same loose component, in inches.</summary>
        private const double ViewGapInches = 6.0;

        /// <summary>Wraps a Cantilever line in the uniform embed envelope; reuses the project store for the JSON.</summary>
        internal static string BuildCantileverPayload(
            CantileverLineDesign design,
            string id,
            string name,
            string view = RackEmbedDocument.ViewFrontal,
            int section = -1,
            RackEmbedDocument source = null,
            RackProject innerSource = null)
        {
            if (design == null)
            {
                return null;
            }

            // The inner Design of a Cantilever block is itself a RackProjectDocument — a boundary INDEPENDENT of
            // the envelope (I-11). innerSource is the ALREADY-RESOLVED source project (null for a fresh one, or the
            // library/initiating project); WithSourceMetadataFrom preserves its unknown fields + non-downgraded
            // version. The RESOLVED line is deliberately not persisted: the document carries the intention.
            var designJson = new RackProjectStore().Serialize(
                RackProject.ForCantilever(design).WithSourceMetadataFrom(innerSource));

            var embed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindCantilever, id, name,
                string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewFrontal : view, section, designJson);

            return new RackEmbedStore().Serialize(embed);
        }

        /// <summary>The envelope's view token as the Application view kind. False for anything else.</summary>
        internal static bool TryViewKind(string view, out CantileverViewKind kind)
        {
            if (string.Equals(view, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase))
            {
                kind = CantileverViewKind.Frontal;
                return true;
            }

            if (string.Equals(view, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase))
            {
                kind = CantileverViewKind.Lateral;
                return true;
            }

            if (string.Equals(view, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase))
            {
                kind = CantileverViewKind.Planta;
                return true;
            }

            kind = CantileverViewKind.Frontal;
            return false;
        }

        /// <summary>
        /// True when the envelope carries a well-formed Cantilever view descriptor: frontal and planta with section
        /// −1 (they are views of the whole LINE), lateral with a station index ≥ 0. Anything else is corrupt and
        /// aborts the edit — it is never coerced into another view.
        /// </summary>
        internal static bool IsValidCantileverDescriptor(RackEmbedDocument embed)
        {
            if (embed == null || !TryViewKind(embed.View, out var kind))
            {
                return false;
            }

            return kind == CantileverViewKind.Lateral
                ? embed.Section >= 0
                : embed.Section == -1;
        }

        /// <summary>The block name of one view, so a rename on edit says the same thing an insert said.</summary>
        private static string ViewBlockName(string baseName, CantileverViewKind kind, int station)
        {
            if (baseName == null)
            {
                return null;
            }

            switch (kind)
            {
                case CantileverViewKind.Lateral:
                    return baseName + " - lateral " + (station + 1).ToString(CultureInfo.InvariantCulture);
                case CantileverViewKind.Planta:
                    return baseName + " - planta";
                default:
                    return baseName + " - frontal";
            }
        }

        /// <summary>
        /// The section geometry factory, over the catalogue loaded FAIL CLOSED by the Plugin's single owner of that
        /// policy. A Cantilever view is projected from real section geometry, so an invalid catalogue means there
        /// is nothing trustworthy to draw.
        /// </summary>
        private static bool TryGeometryFactory(Editor editor, out StructuralSectionGeometryFactory factory)
        {
            factory = null;

            if (!StructuralSectionCatalogAccess.TryLoad(editor, out var catalogue))
            {
                return false;
            }

            factory = new StructuralSectionGeometryFactory(catalogue);
            return true;
        }
    }
}
