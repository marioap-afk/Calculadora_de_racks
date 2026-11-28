#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.Systems.PushBack.Pilot;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// I-64 F4-T4C1-HOST (D-T4C-04) — the executor of the update of the pilot in the host. It is the ONE place of the product that asks AutoCAD
    /// for a command context, and the one that locks the document of the run and creates its writer. It has no fields: everything that
    /// lives, lives for the request, in the runner of Application. It does not know the workspace.
    ///
    /// What the call returns is the completion object of AutoCAD, not a task: it has no failure channel and a native refusal neither throws nor
    /// completes, which the bound of the ticket of the host covers. The callback and both continuations are total: an exception that left them
    /// would cross into native code.
    /// </summary>
    internal sealed class RackPilotUpdateCommandContext : IPushBackPilotUpdateHost
    {
        public bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure) =>
            PushBackPilotHostRunner.Request(documentNativeId, run, new Primitives(), RackLog.Exception, out failure);

        private sealed class Primitives : IPushBackPilotHostPrimitives
        {
            public void EnterCommandContext(Action callback, Action completed)
            {
                var context = SynchronizationContext.Current;
                if (context == null) throw new InvalidOperationException("No hay contexto de sincronización en este hilo.");

                var result = AcApplication.DocumentManager.ExecuteInCommandContextAsync(
                    data =>
                    {
                        try
                        {
                            callback();
                        }
                        catch (System.Exception ex)
                        {
                            RackLog.Exception("RackPilotUpdateCommandContext.Callback", ex);
                        }

                        return Task.CompletedTask;
                    },
                    null);

                if (result.IsCompleted)
                {
                    Hop(context, completed);
                    return;
                }

                result.OnCompleted(() =>
                {
                    try
                    {
                        Hop(context, completed);
                    }
                    catch (System.Exception ex)
                    {
                        RackLog.Exception("RackPilotUpdateCommandContext.OnCompleted", ex);
                    }
                });
            }

            public bool TryOpenTarget(long documentNativeId, out IPushBackPilotHostTarget? target, out string reason)
            {
                target = null;
                reason = PushBackPilotUpdateRun.DocumentMismatchReason;
                foreach (Document candidate in AcApplication.DocumentManager)
                {
                    if (candidate.UnmanagedObject.ToInt64() != documentNativeId) continue;

                    var active = AcApplication.DocumentManager.MdiActiveDocument;
                    if (active == null || active.UnmanagedObject.ToInt64() != documentNativeId) return false;

                    target = new Target(candidate);
                    reason = string.Empty;
                    return true;
                }

                return false;
            }

            // The hop to the scheduler that was captured before the call: the published action is total.
            private static void Hop(SynchronizationContext context, Action completed) =>
                context.Post(
                    _ =>
                    {
                        try
                        {
                            completed();
                        }
                        catch (System.Exception ex)
                        {
                            RackLog.Exception("RackPilotUpdateCommandContext.Completed", ex);
                        }
                    },
                    null);
        }

        // The document of ONE run, seen from Application only through three operations.
        private sealed class Target : IPushBackPilotHostTarget
        {
            private readonly Document _document;

            public Target(Document document)
            {
                _document = document;
            }

            public IDisposable Lock() => _document.LockDocument();

            public IPushBackPilotUpdatePort CreateWriter() => new RackPilotUpdateWriter(_document);

            public void Retire(IPushBackPilotUpdatePort writer)
            {
                if (writer is RackPilotUpdateWriter spent) spent.Retire();
            }
        }
    }
}
