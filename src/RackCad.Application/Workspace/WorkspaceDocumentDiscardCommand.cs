#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3C2 (D-T3C2-02): the pure core of the command that discards, on explicit confirmation, the drafts of the workspace panel
    /// of the active document, without ever creating the controller. The command of the Plugin only supplies the five delegates:
    /// this class holds every decision and every text.
    ///
    /// The target (C-F4-03) is the active session that the bridge follows, provided it authorizes against the document that runs the
    /// command (<c>isTarget</c>). Without it, or when it is ambiguous, nothing is read, nothing is asked and nothing is discarded.
    /// The scope is the whole document, like the button of the panel: every rack at risk and every uncertain one is listed by its
    /// RackId and none is chosen alone.
    ///
    /// The core calls only two operations of the port: <see cref="IWorkspacePilotEditPort.GetRacksAtRisk"/> (twice, before and after the
    /// question) and, after a yes and an equal second look, <see cref="IWorkspacePilotEditPort.DiscardDocument"/>. It writes nothing of
    /// the drawing, reads no intention and opens no dialog. The unknown is never "nothing at risk": it is an explicit failure.
    /// An exception of a delegate is not contained here: the command reports it. Before the discard it changed nothing; if the write of
    /// the final text throws, the discard already happened.
    /// </summary>
    public static class WorkspaceDocumentDiscardCommand
    {
        /// <summary>The question of the confirmation (the text of the keywords is up to the command).</summary>
        public const string Question = "¿Descartar los borradores de este documento?";

        private const string NoController =
            "RackCad: No hay borradores del panel Workspace en esta sesión de AutoCAD; no se descartó nada.";

        private const string NoActiveSession =
            "RackCad: El panel Workspace no sigue ningún documento activo en este momento; no se descartó nada.";

        private const string UnknownRisk =
            "RackCad: No se pudo decidir qué borradores están en riesgo; no se descartó nada.";

        private const string NothingAtRisk = "RackCad: No hay borradores en riesgo en este documento; no se descartó nada.";

        private const string NotConfirmed = "RackCad: No se descartó nada.";

        private const string ActiveChanged = "RackCad: El documento activo cambió; no se descartó nada.";

        private const string UnknownSecondRisk =
            "RackCad: No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada.";

        private const string RisksChanged =
            "RackCad: Los borradores en riesgo cambiaron; no se descartó nada. Vuelve a ejecutar RACKPANELDESCARTAR para revisarlos.";

        private const string AmbiguousTarget =
            "RackCad: El panel Workspace no sigue el documento activo en este momento; no se descartó nada.";

        private const string NotDiscarded = "RackCad: No se descartaron los borradores del documento: ";

        /// <summary>Runs the command. See the class remarks for the flow; every text is the one of the closed table of the package.</summary>
        /// <param name="port">The edit port of the panel, or null when there is no controller.</param>
        /// <param name="activeSession">The active session that the bridge follows, or null.</param>
        /// <param name="isTarget">Whether the session authorizes against the document that runs the command.</param>
        /// <param name="confirm">The question; true only for an explicit yes.</param>
        /// <param name="write">Writes one line for the user.</param>
        public static void Run(
            IWorkspacePilotEditPort? port,
            Func<SessionId?> activeSession,
            Func<SessionId, bool> isTarget,
            Func<string, bool> confirm,
            Action<string> write)
        {
            if (activeSession == null) throw new ArgumentNullException(nameof(activeSession));
            if (isTarget == null) throw new ArgumentNullException(nameof(isTarget));
            if (confirm == null) throw new ArgumentNullException(nameof(confirm));
            if (write == null) throw new ArgumentNullException(nameof(write));

            if (port == null)
            {
                write(NoController);
                return;
            }

            var found = activeSession();
            if (!found.HasValue)
            {
                write(NoActiveSession);
                return;
            }

            var session = found.Value;
            if (!isTarget(session))
            {
                write(AmbiguousTarget);
                return;
            }

            var first = ReadRisks(port, session);
            if (!first.IsKnown)
            {
                write(UnknownRisk);
                return;
            }

            if (first.RackIds.Count == 0 && first.UncertainRackIds.Count == 0)
            {
                write(NothingAtRisk);
                return;
            }

            if (first.RackIds.Count > 0)
                write("RackCad: Borradores con trabajo sin aplicar: " + string.Join(", ", first.RackIds) + ".");
            if (first.UncertainRackIds.Count > 0)
                write("RackCad: No se pudo decidir si tienen trabajo sin aplicar: " + string.Join(", ", first.UncertainRackIds) + ".");

            if (!confirm(Question))
            {
                write(NotConfirmed);
                return;
            }

            var again = activeSession();
            if (!again.HasValue || again.Value != session)
            {
                write(ActiveChanged);
                return;
            }

            if (!isTarget(session))
            {
                write(AmbiguousTarget);
                return;
            }

            var second = ReadRisks(port, session);
            if (!second.IsKnown)
            {
                write(UnknownSecondRisk);
                return;
            }

            if (!SameList(first.RackIds, second.RackIds) || !SameList(first.UncertainRackIds, second.UncertainRackIds))
            {
                write(RisksChanged);
                return;
            }

            EditDiscardOutcome outcome;
            try
            {
                outcome = port.DiscardDocument(new WorkspaceRequest(session));
            }
            catch (Exception ex)
            {
                write(NotDiscarded + "falló. " + ReasonOrDefault(ex.Message, "sin detalle."));
                return;
            }

            write(OutcomeText(outcome, first));
        }

        // A read that throws counts as the unknown, which the default value of the view stands for.
        private static PilotRiskView ReadRisks(IWorkspacePilotEditPort port, SessionId session)
        {
            try
            {
                return port.GetRacksAtRisk(new WorkspaceRequest(session));
            }
            catch (Exception)
            {
                return default;
            }
        }

        private static string OutcomeText(EditDiscardOutcome outcome, PilotRiskView risks)
        {
            switch (outcome.Status)
            {
                case EditDiscardStatus.Discarded:
                    return DiscardedText(outcome, risks);
                case EditDiscardStatus.Busy:
                    return NotDiscarded + ReasonOrDefault(outcome.Reason, "hay una lectura en curso.");
                case EditDiscardStatus.Failed:
                    return NotDiscarded + "falló. " + ReasonOrDefault(outcome.Reason, "sin detalle.");
                default:
                    return NotDiscarded + "se rechazó. " + ReasonOrDefault(outcome.Reason, "sin detalle.");
            }
        }

        private static string DiscardedText(EditDiscardOutcome outcome, PilotRiskView risks)
        {
            var text = "RackCad: ";
            text = text + (risks.RackIds.Count > 0
                ? "Se descartaron los borradores en riesgo: " + string.Join(", ", risks.RackIds) + "."
                : "Se descartaron los borradores del documento.");
            if (risks.UncertainRackIds.Count > 0)
                text = text + " Sin decidir, también descartados: " + string.Join(", ", risks.UncertainRackIds) + ".";

            var clean = 0;
            foreach (var removed in outcome.RemovedRackIds)
            {
                if (!Contains(risks.RackIds, removed) && !Contains(risks.UncertainRackIds, removed)) clean++;
            }

            return text + " Borradores limpios retirados además: " + clean.ToString(CultureInfo.InvariantCulture) + ".";
        }

        private static string ReasonOrDefault(string? reason, string fallback) =>
            string.IsNullOrWhiteSpace(reason) ? fallback : reason!;

        private static bool Contains(IReadOnlyList<string> list, string rackId)
        {
            foreach (var item in list)
            {
                if (string.Equals(item, rackId, StringComparison.Ordinal)) return true;
            }

            return false;
        }

        // The same racks in the same order, compared by ordinal text.
        private static bool SameList(IReadOnlyList<string> first, IReadOnlyList<string> second)
        {
            if (first.Count != second.Count) return false;
            for (var i = 0; i < first.Count; i++)
            {
                if (!string.Equals(first[i], second[i], StringComparison.Ordinal)) return false;
            }

            return true;
        }
    }
}
