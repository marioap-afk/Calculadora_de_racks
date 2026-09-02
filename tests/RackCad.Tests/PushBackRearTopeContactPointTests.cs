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
    /// I-42 (corrección aislada 5D, defecto A) — EL CONTACTO FÍSICO DEL TOPE NO SE MUEVE AL ESPEJAR EL BLOQUE.
    ///
    /// <para>
    /// El tope mata por su ORIGEN sobre un punto medido del poste, y ese punto es un SEMIANCHO: se mide hacia el
    /// lado al que mira la pieza. El SELECTIVO ya lo hace así desde siempre —<c>AtFront</c> suma el troquel y lleva
    /// <c>Mirror = true</c>; el otro lo resta y va sin espejo—, y el Push Back tomaba el signo del espejo de la
    /// COLOCACIÓN, que en el marco de una cama es una constante. Mientras el tope también iba siempre sin espejo las
    /// dos coincidían; desde la ronda 5B puede ir espejado, y entonces el bloque quedaba dibujado con su origen del
    /// lado contrario: desplazado DOS veces el punto medido (2 × 0.875" = 1.75"), que es lo que el dueño midió.
    /// </para>
    /// <para>
    /// Estas pruebas afirman el CONTACTO, no la inserción: la inserción cambia —debe cambiar— para que el contacto
    /// no se mueva. Es la distinción que el dueño cerró en esta ronda.
    /// </para>
    /// </summary>
    public class PushBackRearTopeContactPointTests
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

        private static PushBackSystem ShortCorrida()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCorridaDepth(1, 0, 2);
            return Build(state);
        }

        /// <summary>Una estructura de ocho fondos con camas de 3 a 8: mezcla las DOS manos en un mismo rack.</summary>
        private static PushBackSystem DepthLadder(bool reinforced = true)
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

        // ---- lecturas ------------------------------------------------------------------------------------------

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        private static IReadOnlyList<HeaderBlockInstance> LateralCuts(PushBackSystem system)
            => Enumerable.Range(0, (system.Structure?.Fronts?.Count ?? 0) + 1)
                .SelectMany(postIndex => new PushBackSystemLateralBuilder()
                    .Build(system, Catalog, postIndex).Flatten().Instances)
                .ToList();

        private static IReadOnlyList<HeaderBlockInstance> Planta(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances;

        /// <summary>El punto medido del poste que ancla el tope en <paramref name="view"/>, en local.</summary>
        private static double AnchorLocalX(PushBackSystem system, string view)
        {
            var postId = DynamicFrontGeometry.PostId(system.Structure, Catalog);
            var peralte = DynamicFrontGeometry.PostPeralte(system.Structure, Catalog, postId);
            var anchor = PushBackRearTopeBuilder.PostAnchorLocal(Catalog, postId, peralte, view);
            Assert.True(anchor.HasValue, "el poste no publica el punto que ancla el tope");
            return anchor.Value.X;
        }

        /// <summary>
        /// EL CONTACTO FÍSICO del tope: el punto medido del poste al que su origen se atornilla, leído desde la
        /// inserción hacia el lado al que la pieza mira. Espejar el bloque cambia ese lado, así que la inserción
        /// tiene que cambiar con él para que ESTE punto no se mueva.
        /// </summary>
        private static double Contact(HeaderBlockInstance tope, double anchorLocalX)
            => tope.Insertion.X + (tope.MirroredX ? -anchorLocalX : anchorLocalX);

        /// <summary>
        /// LA COMPROBACIÓN: el contacto de cada tope cae exactamente en la columna de un larguero ALTO de esa misma
        /// vista, lleve la mano que lleve. Antes de 5D los topes espejados caían 1.75" fuera de toda columna.
        /// </summary>
        private static (int Normal, int Mirrored) AssertContactsLandOnTheirColumn(
            string label, PushBackSystem system, IReadOnlyList<HeaderBlockInstance> view, string viewName)
        {
            var anchorLocalX = AnchorLocalX(system, viewName);
            var highId = HighBeamId(system);
            var columns = view
                .Where(instance => instance.Role == HeaderBlockRole.Beam
                    && string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase))
                .Select(instance => instance.Insertion.X)
                .ToList();
            Assert.NotEmpty(columns);

            var normal = 0;
            var mirrored = 0;
            foreach (var tope in view.Where(instance => instance.Role == HeaderBlockRole.Tope))
            {
                var contact = Contact(tope, anchorLocalX);
                Assert.True(
                    columns.Any(column => Math.Abs(column - contact) < 1e-6),
                    $"{label} [{viewName}]: el tope en X={tope.Insertion.X:0.###} (espejo {tope.MirroredX}) hace "
                        + $"contacto en X={contact:0.###}, y ahí no hay ningún larguero alto "
                        + $"(columnas: {string.Join(", ", columns.Distinct().OrderBy(x => x).Select(x => x.ToString("0.###")))})");
                if (tope.MirroredX) { mirrored++; } else { normal++; }
            }

            return (normal, mirrored);
        }

        // ---- la prueba fuerte que el dueño pidió ---------------------------------------------------------------

        /// <summary>
        /// <c>MirroringRearTope_PreservesPhysicalContactPoint</c> — mano normal, mano invertida, corrida corta, y los
        /// dos lados. En cada escenario el contacto de cada tope cae en la columna de su larguero alto, en el corte
        /// lateral y en la planta.
        /// </summary>
        public static IEnumerable<object[]> Scenarios() => new[]
        {
            new object[] { "escalera de fondos (las dos manos en un rack)" },
            new object[] { "sólo A" },
            new object[] { "sólo B" },
            new object[] { "encontradas" },
            new object[] { "corrida A→B" },
            new object[] { "corrida B→A" },
            new object[] { "corrida corta" }
        };

        private static PushBackSystem Scenario(string label)
        {
            switch (label)
            {
                case "escalera de fondos (las dos manos en un rack)": return DepthLadder();
                case "sólo A": return Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
                case "sólo B": return Composite(PushBackCellTopology.SoloB, PushBackRunDirection.AToB);
                case "encontradas": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
                case "corrida A→B": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
                case "corrida B→A": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
                default: return ShortCorrida();
            }
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void MirroringRearTope_PreservesPhysicalContactPoint(string label)
        {
            var system = Scenario(label);
            AssertContactsLandOnTheirColumn(label, system, LateralCuts(system), "LATERAL");
            AssertContactsLandOnTheirColumn(label, system, Planta(system), "PLANTA");
        }

        /// <summary>
        /// Y la prueba no es vacía: la escalera de fondos produce topes de LAS DOS manos en el mismo rack, que es
        /// exactamente la mezcla en la que el defecto se veía.
        /// </summary>
        [Fact]
        public void TheLadder_ProducesBothHands_SoTheContractIsExercised()
        {
            var system = DepthLadder();
            var counts = AssertContactsLandOnTheirColumn("escalera", system, LateralCuts(system), "LATERAL");

            Assert.True(counts.Normal > 0, "ningún tope sin espejo: la prueba no ejercita la mano habitual");
            Assert.True(counts.Mirrored > 0, "ningún tope espejado: la prueba no ejercita el caso del defecto");
        }

        /// <summary>
        /// LA MEDIDA DEL DEFECTO. En la escalera, la cama de 4 fondos acaba en un VANO, así que su larguero alto va
        /// sin espejo (regla 5B) y su tope espejado. El tope se inserta al OTRO lado de la columna —la inserción
        /// cambia— pero su contacto sigue siendo la columna, exactamente como el de una cama de mano normal.
        /// </summary>
        [Fact]
        public void MirroredTope_InsertsOnTheOtherSide_ButContactsTheSameColumn()
        {
            var system = DepthLadder();
            var anchorLocalX = AnchorLocalX(system, "LATERAL");
            var view = LateralCuts(system);
            var highId = HighBeamId(system);

            foreach (var column in new[] { 198.0, 294.0 })   // 198 acaba en vano (espejado), 294 en cabecera (normal)
            {
                // Los cortes repiten la misma pieza física; lo que importa es que no haya DOS respuestas distintas.
                var beam = view.Where(instance => instance.Role == HeaderBlockRole.Beam
                        && string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase)
                        && Math.Abs(instance.Insertion.X - column) < 1e-6)
                    .Select(instance => instance.MirroredX).Distinct().ToList();
                var tope = view.Where(instance => instance.Role == HeaderBlockRole.Tope
                        && Math.Abs(instance.Insertion.X - column) < 2.0)
                    .Select(instance => (instance.Insertion.X, instance.MirroredX)).Distinct().ToList();
                Assert.Single(beam);
                Assert.Single(tope);

                // El tope va con la mano CONTRARIA a la de su larguero (5B) …
                Assert.Equal(!beam[0], tope[0].MirroredX);
                // … se inserta al lado al que mira …
                Assert.Equal(column + (tope[0].MirroredX ? anchorLocalX : -anchorLocalX), tope[0].X, 6);
                // … y hace contacto en la columna, lleve la mano que lleve.
                Assert.Equal(column, tope[0].X + (tope[0].MirroredX ? -anchorLocalX : anchorLocalX), 6);
            }
        }
    }
}
