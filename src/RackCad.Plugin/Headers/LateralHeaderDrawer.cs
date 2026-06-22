using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Thin AutoCAD adapter: turns a pure <see cref="LateralHeaderLayout"/> into real block insertions.
    /// All the geometry/parameters live in the Application layer (testable on any OS); this class only
    /// touches the AutoCAD API, so it compiles only inside the Plugin (Windows + AutoCAD managed DLLs).
    ///
    /// Each instance is inserted at its origin, rotated, optionally mirrored (X scale -1), and its
    /// dynamic-block parameters (LONGITUD, Distancia1, ...) are set by name.
    /// </summary>
    public sealed class LateralHeaderDrawer
    {
        private readonly LateralHeaderLayoutBuilder builder = new LateralHeaderLayoutBuilder();

        /// <summary>Resolve geometry from the catalog, build the plan, and draw it. Returns the plan used.</summary>
        public LateralHeaderLayout BuildAndDraw(
            Database db, Transaction tr, BlockTableRecord space, RackCatalog catalog, LateralHeaderParameters parameters)
        {
            var geometry = HeaderGeometryResolver.Resolve(catalog, parameters);
            var layout = builder.Build(parameters, geometry);
            Draw(db, tr, space, layout);
            return layout;
        }

        public void Draw(Database db, Transaction tr, BlockTableRecord space, LateralHeaderLayout layout)
        {
            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            foreach (var instance in layout.Instances)
            {
                if (string.IsNullOrWhiteSpace(instance.BlockName) || !blockTable.Has(instance.BlockName))
                {
                    continue; // block not defined in the drawing yet — skip rather than throw
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
