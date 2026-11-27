using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 st (I-64, sexta tanda; O-5 of the smoke of the fifth, M-16 of Dinámico; the Owner on 2026-11-27: «Avisar y ofrecer releer»): the
    /// editors of the window after a command that is not of the window. Until now an editor stayed «Limpio» after a «U» (or a RACKEDITAR, a
    /// MOVE…) with what was undone, and a later «Actualizar» wrote it again without a word.
    ///
    /// <para><b>The signal</b> is the one the page «BOM y lista de racks» follows (BM-01), refined by the Owner on 2026-11-27 («Afinarlo»): every
    /// presentation of the host says whether a command runs in the drawing of the window (the panel pulls its snapshot inside
    /// CommandWillStart, and again when the drawing comes back to rest) and how many changes of its database the session received. A command
    /// is followed from its first presentation until the drawing is at rest again, or the window presents another drawing
    /// (<see cref="RackCadCommandWatch"/>); when it ends having changed the database (a «U», an ERASE, a MOVE; not a ZOOM, a PAN or a REGEN),
    /// the editors of its drawing are marked «posiblemente desactualizados», unless it was a command of the door of the window.</para>
    ///
    /// <para><b>What is own</b>: the door of the window hands ONE numbered request at a time, and its command takes it (it answers inside
    /// itself, or says it took it, <c>RequestStarted</c>). A command that started while a request of the door waited, not taken, for the same
    /// drawing, and that took it, is the command of the door: «Editar aquí» (the …ABRIR, with no undo marker), «Actualizar», «Insertar…»,
    /// «Nuevo», the reads of the BOM, of «Variables» and of «Almacén». Any other command is foreign: one that started with no request
    /// waiting, or that ended without taking the request that waited (the request of the door had not run yet, and the user ran another
    /// command), or during which the window stopped waiting. A RackCad modal (the full editor of the pilot) counts as a command.</para>
    ///
    /// <para><b>What it does</b>: it marks, and nothing else. No order leaves the window without a gesture of the user: «Volver a leer del
    /// dibujo» of the editor is the read of «Editar aquí» of its system, and its answer replaces the editor even with changes (the notice says
    /// so). A write of the editor that AutoCAD did clears the mark. A new rack of «Nuevo» is not in the drawing: it is never marked.</para>
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        /// <summary>The mark of the list for an editor of the window that may be out of date (the words of the pilot, <c>RackListViewModel</c>).</summary>
        internal const string PossiblyStaleSuffix = " (posiblemente obsoleto)";

        // The command that runs in the drawing of the window (RackCadCommandWatch), and the request of the door that waited, not taken, when it
        // started: the request its command would take, if it is the command of the door.
        private readonly RackCadCommandWatch _editorsWatch = new RackCadCommandWatch();
        private long? _commandRequest;

        // True only DURING the call of «Volver a leer del dibujo»: the read that «Editar aquí» asks for then carries IsReread (its request).
        private bool _rereading;

        // From Present: a command started, runs or ended in the drawing of the window. When one ends that changed the drawing and is not the
        // command of the door, the editors of its drawing are marked.
        private void PresentDrawingChange()
        {
            var session = Panel.PresentedSession;
            var ended = _editorsWatch.Observe(session, Panel.PresentedBlock, Panel.PresentedDrawingChangeCount, out var started);
            if (ended != null)
            {
                var own = IsOwn(_commandRequest);
                _commandRequest = null;
                if (ended.ChangedTheDrawing && !own) MarkDrawingChanged(ended.Session);
            }

            if (started)
                _commandRequest = _inFlight != null && _inFlight.Session == session.Value && !_inFlight.Started ? _inFlight.Id : (long?)null;
        }

        // The command took the request of the door that waited when it started: it answered (the request no longer waits) or said it took it.
        private bool IsOwn(long? request)
        {
            if (request is not long id) return false;
            return _inFlight == null || _inFlight.Id != id || _inFlight.Started;
        }

        // «Dejar de esperar» while a command runs: whatever ends now did not take the request, so it is not known to be the one of the door.
        private void ForgetCommandRunRequest(long requestId)
        {
            if (_commandRequest == requestId) _commandRequest = null;
        }

        // The editors of the racks of the drawing in that session: «posiblemente desactualizados», with the read of «Volver a leer del dibujo».
        private void MarkDrawingChanged(SessionId session)
        {
            var marked = 0;
            foreach (var pair in _dynamicDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack).ToList())
                marked += Mark(pair.Value.Editor.DrawingChange, pair.Key, EditDynamicHere);
            foreach (var pair in _selectiveDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack).ToList())
                marked += Mark(pair.Value.Editor.DrawingChange, pair.Key, EditSelectiveHere);
            foreach (var pair in _pushBackDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack).ToList())
                marked += Mark(pair.Value.Editor.DrawingChange, pair.Key, EditPushBackHere);
            foreach (var pair in _cantileverDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack).ToList())
                marked += Mark(pair.Value.Editor.DrawingChange, pair.Key, EditCantileverHere);
            foreach (var pair in _camaDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack).ToList())
                marked += Mark(pair.Value.Editor.DrawingChange, pair.Key, EditCamaHere);
            foreach (var pair in _cabeceraDrafts.Where(p => p.Key.Session == session).ToList())
                marked += Mark(pair.Value.Editor.DrawingChange, pair.Key, EditCabeceraHere);
            if (marked > 0) RackList.RefreshStates();
        }

        private int Mark(RackEditorDrawingChange change, (SessionId Session, string RackId) key, Action editHere)
        {
            change.MarkChanged(() => Reread(key.Session, key.RackId, editHere));
            return 1;
        }

        /// <summary>Whether the editor of the window of a rack is marked «posiblemente desactualizado» (the list says it).</summary>
        private bool IsDrawingChanged(SessionId session, string rackId)
        {
            var key = (session, rackId);
            return (_dynamicDrafts.TryGetValue(key, out var dynamic) && dynamic.Editor.DrawingChange.IsChanged)
                   || (_selectiveDrafts.TryGetValue(key, out var selective) && selective.Editor.DrawingChange.IsChanged)
                   || (_pushBackDrafts.TryGetValue(key, out var pushBack) && pushBack.Editor.DrawingChange.IsChanged)
                   || (_cantileverDrafts.TryGetValue(key, out var cantilever) && cantilever.Editor.DrawingChange.IsChanged)
                   || (_camaDrafts.TryGetValue(key, out var cama) && cama.Editor.DrawingChange.IsChanged)
                   || (_cabeceraDrafts.TryGetValue(key, out var cabecera) && cabecera.Editor.DrawingChange.IsChanged);
        }

        // «Volver a leer del dibujo»: the read of «Editar aquí» of the system (the door …ABRIR), for the rack of the editor, in its drawing.
        // Its answer replaces the editor even with changes (the read handlers look at IsReread).
        private void Reread(SessionId session, string rackId, Action editHere)
        {
            Contained(() =>
            {
                if (!Panel.PresentedSession.HasValue || Panel.PresentedSession.Value != session)
                {
                    Say("Este borrador es de otro dibujo: vuelve a ese dibujo para volver a leerlo.", failed: true);
                    return;
                }

                if (!string.Equals(Panel.Browser.ActiveRackId, rackId, StringComparison.OrdinalIgnoreCase))
                {
                    Say("Abre el rack en la lista para volver a leerlo del dibujo.", failed: true);
                    return;
                }

                _rereading = true;
                try
                {
                    editHere();
                }
                finally
                {
                    _rereading = false;
                }
            });
        }
    }
}
