using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.Tests
{
    /// <summary>TEMPORARY perf measurement - delete after use.</summary>
    public class TempPerfMeasurementTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private readonly ITestOutputHelper output;

        public TempPerfMeasurementTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private static SelectivePalletDesign Design(int bayCount, int levelCount = 5)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };
            for (var b = 0; b < bayCount; b++)
            {
                var bay = new SelectiveBayDesign { FloorBeam = b % 4 == 0 };
                for (var l = 0; l < levelCount; l++)
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = 40.0 + (b % 3) * 4.0, Alto = 50.0 + (l % 2) * 10.0 },
                        PalletCount = 1 + (b + l) % 2,
                        BeamId = BeamId,
                        BeamPeralte = 4.0
                    });
                }
                design.Bays.Add(bay);
            }
            return design;
        }

        [Fact]
        public void Measure_TwentyBays_LateralAndPlanta()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var system = new SelectiveGeometryResolver().Resolve(Design(20), catalog);

            // Warm-up (JIT).
            new SelectiveLateralBuilder().Cortes(system, catalog);
            new SelectivePlantaBuilder().Build(system, catalog);

            var sw = Stopwatch.StartNew();
            var cortes = new SelectiveLateralBuilder().Cortes(system, catalog);
            sw.Stop();
            var lateralMs = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            var planta = new SelectivePlantaBuilder().Build(system, catalog);
            sw.Stop();
            var plantaMs = sw.Elapsed.TotalMilliseconds;

            // Each factory.Build returns a NEW RackFrameConfiguration instance; distinct references = distinct builds.
            var distinctCabeceras = new HashSet<object>(cortes.Select(c => (object)c.Cabecera)).Count;
            var horizontals = cortes[0].Cabecera.Horizontals.Count;
            var panels = cortes[0].Cabecera.BracingPanels.Count;

            output.WriteLine($"N=20 bays -> cortes: {cortes.Count}, distinct cabecera builds (lateral): {distinctCabeceras}");
            output.WriteLine($"cabecera[0]: {horizontals} horizontals, {panels} panels, height {cortes[0].Cabecera.Height}");
            output.WriteLine($"lateral Cortes(): {lateralMs:F3} ms; planta Build(): {plantaMs:F3} ms");
            output.WriteLine($"planta instance count: {planta.Count}");
            output.WriteLine($"lateral largueros per corte (post 1): {cortes[1].Largueros.Count}");

            // With uniform-ish heights, how many DISTINCT (height) values? (what memoization would collapse to)
            var distinctHeights = cortes.Select(c => Math.Round(c.Cabecera.Height, 4)).Distinct().Count();
            output.WriteLine($"distinct cabecera heights: {distinctHeights}");

            // Scaling check: N=40 and N=100
            foreach (var n in new[] { 40, 100 })
            {
                var sysN = new SelectiveGeometryResolver().Resolve(Design(n), catalog);
                sw.Restart();
                var cortesN = new SelectiveLateralBuilder().Cortes(sysN, catalog);
                sw.Stop();
                var latN = sw.Elapsed.TotalMilliseconds;
                sw.Restart();
                var plantaN = new SelectivePlantaBuilder().Build(sysN, catalog);
                sw.Stop();
                output.WriteLine($"N={n}: cortes={cortesN.Count} lateral={latN:F3} ms planta={sw.Elapsed.TotalMilliseconds:F3} ms plantaInstances={plantaN.Count}");
            }

            Assert.True(cortes.Count > 0);
        }
    }
}
