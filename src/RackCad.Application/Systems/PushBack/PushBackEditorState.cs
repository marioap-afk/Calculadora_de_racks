using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Pure editor state of a Push Back system. It OWNS two authorities and nothing else:
    /// <list type="bullet">
    /// <item><see cref="Structure"/> — the shared <see cref="DynamicFrontMatrix"/>: fronts, levels, the primary cell and
    /// the multi-cell selection, per-cell pallet/beam values, fondos, DepthStartPosition, heights, IN/OUT and intermediate
    /// beams and per-front length overrides. Push Back reuses the dynamic structure verbatim, so this is the SAME matrix the
    /// dynamic editor drives — never a renamed copy.</item>
    /// <item>a parallel per-front Push Back configuration (<see cref="PushFronts"/>) — authority ONLY for the high-end (rear)
    /// beam PERALTE per front x level and whether the rear pallet-stop tope is active per front x level.</item>
    /// </list>
    /// After every structural mutation the parallel configuration is re-synced to <see cref="Structure"/> so the two never
    /// drift: growing fronts clones the selected front's config, growing levels clones the last cell, shrinking drops only
    /// the cells a reduction left behind, and the surviving intersection is conserved. The selection is the matrix's alone;
    /// there is no parallel selection. No WPF, AutoCAD, geometry or catalog lives here — the assembler resolves the model.
    /// </summary>
    public sealed partial class PushBackEditorState
    {
        private readonly DynamicFrontMatrix structure;
        private readonly List<PushBackEditorFront> pushFronts = new List<PushBackEditorFront>();

        /// <summary>A brand-new Push Back design: one dynamic-default front, rear peralte 3.5 and an active tope on every
        /// cell, with a valid primary selection on the first cell (the dynamic matrix's own default).</summary>
        public PushBackEditorState()
        {
            structure = new DynamicFrontMatrix();
            SyncPushConfig();
        }

        /// <summary>The shared transverse structure (fronts/levels/selection/cells/fondos/...). The authority Push Back reuses.</summary>
        public DynamicFrontMatrix Structure => structure;

        /// <summary>The parallel per-front Push Back configuration, aligned by index with <see cref="DynamicFrontMatrix.Fronts"/>.</summary>
        public IReadOnlyList<PushBackEditorFront> PushFronts => pushFronts;

        /// <summary>The rear pallet-stop stick-out (SAQUE) applied to every rear tope (a single rack-wide Push Back scalar).</summary>
        public double RearTopeSaque { get; set; } = PushBackDefaults.RearTopeSaque;

        /// <summary>
        /// PB-005 (I-32) — the chosen rear-stop catalog variant, or null/blank for the system default. Like the SAQUE,
        /// it is a single rack-wide Push Back scalar and travels through the very same five boundaries: the config the
        /// Seguridad dialog reads and writes, the load from a resolved system, the snapshot and its restore, the reset
        /// of a new design, and the assembled design.
        /// </summary>
        public string RearTopePieceId { get; set; }

        /// <summary>
        /// I-42 (ronda 7E) — el TIPO de defensa de montacargas de este lado: un id de catalogo,
        /// <see cref="PushBackDefaults.NonePieceId"/> para «ninguno», o NULL para el comportamiento historico. Es un
        /// eje distinto de la intencion POR POSTE, que sigue viviendo en la seleccion de seguridad: el tipo dice QUE
        /// pieza usa el pasillo, y la rejilla en QUE lineas se pone.
        /// </summary>
        public string DefensePieceId { get; set; }

        private PushBackSystem workingBaseline;

        /// <summary>The last accepted resolved system whose MODULAR structure — custom cabeceras and manual fondos — the next
        /// recompute preserves. Null for a brand-new design, which rebuilds from a standard structure. Set on load and by the
        /// assembler's AcceptComputation; it is always a fresh resolve, so the editor never mutates the source design/system
        /// through it.</summary>
        public PushBackSystem WorkingBaseline => workingBaseline;

        /// <summary>Replace the working baseline (used by load and by the assembler's AcceptComputation). A null clears it so
        /// the next recompute rebuilds from a standard structure. The module session is re-seeded whenever the module
        /// SIGNATURE (ids + kinds) changed, so an editor never keeps stale module ids; when the signature is the same,
        /// staged module edits survive an unrelated recompute.</summary>
        public void SetWorkingBaseline(PushBackSystem baseline)
        {
            workingBaseline = baseline;
            ReseedModuleSession();
        }

        /// <summary>
        /// Adopt the baseline of a rack that is being LOADED (a new design, or an existing one reopened with
        /// RACKEDITAR). Unlike <see cref="SetWorkingBaseline"/> this ALWAYS discards the current module session.
        /// <para>
        /// I-40 (ronda 3) — la primera divergencia real del caso del Owner. La ventana crea una sesion nada mas
        /// abrirse (su constructor llama a <c>LoadNew</c>), asi que al cargar un rack existente ya habia una sesion
        /// viva sobre el rack ESTANDAR. <see cref="ReseedModuleSession"/> la conservaba porque la firma —ids y
        /// clases de modulo— coincidia, y coincide casi siempre: dos racks del mismo tamaño tienen los mismos
        /// modulos. El editor quedaba mostrando las cabeceras CALCULADAS del rack anterior mientras la geometria
        /// usaba las personalizadas del rack cargado: «Personalizada» con los datos predeterminados.
        /// </para>
        /// <para>
        /// La firma solo distingue racks dentro de UN mismo rack en recalculo, que es para lo que existe. Cargar
        /// otro diseño no es un recalculo: la sesion anterior no describe nada de este rack y se tira entera.
        /// </para>
        /// </summary>
        public void AdoptLoadedBaseline(PushBackSystem baseline)
        {
            workingBaseline = baseline;
            moduleSession = null;
            moduleCommit = null;
            moduleSignature = SignatureOf(baseline?.Structure);
            LastModuleReconciliation = null;
        }

        // ---- Longitudinal modules (I-35) --------------------------------------------------------------------------

        private RackModuleEditSession moduleSession;
        private RackModuleCommit moduleCommit;
        private string moduleSignature;

        /// <summary>
        /// The TRANSACTIONAL edit of the rack's longitudinal modules, seeded from the working baseline. A brand-new
        /// design has no baseline and therefore an empty session: there is nothing to customize until the first
        /// recompute produces a structure.
        /// <para>
        /// Staging on this session changes NOTHING: only <see cref="CommitModuleEdits"/> hands the intents to the
        /// assembler, and <see cref="CancelModuleEdits"/> throws them away. That is where confirm/cancel lives for
        /// Push Back — deliberately NOT in the shared header configurator (Owner, I-35).
        /// </para>
        /// </summary>
        public RackModuleEditSession ModuleSession
        {
            get
            {
                if (moduleSession == null)
                {
                    moduleSession = OpenModuleSession();
                }

                return moduleSession;
            }
        }

        /// <summary>The last ACCEPTED module edit — intents plus restore requests — that the assembler applies on the
        /// next recompute. Null while the user has never confirmed a module edit.</summary>
        public RackModuleCommit ModuleCommit => moduleCommit;

        /// <summary>What the last structural recompute did with each customized module, for the editor to report.
        /// Null before the first reconciliation.</summary>
        public RackModuleReconciliationResult LastModuleReconciliation { get; set; }

        /// <summary>Accept the staged module edits so the next recompute applies them.</summary>
        public RackModuleCommit CommitModuleEdits()
        {
            moduleCommit = ModuleSession.Commit();
            return moduleCommit;
        }

        /// <summary>Throw away the staged module edits; the design is left exactly as it was.</summary>
        public void CancelModuleEdits() => ModuleSession.Cancel();

        /// <summary>
        /// Return the four advanced RACK-WIDE scopes to their standing calculation or default: manual cabecera height,
        /// derived-post reinforcement (and its optional length), separator count and separator spacing. Explicit — the
        /// only way to lose them, exactly like the per-module customizations (Owner, I-35).
        /// </summary>
        public void RestoreAdvancedRackParameters(PushBackEditorInputs inputs)
            => PushBackAdvancedRackParameters.Reset(inputs);

        /// <summary>Consume the accepted edit, so a recompute triggered by something else does not re-apply a restore
        /// that already landed. The intents themselves survive inside the new baseline.</summary>
        public void ClearModuleCommit() => moduleCommit = null;

        private RackModuleEditSession OpenModuleSession()
        {
            var structure = workingBaseline?.Structure;
            moduleSignature = SignatureOf(structure);
            return structure == null
                ? RackModuleEditSession.Begin(Array.Empty<DynamicRackModuleDesign>())
                : RackModuleEditSession.Begin(structure);
        }

        private void ReseedModuleSession()
        {
            var signature = SignatureOf(workingBaseline?.Structure);
            if (moduleSession != null && string.Equals(signature, moduleSignature, StringComparison.Ordinal))
            {
                return;   // same modules: staged edits stay valid
            }

            moduleSession = null;
            moduleSignature = signature;
        }

        /// <summary>Ids and kinds in longitudinal order — the identity a module edit is addressed by.</summary>
        private static string SignatureOf(DynamicRackSystem system)
            => system == null
                ? string.Empty
                : string.Join("|", system.Modules.Select(module => module.ModuleId + ":" + module.Kind));

        /// <summary>The Push Back cell at (<paramref name="frontIndex"/>, <paramref name="levelIndex"/>), or a default when
        /// out of range — never throws and never returns a shared/orphan cell the caller could mutate into the state.</summary>
        public PushBackEditorCell Cell(int frontIndex, int levelIndex)
        {
            if (frontIndex < 0 || frontIndex >= pushFronts.Count)
            {
                return PushBackEditorCell.Default();
            }

            var cells = pushFronts[frontIndex].Cells;
            return levelIndex >= 0 && levelIndex < cells.Count ? cells[levelIndex] : PushBackEditorCell.Default();
        }

        // ---- Coordinating mutations: delegate structure to the matrix, then re-sync the parallel config ----------

        /// <summary>Grow/shrink the front count on the matrix (cloning the selected front), then re-sync the parallel config.</summary>
        public void SetFrontCount(int requested)
        {
            structure.SetFrontCount(requested);
            SyncPushConfig();
        }

        /// <summary>Change a front's pallet-position count on the matrix (levels unchanged; re-sync is defensive).</summary>
        public void AdjustPositions(int index, int delta)
        {
            structure.AdjustPositions(index, delta);
            SyncPushConfig();
        }

        /// <summary>Change a front's load-level count on the matrix, then re-sync the parallel config's per-front cells.</summary>
        public void AdjustLevels(int index, int delta)
        {
            structure.AdjustLevels(index, delta);
            SyncPushConfig();
        }

        /// <summary>
        /// Switch a front between Activo and En blanco (I-33). The matrix owns the flag and refuses to blank the last
        /// active front; the parallel Push Back cells are re-synced but never trimmed, so the blank front's rear
        /// peraltes and tope flags stay DORMANT and come back intact when it is reactivated. Returns the verdict.
        /// </summary>
        public bool SetActive(int index, bool isActive) => SetActive(index, isActive, allowAllBlank: false);

        /// <summary>La misma operacion, admitiendo dejar el lado entero en blanco (I-42, solo el rack compuesto).</summary>
        public bool SetActive(int index, bool isActive, bool allowAllBlank)
        {
            var applied = structure.SetActive(index, isActive, allowAllBlank);
            if (applied)
            {
                SyncPushConfig();
            }

            return applied;
        }

        /// <summary>Set or (extend) toggle the matrix selection at a cell. The selection is the matrix's alone.</summary>
        public void ToggleCell(int frontIndex, int levelIndex, bool extendSelection)
            => structure.ToggleCell(frontIndex, levelIndex, extendSelection);

        /// <summary>Prune out-of-range matrix selections and re-seat the primary cell.</summary>
        public void NormalizeSelection() => structure.NormalizeSelection();

        /// <summary>Normalize every existing cell's rear peralte against the catalog-allowed high-end values (Push Back's
        /// canonical rule). After this, each cell holds a value the resolver will accept unchanged, so the state, the
        /// assembled design and the resolved system agree. A null/empty list resolves every cell to the explicit 3.5 default.</summary>
        public void NormalizePeraltes(IReadOnlyList<double> allowed)
        {
            foreach (var front in pushFronts)
            {
                foreach (var cell in front.Cells)
                {
                    cell.NormalizePeralte(allowed);
                }
            }
        }

        /// <summary>Apply the buffer to the primary cell: the shared values via the matrix, the Push Back values to the
        /// parallel primary cell. The matrix may grow the front's levels, so re-sync first.</summary>
        public void CommitEditorValues(PushBackEditorValues values)
        {
            structure.CommitEditorValues(values.Dynamic);
            SyncPushConfig();
            var frontIndex = structure.SelectedFrontIndex;
            var levelIndex = structure.SelectedLevelIndex;
            if (frontIndex >= 0 && frontIndex < pushFronts.Count)
            {
                var front = pushFronts[frontIndex];
                if (levelIndex >= 0 && levelIndex < front.Cells.Count)
                {
                    front.Cells[levelIndex].Apply(values);
                }
            }
        }

        /// <summary>Apply the buffer across a scope: the shared values via <see cref="DynamicFrontMatrix.ApplyScope"/>, and
        /// the Push Back values (rear peralte + tope) to the SAME cell addresses resolved by the SAME
        /// <see cref="DynamicRackCellScopeResolver"/> — never a second, independently-built target list. Returns the count
        /// of cells the shared apply wrote.</summary>
        public int ApplyScope(PushBackEditorValues values, DynamicRackCellScope scope)
        {
            var written = structure.ApplyScope(values.Dynamic, scope);
            SyncPushConfig();
            var targets = DynamicRackCellScopeResolver.Targets(
                structure.LevelCounts(),
                structure.SelectedFrontIndex,
                structure.SelectedLevelIndex,
                scope,
                structure.SelectedCells());
            foreach (var target in targets)
            {
                if (target.FrontIndex < 0 || target.FrontIndex >= pushFronts.Count)
                {
                    continue;
                }

                var front = pushFronts[target.FrontIndex];
                if (target.LevelIndex >= 0 && target.LevelIndex < front.Cells.Count)
                {
                    front.Cells[target.LevelIndex].Apply(values);
                }
            }

            return written;
        }

        // ---- I-41: fondo y tarima por celda (cada operacion escribe UNA sola propiedad) --------------------------

        /// <summary>
        /// I-41 (PB-015) — escribe el FONDO de las celdas del alcance, y NADA mas. <paramref name="palletsDeep"/> null
        /// es la RESTAURACION: elimina el override y la celda vuelve a heredar el fondo por defecto de su frente.
        /// <para>
        /// Usa el MISMO <see cref="DynamicRackCellScopeResolver"/> y la MISMA seleccion multiple que los alcances que
        /// ya existen; no hay un segundo modelo de seleccion. Lo que no comparte es el buffer de celda: pasar por
        /// <see cref="PushBackEditorCell.Apply"/> arrastraria el resto de los campos de la celda origen, que es
        /// exactamente lo que el contrato de I-41 prohibe.
        /// </para>
        /// Devuelve cuantas celdas se escribieron, para que el editor pueda informarlo.
        /// </summary>
        public int ApplyPalletsDeep(int? palletsDeep, DynamicRackCellScope scope)
            => ForEachTarget(scope, cell => cell.PalletsDeepOverride =
                palletsDeep.HasValue && palletsDeep.Value >= PushBackCellDepth.MinimumPalletsDeep
                    ? palletsDeep
                    : null);

        /// <summary>
        /// I-41 (PB-016) — escribe el flag de TARIMA de las celdas del alcance, y NADA mas. False es a la vez el valor
        /// normal y la restauracion al default legacy, porque ese default ES false.
        /// </summary>
        public int ApplyDrawPallet(bool drawPallet, DynamicRackCellScope scope)
            => ForEachTarget(scope, cell => cell.DrawPallet = drawPallet);

        /// <summary>
        /// Recorre las celdas del alcance resuelto y aplica <paramref name="write"/> a cada una. Es el unico camino por
        /// el que I-41 escribe, de modo que las dos operaciones comparten resolucion de alcance, acotado y conteo.
        /// </summary>
        private int ForEachTarget(DynamicRackCellScope scope, Action<PushBackEditorCell> write)
        {
            var targets = DynamicRackCellScopeResolver.Targets(
                structure.LevelCounts(),
                structure.SelectedFrontIndex,
                structure.SelectedLevelIndex,
                scope,
                structure.SelectedCells());

            var written = 0;
            foreach (var target in targets)
            {
                if (target.FrontIndex < 0 || target.FrontIndex >= pushFronts.Count)
                {
                    continue;
                }

                var front = pushFronts[target.FrontIndex];
                if (target.LevelIndex < 0 || target.LevelIndex >= front.Cells.Count)
                {
                    continue;
                }

                write(front.Cells[target.LevelIndex]);
                written++;
            }

            return written;
        }

        /// <summary>
        /// I-41 (PB-015) — la ENVOLVENTE estructural de cada frente: el mayor fondo efectivo de sus niveles ACTIVOS.
        /// Es lo que el ensamblador escribe en <c>DynamicRackFrontDesign.PalletsDeep</c>, de modo que la estructura
        /// compartida se dimensiona por el nivel mas profundo y los demas terminan antes dentro de ella.
        /// <para>
        /// Esta es LA razon por la que I-40 sobrevive: mientras la envolvente no cambie, el layout de fondos es el
        /// mismo, <c>MustRebuild</c> responde false, el recalculo copia el baseline y con el viajan intactos los
        /// ModuleId, las cabeceras por linea y las alturas de poste derivado por linea.
        /// </para>
        /// </summary>
        public int EnvelopePalletsDeep(int frontIndex)
        {
            if (frontIndex < 0 || frontIndex >= structure.Count)
            {
                return PushBackCellDepth.MinimumPalletsDeep;
            }

            var row = structure.Fronts[frontIndex];
            var overrides = frontIndex < pushFronts.Count
                ? pushFronts[frontIndex].Cells.Select(cell => cell.PalletsDeepOverride).ToList()
                : new List<int?>();
            return PushBackCellDepth.Envelope(
                row.PalletsDeep,
                overrides,
                DynamicFrontActivation.EffectiveLoadLevels(row));
        }

        /// <summary>
        /// Los frentes del diseno con la envolvente ya aplicada: la lista que construye
        /// <see cref="DynamicFrontMatrix.BuildFrontDesigns"/> con <c>PalletsDeep</c> sustituido por
        /// <see cref="EnvelopePalletsDeep"/>. El fondo POR DEFECTO del frente no se pierde: viaja aparte, en
        /// <c>PushBackFrontConfig.DefaultPalletsDeep</c>, que es lo que hace reversible el round trip.
        /// </summary>
        public IReadOnlyList<DynamicRackFrontDesign> BuildEnvelopeFrontDesigns()
        {
            var designs = structure.BuildFrontDesigns();
            for (var index = 0; index < designs.Count; index++)
            {
                designs[index].PalletsDeep = EnvelopePalletsDeep(index);
            }

            return designs;
        }

        // ---- Rear tope (configured EXCLUSIVELY from Seguridad) ---------------------------------------------------

        /// <summary>
        /// Owner decision (2026-07-24) — PROJECT the editor's rear-tope state into the shared config the Seguridad dialog
        /// edits: the SAQUE plus ONLY the deactivated cells, exactly the rule
        /// <see cref="PushBackEditorDesignAssembler"/> materializes into the design (defaults stay implicit, so a
        /// round trip through the dialog cannot turn "default active" into a stored value).
        /// </summary>
        public PushBackRearTopeConfig RearTopeConfig()
        {
            var config = new PushBackRearTopeConfig { Saque = RearTopeSaque, PieceId = RearTopePieceId };
            for (var frontIndex = 0; frontIndex < structure.Count; frontIndex++)
            {
                var levels = Math.Max(1, structure.Fronts[frontIndex].LoadLevels);
                for (var level = 0; level < levels; level++)
                {
                    if (!Cell(frontIndex, level).RearTopeEnabled)
                    {
                        config.OffCells.Add(new SelectiveGridCell { Frente = frontIndex, Level = level });
                    }
                }
            }

            return config;
        }

        /// <summary>
        /// RECOVER the config the Seguridad dialog produced: the SAQUE and the per-cell deactivations. This is the ONLY
        /// path that writes <see cref="PushBackEditorCell.RearTopeEnabled"/> — the cell scopes deliberately cannot
        /// (see <see cref="PushBackEditorCell.Apply"/>). A cell not listed in OffCells is active.
        /// </summary>
        public void LoadRearTopeConfig(PushBackRearTopeConfig config)
        {
            if (config == null)
            {
                return;
            }

            RearTopeSaque = config.Saque > 0.0 ? config.Saque : PushBackDefaults.RearTopeSaque;
            RearTopePieceId = config.PieceId;
            for (var frontIndex = 0; frontIndex < structure.Count; frontIndex++)
            {
                var levels = Math.Max(1, structure.Fronts[frontIndex].LoadLevels);
                for (var level = 0; level < levels; level++)
                {
                    Cell(frontIndex, level).RearTopeEnabled = config.At(frontIndex, level);
                }
            }
        }

        // ---- Snapshot / rollback --------------------------------------------------------------------------------

        /// <summary>Deep-snapshot both authorities plus the FULL selection for rollback: the matrix fronts, the parallel Push
        /// Back configuration (both independent copies, no shared cell) and the primary cell + every multi-selection address.</summary>
        public PushBackEditorSnapshot Snapshot()
            => new PushBackEditorSnapshot(
                structure.Snapshot(),
                pushFronts.Select(front => front.Clone()).ToList(),
                RearTopeSaque,
                RearTopePieceId,
                DefensePieceId,
                structure.SelectedFrontIndex,
                structure.SelectedLevelIndex,
                structure.SelectedCells());

        /// <summary>Restore both authorities from a snapshot (taking fresh clones so the snapshot stays reusable), re-sync
        /// defensively, and rebuild the exact selection (primary + multi-selection) through the matrix's own toggles.</summary>
        public void Restore(PushBackEditorSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            structure.Restore(snapshot.Structure.Select(front => front.Clone()).ToList());
            pushFronts.Clear();
            pushFronts.AddRange(snapshot.PushFronts.Select(front => front.Clone()));
            RearTopeSaque = snapshot.RearTopeSaque;
            RearTopePieceId = snapshot.RearTopePieceId;
            DefensePieceId = snapshot.DefensePieceId;
            SyncPushConfig();
            RestoreSelection(snapshot.SelectedFrontIndex, snapshot.SelectedLevelIndex, snapshot.SelectedCells);
        }

        /// <summary>
        /// Rebuild an exact selection through <see cref="DynamicFrontMatrix.ToggleCell"/> alone (the matrix is never modified):
        /// out-of-range addresses are discarded, the non-primary cells are toggled first and the primary LAST so it becomes the
        /// matrix's primary (a toggle re-seats the primary on the last cell touched), then the selection is normalized.
        /// </summary>
        private void RestoreSelection(int primaryFront, int primaryLevel, IReadOnlyList<DynamicRackCellAddress> cells)
        {
            bool InRange(int front, int level)
                => front >= 0 && front < structure.Count && level >= 0 && level < Math.Max(1, structure.Fronts[front].LoadLevels);

            var primaryInRange = InRange(primaryFront, primaryLevel);
            var others = (cells ?? new List<DynamicRackCellAddress>())
                .Where(address => InRange(address.FrontIndex, address.LevelIndex)
                                  && !(address.FrontIndex == primaryFront && address.LevelIndex == primaryLevel))
                .ToList();

            if (others.Count > 0)
            {
                structure.ToggleCell(others[0].FrontIndex, others[0].LevelIndex, false);
                for (var index = 1; index < others.Count; index++)
                {
                    structure.ToggleCell(others[index].FrontIndex, others[index].LevelIndex, true);
                }

                if (primaryInRange)
                {
                    structure.ToggleCell(primaryFront, primaryLevel, true); // primary last -> it becomes the primary
                }
            }
            else if (primaryInRange)
            {
                structure.ToggleCell(primaryFront, primaryLevel, false);
            }

            structure.NormalizeSelection();
        }

        // ---- Shape sync -----------------------------------------------------------------------------------------

        /// <summary>
        /// Re-align the parallel Push Back configuration to <see cref="Structure"/> after any structural mutation. Fronts and
        /// levels are matched by index: a new front clones the selected front's config (the SAME template the matrix clones),
        /// a new level clones the front's last cell, and a removed front/level drops only the trailing entries it left behind.
        /// The surviving intersection keeps its edited peralte/tope; no cell is ever orphaned or shared.
        /// </summary>
        private void SyncPushConfig()
        {
            var frontCount = structure.Count;
            if (frontCount == 0)
            {
                pushFronts.Clear();
                return;
            }

            // Grow/shrink the FRONT count, cloning the selected front's config as the template (the matrix's own rule).
            if (pushFronts.Count > frontCount)
            {
                pushFronts.RemoveRange(frontCount, pushFronts.Count - frontCount);
            }
            else if (pushFronts.Count < frontCount)
            {
                var template = pushFronts.Count > 0
                    ? pushFronts[Math.Max(0, Math.Min(structure.SelectedFrontIndex, pushFronts.Count - 1))]
                    : null;
                while (pushFronts.Count < frontCount)
                {
                    pushFronts.Add(template?.Clone() ?? new PushBackEditorFront());
                }
            }

            // Align each front's LEVEL count to the matrix front (grow clones the last cell, shrink drops trailing cells).
            for (var index = 0; index < frontCount; index++)
            {
                var levels = Math.Max(1, structure.Fronts[index].LoadLevels);
                pushFronts[index].EnsureCellCount(levels);
                pushFronts[index].TrimToLevelCount(levels);
            }
        }
    }

    /// <summary>An immutable deep snapshot of a <see cref="PushBackEditorState"/> for rollback: the matrix fronts, the
    /// parallel Push Back configuration (both independent copies) and the full selection (primary cell + every address).</summary>
    public sealed class PushBackEditorSnapshot
    {
        public PushBackEditorSnapshot(
            IReadOnlyList<DynamicEditorFront> structure,
            IReadOnlyList<PushBackEditorFront> pushFronts,
            double rearTopeSaque,
            string rearTopePieceId,
            string defensePieceId,
            int selectedFrontIndex,
            int selectedLevelIndex,
            IReadOnlyList<DynamicRackCellAddress> selectedCells)
        {
            Structure = structure ?? new List<DynamicEditorFront>();
            PushFronts = pushFronts ?? new List<PushBackEditorFront>();
            RearTopeSaque = rearTopeSaque;
            RearTopePieceId = rearTopePieceId;
            DefensePieceId = defensePieceId;
            SelectedFrontIndex = selectedFrontIndex;
            SelectedLevelIndex = selectedLevelIndex;
            SelectedCells = selectedCells ?? new List<DynamicRackCellAddress>();
        }

        public IReadOnlyList<DynamicEditorFront> Structure { get; }
        public IReadOnlyList<PushBackEditorFront> PushFronts { get; }
        public double RearTopeSaque { get; }
        public string RearTopePieceId { get; }

        /// <summary>I-42 (ronda 7E) — el tipo de defensa del lado en el momento de la instantanea.</summary>
        public string DefensePieceId { get; }
        public int SelectedFrontIndex { get; }
        public int SelectedLevelIndex { get; }
        public IReadOnlyList<DynamicRackCellAddress> SelectedCells { get; }
    }
}
