using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Planta (top view) Push Back plan by BLACK-BOX composition of <see cref="DynamicSystemPlantaBuilder"/>. It keeps the
    /// common structure (cabeceras, separators, derived posts, plates, intermediate beams, GUIA-free safety and
    /// decorations), keeps the LOW IN/OUT beam, and swaps every HIGH (rear) IN/OUT beam for a TROQUEL_REDONDO plus a rear
    /// tope when the front has an active rear tope. Levels collapse onto one plan line, so the rear tope is one per front.
    /// Instances are identified by Role/PieceId/mirror, never by group name.
    /// </summary>
    public sealed class PushBackSystemPlantaBuilder
    {
        private const string View = "PLANTA";
        private readonly DynamicSystemPlantaBuilder dynamicBuilder = new DynamicSystemPlantaBuilder();

        public HeaderRunPlan BuildPlan(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            // I-42: un rack COMPUESTO tiene dos pasillos, asi que su planta no puede construirse cambiando «el
            // larguero espejado» por un posterior: los dos extremos son pasillos. La compone PushBackCompositePlanta,
            // estructura una vez + contenido por lado.
            if (system.IsComposite)
            {
                return PushBackCompositePlanta.Build(system, catalog);
            }

            var instances = dynamicBuilder.BuildPlan(structure, catalog).Flatten().Instances;
            var redondoId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var redondoBlock = CatalogLookup.Block(catalog, redondoId, View);
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var topePieceId = PushBackRearTopeBuilder.ResolvePieceId(catalog, rearTope);   // PB-005
            var topeBlock = CatalogLookup.Block(catalog, topePieceId, View);
            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);

            var result = new List<HeaderBlockInstance>();
            foreach (var instance in instances)
            {
                if (!PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    result.Add(instance); // keep structure, intermediates, safety (GUIA-free) and decorations
                    continue;
                }

                if (!instance.MirroredX)
                {
                    result.Add(instance); // the LOW (entrance/exit) IN/OUT beam stays
                    continue;
                }

                // The HIGH (rear) IN/OUT beam becomes a TROQUEL_REDONDO with the ENVELOPING rear peralte (planta collapses
                // the levels); add a rear tope if that front has any active cell.
                var frontIndex = PlantaFront(structure, catalog, instance.Insertion.Y);
                var redondo = CloneAt(instance, redondoId, redondoBlock);
                // I-42 (correccion aislada 5C) — la MANO del larguero alto ya esta decidida; aqui solo se TRANSPORTA.
                //
                // Esta vista no la pedia: sustituia el larguero del builder dinamico conservando SU espejo, que en
                // el marco de una cama es un «alto siempre espejado» fijo y nunca paso por la autoridad. Medido en
                // una corrida CORTA: el lateral ponia su alto en X=102 sin espejo —la frontera es un vano— y la
                // planta lo ponia espejado.
                redondo.MirroredX = DynamicIntermediateBeamGeometry.HandAtDepthX(structure, instance.Insertion.X)
                                    ?? instance.MirroredX;
                redondo.DynamicParameters[SelectiveRackDefaults.PeralteParam] = PushBackHighEndBeamGeometry.PlantaPeralte(system, frontIndex);
                result.Add(redondo);

                var front = frontIndex >= 0 && frontIndex < structure.Fronts.Count ? structure.Fronts[frontIndex] : null;
                var anyActive = front != null
                    && Enumerable.Range(0, DynamicFrontActivation.EffectiveLoadLevels(front))
                        .Any(level => rearTope.At(frontIndex, level));
                if (!string.IsNullOrWhiteSpace(topeBlock) && anyActive)
                {
                    // Owner clarification (2026-07-25): the tope block mates by its ORIGIN, so its insertion must land
                    // EXACTLY on the post's TROQUEL_TOPE in this view. No fallback: without the measured point (or
                    // without a post in the plan) no stop is drawn.
                    //
                    // I-42 (correccion aislada 4B) — los DOS ejes de la planta no tienen la misma autoridad. La Y
                    // corre con la retícula transversal y es del POSTE: ahi el mate del poste es exacto. La X corre
                    // con la PROFUNDIDAD, y esa la manda el LARGUERO ALTO de la cama, no el poste mas cercano: el
                    // extremo alto de una cama no tiene por que caer en una linea de postes. Medido: con el contacto
                    // alto de una corrida CORTA en X = 101.845, el lateral pone el tope en 101.125 —del lado por el
                    // que llega la tarima— y la planta lo ponia en 102.875, al otro lado del larguero, porque tomaba
                    // la X del poste del borde. El poste es una CONSECUENCIA cuando coincide, nunca la autoridad.
                    //
                    // El desplazamiento fisico es el MISMO que ya se usaba —el punto medido del poste, con el signo
                    // del espejo de la pieza—: no se resta ninguna constante, solo cambia desde donde se mide.
                    var mate = PushBackRearTopeBuilder.PostMateWorld(
                        catalog, postId, postPeralte, View, instances, instance.Insertion);
                    var anchor = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, postPeralte, View);
                    if (mate.HasValue && anchor.HasValue)
                    {
                        double? longitud = instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var beamLength)
                            ? beamLength + SelectiveTopePlacement.LengthAllowance
                            : (double?)null;
                        // El tope sigue la mano de SU larguero, ya transportada: no tiene autoridad propia.
                        var topeMirrored = PushBackRearTopeBuilder.Mirrored(View, redondo.MirroredX);
                        // I-42 (correccion aislada 5D) — la MEDIDA sigue saliendo del larguero alto (ronda 4B), pero
                        // el SIGNO del desplazamiento lo pone el espejo DEL TOPE, igual que en el lateral y que en el
                        // Selectivo. Con el signo de la otra pieza, un tope espejado quedaba 1.75" del lado contrario.
                        var depth = PushBackRearTopeBuilder.AnchorX(
                            instance.Insertion.X, anchor.Value.X, topeMirrored);
                        result.Add(SelectiveTopePlacement.Tope(
                            topePieceId, topeBlock, View,
                            depth, mate.Value.Y, saque, longitud,
                            mirroredX: topeMirrored));
                    }
                }
            }

            return HeaderInstanceGrouper.Group(result, "PB_PLANTA_PIEZA");
        }

        public IReadOnlyList<HeaderBlockInstance> Build(PushBackSystem system, RackCatalog catalog)
            => BuildPlan(system, catalog).Flatten().Instances;

        private static HeaderBlockInstance CloneAt(HeaderBlockInstance source, string pieceId, string block)
        {
            var clone = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = pieceId,
                BlockName = block,
                View = View,
                MirroredX = source.MirroredX,
                MirroredY = source.MirroredY,
                RotationRadians = source.RotationRadians,
                Insertion = source.Insertion,
                ConnectionAnchor = source.ConnectionAnchor
            };
            if (source.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var length))
            {
                clone.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
            }

            return clone;
        }

        /// <summary>
        /// El FRENTE al que pertenece una pieza de la planta, identificado por su posicion TRANSVERSAL.
        ///
        /// <para>
        /// En PLANTA la X corre con la profundidad y la Y con la retícula transversal, asi que un frente se
        /// reconoce por su Y — la linea de postes entre la que vive—, nunca por la X. Antes se buscaba «el frente
        /// cuyo EndX esta mas cerca», y como TODOS los frentes comparten la profundidad eso devolvia siempre el
        /// mismo: el tope de una celda se decidia leyendo la configuracion de OTRO frente, y en un rack compuesto
        /// —donde cada cama se dibuja sobre una copia con una sola ranura activa— caia en un frente EN BLANCO, que
        /// no tiene niveles efectivos. Resultado: el tope solo aparecia en el primer frente.
        /// </para>
        /// </summary>
        private static int PlantaFront(DynamicRackSystem system, RackCatalog catalog, double y)
        {
            var layout = DynamicFrontGeometry.Compute(system, catalog);
            var posts = layout?.PostPositions;
            if (posts == null || posts.Count < 2)
            {
                return 0;
            }

            // Un frente vive ENTRE dos lineas de postes: se elige aquel cuyo intervalo contiene la Y, y si la pieza
            // cae justo sobre una linea, el frente cuyo centro queda mas cerca.
            var best = 0;
            var bestDistance = double.MaxValue;
            for (var index = 0; index + 1 < posts.Count && index < system.Fronts.Count; index++)
            {
                var centre = (posts[index] + posts[index + 1]) / 2.0;
                var distance = Math.Abs(centre - y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = index;
                }
            }

            return best;
        }
    }
}
