using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
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
            DynamicRackEnd end,
            RackLevelElevations elevations = null,
            Func<int, int, bool> ownsDesviador = null,
            Func<int, bool> ownsBoundary = null)
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
                // I-33 (Owner): la seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se
                // coloca. La celda guardada NO se mueve a otro poste: queda dormida y vuelve al reactivar un frente.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

                // I-42: un corte frontal es de UN LADO, y lo que se atornilla a una linea que solo el otro
                // lado necesita no le pertenece. Sin filtro no cambia nada.
                if (ownsBoundary != null && !ownsBoundary(postIndex))
                {
                    continue;
                }

                var origin = new Point2D(layout.PostPositions[postIndex], 0.0);
                var at = new Point2D(origin.X - plateMate.X, origin.Y - plateMate.Y);
                var depthRange = DynamicDepthGeometry.AtPost(system, postIndex);
                var rangeStart = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? system.TotalLength;
                // La ORIENTACIÓN la trae la COPIA, no el corte. En un sistema de extremo bajo las dos orientaciones
                // caben en el mismo corte, así que deducirla del extremo las confundiría (I-32, round 1).
                SafetyEndCopy? guardCopy = null;
                var guard = guards.FirstOrDefault(element =>
                {
                    guardCopy = DynamicLateralGuardPlan.CopyAtEnd(
                        element.Selection, postIndex, postCount: layout.PostPositions.Count, end: end);
                    return guardCopy.HasValue;
                });
                if (guard != null && guardCopy.HasValue)
                {
                    target.Add(Piece(guard.PieceId, guard.Block, view, at,
                        mirroredX: guardCopy.Value.Mirrored, mirroredY: false, rangeEnd - rangeStart));
                    continue;
                }

                SafetyEndCopy? bootCopy = null;
                var boot = boots.FirstOrDefault(element =>
                {
                    bootCopy = CopyAtEnd(element.Selection, postIndex, end);
                    return bootCopy.HasValue;
                });
                if (boot != null && bootCopy.HasValue)
                {
                    target.Add(Piece(boot.PieceId, boot.Block, view, at,
                        mirroredX: bootCopy.Value.Mirrored, mirroredY: false, null));
                }
            }

            AppendFrontalDesviadores(target, system, catalog, layout, end, elevations, ownsDesviador, ownsBoundary);
            AppendFrontalDefensas(target, system, catalog, layout, plateId, end, ownsBoundary);
            AppendFrontalGuias(target, system, catalog, layout, end, ownsBoundary);
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
                // I-33 (Owner): la seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se
                // coloca. La celda guardada NO se mueve a otro poste: queda dormida y vuelve al reactivar un frente.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

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
                    // I-42 (ronda 6F) — una BOTA protege el poste del impacto del montacargas, y el montacargas
                    // ataca por la cara de CARGA. Las dos copias de esta linea se atornillan a los extremos de su
                    // cobertura de profundidad, y eso vale mientras esos extremos SEAN caras de ataque.
                    //
                    // Con frentes en blanco —una columna de nave, por ejemplo— la cobertura de una linea se acorta y
                    // su extremo pasa a caer en la interfaz entre los dos lados: contra la columna, sin pasillo por
                    // el que entre nadie. Medido: con las dos primeras ranuras de A en blanco aparecia una bota en
                    // X=395.61, junto a la interfaz, mientras la de B seguia bien en su pasillo.
                    //
                    // Un blanco QUITA la necesidad; no muda la pieza a otro borde. Es la misma regla fisica que la
                    // ronda 6D cerro para la defensa de montacargas, sobre la misma declaracion de la estructura: el
                    // Dinamico no declara interior y dibuja exactamente igual que siempre.
                    SelectiveSafetyPlacement.AppendAtPost(
                        target, catalog, view, boots, at, plateId, postIndex,
                        mirrorAxisX: (rangeStart + rangeEnd) / 2.0,
                        faceApplies: atHighEnd => !system.IsInteriorFace(atHighEnd ? rangeEnd : rangeStart),
                        physicalFaces: true);
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
            DynamicRackEnd end,
            Func<int, bool> ownsBoundary)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            if (selection == null)
            {
                return;
            }

            const string view = "FRONTAL";

            // I-42 (ronda 7E): un corte frontal mira UN extremo, asi que lleva la pieza que ESA cara declara.
            var elementId = DynamicDefenseFaces.ElementIdFor(selection, farEnd: end == DynamicRackEnd.Entrance);
            var block = string.IsNullOrWhiteSpace(elementId) ? null : CatalogLookup.Block(catalog, elementId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return;
            }

            var offset = CatalogLookup.Local(
                catalog, elementId, DynamicForkliftDefensePlan.PostOriginPoint, view);
            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);
            var postCount = layout.PostPositions.Count;
            for (var postIndex = 0; postIndex < postCount; postIndex++)
            {
                // I-33 (Owner): la seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se
                // coloca. La celda guardada NO se mueve a otro poste: queda dormida y vuelve al reactivar un frente.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

                // I-42: un corte frontal es de UN LADO, y lo que se atornilla a una linea que solo el otro
                // lado necesita no le pertenece. Sin filtro no cambia nada.
                if (ownsBoundary != null && !ownsBoundary(postIndex))
                {
                    continue;
                }

                var setting = DynamicForkliftDefensePlan.ForSelection(selection, postIndex, postCount);
                var draws = end == DynamicRackEnd.Exit ? setting.DrawsExit : setting.DrawsEntrance;
                if (!draws)
                {
                    continue;
                }

                var direction = end == DynamicRackEnd.Exit ? 1.0 : -1.0;
                target.Add(Piece(
                    elementId,
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
            DynamicRackEnd end,
            Func<int, bool> ownsBoundary)
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

                // I-42: la guia tambien vive sobre una LINEA. Sin filtro no cambia nada (y Push Back no lleva guias).
                if (ownsBoundary != null && !ownsBoundary(placement.PostIndex))
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

            // I-42 (ronda 7E): cada CARA lleva la pieza que su lado eligio, y puede no llevar ninguna. Sin caras
            // declaradas —todo sistema que no las rellene, todo documento anterior— las dos resuelven a la de la
            // seleccion, que es exactamente lo que se hacia antes.
            var nearId = DynamicDefenseFaces.ElementIdFor(selection, farEnd: false);
            var farId = DynamicDefenseFaces.ElementIdFor(selection, farEnd: true);
            var nearBlock = string.IsNullOrWhiteSpace(nearId) ? null : CatalogLookup.Block(catalog, nearId, view);
            var farBlock = string.IsNullOrWhiteSpace(farId) ? null : CatalogLookup.Block(catalog, farId, view);
            if (string.IsNullOrWhiteSpace(nearBlock) && string.IsNullOrWhiteSpace(farBlock))
            {
                return;
            }

            var nearOffset = string.IsNullOrWhiteSpace(nearId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, nearId, DynamicForkliftDefensePlan.PostOriginPoint, view);
            var farOffset = string.IsNullOrWhiteSpace(farId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, farId, DynamicForkliftDefensePlan.PostOriginPoint, view);
            var postCount = layout.PostPositions.Count;
            for (var postIndex = 0; postIndex < postCount; postIndex++)
            {
                // I-33 (Owner): la seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se
                // coloca. La celda guardada NO se mueve a otro poste: queda dormida y vuelve al reactivar un frente.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

                var setting = DynamicForkliftDefensePlan.ForSelection(selection, postIndex, postCount);
                var rangeStart = DynamicDefenseFaces.NearX(system, postIndex);
                var rangeEnd = DynamicDefenseFaces.FarX(system, postIndex);

                // I-42 (ronda 6D) — una defensa protege una CARA DE CARGA: el extremo de la profundidad por donde
                // entra el montacargas. Se coloca en los extremos de la cobertura de esta linea, y eso basta
                // mientras esos extremos SEAN caras. En un Push Back compuesto no siempre lo son: un lado EN BLANCO
                // acorta la cobertura de su linea, y su extremo pasa a caer en la interfaz con el otro lado —dentro
                // del rack, sin pasillo al que mirar—. Medido: con el lado A en blanco en la primera ranura,
                // aparecia una defensa en X=247.25, contra la cara posterior del lado contrario.
                //
                // La estructura declara ese tramo interior; el Dinamico no declara ninguno y dibuja igual que
                // siempre.
                // I-42 (ronda 7D): la MISMA pregunta la hace ahora la rejilla por poste, para que no pueda pintar
                // «apagado» donde el rack si lleva defensa. La regla no cambia, solo dejo de estar solo aqui.
                if (setting.DrawsExit && !string.IsNullOrWhiteSpace(nearBlock)
                    && DynamicDefenseFaces.HasFace(system, postIndex, farEnd: false))
                {
                    target.Add(Piece(nearId, nearBlock, view,
                        new Point2D(rangeStart + nearOffset.X, layout.PostPositions[postIndex] + nearOffset.Y),
                        false, false, setting.ExitLength));
                }

                if (setting.DrawsEntrance && !string.IsNullOrWhiteSpace(farBlock)
                    && DynamicDefenseFaces.HasFace(system, postIndex, farEnd: true))
                {
                    target.Add(Piece(farId, farBlock, view,
                        new Point2D(rangeEnd - farOffset.X, layout.PostPositions[postIndex] + farOffset.Y),
                        true, false, setting.EntranceLength));
                }
            }
        }

        private static void AppendFrontalDesviadores(
            ICollection<HeaderBlockInstance> target,
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicFrontLayout layout,
            DynamicRackEnd end,
            RackLevelElevations elevations,
            Func<int, int, bool> ownsDesviador,
            Func<int, bool> ownsBoundary)
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
                // I-33 (Owner): la seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se
                // coloca. La celda guardada NO se mueve a otro poste: queda dormida y vuelve al reactivar un frente.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

                // I-42: un corte frontal es de UN LADO, y lo que se atornilla a una linea que solo el otro
                // lado necesita no le pertenece. Sin filtro no cambia nada.
                if (ownsBoundary != null && !ownsBoundary(postIndex))
                {
                    continue;
                }

                if (!DrawsAtEnd(selection, postIndex, end))
                {
                    continue;
                }

                // PB-002 (I-32): ONE authority decides which off-cell a post reads, so the lateral, the two frontal
                // cuts, the planta and the BOM cannot disagree about the cell the user switched off.
                var cellKey = SelectiveDesviadorPlan.CellKey(selection, postIndex, system.Fronts.Count);
                var levelsAtPost = LoadLevelsAtPost(system, postIndex);
                for (var levelIndex = 0; levelIndex < levelsAtPost && levelIndex < system.LoadBeamLevels.Count; levelIndex++)
                {
                    if (off.Contains((cellKey, levelIndex)))
                    {
                        continue;
                    }

                    // I-42 — un desviador guia la tarima al ENTRAR, asi que solo existe donde este corte tiene una
                    // cama que se carga por el. Un rack de un solo sentido no pasa predicado y no cambia nada; un
                    // compuesto lo deriva de sus CAMAS, que es la misma autoridad que gobierna el lateral.
                    if (ownsDesviador != null && !ownsDesviador(postIndex, levelIndex))
                    {
                        continue;
                    }

                    var level = system.LoadBeamLevels[levelIndex];

                    // El desviador cuelga del larguero de SU extremo, y sigue al override de ese extremo — sea el
                    // bajo o el alto. Se pregunta POR POSTE porque este bucle recorre postes y en un rack jagged cada
                    // uno puede tener frentes distintos a los lados (PB-004).
                    var beamY = elevations.OrPost(
                        postIndex,
                        level.LevelNumber,
                        end == DynamicRackEnd.Entrance ? level.EntranceElevation : level.ExitElevation);
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
                // I-33 (Owner): la seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se
                // coloca. La celda guardada NO se mueve a otro poste: queda dormida y vuelve al reactivar un frente.
                if (!DynamicFrontActivation.BoundaryExists(system, postIndex))
                {
                    continue;
                }

                var depthRange = DynamicDepthGeometry.AtPost(system, postIndex);
                var rangeStart = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
                var rangeEnd = system.Modules.FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? system.TotalLength;
                var cellKey = SelectiveDesviadorPlan.CellKey(selection, postIndex, system.Fronts.Count);
                var anyLevel = Enumerable.Range(0, LoadLevelsAtPost(system, postIndex))
                    .Any(level => !off.Contains((cellKey, level)));
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

        /// <summary>
        /// La copia de ese poste que va en ese corte, con su orientación — o null si no lleva ninguna.
        /// I-42 (S1): es la BOTA, asi que su pertenencia son UBICACIONES FISICAS y no orientaciones.
        /// </summary>
        private static SafetyEndCopy? CopyAtEnd(SelectiveSafetySelection selection, int postIndex, DynamicRackEnd end)
        {
            var highEnd = end == DynamicRackEnd.Entrance;
            foreach (var copy in SelectiveSafetyEnds.BootCopiesForPost(selection, postIndex))
            {
                if (copy.AtHighEnd == highEnd)
                {
                    return copy;
                }
            }

            return null;
        }

        private static bool DrawsAtEnd(SelectiveSafetySelection selection, int postIndex, DynamicRackEnd end)
        {
            // El FRONTAL elige el CORTE, es decir el extremo longitudinal. Lo resuelve SelectiveSafetyEnds, que
            // respeta la pertenencia por poste y lleva al extremo bajo los sistemas que solo tienen ese (Push Back).
            return SelectiveSafetyEnds.DrawsAt(selection, postIndex, highEnd: end == DynamicRackEnd.Entrance);
        }

        private static int LoadLevelsAtPost(DynamicRackSystem system, int postIndex)
        {
            if (system?.Fronts == null || system.Fronts.Count == 0)
            {
                return system?.LoadBeamLevels.Count ?? 0;
            }

            // Level-indexed safety follows the LOAD, so a blank front contributes none and a post surrounded only by
            // blank fronts receives no level-indexed piece (I-33). EffectiveLoadLevels already carries the historical
            // Math.Max(1, ...) floor for an active front, so a rack without blank fronts is unaffected.
            var count = 0;
            if (postIndex > 0 && postIndex - 1 < system.Fronts.Count)
            {
                count = Math.Max(count, DynamicFrontActivation.EffectiveLoadLevels(system.Fronts[postIndex - 1]));
            }

            if (postIndex < system.Fronts.Count)
            {
                count = Math.Max(count, DynamicFrontActivation.EffectiveLoadLevels(system.Fronts[postIndex]));
            }

            return count;
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
