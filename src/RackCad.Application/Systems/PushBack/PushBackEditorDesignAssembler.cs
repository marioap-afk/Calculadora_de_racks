using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Recompute core of the Push Back editor. It replicates the dynamic editor's recompute cycle WITHOUT touching it or the
    /// dynamic window: it decides when a full rebuild is needed (<see cref="DynamicEditorDesignAssembler.MustRebuild"/>),
    /// preserves the user's per-header manual fondos across a rebuild (<see cref="DynamicEditorDesignAssembler.SnapshotHeaderFondos"/>
    /// / <see cref="DynamicEditorDesignAssembler.RestoreHeaderFondos"/>), updates calculated cabecera heights in place when
    /// only the height changes (<see cref="DynamicEditorDesignAssembler.UpdateHeaderHeightInPlace"/>) so custom cabeceras
    /// survive, and assembles the shared design with <see cref="DynamicEditorDesignAssembler.BuildDesign"/>. It then adds
    /// Push Back's own bits — the canonical rear beam peralte per front x level, the rear-tope OffCells (only DEACTIVATIONS
    /// are materialized) and the authorized (GUIA-free, low-end) safety — and resolves ONCE with <see cref="PushBackResolver"/>.
    /// The modular structure to preserve lives on the editor state's <see cref="PushBackEditorState.WorkingBaseline"/>, which a
    /// successful <see cref="AcceptComputation"/> advances (a failure never replaces it). Pure: no WPF, AutoCAD, window or drawing.
    /// </summary>
    public sealed class PushBackEditorDesignAssembler
    {
        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemBuilder builder;
        private readonly DynamicRackSystemResolver dynamicResolver;
        private readonly DynamicEditorDesignAssembler dynamicAssembler;
        private readonly PushBackResolver pushResolver;
        private readonly PushBackSafetyAuthority safetyAuthority;
        private readonly RackModuleReconciliation reconciliation;
        private readonly RackFrameProjectStore headerClone = new RackFrameProjectStore();
        private readonly PushBackSystemLateralBuilder lateralBuilder = new PushBackSystemLateralBuilder();
        private readonly PushBackSystemFrontalBuilder frontalBuilder = new PushBackSystemFrontalBuilder();
        private readonly PushBackSystemPlantaBuilder plantaBuilder = new PushBackSystemPlantaBuilder();

        public PushBackEditorDesignAssembler(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            builder = new DynamicRackSystemBuilder(this.catalog);
            dynamicResolver = new DynamicRackSystemResolver(this.catalog);
            dynamicAssembler = new DynamicEditorDesignAssembler(this.catalog, builder, dynamicResolver);
            pushResolver = new PushBackResolver(this.catalog);
            safetyAuthority = new PushBackSafetyAuthority(this.catalog);
            reconciliation = new RackModuleReconciliation(builder);
        }

        /// <summary>
        /// The module customizations to carry: the user's ACCEPTED edit when there is one, otherwise the baseline's own
        /// modules, so a recompute triggered by any other control preserves what the rack already had.
        /// </summary>
        private static IReadOnlyList<DynamicRackModuleDesign> ModuleIntents(
            RackModuleCommit commit,
            DynamicRackSystem baseline)
        {
            if (commit != null)
            {
                return commit.Modules;
            }

            return baseline == null
                ? Array.Empty<DynamicRackModuleDesign>()
                : RackModuleEditSession.Begin(baseline).Commit().Modules;
        }

        /// <summary>The per-line derived-post heights a baseline already carries, when there is no accepted edit.</summary>
        private static IReadOnlyList<DynamicDerivedPostLineOverride> DerivedPostLinesOf(DynamicRackSystem baseline)
            => baseline == null
                ? Array.Empty<DynamicDerivedPostLineOverride>()
                : (IReadOnlyList<DynamicDerivedPostLineOverride>)baseline.DerivedPostLineOverrides;

        /// <summary>The per-line configurations a baseline already carries, when there is no accepted edit.</summary>
        private static IReadOnlyList<DynamicHeaderLineOverride> LineOverridesOf(DynamicRackSystem baseline)
            => baseline == null
                ? Array.Empty<DynamicHeaderLineOverride>()
                : (IReadOnlyList<DynamicHeaderLineOverride>)baseline.HeaderLineOverrides;

        /// <summary>The resolver the editor shares for load/snapshot and peralte normalization (single implementation).</summary>
        public PushBackResolver Resolver => pushResolver;

        /// <summary>The catalog-allowed high-end (rear) beam peraltes, for the window's peralte picker and cell normalization.</summary>
        public IReadOnlyList<double> AllowedHighEndPeraltes() => pushResolver.AllowedHighEndPeraltes();

        /// <summary>
        /// The authorized, low-end-only safety set: deep copies of <paramref name="requested"/> with entrance GUIDES removed
        /// and every surviving family normalized to the low end. The input is never mutated. This is the editable safety the
        /// increment-3 window may offer and persist. Shares the single <see cref="PushBackSafetyAuthority"/>.
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> AuthorizedSafety(IEnumerable<SelectiveSafetySelection> requested)
            => safetyAuthority.Authorize(requested);

        /// <summary>
        /// Adopt a successful computation's resolved system as the baseline the NEXT recompute will preserve. An invalid
        /// computation is ignored, so a failed recompute never replaces the surviving baseline.
        /// </summary>
        public void AcceptComputation(PushBackEditorState state, PushBackEditorComputation computation)
        {
            if (state != null && computation != null && computation.IsValid && computation.System != null)
            {
                state.SetWorkingBaseline(computation.System);
            }
        }

        /// <summary>Assemble the canonical persisted design (no resolve). See the <see cref="BuildDesign(PushBackEditorState, PushBackEditorInputs, bool)"/> overload.</summary>
        public PushBackDesign BuildDesign(PushBackEditorState state, PushBackEditorInputs inputs)
            => BuildDesign(state, inputs, forceRebuild: false);

        /// <summary>
        /// Assemble the persisted Push Back design from the editor state and the rack-wide inputs, WITHOUT resolving. The
        /// shared structure comes from the dynamic recompute cycle: with no structural change a COPY of the loaded baseline is
        /// reused (custom modules + manual fondos preserved, calculated cabeceras height-updated); a pallet/fondos change (or
        /// <paramref name="forceRebuild"/>) rebuilds from a standard structure but restores the manual fondos by ordinal; a
        /// brand-new state (no baseline) starts from the standard structure. Push Back's canonical rear peraltes, the rear-tope
        /// OffCells (only deactivations) and the authorized safety are then added. The result is canonical by itself.
        /// </summary>
        public PushBackDesign BuildDesign(PushBackEditorState state, PushBackEditorInputs inputs, bool forceRebuild)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var editorInputs = inputs ?? PushBackEditorInputs.NewDesign();
            var matrix = state.Structure;

            // Canonicalize the editor state so the state, the assembled design and the resolved system agree: every rear
            // peralte snaps to a catalog-allowed value (or the explicit 3.5 default), and a non-positive SAQUE becomes the
            // default. The design produced below is therefore already canonical by itself.
            state.NormalizePeraltes(pushResolver.AllowedHighEndPeraltes());
            if (state.RearTopeSaque <= 0.0)
            {
                state.RearTopeSaque = PushBackDefaults.RearTopeSaque;
            }

            var levels = matrix.MaxLoadLevels();
            var firstLevel = matrix.Fronts.Count > 0
                ? matrix.Fronts[0].FirstLevelHeight
                : DynamicRackDefaults.DefaultFirstLevelHeight;
            var beamDepth = DynamicLoadBeamGeometry.ResolveBeamDepth(
                catalog,
                DynamicRackDefaults.InOutBeamCatalogId,
                editorInputs.BeamDepth > 0.0 ? editorInputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth);
            var postId = string.IsNullOrWhiteSpace(editorInputs.PostCatalogId)
                ? catalog.Defaults?.Post
                : editorInputs.PostCatalogId;
            var palletTolerance = editorInputs.PalletTolerance > 0.0
                ? editorInputs.PalletTolerance
                : DynamicRackDefaults.DefaultPalletTolerance;
            var postPeralte = editorInputs.PostPeralte;
            var pallet = ClonePallet(editorInputs.Pallet);

            // I-41 (PB-015): la estructura se dimensiona por la ENVOLVENTE de cada frente —el mayor fondo efectivo de
            // sus niveles activos—, no por el fondo por defecto. Es la MISMA lista que se usa mas abajo para armar el
            // diseno, construida una sola vez: si el layout se calculara con una lista y el diseno con otra, la
            // decision de reconstruir (MustRebuild) se tomaria sobre una estructura distinta de la que se guarda.
            var envelopeFronts = state.BuildEnvelopeFrontDesigns();
            var depthLayout = DynamicDepthGeometry.Resolve(envelopeFronts, Math.Max(2, editorInputs.PalletsDeep));
            var palletsDeep = depthLayout.TotalPositions;

            // I-35 (Owner round 2) — the four advanced RACK-WIDE scopes. They are validated BEFORE anything is built,
            // so an invalid one blocks with a visible message instead of producing a rack nobody asked for. The manual
            // cabecera height replaces the derived one for the WHOLE recompute (that is what "manual" means), so it is
            // resolved here and every consumer below sees a single effective height.
            PushBackAdvancedRackParameters.Validate(editorInputs);
            var derivedHeight = ComputeHeaderHeight(pallet, palletsDeep, levels, firstLevel, beamDepth);
            var headerHeight = editorInputs.ManualHeaderHeightOverride ?? derivedHeight;
            PushBackAdvancedRackParameters.ValidateReinforcementAgainstPost(editorInputs, headerHeight);

            // Dynamic recompute cycle (composed, never modified): rebuild only on a pallet/fondos change (or a forced reset);
            // otherwise reuse a COPY of the loaded baseline so custom modules and manual fondos survive. The baseline itself is
            // never mutated here — only AcceptComputation advances it, so a failed recompute cannot corrupt it.
            var baseline = state.WorkingBaseline?.Structure;

            // I-35: the accepted module edit. "Restaurar estándar" is a forced rebuild, and an individual restore also
            // needs one — the module's CALCULATED length only exists in a freshly built standard structure.
            var commit = state.ModuleCommit;
            var restoredIds = commit?.RestoredModuleIds ?? (IReadOnlyList<string>)Array.Empty<string>();
            var standardRestore = forceRebuild || (commit?.StandardRestoreRequested ?? false);

            // I-42 (A3-MOD): la pregunta «¿hay que reconstruir la secuencia?» es sobre la FORMA del lado que este
            // ensamblador arma —el A—, y en un rack compuesto la secuencia del baseline es la del RACK: A + hueco +
            // B invertido. Compararla contra el layout de profundidad de A daba siempre distinto (17 modulos contra
            // 8, base 17 contra 8), asi que todo recalculo de un compuesto reconstruia desde cero.
            var rebuildShape = state.WorkingBaseline?.Composite?.Of(PushBackSide.A)?.Local?.Structure ?? baseline;
            var mustRebuild = standardRestore
                              || restoredIds.Count > 0
                              || DynamicEditorDesignAssembler.MustRebuild(rebuildShape, pallet, depthLayout);

            DynamicRackSystem system;
            if (mustRebuild)
            {
                system = builder.BuildDefault(pallet, depthLayout, RackFrameTemplateCatalog.Default, postId, headerHeight, postPeralte);

                // I-42 (A3-MOD): la reconstruccion es del lado A, pero la SECUENCIA es la del rack. La cola del
                // baseline —el hueco y los modulos del lado B— se reanexa antes de reconciliar para que la
                // comparacion se haga contra la secuencia canonica y no contra media: sin esto, un cambio de
                // topologia de A reportaba GAP y B:* como eliminados y se llevaba por delante lo que el usuario
                // habia personalizado en la mitad B, que no habia cambiado. El resolver compuesto reparte despues
                // esta secuencia por lado, exactamente como hace al reabrir un rack guardado.
                foreach (var module in PushBackCompositeStructure.CompositeTail(baseline))
                {
                    // COPIA: el baseline no se toca. La reconciliacion escribe sobre los modulos que reciba, y el
                    // baseline solo lo adelanta AcceptComputation.
                    system.Modules.Add(new DynamicRackModule
                    {
                        ModuleId = module.ModuleId,
                        Kind = module.Kind,
                        Length = module.Length,
                        IsCalculated = module.IsCalculated,
                        IsManualOverride = module.IsManualOverride,
                        UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                        AssociatedFrameConfiguration = headerClone.DeepCopy(module.AssociatedFrameConfiguration),
                        Notes = module.Notes
                    });
                }

                system.RecalculatePositions();
            }
            else
            {
                system = CopyStructureSystem(baseline);
                dynamicAssembler.UpdateHeaderHeightInPlace(system, headerHeight, postId);
            }

            builder.ApplyPostPeralte(system, postPeralte);

            // I-35 (Owner round 2): hand the four advanced scopes to the authorities that already own them. Assignment
            // only — no rule is restated here; the resolver, DynamicSeparatorGeometry, the lateral builder and the BOM
            // read them exactly as they do for the dynamic system. A rack-wide "restaurar estándar" clears them, which
            // is what makes it a reset of EVERYTHING and not only of the modules.
            if (standardRestore)
            {
                PushBackAdvancedRackParameters.Reset(editorInputs);
            }

            PushBackAdvancedRackParameters.ApplyTo(system, editorInputs);

            // I-35: carry the user's module customizations onto whatever structure we ended up with, matching by exact
            // ModuleId + Kind (Owner). This REPLACES the ordinal SnapshotHeaderFondos/RestoreHeaderFondos pair for Push
            // Back — that pair carries the fondo only and re-stamps every restored header as calculated — WITHOUT
            // touching it, because it is also the dynamic editor's and I-35 must not change the Dinámico.
            // A rack-wide "restaurar estándar" carries nothing: that is what makes it a reset.
            var intents = standardRestore
                ? Array.Empty<DynamicRackModuleDesign>()
                : ModuleIntents(commit, baseline);
            state.LastModuleReconciliation = reconciliation.Reconcile(intents, system, restoredIds);

            // I-40 — las configuraciones por LINEA. Se llevan igual que las de modulo: la ACEPTADA cuando la hay, y
            // si no la que el baseline ya tenia, de modo que un recalculo disparado por cualquier otro control las
            // conserva. Un «restaurar estandar» no lleva ninguna: eso es lo que lo hace un reset.
            // Se descartan las que apunten a un modulo que el rack reconstruido ya no tiene como cabecera, por la
            // misma razon por la que la reconciliacion descarta una personalizacion sin modulo.
            system.HeaderLineOverrides.Clear();
            system.DerivedPostLineOverrides.Clear();
            if (!standardRestore)
            {
                foreach (var derived in commit?.DerivedPostOverrides ?? DerivedPostLinesOf(baseline))
                {
                    if (derived != null && derived.Height > 0.0)
                    {
                        system.DerivedPostLineOverrides.Add(new DynamicDerivedPostLineOverride
                        {
                            PostIndex = derived.PostIndex,
                            Height = derived.Height
                        });
                    }
                }

                var headerIds = new HashSet<string>(
                    system.Modules.Where(module => module.IsHeader).Select(module => module.ModuleId),
                    StringComparer.Ordinal);

                var lines = commit?.LineOverrides ?? LineOverridesOf(baseline);
                foreach (var line in lines)
                {
                    if (line?.Header != null && headerIds.Contains(line.ModuleId))
                    {
                        system.HeaderLineOverrides.Add(new DynamicHeaderLineOverride
                        {
                            PostIndex = line.PostIndex,
                            ModuleId = line.ModuleId,
                            Header = headerClone.DeepCopy(line.Header)
                        });
                    }
                }
            }

            var authorizedSafety = safetyAuthority.Authorize(editorInputs.SafetySelections);
            var sharedDesign = dynamicAssembler.BuildDesign(
                system,
                matrix,
                levels,
                firstLevel,
                beamDepth,
                postId,
                palletsDeep,
                postPeralte,
                palletTolerance,
                editorInputs.Annotations ?? new DynamicAnnotationOptions(),
                authorizedSafety);

            // I-42 — el datum de «Alto 1er nivel» es del RACK y viaja con el diseño: un rack nuevo trae el del
            // producto, y uno cargado el que su documento declare (ausente = historico).
            sharedDesign.FirstLevelDatum = editorInputs.FirstLevelDatum;

            var design = new PushBackDesign
            {
                Structure = sharedDesign,
                LegacyHighEndBeamPeralte = PushBackDefaults.HighEndBeamDefaultPeralte,
                RearTope = new PushBackRearTopeConfig { Saque = state.RearTopeSaque, PieceId = state.RearTopePieceId },
                DefensePieceId = state.DefensePieceId
            };

            // I-41 (PB-015): la ENVOLVENTE mandada a la estructura compartida. El diseno se arma desde el matrix, que
            // sigue llevando el fondo POR DEFECTO de cada frente, asi que hay que volver a escribirla — si no, un
            // guardado devolveria el default como si fuera la estructura y el rack encogeria al recargarlo.
            for (var frontIndex = 0; frontIndex < sharedDesign.Fronts.Count && frontIndex < envelopeFronts.Count; frontIndex++)
            {
                sharedDesign.Fronts[frontIndex].PalletsDeep = envelopeFronts[frontIndex].PalletsDeep;
            }

            // Rear peralte per front x level (already canonical), and ONLY deactivations materialized into the OffCells.
            // I-41 anade, en el MISMO recorrido y sobre la MISMA entrada por frente, el fondo por defecto, el override
            // de cada celda y su flag de tarima, de modo que las cuatro listas no pueden desalinearse por nivel.
            for (var frontIndex = 0; frontIndex < matrix.Count; frontIndex++)
            {
                var levelsF = Math.Max(1, matrix.Fronts[frontIndex].LoadLevels);
                var config = new PushBackFrontConfig
                {
                    // El fondo POR DEFECTO es el que el usuario escribe en «Fondos frente»; la envolvente es derivada
                    // y vive en la estructura. Guardar los dos es lo que hace el round trip reversible.
                    DefaultPalletsDeep = Math.Max(
                        PushBackCellDepth.MinimumPalletsDeep, matrix.Fronts[frontIndex].PalletsDeep)
                };
                for (var level = 0; level < levelsF; level++)
                {
                    var cell = state.Cell(frontIndex, level);
                    config.HighEndBeamPeraltes.Add(cell.HighEndBeamPeralte);
                    config.PalletsDeepOverrides.Add(
                        cell.PalletsDeepOverride.HasValue
                        && cell.PalletsDeepOverride.Value >= PushBackCellDepth.MinimumPalletsDeep
                            ? cell.PalletsDeepOverride
                            : null);
                    // Solo el TRUE es intencion: el default legacy es false y se deja implicito, igual que el tope
                    // posterior solo materializa desactivaciones.
                    config.DrawPallets.Add(cell.DrawPallet ? true : (bool?)null);
                    if (!cell.RearTopeEnabled)
                    {
                        design.RearTope.Disable(frontIndex, level);
                    }
                }

                design.Fronts.Add(config);
            }

            return design;
        }

        /// <summary>Assemble the canonical design, resolve once and build the BOM and four plans. See the overload.</summary>
        public PushBackEditorComputation Build(PushBackEditorState state, PushBackEditorInputs inputs)
            => Build(state, inputs, forceRebuild: false);

        /// <summary>Assemble, resolve once with <see cref="PushBackResolver"/> and build the BOM and the four plans. A failure
        /// yields an invalid computation carrying the message, with the geometry left null (and the baseline untouched).</summary>
        public PushBackEditorComputation Build(PushBackEditorState state, PushBackEditorInputs inputs, bool forceRebuild)
        {
            try
            {
                return BuildFrom(BuildDesign(state, inputs, forceRebuild));
            }
            catch (Exception ex)
            {
                return PushBackEditorComputation.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Resuelve un diseno YA armado y construye el BOM y las vistas. Existe para que el editor COMPUESTO de I-42
        /// —que arma su diseno con <see cref="PushBackCompositeEditorAssembler"/>— comparta EXACTAMENTE este camino, y
        /// no acabe con un segundo constructor de vistas que pueda divergir del de un solo sentido.
        /// </summary>
        /// <param name="frontalSide">
        /// I-42: el lado cuyos dos cortes frontales se construyen. Un corte frontal es de UN lado —mira a uno de los
        /// dos pasillos—, asi que en un rack compuesto sigue al selector de lado del editor. En un rack de un solo
        /// sentido el parametro no cambia nada.
        /// </param>
        public PushBackEditorComputation BuildFrom(
            PushBackDesign design, PushBackSide frontalSide = PushBackSide.A)
        {
            try
            {
                var system = pushResolver.Resolve(design);
                var bom = PushBackBomBuilder.Build(system, catalog);
                var lateral = lateralBuilder.Build(system, catalog);
                var cortes = lateralBuilder.Cortes(system, catalog); // the per-post lateral sections, computed once here
                // I-42 (A3-PREVIEW, contrato del dueño) — LA VISTA PREVIA CONSUME EL MISMO CONSTRUCTOR QUE EL
                // DIBUJO. Los dos cortes compuestos se armaban aqui llamando a PushBackCompositeFrontal.Build
                // DIRECTAMENTE, es decir saltandose el envoltorio del constructor final, que es quien retira la
                // seguridad local del marco copiado y proyecta la FISICA del lado y el extremo pedidos
                // (PushBackDefensePlan / PushBackBootPlan). El resultado era una vista previa que enseñaba piezas
                // que el rack insertado no dibuja: medido, con la defensa de A declarada y la de B en «Ninguno», el
                // corte de entrada de B mostraba las tres defensas de A y el dibujo final ninguna; con las botas de
                // A en «Entrada/Salida» y las de B en «Posterior», la vista previa de B las ponia en su entrada y el
                // dibujo en su posterior. El envoltorio decide tambien el caso de un solo sentido, asi que aqui ya
                // no queda ninguna bifurcacion que pueda divergir.
                var entradaSalida = frontalBuilder.BuildPlan(
                    system, catalog, PushBackFrontalEnd.EntradaSalida, frontalSide);
                var posterior = frontalBuilder.BuildPlan(
                    system, catalog, PushBackFrontalEnd.Posterior, frontalSide);
                var planta = plantaBuilder.BuildPlan(system, catalog);
                return PushBackEditorComputation.Success(design, system, bom, lateral, entradaSalida, posterior, planta, cortes);
            }
            catch (Exception ex)
            {
                return PushBackEditorComputation.Failure(ex.Message);
            }
        }

        /// <summary>The header height from the load inputs, mirroring the dynamic window's ComputeHeaderHeight (no manual
        /// override in increment 2): load height = pallet height; the slope run = tarimas x fondo + 12".</summary>
        private static double ComputeHeaderHeight(PalletSpecification pallet, int palletsDeep, int levels, double firstLevel, double beamDepth)
        {
            var totalDepth = palletsDeep * pallet.Depth + 2.0 * DynamicRackDefaults.HeaderEndAllowance;
            return DynamicHeaderHeightCalculator.Calculate(pallet.Height, levels, firstLevel, beamDepth, totalDepth).HeaderHeight;
        }

        /// <summary>An INDEPENDENT copy of a resolved structure system, preserving its modules and header configurations, via
        /// the resolver's own snapshot/resolve round trip. The next recompute mutates this copy, never the baseline.</summary>
        private DynamicRackSystem CopyStructureSystem(DynamicRackSystem system)
        {
            var loadLevels = Math.Max(1, system.LoadBeamLevels.Count);
            var firstLevel = system.Fronts.FirstOrDefault()?.FirstLevelHeight ?? DynamicRackDefaults.DefaultFirstLevelHeight;
            var beamDepth = system.InOutBeamDepth > 0.0 ? system.InOutBeamDepth : DynamicRackDefaults.DefaultBeamDepth;
            var postId = system.Modules
                .FirstOrDefault(module => module != null && module.IsHeader
                    && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
            return dynamicResolver.Resolve(dynamicResolver.Snapshot(system, loadLevels, firstLevel, beamDepth, postId)).System;
        }

        private static PalletSpecification ClonePallet(PalletSpecification pallet)
            => pallet == null
                ? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);
    }
}
