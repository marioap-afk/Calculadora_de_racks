using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>A command of AutoCAD that ended, as the window saw it: its drawing, and whether the database of that drawing changed meanwhile.</summary>
    internal sealed record RackCadCommandEnd(SessionId Session, bool ChangedTheDrawing);

    /// <summary>
    /// T6 st (I-64, sexta tanda; the Owner on 2026-11-27, «Afinarlo»: only the commands that change the drawing say that a reading may be out
    /// of date). One command of AutoCAD as the RackCad window sees it in the presentations of the host: it runs from the first presentation
    /// with a command (the panel pulls its snapshot inside CommandWillStart) or a RackCad modal, until the drawing is presented at rest again,
    /// or another drawing is presented. It changed the drawing when <see cref="WorkspaceHostSnapshot.DrawingChangeCount"/> of its session
    /// moved while it ran (an object appended, erased, modified… as an ERASE or a «U»; a ZOOM, a PAN or a REGEN change no object). When the
    /// window presents another drawing before the command ends, it cannot tell, and says it changed (the prudent answer of before).
    ///
    /// The editors of the window, «BOM y lista de racks» and «Almacén» each keep one, fed by the same presentations, and each decides what of
    /// its own a command is (the door of the window, the read of the page). It reads nothing and sends nothing.
    /// </summary>
    internal sealed class RackCadCommandWatch
    {
        private bool _running;
        private SessionId _session;
        private long _changesAtStart;

        /// <summary>The drawing of the command that runs, or null when none runs.</summary>
        public SessionId? Running => _running ? _session : (SessionId?)null;

        /// <summary>
        /// One presentation of the host: what ended with it (null when no command ended), and whether a command started with it.
        /// </summary>
        public RackCadCommandEnd Observe(SessionId? session, WorkspaceActionBlock block, long databaseChanges, out bool started)
        {
            started = false;
            var runs = session.HasValue && (block == WorkspaceActionBlock.CommandActive || block == WorkspaceActionBlock.ModalActive);
            RackCadCommandEnd ended = null;
            if (_running && (!runs || session.Value != _session))
            {
                _running = false;
                var sameDrawing = session.HasValue && session.Value == _session;
                ended = new RackCadCommandEnd(_session, !sameDrawing || databaseChanges != _changesAtStart);
            }

            if (runs && !_running)
            {
                _running = true;
                _session = session.Value;
                _changesAtStart = databaseChanges;
                started = true;
            }

            return ended;
        }
    }
}
