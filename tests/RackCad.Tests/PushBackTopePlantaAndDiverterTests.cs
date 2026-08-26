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
    /// I-42 (ronda post-82e918b) — TOPES en toda la planta, DESVIADOR en el pasillo real y una sola autoridad
    /// vertical para el tope.
    /// </summary>
    public class PushBackTopePlantaAndDiverterTests
    {
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs(bool safety = false)
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            if (safety)
            {
                foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
                {
                    inputs.SafetySelections.Add(selection);
                }
            }

            return inputs;
        }

        private static PushBackSystem Build(
            PushBackCellTopology topology,
            PushBackRunDirection direction,
            int slots = 4,
            int levels = 3,
            bool safety = false)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetDefaults(topology, direction);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var slot = 0; slot < matrix.Count; slot++)
                {
                    state.Of(side).AdjustLevels(slot, levels - matrix.Fronts[slot].LoadLevels);
                }
            }

            return new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(safety), Catalog).System;
        }

        // ===================== B: los topes en PLANTA, de TODOS los frentes ===================================

        /// <summary>
        /// La PLANTA materializa un tope por FRENTE aplicable, no solo el del primero.
        ///
        /// <para>
        /// En planta la X corre con la profundidad y la Y con la retícula transversal, así que un frente se
        /// identifica por su Y. Se buscaba «el frente cuyo EndX está más cerca» y, como todos los frentes comparten
        /// profundidad, eso devolvía siempre el mismo: en un rack compuesto —donde cada cama se dibuja sobre una
        /// copia con una sola ranura activa— caía en un frente EN BLANCO y no emitía tope. Se cuenta POR LÍNEA, no
        /// con un NotEmpty.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 1)]
        [InlineData(PushBackCellTopology.SoloB, PushBackRunDirection.AToB, 1)]
        [InlineData(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, 1)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, 1)]
        public void TopesEnPlanta_MultiFront_AllApplicableFrontsAppear(
            PushBackCellTopology topology, PushBackRunDirection direction, int perFront)
        {
            const int slots = 4;
            var system = Build(topology, direction, slots: slots);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var planta = new PushBackSystemPlantaBuilder().Build(system, Catalog);
            var topes = planta.Where(i => i.Role == HeaderBlockRole.Tope).ToList();

            Assert.Equal(slots * perFront, topes.Count);

            // Y uno por FRENTE: cada tope cae en la banda transversal de su frente, y ninguno se queda sin el suyo.
            for (var front = 0; front < slots; front++)
            {
                var centre = (layout.PostPositions[front] + layout.PostPositions[front + 1]) / 2.0;
                var half = Math.Abs(layout.PostPositions[front + 1] - layout.PostPositions[front]) / 2.0;
                Assert.Equal(
                    perFront,
                    topes.Count(tope => Math.Abs(tope.Insertion.Y - centre) <= half));
            }
        }

        /// <summary>
        /// Y el larguero posterior de cada frente lleva SU peralte, no el del primero: era el mismo defecto de
        /// identificación, y movía una medida física.
        /// </summary>
        [Fact]
        public void ThePlantaHighBeam_CarriesItsOwnFrontPeralte()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6, LoadLevels = 2, FirstLevelHeight = 6.0, BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1
            });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1
            });

            var custom = new PushBackFrontConfig();
            custom.HighEndBeamPeraltes.Add(5.0);
            custom.HighEndBeamPeraltes.Add(5.0);
            design.Fronts.Add(custom);   // solo el frente 0 lleva 5"

            var system = new PushBackResolver(Catalog).Resolve(design);
            var peraltes = new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(i => string.Equals(i.PieceId, Redondo, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Insertion.Y)
                .Select(i => Math.Round(i.DynamicParameters[SelectiveRackDefaults.PeralteParam], 3))
                .ToList();

            Assert.Equal(2, peraltes.Count);
            Assert.Equal(5.0, peraltes[0], 3);
            Assert.NotEqual(peraltes[0], peraltes[1]);   // el segundo frente NO hereda el peralte del primero
        }

        // ===================== C: una sola autoridad vertical del tope =======================================

        /// <summary>
        /// La regla vertical del tope es UNA y esta: «rise-and-snap canónico + X troqueles» sobre SU larguero alto.
        /// Se comprueba que la elevación dibujada es exactamente eso, medido desde la elevación DERIVADA del
        /// larguero — no desde la que el resolver compartido dio al nivel.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.BToA)]
        public void RearTope_Z_IsDerivedFromTheHighBeamRule(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Build(topology, direction, slots: 2, levels: 2);
            var runs = PushBackRuns.Resolve(system);
            Assert.NotEmpty(runs.Runs);

            var checkedTopes = 0;
            foreach (var batch in PushBackCompositeContent.Batches(runs, includeSlot: null))
            {
                var source = batch.Source;
                // La MISMA base que usa el builder: comprobarlo contra otra seria comprobar una segunda autoridad.
                var grid = PushBackRearTopeBuilder.PostTroquelGridBase(source.Structure, Catalog, "LATERAL");
                var high = PushBackElevations.HighInsertions(source, Catalog, batch.Front);
                var drawn = new PushBackRearTopeBuilder()
                    .BuildLateral(source, Catalog, batch.FrontIndex, batch.Front, batch.Levels)
                    .Select(i => Math.Round(i.Insertion.Y, 6))
                    .OrderBy(y => y)
                    .ToList();

                var expected = batch.Levels
                    .Where(level => high.ContainsKey(level))
                    .Select(level => Math.Round(PushBackRearTopeBuilder.ElevationY(grid, high[level]), 6))
                    .OrderBy(y => y)
                    .ToList();

                Assert.Equal(expected, drawn);
                checkedTopes += drawn.Count;

                // Y la regla es «X troqueles»: el extra es un multiplo entero del paso.
                var steps = PushBackRearTopeBuilder.ExtraRise / SelectiveRackDefaults.TroquelPaso;
                Assert.Equal(Math.Round(steps), steps, 9);
            }

            Assert.True(checkedTopes > 0);
        }

        // ===================== D: el desviador, solo en el pasillo por el que se carga =======================

        public static IEnumerable<object[]> DiverterCases() => new[]
        {
            new object[] { PushBackCellTopology.SoloA, PushBackRunDirection.AToB, true, false },
            new object[] { PushBackCellTopology.SoloB, PushBackRunDirection.AToB, false, true },
            new object[] { PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, true, true },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.AToB, true, false },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.BToA, false, true },
        };

        /// <summary>
        /// Un desviador guía la tarima AL ENTRAR, así que vive en el extremo por el que se CARGA y en ningún otro.
        /// En una corrida eso significa el extremo del larguero de entrada/salida, NUNCA el del posterior.
        /// </summary>
        [Theory]
        [MemberData(nameof(DiverterCases))]
        public void Diverter_OnlyOnTheAislesThatAreLoaded(
            PushBackCellTopology topology, PushBackRunDirection direction, bool atStart, bool atEnd)
        {
            var system = Build(topology, direction, slots: 2, levels: 2, safety: true);
            var total = system.Structure.TotalLength;
            var xs = new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances
                .Where(i => (i.PieceId ?? string.Empty).IndexOf("DESVIADOR", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(i => Math.Round(i.Insertion.X, 3))
                .Distinct()
                .ToList();

            Assert.NotEmpty(xs);
            Assert.Equal(atStart, xs.Any(x => Math.Abs(x) < 1.0));
            Assert.Equal(atEnd, xs.Any(x => Math.Abs(x - total) < 1.0));
            Assert.All(xs, x => Assert.True(
                Math.Abs(x) < 1.0 || Math.Abs(x - total) < 1.0,
                $"{topology}/{direction}: desviador a X={x}, que no es ningun pasillo"));
        }

        /// <summary>
        /// Un rack compuesto PARCIAL no reparte el desviador a las líneas de los frentes que siguen siendo de un
        /// solo sentido: el pasillo se declara POR LÍNEA.
        /// </summary>
        [Fact]
        public void PartialComposite_Diverter_StaysOnTheLinesThatLoad()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(4);
            var before = new PushBackCompositeEditorAssembler(Catalog)
                .Build(state, Inputs(safety: true), Catalog).System;

            IReadOnlyList<double> Diverters(PushBackSystem system, int line)
                => new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                    .Where(i => (i.PieceId ?? string.Empty).IndexOf("DESVIADOR", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(i => Math.Round(i.Insertion.X, 3))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

            var reference = new[] { 3, 4 }.ToDictionary(line => line, line => Diverters(before, line));

            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            var after = new PushBackCompositeEditorAssembler(Catalog)
                .Build(state, Inputs(safety: true), Catalog).System;

            foreach (var line in new[] { 3, 4 })
            {
                Assert.Equal(reference[line], Diverters(after, line));
            }
        }

        // ===================== E/F: la seguridad del otro pasillo, y su mano =================================

        /// <summary>
        /// En un rack compuesto la seguridad por defecto aparece en LOS DOS pasillos —incluida la defensa de
        /// montacargas, que se había quedado en uno solo— y la copia del pasillo lejano es la IMAGEN ESPEJO.
        /// </summary>
        [Fact]
        public void Composite_DefaultSafety_AppearsOnBothAislesMirrored()
        {
            var system = Build(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 3, safety: true);
            var total = system.Structure.TotalLength;
            var lateral = new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances
                .Where(i => i.Role == HeaderBlockRole.Safety)
                .ToList();

            var families = lateral.Select(i => i.PieceId).Distinct().ToList();
            Assert.Contains(families, id => id.IndexOf("DEFENSA", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var family in families)
            {
                var pieces = lateral.Where(i => string.Equals(i.PieceId, family, StringComparison.Ordinal)).ToList();
                var near = pieces.Where(i => i.Insertion.X < total / 2.0).ToList();
                var far = pieces.Where(i => i.Insertion.X > total / 2.0).ToList();

                Assert.NotEmpty(near);
                Assert.True(far.Count > 0, $"{family}: no aparece en el pasillo lejano");
                Assert.Equal(
                    near.Select(i => i.MirroredX).Distinct().Select(hand => !hand).OrderBy(h => h).ToList(),
                    far.Select(i => i.MirroredX).Distinct().OrderBy(h => h).ToList());
            }
        }

        /// <summary>
        /// Y NO en todos los postes: la pertenencia adaptativa sigue siendo la de siempre — solo las dos líneas de
        /// orilla llevan protector, nunca una interior. Es el error viejo que no puede volver.
        /// </summary>
        [Fact]
        public void Composite_Safety_NotEveryPost()
        {
            var system = Build(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, slots: 4, safety: true);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var guards = new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety)
                .Where(i => (i.PieceId ?? string.Empty).IndexOf("PROTECTOR_LATERAL", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(i => Math.Round(i.Insertion.Y, 2))
                .Distinct()
                .ToList();

            Assert.NotEmpty(guards);
            var edges = new[] { layout.PostPositions[0], layout.PostPositions[layout.PostPositions.Count - 1] };
            Assert.All(guards, y => Assert.True(
                edges.Any(edge => Math.Abs(edge - y) < 2.0),
                $"protector lateral en Y={y}, que no es una linea de orilla"));
        }
    }
}
