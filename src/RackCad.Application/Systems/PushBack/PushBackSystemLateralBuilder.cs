using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Lateral Push Back plan by BLACK-BOX composition of the dynamic lateral plan. It invokes
    /// <see cref="DynamicSystemLateralBuilder"/> (whole system and per-post section modes), keeps the common structure
    /// (cabeceras, separators, derived posts, plates, annotations/dimensions), removes the dynamic-specific pieces by
    /// Role/PieceId (both dynamic end beams, the roller bed, all dynamic safety/tope, the dynamic intermediate beams),
    /// and adds the Push Back pieces: the low IN/OUT beam, the high TROQUEL_REDONDO beam per cell, intermediates tangent
    /// to the Push Back axis, the pushback bed, and the rear topes. The dynamic plan is never mutated.
    /// </summary>
    public sealed class PushBackSystemLateralBuilder
    {
        private readonly DynamicSystemLateralBuilder dynamicBuilder = new DynamicSystemLateralBuilder();
        private readonly PushBackFlowBedLateralBuilder bedBuilder = new PushBackFlowBedLateralBuilder();
        private readonly PushBackIntermediateBeamLateralBuilder intermediateBuilder = new PushBackIntermediateBeamLateralBuilder();
        private readonly PushBackRearTopeBuilder rearTopeBuilder = new PushBackRearTopeBuilder();

        public HeaderRunPlan Build(PushBackSystem system, RackCatalog catalog) => BuildCore(system, catalog, -1);

        public HeaderRunPlan Build(PushBackSystem system, RackCatalog catalog, int postIndex) => BuildCore(system, catalog, postIndex);

        /// <summary>The lateral section at each transverse post, following the dynamic Cortes contract.</summary>
        public IReadOnlyList<DynamicLateralCorte> Cortes(PushBackSystem system, RackCatalog catalog)
        {
            var result = new List<DynamicLateralCorte>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                // I-33 (Owner): misma regla que el Dinámico, sobre la MISMA autoridad — la frontera compartida por dos
                // frentes en blanco no existe, así que no hay corte. Los que quedan conservan su índice de poste.
                if (!DynamicFrontActivation.BoundaryExists(structure, postIndex))
                {
                    continue;
                }

                result.Add(new DynamicLateralCorte(postIndex, layout.PostPositions[postIndex], Build(system, catalog, postIndex)));
            }

            return result;
        }

        /// <summary>
        /// La identidad de un frente A EFECTOS DE ESTE CORTE: dos frentes adyacentes solo se colapsan en uno si el
        /// corte los dibujaria IGUAL.
        /// <para>
        /// Hasta I-40 bastaba con (StartX, EndX, LoadLevels): con un unico fondo por frente, esos tres datos
        /// determinaban toda la geometria del corte. I-41 lo rompe — dos frentes pueden compartir envolvente y numero
        /// de niveles y aun asi escalonar sus celdas de forma distinta, o pedir tarima uno si y otro no. Colapsarlos
        /// dibujaria el segundo con la configuracion del primero, en silencio. Por eso la clave incorpora el fondo
        /// EFECTIVO y el flag de tarima de cada nivel.
        /// </para>
        /// Un rack sin overrides ni tarimas produce la MISMA agrupacion que antes de I-41: los sufijos añadidos son
        /// identicos en los frentes que ya se colapsaban.
        /// </summary>
        private static string SectionKey(PushBackSystem system, DynamicRackFront front)
        {
            var frontIndex = PushBackCellDepth.FrontIndexOf(system, front);
            var levels = DynamicFrontActivation.EffectiveLoadLevels(front);
            var cells = string.Join(
                ",",
                Enumerable.Range(0, levels).Select(level => string.Concat(
                    system.EffectivePalletsDeepAt(frontIndex, level).ToString(CultureInfo.InvariantCulture),
                    system.DrawPalletAt(frontIndex, level) ? "T" : "-")));
            return string.Join("|", front.StartX, front.EndX, front.LoadLevels, cells);
        }

        /// <summary>
        /// I-42 — el lateral de un rack COMPUESTO. La regla arquitectonica se ve entera aqui: la ESTRUCTURA se
        /// dibuja UNA sola vez, desde el sistema compuesto (cabeceras, separadores —el central incluido—, postes
        /// derivados, placas, cotas y etiquetas), y encima se monta el CONTENIDO cama a cama, cada una construida en
        /// su marco y llevada al rack con una sola reflexion rigida. No se dibuja un rack A y otro B superpuestos.
        ///
        /// <para>
        /// El contexto de elevaciones que reciben las decoraciones compartidas es el del lado A, el de REFERENCIA:
        /// una sola tabla frente-&gt;nivel-&gt;elevacion no puede describir dos pasillos a la vez, y las elevaciones
        /// propias del lado B ya viajan con sus propias piezas. Limitacion declarada, no un descuido.
        /// </para>
        /// </summary>
        private HeaderRunPlan BuildComposite(PushBackSystem system, RackCatalog catalog, int postIndex, bool sectioned)
        {
            var structure = system.Structure;
            var reference = system.Composite.SideA?.Local;
            var elevations = reference != null ? PushBackElevations.Context(reference, catalog) : null;
            var basePlan = sectioned
                ? dynamicBuilder.Build(structure, catalog, postIndex, elevations)
                : dynamicBuilder.Build(structure, catalog, elevations);

            var headers = PushBackPlanComposer.StructuralHeaderGroups(basePlan);
            var loose = PushBackPlanComposer.StructuralLoose(basePlan);

            var runs = PushBackRuns.Resolve(system);
            var slots = CompositeSlots(system, structure, postIndex, sectioned);
            var levelCap = sectioned ? DynamicFrontGeometry.LoadLevelsAtPost(structure, postIndex) : int.MaxValue;
            var content = PushBackCompositeContent.Lateral(system, catalog, runs, slot => slots.Contains(slot), levelCap);
            headers.AddRange(content.Headers);
            loose.AddRange(content.Loose);

            var intermediates = intermediateBuilder.Build(system, catalog, postIndex, levelCap);
            headers.AddRange(intermediates.Headers);
            loose.AddRange(intermediates.LooseInstances);

            // Etiquetas A/B: informacion grafica del plano, por el pipeline de anotaciones que ya existe. Nunca al BOM.
            loose.AddRange(PushBackSideAnnotations.Lateral(system));

            return new HeaderRunPlan(headers, loose);
        }

        /// <summary>
        /// Las ranuras que este lateral materializa. Un corte por poste dibuja las ranuras adyacentes a ese poste; el
        /// lateral NO seccionado dibuja la ENVOLVENTE, que es la ranura de mayor tramo longitudinal (la mas
        /// profunda), igual que un rack de un solo sentido dibuja la del rack entero.
        /// </summary>
        private static HashSet<int> CompositeSlots(
            PushBackSystem system, DynamicRackSystem structure, int postIndex, bool sectioned)
        {
            if (sectioned)
            {
                return new HashSet<int>(
                    DynamicFrontGeometry.AdjacentFronts(structure, postIndex).Select(front => front.Index));
            }

            var envelope = -1;
            var best = double.MinValue;
            foreach (var front in structure.Fronts)
            {
                var span = front.EndX - front.StartX;
                if (span > best)
                {
                    best = span;
                    envelope = front.Index;
                }
            }

            return envelope >= 0 ? new HashSet<int> { envelope } : new HashSet<int>();
        }

        private HeaderRunPlan BuildCore(PushBackSystem system, RackCatalog catalog, int postIndex)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var sectioned = postIndex >= 0;
            if (system.IsComposite)
            {
                return BuildComposite(system, catalog, postIndex, sectioned);
            }

            // El contexto de elevaciones se construye UNA vez y viaja al builder compartido, que lo usa para el
            // desviador bajo y para las cotas y etiquetas. Los largueros y la cama lo resuelven por su cuenta desde
            // la misma autoridad, así que las cuatro cosas no pueden discrepar (PB-004, I-32).
            var elevations = PushBackElevations.Context(system, catalog);
            var basePlan = sectioned
                ? dynamicBuilder.Build(structure, catalog, postIndex, elevations)
                : dynamicBuilder.Build(structure, catalog, elevations);

            // Keep the common structure; drop every dynamic-specific piece by Role/PieceId.
            var headers = PushBackPlanComposer.StructuralHeaderGroups(basePlan);
            var loose = PushBackPlanComposer.StructuralLoose(basePlan);

            var levelCount = sectioned
                ? DynamicFrontGeometry.LoadLevelsAtPost(structure, postIndex)
                : structure.LoadBeamLevels.Count;
            IReadOnlyList<DynamicRackFront> fronts = sectioned
                ? DynamicFrontGeometry.AdjacentFronts(structure, postIndex)
                    .GroupBy(front => SectionKey(system, front))
                    .Select(group => group.First())
                    .ToList()
                : new List<DynamicRackFront> { null };

            foreach (var front in fronts)
            {
                var frontIndex = front?.Index ?? 0;
                loose.AddRange(PushBackLoadBeamGeometry.LowBeams(system, catalog, front));
                loose.AddRange(PushBackLoadBeamGeometry.HighBeams(system, catalog, frontIndex, front));
                loose.AddRange(rearTopeBuilder.BuildLateral(system, catalog, frontIndex, front));

                var bedLevels = sectioned
                    ? Math.Min(levelCount, DynamicFrontActivation.EffectiveLoadLevels(front))
                    : levelCount;
                // I-41 (PB-015): una definicion anidada por FONDO EFECTIVO distinto. Sin overrides sale exactamente
                // un grupo, que es la cama de siempre.
                headers.AddRange(bedBuilder.BuildLateralGroups(system, catalog, front, bedLevels));

                // I-41 (PB-016): las tarimas de este frente, solo en las celdas que las piden. Son VISUALES: no entran
                // al BOM y no participan de ninguna cota.
                loose.AddRange(PushBackTarimaPlacement.Lateral(system, catalog, front, bedLevels));
            }

            var intermediates = intermediateBuilder.Build(system, catalog, postIndex, levelCount);
            headers.AddRange(intermediates.Headers);
            loose.AddRange(intermediates.LooseInstances);

            return new HeaderRunPlan(headers, loose);
        }
    }
}
