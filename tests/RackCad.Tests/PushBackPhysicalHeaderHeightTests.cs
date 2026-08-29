using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 6C) — LA ALTURA DE CABECERA SALE DE LAS CAMAS REALES.
    ///
    /// <para>
    /// La ronda 6B dejó una sola autoridad consumida por el lateral, los dos frontales y el BOM. Esta ronda corrige
    /// su INPUT: la estructura compuesta se resuelve con una profundidad SINTÉTICA (A + hueco + B invertido) que
    /// ninguna cama recorre. Medido en unas encontradas de 5+5 fondos: la entrada del nivel alto pasaba de 86.6053
    /// —lo que un rack simple de cinco fondos resuelve— a 96.6053, y la cabecera de 120" a 132". Un pie comercial de
    /// más por una cama que no existe.
    /// </para>
    /// <para>
    /// La regla de cabecera NO cambia: entrada del último nivel + peralte de su larguero + un tercio del espacio
    /// libre, redondeado al pie comercial. Cambia de dónde sale la elevación.
    /// </para>
    /// </summary>
    public class PushBackPhysicalHeaderHeightTests
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

        private static PushBackSystem SingleSided(int levels, params int[] depths)
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = depths[0];
            state.SetFrontCount(depths.Length);
            for (var index = 0; index < depths.Length; index++)
            {
                state.Structure.Fronts[index].LoadLevels = levels;
                state.Structure.Fronts[index].PalletsDeep = depths[index];
                state.AdjustLevels(index, 0);
            }

            var computation = new PushBackEditorDesignAssembler(Catalog).Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation.System;
        }

        private static PushBackCompositeEditorState State(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots,
            int levelsA = 2, int levelsB = 2, int deepA = 5, int deepB = 5,
            int? blankSlotA = null, int? blankSlotB = null)
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
                var levels = side == PushBackSide.A ? levelsA : levelsB;
                var deep = side == PushBackSide.A ? deepA : deepB;
                var editor = state.Of(side);
                for (var index = 0; index < editor.Structure.Count; index++)
                {
                    editor.AdjustLevels(index, levels - editor.Structure.Fronts[index].LoadLevels);
                    editor.Structure.Fronts[index].PalletsDeep = deep;
                }
            }

            state.SetDefaults(topology, direction);
            if (blankSlotA.HasValue) { Assert.True(state.SetSlotPresent(PushBackSide.A, blankSlotA.Value, false)); }
            if (blankSlotB.HasValue) { Assert.True(state.SetSlotPresent(PushBackSide.B, blankSlotB.Value, false)); }
            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state, PushBackEditorInputs inputs = null)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs ?? Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        private static PushBackSystem Composite(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots,
            int levelsA = 2, int levelsB = 2, int deepA = 5, int deepB = 5,
            int? blankSlotA = null, int? blankSlotB = null)
            => Build(State(topology, direction, slots, levelsA, levelsB, deepA, deepB, blankSlotA, blankSlotB));

        // ---- lecturas -----------------------------------------------------------------------------------------

        private static double LateralHeight(PushBackSystem system, int line)
        {
            var heights = new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3))
                .Distinct().ToList();
            Assert.Single(heights);
            return heights[0];
        }

        private static IReadOnlyList<double> BomHeaderLengths(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category.IndexOf("abecera", StringComparison.Ordinal) >= 0)
                .Select(component => Math.Round(component.Length, 3))
                .Distinct().OrderBy(value => value).ToList();

        /// <summary>La altura SINTÉTICA que la estructura compuesta resolvería por su propia profundidad.</summary>
        private static double SyntheticHeight(PushBackSystem system)
            => DynamicHeaderHeightCalculator.CalculateResolved(system.Structure.Fronts[0]).HeaderHeight;

        /// <summary>El máximo de las demandas de las camas reales: LA autoridad de esta ronda.</summary>
        private static double PhysicalRequirement(PushBackSystem system)
            => PushBackRuns.Resolve(system).Runs
                .Select(run => PushBackHeaderHeight.Requirement(run, Catalog))
                .DefaultIfEmpty(0.0).Max();

        // ---- las pruebas --------------------------------------------------------------------------------------

        /// <summary>
        /// Unas ENCONTRADAS son dos camas de cinco fondos, no una de once: su cabecera es la que el rack simple de
        /// cinco fondos resuelve, y NO la que sale de sumar los dos lados.
        /// </summary>
        [Fact]
        public void EncounteredHeader_UsesMaxRealRunRequirement_NotCombinedDepth()
        {
            var encontradas = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
            var simple = SingleSided(2, 5);

            Assert.Equal(120.0, LateralHeight(simple, 0), 6);
            Assert.Equal(120.0, LateralHeight(encontradas, 1), 6);
            Assert.Equal(PhysicalRequirement(encontradas), LateralHeight(encontradas, 1), 6);

            // Y la altura sintética de la estructura compuesta, que era la que se usaba, es MAYOR: la prueba no es
            // vacía y mide exactamente el defecto.
            Assert.Equal(132.0, SyntheticHeight(encontradas), 6);
        }

        /// <summary>
        /// Profundidades distintas por lado no crean una cama sintética: manda la cama más exigente de las dos, no
        /// su suma. Con A de 8 fondos y B de 4, la cabecera es la de un rack simple de 8 — no la de uno de 12.
        /// </summary>
        [Fact]
        public void DifferentSideDepths_DoNotCreateSyntheticBedHeight()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, deepA: 8, deepB: 4);

            Assert.Equal(LateralHeight(SingleSided(2, 8), 0), LateralHeight(system, 1), 6);
            Assert.Equal(PhysicalRequirement(system), LateralHeight(system, 1), 6);
            Assert.True(SyntheticHeight(system) > LateralHeight(system, 1), "la altura sintética ya no gobierna");
        }

        /// <summary>
        /// Niveles distintos por lado: la línea COMPARTIDA resuelve el máximo de las demandas físicas reales. Con A
        /// de tres niveles y B de dos, la cabecera es la que un rack simple de tres niveles necesita.
        /// </summary>
        [Fact]
        public void DifferentSideLevels_UseMaxPhysicalRequirementAtSharedLine()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 3);

            Assert.Equal(192.0, LateralHeight(system, 1), 6);
            Assert.Equal(LateralHeight(SingleSided(3, 5), 0), LateralHeight(system, 1), 6);
            Assert.Equal(PhysicalRequirement(system), LateralHeight(system, 1), 6);
            Assert.Equal(204.0, SyntheticHeight(system), 6);   // lo que se resolvía antes
        }

        /// <summary>
        /// Un lado EN BLANCO no aporta demanda: no tiene cama. Subir sus niveles no puede subir una cabecera, y una
        /// ranura en blanco por los DOS lados toma su altura de la línea vecina que sí carga.
        /// </summary>
        [Fact]
        public void BlankSide_DoesNotContributeHeaderHeightDemand()
        {
            var tallBlank = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, levelsA: 3, blankSlotA: 0);
            var plainBlank = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, levelsA: 2, blankSlotA: 0);

            // La línea 0 no la carga el lado A (su ranura está en blanco): su altura no cambia con los niveles de A.
            Assert.Equal(LateralHeight(plainBlank, 0), LateralHeight(tallBlank, 0), 6);

            // Y una ranura en blanco por los DOS lados no aporta demanda propia: su línea vive de la vecina.
            var bothBlank = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotA: 0, blankSlotB: 0);
            Assert.Equal(3, bothBlank.Structure.Fronts.Count);           // la ranura sigue existiendo
            Assert.Equal(0.0, bothBlank.Structure.Fronts[0].Height, 6);  // pero sin demanda propia
            Assert.True(LateralHeight(bothBlank, 0) > 0.0);              // y su línea se dibuja igual
            Assert.Equal(LateralHeight(bothBlank, 1), LateralHeight(bothBlank, 0), 6);
        }

        /// <summary>
        /// Una CORRIDA sí atraviesa los dos lados: su demanda sale de SU propia cama, la que de verdad recorre esa
        /// longitud. Y no es la de la profundidad sintética de la estructura.
        /// </summary>
        [Fact]
        public void ContinuousRun_UsesItsOwnResolvedPhysicalLength()
        {
            foreach (var direction in new[] { PushBackRunDirection.AToB, PushBackRunDirection.BToA })
            {
                var corrida = Composite(PushBackCellTopology.Corrida, direction, 2);
                var runs = PushBackRuns.Resolve(corrida).Runs;

                Assert.NotEmpty(runs);
                // Es UNA cama por celda, que empieza en un lado y acaba en el otro.
                Assert.All(runs, run => Assert.NotEqual(run.LowSide, run.HighSide));
                Assert.Equal(PhysicalRequirement(corrida), LateralHeight(corrida, 1), 6);
                Assert.True(SyntheticHeight(corrida) > LateralHeight(corrida, 1));
            }
        }

        /// <summary>
        /// F) Un frente profundo REMOTO no entra en la altura de una cabecera ajena: sigue valiendo la envolvente
        /// local por línea que la ronda 6B fijó.
        /// </summary>
        [Fact]
        public void RemoteFront_DoesNotLeakIntoHeaderHeight()
        {
            var withRemote = SingleSided(2, 5, 8, 6, 9);
            var without = SingleSided(2, 5, 8, 6);

            for (var line = 0; line <= 2; line++)
            {
                Assert.Equal(LateralHeight(without, line), LateralHeight(withRemote, line), 6);
            }

            Assert.Equal(new[] { 120.0, 120.0, 120.0, 132.0, 132.0 },
                Enumerable.Range(0, 5).Select(line => LateralHeight(withRemote, line)).ToArray());
        }

        /// <summary>
        /// A) UN PUSH BACK SIMPLE NO CAMBIA. Los valores son los de siempre, fijados aquí para que ninguna ronda
        /// futura los mueva por accidente.
        /// </summary>
        [Fact]
        public void SimplePushBack_HeaderHeightUnchanged()
        {
            Assert.Equal(120.0, LateralHeight(SingleSided(2, 5), 0), 6);
            Assert.Equal(120.0, LateralHeight(SingleSided(2, 5, 5), 1), 6);
            Assert.Equal(new[] { 120.0 }, BomHeaderLengths(SingleSided(2, 5, 5)));
            Assert.Equal(new[] { 120.0, 132.0 }, BomHeaderLengths(SingleSided(2, 5, 8, 6, 9)));
        }

        /// <summary>
        /// La conquista de 6B se conserva: lateral, frontal entrada, frontal posterior y BOM leen la MISMA altura
        /// resuelta — ahora la físicamente correcta.
        /// </summary>
        public static IEnumerable<object[]> Scenarios() => new[]
        {
            new object[] { "simple d5" },
            new object[] { "simple 5/8/6/9" },
            new object[] { "solo A" },
            new object[] { "solo B" },
            new object[] { "encontradas d5/d5" },
            new object[] { "encontradas d8/d4" },
            new object[] { "encontradas niv 3/2" },
            new object[] { "corrida A->B" },
            new object[] { "corrida B->A" },
            new object[] { "blank A" }
        };

        private static PushBackSystem Scenario(string label)
        {
            switch (label)
            {
                case "simple d5": return SingleSided(2, 5);
                case "simple 5/8/6/9": return SingleSided(2, 5, 8, 6, 9);
                case "solo A": return Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 2);
                case "solo B": return Composite(PushBackCellTopology.SoloB, PushBackRunDirection.AToB, 2);
                case "encontradas d5/d5": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
                case "encontradas d8/d4": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, deepA: 8, deepB: 4);
                case "encontradas niv 3/2": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 3);
                case "corrida A->B": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, 2);
                case "corrida B->A": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, 2);
                default: return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotA: 0);
            }
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void AllViewsAndBom_ConsumeSameResolvedPhysicalHeaderHeight(string label)
        {
            var system = Scenario(label);
            var lines = Enumerable.Range(0, system.Structure.Fronts.Count + 1)
                .Where(line => DynamicFrontActivation.BoundaryExists(system.Structure, line))
                .ToList();
            var drawn = lines.Select(line => LateralHeight(system, line)).Distinct().OrderBy(value => value).ToList();

            Assert.NotEmpty(drawn);
            Assert.Equal(drawn, BomHeaderLengths(system));

            // Y las alturas dibujadas son las que exige alguna cama real (o, en un rack simple, su propio frente).
            if (system.IsComposite)
            {
                var required = PushBackRuns.Resolve(system).Runs
                    .Select(run => PushBackHeaderHeight.Requirement(run, Catalog))
                    .Distinct().ToList();
                Assert.All(drawn, height => Assert.Contains(height, required));
            }
        }

        /// <summary>Un override manual sigue mandando sobre la propuesta derivada (I-40).</summary>
        [Fact]
        public void HeaderOverride_RemainsEffective()
        {
            var state = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
            var inputs = Inputs();
            var derived = LateralHeight(Build(state, inputs), 1);
            Assert.Equal(120.0, derived, 6);

            inputs.ManualHeaderHeightOverride = 156.0;
            Assert.Equal(156.0, LateralHeight(Build(state, inputs), 1), 6);
        }

        /// <summary>
        /// Restore quita el override y vuelve a la propuesta ACTUAL, recalculada sobre las camas de AHORA — no al
        /// valor que la propuesta tenía cuando se puso el override.
        /// </summary>
        [Fact]
        public void RestoreHeader_RecomputesFromCurrentPhysicalRuns()
        {
            var state = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
            var inputs = Inputs();
            inputs.ManualHeaderHeightOverride = 156.0;
            Assert.Equal(156.0, LateralHeight(Build(state, inputs), 1), 6);

            // Con el override puesto, se hace el rack MÁS EXIGENTE: un tercer nivel en el lado A.
            for (var index = 0; index < state.Of(PushBackSide.A).Structure.Count; index++)
            {
                state.Of(PushBackSide.A).AdjustLevels(index, 1);
            }

            inputs.ManualHeaderHeightOverride = null;   // Restore
            var restored = LateralHeight(Build(state, inputs), 1);

            Assert.Equal(192.0, restored, 6);           // la propuesta de AHORA, no 120 ni 156
            Assert.Equal(PhysicalRequirement(Build(state, inputs)), restored, 6);
        }
    }
}
