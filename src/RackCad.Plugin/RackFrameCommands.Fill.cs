using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Geometry;
using RackCad.Application.Layout;
using RackCad.Application.Persistence;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKRELLENAR: read the warehouse site (envelope polyline + columns) from a layer and auto-fill it
    /// with as many copies of a rack as fit — the first working, deterministic version of the layout optimizer.</summary>
    public sealed partial class RackFrameCommands
    {
        /// <summary>
        /// Auto-fill: pick a rack (its PLANTA view is used), read the site geometry from a layer (the largest CLOSED
        /// polyline = the envelope, anything else there = columns/obstacles by bounding box), compute the maximum grid
        /// that fits (trying both orientations), report it, and place the copies + labels after confirmation. Copies are
        /// LINKED (one shared definition; RACKBOMTOTAL counts them).
        /// </summary>
        [CommandMethod("RACKRELLENAR")]
        public void RackRellenar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;

                if (!PickRackBlock(document, "\nSelecciona el rack con el que rellenar: ", out var embed, out _))
                {
                    return;
                }

                if (embed == null || string.IsNullOrWhiteSpace(embed.Id))
                {
                    editor.WriteMessage("\nRackCad: ese bloque no tiene datos de rack.");
                    return;
                }

                var plantaBlocks = FindRackBlocks(document, embed.Id).Where(IsPlantaViewBlock).ToList();
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

                var window = new RackWarehouseFillWindow(embed.Name, seed.FootprintX, seed.FootprintY);
                AcApplication.ShowModalWindow(window);
                if (window.Result == null)
                {
                    return; // cancelled
                }

                var choice = window.Result;
                if (!ReadSiteFromLayer(document, choice.Layer, choice.ColumnClearance, out var boundary, out var obstacles, out var ignoredLarge, out var problem))
                {
                    editor.WriteMessage("\nRackCad: " + problem);
                    return;
                }

                if (ignoredLarge > 0)
                {
                    editor.WriteMessage("\nRackCad: se ignoraron " + ignoredLarge +
                        " objeto(s) casi tan grandes como el contorno en la capa (¿contorno duplicado?).");
                }

                var site = WarehouseSite.FromBoundary(boundary, choice.WallClearance, 0.0, obstacles);
                var fill = WarehouseAutoFill.Fill(
                    site, seed.FootprintX, seed.FootprintY,
                    choice.AisleBetweenRows, choice.AisleBetweenCols,
                    choice.BackToBack ? RowPairing.BackToBack : RowPairing.Single, choice.BackGap,
                    choice.TryRotated);

                if (fill.Cells.Count == 0)
                {
                    editor.WriteMessage(string.Format(CultureInfo.InvariantCulture,
                        "\nRackCad: no cabe ningún rack en el área — rejilla probada {0}×{1}: {2} celdas sobre obstáculos, {3} fuera del contorno (revisa pasillos, holguras y unidades).",
                        fill.RowsTried, fill.ColsTried, fill.OmittedByObstacle, fill.OmittedOutside));
                    return;
                }

                // The measured seed copy stays where it is; if it sits inside the fill area it will NOT be considered
                // by the fill (a cell may overlap it) — warn so the user moves or erases it afterwards.
                if (SeedInsideSite(seed, site))
                {
                    editor.WriteMessage("\nRackCad: OJO — la copia original del rack está dentro del área a rellenar; " +
                        "el relleno no la considera. Muévela o bórrala después.");
                }

                editor.WriteMessage(string.Format(CultureInfo.InvariantCulture,
                    "\nRackCad: caben {0} racks — orientación {1}, rejilla probada {2}×{3}; {4} fuera del contorno, {5} sobre columnas (sitio {6:0.##}\"×{7:0.##}\", {8} obstáculos).",
                    fill.Cells.Count,
                    fill.Orientation == RackOrientation.Rotated ? "girada 90°" : "normal",
                    fill.RowsTried, fill.ColsTried,
                    fill.OmittedOutside, fill.OmittedByObstacle,
                    site.Width, site.Depth, obstacles.Count));

                if (!AskYes(editor, "\n¿Colocar los racks?", defaultYes: true))
                {
                    return;
                }

                var placed = PlaceFill(document, fill, seed);
                editor.WriteMessage(string.Format(CultureInfo.InvariantCulture,
                    "\nRackCad: relleno colocado — {0} racks enlazados + etiquetas.", placed));
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Read the site from a layer: the largest CLOSED polyline is the envelope (vertices as drawn — arc
        /// bulges are approximated by their vertices); circles, other closed polylines and block references on the layer
        /// become obstacles by bounding box with the given clearance. False (with a message) when no envelope is found.</summary>
        private static bool ReadSiteFromLayer(
            Document document, string layerName, double columnClearance,
            out List<Point2D> boundary, out List<SiteObstacle> obstacles, out int ignoredLarge, out string problem)
        {
            boundary = null;
            obstacles = new List<SiteObstacle>();
            ignoredLarge = 0;
            problem = null;

            var envelopeId = ObjectId.Null;
            List<Point2D> envelopePoints = null;
            var envelopeArea = 0.0;
            var openOutlines = 0;
            var looseLines = 0;
            var splines = 0;
            var anythingOnLayer = 0;
            var obstacleRects = new List<(double X, double Y, double W, double D, string Label)>();

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(document.Database), OpenMode.ForRead);

                // First pass: the largest CLOSED outline on the layer is the envelope. "Closed" means the Closed flag
                // OR coincident endpoints (a nave traced back to its start without the "Cerrar" option). Light
                // (LWPOLYLINE), heavy (2D) and 3D polylines are all accepted; anything else is counted so the error
                // message can say what WAS found and how to fix it.
                foreach (ObjectId id in modelSpace)
                {
                    if (!(transaction.GetObject(id, OpenMode.ForRead) is Entity entity) ||
                        !string.Equals(entity.Layer, layerName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    anythingOnLayer++;
                    if (TryGetClosedOutline(transaction, entity, out var points))
                    {
                        var area = BoundingArea(entity);
                        if (area > envelopeArea)
                        {
                            envelopeId = id;
                            envelopePoints = points;
                            envelopeArea = area;
                        }
                    }
                    else if (entity is Polyline || entity is Polyline2d || entity is Polyline3d)
                    {
                        openOutlines++;
                    }
                    else if (entity is Line)
                    {
                        looseLines++;
                    }
                    else if (entity is Spline)
                    {
                        splines++;
                    }
                }

                if (envelopePoints == null)
                {
                    transaction.Commit();
                    problem = OutlineProblem(layerName, anythingOnLayer, openOutlines, looseLines, splines);
                    return false;
                }

                // Second pass: everything else on the layer is an obstacle (by bounding box).
                foreach (ObjectId id in modelSpace)
                {
                    if (id == envelopeId ||
                        !(transaction.GetObject(id, OpenMode.ForRead) is Entity entity) ||
                        !string.Equals(entity.Layer, layerName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!(entity is Circle) && !(entity is Polyline) && !(entity is Polyline2d) &&
                        !(entity is Polyline3d) && !(entity is Ellipse) && !(entity is BlockReference))
                    {
                        continue; // lines/text/dimensions on the layer are ignored
                    }

                    try
                    {
                        var extents = entity.GeometricExtents;
                        var width = extents.MaxPoint.X - extents.MinPoint.X;
                        var depth = extents.MaxPoint.Y - extents.MinPoint.Y;

                        // A duplicate of the envelope (a common copy-in-place accident) would become an obstacle
                        // covering the whole site → 0 cells with a baffling message. Anything nearly as large as the
                        // envelope is ignored with a warning instead of silently blanking the fill.
                        if (width * depth >= 0.5 * envelopeArea)
                        {
                            ignoredLarge++;
                            continue;
                        }

                        obstacleRects.Add((
                            extents.MinPoint.X, extents.MinPoint.Y, width, depth,
                            entity is Circle ? "columna" : entity is BlockReference ? "bloque" : "obstáculo"));
                    }
                    catch
                    {
                        // Degenerate extents: skip the entity rather than fail the whole read.
                    }
                }

                boundary = envelopePoints;
                transaction.Commit();
            }

            var index = 1;
            foreach (var rect in obstacleRects)
            {
                obstacles.Add(new SiteObstacle(rect.X, rect.Y, rect.W, rect.D, columnClearance,
                    rect.Label + " " + index.ToString(CultureInfo.InvariantCulture)));
                index++;
            }

            return true;
        }

        /// <summary>Endpoint closeness (in) under which an outline counts as closed even without the Closed flag
        /// (a nave traced back to its starting point without using the "Cerrar" option).</summary>
        private const double OutlineCloseTolerance = 0.01;

        /// <summary>
        /// Extract a CLOSED outline's vertices from an entity: light polylines (LWPOLYLINE — arc segments SAMPLED into
        /// 16 sub-chords, since the plain chord would gain illegal floor area on a wall curving inward), heavy 2D
        /// polylines and 3D polylines (flattened to XY). Closed by flag OR by coincident endpoints. False otherwise.
        /// </summary>
        private static bool TryGetClosedOutline(Transaction transaction, Entity entity, out List<Point2D> points)
        {
            points = null;

            if (entity is Polyline light && light.NumberOfVertices >= 3)
            {
                var raw = new List<Point2D>(light.NumberOfVertices);
                for (var i = 0; i < light.NumberOfVertices; i++)
                {
                    var vertex = light.GetPoint2dAt(i);
                    raw.Add(new Point2D(vertex.X, vertex.Y));

                    // The segment AFTER the last vertex only exists when the polyline is flagged Closed.
                    var lastVertex = i == light.NumberOfVertices - 1;
                    if ((!lastVertex || light.Closed) && Math.Abs(light.GetBulgeAt(i)) > 1e-9)
                    {
                        const int Samples = 16;
                        for (var k = 1; k < Samples; k++)
                        {
                            try
                            {
                                var point = light.GetPointAtParameter(i + (double)k / Samples);
                                raw.Add(new Point2D(point.X, point.Y));
                            }
                            catch
                            {
                                break; // best effort: fall back to the plain vertices for this segment
                            }
                        }
                    }
                }

                return FinishOutline(raw, light.Closed, out points);
            }

            if (entity is Polyline2d heavy)
            {
                var raw = new List<Point2D>();
                foreach (ObjectId vertexId in heavy)
                {
                    if (transaction.GetObject(vertexId, OpenMode.ForRead) is Vertex2d vertex &&
                        (vertex.VertexType == Vertex2dType.SimpleVertex || vertex.VertexType == Vertex2dType.CurveFitVertex))
                    {
                        raw.Add(new Point2D(vertex.Position.X, vertex.Position.Y));
                    }
                }

                return FinishOutline(raw, heavy.Closed, out points);
            }

            if (entity is Polyline3d poly3d)
            {
                var raw = new List<Point2D>();
                foreach (ObjectId vertexId in poly3d)
                {
                    if (transaction.GetObject(vertexId, OpenMode.ForRead) is PolylineVertex3d vertex &&
                        vertex.VertexType == Vertex3dType.SimpleVertex)
                    {
                        raw.Add(new Point2D(vertex.Position.X, vertex.Position.Y)); // flattened to XY
                    }
                }

                return FinishOutline(raw, poly3d.Closed, out points);
            }

            return false;
        }

        /// <summary>Validate/normalize an outline: closed by flag or by coincident endpoints; drops the duplicate
        /// closing vertex and consecutive duplicates. False when open or fewer than 3 distinct vertices remain.</summary>
        private static bool FinishOutline(List<Point2D> raw, bool closedFlag, out List<Point2D> points)
        {
            points = null;
            if (raw == null || raw.Count < 3)
            {
                return false;
            }

            var first = raw[0];
            var last = raw[raw.Count - 1];
            var coincident = Math.Abs(first.X - last.X) <= OutlineCloseTolerance &&
                             Math.Abs(first.Y - last.Y) <= OutlineCloseTolerance;
            if (!closedFlag && !coincident)
            {
                return false;
            }

            if (coincident)
            {
                raw.RemoveAt(raw.Count - 1); // the duplicate closing vertex
            }

            // Consecutive duplicates would create zero-length polygon edges; drop them.
            var cleaned = new List<Point2D>(raw.Count);
            foreach (var point in raw)
            {
                if (cleaned.Count == 0 ||
                    Math.Abs(point.X - cleaned[cleaned.Count - 1].X) > 1e-9 ||
                    Math.Abs(point.Y - cleaned[cleaned.Count - 1].Y) > 1e-9)
                {
                    cleaned.Add(point);
                }
            }

            if (cleaned.Count < 3)
            {
                return false;
            }

            points = cleaned;
            return true;
        }

        /// <summary>A useful "no envelope" message: says WHAT was found on the layer and how to arreglarlo.</summary>
        private static string OutlineProblem(string layerName, int anything, int openOutlines, int looseLines, int splines)
        {
            if (anything == 0)
            {
                return "no hay entidades en la capa '" + layerName + "'. Revisa el nombre de la capa y que el contorno " +
                       "esté en el MODELO (no dentro de un bloque ni de un xref).";
            }

            var message = "no encontré una polilínea CERRADA en la capa '" + layerName + "' que sirva de contorno del sitio.";
            if (openOutlines > 0)
            {
                message += " Hay " + openOutlines + " polilínea(s) ABIERTA(s): ciérrala con PEDIT → Cerrar, o termina el trazo exactamente en el punto inicial.";
            }

            if (looseLines > 0)
            {
                message += " Hay " + looseLines + " línea(s) sueltas: únelas en una polilínea cerrada (PEDIT → Juntar).";
            }

            if (splines > 0)
            {
                message += " Hay " + splines + " spline(s): conviértela a polilínea (PEDIT → asumir/convertir, o FLATTEN).";
            }

            return message;
        }

        private static double BoundingArea(Entity entity)
        {
            try
            {
                var extents = entity.GeometricExtents;
                return (extents.MaxPoint.X - extents.MinPoint.X) * (extents.MaxPoint.Y - extents.MinPoint.Y);
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>Place every kept cell as a LINKED reference of the seed's planta definition, plus its label. Cells
        /// are rack MIN CORNERS, so each reference goes at (cell − offset), where the offset is the seed's own
        /// origin→bbox-min gap — recomputed for the rotated orientation (+90° CCW about the insertion turns the AABB
        /// offset (ox, oy, fx, fy) into (−(oy+fy), ox) with spans swapped).</summary>
        private static int PlaceFill(Document document, WarehouseAutoFillResult fill, PlantaSeed seed)
        {
            var database = document.Database;
            var rotated = fill.Orientation == RackOrientation.Rotated;
            var offsetX = rotated ? -(seed.OffsetY + seed.FootprintY) : seed.OffsetX;
            var offsetY = rotated ? seed.OffsetX : seed.OffsetY;
            var rotation = seed.Rotation + (rotated ? Math.PI / 2.0 : 0.0);
            var textHeight = Math.Max(Math.Min(fill.FootprintX, fill.FootprintY) * 0.15, 3.0);
            var placed = 0;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                var labelLayer = LayerHelper.EnsureLayer(database, transaction, LayoutLabelLayer, 4); // cyan, shared with RACKLAYOUT

                foreach (var cell in fill.Cells)
                {
                    var reference = new BlockReference(new Point3d(cell.X - offsetX, cell.Y - offsetY, 0.0), seed.DefinitionId)
                    {
                        Rotation = rotation,
                        ScaleFactors = seed.Scale
                    };
                    modelSpace.AppendEntity(reference);
                    transaction.AddNewlyCreatedDBObject(reference, true);
                    placed++;

                    // The label sits at the footprint's center — cells are min corners, so this is direct.
                    var center = new Point3d(cell.X + fill.FootprintX / 2.0, cell.Y + fill.FootprintY / 2.0, 0.0);
                    var label = new DBText
                    {
                        Height = textHeight,
                        TextString = cell.Label,
                        LayerId = labelLayer,
                        HorizontalMode = TextHorizontalMode.TextCenter,
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        Position = center,
                        AlignmentPoint = center
                    };
                    modelSpace.AppendEntity(label);
                    transaction.AddNewlyCreatedDBObject(label, true);
                    label.AdjustAlignment(database);
                }

                transaction.Commit();
            }

            document.Editor.Regen();
            return placed;
        }

        /// <summary>True when the measured seed copy's bounding box overlaps the site's bounding box.</summary>
        private static bool SeedInsideSite(PlantaSeed seed, WarehouseSite site)
        {
            var x0 = seed.OriginX + seed.OffsetX;
            var y0 = seed.OriginY + seed.OffsetY;
            var x1 = x0 + seed.FootprintX;
            var y1 = y0 + seed.FootprintY;
            return x0 < site.OriginX + site.Width && x1 > site.OriginX
                && y0 < site.OriginY + site.Depth && y1 > site.OriginY;
        }

        /// <summary>Yes/no keyword prompt; Enter takes the default, Esc counts as no.</summary>
        private static bool AskYes(Editor editor, string message, bool defaultYes)
        {
            var options = new PromptKeywordOptions(message);
            options.Keywords.Add("Si");
            options.Keywords.Add("No");
            options.Keywords.Default = defaultYes ? "Si" : "No";
            options.AllowNone = true;
            var result = editor.GetKeywords(options);

            if (result.Status == PromptStatus.None)
            {
                return defaultYes;
            }

            return result.Status == PromptStatus.OK && string.Equals(result.StringResult, "Si", StringComparison.OrdinalIgnoreCase);
        }
    }
}
