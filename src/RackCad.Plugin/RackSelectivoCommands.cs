using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.Selective;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>Selective-rack commands + their draw/edit/payload helpers (frontal / lateral corte / planta), plus alias.</summary>
    public sealed class RackSelectivoCommands
    {
        [CommandMethod("RS")] public void AliasRackSelectivo() => RackSelectivo();        // RACKSELECTIVO

        /// <summary>Opens the selective-rack window; draws it after the modal windows close.</summary>
        [CommandMethod("RACKSELECTIVO")]
        public void RackSelectivo()
        {
            try
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.SetDimensionStyles(RackCommandSupport.ReadDimensionStyleNames(AcApplication.DocumentManager.MdiActiveDocument));
                AcApplication.ShowModalWindow(window);

                if (window.InsertRequested)
                {
                    // I-05: warn once if the drawing is not in inches, before drawing the new view.
                    RackUnitsGuard.WarnIfNotInches(AcApplication.DocumentManager.MdiActiveDocument);
                    DrawSelectiveView(window.InsertView, window.SystemToInsert, window.DesignToInsert, window.RackId, window.RackName);
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        internal static void EditSelective(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            SelectivePalletDesignDocument saved;
            try
            {
                saved = new SelectivePalletDesignStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos del rack. " + ex.Message);
                return;
            }

            // AUTHORED vs EFFECTIVE (I-47 G12). The editor works on the value IN FORCE, so the register has to
            // be read before the window exists — and reading it can say no. A broken reference, a kind from the
            // future or a register this build cannot read do NOT fall back to the frozen literal: falling back
            // would change the geometry in silence. Repairing is an explicit operation and it is not this one.
            var registry = ReadProjectVariables(document, out var entries);
            var open = SelectiveEditorOpen.Resolve(saved, registry);

            if (!open.IsOpen)
            {
                editor.WriteMessage("\nRackCad: " + open.Error);
                return;
            }

            var window = new RackSelectiveWindow(canInsertInAutoCad: true);
            window.SetDimensionStyles(RackCommandSupport.ReadDimensionStyleNames(document)); // before LoadExisting so a saved style selects

            // I-48 G4C: las variables COMPATIBLES, ya ACREDITADAS. La ventana no decide que lo es, y no lee el
            // registro: si el registro no se puede usar como autoridad de identidad, no se ofrece ninguna.
            var options = LinkedPropertyOptions.ForProperty(
                ProjectPropertyIds.SelectiveVerticalClearance, registry);

            if (!options.IsUsable)
            {
                editor.WriteMessage("\nRackCad: " + options.Error);
                return;
            }

            window.SetProjectVariables(options.Options);
            window.LoadExisting(saved, open.Design, open.LinkedPropertyStates);
            AcApplication.ShowModalWindow(window);

            if (window.BindingIntent != null)
            {
                // Vincular no es dibujar: no pasa por el camino de redibujo del editor, pasa por la semantica
                // que ya sabe congelar el literal, materializar el efectivo y negarse ante un vinculo roto.
                ApplyBinding(document, editor, window.BindingIntent, registry, entries);
                return;
            }

            if (!window.InsertRequested)
            {
                return;
            }

            // I-05: a NEW linked view will be inserted below (see the "!UpdateOnly" branch); warn once if the drawing is
            // not in inches, BEFORE any block is (re)drawn. A pure update (UpdateOnly) redraws existing geometry in
            // place at its current scale and must NOT warn.
            if (!window.UpdateOnly)
            {
                RackUnitsGuard.WarnIfNotInches(document);
            }

            // Editing the SYSTEM redraws BOTH views. A rack is one frontal block + N lateral section blocks, all sharing
            // this GUID; find them and redefine each in place (every copy updates). Id comes from the embed (stable); the
            // client name may have been edited in the window.
            var design = window.DesignToInsert;
            var system = window.SystemToInsert;
            var id = string.IsNullOrEmpty(embed.Id) ? window.RackId : embed.Id;
            var name = string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName;
            system.Name = name; // the "Colocar nombre de rack" annotation draws this
            // Base name for syncing the block-definition names across views (null = keep each view's descriptive default).
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

            var blocks = RackCommandSupport.FindRackBlocks(document, id);
            var frontalBlocks = blocks.Where(b => !IsLateralView(b.Embed) && !RackCommandSupport.IsPlantaView(b.Embed)).ToList();
            var lateralBlocks = blocks.Where(b => IsLateralView(b.Embed)).OrderBy(b => b.Embed.Section).ToList();
            // Keep each planta block's own Embed (not just its id) so its per-view unknown metadata is preserved on redraw (I-11).
            var plantaBlocks = blocks.Where(b => RackCommandSupport.IsPlantaView(b.Embed)).ToList();

            // The clicked block might not carry the GUID scan (defensive): make sure the selected one is handled.
            if (frontalBlocks.Count == 0 && lateralBlocks.All(b => b.BlockId != blockId) && plantaBlocks.All(b => b.BlockId != blockId)
                && !IsLateralView(embed) && !RackCommandSupport.IsPlantaView(embed))
            {
                frontalBlocks.Add((blockId, embed));
            }

            // The design JSON is identical for every view-block (only the envelope's view/section differ), so
            // serialize the full design ONCE — not once per frontal + corte + planta.
            //
            // I-48 G4C: el documento final lo produce el RECONCILER de Application a partir del estado final
            // declarado por el editor. Ni esta capa ni la ventana escriben vinculos, y no se transporta una
            // secuencia Unlink -> SetLiteral -> Link: lo que el usuario expreso es donde TERMINO.
            var reconciled = LinkedPropertyReconciler.Reconcile(
                saved, design, window.LinkedPropertyFinalStates, registry, id, name);

            if (!reconciled.IsSuccess)
            {
                editor.WriteMessage("\nRackCad: " + reconciled.Error);
                return;
            }

            var designJson = new SelectivePalletDesignStore().Serialize(reconciled.Authored);

            // Each frontal block draws ONE fondo's face (its Section = fondo index; a legacy block with -1 = fondo 0).
            // Every loop below redraws with regen:false and the drawing regenerates ONCE at the end — a full
            // regeneration per view-block is pure waste on multi-view racks.
            var fondoCount = SelectiveDepthLayout.Count(system);

            // View-blocks whose fondo/corte no longer exists after a shrink: erased below instead of left as phantoms.
            // (User's choice: their peak geometry + GUID payload otherwise linger in every future Regen and
            // RACKEDITAR scan. A later re-grow re-inserts that fondo's frontal / that post's corte via the jig.)
            var staleViewBlocks = new System.Collections.Generic.List<ObjectId>();

            var updatedFrontal = 0;
            foreach (var fb in frontalBlocks)
            {
                var fondo = fb.Embed != null && fb.Embed.Section >= 0 ? fb.Embed.Section : 0;
                if (fondo >= fondoCount)
                {
                    staleViewBlocks.Add(fb.BlockId); // this fondo is gone — erase the phantom frontal
                    continue;
                }

                var fondoView = SelectiveDepthLayout.FondoSystemView(system, fondo);
                fondoView.Name = name;
                var payload = WrapSelectivePayload(designJson, id, name, RackEmbedDocument.ViewFrontal, fondo, fb.Embed);
                var r = new SelectiveFrontalDrawService().RedrawInPlace(document, fb.BlockId, fondoView, payload, regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, fb.BlockId, FrontalName(baseName, fondo, fondoCount));
                    updatedFrontal++;
                }
            }

            // Redraw each existing lateral section in place with the section's new geometry (matched by section index).
            var updatedLateral = 0;
            if (lateralBlocks.Count > 0)
            {
                var cortes = new SelectiveLateralBuilder().Cortes(system, LateralHeaderDrawService.LoadCatalog());
                var lateralService = new LateralHeaderDrawService();
                foreach (var lat in lateralBlocks)
                {
                    var corte = cortes.FirstOrDefault(c => c.PostIndex == lat.Embed.Section);
                    if (corte == null)
                    {
                        staleViewBlocks.Add(lat.BlockId); // this section is gone — erase the phantom lateral (see note above)
                        continue;
                    }

                    var payload = WrapSelectivePayload(designJson, id, name, RackEmbedDocument.ViewLateral, corte.PostIndex, lat.Embed);
                    var r = lateralService.RedrawInPlace(document, lat.BlockId, corte.Cabecera, payload, corte.Largueros, regen: false);
                    if (r != null && r.Success)
                    {
                        RackBlockRenamer.SyncName(document, lat.BlockId,
                            baseName == null ? null : baseName + " - lateral " + (corte.PostIndex + 1).ToString(CultureInfo.InvariantCulture));
                        updatedLateral++;
                    }
                }
            }

            // Redraw the planta block(s) in place (one block for the whole top view).
            var updatedPlanta = 0;
            foreach (var pb in plantaBlocks)
            {
                var payload = WrapSelectivePayload(designJson, id, name, RackEmbedDocument.ViewPlanta, source: pb.Embed);
                var r = new SelectivePlantaDrawService().RedrawInPlace(document, pb.BlockId, system, payload, regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, pb.BlockId, baseName == null ? null : baseName + " - planta");
                    updatedPlanta++;
                }
            }

            // Erase the phantom view-blocks a shrink left behind (fondos/cortes that no longer exist) — but ONLY when
            // at least one view-block SURVIVES the edit. The rack's GUID + embedded design live on these blocks, so
            // erasing the last one would destroy the rack irrecoverably (no surviving block to RACKEDITAR). When nothing
            // survives, keep the phantoms — the rack stays editable and the user can insert a fresh view.
            var survivors = frontalBlocks.Count + lateralBlocks.Count + plantaBlocks.Count - staleViewBlocks.Count;
            var erasedPhantoms = 0;
            if (staleViewBlocks.Count > 0 && survivors > 0)
            {
                erasedPhantoms = RackCommandSupport.EraseViewBlocks(document, staleViewBlocks);
            }
            else if (staleViewBlocks.Count > 0)
            {
                editor.WriteMessage("\nRackCad: las vistas del sistema ya no corresponden al diseno encogido, pero son las unicas del rack: "
                    + "se conservan para no perderlo. Inserta una vista valida (RACKEDITAR) y borra las viejas a mano si lo deseas.");
            }

            if (updatedFrontal + updatedLateral + updatedPlanta + erasedPhantoms > 0)
            {
                document.Editor.Regen(); // ONE regeneration refreshes every redefined (and drops every erased) view-block
            }

            // "Insertar": after refreshing the existing views above, place a NEW linked view-block (same GUID) of the
            // requested view via the normal insertion path (jig). DrawSelectiveView writes its own outcome. "Actualizar"
            // (UpdateOnly) inserts nothing — the refresh above is the whole action.
            if (!window.UpdateOnly)
            {
                // I-48 G4C.1: la vista NUEVA porta el MISMO authored reconciliado que las que ya existian, no
                // uno reconstruido desde el diseno efectivo. Un diseno de dominio no puede llevar vinculos, asi
                // que reconstruirlo dejaba la vista nueva sin vinculo y con el valor de la variable escrito como
                // literal del rack -- es decir, divergencia authored entre hermanas del mismo rack, que es el
                // estado que ninguna operacion puede resolver eligiendo una vista.
                //
                // A NEW view inserted during an edit inherits the initiating (picked) envelope's metadata (I-11).
                DrawSelectiveViewFromAuthored(window.InsertView, system, designJson, id, name, embed);
                return;
            }

            editor.WriteMessage(updatedFrontal + updatedLateral + updatedPlanta + erasedPhantoms > 0
                ? "\nRackCad: sistema actualizado; sus vistas se redibujaron (frontal x"
                    + updatedFrontal.ToString(CultureInfo.InvariantCulture) + ", lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + ")."
                    + (erasedPhantoms > 0 ? " Vistas obsoletas retiradas: x" + erasedPhantoms.ToString(CultureInfo.InvariantCulture) + "." : string.Empty)
                : "\nRackCad: no se pudo actualizar el rack.");
        }

        /// <summary>
        /// The drawing's project-variable register AND its sweep, read in ONE short transaction (I-47 G7/G8).
        /// It is a READ and it happens before the editor exists, so it owns its transaction rather than
        /// borrowing one. The sweep travels with it because a binding gesture has to reason over the rack's
        /// views, and reading it twice could read two different drawings.
        /// </summary>
        private static ProjectVariablesReadResult ReadProjectVariables(
            Document document, out System.Collections.Generic.IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            var swept = new System.Collections.Generic.List<ProjectVariableScanEntry>();

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var read = ProjectVariablesRegistry.Read(transaction, document.Database);

                foreach (var envelope in RackBlockFinder.ScanEnvelopes(
                             transaction, document.Database, includeReferenceCount: true))
                {
                    swept.Add(ProjectVariableScanProjection.Project(
                        envelope.DefinitionId.Handle.ToString(), envelope.Embed, envelope.DirectReferenceCount));
                }

                transaction.Commit();
                entries = swept;
                return read;
            }
        }

        /// <summary>
        /// Runs a binding gesture through the semantics that already exist (I-47 G17): G6 decides what it may
        /// do, G11 writes it. Nothing about freezing a literal, materialising an effective value or refusing a
        /// broken reference is decided here.
        /// </summary>
        private static void ApplyBinding(
            Document document,
            Editor editor,
            SelectiveBindingIntent intent,
            ProjectVariablesReadResult registry,
            System.Collections.Generic.IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            var preflight = SelectiveBindingIntentPreflight.Run(intent, registry.Document, entries);

            if (!preflight.IsSuccess)
            {
                editor.WriteMessage("\nRackCad: " + preflight.Error);
                return;
            }

            var execution = ProjectVariableMutationExecutor.Execute(document, preflight.Plan);

            editor.WriteMessage(execution.IsApplied
                ? "\nRackCad: vinculo actualizado; se redibujaron " + execution.ViewsRedrawn.ToString(
                      CultureInfo.InvariantCulture) + " vista(s) del rack."
                : "\nRackCad: " + (execution.Error ?? "no habia nada que aplicar."));
        }

        /// <summary>True when a view-block draws the LATERAL view (so it is a section of the system, not the frontal).</summary>
        private static bool IsLateralView(RackEmbedDocument embed) =>
            embed != null && string.Equals(embed.View, RackEmbedDocument.ViewLateral, System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Wraps an ALREADY-SERIALIZED authored document in the uniform embed envelope (kind + id + name + view
        /// + section + design JSON).
        ///
        /// <para>
        /// Since I-48 G4C.1 it takes the JSON and not a <see cref="SelectivePalletDesign"/>, and that is the
        /// whole point of the correction: the document a view carries is decided ONCE, upstream, and travels
        /// verbatim. Taking a domain design here meant every insertion re-derived the document, and a domain
        /// design cannot carry a binding — so the derived one silently dropped it and wrote the variable's value
        /// as if it were the rack's own literal.
        /// </para>
        /// </summary>
        private static string BuildSelectivePayload(
            string authoredJson, string id, string name, string view, int section = -1, RackEmbedDocument source = null)
            => WrapSelectivePayload(authoredJson, id, name, view, section, source);

        /// <summary>
        /// The full design serialized once; every view-block carries this SAME JSON (see <see cref="WrapSelectivePayload"/>).
        ///
        /// <para>
        /// With <paramref name="authored"/> the rack ALREADY exists and its document is UPDATED, preserving
        /// the schema version, the bindings, the frozen authored literal and any field a later build wrote.
        /// Without it — a fresh insert — there is nothing to preserve and the document is built from the
        /// design, exactly as before.
        /// </para>
        /// <para>
        /// The carrier is ONE per rack, not one per view: it is the document of the view the user picked. A
        /// per-view carrier would let each sibling keep its own divergence, turning a repairable defect into
        /// a permanent one — saving from a chosen view is precisely what reconciles them today.
        /// </para>
        /// </summary>
        private static string SerializeSelectiveDesign(
            SelectivePalletDesign design, string id, string name, SelectivePalletDesignDocument authored = null)
        {
            if (design == null)
            {
                return null;
            }

            var document = authored == null
                ? SelectivePalletDesignDocument.From(design, id, name)
                : authored.WithDesign(design, id, name);

            return new SelectivePalletDesignStore().Serialize(document);
        }

        /// <summary>Wraps an ALREADY-serialized design in the per-view embed envelope — multi-view redraws reuse one
        /// design JSON instead of re-serializing the whole design per view-block. When <paramref name="source"/> is the
        /// existing block's envelope (an edit redraw) or the initiating envelope (a new view), its unknown fields and a
        /// non-downgraded schema version are inherited via <see cref="RackEmbedComposer"/> (I-11); a fresh insert passes null.</summary>
        private static string WrapSelectivePayload(
            string designJson, string id, string name, string view, int section = -1, RackEmbedDocument source = null)
        {
            if (string.IsNullOrEmpty(designJson))
            {
                return null;
            }

            var embed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindSelective, id, name,
                string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewFrontal : view, section, designJson);
            return new RackEmbedStore().Serialize(embed);
        }

        /// <summary>
        /// Draws the selective in the requested VIEW for a rack that is being CREATED: frontal = one selective
        /// block; lateral = one cabecera "corte" per post.
        ///
        /// <para>
        /// A fresh rack has no previous carrier and no bindings to preserve, so building its document from the
        /// design is exactly right. An EDIT must not come through here — see
        /// <see cref="DrawSelectiveViewFromAuthored"/>.
        /// </para>
        /// </summary>
        internal static void DrawSelectiveView(
            string view, SelectiveRackSystem system, SelectivePalletDesign design, string id, string name, RackEmbedDocument source = null)
            => DrawSelectiveViewFromAuthored(
                view, system, SerializeSelectiveDesign(design, id, name), id, name, source);

        /// <summary>
        /// The same insertion, carrying an authored document that was ALREADY decided upstream (I-48 G4C.1).
        ///
        /// <para>
        /// This is the path an EDIT uses. The reconciler produced the rack's final document and it was
        /// serialized once for the views that already existed; a view inserted in that same operation carries
        /// that very string, so every sibling of the rack ends up with the same authored state and only the
        /// envelope — view, section, per-view metadata — differs.
        /// </para>
        /// <para>
        /// The GEOMETRY still comes from <paramref name="system"/>, which the editor already resolved. Nothing
        /// is resolved twice: effective belongs to the system, authored belongs to the serialized document, and
        /// this correction is exactly the separation of those two.
        /// </para>
        /// </summary>
        internal static void DrawSelectiveViewFromAuthored(
            string view, SelectiveRackSystem system, string authoredJson, string id, string name, RackEmbedDocument source = null)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || system == null)
            {
                return;
            }

            system.Name = name; // the "Colocar nombre de rack" annotation draws this

            if (view == RackEmbedDocument.ViewLateral)
            {
                InsertSelectiveLateralSection(document, system, authoredJson, id, name, source);
                return;
            }

            if (view == RackEmbedDocument.ViewPlanta)
            {
                var plantaPayload = BuildSelectivePayload(authoredJson, id, name, RackEmbedDocument.ViewPlanta, source: source);
                var plantaResult = new SelectivePlantaDrawService().DrawAndPlace(document, system, plantaPayload, name);
                document.Editor.WriteMessage("\n" + DescribeSelective(plantaResult));
                return;
            }

            InsertSelectiveFrontal(document, system, authoredJson, id, name, source);
        }

        /// <summary>
        /// Inserts ONE frontal face, chosen by fondo number (a doble-profundidad rack has a frontal per fondo — each
        /// back-to-back side its own elevation). Single-fondo racks skip the prompt (fondo 0). The block carries the
        /// SAME rack id + full design (View=frontal, Section=fondo), so RACKEDITAR on it reopens the whole system and
        /// redraws every view.
        /// </summary>
        private static void InsertSelectiveFrontal(
            Document document, SelectiveRackSystem system, string authoredJson, string id, string name, RackEmbedDocument source = null)
        {
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            var fondoCount = SelectiveDepthLayout.Count(system);

            var fondo = 0;
            if (fondoCount > 1)
            {
                // Sin acentos en los mensajes de linea de comandos (evita mojibake en consolas no-Unicode).
                var options = new PromptIntegerOptions("\nQue frontal insertar (numero de fondo)?")
                {
                    LowerLimit = 1,
                    UpperLimit = fondoCount,
                    DefaultValue = 1,
                    UseDefaultValue = true,
                    AllowNone = false
                };

                var pick = editor.GetInteger(options);
                if (pick.Status != PromptStatus.OK)
                {
                    return;
                }

                fondo = pick.Value - 1;
            }

            var fondoView = SelectiveDepthLayout.FondoSystemView(system, fondo);
            fondoView.Name = name;
            var payload = BuildSelectivePayload(authoredJson, id, name, RackEmbedDocument.ViewFrontal, fondo, source);
            var blockName = FrontalName(string.IsNullOrWhiteSpace(name) ? "Selectivo" : name.Trim(), fondo, fondoCount);
            var result = new SelectiveFrontalDrawService().DrawAndPlace(document, fondoView, payload, blockName);
            document.Editor.WriteMessage("\n" + DescribeSelective(result));
        }

        /// <summary>Block/definition name for a fondo's frontal: the base name, plus a "frente F{n}" suffix only when the rack has more than one fondo.</summary>
        private static string FrontalName(string baseName, int fondo, int fondoCount)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                return baseName;
            }

            return fondoCount > 1 ? baseName + " - frente F" + (fondo + 1).ToString(CultureInfo.InvariantCulture) : baseName;
        }

        /// <summary>
        /// Inserts ONE lateral "corte" (cross-section), chosen by post number, and jig-places it. The section carries
        /// the SAME rack id + full design (View=lateral, Section=i), so it is tied to the system: RACKEDITAR on it
        /// reopens the whole selective and redraws BOTH views. It is its own block (movable independently), but a view
        /// OF the system, not a loose cabecera. Called after inserting the frontal (via RACKEDITAR) so it links to it.
        /// </summary>
        private static void InsertSelectiveLateralSection(
            Document document, SelectiveRackSystem system, string authoredJson, string id, string name, RackEmbedDocument source = null)
        {
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            var catalog = LateralHeaderDrawService.LoadCatalog();

            var cortes = new SelectiveLateralBuilder().Cortes(system, catalog);
            if (cortes.Count == 0)
            {
                editor.WriteMessage("\nRackCad: no hay cortes laterales que dibujar.");
                return;
            }

            // Ask WHICH post's corte to insert (1-based, matching the frontal preview numbers). The lateral spans the
            // MASTER grid — a corner layout has more cortes than fondo 0's frentes — so bound the pick by the cortes.
            var postCount = 1;
            foreach (var c in cortes)
            {
                if (c.PostIndex + 1 > postCount) postCount = c.PostIndex + 1;
            }
            // Sin acentos: los mensajes de línea de comandos de AutoCAD evitan acentos en todo el plugin
            // (riesgo de mojibake en consolas no-Unicode); solo la UI WPF los lleva.
            var options = new PromptIntegerOptions("\nQue corte lateral insertar (numero de poste)?")
            {
                LowerLimit = 1,
                UpperLimit = postCount,
                DefaultValue = 1,
                UseDefaultValue = true,
                AllowNone = false
            };

            var pick = editor.GetInteger(options);
            if (pick.Status != PromptStatus.OK)
            {
                return;
            }

            var corte = cortes.FirstOrDefault(c => c.PostIndex == pick.Value - 1);
            if (corte == null)
            {
                editor.WriteMessage("\nRackCad: el poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " no tiene corte lateral.");
                return;
            }

            var baseName = string.IsNullOrWhiteSpace(name) ? "Selectivo" : name.Trim();
            var sectionName = baseName + " - lateral " + pick.Value.ToString(CultureInfo.InvariantCulture);
            var payload = BuildSelectivePayload(authoredJson, id, name, RackEmbedDocument.ViewLateral, corte.PostIndex, source);

            var result = new LateralHeaderDrawService().DrawAndPlace(document, corte.Cabecera, payload, sectionName, corte.Largueros);
            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: corte lateral del poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " insertado y ligado al sistema."
                : "\nRackCad: no se pudo insertar el corte lateral. " + (result?.ErrorMessage ?? string.Empty));
        }

        private static string DescribeSelective(HeaderPlacementResult result)
            => RackCommandSupport.DescribePlacement(result, "el selectivo", "selectivo insertado");
    }
}
