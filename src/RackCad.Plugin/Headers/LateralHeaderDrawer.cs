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
                    var headerRef = new BlockReference(new Point3d(placement.InsertionX, placement.InsertionY, 0.0), pair.Key)
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
        public LateralHeaderDrawOutcome RedefineSystemBlock(
            Database db, Transaction tr, ObjectId blockId, DynamicSystemPlan plan, out IReadOnlyCollection<ObjectId> staleDefs)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var systemDef = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForWrite);

            // Clear the current contents (collect first, then erase — don't erase while enumerating). Capture the
            // nested definitions the block references BEFORE erasing: each redefine creates fresh uniquified header
            // defs, so the old ones must be purged afterwards or they pile up in the block table forever (nombre_1, _2…).
            var existing = new List<ObjectId>();
            var priorReferencedDefs = new HashSet<ObjectId>();
            foreach (ObjectId id in systemDef)
            {
                existing.Add(id);
                var obj = tr.GetObject(id, OpenMode.ForRead);
                if (obj is BlockReference nestedRef && !nestedRef.BlockTableRecord.IsNull)
                {
                    priorReferencedDefs.Add(nestedRef.BlockTableRecord);
                }
                else if (obj is Dimension dimension && !dimension.DimBlockId.IsNull)
                {
                    // A dimension owns an anonymous *D block for its graphics; erasing the dimension orphans it, so
                    // enqueue it for the post-commit purge or the block table grows one *D per cota per redraw.
                    priorReferencedDefs.Add(dimension.DimBlockId);
                }
            }

            foreach (var id in existing)
            {
                ((Entity)tr.GetObject(id, OpenMode.ForWrite)).Erase();
            }

            var missing = new List<HeaderBlockInstance>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inserted = 0;

            // Defs the NEW content references. Catalog piece blocks are re-referenced under the SAME ObjectId and
            // the fresh header defs are created here, so subtracting this set from priorReferencedDefs leaves only
            // the genuinely stale defs — sparing the purge its expensive whole-drawing reference scan on catalog
            // blocks (referenced hundreds of times) that always concluded "still referenced, keep".
            var newReferencedDefs = new HashSet<ObjectId>();

            foreach (var group in plan.Headers)
            {
                var headerDef = NewBlock(blockTable, tr, group.Name, out _, out var headerId);
                newReferencedDefs.Add(headerId);

                foreach (var instance in group.Instances)
                {
                    if (AppendInstance(blockTable, headerDef, tr, instance, missing, seen, newReferencedDefs))
                    {
                        inserted++;
                    }
                }

                foreach (var placement in group.Placements)
                {
                    var headerRef = new BlockReference(new Point3d(placement.InsertionX, placement.InsertionY, 0.0), headerId)
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
                if (AppendInstance(blockTable, systemDef, tr, instance, missing, seen, newReferencedDefs))
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

            // Only defs the new content did NOT re-reference can be stale (cheap set difference); everything the
            // repopulated block still uses is trivially alive. The caller purges these AFTER the transaction commits,
            // when Database.Purge can compute purgeability in ONE optimized pass over the committed state (no per-def
            // whole-drawing reference scan). The old references are erased above, so post-commit they are truly gone.
            priorReferencedDefs.ExceptWith(newReferencedDefs);
            staleDefs = priorReferencedDefs;

            return new LateralHeaderDrawOutcome(
                new LateralHeaderLayout(new List<HeaderBlockInstance>(), 0.0, 0, 0, 0.0), inserted, missing);
        }

        /// <summary>
        /// Erase the block DEFINITIONS a redraw left unreferenced (the freshly-uniquified nested header defs a rewrite
        /// abandons — <c>nombre_1</c>, <c>_2</c>, … — which would otherwise pile up in the block table). Meant to run on
        /// the COMMITTED drawing (its own transaction) so <see cref="Database.Purge"/> filters the candidates to those
        /// with no remaining references in ONE optimized pass — cheaper than a per-def
        /// <c>GetBlockReferenceIds(forceValidity:true)</c> whole-drawing scan (see the autocad-insert-perf memory) and
        /// correct because the old references are already committed as erased. Defs still used by another rack survive
        /// the filter untouched. Best effort — a lingering def is preferable to failing the redraw.
        /// </summary>
        internal static void PurgeUnreferenced(Database db, IReadOnlyCollection<ObjectId> candidates)
        {
            if (db == null || candidates == null || candidates.Count == 0)
            {
                return;
            }

            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var ids = new ObjectIdCollection();
                    foreach (var id in candidates)
                    {
                        if (id.IsNull || id.IsErased)
                        {
                            continue;
                        }

                        if (tr.GetObject(id, OpenMode.ForRead) is BlockTableRecord def && !def.IsLayout)
                        {
                            ids.Add(id); // never enqueue model/paper space
                        }
                    }

                    if (ids.Count > 0)
                    {
                        db.Purge(ids); // edits the collection down to only the purgeable (0-reference) defs
                        foreach (ObjectId id in ids)
                        {
                            if (!id.IsNull && !id.IsErased)
                            {
                                ((BlockTableRecord)tr.GetObject(id, OpenMode.ForWrite)).Erase();
                            }
                        }
                    }

                    tr.Commit();
                }
            }
            catch
            {
                // Best effort: a lingering nested def is preferable to failing the redraw.
            }
        }

        private static BlockTableRecord NewBlock(BlockTable blockTable, Transaction tr, string blockName, out string uniqueName, out ObjectId id)
        {
            uniqueName = UniqueBlockName(blockTable, blockName);
            var definition = new BlockTableRecord { Name = uniqueName, Origin = Point3d.Origin };
            id = blockTable.Add(definition);
            tr.AddNewlyCreatedDBObject(definition, true);
            return definition;
        }

        /// <summary>Append one piece reference to a block; record (once) the blocks not defined in the drawing.
        /// When <paramref name="referencedDefs"/> is given, the id of every definition actually referenced is added
        /// to it (the redefine path uses this to know which prior defs are still alive without scanning them).</summary>
        private static bool AppendInstance(
            BlockTable blockTable, BlockTableRecord space, Transaction tr,
            HeaderBlockInstance instance, List<HeaderBlockInstance> missing, HashSet<string> seen,
            HashSet<ObjectId> referencedDefs = null)
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

            // Linear dimensions (cotas) are RotatedDimensions, not blocks. p1=Insertion, p2=ConnectionAnchor; the
            // segment is axis-aligned, so a horizontal measure shares Y (rotation 0) and a vertical one shares X
            // (rotation 90°). DimensionOffset (signed) places the dimension line to one side.
            if (instance.Role == HeaderBlockRole.Dimension)
            {
                AppendDimension(space, tr, instance);
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

            var definitionId = blockTable[instance.BlockName];
            referencedDefs?.Add(definitionId);

            var reference = new BlockReference(
                new Point3d(instance.Insertion.X, instance.Insertion.Y, 0.0),
                definitionId)
            {
                Rotation = instance.RotationRadians,
                ScaleFactors = new Scale3d(instance.MirroredX ? -1.0 : 1.0, instance.MirroredY ? -1.0 : 1.0, 1.0)
            };

            space.AppendEntity(reference);
            tr.AddNewlyCreatedDBObject(reference, true);

            ApplyDynamicParameters(reference, instance.DynamicParameters);
            return true;
        }

        /// <summary>Layer the text annotations (frente/level numbers, rack name) live on, so they can be toggled/frozen apart.</summary>
        private const string AnnotationLayer = "RACKCAD_ANOTACIONES";

        /// <summary>Layer the dimensions (cotas) live on, so they can be frozen/plotted apart from the geometry.</summary>
        private const string DimensionLayer = "RACKCAD_COTAS";

        /// <summary>Ensure the annotations layer exists (yellow); returns its id so the text draws on it (ByLayer).</summary>
        private static ObjectId EnsureAnnotationLayer(Database db, Transaction tr)
            => EnsureLayer(db, tr, AnnotationLayer, 2); // yellow

        /// <summary>Ensure a named layer exists with the given ACI color; returns its id (existing or freshly created).</summary>
        private static ObjectId EnsureLayer(Database db, Transaction tr, string name, short aci)
        {
            var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (layerTable.Has(name))
            {
                return layerTable[name];
            }

            layerTable.UpgradeOpen();
            var record = new LayerTableRecord
            {
                Name = name,
                Color = Color.FromColorIndex(ColorMethod.ByAci, aci)
            };
            var id = layerTable.Add(record);
            tr.AddNewlyCreatedDBObject(record, true);
            return id;
        }

        /// <summary>
        /// Materialize a <see cref="HeaderBlockRole.Dimension"/> as a RotatedDimension on the dimensions layer. Text,
        /// arrows and gaps are sized to the instance's (annotation-scaled) text height via per-dimension DIMVAR
        /// overrides, so cotas stay proportional to the numbers regardless of the drawing's current DIMSTYLE.
        /// </summary>
        private static void AppendDimension(BlockTableRecord space, Transaction tr, HeaderBlockInstance instance)
        {
            var db = space.Database;
            var p1 = new Point3d(instance.Insertion.X, instance.Insertion.Y, 0.0);
            var p2 = new Point3d(instance.ConnectionAnchor.X, instance.ConnectionAnchor.Y, 0.0);

            // Axis-aligned: a horizontal measure shares Y (extension lines drop down), a vertical one shares X.
            var horizontal = Math.Abs(p2.Y - p1.Y) <= Math.Abs(p2.X - p1.X);
            var rotation = horizontal ? 0.0 : Math.PI / 2.0;
            var dimLine = horizontal
                ? new Point3d((p1.X + p2.X) / 2.0, p1.Y + instance.DimensionOffset, 0.0)
                : new Point3d(p1.X + instance.DimensionOffset, (p1.Y + p2.Y) / 2.0, 0.0);

            // A named style (chosen by the user) is respected as-is; otherwise use the drawing's current style and
            // size the cota to the annotation-scaled text height via per-dimension DIMVAR overrides.
            var chosenStyle = ResolveDimStyle(db, tr, instance.DimensionStyleName);
            var dimension = new RotatedDimension(rotation, p1, p2, dimLine, string.Empty, chosenStyle.StyleId)
            {
                LayerId = EnsureLayer(db, tr, DimensionLayer, 1) // red
            };

            space.AppendEntity(dimension);
            tr.AddNewlyCreatedDBObject(dimension, true);

            if (!chosenStyle.IsNamed)
            {
                var textHeight = instance.TextHeight > 0.0 ? instance.TextHeight : 3.0; // fallback if the builder set none
                dimension.Dimscale = 1.0;                 // sizes below are absolute, not multiplied by the current DIMSTYLE
                dimension.Dimtxt = textHeight;            // text height
                dimension.Dimasz = textHeight * 0.7;      // arrowhead size
                dimension.Dimexe = textHeight * 0.4;      // extension line beyond the dimension line
                dimension.Dimexo = textHeight * 0.4;      // extension line offset from the origin points
                dimension.Dimgap = textHeight * 0.3;      // gap around the text
                dimension.Dimtad = 1;                     // text above the dimension line
                dimension.Dimdec = 2;                     // 2 decimal places
            }

            dimension.RecomputeDimensionBlock(true);  // rebuild the dimension geometry with the style/overrides
        }

        /// <summary>Resolve the dimension style to use: the named style when given AND present in the drawing (respected
        /// as-is), else the drawing's current style (IsNamed = false → the caller applies scaled DIMVAR overrides).</summary>
        private static (ObjectId StyleId, bool IsNamed) ResolveDimStyle(Database db, Transaction tr, string styleName)
        {
            if (!string.IsNullOrWhiteSpace(styleName))
            {
                var table = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                if (table.Has(styleName))
                {
                    return (table[styleName], true);
                }
            }

            return (db.Dimstyle, false);
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

            // Match case-insensitively: a dynamic block's parameter names are author-defined and their casing varies
            // block to block (e.g. a tarima with "longitud"/"Alto" vs a larguero with "LONGITUD"). Exact-case matching
            // would silently drop the value and leave the piece un-stretched at its default size.
            var lookup = new Dictionary<string, double>(values.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var pair in values)
            {
                lookup[pair.Key] = pair.Value;
            }

            foreach (DynamicBlockReferenceProperty property in reference.DynamicBlockReferencePropertyCollection)
            {
                if (!property.ReadOnly && lookup.TryGetValue(property.PropertyName, out var value))
                {
                    property.Value = value;
                }
            }
        }
    }
}
