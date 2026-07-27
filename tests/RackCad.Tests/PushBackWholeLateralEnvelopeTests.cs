using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, ámbito ENVOLVENTE (I-32) — el lateral COMPLETO, el que no está seccionado por poste.
    ///
    /// Ese lateral no pertenece a ningún frente: dibuja el rack entero, así que ocupa la profundidad de la
    /// ENVOLVENTE del sistema. Sus largueros lo saben —se resuelven con <c>front: null</c>, con la longitud de cama
    /// del sistema completo—, pero su desviador y sus anotaciones no: leían la elevación del resolver y la del
    /// frente que gana la PROYECCIÓN, que es otro frente distinto.
    ///
    /// Proyección y envolvente son cosas distintas y no se pueden confundir:
    /// <list type="bullet">
    /// <item>la <b>proyección</b> es el frente que originó <c>system.LoadBeamLevels</c> — gana por número de
    /// niveles;</item>
    /// <item>la <b>envolvente</b> es el rack entero — su longitud de cama es la del sistema, no la de ningún
    /// frente.</item>
    /// </list>
    ///
    /// El fixture las separa a propósito: <b>F0 tiene MÁS niveles</b> (4 contra 3) y por tanto gana la proyección,
    /// pero <b>F1 es MÁS profundo</b> (6 fondos contra 4) y por tanto manda en la envolvente. El resolver proyecta
    /// F0; el lateral completo ocupa el fondo de F1. Con esa combinación las dos elevaciones caen en TROQUELES
    /// distintos, y nada de esto se da por supuesto: la primera prueba lo mide.
    /// </summary>
    public class PushBackWholeLateralEnvelopeTests
    {
        private const double Offset = SelectiveDesviadorPlan.BeamYOffset;   // 6"

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DesviadorId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DesviadorType)).Id;

        /// <summary>
        /// F0: 4 niveles, 4 fondos, inicio 1. F1: 3 niveles, 6 fondos, inicio 1.
        ///
        /// El fondo de tarima es 50" y no 48" por una razón medida: con 48" la elevación de la envolvente coincide
        /// EXACTAMENTE con la <c>ExitElevation</c> del resolver —la cama más larga baja el troquel justo lo que la
        /// diferencia de pendiente lo sube, y el ajuste cancela— así que el desviador acertaba por accidente y su
        /// prueba no podía fallar. Con 50" los TRES valores se separan y las cuatro afirmaciones son falsables.
        /// </summary>
        private static PushBackSystem Envelope(RackCatalog catalog)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 50.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 4,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0,
                    NumberLevels = true,
                    NumberFronts = true,
                    Dimensions = DimensionDetail.Detailed
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 4, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new PushBackFrontConfig());
            design.Fronts.Add(new PushBackFrontConfig());

            design.Structure.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = DesviadorId(catalog),
                Quantity = 1,
                Side = SafetySide.Left,   // solo la copia del extremo BAJO
                DesviadorLongitud = SelectiveSafetyDefaults.DesviadorLongitud,
                DesviadorPrimerNivelAltura = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura
            });

            return new PushBackResolver(catalog).Resolve(design);
        }

        /// <summary>Las elevaciones de la ENVOLVENTE: el sistema entero, sin elegir ningún frente.</summary>
        private static IReadOnlyDictionary<int, double> EnvelopeElevations(PushBackSystem system, RackCatalog catalog)
            => PushBackElevations.LowInsertions(system, catalog, null);

        /// <summary>Las elevaciones del frente que originó <c>system.LoadBeamLevels</c>.</summary>
        private static IReadOnlyDictionary<int, double> ProjectionElevations(PushBackSystem system, RackCatalog catalog)
            => PushBackElevations.LowInsertions(system, catalog, system.Structure.Fronts[ProjectedFront(system)]);

        private static int ProjectedFront(PushBackSystem system)
            => system.Structure.Fronts
                .OrderByDescending(front => front.LoadBeamLevels.Count)
                .ThenByDescending(front => front.EndX - front.StartX)
                .First().Index;

        private static List<HeaderBlockInstance> Of(IEnumerable<HeaderBlockInstance> instances, string pieceId)
            => instances.Where(i => string.Equals(i.PieceId, pieceId, StringComparison.OrdinalIgnoreCase)).ToList();

        // ---------------------------------------------------------------------------------------------------
        // El fixture tiene que separar de verdad proyección y envolvente.
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheFixture_SeparatesTheProjectionFromTheEnvelope()
        {
            var catalog = Catalog;
            var system = Envelope(catalog);

            // F0 gana la PROYECCIÓN por número de niveles…
            Assert.Equal(new[] { 4, 3 }, system.Structure.Fronts.Select(f => f.LoadBeamLevels.Count).ToArray());
            Assert.Equal(0, ProjectedFront(system));
            Assert.Equal(4, system.Structure.LoadBeamLevels.Count);

            // …pero F1 es el más PROFUNDO, y el lateral completo ocupa esa envolvente.
            var depth = system.Structure.Fronts.Select(f => f.EndX - f.StartX).ToList();
            Assert.True(depth[1] > depth[0], $"F1 debe ser más profundo que F0 ({depth[1]:F3} vs {depth[0]:F3})");
            Assert.True(
                PushBackFlowBedGeometry.ResolveBedLength(system, null)
                    > PushBackFlowBedGeometry.ResolveBedLength(system, system.Structure.Fronts[0]) + 1e-6,
                "la cama de la envolvente debe ser más larga que la del frente proyectado");

            // Y con eso las dos elevaciones caen en TROQUELES distintos. No se fija ninguna Y a mano: se piden las
            // dos a la autoridad y se comprueba que difieren, que es lo que hace útil a este fixture.
            var envelope = EnvelopeElevations(system, catalog);
            var projection = ProjectionElevations(system, catalog);
            Assert.NotEmpty(envelope);
            Assert.NotEmpty(projection);

            var shared = envelope.Keys.Intersect(projection.Keys).OrderBy(k => k).ToList();
            Assert.NotEmpty(shared);
            Assert.All(shared, level => Assert.True(
                Math.Abs(envelope[level] - projection[level]) > 1e-6,
                $"nivel {level}: envolvente y proyección coinciden ({envelope[level]:F4}); el fixture no discrimina"));

            // Y también difieren de la elevación del RESOLVER, que es la que leen hoy el desviador y las cotas. Sin
            // esto el desviador acertaría por accidente y su regresión no podría fallar (con fondo 48" pasa justo eso).
            var exit = system.Structure.LoadBeamLevels.ToDictionary(level => level.LevelNumber, level => level.ExitElevation);
            Assert.All(shared, level =>
            {
                Assert.True(Math.Abs(envelope[level] - exit[level]) > 1e-6,
                    $"nivel {level}: la envolvente coincide con ExitElevation ({exit[level]:F4}); el desviador acertaría por accidente");
                Assert.True(Math.Abs(projection[level] - exit[level]) > 1e-6,
                    $"nivel {level}: la proyección coincide con ExitElevation ({exit[level]:F4})");
            });

            // Y las tres son troqueles válidos: no es que una esté mal, es que son elevaciones distintas y legítimas.
            var grid = PushBackTroquelGrid.Base(system.Structure, catalog);
            Assert.All(shared, level =>
            {
                Assert.Equal(PushBackTroquelGrid.Snap(envelope[level], grid), envelope[level], 9);
                Assert.Equal(PushBackTroquelGrid.Snap(projection[level], grid), projection[level], 9);
            });
        }



        // ---------------------------------------------------------------------------------------------------
        // 1. El larguero bajo del lateral completo usa la elevación de la ENVOLVENTE
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheWholeLateralLowBeam_UsesTheEnvelopeElevation()
        {
            var catalog = Catalog;
            var system = Envelope(catalog);
            var envelope = EnvelopeElevations(system, catalog);
            var projection = ProjectionElevations(system, catalog);

            var drawn = Of(
                new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances,
                DynamicRackDefaults.InOutBeamCatalogId)
                .Select(beam => Math.Round(beam.Insertion.Y, 6))
                .Distinct()
                .OrderBy(y => y)
                .ToList();
            Assert.NotEmpty(drawn);

            Assert.Equal(envelope.Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList(), drawn);
            Assert.NotEqual(projection.Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList(), drawn);
        }

        // ---------------------------------------------------------------------------------------------------
        // 2. Su desviador superior tiene que colgar de ESE larguero
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheWholeLateralUpperDesviador_HangsFromTheEnvelopeLowBeam()
        {
            var catalog = Catalog;
            var system = Envelope(catalog);
            var envelope = EnvelopeElevations(system, catalog);

            var drawn = Of(
                new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances,
                DesviadorId(catalog));
            Assert.NotEmpty(drawn);

            // El nivel 1 conserva su contrato selectivo (primer troquel + altura) y se descarta por ser el más bajo.
            var upper = drawn.Select(i => Math.Round(i.Insertion.Y, 6)).OrderBy(y => y).Skip(1).ToList();
            var expected = system.Structure.LoadBeamLevels
                .Where(level => level.LevelNumber > 1 && envelope.ContainsKey(level.LevelNumber))
                .Select(level => Math.Round(envelope[level.LevelNumber] - Offset, 6))
                .OrderBy(y => y)
                .ToList();

            Assert.NotEmpty(expected);
            Assert.Equal(expected, upper);
        }

        // ---------------------------------------------------------------------------------------------------
        // 3. Sus cotas y etiquetas tienen que coincidir con el larguero que acompañan
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheWholeLateralDimensionsAndLabels_MatchTheBeamsTheyAnnotate()
        {
            var catalog = Catalog;
            var system = Envelope(catalog);
            var envelope = EnvelopeElevations(system, catalog);

            var instances = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;
            var expected = system.Structure.LoadBeamLevels
                .Where(level => envelope.ContainsKey(level.LevelNumber))
                .Select(level => Math.Round(envelope[level.LevelNumber], 6))
                .OrderBy(y => y)
                .ToList();
            Assert.NotEmpty(expected);

            var labels = instances
                .Where(i => i.Role == HeaderBlockRole.Annotation
                    && string.Equals(i.View, "LATERAL", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(i.Text, out _)
                    && i.Insertion.Y > 0.0)
                .Select(i => Math.Round(i.Insertion.Y, 6))
                .OrderBy(y => y)
                .ToList();
            Assert.Equal(expected, labels);

            var dimensionTops = instances
                .Where(i => i.Role == HeaderBlockRole.Dimension
                    && string.Equals(i.View, "LATERAL", StringComparison.OrdinalIgnoreCase)
                    && Math.Abs(i.ConnectionAnchor.X - i.Insertion.X) < 1e-9
                    && Math.Abs(i.Insertion.Y) < 1e-9)
                .Select(i => Math.Round(i.ConnectionAnchor.Y, 6))
                .Distinct()
                .ToList();
            Assert.All(expected, y => Assert.Contains(y, dimensionTops));

            // Y lo anotado es exactamente lo dibujado: es la propiedad que hace útil a una cota.
            var beams = Of(instances, DynamicRackDefaults.InOutBeamCatalogId)
                .Select(beam => Math.Round(beam.Insertion.Y, 6)).Distinct().OrderBy(y => y).ToList();
            Assert.Equal(beams, labels);
        }

        // ---------------------------------------------------------------------------------------------------
        // 4. Los demás ámbitos NO cambian
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// El lateral SECCIONADO sigue preguntando por FRENTE: su columna baja pertenece a un frente concreto, no a
        /// la envolvente. En este fixture eso se distingue, porque F0 y la envolvente derivan troqueles distintos.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void TheSectionedLateral_StillAsksPerFront(int postIndex)
        {
            var catalog = Catalog;
            var system = Envelope(catalog);

            var lowFront = DynamicFrontGeometry.AdjacentFronts(system.Structure, postIndex)
                .OrderBy(front => front.StartX).First();
            var perFront = PushBackElevations.LowInsertions(system, catalog, lowFront);
            var drawn = Of(
                new PushBackSystemLateralBuilder().Build(system, catalog, postIndex).Flatten().Instances,
                DesviadorId(catalog))
                .Select(i => Math.Round(i.Insertion.Y, 6)).OrderBy(y => y).Skip(1).ToList();

            var lowLevels = lowFront.LoadBeamLevels;
            var highLevels = DynamicFrontGeometry.AdjacentFronts(system.Structure, postIndex)
                .OrderByDescending(front => front.EndX).First().LoadBeamLevels;
            var count = Math.Min(
                DynamicFrontGeometry.LoadLevelsAtPost(system.Structure, postIndex),
                Math.Max(lowLevels.Count, highLevels.Count));

            var expected = new List<double>();
            for (var level = 1; level < count; level++)
            {
                expected.Add(Math.Round(perFront[lowLevels[Math.Min(level, lowLevels.Count - 1)].LevelNumber] - Offset, 6));
            }

            Assert.NotEmpty(expected);
            Assert.Equal(expected.OrderBy(y => y).ToList(), drawn);
        }

        /// <summary>
        /// El frontal global sigue leyendo la PROYECCIÓN, no la envolvente: recorre <c>system.LoadBeamLevels</c>, que
        /// es exactamente la lista del frente proyectado. Aquí las dos difieren, así que la prueba lo distingue.
        /// </summary>
        [Fact]
        public void TheGlobalFrontal_StillReadsTheProjection_NotTheEnvelope()
        {
            var catalog = Catalog;
            var system = Envelope(catalog);
            var projection = ProjectionElevations(system, catalog);
            var envelope = EnvelopeElevations(system, catalog);

            var instances = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances;

            var labels = instances
                .Where(i => i.Role == HeaderBlockRole.Annotation
                    && string.Equals(i.View, "FRONTAL", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(i.Text, out _)
                    && i.Insertion.Y > 0.0)
                .Select(i => Math.Round(i.Insertion.Y, 6))
                .OrderBy(y => y)
                .ToList();

            var expected = system.Structure.LoadBeamLevels
                .Where(level => projection.ContainsKey(level.LevelNumber))
                .Select(level => Math.Round(projection[level.LevelNumber], 6))
                .OrderBy(y => y)
                .ToList();

            Assert.NotEmpty(expected);
            Assert.Equal(expected, labels);
            Assert.NotEqual(
                envelope.Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList(),
                labels);
        }
    }
}
