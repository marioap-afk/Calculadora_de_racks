using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda Owner) — MULTIFRENTE y MULTINIVEL, con aserciones POR FRENTE.
    ///
    /// <para>
    /// El dueño valido en AutoCAD que «el primer frente se ve bien y a partir del segundo solo quedan las cabeceras».
    /// Ninguna prueba lo detecto porque todas median el rack ENTERO: <c>Assert.NotEmpty(vigas)</c> pasa con un solo
    /// frente. Aqui nada se mide en agregado. Cada frente y cada nivel se comprueban por separado, y una prueba que
    /// solo pudiera satisfacerse con F1 falla.
    /// </para>
    /// </summary>
    public class PushBackCompositeMultiFrontTests
    {
        private const int Fronts = 4;
        private const int Levels = 3;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        /// <summary>
        /// El fixture del dueño, por el CAMINO REAL del editor: 4 frentes y 3 niveles en cada uno, en los dos lados.
        /// El conteo de frentes se fija UNA vez, como en la ventana, porque la retícula es del rack.
        /// </summary>
        private static PushBackCompositeEditorState State(
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            PushBackRunDirection direction = PushBackRunDirection.AToB)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();   // como la ventana: los dos lados nacen con los defaults del producto
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(Fronts);
            state.SetDefaults(topology, direction);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var front = 0; front < matrix.Count; front++)
                {
                    state.Of(side).AdjustLevels(front, Levels - matrix.Fronts[front].LoadLevels);
                }
            }

            return state;
        }

        private static PushBackCompositeComputation Build(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);

        // ================= C/D: la cadena conserva CADA frente y CADA nivel ======================================

        /// <summary>
        /// La cadena completa —matriz, estado, ensamblador, diseño, configuracion por lado, resolver y camas— conserva
        /// los CUATRO frentes con sus TRES niveles. Se comprueba eslabon por eslabon: si alguno colapsara al primer
        /// frente, esta prueba dice exactamente cual.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA)]
        [InlineData(PushBackCellTopology.SoloB)]
        [InlineData(PushBackCellTopology.Encontradas)]
        [InlineData(PushBackCellTopology.Corrida)]
        public void EveryFrontKeepsItsLevels_AllTheWayToTheBeds(PushBackCellTopology topology)
        {
            var state = State(topology);

            // 1) La matriz de los DOS lados.
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                Assert.Equal(Fronts, state.Of(side).Structure.Count);
                for (var front = 0; front < Fronts; front++)
                {
                    Assert.Equal(Levels, state.Of(side).Structure.Fronts[front].LoadLevels);
                }
            }

            var computation = Build(state);
            Assert.True(computation.IsValid);
            var design = computation.Design;

            // 2) El DISEÑO ensamblado, frente por frente.
            Assert.Equal(Fronts, design.Structure.Fronts.Count);
            Assert.Equal(Fronts, design.SideB.Fronts.Count);
            for (var front = 0; front < Fronts; front++)
            {
                Assert.Equal(Levels, design.Structure.Fronts[front].LoadLevels);
                Assert.NotNull(design.SideB.Fronts[front]);
                Assert.Equal(Levels, design.SideB.Fronts[front].LoadLevels);
            }

            // 3) La configuracion por lado.
            var sideA = PushBackSideConfiguration.ForA(design);
            var sideB = PushBackSideConfiguration.ForB(design);
            for (var front = 0; front < Fronts; front++)
            {
                Assert.Equal(Levels, sideA.Levels(front));
                Assert.Equal(Levels, sideB.Levels(front));
            }

            // 4) Las CELDAS compuestas: exactamente 4 x 3, ni una menos.
            var system = computation.System;
            Assert.Equal(Fronts, system.Structure.Fronts.Count);
            for (var front = 0; front < Fronts; front++)
            {
                for (var level = 1; level <= Levels; level++)
                {
                    var cell = system.Composite.Cell(front, level);
                    Assert.True(cell != null, "falta la celda F" + front + "/L" + level);
                    Assert.Equal(topology, cell.Topology);
                    Assert.Equal(
                        topology == PushBackCellTopology.Encontradas ? 2 : 1,
                        cell.Beds.Count);
                    Assert.All(cell.Beds, bed => Assert.True(bed.IsValid, "F" + front + "/L" + level + ": " + bed.DisabledReason));
                }
            }

            // 5) Las CAMAS fisicas, agrupadas por frente: cada frente aporta lo mismo, y ninguno cero.
            var runs = PushBackRuns.Resolve(system);
            var expectedPerFront = topology == PushBackCellTopology.Encontradas ? Levels * 2 : Levels;
            for (var front = 0; front < Fronts; front++)
            {
                var ofFront = runs.Runs.Where(run => run.Slot == front).ToList();
                Assert.Equal(expectedPerFront, ofFront.Count);
                Assert.Equal(
                    Enumerable.Range(1, Levels).ToList(),
                    ofFront.Select(run => run.Level).Distinct().OrderBy(level => level).ToList());
                Assert.All(ofFront, run => Assert.Equal(front, run.SourceFrontIndex));
                Assert.All(ofFront, run => Assert.Equal(run.Level, run.SourceLevel));
            }

            Assert.Equal(Fronts * expectedPerFront, runs.Runs.Count);
        }

        // ================= E: la PLANTA proyecta TODOS los frentes ===============================================

        /// <summary>
        /// La Y de cada frente en planta: la linea de postes de su frontera. Sirve para exigir piezas EN CADA FRENTE
        /// en vez de contar el total, que es lo que dejaba pasar el defecto.
        /// </summary>
        private static IReadOnlyList<double> FrontBands(IReadOnlyList<HeaderBlockInstance> planta)
            => planta
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(instance.Insertion.Y, 3))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        [Theory]
        [InlineData(PushBackCellTopology.SoloA)]
        [InlineData(PushBackCellTopology.SoloB)]
        [InlineData(PushBackCellTopology.Encontradas)]
        [InlineData(PushBackCellTopology.Corrida)]
        public void ThePlanta_DrawsIntermediateBeams_InEveryFront(PushBackCellTopology topology)
        {
            var system = Build(State(topology)).System;
            var planta = new PushBackSystemPlantaBuilder().Build(system, Catalog);
            var intermediates = planta.Where(PushBackPlanComposer.IsDynamicIntermediate).ToList();

            Assert.NotEmpty(intermediates);

            // Las lineas de postes delimitan los frentes: con 4 frentes hay 5.
            var bands = FrontBands(planta);
            Assert.Equal(Fronts + 1, bands.Count);

            // Y CADA frente —cada banda entre dos lineas— tiene intermedios propios. Si solo los tuviera el primero,
            // esta prueba falla, que es exactamente lo que la anterior no hacia.
            for (var front = 0; front < Fronts; front++)
            {
                var low = bands[front];
                var high = bands[front + 1];
                var inBand = intermediates
                    .Where(beam => beam.Insertion.Y >= low - 1e-6 && beam.Insertion.Y <= high + 1e-6)
                    .ToList();
                Assert.True(
                    inBand.Count > 0,
                    "el frente " + (front + 1) + " no tiene ningun larguero intermedio en planta");
            }
        }

        /// <summary>
        /// Dos frentes FISICAMENTE distintos nunca se deduplican entre si. La planta colapsa NIVELES, no FRENTES: si
        /// la clave de deduplicacion perdiera la identidad transversal, el rack saldria con los intermedios de uno
        /// solo.
        /// </summary>
        [Fact]
        public void ThePlantaIntermediates_AreNeverDedupedAcrossFronts()
        {
            var system = Build(State(PushBackCellTopology.Encontradas)).System;
            var intermediates = new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(PushBackPlanComposer.IsDynamicIntermediate)
                .ToList();

            var ys = intermediates.Select(beam => Math.Round(beam.Insertion.Y, 3)).Distinct().ToList();
            Assert.Equal(Fronts, ys.Count);

            // Y cada Y aporta el MISMO numero de apoyos: ningun frente queda a medias.
            var perY = ys.Select(y => intermediates.Count(beam => Math.Abs(beam.Insertion.Y - y) < 1e-6)).ToList();
            Assert.Single(perY.Distinct());
        }

        /// <summary>
        /// N — la planta proyecta las piezas de cama de CADA ranura, y en su banda: larguero de entrada/salida en el
        /// pasillo de cada lado presente, y larguero posterior de troquel redondo donde hay extremo alto. Se afirma
        /// por SLOT y con coordenadas, no en agregado.
        /// </summary>
        [Fact]
        public void ThePlanta_ProjectsEachSlotPieces_InItsOwnBand()
        {
            var system = Build(State(PushBackCellTopology.Encontradas)).System;
            var planta = new PushBackSystemPlantaBuilder().Build(system, Catalog);
            var bands = FrontBands(planta);
            var total = system.Structure.TotalLength;

            var inOutId = DynamicRackDefaults.InOutBeamCatalogId;
            var highId = PushBackDefaults.HighEndBeamCatalogId;

            for (var slot = 0; slot < Fronts; slot++)
            {
                var low = bands[slot];
                var high = bands[slot + 1];
                bool InBand(HeaderBlockInstance instance)
                    => instance.Insertion.Y >= low - 1e-6 && instance.Insertion.Y <= high + 1e-6;

                var inOut = planta
                    .Where(instance => string.Equals(instance.PieceId, inOutId, StringComparison.OrdinalIgnoreCase))
                    .Where(InBand)
                    .ToList();
                var redondo = planta
                    .Where(instance => string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase))
                    .Where(InBand)
                    .ToList();

                // Camas encontradas: un pasillo en cada extremo del rack, y dos extremos altos que se miran en medio.
                Assert.True(inOut.Any(beam => beam.Insertion.X < total / 2.0), "ranura " + slot + " sin larguero en el pasillo de A");
                Assert.True(inOut.Any(beam => beam.Insertion.X > total / 2.0), "ranura " + slot + " sin larguero en el pasillo de B");
                Assert.True(redondo.Count >= 2, "ranura " + slot + " sin sus dos largueros posteriores");
            }
        }

        // ================= F: los INTERMEDIOS del lateral, por frente y por cama =================================

        /// <summary>
        /// Cuantos apoyos internos atraviesa FISICAMENTE una cama: las lineas de modulo estrictamente entre su
        /// arranque y su larguero posterior. Es el numero que el plan tiene que materializar, ni uno mas ni uno menos.
        /// </summary>
        private static int InternalSupports(PushBackSystem source, DynamicRackFront front, int levelNumber)
        {
            var structure = source.Structure;
            var rearX = PushBackCellDepth.RearX(source, front, levelNumber);
            // DISTINTAS: con hueco 0 la linea terminal de un lado y la inicial del otro caen en la misma X, y eso es
            // UN apoyo fisico, no dos. Contar modulos en vez de lineas exigiria un larguero duplicado.
            return structure.Modules
                .Where(module => module.StartX > front.StartX + 1e-6 && module.StartX < rearX - 1e-6)
                .Select(module => Math.Round(module.StartX, 6))
                .Distinct()
                .Count();
        }

        [Theory]
        [InlineData(PushBackCellTopology.SoloA)]
        [InlineData(PushBackCellTopology.Encontradas)]
        [InlineData(PushBackCellTopology.Corrida)]
        public void EveryBed_MaterializesExactlyItsInternalSupports(PushBackCellTopology topology)
        {
            var system = Build(State(topology)).System;
            var runs = PushBackRuns.Resolve(system);
            var builder = new PushBackIntermediateBeamLateralBuilder();

            Assert.Equal(Fronts * (topology == PushBackCellTopology.Encontradas ? Levels * 2 : Levels), runs.Runs.Count);

            foreach (var run in runs.Runs)
            {
                var front = run.Front();
                Assert.NotNull(front);

                var expected = InternalSupports(run.Source, front, run.SourceLevel);
                var built = builder.BuildFor(
                    run.Source, Catalog, front, new[] { run.SourceLevel });

                Assert.True(
                    expected == built.Count,
                    "F" + run.Slot + "/L" + run.Level + " (" + topology + "): esperados " + expected
                    + " intermedios, materializados " + built.Count);
            }
        }

        // ================= La CAUSA RAIZ del «solo quedan las cabeceras» ========================================

        /// <summary>
        /// DEFECTO ENCONTRADO en esta ronda. La topologia POR DEFECTO de un rack nuevo es «Solo A» —es un rack de un
        /// sentido— y declarar el lado B no la revisaba. Resultado: B aportaba estructura y ni una sola cama, y el
        /// dueño veia justo eso: cabeceras y postes en la otra mitad, sin niveles ni largueros.
        /// </summary>
        [Fact]
        public void DeclaringSideB_MakesTheDefaultTopologyTwoSided()
        {
            var state = new PushBackCompositeEditorState();
            state.LoadComposite(null);   // rack NUEVO, de un solo sentido
            Assert.Equal(PushBackCellTopology.SoloA, state.DefaultTopology);

            state.SetSideBPresent(true);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }

            Assert.Equal(PushBackCellTopology.Encontradas, state.DefaultTopology);

            state.SideB.LoadNew();
            state.SetSlotCount(Fronts);

            // I-42 (A1/H5) — ampliar la rejilla no declara almacenamiento en el otro lado: las ranuras NUEVAS nacen
            // ausentes en el lado que no las pidio. Este fixture quiere el rack compuesto entero, asi que declara
            // tambien las que acaba de crear — que es la misma decision explicita que hizo con las primeras.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }

            var system = Build(state).System;
            for (var front = 0; front < Fronts; front++)
            {
                var cell = system.Composite.Cell(front, 1);
                Assert.Equal(PushBackCellTopology.Encontradas, cell.Topology);
                Assert.Equal(2, cell.Beds.Count);
            }

            // Y retirarlo devuelve el rack a su default de un sentido.
            state.SetSideBPresent(false);
            Assert.Equal(PushBackCellTopology.SoloA, state.DefaultTopology);
        }

        /// <summary>Una eleccion EXPLICITA distinta del default no la pisa nadie al declarar o retirar el lado B.</summary>
        [Fact]
        public void AnExplicitDefaultTopology_IsNeverOverwritten()
        {
            var state = new PushBackCompositeEditorState();
            state.SetSideBPresent(true);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            state.SetSideBPresent(false);
            state.SetSideBPresent(true);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            Assert.Equal(PushBackCellTopology.Corrida, state.DefaultTopology);
            Assert.Equal(PushBackRunDirection.BToA, state.DefaultDirection);
        }

        // ================= O: los CORTES laterales, uno por uno ================================================

        /// <summary>
        /// Cada CORTE lateral muestra las camas de los frentes que tiene al lado, en TODOS sus niveles. El defecto
        /// «el primer corte se ve bien y los demas solo traen estructura» tiene aqui su prueba: se recorren todos los
        /// postes y ninguno puede quedarse sin camas.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA)]
        [InlineData(PushBackCellTopology.Encontradas)]
        [InlineData(PushBackCellTopology.Corrida)]
        public void EverySection_ShowsTheBedsOfItsAdjacentFronts(PushBackCellTopology topology)
        {
            var system = Build(State(topology)).System;
            var builder = new PushBackSystemLateralBuilder();
            var bedsPerFront = topology == PushBackCellTopology.Encontradas ? Levels * 2 : Levels;

            // Con 4 frentes hay 5 lineas de postes: los extremos tocan UN frente y los interiores DOS.
            for (var post = 0; post <= Fronts; post++)
            {
                var instances = builder.Build(system, Catalog, post).Flatten().Instances;
                var rails = instances.Where(instance => instance.Role == HeaderBlockRole.Rail).ToList();

                // Un corte es una PROYECCION: dos frentes contiguos con la misma configuracion dibujan sus camas
                // EXACTAMENTE una encima de otra, asi que lo que se exige es que esten TODAS las camas distintas de
                // los frentes adyacentes — no un conteo de instancias, que solo mediria cuantas veces se repite la
                // misma linea. Antes se dibujaban duplicadas y superpuestas (I-42, ronda post-5a73b92).
                var expected = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                    .Where(axis => axis.Slot == post - 1 || axis.Slot == post)
                    .Select(axis => Math.Round(axis.LowContact.Y, 3))
                    .Distinct()
                    .ToList();
                var drawn = rails.Select(instance => Math.Round(instance.Insertion.Y, 3)).Distinct().ToList();
                var places = rails
                    .Select(instance => FormattableString.Invariant(
                        $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}"))
                    .Distinct()
                    .Count();

                Assert.True(
                    rails.Count >= bedsPerFront,
                    "corte " + post + " (" + topology + "): solo " + rails.Count + " rieles");
                Assert.True(
                    rails.Count == places,
                    "corte " + post + " (" + topology + "): hay rieles DUPLICADOS y superpuestos");
                Assert.True(
                    expected.All(y => drawn.Any(other => Math.Abs(other - y) < 12.0)),
                    "corte " + post + " (" + topology + "): falta la cama de algun frente adyacente");

                // Y la estructura tambien esta: un corte no es solo camas.
                Assert.Contains(instances, instance => instance.Role == HeaderBlockRole.Post);
            }
        }

        // ================= J: el APOYO BAJO, contra el oraculo del Push Back de un sentido ======================

        /// <summary>
        /// El contacto BAJO de una cama, en coordenadas de rack. Es el punto donde el riel se atornilla, no una
        /// coordenada derivada: viene del mismo <see cref="PushBackRunGeometry"/> que coloca el bloque.
        /// </summary>
        private static double LowContactOf(PushBackSystem system, int slot, int level)
            => PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                .Single(axis => axis.Slot == slot && axis.Level == level)
                .LowContact.X;

        /// <summary>El contacto ALTO de una cama: el extremo que se MUEVE al cambiar la demanda.</summary>
        private static double HighContactOf(PushBackSystem system, int slot, int level)
            => PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                .Single(axis => axis.Slot == slot && axis.Level == level)
                .HighContact.X;

        /// <summary>
        /// LA prueba que el dueño necesita: una cama CORRIDA apoya su extremo bajo con EXACTAMENTE la misma regla
        /// fisica que una cama de un solo lado. Se comparan las dos EN EL MISMO RACK, con el mismo arranque, asi que
        /// cualquier desviacion —y desde luego un desplazamiento de un fondo entero— sale como diferencia.
        /// </summary>
        [Fact]
        public void ACorridaLowContact_ObeysTheSameRule_AsASideBed()
        {
            var state = State(PushBackCellTopology.SoloA);
            // Frente 1 corrido, el resto de un solo lado. Los dos arrancan en el extremo bajo de A.
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);

            var system = Build(state).System;
            var corrida = system.Composite.Cell(0, 1);
            Assert.Equal(PushBackCellTopology.Corrida, corrida.Topology);
            Assert.Equal(PushBackCellTopology.SoloA, system.Composite.Cell(1, 1).Topology);

            // La corrida completa arranca en el mismo sitio que la cama de A: el extremo bajo del rack.
            Assert.Equal(LowContactOf(system, 1, 1), LowContactOf(system, 0, 1), 6);
        }

        /// <summary>
        /// DECISION FISICA DEL DUEÑO: acortar una corrida NO mueve su extremo bajo. El bajo —por donde se carga y se
        /// descarga— queda anclado al poste exterior; el que se mete hacia dentro es el ALTO. Anclar al reves dejaba
        /// la cama arrancando dentro del rack, con el pasillo delante inaccesible.
        /// </summary>
        [Fact]
        public void ShorteningACorrida_MovesTheHighEnd_AndNeverTheLow()
        {
            var full = State(PushBackCellTopology.Corrida);
            var fullSystem = Build(full).System;
            var capacity = fullSystem.Composite.Cell(0, 1).Beds.Single().DemandPositions;

            var shortState = State(PushBackCellTopology.Corrida);
            shortState.SetCorridaDepth(0, 0, capacity - 3);
            var shortSystem = Build(shortState).System;
            var shortBed = shortSystem.Composite.Cell(0, 1).Beds.Single();
            Assert.Equal(capacity - 3, shortBed.DemandPositions);

            // 1) El BAJO no se mueve: misma coordenada fisica, al modulo exterior.
            Assert.Equal(LowContactOf(fullSystem, 0, 1), LowContactOf(shortSystem, 0, 1), 6);

            // 2) El ALTO si: se ha metido hacia dentro.
            Assert.True(HighContactOf(shortSystem, 0, 1) < HighContactOf(fullSystem, 0, 1) - 1.0);

            // 3) Y apoya en una linea de modulo real, no en un punto intermedio.
            var highX = shortSystem.Structure.Modules[0].StartX + shortBed.ResolvedBedLength;
            Assert.Contains(shortSystem.Structure.Modules, module => Math.Abs(module.EndX - highX) < 1e-6);
        }

        /// <summary>
        /// Un HUECO no es una posicion de almacenamiento. La cama lo atraviesa, pero ni cuenta como fondo ni recibe
        /// tarima: contarlo repartia una tarima de mas a lo largo del riel y DESPLAZABA todas las posiciones, que es
        /// como el defecto se veia en el dibujo.
        /// </summary>
        [Fact]
        public void TheGap_IsNeverAStoragePosition()
        {
            var state = State(PushBackCellTopology.Corrida);
            state.SetGap(48.0);
            state.SideA.Structure.ToggleCell(0, 0, extendSelection: false);
            state.SideA.ApplyDrawPallet(true, DynamicRackCellScope.All);

            var system = Build(state).System;
            var runs = PushBackRuns.Resolve(system);
            var run = runs.Runs.Single(candidate => candidate.Slot == 0 && candidate.Level == 1);
            var front = run.Front();

            var demand = system.Composite.Cell(0, 1).Beds.Single().DemandPositions;

            // El fondo EFECTIVO de la celda es la demanda, NO los modulos que su rango atraviesa (uno mas, el hueco).
            Assert.Equal(demand, PushBackCellDepth.Effective(run.Source, front, run.SourceLevel));
            Assert.True(front.PalletsDeep > demand, "el rango fisico incluye el hueco; la demanda no");

            // Y se dibuja UNA tarima por fondo, ninguna dentro del hueco.
            var pallets = PushBackTarimaPlacement.Lateral(
                run.Source, Catalog, front, int.MaxValue, new[] { run.SourceLevel });
            Assert.Equal(demand, pallets.Count);

            var gapModule = run.Source.Structure.Modules.Single(module => module.Kind == DynamicRackModuleKind.Gap);
            Assert.All(pallets, pallet => Assert.True(
                pallet.Insertion.X <= gapModule.StartX + 1e-6 || pallet.Insertion.X >= gapModule.EndX - 1e-6,
                "ninguna tarima puede caer dentro del hueco"));
        }

        // ================= M: una estructura mas larga que la cama ==============================================

        /// <summary>
        /// La ESTRUCTURA no es la longitud obligatoria de una cama. Con una estructura de 8 fondos y un nivel que
        /// pide 4, ese nivel no pone ni una pieza de cama en el tramo sobrante — y otro nivel del MISMO rack puede
        /// usar los 8 a la vez.
        /// </summary>
        [Fact]
        public void ALongStructure_IsReusedByShorterBeds_WithoutLeavingPiecesBehind()
        {
            var state = State(PushBackCellTopology.SoloA);
            state.SetStructureOverride(PushBackSide.A, 8);

            // Nivel 1 = 4 fondos; nivel 2 = 8. Misma estructura, dos camas de longitudes distintas.
            state.SideA.Structure.ToggleCell(0, 0, extendSelection: false);
            state.SideA.ApplyPalletsDeep(4, DynamicRackCellScope.Cell);
            state.SideA.Structure.ToggleCell(0, 1, extendSelection: false);
            state.SideA.ApplyPalletsDeep(8, DynamicRackCellScope.Cell);

            var system = Build(state).System;
            Assert.Equal(8, system.Composite.SideA.EffectiveStructure);

            var shortBed = system.Composite.Cell(0, 1).BedFrom(PushBackSide.A);
            var longBed = system.Composite.Cell(0, 2).BedFrom(PushBackSide.A);
            Assert.Equal(4, shortBed.DemandPositions);
            Assert.Equal(8, longBed.DemandPositions);
            Assert.True(shortBed.ResolvedBedLength < longBed.ResolvedBedLength - 1.0);

            // Ninguna pieza de la cama corta mas alla de su larguero posterior.
            var runs = PushBackRuns.Resolve(system);
            var run = runs.Runs.Single(candidate => candidate.Slot == 0 && candidate.Level == 1);
            var front = run.Front();
            var rearX = PushBackCellDepth.RearX(run.Source, front, run.SourceLevel);
            Assert.True(rearX < run.Source.Structure.TotalLength - 1.0);

            var intermediates = new PushBackIntermediateBeamLateralBuilder()
                .BuildFor(run.Source, Catalog, front, new[] { run.SourceLevel });
            Assert.All(intermediates, beam => Assert.True(beam.Insertion.X < rearX + 1e-6));

            var pallets = PushBackTarimaPlacement.Lateral(
                run.Source, Catalog, front, int.MaxValue, new[] { run.SourceLevel });
            Assert.All(pallets, pallet => Assert.True(pallet.Insertion.X < rearX + 1e-6));
        }

        // ================= Mezcla de topologias y fondos por celda en un rack de 4 x 3 =========================

        /// <summary>
        /// Las cuatro topologias conviven en el MISMO rack, mezcladas por frente y por nivel, cada una con su numero
        /// de camas. Es el caso que junta todo lo que esta ronda toca.
        /// </summary>
        [Fact]
        public void TheFourTopologies_Coexist_MixedByFrontAndLevel()
        {
            var state = State(PushBackCellTopology.Encontradas);
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCell(0, 1, PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            state.SetCell(1, 2, PushBackCellTopology.SoloB, PushBackRunDirection.AToB);
            state.SetCell(2, 0, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);

            var system = Build(state).System;
            var runs = PushBackRuns.Resolve(system);

            Assert.Equal(PushBackCellTopology.Corrida, system.Composite.Cell(0, 1).Topology);
            Assert.Equal(PushBackCellTopology.SoloA, system.Composite.Cell(0, 2).Topology);
            Assert.Equal(PushBackCellTopology.Encontradas, system.Composite.Cell(0, 3).Topology);
            Assert.Equal(PushBackCellTopology.SoloB, system.Composite.Cell(1, 3).Topology);
            Assert.Equal(PushBackCellTopology.Corrida, system.Composite.Cell(2, 1).Topology);

            // Los sentidos opuestos coexisten: una corrida hacia B y otra hacia A en el mismo rack.
            Assert.Equal(PushBackSide.B, runs.Runs.Single(r => r.Slot == 0 && r.Level == 1).HighSide);
            Assert.Equal(PushBackSide.A, runs.Runs.Single(r => r.Slot == 2 && r.Level == 1).HighSide);

            // Y el conteo total es el de las camas fisicas, no el de celdas.
            var expected = 1 + 1 + 2      // F1: corrida, solo A, encontradas
                           + 2 + 2 + 1    // F2: encontradas, encontradas, solo B
                           + 1 + 2 + 2    // F3: corrida, encontradas, encontradas
                           + 2 + 2 + 2;   // F4: encontradas x3
            Assert.Equal(expected, runs.Runs.Count);
        }

        /// <summary>
        /// Fondos de corrida DISTINTOS por frente sobre la misma estructura: cada uno resuelve su propio apoyo, y la
        /// receta sintetica compartida por demanda no mezcla los frentes.
        /// </summary>
        [Fact]
        public void DifferentCorridaDepths_PerFront_ResolveIndependently()
        {
            var state = State(PushBackCellTopology.Corrida);
            var capacity = Build(state).System.Composite.Cell(0, 1).Beds.Single().DemandPositions;

            state.SetCorridaDepth(0, 0, capacity);
            state.SetCorridaDepth(1, 0, capacity - 1);
            state.SetCorridaDepth(2, 0, capacity - 2);

            var system = Build(state).System;
            var lengths = Enumerable.Range(0, 3)
                .Select(front => system.Composite.Cell(front, 1).Beds.Single().ResolvedBedLength)
                .ToList();

            Assert.True(lengths[0] > lengths[1] + 1.0);
            Assert.True(lengths[1] > lengths[2] + 1.0);

            // Y el frente 4, sin fondo propio, sigue el default derivado: atraviesa el rack.
            Assert.Equal(lengths[0], system.Composite.Cell(3, 1).Beds.Single().ResolvedBedLength, 6);

            // La estructura NO se movio por ninguno de ellos.
            Assert.Equal(
                Build(State(PushBackCellTopology.Corrida)).System.Structure.TotalLength,
                system.Structure.TotalLength,
                6);
        }

        // ================= La retícula transversal es del RACK ==================================================

        /// <summary>
        /// Crecer el numero de frentes crece los DOS lados: la retícula es una sola. Antes crecia solo el lado activo,
        /// y el resultado era un rack cuyo primer frente tenia las dos mitades y los demas solo una — exactamente lo
        /// que el dueño vio.
        /// </summary>
        [Fact]
        public void TheSlotCount_BelongsToTheRack_NotToTheActiveSide()
        {
            var state = new PushBackCompositeEditorState();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();

            state.SetSlotCount(4);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            Assert.Equal(4, state.SideA.Structure.Count);
            Assert.Equal(4, state.SideB.Structure.Count);
            for (var slot = 0; slot < 4; slot++)
            {
                Assert.True(state.IsSlotPresent(PushBackSide.A, slot));
                Assert.True(state.IsSlotPresent(PushBackSide.B, slot));
            }
        }

        /// <summary>La asimetria A=3 / B=4 se expresa con PRESENCIA, y sigue produciendo UNA sola estructura.</summary>
        [Fact]
        public void AnAsymmetricRack_IsExpressedWithPresence_OverOneGrid()
        {
            var state = State(PushBackCellTopology.Encontradas);
            Assert.Null(state.WhySlotCannotBeRemoved(PushBackSide.A, 3));
            state.SetSlotPresent(PushBackSide.A, 3, false);

            var system = Build(state).System;
            Assert.Equal(Fronts, system.Structure.Fronts.Count);

            // La cuarta ranura existe solo en B: sus celdas degradan a Solo B, y las otras tres siguen encontradas.
            for (var level = 1; level <= Levels; level++)
            {
                Assert.Equal(PushBackCellTopology.SoloB, system.Composite.Cell(3, level).Topology);
                Assert.Equal(PushBackCellTopology.Encontradas, system.Composite.Cell(2, level).Topology);
            }

            var runs = PushBackRuns.Resolve(system);
            Assert.Equal(Levels, runs.Runs.Count(run => run.Slot == 3));
            Assert.Equal(Levels * 2, runs.Runs.Count(run => run.Slot == 2));
        }

        /// <summary>
        /// DEFECTO ENCONTRADO en la auditoria: la PRESENCIA del lado A no volvia del archivo. El diseño declara las
        /// ranuras ausentes en vez de borrarlas —borrarlas desplazaria todos los indices—, pero el editor no las
        /// releia, asi que un rack asimetrico se reabria con las cuatro ranuras en los dos lados.
        /// </summary>
        [Fact]
        public void ThePresenceOfSideA_ComesBackFromTheDesign()
        {
            var state = State(PushBackCellTopology.Encontradas);
            state.SetSlotPresent(PushBackSide.A, 3, false);

            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());
            Assert.Contains(3, design.Composite.AbsentSlotsA);

            var reopened = new PushBackCompositeEditorState();
            reopened.SetSideBPresent(true);
            reopened.SideB.LoadNew();
            reopened.SetSlotCount(Fronts);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < reopened.SlotCount; declared++)
            {
                reopened.SetSlotPresent(PushBackSide.B, declared, true);
            }

            reopened.LoadComposite(design.Composite);

            Assert.False(reopened.IsSlotPresent(PushBackSide.A, 3));
            Assert.True(reopened.IsSlotPresent(PushBackSide.A, 2));
            Assert.True(reopened.IsSlotPresent(PushBackSide.B, 3));

            // Y el rack que vuelve es el MISMO: la cuarta ranura sigue siendo Solo B.
            var system = Build(reopened).System;
            Assert.Equal(PushBackCellTopology.SoloB, system.Composite.Cell(3, 1).Topology);
        }

        /// <summary>Una ranura no puede quedarse sin ningun lado, ni un lado sin ninguna ranura. Se explica, no se aplica.</summary>
        [Fact]
        public void RemovingASlot_IsRefused_WhenItWouldLeaveNothing()
        {
            var state = State(PushBackCellTopology.Encontradas);
            state.SetSlotPresent(PushBackSide.A, 3, false);

            // La ranura 4 ya solo existe en B: retirarla de B la dejaria sin ningun lado.
            Assert.NotNull(state.WhySlotCannotBeRemoved(PushBackSide.B, 3));

            // Y vaciar un lado entero tampoco es legal.
            state.SetSlotPresent(PushBackSide.A, 1, false);
            state.SetSlotPresent(PushBackSide.A, 2, false);
            Assert.NotNull(state.WhySlotCannotBeRemoved(PushBackSide.A, 0));
        }
    }
}
