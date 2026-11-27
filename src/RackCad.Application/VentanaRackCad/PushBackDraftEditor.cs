using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>What «Actualizar» of a Push Back hands to AutoCAD: copies of the design (and its JSON) and of the system, and the trimmed name.</summary>
    public sealed record PushBackDraftSnapshot(string DesignJson, string Name, PushBackDesign Design, PushBackSystem System);

    /// <summary>
    /// The beds of ONE cell of a two-sided Push Back (question 3 of the prototype «pushback»: one list that joins the topology and the
    /// direction of the run): «Encontradas (dos camas)», «Solo A», «Solo B», «Corrida A → B» and «Corrida B → A».
    /// </summary>
    public enum PushBackCellBeds
    {
        Encontradas = 0,
        SoloA = 1,
        SoloB = 2,
        CorridaAToB = 3,
        CorridaBToA = 4,
    }

    /// <summary>The boxes of «Restaurar layout…» of a Push Back (question 6 of the prototype): each part of the layout it gives back.</summary>
    public sealed record PushBackLayoutRestore(
        bool ModuleLengths = true,
        bool CustomHeaders = true,
        bool RackParameters = true,
        bool CellFondos = true,
        bool BeamLengths = true,
        bool SideStructure = true)
    {
        /// <summary>Nothing is marked.</summary>
        public bool IsEmpty => !ModuleLengths && !CustomHeaders && !RackParameters && !CellFondos && !BeamLengths && !SideStructure;
    }

    /// <summary>One module of the longitudinal sequence of the rack, as «Avanzado» › «Módulos» shows it.</summary>
    public sealed record PushBackModuleRow(string ModuleId, string Label, string Kind, double Start, double Length, bool IsManualLength, bool IsHeader,
        bool IsCustomHeader, bool CanEditLength);

    /// <summary>
    /// The draft of ONE Push Back rack in the RackCad window (I-64, fifth batch, part «pb»): the editable state and its recomposition, with
    /// no WPF and no AutoCAD. It does not add a model of its own: the state is <see cref="PushBackEditableState"/> (the grouping of the
    /// state of the classic editor that the full draft of the pilot already uses: both sides, the composite intent, the parked tail, the
    /// module authority and the rack-wide values), and a recomposition is the pass of the classic window as product code
    /// (<see cref="PushBackEditableStateSeat.Pass"/>). A new rack is the sequence of the classic window when it opens (LoadNew, pass,
    /// default safety, pass), and a loaded one the sequence of RACKEDITAR (<see cref="PushBackEditableStateSeat.LoadExisting"/>), so the
    /// design this draft writes is the one the classic editor would write with the same gestures.
    ///
    /// What the window adds are the gestures of its inspector, over the cells and fronts of the selection and the sides the user writes
    /// («Lado A», «Lado B» or «Ambos lados»), and two rules of the prototype: a module length goes to the draft at once (question 7, no
    /// «Confirmar»), and «Restaurar layout…» gives back only the parts marked (question 6). A gesture that cannot be recomposed is taken
    /// back by the editor of the window with <see cref="Capture"/> and <see cref="Restore"/>: the state is a deep copy, so nothing is half
    /// written. Nothing here writes the drawing.
    /// </summary>
    public sealed partial class PushBackDraftEditor
    {
        private static readonly double[] ClassicRearPeraltes = { 3.0, 3.5, 4.0, 4.5, 5.0, 5.5, 6.0 };

        private readonly PushBackEditorDesignAssembler _assembler;
        private readonly PushBackCompositeEditorAssembler _composite;

        public PushBackDraftEditor(RackCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _assembler = new PushBackEditorDesignAssembler(catalog);
            _composite = new PushBackCompositeEditorAssembler(catalog);
        }

        // ------------------------------------------------------------------ the state

        public RackCatalog Catalog { get; }

        /// <summary>The editable state of the rack (both sides and the rack-wide values).</summary>
        public PushBackEditableState State { get; private set; } = new PushBackEditableState();

        /// <summary>The composite editor state: the two sides, the side being edited, the topology of the cells.</summary>
        public PushBackCompositeEditorState Composite => State.Editor;

        /// <summary>The last valid recomposition (design, system, BOM and plans); null before the first one.</summary>
        public PushBackEditorComputation Computation { get; private set; }

        /// <summary>The persisted design of the last valid recomposition: what «Actualizar» writes.</summary>
        public PushBackDesign Design => Computation?.Design;

        /// <summary>The resolved system of the last valid recomposition.</summary>
        public PushBackSystem System => Computation?.System;

        /// <summary>The name of the rack (trimmed when it is sent).</summary>
        public string Name
        {
            get => State.Name;
            set => State.Name = value;
        }

        /// <summary>Why the last recomposition did not run; null when it ran.</summary>
        public string LastError { get; private set; }

        /// <summary>The rack has a side B (two-way).</summary>
        public bool IsTwoSided => Composite.SideBPresent;

        /// <summary>The output is blocked by a diagnostic of the composite (the gate of the classic editor: no insert, no update, no library).</summary>
        public bool IsOutputBlocked => System != null && PushBackEditorOutputGate.IsBlocked(Composite, System);

        /// <summary>The first diagnostic that blocks, else the first warning; null when there is none.</summary>
        public string OutputStatus => System == null ? null : PushBackEditorOutputGate.StatusOr(Composite, System, null);

        /// <summary>The rear peraltes the catalog admits for the rear beam (the list of the classic editor when the catalog has none).</summary>
        public IReadOnlyList<double> AllowedRearPeraltes()
        {
            var allowed = _assembler.AllowedHighEndPeraltes();
            return allowed != null && allowed.Count > 0 ? allowed : ClassicRearPeraltes;
        }

        // ------------------------------------------------------------------ loads and recomposition

        /// <summary>A new rack, as the classic window opens: LoadNew, a pass, the default safety and a pass. Recomposed.</summary>
        public static PushBackDraftEditor CreateNew(RackCatalog catalog)
        {
            var editor = new PushBackDraftEditor(catalog);
            var state = new PushBackEditableState();
            state.LoadNew(catalog);
            var first = PushBackEditableStateSeat.Pass(state, editor._assembler, editor._composite, PushBackSide.A);
            state.SeedDefaultSafety(catalog);
            var second = PushBackEditableStateSeat.Pass(state, editor._assembler, editor._composite, PushBackSide.A);
            editor.State = state;
            editor.Adopt(second ?? first);
            return editor;
        }

        /// <summary>
        /// Loads a persisted design as RACKEDITAR opens it (<see cref="PushBackEditableStateSeat.LoadExisting"/>: the new window, then the
        /// design, each followed by its pass). False, with <see cref="LastError"/>, when the design cannot be recomposed; the state is then
        /// the loaded one, with no valid system.
        /// </summary>
        public bool LoadFrom(PushBackDesign design, string name)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));

            // The sequence of LoadExisting, which keeps the computation of its last pass (the Seat gives the state only). A test ties the two.
            var state = new PushBackEditableState();
            state.LoadNew(Catalog);
            PushBackEditableStateSeat.Pass(state, _assembler, _composite, PushBackSide.A);
            state.SeedDefaultSafety(Catalog);
            PushBackEditableStateSeat.Pass(state, _assembler, _composite, PushBackSide.A);
            state.LoadDesign(design, name, _assembler.Resolver);
            var computation = PushBackEditableStateSeat.Pass(state, _assembler, _composite, PushBackSide.A);
            State = state;
            Computation = null;
            return Adopt(computation);
        }

        /// <summary>
        /// Recomposes the rack from the current state with the pass of the classic window. False with <see cref="LastError"/> when it cannot
        /// run; the last valid computation stays (the caller takes the state back with <see cref="Restore"/>).
        /// </summary>
        public bool Recompose()
        {
            var computation = PushBackEditableStateSeat.Pass(State, _assembler, _composite, PushBackSide.A);
            if (computation == null)
            {
                // The pass contains the exception, as the window does; the assembly says why on a copy that nobody keeps.
                LastError = "No se pudo generar el sistema: " + (WhyTheAssemblyFails() ?? "sin detalle.");
                return false;
            }

            return Adopt(computation);
        }

        /// <summary>The editor of the window put the state back as it was at the last valid recomposition: its error no longer applies.</summary>
        public void InputsRestoredToLastValid() => LastError = null;

        /// <summary>A deep copy of the state, to take a refused gesture back (<see cref="Restore"/>).</summary>
        public PushBackEditableState Capture() => PushBackEditableStateComparer.Copy(State);

        /// <summary>The state becomes a copy taken with <see cref="Capture"/>; the last valid computation stays.</summary>
        public void Restore(PushBackEditableState copy)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            State = copy;
        }

        /// <summary>The persisted JSON of the design of the last valid recomposition (what the drawing stores); null before the first one.</summary>
        public string DesignJson() => Design == null ? null : new RackProjectStore().Serialize(RackProject.ForPushBack(Design));

        /// <summary>
        /// Copies of the design and of the system of the last valid recomposition, for «Actualizar»: the design goes through its persisted
        /// JSON and the system is resolved again from that copy, as the classic editor resolved it from the design. Nothing the editor does
        /// afterwards reaches the copies. Null before the first valid recomposition. The name is trimmed.
        /// </summary>
        public PushBackDraftSnapshot SnapshotForWrite()
        {
            var json = DesignJson();
            if (json == null) return null;
            var design = new RackProjectStore().Deserialize(json)?.PushBackDesign;
            if (design == null) return null;
            var name = Name?.Trim();
            var copy = _assembler.Resolver.Resolve(design);
            copy.Name = name;
            return new PushBackDraftSnapshot(json, name, design, copy);
        }

        private bool Adopt(PushBackEditorComputation computation)
        {
            if (computation == null)
            {
                LastError = "No se pudo generar el sistema: " + (WhyTheAssemblyFails() ?? "sin detalle.");
                return false;
            }

            if (!computation.IsValid)
            {
                LastError = "No se pudo generar el sistema: " + (string.IsNullOrWhiteSpace(computation.Error) ? "sin detalle." : computation.Error);
                return false;
            }

            Computation = computation;
            LastError = null;
            return true;
        }

        // The message of a failed assembly, on a COPY of the state (the pass already contained the failure and left the state for the caller).
        private string WhyTheAssemblyFails()
        {
            try
            {
                var copy = PushBackEditableStateComparer.Copy(State);
                _assembler.BuildFrom(_composite.BuildDesign(copy.Editor, copy.BuildInputs(_assembler)), PushBackSide.A);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // ------------------------------------------------------------------ sides and shape

        /// <summary>The sides a write reaches with the selection of the matrix: the active one, or both with «Ambos lados».</summary>
        public IReadOnlyList<PushBackSide> EditSides() => Composite.EditSides();

        /// <summary>The sides the rack has: A, and B when it is two-way («Lo esencial» writes them all).</summary>
        public IReadOnlyList<PushBackSide> Sides() => IsTwoSided ? new[] { PushBackSide.A, PushBackSide.B } : new[] { PushBackSide.A };

        /// <summary>The state of one side.</summary>
        public PushBackEditorState Side(PushBackSide side) => Composite.Of(side);

        /// <summary>The transverse slots of the rack (the fronts; with a side B, the larger of the two).</summary>
        public int SlotCount => Math.Max(1, Composite.SlotCount);

        /// <summary>The levels of a front of a side (0 when the side does not have that front).</summary>
        public int LevelsAt(PushBackSide side, int front)
        {
            var matrix = Side(side).Structure;
            return front >= 0 && front < matrix.Count ? Math.Max(1, matrix.Fronts[front].LoadLevels) : 0;
        }

        /// <summary>The front of a side holds load (it is not «En blanco»).</summary>
        public bool IsPresent(PushBackSide side, int front)
            => side == PushBackSide.A
                ? front >= 0 && front < Side(side).Structure.Count && Side(side).Structure.IsActive(front)
                : Composite.IsSlotPresent(side, front);

        /// <summary>The cell exists in that side: its front holds load and has that level.</summary>
        public bool CellExists(PushBackSide side, int front, int level) => IsPresent(side, front) && level >= 0 && level < LevelsAt(side, front);

        /// <summary>The highest number of levels of any front of the sides that exist.</summary>
        public int MaxLevels()
        {
            var levels = Side(PushBackSide.A).Structure.MaxLoadLevels();
            if (IsTwoSided) levels = Math.Max(levels, Side(PushBackSide.B).Structure.MaxLoadLevels());
            return Math.Max(1, levels);
        }

        // ------------------------------------------------------------------ gestures of the rack

        /// <summary>«Frentes»: the slots of the rack (both sides share them; a new slot is absent in the side that does not edit it).</summary>
        public void SetSlotCount(int count)
        {
            if (IsTwoSided) Composite.SetSlotCount(count);
            else Side(PushBackSide.A).SetFrontCount(count);
        }

        /// <summary>
        /// «En blanco» of a front in a side: it keeps its clear and its structure and holds no load. Null when it was written, or why not (the
        /// last active front of a one-way rack, or a slot that would exist in no side).
        /// </summary>
        public string SetBlank(PushBackSide side, int front, bool blank) => SetBlank(new[] { side }, front, blank);

        /// <summary>
        /// «En blanco» of a front in the sides the selection writes, by the path of the classic window (<c>SetFrontActive</c>, review of
        /// 24adfee7, point 1): each side that changes goes through <see cref="PushBackCompositeEditorState.SetSlotPresent"/>, which lets a side
        /// stay entirely blank while the other one holds the rack, and the gesture is written when ANY side took it (<c>applied |=</c>). Null when
        /// it was written (or nothing had to change), or why not, in the words of the classic window for any rack (review of 01e76902, point 4).
        /// Declared (PB-S18): the window decides side by side; the classic returns first when the ACTIVE side (A with «Ambos») is already as
        /// asked.
        /// </summary>
        public string SetBlank(IEnumerable<PushBackSide> sides, int front, bool blank)
        {
            var needed = false;
            var applied = false;
            foreach (var side in (sides ?? Enumerable.Empty<PushBackSide>()).Distinct())
            {
                if (side == PushBackSide.B && !IsTwoSided) continue;
                if (IsPresent(side, front) == !blank) continue;
                needed = true;
                applied |= Composite.SetSlotPresent(side, front, !blank);
            }

            if (!needed || applied) return null;
            return "Al menos un frente del rack debe permanecer activo.";
        }

        /// <summary>«Rack de dos sentidos (lado B)»: declares or removes the side B; removing it keeps its configuration dormant.</summary>
        public void SetSideBPresent(bool present)
        {
            Composite.SetSideBPresent(present);
            if (present) Composite.AlignSlotGrid();
        }

        /// <summary>«Lado A / Lado B / Ambos lados» over the matrix: what is seen is where it is written.</summary>
        public void SetSelection(PushBackSideSelection selection) => Composite.SetActiveSelection(selection);

        // ------------------------------------------------------------------ gestures of cells and fronts

        /// <summary>
        /// Writes the cells of the selection in every side the selection reaches. Only a cell that exists in that side is written (a front in
        /// blank keeps its cells dormant). Returns how many cells were written.
        /// </summary>
        public int WriteCells(IEnumerable<(int Front, int Level)> cells, Action<PushBackSide, DynamicEditorCell, PushBackEditorCell> write, bool allSides = false)
        {
            var targets = (cells ?? Enumerable.Empty<(int, int)>()).Distinct().ToList();
            var written = 0;
            foreach (var side in allSides ? Sides() : EditSides())
            {
                var state = Side(side);
                foreach (var (front, level) in targets)
                {
                    if (!CellExists(side, front, level)) continue;
                    var row = state.Structure.Fronts[front];
                    row.EnsureCellCount(row.LoadLevels);
                    write(side, row.Cells[level], state.Cell(front, level));
                    written++;
                }
            }

            return written;
        }

        /// <summary>Writes the fronts of the selection in every side the selection reaches (a front in blank too: its structure stays editable).</summary>
        public int WriteFronts(IEnumerable<int> fronts, Action<PushBackSide, PushBackEditorState, int> write, bool allSides = false)
        {
            var targets = (fronts ?? Enumerable.Empty<int>()).Distinct().ToList();
            var written = 0;
            foreach (var side in allSides ? Sides() : EditSides())
            {
                var state = Side(side);
                foreach (var front in targets)
                {
                    if (front < 0 || front >= state.Structure.Count) continue;
                    write(side, state, front);
                    written++;
                }
            }

            return written;
        }

        /// <summary>«Niveles» of fronts: through the state of each side, so its rear peraltes and topes follow the new count.</summary>
        public void SetLevels(IEnumerable<int> fronts, int levels, bool allSides = false)
            => WriteFronts(fronts, (side, state, front) =>
            {
                // A front in blank keeps its levels dormant, as the classic editor does (I-33).
                if (!IsPresent(side, front)) return;
                var delta = Math.Max(1, levels) - state.Structure.Fronts[front].LoadLevels;
                if (delta != 0) state.AdjustLevels(front, delta);
            }, allSides);

        /// <summary>«Carriles por frente» of fronts: through the state of each side.</summary>
        public void SetPositions(IEnumerable<int> fronts, int positions, bool allSides = false)
            => WriteFronts(fronts, (_, state, front) =>
            {
                var delta = Math.Max(1, positions) - state.Structure.Fronts[front].PalletCount;
                if (delta != 0) state.AdjustPositions(front, delta);
            }, allSides);

        /// <summary>The beds of a cell (topology and direction of the rack, not of a side).</summary>
        public PushBackCellBeds BedsAt(int front, int level)
        {
            switch (Composite.TopologyAt(front, level))
            {
                case PushBackCellTopology.SoloA: return PushBackCellBeds.SoloA;
                case PushBackCellTopology.SoloB: return PushBackCellBeds.SoloB;
                case PushBackCellTopology.Corrida:
                    return Composite.DirectionAt(front, level) == PushBackRunDirection.BToA ? PushBackCellBeds.CorridaBToA : PushBackCellBeds.CorridaAToB;
                default: return PushBackCellBeds.Encontradas;
            }
        }

        /// <summary>«Camas de la celda»: the topology and the direction of the cells (the rack, both sides at once).</summary>
        public void SetBeds(IEnumerable<(int Front, int Level)> cells, PushBackCellBeds beds)
        {
            var topology = beds == PushBackCellBeds.SoloA ? PushBackCellTopology.SoloA
                : beds == PushBackCellBeds.SoloB ? PushBackCellTopology.SoloB
                : beds == PushBackCellBeds.Encontradas ? PushBackCellTopology.Encontradas
                : PushBackCellTopology.Corrida;
            var direction = beds == PushBackCellBeds.CorridaBToA ? PushBackRunDirection.BToA : PushBackRunDirection.AToB;
            foreach (var (front, level) in (cells ?? Enumerable.Empty<(int, int)>()).Distinct())
                Composite.SetCell(front, level, topology, direction);
        }

        /// <summary>The rear tope that a cell carries in a side: its intent, where the beds of the cell admit it (what the drawing places).</summary>
        public bool TopeDrawnAt(PushBackSide side, int front, int level)
        {
            if (!CellExists(side, front, level) || !Composite.RearTopeAt(side, front, level)) return false;

            // A side whose tope is of type «Ninguno» places none (PushBackRearTopeConfig.Draws), as «Extremos» counts it (review of 24adfee7, point 4).
            if (Side(side).RearTopeConfig().IsNone) return false;
            if (!IsTwoSided) return true;
            var applies = Composite.TopeApplicability(front, level);
            return side == PushBackSide.A ? applies.A : applies.B;
        }

        /// <summary>The intent of the rear tope of a cell in a side (kept also where the beds do not admit it today).</summary>
        public bool TopeIntentAt(PushBackSide side, int front, int level) => CellExists(side, front, level) && Composite.RearTopeAt(side, front, level);

        /// <summary>«Tope posterior» of the cells, in the sides the selection reaches.</summary>
        public int SetTope(IEnumerable<(int Front, int Level)> cells, bool on) => WriteCells(cells, (_, __, push) => push.RearTopeEnabled = on);

        /// <summary>«Posiciones de fondo de la celda»: a run cell keeps it in its own bed; any other cell in each side. Null = inherit.</summary>
        public void SetCellFondo(IEnumerable<(int Front, int Level)> cells, int? palletsDeep)
        {
            var targets = (cells ?? Enumerable.Empty<(int, int)>()).Distinct().ToList();
            var value = palletsDeep.HasValue && palletsDeep.Value >= PushBackCellDepth.MinimumPalletsDeep ? palletsDeep : null;
            var runs = targets.Where(c => IsTwoSided && Composite.TopologyAt(c.Item1, c.Item2) == PushBackCellTopology.Corrida).ToList();
            foreach (var (front, level) in runs) Composite.SetCorridaDepth(front, level, value);
            WriteCells(targets.Except(runs), (_, __, push) => push.PalletsDeepOverride = value);
        }

        /// <summary>The own fondo of a cell in a side, or of its run bed; null when it inherits the one of its front.</summary>
        public int? CellFondoAt(PushBackSide side, int front, int level)
        {
            if (IsTwoSided && Composite.TopologyAt(front, level) == PushBackCellTopology.Corrida) return Composite.CorridaDepthAt(front, level);
            return CellExists(side, front, level) ? Side(side).Cell(front, level).PalletsDeepOverride : null;
        }

        /// <summary>«Dibujar tarima» of the cells (a visual reference, without BOM), in the sides the selection reaches.</summary>
        public int SetDrawPallet(IEnumerable<(int Front, int Level)> cells, bool on) => WriteCells(cells, (_, __, push) => push.DrawPallet = on);

        /// <summary>Every cell that exists, side by side (the active fronts and their levels).</summary>
        public IEnumerable<(PushBackSide Side, int Front, int Level)> AllCells()
        {
            foreach (var side in Sides())
            {
                var matrix = Side(side).Structure;
                for (var front = 0; front < matrix.Count; front++)
                    for (var level = 0; level < LevelsAt(side, front); level++)
                        if (CellExists(side, front, level)) yield return (side, front, level);
            }
        }

        /// <summary>«En todas» / «En ninguna» of the reference pallets of «Poste y anotaciones»: every cell of every side.</summary>
        public void SetDrawPalletEverywhere(bool on)
        {
            foreach (var (side, front, level) in AllCells().ToList()) Side(side).Cell(front, level).DrawPallet = on;
        }

        /// <summary>«En todas las celdas» / «En ninguna» of the rear tope in «Extremos y seguridad»: every cell of every side.</summary>
        public void SetTopeEverywhere(bool on)
        {
            foreach (var (side, front, level) in AllCells().ToList()) Side(side).Cell(front, level).RearTopeEnabled = on;
        }

        // ------------------------------------------------------------------ modules (question 7: no «Confirmar»)

        /// <summary>The modules of the longitudinal sequence of the last valid system, with the length the drawing uses.</summary>
        public IReadOnlyList<PushBackModuleRow> ModuleRows()
        {
            var modules = System?.Structure?.Modules;
            if (modules == null) return Array.Empty<PushBackModuleRow>();
            var session = Composite.SideA.ModuleSession;
            var custom = new HashSet<string>(session.CustomHeaderModuleIds, StringComparer.Ordinal);
            var editable = new HashSet<string>(session.Modules.Select(m => m.ModuleId), StringComparer.Ordinal);
            var rows = new List<PushBackModuleRow>();
            var x = 0.0;
            var headers = new Dictionary<string, int>(StringComparer.Ordinal);
            var separators = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var module in modules.Where(m => m != null))
            {
                // The modules of the side B (and the gap) carry the side in their label: one sequence for the whole rack (I-35).
                var isGap = string.Equals(module.ModuleId, PushBackCompositeStructure.GapModuleId, StringComparison.Ordinal);
                var sideB = module.ModuleId != null && module.ModuleId.StartsWith(PushBackCompositeStructure.SideBModulePrefix, StringComparison.Ordinal);
                var prefix = sideB ? "B · " : string.Empty;
                var counter = module.IsHeader ? headers : separators;
                counter[prefix] = (counter.TryGetValue(prefix, out var n) ? n : 0) + 1;
                var label = isGap ? "Hueco entre lados"
                    : prefix + (module.IsHeader ? "Cabecera " : "Separador ") + counter[prefix].ToString(CultureInfo.InvariantCulture);
                rows.Add(new PushBackModuleRow(module.ModuleId, label, isGap ? "Hueco" : module.IsHeader ? "Cabecera" : "Separador", x, module.Length,
                    module.IsManualOverride, module.IsHeader, custom.Contains(module.ModuleId) || HasLineOverride(session, module.ModuleId),
                    editable.Contains(module.ModuleId)));
                x += module.Length;
            }

            return rows;
        }

        private static bool HasLineOverride(RackModuleEditSession session, string moduleId)
            => session.OverriddenLines.Any(line => session.HasLineOverride(moduleId, line));

        /// <summary>
        /// The length of a module goes to the draft at once: a value is an own length, an empty one gives back the calculated length (with a
        /// custom cabecera it keeps the cabecera). Null when it was taken; otherwise why not. The caller recomposes.
        /// </summary>
        public string SetModuleLength(string moduleId, double? length)
        {
            var session = Composite.SideA.ModuleSession;
            if (session.Modules.All(m => !string.Equals(m.ModuleId, moduleId, StringComparison.Ordinal)))
                return "Ese módulo ya no está en el rack: vuelve a mirar la tabla.";

            if (length.HasValue)
            {
                if (length.Value <= 0.0) return "La longitud debe ser un número mayor que cero, o vacía para la calculada.";
                if (!session.SetLength(moduleId, length.Value)) return "No se pudo cambiar la longitud de ese módulo.";
            }
            else if (!RestoreLength(session, moduleId))
            {
                return "No se pudo volver a la longitud calculada de ese módulo.";
            }

            Composite.SideA.CommitModuleEdits();
            return null;
        }

        // The calculated length back, keeping a custom cabecera of the module: the session restores both, so the cabecera is staged again.
        private static bool RestoreLength(RackModuleEditSession session, string moduleId)
        {
            var custom = session.CustomHeaderModuleIds.Contains(moduleId) ? session.HeaderConfigurationCopy(moduleId) : null;
            if (!session.RestoreModule(moduleId)) return false;
            if (custom != null) session.SetHeaderConfiguration(moduleId, custom);
            return true;
        }

        // ------------------------------------------------------------------ «Restaurar layout…» (question 6)

        /// <summary>What each box of «Restaurar layout…» would give back now; empty for a box with nothing to give back.</summary>
        public IReadOnlyList<string> LayoutLoss(PushBackLayoutRestore restore)
        {
            var lost = new List<string>();
            if (restore == null) return lost;
            var session = Composite.SideA.ModuleSession;
            var lengths = ModuleRows().Count(r => r.IsManualLength);
            var headers = session.CustomHeaderModuleIds.Count + (session.OverriddenLines.Count > 0 ? 1 : 0);
            if (restore.ModuleLengths && lengths > 0) lost.Add(Count(lengths, "longitud propia de módulo", "longitudes propias de módulo"));
            if (restore.CustomHeaders && headers > 0) lost.Add("las cabeceras personalizadas");
            if (restore.RackParameters && HasRackParameters()) lost.Add("los parámetros del rack (altura de cabecera, poste derivado y refuerzo, separadores)");
            var fondos = AllCells().Count(c => CellFondoAt(c.Side, c.Front, c.Level).HasValue);
            if (restore.CellFondos && fondos > 0) lost.Add(Count(fondos, "posición de fondo propia de celda", "posiciones de fondo propias de celda"));
            var beams = AllCells().Count(c => Side(c.Side).Structure.Fronts[c.Front].Cells[c.Level].BeamLengthOverride.HasValue);
            if (restore.BeamLengths && beams > 0) lost.Add(Count(beams, "largo manual de larguero", "largos manuales de larguero"));
            var structure = (Composite.StructureOverrideA.HasValue ? 1 : 0) + (Composite.StructureOverrideB.HasValue ? 1 : 0);
            if (restore.SideStructure && structure > 0) lost.Add(Count(structure, "ajuste de estructura por lado", "ajustes de estructura por lado"));
            return lost;
        }

        /// <summary>Whether a parameter of the rack («Avanzado» › «Parámetros del rack») differs from its calculation.</summary>
        public bool HasRackParameters()
        {
            var a = State.Advanced;
            return a.ManualHeaderHeightOverride.HasValue || !a.DerivedPostReinforced || a.DerivedPostReinforcementHeight.HasValue
                   || a.DerivedPostHeight.HasValue || a.SeparatorCountOverride.HasValue || a.SeparatorSpacingOverride.HasValue;
        }

        /// <summary>Gives back the parts of the layout that are marked. The caller recomposes (and takes the state back if it fails).</summary>
        public void RestoreLayout(PushBackLayoutRestore restore)
        {
            if (restore == null || restore.IsEmpty) return;
            var session = Composite.SideA.ModuleSession;
            if (restore.ModuleLengths)
            {
                foreach (var row in ModuleRows().Where(r => r.IsManualLength && r.CanEditLength))
                {
                    if (restore.CustomHeaders) session.RestoreModule(row.ModuleId);
                    else RestoreLength(session, row.ModuleId);
                }
            }

            if (restore.CustomHeaders)
            {
                var ids = session.HeaderModuleIds.ToList();
                foreach (var id in session.CustomHeaderModuleIds.ToList()) session.ResetHeaderToCalculated(id);
                session.ClearLineOverrides(ids);
            }

            if (session.HasPendingChanges) Composite.SideA.CommitModuleEdits();
            if (restore.RackParameters) PushBackAdvancedRackParameters.Reset(State.Advanced);
            if (restore.CellFondos)
            {
                foreach (var (side, front, level) in AllCells().ToList())
                {
                    Side(side).Cell(front, level).PalletsDeepOverride = null;
                    if (IsTwoSided) Composite.SetCorridaDepth(front, level, null);
                }
            }

            if (restore.BeamLengths)
            {
                foreach (var (side, front, level) in AllCells().ToList()) Side(side).Structure.Fronts[front].Cells[level].BeamLengthOverride = null;
            }

            if (restore.SideStructure)
            {
                Composite.RestoreStructure(PushBackSide.A);
                Composite.RestoreStructure(PushBackSide.B);
            }
        }

        private static string Count(int count, string one, string many)
            => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
    }
}
