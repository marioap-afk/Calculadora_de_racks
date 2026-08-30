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
    /// I-42 (ronda 6E) — EL SELECTOR DE PROTECTORES DE BOTA VUELVE A SIGNIFICAR ALGO.
    ///
    /// <para>
    /// El selector ofrece Ninguno / Izquierda / Derecha / Ambas, y las cuatro opciones producían la MISMA pieza:
    /// medido, <c>Izquierda == Derecha == Ambas</c> en los ocho escenarios probados. Dos causas encadenadas:
    /// </para>
    /// <list type="number">
    /// <item><c>PushBackSafetyAuthority.RestrictToLowEnd</c> escribía <c>Side = Left</c> sobre cualquier Both o Right
    /// «para imponer el extremo bajo». El extremo ya lo impone <c>LowEndOnly</c>, que no toca la orientación: ese
    /// colapso solo borraba la elección del usuario.</item>
    /// <item>Con un solo pasillo, <c>SelectiveSafetyEnds.CopiesForPost</c> devolvía para AMBAS una sola copia —la de
    /// Izquierda—. Un poste tiene sus dos caras aunque se cargue por un solo extremo.</item>
    /// </list>
    /// <para>
    /// El contrato que estas pruebas fijan: <c>None = ∅</c>, <c>Both = Left ∪ Right</c>, y donde los dos conjuntos
    /// existen, <c>Left != Both</c> y <c>Right != Both</c>.
    /// </para>
    /// </summary>
    public class PushBackBootSelectorTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

        /// <summary>Los ajustes de un rack con el selector de botas en <paramref name="side"/>.</summary>
        private static PushBackEditorInputs Inputs(SafetySide side)
        {
            var catalog = Catalog;
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                var element = catalog.SafetyElements?.FirstOrDefault(entry =>
                    string.Equals(entry?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                if (element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.BotaType))
                {
                    selection.Side = side;
                    selection.Quantity = side == SafetySide.None ? 0 : 1;
                }

                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        private static PushBackSystem SingleSided(SafetySide side, int fronts = 2, int levels = 2, int deep = 5)
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = deep;
            foreach (var selection in Inputs(side).SafetySelections)
            {
                inputs.SafetySelections.Add(selection);
            }

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
            SafetySide side, PushBackCellTopology topology, PushBackRunDirection direction, int slots,
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

            state.SetDefaults(topology, direction);
            foreach (var slot in blanksA ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.A, slot, false)); }
            foreach (var slot in blanksB ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false)); }

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(side), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas ------------------------------------------------------------------------------------------

        /// <summary>Las botas materializadas, por posicion y mano: la realidad fisica del plano.</summary>
        private static IReadOnlyList<string> Boots(PushBackSystem system)
        {
            var catalog = Catalog;
            var bootId = BootId(catalog);
            return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety
                    && string.Equals(instance.PieceId, bootId, StringComparison.OrdinalIgnoreCase))
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}|{instance.MirroredY}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static int BomBoots(PushBackSystem system)
        {
            var catalog = Catalog;
            var bootId = BootId(catalog);
            return PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => string.Equals(component.ProfileId, bootId, StringComparison.OrdinalIgnoreCase))
                .Sum(component => component.Quantity);
        }

        // ---- los escenarios ------------------------------------------------------------------------------------

        public static IEnumerable<object[]> Scenarios() => new[]
        {
            new object[] { "simple" },
            new object[] { "solo A" },
            new object[] { "solo B" },
            new object[] { "A+B encontradas" },
            new object[] { "corrida" },
            new object[] { "blank A" },
            new object[] { "blank B" },
            new object[] { "dos blanks consecutivos" },
            new object[] { "parcial compuesto" }
        };

        private static PushBackSystem Scenario(string label, SafetySide side)
        {
            switch (label)
            {
                case "simple": return SingleSided(side);
                case "solo A": return Composite(side, PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 2);
                case "solo B": return Composite(side, PushBackCellTopology.SoloB, PushBackRunDirection.AToB, 2);
                case "A+B encontradas": return Composite(side, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3);
                case "corrida": return Composite(side, PushBackCellTopology.Corrida, PushBackRunDirection.AToB, 2);
                case "blank A": return Composite(side, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0 });
                case "blank B": return Composite(side, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksB: new[] { 0 });
                case "dos blanks consecutivos": return Composite(side, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blanksA: new[] { 0, 1 });
                default: return Composite(side, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, slotsWithB: new[] { 2 });
            }
        }

        // ---- el contrato del selector ---------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_NoneProducesNone(string label)
        {
            var system = Scenario(label, SafetySide.None);
            Assert.Empty(Boots(system));
            Assert.Equal(0, BomBoots(system));
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_LeftProducesOnlyLeftMembership(string label)
        {
            var left = Boots(Scenario(label, SafetySide.Left));
            var both = Boots(Scenario(label, SafetySide.Both));

            Assert.NotEmpty(left);
            // Izquierda es un SUBCONJUNTO estricto de Ambas: ni una pieza suya sobra, ni las trae todas.
            Assert.All(left, boot => Assert.Contains(boot, both));
            Assert.True(left.Count < both.Count, $"{label}: Izquierda no es un subconjunto estricto de Ambas");
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_RightProducesOnlyRightMembership(string label)
        {
            var right = Boots(Scenario(label, SafetySide.Right));
            var both = Boots(Scenario(label, SafetySide.Both));

            Assert.NotEmpty(right);
            Assert.All(right, boot => Assert.Contains(boot, both));
            Assert.True(right.Count < both.Count, $"{label}: Derecha no es un subconjunto estricto de Ambas");
        }

        /// <summary>LA IGUALDAD DEL CONTRATO: Ambas es exactamente la union de Izquierda y Derecha.</summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_BothEqualsUnionOfLeftAndRight(string label)
        {
            var left = Boots(Scenario(label, SafetySide.Left));
            var right = Boots(Scenario(label, SafetySide.Right));
            var both = Boots(Scenario(label, SafetySide.Both));

            var union = new SortedSet<string>(left, StringComparer.Ordinal);
            union.UnionWith(right);

            Assert.Equal(union.ToList(), both.OrderBy(value => value, StringComparer.Ordinal).ToList());
            Assert.Equal(left.Count + right.Count, both.Count);   // y los dos conjuntos son DISJUNTOS
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_LeftIsNotBoth_WhenBothSidesExist(string label)
        {
            var left = Boots(Scenario(label, SafetySide.Left));
            var both = Boots(Scenario(label, SafetySide.Both));
            Assert.NotEmpty(left);
            Assert.NotEqual(both, left);
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_RightIsNotBoth_WhenBothSidesExist(string label)
        {
            var right = Boots(Scenario(label, SafetySide.Right));
            var both = Boots(Scenario(label, SafetySide.Both));
            Assert.NotEmpty(right);
            Assert.NotEqual(both, right);
        }

        /// <summary>Y Izquierda tampoco es Derecha: son las dos caras del poste.</summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BootSelector_LeftIsNotRight(string label)
        {
            Assert.NotEqual(Boots(Scenario(label, SafetySide.Right)), Boots(Scenario(label, SafetySide.Left)));
        }

        // ---- blancos ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Ninguna bota puede quedar en la interfaz entre los dos lados: no es una cara de carga. Es la misma regla
        /// fisica que la ronda 6D fijo para la defensa.
        /// </summary>
        [Theory]
        [InlineData("blank A")]
        [InlineData("blank B")]
        [InlineData("dos blanks consecutivos")]
        [InlineData("parcial compuesto")]
        public void BootBlank_DoesNotRelocateToInterior(string label)
        {
            foreach (var side in new[] { SafetySide.Left, SafetySide.Right, SafetySide.Both })
            {
                var system = Scenario(label, side);
                var composite = system.Composite;
                Assert.NotNull(composite);
                var interiorStart = Math.Min(composite.GapStartX, composite.GapEndX);
                var interiorEnd = Math.Max(composite.GapStartX, composite.GapEndX);

                foreach (var boot in new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                             .Where(instance => instance.Role == HeaderBlockRole.Safety
                                 && string.Equals(instance.PieceId, BootId(Catalog), StringComparison.OrdinalIgnoreCase)))
                {
                    Assert.False(
                        boot.Insertion.X > interiorStart + 1e-6 && boot.Insertion.X < interiorEnd - 1e-6,
                        $"{label}/{side}: bota en X={boot.Insertion.X:0.###}, dentro de la interfaz "
                            + $"[{interiorStart:0.###},{interiorEnd:0.###}]");
                }
            }
        }

        /// <summary>Un blanco no convierte una eleccion en la contraria: quita piezas, nunca las cambia de mano.</summary>
        [Fact]
        public void BootBlank_DoesNotRelocateToOppositeMembership()
        {
            foreach (var side in new[] { SafetySide.Left, SafetySide.Right })
            {
                var full = Boots(Scenario("A+B encontradas", side));
                var blanked = Boots(Scenario("blank A", side));
                var opposite = Boots(Scenario("A+B encontradas", side == SafetySide.Left ? SafetySide.Right : SafetySide.Left));

                Assert.NotEmpty(blanked);
                // Cada bota que sobrevive al blanco existia ya, con la MISMA mano…
                Assert.All(blanked, boot => Assert.Contains(boot, full));
                // …y ninguna es de la eleccion contraria.
                Assert.All(blanked, boot => Assert.DoesNotContain(boot, opposite));
            }
        }

        /// <summary>El blanco conserva la retícula: no compacta indices ni mueve la seguridad de las lineas que cargan.</summary>
        [Fact]
        public void BootBlank_PreservesPhysicalGrid()
        {
            var full = Scenario("A+B encontradas", SafetySide.Both);
            var blanked = Scenario("blank A", SafetySide.Both);

            Assert.Equal(full.Structure.Fronts.Count, blanked.Structure.Fronts.Count);
            Assert.Equal(full.Structure.TotalLength, blanked.Structure.TotalLength, 6);
            Assert.All(Boots(blanked), boot => Assert.Contains(boot, Boots(full)));
        }

        /// <summary>Que exista lado B en otra zona no expande la pertenencia de las botas.</summary>
        [Fact]
        public void PartialComposite_DoesNotExpandBootMembership()
        {
            foreach (var side in new[] { SafetySide.Left, SafetySide.Right, SafetySide.Both })
            {
                var partial = Boots(Scenario("parcial compuesto", side));
                var full = Boots(Scenario("A+B encontradas", side));
                Assert.NotEmpty(partial);
                Assert.All(partial, boot => Assert.Contains(boot, full));
            }
        }

        // ---- dibujo y BOM ----------------------------------------------------------------------------------------

        /// <summary>El BOM cuenta exactamente las botas dibujadas, con cualquier opcion del selector.</summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void DrawAndBomBootsAgree(string label)
        {
            foreach (var side in new[] { SafetySide.None, SafetySide.Left, SafetySide.Right, SafetySide.Both })
            {
                var system = Scenario(label, side);
                Assert.Equal(Boots(system).Count, BomBoots(system));
            }
        }

        /// <summary>
        /// El DEFECTO de un rack nuevo no cambia: sigue siendo Izquierda, que es lo que dibujaba antes de esta
        /// ronda. Ningun documento existente cambia por si solo — solo el que elige Derecha o Ambas.
        /// </summary>
        [Fact]
        public void NewRackDefault_IsStillLeft()
        {
            var boot = new PushBackSafetyAuthority(Catalog).Defaults()
                .Single(selection => string.Equals(selection.ElementId, BootId(Catalog), StringComparison.OrdinalIgnoreCase));

            Assert.Equal(SafetySide.Left, boot.Side);
            Assert.True(boot.LowEndOnly);
        }
    }
}
