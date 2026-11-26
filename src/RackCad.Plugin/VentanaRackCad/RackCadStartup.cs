using System;
using System.Windows.Threading;
using RackCad.Application.Diagnostics;
using RackCad.Application.Settings;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// «Abrir RackCad al iniciar AutoCAD» (I-64 A-14, fourth batch, part «ut», UT-01): read once when the plugin loads. Off (the default),
    /// nothing is created, as before. On, the plugin waits on the dispatcher of the main thread (no event of AutoCAD: the bridge of the
    /// workspace is their one subscriber, F1) until a drawing is open, the first time only, and opens the RackCad window there with exactly
    /// what RACKCAD runs. It reads and writes nothing of the drawing. Its real behaviour in AutoCAD is for a smoke of its own.
    /// </summary>
    internal static class RackCadStartup
    {
        /// <summary>How often the startup looks for the first drawing, and for how long before it gives up (two minutes).</summary>
        internal static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

        internal const int MaxTicks = 240;

        private static DispatcherTimer _timer;
        private static int _ticks;

        /// <summary>Reads the setting and, when it is on, waits for the first drawing. Contained: a failure is logged and the plugin loads.</summary>
        internal static void Arm()
        {
            try
            {
                if (_timer != null || !RackCadStartupRule.OpensWindow(UserSettingsStore.Load())) return;
                _ticks = 0;
                _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = Interval };
                _timer.Tick += OnTick;
                _timer.Start();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadStartup.Arm", ex);
            }
        }

        // The first tick with a drawing opens the window, once; without one in two minutes the startup gives up quietly.
        private static void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (AcApplication.DocumentManager.MdiActiveDocument == null && ++_ticks < MaxTicks) return;
                Stop();
                if (AcApplication.DocumentManager.MdiActiveDocument == null) return;
                new RackMenuCommands().RackCad(); // what RACKCAD and RK run: the one window, the one controller
            }
            catch (System.Exception ex)
            {
                Stop();
                RackLog.Exception("RackCadStartup.OnTick", ex);
            }
        }

        private static void Stop()
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Tick -= OnTick;
        }
    }
}
