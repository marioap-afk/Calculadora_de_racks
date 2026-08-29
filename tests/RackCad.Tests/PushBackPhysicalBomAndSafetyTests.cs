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
    /// I-42 (ronda 6A) — EL BOM CUENTA PIEZAS FÍSICAS, Y LA SEGURIDAD NO SE EXPANDE POR SER COMPUESTO.
    ///
    /// <para>
    /// Defecto medido y corregido en esta ronda: un rack Push Back de UN SOLO SENTIDO con fondos por celda dibujaba
    /// 27 largueros intermedios y facturaba 42. El camino compuesto ya contaba con el mismo builder que dibuja; el
    /// de un solo sentido conservaba la cuenta heredada del Dinámico —fronteras de la estructura × niveles— que no
    /// aplica el fondo EFECTIVO por celda de I-41. Ahora las dos rutas comparten UNA sola cuenta.
    /// </para>
    /// </summary>
    public class PushBackPhysicalBomAndSafetyTests
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

        /// <summary>Una estructura de ocho fondos con una cama por nivel, de 3 a 8 fondos: el escenario del dueño.</summary>
        private static PushBackSystem Ladder(bool reinforced = true)
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = 8;
            inputs.DerivedPostReinforced = reinforced;
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

        private static PushBackSystem Composite(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots,
            IReadOnlyCollection<int> slotsWithB = null, int? blankSlotA = null)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, slotsWithB == null || slotsWithB.Contains(slot));
            }

            state.SetDefaults(topology, direction);
            if (blankSlotA.HasValue)
            {
                Assert.True(state.SetSlotPresent(PushBackSide.A, blankSlotA.Value, false));
            }

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas -----------------------------------------------------------------------------------------

        private static int BomQuantity(PushBackSystem system, string category)
            => PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == category)
                .Sum(component => component.Quantity);

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        /// <summary>Los intermedios que el DIBUJO materializa, por cama física.</summary>
        private static int DrawnIntermediates(PushBackSystem system)
        {
            var builder = new PushBackIntermediateBeamLateralBuilder();
            if (!system.IsComposite)
            {
                return system.Structure.Fronts
                    .Where(front => front != null)
                    .Sum(front => builder.BuildFor(system, Catalog, front, null)
                        .Count(instance => instance.Role == HeaderBlockRole.Beam));
            }

            return PushBackCompositeContent.Batches(PushBackRuns.Resolve(system), null)
                .Where(batch => batch.Front != null)
                .Sum(batch => builder.BuildFor(batch.Source, Catalog, batch.Front, batch.Levels)
                    .Count(instance => instance.Role == HeaderBlockRole.Beam));
        }

        /// <summary>Las piezas de SEGURIDAD dibujadas, por pieza y posición: la realidad física del plano.</summary>
        private static IReadOnlyList<string> DrawnSafety(PushBackSystem system)
        {
            var result = new List<string>();
            foreach (var instance in new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                         .Where(instance => instance.Role == HeaderBlockRole.Safety))
            {
                result.Add(FormattableString.Invariant(
                    $"PLANTA|{instance.PieceId}|{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}"));
            }

            foreach (var side in system.IsComposite ? new[] { PushBackSide.A, PushBackSide.B } : new[] { PushBackSide.A })
            {
                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    foreach (var instance in new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side)
                                 .Flatten().Instances.Where(instance => instance.Role == HeaderBlockRole.Safety))
                    {
                        result.Add(FormattableString.Invariant(
                            $"{side}/{end}|{instance.PieceId}|{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"));
                    }
                }
            }

            result.Sort(StringComparer.Ordinal);
            return result;
        }

        // ---- A) el BOM de intermedios ES el de los apoyos físicos ----------------------------------------------

        public static IEnumerable<object[]> Racks() => new[]
        {
            new object[] { "escalera 3..8" },
            new object[] { "un solo sentido 5/5" },
            new object[] { "un solo sentido 5/8/6/9" },
            new object[] { "compuesto solo A" },
            new object[] { "compuesto solo B" },
            new object[] { "compuesto encontradas" },
            new object[] { "compuesto corrida A->B" },
            new object[] { "compuesto corrida B->A" },
            new object[] { "compuesto blank A" },
            new object[] { "compuesto B parcial" }
        };

        private static PushBackSystem Rack(string label)
        {
            switch (label)
            {
                case "escalera 3..8": return Ladder();
                case "un solo sentido 5/5": return SingleSided(2, 5, 5);
                case "un solo sentido 5/8/6/9": return SingleSided(2, 5, 8, 6, 9);
                case "compuesto solo A": return Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 2);
                case "compuesto solo B": return Composite(PushBackCellTopology.SoloB, PushBackRunDirection.AToB, 2);
                case "compuesto encontradas": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
                case "compuesto corrida A->B": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, 2);
                case "compuesto corrida B->A": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, 2);
                case "compuesto blank A": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotA: 0);
                default: return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, slotsWithB: new[] { 2 });
            }
        }

        /// <summary>
        /// A) El BOM de largueros intermedios es EXACTAMENTE el número de apoyos que el dibujo materializa. Es la
        /// prueba que el defecto 27-vs-42 no habría pasado.
        /// </summary>
        [Theory]
        [MemberData(nameof(Racks))]
        public void BomIntermediateBeams_EqualsPhysicalRunSupports(string label)
        {
            var system = Rack(label);
            var drawn = DrawnIntermediates(system);

            Assert.True(drawn > 0, $"{label}: el dibujo no puso ningún intermedio");
            Assert.Equal(drawn, BomQuantity(system, SystemBomBuilder.IntermediateBeam));
        }

        /// <summary>
        /// B) EL CASO MEDIDO: el fondo por celda detiene la cuenta en el larguero alto de esa cama. 2+3+4+5+6+7 = 27,
        /// no 7 × 6 = 42.
        /// </summary>
        [Fact]
        public void CellDepthStopsIntermediateCountAtHighBoundary()
        {
            var system = Ladder();
            var builder = new PushBackIntermediateBeamLateralBuilder();
            var front = system.Structure.Fronts[0];

            var perLevel = Enumerable.Range(1, 6)
                .Select(level => builder.BuildFor(system, Catalog, front, new[] { level })
                    .Count(instance => instance.Role == HeaderBlockRole.Beam))
                .ToList();

            Assert.Equal(new[] { 2, 3, 4, 5, 6, 7 }, perLevel);
            Assert.Equal(27, perLevel.Sum());
            Assert.Equal(27, BomQuantity(system, SystemBomBuilder.IntermediateBeam));
        }

        /// <summary>C) El refuerzo del poste derivado no cambia la cuenta funcional, tampoco en el BOM.</summary>
        [Fact]
        public void ReinforcedPost_DoesNotChangeFunctionalBeamCount()
        {
            Assert.Equal(
                BomQuantity(Ladder(reinforced: false), SystemBomBuilder.IntermediateBeam),
                BomQuantity(Ladder(reinforced: true), SystemBomBuilder.IntermediateBeam));
        }

        /// <summary>
        /// D) Unas camas ENCONTRADAS son DOS camas: conservan sus dos largueros altos y sus dos juegos de
        /// intermedios aunque se proyecten sobre la misma retícula. El arreglo del 27-vs-42 no las colapsa.
        /// </summary>
        [Fact]
        public void EncounteredRuns_PreserveIndependentHighsAndIntermediates()
        {
            var encontradas = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 1);
            var soloA = Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 1);

            Assert.Equal(2 * BomQuantity(soloA, PushBackBomBuilder.HighEndBeam), BomQuantity(encontradas, PushBackBomBuilder.HighEndBeam));
            Assert.Equal(2 * BomQuantity(soloA, PushBackBomBuilder.RearTope), BomQuantity(encontradas, PushBackBomBuilder.RearTope));
            Assert.Equal(2 * BomQuantity(soloA, SystemBomBuilder.IntermediateBeam), BomQuantity(encontradas, SystemBomBuilder.IntermediateBeam));
            Assert.Equal(DrawnIntermediates(encontradas), BomQuantity(encontradas, SystemBomBuilder.IntermediateBeam));
        }

        // ---- seguridad ------------------------------------------------------------------------------------------

        /// <summary>
        /// E) Que exista lado B EN OTRA ZONA no contamina el resto del rack: la seguridad de las líneas que sólo
        /// toca el lado A es idéntica con B presente en una sola ranura y con B ausente de ellas.
        /// </summary>
        [Fact]
        public void SafetyMembership_DoesNotExpandBecauseCompositeExistsElsewhere()
        {
            var partial = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, slotsWithB: new[] { 2 });
            var full = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3);

            var partialSafety = DrawnSafety(partial);
            var fullSafety = DrawnSafety(full);

            Assert.NotEmpty(partialSafety);
            // Un rack con B en una sola ranura NO puede llevar la misma seguridad que uno con B en las tres…
            Assert.True(
                partialSafety.Count < fullSafety.Count,
                $"la seguridad no depende de dónde está el lado B: parcial {partialSafety.Count}, completo {fullSafety.Count}");
            // …y cada pieza del parcial existe también en el completo: no aparece nada nuevo por ser parcial.
            Assert.All(partialSafety, piece => Assert.Contains(piece, fullSafety));
        }

        /// <summary>
        /// F) Un blanco conserva la retícula FÍSICA de seguridad sin crear almacenamiento: la ranura sigue ahí, sus
        /// líneas siguen ahí, y no aparece ninguna cama en ella.
        /// </summary>
        [Fact]
        public void BlankSlot_PreservesPhysicalSecurityGridWithoutCreatingStorage()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotA: 0);

            Assert.Equal(3, system.Structure.Fronts.Count);                     // la ranura NO se compacta
            Assert.NotEmpty(DrawnSafety(system));                               // y sigue habiendo seguridad física
            var runs = PushBackRuns.Resolve(system);
            Assert.DoesNotContain(runs.Runs, run => run.Slot == 0 && run.LowSide == PushBackSide.A);
        }

        /// <summary>
        /// G) DOS CARAS DE CARGA NO ES <c>Side = Both</c>. El lado de una selección de seguridad es PERTENENCIA
        /// —qué postes llevan la pieza—, salvo en el desviador, la única familia donde significa literalmente «qué
        /// pasillo». Que un rack tenga dos caras no puede escribir Both en las demás familias: eso apagaría sus
        /// reglas adaptativas, que es el defecto que el dueño ya rechazó.
        /// </summary>
        [Fact]
        public void TwoLoadFaces_IsNotEquivalentToSideBoth()
        {
            var composite = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
            var single = SingleSided(2, 5, 5);
            var catalog = Catalog;

            bool IsDesviador(SelectiveSafetySelection selection)
            {
                var element = catalog.SafetyElements?.FirstOrDefault(entry => string.Equals(
                    entry?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                return element != null
                       && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DesviadorType);
            }

            var faces = composite.Structure.SafetySelections.Where(selection => !IsDesviador(selection)).ToList();
            Assert.NotEmpty(faces);
            Assert.All(faces, selection => Assert.NotEqual(SafetySide.Both, selection.Side));

            // Y el lado general de esas familias es el MISMO que en un rack de una sola cara: la segunda cara se
            // modela por pertenencia, no colapsando el eje del lado.
            foreach (var selection in faces)
            {
                var twin = single.Structure.SafetySelections
                    .FirstOrDefault(other => string.Equals(other.ElementId, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                if (twin != null)
                {
                    Assert.Equal(twin.Side, selection.Side);
                }
            }
        }

        /// <summary>
        /// H) El BOM de seguridad cuenta EXACTAMENTE las piezas que existen: ni una dibujada sin comprar, ni una
        /// comprada sin dibujar.
        /// </summary>
        [Theory]
        [MemberData(nameof(Racks))]
        public void DrawAndBomSafetyQuantitiesAgree(string label)
        {
            var system = Rack(label);
            var catalog = Catalog;
            var bom = PushBackBomBuilder.Build(system, catalog);

            var safetyIds = new HashSet<string>(
                catalog.SafetyElements?.Select(entry => entry.Id) ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            var bought = bom.Components
                .Where(component => safetyIds.Contains(component.ProfileId)
                                    && component.Category != PushBackBomBuilder.RearTope)
                .Select(component => component.ProfileId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var drawn = DrawnSafety(system)
                .Select(piece => piece.Split('|')[1])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Ninguna pieza de seguridad comprada sin representación física, y ninguna dibujada sin comprarse.
            Assert.Equal(drawn, bought);
        }
    }
}
