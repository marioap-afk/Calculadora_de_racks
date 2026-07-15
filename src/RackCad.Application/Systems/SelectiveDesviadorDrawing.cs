using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>Projects the physical <see cref="SelectiveDesviadorPlan"/> into each 2D view without changing its BOM count.</summary>
    internal static class SelectiveDesviadorDrawing
    {
        public static void AppendFrontal(
            ICollection<HeaderBlockInstance> target,
            SelectiveRackSystem system,
            RackCatalog catalog,
            string view)
        {
            if (!TryResolve(system, catalog, view, out var pieceId, out var block, out var plan)) return;

            // The two aisle faces project onto the same frontal post. Keep one visible reference when their Y agrees;
            // the physical plan (and therefore the BOM) still owns both pieces.
            var seen = new HashSet<(double X, double Y)>();
            foreach (var spot in plan.Spots.Where(s => s.Enabled).OrderBy(s => s.RunPostX).ThenBy(s => s.Y).ThenBy(s => s.Face))
            {
                var troquel = Troquel(catalog, system.PostId, view, spot.PostPeralte);
                var at = new Point2D(spot.RunPostX + troquel.X, spot.Y);
                if (seen.Add((Round(at.X), Round(at.Y))))
                {
                    target.Add(Piece(pieceId, block, view, at, mirrored: false, plan.Longitud));
                }
            }
        }

        public static void AppendLateral(
            ICollection<HeaderBlockInstance> target,
            SelectiveRackSystem system,
            RackCatalog catalog,
            int postIndex,
            double runPostX,
            double anchorDepthX)
        {
            if (!TryResolve(system, catalog, SelectiveLateralBuilder.LateralView, out var pieceId, out var block, out var plan)) return;

            foreach (var spot in plan.Spots
                         .Where(s => s.Enabled && Math.Abs(s.RunPostX - runPostX) <= 1e-4)
                         .OrderBy(s => s.Y).ThenBy(s => s.Face))
            {
                var at = new Point2D(spot.DepthPostX - anchorDepthX, spot.Y);
                target.Add(Piece(pieceId, block, SelectiveLateralBuilder.LateralView, at, spot.Mirrored, plan.Longitud));
            }
        }

        public static void AppendPlanta(
            ICollection<HeaderBlockInstance> target,
            SelectiveRackSystem system,
            RackCatalog catalog,
            string view)
        {
            if (!TryResolve(system, catalog, view, out var pieceId, out var block, out var plan)) return;

            // Heights overlap in plan. One reference per physical post/aisle face represents every selected level;
            // the plan's PhysicalQuantity retains the full vertical count for the BOM.
            foreach (var spot in plan.Spots.Where(s => s.Enabled)
                         .GroupBy(s => (s.Face, X: Round(s.RunPostX)))
                         .Select(g => g.OrderBy(s => s.Level).First())
                         .OrderBy(s => s.RunPostX).ThenBy(s => s.Face))
            {
                // The PLANTA block contract is origin-to-origin with the post. Unlike frontal/lateral, this view
                // must not inherit the TROQUEL_LARGUERO offset; the back aisle face is represented only by mirror.
                var at = new Point2D(spot.DepthPostX, spot.RunPostX);
                target.Add(Piece(pieceId, block, view, at, spot.Mirrored, plan.Longitud));
            }
        }

        private static bool TryResolve(
            SelectiveRackSystem system,
            RackCatalog catalog,
            string view,
            out string pieceId,
            out string block,
            out SelectiveDesviadorPlan.Result plan)
        {
            pieceId = null;
            block = null;
            plan = SelectiveDesviadorPlan.Build(system, catalog);
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system?.SafetySelections,
                catalog?.SafetyElements,
                SelectiveSafetyDefaults.DesviadorType);
            if (selection == null || plan.Spots.Count == 0) return false;

            pieceId = selection.ElementId;
            block = CatalogLookup.Block(catalog, pieceId, view);
            return !string.IsNullOrWhiteSpace(block);
        }

        private static Point2D Troquel(RackCatalog catalog, string postId, string view, double peralte)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(postId, SelectiveRackDefaults.PostBeamPoint, view);
            return SelectivePostGeometry.Resolve(entry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            });
        }

        private static HeaderBlockInstance Piece(
            string pieceId,
            string block,
            string view,
            Point2D at,
            bool mirrored,
            double longitud)
        {
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = pieceId,
                BlockName = block,
                View = view,
                Insertion = at,
                ConnectionAnchor = at,
                MirroredX = mirrored
            };
            instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud;
            return instance;
        }

        private static double Round(double value) => Math.Round(value, 4);
    }
}
