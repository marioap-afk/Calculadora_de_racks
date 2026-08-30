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
    /// I-42 (ronda 6F) — LA BOTA VA DONDE HAY UNA CARA DE ATAQUE, Y UN BLANCO LA QUITA SIN MOVERLA.
    ///
    /// <para>
    /// Una bota protege el poste del impacto del montacargas, y el montacargas ataca por la cara de CARGA: en un
    /// Push Back, la del extremo BAJO. Un compuesto tiene DOS, una por lado, en los dos exteriores del rack.
    /// </para>
    /// <para>
    /// El defecto que el dueño reprodujo: las dos copias de una línea se atornillan a los extremos de su COBERTURA
    /// de profundidad, y con frentes en blanco —una columna de nave— esa cobertura se acorta y su extremo pasa a
    /// caer en la interfaz entre los dos lados. Medido, con las dos primeras ranuras de A en blanco: una bota en
    /// X=395.61, contra la columna, mientras la de B seguía bien en su pasillo a X=792.39. Es la misma familia del
    /// error de la defensa de montacargas que la ronda 6D cerró, y se corrige con la misma declaración física.
    /// </para>
    /// <para>
    /// Lo que estas pruebas NO afirman: nada sobre el significado global de Izquierda/Derecha/Ambas. Ese selector es
    /// una deuda aparte, declarada en el informe de la ronda.
    /// </para>
    /// </summary>
    public class PushBackBootLowFaceTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

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

        private static PushBackSystem SingleSided(int fronts = 3, int levels = 2, int deep = 5)
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = deep;
            foreach (var selection in Inputs().SafetySelections) { inputs.SafetySelections.Add(selection); }
            state.SetFrontCount(fronts);
            for (var index = 0; index < fronts; index++)
            {
                state.Structure.Fronts[index].LoadLevels = levels;
                state.Structure.Fronts[index].PalletsDeep = deep;
                state.AdjustLevels(index, 0);
            }

            var computation = new PushBackEditorDesignAssembler(Catalog).Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation.System;
        }

        private static PushBackSystem Composite(
            int slots = 3, IReadOnlyCollection<int> blanksA = null, IReadOnlyCollection<int> blanksB = null,
            IReadOnlyCollection<int> slotsWithB = null,
            PushBackCellTopology topology = PushBackCellTopology.Encontradas)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, slotsWithB == null || slotsWithB.Contains(slot));
            }

            state.SetDefaults(topology, PushBackRunDirection.AToB);
            foreach (var slot in blanksA ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.A, slot, false)); }
            foreach (var slot in blanksB ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false)); }

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas ------------------------------------------------------------------------------------------

        private static IReadOnlyList<HeaderBlockInstance> BootInstances(PushBackSystem system)
        {
            var catalog = Catalog;
            var bootId = BootId(catalog);
            return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety
                    && string.Equals(instance.PieceId, bootId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(instance => instance.Insertion.Y)
                .ThenBy(instance => instance.Insertion.X)
                .ToList();
        }

        private static IReadOnlyList<string> Boots(PushBackSystem system)
            => BootInstances(system)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();

        private static int BomBoots(PushBackSystem system)
        {
            var catalog = Catalog;
            var bootId = BootId(catalog);
            return PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => string.Equals(component.ProfileId, bootId, StringComparison.OrdinalIgnoreCase))
                .Sum(component => component.Quantity);
        }

        /// <summary>Cuanto puede separarse una bota del borde al que se atornilla (el mate de la placa).</summary>
        private const double FaceWindow = 1.0;

        // ---- los escenarios ------------------------------------------------------------------------------------

        public static IEnumerable<object[]> Scenarios() => new[]
        {
            new object[] { "simple" },
            new object[] { "compuesto completo" },
            new object[] { "blanks A 0,1 (columna)" },
            new object[] { "blank A 0" },
            new object[] { "blanks B 0,1" },
            new object[] { "blanks A+B 0,1" },
            new object[] { "parcial compuesto" },
            new object[] { "corrida" }
        };

        private static PushBackSystem Scenario(string label)
        {
            switch (label)
            {
                case "simple": return SingleSided();
                case "compuesto completo": return Composite();
                case "blanks A 0,1 (columna)": return Composite(blanksA: new[] { 0, 1 });
                case "blank A 0": return Composite(blanksA: new[] { 0 });
                case "blanks B 0,1": return Composite(blanksB: new[] { 0, 1 });
                case "blanks A+B 0,1": return Composite(blanksA: new[] { 0, 1 }, blanksB: new[] { 0, 1 });
                case "parcial compuesto": return Composite(slotsWithB: new[] { 2 });
                default: return Composite(topology: PushBackCellTopology.Corrida);
            }
        }

        // ---- el contrato fisico ---------------------------------------------------------------------------------

        /// <summary>
        /// LA REGLA: toda bota se atornilla a una cara de ATAQUE real, que en un Push Back es un extremo exterior
        /// del rack. Nunca a la interfaz interior, que no tiene pasillo por el que entre nadie.
        /// </summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void PushBackBoot_IsPlacedOnlyOnApplicableLowAttackFace(string label)
        {
            var system = Scenario(label);
            var total = system.Structure.TotalLength;
            var boots = BootInstances(system);

            Assert.NotEmpty(boots);
            foreach (var boot in boots)
            {
                var atStart = Math.Abs(boot.Insertion.X - 0.0) <= FaceWindow;
                var atEnd = Math.Abs(boot.Insertion.X - total) <= FaceWindow;
                Assert.True(
                    atStart || atEnd,
                    $"{label}: bota en X={boot.Insertion.X:0.###}, que no es una cara exterior "
                        + $"(0 o {total:0.###})");
            }
        }

        /// <summary>
        /// Un compuesto protege sus DOS caras de ataque —una por lado— y son posiciones FISICAMENTE DISTINTAS, en
        /// los dos exteriores del rack.
        /// </summary>
        [Fact]
        public void CompositeBoots_UseDistinctPhysicalLowFaces()
        {
            var system = Composite();
            var total = system.Structure.TotalLength;
            var xs = BootInstances(system).Select(boot => Math.Round(boot.Insertion.X, 3)).Distinct().ToList();

            Assert.Equal(2, xs.Count);
            Assert.Contains(xs, x => Math.Abs(x - 0.0) <= FaceWindow);
            Assert.Contains(xs, x => Math.Abs(x - total) <= FaceWindow);
        }

        /// <summary>
        /// Y NO se protege dos veces la misma cara: no hay dos botas en la misma posicion diferenciadas solo por el
        /// espejo. Dos exteriores distintos pueden tener manos opuestas —miran a pasillos opuestos—, y eso no es lo
        /// mismo que duplicar una copia sobre si misma.
        /// </summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void CompositeBoots_DoNotDuplicateAtSamePostByMirror(string label)
        {
            var system = Scenario(label);
            var positions = BootInstances(system)
                .Select(boot => (Math.Round(boot.Insertion.X, 3), Math.Round(boot.Insertion.Y, 3)))
                .ToList();

            Assert.Equal(positions.Count, positions.Distinct().Count());
        }

        /// <summary>
        /// EL CASO DEL DUEÑO. Con las dos primeras ranuras de A en blanco —la columna de nave—, la linea que queda
        /// entre ellas se queda SIN BOTA AUTOMATICA, y ninguna se muda al borde disponible mas cercano.
        ///
        /// <para>
        /// RETARGETEADO EN S1C, por decision explicita del dueño. La ronda 6F retiraba solo la cara que el blanco se
        /// llevaba —la de A, en X=-0.39— y conservaba la de B en X=792.39. Eso no era una decision de nadie: era la
        /// mitad que el filtro de caras no alcanzaba, y hacia que un blanco acabara eligiendo «posterior» por su
        /// cuenta. El contrato final es que un blanco apaga el AUTOMATICO de esa linea entera.
        /// </para>
        /// <para>
        /// Pieza por pieza en esta linea (Y=53.494): <c>-0.39</c> ANTES no estaba y AHORA tampoco —lo unico que la
        /// ronda 6F ya corregia—; <c>792.39</c> ANTES estaba y AHORA no. El resto del rack no se toca, y la de B se
        /// recupera pidiendola: el blanco decide el defecto, nunca la capacidad de configurar (S1C, §6).
        /// </para>
        /// </summary>
        [Fact]
        public void BlankLowFace_RemovesBootWithoutRelocation()
        {
            var full = Composite();
            var blanked = Composite(blanksA: new[] { 0, 1 });

            // Cada bota que sobrevive existia ya, en la MISMA posicion y con la misma mano…
            Assert.All(Boots(blanked), boot => Assert.Contains(boot, Boots(full)));
            // …y hay dos menos: las dos de la linea que el blanco dejo sin pasillo propio.
            Assert.Equal(Boots(full).Count - 2, Boots(blanked).Count);
            Assert.DoesNotContain(Boots(blanked), boot => boot.StartsWith("-0.39|53.494", StringComparison.Ordinal));
            Assert.DoesNotContain(Boots(blanked), boot => boot.StartsWith("792.39|53.494", StringComparison.Ordinal));
            // Y las de las demas lineas siguen intactas, las dos mitades.
            Assert.Contains(Boots(blanked), boot => boot.StartsWith("-0.39|", StringComparison.Ordinal));
            Assert.Contains(Boots(blanked), boot => boot.StartsWith("792.39|", StringComparison.Ordinal));
        }

        /// <summary>Un blanco en A no crea una bota en el extremo ALTO de A ni en la interfaz.</summary>
        [Fact]
        public void BlankA_DoesNotCreateBootAtAHighOrInterior()
        {
            var system = Composite(blanksA: new[] { 0, 1 });
            var composite = system.Composite;
            Assert.NotNull(composite);

            foreach (var boot in BootInstances(system))
            {
                Assert.False(
                    Math.Abs(boot.Insertion.X - composite.SideA.InnerX) <= 12.0,
                    $"bota en X={boot.Insertion.X:0.###}, contra el extremo alto de A / la interfaz");
            }
        }

        /// <summary>Simetrico: un blanco en B no crea una bota en el extremo ALTO de B ni en la interfaz.</summary>
        [Fact]
        public void BlankB_DoesNotCreateBootAtBHighOrInterior()
        {
            var system = Composite(blanksB: new[] { 0, 1 });
            var composite = system.Composite;
            Assert.NotNull(composite);

            foreach (var boot in BootInstances(system))
            {
                Assert.False(
                    Math.Abs(boot.Insertion.X - composite.SideB.InnerX) <= 12.0,
                    $"bota en X={boot.Insertion.X:0.###}, contra el extremo alto de B / la interfaz");
            }
        }

        /// <summary>Con los dos lados en blanco en la misma zona no queda ninguna bota interior.</summary>
        [Fact]
        public void BlankBoth_DoesNotCreateInternalBoot()
        {
            var system = Composite(blanksA: new[] { 0, 1 }, blanksB: new[] { 0, 1 });
            var total = system.Structure.TotalLength;

            Assert.NotEmpty(BootInstances(system));
            Assert.All(BootInstances(system), boot => Assert.True(
                Math.Abs(boot.Insertion.X) <= FaceWindow || Math.Abs(boot.Insertion.X - total) <= FaceWindow,
                $"bota interior en X={boot.Insertion.X:0.###}"));

            // Y las lineas que perdieron las dos caras no llevan bota: quedan menos que en el rack completo.
            Assert.True(BootInstances(system).Count < BootInstances(Composite()).Count);
        }

        /// <summary>Que exista lado B en otra zona no cambia la pertenencia de las botas de las zonas no compuestas.</summary>
        [Fact]
        public void PartialComposite_DoesNotAffectLegacyBootMembership()
        {
            var partial = Composite(slotsWithB: new[] { 2 });
            var full = Composite();

            Assert.NotEmpty(Boots(partial));
            Assert.All(Boots(partial), boot => Assert.Contains(boot, Boots(full)));
        }

        /// <summary>
        /// UN PUSH BACK SIMPLE NO CAMBIA: su unica cara de ataque es el extremo bajo, y sus botas siguen ahi, con
        /// los valores de siempre.
        /// </summary>
        [Fact]
        public void SimplePushBack_BootBehaviorUnchanged()
        {
            var system = SingleSided();

            Assert.Equal(2, BootInstances(system).Count);
            Assert.All(BootInstances(system), boot => Assert.Equal(-0.39, boot.Insertion.X, 2));
            Assert.All(BootInstances(system), boot => Assert.False(boot.MirroredX));
            Assert.Equal(2, BomBoots(system));
        }

        /// <summary>El BOM cuenta exactamente las botas que se materializan.</summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootBom_EqualsPhysicalBootPlacements(string label)
        {
            var system = Scenario(label);
            Assert.Equal(BootInstances(system).Count, BomBoots(system));
        }

        /// <summary>
        /// REGRESION de la ronda 6D: la defensa de montacargas tampoco vuelve a la interfaz por efecto de los
        /// blancos. Las dos familias comparten la misma declaracion fisica y ninguna puede romper a la otra.
        /// </summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void ForkliftDefense_StillNeverLandsOnTheInterior(string label)
        {
            var system = Scenario(label);
            if (!system.IsComposite) { return; }

            var catalog = Catalog;
            var total = system.Structure.TotalLength;
            foreach (var defense in new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                         .Where(instance => instance.Role == HeaderBlockRole.Safety)
                         .Where(instance =>
                         {
                             var element = catalog.SafetyElements?.FirstOrDefault(entry => string.Equals(
                                 entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
                             return element != null
                                    && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType);
                         }))
            {
                Assert.True(
                    defense.Insertion.X <= 0.0 + 1e-6 || defense.Insertion.X >= total - 1e-6,
                    $"{label}: defensa en X={defense.Insertion.X:0.###}, que no es un extremo del rack");
            }
        }

        /// <summary>
        /// Y 6A sigue intacto: el BOM de largueros intermedios sigue siendo el del dibujo.
        /// </summary>
        [Fact]
        public void IntermediateBom_StillMatchesTheDrawing()
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

            Assert.Equal(27, drawn);
            Assert.Equal(27, PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.IntermediateBeam)
                .Sum(component => component.Quantity));
        }
    }
}
