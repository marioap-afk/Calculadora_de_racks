using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.Persistence;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// Cross-area helpers shared by the RackCad command classes (I-09 F4): error reporting, the rack-block picker,
    /// the GUID scan, the planta-view test, the dimension-style list, the common insert-outcome summary and the
    /// phantom view-block eraser. Extracted verbatim from the former single <c>RackFrameCommands</c> partial so the
    /// per-area command classes consume them without a shared partial. Behavior is unchanged.
    /// </summary>
    internal static class RackCommandSupport
    {
        public static void Report(System.Exception ex)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            document?.Editor.WriteMessage("\nRackCad error: " + ex.Message);
        }

        /// <summary>
        /// Read the source project a dynamic/cabecera view-block carries in its inner <c>Design</c> (a RackProjectDocument),
        /// so a redraw can preserve THAT block's own wrapper metadata (unknown fields + non-downgraded schema version, I-11).
        /// Falls back to the <paramref name="initiating"/> block's project ONLY on a benign failure (malformed/legacy/empty);
        /// an incompatible-MAJOR inner design returns null so it is NOT hidden behind the (lower-major) initiating metadata.
        /// </summary>
        public static RackProject ReadInnerSourceProject(string designJson, RackProject initiating)
        {
            if (new RackProjectStore().TryDeserialize(designJson, out var source, out var incompatibleMajor))
            {
                return source;
            }

            return incompatibleMajor ? null : initiating;
        }

        /// <summary>
        /// Prompt the user to pick a rack block and read its embedded payload from the DEFINITION. Returns false only
        /// when the user cancelled the selection; a picked-but-non-rack block returns true with a null <paramref
        /// name="embed"/> so the caller can report it. Shared by RACKEDITAR, RACKLAYOUT and RACKRELLENAR.
        /// </summary>
        public static bool PickRackBlock(Document document, string prompt, out RackEmbedDocument embed, out ObjectId blockId)
        {
            embed = null;
            blockId = ObjectId.Null;

            var options = new PromptEntityOptions(prompt);
            options.SetRejectMessage("\nEse objeto no es un rack.");
            options.AddAllowedClass(typeof(BlockReference), exactMatch: false);

            var selection = document.Editor.GetEntity(options);
            if (selection.Status != PromptStatus.OK)
            {
                return false;
            }

            var picked = InDocumentTransaction.Run(document, transaction =>
            {
                var reference = (BlockReference)transaction.GetObject(selection.ObjectId, OpenMode.ForRead);
                var definitionId = reference.BlockTableRecord;
                return (BlockId: definitionId, Json: RackBlockData.Read(transaction, definitionId));
            });

            blockId = picked.BlockId;
            embed = new RackEmbedStore().Deserialize(picked.Json);
            return true;
        }

        /// <summary>True when a cabecera view-block draws the PLANTA view (so it is the top view, not the lateral).</summary>
        public static bool IsPlantaView(RackEmbedDocument embed) =>
            embed != null && string.Equals(embed.View, RackEmbedDocument.ViewPlanta, System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Every rack block DEFINITION in the drawing whose embedded payload has the given rack id — i.e. all the
        /// view-blocks (frontal + lateral sections) of the same rack, so an edit can redraw them together.
        /// </summary>
        public static System.Collections.Generic.List<(ObjectId BlockId, RackEmbedDocument Embed)> FindRackBlocks(Document document, string rackId)
        {
            var results = new System.Collections.Generic.List<(ObjectId, RackEmbedDocument)>();
            if (document == null || string.IsNullOrEmpty(rackId))
            {
                return results;
            }

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                // FindRackBlocks keeps its own filter: match by GUID and tolerate a missing Kind (unlike RACKLISTA /
                // RACKBOMTOTAL). It needs no reference count, so the scan skips it.
                foreach (var envelope in RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: false))
                {
                    if (envelope.Embed != null && string.Equals(envelope.Embed.Id, rackId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add((envelope.DefinitionId, envelope.Embed));
                    }
                }

                transaction.Commit();
            }

            return results;
        }

        /// <summary>The names of the dimension styles defined in the drawing (for the cotas style combo). Sorted,
        /// best-effort — a read failure just yields an empty list so the editor falls back to "(Automático)".</summary>
        public static System.Collections.Generic.List<string> ReadDimensionStyleNames(Document document)
        {
            var names = new System.Collections.Generic.List<string>();
            if (document == null)
            {
                return names;
            }

            try
            {
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var table = (DimStyleTable)transaction.GetObject(document.Database.DimStyleTableId, OpenMode.ForRead);
                    foreach (ObjectId id in table)
                    {
                        if (transaction.GetObject(id, OpenMode.ForRead) is DimStyleTableRecord record && !record.IsErased)
                        {
                            names.Add(record.Name);
                        }
                    }

                    transaction.Commit();
                }
            }
            catch
            {
                // Best effort: no styles listed → the editor uses "(Automático)".
            }

            names.Sort(System.StringComparer.OrdinalIgnoreCase);
            return names;
        }

        /// <summary>The ONE insert-outcome summary (failure / canceled jig / inserted + missing blocks) — the per-kind
        /// Describe* methods only supply the noun phrases. Sin acentos: mensajes de linea de comandos de AutoCAD.</summary>
        public static string DescribePlacement(HeaderPlacementResult result, string failNoun, string insertedPhrase)
        {
            if (!result.Success)
            {
                return "RackCad: no se pudo dibujar " + failNoun + ". " + result.ErrorMessage;
            }

            if (!result.Placed)
            {
                return "RackCad: bloque '" + result.BlockName + "' creado, pero la insercion se cancelo.";
            }

            var summary = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "RackCad: {0} como bloque '{1}'. {2} piezas.",
                insertedPhrase,
                result.BlockName,
                result.Outcome.InsertedCount);

            if (result.HasMissingBlocks)
            {
                summary += "\nBloques no definidos en el dibujo (omitidos): " + string.Join(", ", result.MissingBlocks);
            }

            return summary;
        }

        /// <summary>
        /// Erase the view-block DEFINITIONS listed (a shrink's phantom fondos/cortes) together with their model-space
        /// references (the copies) AND the nested ARRAY defs each one owned (SEL_FRONTAL_*), which erasing the owner only
        /// dereferences. Its own locked transaction — like <see cref="LateralHeaderDrawService"/>'s cancelled-insert
        /// cleanup — so it composes with the per-view redraw transactions above; the nested defs are purged post-commit
        /// via <see cref="LateralHeaderDrawer.PurgeUnreferenced"/> (Database.Purge keeps any still shared, e.g. catalog
        /// pieces). Best effort: a lingering phantom is preferable to failing the edit. Returns how many defs were erased.
        /// </summary>
        public static int EraseViewBlocks(Document document, System.Collections.Generic.IReadOnlyList<ObjectId> definitionIds)
        {
            if (document == null || definitionIds == null || definitionIds.Count == 0)
            {
                return 0;
            }

            var erased = 0;
            var orphanedNestedDefs = new System.Collections.Generic.HashSet<ObjectId>();
            try
            {
                using (document.LockDocument())
                {
                    using (var transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var definitionId in definitionIds)
                        {
                            if (definitionId.IsNull || definitionId.IsErased)
                            {
                                continue;
                            }

                            if (!(transaction.GetObject(definitionId, OpenMode.ForRead) is BlockTableRecord definition) || definition.IsLayout)
                            {
                                continue; // never touch model/paper space
                            }

                            // Capture the defs this view-block nests (its ARRAY groups + catalog pieces) BEFORE erasing it,
                            // so the now-orphan group defs can be purged afterwards (Database.Purge spares the still-shared).
                            foreach (ObjectId childId in definition)
                            {
                                if (transaction.GetObject(childId, OpenMode.ForRead) is BlockReference nested && !nested.BlockTableRecord.IsNull)
                                {
                                    orphanedNestedDefs.Add(nested.BlockTableRecord);
                                }
                            }

                            // Erase every model-space reference (the rack's copies of this view) first, then the owner def.
                            foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
                            {
                                if (transaction.GetObject(referenceId, OpenMode.ForWrite) is BlockReference reference)
                                {
                                    reference.Erase();
                                }
                            }

                            definition.UpgradeOpen();
                            definition.Erase();
                            erased++;
                        }

                        transaction.Commit();
                    }

                    // Post-commit: the owner defs are gone, so their nested ARRAY defs are unreferenced — purge them.
                    // Database.Purge filters out anything still referenced (catalog pieces other views use survive).
                    LateralHeaderDrawer.PurgeUnreferenced(document.Database, orphanedNestedDefs);
                }
            }
            catch (System.Exception ex)
            {
                document.Editor.WriteMessage("\nRackCad: no se pudieron retirar algunas vistas obsoletas. " + ex.Message);
            }

            return erased;
        }
    }
}
