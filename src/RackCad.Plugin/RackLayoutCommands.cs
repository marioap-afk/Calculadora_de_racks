using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Layout;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKLAYOUT / RACKRELLENAR: array or auto-fill a rack's PLANTA view into a warehouse grid; plus their aliases.</summary>
    public sealed partial class RackLayoutCommands
    {
        [CommandMethod("RLY")] public void AliasRackLayout() => RackLayout();              // RACKLAYOUT

        private const string LayoutLabelLayer = "RACKCAD_LAYOUT";

        /// <summary>
        /// Replicate a selected rack's top-down (planta) view into a warehouse grid: rows along the depth (separated by
        /// the pick aisle) × columns along the run, with per-cell labels (A1, B2…). Linked copies share one block
        /// definition (edit one = edit all; the BOM counts them as copies); independent copies each get their own GUID.
        /// </summary>
        [CommandMethod("RACKLAYOUT")]
        public void RackLayout()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;

                // I-05: RACKLAYOUT passes through the units guard BEFORE its functional prompts (before the first pick),
                // warning once if the drawing is not in inches.
                RackUnitsGuard.WarnIfNotInches(document);

                if (!RackCommandSupport.PickRackBlock(document, "\nSelecciona el rack a replicar en la rejilla: ", out var embed, out _))
                {
                    return;
                }

                if (embed == null || string.IsNullOrWhiteSpace(embed.Id))
                {
                    editor.WriteMessage("\nRackCad: ese bloque no tiene datos de rack.");
                    return;
                }

                // Refuse an unrecognized kind UP FRONT — before finding/measuring the planta or opening the layout
                // window — for BOTH linked and independent copies. RACKLAYOUT only lays out rack types we know how to
                // handle; independent copies also re-stamp identity per cell, which an unknown kind cannot do safely.
                // Case-insensitive, matching the restamp; the five embedded kinds always resolve.
                if (!KindHandlerDispatch.TryResolveIgnoreCase(editor, embed.Kind, out _))
                {
                    return;
                }

                // The warehouse layout is a top-down arrangement, so it works on the rack's PLANTA view (found by GUID —
                // the user may have clicked any view of the rack).
                var plantaBlocks = RackCommandSupport.FindRackBlocks(document, embed.Id).Where(IsPlantaViewBlock).ToList();
                if (plantaBlocks.Count == 0)
                {
                    editor.WriteMessage("\nRackCad: el rack no tiene vista en planta. Dibújala primero (RACKEDITAR) y reintenta.");
                    return;
                }

                if (!ReadPlantaSeed(document, plantaBlocks, out var seed))
                {
                    editor.WriteMessage("\nRackCad: la vista en planta no tiene copias insertadas, o no se pudo medir su tamaño.");
                    return;
                }

                var window = new RackWarehouseLayoutWindow(embed.Name, seed.FootprintX, seed.FootprintY);
                AcApplication.ShowModalWindow(window);
                if (window.Result == null)
                {
                    return; // cancelled
                }

                var choice = window.Result;
                if (choice.Independent && new RackEmbedStore().Deserialize(seed.Payload) == null)
                {
                    editor.WriteMessage("\nRackCad: la planta no tiene datos de rack embebidos; usa copias enlazadas.");
                    return;
                }

                var plan = WarehouseGridPlanner.Plan(
                    seed.FootprintX, seed.FootprintY,
                    choice.Rows, choice.Cols,
                    choice.AisleBetweenRows, choice.AisleBetweenCols,
                    seed.OriginX, seed.OriginY,
                    choice.BackToBack ? RowPairing.BackToBack : RowPairing.Single,
                    choice.BackGap);

                // Optional fit check: does the grid stay inside a building of the given size (its corner at the seed)?
                if (choice.BuildingWidth > 0.0 && choice.BuildingDepth > 0.0 && !ConfirmFits(editor, plan, seed, choice))
                {
                    return; // doesn't fit and the user declined to place it anyway
                }

                var placed = PlaceGrid(document, plan, seed, embed, choice.Independent);

                editor.WriteMessage(string.Format(CultureInfo.InvariantCulture,
                    "\nRackCad: layout colocado — {0} racks nuevos ({1} filas × {2} columnas, {3}{4}).",
                    placed, choice.Rows, choice.Cols,
                    choice.Independent ? "independientes" : "enlazados",
                    choice.BackToBack && choice.Rows > 1 ? ", espalda-con-espalda" : string.Empty));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        private static bool IsPlantaViewBlock((ObjectId BlockId, RackEmbedDocument Embed) block) => RackCommandSupport.IsPlantaView(block.Embed);

        /// <summary>Bounds fit check against a building envelope whose near corner is anchored at the seed rack's own
        /// corner (so the grid fills from there). If the grid doesn't fit, list the violations and ask whether to place
        /// it anyway. Returns true to proceed (fits, or the user confirmed), false to abort.</summary>
        private static bool ConfirmFits(Editor editor, WarehouseGridPlan plan, PlantaSeed seed, RackWarehouseLayoutWindow.LayoutResult choice)
        {
            var site = new WarehouseSite(
                choice.BuildingWidth, choice.BuildingDepth,
                originX: seed.OriginX + seed.OffsetX, originY: seed.OriginY + seed.OffsetY);

            var fit = WarehouseFitChecker.Check(plan, seed.FootprintX, seed.FootprintY, site, seed.OffsetX, seed.OffsetY);
            if (fit.Fits)
            {
                return true;
            }

            editor.WriteMessage("\nRackCad: la rejilla NO cabe en el edificio indicado:");
            foreach (var violation in fit.Violations)
            {
                editor.WriteMessage("\n  · " + violation.Message);
            }

            var options = new PromptKeywordOptions("\n¿Colocar la rejilla de todos modos?");
            options.Keywords.Add("Si");
            options.Keywords.Add("No");
            options.Keywords.Default = "No";
            options.AllowNone = true;
            var result = editor.GetKeywords(options);
            return result.Status == PromptStatus.OK && string.Equals(result.StringResult, "Si", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>The seed rack read from the drawing: its planta definition, insertion origin, footprint span, and
        /// the (origin → bbox-min) offset used to centre the labels.</summary>
        private struct PlantaSeed
        {
            public ObjectId DefinitionId;
            public string Payload;
            public double OriginX;
            public double OriginY;
            public double FootprintX;
            public double FootprintY;
            public double OffsetX; // bbox-min − origin (constant across identical copies)
            public double OffsetY;
            public double Rotation;   // the seed's own transform, copied onto every cell so a rotated/mirrored
            public Scale3d Scale;     // seed produces a matching grid (footprint AABB stays correct for 0/90/180/270°)
        }

        private static bool ReadPlantaSeed(Document document, List<(ObjectId BlockId, RackEmbedDocument Embed)> plantaBlocks, out PlantaSeed seed)
        {
            seed = default;
            try
            {
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var reference = RackBlockFinder.FindFirstModelSpaceReference(transaction, document.Database, plantaBlocks);
                    if (reference == null)
                    {
                        transaction.Commit();
                        return false;
                    }

                    var extents = reference.GeometricExtents; // throws on degenerate geometry → caught below
                    seed.DefinitionId = reference.BlockTableRecord;
                    seed.Payload = RackBlockData.Read(transaction, seed.DefinitionId);
                    seed.OriginX = reference.Position.X;
                    seed.OriginY = reference.Position.Y;
                    seed.FootprintX = extents.MaxPoint.X - extents.MinPoint.X;
                    seed.FootprintY = extents.MaxPoint.Y - extents.MinPoint.Y;
                    seed.OffsetX = extents.MinPoint.X - reference.Position.X;
                    seed.OffsetY = extents.MinPoint.Y - reference.Position.Y;
                    seed.Rotation = reference.Rotation;
                    seed.Scale = reference.ScaleFactors;

                    transaction.Commit();
                }
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("Leer rack semilla para RACKLAYOUT", ex);
                return false;
            }

            return seed.FootprintX > 0.0 && seed.FootprintY > 0.0;
        }

        private static int PlaceGrid(Document document, WarehouseGridPlan plan, PlantaSeed seed, RackEmbedDocument embed, bool independent)
        {
            var database = document.Database;
            var placed = 0;
            var textHeight = Math.Max(Math.Min(seed.FootprintX, seed.FootprintY) * 0.15, 3.0);

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                var labelLayer = LayerHelper.EnsureLayer(database, transaction, LayoutLabelLayer, 4); // cyan

                foreach (var cell in plan.Cells)
                {
                    if (!cell.IsSeed) // the seed rack already exists; only the OTHER cells get a new reference
                    {
                        var definitionId = seed.DefinitionId;
                        if (independent)
                        {
                            var copyName = ComposeCopyName(embed.Name, cell.Label);
                            var payload = RackEnvelopeRestamp.RestampEnvelope(seed.Payload, copyName);
                            definitionId = RackCloner.CloneDefinition(database, transaction, seed.DefinitionId, copyName, payload, embed.Name, copyName);
                        }

                        // Every copy keeps the seed's orientation/mirror. NOTE (back-to-back v1): the paired row is placed
                        // at the correct spacing (shared flue) but is NOT flipped to face the opposite aisle — a real
                        // back-to-back mirrors the pair. In planta that facing is usually cosmetic; a true mirror (a 180°
                        // turn + bbox-shift accounting) is a future refinement.
                        var reference = new BlockReference(new Point3d(cell.X, cell.Y, 0.0), definitionId)
                        {
                            Rotation = seed.Rotation,
                            ScaleFactors = seed.Scale
                        };
                        modelSpace.AppendEntity(reference);
                        transaction.AddNewlyCreatedDBObject(reference, true);
                        placed++;
                    }

                    AppendLabel(modelSpace, transaction, labelLayer, cell, seed, textHeight); // label every cell, seed too
                }

                transaction.Commit();
            }

            document.Editor.Regen();
            return placed;
        }

        /// <summary>A cell's label, a DBText centred on the rack's footprint, on the layout layer (togglable apart).</summary>
        private static void AppendLabel(BlockTableRecord modelSpace, Transaction transaction, ObjectId layerId, WarehouseCell cell, PlantaSeed seed, double textHeight)
        {
            var center = new Point3d(
                cell.X + seed.OffsetX + seed.FootprintX / 2.0,
                cell.Y + seed.OffsetY + seed.FootprintY / 2.0,
                0.0);

            var label = new DBText
            {
                Height = textHeight,
                TextString = cell.Label,
                LayerId = layerId,
                HorizontalMode = TextHorizontalMode.TextCenter,
                VerticalMode = TextVerticalMode.TextVerticalMid,
                Position = center,
                AlignmentPoint = center
            };

            modelSpace.AppendEntity(label);
            transaction.AddNewlyCreatedDBObject(label, true);
            label.AdjustAlignment(modelSpace.Database);
        }

        /// <summary>The per-cell client name: the rack's base name plus the cell label, e.g. "Rack A" → "Rack A B3".</summary>
        private static string ComposeCopyName(string baseName, string label)
            => (string.IsNullOrWhiteSpace(baseName) ? "Rack" : baseName.Trim()) + " " + label;
    }
}
