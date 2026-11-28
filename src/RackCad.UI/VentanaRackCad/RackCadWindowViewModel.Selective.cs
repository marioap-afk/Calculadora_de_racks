using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Insertar vista…» of the Selectivo editors of the window and «Selectivo en esta ventana» of «Nuevo» (I-64 A-14, line D, Selectivo
    /// E2, part P3), as the second delivery of Dinámico did them (step 4, D-10): the same door, the same one request in flight, the same
    /// answers. A new Selectivo has the defaults of the classic editor and the identity the window gives it now; «Insertar en el dibujo…»
    /// inserts it (RACKSELECTIVONUEVO), and from then on the same editor is the one of the rack of the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        /// <summary>The name of a new Selectivo of the window with no name, in the veto of the close and in the notice of a closed drawing.</summary>
        internal const string NewSelectiveName = "un Selectivo nuevo";

        // M-3 of the review of the entrega 2: the dimension styles of each drawing, as the last Selectivo read from it gave them (S-27). The
        // classic editor opened from RACKNUEVO reads them from the drawing; a new Selectivo of the window takes these, because reading the
        // drawing for it would wait on AutoCAD. A drawing with no Selectivo read yet has none, and the list offers «(Automático)» only.
        private readonly Dictionary<SessionId, IReadOnlyList<string>> _dimensionStylesOfDrawing = new();

        // What a Selectivo read from that drawing said of its styles; a read that did not read them (null) keeps what the window had.
        private void RememberDimensionStyles(SessionId session, IReadOnlyList<string> styles)
        {
            if (styles != null) _dimensionStylesOfDrawing[session] = styles.ToList();
        }

        private IReadOnlyList<string> DimensionStylesOf(SessionId session)
            => _dimensionStylesOfDrawing.TryGetValue(session, out var styles) ? styles : null;

        // The styles of a drawing that was closed are forgotten with it.
        private void ForgetDimensionStylesOfDeadSessions(Func<SessionId, bool> isAlive)
        {
            foreach (var session in _dimensionStylesOfDrawing.Keys.Where(session => !isAlive(session)).ToList())
                _dimensionStylesOfDrawing.Remove(session);
        }

        /// <summary>
        /// The new Selectivo that «Nuevo» shows: the editor of the active draft of the drawing of the window when it is a Selectivo (T4 new:
        /// a drawing holds several drafts, and «Nuevo» shows one at a time); null otherwise.
        /// </summary>
        public SelectiveEditorViewModel NewSelectiveEditor => ActiveDraftEditor as SelectiveEditorViewModel;

        public bool HasNewSelectiveEditor => NewSelectiveEditor != null;

        /// <summary>«Nuevo» shows the new Selectivo being made.</summary>
        public bool ShowNewSelectiveEditor => IsNuevoPage && HasNewSelectiveEditor;

        /// <summary>A new Selectivo can be made here: the window has the door that writes a Selectivo, and a drawing.</summary>
        public bool CanStartNewSelective => _selectiveDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>
        /// «Selectivo en esta ventana» of «Nuevo»: a new Selectivo with the defaults of the classic editor
        /// (<see cref="SelectiveDraftEditor.CreateNew"/>), edited here; «Insertar en el dibujo…» places it. Its identity is given now, so once
        /// inserted the same editor is the one of the rack in the list. T4 new: every click is a new draft («Borrador N»), through the one
        /// entry point of the drafts (<see cref="OpenAsNewDraft"/>), which builds it as before (<c>ForNewRack</c>, the styles of M-3).
        /// </summary>
        public void StartNewSelective() => OpenAsNewDraft(RackCadDraftSeed.Defaults(RackCad.Domain.Systems.Shared.RackSystemKind.SelectiveRack));

        // What every Selectivo editor of the window gets: «Insertar vista…» (T4 ins: the panel of the editor, DG-01; the writes) and the
        // close. The panel is not a modal of AutoCAD, so it needs no modal port.
        private void WireSelective(SelectiveEditorViewModel editor)
        {
            WireInsertPanel(editor); // T4 ins
            editor.InsertViewsAction = InsertSelectiveViews;
            editor.InsertNewAction = InsertNewSelective;
            editor.SaveToLibraryAction = StartSaveToLibrary; // T4 ut: «Más» › «Guardar en biblioteca…»
            editor.PropertyChanged += OnSelectiveEditorChanged;
            editor.CloseRequested += CloseSelectiveEditor;
            // T4 seg (SG-01): no «Editar seguridad…» any more; the safety is edited in «Extremos y seguridad» (D-09 replaced).
        }

        // «Insertar vista…» of the Selectivo editor: the write of «Actualizar», with the views to insert after it (RACKSELECTIVOINSERTAR).
        private void InsertSelectiveViews(SelectiveEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0 || editor.IsNewRack) return;
            WriteSelective(editor, views);
        }

        // «Insertar en el dibujo…» of a new Selectivo: the same write, as the insertion of a new rack with those views (RACKSELECTIVONUEVO).
        private void InsertNewSelective(SelectiveEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0 || !editor.IsNewRack) return;
            WriteSelective(editor, views);
        }

        // AutoCAD placed the new Selectivo: it takes the name the drawing gave it (I-60), «Nuevo» shows its systems again and the window
        // says where it is. The list is not read again (D-10, O-18): the notice asks to read it and open the rack.
        private void NewSelectiveInserted(SelectiveEditorViewModel editor, RackDesignWritten written)
        {
            editor.MarkInserted(written.Name);
            Say(written.Message, failed: false);
            RaiseDynamic();
        }

        private void RaiseNewSelective()
        {
            OnPropertyChanged(nameof(NewSelectiveEditor));
            OnPropertyChanged(nameof(HasNewSelectiveEditor));
            OnPropertyChanged(nameof(ShowNewSelectiveEditor));
            OnPropertyChanged(nameof(CanStartNewSelective));
        }
    }
}
