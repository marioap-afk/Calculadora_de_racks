using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The templates of cabecera the editor offers: those of the catalog (or the built-in ones) merged with the own ones of this computer.</summary>
    public static class CabeceraTemplates
    {
        /// <summary>Test seam: where the templates come from (production reads the catalog and %APPDATA%\RackCad\user-templates.json).</summary>
        internal static Func<IReadOnlyList<RackFrameTemplate>> Source { get; set; }

        public static IReadOnlyList<RackFrameTemplate> Load()
        {
            if (Source != null) return Source();
            IReadOnlyList<RackFrameTemplate> catalog;
            try
            {
                catalog = RackFrameTemplateProvider.FromBaseDirectory().Load();
            }
            catch (Exception)
            {
                catalog = RackFrameTemplateCatalog.All;
            }

            var merged = (catalog ?? RackFrameTemplateCatalog.All).Where(t => t != null).ToList();
            try
            {
                foreach (var own in new UserTemplateStore(UserTemplateStore.DefaultPath).Load())
                {
                    if (own == null) continue;
                    var at = merged.FindIndex(t => string.Equals(t.Id, own.Id, StringComparison.OrdinalIgnoreCase));
                    if (at >= 0) merged[at] = own; // the user wins, as in the classic configurator
                    else merged.Add(own);
                }
            }
            catch (Exception)
            {
                // The own templates are best effort, as in the classic configurator.
            }

            return merged.Count > 0 ? merged : RackFrameTemplateCatalog.All;
        }
    }

    /// <summary>The cabecera of one post in one fondo, in the table «Postes y cabeceras» of «Avanzado» (CB-10: «Editar cabecera…», «Volver a la calculada»).</summary>
    public sealed class SelectiveCabeceraCell : ObservableObject
    {
        private string _status = string.Empty;
        private bool _isCustom;
        private bool _canEdit;

        public SelectiveCabeceraCell(int fondo, int post, string fondoLabel)
        {
            Fondo = fondo;
            Post = post;
            FondoLabel = fondoLabel ?? string.Empty;
            var where = "P" + (post + 1).ToString(CultureInfo.InvariantCulture) + (FondoLabel.Length > 0 ? " en el " + FondoLabel.ToLowerInvariant() : string.Empty);
            EditName = "Editar la cabecera de " + where;
            CalculatedName = "Volver a la calculada la cabecera de " + where;
        }

        public int Fondo { get; }

        public int Post { get; }

        /// <summary>«Fondo 1» with several fondos; empty with one.</summary>
        public string FondoLabel { get; }

        public string Status { get => _status; private set => SetProperty(ref _status, value); }

        public bool IsCustom { get => _isCustom; private set => SetProperty(ref _isCustom, value); }

        public bool CanEdit { get => _canEdit; private set => SetProperty(ref _canEdit, value); }

        /// <summary>Unique in the table: «Editar la cabecera de P2 en el fondo 1».</summary>
        public string EditName { get; }

        public string CalculatedName { get; }

        internal void Set(bool custom, bool canEdit)
        {
            IsCustom = custom;
            Status = custom ? "Personalizada" : "Calculada";
            CanEdit = canEdit;
        }
    }

    /// <summary>T4 cb: the post row carries its cabeceras, one per fondo that reaches the post.</summary>
    public sealed partial class SelectivePostRow
    {
        private IReadOnlyList<SelectiveCabeceraCell> _cabeceras = Array.Empty<SelectiveCabeceraCell>();

        public IReadOnlyList<SelectiveCabeceraCell> Cabeceras => _cabeceras;

        internal void SetCabeceras(IReadOnlyList<SelectiveCabeceraCell> cells)
        {
            if (ReferenceEquals(_cabeceras, cells)) return;
            _cabeceras = cells;
            OnPropertyChanged(nameof(Cabeceras));
        }
    }

    /// <summary>
    /// T4 cb (I-64, fourth batch; prototype «cabecera», CB-01, CB-08, CB-09, CB-10): the editor of cabeceras inside the one of Selectivo, which
    /// only writes the draft of this rack. «Editar cabecera…» of a post and a fondo opens it (the classic «Personalizar»); «Aplicar al
    /// borrador» and «Copiar a otros postes…» go through the protocol of the classic editor (<see cref="SelectiveDraftEditor.WriteCabecera"/>);
    /// «Volver a la calculada» (the classic «Restablecer poste») is in the table. While it is open the editor of cabeceras takes the place of
    /// the editor of the rack, and a cabecera with changes not applied is work not written of this rack (D-08).
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        private CabeceraEditorViewModel _embeddedCabecera;

        /// <summary>The editor of the cabecera of a post, open inside this one; null when the rack is shown.</summary>
        public CabeceraEditorViewModel EmbeddedCabecera => _embeddedCabecera;

        public bool HasEmbeddedCabecera => _embeddedCabecera != null;

        /// <summary>The cabecera open here has changes not applied to the draft (part of «Modificado», S-21 and D-08).</summary>
        internal bool CabeceraPending => _embeddedCabecera?.IsDirty == true;

        /// <summary>«Editar cabecera…» of the post <paramref name="post"/> in the fondo <paramref name="fondo"/>.</summary>
        public void EditCabecera(int fondo, int post)
        {
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return;
            }

            var seed = _editor.CabeceraSeed(fondo, post);
            if (seed == null)
            {
                Say("Ese poste no existe en ese fondo.", error: true);
                return;
            }

            CabeceraDraftEditor draft;
            try
            {
                draft = new CabeceraDraftEditor(seed, _editor.Catalog, CabeceraTemplates.Load(), _editor.CabeceraContext(fondo, post, Name));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Say("No se pudo abrir la cabecera de " + _editor.CabeceraWhere(fondo, post) + ": " + ex.Message, error: true);
                return;
            }

            var host = new SelectiveCabeceraHost(this, fondo, post);
            OpenCabecera(CabeceraEditorViewModel.Embedded(draft, host, _editor.HasCustomCabecera(fondo, post)));
        }

        /// <summary>«Volver a la calculada» (today «Restablecer poste») of one post in one fondo; its own peralte stays.</summary>
        public void ResetCabecera(int fondo, int post)
        {
            var where = _editor.CabeceraWhere(fondo, post);
            var error = Apply(() => _editor.ResetCabeceraAt(fondo, post));
            if (error == null) Say("La cabecera de " + where + " vuelve a la calculada en el borrador. El dibujo cambia con «Actualizar».", error: false);
        }

        private void OpenCabecera(CabeceraEditorViewModel cabecera)
        {
            CloseCabecera();
            _embeddedCabecera = cabecera;
            cabecera.PropertyChanged += OnCabeceraChanged;
            OnPropertyChanged(nameof(EmbeddedCabecera));
            OnPropertyChanged(nameof(HasEmbeddedCabecera));
            RefreshDirty();
        }

        private void CloseCabecera()
        {
            if (_embeddedCabecera == null) return;
            _embeddedCabecera.PropertyChanged -= OnCabeceraChanged;
            _embeddedCabecera = null;
            OnPropertyChanged(nameof(EmbeddedCabecera));
            OnPropertyChanged(nameof(HasEmbeddedCabecera));
            RefreshDirty();
        }

        private void OnCabeceraChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CabeceraEditorViewModel.IsDirty)) RefreshDirty();
        }

        // The cabeceras of the table «Postes y cabeceras», one per fondo that reaches the post, refreshed in place (one line in Rebuild).
        private void RefreshCabeceraCells()
        {
            foreach (var row in _postRows)
            {
                var reach = Enumerable.Range(0, _editor.FondoCount).Where(k => row.PostIndex <= _editor.FrontCount(k)).ToList();
                var cells = row.Cabeceras;
                if (cells.Count != reach.Count || cells.Where((c, i) => c.Fondo != reach[i]).Any())
                {
                    cells = reach.Select(k => new SelectiveCabeceraCell(k, row.PostIndex, _editor.FondoCount > 1 ? "Fondo " + (k + 1).ToString(CultureInfo.InvariantCulture) : string.Empty)).ToList();
                    row.SetCabeceras(cells);
                }

                foreach (var cell in cells) cell.Set(_editor.HasCustomCabecera(cell.Fondo, cell.Post), CanEdit);
            }
        }

        private CabeceraHostAnswer WriteCabecera(int fondo, int post, RackFrameConfiguration cabecera, IReadOnlyList<SelectiveHeaderAddress> copies, bool force)
        {
            if (!CanEdit) return CabeceraHostAnswer.Refuse(BlockedEditText);
            var where = _editor.CabeceraWhere(fondo, post);
            var write = _editor.WriteCabecera(fondo, post, cabecera, copies, force);
            if (write.NeedsConfirmation) return CabeceraHostAnswer.Confirm(write.Confirmation + " ¿Aplicar igual al borrador?");
            if (write.Edit == null || write.Edit.Refused) return CabeceraHostAnswer.Refuse(write.Edit?.Error ?? "No se aplicó nada.");

            var error = Apply(() => write.Edit);
            if (error != null) return CabeceraHostAnswer.Refuse(error);

            var text = copies == null || copies.Count == 0
                ? "Cabecera de " + where + " aplicada al borrador de " + RackLabel() + ", que queda «Modificado». El dibujo cambia con «Actualizar»."
                : "La cabecera de " + where + " se aplicó y se copió a " + string.Join(", ", copies.Select(c => _editor.CabeceraWhere(c.FondoIndex, c.PostIndex)))
                  + ", cada una con su profundidad y su peralte. " + RackLabel() + " queda «Modificado».";
            if (!string.IsNullOrWhiteSpace(write.Notice)) text += " " + write.Notice;
            CloseCabecera();
            Say(text, error: false);
            return CabeceraHostAnswer.Done(text);
        }

        private string RackLabel() => string.IsNullOrWhiteSpace(Name) ? "el rack" : Name.Trim();

        /// <summary>The rack that holds the embedded cabecera of one post in one fondo.</summary>
        private sealed class SelectiveCabeceraHost : ICabeceraEditorHost
        {
            private readonly SelectiveEditorViewModel _owner;
            private readonly int _fondo;
            private readonly int _post;

            public SelectiveCabeceraHost(SelectiveEditorViewModel owner, int fondo, int post)
            {
                _owner = owner;
                _fondo = fondo;
                _post = post;
                CopyTargets = owner._editor.CabeceraCopyTargets(fondo, post)
                    .Select(a => new CabeceraCopyTarget(Key(a), owner._editor.CabeceraWhere(a.FondoIndex, a.PostIndex)))
                    .ToList();
            }

            public string HostName => _owner.RackLabel() == "el rack" ? "Selectivo" : _owner.RackLabel();

            public string Title => "Cabecera de " + _owner._editor.CabeceraWhere(_fondo, _post);

            public string CopyLabel => "Copiar a otros postes…";

            public IReadOnlyList<CabeceraCopyTarget> CopyTargets { get; }

            public CabeceraHostAnswer Apply(RackFrameConfiguration cabecera, bool force)
                => _owner.WriteCabecera(_fondo, _post, cabecera, null, force);

            public CabeceraHostAnswer Copy(RackFrameConfiguration cabecera, IReadOnlyList<CabeceraCopyTarget> targets, bool force)
            {
                var addresses = (targets ?? Array.Empty<CabeceraCopyTarget>()).Select(t => Parse(t.Key)).Where(a => a.HasValue).Select(a => a.Value).ToList();
                if (addresses.Count == 0) return CabeceraHostAnswer.Refuse("Elige al menos un destino.");
                return _owner.WriteCabecera(_fondo, _post, cabecera, addresses, force);
            }

            public void Back() => _owner.CloseCabecera();

            private static string Key(SelectiveHeaderAddress address)
                => address.FondoIndex.ToString(CultureInfo.InvariantCulture) + ":" + address.PostIndex.ToString(CultureInfo.InvariantCulture);

            private static SelectiveHeaderAddress? Parse(string key)
            {
                var parts = (key ?? string.Empty).Split(':');
                return parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var fondo)
                       && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var post)
                    ? new SelectiveHeaderAddress(fondo, post)
                    : (SelectiveHeaderAddress?)null;
            }
        }
    }
}
