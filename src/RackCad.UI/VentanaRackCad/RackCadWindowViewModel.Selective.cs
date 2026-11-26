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

        // «Nuevo» holds ONE new rack of the drawing at a time, of either system: its editor replaces the page, so the other button is not
        // shown until that rack is inserted or its editor closed. The window says so if it is asked anyway.
        internal const string SecondNewRackText =
            "En «Nuevo» ya hay un rack nuevo sin insertar: insértalo en el dibujo o cierra su editor antes de empezar otro.";

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

        /// <summary>The new Selectivo of the drawing of the window, made here and not inserted yet; null when there is none.</summary>
        public SelectiveEditorViewModel NewSelectiveEditor =>
            Panel.PresentedSession.HasValue
                ? _selectiveDrafts.Values.FirstOrDefault(e => e.Session == Panel.PresentedSession.Value && e.Editor.IsNewRack)?.Editor
                : null;

        public bool HasNewSelectiveEditor => NewSelectiveEditor != null;

        /// <summary>«Nuevo» shows the new Selectivo being made (a new Dinámico, if there were one too, would be shown first).</summary>
        public bool ShowNewSelectiveEditor => IsNuevoPage && HasNewSelectiveEditor && !HasNewDynamicEditor;

        /// <summary>A new Selectivo can be made here: the window has the door that writes a Selectivo, and a drawing.</summary>
        public bool CanStartNewSelective => _selectiveDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>
        /// «Selectivo en esta ventana» of «Nuevo»: a new Selectivo with the defaults of the classic editor
        /// (<see cref="SelectiveDraftEditor.CreateNew"/>), edited here; «Insertar en el dibujo…» places it. Its identity is given now, so once
        /// inserted the same editor is the one of the rack in the list. Asked again, the window shows the same new Selectivo.
        /// </summary>
        public void StartNewSelective()
        {
            Contained(() =>
            {
                if (_selectiveDoor == null)
                {
                    Say("Aquí no se puede crear un rack en el dibujo.", failed: true);
                    return;
                }

                if (!Panel.PresentedSession.HasValue)
                {
                    Say("No hay un dibujo en la ventana.", failed: true);
                    return;
                }

                if (NewSelectiveEditor == null)
                {
                    if (NewDynamicEditor != null)
                    {
                        ShowPage(RackCadWindowPage.Nuevo);
                        Say(SecondNewRackText, failed: true);
                        RaiseDynamic();
                        return;
                    }

                    var session = Panel.PresentedSession.Value;
                    var rackId = Guid.NewGuid().ToString();
                    var editor = SelectiveEditorViewModel.ForNewRack(SelectiveDraftEditor.CreateNew(_catalog()), rackId, UpdateSelective,
                        DimensionStylesOf(session));
                    WireSelective(editor);
                    _selectiveDrafts[(session, rackId)] = new SelectiveDraftEntry(session, editor);
                }

                ShowPage(RackCadWindowPage.Nuevo);
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        // What every Selectivo editor of the window gets: «Insertar vista…» (the dialog of «Insertar varias…» with the modal port of the
        // host, the writes) and the close. Without the modal port the views cannot be chosen, so «Insertar vista…» is not offered.
        private void WireSelective(SelectiveEditorViewModel editor)
        {
            if (_modal != null) editor.ViewChooser = options => SelectiveViewChoice.Choose(options, _modal);
            editor.InsertViewsAction = InsertSelectiveViews;
            editor.InsertNewAction = InsertNewSelective;
            editor.SaveToLibraryAction = StartSaveToLibrary; // T4 ut: «Más» › «Guardar en biblioteca…»
            editor.PropertyChanged += OnSelectiveEditorChanged;
            editor.CloseRequested += CloseSelectiveEditor;
            WireSelectiveSafety(editor); // E2 seguridad: «Editar seguridad…» for every Selectivo editor, of the drawing or new
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

        // «Dinámico en esta ventana» while a new Selectivo of the drawing is not inserted: the window shows it instead.
        private bool RefuseSecondNewRack()
        {
            if (NewSelectiveEditor == null) return false;
            ShowPage(RackCadWindowPage.Nuevo);
            Say(SecondNewRackText, failed: true);
            RaiseDynamic();
            return true;
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
