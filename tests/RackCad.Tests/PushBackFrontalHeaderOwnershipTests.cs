using System;
using System.Collections.Generic;
using System.Linq;
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
    /// I-42 (corrección aislada 2C) — un corte FRONTAL proyecta sólo las líneas de cabecera de SU lado.
    ///
    /// <para>
    /// La estructura física global es la UNIÓN de lo que necesitan A y B, y la PLANTA la dibuja entera porque
    /// representa el rack. Un corte frontal no: es de un lado. Con la primera ranura en blanco en A y almacenamiento
    /// de B ahí, la línea exterior existe en el rack —y la planta la muestra— pero el corte de A no la posee.
    /// </para>
    /// <para>
    /// La regla no es nueva: es la MISMA continuidad de <c>BoundaryExists</c>, evaluada sobre la activación del
    /// LADO. Lo único que decae es la excepción de los bordes exteriores, que son del RACK y no de un lado.
    /// </para>
    /// </summary>
    public class PushBackFrontalHeaderOwnershipTests
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

        private static PushBackSystem Rack(int slots, params (int Slot, PushBackSide Side)[] blanks)
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
                    state.Of(side).AdjustLevels(index, 2 - matrix.Fronts[index].LoadLevels);
                }
            }

            foreach (var blank in blanks)
            {
                Assert.True(
                    state.SetSlotPresent(blank.Side, blank.Slot, false),
                    $"el editor rechazó poner en blanco la ranura {blank.Slot} del lado {blank.Side}");
            }

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas: las LÍNEAS de cada vista, por índice ------------------------------------------------------

        /// <summary>Las líneas que existen en la estructura física del rack (la unión de A y B).</summary>
        private static IReadOnlyList<int> GlobalLines(PushBackSystem system)
            => DynamicFrontActivation.PresentBoundaries(system.Structure);

        /// <summary>
        /// Las líneas que un corte FRONTAL dibuja, por su ÍNDICE. Se identifican por la X con la que el propio
        /// layout las coloca —la misma fórmula del builder—, no por proximidad.
        /// </summary>
        private static IReadOnlyList<int> FrontalLines(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
        {
            var local = system.Composite.Of(side).Local;
            var layout = DynamicFrontGeometry.Compute(local.Structure, Catalog);
            var drawn = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, end, side)
                .Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(instance.Insertion.X, 3))
                .Distinct()
                .ToList();

            var result = new List<int>();
            for (var post = 0; post < layout.PostPositions.Count; post++)
            {
                if (drawn.Any(x => Math.Abs(x - layout.PostPositions[post]) < 1e-6))
                {
                    result.Add(post);
                }
            }

            Assert.Equal(drawn.Count, result.Count);   // ninguna X dibujada fuera de la retícula
            return result;
        }

        /// <summary>Las líneas que la PLANTA dibuja, por su índice transversal.</summary>
        private static IReadOnlyList<int> PlantaLines(PushBackSystem system)
        {
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var drawn = new PushBackSystemPlantaBuilder()
                .BuildPlan(system, Catalog)
                .Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(instance.Insertion.Y, 3))
                .Distinct()
                .ToList();

            var result = new List<int>();
            for (var post = 0; post < layout.PostPositions.Count; post++)
            {
                if (drawn.Any(y => Math.Abs(y - layout.PostPositions[post]) < 1e-6))
                {
                    result.Add(post);
                }
            }

            return result;
        }

        private static IReadOnlyList<string> ModuleIds(PushBackSystem system)
            => system.Structure.Modules.Select(module => module.ModuleId).ToList();

        // ---- 1 y 2: la línea que sólo posee el otro lado -----------------------------------------------------------

        /// <summary>
        /// EL CASO DEL DUEÑO. Ranura 1 en blanco en A, con almacenamiento en B: la línea exterior existe en el rack
        /// porque B la necesita, la planta la dibuja, el frontal de B la dibuja — y el de A NO.
        /// </summary>
        [Fact]
        public void FrontalA_DoesNotProjectHeaderOwnedOnlyByB()
        {
            var system = Rack(3, (0, PushBackSide.A));

            Assert.Equal(new[] { 0, 1, 2, 3 }, GlobalLines(system));
            Assert.Equal(new[] { 0, 1, 2, 3 }, PlantaLines(system));
            Assert.Equal(new[] { 0, 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
            Assert.Equal(new[] { 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
        }

        /// <summary>El caso simétrico, para que no quede una regla por lado.</summary>
        [Fact]
        public void FrontalB_DoesNotProjectHeaderOwnedOnlyByA()
        {
            var system = Rack(3, (0, PushBackSide.B));

            Assert.Equal(new[] { 0, 1, 2, 3 }, GlobalLines(system));
            Assert.Equal(new[] { 0, 1, 2, 3 }, PlantaLines(system));
            Assert.Equal(new[] { 0, 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Equal(new[] { 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }

        // ---- 3: la planta sigue siendo la estructura completa -------------------------------------------------------

        [Fact]
        public void Planta_StillProjectsHeaderNeededByEitherSide()
        {
            foreach (var blanked in new[]
            {
                Rack(3, (0, PushBackSide.A)),
                Rack(3, (2, PushBackSide.B)),
                Rack(4, (1, PushBackSide.A), (2, PushBackSide.A))
            })
            {
                Assert.Equal(GlobalLines(blanked), PlantaLines(blanked));
            }
        }

        // ---- 4: la física global no cambia ---------------------------------------------------------------------------

        /// <summary>
        /// Filtrar la PROYECCIÓN no toca el rack: la retícula, las líneas que existen, la longitud y los módulos de
        /// I-40 son los mismos que sin el blanco de un solo lado.
        /// </summary>
        [Fact]
        public void SingleSideBlank_DoesNotChangeGlobalStructure()
        {
            var baseline = Rack(3);
            foreach (var blanked in new[] { Rack(3, (0, PushBackSide.A)), Rack(3, (0, PushBackSide.B)) })
            {
                Assert.Equal(GlobalLines(baseline), GlobalLines(blanked));
                Assert.Equal(PlantaLines(baseline), PlantaLines(blanked));
                Assert.Equal(ModuleIds(baseline), ModuleIds(blanked));
                Assert.Equal(baseline.Structure.TotalLength, blanked.Structure.TotalLength, 6);
                Assert.Equal(baseline.Structure.Fronts.Count, blanked.Structure.Fronts.Count);
            }
        }

        // ---- 5: dos blancos seguidos en UN lado ----------------------------------------------------------------------

        /// <summary>
        /// Dos ranuras en blanco consecutivas SÓLO en A: el corte de A pierde la línea intermedia —no sostiene nada
        /// suyo— y conserva las que delimitan su grupo. El rack no pierde ninguna, porque B las necesita todas.
        /// </summary>
        [Fact]
        public void TwoBlankSlots_FilterOnlyUnusedLocalHeader()
        {
            var system = Rack(4, (1, PushBackSide.A), (2, PushBackSide.A));

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, GlobalLines(system));
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, PlantaLines(system));
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
            Assert.Equal(new[] { 0, 1, 3, 4 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
        }

        /// <summary>Un blanco INTERIOR aislado no quita ninguna línea: sus dos vecinas siguen sosteniendo ese lado.</summary>
        [Fact]
        public void InteriorSingleBlank_KeepsEveryLineOfItsOwnSide()
        {
            var system = Rack(3, (1, PushBackSide.A));

            Assert.Equal(new[] { 0, 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Equal(new[] { 0, 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }

        /// <summary>Con la ranura en blanco en los DOS lados, ningún corte se inventa la pertenencia.</summary>
        [Fact]
        public void BlankOnBothSides_NeitherFrontalInventsOwnership()
        {
            var system = Rack(3, (0, PushBackSide.A), (0, PushBackSide.B));

            Assert.Equal(new[] { 0, 1, 2, 3 }, GlobalLines(system));   // el hueco físico sigue existiendo
            Assert.Equal(new[] { 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Equal(new[] { 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
        }

        // ---- 6: el corte posterior sigue la misma regla ---------------------------------------------------------------

        [Fact]
        public void PosteriorFrontal_UsesTheSameSideOwnershipRule()
        {
            var system = Rack(3, (0, PushBackSide.A));

            Assert.Equal(
                FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A),
                FrontalLines(system, PushBackFrontalEnd.Posterior, PushBackSide.A));
            Assert.Equal(
                FrontalLines(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B),
                FrontalLines(system, PushBackFrontalEnd.Posterior, PushBackSide.B));

            Assert.Equal(new[] { 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.Posterior, PushBackSide.A));
            Assert.Equal(new[] { 0, 1, 2, 3 }, FrontalLines(system, PushBackFrontalEnd.Posterior, PushBackSide.B));
        }

        // ---- un rack de un solo sentido no cambia ---------------------------------------------------------------------

        /// <summary>
        /// Sin lado B no hay filtro: los dos bordes exteriores del rack siguen existiendo siempre, incluso con la
        /// primera ranura en blanco. Es la regla de I-33 y no se toca.
        /// </summary>
        [Fact]
        public void ASingleSidedRack_KeepsItsOuterEdges()
        {
            var state = new PushBackEditorState();
            state.LoadNew();
            state.Structure.SetFrontCount(3);
            Assert.True(state.SetActive(0, false));

            var system = new PushBackEditorDesignAssembler(Catalog).Build(state, Inputs()).System;
            Assert.NotNull(system);

            var plan = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var drawn = plan.Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(instance.Insertion.X, 3))
                .Distinct()
                .ToList();

            Assert.Equal(layout.PostPositions.Count, drawn.Count);
            Assert.Contains(drawn, x => Math.Abs(x - layout.PostPositions[0]) < 1e-6);
        }
    }
}
