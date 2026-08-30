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
    /// I-42 (ronda 8) — HIGIENE DE VISTA. Las vistas son CONSUMIDORAS del modelo fisico: no vuelven a decidir que
    /// run existe, que lado existe, que extremo es alto o bajo, ni que tope aplica.
    ///
    /// <para><b>V1.</b> La letra A/B de un corte representa el ALMACENAMIENTO de ese lado en lo que el corte
    /// muestra. Antes se rotulaba todo lado DECLARADO —una propiedad del rack entero—, asi que un corte cuya unica
    /// ranura tenia el lado en blanco salia igualmente rotulado.</para>
    ///
    /// <para><b>V2.</b> En una corrida, la cara de SALIDA proyecta el extremo ALTO —larguero y tope— y la de ENTRADA
    /// el bajo, y la regla la decide el extremo del run que cae en cada cara, no el lado ni el sentido.</para>
    ///
    /// <para><b>V3.</b> Las correspondencias por proximidad y los caminos de fallo quedan acotados y medidos.</para>
    /// </summary>
    public class PushBackViewHygieneTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(
            int slots,
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            PushBackRunDirection direction = PushBackRunDirection.AToB)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            state.SetDefaults(topology, direction);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return state;
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        private static IReadOnlyList<HeaderBlockInstance> All(HeaderRunPlan plan)
            => plan.Flatten().Instances.ToList();

        /// <summary>Las letras A/B de una vista, en orden.</summary>
        private static IReadOnlyList<string> Labels(IEnumerable<HeaderBlockInstance> instances)
            => instances
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .Select(instance => instance.Text ?? string.Empty)
                .Where(text => text == "A" || text == "B")
                .OrderBy(text => text, StringComparer.Ordinal)
                .ToList();

        private static IReadOnlyList<string> LateralLabels(PushBackSystem system, int postIndex)
            => Labels(All(new PushBackSystemLateralBuilder().Build(system, Catalog, postIndex)));

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => All(new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side));

        private static int Count(IEnumerable<HeaderBlockInstance> instances, HeaderBlockRole role)
            => instances.Count(instance => instance.Role == role);

        // ==================================================================== V1

        /// <summary>Con el lado A en blanco en la ranura que el corte muestra, la letra A no se dibuja.</summary>
        [Fact]
        public void LateralLabel_BlankA_DoesNotShowA()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(state);

            // El corte del ultimo poste muestra SOLO la ranura 1, donde A no almacena.
            Assert.Equal(new[] { "B" }, LateralLabels(system, 2));
        }

        [Fact]
        public void LateralLabel_BlankB_DoesNotShowB()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.B, 1, false);
            var system = Resolve(state);

            Assert.Equal(new[] { "A" }, LateralLabels(system, 2));
        }

        [Fact]
        public void LateralLabel_BothActive_ShowsApplicableSides()
        {
            var system = Resolve(State(2));

            Assert.Equal(new[] { "A", "B" }, LateralLabels(system, 0));
            Assert.Equal(new[] { "A", "B" }, LateralLabels(system, 2));
        }

        /// <summary>Y con los DOS lados en blanco ahi, ninguna letra: no hay almacenamiento que rotular.</summary>
        [Fact]
        public void LateralLabel_BothBlank_ShowsNoFunctionalSideLabel()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            state.SetSlotPresent(PushBackSide.B, 1, false);
            var system = Resolve(state);

            Assert.Empty(LateralLabels(system, 2));
        }

        /// <summary>
        /// La decision NO sale de la presencia global del compuesto: con los dos lados declarados —que es lo que
        /// esa presencia dice— el corte de una ranura en blanco sigue sin rotularla.
        /// </summary>
        [Fact]
        public void LateralLabel_DoesNotUseGlobalCompositePresence()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(state);

            Assert.True(system.Composite.SideA.IsPresent);   // el rack SI declara el lado A
            Assert.True(system.Composite.SideB.IsPresent);
            Assert.DoesNotContain("A", LateralLabels(system, 2));   // y aun asi no se rotula ahi
        }

        /// <summary>Corregir la letra no mueve NADA: la reticula fisica del corte es la misma pieza a pieza.</summary>
        [Fact]
        public void LateralLabel_BlankDoesNotChangeGeometry()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(state);

            var physical = All(new PushBackSystemLateralBuilder().Build(system, Catalog, 2))
                .Where(instance => instance.Role != HeaderBlockRole.Annotation)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Role}|{instance.PieceId}|{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();

            Assert.NotEmpty(physical);
            Assert.DoesNotContain(physical, value => value.Contains("Annotation", StringComparison.Ordinal));
        }

        /// <summary>Un lado que no almacena en NINGUNA ranura tampoco se rotula en planta.</summary>
        [Fact]
        public void PlantaLabel_ASideThatStoresNowhere_IsNotLabelled()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.B, 0, false);
            state.SetSlotPresent(PushBackSide.B, 1, false);
            var system = Resolve(state);

            // Sin almacenamiento en ninguna ranura, el lado B deja de existir funcionalmente y el rack ya no es
            // compuesto: no hay dos pasillos que distinguir, asi que la planta no rotula ninguno — y desde luego
            // no rotula una «B» que no representa nada.
            var labels = Labels(All(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog)));
            Assert.DoesNotContain("B", labels);
        }

        /// <summary>La autoridad de presencia funcional distingue los tres conceptos y no los confunde.</summary>
        [Fact]
        public void FunctionalSides_SeparatesDeclaredFromStored()
        {
            var state = State(2);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(state);

            Assert.True(PushBackFunctionalSides.HasStorageAt(system.Composite.SideA, 0));
            Assert.False(PushBackFunctionalSides.HasStorageAt(system.Composite.SideA, 1));
            Assert.True(PushBackFunctionalSides.HasStorageAt(system.Composite.SideB, 1));
            Assert.Equal(
                new[] { PushBackSide.B },
                PushBackFunctionalSides.In(system, slot => slot == 1));
        }

        // ==================================================================== V2

        /// <summary>Una corrida A→B: la cara de SALIDA proyecta el larguero ALTO.</summary>
        [Fact]
        public void FullSpanCorrida_AtoB_ExitFaceShowsHigh()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));

            Assert.All(PushBackRuns.Resolve(system).Runs, run =>
            {
                Assert.Equal(PushBackSide.A, run.LowSide);
                Assert.Equal(PushBackSide.B, run.HighSide);
            });

            var exit = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B);
            Assert.Equal(6, Count(exit, HeaderBlockRole.Beam));
            Assert.All(exit.Where(i => i.Role == HeaderBlockRole.Beam), beam =>
                Assert.Contains("REDONDO", beam.PieceId, StringComparison.Ordinal));
        }

        [Fact]
        public void FullSpanCorrida_AtoB_ExitFaceShowsRearTope()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));

            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B), HeaderBlockRole.Tope));
        }

        [Fact]
        public void FullSpanCorrida_BtoA_ExitFaceShowsHigh()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.BToA));

            Assert.All(PushBackRuns.Resolve(system).Runs, run =>
            {
                Assert.Equal(PushBackSide.B, run.LowSide);
                Assert.Equal(PushBackSide.A, run.HighSide);
            });

            var exit = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A);
            Assert.Equal(6, Count(exit, HeaderBlockRole.Beam));
        }

        [Fact]
        public void FullSpanCorrida_BtoA_ExitFaceShowsRearTope()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.BToA));

            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A), HeaderBlockRole.Tope));
        }

        /// <summary>Y la cara de ENTRADA proyecta el bajo, nunca el alto: es la otra mitad del contrato.</summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB, PushBackSide.A)]
        [InlineData(PushBackRunDirection.BToA, PushBackSide.B)]
        public void CorridaEntryFaceShowsLowNotHigh(PushBackRunDirection direction, PushBackSide lowSide)
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, direction));
            var entry = Cut(system, PushBackFrontalEnd.EntradaSalida, lowSide);

            Assert.Equal(6, Count(entry, HeaderBlockRole.Beam));
            Assert.All(entry.Where(i => i.Role == HeaderBlockRole.Beam), beam =>
                Assert.Contains("IN_OUT", beam.PieceId, StringComparison.Ordinal));
            Assert.Equal(0, Count(entry, HeaderBlockRole.Tope));
        }

        /// <summary>
        /// El lado que NO posee un extremo del run no proyecta nada de ese run: ni el bajo del lado alto, ni el alto
        /// del lado bajo. Es lo que impide tratar la cara de salida como «otra entrada».
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void Corrida_TheOppositeCutsCarryNoBeams(PushBackRunDirection direction)
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, direction));
            var lowSide = direction == PushBackRunDirection.AToB ? PushBackSide.A : PushBackSide.B;
            var highSide = direction == PushBackRunDirection.AToB ? PushBackSide.B : PushBackSide.A;

            Assert.Equal(0, Count(Cut(system, PushBackFrontalEnd.Posterior, lowSide), HeaderBlockRole.Beam));
            Assert.Equal(0, Count(Cut(system, PushBackFrontalEnd.EntradaSalida, highSide), HeaderBlockRole.Beam));
        }

        /// <summary>
        /// Una corrida que NO alcanza el exterior opuesto no inventa nada ahi: su extremo alto sigue siendo el de su
        /// lado alto, con las mismas piezas, y ninguna aparece en la cara contraria.
        /// </summary>
        [Fact]
        public void ShortCorrida_DoesNotInventHighAtOppositeExterior()
        {
            var state = State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetStructureOverride(PushBackSide.B, state.SideB.Structure.Count + 3);
            var system = Resolve(state);

            Assert.Equal(0, Count(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B), HeaderBlockRole.Beam));
            Assert.Equal(0, Count(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A), HeaderBlockRole.Beam));
            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B), HeaderBlockRole.Beam));
        }

        /// <summary>Con el tope en «Ninguno» la cara de salida conserva el ALTO y pierde SOLO el tope.</summary>
        [Fact]
        public void RearTopeNone_RemovesOnlyTopeFromExitFrontal()
        {
            var state = State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.Of(PushBackSide.B).RearTopePieceId = PushBackDefaults.NonePieceId;
            var system = Resolve(state);

            var exit = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B);
            Assert.Equal(6, Count(exit, HeaderBlockRole.Beam));
            Assert.Equal(0, Count(exit, HeaderBlockRole.Tope));
        }

        /// <summary>
        /// La frontal de salida NO recalcula la mano del alto: proyecta la que la autoridad fisica ya fijo, la misma
        /// que un larguero intermedio tendria en esa frontera (rondas 5B/5D).
        /// </summary>
        [Fact]
        public void ExitFrontal_UsesExistingHighPhysicalHand()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));
            var exit = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B)
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .ToList();

            Assert.NotEmpty(exit);
            Assert.All(exit, beam => Assert.Equal(
                PushBackRearTopeBuilder.ElevationMirrored, beam.MirroredX));
        }

        /// <summary>Y el tope conserva su ancla: su espejo es el CONTRARIO al de su larguero, en toda vista.</summary>
        [Fact]
        public void ExitFrontal_UsesExistingTopeAnchor()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));
            var exit = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B);
            var topes = exit.Where(instance => instance.Role == HeaderBlockRole.Tope).ToList();

            Assert.NotEmpty(topes);
            Assert.All(topes, tope => Assert.Equal(
                PushBackRearTopeBuilder.Mirrored("FRONTAL", beamMirroredX: false), tope.MirroredX));
        }

        /// <summary>
        /// ENCONTRADAS no son una corrida: son DOS runs fisicos, cada uno con su propio extremo alto, y cada lado
        /// proyecta el suyo. Ninguna regla de corrida continua se les aplica.
        /// </summary>
        [Fact]
        public void Encountered_IsNotTreatedAsContinuousRun()
        {
            var system = Resolve(State(2, PushBackCellTopology.Encontradas));
            var runs = PushBackRuns.Resolve(system).Runs;

            Assert.All(runs, run => Assert.Equal(run.LowSide, run.HighSide));
            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A), HeaderBlockRole.Beam));
            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B), HeaderBlockRole.Beam));
            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A), HeaderBlockRole.Beam));
            Assert.Equal(6, Count(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B), HeaderBlockRole.Beam));
        }

        /// <summary>
        /// Y sus DOS altos no se colapsan por proyeccion: en planta siguen siendo dos piezas, aunque una topologia
        /// simetrica las lleve a posiciones espejadas.
        /// </summary>
        [Fact]
        public void EncounteredDistinctHighs_AreNotProjectionDeduped()
        {
            var system = Resolve(State(2, PushBackCellTopology.Encontradas));
            var planta = All(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog));

            var topes = planta.Where(instance => instance.Role == HeaderBlockRole.Tope).ToList();
            var corrida = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));
            var corridaTopes = All(new PushBackSystemPlantaBuilder().BuildPlan(corrida, Catalog))
                .Count(instance => instance.Role == HeaderBlockRole.Tope);

            // Dos camas por celda dan el DOBLE de topes que una sola corrida sobre el mismo rack.
            Assert.Equal(corridaTopes * 2, topes.Count);
        }

        // ==================================================================== V3

        /// <summary>
        /// El agrupamiento de las camas en las vistas usa IDENTIDAD SEMANTICA —sistema de origen, frente local y
        /// reflexion—, nunca la posicion proyectada. Con dos lados simetricos, las camas de A y las de B caen en
        /// grupos distintos aunque se superpongan al proyectarse.
        /// </summary>
        [Fact]
        public void ViewProjection_UsesSemanticRunIdentity()
        {
            var system = Resolve(State(2, PushBackCellTopology.Encontradas));
            var runs = PushBackRuns.Resolve(system).Runs;

            var groups = runs
                .GroupBy(run => (run.Source, run.SourceFrontIndex, run.Reflected))
                .ToList();

            Assert.Equal(4, groups.Count);   // dos ranuras x dos lados: ninguno se funde con otro
            Assert.All(groups, group => Assert.Equal(3, group.Select(run => run.SourceLevel).Distinct().Count()));
        }

        /// <summary>
        /// El FAIL-OPEN del filtro por celda no se ejerce en ningun escenario de la matriz: todo larguero del corte
        /// bajo se atribuye a su celda. Si algun dia dejara de ser cierto, esta prueba lo dice antes de que aparezca
        /// una pieza sin dueño en el dibujo.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.BToA)]
        public void ProjectionFailOpen_DoesNotInventCompositePiece(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var state = State(3, topology, direction);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(state);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var view = system.Composite.Of(side);
                if (view?.Local?.Structure == null)
                {
                    continue;
                }

                var local = view.Local;
                var context = PushBackElevations.Context(local, Catalog);
                var instances = new PushBackSystemFrontalBuilder()
                    .BuildPlan(local, Catalog, PushBackFrontalEnd.EntradaSalida)
                    .Flatten().Instances;

                Assert.Equal(0, PushBackSystemFrontalBuilder.UnidentifiedEndBeams(
                    local.Structure, Catalog, context, instances));
            }
        }

        /// <summary>
        /// PLANTA y LATERAL coinciden en la MANO del larguero alto: las dos leen la misma autoridad fisica, asi que
        /// ninguna puede elegir un poste cercano en lugar de la pieza que le corresponde.
        /// </summary>
        [Fact]
        public void PlantaAndLateral_AgreeOnHighPhysicalHand()
        {
            var system = Resolve(State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));

            var plantaTopes = All(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog))
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .ToList();
            var lateralTopes = All(new PushBackSystemLateralBuilder().Build(system, Catalog))
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .ToList();

            Assert.NotEmpty(plantaTopes);
            Assert.NotEmpty(lateralTopes);

            // La misma pieza de catalogo en las dos vistas: ninguna resuelve una distinta por cercania.
            Assert.Equal(
                plantaTopes.Select(instance => instance.PieceId).Distinct().OrderBy(value => value),
                lateralTopes.Select(instance => instance.PieceId).Distinct().OrderBy(value => value));
        }

        /// <summary>
        /// Y coinciden en el CONTACTO del tope: el ancla de un tope sin espejo y la de uno espejado estan a la misma
        /// distancia del eje del poste, en lados opuestos (ronda 5D). El contacto fisico no se mueve al espejar.
        /// </summary>
        [Fact]
        public void PlantaAndLateral_AgreeOnTopeContact()
        {
            const double columnX = 100.0;
            const double anchorLocalX = 0.875;

            var mirrored = PushBackRearTopeBuilder.AnchorX(columnX, anchorLocalX, topeMirrored: true);
            var plain = PushBackRearTopeBuilder.AnchorX(columnX, anchorLocalX, topeMirrored: false);

            Assert.Equal(columnX + anchorLocalX, mirrored, 9);
            Assert.Equal(columnX - anchorLocalX, plain, 9);
            Assert.Equal(2.0 * anchorLocalX, Math.Abs(mirrored - plain), 9);
        }

        /// <summary>
        /// Un blanco no desplaza los INDICES de la proyeccion: la ranura sigue en su sitio y las piezas de las demas
        /// no se corren a otra columna. Es la regla de la ronda 2, leida desde las vistas.
        /// </summary>
        [Fact]
        public void Blank_DoesNotShiftPhysicalProjectionIndices()
        {
            var full = Resolve(State(3));
            var blanked = State(3);
            blanked.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(blanked);

            // La RETICULA TRANSVERSAL —los indices de ranura y las columnas donde se proyectan— es la misma. Lo que
            // el blanco si acorta, legitimamente, es la COBERTURA longitudinal de esa linea (ronda 6D).
            Assert.Equal(
                full.Structure.Fronts.Select(front => front.Index),
                system.Structure.Fronts.Select(front => front.Index));

            var before = DynamicFrontGeometry.Compute(full.Structure, Catalog);
            var after = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            Assert.Equal(before.PostPositions, after.PostPositions);
            Assert.Equal(before.TroquelPositions, after.TroquelPositions);
        }

        /// <summary>
        /// PREVIEW y dibujo final son EL MISMO plan: la ventana no construye una segunda vista. Se comprueba sobre
        /// los roles y posiciones que la ronda 8 cubre.
        /// </summary>
        [Fact]
        public void PreviewAndFinalDraw_AgreeOnRunEndRoles()
        {
            var state = State(2, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            var built = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            var system = built.System;

            // El PREVIEW del editor y el dibujo final salen del MISMO ensamblador de vistas, con el mismo diseño.
            var computation = new PushBackEditorDesignAssembler(Catalog).BuildFrom(built.Design, PushBackSide.A);
            Assert.True(computation.IsValid);

            var previewExit = computation.FrontalPosterior.Flatten().Instances
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Role}|{instance.PieceId}|{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
            var finalExit = Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Role}|{instance.PieceId}|{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(finalExit, previewExit);
        }
    }
}
