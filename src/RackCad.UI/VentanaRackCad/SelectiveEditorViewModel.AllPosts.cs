using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pp (I-64, fifth batch; the request of the Owner of 2026-11-26: «en el selectivo no hay forma de colocar un peralte de poste en todos
    /// los postes, tienes que ir uno por uno»). Two pieces:
    /// <list type="bullet">
    /// <item>«Avanzado» › «Postes y cabeceras» has a row «Todos los postes»: a peralte and «Aplicar a todos», which makes it the peralte of
    /// the rack and takes every own peralte away (<see cref="SelectiveDraftEditor.SetPeralteForAllPosts"/>), in the draft; the message says
    /// how many own peraltes it took away. Leaving the box writes nothing: only the button (or Enter in the box) does.</item>
    /// <item>«Rack» › «Lo esencial» has the group «Poste» with «Peralte del poste» (before, only in «Poste y anotaciones», which starts
    /// folded): «Para todos los postes», and which posts keep an own one.</item>
    /// </list>
    /// What is written goes to the draft; the drawing changes with «Actualizar» (or «Insertar en el dibujo…» for a new rack), no new command.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private string _allPostsText;
        private string _allPostsShown;
        private string _allPostsError = string.Empty;

        /// <summary>The peralte of the row «Todos los postes»: the one of the rack until the user writes another one.</summary>
        public string AllPostsPeralteText
        {
            get => _allPostsText ?? Num(_editor.PostPeralte);
            set
            {
                if (!SetProperty(ref _allPostsText, value ?? string.Empty)) return;
                AllPostsError = string.Empty;
            }
        }

        /// <summary>Why the last «Aplicar a todos» wrote nothing; empty otherwise.</summary>
        public string AllPostsError
        {
            get => _allPostsError;
            private set => SetProperty(ref _allPostsError, value ?? string.Empty);
        }

        /// <summary>«Aplicar a todos» is offered while the draft can be edited.</summary>
        public bool CanApplyPeralteToAllPosts => CanEdit;

        public bool IsAllPostsReadOnly => !CanEdit;

        /// <summary>What «Aplicar a todos» will do, with the own peraltes it will take away.</summary>
        public string AllPostsHint
        {
            get
            {
                var own = _editor.OwnPostPeraltes();
                if (own.Count == 0) return "Pone este peralte como el del rack, para todos los postes. Ningún poste tiene peralte propio.";
                return "Pone este peralte como el del rack y quita " + (own.Count == 1 ? "el peralte propio de " : "los " + own.Count.ToString(CultureInfo.InvariantCulture) + " peraltes propios: ")
                       + string.Join(", ", own.Select(o => "P" + (o.Post + 1).ToString(CultureInfo.InvariantCulture) + " (" + Num(o.Peralte) + " in)")) + ".";
            }
        }

        /// <summary>
        /// «Aplicar a todos»: the peralte of the row becomes the one of the rack and no post keeps an own one, in the draft. A value that is
        /// not a number greater than zero is refused in the row and writes nothing.
        /// </summary>
        public void ApplyPeralteToAllPosts()
        {
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return;
            }

            if (!UiSupport.TryNum(AllPostsPeralteText, out var peralte) || !(peralte > 0.0) || double.IsInfinity(peralte))
            {
                AllPostsError = SelectiveDraftEditor.InvalidAllPostsPeralteText;
                return;
            }

            SelectivePeralteForAllPosts result = null;
            var error = Apply(() =>
            {
                result = _editor.SetPeralteForAllPosts(peralte);
                return result.Edit;
            });
            if (error != null)
            {
                AllPostsError = error;
                return;
            }

            // The row shows the peralte of the rack again, which is the one just written.
            _allPostsText = null;
            _allPostsShown = null;
            AllPostsError = string.Empty;
            RaiseAllPosts();
            Say(result.NoticeFor(_isNewRack), error: false);
        }

        // «Lo esencial» › «Poste»: the peralte of the rack, the same write as the box of «Poste y anotaciones» had (the own peraltes stay).
        private DynamicFieldItem RackPeralteEssential() =>
            Number("peralte", "Peralte del poste", "in", _editor.PostPeralte, v => v > 0.0, "El peralte de poste debe ser un número mayor que cero.", v =>
            {
                _editor.PostPeralte = v;
                return Done;
            }, RackPeralteHint());

        private string RackPeralteHint()
        {
            var own = _editor.OwnPostPeraltes();
            if (own.Count == 0) return "Para todos los postes. Un peralte propio, poste por poste, en «Avanzado».";
            return "Para todos los postes salvo " + (own.Count == 1 ? "el de peralte propio: P" : "los de peralte propio: P")
                   + string.Join(", P", own.Select(o => (o.Post + 1).ToString(CultureInfo.InvariantCulture)))
                   + ". «Aplicar a todos», en «Avanzado», " + (own.Count == 1 ? "lo quita." : "los quita.");
        }

        // After a recomposition, and when the state changes: the row follows the peralte of the rack unless the user wrote another one.
        private void RaiseAllPosts()
        {
            var rack = Num(_editor.PostPeralte);
            if (_allPostsText != null && _allPostsShown != null && string.Equals(_allPostsText, _allPostsShown, System.StringComparison.Ordinal))
                _allPostsText = null;
            _allPostsShown = rack;
            OnPropertyChanged(nameof(AllPostsPeralteText));
            OnPropertyChanged(nameof(AllPostsHint));
            OnPropertyChanged(nameof(CanApplyPeralteToAllPosts));
            OnPropertyChanged(nameof(IsAllPostsReadOnly));
        }
    }
}
