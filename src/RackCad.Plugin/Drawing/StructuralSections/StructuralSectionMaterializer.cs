using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Plugin.Drawing.StructuralSections
{
    /// <summary>
    /// Turns a neutral plan into AutoCAD entities. An ADAPTER and nothing else.
    ///
    /// It receives points and roles and creates polylines. It never asks a section for a dimension, never
    /// decides geometry and never rounds anything: everything it draws was decided in
    /// <c>RackCad.Application</c> (ADR-0022 §16). That is what makes the preview and the drawing the same
    /// picture rather than two implementations that agree today.
    ///
    /// The representation goes into a block DEFINITION created in THIS drawing. There is no
    /// <c>blocks-library.dwg</c>, no row in <c>blocks.csv</c> and no pre-existing block to depend on: a
    /// parametric profile does not need 983 hand-drawn blocks, which is precisely the point of I-36B
    /// (ADR-0022 §17). Nor is a library of definitions built per section × length — a free length would grow
    /// the drawing without bound.
    ///
    /// Entities are BYBLOCK so the inserted reference controls colour, layer and lineweight as a unit.
    /// </summary>
    internal static class StructuralSectionMaterializer
    {
        /// <summary>Prefix of the internal block name. The commercial designation is never the authority.</summary>
        public const string BlockNamePrefix = "RACKCAD_SECCION_";

        /// <summary>
        /// Layer the axis and the envelope go on, so a user can freeze them and keep the piece.
        ///
        /// The SAME name the header drawer uses for its own annotations: freezing "the RackCad annotations"
        /// must hide all of them, and a second layer for the same idea would be a surprise. The literal is
        /// repeated because the original is a private const in a file this initiative must not touch; a guard
        /// test pins the two together so the duplication cannot drift silently.
        /// </summary>
        internal const string AnnotationLayer = "RACKCAD_ANOTACIONES";

        /// <summary>
        /// Creates the block definition for a plan and returns its id. The caller owns the transaction, so a
        /// system that wants this geometry inside its OWN block can call exactly this and never touch the
        /// RACKSECCION command.
        /// </summary>
        internal static ObjectId CreateBlockDefinition(
            Database database, Transaction transaction, StructuralSectionRepresentationPlan plan, out string blockName)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
            blockName = UniqueBlockName(blockTable, SuggestName(plan));

            var annotationLayerId = LayerHelper.EnsureLayer(database, transaction, AnnotationLayer, 2); // yellow

            var definition = new BlockTableRecord { Name = blockName, Origin = Point3d.Origin };
            var definitionId = blockTable.Add(definition);
            transaction.AddNewlyCreatedDBObject(definition, true);

            foreach (var curve in plan.Curves)
            {
                var entity = BuildPolyline(curve);

                if (IsAnnotation(curve.Role))
                {
                    entity.LayerId = annotationLayerId;
                }

                definition.AppendEntity(entity);
                transaction.AddNewlyCreatedDBObject(entity, true);
            }

            return definitionId;
        }

        /// <summary>Inserts a reference to a definition at a point in model space.</summary>
        internal static ObjectId InsertReference(
            Database database, Transaction transaction, ObjectId definitionId, Point3d insertion)
        {
            // Same model-space lookup BlockPlacement uses, so there is one idiom for "where a reference goes".
            var modelSpace = (BlockTableRecord)transaction.GetObject(
                SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);

            var reference = new BlockReference(insertion, definitionId);
            modelSpace.AppendEntity(reference);
            transaction.AddNewlyCreatedDBObject(reference, true);

            return reference.ObjectId;
        }

        /// <summary>Axis and envelope describe the piece; they are not the piece.</summary>
        private static bool IsAnnotation(SectionCurveRole role) =>
            role == SectionCurveRole.Axis || role == SectionCurveRole.Envelope;

        /// <summary>
        /// One polyline per plan curve, in INCHES — the drawing's own unit (ADR-0005). Nothing is scaled:
        /// the plan is already in the internal unit and converting here would be exactly the silent
        /// reinterpretation ADR-0005 forbids.
        /// </summary>
        private static Polyline BuildPolyline(SectionPlanCurve curve)
        {
            var polyline = new Polyline(curve.Points.Count);

            for (var i = 0; i < curve.Points.Count; i++)
            {
                polyline.AddVertexAt(i, new Point2d(curve.Points[i].X, curve.Points[i].Y), 0.0, 0.0, 0.0);
            }

            polyline.Closed = curve.IsClosed;

            // BYBLOCK on layer 0: the inserted reference decides colour, linetype and lineweight, so the user
            // can drop the section on any layer and restyle the whole piece in one move. Layer 0 is set
            // EXPLICITLY — a new entity is born on the current layer, and inheriting whatever CLAYER happened
            // to be would quietly break that.
            polyline.Layer = "0";
            polyline.ColorIndex = 0;
            polyline.Linetype = "ByBlock";
            polyline.LineWeight = LineWeight.ByBlock;

            return polyline;
        }

        /// <summary>
        /// A readable name built from the id, the view and the length — enough for a human to recognise the
        /// block in the drawing, and NOT a key: nothing resolves a block by this name.
        /// </summary>
        private static string SuggestName(StructuralSectionRepresentationPlan plan)
        {
            var length = plan.Length.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
                .Replace('.', '_');

            return Sanitize(BlockNamePrefix + plan.SectionId.Value + "_" + plan.View + "_L" + length);
        }

        /// <summary>Keeps only what an AutoCAD symbol name accepts.</summary>
        private static string Sanitize(string name)
        {
            var builder = new System.Text.StringBuilder(name.Length);

            foreach (var ch in name)
            {
                builder.Append(
                    (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') || ch == '_' || ch == '-'
                        ? ch
                        : '_');
            }

            return builder.ToString();
        }

        /// <summary>
        /// The first free name of the form <c>base</c>, <c>base_2</c>, … A collision is not an error: two
        /// inserts of the same section at the same length are two independent representations, and neither
        /// should redefine the other's block.
        /// </summary>
        private static string UniqueBlockName(BlockTable blockTable, string baseName)
        {
            if (!blockTable.Has(baseName))
            {
                return baseName;
            }

            for (var suffix = 2; suffix < int.MaxValue; suffix++)
            {
                var candidate = baseName + "_" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);

                if (!blockTable.Has(candidate))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("No hay un nombre de bloque libre para la seccion.");
        }
    }
}
