using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda post-5a73b92) — CAPACIDAD DEL RACK, PRESENCIA EN EL FRENTE y TOPOLOGÍA DE LA CELDA son tres
    /// estados distintos y ninguno puede hablar por los otros.
    ///
    /// <list type="bullet">
    /// <item><b>Capacidad</b>: «existe el lado B como posibilidad». Es del rack.</item>
    /// <item><b>Presencia</b>: «este frente tiene físicamente lado B». Es de cada frente.</item>
    /// <item><b>Topología</b>: Solo A / Solo B / Encontradas / Corrida. Es de cada celda.</item>
    /// </list>
    ///
    /// <para>
    /// El dueño convirtió un rack de CUATRO frentes a compuesto y declaró el lado B en uno solo: se le invirtió la
    /// seguridad y desaparecieron topes en los frentes que seguían siendo de un solo sentido. La causa era que
    /// activar la capacidad mutaba el rack entero — el default de topología pasaba a «encontradas» para todas las
    /// celdas y la segunda cara de carga se aplicaba a todas las líneas de postes.
    /// </para>
    /// </summary>
    public class PushBackPartialCompositeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        /// <summary>Un rack de un solo sentido con <paramref name="fronts"/> frentes, ya configurado.</summary>
        private static PushBackCompositeEditorState SingleSided(int fronts = 4)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(fronts);
            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>
        /// La firma FÍSICA de una línea transversal: todas las piezas de su corte lateral, con identidad, posición y
        /// mano. Es lo que el dueño mira en AutoCAD.
        /// </summary>
        private static string LineSignature(PushBackSystem system, int line)
            => LineSignature(system, line, HighEndHand.Include);

        private static string LineSignature(PushBackSystem system, int line, HighEndHand hand)
            => string.Join(
                "\n",
                new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                    .Where(i => i.Role != HeaderBlockRole.Annotation && i.Role != HeaderBlockRole.Dimension)
                    .Select(i => FormattableString.Invariant(
                        $"{i.Role}|{i.PieceId}|{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{Hand(system, i, hand)}|{i.RotationRadians:0.####}"))
                    .OrderBy(x => x, StringComparer.Ordinal));

        /// <summary>
        /// La mano que entra en la firma. Con <see cref="HighEndHand.Ignore"/> la del larguero ALTO y la de su
        /// tope se omiten: son las dos unicas piezas cuya orientacion depende del EXTREMO de cabecera, que cambia
        /// legitimamente cuando el rack cambia de profundidad. La de cualquier otra pieza sigue comparandose.
        /// </summary>
        private static string Hand(PushBackSystem system, HeaderBlockInstance instance, HighEndHand hand)
            => hand == HighEndHand.Ignore
               && (instance.Role == HeaderBlockRole.Tope || IsHighBeam(system, instance))
                ? "-"
                : instance.MirroredX.ToString();

        /// <summary>Si la instancia es el larguero ALTO del sistema.</summary>
        private static bool IsHighBeam(PushBackSystem system, HeaderBlockInstance instance)
            => instance.Role == HeaderBlockRole.Beam
               && string.Equals(
                   instance.PieceId,
                   string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                       ? PushBackDefaults.HighEndBeamCatalogId
                       : system.HighEndBeamCatalogId,
                   StringComparison.OrdinalIgnoreCase);

        /// <summary>Si la firma incluye la mano del larguero alto y su tope.</summary>
        private enum HighEndHand
        {
            Include,
            Ignore
        }

        // ===================== capacidad ≠ presencia ==========================================================

        /// <summary>
        /// Declarar la CAPACIDAD no declara presencia en ningún frente: ninguna celda cambia de topología efectiva y
        /// el rack sigue siendo, celda a celda, el de un solo sentido.
        /// </summary>
        [Fact]
        public void DeclaringTheCapability_DoesNotMakeAnyFrontComposite()
        {
            var state = SingleSided();
            var before = Enumerable.Range(0, 4).Select(f => state.TopologyAt(f, 0)).ToList();

            state.SetSideBPresent(true);

            Assert.True(state.SideBPresent);
            Assert.All(Enumerable.Range(0, 4), f => Assert.False(state.IsSlotPresent(PushBackSide.B, f)));
            Assert.Equal(before, Enumerable.Range(0, 4).Select(f => state.TopologyAt(f, 0)).ToList());
            Assert.All(
                Enumerable.Range(0, 4),
                f => Assert.Equal(PushBackCellTopology.SoloA, state.TopologyAt(f, 0)));
        }

        /// <summary>
        /// Y el lado B puede declararse en CUALQUIER frente, uno por uno, en cualquier orden — incluido un orden no
        /// contiguo. Cada frente es independiente.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SideB_CanBeEnabledIndependently_OnEveryFront(int slot)
        {
            var state = SingleSided();
            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, slot, true);

            for (var f = 0; f < 4; f++)
            {
                Assert.Equal(f == slot, state.IsSlotPresent(PushBackSide.B, f));
                Assert.Equal(
                    f == slot ? PushBackCellTopology.Encontradas : PushBackCellTopology.SoloA,
                    state.TopologyAt(f, 0));
            }
        }

        /// <summary>Presencia NO CONTIGUA: B en el primero y el tercero, y nada obliga a rellenar el hueco.</summary>
        [Fact]
        public void NonContiguousSideBPresence_IsLegal()
        {
            var state = SingleSided();
            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            state.SetSlotPresent(PushBackSide.B, 2, true);

            var system = Build(state);
            Assert.True(system.IsComposite);
            Assert.Equal(PushBackCellTopology.Encontradas, system.Composite.Cell(0, 1).Topology);
            Assert.Equal(PushBackCellTopology.SoloA, system.Composite.Cell(1, 1).Topology);
            Assert.Equal(PushBackCellTopology.Encontradas, system.Composite.Cell(2, 1).Topology);
            Assert.Equal(PushBackCellTopology.SoloA, system.Composite.Cell(3, 1).Topology);
        }

        /// <summary>Quitar B de un frente y volver a ponerlo no toca a los demás ni destruye nada.</summary>
        [Fact]
        public void TogglingSideBOnOneFront_LeavesTheOthersAlone()
        {
            var state = SingleSided();
            state.SetSideBPresent(true);
            for (var f = 0; f < 4; f++)
            {
                state.SetSlotPresent(PushBackSide.B, f, true);
            }

            var full = Build(state);
            var others = new[] { 0, 2, 3 }.Select(line => LineSignature(full, line)).ToList();

            state.SetSlotPresent(PushBackSide.B, 1, false);
            state.SetSlotPresent(PushBackSide.B, 1, true);

            var again = Build(state);
            Assert.Equal(others, new[] { 0, 2, 3 }.Select(line => LineSignature(again, line)).ToList());
            Assert.True(state.IsSlotPresent(PushBackSide.B, 1));
        }

        // ===================== AISLAMIENTO: los frentes sin B no cambian ======================================

        /// <summary>
        /// LA PRUEBA DE NO REGRESIÓN DE ESTA RONDA. Se fotografían las líneas de F3 y F4 —las que solo sostienen
        /// frentes sin lado B— antes de convertir el rack, se declara B ÚNICAMENTE en F1, y se comprueba que esas
        /// líneas quedan FÍSICAMENTE IDÉNTICAS: misma identidad de pieza, misma posición, misma mano.
        /// </summary>
        [Fact]
        public void PartialComposite_F1Only_PreservesTheOtherFrontsExactly()
        {
            var state = SingleSided();
            var before = Build(state);
            var untouched = new[] { 3, 4 };   // las líneas que solo sostienen a F3 y F4
            var reference = untouched.ToDictionary(
                line => line, line => LineSignature(before, line, HighEndHand.Ignore));

            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            var after = Build(state);

            Assert.True(after.IsComposite);
            foreach (var line in untouched)
            {
                Assert.Equal(reference[line], LineSignature(after, line, HighEndHand.Ignore));
            }

            // I-42 (correccion aislada 5B) — la mano del larguero ALTO y de su tope puede cambiar al convertir el
            // rack, y es correcto: la decide un larguero INTERMEDIO en esa misma posicion fisica, y convertir el rack
            // cambia la secuencia de modulos. Lo que se exige es que siga a esa autoridad, no que se congele.
            var resolved = PushBackRuns.Resolve(after);
            foreach (var run in resolved.Runs)
            {
                var hand = DynamicIntermediateBeamGeometry.HandAtDepthX(
                    run.Source.Structure, PushBackCellDepth.RearX(run.Source, run.Front(), run.SourceLevel));
                if (!hand.HasValue)
                {
                    continue;   // apoyo que no es frontera de modulo: la pieza conserva la mano que traia
                }

                var expected = run.Reflected ? !hand.Value : hand.Value;
                var axis = PushBackRunGeometry.Axis(run, Catalog, resolved.MirrorAxis);
                Assert.True(
                    new PushBackSystemLateralBuilder().Build(after, Catalog).Flatten().Instances
                        .Where(i => IsHighBeam(after, i))
                        .Any(i => axis.HasValue
                                  && Math.Abs(i.Insertion.X - axis.Value.HighContact.X) < 12.0
                                  && i.MirroredX == expected),
                    $"la cama s{run.Slot}/n{run.Level} no lleva la mano del intermedio");
            }
        }

        /// <summary>
        /// Y en concreto la SEGURIDAD: las botas y los protectores de los frentes sin lado B conservan su regla
        /// adaptativa legacy. Una segunda cara de carga que existe solo en F1 no convierte en cara de carga la
        /// línea interior de F3 o F4 — que es exactamente lo que el dueño vio «invertido».
        /// </summary>
        [Fact]
        public void PartialComposite_DoesNotFlipSafetyOnLegacyFronts()
        {
            var state = SingleSided();
            var before = Build(state);
            IReadOnlyList<string> Safety(PushBackSystem system, int line) => new PushBackSystemLateralBuilder()
                .Build(system, Catalog, line).Flatten().Instances
                .Where(i => i.Role == HeaderBlockRole.Safety)
                .Select(i => FormattableString.Invariant($"{i.PieceId}|{i.Insertion.X:0.###}|{i.MirroredX}"))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            var reference = new[] { 2, 3, 4 }.ToDictionary(line => line, line => Safety(before, line));

            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            var after = Build(state);

            foreach (var line in new[] { 2, 3, 4 })
            {
                Assert.Equal(reference[line], Safety(after, line));
            }

            // Y ninguna selección adquiere Side = Both por el camino (el modelado que causó los errores 2 y 3).
            Assert.All(
                after.SafetySelections,
                selection => Assert.NotEqual(SafetySide.Both, selection.Side));
        }

        /// <summary>
        /// Los TOPES de los frentes que siguen siendo de un solo sentido no desaparecen: la conversión parcial no
        /// reinterpreta su intención.
        /// </summary>
        [Fact]
        public void PartialComposite_DoesNotRemoveLegacyTopes()
        {
            var state = SingleSided();
            var before = Build(state);
            double Topes(PushBackSystem system) => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(c => string.Equals(c.Category, PushBackBomBuilder.RearTope, StringComparison.Ordinal))
                .Sum(c => c.Quantity);

            var reference = Topes(before);
            Assert.True(reference > 0);

            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            var after = Build(state);

            // F1 pasa a tener DOS camas encontradas (dos topes donde había uno); los otros tres frentes conservan
            // exactamente los suyos. El total sube por F1, nunca baja.
            var levels = state.SideA.Structure.Fronts[0].LoadLevels;
            Assert.Equal(reference + levels, Topes(after));
        }

        // ===================== el datum de los dos lados ======================================================

        /// <summary>
        /// A y B con la misma intención visible arrancan en el MISMO troquel. Al crear el lado B, sus valores
        /// iniciales parten del lado A: dos convenciones distintas de «Alto 1er nivel» dejaban los dos lados medio
        /// paso de troquel desalineados sin que nada en la ventana lo dijera.
        /// </summary>
        [Fact]
        public void EqualAB_FirstLevel_UsesTheSamePunch()
        {
            var state = SingleSided(2);
            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            state.SetSlotPresent(PushBackSide.B, 1, true);

            Assert.Equal(
                state.SideA.Structure.Fronts[0].FirstLevelHeight,
                state.SideB.Structure.Fronts[0].FirstLevelHeight,
                9);

            var system = Build(state);
            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                .Where(axis => axis.Slot == 0)
                .ToList();
            var forward = axes.Where(a => a.FlowsForward).OrderBy(a => a.Level)
                .Select(a => Math.Round(a.LowContact.Y, 6)).ToList();
            var backward = axes.Where(a => !a.FlowsForward).OrderBy(a => a.Level)
                .Select(a => Math.Round(a.LowContact.Y, 6)).ToList();

            Assert.NotEmpty(forward);
            Assert.Equal(forward, backward);
        }

        /// <summary>
        /// Y sobre un lado A EXISTENTE con altura propia: el lado B recién creado adopta esa misma intención, así
        /// que el resultado físico es el que la ventana promete. No se copia nada más de A.
        /// </summary>
        [Fact]
        public void ExistingA_NewB_SameVisibleHeight_IsPhysicallyAligned()
        {
            var state = SingleSided(2);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideA.Structure.Fronts[slot].FirstLevelHeight = 14.0;
            }

            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            state.SetSlotPresent(PushBackSide.B, 1, true);

            Assert.Equal(14.0, state.SideB.Structure.Fronts[0].FirstLevelHeight, 9);

            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(Build(state)), Catalog)
                .Where(axis => axis.Slot == 0)
                .ToList();
            Assert.Equal(
                axes.Where(a => a.FlowsForward).OrderBy(a => a.Level).Select(a => Math.Round(a.LowContact.Y, 6)).ToList(),
                axes.Where(a => !a.FlowsForward).OrderBy(a => a.Level).Select(a => Math.Round(a.LowContact.Y, 6)).ToList());
        }

        // ===================== frontal B == lateral B =========================================================

        /// <summary>
        /// El corte FRONTAL de un lado y su corte LATERAL leen la MISMA elevación, y se comprueba con los dos lados
        /// deliberadamente DISTINTOS: con lados iguales la prueba pasaría por simetría sin demostrar nada.
        /// </summary>
        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void TheFrontalOfASide_UsesTheSameElevationsAsItsBeds(PushBackSide side)
        {
            var state = SingleSided(2);
            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            state.SetSlotPresent(PushBackSide.B, 1, true);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;   // B distinto de A a proposito
            }

            var system = Build(state);
            var runs = PushBackRuns.Resolve(system).Runs
                .Where(run => run.LowSide == side && run.Slot == 0)
                .OrderBy(run => run.Level)
                .ToList();
            Assert.NotEmpty(runs);

            var lows = runs
                .Select(run => Math.Round(
                    PushBackElevations.LowInsertions(run.Source, Catalog, run.Front())[run.SourceLevel], 4))
                .OrderBy(y => y)
                .ToList();
            var highs = runs
                .Select(run => Math.Round(
                    PushBackElevations.HighInsertions(run.Source, Catalog, run.Front())[run.SourceLevel], 4))
                .OrderBy(y => y)
                .ToList();

            IReadOnlyList<double> Frontal(PushBackFrontalEnd end, string pieceId)
                => PushBackCompositeFrontal.Build(system, Catalog, end, side).Flatten().Instances
                    .Where(i => string.Equals(i.PieceId, pieceId, StringComparison.OrdinalIgnoreCase))
                    .Select(i => Math.Round(i.Insertion.Y, 4))
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

            Assert.Equal(lows, Frontal(PushBackFrontalEnd.EntradaSalida, "LARGUERO_IN_OUT_C6"));
            Assert.Equal(highs, Frontal(PushBackFrontalEnd.Posterior, "LARGUERO_ESCALON_TROQUEL_REDONDO"));
        }
    }
}
