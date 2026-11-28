#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// TEMPORARY measurement (independent review of PB-T4a, OWNER-02; never committed): the cost of the PURE part of one read of
    /// the edit intention, after CaptureDrawing, on the BASE 5aaacce3 (pilot only). It replays what RIA.Read + NH do there with
    /// Application types only: the scan, the pilot reader and FromPilot. In the base every read (Begin and the five rereads) goes to
    /// the Primary, so one path covers the three cases. It ends in Assert.Fail(text) so that the numbers land in the TRX.
    /// </summary>
    public sealed class TmpReadCostProbe
    {
        private static object? _sink;

        private sealed class Scenario
        {
            public string Name = string.Empty;
            public string RackId = string.Empty;
            public List<RackSiblingScanFact> Facts = new List<RackSiblingScanFact>();
            public Dictionary<string, string?> Raw = new Dictionary<string, string?>(StringComparer.Ordinal);
            public int Definitions;
        }

        private static Scenario Build(string name, string fixture, int extraViews, int foreignRacks)
        {
            var text = PilotKit.Text(fixture);
            var id = new RackEmbedStore().Deserialize(text)!.Id!;
            var items = new List<IntentItem> { IntentKit.Member("A1", text) };
            var lateral = IntentKit.Variant(fixture, id, view: "lateral", section: 3);
            items.Add(IntentKit.Member("B2", lateral));
            for (var v = 0; v < extraViews; v++)
                items.Add(IntentKit.Member("V" + (v + 3).ToString(CultureInfo.InvariantCulture), IntentKit.Variant(fixture, id, view: "lateral", section: 4 + v)));
            for (var f = 0; f < foreignRacks; f++)
            {
                var foreignId = "33333333-3333-4333-8333-" + f.ToString("D12", CultureInfo.InvariantCulture);
                items.Add(IntentKit.Member("F" + f.ToString(CultureInfo.InvariantCulture), IntentKit.Variant("pb-01-base", foreignId)));
            }

            return new Scenario
            {
                Name = name,
                RackId = id,
                Facts = items.Select(i => i.Fact).ToList(),
                Raw = IntentKit.ToDictionary(items),
                Definitions = items.Count,
            };
        }

        // What the Plugin does after CaptureDrawing: the input over the scan, at the key of the Primary (A1).
        private static PushBackPilotIntentScan Scan(Scenario s) =>
            new PushBackPilotIntentScan(s.RackId, "A1", s.Facts, s.Raw, IntentKit.Evidence, null);

        private static (double Median, double P95, int N) Measure(Func<object> body)
        {
            for (var i = 0; i < 20; i++) _sink = body();
            var probe = Stopwatch.StartNew();
            for (var i = 0; i < 10; i++) _sink = body();
            var average = probe.Elapsed.TotalMilliseconds / 10.0;
            var n = (int)Math.Max(200, Math.Min(1000, Math.Round(3000.0 / Math.Max(average, 0.001))));
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
            var p95 = samples[(int)Math.Ceiling(0.95 * n) - 1];
            return (median, p95, n);
        }

        private static string F(double value) => value.ToString("0.000", CultureInfo.InvariantCulture);

        private static string Admitted(bool value) => value ? "Admitted" : "NotAdmitted";

        [Fact]
        public void Measure_the_pure_cost_of_the_read_on_the_base()
        {
            var catalog = PilotKit.Catalog;
            var lines = new List<string>
            {
                "copia=5aaacce3 (BASE)",
                "entorno framework=" + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
                    + " config=" +
#if DEBUG
                    "Debug"
#else
                    "Release"
#endif
                    + " cpus=" + Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture)
                    + " servergc=" + GCSettings.IsServerGC
                    + " hires=" + Stopwatch.IsHighResolution,
            };

            var scenarios = new[]
            {
                Build("pb-01-base x2 vistas", "pb-01-base", 0, 0),
                Build("pb-07-combinado x2 vistas", "pb-07-combinado", 0, 0),
                Build("pb-21-cola x2 vistas", "pb-21-cola", 0, 0),
                Build("pb-20-ab x2 vistas", "pb-20-ab", 0, 0),
                Build("pb-07-combinado x4 vistas +200 racks ajenos", "pb-07-combinado", 2, 200),
                Build("pb-20-ab x4 vistas +200 racks ajenos", "pb-20-ab", 2, 200),
            };

            foreach (var s in scenarios)
            {
                var pilot0 = PushBackPilotIntentReader.Read(Scan(s), catalog);
                lines.Add("escenario " + s.Name + " definiciones=" + s.Definitions.ToString(CultureInfo.InvariantCulture)
                    + " | Primary piloto=" + Admitted(pilot0.Read.IsAdmitted));

                var cases = new List<(string Name, Func<object> Body)>
                {
                    ("base (piloto+FromPilot; Begin y las cinco relecturas)", () =>
                    {
                        var input = Scan(s);
                        return EditIntentReadResult.FromPilot(PushBackPilotIntentReader.Read(input, catalog));
                    }),
                    ("parte: solo el escaneo (ctor)", () => Scan(s)),
                    ("parte: solo BuildStaleBase", () => PushBackPilotIntentReader.BuildStaleBase(Scan(s))),
                    ("parte: solo el piloto", () => PushBackPilotIntentReader.Read(Scan(s), catalog)),
                };

                foreach (var c in cases)
                {
                    var m = Measure(c.Body);
                    lines.Add("medida | " + s.Name + " | " + c.Name + " | mediana=" + F(m.Median) + " ms | p95=" + F(m.P95) + " ms | n=" + m.N.ToString(CultureInfo.InvariantCulture));
                }
            }

            Assert.Fail(string.Join("\n", lines));
        }
    }
}
