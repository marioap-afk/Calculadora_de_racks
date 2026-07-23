using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Recompute core of the Push Back editor. It COMPOSES the shared dynamic assembly — <see cref="DynamicEditorDesignAssembler.BuildDesign"/>
    /// builds the persisted <see cref="DynamicRackDesign"/> from the editable matrix, so no dynamic logic is copied — and
    /// then adds Push Back's own bits: the rear beam peralte per front x level, the rear-tope OffCells (only DEACTIVATIONS
    /// are materialized; an active cell is never a positive entry) and the authorized (GUIA-free) safety. It resolves ONCE
    /// with <see cref="PushBackResolver"/> and hands the resolved system to the I-18a BOM and view builders. Pure: no WPF,
    /// AutoCAD, window or drawing lives here.
    /// </summary>
    public sealed class PushBackEditorDesignAssembler
    {
        private readonly RackCatalog catalog;
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
            var builder = new DynamicRackSystemBuilder(this.catalog);
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
        /// Deep copies of <paramref name="requested"/> with entrance GUIDES removed (Push Back admits none); the input is
        /// never mutated. This is the authorized, editable safety set the increment-3 window may offer and persist.
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> AuthorizedSafety(IEnumerable<SelectiveSafetySelection> requested)
            => safetyAuthority.Authorize(requested);

        /// <summary>
        /// Assemble the persisted Push Back design from the editor state and the rack-wide inputs, WITHOUT resolving: seed a
        /// thin dynamic design from the matrix, resolve it once to obtain a structural system, let the dynamic assembler
        /// build the shared design (modules + fronts + annotations + drawable safety), wrap it, copy the rear peraltes per
        /// front x level, and materialize the rear-tope OffCells (only deactivated cells). The authorized (GUIA-free) safety
        /// rides on the shared design. The result is a pure DTO that can be persisted or resolved.
        /// </summary>
        public PushBackDesign BuildDesign(PushBackEditorState state, PushBackEditorInputs inputs)
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
            var beamDepth = editorInputs.BeamDepth > 0.0 ? editorInputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth;
            var postId = string.IsNullOrWhiteSpace(editorInputs.PostCatalogId)
                ? catalog.Defaults?.Post
                : editorInputs.PostCatalogId;
            var palletTolerance = editorInputs.PalletTolerance > 0.0
                ? editorInputs.PalletTolerance
                : DynamicRackDefaults.DefaultPalletTolerance;
            var basePalletsDeep = Math.Max(2, editorInputs.PalletsDeep);
            var authorizedSafety = AuthorizedSafety(editorInputs.SafetySelections);

            var frontDesigns = matrix.BuildFrontDesigns();
            var depthLayout = DynamicDepthGeometry.Resolve(frontDesigns, basePalletsDeep);

            // Seed a thin dynamic design and resolve it once to obtain a structural system to snapshot; the dynamic
            // assembler then produces the persisted shared design (with resolved modules + the matrix's fronts).
            var seed = new DynamicRackDesign
            {
                Pallet = ClonePallet(editorInputs.Pallet),
                PalletsDeep = basePalletsDeep,
                LoadLevels = levels,
                FirstLevelHeight = firstLevel,
                BeamDepth = beamDepth,
                HeaderPostCatalogId = postId,
                PostPeralte = editorInputs.PostPeralte,
                PalletTolerance = palletTolerance
            };
            foreach (var frontDesign in frontDesigns)
            {
                seed.Fronts.Add(frontDesign);
            }

            var structureSystem = dynamicResolver.Resolve(seed).System;
            var sharedDesign = dynamicAssembler.BuildDesign(
                structureSystem,
                matrix,
                levels,
                firstLevel,
                beamDepth,
                postId,
                depthLayout.TotalPositions,
                editorInputs.PostPeralte,
                palletTolerance,
                editorInputs.Annotations ?? new DynamicAnnotationOptions(),
                authorizedSafety);

            var design = new PushBackDesign
            {
                Structure = sharedDesign,
                LegacyHighEndBeamPeralte = PushBackDefaults.HighEndBeamDefaultPeralte,
                RearTope = new PushBackRearTopeConfig { Saque = state.RearTopeSaque }
            };

            // Rear peralte per front x level, and ONLY deactivations materialized into the rear-tope OffCells.
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

        /// <summary>Assemble, resolve once with <see cref="PushBackResolver"/> and build the BOM and the four plans. A
        /// failure yields an invalid computation carrying the message, with the geometry left null.</summary>
        public PushBackEditorComputation Build(PushBackEditorState state, PushBackEditorInputs inputs)
        {
            try
            {
                var design = BuildDesign(state, inputs);
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

        private static PalletSpecification ClonePallet(PalletSpecification pallet)
            => pallet == null
                ? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);
    }
}
