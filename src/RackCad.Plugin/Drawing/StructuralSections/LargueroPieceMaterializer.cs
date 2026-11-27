using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Systems.Larguero;

namespace RackCad.Plugin.Drawing.StructuralSections
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda): turns the plan of a larguero of the page «Larguero» into a block DEFINITION of this drawing. An ADAPTER and
    /// nothing else, as <see cref="StructuralSectionMaterializer"/>: it receives points and roles and creates polylines; it never asks a
    /// section or a ménsula for a dimension (the plan was decided in Application, and the page drew the same one). No block library: the
    /// larguero is a loose piece, the way RACKSECCION draws a section. Entities are BYBLOCK on layer 0, so the reference decides colour, layer
    /// and lineweight; the notes (axis, envelope) go to the annotation layer of RackCad.
    /// </summary>
    internal static class LargueroPieceMaterializer
    {
        /// <summary>Creates the definition of the larguero and returns its id; the caller owns the transaction.</summary>
        internal static ObjectId CreateBlockDefinition(Database database, Transaction transaction, LargueroPiecePlan plan, out string blockName)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
            blockName = UniqueBlockName(blockTable, string.IsNullOrWhiteSpace(plan.BlockBaseName) ? "RACKCAD_LARGUERO" : plan.BlockBaseName);

            var definition = new BlockTableRecord { Name = blockName, Origin = Point3d.Origin };
            var definitionId = blockTable.Add(definition);
            transaction.AddNewlyCreatedDBObject(definition, true);

            ObjectId annotationLayerId = ObjectId.Null;
            foreach (var curve in plan.Curves)
            {
                var polyline = new Polyline(curve.Points.Count);
                for (var i = 0; i < curve.Points.Count; i++)
                {
                    polyline.AddVertexAt(i, new Point2d(curve.Points[i].X, curve.Points[i].Y), 0.0, 0.0, 0.0);
                }

                polyline.Closed = curve.IsClosed;
                polyline.Layer = "0";
                polyline.ColorIndex = 0;
                polyline.Linetype = "ByBlock";
                polyline.LineWeight = LineWeight.ByBlock;
                if (curve.Role == LargueroCurveRole.Annotation)
                {
                    if (annotationLayerId.IsNull)
                        annotationLayerId = LayerHelper.EnsureLayer(database, transaction, StructuralSectionMaterializer.AnnotationLayer, 2);
                    polyline.LayerId = annotationLayerId;
                }

                definition.AppendEntity(polyline);
                transaction.AddNewlyCreatedDBObject(polyline, true);
            }

            return definitionId;
        }

        // The first free name «base», «base_2»…: two insertions of the same larguero are two independent pieces; neither redefines the other.
        private static string UniqueBlockName(BlockTable blockTable, string baseName)
        {
            if (!blockTable.Has(baseName)) return baseName;
            for (var suffix = 2; suffix < int.MaxValue; suffix++)
            {
                var candidate = baseName + "_" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (!blockTable.Has(candidate)) return candidate;
            }

            throw new InvalidOperationException("No hay un nombre de bloque libre para el larguero.");
        }
    }
}
