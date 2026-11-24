using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The pages of the center of the RackCad window: the active rack or one of the sections of the side column.</summary>
    public enum RackCadWindowPage
    {
        Rack,
        Nuevo,
        Variables,
        Bom,
        Biblioteca,
        Configuracion,
    }

    /// <summary>One section of the side column as the window shows it: a plain value.</summary>
    public sealed record RackCadWindowSection(RackCadWindowPage Page, string Label, bool IsActive);

    /// <summary>One card of the page "Nuevo": the system, its caption and the classic action that opens its editor.</summary>
    public sealed record RackCadNewSystemCard(RackCadClassicAction Action, string Title, string Caption);

    /// <summary>
    /// The RackCad window (I-64 A-14, phases 1 and 2): ONE window that does not block AutoCAD, with the sections of RackCad and the list of
    /// the racks of the drawing in a side column and the page of the active rack or of the section in the center. It is built over the
    /// workspace panel, whose view-model keeps every rule it had (the browser of racks, the field of the pilot, the navigation, the drafts);
    /// the window only decides what the center shows and adds the doors to the classic flows.
    ///
    /// While the sections are in their intermediate step each one opens its current window through <see cref="IRackCadClassicCommandPort"/>
    /// ("cada sección abre su ventana actual"): the window never writes the drawing by itself. Every entry is contained: a failure is said in
    /// <see cref="ActionMessage"/> and the window keeps working.
    /// </summary>
    public sealed class RackCadWindowViewModel : ObservableObject
    {
        private static readonly IReadOnlyList<(RackCadWindowPage Page, string Label)> SectionOrder = new[]
        {
            (RackCadWindowPage.Nuevo, "Nuevo"),
            (RackCadWindowPage.Variables, "Variables"),
            (RackCadWindowPage.Bom, "BOM y lista de racks"),
            (RackCadWindowPage.Biblioteca, "Biblioteca de diseños"),
            (RackCadWindowPage.Configuracion, "Configuración"),
        };

        private readonly IRackCadClassicCommandPort _classic;
        private readonly IRackCadSettingsAccess _settings;

        private RackCadWindowPage _page = RackCadWindowPage.Rack;
        private string _actionMessage = string.Empty;
        private bool _actionFailed;
        private bool _sideCollapsed;
        private string _lastActiveRack;
        private IReadOnlyList<RackCadWindowSection> _sections;

        /// <param name="panel">
        /// The view-model of the workspace panel: the page of the rack. The list of the drawing reads through the same summary and edit
        /// ports the panel was built with, so the host hands each port over once.
        /// </param>
        /// <param name="classic">The port of the classic flows; optional: without it the doors say that they are not available.</param>
        /// <param name="settings">The settings of RackCad for "Configuración"; optional: the settings of the machine by default.</param>
        public RackCadWindowViewModel(
            WorkspacePanelViewModel panel,
            IRackCadClassicCommandPort classic = null,
            IRackCadSettingsAccess settings = null)
        {
            Panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _classic = classic;
            _settings = settings ?? new RackCadSettingsAccess();
            RackList = new RackListViewModel(panel.BrowserSummaryPort, panel.PilotEditPort, panel.Browser);
            RackList.RackOpened += () => ShowPage(RackCadWindowPage.Rack);
            panel.Browser.PropertyChanged += OnBrowserChanged;
            NewSystems = new[]
            {
                new RackCadNewSystemCard(RackCadClassicAction.NewSelective, "Selectivo", "Racks selectivos y doble profundidad."),
                new RackCadNewSystemCard(RackCadClassicAction.NewPushBack, "Push Back", "Push Back de uno o dos sentidos."),
                new RackCadNewSystemCard(RackCadClassicAction.NewDynamic, "Dinámico", "Racks dinámicos (Pallet Flow)."),
                new RackCadNewSystemCard(RackCadClassicAction.NewCantilever, "Cantilever", "Líneas Cantilever de uno o dos lados."),
                new RackCadNewSystemCard(RackCadClassicAction.NewHeader, "Cabecera", "Configurador de cabeceras (componente)."),
                new RackCadNewSystemCard(RackCadClassicAction.NewFlowBed, "Cama", "Camas de rodamiento: riel, rodillos y frenos."),
                new RackCadNewSystemCard(RackCadClassicAction.NewLarguero, "Larguero", "Larguero (perfil y ménsulas); vista y BOM."),
                new RackCadNewSystemCard(RackCadClassicAction.StructuralSection, "Perfil estructural", "Perfiles W, HSS, C y L del catálogo AISC."),
            };
            RebuildSections();
            Present();
        }

        // ------------------------------------------------------------------ what the window shows

        /// <summary>The workspace panel: the page of the rack and the state of the panel.</summary>
        public WorkspacePanelViewModel Panel { get; }

        /// <summary>The list of the racks of the drawing in the side column.</summary>
        public RackListViewModel RackList { get; }

        public IReadOnlyList<RackCadWindowSection> Sections => _sections;

        public IReadOnlyList<RackCadNewSystemCard> NewSystems { get; }

        public RackCadWindowPage Page => _page;

        public bool IsRackPage => _page == RackCadWindowPage.Rack;

        public bool IsNuevoPage => _page == RackCadWindowPage.Nuevo;

        public bool IsVariablesPage => _page == RackCadWindowPage.Variables;

        public bool IsBomPage => _page == RackCadWindowPage.Bom;

        public bool IsBibliotecaPage => _page == RackCadWindowPage.Biblioteca;

        public bool IsConfiguracionPage => _page == RackCadWindowPage.Configuracion;

        /// <summary>The line of the header: the racks of the drawing and the ones with changes that are not written yet.</summary>
        public string HeaderText
        {
            get
            {
                if (!Panel.PresentedSession.HasValue) return "Sin dibujo abierto";
                var count = RackList.RackCount;
                if (count == 0) return string.IsNullOrEmpty(RackList.StatusText) ? "Sin racks" : string.Empty;
                var racks = count.ToString(CultureInfo.InvariantCulture) + (count == 1 ? " rack" : " racks");
                var changed = RackList.ChangedCount;
                return changed == 0
                    ? racks + " · todo escrito"
                    : racks + " · " + changed.ToString(CultureInfo.InvariantCulture) + " con cambios sin escribir";
            }
        }

        /// <summary>What the last door of the window did (or why it did nothing); empty when there is nothing to say.</summary>
        public string ActionMessage => _actionMessage;

        public bool ActionFailed => _actionFailed;

        public bool HasActionMessage => _actionMessage.Length > 0;

        /// <summary>Whether the side column is hidden, to give the center the whole width.</summary>
        public bool SideCollapsed
        {
            get => _sideCollapsed;
            set
            {
                if (_sideCollapsed == value) return;
                _sideCollapsed = value;
                OnPropertyChanged(nameof(SideCollapsed));
                OnPropertyChanged(nameof(SideToggleText));
            }
        }

        public string SideToggleText => _sideCollapsed ? "Mostrar la lista" : "Ocultar la lista";

        /// <summary>The text of the page of the rack when no rack is open.</summary>
        public string NoRackText => Panel.Browser.ActiveRackId == null
            ? "Elige un rack en la lista o selecciónalo en el dibujo de AutoCAD."
            : string.Empty;

        // "Configuración"
        public string BlockLibraryPath => SafeRead(() => _settings.BlockLibraryPath, string.Empty);

        public string BlockLibraryStatus
        {
            get
            {
                var exists = SafeRead(() => _settings.BlockLibraryExists, false);
                var overridden = SafeRead(() => _settings.BlockLibraryOverridden, false);
                var where = overridden ? "Ruta personalizada (guardada)." : "Ruta predeterminada (junto a los catálogos).";
                return exists
                    ? where + " Biblioteca disponible."
                    : where + " Biblioteca no encontrada: las piezas que no existan en el dibujo activo se omitirán.";
            }
        }

        public bool BlockLibraryMissing => !SafeRead(() => _settings.BlockLibraryExists, false);

        public string DesignLibraryPath => SafeRead(() => _settings.DesignLibraryPath, string.Empty);

        // ------------------------------------------------------------------ what the host and the user tell it

        /// <summary>
        /// The host announced a change of state: the panel pulls its snapshot and the list follows its session. Contained; it never throws.
        /// </summary>
        public void Refresh()
        {
            try
            {
                Panel.Refresh();
            }
            finally
            {
                Present();
            }
        }

        /// <summary>Shows a section, or the page of the rack.</summary>
        public void ShowPage(RackCadWindowPage page)
        {
            if (_page == page) return;
            _page = page;
            if (page == RackCadWindowPage.Configuracion) RaiseSettings();
            RebuildSections();
            OnPropertyChanged(nameof(Page));
            OnPropertyChanged(nameof(IsRackPage));
            OnPropertyChanged(nameof(IsNuevoPage));
            OnPropertyChanged(nameof(IsVariablesPage));
            OnPropertyChanged(nameof(IsBomPage));
            OnPropertyChanged(nameof(IsBibliotecaPage));
            OnPropertyChanged(nameof(IsConfiguracionPage));
        }

        /// <summary>Opens a rack of the list: the browser makes it active and the center shows its page.</summary>
        public void OpenRack(string rackId) => Contained(() => RackList.Open(rackId));

        /// <summary>
        /// Runs a classic flow (the editor of a new system, Variables, the list, the BOM, the library, the classic menu, the help). The
        /// window says whether AutoCAD took it; it never waits for it and never writes the drawing.
        /// </summary>
        public void Run(RackCadClassicAction action)
        {
            if (_classic == null)
            {
                Say("Esta acción no está disponible aquí.", failed: true);
                return;
            }

            RackCadClassicResult result;
            try
            {
                result = _classic.Run(action) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            switch (result.Status)
            {
                case RackCadClassicStatus.Started:
                    Say(StartedText(action), failed: false);
                    break;
                case RackCadClassicStatus.Busy:
                    Say("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", failed: true);
                    break;
                case RackCadClassicStatus.NoDocument:
                    Say("Abre o crea un dibujo para usar esta acción.", failed: true);
                    break;
                default:
                    Say("No se pudo abrir: " + (string.IsNullOrWhiteSpace(result.Reason) ? "sin detalle." : result.Reason), failed: true);
                    break;
            }
        }

        /// <summary>Saves the block library that the user chose in "Configuración".</summary>
        public void SetBlockLibraryPath(string path)
        {
            Contained(() =>
            {
                _settings.SetBlockLibraryPath(path);
                RaiseSettings();
                Say("Biblioteca de bloques guardada.", failed: false);
            });
        }

        /// <summary>Goes back to the default block library, next to the catalogs.</summary>
        public void ResetBlockLibraryPath()
        {
            Contained(() =>
            {
                _settings.SetBlockLibraryPath(null);
                RaiseSettings();
                Say("Biblioteca de bloques restablecida a la ruta predeterminada.", failed: false);
            });
        }

        /// <summary>Removes the message of the last door.</summary>
        public void DismissMessage() => Say(string.Empty, failed: false);

        /// <summary>A failure that the view contained: it is said in the message of the window.</summary>
        public void ShowError(Exception exception)
        {
            var detail = string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;
            Say("La ventana tuvo un fallo contenido: " + detail, failed: true);
        }

        // ------------------------------------------------------------------ internals

        private void Present()
        {
            try
            {
                RackList.Present(Panel.PresentedSession, Panel.PresentedCanRead, Panel.PresentedBlock);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }

            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(NoRackText));
        }

        // The browser changed (a tab, the field, the active rack): the marks of the list follow, and a new active rack shows its page.
        private void OnBrowserChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(WorkspaceBrowserViewModel.ActiveRackId) || string.IsNullOrEmpty(e.PropertyName))
                {
                    var active = Panel.Browser.ActiveRackId;
                    if (active != null && !string.Equals(active, _lastActiveRack, StringComparison.Ordinal)) ShowPage(RackCadWindowPage.Rack);
                    _lastActiveRack = active;
                    OnPropertyChanged(nameof(NoRackText));
                }

                if (e.PropertyName == nameof(WorkspaceBrowserViewModel.ActiveRackId)
                    || e.PropertyName == nameof(WorkspaceBrowserViewModel.Tabs)
                    || e.PropertyName == nameof(WorkspaceBrowserViewModel.SelectionRackId)
                    || string.IsNullOrEmpty(e.PropertyName))
                {
                    RackList.RefreshStates();
                    OnPropertyChanged(nameof(HeaderText));
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RebuildSections()
        {
            var sections = new List<RackCadWindowSection>(SectionOrder.Count);
            foreach (var (page, label) in SectionOrder) sections.Add(new RackCadWindowSection(page, label, page == _page));
            _sections = sections;
            OnPropertyChanged(nameof(Sections));
        }

        private void RaiseSettings()
        {
            OnPropertyChanged(nameof(BlockLibraryPath));
            OnPropertyChanged(nameof(BlockLibraryStatus));
            OnPropertyChanged(nameof(BlockLibraryMissing));
            OnPropertyChanged(nameof(DesignLibraryPath));
        }

        private void Say(string text, bool failed)
        {
            _actionMessage = text ?? string.Empty;
            _actionFailed = failed && _actionMessage.Length > 0;
            OnPropertyChanged(nameof(ActionMessage));
            OnPropertyChanged(nameof(ActionFailed));
            OnPropertyChanged(nameof(HasActionMessage));
        }

        private static string StartedText(RackCadClassicAction action)
        {
            switch (action)
            {
                case RackCadClassicAction.Variables: return "Se abrió Variables en AutoCAD.";
                case RackCadClassicAction.RackList: return "Se abrió la lista de racks del dibujo en AutoCAD.";
                case RackCadClassicAction.TotalBom: return "Se abrió el BOM total del dibujo en AutoCAD.";
                case RackCadClassicAction.DesignLibrary: return "Se abrió la biblioteca de diseños en AutoCAD.";
                case RackCadClassicAction.ClassicMenu: return "Se abrió el menú clásico de RackCad en AutoCAD.";
                case RackCadClassicAction.Help: return "Se abrió la ayuda de RackCad en AutoCAD.";
                case RackCadClassicAction.StructuralSection: return "Se abrió el generador de perfiles estructurales en AutoCAD.";
                default: return "Se abrió el editor en AutoCAD. Al terminar, el rack nuevo aparece en la lista.";
            }
        }

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private static T SafeRead<T>(Func<T> read, T fallback)
        {
            try
            {
                return read();
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
