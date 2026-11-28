using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>How the window learned that a drawing may have changed.</summary>
    internal enum RackCadChangeKind
    {
        /// <summary>A command the window followed from its start ended, now or (if another drawing was presented meanwhile) when its drawing came back.</summary>
        Command,

        /// <summary>The drawing, presented again with no command seen, had commands that changed it (the palette was hidden meanwhile).</summary>
        Unseen,

        /// <summary>Coming back to the drawing, it had commands that changed it while the window presented another one.</summary>
        Return,
    }

    /// <summary>
    /// What the window learned of one drawing: for <see cref="RackCadChangeKind.Command"/>, whether the command was the consumer's own (it said so
    /// and exactly one command started while it ran) and whether the database of the drawing changed while it ran; for the other two kinds,
    /// always a change that is not own.
    /// </summary>
    internal sealed record RackCadCommandVerdict(SessionId Session, RackCadChangeKind Kind, bool Own, bool ChangedTheDrawing);

    /// <summary>What one presentation gave: the drawing where a command started with it (or null) and the verdicts it brought.</summary>
    internal sealed record RackCadCommandStep(SessionId? Started, IReadOnlyList<RackCadCommandVerdict> Verdicts);

    /// <summary>
    /// T6 st (I-64, sexta tanda; the Owner on 2026-11-27, «Afinarlo»: only the commands that change the drawing say that a reading may be out
    /// of date; and, after the review of 5114345e, «Sí, al volver a mostrarla» and M-1, M-2). The commands of AutoCAD as the RackCad window sees
    /// them in the presentations of the host, with two counts of the session of each presentation: the changes of its database
    /// (<see cref="WorkspaceHostSnapshot.DrawingChangeCount"/>: an object appended, erased, modified…; a ZOOM, a PAN or a REGEN change no object)
    /// and the commands that started in it (<see cref="WorkspaceHostSnapshot.CommandStartCount"/>).
    /// <list type="bullet">
    /// <item>A command runs from the first presentation with a command (the panel pulls its snapshot inside CommandWillStart, before the bridge
    /// counts the start) or a RackCad modal, until its drawing is presented at rest again: then its verdict, with what the consumer says of it
    /// (<c>ownSoFar</c>), taken as own only when exactly one command started while it ran (a script, a macro or a command of the door right after
    /// another run as one).</item>
    /// <item>When the window presents another drawing (or none) before the command ends, the consumer says then whether it was its own, and the
    /// verdict waits until that drawing is presented again: the counts tell then what happened.</item>
    /// <item>A drawing presented again with no command seen (the palette was hidden, or the window presented another drawing) brings a verdict
    /// only when commands started in it AND its database changed meanwhile: changes with no command (the bookkeeping around a command, fields)
    /// are not a command of AutoCAD.</item>
    /// </list>
    /// The editors of the window, «BOM y lista de racks» and «Almacén» each keep one, fed by the same presentations, and each decides what is its
    /// own. It reads nothing and sends nothing.
    /// </summary>
    internal sealed class RackCadCommandWatch
    {
        private sealed record Run(SessionId Session, long Changes, int Starts, bool Own);

        private sealed class Drawing
        {
            public long Changes;
            public int Starts;
            public Run Pending;
        }

        private readonly Dictionary<SessionId, Drawing> _drawings = new();
        private Run _running;
        private SessionId? _lastPresented;

        /// <summary>The drawing of the command that runs, or null when none runs.</summary>
        public SessionId? Running => _running?.Session;

        /// <summary>One presentation of the host. <paramref name="ownSoFar"/> is asked when a command ends or is left by another drawing.</summary>
        public RackCadCommandStep Observe(SessionId? session, WorkspaceActionBlock block, long changes, int starts, Func<bool> ownSoFar)
        {
            var verdicts = new List<RackCadCommandVerdict>();
            var runs = session.HasValue && (block == WorkspaceActionBlock.CommandActive || block == WorkspaceActionBlock.ModalActive);

            // The command that runs ends here, at rest in its drawing, or is left: another drawing (or none) is presented.
            var endedHere = false;
            if (_running != null && (!runs || session != _running.Session))
            {
                var run = _running with { Own = Own(ownSoFar) };
                _running = null;
                if (session == run.Session)
                {
                    verdicts.Add(VerdictOf(run, changes, starts));
                    endedHere = true;
                }
                else
                {
                    DrawingOf(run.Session).Pending = run;
                }
            }

            SessionId? started = null;
            if (session.HasValue && _running == null)
            {
                var id = session.Value;
                if (!endedHere && _drawings.TryGetValue(id, out var known))
                {
                    if (known.Pending != null) verdicts.Add(VerdictOf(known.Pending, changes, starts));
                    else if (changes != known.Changes && starts != known.Starts)
                        verdicts.Add(new RackCadCommandVerdict(id, _lastPresented == id ? RackCadChangeKind.Unseen : RackCadChangeKind.Return, false, true));
                }

                var drawing = DrawingOf(id);
                drawing.Changes = changes;
                drawing.Starts = starts;
                drawing.Pending = null;
                if (runs)
                {
                    _running = new Run(id, changes, starts, false);
                    started = id;
                }
            }

            _lastPresented = session;
            return new RackCadCommandStep(started, verdicts);
        }

        // Own only when the consumer says so and exactly one command started while it ran.
        private static RackCadCommandVerdict VerdictOf(Run run, long changes, int starts) =>
            new(run.Session, RackCadChangeKind.Command, run.Own && starts - run.Starts == 1, changes != run.Changes);

        private static bool Own(Func<bool> ownSoFar)
        {
            try
            {
                return ownSoFar?.Invoke() == true;
            }
            catch (Exception)
            {
                // Not known to be own: the prudent answer.
                return false;
            }
        }

        private Drawing DrawingOf(SessionId session)
        {
            if (!_drawings.TryGetValue(session, out var drawing))
            {
                drawing = new Drawing();
                _drawings[session] = drawing;
            }

            return drawing;
        }
    }
}
