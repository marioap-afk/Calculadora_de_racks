using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (corrección aislada 5D, defecto B) — UN POSTE REFORZADO NO DUPLICA EL APOYO DE LA CAMA.
    ///
    /// <para>
    /// Regla física ya cerrada: una cama tiene exactamente 1 larguero BAJO, N intermedios y 1 ALTO. El refuerzo
    /// pertenece a la ESTRUCTURA del poste; no añade un segundo apoyo funcional.
    /// </para>
    /// <para>
    /// El defecto: en un poste derivado y REFORZADO, <c>FIN_POSTE</c> es la interfaz donde acaba el perfil primario y
    /// empieza el refuerzo, así que el apoyo se DIBUJA una <c>finPoste.X</c> antes de su frontera. El filtro de
    /// aplicabilidad —«¿este apoyo cae dentro de la cama?»— comparaba la X dibujada contra el larguero alto, así que
    /// el apoyo de la frontera donde ACABA la cama se colaba por delante: dos largueros para un solo apoyo físico, y
    /// contados dos veces en el BOM. Medido en una estructura de ocho fondos: la cama de 4 fondos acaba en X=198, un
    /// límite vano-vano, y salían un <c>TROQUEL_REDONDO</c> en 198 y un <c>ESCALON_INFINITO</c> en 195.
    /// </para>
    /// <para>
    /// La corrección NO deduplica por posición: el apoyo lleva ahora su FRONTERA, que es su identidad física, y el
    /// filtro pregunta por ella. Camas distintas siguen produciendo piezas distintas aunque coincidan (encontradas).
    /// </para>
    /// </summary>
    public class PushBackReinforcedPostBeamTests
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

        /// <summary>
        /// Ocho fondos con una cama por nivel, de 3 a 8. Su único límite vano-vano está en X=198, así que la cama de
        /// 4 fondos acaba EXACTAMENTE en un poste derivado —reforzado o no, según <paramref name="reinforced"/>.
        /// </summary>
        private static PushBackSystem DepthLadder(bool reinforced)
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = 8;
            inputs.DerivedPostReinforced = reinforced;
            state.SetFrontCount(1);
            state.Structure.Fronts[0].LoadLevels = 6;
            state.Structure.Fronts[0].PalletsDeep = 8;
            state.AdjustLevels(0, 0);
            for (var level = 0; level < 6; level++)
            {
                state.ToggleCell(0, level, false);
                state.ApplyPalletsDeep(level + 3, DynamicRackCellScope.Cell);
            }

            var computation = new PushBackEditorDesignAssembler(Catalog).Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation.System;
        }

        private static PushBackCompositeEditorState State(int slots = 2, int levels = 2)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var index = 0; index < matrix.Count; index++)
                {
                    state.Of(side).AdjustLevels(index, levels - matrix.Fronts[index].LoadLevels);
                }
            }

            return state;
        }

        private static PushBackSystem Composite(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots = 2)
        {
            var state = State(slots);
            state.SetDefaults(topology, direction);
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas por CAMA (frente x nivel), que es la unidad física ----------------------------------------

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        /// <summary>Los largueros de UNA cama: bajo, intermedios y alto, pedidos a las autoridades que los colocan.</summary>
        private static (IReadOnlyList<HeaderBlockInstance> Low,
                        IReadOnlyList<HeaderBlockInstance> Intermediate,
                        IReadOnlyList<HeaderBlockInstance> High) BedBeams(
            PushBackSystem system, DynamicRackFront front, int frontIndex, int level)
        {
            var levels = new[] { level };
            var low = PushBackLoadBeamGeometry.LowBeams(system, Catalog, front)
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .ToList();
            var high = PushBackLoadBeamGeometry.HighBeams(system, Catalog, frontIndex, front, levels).ToList();
            var intermediate = new PushBackIntermediateBeamLateralBuilder()
                .BuildFor(system, Catalog, front, levels)
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .ToList();
            return (low, intermediate, high);
        }

        /// <summary>
        /// LA FRONTERA FÍSICA a la que sirve un larguero dibujado en <paramref name="x"/>: el final de módulo más
        /// cercano. En un poste derivado y reforzado el dibujo se separa una <c>FIN_POSTE</c> de su frontera, y es
        /// justo esa separación la que escondía el duplicado.
        /// </summary>
        private static double Boundary(DynamicRackSystem structure, double x)
        {
            var best = double.NaN;
            var distance = double.MaxValue;
            foreach (var module in structure.Modules.Where(module => module.Length > 0.0))
            {
                foreach (var candidate in new[] { module.StartX, module.EndX })
                {
                    var delta = Math.Abs(candidate - x);
                    if (delta < distance)
                    {
                        distance = delta;
                        best = candidate;
                    }
                }
            }

            return best;
        }

        // ---- la regla: 1 BAJO, N intermedios, 1 ALTO, y una sola pieza por frontera -----------------------------

        /// <summary>
        /// Ninguna cama pone DOS largueros funcionales en la misma frontera física, ni con el poste derivado
        /// reforzado ni sin él. Y cada una tiene exactamente un bajo y un alto.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReinforcedPost_DoesNotAddASecondFunctionalBeam(bool reinforced)
        {
            var system = DepthLadder(reinforced);
            var front = system.Structure.Fronts[0];
            var label = reinforced ? "poste derivado REFORZADO" : "poste derivado sin refuerzo";

            // Un larguero BAJO por cama, todos en el arranque del frente: el extremo bajo lo comparte el pasillo y no
            // depende del fondo de la celda.
            var low = PushBackLoadBeamGeometry.LowBeams(system, Catalog, front)
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .ToList();
            Assert.Equal(6, low.Count);
            Assert.All(low, instance => Assert.Equal(front.StartX, instance.Insertion.X, 6));

            for (var level = 1; level <= 6; level++)
            {
                var beams = BedBeams(system, front, 0, level);
                Assert.Single(beams.High);

                var boundaries = beams.Intermediate
                    .Select(instance => Boundary(system.Structure, instance.Insertion.X))
                    .Concat(beams.High.Select(instance => Boundary(system.Structure, instance.Insertion.X)))
                    .ToList();
                var duplicated = boundaries.GroupBy(value => Math.Round(value, 4))
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();
                Assert.True(
                    duplicated.Count == 0,
                    $"{label}, cama de {level + 2} fondos: dos largueros para la misma frontera "
                        + string.Join(", ", duplicated.Select(value => value.ToString("0.###"))));
            }
        }

        /// <summary>
        /// EL CASO MEDIDO: la cama de 4 fondos acaba EXACTAMENTE en el límite vano-vano (X=198). Su larguero alto va
        /// ahí, y el apoyo de esa misma frontera —que el refuerzo dibuja en 195— no debe existir para esta cama.
        /// </summary>
        [Fact]
        public void BedEndingOnTheReinforcedBoundary_HasNoSupportThere()
        {
            var system = DepthLadder(reinforced: true);
            var front = system.Structure.Fronts[0];
            var beams = BedBeams(system, front, 0, level: 2);   // nivel 2 = cama de 4 fondos

            Assert.Single(beams.High);
            Assert.Equal(198.0, beams.High[0].Insertion.X, 3);
            Assert.DoesNotContain(beams.Intermediate, instance => Math.Abs(instance.Insertion.X - 195.0) < 1e-6);

            // Y la cama SIGUIENTE, que pasa de largo por esa frontera, sí lo conserva: no se ha borrado el apoyo,
            // se ha dejado de duplicar el de la cama que acaba ahí.
            var deeper = BedBeams(system, front, 0, level: 3);   // cama de 5 fondos
            Assert.Contains(deeper.Intermediate, instance => Math.Abs(instance.Insertion.X - 195.0) < 1e-6);
        }

        /// <summary>
        /// El número de largueros funcionales de una cama NO depende de que su poste derivado lleve refuerzo: el
        /// refuerzo es estructura del poste. Es la comprobación más limpia de que el duplicado era eso, un duplicado.
        /// </summary>
        [Fact]
        public void FunctionalBeamCount_IsIndependentOfTheReinforcement()
        {
            var reinforced = DepthLadder(reinforced: true);
            var plain = DepthLadder(reinforced: false);

            for (var level = 1; level <= 6; level++)
            {
                var a = BedBeams(reinforced, reinforced.Structure.Fronts[0], 0, level);
                var b = BedBeams(plain, plain.Structure.Fronts[0], 0, level);
                Assert.Equal(b.Intermediate.Count, a.Intermediate.Count);
                Assert.Equal(b.High.Count, a.High.Count);
            }
        }

        /// <summary>
        /// Y NO se ha deduplicado a ciegas: unas camas ENCONTRADAS siguen conservando sus DOS largueros altos, que
        /// pertenecen a dos camas distintas aunque topen en la misma frontera. La decisión de la ronda 5B se
        /// mantiene intacta.
        /// </summary>
        [Fact]
        public void Encontradas_StillKeepTheirTwoIndependentHighBeams()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 1);
            var highId = HighBeamId(system);
            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances;

            Assert.Equal(2, planta.Count(instance => instance.Role == HeaderBlockRole.Beam
                && string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase)));
            Assert.Equal(2, planta.Count(instance => instance.Role == HeaderBlockRole.Tope));
        }

        /// <summary>
        /// EL BOM NO SE MUEVE con esta corrección, y estos son los conteos exactos del escenario. Fijarlos aquí es
        /// lo que hace que un cambio silencioso de cantidades no pase.
        ///
        /// <para>
        /// OBSERVACIÓN REPORTADA, NO ARREGLADA AQUÍ: el BOM de un rack de UN SOLO SENTIDO cuenta 42 intermedios
        /// —siete fronteras por seis niveles— mientras el dibujo pone 27, porque ahí el conteo sigue siendo el del
        /// Dinámico y no aplica el fondo POR CELDA. Es una brecha anterior a esta ronda, ajena al poste reforzado
        /// —no cambia con el refuerzo— y de la familia de I-41, no de I-42. Se deja declarada.
        /// </para>
        /// </summary>
        [Fact]
        public void Bom_QuantitiesAreUnchanged()
        {
            foreach (var reinforced in new[] { true, false })
            {
                var bom = PushBackBomBuilder.Build(DepthLadder(reinforced), Catalog);
                int Quantity(string category)
                    => bom.Components.Where(component => component.Category == category).Sum(component => component.Quantity);

                Assert.Equal(42, Quantity(SystemBomBuilder.IntermediateBeam));
                Assert.Equal(6, Quantity(PushBackBomBuilder.RearTope));
                Assert.Equal(6, Quantity(SystemBomBuilder.InOutBeam));
                Assert.Equal(6, Quantity(PushBackBomBuilder.HighEndBeam));
            }
        }

        /// <summary>
        /// Y unas encontradas siguen comprando DOS de cada pieza alta por nivel: dos camas, dos largueros altos y dos
        /// topes, aunque topen en la misma frontera. Con dos niveles, cuatro de cada.
        /// </summary>
        [Fact]
        public void Encontradas_BomStillBuysTwoOfEachPerLevel()
        {
            var bom = PushBackBomBuilder.Build(
                Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 1), Catalog);
            int Quantity(string category)
                => bom.Components.Where(component => component.Category == category).Sum(component => component.Quantity);

            Assert.Equal(4, Quantity(PushBackBomBuilder.RearTope));
            Assert.Equal(4, Quantity(PushBackBomBuilder.HighEndBeam));
        }
    }
}
