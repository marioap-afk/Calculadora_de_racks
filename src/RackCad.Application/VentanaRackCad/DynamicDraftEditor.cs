using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>What «Actualizar» hands to AutoCAD: copies of the design (and its JSON) and of the system, and the trimmed name.</summary>
    public sealed record DynamicDraftSnapshot(string DesignJson, string Name, DynamicRackDesign Design, DynamicRackSystem System);

    /// <summary>
    /// The draft of ONE Dinámico (Pallet Flow) rack in the RackCad window (I-64 A-14, line D, Dinámico): the editable state and its
    /// recomposition, with no WPF and no AutoCAD. It is the same recomposition as the classic editor (<c>RackDynamicSystemWindow</c>):
    /// the same matrix of fronts and cells (<see cref="DynamicFrontMatrix"/>), the same rebuild with reconciliation when the pallet or
    /// the fondos change (<see cref="DynamicRackRebuild"/>), the same in-place update of the calculated cabeceras otherwise, and the same
    /// assembly of the persisted design (<see cref="DynamicEditorDesignAssembler"/>). It composes those Application pieces; it adds no
    /// geometry of its own.
    ///
    /// What the classic editor reads from its boxes is a property here, set by the editor of the window. Two differences are declared
    /// (I-03 of the second review): a value keeps its full precision, where the classic editor rounds every value to two decimals when its
    /// boxes are read again, and the dimension style is kept as read, where the classic editor drops a style that the drawing does not have
    /// (the drawing then dimensions with its current style and the sizes of RackCad in both cases). A recomposition that cannot run says why in <see cref="LastError"/>
    /// and keeps the last valid system and design, exactly as the classic editor keeps its last valid preview. Nothing here writes the
    /// drawing.
    /// </summary>
    public sealed class DynamicDraftEditor
    {
        private readonly DynamicRackSystemBuilder _builder;
        private readonly DynamicRackSystemResolver _resolver;
        private readonly DynamicEditorDesignAssembler _assembler;
        private readonly DynamicHeaderBatchState _headerBatch = new DynamicHeaderBatchState();
        private int _palletsDeepTotal = DynamicRackDefaults.DefaultPalletsDeep;

        public DynamicDraftEditor(RackCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _builder = new DynamicRackSystemBuilder(catalog);
            _resolver = new DynamicRackSystemResolver(catalog);
            _assembler = new DynamicEditorDesignAssembler(catalog, _builder, _resolver);
            PostCatalogId = catalog.Defaults?.Post;
            PostPeralte = DefaultPostPeralte(catalog, PostCatalogId);
            SafetySelections.AddRange(DynamicSafetyDefaults.Build(catalog).Select(selection => selection.DeepCopy()));
            HeaderHeight = catalog.Defaults?.DefaultHeaderHeight ?? 0.0;
        }

        // ------------------------------------------------------------------ the editable state

        public RackCatalog Catalog { get; }

        /// <summary>The fronts, their cells and the selection: the same matrix as the classic editor.</summary>
        public DynamicFrontMatrix Matrix { get; } = new DynamicFrontMatrix();

        /// <summary>The name of the rack (trimmed on recomposition).</summary>
        public string Name { get; set; }

        /// <summary>«Fondo de tarima» of the whole rack, in inches.</summary>
        public double PalletDepth { get; set; } = 48.0;

        /// <summary>The post of the cabeceras, by its catalog id.</summary>
        public string PostCatalogId { get; set; }

        /// <summary>«Peralte de poste», in inches; applied to every cabecera, also the custom ones (as the classic editor does).</summary>
        public double PostPeralte { get; set; }

        /// <summary>Numbering, rack name, annotation scale and dimensions. <c>DimensionViews</c> stays null (legacy) until it is chosen.</summary>
        public DynamicAnnotationOptions Annotations { get; set; } = new DynamicAnnotationOptions();

        /// <summary>A fixed header height instead of the one derived from the levels; null = derived.</summary>
        public double? ManualHeaderHeight { get; set; }

        public int? SeparatorCountOverride { get; set; }

        public double? SeparatorSpacingOverride { get; set; }

        public bool DerivedPostReinforced { get; set; } = true;

        public double? DerivedPostReinforcementHeight { get; set; }

        /// <summary>The safety of the rack, as the classic editor keeps it (only the drawable families are carried).</summary>
        public List<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        // ------------------------------------------------------------------ what the last recomposition produced

        /// <summary>The last valid resolved system; null before the first valid recomposition.</summary>
        public DynamicRackSystem System { get; private set; }

        /// <summary>The persisted design of the last valid recomposition: what «Actualizar» writes.</summary>
        public DynamicRackDesign Design { get; private set; }

        /// <summary>The height of the cabeceras of the last valid recomposition (the manual one when it is fixed).</summary>
        public double HeaderHeight { get; private set; }

        /// <summary>What the levels give, also when the height is fixed by hand; null before the first valid recomposition.</summary>
        public DynamicHeaderHeightResult DerivedHeight { get; private set; }

        /// <summary>
        /// The height of the cabeceras at each transverse post (N fronts, N + 1 posts) of the last valid system: each post takes the highest
        /// of the fronts it touches (rule 1 of the Owner on the prototype: with 5 levels in F1 and 4 in F2 the posts are for 5, 5 and 4),
        /// unless a manual height makes them all equal. Empty before the first valid recomposition.
        /// </summary>
        public IReadOnlyList<double> PostHeights => System == null
            ? Array.Empty<double>()
            : Enumerable.Range(0, System.Fronts.Count + 1).Select(post => DynamicFrontGeometry.PostHeight(System, post)).ToList();

        /// <summary>The real rise of the bed of each front (entrance minus exit of its first level, on the grid of the troqueles).</summary>
        public IReadOnlyList<double> FrontRises => System == null
            ? Array.Empty<double>()
            : System.Fronts.Select(front => DynamicHeaderHeightCalculator.CalculateResolved(front).Slope).ToList();

        /// <summary>The total positions of fondo of the rack, as resolved by the last valid recomposition.</summary>
        public int PalletsDeep => _palletsDeepTotal;

        /// <summary>Why the last recomposition did not run; null when it ran.</summary>
        public string LastError { get; private set; }

        /// <summary>What the last rebuild of the module sequence reported (kept or lost customizations); null when there was none.</summary>
        public string LastRebuildReport { get; private set; }

        // ------------------------------------------------------------------ operations

        /// <summary>A new rack with the defaults of the classic editor, already recomposed.</summary>
        public static DynamicDraftEditor CreateNew(RackCatalog catalog)
        {
            var editor = new DynamicDraftEditor(catalog);
            editor.Recompose();
            return editor;
        }

        /// <summary>
        /// Loads a persisted design, as the classic editor restores it from the drawing: the system is resolved from the design, the
        /// matrix and the scalar inputs are taken from the resolved system, and the design itself is kept as the current one. It does not
        /// recompose, so a design opened and not touched is written back as it was read.
        /// </summary>
        public void LoadFrom(DynamicRackDesign loaded, string name)
        {
            if (loaded == null) throw new ArgumentNullException(nameof(loaded));

            SafetySelections.Clear();
            SafetySelections.AddRange(loaded.SafetySelections.Where(DynamicEditorSafety.Draws).Select(selection => selection.DeepCopy()));
            var resolution = _resolver.Resolve(loaded);
            Design = loaded;
            System = resolution.System;
            Name = name;
            System.Name = Name?.Trim();
            _headerBatch.ForgetSource();
            _headerBatch.Targets.FollowCurrentModule();
            LastRebuildReport = null;
            LastError = null;

            PalletDepth = System.Pallet.Depth;
            _palletsDeepTotal = System.PalletsDeep;
            PostPeralte = System.PostPeralte > 0.0 ? System.PostPeralte : DynamicFrontGeometry.PostPeralte(System, Catalog);
            Matrix.RestoreFromResolved(System.Fronts);
            Annotations = new DynamicAnnotationOptions
            {
                NumberFronts = loaded.NumberFronts,
                NumberLevels = loaded.NumberLevels,
                DrawRackName = loaded.DrawRackName,
                AnnotationScale = loaded.AnnotationScale > 0.0 ? loaded.AnnotationScale : 1.0,
                Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, (int)loaded.Dimensions)),
                DimensionViews = loaded.DimensionViews,
                DimensionStyle = loaded.DimensionStyle,
            };
            SeparatorCountOverride = System.SeparatorCountOverride;
            SeparatorSpacingOverride = System.SeparatorSpacingOverride;
            DerivedPostReinforced = System.DerivedPostReinforced;
            DerivedPostReinforcementHeight = System.DerivedPostReinforcementHeight;
            ManualHeaderHeight = System.ManualHeaderHeightOverride;
            DerivedHeight = resolution.Height;
            HeaderHeight = System.ManualHeaderHeightOverride ?? resolution.Height.HeaderHeight;

            var postId = loaded.HeaderPostCatalogId;
            if (string.IsNullOrWhiteSpace(postId))
            {
                postId = System.Modules
                    .FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration?.LeftPost != null)?
                    .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
            }

            // The post as the classic editor chooses it: one of the catalog. A known post in another spelling is that post; a post that the
            // catalog does not have gives way to the default post of the catalog, and the editor says so (the classic one does it silently).
            PostCatalogId = Catalog.Defaults?.Post;
            LoadNotice = null;
            if (!string.IsNullOrWhiteSpace(postId))
            {
                var posts = Catalog.PostProfiles ?? Array.Empty<ProfileCatalogEntry>();
                var known = posts.FirstOrDefault(p => string.Equals(p?.Id, postId.Trim(), StringComparison.OrdinalIgnoreCase));
                if (known != null || posts.Count == 0) PostCatalogId = known?.Id ?? postId;
                else LoadNotice = "El poste «" + postId + "» no está en el catálogo: se usa «" + PostCatalogId + "», como en el editor clásico.";
            }
        }

        /// <summary>What the last load had to change to open the design (a post that the catalog does not have); null when nothing.</summary>
        public string LoadNotice { get; private set; }

        /// <summary>
        /// Recomposes the rack from the current state, with the rules of the classic editor: the inputs are validated with its messages,
        /// the module sequence is rebuilt with reconciliation only when the pallet or the fondos change (or when the user restores the
        /// standard layout), otherwise the calculated cabeceras are updated in place so per-module edits survive; then the design is
        /// assembled and resolved again. False with <see cref="LastError"/> when it cannot run; the last valid system and design stay.
        /// </summary>
        public bool Recompose(bool restoreStandard = false)
        {
            LastError = null;

            if (!TryValidateFronts(out var error) || !TryReadPallet(out var pallet, out error))
            {
                LastError = error;
                return false;
            }

            DynamicDepthLayout depthLayout;
            try
            {
                depthLayout = DynamicDepthGeometry.Resolve(
                    Matrix.Fronts.Select(row => new DynamicRackFrontDesign
                    {
                        PalletsDeep = row.PalletsDeep,
                        DepthStartPosition = row.DepthStartPosition,
                    }),
                    _palletsDeepTotal);
            }
            catch (ArgumentException ex)
            {
                LastError = ex.Message;
                return false;
            }

            var levels = Matrix.MaxLoadLevels();
            var firstRow = Matrix.Fronts[0];
            firstRow.EnsureCellCount(firstRow.LoadLevels);
            var firstLevel = firstRow.FirstLevelHeight;
            var beamDepth = firstRow.Cells[0].InOutBeamDepth;

            if (PostPeralte <= 0.0)
            {
                LastError = "El peralte de poste debe ser un número mayor que cero.";
                return false;
            }

            if (ManualHeaderHeight.HasValue && ManualHeaderHeight.Value <= 0.0)
            {
                LastError = "Altura manual inválida (debe ser mayor que cero).";
                return false;
            }

            try
            {
                var palletsDeep = depthLayout.TotalPositions;
                beamDepth = DynamicLoadBeamGeometry.ResolveBeamDepth(Catalog, DynamicRackDefaults.InOutBeamCatalogId, beamDepth);
                var totalDepth = palletsDeep * pallet.Depth + 2.0 * DynamicRackDefaults.HeaderEndAllowance;
                var derived = DynamicHeaderHeightCalculator.Calculate(pallet.Height, levels, firstLevel, beamDepth, totalDepth);
                var headerHeight = ManualHeaderHeight ?? derived.HeaderHeight;

                // The recomposition works on a copy of the last valid system (the system IS the resolution of the last valid design), so
                // the system already published, or handed to AutoCAD, is never mutated, and a recomposition that fails leaves it whole.
                var system = WorkingCopy();
                var mustRebuild = restoreStandard || DynamicEditorDesignAssembler.MustRebuild(system, pallet, depthLayout);
                string report = null;
                if (mustRebuild)
                {
                    var rebuild = DynamicRackRebuild.Rebuild(
                        system,
                        _headerBatch,
                        new DynamicRackRebuildRequest
                        {
                            Pallet = pallet,
                            DepthLayout = depthLayout,
                            HeaderPostCatalogId = PostCatalogId,
                            HeaderHeight = headerHeight,
                            PostPeralte = PostPeralte,
                            LoadLevels = levels,
                            FirstLevelHeight = firstLevel,
                            BeamDepth = beamDepth,
                            RestoreStandard = restoreStandard,
                        },
                        _builder,
                        _resolver);
                    system = rebuild.System;
                    report = rebuild.Describe();
                }
                else
                {
                    _assembler.UpdateHeaderHeightInPlace(system, headerHeight, PostCatalogId);
                }

                _builder.ApplyPostPeralte(system, PostPeralte);
                system.SeparatorCountOverride = SeparatorCountOverride.HasValue && SeparatorCountOverride.Value >= 1 ? SeparatorCountOverride : null;
                system.SeparatorSpacingOverride = SeparatorSpacingOverride.HasValue && SeparatorSpacingOverride.Value > 0.0 ? SeparatorSpacingOverride : null;
                system.DerivedPostReinforced = DerivedPostReinforced;
                system.DerivedPostReinforcementHeight = DerivedPostReinforcementHeight.HasValue && DerivedPostReinforcementHeight.Value > 0.0
                    ? DerivedPostReinforcementHeight
                    : null;
                system.ManualHeaderHeightOverride = ManualHeaderHeight;

                var design = _assembler.BuildDesign(
                    system,
                    Matrix,
                    levels,
                    firstLevel,
                    beamDepth,
                    PostCatalogId,
                    palletsDeep,
                    PostPeralte,
                    DynamicRackDefaults.DefaultPalletTolerance,
                    Annotations,
                    SafetySelections);
                var resolution = _resolver.Resolve(design);
                System = resolution.System;
                System.Name = Name?.Trim();
                Design = design;
                Matrix.RefreshFromResolved(System.Fronts);
                _palletsDeepTotal = palletsDeep;
                DerivedHeight = derived;
                HeaderHeight = System.ManualHeaderHeightOverride ?? resolution.Height.HeaderHeight;
                LastRebuildReport = report;
                return true;
            }
            catch (Exception ex)
            {
                // The last valid system and design stay: the recomposition worked on a copy.
                LastError = "No se pudo generar el sistema: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// The editor put the inputs back as they were at the last valid recomposition (the rollback of a change that could not be
        /// recomposed): its system and design are the current ones again, so the error of the refused change no longer applies (I-09 of the
        /// third review). It recomposes nothing, so a design opened and not touched stays as it was read.
        /// </summary>
        public void InputsRestoredToLastValid() => LastError = null;

        /// <summary>
        /// Copies of the design and of the system of the last valid recomposition, for «Actualizar» (I-64 A-14, line D): the design goes
        /// through its persisted JSON (what the drawing stores) and the system is resolved again from that copy, exactly as the published
        /// one was resolved from the design. Nothing the editor does afterwards reaches the copies. Null before the first valid
        /// recomposition. The name is trimmed, as the classic editor sends it.
        /// </summary>
        public DynamicDraftSnapshot SnapshotForWrite()
        {
            if (Design == null) return null;

            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForDynamic(Design));
            var design = store.Deserialize(json)?.DynamicDesign;
            if (design == null) return null;

            var name = Name?.Trim();
            var system = _resolver.Resolve(design).System;
            system.Name = name;
            return new DynamicDraftSnapshot(json, name, design, system);
        }

        // A fresh copy of the last valid system: the resolution of the last valid design, with the name; null before the first one.
        private DynamicRackSystem WorkingCopy()
        {
            if (System == null || Design == null) return System;
            var copy = _resolver.Resolve(Design).System;
            copy.Name = System.Name;
            return copy;
        }

        // ------------------------------------------------------------------ the checks of the classic editor, with its messages

        private bool TryValidateFronts(out string error)
        {
            error = null;
            if (Matrix.Count == 0)
            {
                error = "Se requiere al menos un frente.";
                return false;
            }

            foreach (var row in Matrix.Fronts)
            {
                if (row.PalletCount < 1)
                {
                    error = "Cada frente requiere al menos una posición de tarima.";
                    return false;
                }

                if (row.LoadLevels < 1)
                {
                    error = "Cada frente requiere al menos un nivel de carga.";
                    return false;
                }

                if (row.PalletsDeep < 2 || row.DepthStartPosition < 1)
                {
                    error = "Cada frente requiere al menos 2 fondos y una posición inicial >= 1.";
                    return false;
                }

                if (row.FirstLevelHeight < 0.0)
                {
                    error = "El inicio del primer larguero debe ser mayor o igual que cero.";
                    return false;
                }

                row.EnsureCellCount(row.LoadLevels);
                if (row.Cells.Take(row.LoadLevels).Any(cell => !cell.IsValid))
                {
                    error = "Revisa los valores de tarima, claro y largueros de cada celda.";
                    return false;
                }
            }

            return true;
        }

        // The pallet of the rack: the fondo of the whole rack and the front, height and weight of the first cell of the first front, as
        // the classic editor reads them.
        private bool TryReadPallet(out PalletSpecification pallet, out string error)
        {
            pallet = null;
            error = null;
            if (PalletDepth <= 0.0)
            {
                error = "Fondo inválido.";
                return false;
            }

            if (_palletsDeepTotal < 2)
            {
                error = "Las tarimas de fondo deben ser un entero >= 2.";
                return false;
            }

            var firstRow = Matrix.Fronts[0];
            firstRow.EnsureCellCount(firstRow.LoadLevels);
            var firstCell = firstRow.Cells[0];
            pallet = new PalletSpecification(firstCell.PalletFront, PalletDepth, firstCell.PalletHeight, firstCell.PalletWeight, "kg");
            return true;
        }

        /// <summary>The width of the post in the catalog, or the default peralte of the Dinámico: the rule of the classic editor.</summary>
        public static double DefaultPostPeralte(RackCatalog catalog, string postId)
        {
            var width = catalog?.PostProfiles?.FirstOrDefault(profile => string.Equals(
                profile?.Id,
                postId,
                StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : DynamicRackDefaults.DefaultPostPeralte;
        }
    }
}
