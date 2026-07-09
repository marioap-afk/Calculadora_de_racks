using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// Builds the PLANTA (top-down) view of a cabecera as loose block instances: the two post FOOTPRINTS (front at
    /// the origin, back at X = fondo) with their base plates, plus the celosía collapsed to a single member between
    /// the posts. Pure — no AutoCAD.
    ///
    /// The _PLANTA post/plate blocks are VIEW-SPECIFIC (not shared with the lateral/frontal), so they carry their own
    /// base orientation in the library; here the two posts are the SAME block MIRRORED (front + back) so their omegas
    /// open inward. The celosía LONGITUD is NOT the raw fondo: it is the travesaño's cut length, computed exactly like
    /// the frontal/lateral view (fondo - 2 × the post's celosía-troquel inset). Peraltes: post = post peralte, plate =
    /// override/standard, celosía = post peralte - 1". Axes: X = fondo (between the posts), Y = the post's face.
    /// </summary>
    public sealed class PlantaHeaderLayoutBuilder
    {
        public const string View = "PLANTA";
        private const string LateralView = "LATERAL";

        private const string MontajePostePoint = "MONTAJE_POSTE";
        private const string TroquelCelosiaPoint = "TROQUEL_CELOSIA";
        private const string CelosiaPoint = "CELOSIA";
        private const string LengthParam = "LONGITUD";
        private const string PeralteParam = "PERALTE";

        public IReadOnlyList<HeaderBlockInstance> Build(RackFrameConfiguration config, RackCatalog catalog)
        {
            var instances = new List<HeaderBlockInstance>();
            if (config == null)
            {
                return instances;
            }

            var depth = config.Depth > 0.0 ? config.Depth : 42.0;
            var postId = FirstNonEmpty(config.LeftPost?.PostCatalogId, catalog?.Defaults?.Post);
            var plateId = FirstNonEmpty(config.LeftBasePlate?.PlateCatalogId, catalog?.Defaults?.BasePlate);
            var trussId = ResolveTrussId(config, catalog);

            var postPeralte = PostWidth(catalog, postId);
            var celosiaPeralte = Math.Max(0.0, postPeralte - 1.0);
            var plateEntry = catalog?.BasePlates.FindBasePlate(plateId);
            var platePeralte = config.LeftBasePlate?.PeralteOverride ?? plateEntry?.StandardPeralte(postPeralte) ?? 0.0;

            var postBlock = Block(catalog, postId, View);
            var plateBlock = Block(catalog, plateId, View);
            var trussBlock = Block(catalog, trussId, View);

            var montaje = Local(catalog, plateId, MontajePostePoint, View);
            var celosia = Local(catalog, trussId, CelosiaPoint, View);
            // The travesaño CUT length matches the frontal/lateral: the beam mates at the post's celosía troquel
            // (inset = its X in the LATERAL view) but its steel overhangs the mate by the ménsula (the CELOSIA point's
            // X) on each side. So the A-corte = fondo - 2 × (troquelInset - ménsula), same value as the lateral view.
            var troquelInset = Local(catalog, postId, TroquelCelosiaPoint, LateralView).X;
            var celosiaY = Local(catalog, postId, TroquelCelosiaPoint, View).Y;

            var front = new Point2D(0.0, 0.0);
            var back = new Point2D(depth, 0.0);

            // Post footprints + plates. The _PLANTA block already carries the right base orientation (rotated in the
            // library); here we only MIRROR the back one so both omegas open inward.
            AddPost(instances, postId, postBlock, front, postPeralte, mirrored: false);
            AddPlate(instances, plateId, plateBlock, montaje, front, platePeralte, mirrored: false);
            AddPost(instances, postId, postBlock, back, postPeralte, mirrored: true);
            AddPlate(instances, plateId, plateBlock, montaje, back, platePeralte, mirrored: true);

            // Celosía: one member spanning between the two posts, cut to the travesaño A-corte (mate inset minus the
            // ménsula overhang on each side) — the same length the travesaño has in the lateral view.
            var longitud = depth - 2.0 * (troquelInset - celosia.X);
            if (longitud > 1e-6 && !string.IsNullOrWhiteSpace(trussBlock))
            {
                var anchor = new Point2D(troquelInset, celosiaY);
                var beam = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Horizontal,
                    PieceId = trussId,
                    BlockName = trussBlock,
                    View = View,
                    ConnectionAnchor = anchor,
                    Insertion = new Point2D(anchor.X - celosia.X, anchor.Y - celosia.Y)
                };
                beam.DynamicParameters[LengthParam] = longitud;
                beam.DynamicParameters[PeralteParam] = celosiaPeralte;
                instances.Add(beam);
            }

            return instances;
        }

        private static void AddPost(ICollection<HeaderBlockInstance> instances, string postId, string block, Point2D origin, double peralte, bool mirrored)
        {
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = postId,
                BlockName = block,
                View = View,
                MirroredX = mirrored,
                ConnectionAnchor = origin,
                Insertion = origin
            };
            if (peralte > 0.0)
            {
                post.DynamicParameters[PeralteParam] = peralte;
            }

            instances.Add(post);
        }

        private static void AddPlate(ICollection<HeaderBlockInstance> instances, string plateId, string block, Point2D montaje, Point2D postOrigin, double peralte, bool mirrored)
        {
            var sign = mirrored ? -1.0 : 1.0;
            var plate = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = plateId,
                BlockName = block,
                View = View,
                MirroredX = mirrored,
                ConnectionAnchor = postOrigin,
                Insertion = new Point2D(postOrigin.X - sign * montaje.X, postOrigin.Y - montaje.Y)
            };
            if (peralte > 0.0)
            {
                plate.DynamicParameters[PeralteParam] = peralte;
            }

            instances.Add(plate);
        }

        private static double PostWidth(RackCatalog catalog, string postId)
        {
            var width = catalog?.PostProfiles
                .FirstOrDefault(p => string.Equals(p?.Id, postId, StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : 3.0;
        }

        private static string ResolveTrussId(RackFrameConfiguration config, RackCatalog catalog)
        {
            var fromHorizontal = config.Horizontals?
                .Select(h => h.ProfileId)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
            if (!string.IsNullOrWhiteSpace(fromHorizontal))
            {
                return fromHorizontal;
            }

            var fromPanel = config.BracingPanels?
                .Select(p => p.DiagonalProfileId)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
            return FirstNonEmpty(fromPanel, catalog?.Defaults?.HorizontalProfile, catalog?.Defaults?.DiagonalProfile);
        }

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;

        private static string FirstNonEmpty(params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return null;
        }
    }
}
