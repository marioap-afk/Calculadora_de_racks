using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Runs a test body on a single, shared STA thread that owns a WPF <see cref="Application"/> and a
    /// live <see cref="Dispatcher"/>. WPF windows (and, defensively, any pack-URI resource lookup) require
    /// STA; xUnit runs tests on MTA thread-pool threads. A dedicated STA dispatcher lets the UI-control tests
    /// construct real controls without pulling in a third-party STA test package (the repo forbids new
    /// dependencies outside the agreed test set — AGENTS.md).
    ///
    /// One <see cref="Application"/> instance is allowed per process, so the thread is created once and reused;
    /// every UI-touching test marshals onto it via <see cref="Run(Action)"/>. Exceptions are captured and
    /// rethrown on the caller with their original stack trace so xUnit reports them normally.
    /// </summary>
    public static class StaTestRunner
    {
        private static readonly object Gate = new object();
        private static Dispatcher dispatcher;

        private static Dispatcher EnsureDispatcher()
        {
            lock (Gate)
            {
                if (dispatcher != null)
                {
                    return dispatcher;
                }

                using (var ready = new ManualResetEventSlim(false))
                {
                    var thread = new Thread(() =>
                    {
                        // One Application per AppDomain; created here so pack:// resource URIs resolve
                        // and controls that load templates behave exactly as they do at runtime.
                        if (System.Windows.Application.Current == null)
                        {
                            _ = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                        }

                        dispatcher = Dispatcher.CurrentDispatcher;
                        ready.Set();
                        Dispatcher.Run();
                    })
                    {
                        IsBackground = true,
                        Name = "RackCad.UI.Tests STA",
                    };

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    ready.Wait();
                }

                return dispatcher;
            }
        }

        /// <summary>Runs <paramref name="action"/> synchronously on the shared STA dispatcher.</summary>
        public static void Run(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Exception captured = null;
            EnsureDispatcher().Invoke(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
            });

            if (captured != null)
            {
                ExceptionDispatchInfo.Capture(captured).Throw();
            }
        }

        /// <summary>Runs <paramref name="func"/> synchronously on the shared STA dispatcher and returns its result.</summary>
        public static T Run<T>(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var result = default(T);
            Run(() => { result = func(); });
            return result;
        }
    }
}
