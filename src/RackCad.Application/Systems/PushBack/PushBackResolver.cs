using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
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
        private readonly PushBackSafetyAuthority safety;

        public PushBackResolver(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            structureResolver = new DynamicRackSystemResolver(this.catalog);
            safety = new PushBackSafetyAuthority(this.catalog);
        }

        public PushBackSystem Resolve(PushBackDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            // I-42: un rack de UN solo sentido no pasa por la composicion. El camino de abajo es exactamente el que
            // existia antes de la iniciativa, asi que un documento legacy resuelve el mismo sistema hasta el bit.
            if (design.IsComposite)
            {
                return ResolveComposite(design);
            }

            return ResolveSingleSided(design);
        }

        /// <summary>
        /// El Push Back COMPUESTO: una sola estructura fisica (A + hueco + B invertido) sobre la que se montan los
        /// dos lados. El contenido de cada lado se resuelve por el MISMO camino de un sentido — en su marco local —
        /// de modo que no existe una segunda fisica para el lado nuevo.
        /// </summary>
        private PushBackSystem ResolveComposite(PushBackDesign design)
        {
            var resolution = new PushBackCompositeResolver(catalog).Resolve(design, ResolveSingleSided);
            var structure = resolution.Structure;
            var composite = resolution.Composite;

            var system = new PushBackSystem
            {
                Structure = structure,
                HighEndBeamCatalogId = PushBackDefaults.HighEndBeamCatalogId,
                RearTope = design.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig(),
                Composite = composite
            };

            // El lado A NO tiene una segunda autoridad: HighEndBeams contiene LAS MISMAS instancias que la vista del
            // lado, y la rejilla de topes es EL MISMO objeto. Editar una es editar la otra, por construccion.
            composite.SideA.RearTope = system.RearTope;
            foreach (var resolved in composite.SideA.ResolvedFronts)
            {
                system.HighEndBeams.Add(resolved);
            }

            ApplySafety(system, structure);
            return system;
        }

        private PushBackSystem ResolveSingleSided(PushBackDesign design)
        {
            var structure = structureResolver.Resolve(design.Structure ?? new DynamicRackDesign()).System;

            var system = new PushBackSystem
            {
                Structure = structure,
                HighEndBeamCatalogId = PushBackDefaults.HighEndBeamCatalogId,
                RearTope = design.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };

            // High-end (rear) beam peralte per front x level, aligned by index with the resolved fronts. I-41 resolves
            // the cell's EFFECTIVE fondo and its pallet flag in the SAME pass and onto the SAME per-front entry, so the
            // three lists cannot drift apart by level.
            var allowed = AllowedHighEndPeraltes();
            var designFronts = design.Structure?.Fronts;
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                var frontConfig = design.FrontConfig(frontIndex);
                // I-41 (PB-015): el fondo POR DEFECTO del frente. Un documento anterior a I-41 no lo trae, y entonces
                // es el fondo estructural del frente — que en ese rack es exactamente lo que todos sus niveles usaban.
                var structuralDeep = Math.Max(PushBackCellDepth.MinimumPalletsDeep, front.PalletsDeep);
                var frontDefault = frontConfig?.DefaultPalletsDeep is int stored && stored >= PushBackCellDepth.MinimumPalletsDeep
                    ? stored
                    : DesignedDeep(designFronts, frontIndex, structuralDeep);
                var resolved = new PushBackResolvedFront { DefaultPalletsDeep = frontDefault };
                // A blank front carries no level, so it resolves no rear beam either (I-33). The entry stays in the
                // list so HighEndBeams remains aligned by index with the resolved fronts.
                var levels = DynamicFrontActivation.EffectiveLoadLevels(front);
                for (var level = 0; level < levels; level++)
                {
                    resolved.HighEndBeamPeraltes.Add(
                        ResolvePeralte(frontConfig?.PeralteAt(level), design.LegacyHighEndBeamPeralte, allowed));

                    // La precedencia vive en UNA funcion; aqui solo se acota contra la envolvente ya construida, que
                    // es la profundidad estructural del frente. Nunca por debajo de 2.
                    var effective = PushBackCellDepth.Effective(frontConfig?.PalletsDeepOverrideAt(level), frontDefault);
                    resolved.PalletsDeep.Add(Math.Min(effective, structuralDeep));
                    resolved.DrawPallets.Add(frontConfig?.DrawPalletAt(level) ?? false);
                }

                system.HighEndBeams.Add(resolved);
            }

            ApplySafety(system, structure);
            return system;
        }

        /// <summary>
        /// Safety authority: Push Back admits every applicable family EXCEPT entrance guides (removed), and normal
        /// safety only at the LOW (entrance/exit) end — never the rear. Each authorized selection is restricted to the
        /// low end (Left = the exit end in the dynamic builders) so a "Both" selection materializes once, on the low
        /// side, in every view and in the BOM. The GUIA-free, low-only set is exposed on the Push Back system AND
        /// written back onto the shared structure, so the dynamic builders — used later as a BLACK BOX — never emit a
        /// guide, and never emit rear-end safety.
        /// <para>
        /// I-42: la seguridad es del RACK, no de un lado. Se autoriza UNA vez sobre la estructura compartida, asi que
        /// una bota o un protector nunca se cuenta dos veces por el hecho de que el rack tenga dos sentidos. Lo que
        /// SI cambia con el rack es CUANTOS pasillos tiene: un compuesto con camas en las dos mitades tiene dos
        /// extremos bajos, y los dos llevan su seguridad. No hay que pedirla a mano para el segundo lado.
        /// </para>
        /// </summary>
        private void ApplySafety(PushBackSystem system, DynamicRackSystem structure)
        {
            var authorized = safety.Authorize(structure.SafetySelections, AislesOf(system));
            var secondFaceLines = SecondLoadFaceLines(system);
            foreach (var selection in authorized)
            {
                foreach (var line in secondFaceLines)
                {
                    selection.SecondLoadFacePosts.Add(line);
                }

                system.SafetySelections.Add(selection);
            }

            structure.SafetySelections.Clear();
            foreach (var selection in authorized)
            {
                structure.SafetySelections.Add(selection.DeepCopy());
            }
        }

        /// <summary>
        /// Los PASILLOS de carga del rack. Un rack COMPUESTO tiene dos por construccion —una cara de carga en cada
        /// extremo longitudinal—, y los dos son extremos BAJOS: no hay ningun extremo alto donde la seguridad
        /// estorbe. Uno de un sentido tiene uno solo, y su comportamiento no cambia en nada.
        ///
        /// <para>
        /// Es del RACK, no de las camas: la seguridad protege el PASILLO. Que hoy una mitad no tenga ninguna cama no
        /// retira su cara de carga ni la pone a salvo de un montacargas.
        /// </para>
        /// </summary>
        internal static PushBackSafetyAisles AislesOf(PushBackSystem system)
            => system?.Composite != null && (system.Composite.SideB?.IsPresent ?? false)
                ? PushBackSafetyAisles.Both
                : PushBackSafetyAisles.NearOnly;

        /// <summary>
        /// I-42 (ronda post-5a73b92) — las LINEAS de postes que de verdad tienen la segunda cara de carga.
        ///
        /// <para>
        /// Declarar el lado B es una CAPACIDAD del rack; tener lado B es una propiedad de cada FRENTE. Una linea
        /// solo adquiere la segunda cara si alguno de los frentes que sostiene —el de su izquierda o el de su
        /// derecha— existe fisicamente en el lado B. Antes bastaba con que el rack fuera compuesto, asi que
        /// declarar B en UN frente ponia botas y protectores en las lineas de todos los demas, que siguen siendo de
        /// un solo sentido: es la regresion que el dueño vio.
        /// </para>
        /// <para>
        /// Lista VACIA significa «todas las lineas», que es lo que responde un rack no compuesto y lo que deja el
        /// comportamiento anterior intacto donde no hay nada que acotar.
        /// </para>
        /// </summary>
        internal static IReadOnlyList<int> SecondLoadFaceLines(PushBackSystem system)
        {
            var composite = system?.Composite;
            var fronts = system?.Structure?.Fronts;
            if (composite == null || fronts == null || !(composite.SideB?.IsPresent ?? false))
            {
                return Array.Empty<int>();
            }

            var withB = new HashSet<int>();
            for (var slot = 0; slot < fronts.Count; slot++)
            {
                if (composite.SideB.Resolved(slot)?.IsPresent ?? false)
                {
                    withB.Add(slot);
                }
            }

            if (withB.Count == 0)
            {
                // Capacidad declarada y ningun frente con lado B: no hay segunda cara en ninguna linea. Se devuelve
                // una linea IMPOSIBLE en vez de la lista vacia, que significaria «todas».
                return new[] { -1 };
            }

            var lines = new List<int>();
            for (var post = 0; post <= fronts.Count; post++)
            {
                if (withB.Contains(post - 1) || withB.Contains(post))
                {
                    lines.Add(post);
                }
            }

            return lines;
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

            for (var frontIndex = 0; frontIndex < system.HighEndBeams.Count; frontIndex++)
            {
                var resolvedFront = system.HighEndBeams[frontIndex];
                // I-41: el fondo por defecto del frente viaja EXPLICITO en el snapshot. Sin el, el round trip lo
                // perderia: la estructura solo conserva la ENVOLVENTE, y una envolvente no sabe que heredaba cada
                // nivel — un frente con default 3 y una celda en 5 volveria con default 5.
                var config = new PushBackFrontConfig
                {
                    DefaultPalletsDeep = resolvedFront.DefaultPalletsDeep >= PushBackCellDepth.MinimumPalletsDeep
                        ? resolvedFront.DefaultPalletsDeep
                        : (int?)null
                };
                foreach (var peralte in resolvedFront.HighEndBeamPeraltes)
                {
                    config.HighEndBeamPeraltes.Add(peralte);
                }

                for (var level = 0; level < resolvedFront.PalletsDeep.Count; level++)
                {
                    // Solo se re-escribe como override lo que DIFIERE del default: asi un rack sin overrides vuelve a
                    // salir sin overrides, y no se fabrica intencion que el usuario nunca expreso.
                    var effective = resolvedFront.PalletsDeep[level];
                    config.PalletsDeepOverrides.Add(
                        effective >= PushBackCellDepth.MinimumPalletsDeep && effective != resolvedFront.DefaultPalletsDeep
                            ? effective
                            : (int?)null);
                }

                for (var level = 0; level < resolvedFront.DrawPallets.Count; level++)
                {
                    // El default legacy es false, asi que solo el true se conserva como intencion.
                    config.DrawPallets.Add(resolvedFront.DrawPallets[level] ? true : (bool?)null);
                }

                design.Fronts.Add(config);
            }

            return design;
        }

        /// <summary>
        /// El fondo que el DISENO pidio para un frente, antes de que la envolvente lo reescribiera. Es el respaldo del
        /// fondo por defecto cuando el documento no trae uno explicito: en un rack anterior a I-41 el disenno y la
        /// envolvente coinciden, asi que responde exactamente lo de siempre.
        /// </summary>
        private static int DesignedDeep(IList<DynamicRackFrontDesign> designFronts, int frontIndex, int fallback)
        {
            if (designFronts != null && frontIndex >= 0 && frontIndex < designFronts.Count)
            {
                var requested = designFronts[frontIndex]?.PalletsDeep;
                if (requested.HasValue && requested.Value >= PushBackCellDepth.MinimumPalletsDeep)
                {
                    return requested.Value;
                }
            }

            return fallback;
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

        /// <summary>True when <paramref name="selection"/>'s catalog element is an entrance guide (type GUIA). Delegates to
        /// the shared <see cref="PushBackSafetyAuthority"/> so the guide rule lives in exactly one place.</summary>
        public bool IsEntranceGuide(SelectiveSafetySelection selection) => safety.IsEntranceGuide(selection);

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
