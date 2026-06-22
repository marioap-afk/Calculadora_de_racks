using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Headers;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Thin AutoCAD adapter: turns a pure <see cref="LateralHeaderLayout"/> into a single AutoCAD block whose
    /// sub-entities are the header pieces. All the geometry/parameters live in the Application layer (testable
    /// on any OS); this class only touches the AutoCAD API, so it compiles only inside the Plugin.
    ///
    /// Each piece is appended to a new block definition at its plan origin, rotated, optionally mirrored
    /// (X scale -1), with its dynamic-block parameters (LONGITUD) set by name. The caller then inserts a
    /// reference to that block wherever the user clicks.
    /// </summary>
    public sealed class LateralHeaderDrawer
    {
        /// <summary>
        /// Build a block definition named <paramref name="blockName"/> (uniquified if it already exists)
        /// containing every piece of the plan that the drawing actually defines. Returns the new block's id
        /// plus what was inserted/skipped, so the caller can jig-insert a reference and report missing blocks.
        /// </summary>
        public LateralHeaderBlockResult CreateHeaderBlock(
            Database db, Transaction tr, LateralHeaderLayout layout, string blockName)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            var uniqueName = UniqueBlockName(blockTable, blockName);

            var definition = new BlockTableRecord
            {
                Name = uniqueName,
                Origin = Point3d.Origin
            };

            var definitionId = blockTable.Add(definition);
            tr.AddNewlyCreatedDBObject(definition, true);

            var missing = new List<HeaderBlockInstance>();
            var seenMissingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inserted = 0;

            foreach (var instance in layout.Instances)
            {
                if (string.IsNullOrWhiteSpace(instance.BlockName) || !blockTable.Has(instance.BlockName))
                {
                    // Report each missing piece once (several horizontals share one truss block).
                    var key = (instance.BlockName ?? instance.PieceId ?? instance.Role.ToString()) + "|" + instance.View;

                    if (seenMissingKeys.Add(key))
                    {
                        missing.Add(instance);
                    }

                    continue; // block not defined in the drawing yet — skip rather than throw
                }

                var reference = new BlockReference(
                    new Point3d(instance.Insertion.X, instance.Insertion.Y, 0.0),
                    blockTable[instance.BlockName])
                {
                    Rotation = instance.RotationRadians,
                    ScaleFactors = instance.MirroredX ? new Scale3d(-1.0, 1.0, 1.0) : new Scale3d(1.0)
                };

                definition.AppendEntity(reference);
                tr.AddNewlyCreatedDBObject(reference, true);

                ApplyDynamicParameters(reference, instance.DynamicParameters);
                inserted++;
            }

            return new LateralHeaderBlockResult(definitionId, uniqueName, new LateralHeaderDrawOutcome(layout, inserted, missing));
        }

        /// <summary>Ensure the block name is free; if taken, append _1, _2, … so we never rename another block.</summary>
        private static string UniqueBlockName(BlockTable blockTable, string baseName)
        {
            var name = SanitizeBlockName(baseName);

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

        private static string SanitizeBlockName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Cabecera";
            }

            // AutoCAD block names cannot contain < > / \ " : ; ? * | , = `
            var invalid = new[] { '<', '>', '/', '\\', '"', ':', ';', '?', '*', '|', ',', '=', '`' };
            var cleaned = name.Trim();

            foreach (var character in invalid)
            {
                cleaned = cleaned.Replace(character, ' ');
            }

            return cleaned.Trim();
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
