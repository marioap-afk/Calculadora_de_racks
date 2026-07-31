using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using RackCad.Application.Systems.Cantilever;

namespace RackCad.Plugin.Drawing.Cantilever
{
    /// <summary>
    /// Turns a <see cref="CantileverViewPlan"/> into AutoCAD entities. An ADAPTER and nothing else.
    ///
    /// It receives points and piece kinds and creates polylines. It never asks a section for a dimension, never
    /// decides geometry and never rounds anything: every coordinate it draws was decided in
    /// <c>RackCad.Application</c> (ADR-0028, D3). That is what makes the editor's preview and the drawing the same
    /// picture rather than two implementations that agree today.
    ///
    /// It follows the <c>RACKSECCION</c> precedent (I-36C) and not the header one: a Cantilever line is built from
    /// the neutral section catalogue, so there is no <c>blocks-library.dwg</c> entry, no row in <c>blocks.csv</c>
    /// and no pre-existing block to depend on. The representation goes into a block DEFINITION created in THIS
    /// drawing, and its entities are BYBLOCK so the inserted reference controls colour, layer and lineweight as a
    /// unit.
    /// </summary>
    internal static class CantileverViewMaterializer
    {
        /// <summary>Prefix of the internal block name. It is a label for a human, never a key.</summary>
        internal const string BlockNamePrefix = "RACKCAD_CANTILEVER_";

        /// <summary>
        /// Creates the block definition for a view and returns its id.
        ///
        /// The caller owns the transaction, so the definition, its payload and its reference commit together or
        /// none of them exists.
        /// </summary>
        internal static ObjectId CreateBlockDefinition(
            Database database, Transaction transaction, CantileverViewPlan plan, string rackName, out string blockName)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
            blockName = UniqueBlockName(blockTable, SuggestName(plan, rackName));

            var definition = new BlockTableRecord { Name = blockName, Origin = Point3d.Origin };
            var definitionId = blockTable.Add(definition);
            transaction.AddNewlyCreatedDBObject(definition, true);

            AppendCurves(definition, transaction, plan);

            return definitionId;
        }

        /// <summary>
        /// Redefines an existing definition IN PLACE from a new plan: every reference of it updates on regen.
        ///
        /// The old entities are erased and the new ones appended inside the caller's transaction. Nothing nested is
        /// purged afterwards because nothing is nested: a Cantilever view is polylines, not references to library
        /// blocks, so a redraw cannot orphan a definition.
        /// </summary>
        internal static void RedefineBlock(
            Database database, Transaction transaction, ObjectId blockId, CantileverViewPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var definition = (BlockTableRecord)transaction.GetObject(blockId, OpenMode.ForWrite);

            foreach (ObjectId entityId in definition)
            {
                var entity = (Entity)transaction.GetObject(entityId, OpenMode.ForWrite);
                entity.Erase();
            }

            AppendCurves(definition, transaction, plan);

            // A redefinition that GROWS the line draws the new region but leaves AutoCAD testing selection against
            // the old, smaller bounding box — so a window over the grown part selects nothing. Forcing each
            // reference to recompute its graphics AND extents, together with the caller's single regen, makes the
            // whole redrawn line selectable again. (The same defect the header drawer documents.)
            foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
            {
                var reference = (BlockReference)transaction.GetObject(referenceId, OpenMode.ForWrite);
                reference.RecordGraphicsModified(true);
            }
        }

        /// <summary>
        /// Creates the block definition of a plan under an EXPLICIT base name.
        ///
        /// The stand-alone component insertion names its blocks itself — the name carries the component kind, its
        /// designation, the view and its own id — so it needs the same materialisation with its own label. The
        /// uniqueness rule is the shared one: a collision is not an error, it is two independent pieces.
        /// </summary>
        internal static ObjectId CreateBlockDefinitionNamed(
            Database database, Transaction transaction, CantileverViewPlan plan, string baseName, out string blockName)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
            blockName = UniqueBlockName(blockTable, Sanitize(baseName));

            var definition = new BlockTableRecord { Name = blockName, Origin = Point3d.Origin };
            var definitionId = blockTable.Add(definition);
            transaction.AddNewlyCreatedDBObject(definition, true);

            AppendCurves(definition, transaction, plan);

