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
    /// I-42 (corrección aislada 4) — <c>RearTope(run) = HighEnd(run)</c>.
    ///
    /// <para>
    /// El tope trasero pertenece SIEMPRE al extremo ALTO de una cama física. No al lado A/B, ni a izquierda/derecha,
    /// ni al primer o último frente. Dos camas encontradas admiten dos topes independientes; una corrida admite
    /// exactamente UNO, del lado que sea su extremo alto — y la intención guardada en el otro lado queda DORMANTE.
    /// </para>
    /// <para>
    /// El lateral es el oráculo de posición: estas pruebas derivan lo esperado de cada cama con la MISMA autoridad
    /// que lo coloca (<see cref="PushBackRearTopeBuilder"/> en el marco de la cama, más la reflexión rígida) y
    /// exigen correspondencia 1:1 en lateral, frontal, planta y BOM. Ninguna vista vuelve a decidir aplicabilidad.
    /// </para>
    /// </summary>
    public class PushBackRearTopeRunAuthorityTests
    {
        private const double Eps = 1e-6;

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

        private static PushBackSystem Build(PushBackCompositeEditorState state)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lo que CADA cama exige, derivado de ella misma ------------------------------------------------------

        private sealed class Expected
        {
            public PushBackRun Run;
            public double X;
            public double Z;
            public string PieceId;
        }

        /// <summary>
        /// El tope que una cama pide: se construye con la MISMA autoridad que lo coloca, en el marco de la cama, y
        /// se lleva al mundo con la MISMA reflexión rígida. Es identidad, no búsqueda por cercanía.
        /// </summary>
        private static IReadOnlyList<Expected> ExpectedTopes(PushBackSystem system)
        {
            var runs = PushBackRuns.Resolve(system);
            var builder = new PushBackRearTopeBuilder();
            var result = new List<Expected>();

            foreach (var run in runs.Runs)
            {
                var config = run.Source?.RearTope ?? new PushBackRearTopeConfig();
                if (run.Front() == null || !config.At(run.SourceFrontIndex, run.SourceLevel - 1))
                {
                    continue;   // la celda esta desactivada: ninguna vista puede dibujarla
                }

                var pieces = builder.BuildLateral(
                    run.Source, Catalog, run.SourceFrontIndex, run.Front(), new[] { run.SourceLevel });
                var placed = run.Reflected ? PushBackMirror.Instances(pieces, runs.MirrorAxis) : pieces;
                foreach (var instance in placed)
                {
                    result.Add(new Expected
                    {
                        Run = run,
                        X = Math.Round(instance.Insertion.X, 3),
                        Z = Math.Round(instance.Insertion.Y, 4),
                        PieceId = instance.PieceId
                    });
                }
            }

            return result;
        }

        // ---- lo que cada vista DIBUJA --------------------------------------------------------------------------

        private static IReadOnlyList<(double X, double Y, string PieceId)> Topes(HeaderRunPlan plan)
            => plan.Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .Select(instance => (Math.Round(instance.Insertion.X, 3), Math.Round(instance.Insertion.Y, 4), instance.PieceId))
                .Distinct()
                .ToList();

        private static IReadOnlyList<(double X, double Y, string PieceId)> Lateral(PushBackSystem system)
        {
            var builder = new PushBackSystemLateralBuilder();
            var pieces = Topes(builder.Build(system, Catalog)).ToList();
            foreach (var corte in builder.Cortes(system, Catalog))
            {
                pieces.AddRange(Topes(corte.Plan));
            }

            return pieces.Distinct().ToList();
        }

        private static IReadOnlyList<(double X, double Y, string PieceId)> Frontal(
            PushBackSystem system, PushBackSide side)
            => Topes(new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.Posterior, side));

        private static IReadOnlyList<(double X, double Y, string PieceId)> Planta(PushBackSystem system)
            => Topes(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog));

        private static IReadOnlyList<(string PieceId, int Quantity)> BomTopes(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == PushBackBomBuilder.RearTope)
                .GroupBy(component => component.ProfileId)
                .Select(group => (group.Key, group.Sum(component => component.Quantity)))
                .OrderBy(entry => entry.Key)
                .ToList();

        // ---- LA comprobación 1:1 --------------------------------------------------------------------------------

        /// <summary>
        /// Cada cama con tope aparece EXACTAMENTE una vez en cada vista que le corresponde, con su pieza; y ninguna
        /// vista dibuja un tope que ninguna cama pida.
        /// </summary>
        private static void AssertTopesMatchTheRuns(string label, PushBackSystem system, bool checkPlantaX = true)
        {
            var expected = ExpectedTopes(system);

            // 1) LATERAL — el oráculo: el conjunto de (X, Z, pieza) es exactamente el que piden las camas.
            var lateral = Lateral(system).OrderBy(t => t.X).ThenBy(t => t.Y).ThenBy(t => t.PieceId).ToList();
            var wantedLateral = expected
                .Select(e => (e.X, e.Z, e.PieceId))
                .Distinct()
                .OrderBy(t => t.X).ThenBy(t => t.Z).ThenBy(t => t.PieceId)
                .ToList();
            Assert.Equal(wantedLateral.Count, lateral.Count);
            for (var index = 0; index < wantedLateral.Count; index++)
            {
                Assert.Equal(wantedLateral[index].X, lateral[index].X, 3);
                Assert.Equal(wantedLateral[index].Z, lateral[index].Y, 4);
                Assert.Equal(wantedLateral[index].PieceId, lateral[index].PieceId);
            }

            // 2) FRONTAL — sólo en el corte del lado que posee el extremo ALTO, y con las mismas elevaciones.
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var wanted = expected
                    .Where(e => e.Run.HighSide == side)
                    .Select(e => (e.Z, e.PieceId))
                    .Distinct()
                    .OrderBy(t => t.Z).ThenBy(t => t.PieceId)
                    .ToList();
                var drawn = Frontal(system, side)
                    .Select(t => (t.Y, t.PieceId))
                    .Distinct()
                    .OrderBy(t => t.Y).ThenBy(t => t.PieceId)
                    .ToList();

                Assert.Equal(wanted.Count, drawn.Count);
                for (var index = 0; index < wanted.Count; index++)
                {
                    Assert.Equal(wanted[index].Z, drawn[index].Y, 4);
                    Assert.Equal(wanted[index].PieceId, drawn[index].PieceId);
                }
            }

            // 3) PLANTA — colapsa los niveles, así que proyecta (X del extremo alto, columna de la ranura, pieza).
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var wantedPlanta = expected
                .Select(e => (e.X, Column: Math.Round(PlantaColumn(layout, e.Run.Slot, e.X, system), 3), e.PieceId))
                .Distinct()
                .OrderBy(t => t.X).ThenBy(t => t.Column).ThenBy(t => t.PieceId)
                .ToList();
            var planta = Planta(system).OrderBy(t => t.X).ThenBy(t => t.Y).ThenBy(t => t.PieceId).ToList();

            Assert.Equal(wantedPlanta.Count, planta.Count);
            for (var index = 0; index < wantedPlanta.Count; index++)
            {
                if (checkPlantaX)
                {
                    Assert.Equal(wantedPlanta[index].X, planta[index].X, 3);
                }

                Assert.Equal(wantedPlanta[index].PieceId, planta[index].PieceId);
            }

            // 4) BOM — una pieza física por cama con tope, con la variante de SU cama.
            var wantedBom = expected
                .GroupBy(e => e.PieceId)
                .Select(group => (PieceId: group.Key, Quantity: group.Count()))
                .OrderBy(entry => entry.PieceId)
                .ToList();
            Assert.Equal(wantedBom, BomTopes(system));
        }

        /// <summary>
        /// La columna transversal en la que la planta dibuja el tope de una ranura: la del poste de esa ranura, más
        /// el desplazamiento que el propio bloque ya lleva en el lateral. Se toma del dibujo de la planta sólo para
        /// ordenar; la comprobación exigente es la X longitudinal y la pieza.
        /// </summary>
        private static double PlantaColumn(DynamicFrontLayout layout, int slot, double x, PushBackSystem system)
            => slot >= 0 && slot < layout.PostPositions.Count ? layout.PostPositions[slot] : 0.0;

        // ---- 1-5: las cinco topologías ---------------------------------------------------------------------------

        public static IEnumerable<object[]> Topologies() => new[]
        {
            new object[] { PushBackCellTopology.SoloA, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.SoloB, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Encontradas, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.BToA }
        };

        [Theory]
        [MemberData(nameof(Topologies))]
        public void RearTope_AlwaysBelongsToRunHighEnd(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var state = State();
            state.SetDefaults(topology, direction);
            AssertTopesMatchTheRuns($"{topology}/{direction}", Build(state));
        }

        /// <summary>Una corrida A→B tiene UN tope y está en B; el corte posterior de A no lleva ninguno.</summary>
        [Fact]
        public void CorridaAtoB_HasOnlyOneRearTopeAtB()
        {
            var state = State(slots: 1);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            var system = Build(state);

            var runs = PushBackRuns.Resolve(system).Runs.ToList();
            Assert.All(runs, run => Assert.Equal(PushBackSide.B, run.HighSide));
            Assert.Equal(runs.Count, Lateral(system).Count);
            Assert.Empty(Frontal(system, PushBackSide.A));
            Assert.NotEmpty(Frontal(system, PushBackSide.B));
            AssertTopesMatchTheRuns("corrida A->B", system);
        }

        [Fact]
        public void CorridaBtoA_HasOnlyOneRearTopeAtA()
        {
            var state = State(slots: 1);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            var system = Build(state);

            var runs = PushBackRuns.Resolve(system).Runs.ToList();
            Assert.All(runs, run => Assert.Equal(PushBackSide.A, run.HighSide));
            Assert.Equal(runs.Count, Lateral(system).Count);
            Assert.NotEmpty(Frontal(system, PushBackSide.A));
            Assert.Empty(Frontal(system, PushBackSide.B));
            AssertTopesMatchTheRuns("corrida B->A", system);
        }

        /// <summary>Camas encontradas: dos topes independientes, uno por cama, cada uno en su extremo.</summary>
        [Fact]
        public void Encontradas_HaveIndependentRearTopes()
        {
            var state = State(slots: 1);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var system = Build(state);

            var runs = PushBackRuns.Resolve(system).Runs.ToList();
            Assert.Equal(2, runs.Count(run => run.Level == 1));   // una cama por lado
            Assert.NotEmpty(Frontal(system, PushBackSide.A));
            Assert.NotEmpty(Frontal(system, PushBackSide.B));

            // Y sus extremos altos son DOS líneas distintas, no una.
            Assert.Equal(2, Lateral(system).Select(tope => tope.X).Distinct().Count());
            AssertTopesMatchTheRuns("encontradas", system);
        }

        // ---- 6-7: niveles con topologías distintas ---------------------------------------------------------------

        [Fact]
        public void MixedLevels_EncontradasAndCorrida_EachRunKeepsItsOwnTope()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetCell(slot, 1, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            }

            AssertTopesMatchTheRuns("N1 encontradas / N2 corrida", Build(state));
        }

        [Fact]
        public void OppositeDirectionsPerLevel_EachRunKeepsItsOwnTope()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetCell(slot, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
                state.SetCell(slot, 1, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            }

            var system = Build(state);
            AssertTopesMatchTheRuns("N1 A->B / N2 B->A", system);

            // Cada corte posterior lleva EXACTAMENTE el de su nivel.
            Assert.Single(Frontal(system, PushBackSide.A).Select(tope => tope.Y).Distinct());
            Assert.Single(Frontal(system, PushBackSide.B).Select(tope => tope.Y).Distinct());
        }

        // ---- 8: la variante de cada lado -------------------------------------------------------------------------

        /// <summary>
        /// EL DEFECTO DEL BOM. Con topes de variante distinta en A y en B, los dibujos ya ponían cada uno el suyo
        /// pero el BOM contaba TODOS como la variante del primer lado — la sacaba de <c>system.RearTope</c>.
        /// </summary>
        [Fact]
        public void RearTope_BomUsesTheRunsCorrectPieceId()
        {
            var variants = Catalog.SafetyElements
                .Where(entry => string.Equals(
                    entry.Type,
                    RackCad.Domain.Systems.Selective.SelectiveSafetyDefaults.TopeType,
                    StringComparison.OrdinalIgnoreCase))
                .Select(entry => entry.Id)
                .ToList();
            Assert.True(variants.Count >= 2, "el catálogo necesita dos variantes de tope para este caso");

            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            state.SideA.RearTopePieceId = variants[0];
            state.SideB.RearTopePieceId = variants[1];

            var system = Build(state);
            var bom = BomTopes(system);

            Assert.Equal(2, bom.Count);
            Assert.Equal(4, bom.Single(entry => entry.PieceId == variants[0]).Quantity);
            Assert.Equal(4, bom.Single(entry => entry.PieceId == variants[1]).Quantity);

            // Y los dibujos siguen diciendo lo mismo que el BOM.
            AssertTopesMatchTheRuns("piezas distintas A/B", system);
        }

        [Fact]
        public void RearTope_BomCountMatchesPhysicalRuns()
        {
            foreach (var topology in new[]
            {
                PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
            })
            {
                var state = State(slots: 3);
                state.SetDefaults(topology, PushBackRunDirection.AToB);
                var system = Build(state);

                var expected = ExpectedTopes(system).Count;
                Assert.Equal(expected, BomTopes(system).Sum(entry => entry.Quantity));
                Assert.Equal(PushBackRuns.Resolve(system).Runs.Count, expected);
            }
        }

        // ---- 9-10: celdas desactivadas ---------------------------------------------------------------------------

        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void RearTope_OffCellSuppressesAllConsumers(PushBackSide side)
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            state.Of(side).Cell(0, 0).RearTopeEnabled = false;

            var system = Build(state);
            AssertTopesMatchTheRuns($"off-cell en {side}", system);

            // Una celda menos, en TODAS las vistas y en el BOM.
            var full = State();
            full.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var reference = Build(full);

            Assert.Equal(
                BomTopes(reference).Sum(entry => entry.Quantity) - 1,
                BomTopes(system).Sum(entry => entry.Quantity));
        }

        // ---- 7 (planta): la intención dormante del lado bajo ------------------------------------------------------

        /// <summary>
        /// EL DEFECTO DE LA PLANTA. Con el nivel 1 corrido A→B (su alto está en B, al final del rack) y el nivel 2
        /// sólo-A con su tope APAGADO, la planta dibujaba igualmente un tope en la interfaz: lo pedía la intención
        /// DORMANTE que el lado A guarda para el nivel corrido, que no es una cama suya. El lateral y el BOM ya no
        /// lo dibujaban, así que las vistas se contradecían.
        /// </summary>
        [Fact]
        public void Planta_DoesNotMaterializeDormantLowSideTope()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetCell(slot, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
                state.SetCell(slot, 1, PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
                state.SideA.Cell(slot, 1).RearTopeEnabled = false;
            }

            var system = Build(state);
            var runs = PushBackRuns.Resolve(system).Runs;

            // Sólo las corridas piden tope, y su extremo alto está en B.
            var expected = ExpectedTopes(system);
            Assert.Equal(runs.Count(run => run.Topology == PushBackCellTopology.Corrida), expected.Count);
            Assert.All(expected, entry => Assert.Equal(PushBackSide.B, entry.Run.HighSide));

            // La planta no añade ninguno en la interfaz.
            var interfaceX = Math.Round(system.Structure.TotalLength / 2.0, 0);
            Assert.DoesNotContain(Planta(system), tope => Math.Abs(tope.X - interfaceX) < 5.0);

            AssertTopesMatchTheRuns("intención dormante del lado bajo", system);
        }

        /// <summary>
        /// La planta usa los niveles REALES de cada cama, no «cualquier nivel del lado»: con el tope encendido sólo
        /// en un nivel, la ranura sigue teniendo uno; con TODOS apagados, ninguno.
        /// </summary>
        [Fact]
        public void Planta_UsesTheRunsActualLevels_NotAnyLevelFromTheSide()
        {
            var state = State(slots: 2, levels: 2);
            state.SetDefaults(PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            for (var level = 0; level < 2; level++)
            {
                state.SideA.Cell(0, level).RearTopeEnabled = false;   // la ranura 0 se queda SIN ningún tope
            }

            var system = Build(state);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);

            // Ni un tope en la columna de la ranura 0, y los de la ranura 1 intactos.
            Assert.DoesNotContain(
                Planta(system),
                tope => Math.Abs(tope.Y - layout.PostPositions[0]) < 2.0);
            Assert.Contains(
                Planta(system),
                tope => Math.Abs(tope.Y - layout.PostPositions[1]) < 2.0);

            AssertTopesMatchTheRuns("ranura 0 sin topes", system);
        }

        [Fact]
        public void RearTope_PlantMatchesLateral()
        {
            foreach (var topology in new[]
            {
                PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
            })
            {
                var state = State(slots: 3);
                state.SetDefaults(topology, PushBackRunDirection.AToB);
                var system = Build(state);

                var lateralX = Lateral(system).Select(tope => Math.Round(tope.X, 2)).Distinct().OrderBy(x => x).ToList();
                var plantaX = Planta(system).Select(tope => Math.Round(tope.X, 2)).Distinct().OrderBy(x => x).ToList();
                Assert.Equal(lateralX, plantaX);
            }
        }

        [Fact]
        public void RearTope_FrontalMatchesLateral()
        {
            var state = State(slots: 2);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var system = Build(state);

            var lateralZ = Lateral(system).Select(tope => tope.Y).Distinct().OrderBy(z => z).ToList();
            var frontalZ = Frontal(system, PushBackSide.A)
                .Concat(Frontal(system, PushBackSide.B))
                .Select(tope => tope.Y)
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            Assert.Equal(lateralZ, frontalZ);
        }

        // ---- 11-14: geometría del rack ---------------------------------------------------------------------------

        [Fact]
        public void MultiFront_RearTopeKeepsItsSlotIdentity()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            for (var level = 0; level < 2; level++)
            {
                state.SideA.Cell(1, level).RearTopeEnabled = false;   // sólo la ranura DEL MEDIO se queda sin tope
            }

            var system = Build(state);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var columns = Planta(system).Select(tope => tope.Y).ToList();

            Assert.Contains(columns, y => Math.Abs(y - layout.PostPositions[0]) < 2.0);
            Assert.DoesNotContain(columns, y => Math.Abs(y - layout.PostPositions[1]) < 2.0);
            Assert.Contains(columns, y => Math.Abs(y - layout.PostPositions[2]) < 2.0);

            AssertTopesMatchTheRuns("ranura del medio sin tope", system);
        }

        [Fact]
        public void BlankEarlierFront_DoesNotMoveLaterRearTope()
        {
            var baseline = State(slots: 3);
            baseline.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var before = Build(baseline);
            var beforeSlot2 = Planta(before)
                .Where(tope => tope.Y > DynamicFrontGeometry.Compute(before.Structure, Catalog).PostPositions[1])
                .OrderBy(tope => tope.X).ThenBy(tope => tope.Y)
                .ToList();

            var blanked = State(slots: 3);
            blanked.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            Assert.True(blanked.SetSlotPresent(PushBackSide.A, 0, false));
            var after = Build(blanked);
            var afterSlot2 = Planta(after)
                .Where(tope => tope.Y > DynamicFrontGeometry.Compute(after.Structure, Catalog).PostPositions[1])
                .OrderBy(tope => tope.X).ThenBy(tope => tope.Y)
                .ToList();

            Assert.Equal(beforeSlot2, afterSlot2);
            AssertTopesMatchTheRuns("ranura 0 en blanco en A", after);
        }

        [Theory]
        [InlineData(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB)]
        public void WithGap_TopesStillFollowTheirRuns(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var state = State(slots: 2);
            state.SetGap(48.0);
            state.SetDefaults(topology, direction);
            AssertTopesMatchTheRuns($"{topology} con calle 48", Build(state));
        }

        /// <summary>
        /// Una corrida CORTA lleva su tope a SU propio extremo alto, que no es el del rack: aplicabilidad, pieza,
        /// conteo, lateral y frontal lo siguen.
        ///
        /// <para>
        /// La X que la PLANTA le da NO se comprueba aquí, y es deliberado: está medida y REPORTADA como un defecto
        /// aparte que esta corrida no arregla. El lateral pone ese tope en X = 101.125 —a 0.72" del contacto alto,
        /// del lado por el que llega la tarima— y la planta en X = 102.875, al otro lado del larguero. La causa es
        /// que en planta el punto de anclaje se busca en el POSTE MÁS CERCANO, y el extremo alto de una corrida
        /// corta no cae en ninguna línea de postes. Arreglarlo toca <c>PushBackRearTopeBuilder.PostMateWorld</c>,
        /// que comparten las tres vistas y también un rack de un solo sentido.
        /// </para>
        /// </summary>
        [Fact]
        public void ShortCorrida_TopeFollowsItsOwnHighEnd()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCorridaDepth(1, 1, 2);
            var system = Build(state);

            // La cama corta tiene su extremo alto DENTRO de la estructura, y su tope va con ella.
            var expected = ExpectedTopes(system);
            var interior = expected.Where(entry => entry.X < system.Structure.TotalLength / 2.0).ToList();
            Assert.Single(interior);
            Assert.Contains(Lateral(system), tope => Math.Abs(tope.X - interior[0].X) < Eps);

            AssertTopesMatchTheRuns("corrida corta en la ranura 1", system, checkPlantaX: false);
        }



        // ---- 15: persistencia -------------------------------------------------------------------------------------

        /// <summary>
        /// La INTENCIÓN de cada lado sobrevive al archivo, y su APLICABILIDAD se vuelve a derivar de la topología:
        /// un tope dormante sigue dormante, y uno efectivo sigue efectivo.
        /// </summary>
        [Fact]
        public void SaveLoad_PreservesRearTopeIntentAndApplicability()
        {
            var state = State(slots: 2);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            state.SetCell(0, 1, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SideA.Cell(1, 0).RearTopeEnabled = false;

            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());
            var before = new PushBackResolver(Catalog).Resolve(design);

            var json = System.Text.Json.JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var after = new PushBackResolver(Catalog).Resolve(
                System.Text.Json.JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain());

            Assert.Equal(
                ExpectedTopes(before).Select(e => (e.X, e.Z, e.PieceId)).OrderBy(t => t.X).ThenBy(t => t.Z),
                ExpectedTopes(after).Select(e => (e.X, e.Z, e.PieceId)).OrderBy(t => t.X).ThenBy(t => t.Z));
            Assert.Equal(BomTopes(before), BomTopes(after));
            AssertTopesMatchTheRuns("tras guardar y abrir", after);
        }
    }
}
