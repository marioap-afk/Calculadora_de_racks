using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure design→system boundary for Push Back. It REUSES <see cref="DynamicRackSystemResolver"/> for the shared
    /// structure (headers, separators, derived posts, per-front depth ranges, and the load-beam elevations already
    /// computed with the 7/16"/ft slope and the 2" troquel snap) and then resolves Push Back's OWN bits:
    /// <list type="bullet">
    /// <item>the high-end (rear) beam PERALTE PER FRONT AND LEVEL — the requested value when the catalog allows it,
    /// else the design's legacy rack-wide fallback, else the EXPLICIT default 3.5 (never silently "the first row");</item>
    /// <item>the entrance-side safety selections with entrance GUIDES REMOVED (Push Back admits no GUIA).</item>
    /// </list>
    /// No dynamic behaviour is altered: the dynamic resolver is composed, not modified.
    /// </summary>
    public sealed class PushBackResolver
    {
        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemResolver structureResolver;

        public PushBackResolver(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            structureResolver = new DynamicRackSystemResolver(this.catalog);
        }

        public PushBackSystem Resolve(PushBackDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            var structure = structureResolver.Resolve(design.Structure ?? new DynamicRackDesign()).System;

            var system = new PushBackSystem
            {
                Structure = structure,
                HighEndBeamCatalogId = PushBackDefaults.HighEndBeamCatalogId,
                RearTope = design.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };

            // High-end (rear) beam peralte per front x level, aligned by index with the resolved fronts.
            var allowed = AllowedHighEndPeraltes();
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                var frontConfig = design.FrontConfig(frontIndex);
                var resolved = new PushBackResolvedFront();
                var levels = Math.Max(1, front.LoadLevels);
                for (var level = 0; level < levels; level++)
                {
                    resolved.HighEndBeamPeraltes.Add(
                        ResolvePeralte(frontConfig?.PeralteAt(level), design.LegacyHighEndBeamPeralte, allowed));
                }

                system.HighEndBeams.Add(resolved);
            }

            // Safety authority: Push Back admits every applicable family EXCEPT entrance guides (removed), and normal
            // safety only at the LOW (entrance/exit) end — never the rear. Each authorized selection is restricted to the
            // low end (Left = the exit end in the dynamic builders) so a "Both" selection materializes once, on the low
            // side, in every view and in the BOM. The GUIA-free, low-only set is exposed on the Push Back system AND
            // written back onto the shared structure, so the dynamic builders — used later as a BLACK BOX — never emit a
            // guide, and never emit rear-end safety.
            var authorized = (structure.SafetySelections ?? Enumerable.Empty<SelectiveSafetySelection>())
                .Where(selection => selection != null && !IsEntranceGuide(selection))
                .Select(selection =>
                {
                    var copy = selection.DeepCopy();
                    RestrictToLowEnd(copy);
                    return copy;
                })
                .ToList();
            foreach (var selection in authorized)
            {
                system.SafetySelections.Add(selection);
            }

            structure.SafetySelections.Clear();
            foreach (var selection in authorized)
            {
                structure.SafetySelections.Add(selection.DeepCopy());
            }

            return system;
        }

        /// <summary>
        /// Restrict a safety selection to the LOW (entrance/exit) end only. A two-ended or rear (Right) side collapses to
        /// Left (the exit end); per-post overrides are cleared so every post uses the low side; a forklift defense keeps
        /// only its exit length. Botas/laterales/desviadores then draw exactly once, on the low side.
        /// </summary>
        private static void RestrictToLowEnd(SelectiveSafetySelection selection)
        {
            if (selection.Side == SafetySide.Both || selection.Side == SafetySide.Right)
            {
                selection.Side = SafetySide.Left;
            }

            selection.PostSides.Clear();
            foreach (var post in selection.DefensaPosts)
            {
                if (post != null)
                {
                    post.EntranceLength = 0.0;
                }
            }
        }

        /// <summary>
        /// Captures the editable intent of a resolved Push Back system back into a <see cref="PushBackDesign"/>, with
        /// INDEPENDENT copies, so the round trip Design → Resolve → Snapshot → Resolve preserves geometry and intent.
        /// It reuses the dynamic resolver's snapshot for the shared structure, then re-attaches Push Back's own bits:
        /// the high-end beam peralte PER FRONT AND LEVEL, the rear-tope selection, and the allowed safety with entrance
        /// GUIDES already excluded (a GUIA can never reappear in the snapshot, so it cannot reach a re-resolve, a plan,
        /// a BOM or a document generated from the snapshot).
        /// </summary>
        public PushBackDesign Snapshot(PushBackSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var structure = system.Structure ?? new DynamicRackSystem();
            var loadLevels = Math.Max(1, structure.LoadBeamLevels.Count);
            var firstLevelHeight = structure.Fronts.FirstOrDefault()?.FirstLevelHeight
                ?? DynamicRackDefaults.DefaultFirstLevelHeight;
            var beamDepth = structure.InOutBeamDepth > 0.0 ? structure.InOutBeamDepth : DynamicRackDefaults.DefaultBeamDepth;
            var postId = structure.Modules
                .FirstOrDefault(module => module != null && module.IsHeader
                    && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId;

            var structureDesign = structureResolver.Snapshot(structure, loadLevels, firstLevelHeight, beamDepth, postId);

            // The shared structure's safety may still carry a guide; the design's allowed safety is the GUIA-free set.
            structureDesign.SafetySelections.Clear();
            foreach (var selection in system.SafetySelections)
            {
                if (selection != null)
                {
                    structureDesign.SafetySelections.Add(selection.DeepCopy());
                }
            }

            var legacy = system.HighEndBeams.Count > 0 && system.HighEndBeams[0].HighEndBeamPeraltes.Count > 0
                ? system.HighEndBeams[0].HighEndBeamPeraltes[0]
                : PushBackDefaults.HighEndBeamDefaultPeralte;

            var design = new PushBackDesign
            {
                Structure = structureDesign,
                LegacyHighEndBeamPeralte = legacy,
                RearTope = system.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };

            foreach (var resolvedFront in system.HighEndBeams)
            {
                var config = new PushBackFrontConfig();
                foreach (var peralte in resolvedFront.HighEndBeamPeraltes)
                {
                    config.HighEndBeamPeraltes.Add(peralte);
                }

                design.Fronts.Add(config);
            }

            return design;
        }

        /// <summary>
        /// The high-end beam PERALTE at one cell: the requested per-cell value if the catalog allows it, else the
        /// design's legacy rack-wide fallback (if allowed), else the EXPLICIT default 3.5 (if allowed), else the first
        /// catalog value. 3.5 is a rule, never silently "allowed[0]".
        /// </summary>
        public double ResolveHighEndBeamPeralte(double? requested, double legacyFallback)
            => ResolvePeralte(requested, legacyFallback, AllowedHighEndPeraltes());

        /// <summary>Single-argument overload: resolve a per-cell request with the explicit 3.5 default as the fallback.</summary>
        public double ResolveHighEndBeamPeralte(double requested)
            => ResolveHighEndBeamPeralte(requested > 0.0 ? requested : (double?)null, PushBackDefaults.HighEndBeamDefaultPeralte);

        private static double ResolvePeralte(double? requested, double legacyFallback, IReadOnlyList<double> allowed)
        {
            bool InList(double value) => allowed.Any(candidate => Math.Abs(candidate - value) < 1e-6);

            if (requested.HasValue && requested.Value > 0.0 && InList(requested.Value))
            {
                return requested.Value;
            }

            if (legacyFallback > 0.0 && InList(legacyFallback))
            {
                return legacyFallback;
            }

            if (InList(PushBackDefaults.HighEndBeamDefaultPeralte))
            {
                return PushBackDefaults.HighEndBeamDefaultPeralte;
            }

            return allowed.Count > 0 ? allowed[0] : PushBackDefaults.HighEndBeamDefaultPeralte;
        }

        /// <summary>True when <paramref name="selection"/>'s catalog element is an entrance guide (type GUIA).</summary>
        public bool IsEntranceGuide(SelectiveSafetySelection selection)
        {
            if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return false;
            }

            var element = catalog?.SafetyElements?.FirstOrDefault(entry => entry != null
                && string.Equals(entry.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType);
        }

        /// <summary>Catalog-allowed peraltes of the high-end beam (LARGUERO_ESCALON_TROQUEL_REDONDO), read like the intermediate beam.</summary>
        public IReadOnlyList<double> AllowedHighEndPeraltes()
        {
            var profile = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(
                entry?.Id,
                PushBackDefaults.HighEndBeamCatalogId,
                StringComparison.OrdinalIgnoreCase));
            return PeralteList.Parse(profile?.Peraltes);
        }
    }
}
