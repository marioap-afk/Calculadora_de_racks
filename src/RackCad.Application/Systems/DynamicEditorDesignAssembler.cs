using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The dynamic editor's recompute core, extracted from the window (I-21): it decides when a full rebuild is needed,
    /// preserves the user's per-header fondos across a rebuild, updates calculated cabecera heights in place, and assembles
    /// the persisted <see cref="DynamicRackDesign"/> from the resolved system, the editable <see cref="DynamicFrontMatrix"/>
    /// and the scalar inputs. It composes the existing builder/resolver instead of duplicating their geometry. The window
    /// keeps only WPF: reading fields, choosing the header height, and drawing. No behavior changes vs. the inline version.
    /// </summary>
    public sealed class DynamicEditorDesignAssembler
    {
        private readonly DynamicRackSystemBuilder builder;
        private readonly DynamicRackSystemResolver resolver;
        private readonly RackFrameConfigurationFactory factory;

        public DynamicEditorDesignAssembler(
            RackCatalog catalog,
            DynamicRackSystemBuilder builder,
            DynamicRackSystemResolver resolver)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            factory = new RackFrameConfigurationFactory(catalog);
        }

        /// <summary>
        /// Whether the module SEQUENCE must be rebuilt from scratch: only a pallet change or a change in the number of
        /// fondos does that. Height-only inputs update in place so per-module edits survive. (The window ORs this with its
        /// explicit "restaurar estándar" flag.)
        /// </summary>
        public static bool MustRebuild(DynamicRackSystem system, PalletSpecification pallet, DynamicDepthLayout depthLayout)
            => system == null
               || system.Modules.Count == 0
               || !SamePallet(system.Pallet, pallet)
               || !DynamicDepthGeometry.Matches(system, depthLayout);

        private static bool SamePallet(PalletSpecification a, PalletSpecification b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return Math.Abs(a.Front - b.Front) < 1e-6
                && Math.Abs(a.Depth - b.Depth) < 1e-6
                && Math.Abs(a.Height - b.Height) < 1e-6
                && Math.Abs(a.Weight - b.Weight) < 1e-6;
        }

        /// <summary>Snapshot each header module's custom fondo, in header order (null = default), so a full rebuild can
        /// restore the user's per-header fondos afterwards.</summary>
        public IReadOnlyList<double?> SnapshotHeaderFondos(DynamicRackSystem system)
        {
            var fondos = new List<double?>();
            if (system == null)
            {
                return fondos;
            }

            foreach (var module in system.Modules.Where(m => m.IsHeader))
            {
                fondos.Add(module.IsManualOverride && module.Length > 0.0 ? module.Length : (double?)null);
            }

            return fondos;
        }

        /// <summary>Re-apply snapshot fondos to the freshly-rebuilt header modules by header order (only where the header
        /// still exists), rebuilding each restored cabecera at the NEW height. Returns how many were restored.</summary>
        public int RestoreHeaderFondos(
            DynamicRackSystem system,
            IReadOnlyList<double?> savedFondos,
            double newHeight,
            string postId)
        {
            if (savedFondos == null || savedFondos.Count == 0 || system == null)
            {
                return 0;
            }

            var ordinal = 0;
            var restored = 0;

            foreach (var module in system.Modules.Where(m => m.IsHeader))
            {
                if (ordinal < savedFondos.Count && savedFondos[ordinal].HasValue)
                {
                    var fondo = savedFondos[ordinal].Value;
                    module.Length = fondo;
                    module.IsManualOverride = true;
                    module.IsCalculated = false;
                    module.UseCalculatedHeaderConfiguration = true;
                    module.AssociatedFrameConfiguration = factory.Build(RackFrameTemplateCatalog.Default, postId, newHeight, fondo);
                    restored++;
                }

                ordinal++;
            }

            if (restored > 0)
            {
                system.RecalculatePositions();
                builder.Refresh(system);
            }

            return restored;
        }

        /// <summary>Update calculated cabeceras on the EXISTING modules to a new height. Custom cabeceras remain untouched,
        /// matching the selective editor's contract: calculated inputs may regenerate defaults but never overwrite edits.</summary>
        public void UpdateHeaderHeightInPlace(DynamicRackSystem system, double newHeight, string postId)
        {
            if (system == null)
            {
                return;
            }

            foreach (var module in system.Modules)
            {
                if (!module.IsHeader || !module.UseCalculatedHeaderConfiguration)
                {
                    continue;
                }

                var fondo = module.Length > 0.0 ? module.Length : system.DefaultHeaderLength;
                module.AssociatedFrameConfiguration = factory.Build(RackFrameTemplateCatalog.Default, postId, newHeight, fondo);
            }

            builder.Refresh(system);
        }

        /// <summary>
        /// Assemble the persisted design from the resolved system, the editable fronts and the scalar inputs (was the
        /// snapshot+fill block of Recompose). The result owns independent header configurations and can be resolved again.
        /// </summary>
        public DynamicRackDesign BuildDesign(
            DynamicRackSystem system,
            DynamicFrontMatrix matrix,
            int levels,
            double firstLevel,
            double beamDepth,
            string headerPostCatalogId,
            int palletsDeep,
            double postPeralte,
            double palletTolerance,
            DynamicAnnotationOptions annotations,
            IEnumerable<SelectiveSafetySelection> safetySelections)
        {
            var design = resolver.Snapshot(system, levels, firstLevel, beamDepth, headerPostCatalogId);
            design.PalletsDeep = palletsDeep;
            design.PostPeralte = postPeralte;
            design.PalletTolerance = palletTolerance;
            design.IntermediateBeamDepths.Clear();
            design.Fronts.Clear();
            foreach (var frontDesign in matrix.BuildFrontDesigns())
            {
                design.Fronts.Add(frontDesign);
            }

            var options = annotations ?? new DynamicAnnotationOptions();
            design.NumberFronts = options.NumberFronts;
            design.NumberLevels = options.NumberLevels;
            design.DrawRackName = options.DrawRackName;
            design.AnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;
            design.Dimensions = options.Dimensions;
            design.DimensionStyle = options.DimensionStyle;
            DynamicEditorSafety.CopyDrawable(design.SafetySelections, safetySelections);
            return design;
        }
    }
}
