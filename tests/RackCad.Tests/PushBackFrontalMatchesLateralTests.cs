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
    /// I-42 (corrección aislada 1B) — el corte FRONTAL y el LATERAL son dos PROYECCIONES de la misma cama.
    ///
    /// <para>
    /// El lateral es el oráculo: el dueño lo validó. Para una misma <see cref="PushBackRun"/> los tres cortes
    /// —lateral, frontal de entrada/salida y frontal posterior— tienen que mostrar el mismo LOW, el mismo HIGH y las
    /// mismas piezas. La frontal no vuelve a resolver la física: proyecta la que la cama ya resolvió.
    /// </para>
    /// <para>
    /// La cama se identifica por IDENTIDAD —ranura, nivel, lado bajo, lado alto—, nunca por cercanía: la columna de
    /// un frente se calcula con la MISMA fórmula con la que el builder la coloca, y el nivel sale del run.
    /// </para>
    /// </summary>
    public class PushBackFrontalMatchesLateralTests
    {
        private const double Eps = 1e-6;

        /// <summary>Cuánto cuelga el desviador por debajo de su larguero, salvo en el primer nivel.</summary>
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

        // ---- proyecciones ---------------------------------------------------------------------------------------

        private sealed class Piece
        {
            public double X;
            public double Y;
            public string PieceId;
            public double Peralte;
        }

        private static List<Piece> Beams(HeaderRunPlan plan)
            => plan.Flatten().Instances
                .Where(i => i.Role == HeaderBlockRole.Beam)
                .Select(i => new Piece
                {
                    X = Math.Round(i.Insertion.X, 3),
                    Y = Math.Round(i.Insertion.Y, 4),
                    PieceId = i.PieceId,
                    Peralte = i.DynamicParameters.TryGetValue("PERALTE", out var p) ? p : -1.0
                })
                .ToList();

        private static List<Piece> Frontal(PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => Beams(new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side));

        /// <summary>
        /// El LATERAL que el dueño valida: la union de los CORTES por poste, que es lo que la ventana ofrece e
        /// inserta. El plan del sistema entero se incluye tambien, para que ninguna pieza quede fuera del oraculo.
        /// </summary>
        private static List<Piece> Lateral(PushBackSystem system)
        {
            var builder = new PushBackSystemLateralBuilder();
            var pieces = Beams(builder.Build(system, Catalog));
            foreach (var corte in builder.Cortes(system, Catalog))
            {
                pieces.AddRange(Beams(corte.Plan));
            }

            return pieces;
        }

        private static IReadOnlyList<(double X, double Y)> Diverters(HeaderRunPlan plan)
            => plan.Flatten().Instances
                .Where(i => PushBackDiverterPlan.IsDiverter(i, Catalog))
                .Select(i => (Math.Round(i.Insertion.X, 3), Math.Round(i.Insertion.Y, 4)))
                .Distinct()
                .OrderBy(p => p.Item1).ThenBy(p => p.Item2)
                .ToList();

        /// <summary>
        /// La COLUMNA de un frente en su corte frontal: la misma fórmula con la que el builder coloca ahí el
        /// larguero (poste + troquel). Es identidad, no búsqueda por cercanía.
        /// </summary>
        private static double Column(PushBackSystem local, int localFrontIndex)
        {
            var layout = DynamicFrontGeometry.Compute(local.Structure, Catalog);
            return Math.Round(layout.PostPositions[localFrontIndex] + layout.TroquelPositions[localFrontIndex], 3);
        }

        private static int LocalIndex(PushBackSystem system, PushBackSide side, int slot)
        {
            var view = system.Composite.Of(side);
            return slot >= 0 && slot < view.LocalIndexBySlot.Count ? view.LocalIndexBySlot[slot] : -1;
        }

        private static double LowZ(PushBackRun run)
            => PushBackElevations.LowInsertions(run.Source, Catalog, run.Front())[run.SourceLevel];

        private static double HighZ(PushBackRun run)
            => PushBackElevations.HighInsertions(run.Source, Catalog, run.Front())[run.SourceLevel];

        // ---- LA comprobación ------------------------------------------------------------------------------------

        /// <summary>
        /// Para CADA cama: el lateral y los dos frontales muestran el mismo LOW y el mismo HIGH, en la columna de su
        /// ranura, con la pieza y el peralte que le tocan; el lado contrario no inventa un segundo extremo; y no
        /// sobra ningún larguero que ninguna cama pida.
        /// </summary>
        /// <summary>
        /// I-42 (ronda 8B) — EL CORTE donde la cama termina, preguntado a la autoridad de corte. Una corrida acaba
        /// en la cara EXTERIOR de su lado alto; una cama propia, en la interior de su lado.
        /// </summary>
        private static PushBackFrontalEnd? HighCutOf(PushBackSystem system, PushBackRunSet runs, PushBackRun run)
        {
            foreach (var end in new[] { PushBackFrontalEnd.Posterior, PushBackFrontalEnd.EntradaSalida })
            {
                if (PushBackRunSupports.At(system, runs, run, run.HighSide, end) == PushBackSupportRole.High)
                {
                    return end;
                }
            }

            return null;   // la cama termina dentro del rack: ningun plano de corte coincide con su alto
        }

        private static void AssertProjectionsAgree(string label, PushBackSystem system)
        {
            var runs = PushBackRuns.Resolve(system);
            Assert.NotEmpty(runs.Runs);

            var lateral = Lateral(system);
            var frontal = new Dictionary<(PushBackFrontalEnd, PushBackSide), List<Piece>>();
            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
                {
                    frontal[(end, side)] = Frontal(system, end, side);
                }
            }

            var expectedLow = new HashSet<(PushBackSide, double, double)>();
            var expectedHigh = new HashSet<(PushBackSide, double, double)>();
            var expectedHighCuts = new HashSet<(PushBackFrontalEnd, PushBackSide, double, double)>();

            foreach (var run in runs.Runs)
            {
                var lowZ = Math.Round(LowZ(run), 4);
                var highZ = Math.Round(HighZ(run), 4);
                var id = $"{label} cama ranura={run.Slot} nivel={run.Level} {run.LowSide}->{run.HighSide}";

                // 1) El LATERAL, que es el oráculo: la cama tiene su larguero bajo y su larguero alto.
                Assert.True(
                    lateral.Any(p => Math.Abs(p.Y - lowZ) < Eps && p.PieceId == PushBackTestBeams.InOut),
                    $"{id}: el lateral no tiene larguero de entrada en Z={lowZ:0.####}");
                Assert.True(
                    lateral.Any(p => Math.Abs(p.Y - highZ) < Eps && p.PieceId == PushBackTestBeams.Redondo),
                    $"{id}: el lateral no tiene larguero posterior en Z={highZ:0.####}");

                // 2) El FRONTAL de entrada del lado BAJO, en la columna de esta ranura y a la MISMA Z.
                var lowLocal = LocalIndex(system, run.LowSide, run.Slot);
                Assert.True(lowLocal >= 0, $"{id}: el lado bajo no tiene esta ranura");
                var lowColumn = Column(system.Composite.Of(run.LowSide).Local, lowLocal);
                var lowPiece = frontal[(PushBackFrontalEnd.EntradaSalida, run.LowSide)]
                    .FirstOrDefault(p => Math.Abs(p.X - lowColumn) < Eps && Math.Abs(p.Y - lowZ) < Eps);
                Assert.True(
                    lowPiece != null,
                    $"{id}: el frontal de entrada de {run.LowSide} no lo muestra en ({lowColumn:0.##}, {lowZ:0.####})");
                Assert.Equal(PushBackTestBeams.InOut, lowPiece.PieceId);
                Assert.Equal(system.Composite.Of(run.LowSide).Local.Structure.InOutBeamDepth, lowPiece.Peralte, 6);
                expectedLow.Add((run.LowSide, lowColumn, lowZ));

                // 3) El FRONTAL del corte donde la cama TERMINA, con el peralte del larguero alto de ese lado.
                // I-42 (ronda 8B): cual es ese corte lo dice la autoridad de corte, no el nombre de la vista — una
                // corrida termina en la cara EXTERIOR de su lado alto, y una cama propia en la interior.
                var highLocal = LocalIndex(system, run.HighSide, run.Slot);
                Assert.True(highLocal >= 0, $"{id}: el lado alto no tiene esta ranura");
                var highColumn = Column(system.Composite.Of(run.HighSide).Local, highLocal);
                // I-42 (ronda 8B): el alto solo se proyecta en el corte que COINCIDE con el. Una cama que termina
                // dentro del rack —una corrida corta— no tiene su alto en ninguno de los cuatro planos, y ninguno
                // debe inventarlo: el lateral, que si tiene profundidad, ya lo mostro arriba.
                var highEnd = HighCutOf(system, runs, run);
                if (highEnd.HasValue)
                {
                    var highPiece = frontal[(highEnd.Value, run.HighSide)]
                        .FirstOrDefault(p => Math.Abs(p.X - highColumn) < Eps && Math.Abs(p.Y - highZ) < Eps);
                    Assert.True(
                        highPiece != null,
                        $"{id}: el frontal {highEnd} de {run.HighSide} no lo muestra en ({highColumn:0.##}, {highZ:0.####})");
                    Assert.Equal(PushBackTestBeams.Redondo, highPiece.PieceId);
                    Assert.Equal(
                        system.Composite.Of(run.HighSide).Local.HighEndBeamPeralteAt(highLocal, run.Level - 1),
                        highPiece.Peralte,
                        6);
                    expectedHigh.Add((run.HighSide, highColumn, highZ));
                    expectedHighCuts.Add((highEnd.Value, run.HighSide, highColumn, highZ));
                }
                else
                {
                    foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                    {
                        Assert.DoesNotContain(
                            frontal[(end, run.HighSide)],
                            p => p.PieceId == PushBackTestBeams.Redondo
                                 && Math.Abs(p.X - highColumn) < Eps && Math.Abs(p.Y - highZ) < Eps);
                    }
                }

                // 4) El otro lado NO inventa un segundo extremo para esta cama.
                if (run.LowSide != run.HighSide)
                {
                    var other = frontal[(PushBackFrontalEnd.EntradaSalida, run.HighSide)];
                    Assert.DoesNotContain(other, p => Math.Abs(p.Y - lowZ) < Eps && p.PieceId == PushBackTestBeams.InOut);
                }
            }

            // 5) Y nada sobra: cada larguero de cada corte pertenece a una cama.
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                foreach (var piece in frontal[(PushBackFrontalEnd.EntradaSalida, side)]
                    .Where(p => p.PieceId == PushBackTestBeams.InOut))
                {
                    Assert.True(
                        expectedLow.Contains((side, piece.X, piece.Y)),
                        $"{label}: el frontal de entrada de {side} muestra un larguero en ({piece.X:0.##}, {piece.Y:0.####}) que ninguna cama pide");
                }

                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    foreach (var piece in frontal[(end, side)]
                        .Where(p => p.PieceId == PushBackTestBeams.Redondo))
                    {
                        Assert.True(
                            expectedHighCuts.Contains((end, side, piece.X, piece.Y)),
                            $"{label}: el frontal {end} de {side} muestra un larguero alto en ({piece.X:0.##}, {piece.Y:0.####}) que ninguna cama pide");
                    }
                }
            }
        }

        // ---- 1-10: los casos obligatorios -----------------------------------------------------------------------

        /// <summary>1 y 2 — corrida en un sentido y en el otro, un solo nivel.</summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void Corrida_OneLevel_EveryProjectionAgrees(PushBackRunDirection direction)
        {
            var state = State(levels: 1);
            state.SetDefaults(PushBackCellTopology.Corrida, direction);
            AssertProjectionsAgree($"corrida {direction} 1 nivel", Build(state));
        }

        /// <summary>3 — A y B con «Alto 1er nivel» distintos: leer el lado equivocado se ve en la Z.</summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void Corrida_DifferentFirstLevelHeights_EveryProjectionAgrees(PushBackRunDirection direction)
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, direction);
            for (var slot = 0; slot < state.SideA.Structure.Count; slot++)
            {
                state.SideA.Structure.Fronts[slot].FirstLevelHeight = 4.0;
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            AssertProjectionsAgree($"corrida {direction} alturas 4/18", Build(state));
        }

        /// <summary>4 y 5 — distinto número de niveles por lado, y varios niveles.</summary>
        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        public void Corrida_AsymmetricLevelCounts_EveryProjectionAgrees(int deltaB)
        {
            var state = State(levels: 3);
            for (var slot = 0; slot < state.SideB.Structure.Count; slot++)
            {
                state.Of(PushBackSide.B).AdjustLevels(slot, deltaB);
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            AssertProjectionsAgree($"corrida A=3 niveles B={3 + deltaB}", Build(state));
        }

        /// <summary>6 — frentes 2 y 3, no solamente el primero, con topologías distintas por ranura.</summary>
        [Fact]
        public void MixedSlots_EveryProjectionAgrees()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 3; slot++)
            {
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            for (var level = 0; level < 2; level++)
            {
                state.SetCell(1, level, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
                state.SetCell(2, level, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            }

            AssertProjectionsAgree("ranura 0 sólo A, 1 encontradas, 2 corrida B->A", Build(state));
        }

        /// <summary>7 — con calle central: el hueco no es fondo y tampoco desplaza ninguna proyección.</summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void Corrida_WithGap_EveryProjectionAgrees(PushBackRunDirection direction)
        {
            var state = State();
            state.SetGap(48.0);
            state.SetDefaults(PushBackCellTopology.Corrida, direction);
            for (var slot = 0; slot < state.SideB.Structure.Count; slot++)
            {
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            AssertProjectionsAgree($"corrida {direction} con calle 48", Build(state));
        }

        /// <summary>8 — una corrida CORTA dentro de una estructura larga.</summary>
        [Fact]
        public void ShortCorrida_InsideALongStructure_EveryProjectionAgrees()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCorridaDepth(1, 1, 2);
            AssertProjectionsAgree("corrida corta en la ranura 1", Build(state));
        }

        /// <summary>
        /// 9 — la MISMA línea con el nivel 1 corriendo A→B y el 2 B→A. Una clasificación por línea no puede
        /// expresarlo; la cama sí.
        /// </summary>
        [Fact]
        public void SameLine_OppositeDirectionsPerLevel_EveryProjectionAgrees()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideA.Structure.Fronts[slot].FirstLevelHeight = 4.0;
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
                state.SetCell(slot, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
                state.SetCell(slot, 1, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            }

            var system = Build(state);
            AssertProjectionsAgree("N1 A->B / N2 B->A", system);

            // Y explícitamente: cada nivel entra por SU lado, no los dos por el mismo.
            var runs = PushBackRuns.Resolve(system).Runs;
            Assert.Equal(PushBackSide.A, runs.First(run => run.Level == 1).LowSide);
            Assert.Equal(PushBackSide.B, runs.First(run => run.Level == 2).LowSide);
        }

        /// <summary>10 — los cuatro cortes se piden por su SECCIÓN, y cada sección devuelve el suyo.</summary>
        [Fact]
        public void EverySection_AddressesItsOwnCut()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            var system = Build(state);

            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
                {
                    var section = PushBackSystemFrontalBuilder.EncodeSection(end, side);
                    var decoded = PushBackSystemFrontalBuilder.DecodeSection(section);
                    Assert.Equal(end, decoded.End);
                    Assert.Equal(side, decoded.Side);
                }
            }

            // Una corrida A→B carga por A y TERMINA en el pasillo de B. I-42 (ronda 8B): cada corte muestra el
            // apoyo que coincide con SU plano, asi que el bajo esta en la cara exterior de A, el alto en la de B, y
            // las dos lineas interiores llevan el apoyo INTERMEDIO de la cama que las atraviesa.
            var entradaA = Frontal(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A);
            var entradaB = Frontal(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B);
            var posteriorA = Frontal(system, PushBackFrontalEnd.Posterior, PushBackSide.A);
            var posteriorB = Frontal(system, PushBackFrontalEnd.Posterior, PushBackSide.B);

            Assert.All(entradaA, piece => Assert.Equal(PushBackTestBeams.InOut, piece.PieceId));
            Assert.All(entradaB, piece => Assert.Equal(PushBackTestBeams.Redondo, piece.PieceId));
            Assert.NotEmpty(entradaA);
            Assert.NotEmpty(entradaB);

            // Y ninguna de las dos caras interiores muestra un extremo: solo apoyos intermedios.
            Assert.DoesNotContain(posteriorA, piece => piece.PieceId == PushBackTestBeams.InOut
                || piece.PieceId == PushBackTestBeams.Redondo);
            Assert.DoesNotContain(posteriorB, piece => piece.PieceId == PushBackTestBeams.InOut
                || piece.PieceId == PushBackTestBeams.Redondo);
        }

        // ---- el DESVIADOR: la misma cama en las dos vistas ------------------------------------------------------

        /// <summary>
        /// El desviador es un elemento de ENTRADA: existe donde hay una cama que se carga por ese pasillo. En una
        /// corrida A→B el pasillo de B no carga nada, así que no lleva ninguno — el lateral corregido tampoco.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB, PushBackSide.A, PushBackSide.B)]
        [InlineData(PushBackRunDirection.BToA, PushBackSide.B, PushBackSide.A)]
        public void Corrida_TheHighAisle_HasNoDiverter(
            PushBackRunDirection direction, PushBackSide low, PushBackSide high)
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, direction);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideA.Structure.Fronts[slot].FirstLevelHeight = 4.0;
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
            }

            var system = Build(state);
            var lowCut = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, low);
            var highCut = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, high);

            Assert.NotEmpty(Diverters(lowCut));
            Assert.Empty(Diverters(highCut));

            // Y los que sí existen cuelgan del larguero de la cama, igual que en el lateral.
            var runs = PushBackRuns.Resolve(system).Runs.Where(run => run.Level > 1).ToList();
            Assert.NotEmpty(runs);
            foreach (var run in runs)
            {
                var z = Math.Round(LowZ(run) - Offset, 4);
                Assert.Contains(Diverters(lowCut), piece => Math.Abs(piece.Y - z) < Eps);
            }
        }

        /// <summary>
        /// Con las dos direcciones en la misma línea, CADA pasillo lleva el desviador de SU nivel y ninguno el del
        /// otro: es la granularidad por cama que una clasificación por línea no tiene.
        /// </summary>
        [Fact]
        public void OppositeDirectionsPerLevel_EachAisleKeepsItsOwnDiverter()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SideA.Structure.Fronts[slot].FirstLevelHeight = 4.0;
                state.SideB.Structure.Fronts[slot].FirstLevelHeight = 18.0;
                state.SetCell(slot, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
                state.SetCell(slot, 1, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            }

            var system = Build(state);
            var second = PushBackRuns.Resolve(system).Runs.First(run => run.Level == 2);
            var secondZ = Math.Round(LowZ(second) - Offset, 4);

            var fromA = Diverters(new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            var fromB = Diverters(new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));

            // El nivel 2 entra por B: su desviador está en el corte de B y NO en el de A.
            Assert.Contains(fromB, piece => Math.Abs(piece.Y - secondZ) < Eps);
            Assert.DoesNotContain(fromA, piece => Math.Abs(piece.Y - secondZ) < Eps);
        }

        /// <summary>El corte POSTERIOR no lleva desviador: allí no entra ninguna tarima.</summary>
        [Fact]
        public void TheRearCut_NeverCarriesADiverter()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var system = Build(state);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                Assert.Empty(Diverters(new PushBackSystemFrontalBuilder()
                    .BuildPlan(system, Catalog, PushBackFrontalEnd.Posterior, side)));
            }
        }
    }

    /// <summary>Los identificadores de las dos piezas que un corte Push Back distingue por su papel en la cama.</summary>
    internal static class PushBackTestBeams
    {
        public const string InOut = "LARGUERO_IN_OUT_C6";
        public const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";
    }
}
