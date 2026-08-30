using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
    /// I-42 (ronda 7B) — LA DEFENSA DE MONTACARGAS SE DECIDE POR POSTE FÍSICO.
    ///
    /// <para>
    /// La ronda 7 propuso dos interruptores por LADO. El dueño lo rechazó: dentro de un mismo lado puede querer
    /// P1 sí, P2 no, P3 sí. Y la granularidad correcta <b>ya existía</b> en el producto —
    /// <c>SelectiveSafetySelection.DefensaPosts</c>, una entrada <c>SafetyPostDefense</c> por poste, editable desde
    /// la ventana «Elementos de seguridad» con <c>SafetyDefensaGridWindow</c> («Defensa de montacargas por poste»)—.
    /// El contrato de la ronda 7 se retiró; estas pruebas fijan el que el producto ya tenía.
    /// </para>
    /// <para>
    /// La identidad del poste es su <c>PostIndex</c>, la LÍNEA transversal física. Es estable por construcción: un
    /// blanco conserva su ranura y no compacta la retícula (ronda 2), así que el índice de una línea no se mueve.
    /// </para>
    /// <para>
    /// Los tres ejes siguen separados: INTENCIÓN (esta matriz), APLICABILIDAD (la física de la ronda 6D: sólo una
    /// cara de ataque real) y COLOCACIÓN (la geometría). Una intención sobre una cara que no existe no dibuja nada y
    /// no se traslada a ningún otro poste.
    /// </para>
    /// </summary>
    public class PushBackDefensePerPostTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Las lineas transversales del escenario base: cuatro ranuras dan cinco.</summary>
        private const int PostCount = 5;

        private static string DefenseId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;

        /// <summary>Los ajustes con la defensa declarada POR POSTE: los que estén en <paramref name="on"/>.</summary>
        private static PushBackEditorInputs Inputs(params int[] on)
        {
            var catalog = Catalog;
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                var element = catalog.SafetyElements?.FirstOrDefault(entry =>
                    string.Equals(entry?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                if (element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType)
                    && on != null)
                {
                    // Asi es como el editor POR POSTE lo expresa desde siempre: una entrada por poste, con la
                    // LONGITUD de cada extremo. Longitud cero = ese poste no lleva defensa en esa cara. Un poste SIN
                    // entrada sigue la regla automatica de 12"/36", que es el comportamiento legacy.
                    selection.DefensaPosts.Clear();
                    for (var post = 0; post < PostCount; post++)
                    {
                        var enabled = on.Contains(post);
                        selection.DefensaPosts.Add(new SafetyPostDefense
                        {
                            PostIndex = post,
                            ExitLength = enabled ? 36.0 : 0.0,
                            EntranceLength = enabled ? 36.0 : 0.0
                        });
                    }
                }

                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        private static PushBackCompositeEditorState State(
            int slots = 4, IReadOnlyCollection<int> blanksA = null, IReadOnlyCollection<int> blanksB = null)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            foreach (var slot in blanksA ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.A, slot, false)); }
            foreach (var slot in blanksB ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false)); }
            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state, PushBackEditorInputs inputs)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        private static PushBackDesign Design(PushBackCompositeEditorState state, PushBackEditorInputs inputs)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog);
            Assert.NotNull(computation.Design);
            return computation.Design;
        }

        private static PushBackDesign RoundTrip(PushBackDesign design)
            => JsonSerializer.Deserialize<PushBackDesignDocument>(
                JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design))).ToDomain();

        // ---- lecturas -------------------------------------------------------------------------------------------

        private static IReadOnlyList<HeaderBlockInstance> DefenseInstances(PushBackSystem system)
        {
            var catalog = Catalog;
            var id = DefenseId(catalog);
            return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety
                    && string.Equals(instance.PieceId, id, StringComparison.OrdinalIgnoreCase))
                .OrderBy(instance => instance.Insertion.Y).ThenBy(instance => instance.Insertion.X)
                .ToList();
        }

        /// <summary>Las LÍNEAS físicas donde hay defensa, por su índice transversal.</summary>
        private static IReadOnlyList<int> DefenseLines(PushBackSystem system)
        {
            var positions = DynamicFrontGeometry.Compute(system.Structure, Catalog).PostPositions;
            var lines = new SortedSet<int>();
            foreach (var defense in DefenseInstances(system))
            {
                for (var line = 0; line < positions.Count; line++)
                {
                    if (Math.Abs(positions[line] - defense.Insertion.Y) < 12.0)
                    {
                        lines.Add(line);
                        break;
                    }
                }
            }

            return lines.ToList();
        }

        private static int BomDefenses(PushBackSystem system)
        {
            var catalog = Catalog;
            var id = DefenseId(catalog);
            return PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => string.Equals(component.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                .Sum(component => component.Quantity);
        }

        // ---- el contrato ----------------------------------------------------------------------------------------

        /// <summary>La intención es del POSTE: se dibuja exactamente en las líneas declaradas, en ninguna más.</summary>
        [Fact]
        public void DefenseIntent_IsPerPhysicalPost()
        {
            var system = Build(State(), Inputs(0, 2));
            Assert.Equal(new[] { 0, 2 }, DefenseLines(system));
        }

        /// <summary>Dentro de un mismo rack, P1 sí / P2 no / P3 sí: postes vecinos pueden diferir.</summary>
        [Fact]
        public void DefenseIntent_SameSidePostsCanDiffer()
        {
            var system = Build(State(), Inputs(1, 3));
            Assert.Equal(new[] { 1, 3 }, DefenseLines(system));
            Assert.DoesNotContain(0, DefenseLines(system));
            Assert.DoesNotContain(2, DefenseLines(system));
        }

        /// <summary>Y tocar un poste no toca a su vecino.</summary>
        [Fact]
        public void DefenseIntent_Post1DoesNotChangePost2()
        {
            var only1 = DefenseLines(Build(State(), Inputs(1)));
            var both = DefenseLines(Build(State(), Inputs(1, 2)));

            Assert.Equal(new[] { 1 }, only1);
            Assert.Equal(new[] { 1, 2 }, both);
        }

        /// <summary>La identidad usada es la LÍNEA física, y sobrevive a un blanco: la retícula no se compacta.</summary>
        [Fact]
        public void DefenseIntent_UsesStablePostIdentity()
        {
            var plain = Build(State(), Inputs(2));
            var blanked = Build(State(blanksA: new[] { 0 }), Inputs(2));

            Assert.Equal(new[] { 2 }, DefenseLines(plain));
            Assert.Equal(new[] { 2 }, DefenseLines(blanked));
            Assert.Equal(plain.Structure.Fronts.Count, blanked.Structure.Fronts.Count);
        }

        /// <summary>Cambiar de lado activo no toca la intención: no es una propiedad del lado.</summary>
        [Fact]
        public void DefenseIntent_SurvivesSideSwitch()
        {
            var state = State();
            var inputs = Inputs(0, 3);
            var before = DefenseLines(Build(state, inputs));

            state.SetActiveSide(PushBackSide.B);
            state.SetActiveSide(PushBackSide.A);

            Assert.Equal(before, DefenseLines(Build(state, inputs)));
        }

        /// <summary>Y sobrevive al guardado y a RACKEDITAR, por poste.</summary>
        [Fact]
        public void DefenseIntent_SurvivesRackEditar()
        {
            var state = State();
            var inputs = Inputs(1, 3);
            var design = Design(state, inputs);
            var reloaded = RoundTrip(design);

            var stored = reloaded.Structure.SafetySelections
                .Single(selection => string.Equals(selection.ElementId, DefenseId(Catalog), StringComparison.OrdinalIgnoreCase));

            // La matriz POR POSTE viaja completa: cinco entradas, y el ON/OFF de cada una es su longitud.
            Assert.Equal(
                Enumerable.Range(0, PostCount).ToArray(),
                stored.DefensaPosts.Select(post => post.PostIndex).OrderBy(index => index).ToArray());
            Assert.Equal(
                new[] { 1, 3 },
                stored.DefensaPosts.Where(post => post.ExitLength > 0.0)
                    .Select(post => post.PostIndex).OrderBy(index => index).ToArray());

            Assert.Equal(
                DefenseLines(Build(state, inputs)),
                DefenseLines(new PushBackResolver(Catalog).Resolve(reloaded)));
        }

        /// <summary>Un cambio de geometría ajeno —el hueco— no mueve ninguna intención.</summary>
        [Fact]
        public void DefenseIntent_SurvivesUnrelatedGeometryChange()
        {
            var state = State();
            var inputs = Inputs(0, 2);
            var before = DefenseLines(Build(state, inputs));

            state.SetGap(48.0);
            Assert.Equal(before, DefenseLines(Build(state, inputs)));
        }

        /// <summary>
        /// Un blanco NO relocaliza: la intención de un poste no salta al siguiente, ni al lado contrario, ni a la
        /// interfaz. La regla física de la ronda 6D sigue mandando sobre dónde puede materializarse.
        /// </summary>
        [Fact]
        public void DefenseIntent_BlankDoesNotRelocate()
        {
            var state = State(blanksA: new[] { 0, 1 });
            var inputs = Inputs(0, 1, 2, 3, 4);
            var system = Build(state, inputs);
            var total = system.Structure.TotalLength;

            Assert.NotEmpty(DefenseInstances(system));
            Assert.All(DefenseInstances(system), defense => Assert.True(
                defense.X() <= 0.0 + 1e-6 || defense.X() >= total - 1e-6,
                $"defensa en X={defense.X():0.###}, que no es un extremo del rack"));
        }

        /// <summary>Una intención sobre una cara que no aplica no crea pieza… ni en el dibujo ni en el BOM.</summary>
        [Fact]
        public void DefenseIntent_NonApplicableDoesNotCreatePiece()
        {
            var state = State(blanksA: new[] { 0, 1 });
            var withAll = Build(state, Inputs(0, 1, 2, 3, 4));
            var total = withAll.Structure.TotalLength;

            // Las lineas 0 y 1 pierden su cara de A: su intencion no crea ninguna pieza AHI, y desde luego no en la
            // interfaz. La cara de B de esas mismas lineas si existe, y esa si se materializa — que es exactamente lo
            // que separa INTENCION de APLICABILIDAD.
            Assert.All(DefenseInstances(withAll), defense => Assert.True(
                defense.Insertion.X <= 0.0 + 1e-6 || defense.Insertion.X >= total - 1e-6,
                $"defensa en X={defense.Insertion.X:0.###}, que no es una cara de ataque"));
            Assert.Equal(DefenseInstances(withAll).Count, BomDefenses(withAll));
        }

        [Theory]
        [InlineData(new int[0])]
        [InlineData(new[] { 0 })]
        [InlineData(new[] { 1, 3 })]
        [InlineData(new[] { 0, 1, 2, 3, 4 })]
        public void DefenseIntent_DrawEqualsBom(int[] on)
        {
            var system = Build(State(), Inputs(on));
            Assert.Equal(DefenseInstances(system).Count, BomDefenses(system));
        }

        /// <summary>
        /// Un documento LEGACY —sin ninguna entrada por poste— dibuja exactamente lo que dibujaba: la regla
        /// automática de siempre. No se reinterpreta como una selección por poste.
        /// </summary>
        [Fact]
        public void LegacyDefense_PreservesPhysicalOutput()
        {
            var state = State();
            var legacy = Inputs(null);   // sin tocar DefensaPosts: es el documento de siempre
            var design = Design(state, legacy);
            var stored = design.Structure.SafetySelections
                .Single(selection => string.Equals(selection.ElementId, DefenseId(Catalog), StringComparison.OrdinalIgnoreCase));

            Assert.Empty(stored.DefensaPosts);
            Assert.Equal(
                DefenseLines(Build(state, legacy)),
                DefenseLines(new PushBackResolver(Catalog).Resolve(RoundTrip(design))));
        }

        /// <summary>
        /// Un poste que deja de existir no deja defensa fantasma, y uno nuevo toma el DEFECTO definido —la regla
        /// automática—, no la intención de otro.
        /// </summary>
        [Fact]
        public void RemovedPost_DoesNotLeaveGhostDefense_AndNewPostUsesDefault()
        {
            var wide = State(slots: 4);
            var inputs = Inputs(0, 4);
            Assert.Contains(4, DefenseLines(Build(wide, inputs)));

            // El rack encoge: la linea 4 ya no existe.
            var narrow = State(slots: 2);
            var system = Build(narrow, inputs);
            var positions = DynamicFrontGeometry.Compute(system.Structure, Catalog).PostPositions;

            Assert.True(positions.Count <= 3);
            Assert.All(DefenseLines(system), line => Assert.True(line < positions.Count));
            Assert.Equal(DefenseInstances(system).Count, BomDefenses(system));
        }
    }

    internal static class DefenseInstanceExtensions
    {
        public static double X(this HeaderBlockInstance instance) => instance.Insertion.X;
    }
}
