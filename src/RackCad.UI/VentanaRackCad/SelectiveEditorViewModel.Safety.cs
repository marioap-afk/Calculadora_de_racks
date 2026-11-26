using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One row of «Seguridad por poste» of a Selectivo, read only, with the columns of the approved draft: Poste · Botas · Protector lateral
    /// · Desviadores (niveles) · Separadores entre fondos. An empty cell is «—», as in Dinámico.
    /// </summary>
    public sealed class SelectivePostSafetyItem
    {
        internal SelectivePostSafetyItem(string post, string boots, string sideGuards, string deflectors, string separators)
        {
            Post = post;
            Boots = boots;
            SideGuards = sideGuards;
            Deflectors = deflectors;
            Separators = separators;
        }

        internal static SelectivePostSafetyItem Of(SelectivePostSafetyRow row)
            => new SelectivePostSafetyItem(
                PostText(row),
                Dash(SelectiveSafetyView.BootSide(row)),
                row.SideGuards == 0 ? "—" : (row.IsEndPost ? "Sí, extremo" : "Sí") + (row.SideGuards > 1 ? " (" + N(row.SideGuards) + ")" : string.Empty),
                row.Deflectors == 0 ? "—" : Levels(row.DeflectorLevels) + " (" + N(row.Deflectors) + ")",
                row.Separators == 0 ? "—" : N(row.Separators));

        /// <summary>The «Total» row: the pieces of each column (the separadores, «—» when the rack has one fondo).</summary>
        internal static SelectivePostSafetyItem Total(IReadOnlyList<SelectivePostSafetyRow> rows, bool severalFondos)
            => new SelectivePostSafetyItem(
                "Total",
                N(rows.Sum(r => r.Boots)),
                N(rows.Sum(r => r.SideGuards)),
                N(rows.Sum(r => r.Deflectors)),
                severalFondos ? N(rows.Sum(r => r.Separators)) : "—");

        public string Post { get; }

        /// <summary>The side of the bota: «Izquierda», «Derecha» or both (decision 13; the text for both is provisional).</summary>
        public string Boots { get; }

        public string SideGuards { get; }

        /// <summary>The levels of the desviadores («N1–N3») and, in brackets, the pieces (both faces count).</summary>
        public string Deflectors { get; }

        public string Separators { get; }

        /// <summary>«P1»… for a frame post; an intermediate post of a medio frente is named after its front.</summary>
        internal static string PostText(SelectivePostSafetyRow row)
            => row.IsIntermediate
                ? "F" + N(row.IntermediateOfFront.Value + 1) + " · intermedio " + N(row.IntermediateOrdinal)
                : "P" + N(row.PostIndex + 1);

        // «N1–N3, N5»: the zero-based load levels of the desviador plan, N1 being its first troquel row.
        internal static string Levels(IReadOnlyList<int> levels)
        {
            var runs = new List<string>();
            var ordered = levels.Distinct().OrderBy(l => l).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                var start = ordered[i];
                while (i + 1 < ordered.Count && ordered[i + 1] == ordered[i] + 1) i++;
                runs.Add(start == ordered[i] ? "N" + N(start + 1) : "N" + N(start + 1) + "–N" + N(ordered[i] + 1));
            }

            return string.Join(", ", runs);
        }

        private static string Dash(string text) => string.IsNullOrEmpty(text) ? "—" : text;

        private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>«Extremos y seguridad» of the Selectivo editor (entrega 2, part C): T/P on the selected cells, for the whole rack, and the table by post.</summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private Func<SelectiveDraftEditor, IReadOnlyList<SelectiveSafetySelection>> _safetyEditor;

        // The table by post of the system it was counted on: a recomposition gives a new system, and only then is it counted again.
        private SelectiveRackSystem _postSafetyOf;
        private IReadOnlyList<SelectivePostSafetyRow> _postSafetyRows = Array.Empty<SelectivePostSafetyRow>();

        /// <summary>
        /// What «Editar seguridad…» opens: the safety dialog of the classic editor over copies of the safety of the draft (its result, or null
        /// when it was cancelled). The window sets it when it can show a modal over AutoCAD (D-09); without it the button is not offered.
        /// </summary>
        public Func<SelectiveDraftEditor, IReadOnlyList<SelectiveSafetySelection>> SafetyEditor
        {
            get => _safetyEditor;
            set
            {
                _safetyEditor = value;
                OnPropertyChanged(nameof(CanEditSafety));
            }
        }

        public bool CanEditSafety => CanEdit && _safetyEditor != null;

        /// <summary>«Seguridad por poste», counted with the projections of the BOM over the last valid recomposition.</summary>
        public IReadOnlyList<SelectivePostSafetyItem> PostSafety => PostSafetyRows().Select(SelectivePostSafetyItem.Of).ToList();

        /// <summary>The «Total» row of «Seguridad por poste».</summary>
        public SelectivePostSafetyItem PostSafetyTotal => SelectivePostSafetyItem.Total(PostSafetyRows(), _editor.FondoCount > 1);

        /// <summary>The summary above the table, as the draft says it: «8 botas · 2 protectores · 12 desviadores, en 5 postes.»</summary>
        public string SafetySummary
        {
            get
            {
                var rows = PostSafetyRows();
                var several = _editor.FondoCount > 1;
                return Plural(rows.Sum(r => r.Boots), "bota", "botas")
                       + " · " + Plural(rows.Sum(r => r.SideGuards), "protector", "protectores")
                       + " · " + Plural(rows.Sum(r => r.Deflectors), "desviador", "desviadores")
                       + (several ? " · " + Plural(rows.Sum(r => r.Separators), "separador", "separadores") : string.Empty)
                       + ", en " + Plural(rows.Count, "poste", "postes") + ".";
            }
        }

        /// <summary>T at the selected cells (null when it varies or no tope is chosen).</summary>
        public bool? SelectionTope
        {
            get
            {
                var value = SelectiveSafetyView.ReadTope(_editor, SelectedPositions);
                return value.HasValue ? value.Value : (bool?)null;
            }
        }

        /// <summary>P at the selected cells (null when it varies or no parrilla is chosen).</summary>
        public bool? SelectionParrilla
        {
            get
            {
                var value = SelectiveSafetyView.ReadParrilla(_editor, SelectedPositions);
                return value.HasValue ? value.Value : (bool?)null;
            }
        }

        /// <summary>T on or off at the selected cells, for the whole rack; returns the reason when it was not written.</summary>
        public string SetTope(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteTope(_editor, SelectedPositions, on));

        /// <summary>P on or off at the selected cells, for the whole rack; returns the reason when it was not written.</summary>
        public string SetParrilla(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteParrilla(_editor, SelectedPositions, on));

        /// <summary>«En todas las celdas» (true) / «En ninguna» (false) of the tope card.</summary>
        public string SetTopeEverywhere(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteAll(_editor, SelectiveSafetyDefaults.TopeType, on));

        /// <summary>«En todas las celdas» (true) / «En ninguna» (false) of the parrilla card.</summary>
        public string SetParrillaEverywhere(bool on) => SetSafetyCells(() => SelectiveSafetyView.WriteAll(_editor, SelectiveSafetyDefaults.ParrillaType, on));

        /// <summary>«Editar seguridad…»: the classic dialog; its result replaces the safety of the draft, which is recomposed.</summary>
        public void EditSafety()
        {
            if (!CanEditSafety)
            {
                Say(_safetyEditor == null
                    ? "El diálogo de seguridad no está disponible aquí: usa «Abrir en editor clásico»."
                    : WritingText, error: true);
                return;
            }

            var result = _safetyEditor(_editor);
            if (result == null) return;
            Apply(() =>
            {
                _editor.ReplaceSafety(result);
                return Done;
            });
            OnSafetyChanged();
        }

        private string SetSafetyCells(Func<SelectiveSafetyEdit> write)
        {
            SelectiveSafetyEdit outcome = null;
            var error = Apply(() => (outcome = write()).ToDraftEdit());
            if (error != null) Say(error, error: true);
            else if (outcome?.Notice != null) Say(outcome.Notice, error: false);
            OnSafetyChanged();
            return error;
        }

        private IReadOnlyList<SelectivePostSafetyRow> PostSafetyRows()
        {
            if (!ReferenceEquals(_postSafetyOf, _editor.System))
            {
                _postSafetyRows = SelectiveSafetyView.PostRows(_editor);
                _postSafetyOf = _editor.System;
            }

            return _postSafetyRows;
        }

        private static string Plural(int count, string one, string many)
            => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        private void OnSafetyChanged()
        {
            OnPropertyChanged(nameof(SelectionTope));
            OnPropertyChanged(nameof(SelectionParrilla));
            OnPropertyChanged(nameof(PostSafety));
            OnPropertyChanged(nameof(PostSafetyTotal));
            OnPropertyChanged(nameof(SafetySummary));
        }
    }
}
