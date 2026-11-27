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
    /// <item>«Rack» › «Lo esencial» has the group «Peralte de poste» with «Peralte del poste» (before, only in «Poste y anotaciones», which
    /// starts folded): «Para todos los postes», and which posts keep an own one.</item>
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

        /// <summary>What «Aplicar a todos» will do, with the own peraltes it will take away and the base plates that keep theirs.</summary>
        public string AllPostsHint
        {
            get
            {
                var own = _editor.OwnPostPeraltes();
                var hint = own.Count == 0
                    ? "Pone este peralte como el del rack, para todos los postes. Ningún poste tiene peralte propio."
                    : "Pone este peralte como el del rack y quita " + (own.Count == 1 ? "el peralte propio de " : "los " + own.Count.ToString(CultureInfo.InvariantCulture) + " peraltes propios: ")
                      + string.Join(", ", own.Select(o => "P" + (o.Post + 1).ToString(CultureInfo.InvariantCulture) + " (" + Num(o.Peralte) + " in)")) + ".";
                return WithPlates(hint); // correction 3 of the review
            }
        }

        /// <summary>
        /// «Aplicar a todos»: the peralte of the row becomes the one of the rack and no post keeps an own one, in the draft. A value that is
        /// not a number greater than zero is refused in the row and writes nothing; so is one that a «medio frente» would no longer fit
        /// (correction 1 of the review). With the row as it shows the peralte of the rack (the user wrote nothing, or wrote what it shows), the
        /// peralte of the rack is applied as it is, not rounded to what the box shows (correction 2).
        /// </summary>
        public void ApplyPeralteToAllPosts()
        {
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return;
            }

            var shown = _allPostsText == null || string.Equals(_allPostsText.Trim(), Num(_editor.PostPeralte), System.StringComparison.Ordinal);
            var peralte = _editor.PostPeralte;
            if (!shown && (!UiSupport.TryNum(_allPostsText, out peralte) || !(peralte > 0.0) || double.IsInfinity(peralte)))
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
            Say(result.NoticeFor(_isNewRack, CanDiscard, _isCopy), error: false); // T6 ui (point 8): «Descartar» only while it is on; a copy
        }

        // «Lo esencial» › «Peralte de poste»: the peralte of the rack, the same write as the box of «Poste y anotaciones» had (the own peraltes
        // stay), and refused when a «medio frente» would no longer fit (correction 1 of the review).
        private DynamicFieldItem RackPeralteEssential() =>
            Number("peralte", "Peralte del poste", "in", _editor.PostPeralte, v => v > 0.0, SelectiveDraftEditor.InvalidAllPostsPeralteText,
                v => _editor.SetRackPostPeralte(v), RackPeralteHint());

        private string RackPeralteHint()
        {
            var own = _editor.OwnPostPeraltes();
            var hint = own.Count == 0
                ? "Para todos los postes. Un peralte propio, poste por poste, en «Avanzado»."
                : "Para todos los postes salvo " + (own.Count == 1 ? "el de peralte propio: P" : "los de peralte propio: P")
                  + string.Join(", P", own.Select(o => (o.Post + 1).ToString(CultureInfo.InvariantCulture)))
                  + ". «Aplicar a todos», en «Avanzado», " + (own.Count == 1 ? "lo quita." : "los quita.");
            return WithPlates(hint); // correction 3 of the review
        }

        // Correction 3 of the review: the base plates of custom cabeceras with a peralte of their own do not follow the post; say which.
        private string WithPlates(string hint)
        {
            var plates = SelectiveDraftEditor.OwnPlatePeralteText(_editor.CabecerasWithOwnPlatePeralte());
            return plates.Length == 0 ? hint : hint + " " + plates;
        }

        // Correction 4 of the review: «Descartar» and what AutoCAD wrote start the row again from the peralte of the rack, with no reason.
        private void ResetAllPosts()
        {
            _allPostsText = null;
            _allPostsShown = null;
            _allPostsError = string.Empty;
            OnPropertyChanged(nameof(AllPostsError));
            OnPropertyChanged(nameof(AllPostsPeralteText));
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
