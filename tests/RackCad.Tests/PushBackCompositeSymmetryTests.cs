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
    /// I-42 (3a validacion) — SIMETRIA VERTICAL A/B e INDEPENDENCIA estructural.
    ///
    /// <para>
    /// Dos camas con condiciones fisicas equivalentes tienen que quedar a la misma altura: la reflexion del lado B
    /// cambia X y orientacion e invierte la pendiente, pero NO introduce ningun desplazamiento vertical. Y la misma
    /// autoridad de elevaciones tiene que servir al LATERAL y al FRONTAL: si divergen, el modelo es uno y el dibujo
    /// otro.
    /// </para>
    /// <para>
    /// Lo contrario tambien se fija aqui: en cuanto los lados dejan de ser equivalentes, SI pueden diferir, y una
    /// edicion de un lado no puede mover ni una pieza del otro (D2).
    /// </para>
    /// </summary>
    public class PushBackCompositeSymmetryTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackSystem Build(
            int levelsA, int levelsB, int deepA = 4, int deepB = 4, int? overrideA = null,
            PushBackCellTopology topology = PushBackCellTopology.Encontradas)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);
            state.SetDefaults(topology, PushBackRunDirection.AToB);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var front = 0; front < matrix.Count; front++)
                {
                    state.Of(side).AdjustLevels(
                        front, (side == PushBackSide.A ? levelsA : levelsB) - matrix.Fronts[front].LoadLevels);
                    matrix.Fronts[front].PalletsDeep = side == PushBackSide.A ? deepA : deepB;
                }
            }

            if (overrideA.HasValue)
            {
                state.SetStructureOverride(PushBackSide.A, overrideA.Value);
            }

            return new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;
        }

        /// <summary>Las elevaciones de las camas de un lado, en la ranura 0, por nivel.</summary>
        private static IReadOnlyList<(double Low, double High)> BedElevations(PushBackSystem system, PushBackSide side)
            => PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                .Where(axis => axis.Slot == 0)
                .Where(axis => axis.FlowsForward == (side == PushBackSide.A))
                .OrderBy(axis => axis.Level)
                .Select(axis => (Math.Round(axis.LowContact.Y, 4), Math.Round(axis.HighContact.Y, 4)))
                .ToList();

        // ================= B: misma configuracion, misma altura ================================================

        /// <summary>
        /// PRUEBA VINCULANTE. Con A y B identicos —mismos niveles, mismos fondos, misma altura inicial— cada nivel
        /// tiene EXACTAMENTE la misma elevacion en los dos lados. La reflexion no puede añadir ni una milesima.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void IdenticalSides_ProduceIdenticalBedElevations(int levels)
        {
            var system = Build(levels, levels);

            var sideA = BedElevations(system, PushBackSide.A);
            var sideB = BedElevations(system, PushBackSide.B);

            Assert.Equal(levels, sideA.Count);
            Assert.Equal(levels, sideB.Count);
            Assert.Equal(sideA, sideB);
        }

        /// <summary>
        /// Y la misma autoridad sirve al LATERAL y al FRONTAL: los dos hablan del mismo numero de niveles y de las
        /// mismas alturas relativas. Una divergencia entre builders es lo que hace que una vista se vea bien y la
        /// otra no.
        /// </summary>
        [Theory]
        [InlineData(3, 3)]
        [InlineData(4, 2)]
        [InlineData(2, 4)]
        public void TheLateralAndTheFrontal_AgreeOnEachSideLevels(int levelsA, int levelsB)
        {
            var system = Build(levelsA, levelsB);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var expected = side == PushBackSide.A ? levelsA : levelsB;

                var lateral = BedElevations(system, side);
                Assert.Equal(expected, lateral.Count);

                var frontal = new PushBackSystemFrontalBuilder()
                    .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, side).Flatten().Instances
                    .Where(instance => instance.Role == HeaderBlockRole.Beam)
                    .Select(instance => Math.Round(instance.Insertion.Y, 2))
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                Assert.Equal(expected, frontal.Count);

                // Las SEPARACIONES entre niveles son las mismas en las dos vistas: si una tuviera otro origen o otro
                // paso, aqui divergirian.
                for (var level = 1; level < expected; level++)
                {
                    Assert.Equal(
                        Math.Round(lateral[level].Low - lateral[level - 1].Low, 2),
                        Math.Round(frontal[level] - frontal[level - 1], 2),
                        2);
                }
            }
        }

        /// <summary>En cuanto los lados dejan de ser equivalentes, SI pueden —y deben— diferir.</summary>
        [Fact]
        public void DifferentSides_AreAllowedToDiffer()
        {
            var system = Build(levelsA: 4, levelsB: 2);

            Assert.Equal(4, BedElevations(system, PushBackSide.A).Count);
            Assert.Equal(2, BedElevations(system, PushBackSide.B).Count);
        }

        // ================= K: una edicion de un lado no mueve el otro ===========================================

        /// <summary>Lo que describe a un lado: su propuesta, su estructura efectiva, su longitud y su altura.</summary>
        private static string Describe(PushBackSystem system, PushBackSide side)
        {
            var view = system.Composite.Of(side);
            return string.Join(
                "|",
                view.ProposedStructure,
                view.EffectiveStructure,
                Math.Round(view.Local.Structure.TotalLength, 4),
                Math.Round(view.Local.Structure.Fronts[0].Height, 4));
        }

        /// <summary>
        /// PRUEBA VINCULANTE (D2). Subir NIVELES en A, subir su FONDO o fijarle un ajuste manual de estructura no
        /// puede cambiar NADA del lado B: ni su propuesta, ni su estructura efectiva, ni su longitud, ni su altura.
        /// </summary>
        [Fact]
        public void EditingOneSide_NeverMovesTheOther()
        {
            var baseline = Build(levelsA: 2, levelsB: 2);
            var beforeB = Describe(baseline, PushBackSide.B);
            var beforeA = Describe(baseline, PushBackSide.A);

            // 1) Mas NIVELES en A: solo cambia la altura de A.
            var moreLevels = Build(levelsA: 4, levelsB: 2);
            Assert.Equal(beforeB, Describe(moreLevels, PushBackSide.B));
            Assert.NotEqual(beforeA, Describe(moreLevels, PushBackSide.A));

            // 2) Mas FONDO en A: solo cambia la longitud y la propuesta de A.
            var deeper = Build(levelsA: 2, levelsB: 2, deepA: 8);
            Assert.Equal(beforeB, Describe(deeper, PushBackSide.B));
            Assert.NotEqual(beforeA, Describe(deeper, PushBackSide.A));

            // 3) Ajuste manual de estructura en A: solo cambia A.
            var overridden = Build(levelsA: 2, levelsB: 2, overrideA: 9);
            Assert.Equal(beforeB, Describe(overridden, PushBackSide.B));
            Assert.NotEqual(beforeA, Describe(overridden, PushBackSide.A));
        }

        /// <summary>Y al reves: subir B no toca A. La independencia no tiene lado preferente.</summary>
        [Fact]
        public void EditingSideB_NeverMovesSideA()
        {
            var baseline = Build(levelsA: 2, levelsB: 2);
            var beforeA = Describe(baseline, PushBackSide.A);

            var raised = Build(levelsA: 2, levelsB: 4, deepB: 8);
            Assert.Equal(beforeA, Describe(raised, PushBackSide.A));
            Assert.NotEqual(Describe(baseline, PushBackSide.B), Describe(raised, PushBackSide.B));
        }
    }
}
