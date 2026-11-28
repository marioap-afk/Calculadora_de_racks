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
    /// workspace is their one subscriber, F1) for the first drawing, however long it takes, and opens the RackCad window with exactly what
    /// RACKCAD runs, once, and only when AutoCAD is at rest (no command in progress and a quiescent editor: the guard of the doors of the
    /// window). It stops when it opens the window or when the plugin unloads (<see cref="Disarm"/>). It reads and writes nothing of the
    /// drawing. Its real behaviour in AutoCAD is for a smoke of its own.
    /// </summary>
    internal static class RackCadStartup
    {
        /// <summary>How often the startup looks for the first drawing at rest (review of part «ut», point 3: with no limit of time).</summary>
        internal static readonly TimeSpan Interval = TimeSpan.FromSeconds(2);

        private static DispatcherTimer _timer;

        /// <summary>Reads the setting and, when it is on, waits for the first drawing. Contained: a failure is logged and the plugin loads.</summary>
        internal static void Arm()
        {
            try
            {
                if (_timer != null || !RackCadStartupRule.OpensWindow(UserSettingsStore.Load())) return;
                _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = Interval };
                _timer.Tick += OnTick;
                _timer.Start();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadStartup.Arm", ex);
            }
        }

        /// <summary>The plugin unloads: the wait stops, whether or not it opened the window (review of part «ut», point 6).</summary>
        internal static void Disarm()
        {
            try
            {
                Stop();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadStartup.Disarm", ex);
            }
        }

        // A tick with no drawing, or with AutoCAD busy, waits for the next one; the first one with a drawing at rest opens the window, once.
        private static void OnTick(object sender, EventArgs e)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null) return;
                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent) return; // the guard of Send (point 2)
                Stop();
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
            var timer = _timer;
            if (timer == null) return;
            _timer = null;
            timer.Stop();
            timer.Tick -= OnTick;
        }
    }
}
