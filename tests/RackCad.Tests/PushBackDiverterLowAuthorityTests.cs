using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (corrección aislada) — <c>Diverter(run) = LowEnd(run)</c>.
    ///
    /// <para>
    /// Un desviador guía la tarima al ENTRAR, así que pertenece siempre al extremo por el que se carga esa cama.
    /// El corte lateral compuesto lo heredaba del builder dinámico, que conserva la regla de un rack de un solo
    /// sentido —«izquierda = extremo bajo, derecha = extremo alto»—: el lado A acertaba por coincidencia y el B
    /// salía a la elevación del extremo contrario. Y una clasificación por línea no puede expresar que en la MISMA
    /// línea el nivel 1 corra A→B y el nivel 2 B→A.
    /// </para>
    /// <para>
    /// Estas pruebas afirman X <b>y</b> Z <b>y</b> la identidad del run: comprobar solo la X dejaba pasar
    /// exactamente el defecto que el dueño vio.
    /// </para>
    /// </summary>
    public class PushBackDiverterLowAuthorityTests
    {
        /// <summary>Cuánto cuelga el desviador por debajo de su larguero, en los niveles que no son el primero.</summary>
        private static double Offset => RackCad.Application.Systems.Selective.SelectiveDesviadorPlan.BeamYOffset;

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
                for (var slot = 0; slot < matrix.Count; slot++)
                {
                    state.Of(side).AdjustLevels(slot, levels - matrix.Fronts[slot].LoadLevels);
                }
            }

            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>Los desviadores DIBUJADOS en el corte lateral, con su posición y su mano.</summary>
        private static IReadOnlyList<(double X, double Y, bool Mirrored)> Drawn(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances
                .Where(i => PushBackDiverterPlan.IsDiverter(i, Catalog))
                .Select(i => (Math.Round(i.Insertion.X, 3), Math.Round(i.Insertion.Y, 4), i.MirroredX))
                .Distinct()
                .OrderBy(p => p.Item1).ThenBy(p => p.Item2)
                .ToList();

        /// <summary>
        /// Lo que CADA cama exige: su extremo bajo en X y, salvo el primer nivel, seis pulgadas por debajo de su
        /// larguero de entrada. Se deriva del run, que es la autoridad.
        /// </summary>
        private static IReadOnlyList<(double X, double Y)> ExpectedFromRuns(PushBackSystem system)
        {
            var runs = PushBackRuns.Resolve(system);
            var expected = new List<(double, double)>();
            foreach (var run in runs.Runs)
            {
                var axis = PushBackRunGeometry.Axis(run, Catalog, runs.MirrorAxis);
                if (!axis.HasValue || run.Level <= 1)
                {
                    continue;   // el primer nivel conserva el contrato Selectivo: mide desde el troquel del poste
                }

                var low = PushBackElevations.LowInsertions(run.Source, Catalog, run.Front());
                if (!low.TryGetValue(run.SourceLevel, out var z))
                {
                    continue;
                }

                var total = system.Structure.TotalLength;
                var atStart = Math.Abs(axis.Value.LowContact.X) <= Math.Abs(axis.Value.LowContact.X - total);
                expected.Add((Math.Round(atStart ? 0.0 : total, 3), Math.Round(z - Offset, 4)));
            }

            return expected.Distinct().OrderBy(p => p.Item1).ThenBy(p => p.Item2).ToList();
        }

        // ===================== 1-5: las cinco topologias =====================================================

        public static IEnumerable<object[]> Topologies() => new[]
        {
            new object[] { PushBackCellTopology.SoloA, PushBackRunDirection.AToB, true, false },
            new object[] { PushBackCellTopology.SoloB, PushBackRunDirection.AToB, false, true },
            new object[] { PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, true, true },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.AToB, true, false },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.BToA, false, true },
        };

        /// <summary>
        /// El desviador de cada cama cae EXACTAMENTE en su extremo bajo, en X y en Z. Una corrida tiene UNO, y nunca
        /// en su extremo alto.
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void Diverter_IsAtTheLowEndOfEveryRun(
            PushBackCellTopology topology, PushBackRunDirection direction, bool atStart, bool atEnd)
        {
            var state = State();
            state.SetDefaults(topology, direction);
            var system = Build(state);
            var total = system.Structure.TotalLength;
            var drawn = Drawn(system);

            Assert.NotEmpty(drawn);

            // X: solo los pasillos que de verdad cargan.
            Assert.Equal(atStart, drawn.Any(p => Math.Abs(p.X) < 1.0));
            Assert.Equal(atEnd, drawn.Any(p => Math.Abs(p.X - total) < 1.0));

            // Z: cada cama exige el suyo, y todos estan.
            var expected = ExpectedFromRuns(system);
            Assert.NotEmpty(expected);
            foreach (var want in expected)
            {
                Assert.True(
                    drawn.Any(p => Math.Abs(p.X - want.X) < 1.0 && Math.Abs(p.Y - want.Y) < 1e-6),
                    $"{topology}/{direction}: falta el desviador en ({want.X:0.##}, {want.Y:0.####})");
            }

            // Y ninguno sobra: todo lo dibujado por encima del primer nivel pertenece a una cama.
            var firstLevel = drawn.OrderBy(p => p.Y).Take(atStart && atEnd ? 2 : 1).ToList();
            foreach (var piece in drawn.Except(firstLevel))
            {
                Assert.True(
                    expected.Any(want => Math.Abs(piece.X - want.X) < 1.0 && Math.Abs(piece.Y - want.Y) < 1e-6),
                    $"{topology}/{direction}: desviador en ({piece.X:0.##}, {piece.Y:0.####}) sin cama que lo pida");
            }
        }

        /// <summary>Y una CORRIDA no tiene ninguno en su extremo ALTO, dicho explicitamente.</summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void Corrida_Diverter_NotOnHighSide(PushBackRunDirection direction)
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, direction);
            var system = Build(state);
            var runs = PushBackRuns.Resolve(system);
            var total = system.Structure.TotalLength;

            var highX = PushBackRunGeometry.Axes(runs, Catalog).Select(axis => axis.HighContact.X).Distinct().ToList();
            Assert.NotEmpty(highX);

            foreach (var piece in Drawn(system))
            {
                Assert.True(
                    highX.All(x => Math.Abs(piece.X - x) > 12.0),
                    $"corrida {direction}: desviador en el extremo ALTO (X={piece.X:0.##})");
            }

            // Y exactamente un pasillo, no dos.
            Assert.Single(Drawn(system).Select(p => Math.Abs(p.X) < 1.0).Distinct());
        }

        // ===================== 6-7: alturas y niveles asimetricos ============================================

        /// <summary>
        /// Con A y B a alturas DISTINTAS, cada desviador coincide con el <c>LowInsertion</c> de SU cama. Es el caso
        /// que el defecto no podia acertar por coincidencia.
        /// </summary>
        [Fact]
        public void EachDiverter_MatchesTheLowInsertionOfItsOwnRun()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            var system = Build(state);
            var expected = ExpectedFromRuns(system);
            var drawn = Drawn(system);

            Assert.Equal(2, expected.Count);           // un nivel 2 por lado, a alturas distintas
            Assert.Equal(2, expected.Select(e => e.Y).Distinct().Count());
            foreach (var want in expected)
            {
                Assert.Contains(drawn, p => Math.Abs(p.X - want.X) < 1.0 && Math.Abs(p.Y - want.Y) < 1e-6);
            }
        }

        /// <summary>Y con NIVELES asimetricos: cada lado aporta los suyos y ninguno hereda los del otro.</summary>
        [Fact]
        public void AsymmetricLevels_EachSideKeepsItsOwnDiverters()
        {
            var state = State(levels: 2);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideB.AdjustLevels(slot, 2);   // B con dos niveles mas que A
            }

            var system = Build(state);
            foreach (var want in ExpectedFromRuns(system))
            {
                Assert.Contains(Drawn(system), p => Math.Abs(p.X - want.X) < 1.0 && Math.Abs(p.Y - want.Y) < 1e-6);
            }
        }

        // ===================== 8: la MISMA linea, direcciones opuestas por nivel =============================

        /// <summary>
        /// LA PRUEBA QUE NINGUNA CLASIFICACIÓN POR LÍNEA PUEDE PASAR: en la misma línea, el nivel 1 corre A→B y el
        /// nivel 2 B→A. Cada nivel lleva su desviador en SU extremo bajo, y hay exactamente dos piezas.
        /// </summary>
        [Fact]
        public void SameLine_OppositeDirectionsPerLevel_EachLevelUsesItsOwnLow()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCell(0, 1, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            state.SetCell(1, 1, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);

            var system = Build(state);
            var total = system.Structure.TotalLength;
            var drawn = Drawn(system);

            // Dos piezas: el nivel 1 en el arranque y el nivel 2 en el final.
            Assert.Equal(2, drawn.Count);
            Assert.Single(drawn, p => Math.Abs(p.X) < 1.0);
            Assert.Single(drawn, p => Math.Abs(p.X - total) < 1.0);

            foreach (var want in ExpectedFromRuns(system))
            {
                Assert.Contains(drawn, p => Math.Abs(p.X - want.X) < 1.0 && Math.Abs(p.Y - want.Y) < 1e-6);
            }
        }

        // ===================== 9: compuesto parcial ==========================================================

        /// <summary>Con B solo en F1, las lineas de los frentes legacy conservan su desviador tal cual.</summary>
        [Fact]
        public void PartialComposite_LegacyFronts_KeepTheirDiverters()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(4);
            var before = Build(state);

            IReadOnlyList<(double, double, bool)> Cut(PushBackSystem system, int line)
                => new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                    .Where(i => PushBackDiverterPlan.IsDiverter(i, Catalog))
                    .Select(i => (Math.Round(i.Insertion.X, 3), Math.Round(i.Insertion.Y, 4), i.MirroredX))
                    .Distinct()
                    .OrderBy(p => p.Item1).ThenBy(p => p.Item2)
                    .ToList();

            var reference = new[] { 3, 4 }.ToDictionary(line => line, line => Cut(before, line));
            Assert.All(reference.Values, list => Assert.NotEmpty(list));

            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            var after = Build(state);

            foreach (var line in new[] { 3, 4 })
            {
                Assert.Equal(reference[line], Cut(after, line));
            }
        }

        // ===================== 10: lateral y frontal, la misma pieza =========================================

        /// <summary>
        /// El corte LATERAL y el FRONTAL de un lado proyectan la MISMA pieza a la misma elevación. Se comprueba con
        /// los dos lados deliberadamente distintos: con lados iguales pasaría por simetría sin demostrar nada.
        /// </summary>
        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void LateralAndFrontal_AgreeOnTheDiverterElevation(PushBackSide side)
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            var system = Build(state);
            var total = system.Structure.TotalLength;
            var atStart = side == PushBackSide.A;

            var lateral = Drawn(system)
                .Where(p => (Math.Abs(p.X) < 1.0) == atStart)
                .Select(p => p.Y)
                .OrderBy(y => y)
                .ToList();
            var frontal = PushBackCompositeFrontal
                .Build(system, Catalog, PushBackFrontalEnd.EntradaSalida, side)
                .Flatten().Instances
                .Where(i => PushBackDiverterPlan.IsDiverter(i, Catalog))
                .Select(i => Math.Round(i.Insertion.Y, 4))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            Assert.NotEmpty(lateral);
            Assert.Equal(frontal, lateral);
        }
    }
}
