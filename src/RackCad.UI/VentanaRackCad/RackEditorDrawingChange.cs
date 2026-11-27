using System;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 st (I-64, sexta tanda; O-5 of the smoke of the fifth, M-16 of Dinámico; the Owner on 2026-11-27: «Avisar y ofrecer releer»): what
    /// an editor of the window says when a command that is not of the window ran in its drawing after the editor read it and changed it (a
    /// «U», a RACKEDITAR, an ERASE…; not a ZOOM: the Owner, «Afinarlo»). The window marks it (<see cref="MarkChanged"/>); the editor shows the
    /// notice and «Volver a leer del dibujo», which is the read
    /// of «Editar aquí» with the gesture of the user. Nothing is read on its own: no order leaves the window without a gesture.
    ///
    /// With changes not written, the notice says that «Actualizar» writes the draft over what the drawing has, and «Actualizar» asks first,
    /// inline (<see cref="IsConfirmingUpdate"/>): «Escribir encima» writes, «Cancelar» does not. A reread (a new editor) or a write of the
    /// editor that AutoCAD did (<see cref="Clear"/>) removes the mark. One object per editor; the six editors share it.
    /// </summary>
    public sealed class RackEditorDrawingChange : ObservableObject
    {
        internal const string ChangedText = "El dibujo cambió desde que se leyó (corrió una orden en AutoCAD).";

        internal const string CleanText = ChangedText + " Lo que muestra el editor puede no ser lo que hay ahora en el dibujo: pulsa «Volver a leer del dibujo».";

        internal const string DirtyText = ChangedText + " Tienes cambios sin escribir: «Actualizar» escribirá el borrador encima de lo que hay en el dibujo. "
                                          + "«Volver a leer del dibujo» descarta tus cambios y lee lo que hay ahora.";

        internal const string ConfirmUpdateText = "¿Escribir el borrador encima de lo que hay ahora en el dibujo? Si la orden que corrió cambió este rack, ese cambio se pierde.";

        internal const string InsertCleanText = "El dibujo cambió desde que se leyó: pulsa «Volver a leer del dibujo» antes de insertar vistas.";

        internal const string InsertDirtyText = "El dibujo cambió desde que se leyó: escribe antes el borrador con «Actualizar», o vuelve a leer del dibujo, para insertar vistas.";

        private readonly Func<bool> _isDirty;
        private readonly Func<bool> _isWriting;
        private readonly Action _write;
        private readonly Action _stateChanged;
        private Action _reread;
        private bool _changed;
        private bool _confirming;

        /// <param name="isDirty">Whether the draft has changes not written.</param>
        /// <param name="isWriting">Whether AutoCAD is writing the draft (nothing is offered meanwhile).</param>
        /// <param name="write">The write of «Actualizar», with its checks, for «Escribir encima».</param>
        /// <param name="stateChanged">The editor raises its state (what «Insertar vista…» can do follows the mark).</param>
        internal RackEditorDrawingChange(Func<bool> isDirty, Func<bool> isWriting, Action write, Action stateChanged)
        {
            _isDirty = isDirty ?? throw new ArgumentNullException(nameof(isDirty));
            _isWriting = isWriting ?? throw new ArgumentNullException(nameof(isWriting));
            _write = write ?? throw new ArgumentNullException(nameof(write));
            _stateChanged = stateChanged;
        }

        /// <summary>A command that is not of the window ran in the drawing since the editor read it (or last wrote it).</summary>
        public bool IsChanged => _changed;

        /// <summary>The notice: with changes not written, what «Actualizar» and «Volver a leer del dibujo» do; empty without the mark.</summary>
        public string Text => !_changed ? string.Empty : _isDirty() ? DirtyText : CleanText;

        /// <summary>«Volver a leer del dibujo» is on: the mark, the read of the window, and AutoCAD is not writing the draft.</summary>
        public bool CanReread => _changed && _reread != null && !_isWriting();

        /// <summary>«Actualizar» of a draft with changes over a drawing that changed asks first, inline.</summary>
        public bool IsConfirmingUpdate => _confirming;

        public string ConfirmText => ConfirmUpdateText;

        /// <summary>Why «Insertar vista…» waits while the mark is on (it writes the draft in the views first); empty without the mark.</summary>
        internal string InsertBlockedText => !_changed ? string.Empty : _isDirty() ? InsertDirtyText : InsertCleanText;

        /// <summary>«Volver a leer del dibujo»: the read of «Editar aquí», asked by the gesture of the user.</summary>
        public void Reread()
        {
            if (!CanReread) return;
            SetConfirming(false);
            _reread();
        }

        /// <summary>«Escribir encima» of the question: the write of «Actualizar».</summary>
        public void ConfirmUpdate()
        {
            if (!_confirming) return;
            SetConfirming(false);
            _write();
        }

        /// <summary>«Cancelar» of the question: nothing is written, the notice stays.</summary>
        public void CancelUpdate() => SetConfirming(false);

        /// <summary>«Actualizar» asks first when the drawing changed and the draft has changes: true when it asked (nothing is written yet).</summary>
        internal bool AsksBeforeWrite()
        {
            if (!_changed || !_isDirty()) return false;
            SetConfirming(true);
            return true;
        }

        /// <summary>The window marks the editor, with the read that «Volver a leer del dibujo» asks for.</summary>
        internal void MarkChanged(Action reread)
        {
            _reread = reread;
            var was = _changed;
            _changed = true;
            Raise();
            if (!was) _stateChanged?.Invoke();
        }

        /// <summary>AutoCAD wrote the draft of the editor: what was written is the drawing now.</summary>
        internal void Clear()
        {
            if (!_changed && !_confirming) return;
            _changed = false;
            _confirming = false;
            Raise();
            _stateChanged?.Invoke();
        }

        /// <summary>The editor changed (its changes, or AutoCAD writes it): the texts follow, and a question that no longer applies goes away.</summary>
        internal void Refresh()
        {
            if (_confirming && (!_changed || !_isDirty() || _isWriting())) _confirming = false;
            Raise();
        }

        private void SetConfirming(bool value)
        {
            if (_confirming == value) return;
            _confirming = value;
            OnPropertyChanged(nameof(IsConfirmingUpdate));
        }

        private void Raise()
        {
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(CanReread));
            OnPropertyChanged(nameof(IsConfirmingUpdate));
        }
    }
}
