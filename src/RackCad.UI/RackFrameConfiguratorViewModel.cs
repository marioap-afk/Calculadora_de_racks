using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Formatting;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    public sealed class RackFrameConfiguratorViewModel : ObservableObject
    {
        private const double HeightTolerance = 0.01;

        /// <summary>The post ends this far above the top horizontal; the height is derived from it. Shared with the
        /// factory (which places the top horizontal at height - remate) so the built height equals the requested one.</summary>
        private static readonly double PostTopRemate = RackFrameConfigurationFactory.PostTopRemate;
        private readonly string defaultDiagonalProfileId;
        private readonly string defaultHorizontalProfileId;
        private readonly string defaultStartConnectionPointId;
        private readonly string defaultEndConnectionPointId;

        private readonly string standardLeftPostCatalogId;
        private readonly string standardLeftReinforcementCatalogId;
        private readonly bool standardLeftHasReinforcement;
        private readonly string standardRightPostCatalogId;
        private readonly string standardRightReinforcementCatalogId;
        private readonly bool standardRightHasReinforcement;
        private readonly string standardLeftPlateCatalogId;
        private readonly string standardLeftPlateConnectionPointId;
        private readonly string standardRightPlateCatalogId;
        private readonly string standardRightPlateConnectionPointId;
        private readonly RackCatalog catalog;
        private readonly BracingPanelMemberBuilder memberBuilder;
        private readonly ConfiguratorNavigationItem panelsNavigationItem;
        private readonly ConfiguratorNavigationItem horizontalsNavigationItem;
        private readonly ObservableCollection<string> modelWarnings;
        private RackFrameConfiguration standardConfigurationSnapshot;

        private bool isAdvancedEditor;
        private IReadOnlyList<RackFrameTemplate> headerTemplateOptions;
        private RackFrameTemplate selectedHeaderTemplate;
        private string simplePostCatalogId;
        private double simpleHeight;
        private double simpleDepth;
        private double simplePostPeralte; // 0 = inherit the post profile width
        private string simpleName;
        private ConfiguratorNavigationItem selectedNavigationItem;
        private BracingSegmentEditorRow selectedBracingSegment;
        private HorizontalEditorRow selectedHorizontal;
        private string statusMessage = "Configuración editable en memoria. Usa \"Insertar lateral\" para dibujar la cabecera.";
        private string statusBrush = "#415161";
        private BracingPattern bulkPattern = BracingPattern.SingleDiagonal;
        private FrameSide bulkSide = FrameSide.Front;
        private string bulkProfileId;
        private string bulkHorizontalProfileId;
        private FrameSide bulkHorizontalFace = FrameSide.Front;
        private int bulkHorizontalQuantity = 1;
        private double bulkHorizontalElevationOffset;

        public RackFrameConfiguratorViewModel(RackFrameConfiguration configuration)
            : this(configuration, null)
        {
        }

        public RackFrameConfiguratorViewModel(RackFrameConfiguration configuration, RackCatalog catalog)
        {
            Configuration = configuration;
            this.catalog = catalog ?? LoadCatalogSafe();
            defaultDiagonalProfileId = this.catalog.Defaults.DiagonalProfile;
            defaultHorizontalProfileId = this.catalog.Defaults.HorizontalProfile;
            defaultStartConnectionPointId = this.catalog.Defaults.BraceStartConnectionPoint;
            defaultEndConnectionPointId = this.catalog.Defaults.BraceEndConnectionPoint;
            bulkProfileId = defaultDiagonalProfileId;
            memberBuilder = new BracingPanelMemberBuilder();

            PostProfileOptions = ToCatalogOptions(this.catalog.PostProfiles);
            // Horizontals and diagonals are both truss members: both dropdowns list the same truss catalog.
            HorizontalProfileOptions = ToCatalogOptions(this.catalog.TrussProfiles);
            DiagonalProfileOptions = ToCatalogOptions(this.catalog.TrussProfiles);
            // Reinforcements are posts: the reinforcement dropdown lists the same post catalog.
            ReinforcementProfileOptions = ToCatalogOptions(this.catalog.PostProfiles);
            BasePlateOptions = ToCatalogOptions(this.catalog.BasePlates);
            ConnectionPointOptions = ToCatalogOptions(this.catalog.ConnectionPoints);

            PatternOptions = Enum.GetValues(typeof(BracingPattern));
            SideOptions = Enum.GetValues(typeof(FrameSide));
            DirectionOptions = Enum.GetValues(typeof(DiagonalDirection));
            HorizontalStateOptions = Enum.GetValues(typeof(FrameComponentState));

            panelsNavigationItem = new ConfiguratorNavigationItem("Segments", "Paneles", "Espacios entre horizontales");
            horizontalsNavigationItem = new ConfiguratorNavigationItem("Horizontals", "Horizontales", "Piezas físicas independientes");
            NavigationItems = new ObservableCollection<ConfiguratorNavigationItem>
            {
                new ConfiguratorNavigationItem("Header", "Cabecera", "Datos generales"),
                new ConfiguratorNavigationItem("LeftPost", "Poste izquierdo", "Perfil y refuerzo"),
                new ConfiguratorNavigationItem("RightPost", "Poste derecho", "Perfil y refuerzo"),
                panelsNavigationItem,
                horizontalsNavigationItem,
                new ConfiguratorNavigationItem("Reinforcements", "Refuerzos", "Refuerzos por poste"),
                new ConfiguratorNavigationItem("Plates", "Placas", "Placa base y conexión"),
                new ConfiguratorNavigationItem("Connections", "Conexiones", "Puntos y offsets"),
                new ConfiguratorNavigationItem("Other", "Otros elementos", "Reservado")
            };

            Horizontals = new ObservableCollection<HorizontalEditorRow>();
            BracingSegments = new ObservableCollection<BracingSegmentEditorRow>();
            SelectedBracingSegments = new ObservableCollection<BracingSegmentEditorRow>();
            SelectedHorizontals = new ObservableCollection<HorizontalEditorRow>();
            HorizontalOptions = new ObservableCollection<string>();
            Exceptions = new ObservableCollection<FrameExceptionEditorRow>();
            ExceptionGroups = new ObservableCollection<FrameExceptionGroup>();
            modelWarnings = new ObservableCollection<string>();

            standardLeftPostCatalogId = NormalizeText(configuration.LeftPost?.PostCatalogId);
            standardLeftReinforcementCatalogId = NormalizeText(configuration.LeftPost?.ReinforcementCatalogId);
            standardLeftHasReinforcement = configuration.LeftPost != null && configuration.LeftPost.HasReinforcement;
            standardRightPostCatalogId = NormalizeText(configuration.RightPost?.PostCatalogId);
            standardRightReinforcementCatalogId = NormalizeText(configuration.RightPost?.ReinforcementCatalogId);
            standardRightHasReinforcement = configuration.RightPost != null && configuration.RightPost.HasReinforcement;
            standardLeftPlateCatalogId = NormalizeText(configuration.LeftBasePlate?.PlateCatalogId);
            standardLeftPlateConnectionPointId = NormalizeText(configuration.LeftBasePlate?.ConnectionPointId);
            standardRightPlateCatalogId = NormalizeText(configuration.RightBasePlate?.PlateCatalogId);
            standardRightPlateConnectionPointId = NormalizeText(configuration.RightBasePlate?.ConnectionPointId);

            // Merge the shared catalog/built-in templates with the user's saved ones (%APPDATA%\RackCad).
            var headerTemplates = LoadMergedHeaderTemplates(out var templateLoadWarning);

            HeaderTemplateOptions = headerTemplates;
            selectedHeaderTemplate = headerTemplates.Count > 0 ? headerTemplates[0] : RackFrameTemplateCatalog.Default;
            simplePostCatalogId = NormalizeText(configuration.LeftPost?.PostCatalogId);
            simpleHeight = configuration.Height;
            simpleDepth = configuration.Depth;
            simplePostPeralte = configuration.PostPeralte;
            simpleName = configuration.Name;

            EnsureModernConfiguration();
            LoadRowsFromConfiguration();
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshPhysicalMembers();
            standardConfigurationSnapshot = CloneConfiguration(Configuration);
            RebuildNavigationItems();
            RebuildExceptions();
            SelectedNavigationItem = NavigationItems.First();

            if (templateLoadWarning != null)
            {
                StatusMessage = templateLoadWarning;
                StatusBrush = "#B00020";
            }
        }

        public RackFrameConfiguration Configuration { get; private set; }
        public Array PatternOptions { get; private set; }
        public Array SideOptions { get; private set; }
        public Array DirectionOptions { get; private set; }
        public Array HorizontalStateOptions { get; private set; }
        public ObservableCollection<ConfiguratorNavigationItem> NavigationItems { get; private set; }
        public ObservableCollection<HorizontalEditorRow> Horizontals { get; private set; }
        public ObservableCollection<BracingSegmentEditorRow> BracingSegments { get; private set; }
        public ObservableCollection<BracingSegmentEditorRow> SelectedBracingSegments { get; private set; }
        public ObservableCollection<HorizontalEditorRow> SelectedHorizontals { get; private set; }
        public ObservableCollection<string> HorizontalOptions { get; private set; }
        public ObservableCollection<CatalogOption> PostProfileOptions { get; private set; }
        public ObservableCollection<CatalogOption> HorizontalProfileOptions { get; private set; }
        public ObservableCollection<CatalogOption> DiagonalProfileOptions { get; private set; }
        public ObservableCollection<CatalogOption> ReinforcementProfileOptions { get; private set; }
        public ObservableCollection<CatalogOption> BasePlateOptions { get; private set; }
        public ObservableCollection<CatalogOption> ConnectionPointOptions { get; private set; }

        public bool IsAdvancedEditor
        {
            get => isAdvancedEditor;
            set
            {
                if (isAdvancedEditor == value)
                {
                    return;
                }

                isAdvancedEditor = value;
                OnPropertyChanged(nameof(IsAdvancedEditor));
                OnPropertyChanged(nameof(IsSimpleEditor));
                OnPropertyChanged(nameof(EditorModeLabel));
            }
        }

        public bool IsSimpleEditor => !isAdvancedEditor;

        public string EditorModeLabel => isAdvancedEditor ? "Editor avanzado" : "Configuración rápida";

        public IReadOnlyList<RackFrameTemplate> HeaderTemplateOptions
        {
            get => headerTemplateOptions;
            private set => SetProperty(ref headerTemplateOptions, value);
        }

        public RackFrameTemplate SelectedHeaderTemplate
        {
            get => selectedHeaderTemplate;
            set
            {
                if (selectedHeaderTemplate == value)
                {
                    return;
                }

                selectedHeaderTemplate = value;

                if (value != null)
                {
                    if (value.DefaultHeight > 0.0)
                    {
                        simpleHeight = value.DefaultHeight;
                        OnPropertyChanged(nameof(SimpleHeightText));
                    }

                    if (value.DefaultDepth > 0.0)
                    {
                        simpleDepth = value.DefaultDepth;
                        OnPropertyChanged(nameof(SimpleDepthText));
                    }
                }

                OnPropertyChanged(nameof(SelectedHeaderTemplate));
            }
        }

        public string SimplePostCatalogId
        {
            get => simplePostCatalogId;
            set
            {
                var normalized = NormalizeText(value);

                if (simplePostCatalogId == normalized)
                {
                    return;
                }

                simplePostCatalogId = normalized;
                OnPropertyChanged(nameof(SimplePostCatalogId));
            }
        }

        public string SimpleHeightText
        {
            get => FormatEditableNumber(simpleHeight);
            set
            {
                if (TryParseDimension(value, out var parsedValue) && parsedValue > 0.0)
                {
                    simpleHeight = parsedValue;
                    ClearInputError("Alto inválido: escribe un número mayor que cero.");
                }
                else if (!string.IsNullOrWhiteSpace(value))
                {
                    StatusMessage = "Alto inválido: escribe un número mayor que cero.";
                    StatusBrush = "#B00020";
                }

                OnPropertyChanged();
            }
        }

        public string SimpleDepthText
        {
            get => FormatEditableNumber(simpleDepth);
            set
            {
                if (TryParseDimension(value, out var parsedValue) && parsedValue > 0.0)
                {
                    simpleDepth = parsedValue;
                    ClearInputError("Fondo inválido: escribe un número mayor que cero.");
                }
                else if (!string.IsNullOrWhiteSpace(value))
                {
                    StatusMessage = "Fondo inválido: escribe un número mayor que cero.";
                    StatusBrush = "#B00020";
                }

                OnPropertyChanged();
            }
        }

        /// <summary>Client-facing rack name (like the selective's). Stored on the configuration and used to name the block.</summary>
        public string SimpleNameText
        {
            get => simpleName ?? string.Empty;
            set
            {
                simpleName = value;
                if (Configuration != null) Configuration.Name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Post peralte for the quick config (in). Empty = inherit the post profile width. Affects the planta
        /// (and, when this is a selective per-post cabecera, the frontal).</summary>
        public string SimplePostPeralteText
        {
            get => simplePostPeralte > 0.0 ? FormatEditableNumber(simplePostPeralte) : string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    simplePostPeralte = 0.0; // inherit the profile width
                    ClearInputError("Peralte de poste inválido: escribe un número mayor que cero.");
                }
                else if (TryParseDimension(value, out var parsedValue) && parsedValue > 0.0)
                {
                    simplePostPeralte = parsedValue;
                    ClearInputError("Peralte de poste inválido: escribe un número mayor que cero.");
                }
                else
                {
                    StatusMessage = "Peralte de poste inválido: escribe un número mayor que cero.";
                    StatusBrush = "#B00020";
                }

                OnPropertyChanged();
            }
        }

        /// <summary>Clears a sticky input-error status once the offending input becomes valid again.</summary>
        private void ClearInputError(string message)
        {
            if (StatusMessage == message)
            {
                StatusMessage = string.Empty;
            }
        }
        public ObservableCollection<FrameExceptionEditorRow> Exceptions { get; private set; }
        public ObservableCollection<FrameExceptionGroup> ExceptionGroups { get; private set; }
        public ObservableCollection<string> ModelWarnings => modelWarnings;

        public ConfiguratorNavigationItem SelectedNavigationItem
        {
            get => selectedNavigationItem;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (SetProperty(ref selectedNavigationItem, value))
                {
                    if (selectedNavigationItem.Segment != null)
                    {
                        SelectedBracingSegment = selectedNavigationItem.Segment;
                    }

                    if (selectedNavigationItem.Horizontal != null)
                    {
                        SelectedHorizontal = selectedNavigationItem.Horizontal;
                    }

                    if (selectedNavigationItem.Key == "Segments" && SelectedBracingSegment == null && BracingSegments.Count > 0)
                    {
                        SelectedBracingSegment = BracingSegments[0];
                    }

                    if (selectedNavigationItem.Key == "Horizontals" && SelectedHorizontal == null && Horizontals.Count > 0)
                    {
                        SelectedHorizontal = Horizontals[0];
                    }

                    OnPropertyChanged(nameof(ActiveDetailKey));
                    OnPropertyChanged(nameof(ActiveDetailTitle));
                    OnPropertyChanged(nameof(ActiveDetailSubtitle));
                }
            }
        }

        public string ActiveDetailKey => SelectedNavigationItem == null
            ? "Header"
            : SelectedNavigationItem.IsSegment ? "Segments" : SelectedNavigationItem.IsHorizontal ? "Horizontals" : SelectedNavigationItem.Key;

        public string ActiveDetailTitle => SelectedNavigationItem == null ? "Cabecera" : SelectedNavigationItem.Title;

        public string ActiveDetailSubtitle => SelectedNavigationItem == null ? "Datos generales" : SelectedNavigationItem.Subtitle;

        public string Name
        {
            get => Configuration.Name;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (Configuration.Name == normalizedValue)
                {
                    return;
                }

                Configuration.Name = normalizedValue;
                OnPropertyChanged();
                MarkConfigurationEdited("Nombre de cabecera actualizado.", structural: false);
            }
        }

        public string Units => Configuration.Units;

        public double Height
        {
            get => Configuration.Height;
            private set
            {
                if (value <= 0.0 || AreClose(Configuration.Height, value))
                {
                    return;
                }

                Configuration.Height = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HeightText));
                RefreshValidationProperties();
                MarkConfigurationEdited("Altura actualizada.");
            }
        }

        public string HeightText
        {
            get => FormatEditableNumber(Configuration.Height);
            set
            {
                if (TryParseDimension(value, out var parsedValue))
                {
                    Height = parsedValue;
                }

                OnPropertyChanged();
            }
        }

        public double Depth
        {
            get => Configuration.Depth;
            private set
            {
                if (value <= 0.0 || AreClose(Configuration.Depth, value))
                {
                    return;
                }

                Configuration.Depth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DepthText));
                RefreshPhysicalMembers();
                MarkConfigurationEdited("Fondo de cabecera actualizado.");
            }
        }

        public string DepthText
        {
            get => FormatEditableNumber(Configuration.Depth);
            set
            {
                if (TryParseDimension(value, out var parsedValue))
                {
                    Depth = parsedValue;
                }

                OnPropertyChanged();
            }
        }

        // ---- Celosía / diagonal parameters (advanced editor) ----

        public int CelosiaStartTroquel
        {
            get => Configuration.CelosiaStartTroquel;
            private set
            {
                if (value < 1 || Configuration.CelosiaStartTroquel == value)
                {
                    return;
                }

                Configuration.CelosiaStartTroquel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CelosiaStartTroquelText));
                MarkConfigurationEdited("Inicio de celosía actualizado.");
            }
        }

        public string CelosiaStartTroquelText
        {
            get => Configuration.CelosiaStartTroquel.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (TryParseIntAtLeast(value, 1, out var parsed))
                {
                    CelosiaStartTroquel = parsed;
                }

                OnPropertyChanged();
            }
        }

        public int DiagonalStartOffsetTroqueles
        {
            get => Configuration.DiagonalStartOffsetTroqueles;
            private set
            {
                if (value < 0 || Configuration.DiagonalStartOffsetTroqueles == value)
                {
                    return;
                }

                Configuration.DiagonalStartOffsetTroqueles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiagonalStartOffsetText));
                MarkConfigurationEdited("Offset de diagonal (inicio) actualizado.");
            }
        }

        public string DiagonalStartOffsetText
        {
            get => Configuration.DiagonalStartOffsetTroqueles.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (TryParseIntAtLeast(value, 0, out var parsed))
                {
                    DiagonalStartOffsetTroqueles = parsed;
                }

                OnPropertyChanged();
            }
        }

        public int DiagonalEndOffsetTroqueles
        {
            get => Configuration.DiagonalEndOffsetTroqueles;
            private set
            {
                if (value < 0 || Configuration.DiagonalEndOffsetTroqueles == value)
                {
                    return;
                }

                Configuration.DiagonalEndOffsetTroqueles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiagonalEndOffsetText));
                MarkConfigurationEdited("Offset de diagonal (fin) actualizado.");
            }
        }

        public string DiagonalEndOffsetText
        {
            get => Configuration.DiagonalEndOffsetTroqueles.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (TryParseIntAtLeast(value, 0, out var parsed))
                {
                    DiagonalEndOffsetTroqueles = parsed;
                }

                OnPropertyChanged();
            }
        }

        private static bool TryParseIntAtLeast(string text, int minimum, out int value)
        {
            return LocalizedNumberParser.TryInteger(text, out value)
                && value >= minimum;
        }

        public string StandardBaselineId => Configuration.StandardBaselineId;
        public string StandardBaselineVersion => Configuration.StandardBaselineVersion;
        public PostAssembly LeftPost => Configuration.LeftPost;
        public PostAssembly RightPost => Configuration.RightPost;
        public BasePlatePlacement LeftBasePlate => Configuration.LeftBasePlate;
        public BasePlatePlacement RightBasePlate => Configuration.RightBasePlate;
        public string TemplateSummary => StandardBaselineId + " / versión " + StandardBaselineVersion;

        public string LeftPostCatalogId
        {
            get => NormalizeText(LeftPost?.PostCatalogId);
            set
            {
                if (LeftPost == null)
                {
                    return;
                }

                var normalizedValue = NormalizeText(value);

                if (LeftPost.PostCatalogId == normalizedValue)
                {
                    return;
                }

                LeftPost.PostCatalogId = normalizedValue;
                OnPropertyChanged();
                MarkConfigurationEdited("Poste izquierdo actualizado.");
            }
        }

        public string LeftPostDescription
        {
            get => NormalizeText(LeftPost?.Description);
            set
            {
                if (LeftPost == null)
                {
                    return;
                }

                LeftPost.Description = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Descripción del poste izquierdo actualizada.", structural: false);
            }
        }

        public bool LeftPostHasReinforcement
        {
            get => LeftPost != null && LeftPost.HasReinforcement;
            set
            {
                if (LeftPost == null || LeftPost.HasReinforcement == value)
                {
                    return;
                }

                LeftPost.HasReinforcement = value;
                OnPropertyChanged();
                MarkConfigurationEdited("Refuerzo de poste izquierdo actualizado.");
            }
        }

        public string LeftPostReinforcementCatalogId
        {
            get => NormalizeText(LeftPost?.ReinforcementCatalogId);
            set
            {
                if (LeftPost == null)
                {
                    return;
                }

                LeftPost.ReinforcementCatalogId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Tipo de refuerzo izquierdo actualizado.");
            }
        }

        public double LeftPostReinforcementHeight
        {
            get => LeftPost?.ReinforcementHeight ?? 0.0;
            set
            {
                if (LeftPost == null || Math.Abs(LeftPost.ReinforcementHeight - value) < 1e-6)
                {
                    return;
                }

                LeftPost.ReinforcementHeight = Math.Max(0.0, value);
                OnPropertyChanged();
                MarkConfigurationEdited("Altura de refuerzo izquierdo actualizada.");
            }
        }

        public string RightPostCatalogId
        {
            get => NormalizeText(RightPost?.PostCatalogId);
            set
            {
                if (RightPost == null)
                {
                    return;
                }

                RightPost.PostCatalogId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Poste derecho actualizado.");
            }
        }

        public string RightPostDescription
        {
            get => NormalizeText(RightPost?.Description);
            set
            {
                if (RightPost == null)
                {
                    return;
                }

                RightPost.Description = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Descripción del poste derecho actualizada.", structural: false);
            }
        }

        public bool RightPostHasReinforcement
        {
            get => RightPost != null && RightPost.HasReinforcement;
            set
            {
                if (RightPost == null || RightPost.HasReinforcement == value)
                {
                    return;
                }

                RightPost.HasReinforcement = value;
                OnPropertyChanged();
                MarkConfigurationEdited("Refuerzo de poste derecho actualizado.");
            }
        }

        public string RightPostReinforcementCatalogId
        {
            get => NormalizeText(RightPost?.ReinforcementCatalogId);
            set
            {
                if (RightPost == null)
                {
                    return;
                }

                RightPost.ReinforcementCatalogId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Tipo de refuerzo derecho actualizado.");
            }
        }

        public double RightPostReinforcementHeight
        {
            get => RightPost?.ReinforcementHeight ?? 0.0;
            set
            {
                if (RightPost == null || Math.Abs(RightPost.ReinforcementHeight - value) < 1e-6)
                {
                    return;
                }

                RightPost.ReinforcementHeight = Math.Max(0.0, value);
                OnPropertyChanged();
                MarkConfigurationEdited("Altura de refuerzo derecho actualizada.");
            }
        }

        public string LeftPlateCatalogId
        {
            get => NormalizeText(LeftBasePlate?.PlateCatalogId);
            set
            {
                if (LeftBasePlate == null)
                {
                    return;
                }

                LeftBasePlate.PlateCatalogId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Placa izquierda actualizada.");
            }
        }

        public string LeftPlateConnectionPointId
        {
            get => NormalizeText(LeftBasePlate?.ConnectionPointId);
            set
            {
                if (LeftBasePlate == null)
                {
                    return;
                }

                LeftBasePlate.ConnectionPointId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Punto de placa izquierda actualizado.");
            }
        }

        public string RightPlateCatalogId
        {
            get => NormalizeText(RightBasePlate?.PlateCatalogId);
            set
            {
                if (RightBasePlate == null)
                {
                    return;
                }

                RightBasePlate.PlateCatalogId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Placa derecha actualizada.");
            }
        }

        public string RightPlateConnectionPointId
        {
            get => NormalizeText(RightBasePlate?.ConnectionPointId);
            set
            {
                if (RightBasePlate == null)
                {
                    return;
                }

                RightBasePlate.ConnectionPointId = NormalizeText(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Punto de placa derecha actualizado.");
            }
        }

        /// <summary>Manual plate PERALTE (in) for a custom cabecera; empty = derived from the post.</summary>
        public string LeftPlatePeralte
        {
            get => FormatOptional(LeftBasePlate?.PeralteOverride);
            set
            {
                if (LeftBasePlate == null)
                {
                    return;
                }

                LeftBasePlate.PeralteOverride = ParseOptional(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Peralte de placa izquierda actualizado.");
            }
        }

        public string RightPlatePeralte
        {
            get => FormatOptional(RightBasePlate?.PeralteOverride);
            set
            {
                if (RightBasePlate == null)
                {
                    return;
                }

                RightBasePlate.PeralteOverride = ParseOptional(value);
                OnPropertyChanged();
                MarkConfigurationEdited("Peralte de placa derecha actualizado.");
            }
        }

        public double ConfiguredHeight => Horizontals.Count == 0 ? 0.0 : Horizontals.Max(horizontal => horizontal.Elevation);

        public bool IsModelConsistent => ModelWarnings.Count == 0;

        public string ConfiguredHeightText => FormatInches(ConfiguredHeight);

        public string PostHeightText => FormatInches(Height);

        // The height is derived (última horizontal + remate), so there is no target-vs-configured check:
        // the banner reports only real model inconsistencies.
        public string HeightValidationMessage => IsModelConsistent
            ? "Modelo consistente. La altura se recalcula desde las horizontales (última + " + FormatInches(PostTopRemate) + ")."
            : ModelWarnings.FirstOrDefault();

        public string HeightValidationBrush => IsModelConsistent ? "#2F855A" : "#B00020";

        public string HeightValidationBackground => IsModelConsistent ? "#E8F5E9" : "#FDECEC";

        public string WarningSummary => IsModelConsistent
            ? "Sin advertencias de modelo."
            : "Revisar consistencia del modelo antes de generar dibujo.";

        public string SelectedSegmentCountLabel => SelectedBracingSegments.Count == 0
            ? "Sin paneles seleccionados"
            : SelectedBracingSegments.Count.ToString(CultureInfo.InvariantCulture) + " panel(es) seleccionado(s)";

        public string SelectedHorizontalCountLabel => SelectedHorizontals.Count == 0
            ? "Sin horizontales seleccionadas"
            : SelectedHorizontals.Count.ToString(CultureInfo.InvariantCulture) + " horizontal(es) seleccionada(s)";

        public BracingSegmentEditorRow SelectedBracingSegment
        {
            get => selectedBracingSegment;
            set
            {
                if (SetProperty(ref selectedBracingSegment, value))
                {
                    RefreshSelectionProperties();
                }
            }
        }

        public HorizontalEditorRow SelectedHorizontal
        {
            get => selectedHorizontal;
            set
            {
                if (SetProperty(ref selectedHorizontal, value))
                {
                    RefreshSelectionProperties();
                }
            }
        }

        public bool CanRestoreSelectedSegment => SelectedBracingSegment != null && SelectedBracingSegment.IsModified;
        public bool CanAddSegmentNearSelection => Horizontals.Count > 0;
        public bool CanSplitSelectedSegment => SelectedBracingSegment != null && SelectedBracingSegment.ClearHeight > 0.1;
        public bool CanCombineSegments => BracingSegments.Count > 1 && SelectedBracingSegment != null && Horizontals.Count > 2;
        public bool CanDeleteSelectedHorizontal => SelectedHorizontal != null && Horizontals.Count > 2;
        public bool CanDuplicateSelectedHorizontal => SelectedHorizontal != null;

        /// <summary>True when discarding the model (Restore / Generate) would lose manual edits or exceptions.</summary>
        public bool HasUnsavedManualEdits =>
            Horizontals.Any(row => row != null && row.IsModified)
            || BracingSegments.Any(row => row != null && row.IsModified)
            || Exceptions.Count > 0;

        public string SelectedSegmentSummary => SelectedBracingSegment == null
            ? "Selecciona un panel en la tabla para editar sus propiedades."
            : "Panel " + SelectedBracingSegment.Index.ToString(CultureInfo.InvariantCulture) + " / " +
              SelectedBracingSegment.ClearHeightText + " in / " + SelectedBracingSegment.Pattern;

        public string ExceptionSummary => Exceptions.Count == 0
            ? "Sin excepciones registradas."
            : Exceptions.Count.ToString(CultureInfo.InvariantCulture) + " excepcion(es) registradas.";

        public string StatusMessage
        {
            get => statusMessage;
            private set => SetProperty(ref statusMessage, value);
        }

        public string StatusBrush
        {
            get => statusBrush;
            private set => SetProperty(ref statusBrush, value);
        }

        public BracingPattern BulkPattern
        {
            get => bulkPattern;
            set => SetProperty(ref bulkPattern, value);
        }

        public FrameSide BulkSide
        {
            get => bulkSide;
            set => SetProperty(ref bulkSide, value);
        }

        public string BulkProfileId
        {
            get => bulkProfileId;
            set => SetProperty(ref bulkProfileId, value);
        }

        public string BulkHorizontalProfileId
        {
            get => bulkHorizontalProfileId;
            set => SetProperty(ref bulkHorizontalProfileId, value);
        }

        public FrameSide BulkHorizontalFace
        {
            get => bulkHorizontalFace;
            set => SetProperty(ref bulkHorizontalFace, value);
        }

        public int BulkHorizontalQuantity
        {
            get => bulkHorizontalQuantity;
            set => SetProperty(ref bulkHorizontalQuantity, value);
        }

        public double BulkHorizontalElevationOffset
        {
            get => bulkHorizontalElevationOffset;
            set => SetProperty(ref bulkHorizontalElevationOffset, value);
        }

        /// <summary>Set the multi-selection of horizontals from the grid (mirrors <see cref="SetSelectedSegments"/>).</summary>
        public void SetSelectedHorizontals(IEnumerable<HorizontalEditorRow> selectedHorizontals)
        {
            SelectedHorizontals.Clear();
            foreach (var horizontal in (selectedHorizontals ?? Enumerable.Empty<HorizontalEditorRow>()).Where(h => h != null))
            {
                SelectedHorizontals.Add(horizontal);
            }

            OnPropertyChanged(nameof(SelectedHorizontalCountLabel));
        }

        public void SetSelectedSegments(IEnumerable<BracingSegmentEditorRow> selectedSegments)
        {
            SelectedBracingSegments.Clear();

            foreach (var segment in selectedSegments.Where(segment => segment != null))
            {
                SelectedBracingSegments.Add(segment);
            }

            RefreshSelectionProperties();
        }

        public void SelectNavigation(string key)
        {
            var item = NavigationItems.FirstOrDefault(navigationItem => navigationItem.Key == key);

            if (item != null)
            {
                SelectedNavigationItem = item;
            }
        }

        public void SelectNavigationItem(ConfiguratorNavigationItem item)
        {
            if (item != null)
            {
                SelectedNavigationItem = item;
            }
        }

        public ConfiguratorNavigationItem GetNavigationItemForSegment(BracingSegmentEditorRow segment)
        {
            return panelsNavigationItem.Children.FirstOrDefault(item => item.Segment == segment);
        }

        public ConfiguratorNavigationItem GetNavigationItemForHorizontal(HorizontalEditorRow horizontal)
        {
            return horizontalsNavigationItem.Children.FirstOrDefault(item => item.Horizontal == horizontal);
        }

        public void AddCommonSegment(double clearHeight)
        {
            AddHorizontalAt(ConfiguredHeight + Math.Max(1.0, clearHeight), defaultHorizontalProfileId, 1, "Horizontal agregada para crear claro de " + FormatInches(clearHeight) + ".");
        }

        public void AddHorizontalSegment()
        {
            // New horizontals open the CONFIGURED panel clear, not a re-declared 44" literal.
            AddHorizontalAt(ConfiguredHeight + Configuration.PanelClear, defaultHorizontalProfileId, 1, "Horizontal agregada.");
        }

        public void AddHorizontal()
        {
            AddHorizontalSegment();
        }

        public void DeleteSelectedHorizontal()
        {
            if (SelectedHorizontal == null || Horizontals.Count <= 2)
            {
                StatusMessage = "Selecciona una horizontal. Deben quedar al menos dos horizontales.";
                StatusBrush = "#B00020";
                return;
            }

            var nextIndex = Math.Max(0, Horizontals.IndexOf(SelectedHorizontal) - 1);
            Configuration.Horizontals.Remove(SelectedHorizontal.DomainHorizontal);
            Horizontals.Remove(SelectedHorizontal);
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            SelectedHorizontal = Horizontals[Math.Min(nextIndex, Horizontals.Count - 1)];
            RefreshAfterStructureChange("Horizontal eliminada.");
        }

        public void DuplicateSelectedHorizontal()
        {
            if (SelectedHorizontal == null)
            {
                StatusMessage = "Selecciona una horizontal para duplicar.";
                StatusBrush = "#B00020";
                return;
            }

            AddHorizontalAt(
                SelectedHorizontal.Elevation + Configuration.PanelClear,
                SelectedHorizontal.ProfileId,
                SelectedHorizontal.Quantity,
                "Horizontal duplicada.");
        }

        public void SplitSelectedSegment()
        {
            if (SelectedBracingSegment == null)
            {
                StatusMessage = "Selecciona un panel antes de dividir.";
                StatusBrush = "#B00020";
                return;
            }

            AddHorizontalAt(
                (SelectedBracingSegment.StartElevation + SelectedBracingSegment.EndElevation) / 2.0,
                defaultHorizontalProfileId,
                1,
                "Panel dividido con una nueva horizontal intermedia.");
        }

        public void CombineSelectedSegments()
        {
            if (SelectedBracingSegment == null)
            {
                StatusMessage = "Selecciona un panel antes de combinar.";
                StatusBrush = "#B00020";
                return;
            }

            RemoveSharedHorizontalForPanel(SelectedBracingSegment);
        }

        public void ApplyNoBracingToSelection()
        {
            ApplyToTargetSegments(segment => segment.Pattern = BracingPattern.NoBracing, "Sin celosía aplicado a la selección.");
        }

        public void ApplyDoubleBracingToSelection()
        {
            ApplyToTargetSegments(segment =>
            {
                segment.Pattern = BracingPattern.DoubleDiagonal;
                segment.SideMode = FrameSide.Front;
            }, "Doble diagonal aplicada a la selección.");
        }

        public void ApplyBulkPattern()
        {
            ApplyToTargetSegments(segment => segment.Pattern = BulkPattern, "Arreglo de panel aplicado a la selección.");
        }

        public void ApplyBulkSide()
        {
            ApplyToTargetSegments(segment => segment.SideMode = BulkSide, "Cara de montaje aplicada a la selección.");
        }

        public void ApplyBulkProfile()
        {
            if (string.IsNullOrWhiteSpace(BulkProfileId))
            {
                StatusMessage = "Captura un perfil antes de aplicar edición masiva.";
                StatusBrush = "#B00020";
                return;
            }

            ApplyToTargetSegments(segment => segment.BraceProfileId = BulkProfileId.Trim(), "Perfil aplicado a la selección.");
        }

        // ---- Edición masiva de HORIZONTALES (espeja el patrón de paneles) ----

        public void ApplyBulkHorizontalProfile()
        {
            if (string.IsNullOrWhiteSpace(BulkHorizontalProfileId))
            {
                StatusMessage = "Elige un perfil antes de aplicar edición masiva de horizontales.";
                StatusBrush = "#B00020";
                return;
            }

            ApplyToTargetHorizontals(h => h.ProfileId = BulkHorizontalProfileId.Trim(), "Perfil aplicado a las horizontales seleccionadas.");
        }

        public void ApplyBulkHorizontalFace()
            => ApplyToTargetHorizontals(h => h.MountingFace = BulkHorizontalFace, "Cara aplicada a las horizontales seleccionadas.");

        public void ApplyBulkHorizontalQuantity()
            => ApplyToTargetHorizontals(h => h.Quantity = Math.Max(1, BulkHorizontalQuantity), "Cantidad aplicada a las horizontales seleccionadas.");

        public void ApplyBulkHorizontalElevationOffset()
        {
            var delta = BulkHorizontalElevationOffset;
            if (Math.Abs(delta) < 1e-9)
            {
                StatusMessage = "Indica un desplazamiento de elevación distinto de cero.";
                StatusBrush = "#B00020";
                return;
            }

            ApplyToTargetHorizontals(h => h.Elevation = Math.Max(0.0, h.Elevation + delta), "Elevación desplazada en las horizontales seleccionadas.");
        }

        public void RestoreSelectedSegment()
        {
            SelectedBracingSegment?.RestoreStandard();
        }

        public void RestoreStandardConfiguration()
        {
            // Replace with a fresh clone of the standard snapshot (initiative I-17): LoadRowsFromConfiguration +
            // RefreshPhysicalMembers below rebuild every row and derived member from it, so the reference swap is
            // equivalent to the previous in-place field copy. The standard snapshot carries no overrides, so this
            // clears the user's modifications and exceptions exactly as before.
            Configuration = CloneConfiguration(standardConfigurationSnapshot);
            SelectedBracingSegments.Clear();
            SelectedBracingSegment = null;
            SelectedHorizontal = null;
            LoadRowsFromConfiguration();
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshPhysicalMembers();
            RebuildNavigationItems();
            RebuildExceptions();

            SelectedBracingSegment = BracingSegments.Count > 0 ? BracingSegments[0] : null;
            SelectedHorizontal = Horizontals.Count > 0 ? Horizontals[0] : null;
            SelectedNavigationItem = SelectedBracingSegment == null
                ? panelsNavigationItem
                : GetNavigationItemForSegment(SelectedBracingSegment) ?? panelsNavigationItem;
            RefreshAllConfigurationProperties();
            StatusMessage = "Cabecera estándar restaurada. Modificaciones y excepciones limpiadas.";
            StatusBrush = "#2F855A";
        }

        public void ApplySimpleConfiguration()
        {
            try
            {
                var template = SelectedHeaderTemplate ?? RackFrameTemplateCatalog.Default;
                var generated = new RackFrameConfigurationFactory(catalog)
                    .Build(template, SimplePostCatalogId, simpleHeight, simpleDepth);
                generated.PostPeralte = simplePostPeralte; // carry the quick-config post peralte (0 = inherit)
                if (!string.IsNullOrWhiteSpace(simpleName)) generated.Name = simpleName.Trim(); // keep the user's rack name

                ReplaceConfigurationAndReload(
                    generated,
                    "Cabecera generada: " + template.Name + " (" + FormatInches(simpleHeight) + " alto, " + FormatInches(simpleDepth) + " fondo).");
            }
            catch (Exception ex)
            {
                StatusMessage = "No se pudo generar la cabecera: " + ex.Message;
                StatusBrush = "#B00020";
            }
        }

        /// <summary>
        /// Saves the CURRENT cabecera as a reusable user template under
        /// <c>%APPDATA%\RackCad\user-templates.json</c>, then refreshes the "Tipo de cabecera" dropdown and
        /// selects the new one. Only the standard shape/profiles/post/plate are stored — per-panel exceptions
        /// are NOT part of a template. <paramref name="error"/> is null on success, or the reason on failure
        /// (also mirrored to the red status line so the UI shows it either way).
        /// </summary>
        public void SaveAsUserTemplate(string name, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Escribe un nombre para la plantilla.";
                StatusMessage = error;
                StatusBrush = "#B00020";
                return;
            }

            var trimmedName = name.Trim();
            var id = "USER-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var template = RackFrameTemplateFactory.FromConfiguration(Configuration, id, trimmedName);

            try
            {
                new UserTemplateStore(UserTemplateStore.DefaultPath).Save(template);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                StatusMessage = "No se pudo guardar la plantilla: " + ex.Message;
                StatusBrush = "#B00020";
                return;
            }

            // Refresh from disk and select the one we just created. Match by id: the merged list holds the
            // freshly deserialized instance (which the ComboBox uses), not our local `template` reference.
            HeaderTemplateOptions = LoadMergedHeaderTemplates(out _);
            SelectedHeaderTemplate = FindTemplateById(HeaderTemplateOptions, id) ?? SelectedHeaderTemplate;

            StatusMessage = "Plantilla '" + trimmedName + "' guardada. Se guarda la forma/perfiles/poste/placa actuales; no las excepciones por panel.";
            StatusBrush = "#2F855A";
        }

        /// <summary>
        /// The templates shown in "Tipo de cabecera": the shared catalog/built-in templates MERGED with the
        /// user's saved ones (<c>%APPDATA%\RackCad\user-templates.json</c>). Catalog templates keep their order
        /// (so the standard stays the default selection); a user template with the same id OVERRIDES it (user
        /// wins), and remaining user templates are appended. <paramref name="loadWarning"/> reports a catalog
        /// parse failure (else null); the user library is best-effort and never throws.
        /// </summary>
        private IReadOnlyList<RackFrameTemplate> LoadMergedHeaderTemplates(out string loadWarning)
        {
            loadWarning = null;
            IReadOnlyList<RackFrameTemplate> catalogTemplates;

            try
            {
                catalogTemplates = RackFrameTemplateProvider.FromBaseDirectory().Load();
            }
            catch (Exception ex)
            {
                catalogTemplates = RackFrameTemplateCatalog.All;
                loadWarning = "Plantillas: " + ex.Message + " Se usaron las plantillas internas.";
            }

            var userTemplates = new UserTemplateStore(UserTemplateStore.DefaultPath).Load();

            var merged = new List<RackFrameTemplate>();
            var placedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Catalog first (keeps the standard as [0]); a colliding user id replaces it in place (user wins).
            foreach (var template in catalogTemplates)
            {
                if (template == null)
                {
                    continue;
                }

                var chosen = FindTemplateById(userTemplates, template.Id) ?? template;
                merged.Add(chosen);

                if (!string.IsNullOrWhiteSpace(template.Id))
                {
                    placedIds.Add(template.Id);
                }
            }

            // Append user templates that don't collide with a catalog id.
            foreach (var template in userTemplates)
            {
                if (template == null || (!string.IsNullOrWhiteSpace(template.Id) && placedIds.Contains(template.Id)))
                {
                    continue;
                }

                merged.Add(template);

                if (!string.IsNullOrWhiteSpace(template.Id))
                {
                    placedIds.Add(template.Id);
                }
            }

            return merged;
        }

        private static RackFrameTemplate FindTemplateById(IReadOnlyList<RackFrameTemplate> templates, string id)
        {
            if (templates == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (var template in templates)
            {
                if (template != null && string.Equals(template.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return template;
                }
            }

            return null;
        }

        public BillOfMaterials BuildBom()
        {
            return BomBuilder.Build(Configuration, catalog);
        }

        public void SaveProjectTo(string path)
        {
            try
            {
                new RackFrameProjectStore().Save(Configuration, path);
                StatusMessage = "Proyecto guardado: " + Path.GetFileName(path);
                StatusBrush = "#2F855A";
            }
            catch (Exception ex)
            {
                StatusMessage = "No se pudo guardar el proyecto: " + ex.Message;
                StatusBrush = "#B00020";
            }
        }

        public void LoadProjectFrom(string path)
        {
            try
            {
                var loaded = new RackFrameProjectStore().Load(path);
                ReplaceConfigurationAndReload(loaded, "Proyecto abierto: " + Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                StatusMessage = "No se pudo abrir el proyecto: " + ex.Message;
                StatusBrush = "#B00020";
            }
        }

        private void ReplaceConfigurationAndReload(RackFrameConfiguration source, string successMessage)
        {
            // Reference swap to a fresh clone (initiative I-17); the reload sequence below rebuilds rows and
            // derived members from it, matching the previous in-place field copy.
            Configuration = CloneConfiguration(source);
            SelectedBracingSegments.Clear();
            SelectedBracingSegment = null;
            SelectedHorizontal = null;
            LoadRowsFromConfiguration();
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshPhysicalMembers();
            standardConfigurationSnapshot = CloneConfiguration(Configuration);
            RebuildNavigationItems();
            RebuildExceptions();

            SelectedBracingSegment = BracingSegments.Count > 0 ? BracingSegments[0] : null;
            SelectedHorizontal = Horizontals.Count > 0 ? Horizontals[0] : null;
            SelectedNavigationItem = SelectedBracingSegment == null
                ? panelsNavigationItem
                : GetNavigationItemForSegment(SelectedBracingSegment) ?? panelsNavigationItem;

            simpleHeight = Configuration.Height;
            simplePostPeralte = Configuration.PostPeralte;
            simpleDepth = Configuration.Depth;
            simpleName = Configuration.Name;
            simplePostCatalogId = NormalizeText(Configuration.LeftPost?.PostCatalogId);
            OnPropertyChanged(nameof(SimpleHeightText));
            OnPropertyChanged(nameof(SimpleDepthText));
            OnPropertyChanged(nameof(SimplePostPeralteText));
            OnPropertyChanged(nameof(SimpleNameText));
            OnPropertyChanged(nameof(SimplePostCatalogId));

            RefreshAllConfigurationProperties();
            StatusMessage = successMessage;
            StatusBrush = "#2F855A";
        }

        public void ReportNonBlockingError(Exception ex)
        {
            StatusMessage = "Error no bloqueante del configurador: " + ex.Message;
            StatusBrush = "#B00020";
        }

        internal void SegmentWasEdited(BracingSegmentEditorRow editedSegment)
        {
            ResolvePanelElevations();
            RefreshPhysicalMembers();
            RebuildNavigationItems();
            RebuildExceptions();
            StatusMessage = "Panel actualizado en memoria.";
            StatusBrush = "#415161";
            RefreshSelectionProperties();
        }

        internal void HorizontalWasEdited(HorizontalEditorRow editedHorizontal)
        {
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshAfterStructureChange("Horizontal actualizada. Paneles adyacentes recalculados.");
        }

        private void EnsureModernConfiguration()
        {
            if (Configuration.BracingPanels.Count == 0)
            {
                GeneratePanelsFromHorizontals(new Dictionary<string, BracingPanel>());
            }
        }

        private void LoadRowsFromConfiguration()
        {
            Horizontals.Clear();
            BracingSegments.Clear();

            foreach (var horizontal in Configuration.Horizontals.OrderBy(item => item.Elevation).ThenBy(item => item.Number))
            {
                Horizontals.Add(new HorizontalEditorRow(this, horizontal, horizontal.IsStandard));
            }

            foreach (var panel in Configuration.BracingPanels.OrderBy(item => item.Number))
            {
                BracingSegments.Add(new BracingSegmentEditorRow(this, panel, panel.IsStandard));
            }
        }

        private void AddHorizontalAt(double elevation, string profileId, int quantity, string successMessage)
        {
            if (FrameModelValidator.CollidesWithExisting(Configuration.Horizontals.Select(h => h.Elevation), elevation, HeightTolerance))
            {
                StatusMessage = "Ya existe una horizontal en " + FormatInches(elevation) + " (o muy cerca). No se agregó para evitar un panel de altura cero.";
                StatusBrush = "#B00020";
                return;
            }

            var horizontal = new FrameHorizontal
            {
                Id = CreateNextHorizontalId(),
                Number = Configuration.Horizontals.Count + 1,
                Elevation = elevation,
                ProfileId = NormalizeText(profileId),
                Quantity = Math.Max(1, quantity),
                MountingFace = FrameSide.Front,
                State = FrameComponentState.Manual,
                IsStandard = false
            };

            Configuration.Horizontals.Add(horizontal);
            var row = new HorizontalEditorRow(this, horizontal, isStandardHorizontal: false);
            Horizontals.Add(row);
            SelectedHorizontal = row;
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshAfterStructureChange(successMessage);
        }

        private void RemoveSharedHorizontalForPanel(BracingSegmentEditorRow panel)
        {
            if (panel == null || Horizontals.Count <= 2)
            {
                StatusMessage = "No se puede combinar: deben quedar al menos dos horizontales.";
                StatusBrush = "#B00020";
                return;
            }

            var upperHorizontal = Horizontals.FirstOrDefault(horizontal => horizontal.Id == panel.UpperHorizontalId);

            if (upperHorizontal == null || upperHorizontal == Horizontals.Last())
            {
                upperHorizontal = Horizontals.FirstOrDefault(horizontal => horizontal.Id == panel.LowerHorizontalId);
            }

            if (upperHorizontal == null || Horizontals.Count <= 2)
            {
                StatusMessage = "No se encontró una horizontal compartida para combinar.";
                StatusBrush = "#B00020";
                return;
            }

            Configuration.Horizontals.Remove(upperHorizontal.DomainHorizontal);
            Horizontals.Remove(upperHorizontal);
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshAfterStructureChange("Paneles combinados eliminando una horizontal intermedia.");
        }

        private void NormalizeHorizontalsAndPanels(bool preservePanelOverrides)
        {
            var orderedHorizontals = Configuration.Horizontals
                .OrderBy(horizontal => horizontal.Elevation)
                .ThenBy(horizontal => horizontal.Number)
                .ToList();
            var horizontalIdMap = CreateSequentialHorizontalIdMap(orderedHorizontals);
            var existingPanels = preservePanelOverrides
                ? CreatePanelTemplateMap(Configuration.BracingPanels, horizontalIdMap)
                : new Dictionary<string, BracingPanel>();
            var selectedHorizontal = SelectedHorizontal?.DomainHorizontal;

            Configuration.Horizontals.Clear();
            Horizontals.Clear();
            HorizontalOptions.Clear();

            if (selectedHorizontal != null)
            {
                SelectedHorizontal = null;
            }

            for (var index = 0; index < orderedHorizontals.Count; index++)
            {
                var horizontal = orderedHorizontals[index];
                horizontal.Id = "H" + (index + 1).ToString(CultureInfo.InvariantCulture);
                horizontal.Number = index + 1;
                Configuration.Horizontals.Add(horizontal);
                var row = new HorizontalEditorRow(this, horizontal, horizontal.IsStandard);
                Horizontals.Add(row);
                HorizontalOptions.Add(horizontal.Id);

                if (selectedHorizontal == horizontal)
                {
                    SelectedHorizontal = row;
                }
            }

            if (selectedHorizontal != null && SelectedHorizontal == null && Horizontals.Count > 0)
            {
                SelectedHorizontal = Horizontals[0];
            }

            GeneratePanelsFromHorizontals(existingPanels);
            LoadPanelRows();
            ResolvePanelElevations();
            SyncHeightToHorizontals();
            ValidateModelConsistency();
            RefreshValidationProperties();
        }

        /// <summary>
        /// The height is derived, not a target: after any structural change the post ends
        /// <see cref="PostTopRemate"/> above the top horizontal, so editing claros recalculates the
        /// height (and with it posts, reinforcements and the drawing).
        /// </summary>
        private void SyncHeightToHorizontals()
        {
            var top = Configuration.Horizontals.Count == 0
                ? 0.0
                : Configuration.Horizontals.Max(horizontal => horizontal.Elevation);

            if (top <= 0.0)
            {
                return;
            }

            var derived = Math.Round(top + PostTopRemate, 4);

            if (AreClose(Configuration.Height, derived))
            {
                return;
            }

            Configuration.Height = derived;
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(HeightText));
        }

        private static Dictionary<string, string> CreateSequentialHorizontalIdMap(IList<FrameHorizontal> orderedHorizontals)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (orderedHorizontals == null)
            {
                return result;
            }

            for (var index = 0; index < orderedHorizontals.Count; index++)
            {
                var oldId = NormalizeText(orderedHorizontals[index].Id);

                if (string.IsNullOrWhiteSpace(oldId) || result.ContainsKey(oldId))
                {
                    continue;
                }

                result.Add(oldId, "H" + (index + 1).ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }

        private void GeneratePanelsFromHorizontals(IDictionary<string, BracingPanel> existingPanels)
        {
            var orderedHorizontals = Configuration.Horizontals
                .OrderBy(horizontal => horizontal.Elevation)
                .ThenBy(horizontal => horizontal.Number)
                .ToList();

            Configuration.BracingPanels.Clear();

            for (var index = 0; index < orderedHorizontals.Count - 1; index++)
            {
                var lower = orderedHorizontals[index];
                var upper = orderedHorizontals[index + 1];
                var key = GetPanelKey(lower.Id, upper.Id);
                var panel = existingPanels != null && existingPanels.TryGetValue(key, out var existingPanel)
                    ? existingPanel
                    : CreateDefaultPanel(index + 1, lower.Id, upper.Id, isStandard: lower.IsStandard && upper.IsStandard);

                panel.Number = index + 1;
                panel.PanelId = "P" + (index + 1).ToString(CultureInfo.InvariantCulture);
                panel.LowerHorizontalId = lower.Id;
                panel.UpperHorizontalId = upper.Id;
                panel.StartElevation = lower.Elevation;
                panel.EndElevation = upper.Elevation;
                Configuration.BracingPanels.Add(panel);
            }
        }

        private static Dictionary<string, BracingPanel> CreatePanelTemplateMap(IEnumerable<BracingPanel> panels, IDictionary<string, string> horizontalIdMap)
        {
            var result = new Dictionary<string, BracingPanel>();

            if (panels == null)
            {
                return result;
            }

            foreach (var panel in panels.OrderBy(item => item.Number))
            {
                if (panel == null)
                {
                    continue;
                }

                if (!TryGetMappedPanelKey(panel, horizontalIdMap, out var key, out var mappedLowerHorizontalId, out var mappedUpperHorizontalId))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(key) || result.ContainsKey(key))
                {
                    continue;
                }

                result.Add(key, ClonePanelTemplate(panel, mappedLowerHorizontalId, mappedUpperHorizontalId));
            }

            return result;
        }

        private static bool TryGetMappedPanelKey(BracingPanel panel, IDictionary<string, string> horizontalIdMap, out string key, out string mappedLowerHorizontalId, out string mappedUpperHorizontalId)
        {
            key = string.Empty;
            mappedLowerHorizontalId = string.Empty;
            mappedUpperHorizontalId = string.Empty;

            if (panel == null || horizontalIdMap == null)
            {
                return false;
            }

            if (!horizontalIdMap.TryGetValue(NormalizeText(panel.LowerHorizontalId), out mappedLowerHorizontalId) ||
                !horizontalIdMap.TryGetValue(NormalizeText(panel.UpperHorizontalId), out mappedUpperHorizontalId))
            {
                return false;
            }

            if (TryGetHorizontalIdNumber(mappedLowerHorizontalId, out var lowerNumber) &&
                TryGetHorizontalIdNumber(mappedUpperHorizontalId, out var upperNumber) &&
                upperNumber < lowerNumber)
            {
                var swap = mappedLowerHorizontalId;
                mappedLowerHorizontalId = mappedUpperHorizontalId;
                mappedUpperHorizontalId = swap;
            }

            key = GetPanelKey(mappedLowerHorizontalId, mappedUpperHorizontalId);
            return !string.IsNullOrWhiteSpace(key);
        }

        private static BracingPanel ClonePanelTemplate(BracingPanel source, string lowerHorizontalId, string upperHorizontalId)
        {
            return new BracingPanel
            {
                PanelId = source.PanelId,
                Number = source.Number,
                LowerHorizontalId = lowerHorizontalId,
                UpperHorizontalId = upperHorizontalId,
                StartElevation = source.StartElevation,
                EndElevation = source.EndElevation,
                Arrangement = source.Arrangement,
                MountingFace = source.MountingFace,
                DiagonalDirection = source.DiagonalDirection,
                DiagonalProfileId = source.DiagonalProfileId,
                StartConnectionPointId = source.StartConnectionPointId,
                EndConnectionPointId = source.EndConnectionPointId,
                IsStandard = source.IsStandard,
                IsException = source.IsException
            };
        }

        private BracingPanel CreateDefaultPanel(int number, string lowerHorizontalId, string upperHorizontalId, bool isStandard)
        {
            return new BracingPanel
            {
                PanelId = "P" + number.ToString(CultureInfo.InvariantCulture),
                Number = number,
                LowerHorizontalId = lowerHorizontalId,
                UpperHorizontalId = upperHorizontalId,
                Arrangement = BracingPattern.SingleDiagonal,
                MountingFace = FrameSide.Front,
                DiagonalDirection = DiagonalDirection.AutoAlternating,
                DiagonalProfileId = defaultDiagonalProfileId,
                StartConnectionPointId = defaultStartConnectionPointId,
                EndConnectionPointId = defaultEndConnectionPointId,
                IsStandard = isStandard,
                IsException = !isStandard
            };
        }

        private void LoadPanelRows()
        {
            var selectedPanelId = SelectedBracingSegment?.DomainPanel.PanelId;
            BracingSegments.Clear();

            foreach (var panel in Configuration.BracingPanels.OrderBy(item => item.Number))
            {
                var row = new BracingSegmentEditorRow(this, panel, panel.IsStandard);
                BracingSegments.Add(row);

                if (selectedPanelId == panel.PanelId)
                {
                    SelectedBracingSegment = row;
                }
            }

            if (SelectedBracingSegment == null && BracingSegments.Count > 0)
            {
                SelectedBracingSegment = BracingSegments[0];
            }
        }

        private void ResolvePanelElevations()
        {
            var horizontalsById = Configuration.Horizontals
                .GroupBy(horizontal => horizontal.Id)
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var panel in Configuration.BracingPanels)
            {
                if (!horizontalsById.TryGetValue(panel.LowerHorizontalId, out var lower) ||
                    !horizontalsById.TryGetValue(panel.UpperHorizontalId, out var upper))
                {
                    continue;
                }

                panel.StartElevation = Math.Min(lower.Elevation, upper.Elevation);
                panel.EndElevation = Math.Max(lower.Elevation, upper.Elevation);
            }

            foreach (var row in BracingSegments)
            {
                row.RefreshDerivedValues();
            }

            ValidateModelConsistency();
        }

        private void ValidateModelConsistency()
        {
            ModelWarnings.Clear();

            var orderedHorizontals = Configuration.Horizontals
                .OrderBy(horizontal => horizontal.Elevation)
                .ThenBy(horizontal => horizontal.Number)
                .ToList();

            foreach (var duplicateIdGroup in Configuration.Horizontals
                .GroupBy(horizontal => NormalizeText(horizontal.Id))
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1))
            {
                ModelWarnings.Add("Horizontal duplicada: " + duplicateIdGroup.Key + ".");
            }

            // Tolerance-based model checks (near-equal elevations, zero-height panels, non-zero base,
            // unknown catalog ids) live in the testable Application layer to stay consistent with the
            // model's own equality tolerance instead of exact rounding.
            foreach (var warning in FrameModelValidator.Validate(Configuration, catalog, HeightTolerance))
            {
                ModelWarnings.Add(warning);
            }

            for (var index = 1; index < orderedHorizontals.Count; index++)
            {
                if (orderedHorizontals[index].Elevation < orderedHorizontals[index - 1].Elevation)
                {
                    ModelWarnings.Add("Orden incorrecto de elevaciones en horizontales.");
                    break;
                }
            }

            if (!HorizontalIdsAreIncreasingByElevation(orderedHorizontals))
            {
                ModelWarnings.Add("El orden visual de IDs no coincide con elevaciones. Se espera H1 < H2 < H3...");
            }

            var horizontalIds = new HashSet<string>(Configuration.Horizontals.Select(horizontal => NormalizeText(horizontal.Id)));

            foreach (var panel in Configuration.BracingPanels)
            {
                if (!horizontalIds.Contains(NormalizeText(panel.LowerHorizontalId)) ||
                    !horizontalIds.Contains(NormalizeText(panel.UpperHorizontalId)))
                {
                    ModelWarnings.Add("Panel " + panel.Number.ToString(CultureInfo.InvariantCulture) + " tiene referencias huerfanas.");
                }
            }

            foreach (var duplicatePanelGroup in Configuration.BracingPanels
                .GroupBy(panel => GetPanelKey(panel.LowerHorizontalId, panel.UpperHorizontalId))
                .Where(group => group.Count() > 1))
            {
                ModelWarnings.Add("Panel duplicado: " + duplicatePanelGroup.Key + ".");
            }

            var expectedKeys = new HashSet<string>();

            for (var index = 0; index < orderedHorizontals.Count - 1; index++)
            {
                expectedKeys.Add(GetPanelKey(orderedHorizontals[index].Id, orderedHorizontals[index + 1].Id));
            }

            var actualKeys = new HashSet<string>(Configuration.BracingPanels.Select(panel => GetPanelKey(panel.LowerHorizontalId, panel.UpperHorizontalId)));

            foreach (var actualKey in actualKeys)
            {
                if (!expectedKeys.Contains(actualKey))
                {
                    ModelWarnings.Add("Panel no consecutivo detectado: " + actualKey + ".");
                }
            }

            foreach (var expectedKey in expectedKeys)
            {
                if (!actualKeys.Contains(expectedKey))
                {
                    ModelWarnings.Add("Falta panel consecutivo: " + expectedKey + ".");
                }
            }

            OnPropertyChanged(nameof(IsModelConsistent));
        }

        private static bool HorizontalIdsAreIncreasingByElevation(IList<FrameHorizontal> orderedHorizontals)
        {
            var previousNumber = int.MinValue;

            foreach (var horizontal in orderedHorizontals)
            {
                if (!TryGetHorizontalIdNumber(horizontal.Id, out var number))
                {
                    continue;
                }

                if (number < previousNumber)
                {
                    return false;
                }

                previousNumber = number;
            }

            return true;
        }

        private static bool TryGetHorizontalIdNumber(string horizontalId, out int number)
        {
            number = 0;

            if (string.IsNullOrWhiteSpace(horizontalId) ||
                !horizontalId.StartsWith("H", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return int.TryParse(horizontalId.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
        }

        private void RefreshAfterStructureChange(string message)
        {
            RefreshPhysicalMembers();
            RebuildNavigationItems();
            RebuildExceptions();
            StatusMessage = message;
            StatusBrush = "#415161";
            RefreshSelectionProperties();
        }

        private void RefreshPhysicalMembers()
        {
            memberBuilder.RefreshPhysicalModel(Configuration);
        }

        private void RebuildNavigationItems()
        {
            panelsNavigationItem.Children.Clear();
            horizontalsNavigationItem.Children.Clear();

            foreach (var panel in BracingSegments)
            {
                panelsNavigationItem.Children.Add(new ConfiguratorNavigationItem(
                    "Panel-" + panel.Index.ToString(CultureInfo.InvariantCulture),
                    "Panel " + panel.Index.ToString(CultureInfo.InvariantCulture),
                    panel.ClearHeightText + " in / " + panel.Pattern + " / " + panel.SideMode,
                    panel));
            }

            foreach (var horizontal in Horizontals)
            {
                horizontalsNavigationItem.Children.Add(new ConfiguratorNavigationItem(
                    "Horizontal-" + horizontal.Number.ToString(CultureInfo.InvariantCulture),
                    horizontal.Label,
                    horizontal.ElevationText + " in / " + horizontal.ProfileId,
                    horizontal));
            }
        }

        private void RebuildExceptions()
        {
            Exceptions.Clear();
            Configuration.Exceptions.Clear();
            ExceptionGroups.Clear();

            foreach (var horizontal in Horizontals)
            {
                horizontal.RefreshModificationState();
                horizontal.AddExceptionsTo(Configuration.Exceptions, Exceptions);
            }

            foreach (var panel in BracingSegments)
            {
                panel.RefreshModificationState();
                panel.AddExceptionsTo(Configuration.Exceptions, Exceptions);
            }

            AddAssemblyExceptions();
            RebuildExceptionGroups();
            OnPropertyChanged(nameof(ExceptionSummary));
        }

        private void RebuildExceptionGroups()
        {
            foreach (var group in Exceptions.GroupBy(exception => exception.TargetId))
            {
                var exceptionGroup = new FrameExceptionGroup(group.Key);

                foreach (var exception in group)
                {
                    exceptionGroup.Changes.Add(exception);
                }

                ExceptionGroups.Add(exceptionGroup);
            }
        }

        private void AddAssemblyExceptions()
        {
            AddTextException("Poste izquierdo", "Perfil", ExceptionType.ProfileChange, standardLeftPostCatalogId, LeftPostCatalogId);
            AddTextException("Poste izquierdo", "Refuerzo", ExceptionType.Reinforcement, FormatBoolean(standardLeftHasReinforcement), FormatBoolean(LeftPostHasReinforcement));
            AddTextException("Poste izquierdo", "Tipo de refuerzo", ExceptionType.Reinforcement, standardLeftReinforcementCatalogId, LeftPostReinforcementCatalogId);
            AddTextException("Poste derecho", "Perfil", ExceptionType.ProfileChange, standardRightPostCatalogId, RightPostCatalogId);
            AddTextException("Poste derecho", "Refuerzo", ExceptionType.Reinforcement, FormatBoolean(standardRightHasReinforcement), FormatBoolean(RightPostHasReinforcement));
            AddTextException("Poste derecho", "Tipo de refuerzo", ExceptionType.Reinforcement, standardRightReinforcementCatalogId, RightPostReinforcementCatalogId);
            AddTextException("Placa izquierda", "Placa", ExceptionType.PlateChange, standardLeftPlateCatalogId, LeftPlateCatalogId);
            AddTextException("Placa izquierda", "Punto de conexión", ExceptionType.ConnectionPointChange, standardLeftPlateConnectionPointId, LeftPlateConnectionPointId);
            AddTextException("Placa derecha", "Placa", ExceptionType.PlateChange, standardRightPlateCatalogId, RightPlateCatalogId);
            AddTextException("Placa derecha", "Punto de conexión", ExceptionType.ConnectionPointChange, standardRightPlateConnectionPointId, RightPlateConnectionPointId);
        }

        private void AddTextException(string targetId, string fieldName, ExceptionType exceptionType, string standardValue, string overrideValue)
        {
            var normalizedStandardValue = NormalizeText(standardValue);
            var normalizedOverrideValue = NormalizeText(overrideValue);

            if (normalizedStandardValue == normalizedOverrideValue)
            {
                return;
            }

            Configuration.Exceptions.Add(new FrameExceptionOverride
            {
                ExceptionType = exceptionType,
                TargetId = targetId,
                StandardValue = normalizedStandardValue,
                OverrideValue = normalizedOverrideValue,
                Reason = "Cambio manual desde configurador MVP"
            });

            Exceptions.Add(new FrameExceptionEditorRow(targetId, fieldName, exceptionType, normalizedStandardValue, normalizedOverrideValue));
        }

        private void ApplyToTargetSegments(Action<BracingSegmentEditorRow> apply, string successMessage)
        {
            var targetSegments = GetTargetSegments();

            if (targetSegments.Count == 0)
            {
                StatusMessage = "Selecciona uno o varios paneles antes de aplicar esta acción.";
                StatusBrush = "#B00020";
                return;
            }

            foreach (var segment in targetSegments)
            {
                apply(segment);
            }

            StatusMessage = successMessage;
            StatusBrush = "#415161";
        }

        private IList<BracingSegmentEditorRow> GetTargetSegments()
        {
            if (SelectedBracingSegments.Count > 0)
            {
                return SelectedBracingSegments.ToList();
            }

            if (SelectedBracingSegment != null)
            {
                return new List<BracingSegmentEditorRow> { SelectedBracingSegment };
            }

            return new List<BracingSegmentEditorRow>();
        }

        /// <summary>Apply a change to every target horizontal. Mutates the DOMAIN horizontal directly (not the row's
        /// setters) so it does NOT trigger a per-edit normalize mid-loop — the collection is rebuilt ONCE at the end.</summary>
        private void ApplyToTargetHorizontals(Action<FrameHorizontal> apply, string successMessage)
        {
            var targets = GetTargetHorizontals();

            if (targets.Count == 0)
            {
                StatusMessage = "Selecciona una o varias horizontales antes de aplicar esta acción.";
                StatusBrush = "#B00020";
                return;
            }

            foreach (var row in targets)
            {
                var domain = row?.DomainHorizontal;
                if (domain != null)
                {
                    apply(domain);
                }
            }

            SelectedHorizontals.Clear(); // rows get recreated below; the old references are stale
            NormalizeHorizontalsAndPanels(preservePanelOverrides: true);
            RefreshAfterStructureChange(successMessage);
            OnPropertyChanged(nameof(SelectedHorizontalCountLabel));
        }

        private IList<HorizontalEditorRow> GetTargetHorizontals()
        {
            if (SelectedHorizontals.Count > 0)
            {
                return SelectedHorizontals.ToList();
            }

            if (SelectedHorizontal != null)
            {
                return new List<HorizontalEditorRow> { SelectedHorizontal };
            }

            return new List<HorizontalEditorRow>();
        }

        /// <summary>Pass <paramref name="structural"/> = false for text-only edits (name, descriptions): they
        /// must not pay a full physical-model rebuild + exception re-scan.</summary>
        private void MarkConfigurationEdited(string message, bool structural = true)
        {
            if (structural)
            {
                RefreshPhysicalMembers();
                RebuildExceptions();
            }

            StatusMessage = message;
            StatusBrush = "#415161";
        }

        private void RefreshAllConfigurationProperties()
        {
            OnPropertyChanged(nameof(Configuration));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Units));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(HeightText));
            OnPropertyChanged(nameof(Depth));
            OnPropertyChanged(nameof(DepthText));
            OnPropertyChanged(nameof(StandardBaselineId));
            OnPropertyChanged(nameof(StandardBaselineVersion));
            OnPropertyChanged(nameof(TemplateSummary));
            OnPropertyChanged(nameof(LeftPost));
            OnPropertyChanged(nameof(LeftPostCatalogId));
            OnPropertyChanged(nameof(LeftPostDescription));
            OnPropertyChanged(nameof(LeftPostHasReinforcement));
            OnPropertyChanged(nameof(LeftPostReinforcementCatalogId));
            OnPropertyChanged(nameof(LeftPostReinforcementHeight));
            OnPropertyChanged(nameof(RightPost));
            OnPropertyChanged(nameof(RightPostCatalogId));
            OnPropertyChanged(nameof(RightPostDescription));
            OnPropertyChanged(nameof(RightPostHasReinforcement));
            OnPropertyChanged(nameof(RightPostReinforcementCatalogId));
            OnPropertyChanged(nameof(RightPostReinforcementHeight));
            OnPropertyChanged(nameof(LeftBasePlate));
            OnPropertyChanged(nameof(LeftPlateCatalogId));
            OnPropertyChanged(nameof(LeftPlateConnectionPointId));
            OnPropertyChanged(nameof(LeftPlatePeralte));
            OnPropertyChanged(nameof(RightBasePlate));
            OnPropertyChanged(nameof(RightPlateCatalogId));
            OnPropertyChanged(nameof(RightPlateConnectionPointId));
            OnPropertyChanged(nameof(RightPlatePeralte));
            RefreshSelectionProperties();
            RefreshValidationProperties();
            OnPropertyChanged(nameof(ExceptionSummary));
        }

        private void RefreshSelectionProperties()
        {
            OnPropertyChanged(nameof(SelectedSegmentCountLabel));
            OnPropertyChanged(nameof(SelectedHorizontalCountLabel));
            OnPropertyChanged(nameof(CanRestoreSelectedSegment));
            OnPropertyChanged(nameof(CanAddSegmentNearSelection));
            OnPropertyChanged(nameof(CanSplitSelectedSegment));
            OnPropertyChanged(nameof(CanCombineSegments));
            OnPropertyChanged(nameof(CanDeleteSelectedHorizontal));
            OnPropertyChanged(nameof(CanDuplicateSelectedHorizontal));
            OnPropertyChanged(nameof(SelectedSegmentSummary));
        }

        private void RefreshValidationProperties()
        {
            OnPropertyChanged(nameof(ConfiguredHeight));
            OnPropertyChanged(nameof(IsModelConsistent));
            OnPropertyChanged(nameof(ConfiguredHeightText));
            OnPropertyChanged(nameof(PostHeightText));
            OnPropertyChanged(nameof(HeightValidationMessage));
            OnPropertyChanged(nameof(HeightValidationBrush));
            OnPropertyChanged(nameof(HeightValidationBackground));
            OnPropertyChanged(nameof(WarningSummary));
        }

        private string CreateNextHorizontalId()
        {
            var index = Configuration.Horizontals.Count + 1;

            while (Configuration.Horizontals.Any(horizontal => horizontal.Id == "H" + index.ToString(CultureInfo.InvariantCulture)))
            {
                index++;
            }

            return "H" + index.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetPanelKey(string lowerHorizontalId, string upperHorizontalId)
        {
            return NormalizeText(lowerHorizontalId) + ">" + NormalizeText(upperHorizontalId);
        }

        // Single canonical deep-clone (initiative I-17): delegate to RackFrameProjectStore.DeepCopy. The persisted
        // model is owned by RackFrameProjectDocument (no hand-maintained per-field clone that drifts out of sync as
        // fields are added — the audit's U4); the derived model (Members, panel elevations) is rebuilt on load; and
        // the runtime-only Exceptions the document does not persist are re-attached by DeepCopy, so the clone is
        // complete (Exceptions are NOT regenerated on load — only members and elevations are).
        private static RackFrameConfiguration CloneConfiguration(RackFrameConfiguration source)
            => new RackFrameProjectStore().DeepCopy(source);

        private static string FormatOptional(double? value)
            => value.HasValue ? value.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;

        private static double? ParseOptional(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return LocalizedNumberParser.TryDouble(text, out var value) && value > 0.0
                ? value
                : (double?)null;
        }

        private static string FormatInches(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }

        private static string FormatEditableNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string FormatBoolean(bool value)
        {
            return value ? "Si" : "No";
        }

        private static bool TryParseDimension(string value, out double dimension)
        {
            dimension = 0.0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalizedValue = value
                .Trim()
                .Replace("in", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("\"", string.Empty)
                .Trim();

            if (LocalizedNumberParser.TryDouble(normalizedValue, out dimension))
            {
                return dimension > 0.0;
            }

            return false;
        }

        private static bool AreClose(double left, double right)
        {
            return Math.Abs(left - right) < HeightTolerance;
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>Short, human-readable label for a catalog id (its description), for the preview.</summary>
        public string DescribeCatalogId(string id)
        {
            var text = catalog.DescribeId(id);

            if (string.IsNullOrWhiteSpace(text))
            {
                return "-";
            }

            text = text.Trim();
            return text.Length <= 18 ? text : text.Substring(0, 18);
        }

        private static RackCatalog LoadCatalogSafe() => UiSupport.LoadCatalogSafe();

        /// <summary>
        /// Builds combo options that show the catalog display name (Label = displayName, else description,
        /// else id) but carry the id as their value, so the UI is readable while the model keeps ids.
        /// </summary>
        private static ObservableCollection<CatalogOption> ToCatalogOptions<T>(IEnumerable<T> entries)
            where T : CatalogEntryBase
            => new ObservableCollection<CatalogOption>(UiSupport.ToOptions(entries));
    }
}
