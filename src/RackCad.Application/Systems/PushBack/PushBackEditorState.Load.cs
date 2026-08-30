using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack
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
        /// <summary>Reset to a brand-new design: one default front with Push Back's own first-level height, rear peralte
        /// 3.5 and active topes, a valid primary selection on the first cell. Returns the rack-wide inputs a new Push
        /// Back system opens with.</summary>
        public PushBackEditorInputs LoadNew()
        {
            structure.RestoreFromResolved(Enumerable.Empty<DynamicRackFront>()); // falls back to one default front

            // PB-012 (I-32): the shared dynamic front opens at 6"; Push Back loads at floor level and opens at 4". It is
            // applied HERE, on this state's own fronts, so the shared constant and the dynamic editor keep their value.
            // Doing it only on the NEW-design path is what leaves a persisted rack's own height untouched on load, and
            // it also seeds every front added afterwards (a new front copies the selected one).
            foreach (var front in structure.Fronts)
            {
                front.FirstLevelHeight = PushBackDefaults.DefaultFirstLevelHeight;
            }

            pushFronts.Clear();
            RearTopeSaque = PushBackDefaults.RearTopeSaque;
            RearTopePieceId = null;   // PB-005: a new rack starts on the default variant, never on the previous rack's
            DefensePieceId = null;    // I-42 (7E): y sobre la defensa historica, no sobre la del rack anterior
            AdoptLoadedBaseline(null);   // new design: rebuild from a standard structure, drop any loaded baseline
                                         // AND the module session of whatever rack was open before (I-40, ronda 3)
            SyncPushConfig();
            structure.ToggleCell(0, 0, false);   // deterministic single (0,0) selection; never keep the previous one
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

            // I-40 (ronda 3): cargar es ADOPTAR otro rack, no recalcular este. La sesion de modulos del rack
            // anterior —la que el constructor de la ventana crea con LoadNew— describe cabeceras que no son estas,
            // y conservarla por coincidencia de firma es lo que hacia que RACKEDITAR mostrase la configuracion
            // predeterminada sobre una cabecera personalizada.
            AdoptLoadedBaseline(system);   // a fresh resolve: an independent deep baseline the recompute preserves
            RebuildFromResolved(system);
            var inputs = RecoverInputs(design, system);
            MigrateFirstLevelDatum(system, resolver.Catalog, inputs);
            return inputs;
        }

        /// <summary>
        /// I-42 (correccion aislada 3) — LA UNICA conversion de datum del producto, y esta es su frontera.
        ///
        /// <para>
        /// Un documento SIN el marcador guarda «Alto 1er nivel» con la semantica historica —una elevacion absoluta
        /// que se ajusta al troquel mas cercano—, y con ella se acaba de resolver. Aqui se re-expresa ese numero
        /// sobre el datum del producto MIDIENDO la geometria que ya tiene: cada frente pasa a guardar su distancia
        /// al troquel utilizable mas bajo. No se resta ninguna constante y no se mueve ni una milesima — la
        /// elevacion resuelta ya esta EN la retícula, y el troquel utilizable mas bajo tambien, asi que volver a
        /// medir desde el uno hasta la otra devuelve exactamente el mismo troquel.
        /// </para>
        /// <para>
        /// A partir de aqui el datum solo se TRANSPORTA: ni la ventana, ni el ensamblador, ni la estructura
        /// compuesta vuelven a decidirlo. Un documento que ya trae el marcador no pasa por aqui.
        /// </para>
        /// </summary>
        private void MigrateFirstLevelDatum(
            PushBackSystem system, RackCatalog catalog, PushBackEditorInputs inputs)
        {
            if (inputs == null
                || inputs.FirstLevelDatum == (int)RackFirstLevelDatumMode.LowestUsablePunch
                || system?.Structure == null
                || catalog == null)
            {
                return;
            }

            var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
            var pitch = SelectiveRackDefaults.TroquelPaso;
            if (pitch <= 0.0)
            {
                return;   // sin retícula medida no hay a que re-expresar: se conserva la lectura historica
            }

            var fronts = system.Structure.Fronts;
            for (var index = 0; index < fronts.Count && index < structure.Count; index++)
            {
                var levels = fronts[index].LoadBeamLevels;
                if (levels == null || levels.Count == 0)
                {
                    continue;
                }

                var physical = levels.OrderBy(level => level.LevelNumber).First().ExitElevation;
                structure.Fronts[index].FirstLevelHeight =
                    RackFirstLevelDatum.ToLowestPunchOffset(physical, gridBase, pitch);
            }

            inputs.FirstLevelDatum = (int)RackFirstLevelDatumMode.LowestUsablePunch;
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
            RearTopePieceId = rearTope.PieceId;
            DefensePieceId = system.DefensePieceId;

            pushFronts.Clear();
            for (var frontIndex = 0; frontIndex < structure.Count; frontIndex++)
            {
                var levels = Math.Max(1, structure.Fronts[frontIndex].LoadLevels);
                var front = new PushBackEditorFront();

                // I-41 (PB-015): la matriz vuelve de la estructura, que solo lleva la ENVOLVENTE. El fondo POR DEFECTO
                // del frente —lo que el usuario ve y edita en «Fondos frente»— viaja aparte en el sistema resuelto, y
                // se restituye aqui. Sin esto, cada guardar/abrir subiria el default hasta la envolvente y el override
                // de la celda mas profunda desapareceria en silencio al segundo round trip.
                var frontDefault = system.DefaultPalletsDeepAt(frontIndex);
                structure.Fronts[frontIndex].PalletsDeep = Math.Max(PushBackCellDepth.MinimumPalletsDeep, frontDefault);

                for (var level = 0; level < levels; level++)
                {
                    var effective = system.EffectivePalletsDeepAt(frontIndex, level);
                    front.Cells.Add(new PushBackEditorCell
                    {
                        HighEndBeamPeralte = system.HighEndBeamPeralteAt(frontIndex, level),
                        RearTopeEnabled = rearTope.At(frontIndex, level),
                        // Solo es override lo que DIFIERE del default: una celda que coincide vuelve heredando, que es
                        // lo que el usuario expreso, y no queda un override huerfano esperando a divergir.
                        PalletsDeepOverride = effective != frontDefault ? effective : (int?)null,
                        DrawPallet = system.DrawPalletAt(frontIndex, level)
                    });
                }

                pushFronts.Add(front);
            }

            SyncPushConfig();
            structure.ToggleCell(0, 0, false);   // deterministic (0,0) selection on load; never drag the previous one
            structure.NormalizeSelection();
        }

        /// <summary>Recover the rack-wide inputs from the persisted design's shared structure and the resolved (GUIA-free)
        /// safety, so the window can repopulate its shared panels. Independent copies; the design is not mutated.</summary>
        private static PushBackEditorInputs RecoverInputs(PushBackDesign design, PushBackSystem system)
        {
            // I-42 — el datum de «Alto 1er nivel» sale del DOCUMENTO, no del default de un rack nuevo: un archivo
            // anterior no lo trae y se lee con la semantica historica, asi que reabre en la MISMA geometria fisica.

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

            // I-35 (Owner round 2): the four advanced RACK-WIDE scopes come back from the very properties that own
            // them, so reopening a design repopulates the advanced panel with what was persisted.
            PushBackAdvancedRackParameters.ReadFrom(s, inputs);

            foreach (var safety in system.SafetySelections ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (safety != null)
                {
                    inputs.SafetySelections.Add(safety.DeepCopy());
                }
            }

            inputs.FirstLevelDatum = design?.Structure?.FirstLevelDatum;
            return inputs;
        }

        private static PalletSpecification ClonePallet(PalletSpecification pallet)
            => pallet == null
                ? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);
    }
}
