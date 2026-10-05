using System;
using System.Linq;
using RackCad.Application.ComputedParameters;
using Xunit;
using Xunit.Abstractions;
using static RackCad.Tests.ComputedParametersSummaryKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G4 - rendimiento (D-24) e INV-30. La caracterizacion es sintetica y de Core: N RackIds con TRES vistas cada
    /// uno, el nivel <c>Population</c> frente a <c>Full</c>. El oraculo son los CONTADORES inyectados (resoluciones,
    /// lecturas de diseno en el costado del lector D-26 y evaluaciones de poblacion); los tiempos se miden y se
    /// registran en la salida, NUNCA se asertan.
    /// </summary>
    public class ComputedParametersSummaryPerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public ComputedParametersSummaryPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void INV30_NRacksConTresVistas_DanNResolucionesEnFull_YCeroEnPopulation_NuncaTresN()
        {
            const int racks = 20;
            var captures = SelectiveRacksWithThreeViews(racks);
            Assert.Equal(racks * 3, captures.Count);

            var population = Population(captures);
            var full = Full(captures);

            // Population nunca resuelve metricas (contador del costado de resolucion REAL).
            Assert.Equal(racks, population.Population.Racks.Count);
            Assert.Equal(0, population.ScopedResolutions);

            // Full: una resolucion por RackId, no una por vista (3N).
            Assert.Equal(racks, full.Summary.Racks.Count);
            Assert.Equal(racks, full.Resolutions);
            Assert.NotEqual(racks * 3, full.Resolutions);
            Assert.Equal(racks, full.ScopedResolutions);
            Assert.All(full.Summary.Racks, rack => Assert.Equal(4.0, rack.Metrics[RackMetricIds.Frentes].Value));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void D24_Caracterizacion_PopulationFrenteAFull_ConContadoresComoOraculo(int racks)
        {
            var captures = SelectiveRacksWithThreeViews(racks);
            Assert.Equal(racks * 3, captures.Count);

            var population = Population(captures);
            var full = Full(captures);

            _output.WriteLine(
                "N=" + racks + " vistas=3 | Population: ms=" + population.Milliseconds + " lecturas=" + population.Reads
                + " evaluacionesPoblacion=" + population.PopulationEvaluations + " resoluciones=" + population.ScopedResolutions);
            _output.WriteLine(
                "N=" + racks + " vistas=3 | Full: ms=" + full.Milliseconds + " lecturas=" + full.Reads
                + " evaluacionesPoblacion=" + full.PopulationEvaluations + " resoluciones=" + full.Resolutions);

            // Una captura, una evaluacion de poblacion por peticion, en los dos niveles.
            Assert.Equal(1, population.PopulationEvaluations);
            Assert.Equal(1, full.PopulationEvaluations);

            // Population: ninguna resolucion de metricas. Full: una por RackId (N), nunca por vista (3N).
            Assert.Equal(0, population.ScopedResolutions);
            Assert.Equal(racks, full.Resolutions);
            Assert.Equal(racks, full.ScopedResolutions);

            // Lecturas de diseno: UNA por vista y peticion; Full no repite las de la pertenencia.
            Assert.Equal(racks * 3, population.Reads);
            Assert.Equal(population.Reads, full.Reads);

            // El resultado es el esperado en los dos niveles.
            Assert.Equal(racks, population.Population.Racks.Count);
            Assert.Equal(racks, full.Summary.Racks.Count);
            Assert.True(full.Summary.Totals.TotalRacks.Value.Equals((double)racks));
            Assert.All(full.Summary.Racks, rack => Assert.Equal(MetricStatus.Available, rack.Metrics[RackMetricIds.Frentes].Status));
            Assert.Equal(
                4.0 * racks,
                full.Summary.BySystem.Single(item => item.KindToken == RackCad.Application.Persistence.RackEmbedDocument.KindSelective).TotalFrentes.Value);
        }

        [Fact]
        public void D24_UnaSolaPeticionPorRack_RackMetricRequestResuelveComoMaximoUnRack_SinPoblacion()
        {
            var captures = SelectiveRacksWithThreeViews(5);
            var siblings = SiblingsOf(captures, "rack-0002");
            Assert.Equal(3, siblings.Count);

            var reader = new CountingReader();
            var side = new CountingSide();
            using (var population = RackPopulationEvaluationCounter.Begin())
            {
                var results = new RackMetricRequest(
                    siblings,
                    RackCad.Application.Persistence.ProjectVariablesReadResult.Absent(),
                    EmptyCatalog(),
                    reader,
                    side).Execute();

                Assert.Equal(MetricStatus.Available, results[RackMetricIds.Frentes].Status);
                Assert.Equal(1, side.Resolutions);
                Assert.Equal(3, reader.Reads);
                Assert.Equal(0, population.Count);
            }
        }
    }
}
