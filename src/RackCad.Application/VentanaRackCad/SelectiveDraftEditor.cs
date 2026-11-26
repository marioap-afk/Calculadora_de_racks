using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// What «Actualizar» hands to AutoCAD for a Selectivo: copies of the design (and its JSON) and of the system, which share no object (a
    /// reader of the system, such as the cuts, never changes the design that is written), the trimmed name and the
    /// final state of every linked property, which the drawing half of RACKEDITAR reconciles onto the authored document so a binding the
    /// user did not touch is kept exactly as it was read.
    /// </summary>
    public sealed record SelectiveDraftSnapshot(
        string DesignJson,
        string Name,
        SelectivePalletDesign Design,
        SelectiveRackSystem System,
        IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyFinalStates);

    /// <summary>
    /// The outcome of one write of the draft: how many cells, fronts or fondos it wrote, how many of the asked ones do not exist in a fondo
    /// of the scope (a front that only exists in one fondo), or why it wrote nothing at all.
    /// </summary>
    public sealed record SelectiveDraftEdit(int Written, int Omitted, string Error)
    {
        public bool Refused => Error != null;

        public static SelectiveDraftEdit Refuse(string error) => new SelectiveDraftEdit(0, 0, error);

        /// <summary>What the editor tells the user: the reason of a refusal, or what was not written because it does not exist there.</summary>
        public string Notice => Error ?? (Omitted <= 0
            ? null
            : Written > 0
                ? "Parte de lo elegido no existe en todos los fondos: se escribió solo donde existe."
                : "Lo elegido no existe en los fondos de la vista: no se escribió nada.");
    }

    /// <summary>
    /// A value read over the fondos of the scope (decision 2 of the Owner on Selectivo: what you see is where you write). It varies when
    /// two instances differ; <see cref="Missing"/> counts the asked positions that a fondo of the scope does not have.
    /// </summary>
    public readonly struct SelectiveScopedValue<T>
    {
        public SelectiveScopedValue(int count, int missing, bool varies, T value)
        {
            Count = count;
            Missing = missing;
            Varies = varies;
            Value = value;
        }

        /// <summary>How many instances were read.</summary>
        public int Count { get; }

        /// <summary>How many asked instances do not exist in a fondo of the scope.</summary>
        public int Missing { get; }

        /// <summary>True when two instances differ («Varía»).</summary>
        public bool Varies { get; }

        /// <summary>The common value; the default when it varies or when nothing was read.</summary>
        public T Value { get; }

        public bool HasValue => Count > 0 && !Varies;
    }

    /// <summary>One front of one fondo as the draft holds it (read only; the segments are copies).</summary>
    public sealed record SelectiveDraftFront(
        int Fondo,
        int Front,
        int LevelCount,
        bool FloorBeam,
        double FloorBeamRise,
        double? HeightOverride,
        IReadOnlyList<SelectiveSegment> Segments)
    {
        /// <summary>The front is split in tramos («medio frente»).</summary>
        public bool IsSplit => Segments != null && Segments.Count > 0;
    }

    /// <summary>One fondo (a line in depth) as the draft holds it.</summary>
    public sealed record SelectiveDraftFondo(int Fondo, int FrontCount, double PalletDepth, double? CabeceraDepthOverride, double CabeceraDepth);

    /// <summary>
    /// The automatic values next to the manual ones (the approved draft: «Automático: …», «Calculada: ≈ …»): the distinct larguero lengths
    /// and front heights the resolver gives with those manual values emptied (<see cref="SelectiveDraftEditor.AutomaticValues"/>).
    /// </summary>
    public sealed record SelectiveAutomaticValues(IReadOnlyList<double> BeamLengths, IReadOnlyList<double> Heights);

    /// <summary>
    /// The counts of «Calculado» of the window (the approved draft: «Cabeceras», «Largueros», «Separadores»), read from the BOM of the
    /// system (<see cref="SelectiveBomBuilder.Build"/>) and never counted again here: the cabeceras (one per frame, the intermediate ones
    /// of a half front included), the largueros (front and back of every beam) and the separators between fondos, as the BOM lists them.
    /// The BOM imposes the depth of its fondo on the custom cabeceras of the system it reads: the window counts through
    /// <see cref="SelectiveDraftEditor.BomCounts"/>, on a system of its own.
    /// </summary>
    public sealed record SelectiveBomCounts(int Cabeceras, int Largueros, int Separadores)
    {
        /// <summary>The counts of the BOM of that system; zeros for no system.</summary>
        public static SelectiveBomCounts Of(SelectiveRackSystem system, RackCatalog catalog)
        {
            if (system == null) return new SelectiveBomCounts(0, 0, 0);
            var components = SelectiveBomBuilder.Build(system, catalog).Components;
            int Sum(string category) => components.Where(c => string.Equals(c.Category, category, StringComparison.Ordinal)).Sum(c => c.Quantity);
            return new SelectiveBomCounts(Sum(RackCad.Application.Bom.BomBuilder.Cabecera), Sum(RackCad.Application.Systems.Larguero.LargueroBomBuilder.Larguero),
                Sum(SelectiveBomBuilder.Separador));
        }
    }

    /// <summary>
    /// The draft of ONE Selectivo rack in the RackCad window (I-64 A-14, line D, Selectivo E1-A), with no WPF and no AutoCAD. It is the SAME
    /// recomposition as the classic editor (<c>RackSelectiveWindow</c>): the editor state of I-20 (<see cref="SelectiveEditorState"/>,
    /// one matrix per fondo), the <see cref="SelectiveDesignInputs"/> the classic editor reads from its boxes, the assembly of
    /// <see cref="SelectiveEditorState.BuildDesign"/> and the <see cref="SelectiveGeometryResolver"/>; the writes go through the same
    /// operations of the state the classic editor uses (I-43: fondo targets, front operations, cells reached by
    /// <see cref="SelectiveTargetResolver"/>). It adds no geometry of its own.
    ///
    /// <para>
    /// The fondo the classic editor is «editing» is always fondo 0 here, and its slot is committed after every write, so every read comes
    /// from the slots (the stale slot of I-20 never leaks out). Where the classic editor has «Editando fondo» and «Fondos destino», the draft
    /// has ONE scope (decision 2): all the fondos, or fondo n. What it reads over the scope says «varía» when the fondos differ, and a front
    /// that only one fondo has is reported as missing in the other.
    /// </para>
    /// <para>
    /// Declared differences with the classic editor, each with its test (like D-04 of Dinámico): (1) a value keeps its full precision,
    /// where the classic editor rounds the post peralte and the separations to three decimals through its boxes; (2) a post that the
    /// catalog does not have gives way to the first post of the list, as in the classic editor, but the draft says so, and a catalog post
    /// written with other capitals is that post (with the catalog of today, one post, the classic list ends on the same post). The
    /// holguras (decision 12): the window does not offer «=Variable», so a linked holgura
    /// opens with its effective value and is read only (<see cref="IsLinked"/>); the final states keep its binding exactly as it was read,
    /// as the classic editor does when the user does not touch it. The work of <c>LinkedPropertyEditor</c> is not touched.
    /// </para>
    /// <para>
    /// A recomposition that cannot run says why in <see cref="LastError"/> and keeps the last valid design and system; they are never the
    /// objects the state holds, so nothing edited afterwards reaches them, and the system never shares an object with the design, so no
    /// reader of the system changes the design (I-1 of the review of the entrega 2). Nothing here writes the drawing.
    /// </para>
    /// </summary>
    public sealed class SelectiveDraftEditor
    {
        private const string AutomaticDimensionStyle = "(Automático)";

        private static readonly PropertyId Tolerance = ProjectPropertyIds.SelectivePalletTolerance;
        private static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;

        private readonly SelectiveEditorState _state = new SelectiveEditorState();
        private readonly SelectiveGeometryResolver _resolver = new SelectiveGeometryResolver();
        private readonly SelectivePalletDesignStore _store = new SelectivePalletDesignStore();
        private readonly List<double> _separators = new List<double>();
        private readonly List<SelectiveSafetySelection> _safety = new List<SelectiveSafetySelection>();
        private readonly Dictionary<PropertyId, LinkedPropertyEditState> _linkedStates = new Dictionary<PropertyId, LinkedPropertyEditState>();
        private readonly Dictionary<PropertyId, double> _linkedEffective = new Dictionary<PropertyId, double>();
        private readonly Dictionary<PropertyId, string> _linkedNames = new Dictionary<PropertyId, string>();

        // The run-wide floor-beam rise a loaded document carried: written again only for older readers (I-43, gate 8A).
        private double _legacyFloorBeamRise = SelectiveRackDefaults.DefaultFloorBeamRise;

        // The dimension views as loaded: the choice of the user keeps its unknown bits (I-50, ADR-0035).
        private DimensionViewVisibility? _loadedDimensionViews;

        public SelectiveDraftEditor(RackCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

            // The defaults of the classic editor's constructor: the first larguero of its list, 2 fronts × 4 levels, one fondo of 48 in.
            _state.DefaultBeamId = BeamChoices.FirstOrDefault()?.Id?.Trim();
            _state.InitMatrix(2, 4);
            _state.FondoMatrices.Clear();
            _state.FondoMatrices.Add(_state.SnapshotWorking(SelectiveRackDefaults.DefaultPalletDepth, 0.0));
            _state.SelectedFondo = 0;
            _state.MaterializeFloorBeamRises(_legacyFloorBeamRise);
            _state.SyncPostCabeceras();

            PostId = ChoosePost(catalog.Defaults?.Post, out _);
            var defaults = new SelectivePalletDesign();
            PalletTolerance = defaults.PalletTolerance;
            VerticalClearance = defaults.VerticalClearance;
            _linkedStates[Clearance] = LinkedPropertyEditState.Literal(defaults.VerticalClearance);
            _linkedStates[Tolerance] = LinkedPropertyEditState.Literal(defaults.PalletTolerance);
            AimAtScope();
        }

        // ------------------------------------------------------------------ the rack-wide inputs

        public RackCatalog Catalog { get; }

        /// <summary>The name of the rack (trimmed when written).</summary>
        public string Name { get; set; }

        /// <summary>The post of the rack, by its catalog id.</summary>
        public string PostId { get; set; }

        /// <summary>«Peralte poste», in inches.</summary>
        public double PostPeralte { get; set; } = 3.0;

        /// <summary>The horizontal tolerance, in inches. Read only while a project variable or a formula governs it (<see cref="IsLinked"/>).</summary>
        public double PalletTolerance { get; set; }

        /// <summary>The vertical clearance, in inches. Read only while a project variable or a formula governs it (<see cref="IsLinked"/>).</summary>
        public double VerticalClearance { get; set; }

        public bool DrawBasePlate { get; set; } = true;

        public bool NumberFronts { get; set; }

        public bool NumberLevels { get; set; }

        public bool DrawRackName { get; set; }

        /// <summary>«Mostrar tarimas»: a visual reference in the frontal only; not in the BOM (decision 10).</summary>
        public bool DrawPallets { get; set; }

        /// <summary>The annotation scale (1 = 6"); a value that is not positive is written as 1, as the classic editor does.</summary>
        public double AnnotationScale { get; set; } = 1.0;

        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;

        /// <summary>The dimension views that are written: the loaded policy, exact, until <see cref="SetDimensionViews"/> chooses them.</summary>
        public DimensionViewVisibility? DimensionViews { get; set; }

        /// <summary>The dimension style; blank (or «(Automático)») is the automatic one.</summary>
        public string DimensionStyle { get; set; }

        /// <summary>
        /// The views chosen by the user: the policy of the classic editor, keeping the unknown bits of the loaded one. Boxes that are back to
        /// what the loaded policy shows are «not touched» (<see cref="DimensionViewPolicy.FromEditor"/>), so the value is the loaded one,
        /// exactly: a legacy rack (no policy) unticked and ticked again stays with no policy (S-22, M-05 of the review of the entrega 1).
        /// </summary>
        public void SetDimensionViews(bool frontal, bool lateral, bool planta)
        {
            var touched = frontal != Shows(_loadedDimensionViews, DimensionViewKind.Frontal)
                          || lateral != Shows(_loadedDimensionViews, DimensionViewKind.Lateral)
                          || planta != Shows(_loadedDimensionViews, DimensionViewKind.Planta);
            DimensionViews = DimensionViewPolicy.FromEditor(_loadedDimensionViews, touched, frontal, lateral, planta);
        }

        /// <summary>Whether that type of view draws dimensions with the current policy (the rule of ADR-0035, as the classic boxes show it).</summary>
        public bool ShowsDimensions(DimensionViewKind kind) => Shows(DimensionViews, kind);

        private static bool Shows(DimensionViewVisibility? views, DimensionViewKind kind)
            => DimensionViewPolicy.EffectiveDetail(DimensionDetail.Minimal, views, kind) != DimensionDetail.None;

        /// <summary>The posts of the catalog, in the order of the classic list.</summary>
        public IReadOnlyList<ProfileCatalogEntry> PostChoices => Choices(Catalog.PostProfiles);

        /// <summary>The largueros of the catalog, in the order of the classic list.</summary>
        public IReadOnlyList<BeamProfileCatalogEntry> BeamChoices => Choices(Catalog.BeamProfiles);

        /// <summary>The peraltes the catalog allows for a larguero, as the list of the classic cell editor offers them.</summary>
        public IReadOnlyList<double> BeamPeraltes(string beamId)
        {
            var raw = Catalog.BeamProfiles?.FirstOrDefault(b => string.Equals(b?.Id?.Trim(), beamId?.Trim(), StringComparison.OrdinalIgnoreCase))?.Peraltes;
            return PeralteList.Parse(raw);
        }

        // ------------------------------------------------------------------ the holguras (decision 12)

        /// <summary>
        /// True when a project variable or a formula governs that holgura. The window does not offer «=Variable», so it shows the effective
        /// value read only; the final states keep the binding as it was read.
        /// </summary>
        public bool IsLinked(PropertyId property)
            => _linkedStates.TryGetValue(property, out var state) && state != null && state.Source.Kind != LinkedPropertySourceKind.Literal;

        public bool PalletToleranceIsLinked => IsLinked(Tolerance);

        public bool VerticalClearanceIsLinked => IsLinked(Clearance);

        /// <summary>Why that holgura is read only in the window; null when it belongs to the rack.</summary>
        public string LinkedReadOnlyReason(PropertyId property)
        {
            if (!IsLinked(property)) return null;
            if (_linkedStates[property].IsExpression) return "La gobierna una fórmula del dibujo: en esta ventana es de solo lectura.";
            return _linkedNames.TryGetValue(property, out var name) && !string.IsNullOrWhiteSpace(name)
                ? "La gobierna la variable de proyecto «" + name.Trim() + "»: en esta ventana es de solo lectura."
                : "La gobierna una variable de proyecto: en esta ventana es de solo lectura.";
        }

        /// <summary>
        /// The final state of every linked property for the design of the last valid recomposition: a holgura of the rack is the literal it
        /// has now; a linked one is the state that was read, untouched, so its binding and its frozen literal survive the write.
        /// </summary>
        public IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyFinalStates => FinalStatesFor(Design);

        // ------------------------------------------------------------------ the fondos, the matrix and the scope

        /// <summary>How many fondos (lines in depth) the rack has.</summary>
        public int FondoCount => Math.Max(1, _state.FondoMatrices.Count);

        /// <summary>The scope of the writes and of the «varía» reads: null = all the fondos (where the window starts), else that fondo.</summary>
        public int? FondoScope { get; private set; }

        /// <summary>The fondos of the scope, ascending.</summary>
        public IReadOnlyList<int> ScopeFondos => FondoScope is int fondo ? new[] { fondo } : Enumerable.Range(0, FondoCount).ToArray();

        /// <summary>Sets the scope: null for all the fondos, or a fondo the rack has (another one gives all the fondos).</summary>
        public void SetFondoScope(int? fondo)
        {
            FondoScope = fondo is int f && f >= 0 && f < FondoCount ? f : (int?)null;
            AimAtScope();
        }

        /// <summary>
        /// Runs <paramref name="body"/> (a read or a write of the draft) with that scope, whatever the scope of the view is, and puts the
        /// scope of the view back afterwards, also when the body throws. The «Rack» tab of the window writes the whole rack through it (null,
        /// every fondo: the same write as «Todo» of the matrix with «Todos los fondos»), and the table «Fondos» one fondo. A fondo the rack
        /// no longer has (the body changed the count) leaves the view on all the fondos, as <see cref="SetFondoScope"/> does.
        /// </summary>
        public T WithinScope<T>(int? fondo, Func<T> body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            var view = FondoScope;
            SetFondoScope(fondo);
            try
            {
                return body();
            }
            finally
            {
                SetFondoScope(view);
            }
        }

        /// <summary>Every position (front, level) that some fondo of the scope has: what «Todo» of the matrix selects.</summary>
        public IReadOnlyList<SelectiveMatrixPosition> ScopePositions()
        {
            var positions = new List<SelectiveMatrixPosition>();
            for (var front = 0; front < ScopeFrontCount; front++)
                for (var level = 0; level < ScopeLevelCount(front); level++)
                    positions.Add(new SelectiveMatrixPosition(front, level));
            return positions;
        }

        /// <summary>
        /// What the drawing would have if the manual values of those cells and fronts were emptied («vacío = automático», «vacía =
        /// calculada»), over the fondos of the scope: the larguero length of the front of each cell (the widest level of the front governs,
        /// a manual length of another cell included) and the height of each front. It is the SAME resolver over a copy of the last valid
        /// design with those manual values removed, so no rule of the geometry is repeated here. Distinct values, ascending, with three
        /// decimals; empty before the first valid recomposition.
        /// </summary>
        public SelectiveAutomaticValues AutomaticValues(IEnumerable<SelectiveMatrixPosition> cells, IEnumerable<int> fronts)
        {
            var empty = new SelectiveAutomaticValues(Array.Empty<double>(), Array.Empty<double>());
            if (Design == null) return empty;
            var design = Copy(Design);
            if (design == null) return empty;

            var askedCells = (cells ?? Enumerable.Empty<SelectiveMatrixPosition>()).Distinct().ToList();
            var askedFronts = (fronts ?? Enumerable.Empty<int>()).Distinct().ToList();
            foreach (var fondo in ScopeFondos)
            {
                var bays = DesignBays(design, fondo);
                if (bays == null) continue;
                foreach (var p in askedCells)
                {
                    if (p.FrontIndex < 0 || p.FrontIndex >= bays.Count) continue;
                    var levels = bays[p.FrontIndex].Levels;
                    if (p.LevelIndex >= 0 && p.LevelIndex < levels.Count) levels[p.LevelIndex].BeamLengthOverride = null;
                }

                foreach (var front in askedFronts)
                {
                    if (front >= 0 && front < bays.Count) bays[front].HeightOverride = null;
                }
            }

            var system = _resolver.Resolve(design, Catalog);
            var lengths = new List<double>();
            var heights = new List<double>();
            foreach (var fondo in ScopeFondos)
            {
                var resolved = SelectiveDepthLayout.BaysOfFondo(system, fondo);
                if (resolved == null) continue;
                foreach (var p in askedCells)
                {
                    if (p.FrontIndex >= 0 && p.FrontIndex < resolved.Count && p.LevelIndex >= 0 && p.LevelIndex < LevelCount(fondo, p.FrontIndex))
                        lengths.Add(resolved[p.FrontIndex].BeamLength);
                }

                foreach (var front in askedFronts)
                {
                    if (front >= 0 && front < resolved.Count) heights.Add(resolved[front].Height);
                }
            }

            return new SelectiveAutomaticValues(Distinct(lengths), Distinct(heights));
        }

        /// <summary>
        /// The counts of «Calculado» (<see cref="SelectiveBomCounts"/>) of the last valid design, counted on a system of their own
        /// (<see cref="ResolveCopy"/>). The BOM imposes the depth of its fondo on every custom cabecera it reads, in place
        /// (<see cref="SelectiveCabeceraAuthority.EffectiveCustomAt"/>; ARQ-43-10, deferred). Since I-1 of the review of the entrega 2 that can
        /// no longer reach <see cref="Design"/> (the system is resolved from a copy of its own); counting apart also leaves
        /// <see cref="System"/> as the resolver gave it (I-01 of the review of the polish of the entrega 1). Zeros before the first valid
        /// recomposition.
        /// </summary>
        public SelectiveBomCounts BomCounts() => SelectiveBomCounts.Of(ResolveCopy(), Catalog);

        /// <summary>
        /// A system resolved from a COPY of the last valid design, for a reader that keeps it or imposes on what it reads: the BOM and the
        /// plans of the safety (the depth of its fondo on every custom cabecera, ARQ-43-10) and the classic dialog «Elementos de seguridad»,
        /// which keeps the system it is given. Neither <see cref="Design"/> (whose system never shares an object with it, I-1 of the review of
        /// the entrega 2) nor <see cref="System"/> is touched by what they do. Null before the first valid recomposition.
        /// </summary>
        public SelectiveRackSystem ResolveCopy()
        {
            var design = Copy(Design);
            return design == null ? null : _resolver.Resolve(design, Catalog);
        }

        /// <summary>
        /// The fondo whose fronts give their width to the same fronts of every fondo: the one with the most fronts, the first on a tie
        /// (<see cref="SelectiveDepthLayout.MasterFondoIndex"/>, the master of the resolver). A manual larguero length of a cell of another
        /// fondo is kept and written, but the drawing does not use it (M-02 of the review of the polish). The resolver only lets another fondo
        /// govern a front where the master has an empty column, which the draft never has (every front keeps at least one level, and a load
        /// pads an empty one). 0 before the first valid recomposition.
        /// </summary>
        public int WidthMasterFondo => System == null ? 0 : SelectiveDepthLayout.MasterFondoIndex(System);

        // The design bays of a fondo, as the resolver reads them (a fondo with no own bays uses those of fondo 0).
        private static IList<SelectiveBayDesign> DesignBays(SelectivePalletDesign design, int fondo)
        {
            if (fondo <= 0) return design.Bays;
            var index = fondo - 1;
            return index < design.ExtraFondoBays.Count && design.ExtraFondoBays[index] != null && design.ExtraFondoBays[index].Count > 0
                ? design.ExtraFondoBays[index]
                : design.Bays;
        }

        private static IReadOnlyList<double> Distinct(IEnumerable<double> values)
            => values.Select(v => Math.Round(v, 3)).Distinct().OrderBy(v => v).ToList();

        public int FrontCount(int fondo) => Slot(fondo)?.Bays.Count ?? 0;

        public int LevelCount(int fondo, int front)
        {
            var slot = Slot(fondo);
            return slot != null && front >= 0 && front < slot.Bays.Count ? slot.Bays[front].Count : 0;
        }

        /// <summary>The most fronts a fondo of the scope has (the columns of the matrix of the window).</summary>
        public int ScopeFrontCount => ScopeFondos.Select(FrontCount).DefaultIfEmpty(0).Max();

        /// <summary>The most levels that front has in a fondo of the scope (the rows of its column).</summary>
        public int ScopeLevelCount(int front) => ScopeFondos.Select(fondo => LevelCount(fondo, front)).DefaultIfEmpty(0).Max();

        /// <summary>A copy of a cell; null when that fondo does not have it.</summary>
        public SelectiveEditorCell Cell(int fondo, int front, int level)
        {
            var slot = Slot(fondo);
            if (slot == null || front < 0 || front >= slot.Bays.Count) return null;
            var column = slot.Bays[front];
            return level >= 0 && level < column.Count ? column[level].Clone() : null;
        }

        /// <summary>A front of a fondo; null when that fondo does not have it.</summary>
        public SelectiveDraftFront Front(int fondo, int front)
        {
            var slot = Slot(fondo);
            if (slot == null || front < 0 || front >= slot.Bays.Count) return null;
            var segments = front < slot.BaySegments.Count ? slot.BaySegments[front] : null;
            return new SelectiveDraftFront(
                fondo,
                front,
                slot.Bays[front].Count,
                front < slot.FloorBeams.Count && slot.FloorBeams[front],
                (front < slot.FloorBeamRiseOverrides.Count ? slot.FloorBeamRiseOverrides[front] : null) ?? _legacyFloorBeamRise,
                front < slot.BayHeights.Count ? slot.BayHeights[front] : null,
                (segments ?? new List<SelectiveSegment>()).Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }).ToList());
        }

        public SelectiveDraftFondo Fondo(int fondo)
        {
            var slot = Slot(fondo);
            if (slot == null) return null;
            return new SelectiveDraftFondo(
                fondo,
                slot.Bays.Count,
                slot.Depth > 0.0 ? slot.Depth : SelectiveRackDefaults.DefaultPalletDepth,
                slot.CabeceraOverride > 0.0 ? slot.CabeceraOverride : (double?)null,
                _state.CabeceraDepthOfFondo(fondo));
        }

        /// <summary>The value of those cells over the fondos of the scope (a copy of each cell is read).</summary>
        public SelectiveScopedValue<T> ReadCells<T>(IEnumerable<SelectiveMatrixPosition> positions, Func<SelectiveEditorCell, T> read, IEqualityComparer<T> comparer = null)
        {
            var asked = (positions ?? Enumerable.Empty<SelectiveMatrixPosition>()).Distinct().ToList();
            return Scoped(ScopeFondos.SelectMany(fondo => asked.Select(p => Cell(fondo, p.FrontIndex, p.LevelIndex))), read, comparer);
        }

        /// <summary>The value of those fronts over the fondos of the scope.</summary>
        public SelectiveScopedValue<T> ReadFronts<T>(IEnumerable<int> fronts, Func<SelectiveDraftFront, T> read, IEqualityComparer<T> comparer = null)
        {
            var asked = (fronts ?? Enumerable.Empty<int>()).Distinct().ToList();
            return Scoped(ScopeFondos.SelectMany(fondo => asked.Select(front => Front(fondo, front))), read, comparer);
        }

        /// <summary>The value of the fondos of the scope.</summary>
        public SelectiveScopedValue<T> ReadFondos<T>(Func<SelectiveDraftFondo, T> read, IEqualityComparer<T> comparer = null)
            => Scoped(ScopeFondos.Select(Fondo), read, comparer);

        /// <summary>The separation of each gap between consecutive fondos, in inches (none with one fondo).</summary>
        public IReadOnlyList<double> Separators => _separators.ToList();

        /// <summary>How many posts the rack has (the fronts of its longest fondo plus one).</summary>
        public int PostCount => _state.PostPeraltes.Count;

        /// <summary>The own peralte of a post; null when it takes the peralte of the rack.</summary>
        public double? PostPeralteOverride(int post)
            => post >= 0 && post < _state.PostPeraltes.Count && _state.PostPeraltes[post] > 0.0 ? _state.PostPeraltes[post] : (double?)null;

        /// <summary>Whether that post of that fondo carries a custom cabecera (they are kept; they are edited in the classic editor).</summary>
        public bool HasCustomCabecera(int fondo, int post) => _state.CabeceraAt(fondo, post) != null;

        /// <summary>E2-B «Restaurar layout…»: the custom cabecera of that post goes back to the standard one in the fondos of the scope
        /// (the classic «Restablecer poste», without its peralte, which <see cref="SetPostPeralte"/> restores).</summary>
        public SelectiveDraftEdit ResetCustomCabecera(int post)
        {
            if (post < 0 || post >= _state.PostPeraltes.Count) return SelectiveDraftEdit.Refuse("Ese poste no existe.");
            AimAtScope();
            var result = _state.ApplyCabeceraToTargets(post, null, configuration => configuration);
            return new SelectiveDraftEdit(result.AppliedFondos.Count, result.OmittedFondos.Count, null);
        }

        // ------------------------------------------------------------------ writes over the scope (the caller recomposes)

        /// <summary>«Número de fondos» of the rack: a new fondo copies fondo 0, the fondos beyond the count are dropped, as in the classic editor.</summary>
        public SelectiveDraftEdit SetFondoCount(int count)
        {
            if (count < 1) return SelectiveDraftEdit.Refuse("Número de fondos inválido (mínimo 1).");
            count = Math.Min(SelectiveRackDefaults.MaxDepthCount, count);
            if (count == FondoCount) return new SelectiveDraftEdit(0, 0, null);

            var matrices = _state.FondoMatrices;
            var first = matrices[0];
            while (matrices.Count < count) matrices.Add(_state.CloneAligned(first, first.Bays.Count, first));
            while (matrices.Count > count) matrices.RemoveAt(matrices.Count - 1);

            // The separations follow the classic fields: the gaps that stay keep their value, a new gap takes the last one.
            var current = _separators.ToList();
            _separators.Clear();
            for (var gap = 0; gap < count - 1; gap++) _separators.Add(SelectiveDepthLayout.Separator(current, gap));

            _state.SyncTargetFondos();
            Commit();
            _state.SyncPostCabeceras();
            if (FondoScope is int f && f >= count) FondoScope = null;
            AimAtScope();
            return new SelectiveDraftEdit(1, 0, null);
        }

        /// <summary>«Frentes» of every fondo of the scope: each one is resized on its own; a new front copies the last one.</summary>
        public SelectiveDraftEdit SetFrontCount(int fronts)
        {
            if (fronts < 1) return SelectiveDraftEdit.Refuse("Cantidad de frentes inválida (mínimo 1).");
            AimAtScope();
            var result = _state.ApplyBayCountToTargets(fronts);
            _state.MaterializeFloorBeamRises(_legacyFloorBeamRise); // the fronts that just appeared get a direct value
            Commit();
            _state.SyncPostCabeceras();
            return new SelectiveDraftEdit(result.AppliedFondos.Count, result.OmittedFondos.Count, null);
        }

        /// <summary>«Profundidad de tarima» of the fondos of the scope; their custom cabeceras follow it.</summary>
        public SelectiveDraftEdit SetPalletDepth(double depth)
        {
            if (!(depth > 0.0) || double.IsInfinity(depth)) return SelectiveDraftEdit.Refuse("Profundidad de tarima inválida.");
            AimAtScope();
            var result = _state.ApplyPalletDepthToTargets(depth);
            Commit();
            return new SelectiveDraftEdit(result.AppliedFondos.Count, result.OmittedFondos.Count, null);
        }

        /// <summary>«Profundidad de cabecera» of the fondos of the scope; null goes back to the rule (tarima − 6").</summary>
        public SelectiveDraftEdit SetCabeceraDepth(double? depth)
        {
            if (depth.HasValue && (!(depth.Value > 0.0) || double.IsInfinity(depth.Value)))
                return SelectiveDraftEdit.Refuse("Profundidad de cabecera inválida (vacía = la de la tarima menos 6\").");
            AimAtScope();
            var result = _state.ApplyCabeceraDepthToTargets(depth);
            Commit();
            return new SelectiveDraftEdit(result.AppliedFondos.Count, result.OmittedFondos.Count, null);
        }

        /// <summary>The separation of one gap between consecutive fondos (zero-based).</summary>
        public SelectiveDraftEdit SetSeparator(int gap, double value)
        {
            if (gap < 0 || gap >= _separators.Count) return SelectiveDraftEdit.Refuse("Esa separación no existe.");
            if (!(value > 0.0) || double.IsInfinity(value)) return SelectiveDraftEdit.Refuse("Separación inválida (debe ser mayor que cero).");
            _separators[gap] = value;
            return new SelectiveDraftEdit(1, 0, null);
        }

        public SelectiveDraftEdit SetPalletFront(IEnumerable<SelectiveMatrixPosition> cells, double frente)
            => !(frente > 0.0) || double.IsInfinity(frente)
                ? SelectiveDraftEdit.Refuse("Frente de tarima inválido.")
                : WriteCells(cells, null, cell => cell.Frente = frente);

        public SelectiveDraftEdit SetPalletHeight(IEnumerable<SelectiveMatrixPosition> cells, double alto)
            => !(alto > 0.0) || double.IsInfinity(alto)
                ? SelectiveDraftEdit.Refuse("Alto de tarima inválido.")
                : WriteCells(cells, null, cell => cell.Alto = alto);

        public SelectiveDraftEdit SetPalletCount(IEnumerable<SelectiveMatrixPosition> cells, int count)
            => count < 1
                ? SelectiveDraftEdit.Refuse("Tarimas por nivel inválido.")
                : WriteCells(cells, null, cell => cell.PalletCount = count);

        /// <summary>
        /// The larguero of those cells. As the list of the classic cell editor does, a peralte the new larguero does not allow becomes its
        /// first allowed one.
        /// </summary>
        public SelectiveDraftEdit SetBeam(IEnumerable<SelectiveMatrixPosition> cells, string beamId)
        {
            var beam = Catalog.BeamProfiles?.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b?.Id)
                && string.Equals(b.Id.Trim(), beamId?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (beam == null) return SelectiveDraftEdit.Refuse(string.IsNullOrWhiteSpace(beamId) ? "Selecciona un larguero." : "El larguero «" + beamId.Trim() + "» no está en el catálogo.");

            var id = beam.Id.Trim();
            var allowed = BeamPeraltes(id);
            return WriteCells(cells, null, cell =>
            {
                cell.BeamId = id;
                if (allowed.Count > 0 && !allowed.Any(p => Shown(p) == Shown(cell.BeamPeralte))) cell.BeamPeralte = allowed[0];
            });
        }

        /// <summary>The peralte of the larguero of those cells: only a value the catalog allows for the larguero of every cell.</summary>
        public SelectiveDraftEdit SetBeamPeralte(IEnumerable<SelectiveMatrixPosition> cells, double peralte)
        {
            if (!(peralte > 0.0) || double.IsInfinity(peralte)) return SelectiveDraftEdit.Refuse("Selecciona un peralte de larguero.");
            return WriteCells(
                cells,
                cell =>
                {
                    var allowed = BeamPeraltes(cell.BeamId);
                    if (allowed.Count == 0) return "El larguero «" + cell.BeamId + "» no tiene peraltes en el catálogo.";
                    return allowed.Any(p => Shown(p) == Shown(peralte))
                        ? null
                        : "El larguero «" + cell.BeamId + "» no admite un peralte de " + Shown(peralte) + " in en el catálogo.";
                },
                cell => cell.BeamPeralte = peralte);
        }

        /// <summary>The manual length of the larguero; null = automatic.</summary>
        public SelectiveDraftEdit SetBeamLengthOverride(IEnumerable<SelectiveMatrixPosition> cells, double? length)
            => length.HasValue && (!(length.Value > 0.0) || double.IsInfinity(length.Value))
                ? SelectiveDraftEdit.Refuse("Longitud de larguero inválida (deja vacío para auto).")
                : WriteCells(cells, null, cell => cell.BeamLength = length);

        /// <summary>The manual clear under the larguero; null = automatic (the resolver fits it to the troquel).</summary>
        public SelectiveDraftEdit SetClearOverride(IEnumerable<SelectiveMatrixPosition> cells, double? clear)
            => clear.HasValue && (!(clear.Value > 0.0) || double.IsInfinity(clear.Value))
                ? SelectiveDraftEdit.Refuse("Claro inválido (deja vacío para auto).")
                : WriteCells(cells, null, cell => cell.Clear = clear);

        /// <summary>«Larguero a piso» of those fronts; their elevations stay as they are.</summary>
        public SelectiveDraftEdit SetFloorBeam(IEnumerable<int> fronts, bool floorBeam)
            => WriteFronts(fronts, front => _state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, front, floorBeam));

        /// <summary>The elevation of the floor larguero of those fronts (a direct value of each front; 0 is a value).</summary>
        public SelectiveDraftEdit SetFloorBeamRise(IEnumerable<int> fronts, double rise)
            => !(rise >= 0.0) || double.IsInfinity(rise)
                ? SelectiveDraftEdit.Refuse("Elevación del larguero a piso inválida (número ≥ 0).")
                : WriteFronts(fronts, front => _state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, front, rise));

        /// <summary>The manual height of those fronts (decision 9: <c>HeightOverride</c>); null = calculated.</summary>
        public SelectiveDraftEdit SetHeightOverride(IEnumerable<int> fronts, double? height)
            => height.HasValue && (!(height.Value > 0.0) || double.IsInfinity(height.Value))
                ? SelectiveDraftEdit.Refuse("Altura manual inválida (vacía = calculada).")
                : WriteFronts(fronts, front => _state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, front, height));

        /// <summary>The exact number of levels of those fronts: growing copies the top level, shrinking drops the tail.</summary>
        public SelectiveDraftEdit SetLevelCount(IEnumerable<int> fronts, int levels)
            => levels < 1
                ? SelectiveDraftEdit.Refuse("Número de niveles inválido (mínimo 1).")
                : WriteFronts(fronts, front => _state.ApplyLevelCountToTargets(SelectiveFrontApplyScope.Front, front, levels));

        /// <summary>
        /// The tramos of «medio frente» of one front, in every fondo of the scope (decision 7; the checks of the classic dialog): fewer than
        /// two tramos is a whole front; every tramo but the last needs a length, and they cannot reach the whole front.
        /// </summary>
        public SelectiveDraftEdit SetSegments(int front, IEnumerable<SelectiveSegment> segments)
        {
            var working = (segments ?? Enumerable.Empty<SelectiveSegment>()).Where(s => s != null)
                .Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }).ToList();
            if (working.Count < 2)
            {
                working.Clear();
            }
            else
            {
                for (var i = 0; i < working.Count - 1; i++)
                {
                    if (!(working[i].Length > 0.0))
                        return SelectiveDraftEdit.Refuse("El tramo " + (i + 1).ToString(CultureInfo.InvariantCulture) + " necesita una longitud mayor que 0 (el último se calcula solo).");
                }

                var full = FrontWidth(front);
                var specified = working.Take(working.Count - 1).Sum(s => s.Length);
                if (full > 0.0 && specified >= full)
                {
                    return SelectiveDraftEdit.Refuse(string.Format(CultureInfo.InvariantCulture,
                        "Los tramos ({0:0.#} in) no pueden igualar ni superar el frente completo ({1:0.#} in).", specified, full));
                }
            }

            AimAtScope();
            var result = _state.ApplySegmentsToTargets(front, working);
            Commit();
            return new SelectiveDraftEdit(result.Count, result.OmittedFondos.Count, null);
        }

        /// <summary>The whole width of a front in the last valid system (the first fondo of the scope that has it); 0 when none.</summary>
        public double FrontWidth(int front)
        {
            if (System == null) return 0.0;
            foreach (var fondo in ScopeFondos)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(System, fondo);
                if (bays != null && front >= 0 && front < bays.Count) return bays[front].BeamLength;
            }

            return 0.0;
        }

        /// <summary>
        /// The own peralte of a post (the rule of the classic box): null takes the peralte of the rack, and a value equal to it is stored as
        /// «takes the peralte of the rack», so it follows it if it changes.
        /// </summary>
        public SelectiveDraftEdit SetPostPeralte(int post, double? peralte)
        {
            if (post < 0 || post >= _state.PostPeraltes.Count) return SelectiveDraftEdit.Refuse("Ese poste no existe.");
            if (peralte.HasValue && (!(peralte.Value > 0.0) || double.IsInfinity(peralte.Value)))
                return SelectiveDraftEdit.Refuse("Peralte de poste inválido (deja vacío para usar el peralte del rack).");

            var value = peralte ?? 0.0;
            if (value > 0.0 && Math.Abs(value - PostPeralte) < 1e-6) value = 0.0;
            _state.PostPeraltes[post] = value;
            return new SelectiveDraftEdit(1, 0, null);
        }

        // ------------------------------------------------------------------ safety (kept; edited in entrega 2)

        /// <summary>Copies of the safety of the draft.</summary>
        public IReadOnlyList<SelectiveSafetySelection> CopySafety() => _safety.Select(s => s.DeepCopy()).ToList();

        /// <summary>The safety of the draft becomes copies of these selections (only those that draw or count, as in the classic editor).</summary>
        public void ReplaceSafety(IEnumerable<SelectiveSafetySelection> selections)
        {
            var copies = (selections ?? Enumerable.Empty<SelectiveSafetySelection>()).Where(Draws).Select(s => s.DeepCopy()).ToList();
            _safety.Clear();
            _safety.AddRange(copies);
        }

        // ------------------------------------------------------------------ what the last recomposition produced

        /// <summary>The last valid design (a copy the state does not hold); null before the first valid recomposition.</summary>
        public SelectivePalletDesign Design { get; private set; }

        /// <summary>
        /// The system resolved from a copy of <see cref="Design"/> of its own: it shares no object with the design (nor with the state), so
        /// no reader of it can change the design that is written, whatever it imposes on what it reads (I-1 of the review of the entrega 2).
        /// Null before the first valid recomposition.
        /// </summary>
        public SelectiveRackSystem System { get; private set; }

        /// <summary>Why the last recomposition did not run; null when it ran.</summary>
        public string LastError { get; private set; }

        /// <summary>What the last load had to change to open the design (a post the catalog does not have, an empty front); null when nothing.</summary>
        public string LoadNotice { get; private set; }

        // ------------------------------------------------------------------ operations

        /// <summary>A new rack with the defaults of the classic editor, already recomposed.</summary>
        public static SelectiveDraftEditor CreateNew(RackCatalog catalog)
        {
            var editor = new SelectiveDraftEditor(catalog);
            editor.Recompose();
            return editor;
        }

        /// <summary>
        /// Loads the EFFECTIVE design that opens a rack of the drawing (what <see cref="SelectiveEditorOpen"/> gives RACKEDITAR), with the
        /// committed state of every linked property and the name, as the classic editor loads it (<c>LoadExisting</c>), and recomposes,
        /// as the classic load ends with a recomputation: a rack opened and written without a change is written as the classic editor would
        /// write it. The design is copied first, so nothing the draft does reaches the caller's design. Variable names are for display only.
        /// </summary>
        public bool LoadFrom(
            SelectivePalletDesign effective,
            IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> linkedPropertyStates,
            string name,
            IReadOnlyDictionary<PropertyId, string> linkedVariableNames = null)
        {
            if (effective == null) throw new ArgumentNullException(nameof(effective));

            Name = name;
            Design = null;
            System = null;
            LoadState(effective);

            // A domain design IS the unbound case; the states read from the authored document come on top (I-48 G4C): their literal is
            // the frozen one, so writing without a change keeps it.
            _linkedStates.Clear();
            _linkedEffective.Clear();
            _linkedNames.Clear();
            _linkedStates[Clearance] = LinkedPropertyEditState.Literal(VerticalClearance);
            _linkedStates[Tolerance] = LinkedPropertyEditState.Literal(PalletTolerance);
            foreach (var pair in linkedPropertyStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>())
            {
                if (pair.Value != null) _linkedStates[pair.Key] = pair.Value;
            }

            _linkedEffective[Clearance] = VerticalClearance;
            _linkedEffective[Tolerance] = PalletTolerance;
            foreach (var pair in linkedVariableNames ?? new Dictionary<PropertyId, string>())
            {
                _linkedNames[pair.Key] = pair.Value;
            }

            return Recompose();
        }

        /// <summary>
        /// Recomposes the rack from the current state with the rules of the classic editor (its checks and messages, the assembly of
        /// <see cref="SelectiveEditorState.BuildDesign"/> and the resolver). False with <see cref="LastError"/> when it cannot run; the last
        /// valid design and system stay.
        /// </summary>
        public bool Recompose()
        {
            LastError = null;

            // The classic list only holds posts of the catalog: a post outside it cannot be chosen there.
            var posts = PostChoices;
            if (string.IsNullOrWhiteSpace(PostId)
                || (posts.Count > 0 && !posts.Any(p => string.Equals(p.Id.Trim(), PostId.Trim(), StringComparison.OrdinalIgnoreCase))))
            {
                LastError = "Selecciona un poste.";
                return false;
            }

            if (!(PostPeralte > 0.0) || double.IsInfinity(PostPeralte))
            {
                LastError = "Peralte de poste inválido.";
                return false;
            }

            if (!CheckHolgura(Tolerance, PalletTolerance, "Tolerancia horizontal", out var error)
                || !CheckHolgura(Clearance, VerticalClearance, "Holgura vertical", out error))
            {
                LastError = error;
                return false;
            }

            try
            {
                var built = _state.BuildDesign(Inputs());
                if (built == null)
                {
                    LastError = "Define frentes y niveles.";
                    return false;
                }

                // The published design is a copy through its document, never the objects of the state (the custom cabeceras are shared
                // with the state, and a later depth change imposes itself on them). The system is resolved from ANOTHER copy: the resolver
                // hands the custom cabeceras of the design to the system as they are, and the readers of the drawing (the BOM, the cuts,
                // the planta) impose the depth of its fondo on them in place (ARQ-43-10), so a system sharing them with the design would
                // let any reader of System change the design that is written (I-1 of the review of the entrega 2).
                var design = Copy(built);
                var system = _resolver.Resolve(Copy(design), Catalog);
                if (system.Height <= 0.0)
                {
                    LastError = "No se pudo derivar la geometría (revisa tarima/niveles).";
                    return false;
                }

                system.Name = Name?.Trim();
                Design = design;
                System = system;
                return true;
            }
            catch (Exception ex)
            {
                LastError = "No se pudo generar el sistema: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Puts every input back as it was at the last valid recomposition (the rollback of a change that could not be recomposed), and
        /// clears the error of the refused change (I-09 of Dinámico). The linked states and the scope stay. False when there was never a
        /// valid recomposition.
        /// </summary>
        public bool RestoreLastValid()
        {
            if (Design == null) return false;
            var notice = LoadNotice;
            LoadState(Design);
            LoadNotice = notice; // what the opening had to change still holds
            LastError = null;
            return true;
        }

        /// <summary>
        /// Copies for «Actualizar»: the design through its JSON, the system resolved again from ANOTHER copy of that JSON, the trimmed name
        /// and the final states of the linked properties for that design. Nothing the editor does afterwards reaches them, and the design
        /// and the system share no object: RACKSELECTIVOINSERTAR and RACKSELECTIVONUEVO read the cuts of the system before they reconcile
        /// the design, and the cuts impose the depth of its fondo on the custom cabeceras they read (M-1 of the review of the entrega 2), so
        /// what AutoCAD writes is <see cref="SelectiveDraftSnapshot.DesignJson"/>. Null before the first valid recomposition.
        /// </summary>
        public SelectiveDraftSnapshot SnapshotForWrite()
        {
            if (Design == null) return null;

            var json = _store.Serialize(SelectivePalletDesignDocument.From(Design, null, null));
            var design = _store.Deserialize(json)?.ToDomain();
            var source = _store.Deserialize(json)?.ToDomain();
            if (design == null || source == null) return null;

            var name = Name?.Trim();
            var system = _resolver.Resolve(source, Catalog);
            system.Name = name;
            return new SelectiveDraftSnapshot(json, name, design, system, FinalStatesFor(design));
        }

        // ------------------------------------------------------------------ the load and the inputs of the classic editor

        // The classic LoadDesign, on a private copy (the load coerces the depth of the custom cabeceras of fondo 0 and the state keeps them).
        private void LoadState(SelectivePalletDesign source)
        {
            // The fronts are checked BEFORE the copy: the copy goes through the store, which refuses a design with no fronts (S-29, M-03 of
            // the review of the entrega 1). The classic editor keeps what it had in that case; so does the draft, and it says so.
            if (source == null || source.Bays.Count == 0)
            {
                LoadNotice = "El diseño leído no tiene frentes: el editor conserva el rack que tenía.";
                return;
            }

            var design = Copy(source);
            var notices = new List<string>();

            PostId = ChoosePost(design.PostId, out var postNotice);
            if (postNotice != null) notices.Add(postNotice);
            PostPeralte = design.PostPeralte;
            PalletTolerance = design.PalletTolerance;
            VerticalClearance = design.VerticalClearance;
            // 0 is a legitimate rise ("no elevation at all"), so only a NEGATIVE value falls back to the default.
            _legacyFloorBeamRise = design.FloorBeamRise >= 0.0 ? design.FloorBeamRise : SelectiveRackDefaults.DefaultFloorBeamRise;

            var matrices = _state.FondoMatrices;
            matrices.Clear();
            matrices.Add(_state.FondoMatrixFromDesignBays(design.Bays, out var padded));
            var depthCount = Math.Max(1, design.DepthCount);
            for (var k = 1; k < depthCount; k++)
            {
                var extra = (k - 1) < design.ExtraFondoBays.Count ? design.ExtraFondoBays[k - 1] : null;
                if (extra != null && extra.Count > 0)
                {
                    matrices.Add(_state.FondoMatrixFromDesignBays(extra, out var morePadded));
                    padded += morePadded;
                }
                else
                {
                    matrices.Add(_state.CloneAligned(matrices[0], matrices[0].Bays.Count, matrices[0]));
                }
            }

            var baseDepth = design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            matrices[0].Depth = baseDepth;
            for (var k = 1; k < matrices.Count; k++)
            {
                var over = (k - 1) < design.ExtraFondoDepths.Count ? design.ExtraFondoDepths[k - 1] : 0.0;
                matrices[k].Depth = over > 0.0 ? over : baseDepth;
            }

            for (var k = 0; k < matrices.Count; k++)
            {
                matrices[k].CabeceraOverride = k < design.CabeceraFondoOverrides.Count ? design.CabeceraFondoOverrides[k] : 0.0;
            }

            _state.SelectedFondo = 0;
            _state.RestoreWorkingFrom(matrices[0]);
            _state.MaterializeFloorBeamRises(_legacyFloorBeamRise);

            _separators.Clear();
            for (var gap = 0; gap < depthCount - 1; gap++) _separators.Add(SelectiveDepthLayout.Separator(design.SeparatorLengths, gap));

            // A custom cabecera of fondo 0 obeys the rule of its fondo (cabecera = tarima − 6", or the override), as the classic load coerces it.
            _state.PostCabeceras.Clear();
            var loadedOverride = design.CabeceraFondoOverrides.Count > 0 ? design.CabeceraFondoOverrides[0] : 0.0;
            var loadedCabeceraFondo = loadedOverride > 0.0 ? loadedOverride : baseDepth - SelectiveRackDefaults.CabeceraFondoAllowance;
            if (loadedCabeceraFondo <= 0.0) loadedCabeceraFondo = baseDepth;
            foreach (var cabecera in design.PostCabeceras)
            {
                if (cabecera != null) cabecera.Depth = loadedCabeceraFondo;
                _state.PostCabeceras.Add(cabecera);
            }

            _state.ExtraFondoPostCabeceras.Clear();
            for (var k = 1; k < matrices.Count; k++)
            {
                var stored = (k - 1) < design.ExtraFondoPostCabeceras.Count ? design.ExtraFondoPostCabeceras[k - 1] : null;
                _state.ExtraFondoPostCabeceras.Add(stored == null ? new List<RackFrameConfiguration>() : new List<RackFrameConfiguration>(stored));
            }

            _state.PostPeraltes.Clear();
            _state.PostPeraltes.AddRange(design.PostPeraltes);

            DrawBasePlate = design.DrawBasePlate;
            NumberFronts = design.NumberFronts;
            NumberLevels = design.NumberLevels;
            DrawRackName = design.DrawRackName;
            DrawPallets = design.DrawPallets;
            AnnotationScale = design.AnnotationScale > 0.0 ? design.AnnotationScale : 1.0;
            Dimensions = design.Dimensions >= DimensionDetail.None && design.Dimensions <= DimensionDetail.Detailed ? design.Dimensions : DimensionDetail.None;
            _loadedDimensionViews = design.DimensionViews;
            DimensionViews = design.DimensionViews;
            DimensionStyle = design.DimensionStyle;

            ReplaceSafety(design.SafetySelections);

            _state.SelBay = 0;
            _state.SelLevel = 0;
            _state.ClampSelection();
            _state.SyncPostCabeceras();
            if (FondoScope is int f && f >= matrices.Count) FondoScope = null;
            AimAtScope();

            if (padded > 0)
            {
                notices.Add(padded.ToString(CultureInfo.InvariantCulture)
                    + " frente(s) vacío(s) (columna) del diseño se cargaron con un nivel por defecto: el editor aún no maneja columnas; revisa antes de escribir.");
            }

            LoadNotice = notices.Count == 0 ? null : string.Join(" ", notices);
        }

        // What the classic BuildDesign reads from its boxes, from the committed state (INV-13: never a pending text).
        private SelectiveDesignInputs Inputs()
        {
            var slot = _state.FondoMatrices.Count > 0 ? _state.FondoMatrices[0] : null;
            var depth = slot != null && slot.Depth > 0.0 ? slot.Depth : SelectiveRackDefaults.DefaultPalletDepth;
            var style = string.IsNullOrWhiteSpace(DimensionStyle) || DimensionStyle.Trim() == AutomaticDimensionStyle ? null : DimensionStyle.Trim();
            return new SelectiveDesignInputs
            {
                PostId = PostId.Trim(),
                PostPeralte = PostPeralte,
                PalletTolerance = PalletTolerance,
                VerticalClearance = VerticalClearance,
                FloorBeamRise = _legacyFloorBeamRise, // legacy field only: every front carries its own value
                Fondo = depth,
                DepthCount = Math.Max(1, _state.FondoMatrices.Count),
                WorkingDepth = depth,
                WorkingCabeceraOverride = slot?.CabeceraOverride ?? 0.0,
                Separators = _separators.Select(value => value > 0.0 ? value : SelectiveRackDefaults.DefaultSeparator).ToList(),
                DrawBasePlate = DrawBasePlate,
                NumberFronts = NumberFronts,
                NumberLevels = NumberLevels,
                DrawRackName = DrawRackName,
                DrawPallets = DrawPallets,
                AnnotationScale = AnnotationScale > 0.0 ? AnnotationScale : 1.0,
                Dimensions = Dimensions >= DimensionDetail.None && Dimensions <= DimensionDetail.Detailed ? Dimensions : DimensionDetail.None,
                DimensionViews = DimensionViews,
                DimensionStyle = style,
                SafetySelections = _safety.Where(Draws).Select(s => s.DeepCopy()).ToList(),
            };
        }

        private bool CheckHolgura(PropertyId property, double value, string label, out string error)
        {
            error = null;
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.0)
            {
                error = label + " inválida.";
                return false;
            }

            if (IsLinked(property) && _linkedEffective.TryGetValue(property, out var effective) && value != effective)
            {
                error = label + ": " + LinkedReadOnlyReason(property);
                return false;
            }

            return true;
        }

        private IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> FinalStatesFor(SelectivePalletDesign design)
        {
            var states = new Dictionary<PropertyId, LinkedPropertyEditState>();
            foreach (var pair in _linkedStates)
            {
                if (pair.Value.Source.Kind != LinkedPropertySourceKind.Literal)
                {
                    states[pair.Key] = pair.Value;
                    continue;
                }

                var value = pair.Key == Clearance ? design?.VerticalClearance ?? VerticalClearance
                    : pair.Key == Tolerance ? design?.PalletTolerance ?? PalletTolerance
                    : pair.Value.CommittedLiteral;
                states[pair.Key] = LinkedPropertyEditState.Literal(value);
            }

            return states;
        }

        // ------------------------------------------------------------------ helpers

        // The slot of a fondo: authoritative, because the live matrix (fondo 0) is committed after every write.
        private SelectiveEditorFondoMatrix Slot(int fondo)
            => fondo >= 0 && fondo < _state.FondoMatrices.Count ? _state.FondoMatrices[fondo] : null;

        private void Commit()
        {
            var slot = _state.FondoMatrices.Count > 0 ? _state.FondoMatrices[0] : null;
            _state.SaveWorkingToSelected(slot?.Depth ?? SelectiveRackDefaults.DefaultPalletDepth, slot?.CabeceraOverride ?? 0.0);
        }

        private void AimAtScope()
        {
            if (FondoScope is int fondo && fondo >= 0 && fondo < FondoCount) _state.SetTargetFondos(new[] { fondo });
            else
            {
                FondoScope = null;
                _state.FollowAllFondos();
            }
        }

        // The cells of the scope reached by the write authority of I-43 (the positions projected on every target fondo), checked first and
        // then written, so a refused write writes nothing.
        private SelectiveDraftEdit WriteCells(IEnumerable<SelectiveMatrixPosition> positions, Func<SelectiveEditorCell, string> check, Action<SelectiveEditorCell> write)
        {
            AimAtScope();
            var plan = SelectiveTargetResolver.Resolve(
                SelectiveTopology.From(_state),
                _state.TargetFondos,
                SelectiveApplyScope.Selected,
                new SelectiveCellAddress(0, 0, 0),
                (positions ?? Enumerable.Empty<SelectiveMatrixPosition>()).ToList());
            var cells = plan.Targets.Select(_state.CellAt).Where(cell => cell != null).ToList();
            if (check != null)
            {
                foreach (var cell in cells)
                {
                    var error = check(cell);
                    if (error != null) return SelectiveDraftEdit.Refuse(error);
                }
            }

            foreach (var cell in cells) write(cell);
            Commit();
            return new SelectiveDraftEdit(cells.Count, plan.OmittedCells.Count, null);
        }

        private SelectiveDraftEdit WriteFronts(IEnumerable<int> fronts, Func<int, SelectiveFrontApplyResult> apply)
        {
            AimAtScope();
            var written = 0;
            var omitted = 0;
            foreach (var front in (fronts ?? Enumerable.Empty<int>()).Distinct().OrderBy(index => index))
            {
                var result = apply(front);
                written += result.Count;
                omitted += result.OmittedFondos.Count;
            }

            Commit();
            return new SelectiveDraftEdit(written, omitted, null);
        }

        private static SelectiveScopedValue<TValue> Scoped<TItem, TValue>(IEnumerable<TItem> items, Func<TItem, TValue> read, IEqualityComparer<TValue> comparer)
            where TItem : class
        {
            comparer ??= EqualityComparer<TValue>.Default;
            var count = 0;
            var missing = 0;
            var varies = false;
            var first = default(TValue);
            foreach (var item in items)
            {
                if (item == null)
                {
                    missing++;
                    continue;
                }

                var value = read(item);
                if (count == 0) first = value;
                else if (!comparer.Equals(first, value)) varies = true;
                count++;
            }

            return new SelectiveScopedValue<TValue>(count, missing, varies, varies ? default : first);
        }

        // The post as the classic list chooses it: one of the catalog. A catalog post in another spelling is that post (declared
        // difference: the classic list matches it exactly); a post the catalog does not have gives way to the first post of the list.
        private string ChoosePost(string postId, out string notice)
        {
            notice = null;
            var choices = PostChoices;
            var first = choices.FirstOrDefault()?.Id?.Trim();
            if (string.IsNullOrWhiteSpace(postId)) return first ?? postId;

            var known = choices.FirstOrDefault(p => string.Equals(p.Id.Trim(), postId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (known != null) return known.Id.Trim();
            if (choices.Count == 0) return postId;

            notice = "El poste «" + postId + "» no está en el catálogo: se usa «" + first + "», como en el editor clásico.";
            return first;
        }

        // The options of a classic list (UiSupport.ToOptions): one per id, ordered by their label.
        private static IReadOnlyList<T> Choices<T>(IEnumerable<T> entries) where T : CatalogEntryBase
            => (entries ?? Enumerable.Empty<T>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(entry => entry.Label, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

        // A selection draws or counts: a quantity, a side by default, or a post that draws (the rule of the classic editor and of the resolver).
        private static bool Draws(SelectiveSafetySelection selection)
            => selection != null && !string.IsNullOrWhiteSpace(selection.ElementId)
               && (selection.Quantity > 0 || selection.Side != SafetySide.None || (selection.PostSides?.Any(p => p != null && p.Side != SafetySide.None) ?? false));

        private SelectivePalletDesign Copy(SelectivePalletDesign design)
            => design == null ? null : _store.Deserialize(_store.Serialize(SelectivePalletDesignDocument.From(design, null, null)))?.ToDomain();

        private static string Shown(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
