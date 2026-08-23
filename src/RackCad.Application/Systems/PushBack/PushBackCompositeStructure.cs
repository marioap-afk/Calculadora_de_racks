using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el reparto de POSICIONES de fondo de un Push Back compuesto: cuantas ocupa cada lado, donde cae el
    /// hueco y en que posiciones vive cada ranura transversal. Es aritmetica pura sobre la estructura efectiva de
    /// los dos lados; no conoce catalogos ni coordenadas.
    /// </summary>
    public sealed class PushBackCompositeLayout
    {
        public PushBackCompositeLayout(int positionsA, int positionsB, bool hasGap, double gap, bool centralSeparator)
        {
            PositionsA = positionsA;
            PositionsB = positionsB;
            HasGap = hasGap;
            Gap = gap;
            CentralSeparator = centralSeparator;
        }

        /// <summary>Posiciones de fondo que ocupa el lado A, desde la posicion 1.</summary>
        public int PositionsA { get; }

        /// <summary>Posiciones de fondo que ocupa el lado B, contadas desde el final.</summary>
        public int PositionsB { get; }

        /// <summary>True cuando existe la posicion de interfaz entre los dos lados (siempre que hay dos lados).</summary>
        public bool HasGap { get; }

        /// <summary>Longitud fisica (in) del hueco. Puede ser 0: los dos extremos fisicos siguen existiendo.</summary>
        public double Gap { get; }

        /// <summary>Si el hueco lleva el separador central. Solo se materializa con <see cref="Gap"/> &gt; 0.</summary>
        public bool CentralSeparator { get; }

        /// <summary>La posicion (1-based) del hueco, o 0 si no hay.</summary>
        public int GapPosition => HasGap ? PositionsA + 1 : 0;

        /// <summary>Total de posiciones de la secuencia compartida.</summary>
        public int TotalPositions => PositionsA + (HasGap ? 1 : 0) + PositionsB;

        /// <summary>Primera posicion (1-based) del lado B.</summary>
        public int FirstPositionB => PositionsA + (HasGap ? 1 : 0) + 1;

        /// <summary>El rango de posiciones que una ranura ocupa, segun en que lados exista.</summary>
        public (int Start, int Count) SlotRange(int structureA, int structureB)
        {
            var hasA = structureA > 0;
            var hasB = structureB > 0;
            if (hasA && hasB)
            {
                return (1, TotalPositions);
            }

            if (hasA)
            {
                return (1, Math.Min(structureA, PositionsA));
            }

            if (hasB)
            {
                var count = Math.Min(structureB, PositionsB);
                return (TotalPositions - count + 1, count);
            }

            return (0, 0);
        }
    }

    /// <summary>
    /// I-42 — la composicion de la ESTRUCTURA FISICA UNICA de un Push Back compuesto.
    ///
    /// <para>
    /// La regla arquitectonica es que no existe «rack A + rack B»: existe UNA estructura, con una sola secuencia de
    /// modulos, una sola retícula transversal y una sola propiedad de postes, cabeceras, placas y separadores. Esta
    /// clase la construye a partir de dos sub-estructuras ya resueltas —una por lado, cada una en su marco local— y
    /// de la interfaz central.
    /// </para>
    /// <para>
    /// El orden fisico es: <c>modulos de A -&gt; linea terminal de A -&gt; HUECO -&gt; linea inicial de B -&gt;
    /// modulos de B</c>. Los modulos de B se INVIERTEN, porque el lado B mira al otro pasillo: su modulo local 1 (su
    /// cabecera exterior) es el ultimo de la secuencia compartida. Son dos lineas de postes distintas en la interfaz
    /// incluso con hueco 0: ninguna cabecera se fusiona con la otra.
    /// </para>
    /// <para>
    /// La retícula TRANSVERSAL si es compartida: cada ranura toma la mayor demanda aplicable de los dos lados
    /// (calles, ancho de larguero y niveles), de modo que las lineas fisicas de postes y el BFR son unicos y ni una
    /// cabecera se cuenta dos veces.
    /// </para>
    /// </summary>
    public static class PushBackCompositeStructure
    {
        /// <summary>
        /// Prefijo de los ModuleId del lado B en la secuencia compartida. Es lo que mantiene la identidad de I-40
        /// (<c>HeaderLineOverrides</c> por <c>(PostIndex, ModuleId)</c>) separada entre los dos lados sin colisiones.
        /// </summary>
        public const string SideBModulePrefix = "B:";

        /// <summary>ModuleId de la posicion de interfaz. Es unico y estable, asi que el hueco tambien es direccionable.</summary>
        public const string GapModuleId = "GAP";

        /// <summary>
        /// El reparto de posiciones que imponen las dos estructuras efectivas y el gap declarado.
        /// </summary>
        public static PushBackCompositeLayout Layout(
            PushBackSideConfiguration sideA, PushBackSideConfiguration sideB, PushBackCompositeDesign composite)
        {
            var positionsA = Math.Max(PushBackCellDepth.MinimumPalletsDeep, sideA?.EffectiveStructure() ?? 0);
            var hasB = sideB != null && sideB.IsPresent;
            var positionsB = hasB ? Math.Max(PushBackCellDepth.MinimumPalletsDeep, sideB.EffectiveStructure()) : 0;
            var gap = Math.Max(0.0, composite?.Gap ?? 0.0);
            // El separador central es la MISMA pieza del rack; solo puede materializarse si hay hueco donde ponerlo.
            var separator = (composite?.CentralSeparator ?? false) && gap > 0.0;
            return new PushBackCompositeLayout(positionsA, positionsB, hasB, gap, separator);
        }

        /// <summary>
        /// El diseno estructural de UN lado en su MARCO LOCAL: sus ranuras presentes, cada una con la profundidad
        /// estructural que le corresponde, arrancando todas en la posicion 1. Es lo que se le entrega al resolver
        /// dinamico para obtener la sub-estructura del lado, de modo que ni una regla de cabeceras, separadores o
        /// postes derivados se reescribe aqui.
        /// </summary>
        public static DynamicRackDesign SideStructuralDesign(
            PushBackDesign design, PushBackSideConfiguration side, IReadOnlyList<DynamicRackModuleDesign> modules)
        {
            var shared = design?.Structure ?? new DynamicRackDesign();
            var local = CopySharedStructuralIntent(shared);
            local.LoadLevels = Math.Max(1, side.LoadLevels);
            local.FirstLevelHeight = side.FirstLevelHeight;
            local.PalletsDeep = Math.Max(PushBackCellDepth.MinimumPalletsDeep, side.EffectiveStructure());

            for (var slot = 0; slot < side.SlotCount; slot++)
            {
                var front = side.Front(slot);
                if (front == null)
                {
                    continue;
                }

                var copy = PushBackSideDesign.CopyFront(front);
                copy.DepthStartPosition = 1;
                copy.PalletsDeep = side.SlotStructure(slot);
                local.Fronts.Add(copy);
            }

            if (local.Fronts.Count == 0)
            {
                local.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = DynamicRackDefaults.DefaultPalletsWide,
                    PalletsDeep = local.PalletsDeep,
                    DepthStartPosition = 1
                });
            }

            if (modules != null)
            {
                foreach (var module in modules)
                {
                    local.Modules.Add(module);
                }
            }

            return local;
        }

        /// <summary>
        /// La estructura COMPUESTA: una sola retícula transversal (la mayor demanda de los dos lados por ranura), una
        /// sola secuencia de modulos (A, hueco, B invertido) y toda la intencion fisica compartida del rack.
        /// </summary>
        public static DynamicRackDesign Compose(
            PushBackDesign design,
            PushBackSideConfiguration sideA,
            PushBackSideConfiguration sideB,
            PushBackCompositeLayout layout,
            DynamicRackSystem localA,
            DynamicRackSystem localB)
        {
            var shared = design?.Structure ?? new DynamicRackDesign();
            var composite = CopySharedStructuralIntent(shared);
            composite.LoadLevels = Math.Max(sideA.LoadLevels, sideB.IsPresent ? sideB.LoadLevels : 1);
            composite.PalletsDeep = layout.TotalPositions;
            composite.FirstLevelHeight = sideB.IsPresent
                ? Math.Min(sideA.FirstLevelHeight, sideB.FirstLevelHeight)
                : sideA.FirstLevelHeight;

            var slots = Math.Max(sideA.SlotCount, sideB.SlotCount);
            var hasAOnly = false;
            var hasBOnly = false;
            for (var slot = 0; slot < slots; slot++)
            {
                var structureA = sideA.HasSlot(slot) ? sideA.SlotStructure(slot) : 0;
                var structureB = sideB.HasSlot(slot) ? sideB.SlotStructure(slot) : 0;
                if (structureA <= 0 && structureB <= 0)
                {
                    continue;
                }

                hasAOnly |= structureA > 0 && structureB <= 0;
                hasBOnly |= structureB > 0 && structureA <= 0;

                var range = layout.SlotRange(structureA, structureB);
                var reference = sideA.Front(slot) ?? sideB.Front(slot);
                composite.Fronts.Add(new DynamicRackFrontDesign
                {
                    // Una ranura esta ACTIVA si lo esta en cualquiera de los dos lados: su estructura existe igual.
                    IsActive = (sideA.Front(slot)?.IsActive ?? false) || (sideB.Front(slot)?.IsActive ?? false),
                    // La mayor demanda aplicable gobierna la envolvente compartida: calles, ancho y niveles.
                    PalletCount = Math.Max(
                        sideA.Front(slot)?.PalletCount ?? 0,
                        Math.Max(1, sideB.Front(slot)?.PalletCount ?? 0)),
                    LoadLevels = Math.Max(sideA.Levels(slot), sideB.Levels(slot)) is var levels && levels > 0
                        ? levels
                        : (int?)null,
                    PalletsDeep = range.Count,
                    DepthStartPosition = range.Start,
                    BeamLengthOverride = MaxOverride(
                        sideA.Front(slot)?.BeamLengthOverride, sideB.Front(slot)?.BeamLengthOverride),
                    FirstLevelHeight = reference?.FirstLevelHeight
                });
            }

            if (hasAOnly && hasBOnly)
            {
                // LIMITACION DECLARADA: la retícula de profundidad compartida exige que los rangos de los frentes
                // aniden. Una ranura solo-A empieza en la posicion 1 y una solo-B acaba en la ultima, asi que
                // ninguna contiene a la otra. Se reporta en vez de producir una estructura incoherente en silencio.
                throw new ArgumentException(
                    "Push Back compuesto: no pueden coexistir una ranura presente solo en el lado A y otra presente "
                    + "solo en el lado B. La retícula de profundidad compartida exige que los rangos de los frentes "
                    + "aniden entre si.");
            }

            foreach (var module in ComposeModules(design, layout, localA, localB))
            {
                composite.Modules.Add(module);
            }

            return composite;
        }

        /// <summary>
        /// La secuencia de modulos compartida: los de A tal cual, el hueco, y los de B INVERTIDOS y renombrados. Los
        /// ModuleId se conservan (los de B con prefijo), que es lo que permite a I-40 seguir localizando sus
        /// cabeceras por linea despues de una recomposicion.
        /// </summary>
        public static IReadOnlyList<DynamicRackModuleDesign> ComposeModules(
            PushBackDesign design, PushBackCompositeLayout layout, DynamicRackSystem localA, DynamicRackSystem localB)
        {
            var result = new List<DynamicRackModuleDesign>();
            foreach (var module in localA?.Modules ?? new List<DynamicRackModule>())
            {
                result.Add(ToDesign(module, module.ModuleId));
            }

            if (layout.HasGap)
            {
                result.Add(new DynamicRackModuleDesign
                {
                    ModuleId = GapModuleId,
                    // Con separador central el hueco ES un separador (la MISMA pieza del rack, contada una vez);
                    // sin el, es un hueco fisico que no dibuja nada.
                    Kind = layout.CentralSeparator ? DynamicRackModuleKind.Separator : DynamicRackModuleKind.Gap,
                    Length = layout.Gap,
                    IsCalculated = false,
                    IsManualOverride = true,
                    UseCalculatedHeaderConfiguration = true
                });
            }

            var sideBModules = (localB?.Modules ?? new List<DynamicRackModule>()).Reverse().ToList();
            foreach (var module in sideBModules)
            {
                result.Add(ToDesign(module, SideBModulePrefix + (module.ModuleId ?? string.Empty)));
            }

            return result;
        }

        /// <summary>Los modulos ALMACENADOS que corresponden a un lado, extraidos de la secuencia compartida.</summary>
        public static IReadOnlyList<DynamicRackModuleDesign> StoredSideModules(
            PushBackDesign design, PushBackCompositeLayout layout, PushBackSide side)
        {
            var stored = design?.Structure?.Modules?.ToList() ?? new List<DynamicRackModuleDesign>();
            if (stored.Count != layout.TotalPositions)
            {
                // La secuencia almacenada no describe ESTA estructura (cambio la demanda o el override): se
                // reconstruye por defecto en vez de reinterpretar posiciones que ya no significan lo mismo.
                return null;
            }

            if (side == PushBackSide.A)
            {
                return stored.Take(layout.PositionsA).ToList();
            }

            var tail = stored.Skip(layout.FirstPositionB - 1).ToList();
            tail.Reverse();
            return tail
                .Select(module =>
                {
                    var copy = ToDesign(module);
                    copy.ModuleId = StripSideB(module.ModuleId);
                    return copy;
                })
                .ToList();
        }

        /// <summary>Devuelve el ModuleId local de B a partir del compartido.</summary>
        public static string StripSideB(string moduleId)
            => !string.IsNullOrEmpty(moduleId) && moduleId.StartsWith(SideBModulePrefix, StringComparison.Ordinal)
                ? moduleId.Substring(SideBModulePrefix.Length)
                : moduleId;

        private static double? MaxOverride(double? first, double? second)
        {
            if (!first.HasValue)
            {
                return second;
            }

            return second.HasValue ? Math.Max(first.Value, second.Value) : first;
        }

        private static DynamicRackModuleDesign ToDesign(DynamicRackModule module, string moduleId = null)
            => new DynamicRackModuleDesign
            {
                ModuleId = moduleId ?? module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                HeaderConfiguration = module.AssociatedFrameConfiguration,
                Notes = module.Notes
            };

        private static DynamicRackModuleDesign ToDesign(DynamicRackModuleDesign module)
            => new DynamicRackModuleDesign
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                HeaderConfiguration = module.HeaderConfiguration,
                Notes = module.Notes
            };

        /// <summary>
        /// Copia de la intencion fisica COMPARTIDA del rack: postes, peralte, separadores, postes derivados, los
        /// overrides por linea de I-40, la altura manual, las anotaciones y la seguridad. Nada de esto pertenece a un
        /// lado, y por eso se copia una sola vez a cada sub-estructura y a la compuesta.
        /// </summary>
        private static DynamicRackDesign CopySharedStructuralIntent(DynamicRackDesign shared)
        {
            var copy = new DynamicRackDesign
            {
                Pallet = shared.Pallet,
                BeamDepth = shared.BeamDepth,
                PalletTolerance = shared.PalletTolerance,
                InOutBeamCatalogId = shared.InOutBeamCatalogId,
                HeaderPostCatalogId = shared.HeaderPostCatalogId,
                PostPeralte = shared.PostPeralte,
                SeparatorCountOverride = shared.SeparatorCountOverride,
                SeparatorSpacingOverride = shared.SeparatorSpacingOverride,
                DerivedPostReinforced = shared.DerivedPostReinforced,
                DerivedPostReinforcementHeight = shared.DerivedPostReinforcementHeight,
                DerivedPostHeight = shared.DerivedPostHeight,
                ManualHeaderHeightOverride = shared.ManualHeaderHeightOverride,
                NumberFronts = shared.NumberFronts,
                NumberLevels = shared.NumberLevels,
                DrawRackName = shared.DrawRackName,
                AnnotationScale = shared.AnnotationScale,
                Dimensions = shared.Dimensions,
                DimensionStyle = shared.DimensionStyle
            };

            foreach (var peralte in shared.IntermediateBeamDepths)
            {
                copy.IntermediateBeamDepths.Add(peralte);
            }

            foreach (var line in shared.HeaderLineOverrides)
            {
                copy.HeaderLineOverrides.Add(line);
            }

            foreach (var line in shared.DerivedPostLineOverrides)
            {
                copy.DerivedPostLineOverrides.Add(line);
            }

            foreach (var safety in shared.SafetySelections)
            {
                copy.SafetySelections.Add(safety);
            }

            return copy;
        }
    }
}