            return definitionId;
        }

        /// <summary>Inserts a reference to a definition at a point in model space.</summary>
        internal static ObjectId InsertReference(
            Database database, Transaction transaction, ObjectId definitionId, Point3d insertion)
        {
            // The same model-space lookup BlockPlacement uses, so there is one idiom for "where a reference goes".
            var modelSpace = (BlockTableRecord)transaction.GetObject(
                SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);

            var reference = new BlockReference(insertion, definitionId);
            modelSpace.AppendEntity(reference);
            transaction.AddNewlyCreatedDBObject(reference, true);

            return reference.ObjectId;
        }

        private static void AppendCurves(
            BlockTableRecord definition, Transaction transaction, CantileverViewPlan plan)
        {
            // Aqui, y no en el llamador. Una entidad se pone en la capa de su naturaleza, asi que esa capa tiene
            // que existir ANTES de la primera curva; dejarlo en manos de quien crea la definicion ya fallo una
            // vez: de las tres puertas que appendean curvas solo una creaba las capas, y las otras dos —redibujo
            // en sitio e insercion de un componente suelto— morian con la capa inexistente. La INSERCION DE UN
            // COMPONENTE SUELTO era una de ellas, y por eso la columna no se podia insertar.
            EnsureRoleLayers(definition.Database, transaction);

            foreach (var curve in plan.Curves)
            {
                var entity = Build(curve);

                if (entity == null)
                {
                    continue;
                }

                ApplyRole(entity, curve.Role);

                definition.AppendEntity(entity);
                transaction.AddNewlyCreatedDBObject(entity, true);
            }
        }

        /// <summary>
        /// Creates the layer of every visual role, once, before anything is drawn on one.
        ///
        /// ALL of them, and not just the ones this plan happens to use: a user who freezes «troqueles» on one
        /// view expects the same layer to be there on the next, and a layer that appears only when a hole does
        /// makes that setting come and go. Existing layers are left ALONE — colour included — because the
        /// drawing belongs to the user and re-imposing a colour would undo a deliberate change every time they
        /// redrew.
        /// </summary>
        private static void EnsureRoleLayers(Database database, Transaction transaction)
        {
            if (database == null)
            {
                return; // una definicion sin base de datos no llega al dibujo
            }

            var table = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);

            foreach (CantileverVisualRole role in Enum.GetValues(typeof(CantileverVisualRole)))
            {
                var name = CantileverVisualRoles.LayerNameOf(role);

                if (table.Has(name))
                {
                    continue;
                }

                table.UpgradeOpen();

                var layer = new LayerTableRecord
                {
                    Name = name,
                    Color = Color.FromColorIndex(ColorMethod.ByAci, CantileverVisualRoles.ColorIndexOf(role))
                };

                table.Add(layer);
                transaction.AddNewlyCreatedDBObject(layer, true);
            }
        }

        /// <summary>
        /// Puts an entity on the layer of its NATURE, drawing its colour from there.
        ///
        /// It replaces «everything BYBLOCK on layer 0», which made a column, a plate, a gusset and a hole read
        /// exactly alike — motivo 5 del rechazo de la ronda 2. What is lost is restyling a whole view from its
        /// inserted reference in one move; what is gained is the control a reader of the drawing actually needs,
        /// which is turning the holes off to look at the steel. Colour and lineweight stay BYLAYER rather than
        /// being stamped on the entity, so the user still owns the look — one layer, not one entity at a time.
        /// </summary>
        private static void ApplyRole(Entity entity, CantileverVisualRole role)
        {
            // El rol viene DECIDIDO en el plan, no se deduce aqui: deducirlo seria una segunda implementacion
            // de la misma regla, y la primera vez que una de las dos cambiase el bloque dejaria de parecerse a
            // la previa que el usuario aprobo.
            entity.Layer = CantileverVisualRoles.LayerNameOf(role);
            entity.ColorIndex = 256; // BYLAYER
            entity.Linetype = "ByLayer";
            entity.LineWeight = LineWeight.ByLayer;
        }

        /// <summary>A curve becomes a real <see cref="Circle"/> when the plan says it is one, a polyline otherwise.</summary>
        private static Entity Build(CantileverViewCurve curve) =>
            curve.IsCircle ? BuildCircle(curve) : (Entity)BuildPolyline(curve);

        /// <summary>
        /// A hole seen down its own axis, as a REAL circle of the diameter the plan carries.
        ///
        /// Not a polygon that approximates one: whoever measures the drawing would measure the polygon. The
        /// plan gives a centre and a diameter — neutral data — and this is the only place that knows AutoCAD
        /// has a <c>Circle</c>.
        /// </summary>
        private static Circle BuildCircle(CantileverViewCurve curve)
        {
            if (curve.Points == null || curve.Points.Count != 1 || !(curve.CircleDiameter > 0.0))
            {
                return null;
            }

            var centre = new Point3d(curve.Points[0].X, curve.Points[0].Y, 0.0);
            var circle = new Circle(centre, Vector3d.ZAxis, curve.CircleDiameter.Value / 2.0);

            ApplyByBlock(circle);
            return circle;
        }

        /// <summary>
        /// One polyline per plan curve, in INCHES — the drawing's own unit (ADR-0005). Nothing is scaled: the plan
        /// is already in the internal unit and converting here would be the silent reinterpretation ADR-0005
        /// forbids.
        /// </summary>
        private static Polyline BuildPolyline(CantileverViewCurve curve)
        {
            if (curve.Points == null || curve.Points.Count < 2)
            {
                return null; // a single point is not a drawable outline
            }

            var polyline = new Polyline(curve.Points.Count);

            for (var i = 0; i < curve.Points.Count; i++)
            {
                polyline.AddVertexAt(i, new Point2d(curve.Points[i].X, curve.Points[i].Y), 0.0, 0.0, 0.0);
            }

            polyline.Closed = curve.IsClosed;

            ApplyByBlock(polyline);
            return polyline;
        }

        /// <summary>
        /// A newborn entity's starting state, before its role dresses it.
        ///
        /// Layer 0 is set EXPLICITLY because a new entity is born on the current layer, and inheriting whatever
        /// CLAYER happened to be would leave a curve on the user's layer if its role were ever not applied.
        /// <see cref="ApplyRole"/> then moves it and switches it to BYLAYER; this is the floor it starts from,
        /// never the state it ships in.
        /// </summary>
        private static void ApplyByBlock(Entity entity)
        {
            entity.Layer = "0";
            entity.ColorIndex = 0;
            entity.Linetype = "ByBlock";
            entity.LineWeight = LineWeight.ByBlock;
        }

        /// <summary>A readable name built from the rack, the view and the station. Nothing resolves a block by it.</summary>
        private static string SuggestName(CantileverViewPlan plan, string rackName)
        {
            var name = string.IsNullOrWhiteSpace(rackName) ? "LINEA" : rackName.Trim();
            var suffix = plan.View == CantileverViewKind.Lateral && plan.StationIndex >= 0
                ? "_E" + (plan.StationIndex + 1).ToString(CultureInfo.InvariantCulture)
                : string.Empty;

            return Sanitize(BlockNamePrefix + name + "_" + plan.View.ToString().ToUpperInvariant() + suffix);
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
        /// The first free name of the form <c>base</c>, <c>base_2</c>, … A collision is not an error: two frontals
        /// of two lines with the same name are two independent views, and neither may redefine the other's block.
        /// </summary>
        private static string UniqueBlockName(BlockTable blockTable, string baseName)
        {
            if (!blockTable.Has(baseName))
            {
                return baseName;
            }

            for (var suffix = 2; suffix < int.MaxValue; suffix++)
            {
                var candidate = baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture);

                if (!blockTable.Has(candidate))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("No hay un nombre de bloque libre para la vista Cantilever.");
        }

        /// <summary>Every piece kind the plan drew, for the message the user reads afterwards.</summary>
        internal static IReadOnlyList<string> DescribeContents(CantileverViewPlan plan)
        {
            var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);

            foreach (var curve in plan.Curves)
            {
                var key = curve.Kind.ToString();
                counts[key] = counts.TryGetValue(key, out var current) ? current + 1 : 1;
            }

            var lines = new List<string>(counts.Count);

            foreach (var pair in counts)
            {
                lines.Add(pair.Key + " x" + pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            return lines;
        }
    }
}
