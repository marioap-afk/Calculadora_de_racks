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
    /// I-42 (ronda 6D) — A Y B TIENEN ALTURAS INDEPENDIENTES, Y UNA DEFENSA SOLO VA EN UNA CARA DE CARGA.
    ///
    /// <para>
    /// 6D-A. Una cabecera vive en una LINEA transversal y en una POSICION longitudinal, y esa segunda coordenada
    /// decide a qué lado sirve: la primera mitad de la profundidad es de A y la segunda de B. Medido antes de esta
    /// ronda, con A de cuatro niveles y B de tres: los DOCE postes del corte —los de A y los de B— salían a 264",
    /// la demanda de A. Ahora los de A miden 264" y los de B 192", que es lo que cada lado necesita.
    /// </para>
    /// <para>
    /// 6D-B. Una defensa protege la cara por donde entra el montacargas. Se colocaba en los extremos de la COBERTURA
    /// de su línea, y un lado en blanco acorta esa cobertura: con A en blanco en la primera ranura aparecía una
    /// defensa en X=247.25, dentro del rack, contra la cara posterior del lado contrario. La interfaz entre los dos
    /// lados no es una cara de carga.
    /// </para>
    /// </summary>
    public class PushBackSideIndependentHeightTests
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
            IReadOnlyCollection<int> blanksA = null, IReadOnlyCollection<int> blanksB = null,
            IReadOnlyCollection<int> slotsWithB = null)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, slotsWithB == null || slotsWithB.Contains(slot));
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
            foreach (var slot in blanksA ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.A, slot, false)); }
            foreach (var slot in blanksB ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false)); }
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
            IReadOnlyCollection<int> blanksA = null, IReadOnlyCollection<int> blanksB = null,
            IReadOnlyCollection<int> slotsWithB = null)
            => Build(State(topology, direction, slots, levelsA, levelsB, deepA, deepB, blanksA, blanksB, slotsWithB));

        // ---- lecturas por LADO ----------------------------------------------------------------------------------

        /// <summary>Las alturas de cabecera que el corte de una línea dibuja DENTRO del tramo de un lado.</summary>
        private static IReadOnlyList<double> SideHeights(PushBackSystem system, int line, PushBackSide side)
        {
            var view = system.IsComposite ? system.Composite?.Of(side) : null;
            var minX = view == null ? double.NegativeInfinity : Math.Min(view.OuterX, view.InnerX);
            var maxX = view == null ? double.PositiveInfinity : Math.Max(view.OuterX, view.InnerX);
            var inner = view?.InnerX;
            return new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Where(instance => instance.Insertion.X >= minX - 1e-6 && instance.Insertion.X <= maxX + 1e-6)
                .Where(instance => !inner.HasValue || Math.Abs(instance.Insertion.X - inner.Value) > 1e-6)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3))
                .Distinct().OrderBy(value => value).ToList();
        }

        private static double SideHeight(PushBackSystem system, int line, PushBackSide side)
        {
            var heights = SideHeights(system, line, side);
            Assert.Single(heights);
            return heights[0];
        }

        private static IReadOnlyList<double> FrontalHeights(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3))
                .Distinct().OrderBy(value => value).ToList();

        // ---- 6D-A ----------------------------------------------------------------------------------------------

        /// <summary>
        /// EL CASO DEL DUEÑO: A con cuatro niveles y B con tres. Cada lado mide lo suyo, y son distintos.
        /// </summary>
        [Fact]
        public void HeaderHeight_AAndBAreIndependent()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 4, levelsB: 3);

            Assert.Equal(264.0, SideHeight(system, 1, PushBackSide.A), 6);
            Assert.Equal(192.0, SideHeight(system, 1, PushBackSide.B), 6);

            // Y cada altura es la que un rack simple de esos niveles resuelve: nada nuevo, la demanda de su cama.
            Assert.Equal(SideHeight(SingleSided(4, 5), 0, PushBackSide.A), SideHeight(system, 1, PushBackSide.A), 6);
            Assert.Equal(SideHeight(SingleSided(3, 5), 0, PushBackSide.A), SideHeight(system, 1, PushBackSide.B), 6);
        }

        /// <summary>Subir A no alarga ni un poste de B.</summary>
        [Fact]
        public void IncreasingAHeight_DoesNotGrowB()
        {
            var low = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 2, levelsB: 2);
            var tall = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 4, levelsB: 2);

            Assert.True(SideHeight(tall, 1, PushBackSide.A) > SideHeight(low, 1, PushBackSide.A), "A no creció");
            Assert.Equal(SideHeight(low, 1, PushBackSide.B), SideHeight(tall, 1, PushBackSide.B), 6);
        }

        /// <summary>Y al revés: subir B no alarga ni un poste de A.</summary>
        [Fact]
        public void IncreasingBHeight_DoesNotGrowA()
        {
            var low = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 2, levelsB: 2);
            var tall = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 2, levelsB: 4);

            Assert.True(SideHeight(tall, 1, PushBackSide.B) > SideHeight(low, 1, PushBackSide.B), "B no creció");
            Assert.Equal(SideHeight(low, 1, PushBackSide.A), SideHeight(tall, 1, PushBackSide.A), 6);
        }

        /// <summary>
        /// La altura y la profundidad son ejes distintos: un lado profundo y bajo no hereda la altura del corto y
        /// alto, ni al revés.
        /// </summary>
        [Fact]
        public void DeepAndTall_AreIndependentAxes()
        {
            var system = Composite(
                PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2,
                levelsA: 2, levelsB: 4, deepA: 8, deepB: 4);

            Assert.Equal(SideHeight(SingleSided(2, 8), 0, PushBackSide.A), SideHeight(system, 1, PushBackSide.A), 6);
            Assert.Equal(SideHeight(SingleSided(4, 4), 0, PushBackSide.A), SideHeight(system, 1, PushBackSide.B), 6);
            Assert.True(SideHeight(system, 1, PushBackSide.B) > SideHeight(system, 1, PushBackSide.A));
        }

        /// <summary>
        /// La conquista de 6B se conserva: la MISMA pieza física —la de un lado— mide lo mismo en su lateral y en
        /// sus dos cortes frontales. Lo que ya no se exige es que A y B midan lo mismo.
        /// </summary>
        public static IEnumerable<object[]> Scenarios() => new[]
        {
            new object[] { "A alto / B bajo" },
            new object[] { "B alto / A bajo" },
            new object[] { "iguales" },
            new object[] { "A profundo bajo / B corto alto" },
            new object[] { "corrida A->B" },
            new object[] { "corrida B->A" },
            new object[] { "blank A" },
            new object[] { "blank B" },
            new object[] { "solo A" },
            new object[] { "solo B" }
        };

        private static PushBackSystem Scenario(string label)
        {
            switch (label)
            {
                case "A alto / B bajo": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 4, levelsB: 3);
                case "B alto / A bajo": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 2, levelsB: 4);
                case "iguales": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
                case "A profundo bajo / B corto alto": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 2, levelsB: 4, deepA: 8, deepB: 4);
                case "corrida A->B": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, 2);
                case "corrida B->A": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, 2);
                case "blank A": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0 });
                case "blank B": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksB: new[] { 1 });
                case "solo A": return Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 2);
                default: return Composite(PushBackCellTopology.SoloB, PushBackRunDirection.AToB, 2);
            }
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void SamePhysicalSideHeader_AgreesAcrossViews(string label)
        {
            var system = Scenario(label);
            var checkedSides = 0;
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var view = system.Composite?.Of(side);
                if (view == null || !view.IsPresent) { continue; }

                var lateral = Enumerable.Range(0, system.Structure.Fronts.Count + 1)
                    .Where(line => DynamicFrontActivation.BoundaryExists(system.Structure, line))
                    .SelectMany(line => SideHeights(system, line, side))
                    .Distinct().OrderBy(value => value).ToList();
                if (lateral.Count == 0) { continue; }

                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    var frontal = FrontalHeights(system, end, side);
                    if (frontal.Count == 0) { continue; }
                    Assert.All(frontal, height => Assert.Contains(height, lateral));
                    checkedSides++;
                }
            }

            Assert.True(checkedSides > 0, $"{label}: no se comprobó ningún lado");
        }

        /// <summary>
        /// Una CORRIDA sí atraviesa los dos lados: es una pieza compartida de verdad, y entonces las dos zonas
        /// resuelven la MISMA envolvente. La independencia no significa «siempre distintos», significa «cada uno lo
        /// que le corresponde».
        /// </summary>
        [Fact]
        public void ActuallySharedHeader_UsesOnlyApplicableEnvelope()
        {
            foreach (var direction in new[] { PushBackRunDirection.AToB, PushBackRunDirection.BToA })
            {
                var corrida = Composite(PushBackCellTopology.Corrida, direction, 2);
                Assert.Equal(SideHeight(corrida, 1, PushBackSide.A), SideHeight(corrida, 1, PushBackSide.B), 6);
            }

            // Y unas encontradas con la misma configuración por lado tampoco difieren: la regla no inventa
            // diferencias, solo deja de imponerlas.
            var simetricas = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
            Assert.Equal(SideHeight(simetricas, 1, PushBackSide.A), SideHeight(simetricas, 1, PushBackSide.B), 6);
        }

        /// <summary>Un rack de un solo sentido no cambia: es el caso legacy.</summary>
        [Fact]
        public void SinglePushBack_HeaderHeightUnchanged()
        {
            Assert.Equal(120.0, SideHeight(SingleSided(2, 5), 0, PushBackSide.A), 6);
            Assert.Equal(new[] { 120.0, 132.0 },
                Enumerable.Range(0, 5).SelectMany(line => SideHeights(SingleSided(2, 5, 8, 6, 9), line, PushBackSide.A))
                    .Distinct().OrderBy(value => value).ToArray());
        }

        /// <summary>Un override manual sigue mandando, y lo hace sobre los dos lados: es del rack.</summary>
        [Fact]
        public void HeaderOverride_IsSideIndependent()
        {
            var state = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 4, levelsB: 3);
            var inputs = Inputs();
            Assert.Equal(264.0, SideHeight(Build(state, inputs), 1, PushBackSide.A), 6);
            Assert.Equal(192.0, SideHeight(Build(state, inputs), 1, PushBackSide.B), 6);

            inputs.ManualHeaderHeightOverride = 300.0;
            var overridden = Build(state, inputs);
            Assert.Equal(300.0, SideHeight(overridden, 1, PushBackSide.A), 6);
            Assert.Equal(300.0, SideHeight(overridden, 1, PushBackSide.B), 6);
        }

        /// <summary>
        /// Restore quita el override y devuelve la propuesta ACTUAL de CADA lado, no un valor común.
        /// </summary>
        [Fact]
        public void RestoreHeader_IsSideIndependent()
        {
            var state = State(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 4, levelsB: 3);
            var inputs = Inputs();
            inputs.ManualHeaderHeightOverride = 300.0;
            Assert.Equal(300.0, SideHeight(Build(state, inputs), 1, PushBackSide.A), 6);

            inputs.ManualHeaderHeightOverride = null;   // Restore
            var restored = Build(state, inputs);
            Assert.Equal(264.0, SideHeight(restored, 1, PushBackSide.A), 6);
            Assert.Equal(192.0, SideHeight(restored, 1, PushBackSide.B), 6);
        }

        // ---- 6D-B: la defensa ------------------------------------------------------------------------------------

        private static IReadOnlyList<(double X, double Y)> Defensas(PushBackSystem system)
        {
            var catalog = Catalog;
            return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety)
                .Where(instance =>
                {
                    var element = catalog.SafetyElements?.FirstOrDefault(entry => string.Equals(
                        entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
                    return element != null
                           && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType);
                })
                .Select(instance => (Math.Round(instance.Insertion.X, 3), Math.Round(instance.Insertion.Y, 3)))
                .Distinct().OrderBy(entry => entry.Item1).ThenBy(entry => entry.Item2).ToList();
        }

        /// <summary>
        /// EL CASO DEL DUEÑO: con un lado en blanco, la defensa NO puede saltar a la cara posterior del contrario.
        /// Toda defensa vive en un extremo real del rack, que es donde hay pasillo.
        /// </summary>
        [Theory]
        [InlineData("blank A")]
        [InlineData("blank B")]
        [InlineData("dos blanks A")]
        [InlineData("blank A+B")]
        public void SafetyBlank_DoesNotMoveProtectionToOppositeIrrelevantFace(string label)
        {
            PushBackSystem system;
            switch (label)
            {
                case "blank A": system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0 }); break;
                case "blank B": system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksB: new[] { 0 }); break;
                case "dos blanks A": system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0, 1 }); break;
                default: system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0 }, blanksB: new[] { 0 }); break;
            }

            var total = system.Structure.TotalLength;
            var defensas = Defensas(system);
            Assert.NotEmpty(defensas);
            foreach (var defensa in defensas)
            {
                var atStart = defensa.X <= 0.0 + 1e-6;
                var atEnd = defensa.X >= total - 1e-6;
                Assert.True(
                    atStart || atEnd,
                    $"{label}: defensa en X={defensa.X:0.###}, que no es un extremo del rack (0 o {total:0.###})");
            }
        }

        /// <summary>
        /// Y la retícula física no se movió: un blanco conserva su ranura y su línea, y las defensas de las líneas
        /// que SÍ cargan siguen exactamente donde estaban.
        /// </summary>
        [Fact]
        public void SafetyPhysicalGrid_RemainsStableWithBlanks()
        {
            var full = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3);
            var blanked = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0 });

            Assert.Equal(full.Structure.Fronts.Count, blanked.Structure.Fronts.Count);   // no se compacta
            Assert.Equal(full.Structure.TotalLength, blanked.Structure.TotalLength, 6);

            // Cada defensa del rack con blanco existe también en el completo: el blanco QUITA, nunca MUEVE.
            Assert.All(Defensas(blanked), defensa => Assert.Contains(defensa, Defensas(full)));
            Assert.True(Defensas(blanked).Count < Defensas(full).Count, "el blanco no quitó ninguna defensa");
        }

        /// <summary>
        /// Que exista lado B en OTRA zona no cambia la seguridad de las zonas no compuestas: sigue siendo la regla
        /// de la ronda 6A y esta corrección no la toca.
        /// </summary>
        [Fact]
        public void PartialComposite_DoesNotChangeUnrelatedSafety()
        {
            var partial = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, slotsWithB: new[] { 2 });
            var full = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3);

            Assert.NotEmpty(Defensas(partial));
            Assert.All(Defensas(partial), defensa => Assert.Contains(defensa, Defensas(full)));
        }

        /// <summary>El BOM sigue contando exactamente las defensas que existen.</summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void DrawAndBomSafetyStillAgree(string label)
        {
            var system = Scenario(label);
            var catalog = Catalog;
            var drawnDefensas = Defensas(system).Count;
            var bought = PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => catalog.SafetyElements?.Any(entry =>
                    string.Equals(entry?.Id, component.ProfileId, StringComparison.OrdinalIgnoreCase)
                    && SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)) ?? false)
                .Sum(component => component.Quantity);

            Assert.Equal(drawnDefensas, bought);
        }

        /// <summary>6A intacto: el BOM de intermedios sigue siendo el del dibujo.</summary>
        [Fact]
        public void IntermediateBomStillMatchesTheDrawing()
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

            var system = new PushBackEditorDesignAssembler(Catalog).Build(state, inputs).System;
            var drawn = new PushBackIntermediateBeamLateralBuilder()
                .BuildFor(system, Catalog, system.Structure.Fronts[0], null)
                .Count(instance => instance.Role == HeaderBlockRole.Beam);
            var bom = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.IntermediateBeam)
                .Sum(component => component.Quantity);

            Assert.Equal(27, drawn);
            Assert.Equal(27, bom);
        }
    }
}
