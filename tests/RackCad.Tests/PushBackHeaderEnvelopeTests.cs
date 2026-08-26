using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42, ERROR 4 — la envolvente longitudinal se resuelve POR LÍNEA FÍSICA TRANSVERSAL.
    ///
    /// <para>
    /// La regla del dueño: <c>RequiredHeaderEnvelope(línea) = la máxima envolvente que exigen los frentes
    /// FÍSICAMENTE ADYACENTES a esa línea</c>. Ni el máximo del rack, ni un frente arbitrario, ni un frente remoto
    /// que la línea no sostiene. Una línea INTERMEDIA sí se extiende aunque uno de sus dos frentes sea corto,
    /// porque sostiene también al otro.
    /// </para>
    /// <para>
    /// El rack de un solo sentido ya cumplía la regla y estas pruebas la fijan para que no se pierda. El COMPUESTO
    /// no: toda ranura presente en los dos lados reclamaba la profundidad ENTERA, de modo que una ranura corta se
    /// extendía hasta donde llegaba la más larga del rack. Ahora declara sus TRAMOS —lo que demanda cada lado— y la
    /// cobertura de la línea es la unión de los tramos de sus frentes adyacentes.
    /// </para>
    /// </summary>
    public class PushBackHeaderEnvelopeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ===================== rack de un solo sentido =======================================================

        private static PushBackSystem Single(params int[] deeps)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = deeps.Min(),
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            foreach (var deep in deeps)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = 1, LoadLevels = 2, PalletsDeep = deep, DepthStartPosition = 1
                });
            }

            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>La X final de una posición de fondo, en coordenadas del rack.</summary>
        private static double EndXOf(DynamicRackSystem structure, int position)
            => structure.Modules.First(module => module.Index + 1 == position).EndX;

        /// <summary>Las X de los postes que la planta dibuja en una línea transversal.</summary>
        private static IReadOnlyList<double> PostsOnLine(PushBackSystem system, int postIndex)
        {
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var y = layout.PostPositions[postIndex];
            return new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post && Math.Abs(i.Insertion.Y - y) < 1e-4)
                .Select(i => Math.Round(i.Insertion.X, 3))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        /// <summary>
        /// El ejemplo literal del dueño. La línea EXTERIOR de un frente corto termina donde termina ESE frente; una
        /// INTERMEDIA llega hasta el más profundo de los dos que separa.
        /// </summary>
        [Fact]
        public void EachLine_ReachesOnlyWhatItsAdjacentFrontsDemand()
        {
            var system = Single(5, 8, 6, 9);
            var structure = system.Structure;

            var expected = new[]
            {
                EndXOf(structure, 5),   // exterior de F0 (5)
                EndXOf(structure, 8),   // F0(5) | F1(8)
                EndXOf(structure, 8),   // F1(8) | F2(6)
                EndXOf(structure, 9),   // F2(6) | F3(9)
                EndXOf(structure, 9)    // exterior de F3 (9)
            };

            for (var line = 0; line < expected.Length; line++)
            {
                var posts = PostsOnLine(system, line);
                Assert.NotEmpty(posts);
                Assert.Equal(expected[line], posts.Max(), 3);
            }
        }

        /// <summary>
        /// Y un frente REMOTO no alarga una línea: con 5, 5 y 9, las dos primeras líneas terminan en 5 aunque el
        /// rack llegue mucho más lejos.
        /// </summary>
        [Fact]
        public void ARemoteFront_NeverExtendsALine()
        {
            var system = Single(5, 5, 9);
            var structure = system.Structure;

            Assert.Equal(EndXOf(structure, 5), PostsOnLine(system, 0).Max(), 3);
            Assert.Equal(EndXOf(structure, 5), PostsOnLine(system, 1).Max(), 3);
            Assert.Equal(EndXOf(structure, 9), PostsOnLine(system, 2).Max(), 3);
            Assert.Equal(EndXOf(structure, 9), PostsOnLine(system, 3).Max(), 3);
        }

        // ===================== rack compuesto ================================================================

        private static PushBackCompositeEditorState State(
            PushBackCellTopology topology, int[] deepsA, int[] deepsB, double gap = 0.0)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(deepsA.Length);
            state.SetGap(gap);
            state.SetDefaults(topology, PushBackRunDirection.AToB);
            for (var slot = 0; slot < deepsA.Length; slot++)
            {
                state.SideA.Structure.Fronts[slot].PalletsDeep = deepsA[slot];
                state.SideB.Structure.Fronts[slot].PalletsDeep = deepsB[slot];
            }

            return state;
        }

        private static PushBackSystem Composite(PushBackCompositeEditorState state)
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System;
        }

        /// <summary>
        /// EL DEFECTO. Una ranura presente en los dos lados con demanda CORTA en A no puede llevar estructura donde
        /// ninguna de sus dos camas llega. Se mide en la línea EXTERIOR de esa ranura, que es la que solo ella
        /// sostiene.
        /// </summary>
        [Fact]
        public void ACompositeSlot_OnlyCarriesStructureWhereOneOfItsBedsReaches()
        {
            var system = Composite(State(
                PushBackCellTopology.Encontradas, new[] { 5, 8, 6 }, new[] { 8, 8, 8 }));
            var structure = system.Structure;

            // El lado A del rack ocupa 8 posiciones (la ranura más profunda); la ranura 0 solo pide 5.
            var shortEnd = EndXOf(structure, 5);
            var sideAEnd = EndXOf(structure, 8);
            Assert.True(sideAEnd > shortEnd);

            var exterior = PostsOnLine(system, 0);
            // Nada entre el final de SU demanda y el final de la mitad de A: esas eran las cabeceras de más.
            Assert.DoesNotContain(exterior, x => x > shortEnd + 1e-6 && x < sideAEnd - 1e-6);
            // Y sí lo que su lado B demanda, al otro extremo.
            Assert.Equal(structure.TotalLength, exterior.Max(), 3);

            // La línea INTERMEDIA entre la ranura corta y la profunda SÍ se extiende: sostiene a las dos.
            Assert.Contains(PostsOnLine(system, 1), x => Math.Abs(x - sideAEnd) < 1e-6);
        }

        /// <summary>
        /// Cuando los dos lados llegan a la interfaz la cobertura es CONTINUA y nada cambia: es el caso corriente y
        /// es lo que sostiene el separador central.
        /// </summary>
        [Fact]
        public void WhenBothSidesReachTheInterface_TheCoverageStaysContinuous()
        {
            var system = Composite(State(PushBackCellTopology.Encontradas, new[] { 6, 6 }, new[] { 6, 6 }));

            Assert.All(system.Structure.Fronts, front => Assert.Empty(front.DepthSegments));
            for (var line = 0; line <= system.Structure.Fronts.Count; line++)
            {
                var coverage = DynamicDepthGeometry.CoverageAtPost(system.Structure, line);
                Assert.Single(coverage.Segments);
                Assert.Equal(1, coverage.StartPosition);
                Assert.Equal(system.Structure.Modules.Count, coverage.EndPosition);
            }
        }

        /// <summary>
        /// Una ranura con alguna celda CORRIDA conserva la profundidad entera: su cama atraviesa la interfaz y
        /// necesita apoyos en todo el recorrido. Recortarla ahí dejaría el riel en el aire.
        /// </summary>
        [Fact]
        public void ACorridaSlot_KeepsTheWholeDepth()
        {
            var system = Composite(State(
                PushBackCellTopology.Corrida, new[] { 5, 8 }, new[] { 8, 8 }));

            Assert.All(system.Structure.Fronts, front => Assert.Empty(front.DepthSegments));
            var exterior = PostsOnLine(system, 0);
            Assert.Equal(0.0, exterior.Min(), 3);
            Assert.Equal(system.Structure.TotalLength, exterior.Max(), 3);
            Assert.Contains(exterior, x => Math.Abs(x - EndXOf(system.Structure, 8)) < 1e-6);
        }

        /// <summary>
        /// El BOM cuenta lo que la planta dibuja: si una línea deja de llevar cabeceras, dejan de cotizarse. Se
        /// compara el rack corto contra el mismo rack con la ranura a fondo completo.
        /// </summary>
        [Fact]
        public void TheBom_DropsTheHeadersThatNoLineCarriesAnyMore()
        {
            var shortSlot = Composite(State(
                PushBackCellTopology.Encontradas, new[] { 5, 8, 6 }, new[] { 8, 8, 8 }));
            var fullSlot = Composite(State(
                PushBackCellTopology.Encontradas, new[] { 8, 8, 8 }, new[] { 8, 8, 8 }));

            int Posts(PushBackSystem system) => new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Count(i => i.Role == HeaderBlockRole.Post);
            double Headers(PushBackSystem system) => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(c => string.Equals(c.Category, BomBuilder.Cabecera, StringComparison.Ordinal))
                .Sum(c => c.Quantity);

            Assert.True(Posts(shortSlot) < Posts(fullSlot), "la ranura corta tiene que dibujar menos postes");
            Assert.True(Headers(shortSlot) < Headers(fullSlot), "y cotizarlos de menos");
        }

        // ===================== I-40: la identidad por línea sobrevive =========================================

        /// <summary>
        /// SECCIÓN K del dueño. Una envolvente nueva NO justifica reemplazar la identidad de una línea: los
        /// <c>ModuleId</c> de I-40 siguen ahí y siguen apuntando a los mismos módulos después de recomponer con
        /// fondos distintos.
        /// </summary>
        [Fact]
        public void TheLineIdentity_SurvivesAChangeOfEnvelope()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 3, slotsB: 3, deepA: 8, deepB: 8, levelsA: 2, levelsB: 2);
            var before = new PushBackResolver(Catalog).Resolve(design).Structure;
            var ids = before.Modules.Select(module => module.ModuleId).ToList();
            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());

            // Se acorta el lado A de la ranura 0: cambia la envolvente de su línea exterior, no la identidad.
            design.Structure.Fronts[0].PalletsDeep = 5;
            design.Fronts[0].DefaultPalletsDeep = 5;
            var after = new PushBackResolver(Catalog).Resolve(design).Structure;

            Assert.Equal(ids, after.Modules.Select(module => module.ModuleId).ToList());
            Assert.Equal(
                before.Modules.Select(module => module.Kind).ToList(),
                after.Modules.Select(module => module.Kind).ToList());
            Assert.NotEmpty(after.Fronts[0].DepthSegments);
            Assert.Empty(after.Fronts[1].DepthSegments);
        }

        /// <summary>
        /// Y un override de I-40 puesto sobre una línea sigue aplicándose después del recorte: se resuelve POR
        /// LÍNEA antes de materializar, no por una envolvente global.
        /// </summary>
        [Fact]
        public void AHeaderLineOverride_StillAppliesAfterTheEnvelopeShrinks()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 3, slotsB: 3, deepA: 8, deepB: 8, levelsA: 2, levelsB: 2);
            var reference = new PushBackResolver(Catalog).Resolve(design).Structure;
            var module = reference.Modules.First(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            var custom = new RackFrameConfiguration
            {
                PostPeralte = module.AssociatedFrameConfiguration.PostPeralte + 1.0
            };

            design.Structure.HeaderLineOverrides.Add(new DynamicHeaderLineOverride
            {
                PostIndex = 0,
                ModuleId = module.ModuleId,
                Header = custom
            });
            design.Structure.Fronts[0].PalletsDeep = 5;
            design.Fronts[0].DefaultPalletsDeep = 5;

            var after = new PushBackResolver(Catalog).Resolve(design).Structure;
            var target = after.Modules.First(m => string.Equals(m.ModuleId, module.ModuleId, StringComparison.Ordinal));
            var applied = DynamicFrontGeometry.HeaderConfigurationAtPost(after, target, Catalog, postIndex: 0);

            Assert.NotNull(applied);
            Assert.Equal(custom.PostPeralte, applied.PostPeralte, 6);
        }
    }
}
