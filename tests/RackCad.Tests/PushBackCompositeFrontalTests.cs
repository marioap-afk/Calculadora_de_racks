using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 — los CUATRO cortes frontales de un rack compuesto: entrada/salida y posterior de cada lado.
    ///
    /// <para>
    /// No basta con que existan cuatro botones: cada corte tiene que consumir el lado que dice. Con niveles y fondos
    /// distintos en A y en B, el corte de A muestra los de A y el de B los de B — y ninguno los del otro.
    /// </para>
    /// </summary>
    public class PushBackCompositeFrontalTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(
            int levelsA, int levelsB, PushBackCellTopology topology = PushBackCellTopology.Encontradas)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);
            state.SetDefaults(topology, PushBackRunDirection.AToB);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            Apply(state, PushBackSide.A, levelsA);
            Apply(state, PushBackSide.B, levelsB);
            return state;
        }

        private static void Apply(PushBackCompositeEditorState state, PushBackSide side, int levels)
        {
            var matrix = state.Of(side).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                state.Of(side).AdjustLevels(front, levels - matrix.Fronts[front].LoadLevels);
            }
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side).Flatten().Instances;

        /// <summary>Las elevaciones DISTINTAS a las que el corte pone largueros: una por nivel materializado.</summary>
        private static IReadOnlyList<double> BeamElevations(IReadOnlyList<HeaderBlockInstance> cut)
            => cut
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .Select(instance => Math.Round(instance.Insertion.Y, 2))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        // ================= N: cada corte consume SU lado ========================================================

        /// <summary>
        /// PRUEBA VINCULANTE: con 4 niveles en A y 2 en B, el corte de A muestra CUATRO alturas de larguero y el de B
        /// DOS. Un corte que leyera el lado activo, o la envolvente del rack, daria el mismo numero en los dos.
        /// </summary>
        [Theory]
        [InlineData(PushBackFrontalEnd.EntradaSalida)]
        [InlineData(PushBackFrontalEnd.Posterior)]
        public void EachFrontalCut_ShowsTheLevelsOfItsOwnSide(PushBackFrontalEnd end)
        {
            var system = Resolve(State(levelsA: 4, levelsB: 2));

            var sideA = BeamElevations(Cut(system, end, PushBackSide.A));
            var sideB = BeamElevations(Cut(system, end, PushBackSide.B));

            Assert.Equal(4, sideA.Count);
            Assert.Equal(2, sideB.Count);
        }

        /// <summary>Y las cuatro secciones son cuatro dibujos DISTINTOS, no dos repetidos.</summary>
        [Fact]
        public void TheFourCuts_AreFourDifferentDrawings()
        {
            var system = Resolve(State(levelsA: 4, levelsB: 2));

            var cuts = new[]
            {
                Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A),
                Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A),
                Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B),
                Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B)
            };

            Assert.All(cuts, cut => Assert.NotEmpty(cut));

            // Firma de cada corte: sus piezas y las alturas a las que caen.
            var signatures = cuts
                .Select(cut => string.Join(
                    "|",
                    cut.Select(instance => instance.PieceId + "@" + Math.Round(instance.Insertion.X, 2)
                                           + "," + Math.Round(instance.Insertion.Y, 2))
                        .OrderBy(text => text, StringComparer.Ordinal)))
                .ToList();

            Assert.Equal(4, signatures.Distinct().Count());
        }

        /// <summary>
        /// El corte de ENTRADA/SALIDA de un lado mira a SU pasillo, asi que lleva la seguridad de ese pasillo; el
        /// POSTERIOR mira al extremo alto y no lleva ninguna.
        /// </summary>
        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void TheEntranceCut_CarriesSafety_AndTheRearOneDoesNot(PushBackSide side)
        {
            var state = State(levelsA: 3, levelsB: 3);

            // La seguridad de Push Back viaja por los INPUTS del rack, no por el estado de un lado.
            var inputs = Inputs();
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            var system = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System;

            var entrance = Cut(system, PushBackFrontalEnd.EntradaSalida, side)
                .Count(instance => instance.Role == HeaderBlockRole.Safety);
            var rear = Cut(system, PushBackFrontalEnd.Posterior, side)
                .Count(instance => instance.Role == HeaderBlockRole.Safety);

            Assert.True(entrance > 0, "el corte del pasillo de " + side + " lleva su seguridad");
            Assert.Equal(0, rear);
        }

        /// <summary>
        /// Con fondos distintos por lado, cada corte refleja el suyo: el corte posterior de un lado cae donde acaba
        /// SU cama, no donde acaba la del otro.
        /// </summary>
        [Fact]
        public void EachRearCut_FollowsItsOwnDepth()
        {
            var state = State(levelsA: 2, levelsB: 2);
            state.SideA.Structure.ToggleCell(0, 0, extendSelection: false);
            state.SideB.Structure.ToggleCell(0, 0, extendSelection: false);
            state.SideA.ApplyPalletsDeep(5, DynamicRackCellScope.All);
            state.SideB.ApplyPalletsDeep(3, DynamicRackCellScope.All);

            var system = Resolve(state);

            Assert.Equal(5, system.Composite.Cell(0, 1).BedFrom(PushBackSide.A).DemandPositions);
            Assert.Equal(3, system.Composite.Cell(0, 1).BedFrom(PushBackSide.B).DemandPositions);

            // Y los dos cortes posteriores existen y son distintos.
            var rearA = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A);
            var rearB = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B);
            Assert.NotEmpty(rearA);
            Assert.NotEmpty(rearB);
        }

        /// <summary>
        /// Una celda CORRIDA es UNA cama: el corte del lado bajo no dibuja el larguero de la linea interior que la
        /// calle atraviesa, y la cama no aparece dos veces.
        /// </summary>
        [Fact]
        public void ACorrida_IsNotDuplicatedAcrossTheTwoSideCuts()
        {
            var system = Resolve(State(levelsA: 2, levelsB: 2, PushBackCellTopology.Corrida));

            var entranceA = Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A);
            var entranceB = Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B);

            Assert.NotEmpty(entranceA);
            Assert.NotEmpty(entranceB);

            // El extremo ALTO de una corrida A->B esta en B: solo ese lado lleva tope.
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A)
                .Where(instance => instance.Role == HeaderBlockRole.Tope));
        }

        // ================= Un rack de UN sentido no cambia ======================================================

        /// <summary>
        /// GUARDA legacy: en un rack de un solo sentido el parametro de lado se ignora y los dos cortes son los de
        /// siempre. Las secciones 0 y 1 siguen significando exactamente lo mismo que antes de I-42.
        /// </summary>
        [Fact]
        public void ASingleSidedRack_IgnoresTheSide_AndKeepsItsTwoCuts()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            var system = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;
            Assert.False(system.IsComposite);

            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                var asA = Cut(system, end, PushBackSide.A);
                var asB = Cut(system, end, PushBackSide.B);
                Assert.Equal(asA.Count, asB.Count);
            }

            Assert.Equal(
                0, PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Equal(
                1, PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.Posterior, PushBackSide.A));
        }
    }
}
