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
            () => (_config ?? RefreshUtilitySettings()).DesignFolder, // only a reading of the library asks it, never a binding
            () => Config.DesignOverridden,
            OpensHere,
            OpenLibraryDesign,
            IsEditorAlive,
            (text, failed) => Say(text, failed));

        /// <summary>The page «Órdenes y atajos».</summary>
        public RackCadHelpViewModel Help => _help ??= new RackCadHelpViewModel(RunHelpCommand, ShowPage);

        // ShowPage tells the utilities which page is shown. Only «Biblioteca» and «Configuración» read the disk, and only when they are shown
        // (review of part «ut», point 4): «Biblioteca» reads the settings again and then its folder; «Configuración» reads the settings again
        // through RaiseSettings, which ShowPage calls for it. «Nuevo» shows the recent designs of the last reading and reads nothing.
        private void OnUtilityPageShown(RackCadWindowPage page)
        {
            OnPropertyChanged(nameof(IsAyudaPage));
            if (page != RackCadWindowPage.Biblioteca) return;
            RefreshUtilitySettings();
            Library.Read();
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
            new RackCadMenuPlace("Diseñar cama de rodamiento", "«Cama en esta ventana» y la tarjeta «Cama» (editor clásico)"), // T5 cm
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
            (kind == RackSystemKind.SelectiveRack && CanStartNewSelective) || (kind == RackSystemKind.PalletFlow && CanStartNewDynamic)
            || (kind == RackSystemKind.Cama && CanStartNewCama) // T5 cm: a cama opens here as a draft too
            || (kind == RackSystemKind.Cantilever && CanStartNewCantilever); // T5 ct

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
        /// with the name of the design and an identity of its own, not in the drawing until «Insertar en el dibujo…». It opens it through
        /// the entry point of the drafts of «Nuevo» (<see cref="OpenAsNewDraft"/>, T4 new): one more «Borrador N» of the drawing.
        /// False, with the reason said by the entry point, when it did not open.
        /// </summary>
        // T4 integration (session): the call moved to the entry point of the drafts of part «new» (OpenAsNewDraft), as both parts agreed.
        // The design opens as one more «Borrador N» of the drawing (SD-06), named as the classic editor opened from the library names it
        // (RackCadDraftSeed.FromLibrary); the draft keeps the project it came from, for the metadata of «Guardar en biblioteca».
        internal bool OpenDesignAsNewRack(RackProject project, string designName)
        {
            if (project == null) return false;
            var name = (designName ?? string.Empty).Trim();
            var opening = OpenAsNewDraft(RackCadDraftSeed.FromLibrary(project, name, LibraryOrigin(name)));
            if (opening == null || !opening.Opened) return false;

            object editor = project.Kind == RackSystemKind.SelectiveRack ? NewSelectiveEditor : project.Kind == RackSystemKind.Cama ? NewCamaEditor // T5 cm
                : project.Kind == RackSystemKind.Cantilever ? NewCantileverEditor : NewDynamicEditor; // T5 ct
            if (editor != null) _librarySources[editor] = project;
            return true;
        }

        /// <summary>Where a draft opened from the library comes from, as its editor says it first («Borrador N: … nuevo en esta ventana, …»).</summary>
        internal static string LibraryOrigin(string name) => "del diseño «" + name + "» de la biblioteca";

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
            // The refusal of the editor is asked again at «Guardar» and «Reemplazar» (review of part «ut», point 1).
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Selectivo",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildSelectiveProject(editor, name), editor.SaveRefusal)));
        }

        // The same for a Dinámico editor.
        private void StartSaveToLibrary(DynamicEditorViewModel editor)
        {
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Dinámico",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildDynamicProject(editor, name), editor.SaveRefusal)));
        }

        private void BeginLibrarySave(RackCadLibrarySaveSource source)
        {
            var wasShown = _page == RackCadWindowPage.Biblioteca;
            Library.BeginSave(source);
            ShowPage(RackCadWindowPage.Biblioteca);
            if (wasShown)
            {
                RefreshUtilitySettings();
                Library.Read();
            }

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
            _selectiveDrafts.Values.Any(e => ReferenceEquals(e.Editor, editor)) || _dynamicDrafts.Values.Any(e => ReferenceEquals(e.Editor, editor))
            || IsCamaEditorAlive(editor) // T5 cm
            || IsCantileverEditorAlive(editor); // T5 ct

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

        // What «Configuración» and «Biblioteca» show of the settings and of the disk, read ONCE when one of them is shown, and again after
        // «Cambiar…», «Restablecer» or the box (review of part «ut», point 4). Nothing of it is read when the window loads or refreshes: a
        // folder on a disconnected network share would freeze AutoCAD until the timeout of SMB, also at the start with the box on.
        private sealed record UtilityConfig(string DesignFolder, bool DesignOverridden, bool DesignExists, bool OpenOnStartup, string SettingsPath,
            RackCadAboutInfo About, string BlockPath, string BlockStatus, bool BlockMissing, bool BlockOverridden)
        {
            public static readonly UtilityConfig Empty = new(string.Empty, false, false, false, null, null, string.Empty, string.Empty, false, false);
        }

        private UtilityConfig _config;

        private UtilityConfig Config => _config ?? UtilityConfig.Empty;

        /// <summary>Whether the window read the settings of the utilities (only «Configuración» and «Biblioteca» read them): a seam for tests.</summary>
        internal bool HasReadUtilitySettings => _config != null;

        // Reads the settings and the existence of the folder of the designs once, and tells the view. RaiseSettings calls it (ShowPage of
        // «Configuración», the block library), and so do «Biblioteca» and the changes of the folder.
        private UtilityConfig RefreshUtilitySettings()
        {
            var utility = UtilitySettings;
            var folder = DesignLibraryPath;
            _config = new UtilityConfig(
                folder,
                SafeRead(() => utility?.DesignLibraryOverridden ?? false, false),
                !string.IsNullOrWhiteSpace(folder) && SafeRead(() => Directory.Exists(folder), false),
                SafeRead(() => utility?.OpenOnStartup ?? false, false),
                SafeRead(() => utility?.SettingsFilePath, null),
                SafeRead(() => utility?.About, null),
                BlockLibraryPath,
                BlockLibraryStatus,
                BlockLibraryMissing,
                SafeRead(() => _settings.BlockLibraryOverridden, false));
            RaiseUtilitySettings();
            return _config;
        }

        /// <summary>The controls of the utilities are offered only with a settings access that has them.</summary>
        public bool HasUtilitySettings => UtilitySettings != null;

        public string SettingsIntro =>
            "Ajustes de este usuario en este equipo" + (string.IsNullOrWhiteSpace(Config.SettingsPath) ? string.Empty : " (" + Config.SettingsPath + ")")
            + ". No van en el dibujo y se guardan al cambiarlos.";

        /// <summary>The block library as «Configuración» last read it (the page binds these, not the live reads).</summary>
        public string ConfigBlockLibraryPath => Config.BlockPath;

        public string ConfigBlockLibraryStatus => Config.BlockStatus;

        public bool ConfigBlockLibraryMissing => Config.BlockMissing;

        /// <summary>«Restablecer» of the block library is offered only for a path chosen by the user (the prototype).</summary>
        public bool CanResetBlockLibrary => Config.BlockOverridden;

        /// <summary>The folder of the designs as «Configuración» or «Biblioteca» last read it.</summary>
        public string ConfigDesignLibraryPath => Config.DesignFolder;

        public bool DesignLibraryOverridden => Config.DesignOverridden;

        public bool DesignLibraryExists => Config.DesignExists;

        public string DesignLibraryStatus => _config == null
            ? string.Empty
            : (DesignLibraryOverridden ? "Carpeta elegida (guardada)." : "Carpeta predeterminada.")
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
                RaiseSettings(); // the settings again (RefreshUtilitySettings), then the library reads the new folder
                Library.Read();
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
                RaiseSettings();
                Library.Read();
                Say("Carpeta de diseños restablecida a la predeterminada.", failed: false);
            });
        }

        /// <summary>«Abrir RackCad al iniciar AutoCAD» (UT-01): off by default, it applies from the next start.</summary>
        public bool OpenOnStartup
        {
            get => Config.OpenOnStartup;
            set
            {
                Contained(() =>
                {
                    var settings = UtilitySettings;
                    if (settings == null || settings.OpenOnStartup == value) return;
                    settings.SetOpenOnStartup(value);
                    // Only the box is read back: what the settings kept, without the folder or the disk again.
                    _config = Config with { OpenOnStartup = SafeRead(() => settings.OpenOnStartup, value) };
                });
                OnPropertyChanged(nameof(OpenOnStartup));
                OnPropertyChanged(nameof(OpenOnStartupHint));
            }
        }

        public string OpenOnStartupHint => OpenOnStartup
            ? "Encendida: al iniciar AutoCAD, RackCad abre la ventana con el primer dibujo. Se aplica desde el próximo arranque."
            : "Apagada (por defecto): la ventana se abre con RACKCAD o RK.";

        public string AboutVersion => Config.About?.Version ?? string.Empty;

        public string AboutCatalogsFolder => Config.About?.CatalogsFolder ?? string.Empty;

        public string AboutLogsFolder => Config.About?.LogsFolder ?? string.Empty;

        private void RaiseUtilitySettings()
        {
            OnPropertyChanged(nameof(HasUtilitySettings));
            OnPropertyChanged(nameof(SettingsIntro));
            OnPropertyChanged(nameof(ConfigBlockLibraryPath));
            OnPropertyChanged(nameof(ConfigBlockLibraryStatus));
            OnPropertyChanged(nameof(ConfigBlockLibraryMissing));
            OnPropertyChanged(nameof(CanResetBlockLibrary));
            OnPropertyChanged(nameof(ConfigDesignLibraryPath));
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
