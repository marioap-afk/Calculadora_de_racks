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

        // Aliases of the shared ids (CatalogIds / SelectiveRackDefaults are the single source of truth).
        private const string MontajePostePoint = RackFrames.CatalogIds.BasePlateConnectionPoint;
        private const string TroquelCelosiaPoint = RackFrames.CatalogIds.BraceStartConnectionPoint;
        private const string CelosiaPoint = RackFrames.CatalogIds.BraceEndConnectionPoint;
        private const string LengthParam = Domain.Systems.SelectiveRackDefaults.LengthParam;
        private const string PeralteParam = Domain.Systems.SelectiveRackDefaults.PeralteParam;

        /// <summary>Fallback fondo when a config carries none (the standard template's default depth).</summary>
        private const double FallbackDepth = 42.0;

        /// <summary>Business rule: the celosía peralte is one inch less than the post peralte.</summary>
        private const double CelosiaPeralteReduction = 1.0;

        /// <summary>
        /// The cabecera's planta instances. <paramref name="origin"/> shifts the whole frame (used by the selective
        /// planta to stack one frame per frente along Y); default is the frame's own origin.
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> Build(RackFrameConfiguration config, RackCatalog catalog, Point2D origin = default)
        {
            var instances = new List<HeaderBlockInstance>();
            if (config == null)
            {
                return instances;
            }

            var depth = config.Depth > 0.0 ? config.Depth : FallbackDepth;

            // Each side resolves ITS OWN post/plate/peralte — the configurator edits them per side, and the
            // lateral view honours that; the planta must not apply the left side's values to both.
            var postId = FirstNonEmpty(config.LeftPost?.PostCatalogId, catalog?.Defaults?.Post);
            var plateId = FirstNonEmpty(config.LeftBasePlate?.PlateCatalogId, catalog?.Defaults?.BasePlate);
            var backPostId = FirstNonEmpty(config.RightPost?.PostCatalogId, postId);
            var backPlateId = FirstNonEmpty(config.RightBasePlate?.PlateCatalogId, plateId);
            var trussId = ResolveTrussId(config, catalog);

            var postPeralte = PostWidth(catalog, postId);
            var backPostPeralte = PostWidth(catalog, backPostId);
            var celosiaPeralte = Math.Max(0.0, postPeralte - CelosiaPeralteReduction);
            var plateEntry = catalog?.BasePlates.FindBasePlate(plateId);
            var backPlateEntry = catalog?.BasePlates.FindBasePlate(backPlateId);
            var platePeralte = config.LeftBasePlate?.PeralteOverride ?? plateEntry?.StandardPeralte(postPeralte) ?? 0.0;
            var backPlatePeralte = config.RightBasePlate?.PeralteOverride ?? backPlateEntry?.StandardPeralte(backPostPeralte) ?? 0.0;

            var postBlock = Block(catalog, postId, View);
            var plateBlock = Block(catalog, plateId, View);
            var backPostBlock = Block(catalog, backPostId, View);
            var backPlateBlock = Block(catalog, backPlateId, View);
            var trussBlock = Block(catalog, trussId, View);

            var montaje = Local(catalog, plateId, MontajePostePoint, View);
            var backMontaje = Local(catalog, backPlateId, MontajePostePoint, View);
            var celosia = Local(catalog, trussId, CelosiaPoint, View);
            // The travesaño CUT length matches the frontal/lateral: the beam mates at the post's celosía troquel
            // (inset = its X in the LATERAL view) but its steel overhangs the mate by the ménsula (the CELOSIA point's
            // X) on each side. So the A-corte = fondo - 2 × (troquelInset - ménsula), same value as the lateral view.
            var troquelInset = Local(catalog, postId, TroquelCelosiaPoint, LateralView).X;
            var celosiaY = Local(catalog, postId, TroquelCelosiaPoint, View).Y;

            var front = new Point2D(origin.X, origin.Y);
            var back = new Point2D(origin.X + depth, origin.Y);

            // Post footprints + plates. The _PLANTA block already carries the right base orientation (rotated in the
            // library); here we only MIRROR the back one so both omegas open inward.
            AddPost(instances, postId, postBlock, front, postPeralte, mirrored: false);
            AddPlate(instances, plateId, plateBlock, montaje, front, platePeralte, mirrored: false);
            AddPost(instances, backPostId, backPostBlock, back, backPostPeralte, mirrored: true);
            AddPlate(instances, backPlateId, backPlateBlock, backMontaje, back, backPlatePeralte, mirrored: true);

            // Celosía: one member spanning between the two posts, cut to the travesaño A-corte (mate inset minus the
            // ménsula overhang on each side) — the same length the travesaño has in the lateral view.
            var longitud = depth - 2.0 * (troquelInset - celosia.X);
            if (longitud > 1e-6 && !string.IsNullOrWhiteSpace(trussBlock))
            {
                var anchor = new Point2D(origin.X + troquelInset, origin.Y + celosiaY);
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
