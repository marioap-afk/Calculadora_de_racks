using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (correcciones aisladas 5B y 5C) — LA MANO DEL LARGUERO ALTO.
    ///
    /// <para>
    /// Regla del dueño, que SUSTITUYE a la de la ronda 5 («último / primer / interior poste», retirada): el larguero
    /// HIGH —el de salida— debe llevar <b>exactamente la orientación que tendría un larguero INTERMEDIO colocado en
    /// esa misma posición física</b>. El programa ya orienta bien los intermedios; aquí no se inventa nada, se
    /// consume esa misma autoridad.
    /// </para>
    /// <para>
    /// 5C añade la otra mitad: esa mano se decide UNA vez y la PLANTA la transporta. Ninguna vista la recalcula.
    /// </para>
    /// <para>
    /// Y una separación que el dueño cerró explícitamente: la mano del tope sale de su larguero, pero su POSICIÓN es
    /// la que quedó validada en la ronda 4B. Corregir la orientación no puede mover la pieza.
    /// </para>
    /// </summary>
    public class PushBackHighEndHandTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>
        /// Cuanto puede separarse el eje dibujado de un apoyo intermedio de la frontera de modulos que lo genera: en
        /// una frontera vano-vano el apoyo se traza en el eje del poste DERIVADO, unas pulgadas antes del limite. Los
        /// modulos miden decenas de pulgadas, asi que no hay ambiguedad.
        /// </summary>
        private const double BoundaryWindow = 6.0;

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

        private static PushBackSystem Composite(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots = 2)
        {
            var state = State(slots);
            state.SetDefaults(topology, direction);
            return Build(state);
        }

        /// <summary>
        /// EL ESCENARIO DEL DUEÑO: una estructura de OCHO fondos con una cama por nivel, de 3 a 8 fondos de
        /// profundidad. Es el que hizo visible el defecto — las camas de 3 y 6 salían bien y las de 4, 5, 7 y 8 mal.
        /// </summary>
        private static PushBackSystem DepthLadder()
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = 8;
            state.SetFrontCount(1);
            state.Structure.Fronts[0].LoadLevels = 6;
            state.Structure.Fronts[0].PalletsDeep = 8;
            state.AdjustLevels(0, 0);
            for (var level = 0; level < 6; level++)
            {
                state.ToggleCell(0, level, false);
                state.ApplyPalletsDeep(level + 3, DynamicRackCellScope.Cell);
            }

            var computation = new PushBackEditorDesignAssembler(Catalog).Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation.System;
        }

        // ---- lecturas físicas --------------------------------------------------------------------------------

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        private static IReadOnlyList<HeaderBlockInstance> Lateral(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances;

        /// <summary>
        /// El corte lateral en un poste. Es el que hay que leer cuando las celdas tienen fondos DISTINTOS: el lateral
        /// sin seccionar no tiene celda a la que preguntar y dibuja el rack entero (I-41), asi que ahi todas las camas
        /// acaban en el fondo de la estructura.
        /// </summary>
        private static IReadOnlyList<HeaderBlockInstance> LateralCut(PushBackSystem system, int postIndex)
            => new PushBackSystemLateralBuilder().Build(system, Catalog, postIndex).Flatten().Instances;

        /// <summary>Todos los cortes laterales del rack: entre ellos aparece cada cama, tenga el fondo que tenga.</summary>
        private static IReadOnlyList<HeaderBlockInstance> LateralCuts(PushBackSystem system)
            => Enumerable.Range(0, (system.Structure?.Fronts?.Count ?? 0) + 1)
                .SelectMany(postIndex => LateralCut(system, postIndex))
                .ToList();

        private static IReadOnlyList<HeaderBlockInstance> Planta(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances;

        private static IEnumerable<HeaderBlockInstance> HighBeams(
            IEnumerable<HeaderBlockInstance> source, PushBackSystem system)
        {
            var highId = HighBeamId(system);
            return source.Where(instance => instance.Role == HeaderBlockRole.Beam
                && string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<HeaderBlockInstance> Topes(IEnumerable<HeaderBlockInstance> source)
            => source.Where(instance => instance.Role == HeaderBlockRole.Tope);

        /// <summary>Las manos físicas de una vista: (X redondeada, espejo), sin repetir.</summary>
        private static IReadOnlyList<(double X, bool Mirrored)> Hands(IEnumerable<HeaderBlockInstance> source)
            => source.Select(instance => (X: Math.Round(instance.Insertion.X, 3), Mirrored: instance.MirroredX))
                .Distinct()
                .OrderBy(entry => entry.X)
                .ToList();

        /// <summary>
        /// LA REFERENCIA INDEPENDIENTE: los apoyos INTERMEDIOS que el builder dinámico coloca en esta misma
        /// estructura, con la mano que él les da. No se reproduce ninguna regla — se lee lo que el programa ya hace.
        /// </summary>
        private static IReadOnlyList<DynamicIntermediateBeamSupport> IntermediateSupports(DynamicRackSystem structure)
        {
            var postId = structure.Modules
                .FirstOrDefault(module => module.IsHeader && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
            var finPoste = CatalogLookup.Local(Catalog, postId, "FIN_POSTE", DynamicRackDefaults.IntermediateBeamView);
            return DynamicIntermediateBeamGeometry.Supports(structure, finPoste);
        }

        // ---- 5B: la mano del alto ES la del intermedio en esa misma posición -----------------------------------

        /// <summary>
        /// LA PRUEBA CENTRAL, sobre el escenario que el dueño midió: en una estructura de 8 fondos, la cama de
        /// <paramref name="depth"/> fondos acaba en una frontera de módulos, y su larguero alto debe llevar la mano
        /// que el builder dinámico le da a SU apoyo intermedio en esa misma X.
        ///
        /// <para>
        /// La referencia sale de <see cref="DynamicIntermediateBeamGeometry.Supports"/>, es decir de las piezas que
        /// el programa coloca de verdad, no de una regla reescrita aquí. Antes de 5B fallaban 4, 5, 7 y 8 —el alto
        /// llevaba «siempre espejado», la constante del marco de una cama— y coincidían 3 y 6 por casualidad.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void HighBeam_UsesSameHandAsIntermediateAtSamePhysicalPosition(int depth)
        {
            var system = DepthLadder();
            var level = depth - 2;                                   // nivel 1 = 3 fondos … nivel 6 = 8 fondos
            Assert.Equal(depth, system.EffectivePalletsDeepAt(0, level - 1));

            var rearX = PushBackCellDepth.RearX(system, system.Structure.Fronts[0], level);
            var beam = HighBeams(LateralCut(system, 0), system)
                .Where(instance => Math.Abs(instance.Insertion.X - rearX) < 1.0)
                .Select(instance => instance.MirroredX)
                .Distinct()
                .ToList();
            Assert.Single(beam);

            var supports = IntermediateSupports(system.Structure)
                .Where(support => Math.Abs(support.PostAxisX - rearX) < BoundaryWindow)
                .Select(support => support.Mirrored)
                .Distinct()
                .ToList();

            if (supports.Count == 1)
            {
                Assert.True(
                    beam[0] == supports[0],
                    $"cama de {depth} fondos (X={rearX:0.###}): el larguero alto va "
                        + $"{(beam[0] ? "espejado" : "normal")} y un intermedio ahí iría "
                        + $"{(supports[0] ? "espejado" : "normal")}");
                return;
            }

            // El extremo del sistema no recibe apoyo intermedio —lo ocupa el larguero de extremo—, así que ahí la
            // referencia es el módulo que TERMINA en esa X, que es de donde el builder saca la mano del intermedio.
            var last = system.Structure.Modules.Last(module => module.Length > 0.0);
            Assert.True(Math.Abs(last.EndX - rearX) < 1.0, $"la cama de {depth} no acaba en el extremo ni en un apoyo");
            Assert.Equal(DynamicIntermediateBeamGeometry.HandAtBoundary(last), beam[0]);
        }

        /// <summary>
        /// La misma tabla, leída de una vez: ninguna de las seis camas discrepa de su intermedio. Es la forma en la
        /// que el dueño la reportó —una fila por cama— y falla en cuanto una sola vuelve a la regla anterior.
        /// </summary>
        [Fact]
        public void DepthLadder_EveryBedAgreesWithItsIntermediate()
        {
            var system = DepthLadder();
            var supports = IntermediateSupports(system.Structure);
            var wrong = new List<string>();
            for (var level = 1; level <= 6; level++)
            {
                var rearX = PushBackCellDepth.RearX(system, system.Structure.Fronts[0], level);
                var expected = supports
                    .Where(support => Math.Abs(support.PostAxisX - rearX) < BoundaryWindow)
                    .Select(support => (bool?)support.Mirrored)
                    .FirstOrDefault()
                    ?? DynamicIntermediateBeamGeometry.HandAtDepthX(system.Structure, rearX);
                foreach (var beam in HighBeams(LateralCut(system, 0), system)
                             .Where(instance => Math.Abs(instance.Insertion.X - rearX) < 1.0))
                {
                    if (expected.HasValue && beam.MirroredX != expected.Value)
                    {
                        wrong.Add($"cama de {level + 2} fondos en X={rearX:0.###}: alto {beam.MirroredX}, intermedio {expected.Value}");
                    }
                }
            }

            Assert.True(wrong.Count == 0, "camas con la mano equivocada:\n" + string.Join("\n", wrong));
        }

        // ---- 5C: la planta TRANSPORTA esa mano, no la recalcula ------------------------------------------------

        public static IEnumerable<object[]> Topologies() => new[]
        {
            new object[] { PushBackCellTopology.SoloA, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.SoloB, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Encontradas, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.BToA }
        };

        /// <summary>
        /// 5C — la MISMA pieza física no puede llevar una mano en el corte lateral y otra en la planta. La planta
        /// sustituía el larguero conservando el espejo del builder dinámico, que en el marco de una cama es un
        /// «alto siempre espejado» fijo y nunca pasaba por la autoridad.
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void CompositePlan_HighPhysicalHand_MatchesLateralForEveryRun(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Composite(topology, direction);
            var lateral = Hands(HighBeams(Lateral(system), system));
            var planta = Hands(HighBeams(Planta(system), system));

            Assert.NotEmpty(lateral);
            foreach (var beam in lateral)
            {
                Assert.True(
                    planta.Any(other => Math.Abs(other.X - beam.X) < 1e-6 && other.Mirrored == beam.Mirrored),
                    $"{topology}/{direction}: el larguero alto en X={beam.X:0.###} va "
                        + $"{(beam.Mirrored ? "espejado" : "normal")} en el lateral y al revés en la planta");
            }
        }

        /// <summary>
        /// Camas ENCONTRADAS: son DOS camas físicas, dos largueros altos y dos topes, y topan en la misma frontera.
        /// Desde que la mano sale de la POSICIÓN y no del sentido, las dos piezas coinciden también en el espejo —
        /// así que la deduplicación de proyección tiene que distinguirlas por su cama, no colapsarlas en una.
        /// </summary>
        [Fact]
        public void Encontradas_KeepTwoPhysicalHighBeams_AfterProjectionDedupe()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 1);
            var beams = HighBeams(Planta(system), system).ToList();
            var topes = Topes(Planta(system)).ToList();

            Assert.Equal(2, beams.Count);
            Assert.Equal(2, topes.Count);
        }

        // ---- la separación que el dueño cerró: mano SÍ, posición NO --------------------------------------------

        /// <summary>
        /// LA PRUEBA FUERTE DE LA CORRIDA CORTA. Las cuatro condiciones a la vez:
        /// <list type="number">
        /// <item>la mano del alto es la del intermedio en esa misma frontera;</item>
        /// <item>la planta lleva la misma mano que el lateral;</item>
        /// <item>el tope va con la mano contraria a la de SU larguero;</item>
        /// <item>y la X del tope es EXACTAMENTE la que la ronda 4B cerró — corregir la mano no mueve la pieza.</item>
        /// </list>
        /// </summary>
        [Fact]
        public void ShortCorrida_HandFollowsTheIntermediate_AndTheTopeKeepsIts4BPosition()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCorridaDepth(1, 0, 2);
            var system = Build(state);

            var resolved = PushBackRuns.Resolve(system);
            var runs = resolved.Runs
                .Select(run => new
                {
                    Run = run,
                    Axis = PushBackRunGeometry.Axis(run, Catalog, resolved.MirrorAxis),
                    Hand = DynamicIntermediateBeamGeometry.HandAtDepthX(
                        run.Source.Structure,
                        PushBackCellDepth.RearX(run.Source, run.Front(), run.SourceLevel))
                })
                .Where(entry => entry.Axis.HasValue && entry.Hand.HasValue)
                .ToList();
            Assert.NotEmpty(runs);

            // La corrida CORTA no aparece en el lateral sin seccionar —ahi no hay celda a la que preguntar y el
            // rack se dibuja entero (I-41)—, asi que se leen los cortes, que es donde cada cama sale con su fondo.
            var lateral = LateralCuts(system);
            var planta = Planta(system);
            foreach (var entry in runs)
            {
                var x = entry.Axis.Value.HighContact.X;
                var expected = entry.Run.Reflected ? !entry.Hand.Value : entry.Hand.Value;

                // (1) la mano del alto sale de la autoridad del intermedio…
                var lateralBeams = HighBeams(lateral, system)
                    .Where(instance => Math.Abs(instance.Insertion.X - x) < 12.0).ToList();
                Assert.NotEmpty(lateralBeams);
                Assert.Contains(lateralBeams, beam => beam.MirroredX == expected);

                // (2) …y la planta la transporta sin recalcularla.
                var plantaBeams = HighBeams(planta, system)
                    .Where(instance => Math.Abs(instance.Insertion.X - x) < 12.0).ToList();
                Assert.NotEmpty(plantaBeams);
                Assert.All(plantaBeams, beam => Assert.Equal(expected, beam.MirroredX));

                // (3) el tope va con la mano contraria a la de su larguero, en las dos vistas.
                foreach (var tope in Topes(planta).Where(instance => Math.Abs(instance.Insertion.X - x) < 12.0))
                {
                    Assert.Equal(!expected, tope.MirroredX);
                }
            }

            // (4) LA POSICIÓN NO SE MUEVE. Estas X son las que la ronda 4B midió y el dueño validó: la corrida corta
            // acaba en X=101.845 y su tope queda en 101.125, en el lateral y en la planta.
            var shortHigh = runs
                .Select(entry => entry.Axis.Value.HighContact.X)
                .Where(value => value > 5.0 && value < system.Structure.TotalLength - 5.0)
                .Distinct()
                .ToList();
            Assert.NotEmpty(shortHigh);
            foreach (var x in shortHigh)
            {
                var lateralX = Topes(lateral).Select(tope => tope.Insertion.X)
                    .Where(value => Math.Abs(value - x) < 12.0).Distinct().ToList();
                var plantaX = Topes(planta).Select(tope => tope.Insertion.X)
                    .Where(value => Math.Abs(value - x) < 12.0).Distinct().ToList();
                Assert.Single(lateralX);
                Assert.Single(plantaX);
                Assert.Equal(lateralX[0], plantaX[0], 6);
                Assert.Equal(101.125, lateralX[0], 3);
            }
        }

        /// <summary>El BOM no depende de la mano: misma pieza, mismas cantidades.</summary>
        [Fact]
        public void Hand_DoesNotChangeTheBom()
        {
            foreach (var system in new[]
            {
                Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB),
                Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA),
                DepthLadder()
            })
            {
                var topes = PushBackBomBuilder.Build(system, Catalog).Components
                    .Where(component => component.Category == PushBackBomBuilder.RearTope)
                    .ToList();
                Assert.NotEmpty(topes);
                Assert.Single(topes.Select(component => component.ProfileId).Distinct());
            }
        }
    }
}
