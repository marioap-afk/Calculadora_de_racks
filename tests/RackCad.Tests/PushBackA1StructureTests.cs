using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1) — LA RETICULA COMPARTIDA NO DECLARA ALMACENAMIENTO, y el corte lateral no depende de que el primer
    /// poste lleve pieza.
    /// </summary>
    public class PushBackA1StructureTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState Composite(int slots)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return state;
        }

        // ==================================================================== H5

        /// <summary>
        /// H5 — anadir un frente en A amplia la rejilla compartida, pero la ranura nueva del lado B nace AUSENTE.
        /// Antes nacia activa: crecer por A convertia el lado B en almacenamiento sin que nadie lo decidiera.
        /// </summary>
        [Fact]
        public void AddingFrontA_DoesNotInventPresenceInB()
        {
            var state = Composite(3);
            state.SetActiveSide(PushBackSide.A);

            state.SetSlotCount(4);

            Assert.True(state.IsSlotPresent(PushBackSide.A, 3));
            Assert.False(state.IsSlotPresent(PushBackSide.B, 3));
            Assert.True(state.IsSlotPresent(PushBackSide.B, 2));   // lo declarado antes no se toca
        }

        /// <summary>Y simetrico: creciendo desde B, la ranura nueva de A nace ausente.</summary>
        [Fact]
        public void AddingFrontB_DoesNotInventPresenceInA()
        {
            var state = Composite(3);
            state.SetActiveSide(PushBackSide.B);

            state.SetSlotCount(4);

            Assert.True(state.IsSlotPresent(PushBackSide.B, 3));
            Assert.False(state.IsSlotPresent(PushBackSide.A, 3));
        }

        /// <summary>
        /// BITE F — si la rejilla volviera a declarar presencia, el lado dormido pasaria a almacenar solo, que es lo
        /// que este contrato prohibe.
        /// </summary>
        [Fact]
        public void Bite_SharedGridDeclaringPresence_BreaksTheDormantSide()
        {
            var state = Composite(2);
            state.SetActiveSide(PushBackSide.A);
            state.SetSlotCount(5);

            Assert.All(
                new[] { 2, 3, 4 },
                slot => Assert.False(state.IsSlotPresent(PushBackSide.B, slot)));
        }

        // ==================================================================== H6

        /// <summary>
        /// H6 — el corte lateral proyecta el PLAN FISICO de botas. Antes, si el pipeline compartido no dibujaba
        /// ninguna, la familia entera desaparecia de esta vista: bastaba con que el primer poste no llevara bota
        /// mientras la planta, los cortes y el BOM si las tenian.
        /// </summary>
        [Fact]
        public void CompositeLateral_BootsDoNotDependOnPostZero()
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).Build(Composite(3), Inputs(), Catalog).Design;
            design.Structure.SafetySelections.Clear();
            var selection = new SelectiveSafetySelection
            {
                ElementId = BootId,
                Quantity = 1,
                BootSidesDeclared = true,
            };
            selection.Bota.Placement = BootPlacement.None;
            selection.BotaB.Placement = BootPlacement.None;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 0, Placement = BootPlacement.None });
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.Rear });
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Both });
            design.Structure.SafetySelections.Add(selection);

            var system = new PushBackResolver(Catalog).Resolve(design);
            var physical = PushBackBootPlan.Resolve(system, Catalog);
            var lateral = new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, BootId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(physical);                      // el rack SI lleva botas…
            Assert.NotEmpty(lateral);                       // …y el lateral las proyecta
            Assert.Equal(
                physical.Select(boot => Math.Round(boot.FaceX, 3)).Distinct().OrderBy(x => x).ToList(),
                lateral.Select(instance => Math.Round(instance.Insertion.X, 3)).Distinct().OrderBy(x => x).ToList());
        }
    }
}
