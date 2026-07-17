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
    /// Projects the dynamic rack's shared BOTA/LATERAL/DESVIADOR selections into its transverse cuts and plan.
    /// Left is the exit end and Right the entrance end. A lateral guard replaces the boots at the same post, matching
    /// the mature selective placement contract; plan collapses repeated load levels to one visible reference.
    /// </summary>
    public sealed class DynamicSafetyMultiViewBuilder
    {
        public void AppendFrontal(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            string plateId,
            DynamicRackEnd end)
        {
            if (target == null || system == null || catalog == null || layout?.PostPositions == null)
            {
                return;
            }

            const string view = "FRONTAL";
            var boots = SelectiveSafetyPlacement.EnabledOfType(
                system.SafetySelections, catalog, view, SelectiveSafetyPlacement.BotaType);
            var guards = SelectiveSafetyPlacement.EnabledOfType(
                system.SafetySelections, catalog, view, SelectiveSafetyPlacement.LateralType, allowEmptySide: true);
            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                var origin = new Point2D(layout.PostPositions[postIndex], 0.0);
                var at = new Point2D(origin.X - plateMate.X, origin.Y - plateMate.Y);
                var depthRange = DynamicDepthGeometry.AtPost(system, postIndex);
                var rangeStart = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? system.TotalLength;
                var guard = guards.FirstOrDefault(element => DynamicLateralGuardPlan.DrawsAtEnd(
                    element.Selection, postIndex, postCount: layout.PostPositions.Count, end: end));
                if (guard != null)
                {
                    target.Add(Piece(guard.PieceId, guard.Block, view, at,
                        mirroredX: end == DynamicRackEnd.Entrance, mirroredY: false, rangeEnd - rangeStart));
                    continue;
                }

                var boot = boots.FirstOrDefault(element => DrawsAtEnd(element.Selection, postIndex, end));
                if (boot != null)
                {
                    target.Add(Piece(boot.PieceId, boot.Block, view, at,
                        mirroredX: end == DynamicRackEnd.Entrance, mirroredY: false, null));
                }
            }

            AppendFrontalDesviadores(target, system, catalog, layout, end);
            AppendFrontalDefensas(target, system, catalog, layout, plateId, end);
            AppendFrontalGuias(target, system, catalog, layout, end);
        }

        public void AppendPlanta(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            string plateId)
        {
            if (target == null || system == null || catalog == null || layout?.PostPositions == null)
            {
                return;
            }

            const string view = "PLANTA";
            var boots = SelectiveSafetyPlacement.EnabledOfType(
                system.SafetySelections, catalog, view, SelectiveSafetyPlacement.BotaType);
            var guards = SelectiveSafetyPlacement.EnabledOfType(
                system.SafetySelections, catalog, view, SelectiveSafetyPlacement.LateralType, allowEmptySide: true);

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                var depthRange = DynamicDepthGeometry.AtPost(system, postIndex);
                var rangeStart = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? system.TotalLength;
                var at = new Point2D(rangeStart, layout.PostPositions[postIndex]);
                var guardSide = guards.Count > 0
                    ? DynamicLateralGuardPlan.SideAt(guards[0].Selection, postIndex, layout.PostPositions.Count)
                    : SafetySide.None;
                if (guardSide != SafetySide.None)
                {
                    SelectiveSafetyPlacement.AppendAtPost(
                        target, catalog, view, guards, at, plateId, postIndex,
                        longitud: rangeEnd - rangeStart, mirrorYInPlace: true, sideOverride: guardSide);
                }
                else
                {
                    SelectiveSafetyPlacement.AppendAtPost(
                        target, catalog, view, boots, at, plateId, postIndex,
                        mirrorAxisX: (rangeStart + rangeEnd) / 2.0);
                }
            }

            AppendPlantaDesviadores(target, system, catalog, layout);
            AppendPlantaDefensas(target, system, catalog, layout);
            AppendPlantaGuias(target, system, catalog, layout);
        }

        private static void AppendFrontalDefensas(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            string plateId,
            DynamicRackEnd end)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            if (selection == null)
            {
                return;
            }

            const string view = "FRONTAL";
            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var offset = CatalogLookup.Local(
                catalog, selection.ElementId, DynamicForkliftDefensePlan.PostOriginPoint, view);
            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);
            var postCount = layout.PostPositions.Count;
            for (var postIndex = 0; postIndex < postCount; postIndex++)
            {
                var setting = DynamicForkliftDefensePlan.At(selection.DefensaPosts, postIndex, postCount);
                var draws = end == DynamicRackEnd.Exit ? setting.DrawsExit : setting.DrawsEntrance;
                if (!draws)
                {
                    continue;
                }

                var direction = end == DynamicRackEnd.Exit ? 1.0 : -1.0;
                target.Add(Piece(
                    selection.ElementId,
                    block,
                    view,
                    new Point2D(
                        layout.PostPositions[postIndex] + direction * offset.X,
                        -plateMate.Y + offset.Y),
                    mirroredX: end == DynamicRackEnd.Entrance,
                    mirroredY: false,
                    end == DynamicRackEnd.Exit ? setting.ExitLength : setting.EntranceLength));
            }
        }

        private static void AppendFrontalGuias(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            DynamicRackEnd end)
        {
            if (end != DynamicRackEnd.Entrance)
            {
                return;
            }

            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.GuiaType);
            if (selection == null)
            {
                return;
            }

            const string view = "FRONTAL";
            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            foreach (var placement in DynamicEntranceGuidePlan.Build(system, selection))
            {
                if (placement.PostIndex < 0 || placement.PostIndex >= layout.PostPositions.Count)
                {
                    continue;
                }

                var troquel = layout.TroquelPositions[placement.PostIndex];
                var x = layout.PostPositions[placement.PostIndex]
                        + (placement.MirroredAcrossFront ? -troquel : troquel);
                target.Add(Piece(
                    selection.ElementId,
                    block,
                    view,
                    new Point2D(x, placement.Elevation),
                    mirroredX: placement.MirroredAcrossFront,
                    mirroredY: false,
                    placement.Length));
            }
        }

        private static void AppendPlantaGuias(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.GuiaType);
            if (selection == null)
            {
                return;
            }

            const string view = "PLANTA";
            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var postId = DynamicFrontGeometry.PostId(system, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system, catalog, postId);
            var troquelEntry = catalog.ConnectionLayout.FindConnectionLayout(
                postId, SelectiveRackDefaults.PostBeamPoint, view);
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = postPeralte
            });
            var collapsed = DynamicEntranceGuidePlan.Build(system, selection)
                .GroupBy(placement => new
                {
                    placement.FrontIndex,
                    placement.PostIndex,
                    placement.MirroredAcrossFront,
                    Length = Math.Round(placement.Length, 6)
                })
                .Select(group => group.First());
            foreach (var placement in collapsed)
            {
                if (placement.FrontIndex < 0 || placement.FrontIndex >= system.Fronts.Count
                    || placement.PostIndex < 0 || placement.PostIndex >= layout.PostPositions.Count)
                {
                    continue;
                }

                var front = system.Fronts[placement.FrontIndex];
                var y = layout.PostPositions[placement.PostIndex]
                        + (placement.MirroredAcrossFront ? -troquel.Y : troquel.Y);
                target.Add(Piece(
                    selection.ElementId,
                    block,
                    view,
                    new Point2D(front.EndX - troquel.X, y),
                    mirroredX: true,
                    mirroredY: placement.MirroredAcrossFront,
                    placement.Length));
            }
        }

        private static void AppendPlantaDefensas(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            if (selection == null)
            {
                return;
            }

            const string view = "PLANTA";
            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var offset = CatalogLookup.Local(
                catalog, selection.ElementId, DynamicForkliftDefensePlan.PostOriginPoint, view);
            var postCount = layout.PostPositions.Count;
            for (var postIndex = 0; postIndex < postCount; postIndex++)
            {
                var setting = DynamicForkliftDefensePlan.At(selection.DefensaPosts, postIndex, postCount);
                var depthRange = DynamicDepthGeometry.AtPost(system, postIndex);
                var rangeStart = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? system.TotalLength;
                var y = layout.PostPositions[postIndex] + offset.Y;
                if (setting.DrawsExit)
                {
                    target.Add(Piece(selection.ElementId, block, view,
                        new Point2D(rangeStart + offset.X, y), false, false, setting.ExitLength));
                }

                if (setting.DrawsEntrance)
                {
                    target.Add(Piece(selection.ElementId, block, view,
                        new Point2D(rangeEnd - offset.X, y), true, false, setting.EntranceLength));
                }
            }
        }

        private static void AppendFrontalDesviadores(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            DynamicRackEnd end)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            if (selection == null || system.LoadBeamLevels.Count == 0)
            {
                return;
            }

            const string view = "FRONTAL";
            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var postId = DynamicFrontGeometry.PostId(system, catalog);
            var peralte = DynamicFrontGeometry.PostPeralte(system, catalog, postId);
            var troquelEntry = catalog.ConnectionLayout.FindConnectionLayout(
                postId, SelectiveRackDefaults.PostBeamPoint, view);
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            });
            var firstHeight = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorPrimerNivelAltura)
                ? selection.DesviadorPrimerNivelAltura
                : SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;
            var length = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorLongitud)
                ? selection.DesviadorLongitud
                : SelectiveSafetyDefaults.DesviadorLongitud;
            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                if (!DrawsAtEnd(selection, postIndex, end))
                {
                    continue;
                }

                var front = Math.Min(postIndex, Math.Max(0, system.Fronts.Count - 1));
                var levelsAtPost = LoadLevelsAtPost(system, postIndex);
                for (var levelIndex = 0; levelIndex < levelsAtPost && levelIndex < system.LoadBeamLevels.Count; levelIndex++)
                {
                    if (off.Contains((front, levelIndex)))
                    {
                        continue;
                    }

                    var level = system.LoadBeamLevels[levelIndex];
                    var beamY = end == DynamicRackEnd.Entrance ? level.EntranceElevation : level.ExitElevation;
                    var y = levelIndex == 0 ? troquel.Y + firstHeight : beamY - SelectiveDesviadorPlan.BeamYOffset;
                    target.Add(Piece(
                        selection.ElementId,
                        block,
                        view,
                        new Point2D(layout.PostPositions[postIndex] + troquel.X, y),
                        mirroredX: false,
                        mirroredY: false,
                        length));
                }
            }
        }

        private static void AppendPlantaDesviadores(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            if (selection == null || system.LoadBeamLevels.Count == 0)
            {
                return;
            }

            const string view = "PLANTA";
            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var length = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorLongitud)
                ? selection.DesviadorLongitud
                : SelectiveSafetyDefaults.DesviadorLongitud;
            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                var depthRange = DynamicDepthGeometry.AtPost(system, postIndex);
                var rangeStart = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? system.TotalLength;
                var front = Math.Min(postIndex, Math.Max(0, system.Fronts.Count - 1));
                var anyLevel = Enumerable.Range(0, LoadLevelsAtPost(system, postIndex))
                    .Any(level => !off.Contains((front, level)));
                if (!anyLevel)
                {
                    continue;
                }

                var side = selection.SideForPost(postIndex);
                if (side == SafetySide.Left || side == SafetySide.Both)
                {
                    target.Add(Piece(selection.ElementId, block, view,
                        new Point2D(rangeStart, layout.PostPositions[postIndex]), false, false, length));
                }

                if (side == SafetySide.Right || side == SafetySide.Both)
                {
                    target.Add(Piece(selection.ElementId, block, view,
                        new Point2D(rangeEnd, layout.PostPositions[postIndex]), true, false, length));
                }
            }
        }

        private static bool DrawsAtEnd(SelectiveSafetySelection selection, int postIndex, DynamicRackEnd end)
        {
            var side = selection?.SideForPost(postIndex) ?? SafetySide.None;
            return end == DynamicRackEnd.Exit
                ? side == SafetySide.Left || side == SafetySide.Both
                : side == SafetySide.Right || side == SafetySide.Both;
        }

        private static int LoadLevelsAtPost(DynamicRackSystem system, int postIndex)
        {
            if (system?.Fronts == null || system.Fronts.Count == 0)
            {
                return system?.LoadBeamLevels.Count ?? 0;
            }

            var count = 0;
            if (postIndex > 0 && postIndex - 1 < system.Fronts.Count)
            {
                count = Math.Max(count, system.Fronts[postIndex - 1].LoadLevels);
            }

            if (postIndex < system.Fronts.Count)
            {
                count = Math.Max(count, system.Fronts[postIndex].LoadLevels);
            }

            return Math.Max(1, count);
        }

        private static HeaderBlockInstance Piece(
            string pieceId,
            string block,
            string view,
            Point2D at,
            bool mirroredX,
            bool mirroredY,
            double? length)
        {
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = pieceId,
                BlockName = block,
                View = view,
                Insertion = at,
                ConnectionAnchor = at,
                MirroredX = mirroredX,
                MirroredY = mirroredY
            };
            if (length.HasValue && length.Value > 0.0)
            {
                instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = length.Value;
            }

            return instance;
        }
    }
}
