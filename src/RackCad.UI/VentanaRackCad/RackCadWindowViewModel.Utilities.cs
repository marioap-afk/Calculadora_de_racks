using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One element of the classic menu and where it is in the window («Dónde quedó el menú clásico», prototype «utilidades»).</summary>
    public sealed record RackCadMenuPlace(string Menu, string Where);

    /// <summary>
    /// The utilities of the RackCad window (I-64 A-14, line D, fourth batch, part «ut»; prototype «utilidades», UT-01 to UT-12, all as the
    /// Owner accepted them): the page «Biblioteca de diseños» (<see cref="Library"/>) with «Abrir» (a Selectivo or a Dinámico as a new rack
    /// in this window, the others in their classic editor through the door RACKBIBLIOTECAABRIR) and «Guardar en biblioteca…» of the editors
    /// of the window; the page «Órdenes y atajos» (<see cref="Help"/>), behind the button «Ayuda» of the header, with no sixth section;
    /// «Configuración» with the folder of the designs, «Abrir RackCad al iniciar AutoCAD» and «Acerca de RackCad»; and, in «Nuevo», the
    /// recent designs and where the classic menu went. Nothing here writes the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private RackCadLibraryViewModel _library;
        private RackCadHelpViewModel _help;

        // The project a draft of the window was opened from (a design of the library), so «Guardar en biblioteca…» of that draft keeps the
        // fields of the file this build does not know (I-11), as the classic editors do with their sourceProject.
        private readonly Dictionary<object, RackProject> _librarySources = new(ReferenceEqualityComparer.Instance);

        private long _lastLibraryDesignRequest;

        private IRackCadUtilitySettings UtilitySettings => _settings as IRackCadUtilitySettings;

        private IRackCadUtilityCommandPort UtilityDoor => _classic as IRackCadUtilityCommandPort;

        // ------------------------------------------------------------------ the pages

        /// <summary>«Órdenes y atajos», opened by «Ayuda» of the header (UT-09): a page of the window that is not a section of the column.</summary>
        public bool IsAyudaPage => _page == RackCadWindowPage.Ayuda;

        /// <summary>«Ayuda» of the header.</summary>
        public void ShowHelp() => ShowPage(RackCadWindowPage.Ayuda);

        /// <summary>The page «Biblioteca de diseños».</summary>
        public RackCadLibraryViewModel Library => _library ??= new RackCadLibraryViewModel(
            () => DesignLibraryPath,
            () => SafeRead(() => UtilitySettings?.DesignLibraryOverridden ?? false, false),
            OpensHere,
            OpenLibraryDesign,
            IsEditorAlive,
            (text, failed) => Say(text, failed));

        /// <summary>The page «Órdenes y atajos».</summary>
        public RackCadHelpViewModel Help => _help ??= new RackCadHelpViewModel(RunHelpCommand, ShowPage);

        // ShowPage tells the utilities which page is shown: the library reads its folder when its page is shown, and the first time «Nuevo»
        // is shown (its recent designs); «Configuración» reads the settings again.
        private void OnUtilityPageShown(RackCadWindowPage page)
        {
            OnPropertyChanged(nameof(IsAyudaPage));
            if (page == RackCadWindowPage.Biblioteca || (page == RackCadWindowPage.Nuevo && !Library.HasRead)) Library.Read();
            if (page == RackCadWindowPage.Configuracion) RaiseUtilitySettings();
        }

        // ------------------------------------------------------------------ «Nuevo»: the recent designs and the classic menu

        /// <summary>«Ver» of a recent design of «Nuevo»: the library, with that design chosen.</summary>
        public void ShowRecentDesign(RackCadLibraryRow row)
        {
            Contained(() =>
            {
                ShowPage(RackCadWindowPage.Biblioteca);
                Library.ClearFilters();
                if (row != null) Library.SelectPath(row.Path);
            });
        }

        /// <summary>«Dónde quedó el menú clásico (RACKMENU)», element by element (the prototype, with the decisions UT-08 and Q-07).</summary>
        public IReadOnlyList<RackCadMenuPlace> ClassicMenuMap { get; } = new[]
        {
            new RackCadMenuPlace("Diseñar sistema selectivo", "«Selectivo en esta ventana» y la tarjeta «Selectivo» (editor clásico)"),
            new RackCadMenuPlace("Diseñar sistema dinámico (Pallet Flow)", "«Dinámico en esta ventana» y la tarjeta «Dinámico»"),
            new RackCadMenuPlace("Diseñar sistema Push Back", "Tarjeta «Push Back»"),
            new RackCadMenuPlace("Diseñar línea Cantilever", "Tarjeta «Cantilever»"),
            new RackCadMenuPlace("Diseñar cabecera", "Tarjeta «Cabecera»"),
            new RackCadMenuPlace("Diseñar cama de rodamiento", "Tarjeta «Cama»"),
            new RackCadMenuPlace("Diseñar larguero", "Tarjeta «Larguero»"),
            new RackCadMenuPlace("Generar perfil estructural", "Tarjeta «Perfil estructural»"),
            new RackCadMenuPlace("Abrir de la biblioteca de diseños", "Sección «Biblioteca de diseños», sin ventana aparte"),
            new RackCadMenuPlace("Biblioteca de bloques (.dwg): ruta, «Examinar…», «Restablecer»", "Sección «Configuración»"),
            new RackCadMenuPlace("Diseñar drive-in («Próximamente»)", "No aparece hasta que exista"),
        };

        // ------------------------------------------------------------------ «Abrir» of the library (UT-03)

        // A design of Selectivo or Dinámico opens here when the window can make a new rack of that system; the others (and those two
        // without their door) open in their classic editor.
        private bool OpensHere(RackSystemKind kind) =>
            (kind == RackSystemKind.SelectiveRack && CanStartNewSelective) || (kind == RackSystemKind.PalletFlow && CanStartNewDynamic);

        private void OpenLibraryDesign(RackCadLibraryRow row)
        {
            Contained(() =>
            {
                if (row == null) return;
                if (!OpensHere(row.Kind))
                {
                    OpenLibraryDesignInClassicEditor(row);
                    return;
                }

                RackProject project;
                try
                {
                    project = new RackProjectStore().Load(row.Path);
                }
                catch (Exception ex)
                {
                    Say("No se pudo abrir «" + row.Name + "»: " + ex.Message, failed: true);
                    return;
                }

                OpenDesignAsNewRack(project, row.Name);
            });
        }

        /// <summary>
        /// THE one method that opens a design of the library as a new rack of this window (UT-03): a Selectivo or a Dinámico, in «Nuevo»,
        /// with the name of the design and an identity of its own, not in the drawing until «Insertar en el dibujo…». It rides on the
        /// mechanism of today: ONE new rack of the drawing at a time in «Nuevo» (P3-04). <b>At integration the session moves this call to
        /// the entry point of the drafts of part «new»</b> («abrir un diseño como borrador nuevo»); nothing else of the library changes.
        /// False, with the reason said, when it did not open.
        /// </summary>
        internal bool OpenDesignAsNewRack(RackProject project, string designName)
        {
            if (project == null) return false;
            if (!Panel.PresentedSession.HasValue)
            {
                Say("No hay un dibujo en la ventana.", failed: true);
                return false;
            }

            if (NewSelectiveEditor != null || NewDynamicEditor != null)
            {
                Say(OneNewRackLibraryText, failed: true);
                return false;
            }

            var session = Panel.PresentedSession.Value;
            var rackId = Guid.NewGuid().ToString();
            var name = (designName ?? string.Empty).Trim();
            if (project.Kind == RackSystemKind.SelectiveRack && project.SelectiveRack != null && _selectiveDoor != null)
            {
                var draft = new SelectiveDraftEditor(_catalog());
                if (!draft.LoadFrom(project.SelectiveRack.ToDomain(), null, string.IsNullOrWhiteSpace(project.SelectiveRack.Name) ? name : project.SelectiveRack.Name))
                {
                    Say("No se pudo abrir «" + name + "» aquí: " + (draft.LastError ?? "sin detalle.") + " Ábrelo con RACKBIBLIOTECA en el editor clásico.", failed: true);
                    return false;
                }

                var editor = SelectiveEditorViewModel.ForNewRack(draft, rackId, UpdateSelective, DimensionStylesOf(session));
                WireSelective(editor);
                _selectiveDrafts[(session, rackId)] = new SelectiveDraftEntry(session, editor);
                _librarySources[editor] = project;
                editor.Say(OpenedFromLibraryText(name, "Selectivo"), error: false);
            }
            else if (project.Kind == RackSystemKind.PalletFlow && project.DynamicDesign != null && _door != null)
            {
                var draft = new DynamicDraftEditor(_catalog());
                draft.LoadFrom(project.DynamicDesign, name);
                var editor = new DynamicEditorViewModel(draft, rackId, UpdateDynamic, isNewRack: true);
                Wire(editor);
                _dynamicDrafts[(session, rackId)] = new DynamicDraftEntry(session, editor);
                _librarySources[editor] = project;
                editor.Say(OpenedFromLibraryText(name, "Dinámico"), error: false);
            }
            else
            {
                Say("«" + name + "» no se puede abrir en esta ventana.", failed: true);
                return false;
            }

            // The editor says what it is (OpenedFromLibraryText); the message of the window is cleared, as «Selectivo en esta ventana» does.
            ShowPage(RackCadWindowPage.Nuevo);
            Say(string.Empty, failed: false);
            RaiseDynamic();
            return true;
        }

        internal const string OneNewRackLibraryText =
            "En «Nuevo» ya hay un rack nuevo sin insertar: insértalo en el dibujo o cierra su editor antes de abrir un diseño de la biblioteca.";

        internal static string OpenedFromLibraryText(string name, string system) =>
            "«" + name + "» se abrió como " + system + " nuevo en esta ventana, con una identidad nueva. Todavía no está en el dibujo.";

        // A design of a system that is not edited in the window yet: its classic editor, already loaded, through the numbered door.
        private void OpenLibraryDesignInClassicEditor(RackCadLibraryRow row)
        {
            var door = UtilityDoor;
            if (door == null)
            {
                Say("Aquí no se puede abrir el editor clásico de " + row.KindLabel + ".", failed: true);
                return;
            }

            if (!Panel.PresentedSession.HasValue)
            {
                Say("Abre o crea un dibujo para usar esta acción.", failed: true);
                return;
            }

            var request = new RackCadLibraryDesignRequest(++_lastLibraryDesignRequest, Panel.PresentedSession.Value, row.Path);
            RackCadClassicResult result;
            try
            {
                result = door.OpenLibraryDesign(request) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted)
                Say("Se pidió a AutoCAD abrir «" + row.Name + "» en el editor clásico de " + row.KindLabel + ", ya cargado. Se abre aparte y bloquea "
                    + "AutoCAD hasta que lo cierres, como hoy. Al insertar el rack, pulsa «Volver a leer la lista» para verlo en ella.", failed: false);
            else Say(RefusalText(result), failed: true);
        }

        // ------------------------------------------------------------------ «Guardar en biblioteca…» of the editors (UT-04)

        // «Más» › «Guardar en biblioteca…» of a Selectivo editor: the page of the library with the line of the save.
        private void StartSaveToLibrary(SelectiveEditorViewModel editor)
        {
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Selectivo",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildSelectiveProject(editor, name))));
        }

        // The same for a Dinámico editor.
        private void StartSaveToLibrary(DynamicEditorViewModel editor)
        {
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Dinámico",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildDynamicProject(editor, name))));
        }

        private void BeginLibrarySave(RackCadLibrarySaveSource source)
        {
            var wasShown = _page == RackCadWindowPage.Biblioteca;
            Library.BeginSave(source);
            ShowPage(RackCadWindowPage.Biblioteca);
            if (wasShown) Library.Read();
            Say(string.Empty, failed: false);
        }

        private static string ChangesNote(bool isNewRack, bool isDirty) =>
            isNewRack ? " (este rack nuevo todavía no está en el dibujo)" : isDirty ? " (este rack tiene cambios sin escribir)" : string.Empty;

        // What is saved for a Selectivo: what the editor shows (the last valid draft, «borrador incluido»), materialized as the classic save
        // does (SelectiveLibraryExport.FromEffective: the library is literal only), with the name typed as the name of the design.
        private (RackProject Project, string Error) BuildSelectiveProject(SelectiveEditorViewModel editor, string name)
        {
            var snapshot = editor.Draft.SnapshotForWrite();
            if (snapshot == null) return (null, "no hay un borrador válido que guardar.");
            var document = SelectiveLibraryExport.FromEffective(snapshot.Design, editor.RackId, name);
            _librarySources.TryGetValue(editor, out var source);
            return (RackProject.ForSelectiveRack(document).WithSourceMetadataFrom(source), null);
        }

        // What is saved for a Dinámico: the design and the system of the draft, as «Guardar proyecto» of the classic editor saves them.
        private (RackProject Project, string Error) BuildDynamicProject(DynamicEditorViewModel editor, string name)
        {
            var snapshot = editor.Draft.SnapshotForWrite();
            if (snapshot == null) return (null, "no hay un borrador válido que guardar.");
            snapshot.System.Name = name;
            _librarySources.TryGetValue(editor, out var source);
            return (RackProject.ForDynamic(snapshot.Design, snapshot.System).WithSourceMetadataFrom(source), null);
        }

        private bool IsEditorAlive(object editor) =>
            _selectiveDrafts.Values.Any(e => ReferenceEquals(e.Editor, editor)) || _dynamicDrafts.Values.Any(e => ReferenceEquals(e.Editor, editor));

        // ------------------------------------------------------------------ «Ejecutar» of the help (UT-10)

        private void RunHelpCommand(string command)
        {
            var door = UtilityDoor;
            if (door == null)
            {
                Say("Esta acción no está disponible aquí.", failed: true);
                return;
            }

            RackCadClassicResult result;
            try
            {
                result = door.RunHelpCommand(command) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted)
                Say("Se pidió a AutoCAD ejecutar " + command + ". El foco vuelve al dibujo; lo que la orden pregunte sale en la línea de órdenes.", failed: false);
            else Say(RefusalText(result), failed: true);
        }

        // ------------------------------------------------------------------ «Configuración» (UT-01, UT-02, UT-12)

        /// <summary>The controls of the utilities are offered only with a settings access that has them.</summary>
        public bool HasUtilitySettings => UtilitySettings != null;

        public string SettingsIntro
        {
            get
            {
                var path = SafeRead(() => UtilitySettings?.SettingsFilePath, null);
                return "Ajustes de este usuario en este equipo" + (string.IsNullOrWhiteSpace(path) ? string.Empty : " (" + path + ")")
                       + ". No van en el dibujo y se guardan al cambiarlos.";
            }
        }

        /// <summary>«Restablecer» of the block library is offered only for a path chosen by the user (the prototype).</summary>
        public bool CanResetBlockLibrary => SafeRead(() => _settings.BlockLibraryOverridden, false);

        public bool DesignLibraryOverridden => SafeRead(() => UtilitySettings?.DesignLibraryOverridden ?? false, false);

        public bool DesignLibraryExists
        {
            get
            {
                var path = DesignLibraryPath;
                return !string.IsNullOrWhiteSpace(path) && SafeRead(() => Directory.Exists(path), false);
            }
        }

        public string DesignLibraryStatus =>
            (DesignLibraryOverridden ? "Carpeta elegida (guardada)." : "Carpeta predeterminada.")
            + (DesignLibraryExists ? string.Empty : " Todavía no existe: se crea al guardar el primer diseño.");

        public bool CanChangeDesignLibrary => HasUtilitySettings;

        public bool CanResetDesignLibrary => HasUtilitySettings && DesignLibraryOverridden;

        /// <summary>«Cambiar…» of the folder of the designs (UT-02): the folder the user chose in the dialog of Windows.</summary>
        public void SetDesignLibraryPath(string path)
        {
            Contained(() =>
            {
                var settings = UtilitySettings;
                if (settings == null || string.IsNullOrWhiteSpace(path)) return;
                settings.SetDesignLibraryPath(path);
                Library.Read();
                RaiseSettings();
                Say("Carpeta de diseños guardada: mira «Biblioteca de diseños».", failed: false);
            });
        }

        /// <summary>«Restablecer» of the folder of the designs: the default one, %AppData%\RackCad\Designs.</summary>
        public void ResetDesignLibraryPath()
        {
            Contained(() =>
            {
                var settings = UtilitySettings;
                if (settings == null) return;
                settings.SetDesignLibraryPath(null);
                Library.Read();
                RaiseSettings();
                Say("Carpeta de diseños restablecida a la predeterminada.", failed: false);
            });
        }

        /// <summary>«Abrir RackCad al iniciar AutoCAD» (UT-01): off by default, it applies from the next start.</summary>
        public bool OpenOnStartup
        {
            get => SafeRead(() => UtilitySettings?.OpenOnStartup ?? false, false);
            set
            {
                Contained(() =>
                {
                    var settings = UtilitySettings;
                    if (settings == null || settings.OpenOnStartup == value) return;
                    settings.SetOpenOnStartup(value);
                });
                OnPropertyChanged(nameof(OpenOnStartup));
                OnPropertyChanged(nameof(OpenOnStartupHint));
            }
        }

        public string OpenOnStartupHint => OpenOnStartup
            ? "Encendida: al iniciar AutoCAD, RackCad abre la ventana con el primer dibujo. Se aplica desde el próximo arranque."
            : "Apagada (por defecto): la ventana se abre con RACKCAD o RK.";

        public string AboutVersion => SafeRead(() => UtilitySettings?.About?.Version, null) ?? string.Empty;

        public string AboutCatalogsFolder => SafeRead(() => UtilitySettings?.About?.CatalogsFolder, null) ?? string.Empty;

        public string AboutLogsFolder => SafeRead(() => UtilitySettings?.About?.LogsFolder, null) ?? string.Empty;

        private void RaiseUtilitySettings()
        {
            OnPropertyChanged(nameof(HasUtilitySettings));
            OnPropertyChanged(nameof(SettingsIntro));
            OnPropertyChanged(nameof(CanResetBlockLibrary));
            OnPropertyChanged(nameof(DesignLibraryOverridden));
            OnPropertyChanged(nameof(DesignLibraryExists));
            OnPropertyChanged(nameof(DesignLibraryStatus));
            OnPropertyChanged(nameof(CanChangeDesignLibrary));
            OnPropertyChanged(nameof(CanResetDesignLibrary));
            OnPropertyChanged(nameof(OpenOnStartup));
            OnPropertyChanged(nameof(OpenOnStartupHint));
            OnPropertyChanged(nameof(AboutVersion));
            OnPropertyChanged(nameof(AboutCatalogsFolder));
            OnPropertyChanged(nameof(AboutLogsFolder));
        }

        // An editor that the window forgets takes its pending save with it, and the project it was opened from.
        private void ForgetUtilitiesOf(object editor)
        {
            _librarySources.Remove(editor);
            _library?.ForgetSaveOf(editor);
        }
    }
}
