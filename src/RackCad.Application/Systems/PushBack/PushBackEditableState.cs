using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>Whether a design is a rack whose editable state survives a round trip without losing anything.</summary>
    public enum PushBackEditableSupport
    {
        /// <summary>Every part of the design has a place in the editable state.</summary>
        Supported = 0,

        /// <summary>A one-way rack whose structure keeps modules or lines of a parked composite tail: the tail has no
        /// place in a one-way state, so it is outside the lossless guarantee.</summary>
        DormantCompositeTail = 1
    }

    /// <summary>
    /// The editable state of a Push Back rack outside any window: a GROUPING of the state that already exists, never a
    /// parallel model. It holds the composite editor state (both sides, the composite intent, the parked tail and the
    /// module authority of each side) plus the rack-wide values the editor window used to keep in its own fields and
    /// controls — the general pallet and its unit, the fondos, the post, the tolerance, the beam depth, the annotation
    /// options, the dimension policy, the six advanced scopes, the safety selections and the name.
    /// <para>
    /// It loads from a design the way the editor loads, produces the inputs of a recompute (<see cref="BuildInputs"/>),
    /// and is copied and compared by <see cref="PushBackEditableStateComparer"/>. It is a pure Application type: no
    /// window, no drawing host, no catalog IO.
    /// </para>
    /// </summary>
    public sealed class PushBackEditableState
    {
        public PushBackEditableState()
        {
            Editor = new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState());
            Pallet = PushBackEditorInputs.NewDesign().Pallet;
            PalletsDeep = DynamicRackDefaults.DefaultPalletsDeep;
            PostPeralte = PushBackEditorInputs.NewDesign().PostPeralte;
            PalletTolerance = DynamicRackDefaults.DefaultPalletTolerance;
            BeamDepth = DynamicRackDefaults.DefaultBeamDepth;
            AnnotationScale = 1.0;
            FirstLevelDatum = (int)RackFirstLevelDatumMode.LowestUsablePunch;
            Dimensions = DimensionDetail.None;
            Advanced = PushBackEditorInputs.NewDesign();
        }

        /// <summary>The composite editor state: both sides, the composite intent, the parked tail and the selection.</summary>
        public PushBackCompositeEditorState Editor { get; }

        /// <summary>The general pallet. It also carries the weight unit the editor SHOWS, which is the one every recompute
        /// uses (see <see cref="ResolveWeightUnit"/>).</summary>
        public PalletSpecification Pallet { get; set; }

        public int PalletsDeep { get; set; }
        public string PostCatalogId { get; set; }
        public double PostPeralte { get; set; }
        public double PalletTolerance { get; set; }
        public double BeamDepth { get; set; }
        public double AnnotationScale { get; set; }

        /// <summary>Where «Alto 1er nivel» is measured from; transported, never invented.</summary>
        public int? FirstLevelDatum { get; set; }

        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public DimensionDetail Dimensions { get; set; }

        /// <summary>The named dimension style the loaded design carried; it travels in every recompute.</summary>
        public string DimensionStyle { get; set; }

        /// <summary>The per-view dimension policy the loaded design carried; <c>null</c> is the legacy one.</summary>
        public DimensionViewVisibility? LoadedDimensionViews { get; set; }

        /// <summary>Whether the user changed one of the three view checks since the last load.</summary>
        public bool DimensionViewsTouched { get; set; }

        public bool DimensionsFrontal { get; set; }
        public bool DimensionsLateral { get; set; }
        public bool DimensionsPlanta { get; set; }

        /// <summary>The carrier of the six advanced rack-wide scopes (only those six are meaningful).</summary>
        public PushBackEditorInputs Advanced { get; }

        public List<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>The logical name of the rack (the identity is the GUID, which lives elsewhere).</summary>
        public string Name { get; set; }

        // ---- Loads ---------------------------------------------------------------------------------------------------

        /// <summary>The editor half of a new rack: one-way, with the first side active. Returns the inputs of the new design.</summary>
        public PushBackEditorInputs LoadNewEditor()
        {
            Editor.SetActiveSide(PushBackSide.A);
            Editor.SetSideBPresent(false);
            Editor.LoadComposite(null);
            return Editor.Active.LoadNew();
        }

        /// <summary>The editor half of loading a design: side A against its own design, then the composite part.</summary>
        public PushBackEditorInputs LoadDesignEditor(PushBackDesign design, PushBackResolver resolver)
        {
            Editor.SetActiveSide(PushBackSide.A);
            Editor.SetSideBPresent(false);
            var inputs = Editor.Active.LoadFromDesign(SideAOnly(design), resolver);
            LoadComposite(design, false, resolver);
            return inputs;
        }

        /// <summary>The editor half of «Restaurar»: like a load, but a parked tail and the blank slots of side B survive.</summary>
        public PushBackEditorInputs RestoreEditor(PushBackDesign lastValidDesign, PushBackResolver resolver)
        {
            Editor.SetActiveSide(PushBackSide.A);
            Editor.SetSideBPresent(false);
            var inputs = Editor.Active.LoadFromDesign(SideAOnly(lastValidDesign), resolver);
            LoadComposite(lastValidDesign, true, resolver);
            return inputs;
        }

        /// <summary>Adopt the rack-wide values a load recovered, with the normalizations the editor applies when it seeds
        /// its controls. Call it after the editor half, once any recompute that load triggers has run.</summary>
        public void AdoptCarriers(PushBackEditorInputs inputs, RackCatalog catalog)
        {
            FirstLevelDatum = inputs.FirstLevelDatum;
            var pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            Pallet = new PalletSpecification(
                pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, ResolveWeightUnit(Pallet?.WeightUnit, pallet.WeightUnit));
            PalletsDeep = Math.Max(2, inputs.PalletsDeep);
            PalletTolerance = inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance;
            PostCatalogId = ResolvePostCatalogId(catalog, inputs.PostCatalogId);
            PostPeralte = inputs.PostPeralte;
            BeamDepth = inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth;

            Advanced.ManualHeaderHeightOverride = inputs.ManualHeaderHeightOverride;
            Advanced.DerivedPostReinforced = inputs.DerivedPostReinforced;
            Advanced.DerivedPostReinforcementHeight = inputs.DerivedPostReinforcementHeight;
            Advanced.DerivedPostHeight = inputs.DerivedPostHeight;
            Advanced.SeparatorCountOverride = inputs.SeparatorCountOverride;
            Advanced.SeparatorSpacingOverride = inputs.SeparatorSpacingOverride;

            var options = inputs.Annotations ?? new DynamicAnnotationOptions();
            DimensionStyle = options.DimensionStyle;
            NumberFronts = options.NumberFronts;
            NumberLevels = options.NumberLevels;
            DrawRackName = options.DrawRackName;
            AnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;
            Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, (int)options.Dimensions));
            LoadedDimensionViews = options.DimensionViews;
            DimensionViewsTouched = false;
            bool ShowsCotas(DimensionViewKind kind)
                => DimensionViewPolicy.EffectiveDetail(DimensionDetail.Minimal, options.DimensionViews, kind) != DimensionDetail.None;
            DimensionsFrontal = ShowsCotas(DimensionViewKind.Frontal);
            DimensionsLateral = ShowsCotas(DimensionViewKind.Lateral);
            DimensionsPlanta = ShowsCotas(DimensionViewKind.Planta);

            SafetySelections.Clear();
            foreach (var safety in inputs.SafetySelections ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (safety != null) SafetySelections.Add(safety.DeepCopy());
            }
        }

        /// <summary>Seed the safety selections a new rack opens with: the catalog defaults, through the one authority that
        /// filters, deep-copies and restricts them.</summary>
        public void SeedDefaultSafety(RackCatalog catalog)
        {
            SafetySelections.Clear();
            SafetySelections.AddRange(new PushBackSafetyAuthority(catalog).Defaults());
        }

        /// <summary>A new rack, for a host that is not the editor window: the editor half, the carriers and no name.</summary>
        public PushBackEditorInputs LoadNew(RackCatalog catalog)
        {
            var inputs = LoadNewEditor();
            AdoptCarriers(inputs, catalog);
            Name = null;
            return inputs;
        }

        /// <summary>A design, for a host that is not the editor window: the editor half, the carriers and the name.</summary>
        public PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver)
        {
            var inputs = LoadDesignEditor(design, resolver);
            AdoptCarriers(inputs, resolver.Catalog);
            Name = rackName;
            return inputs;
        }

        /// <summary>«Restaurar» from the last valid design, for a host that is not the editor window; the name stays.</summary>
        public PushBackEditorInputs Restore(PushBackDesign lastValidDesign, PushBackResolver resolver)
        {
            var inputs = RestoreEditor(lastValidDesign, resolver);
            AdoptCarriers(inputs, resolver.Catalog);
            return inputs;
        }

        /// <summary>
        /// The design of side A ALONE, without the composite part. The editor state is rebuilt from the RESOLVED system and
        /// that of a composite rack is the shared structure — A + gap + B — so side A is loaded against its own design,
        /// exactly as side B is. A design that is not composite returns the SAME object. The copy shares the references of
        /// the design: only SideB and Composite are dropped, and nothing that is shared is mutated on this path.
        /// </summary>
        public static PushBackDesign SideAOnly(PushBackDesign design)
        {
            if (design == null || !design.IsComposite)
            {
                return design;
            }

            var copy = new PushBackDesign
            {
                Structure = PushBackCompositeStructure.SideAStructure(design.Structure),
                LegacyHighEndBeamPeralte = design.LegacyHighEndBeamPeralte,
                RearTope = design.RearTope,
                DefensePieceId = design.DefensePieceId,
            };

            foreach (var front in design.Fronts)
            {
                copy.Fronts.Add(front);
            }

            return copy;
        }

        /// <summary>
        /// Recover the composite intent of a loaded design (save/load, RACKEDITAR, library). Loading another rack erases
        /// what slept; restoring the open one does not (<paramref name="preserveDormantIntent"/>).
        /// </summary>
        private void LoadComposite(PushBackDesign design, bool preserveDormantIntent, PushBackResolver resolver)
        {
            // The tail that is parked from a design is copied field by field and LITERALLY (the same eight fields a
            // wake-up copies), so the state shares nothing mutable with the design it was loaded from. The store is local
            // to this method on purpose: a field would enter the census of the state.
            var store = new RackFrameProjectStore();
            DynamicRackModuleDesign CopyModuleOf(DynamicRackModuleDesign module) => new DynamicRackModuleDesign
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                HeaderConfiguration = store.DeepCopy(module.HeaderConfiguration),
                Notes = module.Notes,
            };

            DynamicHeaderLineOverride CopyLineOf(DynamicHeaderLineOverride line) => new DynamicHeaderLineOverride
            {
                PostIndex = line.PostIndex,
                ModuleId = line.ModuleId,
                Header = store.DeepCopy(line.Header),
            };

            var dormantTail = preserveDormantIntent ? Editor.DormantCompositeTail.ToList() : null;
            var dormantLines = preserveDormantIntent ? Editor.DormantTailLineOverrides.ToList() : null;
            var dormantSlots = preserveDormantIntent
                ? Enumerable.Range(0, Editor.Of(PushBackSide.B).Structure.Count)
                    .Select(slot => Editor.Of(PushBackSide.B).Structure.IsActive(slot))
                    .ToList()
                : null;

            Editor.SetSideBPresent(design != null && design.IsComposite);
            Editor.LoadComposite(design?.Composite);

            // Loading a rack is an EXPLICIT erase of what slept; a park with an empty source erases nothing.
            Editor.ClearDormantTail();
            if (preserveDormantIntent)
            {
                // The sleeping intent comes back whole: its modules, its per-line configurations and the presence of its
                // slots, which an effective one-way design cannot declare.
                Editor.ParkDormantTail(dormantTail, dormantLines);
                var matrix = Editor.Of(PushBackSide.B).Structure;
                for (var slot = 0; slot < dormantSlots.Count && slot < matrix.Count; slot++)
                {
                    matrix.Fronts[slot].IsActive = dormantSlots[slot];
                }
            }

            var structure = design?.Structure;
            if (structure != null)
            {
                Editor.ParkDormantTail(
                    structure.Modules
                        .Where(module => module != null && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId))
                        .Select(CopyModuleOf)
                        .ToList(),
                    structure.HeaderLineOverrides
                        .Where(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId))
                        .Select(CopyLineOf)
                        .ToList());
            }

            if (design?.SideB != null && design.SideB.IsPresent)
            {
                // Side B is recovered in its own state, by the SAME load path as side A.
                var sideDesign = new PushBackDesign
                {
                    Structure = new DynamicRackDesign
                    {
                        Pallet = design.Structure?.Pallet,
                        PalletsDeep = design.Structure?.PalletsDeep ?? 2,
                        LoadLevels = design.SideB.LoadLevels,
                        FirstLevelHeight = design.SideB.FirstLevelHeight,
                        BeamDepth = design.Structure?.BeamDepth ?? 6.0,
                        HeaderPostCatalogId = design.Structure?.HeaderPostCatalogId,
                        PostPeralte = design.Structure?.PostPeralte ?? 0.0,
                        // Side B is rebuilt with the SAME datum as the document.
                        FirstLevelDatum = design.Structure?.FirstLevelDatum
                    },
                    LegacyHighEndBeamPeralte = design.SideB.LegacyHighEndBeamPeralte,
                    RearTope = design.SideB.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig(),
                    DefensePieceId = design.SideB.DefensePieceId
                };

                // The fronts of side B travel as they come; only an OLDER document brings a null entry, and only for that
                // one is the filler kept, inventing nothing that was not saved.
                var blank = new List<int>();
                for (var slot = 0; slot < design.SideB.Fronts.Count; slot++)
                {
                    var front = design.SideB.Fronts[slot];
                    if (front == null || design.Composite != null && design.Composite.IsSlotAbsentInB(slot))
                    {
                        blank.Add(slot);
                    }

                    sideDesign.Structure.Fronts.Add(front ?? new DynamicRackFrontDesign
                    {
                        PalletCount = 1,
                        LoadLevels = design.SideB.LoadLevels,
                        PalletsDeep = 2,
                        DepthStartPosition = 1,
                        IsActive = false
                    });
                    sideDesign.Fronts.Add(design.SideB.FrontConfig(slot) ?? new PushBackFrontConfig());
                }

                Editor.SideB.LoadFromDesign(sideDesign, resolver);

                // Presence is applied AFTER the matrix is rebuilt: before it, a reopened rack would resurrect side B in
                // the slots the user left blank.
                foreach (var slot in blank)
                {
                    Editor.SetSlotPresent(PushBackSide.B, slot, false);
                }
            }

            // The transverse grid belongs to the RACK also when it comes back from a file: it grows to the larger side,
            // never trimming one and never inventing presence.
            Editor.AlignSlotGrid();
        }

        // ---- Output --------------------------------------------------------------------------------------------------

        /// <summary>
        /// The inputs of a recompute, field by field the ones the editor window reads from its carriers and controls. A NEW
        /// object on every call: the assembler mutates the inputs it receives, so none may be reused.
        /// </summary>
        public PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)
        {
            var inputs = new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(
                    Pallet.Front, Pallet.Depth, Pallet.Height, Pallet.Weight, Pallet.WeightUnit ?? "kg"),
                PalletsDeep = PalletsDeep,
                FirstLevelDatum = FirstLevelDatum,
                ManualHeaderHeightOverride = Advanced.ManualHeaderHeightOverride,
                DerivedPostReinforced = Advanced.DerivedPostReinforced,
                DerivedPostReinforcementHeight = Advanced.DerivedPostReinforcementHeight,
                DerivedPostHeight = Advanced.DerivedPostHeight,
                SeparatorCountOverride = Advanced.SeparatorCountOverride,
                SeparatorSpacingOverride = Advanced.SeparatorSpacingOverride,
                PostCatalogId = PostCatalogId,
                PostPeralte = PostPeralte,
                PalletTolerance = PalletTolerance,
                BeamDepth = BeamDepth,
                Annotations = new DynamicAnnotationOptions
                {
                    NumberFronts = NumberFronts,
                    NumberLevels = NumberLevels,
                    DrawRackName = DrawRackName,
                    AnnotationScale = AnnotationScale,
                    Dimensions = Dimensions,
                    DimensionStyle = DimensionStyle,
                    DimensionViews = DimensionViewPolicy.FromEditor(
                        LoadedDimensionViews, DimensionViewsTouched, DimensionsFrontal, DimensionsLateral, DimensionsPlanta),
                },
            };

            // Only the authorized (entrance-guide-free) safety reaches the design; the assembler filters again.
            foreach (var safety in assembler.AuthorizedSafety(SafetySelections))
            {
                inputs.SafetySelections.Add(safety);
            }

            return inputs;
        }

        // ---- Explicit rules ------------------------------------------------------------------------------------------

        /// <summary>
        /// The post the editor shows for a loaded id. Blank or only spaces is the catalog default; an id the post combo
        /// offers is kept as it is (compared ORDINALLY, as the combo does); an id it does not offer falls back to the
        /// catalog default. The ids offered are the distinct non-blank ones, by trimmed id ignoring case, first of each
        /// group.
        /// </summary>
        public static string ResolvePostCatalogId(RackCatalog catalog, string requested)
        {
            var fallback = catalog?.Defaults?.Post;
            if (string.IsNullOrWhiteSpace(requested))
            {
                return fallback;
            }

            var offered = (catalog?.PostProfiles ?? Enumerable.Empty<ProfileCatalogEntry>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First().Id.Trim());
            return offered.Any(id => string.Equals(id, requested, StringComparison.Ordinal)) ? requested : fallback;
        }

        /// <summary>
        /// The weight unit the editor shows after seeding. The box offers only «kg» and «lb»: a blank unit is «kg», one of
        /// the two is kept, and anything else (a unit the box does not offer) leaves the box on its previous selection —
        /// <paramref name="current"/> when that is «kg» or «lb», else the initial «kg».
        /// </summary>
        public static string ResolveWeightUnit(string current, string requested)
        {
            if (string.IsNullOrWhiteSpace(requested))
            {
                return "kg";
            }

            if (string.Equals(requested, "kg", StringComparison.Ordinal) || string.Equals(requested, "lb", StringComparison.Ordinal))
            {
                return requested;
            }

            return string.Equals(current, "kg", StringComparison.Ordinal) || string.Equals(current, "lb", StringComparison.Ordinal)
                ? current
                : "kg";
        }

        /// <summary>Whether the design is inside the lossless guarantee: a one-way rack whose structure keeps modules or
        /// lines with the ids of a parked composite tail is not.</summary>
        public static PushBackEditableSupport SupportOf(PushBackDesign design)
        {
            var structure = design?.Structure;
            if (design == null || design.IsComposite || structure == null)
            {
                return PushBackEditableSupport.Supported;
            }

            var hasTail = structure.Modules.Any(module => module != null && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId))
                          || structure.HeaderLineOverrides.Any(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId));
            return hasTail ? PushBackEditableSupport.DormantCompositeTail : PushBackEditableSupport.Supported;
        }
    }
}
