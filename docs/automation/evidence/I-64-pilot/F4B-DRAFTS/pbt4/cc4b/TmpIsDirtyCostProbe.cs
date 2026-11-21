#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// TEMPORARY measurement (precondition 6 of PB-T4b, package v4.5, R-T4-17; never committed): the cost of IsDirty of a full draft of the panel,
    /// clean and dirty (the scale of the user changed), over the fixtures of read-cost. IsDirty is `_pending.Count > 0 || !comparer.Equals(current,
    /// initial)` with the comparer of the full model. It ends in Assert.Fail(text) so that the numbers land in the TRX.
    /// </summary>
    public sealed class TmpIsDirtyCostProbe
    {
        private static object? _sink;

        private static (double Median, double P95, int N) Measure(Func<object> body)
        {
            for (var i = 0; i < 50; i++) _sink = body();
            var probe = Stopwatch.StartNew();
            for (var i = 0; i < 50; i++) _sink = body();
            var average = probe.Elapsed.TotalMilliseconds / 50.0;
            var n = (int)Math.Max(1000, Math.Min(20000, Math.Round(2000.0 / Math.Max(average, 0.0001))));
            var samples = new double[n];
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            for (var i = 0; i < n; i++)
            {
                var t0 = Stopwatch.GetTimestamp();
                _sink = body();
                var t1 = Stopwatch.GetTimestamp();
                samples[i] = (t1 - t0) * 1000.0 / Stopwatch.Frequency;
            }

            Array.Sort(samples);
            var median = n % 2 == 1 ? samples[n / 2] : (samples[n / 2 - 1] + samples[n / 2]) / 2.0;
            return (median, samples[(int)Math.Ceiling(0.95 * n) - 1], n);
        }

        private static string F(double value) => value.ToString("0.0000", CultureInfo.InvariantCulture);

        [Fact]
        public void Measure_the_cost_of_IsDirty_of_a_full_draft()
        {
            var lines = new List<string> { "copia=<el GREEN de PB-T4b> (IsDirty, R-T4-17, paquete v4.5)" };
            foreach (var fixture in new[] { "pb-01-base", "pb-07-combinado", "pb-20-ab" })
            {
                var text = PilotKit.Text(fixture);
                var clean = FullPrepareKit.DraftOf(FullPrepareKit.BaseOf(("A1", text)), text);
                var dirty = FullPrepareKit.DraftOf(FullPrepareKit.BaseOf(("A1", text)), text);
                FullPrepareKit.SetScale(dirty, 7.25);
                lines.Add("escenario " + fixture + " limpio-isdirty=" + clean.IsDirty + " sucio-isdirty=" + dirty.IsDirty);
                foreach (var (name, draft) in new[] { ("limpio", clean), ("sucio", dirty) })
                {
                    var m = Measure(() => draft.IsDirty);
                    lines.Add("medida | " + fixture + " | IsDirty " + name + " | mediana=" + F(m.Median) + " ms | p95=" + F(m.P95) + " ms | n=" + m.N.ToString(CultureInfo.InvariantCulture));
                }
            }

            Assert.Fail(string.Join("\n", lines));
        }
    }
}
