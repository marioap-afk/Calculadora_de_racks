#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;
using RackCad.Application.Systems.PushBack.Draft;

namespace RackCad.Tests;

// PB-T4d (paquete v5.2), OWNER-05: la sonda de la prueba lenta de la copia de WPP. No la toca: repite su preparacion y mide su bucle.
public sealed class TmpPbt4dSlowProbe
{
    private sealed class CountingList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _inner;
        private readonly Func<bool> _fails;
        private readonly Action _touched;

        public CountingList(IReadOnlyList<T> inner, Action touched, Func<bool> fails)
        {
            _inner = inner;
            _touched = touched;
            _fails = fails;
        }

        private void Touch()
        {
            _touched();
            if (_fails()) throw new InvalidOperationException("catalog-boom");
        }

        public int Count
        {
            get
            {
                Touch();
                return _inner.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                Touch();
                return _inner[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Touch();
            return _inner.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CountingCatalog
    {
        private readonly int _allowed;

        public CountingCatalog(RackCatalog real, int allowed)
        {
            _allowed = allowed;
            Catalog = new RackCatalog
            {
                PostProfiles = new CountingList<ProfileCatalogEntry>(real.PostProfiles, () => Touches++, Fails),
                BeamProfiles = new CountingList<BeamProfileCatalogEntry>(real.BeamProfiles, () => Touches++, Fails),
                SafetyElements = new CountingList<SafetyElementCatalogEntry>(real.SafetyElements, () => Touches++, Fails),
                TrussProfiles = real.TrussProfiles,
                BasePlates = real.BasePlates,
                FlowBedProfiles = real.FlowBedProfiles,
                Mensulas = real.Mensulas,
                SpacerProfiles = real.SpacerProfiles,
                ConnectionPoints = real.ConnectionPoints,
                ConnectionLayout = real.ConnectionLayout,
                Views = real.Views,
                Blocks = real.Blocks,
                Defaults = real.Defaults,
            };
        }

        public RackCatalog Catalog { get; }

        public int Touches { get; private set; }

        private bool Fails() => Touches > _allowed;
    }

    [Fact]
    public void Probe()
    {
        var lines = new List<string>();
        var scan = FullPrepareKit.RealScan();
        var read = IntentKit.Read(scan);
        var draft = FullPrepareKit.DraftOf(read);
        FullPrepareKit.SetScale(draft, 2.25);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var armed = new CountingCatalog(PilotKit.Catalog, int.MaxValue);
        var ready = FullHandKit.Prepare(PushBackFullPrepareInput.From(draft), read, armed.Catalog);
        var full = sw.Elapsed.TotalMilliseconds;
        var total = armed.Touches;
        lines.Add("prepare-completo estado=" + ready.Kind + " toques=" + total + " ms=" + full.ToString("F1", CultureInfo.InvariantCulture));

        double sum = 0;
        foreach (var frac in new[] { 0.0, 0.25, 0.5, 0.75, 1.0 })
        {
            var allowed = Math.Max(0, Math.Min(total - 1, (int)(frac * total)));
            sw.Restart();
            var failing = new CountingCatalog(PilotKit.Catalog, allowed);
            var result = FullHandKit.Prepare(PushBackFullPrepareInput.From(draft), read, failing.Catalog);
            var ms = sw.Elapsed.TotalMilliseconds;
            sum += ms;
            lines.Add("falla-en-el-toque " + allowed + " estado=" + result.Kind + " ms=" + ms.ToString("F1", CultureInfo.InvariantCulture));
        }

        lines.Add("vueltas-del-bucle=" + total + " estimacion-s=" + (total * sum / 5 / 1000).ToString("F0", CultureInfo.InvariantCulture));
        Assert.Fail(string.Join("\n", lines));
    }
}
