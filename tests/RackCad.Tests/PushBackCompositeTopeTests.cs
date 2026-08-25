using System;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda Owner) — los TOPES de los dos lados: ninguno / solo A / solo B / ambos.
    ///
    /// <para>
    /// Era requisito vinculante de la iniciativa y el dueño no encontro forma de decidirlo. La autoridad no cambia
    /// —sigue siendo el <see cref="PushBackRearTopeConfig"/> de cada lado y sus celdas apagadas—; lo que se fija aqui
    /// es que las cuatro combinaciones existen, que la topologia decide cual puede MATERIALIZARSE y que la intencion
    /// del lado que hoy no aplica queda DORMANTE en vez de destruirse.
    /// </para>
    /// </summary>
    public class PushBackCompositeTopeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            PushBackRunDirection direction = PushBackRunDirection.AToB)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();   // como la ventana: los dos lados nacen con los defaults del producto
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);
            state.SetDefaults(topology, direction);
            return state;
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>Los topes que el LATERAL materializa realmente, por lado, para la celda (0, 1).</summary>
        private static (int A, int B) DrawnTopes(PushBackSystem system)
        {
            var runs = PushBackRuns.Resolve(system);
            var content = PushBackCompositeContent.Lateral(
                system, Catalog, runs, slot => slot == 0, int.MaxValue, -1);
            var topes = content.Loose
                .Concat(content.Headers.SelectMany(group => group.Instances))
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .ToList();

            // El tope vive en el extremo ALTO de su cama. En A ese extremo mira hacia +X y en B hacia -X, asi que el
            // lado se lee por la mitad del rack en la que cae, que es donde fisicamente esta.
            var middle = system.Structure.TotalLength / 2.0;
            return (topes.Count(t => t.Insertion.X < middle), topes.Count(t => t.Insertion.X >= middle));
        }

        // ================= Encontradas: las cuatro combinaciones ================================================

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Encontradas_SupportTheFourCombinations(bool topeA, bool topeB)
        {
            var state = State(PushBackCellTopology.Encontradas);
            state.ApplyRearTope(PushBackSide.A, topeA, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, topeB, DynamicRackCellScope.All);

            var drawn = DrawnTopes(Resolve(state));

            Assert.Equal(topeA, drawn.A > 0);
            Assert.Equal(topeB, drawn.B > 0);
        }

        /// <summary>Con camas encontradas, los DOS lados admiten tope y la ventana lo dice.</summary>
        [Fact]
        public void Encontradas_DeclareBothSidesApplicable()
        {
            var applicability = State(PushBackCellTopology.Encontradas).TopeApplicability(0, 0);
            Assert.True(applicability.A);
            Assert.True(applicability.B);
        }

        // ================= Una sola cama: solo su lado ALTO puede llevar tope ===================================

        [Theory]
        [InlineData(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, true, false)]
        [InlineData(PushBackCellTopology.SoloB, PushBackRunDirection.AToB, false, true)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, false, true)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, true, false)]
        public void OnlyTheHighSide_CanCarryATope(
            PushBackCellTopology topology, PushBackRunDirection direction, bool expectA, bool expectB)
        {
            var state = State(topology, direction);
            var applicability = state.TopeApplicability(0, 0);
            Assert.Equal(expectA, applicability.A);
            Assert.Equal(expectB, applicability.B);

            // Y con los dos encendidos, el dibujo materializa UNO solo: el del lado alto.
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, true, DynamicRackCellScope.All);
            var drawn = DrawnTopes(Resolve(state));

            Assert.Equal(expectA, drawn.A > 0);
            Assert.Equal(expectB, drawn.B > 0);
        }

        // ================= La intencion del lado que no aplica queda DORMANTE ===================================

        /// <summary>
        /// Cambiar la topologia —o el sentido— no destruye lo que el usuario eligio para el otro lado. Volver a
        /// encontradas devuelve las DOS elecciones exactamente como estaban.
        /// </summary>
        [Fact]
        public void ChangingTheTopology_KeepsTheOtherSideTopeDormant()
        {
            var state = State(PushBackCellTopology.Encontradas);
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, false, DynamicRackCellScope.All);

            var before = DrawnTopes(Resolve(state));
            Assert.True(before.A > 0);
            Assert.True(before.B == 0);

            // Corrida A->B: el extremo alto pasa a B, que tiene el tope APAGADO. No aparece ninguno...
            state.ApplyTopology(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, DynamicRackCellScope.All);
            var corrida = DrawnTopes(Resolve(state));
            Assert.Equal(0, corrida.A);
            Assert.Equal(0, corrida.B);

            // ...pero la intencion de A sigue viva y vuelve intacta.
            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 0));
            Assert.False(state.RearTopeAt(PushBackSide.B, 0, 0));

            state.ApplyTopology(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, DynamicRackCellScope.All);
            var after = DrawnTopes(Resolve(state));
            Assert.Equal(before.A, after.A);
            Assert.Equal(before.B, after.B);
        }

        /// <summary>Cambiar solo el SENTIDO de una corrida mueve el tope de lado, sin perder ninguna eleccion.</summary>
        [Fact]
        public void FlippingTheRunDirection_MovesTheTope_WithoutLosingIntent()
        {
            var state = State(PushBackCellTopology.Corrida);
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, true, DynamicRackCellScope.All);

            var forward = DrawnTopes(Resolve(state));
            Assert.Equal(0, forward.A);
            Assert.True(forward.B > 0);

            state.ApplyTopology(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, DynamicRackCellScope.All);
            var backward = DrawnTopes(Resolve(state));
            Assert.True(backward.A > 0);
            Assert.Equal(0, backward.B);

            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 0));
            Assert.True(state.RearTopeAt(PushBackSide.B, 0, 0));
        }

        // ================= Los cinco alcances y la persistencia =================================================

        [Fact]
        public void TheTopeScopes_AreTheSameFive_AndWriteOnlyTheirTargets()
        {
            var state = State(PushBackCellTopology.Encontradas);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var front = 0; front < matrix.Count; front++)
                {
                    state.Of(side).AdjustLevels(front, 3 - matrix.Fronts[front].LoadLevels);
                }
            }
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);

            state.SideA.Structure.ToggleCell(1, 1, extendSelection: false);   // F2/L2
            Assert.Equal(1, state.ApplyRearTope(PushBackSide.A, false, DynamicRackCellScope.Cell));

            Assert.False(state.RearTopeAt(PushBackSide.A, 1, 1));
            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 1));
            Assert.True(state.RearTopeAt(PushBackSide.A, 1, 0));
            Assert.True(state.RearTopeAt(PushBackSide.A, 1, 2));

            // Y el lado B no se entera: son dos autoridades.
            Assert.True(state.RearTopeAt(PushBackSide.B, 1, 1));

            // Nivel: ese nivel de todos los frentes.
            Assert.Equal(2, state.ApplyRearTope(PushBackSide.A, false, DynamicRackCellScope.Level));
            Assert.False(state.RearTopeAt(PushBackSide.A, 0, 1));
            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 0));

            // Frente: todos los niveles del frente seleccionado.
            Assert.Equal(3, state.ApplyRearTope(PushBackSide.A, false, DynamicRackCellScope.Front));
            Assert.False(state.RearTopeAt(PushBackSide.A, 1, 2));
            Assert.True(state.RearTopeAt(PushBackSide.A, 0, 0));
        }

        [Fact]
        public void TheTwoTopeConfigurations_SurviveASaveAndLoad()
        {
            var state = State(PushBackCellTopology.Encontradas);
            state.ApplyRearTope(PushBackSide.A, true, DynamicRackCellScope.All);
            state.ApplyRearTope(PushBackSide.B, false, DynamicRackCellScope.All);

            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var restored = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();

            var before = DrawnTopes(new PushBackResolver(Catalog).Resolve(design));
            var after = DrawnTopes(new PushBackResolver(Catalog).Resolve(restored));

            Assert.Equal(before.A, after.A);
            Assert.Equal(before.B, after.B);
            Assert.True(before.A > 0);
            Assert.Equal(0, before.B);
        }

        /// <summary>El BOM cuenta los topes que se DIBUJAN: ni uno inventado por el lado que no lleva.</summary>
        [Fact]
        public void TheBom_CountsExactlyTheDrawnTopes()
        {
            foreach (var (topeA, topeB) in new[] { (false, false), (true, false), (false, true), (true, true) })
            {
                var state = State(PushBackCellTopology.Encontradas);
                state.ApplyRearTope(PushBackSide.A, topeA, DynamicRackCellScope.All);
                state.ApplyRearTope(PushBackSide.B, topeB, DynamicRackCellScope.All);

                var system = Resolve(state);
                var runs = PushBackRuns.Resolve(system);
                var expected = runs.Runs.Count(run =>
                    run.HighSide == PushBackSide.A ? topeA : topeB);

                var quoted = PushBackBomBuilder.Build(system, Catalog).Components
                    .Where(component => component.Category == PushBackBomBuilder.RearTope)
                    .Sum(component => component.Quantity);

                Assert.Equal(expected, quoted);
            }
        }
    }
}
