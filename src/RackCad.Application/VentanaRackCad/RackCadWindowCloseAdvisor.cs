using System;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// The close of a drawing also asks the RackCad window (I-64 A-14, line D; decision 11 of the Owner on the prototype of Dinámico, «Que
    /// avise antes de cerrar», and his authorization of 2026-11-24 to change the rule of the close: «te autorizo cambiar la regla de
    /// cierre»). Until then one advisor decided the close (F4-T3C2): the relay over the host core, which only knows the drafts of the panel.
    /// The drafts of the editors of the window (Dinámico) live in the window, so this advisor wraps that one: the host decides as before,
    /// with its own notice, and a drawing whose window still has work not written does not close either, with this notice on the command
    /// line. The destruction of a session is the host's alone.
    ///
    /// The decision starts from Veto, as the relay's does: a failure of the host or of the window is a Veto. Nothing here reads or writes the
    /// drawing, opens a dialog or keeps a state of drafts: the window says, when asked, the names of its racks with work not written.
    /// </summary>
    public sealed class RackCadWindowCloseAdvisor : IWorkspaceLifecycleAdvisor
    {
        private const string ContextAdviseClose = "RackCadWindowCloseAdvisor.AdviseClose";

        private readonly IWorkspaceLifecycleAdvisor _inner;
        private readonly Action<string> _writeNow;
        private readonly Action<string, Exception> _report;
        private Func<SessionId, string> _windowWork;

        /// <param name="inner">The advisor of the host (the relay): its decision and its notices stay.</param>
        /// <param name="writeNow">Writes a line on the command line of AutoCAD.</param>
        /// <param name="report">Logs a contained failure; optional.</param>
        public RackCadWindowCloseAdvisor(IWorkspaceLifecycleAdvisor inner, Action<string> writeNow, Action<string, Exception> report)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _writeNow = writeNow ?? throw new ArgumentNullException(nameof(writeNow));
            _report = report;
        }

        /// <summary>
        /// The window says what it still has to write in the drawing of a session: the names of the racks, or null when nothing. Set once
        /// by the host of the palette when the window exists; until then the window has nothing to say.
        /// </summary>
        public void SetWindowWork(Func<SessionId, string> unwrittenWorkOf) => _windowWork = unwrittenWorkOf;

        public CloseDecision AdviseClose(SessionId session)
        {
            var decision = CloseDecision.Veto;
            try
            {
                decision = _inner.AdviseClose(session) == CloseDecision.Allow ? CloseDecision.Allow : CloseDecision.Veto;
            }
            catch (Exception ex)
            {
                Report(ContextAdviseClose, ex);
            }

            string work;
            try
            {
                work = _windowWork?.Invoke(session);
            }
            catch (Exception ex)
            {
                Report(ContextAdviseClose, ex);
                Write("RackCad: el dibujo no se cerró: no se pudo comprobar si la ventana RackCad tiene cambios sin escribir. Vuelve a intentarlo.");
                return CloseDecision.Veto;
            }

            if (string.IsNullOrWhiteSpace(work)) return decision;

            Write("RackCad: el dibujo no se cerró: la ventana RackCad tiene cambios sin escribir en " + work
                  + ". Escríbelos con «Actualizar» (un Dinámico nuevo, con «Insertar en el dibujo…») o descártalos en su editor, y vuelve a cerrar.");
            return CloseDecision.Veto;
        }

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact) => _inner.SessionDestroyed(session, closeSequenceIntact);

        private void Write(string text)
        {
            try
            {
                _writeNow(text);
            }
            catch (Exception ex)
            {
                // The notice is lost; the decision is not.
                Report(ContextAdviseClose, ex);
            }
        }

        private void Report(string context, Exception ex)
        {
            try
            {
                _report?.Invoke(context, ex);
            }
            catch (Exception)
            {
                // Nothing else can be done without risking the close of AutoCAD.
            }
        }
    }
}
