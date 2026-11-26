using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One design of the library as the page shows it: a plain value. Its name for UI Automation carries its system and its date.</summary>
    public sealed record RackCadLibraryRow(string Path, string Name, RackSystemKind Kind, string KindLabel, string ModifiedText, string FileName,
        bool IsSelected)
    {
        public string AccessibleText => Name + " · " + KindLabel + " · " + ModifiedText + (IsSelected ? " · elegido" : string.Empty);
    }

    /// <summary>One option of «Sistema» of the library: «todos» or the name of a <see cref="RackSystemKind"/>, with its count.</summary>
    public sealed record RackCadLibraryKindOption(string Key, string Label);

    /// <summary>
    /// What «Más» › «Guardar en biblioteca…» of an editor of the window hands to the page (UT-04): the editor, the name it proposes (the
    /// name of the rack), its system, what to say about its changes and how the design is built when the user presses «Guardar». The build
    /// gives the project to write, or why there is none.
    /// </summary>
    public sealed class RackCadLibrarySaveSource
    {
        public RackCadLibrarySaveSource(object editor, string rackName, string kindLabel, Func<string> changesNote, Func<string, (RackProject Project, string Error)> build)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            RackName = (rackName ?? string.Empty).Trim();
            KindLabel = kindLabel ?? string.Empty;
            ChangesNote = changesNote ?? (() => string.Empty);
            Build = build ?? throw new ArgumentNullException(nameof(build));
        }

        public object Editor { get; }

        public string RackName { get; }

        public string KindLabel { get; }

        /// <summary>« (este rack tiene cambios sin escribir)», or nothing: read when the page shows the hint.</summary>
        public Func<string> ChangesNote { get; }

        public Func<string, (RackProject Project, string Error)> Build { get; }
    }

    /// <summary>
    /// The page «Biblioteca de diseños» of the RackCad window (I-64 A-14, line D, fourth batch, part «ut»; prototype «utilidades», UT-02 to
    /// UT-07): the designs of the folder with a search and a filter by system, the files it could not read, a folder that does not exist
    /// told apart from an empty one, an inspector that says what «Abrir» does, and the line of «Guardar en biblioteca…» of the editors of
    /// the window, without the dialog of Windows. It reads and writes only files of the folder of the library, never the drawing; opening a
    /// design is the window's (a new rack here, or the classic editor through its door).
    /// </summary>
    public sealed class RackCadLibraryViewModel : ObservableObject
    {
        internal const string AllKinds = "todos";

        // The names of the systems of the column of the window (UT-07), in its order, also in the library («Dinámico», «Cama»).
        private static readonly IReadOnlyList<(RackSystemKind Kind, string Label)> KindOrder = new[]
        {
            (RackSystemKind.SelectiveRack, "Selectivo"),
            (RackSystemKind.PushBack, "Push Back"),
            (RackSystemKind.PalletFlow, "Dinámico"),
            (RackSystemKind.Cantilever, "Cantilever"),
            (RackSystemKind.Selective, "Cabecera"),
            (RackSystemKind.Cama, "Cama"),
            (RackSystemKind.Larguero, "Larguero"),
        };

        private static readonly RackProjectStore Store = new RackProjectStore();

        private readonly Func<string> _folder;
        private readonly Func<bool> _folderOverridden;
        private readonly Func<RackSystemKind, bool> _opensHere;
        private readonly Action<RackCadLibraryRow> _open;
        private readonly Func<object, bool> _isAlive;
        private readonly Action<string, bool> _say;

        private RackDesignLibraryListing _listing;
        private string _readError;
        private string _search = string.Empty;
        private string _kind = AllKinds;
        private string _selectedPath;
        private RackCadLibrarySaveSource _save;
        private string _saveName = string.Empty;
        private bool _saveConflict;

        /// <param name="folder">The folder of the library, as the settings resolve it now.</param>
        /// <param name="folderOverridden">Whether that folder was chosen by the user (the banner of a folder that does not exist says how to go back).</param>
        /// <param name="opensHere">Whether a design of that system opens as a new rack in the window (Selectivo and Dinámico); the others, in their classic editor.</param>
        /// <param name="open">What «Abrir» does with the chosen design (the window).</param>
        /// <param name="isAlive">Whether the editor of a pending «Guardar en biblioteca…» is still open in the window.</param>
        /// <param name="say">The message line of the window.</param>
        public RackCadLibraryViewModel(Func<string> folder, Func<bool> folderOverridden, Func<RackSystemKind, bool> opensHere,
            Action<RackCadLibraryRow> open, Func<object, bool> isAlive, Action<string, bool> say)
        {
            _folder = folder ?? (() => string.Empty);
            _folderOverridden = folderOverridden ?? (() => false);
            _opensHere = opensHere ?? (_ => false);
            _open = open;
            _isAlive = isAlive ?? (_ => true);
            _say = say ?? ((_, _) => { });
        }

        // ------------------------------------------------------------------ the folder and its designs

        /// <summary>Whether the page read the folder at least once (the recent designs of «Nuevo» read it the first time).</summary>
        public bool HasRead => _listing != null || _readError != null;

        public string Folder => SafeFolder();

        /// <summary>Every design read, from the most recent.</summary>
        public IReadOnlyList<RackDesignLibraryEntry> Entries => _listing?.Entries ?? Array.Empty<RackDesignLibraryEntry>();

        public string ReadError => _readError ?? string.Empty;

        public bool HasReadError => !string.IsNullOrEmpty(_readError);

        /// <summary>UT-08: a folder that does not exist is not an empty folder.</summary>
        public bool FolderMissing => _listing != null && !_listing.FolderExists;

        public string FolderMissingText => _folderOverridden()
            ? "La carpeta todavía no existe: se crea al guardar el primer diseño. Para volver a la de siempre, «Restablecer» en Configuración."
            : "La carpeta todavía no existe: se crea al guardar el primer diseño.";

        public bool FolderEmpty => _listing != null && _listing.FolderExists && _listing.Entries.Count == 0 && _listing.Unreadable.Count == 0;

        public bool HasAny => Entries.Count > 0;

        /// <summary>UT-06: the files of the folder that could not be read, by name, instead of skipping them in silence.</summary>
        public IReadOnlyList<string> Unreadable => _listing?.Unreadable ?? Array.Empty<string>();

        public bool HasUnreadable => Unreadable.Count > 0;

        public string UnreadableTitle => Unreadable.Count == 1
            ? "1 archivo de la carpeta no se pudo leer"
            : Unreadable.Count + " archivos de la carpeta no se pudieron leer";

        public string UnreadableText => Unreadable.Count == 0
            ? string.Empty
            : string.Join(", ", Unreadable) + (Unreadable.Count == 1 ? ": no aparece en la lista." : ": no aparecen en la lista.")
              + " Puede estar dañado o ser de otro programa.";

        /// <summary>Reads the folder again (when the page is shown, with «Volver a leer la carpeta» and after a save). Contained.</summary>
        public void Read()
        {
            var folder = SafeFolder();
            try
            {
                _listing = RackDesignLibrary.Read(folder);
                _readError = null;
            }
            catch (Exception ex)
            {
                _listing = null;
                _readError = "No se pudo leer la carpeta: " + ex.Message;
            }

            if (_selectedPath != null && !Entries.Any(e => SamePath(e.Path, _selectedPath))) _selectedPath = null;
            if (_selectedPath == null) _selectedPath = Entries.FirstOrDefault()?.Path;
            RaiseAll();
        }

        /// <summary>«Volver a leer la carpeta»: reads it and says what it found.</summary>
        public void Reread()
        {
            Read();
            if (HasReadError)
            {
                _say(_readError, true);
                return;
            }

            var count = Entries.Count;
            _say("Carpeta leída de nuevo: " + count + (count == 1 ? " diseño" : " diseños")
                 + (HasUnreadable ? ", " + Unreadable.Count + " sin leer." : "."), false);
        }

        // ------------------------------------------------------------------ the search, the filter and the rows

        /// <summary>The search by name or file, with no case and no accents (as the list of racks).</summary>
        public string SearchText
        {
            get => _search;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_search, next, StringComparison.Ordinal)) return;
                _search = next;
                RaiseRows();
            }
        }

        /// <summary>«Sistema»: <see cref="AllKinds"/> or the name of a <see cref="RackSystemKind"/>.</summary>
        public string SelectedKind
        {
            get => _kind;
            set
            {
                var next = string.IsNullOrWhiteSpace(value) ? AllKinds : value;
                if (string.Equals(_kind, next, StringComparison.Ordinal)) return;
                _kind = next;
                OnPropertyChanged(nameof(SelectedKind));
                RaiseRows();
            }
        }

        public IReadOnlyList<RackCadLibraryKindOption> KindOptions
        {
            get
            {
                var options = new List<RackCadLibraryKindOption> { new RackCadLibraryKindOption(AllKinds, "Todos los sistemas (" + Entries.Count + ")") };
                foreach (var (kind, label) in KindOrder)
                    options.Add(new RackCadLibraryKindOption(kind.ToString(), label + " (" + Entries.Count(e => e.Kind == kind) + ")"));
                return options;
            }
        }

        public IReadOnlyList<RackCadLibraryRow> Rows => Filtered().Select(RowOf).ToList();

        public string ShownText => "Mostrando " + Filtered().Count() + " de " + Entries.Count;

        public bool NoMatch => HasAny && !Filtered().Any();

        /// <summary>The three most recent designs, for «Recientes de la biblioteca» of «Nuevo».</summary>
        public IReadOnlyList<RackCadLibraryRow> Recent => Entries.Take(3).Select(RowOf).ToList();

        public bool HasRecent => Entries.Count > 0;

        /// <summary>The name of the system of a design in the window (UT-07): the one of the column, not «Sistema dinámico» nor «Cama de rodamiento».</summary>
        public static string LabelOf(RackSystemKind kind)
        {
            foreach (var (k, label) in KindOrder) if (k == kind) return label;
            return kind.ToString();
        }

        // ------------------------------------------------------------------ the chosen design (the inspector)

        public void Select(RackCadLibraryRow row) => SelectPath(row?.Path);

        /// <summary>Chooses a design by its file (also from «Ver» of the recent designs of «Nuevo», which first clears the search and the filter).</summary>
        public void SelectPath(string path)
        {
            if (path == null || !Entries.Any(e => SamePath(e.Path, path))) return;
            _selectedPath = Entries.First(e => SamePath(e.Path, path)).Path;
            RaiseRows();
        }

        /// <summary>Clears the search and the filter, so every design shows (the recent designs of «Nuevo» and a save do it).</summary>
        public void ClearFilters()
        {
            _search = string.Empty;
            _kind = AllKinds;
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedKind));
            RaiseRows();
        }

        /// <summary>The chosen design, when it is one of the rows shown (the search and the filter apply to the inspector too).</summary>
        public RackCadLibraryRow Selected => Filtered().Where(e => SamePath(e.Path, _selectedPath)).Select(RowOf).FirstOrDefault();

        public bool HasSelection => Selected != null;

        public bool NoSelection => HasAny && !NoMatch && Selected == null;

        /// <summary>A design of Selectivo or Dinámico opens here as a new rack; the others, in their classic editor (UT-03).</summary>
        public bool SelectionOpensHere => Selected != null && _opensHere(Selected.Kind);

        public bool SelectionOpensClassic => Selected != null && !_opensHere(Selected.Kind);

        public string SelectionNote
        {
            get
            {
                var row = Selected;
                if (row == null) return string.Empty;
                if (_opensHere(row.Kind))
                    return "Se abre en «Nuevo» con el editor de " + row.KindLabel + " de la ventana, sin bloquear AutoCAD, como un rack nuevo con el nombre "
                           + "del diseño. No toca el dibujo hasta «Insertar en el dibujo…».";
                var note = "El editor de " + row.KindLabel + " aún es el clásico: se abre aparte, ya con este diseño, y bloquea AutoCAD hasta que lo "
                           + "cierres, como hoy. Cuando " + row.KindLabel + " llegue a la ventana, se abrirá aquí.";
                return row.Kind == RackSystemKind.Larguero ? note + " Un larguero no se inserta en el dibujo: su editor lo muestra y lo guarda." : note;
            }
        }

        public string SelectionNameNote
        {
            get
            {
                var row = Selected;
                if (row == null) return string.Empty;
                return NameIsTheFile(row.Kind)
                    ? "El nombre que se ve es el del archivo: así lo lee hoy la biblioteca para este sistema."
                    : "El nombre que se ve es el que lleva el diseño.";
            }
        }

        /// <summary>«Abrir como nuevo en esta ventana» / «Abrir en el editor clásico…»: the window opens the chosen design.</summary>
        public void OpenSelected()
        {
            var row = Selected;
            if (row == null)
            {
                _say("Elige un diseño de la lista.", true);
                return;
            }

            _open?.Invoke(row);
        }

        // ------------------------------------------------------------------ «Guardar en biblioteca…» (UT-04)

        public bool HasSave => _save != null;

        public string SaveTitle => _save == null ? string.Empty : "Guardar «" + (_save.RackName.Length == 0 ? "el rack nuevo" : _save.RackName) + "» en la biblioteca";

        /// <summary>The name of the design; it is the name of its file too, with the characters Windows does not admit as «_».</summary>
        public string SaveName
        {
            get => _saveName;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_saveName, next, StringComparison.Ordinal)) return;
                _saveName = next;
                _saveConflict = false;
                RaiseSave();
            }
        }

        public bool SaveNameMissing => _save != null && RackCadLibraryFileName.BaseOf(_saveName) == null;

        public string SaveNameError => SaveNameMissing ? "Escribe un nombre: sin nombre no se guarda." : string.Empty;

        public string SaveFile => RackCadLibraryFileName.FileOf(_saveName) ?? string.Empty;

        public string SaveHint
        {
            get
            {
                if (_save == null || SaveNameMissing) return string.Empty;
                var cleaned = RackCadLibraryFileName.IsCleaned(_saveName) ? " (los caracteres que Windows no admite pasan a «_»)" : string.Empty;
                return "Archivo: " + SaveFile + cleaned + ". Se guarda lo que ves en el editor, borrador incluido" + (_save.ChangesNote() ?? string.Empty)
                       + ". No toca el dibujo.";
            }
        }

        public bool CanSave => _save != null && !SaveNameMissing && !_saveConflict;

        /// <summary>A design with the same file is already in the folder: «Reemplazar» or «Cambiar el nombre».</summary>
        public bool HasSaveConflict => _save != null && _saveConflict;

        public string SaveConflictTitle => HasSaveConflict ? "Ya hay un diseño «" + SaveFile + "» en la carpeta" : string.Empty;

        public string SaveConflictText
        {
            get
            {
                if (!HasSaveConflict) return string.Empty;
                var existing = Entries.FirstOrDefault(e => SamePath(e.Path, SavePath()));
                var what = existing == null
                    ? "Es un archivo que la biblioteca no pudo leer."
                    : "Es un diseño de " + LabelOf(existing.Kind) + " guardado el " + Modified(existing) + ".";
                return what + " Si lo reemplazas, ese archivo se pierde.";
            }
        }

        /// <summary>The line of «Guardar en biblioteca…» for an editor of the window, with the name of its rack. Nothing is written yet.</summary>
        public void BeginSave(RackCadLibrarySaveSource source)
        {
            if (source == null) return;
            _save = source;
            _saveName = source.RackName;
            _saveConflict = false;
            _search = string.Empty;
            _kind = AllKinds;
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedKind));
            RaiseSave();
            RaiseRows();
        }

        /// <summary>«Guardar»: writes the design unless a file of that name exists, which it then shows with «Reemplazar» and «Cambiar el nombre».</summary>
        public void Save() => Commit(replace: false);

        /// <summary>«Reemplazar»: writes over the design of the same file.</summary>
        public void Replace() => Commit(replace: true);

        /// <summary>«Cambiar el nombre»: the first «name (n)» whose file is not in the folder.</summary>
        public void RenameSave()
        {
            if (_save == null) return;
            SaveName = RackCadLibraryFileName.NextFreeName(SafeFolder(), _saveName.Trim().Length == 0 ? _save.RackName : _saveName);
        }

        /// <summary>«Cancelar»: nothing is saved.</summary>
        public void CancelSave()
        {
            if (_save == null) return;
            _save = null;
            _saveConflict = false;
            RaiseSave();
            _say("No se guardó nada.", false);
        }

        /// <summary>The editor of the pending save was closed or forgotten: the line goes away and the page says it (the window calls it).</summary>
        public void ForgetSaveOf(object editor)
        {
            if (_save == null || !ReferenceEquals(_save.Editor, editor)) return;
            _save = null;
            _saveConflict = false;
            RaiseSave();
        }

        private void Commit(bool replace)
        {
            var source = _save;
            if (source == null) return;
            if (SaveNameMissing)
            {
                RaiseSave();
                return;
            }

            if (!_isAlive(source.Editor))
            {
                _save = null;
                RaiseSave();
                _say("El editor de «" + (source.RackName.Length == 0 ? "el rack nuevo" : source.RackName) + "» se cerró: no se guardó nada.", true);
                return;
            }

            var folder = SafeFolder();
            var path = SavePath();
            if (string.IsNullOrWhiteSpace(folder) || path == null)
            {
                _say("No hay una carpeta de la biblioteca: elígela en Configuración.", true);
                return;
            }

            if (!replace && File.Exists(path))
            {
                // The folder is read again first, so the line says what that file is now.
                Read();
                _saveConflict = true;
                RaiseSave();
                return;
            }

            var (project, error) = source.Build(_saveName.Trim());
            if (project == null)
            {
                _say("No se guardó: " + (string.IsNullOrWhiteSpace(error) ? "no hay un borrador válido que guardar." : error), true);
                return;
            }

            try
            {
                Directory.CreateDirectory(folder);
                Store.Save(project, path);
            }
            catch (Exception ex)
            {
                _say("No se pudo guardar en la biblioteca: " + ex.Message, true);
                return;
            }

            var file = Path.GetFileName(path);
            var replaced = replace;
            _save = null;
            _saveConflict = false;
            _search = string.Empty;
            _kind = AllKinds;
            Read();
            _selectedPath = Entries.FirstOrDefault(e => SamePath(e.Path, path))?.Path ?? _selectedPath;
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedKind));
            RaiseSave();
            RaiseRows();
            _say((replaced ? "Reemplazado" : "Guardado") + " en la biblioteca: «" + file + "», con lo que ves en el editor (borrador incluido). No toca el dibujo.",
                false);
        }

        // ------------------------------------------------------------------ internals

        private string SavePath()
        {
            var file = RackCadLibraryFileName.FileOf(_saveName);
            var folder = SafeFolder();
            return file == null || string.IsNullOrWhiteSpace(folder) ? null : Path.Combine(folder, file);
        }

        private IEnumerable<RackDesignLibraryEntry> Filtered()
        {
            var query = Fold(_search.Trim());
            foreach (var entry in Entries)
            {
                if (!string.Equals(_kind, AllKinds, StringComparison.Ordinal) && !string.Equals(entry.Kind.ToString(), _kind, StringComparison.Ordinal)) continue;
                if (query.Length > 0 && !Fold(entry.Name).Contains(query, StringComparison.Ordinal) && !Fold(Path.GetFileName(entry.Path)).Contains(query, StringComparison.Ordinal))
                    continue;
                yield return entry;
            }
        }

        private RackCadLibraryRow RowOf(RackDesignLibraryEntry entry) => new RackCadLibraryRow(entry.Path, entry.Name, entry.Kind, LabelOf(entry.Kind),
            Modified(entry), Path.GetFileName(entry.Path), SamePath(entry.Path, _selectedPath));

        private static string Modified(RackDesignLibraryEntry entry) => entry.Modified.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        // Dinámico, Cama, Push Back and Cantilever are listed by the name of their file (RackDesignLibrary: their payload name is not read).
        private static bool NameIsTheFile(RackSystemKind kind) =>
            kind == RackSystemKind.PalletFlow || kind == RackSystemKind.Cama || kind == RackSystemKind.PushBack || kind == RackSystemKind.Cantilever;

        private static bool SamePath(string a, string b) => a != null && b != null && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private string SafeFolder()
        {
            try
            {
                return _folder() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        // Lower case and no accents: «dinamico» finds «Dinámico».
        private static string Fold(string text)
        {
            var decomposed = (text ?? string.Empty).ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var c in decomposed)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) builder.Append(c);
            return builder.ToString();
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(HasRead));
            OnPropertyChanged(nameof(Folder));
            OnPropertyChanged(nameof(Entries));
            OnPropertyChanged(nameof(ReadError));
            OnPropertyChanged(nameof(HasReadError));
            OnPropertyChanged(nameof(FolderMissing));
            OnPropertyChanged(nameof(FolderMissingText));
            OnPropertyChanged(nameof(FolderEmpty));
            OnPropertyChanged(nameof(HasAny));
            OnPropertyChanged(nameof(Unreadable));
            OnPropertyChanged(nameof(HasUnreadable));
            OnPropertyChanged(nameof(UnreadableTitle));
            OnPropertyChanged(nameof(UnreadableText));
            OnPropertyChanged(nameof(KindOptions));
            OnPropertyChanged(nameof(Recent));
            OnPropertyChanged(nameof(HasRecent));
            RaiseRows();
            RaiseSave();
        }

        private void RaiseRows()
        {
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(ShownText));
            OnPropertyChanged(nameof(NoMatch));
            OnPropertyChanged(nameof(Selected));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(NoSelection));
            OnPropertyChanged(nameof(SelectionOpensHere));
            OnPropertyChanged(nameof(SelectionOpensClassic));
            OnPropertyChanged(nameof(SelectionNote));
            OnPropertyChanged(nameof(SelectionNameNote));
        }

        private void RaiseSave()
        {
            OnPropertyChanged(nameof(HasSave));
            OnPropertyChanged(nameof(SaveTitle));
            OnPropertyChanged(nameof(SaveName));
            OnPropertyChanged(nameof(SaveNameMissing));
            OnPropertyChanged(nameof(SaveNameError));
            OnPropertyChanged(nameof(SaveFile));
            OnPropertyChanged(nameof(SaveHint));
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(HasSaveConflict));
            OnPropertyChanged(nameof(SaveConflictTitle));
            OnPropertyChanged(nameof(SaveConflictText));
        }
    }
}
