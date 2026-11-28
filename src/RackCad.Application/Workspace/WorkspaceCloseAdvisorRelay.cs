#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3C2 (D-T3C2-04): the wrapper of the lifecycle advisor. Every decision is the one of the inner advisor (the host core): the
    /// wrapper never decides alone, holds no state of drafts and consumes no queue of the panel. It only adds the words that the user
    /// needs on the command line: a notice when a close was vetoed and, later, a notice of the work that a destroyed document lost.
    ///
    /// <see cref="AdviseClose"/> starts from Veto (the default value of <see cref="CloseDecision"/> is Allow, so it is never the start) and
    /// gives Allow only when the inner advisor does; an exception of the inner advisor is a Veto. The write of the notice and the queries
    /// for it never change the decision.
    ///
    /// <see cref="SessionDestroyed"/> reads the risk of the session BEFORE it delegates, because after the retirement the host answers
    /// "nothing at risk"; all of that work is contained, and the delegation happens always and once, uncontained: its exception goes to
    /// the core, which logs it as before. The session is no longer in the bridge then, so this method neither asks the bridge nor
    /// writes: it keeps the message until <see cref="TakeMessages"/> is called with an active session.
    ///
    /// Declared (I-64 F4-T3C2, D-T3-08): two bounded extensions of the advisor of F4-T2B2B. In <see cref="AdviseClose"/>, the in-memory
    /// reading of the name and of the activity of the document and the write of the notice of the veto on the command line; in
    /// <see cref="SessionDestroyed"/>, the in-memory reading of the risk and the deferred message. None of them reads or writes the drawing
    /// or opens a dialog. The wrapper does not empty live text: the veto sees the pending text of a draft without touching it.
    /// </summary>
    public sealed class WorkspaceCloseAdvisorRelay : IWorkspaceLifecycleAdvisor
    {
        private const string ContextAdviseClose = "WorkspaceCloseAdvisorRelay.AdviseClose";
        private const string ContextVetoMessage = "WorkspaceCloseAdvisorRelay.VetoMessage";
        private const string ContextSessionDestroyed = "WorkspaceCloseAdvisorRelay.SessionDestroyed";

        private readonly IWorkspaceLifecycleAdvisor _inner;
        private readonly IWorkspacePilotEditPort _risks;
        private readonly Func<SessionId, string?> _documentNameOf;
        private readonly Func<SessionId, bool> _isActive;
        private readonly Action<string> _writeNow;
        private readonly Action<string, Exception>? _report;
        private readonly object _gate = new();
        private readonly List<string> _messages = new();

        public WorkspaceCloseAdvisorRelay(
            IWorkspaceLifecycleAdvisor inner,
            IWorkspacePilotEditPort risks,
            Func<SessionId, string?> documentNameOf,
            Func<SessionId, bool> isActive,
            Action<string> writeNow,
            Action<string, Exception>? report)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _risks = risks ?? throw new ArgumentNullException(nameof(risks));
            _documentNameOf = documentNameOf ?? throw new ArgumentNullException(nameof(documentNameOf));
            _isActive = isActive ?? throw new ArgumentNullException(nameof(isActive));
            _writeNow = writeNow ?? throw new ArgumentNullException(nameof(writeNow));
            _report = report;
        }

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

            if (decision == CloseDecision.Veto) WriteVetoMessage(session);
            return decision;
        }

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact)
        {
            RememberLoss(session);
            _inner.SessionDestroyed(session, closeSequenceIntact);
        }

        /// <summary>
        /// The messages that were kept for the user, in order. Without an active session it answers nothing and keeps them; with one it
        /// answers all of them and empties the list.
        /// </summary>
        public IReadOnlyList<string> TakeMessages(SessionId? activeSession)
        {
            if (!activeSession.HasValue) return Array.Empty<string>();

            lock (_gate)
            {
                var taken = _messages.ToArray();
                _messages.Clear();
                return taken;
            }
        }

        private void WriteVetoMessage(SessionId session)
        {
            try
            {
                string? name = null;
                try
                {
                    name = ShortName(_documentNameOf(session));
                }
                catch (Exception ex)
                {
                    Report(ContextVetoMessage, ex);
                }

                var active = true;
                try
                {
                    active = _isActive(session);
                }
                catch (Exception ex)
                {
                    Report(ContextVetoMessage, ex);
                }

                var document = string.IsNullOrWhiteSpace(name) ? "el documento" : "«" + name + "»";
                var way = active
                    ? "Descártalos en el panel con \"Descartar borradores de este documento\" o con el comando RACKPANELDESCARTAR, y vuelve a cerrarlo."
                    : "Actívalo primero; después descártalos en el panel con \"Descartar borradores de este documento\" o con el comando RACKPANELDESCARTAR, y vuelve a cerrarlo.";
                _writeNow(
                    "RackCad: No se cerró " + document + ": puede tener borradores del panel Workspace con trabajo sin aplicar. " + way);
            }
            catch (Exception ex)
            {
                Report(ContextVetoMessage, ex);
            }
        }

        // The risk is read before the delegation, because afterwards the host answers "nothing at risk". Whatever fails here is reported
        // and never prevents the delegation.
        private void RememberLoss(SessionId session)
        {
            try
            {
                var risk = default(PilotRiskView);
                try
                {
                    risk = _risks.GetRacksAtRisk(new WorkspaceRequest(session));
                }
                catch (Exception ex)
                {
                    Report(ContextSessionDestroyed, ex);
                }

                var message = LossMessage(risk);
                if (message == null) return;

                lock (_gate)
                {
                    _messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                Report(ContextSessionDestroyed, ex);
            }
        }

        private static string? LossMessage(PilotRiskView risk)
        {
            if (!risk.IsKnown)
                return "RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace.";

            if (risk.RackIds.Count > 0)
            {
                var text = "RackCad: Al cerrarse un documento se perdió el trabajo sin aplicar de los borradores del panel Workspace: "
                    + string.Join(", ", risk.RackIds) + ".";
                if (risk.UncertainRackIds.Count > 0) text = text + " Sin decidir: " + string.Join(", ", risk.UncertainRackIds) + ".";
                return text;
            }

            if (risk.UncertainRackIds.Count > 0)
            {
                return "RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace: "
                    + string.Join(", ", risk.UncertainRackIds) + ".";
            }

            return null;
        }

        // The last segment after a backslash or a slash, with string operations only.
        private static string? ShortName(string? name)
        {
            if (name == null) return null;
            var cut = name.LastIndexOfAny(new[] { '\\', '/' });
            return cut < 0 ? name : name.Substring(cut + 1);
        }

        // A report that throws is ignored, like the report of the host.
        private void Report(string context, Exception exception)
        {
            try
            {
                _report?.Invoke(context, exception);
            }
            catch (Exception)
            {
            }
        }
    }
}
