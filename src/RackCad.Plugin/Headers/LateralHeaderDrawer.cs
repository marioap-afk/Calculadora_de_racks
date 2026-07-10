using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application;
using RackCad.Application.Headers;
using RackCad.Application.Systems;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Thin AutoCAD adapter: turns a pure plan into AutoCAD block definitions whose sub-entities are the
    /// pieces. All the geometry/parameters live in the Application layer (testable on any OS); this class only
    /// touches the AutoCAD API, so it compiles only inside the Plugin.
    ///
    /// Each piece is appended at its plan origin, rotated, optionally mirrored (X scale -1), with its
    /// dynamic-block parameters (LONGITUD) set by name. For a whole dynamic system, each distinct header is a
    /// nested block reused at every position; separators/posts are appended directly. The caller jig-inserts a
    /// reference to the resulting block.
    /// </summary>
    public sealed class LateralHeaderDrawer
    {
        /// <summary>
        /// Build a block definition named <paramref name="blockName"/> (uniquified if it already exists)
        /// containing every piece of the plan that the drawing actually defines.
        /// </summary>
        public LateralHeaderBlockResult CreateHeaderBlock(
            Database db, Transaction tr, LateralHeaderLayout layout, string blockName)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var definition = NewBlock(blockTable, tr, blockName, out var uniqueName, out var definitionId);

            var missing = new List<HeaderBlockInstance>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inserted = 0;

            foreach (var instance in layout.Instances)
            {
                if (AppendInstance(blockTable, definition, tr, instance, missing, seen))
                {
                    inserted++;
                }
            }

            return new LateralHeaderBlockResult(definitionId, uniqueName, new LateralHeaderDrawOutcome(layout, inserted, missing));
        }

        /// <summary>
        /// Build a dynamic-system block: each distinct header becomes one nested block definition (reused at
        /// every run position), and separators/derived posts are appended directly. Returns the system block.
        /// </summary>
        public LateralHeaderBlockResult CreateSystemBlock(
            Database db, Transaction tr, DynamicSystemPlan plan, string systemBlockName)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var missing = new List<HeaderBlockInstance>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inserted = 0;

            // 1. One block definition per distinct header.
            var headerDefs = new List<KeyValuePair<ObjectId, HeaderGroup>>();
            foreach (var group in plan.Headers)
            {
                var headerDef = NewBlock(blockTable, tr, group.Name, out _, out var headerId);

                foreach (var instance in group.Instances)
                {
                    if (AppendInstance(blockTable, headerDef, tr, instance, missing, seen))
                    {
                        inserted++;
                    }
                }

                headerDefs.Add(new KeyValuePair<ObjectId, HeaderGroup>(headerId, group));
            }

            // 2. The system block: references to each header at its run positions + the loose pieces.
            var systemDef = NewBlock(blockTable, tr, systemBlockName, out var systemName, out var systemId);

            foreach (var pair in headerDefs)
            {
                foreach (var placement in pair.Value.Placements)
                {
                    var headerRef = new BlockReference(new Point3d(placement.InsertionX, 0.0, 0.0), pair.Key)
                    {
                        ScaleFactors = placement.Mirrored ? new Scale3d(-1.0, 1.0, 1.0) : new Scale3d(1.0)
                    };
                    systemDef.AppendEntity(headerRef);
                    tr.AddNewlyCreatedDBObject(headerRef, true);
                    inserted++;
                }
            }

            foreach (var instance in plan.LooseInstances)
            {
                if (AppendInstance(blockTable, systemDef, tr, instance, missing, seen))
                {
                    inserted++;
                }
            }

            var outcome = new LateralHeaderDrawOutcome(
                new LateralHeaderLayout(new List<HeaderBlockInstance>(), 0.0, 0, 0, 0.0), inserted, missing);
            return new LateralHeaderBlockResult(systemId, systemName, outcome);
        }

        /// <summary>
        /// Redefine an existing system block IN PLACE: erase its contents and repopulate them from the plan, so
        /// every reference to it (all the copies of that rack) updates on the next regen. Keeps the same block id
        /// and name. Selective frontal is all-loose; header groups are handled too for future reuse.
        /// </summary>
        public LateralHeaderDrawOutcome RedefineSystemBlock(Database db, Transaction tr, ObjectId blockId, DynamicSystemPlan plan)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var systemDef = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForWrite);

            // Clear the current contents (collect first, then erase — don't erase while enumerating).
            var existing = new List<ObjectId>();
            foreach (ObjectId id in systemDef)
            {
                existing.Add(id);
            }

            foreach (var id in existing)
            {
                ((Entity)tr.GetObject(id, OpenMode.ForWrite)).Erase();
            }

            var missing = new List<HeaderBlockInstance>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inserted = 0;

            foreach (var group in plan.Headers)
            {
                var headerDef = NewBlock(blockTable, tr, group.Name, out _, out var headerId);

                foreach (var instance in group.Instances)
                {
                    if (AppendInstance(blockTable, headerDef, tr, instance, missing, seen))
                    {
                        inserted++;
                    }
                }

                foreach (var placement in group.Placements)
                {
                    var headerRef = new BlockReference(new Point3d(placement.InsertionX, 0.0, 0.0), headerId)
                    {
                        ScaleFactors = placement.Mirrored ? new Scale3d(-1.0, 1.0, 1.0) : new Scale3d(1.0)
                    };
                    systemDef.AppendEntity(headerRef);
                    tr.AddNewlyCreatedDBObject(headerRef, true);
                    inserted++;
                }
            }

            foreach (var instance in plan.LooseInstances)
            {
                if (AppendInstance(blockTable, systemDef, tr, instance, missing, seen))
                {
                    inserted++;
                }
            }

            // A redefined definition keeps its references' CACHED geometric extents from the old size. When the
            // rack grows, the new region draws but AutoCAD still tests selection against the old (smaller) bounding
            // box, so a window/crossing over the grown part selects nothing. Force each reference to recompute its
            // graphics AND extents; combined with the caller's Regen this makes the whole redrawn rack selectable.
            foreach (ObjectId referenceId in systemDef.GetBlockReferenceIds(directOnly: true, forceValidity: true))
            {
                var reference = (BlockReference)tr.GetObject(referenceId, OpenMode.ForWrite);
                reference.RecordGraphicsModified(true);
            }

            return new LateralHeaderDrawOutcome(
                new LateralHeaderLayout(new List<HeaderBlockInstance>(), 0.0, 0, 0, 0.0), inserted, missing);
        }

        private static BlockTableRecord NewBlock(BlockTable blockTable, Transaction tr, string blockName, out string uniqueName, out ObjectId id)
        {
            uniqueName = UniqueBlockName(blockTable, blockName);
            var definition = new BlockTableRecord { Name = uniqueName, Origin = Point3d.Origin };
            id = blockTable.Add(definition);
            tr.AddNewlyCreatedDBObject(definition, true);
            return definition;
        }

        /// <summary>Append one piece reference to a block; record (once) the blocks not defined in the drawing.</summary>
        private static bool AppendInstance(
            BlockTable blockTable, BlockTableRecord space, Transaction tr,
            HeaderBlockInstance instance, List<HeaderBlockInstance> missing, HashSet<string> seen)
        {
            // Text annotations (frente/level numbers, rack name) are DBText, not blocks.
            if (instance.Role == HeaderBlockRole.Annotation)
            {
                if (string.IsNullOrWhiteSpace(instance.Text))
                {
                    return false;
                }

                var anchor = new Point3d(instance.Insertion.X, instance.Insertion.Y, 0.0);
                var label = new DBText
                {
                    Height = instance.TextHeight > 0.0 ? instance.TextHeight : 3.0,
                    TextString = instance.Text,
                    LayerId = EnsureAnnotationLayer(space.Database, tr),
                    // Center the label on its anchor (frente/level number, name) instead of using it as the
                    // bottom-left base point, so numbers sit exactly on the piece they annotate.
                    HorizontalMode = TextHorizontalMode.TextCenter,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    Position = anchor,
                    AlignmentPoint = anchor
                };
                space.AppendEntity(label);
                tr.AddNewlyCreatedDBObject(label, true);
                label.AdjustAlignment(space.Database); // recompute the drawn position from the alignment modes
                return true;
            }

            if (string.IsNullOrWhiteSpace(instance.BlockName) || !blockTable.Has(instance.BlockName))
            {
                var key = (instance.BlockName ?? instance.PieceId ?? instance.Role.ToString()) + "|" + instance.View;

                if (seen.Add(key))
                {
                    missing.Add(instance);
                }

                return false; // block not defined in the drawing yet — skip rather than throw
            }

            var reference = new BlockReference(
                new Point3d(instance.Insertion.X, instance.Insertion.Y, 0.0),
                blockTable[instance.BlockName])
            {
                Rotation = instance.RotationRadians,
                ScaleFactors = instance.MirroredX ? new Scale3d(-1.0, 1.0, 1.0) : new Scale3d(1.0)
            };

            space.AppendEntity(reference);
            tr.AddNewlyCreatedDBObject(reference, true);

            ApplyDynamicParameters(reference, instance.DynamicParameters);
            return true;
        }

        /// <summary>Layer the text annotations (frente/level numbers, rack name) live on, so they can be toggled/frozen apart.</summary>
        private const string AnnotationLayer = "RACKCAD_ANOTACIONES";

        /// <summary>Ensure the annotations layer exists (yellow); returns its id so the text draws on it (ByLayer).</summary>
        private static ObjectId EnsureAnnotationLayer(Database db, Transaction tr)
        {
            var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (layerTable.Has(AnnotationLayer))
            {
                return layerTable[AnnotationLayer];
            }

            layerTable.UpgradeOpen();
            var record = new LayerTableRecord
            {
                Name = AnnotationLayer,
                Color = Color.FromColorIndex(ColorMethod.ByAci, 2) // yellow
            };
            var id = layerTable.Add(record);
            tr.AddNewlyCreatedDBObject(record, true);
            return id;
        }

        /// <summary>Ensure the block name is free; if taken, append _1, _2, … so we never rename another block.</summary>
        private static string UniqueBlockName(BlockTable blockTable, string baseName)
        {
            var name = BlockNaming.SanitizeBlockName(baseName);

            if (!blockTable.Has(name))
            {
                return name;
            }

            for (var suffix = 1; ; suffix++)
            {
                var candidate = name + "_" + suffix.ToString(CultureInfo.InvariantCulture);

                if (!blockTable.Has(candidate))
                {
                    return candidate;
                }
            }
        }

        private static void ApplyDynamicParameters(BlockReference reference, IReadOnlyDictionary<string, double> values)
        {
            if (!reference.IsDynamicBlock || values.Count == 0)
            {
                return;
            }

            foreach (DynamicBlockReferenceProperty property in reference.DynamicBlockReferencePropertyCollection)
            {
                if (!property.ReadOnly && values.TryGetValue(property.PropertyName, out var value))
                {
                    property.Value = value;
                }
            }
        }
    }
}
