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
            IReadOnlyList<DynamicRackFront> adjacentFronts = null,
            RackLevelElevations elevations = null)
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
            // El protector lateral trae sus copias FÍSICAS: cada una con su extremo y su orientación resueltos por
            // separado. Traducirlo a un SafetySide y leerlo literal aquí volvía a mezclar los dos ejes y mandaba
            // atrás la copia del último poste (I-32, round 2).
            var guardCopies = laterales.Count > 0
                ? DynamicLateralGuardPlan.CopiesAt(
                    laterales[0].Selection, postIndex, Math.Max(1, system.Fronts.Count + 1))
                : (IReadOnlyList<SafetyEndCopy>)new SafetyEndCopy[0];

            if (guardCopies.Count > 0)
            {
                // I-40 (Owner, ronda 5) — el PROTECTOR del ULTIMO corte salia invertido.
                //
                // La regla adaptativa da al primer poste una copia sin espejo y al ultimo una ESPEJADA, porque los
                // dos protegen caras OPUESTAS DEL PASILLO: ese espejo es a lo ancho del rack, y por eso la PLANTA
                // —que ve el ancho— lo dibuja bien. El corte LATERAL mira una sola linea de postes: su eje horizontal
                // es el FONDO, no el ancho, asi que aplicar ahi ese espejo volteaba la pieza sobre el fondo y la
                // mandaba mirando hacia dentro del rack.
                //
                // En el lateral el volteo depende UNICAMENTE del extremo en que la copia se apoya. Con extremo alto
                // (Dinamico) sale exactamente lo mismo que antes; en extremo bajo (Push Back) deja de invertirse.
                AppendEndpointFamily(
                    result, laterales, left.PlateOrigin, right.PlateOrigin,
                    sectionEnd - sectionStart, postIndex,
                    guardCopies.Select(copy => new SafetyEndCopy(copy.AtHighEnd, mirrored: copy.AtHighEnd)).ToList());
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
                adjacentFronts,
                elevations);
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

            var setting = DynamicForkliftDefensePlan.ForSelection(
                selection, postIndex, Math.Max(1, system.Fronts.Count + 1));
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
            IReadOnlyList<SafetyEndCopy> copiesOverride = null)
        {
            foreach (var element in elements ?? Array.Empty<SelectiveSafetyPlacement.SafetyElement>())
            {
                // El CORTE LATERAL elige el extremo FÍSICO de la línea del poste. Cada copia trae SU extremo y SU
                // orientación por separado: en un sistema de extremo bajo, una elección Right se dibuja delante,
                // espejada en su propio sitio, nunca atrás. El llamador puede traer sus propias copias —el
                // protector lateral las resuelve con su regla adaptativa— y si no, se leen de la matriz por poste.
                var copies = copiesOverride ?? SelectiveSafetyEnds.CopiesForPost(element.Selection, postIndex);
                foreach (var copy in copies)
                {
                    target.Add(Piece(
                        element.PieceId, element.Block,
                        copy.AtHighEnd ? right : left,
                        mirrored: copy.Mirrored,
                        longitud));
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
            IReadOnlyList<DynamicRackFront> adjacentFronts,
            RackLevelElevations elevations)
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
            var leftFront = fronts.OrderBy(front => front.StartX).FirstOrDefault();
            var leftLevels = leftFront != null
                ? DynamicFrontGeometry.LoadBeamLevels(system, leftFront)
                : system.LoadBeamLevels.ToList();
            var rightLevels = fronts.OrderByDescending(front => front.EndX)
                .Select(front => DynamicFrontGeometry.LoadBeamLevels(system, front))
                .FirstOrDefault() ?? system.LoadBeamLevels.ToList();
            var count = Math.Min(levelCount, Math.Max(leftLevels.Count, rightLevels.Count));
            for (var level = 0; level < count; level++)
            {
                if (off.Contains((SelectiveDesviadorPlan.CellKey(selection, postIndex, system.Fronts.Count), level)))
                {
                    continue;
                }

                var leftLoad = leftLevels[Math.Min(level, leftLevels.Count - 1)];
                var rightLoad = rightLevels[Math.Min(level, rightLevels.Count - 1)];

                // El desviador cuelga del larguero de SU extremo, y CADA extremo consulta su propio contexto: el
                // bajo el principal y el alto el acompañante (HighEnd). Desde la inversion vertical de I-42 los dos
                // se derivan, asi que leer la elevacion del resolver para el alto lo dejaria colgando de un larguero
                // que ya no esta ahi. El primer nivel no consulta ninguno: mide desde el troquel del poste.
                //
                // Qué ámbito se consulta lo decide si este corte pertenece a un frente. SECCIONADO por poste: la
                // columna baja pertenece al frente adyacente de menor StartX, así que se pregunta POR FRENTE. SIN
                // seccionar: el corte dibuja el rack entero y ocupa su profundidad completa, que no es la de ningún
                // frente, así que se pregunta por la ENVOLVENTE — la misma con la que se resuelven sus largueros.
                var leftY = level == 0
                    ? firstLeftY
                    : (leftFront != null
                        ? elevations.OrFront(leftFront.Index, leftLoad.LevelNumber, leftLoad.ExitElevation)
                        : elevations.OrSystemEnvelope(leftLoad.LevelNumber, leftLoad.ExitElevation))
                      - SelectiveDesviadorPlan.BeamYOffset;
                var rightFront = fronts.OrderByDescending(front => front.EndX).FirstOrDefault();
                var high = elevations?.HighEnd;
                var rightY = level == 0
                    ? firstRightY
                    : (rightFront != null
                        ? high.OrFront(rightFront.Index, rightLoad.LevelNumber, rightLoad.EntranceElevation)
                        : high.OrSystemEnvelope(rightLoad.LevelNumber, rightLoad.EntranceElevation))
                      - SelectiveDesviadorPlan.BeamYOffset;

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
