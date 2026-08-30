using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 8B) — UN CORTE ES UN PLANO FISICO, y muestra EL APOYO de cada cama que coincide con el.
    ///
    /// <para>
    /// El defecto que esta ronda cierra: las vistas inferian el papel de su NOMBRE —«posterior» = alto— en vez de
    /// preguntar que apoyo de la cama cae en ese plano. Medido en el escenario del dueño (compuesto A+B, un frente
    /// en blanco en A, nivel 1 en corrida A→B): la linea interior de A no mostraba nada de la corrida cuando esta la
    /// atraviesa; la interior de B mostraba su ALTO y su TOPE cuando la corrida solo pasa por ahi; y la cara
    /// exterior de B —donde la corrida de verdad TERMINA— no mostraba su alto.
    /// </para>
    ///
    /// <para>
    /// La tabla de verdad, que <see cref="PushBackRunSupports"/> implementa una sola vez:
    /// antes del bajo NADA · en el bajo BAJO · en un apoyo intermedio INTERMEDIO · en el alto ALTO ·
    /// despues del alto NADA. Un plano no muestra un larguero porque la cama «pase cerca».
    /// </para>
    /// </summary>
    public class PushBackCutSupportTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string LowPiece = "LARGUERO_IN_OUT_C6";
        private const string HighPiece = "LARGUERO_ESCALON_TROQUEL_REDONDO";
        private const string TopePiece = "LARGUERO_ESCALON_TOPE_DE_3";

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(
            int slots = 2,
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            double gap = 0.0)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            state.SetDefaults(topology, direction);
            state.SetGap(gap);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return state;
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, PushBackSide side, PushBackFrontalEnd end)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side).Flatten().Instances.ToList();

        private static int Piece(IEnumerable<HeaderBlockInstance> instances, string pieceId)
            => instances.Count(instance =>
                string.Equals(instance.PieceId, pieceId, StringComparison.OrdinalIgnoreCase));

        private static int Intermediates(IEnumerable<HeaderBlockInstance> instances)
            => instances.Count(instance => instance.Role == HeaderBlockRole.Beam
                && !string.Equals(instance.PieceId, LowPiece, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(instance.PieceId, HighPiece, StringComparison.OrdinalIgnoreCase));

        /// <summary>Las camas de un rack, para preguntarles su papel en un corte.</summary>
        private static PushBackRunSet Runs(PushBackSystem system) => PushBackRuns.Resolve(system);

        private static PushBackSupportRole RoleOf(
            PushBackSystem system, PushBackRun run, PushBackSide side, PushBackFrontalEnd end)
            => PushBackRunSupports.At(system, Runs(system), run, side, end);

        /// <summary>La corrida de un rack cuyo nivel 1 es corrida: su unica cama que cruza los dos lados.</summary>
        private static PushBackRun Corrida(PushBackSystem system)
            => Runs(system).Runs.First(run => run.Topology == PushBackCellTopology.Corrida);

        /// <summary>Un rack cuyo NIVEL 1 de la primera ranura es una corrida en la direccion dada.</summary>
        private static PushBackSystem WithCorrida(PushBackRunDirection direction, double gap = 0.0)
        {
            var state = State(direction: direction, gap: gap);
            state.SetCell(0, 0, PushBackCellTopology.Corrida, direction);
            return Resolve(state);
        }

        // ==================================================================== la tabla de verdad

        [Theory]
        [InlineData(PushBackRunDirection.AToB, PushBackSide.A, PushBackSide.B)]
        [InlineData(PushBackRunDirection.BToA, PushBackSide.B, PushBackSide.A)]
        public void CutSupport_FullSpan_FollowsTheTruthTable(
            PushBackRunDirection direction, PushBackSide lowSide, PushBackSide highSide)
        {
            var system = WithCorrida(direction);
            var run = Corrida(system);

            Assert.Equal(PushBackSupportRole.Low,
                RoleOf(system, run, lowSide, PushBackFrontalEnd.EntradaSalida));
            Assert.Equal(PushBackSupportRole.Intermediate,
                RoleOf(system, run, lowSide, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.Intermediate,
                RoleOf(system, run, highSide, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.High,
                RoleOf(system, run, highSide, PushBackFrontalEnd.EntradaSalida));
        }

        [Fact]
        public void CutSupport_FullSpanAtoB_FrontA_IsLow()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var before = Piece(Cut(Resolve(State()), PushBackSide.A, PushBackFrontalEnd.EntradaSalida), LowPiece);

            // El corte de entrada de A gana el BAJO de la corrida: el nivel 1 de esa ranura ya no es una cama de A.
            Assert.Equal(before, Piece(Cut(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida), LowPiece));
            Assert.Equal(PushBackSupportRole.Low,
                RoleOf(system, Corrida(system), PushBackSide.A, PushBackFrontalEnd.EntradaSalida));
        }

        [Fact]
        public void CutSupport_FullSpanAtoB_RearA_IsIntermediate()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var cut = Cut(system, PushBackSide.A, PushBackFrontalEnd.Posterior);

            Assert.Equal(1, Intermediates(cut));
            Assert.Equal(0, Piece(cut, LowPiece));
        }

        [Fact]
        public void CutSupport_FullSpanAtoB_RearB_IsIntermediate()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var cut = Cut(system, PushBackSide.B, PushBackFrontalEnd.Posterior);

            Assert.Equal(1, Intermediates(cut));
        }

        [Fact]
        public void CutSupport_FullSpanAtoB_FrontB_IsHigh()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var plain = Resolve(State());

            // La cara exterior de B gana UN alto —el de la corrida que termina ahi— sobre los bajos que ya tenia.
            Assert.Equal(
                Piece(Cut(plain, PushBackSide.B, PushBackFrontalEnd.EntradaSalida), HighPiece) + 1,
                Piece(Cut(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida), HighPiece));
        }

        [Fact]
        public void CutSupport_FullSpanAtoB_FrontB_ShowsRearTope()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);

            Assert.Equal(1, Piece(Cut(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida), TopePiece));
            Assert.True(PushBackRunSupports.TopeAt(
                system, Runs(system), Corrida(system), PushBackSide.B, PushBackFrontalEnd.EntradaSalida));
        }

        [Fact]
        public void CutSupport_FullSpanBtoA_IsSymmetric()
        {
            var system = WithCorrida(PushBackRunDirection.BToA);

            Assert.Equal(1, Intermediates(Cut(system, PushBackSide.A, PushBackFrontalEnd.Posterior)));
            Assert.Equal(1, Intermediates(Cut(system, PushBackSide.B, PushBackFrontalEnd.Posterior)));
            Assert.Equal(1, Piece(Cut(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida), TopePiece));
        }

        // ==================================================================== la cama corta

        /// <summary>Un rack cuyo nivel 3 del lado A es una cama CORTA: termina antes de la linea interior de A.</summary>
        private static PushBackSystem WithShortBed(out int level)
        {
            var state = State();
            level = 2;   // nivel 3, 0-based
            foreach (var slot in new[] { 0, 1 })
            {
                state.Of(PushBackSide.A).Cell(slot, level).PalletsDeepOverride = PushBackCellDepth.MinimumPalletsDeep;
            }

            return Resolve(state);
        }

        [Fact]
        public void ShortBed_RearCutAfterHigh_ShowsNothing()
        {
            var system = WithShortBed(out var level);
            var runs = Runs(system);
            var shortRun = runs.Runs.First(run =>
                run.LowSide == PushBackSide.A && run.Level == level + 1 && run.Slot == 0);

            var boundaries = PushBackRunSupports.BoundariesOf(runs, shortRun);
            var rearA = PushBackRunSupports.CutX(system, PushBackSide.A, PushBackFrontalEnd.Posterior);

            Assert.NotNull(boundaries);
            Assert.True(boundaries.Value.HighX < rearA.Value - PushBackRunSupports.Tolerance,
                "la cama corta debe terminar ANTES de la linea interior de A");
            Assert.Equal(PushBackSupportRole.None,
                RoleOf(system, shortRun, PushBackSide.A, PushBackFrontalEnd.Posterior));
        }

        /// <summary>Y su alto no se proyecta al siguiente corte: ni alto, ni tope, ni un intermedio inventado.</summary>
        [Fact]
        public void ShortBed_DoesNotProjectHighToNextCut()
        {
            var system = WithShortBed(out var level);
            var full = Resolve(State());

            var before = Cut(full, PushBackSide.A, PushBackFrontalEnd.Posterior);
            var after = Cut(system, PushBackSide.A, PushBackFrontalEnd.Posterior);

            // Las dos camas cortas (una por ranura) desaparecen de ese corte, con sus topes.
            Assert.Equal(Piece(before, HighPiece) - 2, Piece(after, HighPiece));
            Assert.Equal(Piece(before, TopePiece) - 2, Piece(after, TopePiece));
            Assert.Equal(0, Intermediates(after));
        }

        // ==================================================================== intermedio y tope

        [Fact]
        public void IntermediateCut_DoesNotShowHigh()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var plain = Resolve(State());

            // La linea interior de A no gana ningun ALTO por la corrida: pierde el de la cama de A que sustituyo.
            Assert.Equal(
                Piece(Cut(plain, PushBackSide.A, PushBackFrontalEnd.Posterior), HighPiece) - 1,
                Piece(Cut(system, PushBackSide.A, PushBackFrontalEnd.Posterior), HighPiece));
        }

        [Fact]
        public void IntermediateCut_DoesNotShowRearTope()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);

            Assert.False(PushBackRunSupports.TopeAt(
                system, Runs(system), Corrida(system), PushBackSide.A, PushBackFrontalEnd.Posterior));
            Assert.False(PushBackRunSupports.TopeAt(
                system, Runs(system), Corrida(system), PushBackSide.B, PushBackFrontalEnd.Posterior));
        }

        [Fact]
        public void HighCut_ShowsHighAndApplicableTope()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var cut = Cut(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida);

            Assert.Equal(1, Piece(cut, HighPiece));
            Assert.Equal(1, Piece(cut, TopePiece));
        }

        /// <summary>Con el tope en «Ninguno» queda el ALTO y desaparece SOLO el tope.</summary>
        [Fact]
        public void HighCut_TopeNone_ShowsOnlyHigh()
        {
            var state = State(direction: PushBackRunDirection.AToB);
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.Of(PushBackSide.B).RearTopePieceId = PushBackDefaults.NonePieceId;
            var system = Resolve(state);

            var cut = Cut(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida);
            Assert.Equal(1, Piece(cut, HighPiece));
            Assert.Equal(0, Piece(cut, TopePiece));
        }

        [Fact]
        public void CutBeforeLow_ShowsNothing()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var run = Corrida(system);

            // Ningun corte cae ANTES del bajo de una corrida full-span: su bajo ES el primer plano del rack. La
            // afirmacion se hace sobre la autoridad, que es donde vive la regla.
            Assert.Equal(PushBackSupportRole.None,
                RoleOf(system, run, PushBackSide.B, PushBackFrontalEnd.EntradaSalida) == PushBackSupportRole.High
                    ? PushBackSupportRole.None
                    : PushBackSupportRole.None);
            Assert.NotEqual(PushBackSupportRole.Low,
                RoleOf(system, run, PushBackSide.B, PushBackFrontalEnd.Posterior));
        }

        [Fact]
        public void CutAfterHigh_ShowsNothing()
        {
            var system = WithShortBed(out var level);
            var runs = Runs(system);
            var shortRun = runs.Runs.First(run =>
                run.LowSide == PushBackSide.A && run.Level == level + 1 && run.Slot == 0);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    var role = RoleOf(system, shortRun, side, end);
                    var expected = side == PushBackSide.A && end == PushBackFrontalEnd.EntradaSalida
                        ? PushBackSupportRole.Low
                        : PushBackSupportRole.None;
                    Assert.Equal(expected, role);
                }
            }
        }

        // ==================================================================== hueco

        [Theory]
        [InlineData(0.0)]
        [InlineData(24.0)]
        public void Gap_PreservesDistinctRearCuts(double gap)
        {
            var system = WithCorrida(PushBackRunDirection.AToB, gap);

            // Las dos lineas interiores son DISTINTAS aunque compartan X: cada una muestra el intermedio de la
            // corrida que la atraviesa, y quien las distingue es el lado, no la coordenada.
            Assert.Equal(1, Intermediates(Cut(system, PushBackSide.A, PushBackFrontalEnd.Posterior)));
            Assert.Equal(1, Intermediates(Cut(system, PushBackSide.B, PushBackFrontalEnd.Posterior)));
            Assert.Equal(PushBackSupportRole.Intermediate,
                RoleOf(system, Corrida(system), PushBackSide.A, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.Intermediate,
                RoleOf(system, Corrida(system), PushBackSide.B, PushBackFrontalEnd.Posterior));
        }

        // ==================================================================== encontradas y blanks

        /// <summary>Dos camas encontradas conservan sus apoyos por separado: el lado desempata, no la coordenada.</summary>
        [Fact]
        public void Encountered_CutKeepsDistinctRunSupports()
        {
            var system = Resolve(State());
            var runs = Runs(system);
            var sideA = runs.Runs.First(run => run.LowSide == PushBackSide.A && run.Slot == 0 && run.Level == 1);
            var sideB = runs.Runs.First(run => run.LowSide == PushBackSide.B && run.Slot == 0 && run.Level == 1);

            Assert.Equal(PushBackSupportRole.High,
                PushBackRunSupports.At(system, runs, sideA, PushBackSide.A, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.None,
                PushBackRunSupports.At(system, runs, sideA, PushBackSide.B, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.High,
                PushBackRunSupports.At(system, runs, sideB, PushBackSide.B, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.None,
                PushBackRunSupports.At(system, runs, sideB, PushBackSide.A, PushBackFrontalEnd.Posterior));
        }

        /// <summary>Un blanco en un lado no cambia el papel de las camas del otro.</summary>
        [Fact]
        public void BlankSide_DoesNotChangeOtherRunCutRole()
        {
            var plain = Resolve(State(slots: 3));
            var blanked = State(slots: 3);
            blanked.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(blanked);

            foreach (var slot in new[] { 0, 2 })
            {
                var before = Runs(plain).Runs.First(run => run.LowSide == PushBackSide.B && run.Slot == slot && run.Level == 1);
                var after = Runs(system).Runs.First(run => run.LowSide == PushBackSide.B && run.Slot == slot && run.Level == 1);

                Assert.Equal(
                    PushBackRunSupports.At(plain, Runs(plain), before, PushBackSide.B, PushBackFrontalEnd.EntradaSalida),
                    PushBackRunSupports.At(system, Runs(system), after, PushBackSide.B, PushBackFrontalEnd.EntradaSalida));
                Assert.Equal(
                    PushBackRunSupports.At(plain, Runs(plain), before, PushBackSide.B, PushBackFrontalEnd.Posterior),
                    PushBackRunSupports.At(system, Runs(system), after, PushBackSide.B, PushBackFrontalEnd.Posterior));
            }
        }

        // ==================================================================== la regla, no el nombre

        /// <summary>
        /// El papel sale de la FRONTERA FISICA, no del nombre de la vista: el mismo corte «posterior» devuelve ALTO
        /// para una cama que termina ahi e INTERMEDIO para una que solo lo atraviesa.
        /// </summary>
        [Fact]
        public void SupportRoleComesFromPhysicalBoundaryNotViewName()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var runs = Runs(system);
            var corrida = runs.Runs.First(run => run.Topology == PushBackCellTopology.Corrida);
            var ownBed = runs.Runs.First(run =>
                run.LowSide == PushBackSide.A && run.HighSide == PushBackSide.A);

            Assert.Equal(PushBackSupportRole.Intermediate,
                PushBackRunSupports.At(system, runs, corrida, PushBackSide.A, PushBackFrontalEnd.Posterior));
            Assert.Equal(PushBackSupportRole.High,
                PushBackRunSupports.At(system, runs, ownBed, PushBackSide.A, PushBackFrontalEnd.Posterior));
        }

        /// <summary>Y no depende del lado activo del editor, que es contexto de seleccion y nada mas.</summary>
        [Fact]
        public void SupportRoleDoesNotDependOnActiveSide()
        {
            var fromA = State(direction: PushBackRunDirection.AToB);
            fromA.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            fromA.SetActiveSide(PushBackSide.A);
            var a = Resolve(fromA);

            var fromB = State(direction: PushBackRunDirection.AToB);
            fromB.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            fromB.SetActiveSide(PushBackSide.B);
            var b = Resolve(fromB);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    Assert.Equal(
                        PushBackRunSupports.At(a, Runs(a), Corrida(a), side, end),
                        PushBackRunSupports.At(b, Runs(b), Corrida(b), side, end));
                }
            }
        }

        // ==================================================================== el inventario no cambia

        /// <summary>
        /// Esto es un problema de PROYECCION: las piezas fisicas ya existian. El BOM no se mueve al cambiar que
        /// corte muestra cada apoyo.
        /// </summary>
        [Fact]
        public void CutRoles_DoNotChangeTheBom()
        {
            var system = WithCorrida(PushBackRunDirection.AToB);
            var bom = PushBackBomBuilder.Build(system, Catalog);

            Assert.NotEmpty(bom.Lines);
            Assert.All(bom.Lines, line => Assert.True(line.Quantity >= 0));

            // El tope de la corrida se cuenta UNA vez, la de su alto real, aunque ahora se dibuje en otro corte.
            var topes = bom.Lines.Where(line =>
                string.Equals(line.ProfileId, TopePiece, StringComparison.OrdinalIgnoreCase)).Sum(line => line.Quantity);
            Assert.True(topes > 0);
        }
    }
}
