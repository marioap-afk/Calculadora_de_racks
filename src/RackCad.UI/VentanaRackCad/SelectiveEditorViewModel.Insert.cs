using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.UI.Views;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Insertar vista…» and the new Selectivo of the Selectivo editor (entrega 2, part P3), as the second delivery of Dinámico did them
    /// (<see cref="DynamicEditorViewModel"/>, step 4; decision 10 of the prototype): the views are chosen with the dialog of «Insertar
    /// varias…» of the classic editor, and the window writes the draft in the views that exist and then inserts the chosen ones
    /// (RACKSELECTIVOINSERTAR). A new Selectivo made in the window («Selectivo en esta ventana» of «Nuevo») has its identity but is not in
    /// the drawing: the same button, «Insertar en el dibujo…», inserts it (RACKSELECTIVONUEVO), and until then «Actualizar» is not
    /// offered. Once AutoCAD placed it, the same editor is the one of the rack of the drawing.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private const string NewRackUpdateText = "Un rack nuevo se escribe en el dibujo con «Insertar en el dibujo…».";

        // What the window says when it opens a new Selectivo here (the approved draft).
        internal const string NewRackOpenedText = "Selectivo nuevo en esta ventana, con los valores por defecto del editor clásico. Todavía no está en el dibujo.";

        private bool _isNewRack;
        private Action<SelectiveEditorViewModel, IReadOnlyList<RackViewAddress>> _insertViews;
        private Action<SelectiveEditorViewModel, IReadOnlyList<RackViewAddress>> _insertNew;
        private Func<IReadOnlyList<RackViewBatchOption>, IReadOnlyList<RackViewAddress>> _viewChooser;

        /// <summary>
        /// The editor of a new Selectivo made in the window: <paramref name="rackId"/> is the identity the window gave it, and it is not in
        /// the drawing until «Insertar en el dibujo…» places it. <paramref name="update"/> is what «Actualizar» does once it is.
        /// <paramref name="dimensionStyles"/> are the styles of its drawing the window knows (M-3 of the review of the entrega 2: those of the
        /// last Selectivo read from it); null = none, and «Estilo de cota» offers «(Automático)» only.
        /// </summary>
        public static SelectiveEditorViewModel ForNewRack(SelectiveDraftEditor editor, string rackId, Action<SelectiveEditorViewModel> update = null,
            IReadOnlyList<string> dimensionStyles = null)
        {
            var created = new SelectiveEditorViewModel(editor, rackId, update, dimensionStyles: dimensionStyles) { _isNewRack = true };
            if (!created.IsLoadFailed) created.Say(NewRackOpenedText, error: false);
            created.RaiseNewRack();
            return created;
        }

        /// <summary>A new Selectivo of the window that is not in the drawing yet («Insertar en el dibujo…» places it).</summary>
        public bool IsNewRack => _isNewRack;

        /// <summary>What «Insertar vista…» does with the views chosen: the window first writes the draft in the views that exist, then inserts them.</summary>
        public Action<SelectiveEditorViewModel, IReadOnlyList<RackViewAddress>> InsertViewsAction
        {
            get => _insertViews;
            set
            {
                _insertViews = value;
                RaiseState();
            }
        }

        /// <summary>How the views are chosen: the dialog of «Insertar varias…» of the classic editor, over AutoCAD; null when cancelled.</summary>
        public Func<IReadOnlyList<RackViewBatchOption>, IReadOnlyList<RackViewAddress>> ViewChooser
        {
            get => _viewChooser;
            set
            {
                _viewChooser = value;
                RaiseState();
            }
        }

        /// <summary>What «Insertar en el dibujo…» of a new Selectivo does with the views chosen: the window inserts the rack with them.</summary>
        public Action<SelectiveEditorViewModel, IReadOnlyList<RackViewAddress>> InsertNewAction
        {
            get => _insertNew;
            set
            {
                _insertNew = value;
                RaiseState();
            }
        }

        /// <summary>«Insertar vista…» for a rack of the drawing; «Insertar en el dibujo…» for a new one.</summary>
        public string InsertViewLabel => _isNewRack ? "Insertar en el dibujo…" : "Insertar vista…";

        /// <summary>The name of the button for UI Automation, which follows its label.</summary>
        public string InsertViewAccessibleText => _isNewRack ? "Insertar el rack nuevo en el dibujo" : "Insertar vistas del rack";

        /// <summary>What the button does (the text of Dinámico; for a new rack, the note of the approved draft, without the name it will take).</summary>
        public string InsertViewToolTip => _isNewRack
            ? "Inserta el rack nuevo en el dibujo, con una identidad nueva, con las vistas que elijas."
            : "Escribe el borrador en las vistas del dibujo y después inserta las vistas que elijas, enlazadas al rack.";

        public bool CanInsertView => string.IsNullOrEmpty(InsertViewBlockedText);

        /// <summary>
        /// Why «Insertar vista…» cannot run now; empty when it can. The rules of Dinámico, and the two of «Actualizar» of a Selectivo: a rack
        /// read only (it could not be recomposed when it was opened, S-08) and tramos written in the inspector and not applied (S-21), which
        /// the write would not carry. With no changes it still runs: the views that exist are redrawn as they are.
        /// </summary>
        public string InsertViewBlockedText
        {
            get
            {
                var action = _isNewRack ? _insertNew : _insertViews;
                if (action == null || _viewChooser == null || string.IsNullOrWhiteSpace(RackId))
                    return _isNewRack ? "Aquí no se puede insertar el rack nuevo." : "Aquí no se pueden insertar vistas del rack.";
                if (_loadError != null) return LoadFailedText;
                if (_writing) return WritingText;
                if (_segmentsPending) return PendingSegmentsText;
                if (_editor.Design == null) return "No hay un borrador válido que escribir.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                return string.Empty;
            }
        }

        /// <summary>
        /// «Insertar vista…»: the views to insert, chosen as in «Insertar varias…» of the classic editor (a frontal per fondo, a lateral per
        /// post with a cut, the plan); AutoCAD first writes the draft in the views that exist (also with no change: they are redrawn as they
        /// are) and then places the new ones, linked to the rack. For a new Selectivo, «Insertar en el dibujo…» inserts it with them.
        /// </summary>
        public void InsertView()
        {
            if (!CanInsertView)
            {
                Say(InsertViewBlockedText, error: true);
                return;
            }

            // The options read the cuts of the system, which impose on its custom cabeceras (ARQ-43-10); the system of the draft shares no
            // object with its design, so choosing (or cancelling) changes nothing of what is written (I-1 of the review of the entrega 2).
            var views = _viewChooser(SelectiveViewChoice.Options(_editor.System, _editor.Catalog));
            if (views == null || views.Count == 0) return;
            (_isNewRack ? _insertNew : _insertViews)(this, views);
        }

        /// <summary>
        /// AutoCAD placed the new Selectivo: from now on it is a rack of the drawing, written with «Actualizar» and «Insertar vista…». The
        /// name the drawing gave it (the automatic one of I-60) becomes its name and the name of «Limpio», only when the draft had none.
        /// </summary>
        public void MarkInserted(string name = null)
        {
            if (!_isNewRack) return;
            _isNewRack = false;
            if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(_editor.Name))
            {
                _editor.Name = name.Trim();
                _baselineName = _editor.Name;
                OnPropertyChanged(nameof(Name));
                UpdateDirty();
            }

            RaiseNewRack();
        }

        private void RaiseNewRack()
        {
            OnPropertyChanged(nameof(IsNewRack));
            OnPropertyChanged(nameof(InsertViewLabel));
            OnPropertyChanged(nameof(InsertViewAccessibleText));
            OnPropertyChanged(nameof(InsertViewToolTip));
            RaiseState();
        }
    }
}
