using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
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
        }

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

            var depthLayout = DynamicDepthGeometry.Resolve(matrix.BuildFrontDesigns(), Math.Max(2, editorInputs.PalletsDeep));
            var palletsDeep = depthLayout.TotalPositions;
            var headerHeight = ComputeHeaderHeight(pallet, palletsDeep, levels, firstLevel, beamDepth);

            // Dynamic recompute cycle (composed, never modified): rebuild only on a pallet/fondos change (or a forced reset);
            // otherwise reuse a COPY of the loaded baseline so custom modules and manual fondos survive. The baseline itself is
            // never mutated here — only AcceptComputation advances it, so a failed recompute cannot corrupt it.
            var baseline = state.WorkingBaseline?.Structure;
            var mustRebuild = forceRebuild || DynamicEditorDesignAssembler.MustRebuild(baseline, pallet, depthLayout);
            var savedFondos = !forceRebuild && mustRebuild && baseline != null
                ? dynamicAssembler.SnapshotHeaderFondos(baseline)
                : null;

            DynamicRackSystem system;
            if (mustRebuild)
            {
                system = builder.BuildDefault(pallet, depthLayout, RackFrameTemplateCatalog.Default, postId, headerHeight, postPeralte);
                if (savedFondos != null)
                {
                    dynamicAssembler.RestoreHeaderFondos(system, savedFondos, headerHeight, postId);
                }
            }
            else
            {
                system = CopyStructureSystem(baseline);
                dynamicAssembler.UpdateHeaderHeightInPlace(system, headerHeight, postId);
            }

            builder.ApplyPostPeralte(system, postPeralte);

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

            var design = new PushBackDesign
            {
                Structure = sharedDesign,
                LegacyHighEndBeamPeralte = PushBackDefaults.HighEndBeamDefaultPeralte,
                RearTope = new PushBackRearTopeConfig { Saque = state.RearTopeSaque }
            };

            // Rear peralte per front x level (already canonical), and ONLY deactivations materialized into the OffCells.
            for (var frontIndex = 0; frontIndex < matrix.Count; frontIndex++)
            {
                var levelsF = Math.Max(1, matrix.Fronts[frontIndex].LoadLevels);
                var config = new PushBackFrontConfig();
                for (var level = 0; level < levelsF; level++)
                {
                    var cell = state.Cell(frontIndex, level);
                    config.HighEndBeamPeraltes.Add(cell.HighEndBeamPeralte);
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
                var design = BuildDesign(state, inputs, forceRebuild);
                var system = pushResolver.Resolve(design);
                var bom = PushBackBomBuilder.Build(system, catalog);
                var lateral = lateralBuilder.Build(system, catalog);
                var entradaSalida = frontalBuilder.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida);
                var posterior = frontalBuilder.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior);
                var planta = plantaBuilder.BuildPlan(system, catalog);
                return PushBackEditorComputation.Success(design, system, bom, lateral, entradaSalida, posterior, planta);
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
