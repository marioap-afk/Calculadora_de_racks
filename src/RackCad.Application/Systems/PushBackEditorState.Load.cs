using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Load / restore paths of <see cref="PushBackEditorState"/>. There is exactly ONE rebuild implementation: a resolved
    /// system is snapshotted back to a design and both entry points converge on the design path, so the matrix, the per-cell
    /// peraltes and the rear topes are always the canonical RESOLVED values. GUIA selections never survive (the resolver
    /// strips them). Every load returns the rack-wide inputs recovered from the design so the window can repopulate its
    /// shared panels. The source design/system is never mutated.
    /// </summary>
    public sealed partial class PushBackEditorState
    {
        /// <summary>Reset to a brand-new design: one dynamic-default front, rear peralte 3.5 and active topes, a valid
        /// primary selection on the first cell. Returns the rack-wide inputs a new Push Back system opens with.</summary>
        public PushBackEditorInputs LoadNew()
        {
            structure.RestoreFromResolved(Enumerable.Empty<DynamicRackFront>()); // falls back to one default front
            pushFronts.Clear();
            RearTopeSaque = PushBackDefaults.RearTopeSaque;
            SyncPushConfig();
            structure.NormalizeSelection();
            return PushBackEditorInputs.NewDesign();
        }

        /// <summary>
        /// Load a persisted <see cref="PushBackDesign"/>: resolve it once, rebuild the matrix from the resolved fronts
        /// (conserving different fondo counts, DepthStartPosition, per-front level counts, first-level heights, length
        /// overrides, IN/OUT and intermediate beams) and rebuild the parallel Push Back configuration from the resolved
        /// high-end peraltes and the rear-tope OffCells. Returns the recovered rack-wide inputs.
        /// </summary>
        public PushBackEditorInputs LoadFromDesign(PushBackDesign design, PushBackResolver resolver)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            var system = resolver.Resolve(design);
            RebuildFromResolved(system);
            return RecoverInputs(design, system);
        }

        /// <summary>
        /// Load a resolved <see cref="PushBackSystem"/> by snapshotting it back to a design and taking the single design
        /// load path (one restore implementation, no duplicated logic). Returns the recovered rack-wide inputs.
        /// </summary>
        public PushBackEditorInputs LoadFromSystem(PushBackSystem system, PushBackResolver resolver)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            return LoadFromDesign(resolver.Snapshot(system), resolver);
        }

        /// <summary>Rebuild both authorities from a resolved system: the matrix from the resolved fronts, the parallel
        /// config from the resolved per-cell peraltes and the rear-tope activation (canonical, GUIA already stripped).</summary>
        private void RebuildFromResolved(PushBackSystem system)
        {
            var fronts = system.Structure?.Fronts ?? new List<DynamicRackFront>();
            structure.RestoreFromResolved(fronts);

            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            RearTopeSaque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;

            pushFronts.Clear();
            for (var frontIndex = 0; frontIndex < structure.Count; frontIndex++)
            {
                var levels = Math.Max(1, structure.Fronts[frontIndex].LoadLevels);
                var front = new PushBackEditorFront();
                for (var level = 0; level < levels; level++)
                {
                    front.Cells.Add(new PushBackEditorCell
                    {
                        HighEndBeamPeralte = system.HighEndBeamPeralteAt(frontIndex, level),
                        RearTopeEnabled = rearTope.At(frontIndex, level)
                    });
                }

                pushFronts.Add(front);
            }

            SyncPushConfig();
            structure.NormalizeSelection();
        }

        /// <summary>Recover the rack-wide inputs from the persisted design's shared structure and the resolved (GUIA-free)
        /// safety, so the window can repopulate its shared panels. Independent copies; the design is not mutated.</summary>
        private static PushBackEditorInputs RecoverInputs(PushBackDesign design, PushBackSystem system)
        {
            var s = design.Structure ?? new DynamicRackDesign();
            var inputs = new PushBackEditorInputs
            {
                Pallet = ClonePallet(s.Pallet),
                PalletsDeep = Math.Max(2, s.PalletsDeep),
                PostCatalogId = s.HeaderPostCatalogId,
                PostPeralte = s.PostPeralte,
                PalletTolerance = s.PalletTolerance > 0.0 ? s.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance,
                BeamDepth = s.BeamDepth > 0.0 ? s.BeamDepth : DynamicRackDefaults.DefaultBeamDepth,
                Annotations = new DynamicAnnotationOptions
                {
                    NumberFronts = s.NumberFronts,
                    NumberLevels = s.NumberLevels,
                    DrawRackName = s.DrawRackName,
                    AnnotationScale = s.AnnotationScale > 0.0 ? s.AnnotationScale : 1.0,
                    Dimensions = s.Dimensions,
                    DimensionStyle = s.DimensionStyle
                }
            };

            foreach (var safety in system.SafetySelections ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (safety != null)
                {
                    inputs.SafetySelections.Add(safety.DeepCopy());
                }
            }

            return inputs;
        }

        private static PalletSpecification ClonePallet(PalletSpecification pallet)
            => pallet == null
                ? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);
    }
}
