using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Projects the shared safety selection model onto the current pallet-flow lateral cut. Left and Right are the
    /// physical exit/entrance ends. BOTA and LATERAL sit at the real endpoint plate origins; a selected LATERAL replaces
    /// the boots for this cut and receives LONGITUD = the complete system length. DESVIADOR keeps the selective vertical
    /// contract: level one is measured from TROQUEL_LARGUERO and upper levels sit 6&quot; below their endpoint IN/OUT beam.
    /// </summary>
    public sealed class DynamicSafetyLateralBuilder
    {
        public const string View = "LATERAL";

        public IReadOnlyList<HeaderBlockInstance> Build(
            DynamicRackSystem system,
            RackCatalog catalog,
            IReadOnlyList<HeaderBlockInstance> frameInstances,
            int postIndex = 0,
            int levelCount = int.MaxValue,
            double? startX = null,
            double? endX = null,
            IReadOnlyList<DynamicRackFront> adjacentFronts = null)
        {
            var result = new List<HeaderBlockInstance>();
            if (system == null || catalog == null || system.TotalLength <= 0.0)
            {
                return result;
            }

            var sectionStart = startX ?? 0.0;
            var sectionEnd = endX ?? system.TotalLength;
            var left = Endpoint(frameInstances, catalog, sectionStart);
            var right = Endpoint(frameInstances, catalog, sectionEnd);
            var laterales = SelectiveSafetyPlacement.EnabledOfType(
                system.SafetySelections, catalog, View, SelectiveSafetyPlacement.LateralType, allowEmptySide: true);
            var lateralSide = laterales.Count > 0
                ? DynamicLateralGuardPlan.SideAt(
                    laterales[0].Selection, postIndex, Math.Max(1, system.Fronts.Count + 1))
                : SafetySide.None;

            if (lateralSide != SafetySide.None)
            {
                AppendEndpointFamily(
                    result, laterales, left.PlateOrigin, right.PlateOrigin,
                    sectionEnd - sectionStart, postIndex, lateralSide);
            }
            else
            {
                var botas = SelectiveSafetyPlacement.EnabledOfType(
                    system.SafetySelections, catalog, View, SelectiveSafetyPlacement.BotaType);
                AppendEndpointFamily(result, botas, left.PlateOrigin, right.PlateOrigin, null, postIndex);
            }

            AppendDesviadores(
                result,
                system,
                catalog,
                left,
                right,
                postIndex,
                levelCount,
                sectionStart,
                sectionEnd,
                adjacentFronts);
            AppendDefensas(result, system, catalog, left, right, postIndex);
            AppendGuias(result, system, catalog, postIndex);
            return result;
        }

        private static void AppendDefensas(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            EndpointGeometry left,
            EndpointGeometry right,
            int postIndex)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            if (selection == null)
            {
                return;
            }

            var block = CatalogLookup.Block(catalog, selection.ElementId, View);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var setting = DynamicForkliftDefensePlan.At(
                selection.DefensaPosts, postIndex, Math.Max(1, system.Fronts.Count + 1));
            var offset = CatalogLookup.Local(
                catalog, selection.ElementId, DynamicForkliftDefensePlan.PostOriginPoint, View);
            if (setting.DrawsExit)
            {
                target.Add(Piece(
                    selection.ElementId,
                    block,
                    new Point2D(left.PostOrigin.X + offset.X, left.PlateOrigin.Y + offset.Y),
                    mirrored: false,
                    setting.ExitLength));
            }

            if (setting.DrawsEntrance)
            {
                target.Add(Piece(
                    selection.ElementId,
                    block,
                    new Point2D(right.PostOrigin.X - offset.X, right.PlateOrigin.Y + offset.Y),
                    mirrored: true,
                    setting.EntranceLength));
            }
        }

        private static void AppendGuias(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            int postIndex)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.GuiaType);
            if (selection == null)
            {
                return;
            }

            var block = CatalogLookup.Block(catalog, selection.ElementId, View);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            foreach (var placement in DynamicEntranceGuidePlan.Build(system, selection)
                         .Where(placement => placement.PostIndex == postIndex)
                         .GroupBy(placement => new
                         {
                             X = Math.Round(system.Fronts[placement.FrontIndex].EndX, 6),
                             Y = Math.Round(placement.Elevation, 6),
                             Length = Math.Round(placement.Length, 6)
                         })
                         .Select(group => group.First()))
            {
                target.Add(Piece(
                    selection.ElementId,
                    block,
                    new Point2D(system.Fronts[placement.FrontIndex].EndX, placement.Elevation),
                    mirrored: true,
                    placement.Length));
            }
        }

        private static void AppendEndpointFamily(
            ICollection<HeaderBlockInstance> target,
            IReadOnlyList<SelectiveSafetyPlacement.SafetyElement> elements,
            Point2D left,
            Point2D right,
            double? longitud,
            int postIndex,
            SafetySide? sideOverride = null)
        {
            foreach (var element in elements ?? Array.Empty<SelectiveSafetyPlacement.SafetyElement>())
            {
                var side = sideOverride ?? element.Selection.SideForPost(postIndex);
                if (side == SafetySide.Left || side == SafetySide.Both)
                {
                    target.Add(Piece(element.PieceId, element.Block, left, mirrored: false, longitud));
                }

                if (side == SafetySide.Right || side == SafetySide.Both)
                {
                    target.Add(Piece(element.PieceId, element.Block, right, mirrored: true, longitud));
                }
            }
        }

        private static void AppendDesviadores(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            EndpointGeometry left,
            EndpointGeometry right,
            int postIndex,
            int levelCount,
            double startX,
            double endX,
            IReadOnlyList<DynamicRackFront> adjacentFronts)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections,
                catalog.SafetyElements,
                SelectiveSafetyDefaults.DesviadorType);
            if (selection == null || system.LoadBeamLevels.Count == 0)
            {
                return;
            }

            var block = CatalogLookup.Block(catalog, selection.ElementId, View);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var longitud = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorLongitud)
                ? selection.DesviadorLongitud
                : SelectiveSafetyDefaults.DesviadorLongitud;
            var firstHeight = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorPrimerNivelAltura)
                ? selection.DesviadorPrimerNivelAltura
                : SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;
            var firstLeftY = FirstTroquelY(catalog, left.PostId, left.PostPeralte) + firstHeight;
            var firstRightY = FirstTroquelY(catalog, right.PostId, right.PostPeralte) + firstHeight;
            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);

            var fronts = adjacentFronts ?? Array.Empty<DynamicRackFront>();
            var leftLevels = fronts.OrderBy(front => front.StartX)
                .Select(front => DynamicFrontGeometry.LoadBeamLevels(system, front))
                .FirstOrDefault() ?? system.LoadBeamLevels.ToList();
            var rightLevels = fronts.OrderByDescending(front => front.EndX)
                .Select(front => DynamicFrontGeometry.LoadBeamLevels(system, front))
                .FirstOrDefault() ?? system.LoadBeamLevels.ToList();
            var count = Math.Min(levelCount, Math.Max(leftLevels.Count, rightLevels.Count));
            for (var level = 0; level < count; level++)
            {
                if (off.Contains((postIndex, level)))
                {
                    continue;
                }

                var leftLoad = leftLevels[Math.Min(level, leftLevels.Count - 1)];
                var rightLoad = rightLevels[Math.Min(level, rightLevels.Count - 1)];
                var leftY = level == 0 ? firstLeftY : leftLoad.ExitElevation - SelectiveDesviadorPlan.BeamYOffset;
                var rightY = level == 0 ? firstRightY : rightLoad.EntranceElevation - SelectiveDesviadorPlan.BeamYOffset;

                if (selection.Side == SafetySide.Left || selection.Side == SafetySide.Both)
                {
                    target.Add(Piece(selection.ElementId, block, new Point2D(startX, leftY), mirrored: false, longitud));
                }

                if (selection.Side == SafetySide.Right || selection.Side == SafetySide.Both)
                {
                    target.Add(Piece(selection.ElementId, block, new Point2D(endX, rightY), mirrored: true, longitud));
                }
            }
        }

        private static double FirstTroquelY(RackCatalog catalog, string postId, double peralte)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                SelectiveRackDefaults.View);
            return SelectivePostGeometry.Resolve(entry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            }).Y;
        }

        private static EndpointGeometry Endpoint(IReadOnlyList<HeaderBlockInstance> instances, RackCatalog catalog, double x)
        {
            var candidates = instances ?? Array.Empty<HeaderBlockInstance>();
            var plate = candidates
                .Where(instance => instance.Role == HeaderBlockRole.BasePlate)
                .OrderBy(instance => Math.Abs(instance.ConnectionAnchor.X - x))
                .FirstOrDefault();
            var post = candidates
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .OrderBy(instance => Math.Abs(instance.ConnectionAnchor.X - x))
                .FirstOrDefault();
            var plateAt = plate != null && Math.Abs(plate.ConnectionAnchor.X - x) <= 1e-4
                ? plate.Insertion
                : new Point2D(x, 0.0);
            var postId = post != null && Math.Abs(post.ConnectionAnchor.X - x) <= 1e-4
                ? post.PieceId
                : null;
            var peralte = candidates
                .Where(instance => instance.Role == HeaderBlockRole.Post && string.Equals(instance.PieceId, postId, StringComparison.OrdinalIgnoreCase))
                .Select(instance => instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.PeralteParam, out var value) ? value : 0.0)
                .FirstOrDefault(value => value > 0.0);
            if (peralte <= 0.0)
            {
                peralte = catalog?.PostProfiles.FindProfile(postId)?.Width ?? 0.0;
            }

            var postAt = post != null && Math.Abs(post.ConnectionAnchor.X - x) <= 1e-4
                ? post.ConnectionAnchor
                : new Point2D(x, 0.0);
            return new EndpointGeometry(plateAt, postAt, postId, peralte);
        }

        private static HeaderBlockInstance Piece(
            string pieceId,
            string block,
            Point2D at,
            bool mirrored,
            double? longitud)
        {
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = pieceId,
                BlockName = block,
                View = View,
                Insertion = at,
                ConnectionAnchor = at,
                MirroredX = mirrored
            };

            if (longitud.HasValue && longitud.Value > 0.0)
            {
                instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud.Value;
            }

            return instance;
        }

        private readonly struct EndpointGeometry
        {
            public EndpointGeometry(Point2D plateOrigin, Point2D postOrigin, string postId, double postPeralte)
            {
                PlateOrigin = plateOrigin;
                PostOrigin = postOrigin;
                PostId = postId;
                PostPeralte = postPeralte;
            }

            public Point2D PlateOrigin { get; }
            public Point2D PostOrigin { get; }
            public string PostId { get; }
            public double PostPeralte { get; }
        }
    }
}
