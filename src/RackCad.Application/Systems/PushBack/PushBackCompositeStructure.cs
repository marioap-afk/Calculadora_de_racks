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

        /// <summary>
        /// El rango de posiciones que una ranura ocupa segun en que lados exista.
        ///
        /// <para>
        /// Una ranura presente en LOS DOS lados es una bahia continua que atraviesa el rack, asi que su estructura
        /// recorre toda la profundidad: su fondo por lado gobierna donde acaban sus CAMAS, no donde acaban sus
        /// marcos, exactamente como en I-41 un nivel mas corto termina antes dentro de la estructura de su frente.
        /// </para>
        /// <para>
        /// Una ranura presente en un solo lado ocupa unicamente la mitad de ese lado. Es el caso que obliga a admitir
        /// rangos NO ANIDADOS: una solo-A empieza en 1 y una solo-B acaba en la ultima posicion, y ninguna contiene
        /// a la otra.
        /// </para>
        /// </summary>
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
                return (1, Math.Max(PushBackCellDepth.MinimumPalletsDeep, Math.Min(structureA, PositionsA)));
            }

            if (hasB)
            {
                var count = Math.Max(PushBackCellDepth.MinimumPalletsDeep, Math.Min(structureB, PositionsB));
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
    /// La retícula TRANSVERSAL si es compartida: TODA ranura existe en las dos sub-estructuras —las que no pertenecen
    /// a un lado viajan alli como frente EN BLANCO (I-33)—, de modo que las columnas de postes y el BFR son unicos y
    /// los indices de ranura significan lo mismo en los dos lados y en el compuesto.
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
        /// El diseno estructural de UN lado en su MARCO LOCAL, con TODAS las ranuras transversales del rack: las que
        /// no pertenecen a este lado viajan como frentes EN BLANCO. Eso es lo que hace que la retícula transversal
        /// sea UNA sola y que el indice de ranura signifique lo mismo en A, en B y en la estructura compuesta — sin
        /// el, una ranura ausente desplazaria todas las columnas siguientes de ese lado.
        /// </summary>
        public static DynamicRackDesign SideStructuralDesign(
            PushBackDesign design,
            PushBackSideConfiguration side,
            PushBackSideConfiguration other,
            IReadOnlyList<DynamicRackModuleDesign> modules)
        {
            var shared = design?.Structure ?? new DynamicRackDesign();
            var local = CopySharedStructuralIntent(shared);
            local.LoadLevels = Math.Max(1, side.LoadLevels);
            local.FirstLevelHeight = side.FirstLevelHeight;
            local.PalletsDeep = Math.Max(PushBackCellDepth.MinimumPalletsDeep, side.EffectiveStructure());

            var slots = Math.Max(side.SlotCount, other?.SlotCount ?? 0);
            var absent = new List<bool>();
            for (var slot = 0; slot < slots; slot++)
            {
                var front = side.Front(slot);
                if (front != null)
                {
                    var copy = PushBackSideDesign.CopyFront(front);
                    copy.DepthStartPosition = 1;
                    copy.PalletsDeep = side.SlotStructure(slot);
                    local.Fronts.Add(copy);
                    absent.Add(false);
                    continue;
                }

                // La ranura no existe en este lado: viaja EN BLANCO para conservar la columna. Su claro lo aporta el
                // lado que si la tiene, porque el BFR es una propiedad fisica compartida.
                var reference = other?.Front(slot);
                if (reference == null)
                {
                    continue;
                }

                var blank = PushBackSideDesign.CopyFront(reference);
                blank.IsActive = false;
                blank.DepthStartPosition = 1;
                blank.PalletsDeep = local.PalletsDeep;
                local.Fronts.Add(blank);
                absent.Add(true);
            }

            // Las ranuras ausentes del FINAL se retiran: son el borde del rack en este lado, y dejarlas en blanco
            // dibujaria alli una linea de postes que no existe (la regla de I-33 conserva siempre los dos bordes
            // exteriores). Quitarlas por el final no mueve ninguna columna, porque la retícula se acumula desde 0;
            // por eso las ausencias INTERIORES si se conservan en blanco — retirarlas desplazaria las siguientes.
            while (local.Fronts.Count > 1 && absent.Count == local.Fronts.Count && absent[absent.Count - 1])
            {
                local.Fronts.RemoveAt(local.Fronts.Count - 1);
                absent.RemoveAt(absent.Count - 1);
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
            // I-42: una ranura solo-A y otra solo-B no anidan, y las dos son fisicamente reales sobre esta unica
            // estructura. La intencion es DERIVADA (se construye aqui) y nunca se persiste.
            composite.AllowsNonNestedDepthRanges = true;

            var slots = Math.Max(sideA.SlotCount, sideB.SlotCount);
            for (var slot = 0; slot < slots; slot++)
            {
                var structureA = sideA.HasSlot(slot) ? sideA.SlotStructure(slot) : 0;
                var structureB = sideB.HasSlot(slot) ? sideB.SlotStructure(slot) : 0;
                var blankOnBothSides = structureA <= 0 && structureB <= 0;
                if (blankOnBothSides)
                {
                    // I-33 (y I-42, ronda post-82e918b): una ranura EN BLANCO en los dos lados sigue existiendo
                    // fisicamente — conserva su claro y desplaza a las de atras—, asi que NO se salta: se emite como
                    // frente en blanco. Saltarla encogia la retícula transversal y corria todos los frentes
                    // siguientes, que es justo lo que «en blanco» promete no hacer.
                    structureA = Math.Max(PushBackCellDepth.MinimumPalletsDeep, sideA.SlotStructure(slot));
                    structureB = sideB.IsPresent
                        ? Math.Max(PushBackCellDepth.MinimumPalletsDeep, sideB.SlotStructure(slot))
                        : 0;
                }

                var range = layout.SlotRange(structureA, structureB);
                var reference = sideA.Front(slot) ?? sideB.Front(slot);
                var levels = Math.Max(sideA.Levels(slot), sideB.Levels(slot));
                var front = new DynamicRackFrontDesign
                {
                    // Una ranura esta ACTIVA si lo esta en cualquiera de los dos lados: su estructura existe igual.
                    IsActive = !blankOnBothSides
                        && ((sideA.Front(slot)?.IsActive ?? false) || (sideB.Front(slot)?.IsActive ?? false)),
                    // La mayor demanda aplicable gobierna la envolvente compartida: calles, ancho y niveles.
                    PalletCount = Math.Max(
                        sideA.Front(slot)?.PalletCount ?? 0,
                        Math.Max(1, sideB.Front(slot)?.PalletCount ?? 0)),
                    LoadLevels = levels > 0 ? levels : (int?)null,
                    PalletsDeep = range.Count,
                    DepthStartPosition = range.Start,
                    BeamLengthOverride = MaxOverride(
                        sideA.Front(slot)?.BeamLengthOverride, sideB.Front(slot)?.BeamLengthOverride),
                    FirstLevelHeight = reference?.FirstLevelHeight
                };

                foreach (var segment in SlotSegments(design, layout, sideA, sideB, slot, structureA, structureB))
                {
                    front.DepthSegments.Add(segment);
                }

                composite.Fronts.Add(front);
            }

            foreach (var module in ComposeModules(layout, localA, localB))
            {
                composite.Modules.Add(module);
            }

            return composite;
        }

        /// <summary>
        /// ERROR 4 (I-42) — los TRAMOS de profundidad que una ranura ocupa DE VERDAD.
        ///
        /// <para>
        /// Una ranura presente en los dos lados sigue siendo UNA bahia que atraviesa el rack —su rango continuo no
        /// cambia, y con el su claro, su ancho y sus coordenadas—, pero su ESTRUCTURA solo tiene que existir donde
        /// alguna de sus camas la usa: pegada al arranque hasta donde llega la demanda de A, y pegada al final hasta
        /// donde llega la de B. La profundidad intermedia que ninguno de los dos alcanza no lleva cabecera. Antes
        /// toda ranura de dos lados reclamaba la profundidad entera, de modo que una ranura corta se extendia hasta
        /// donde llegaba la mas larga del rack: las «cabeceras de mas» que reporto el dueño.
        /// </para>
        /// <para>
        /// Dos excepciones, y las dos son fisicas:
        /// </para>
        /// <list type="bullet">
        /// <item>una ranura de un solo lado ya ocupa un rango exacto: no declara tramos;</item>
        /// <item>una ranura con alguna celda CORRIDA atraviesa la interfaz, asi que su cama necesita apoyos en TODA
        /// la profundidad y tampoco declara tramos.</item>
        /// </list>
        /// <para>
        /// Una lista VACIA significa «un solo tramo, el rango de siempre»: es la respuesta cuando la cobertura ya
        /// coincide con el rango, y es lo que hace que nada cambie donde no habia defecto.
        /// </para>
        /// </summary>
        public static IReadOnlyList<DynamicDepthSegment> SlotSegments(
            PushBackDesign design,
            PushBackCompositeLayout layout,
            PushBackSideConfiguration sideA,
            PushBackSideConfiguration sideB,
            int slot,
            int structureA,
            int structureB)
        {
            var none = Array.Empty<DynamicDepthSegment>();
            if (layout == null || structureA <= 0 || structureB <= 0)
            {
                return none;
            }

            var levels = Math.Max(sideA?.Levels(slot) ?? 0, sideB?.Levels(slot) ?? 0);
            for (var level = 0; level < levels; level++)
            {
                if (design?.Composite?.TopologyAt(slot, level) == PushBackCellTopology.Corrida)
                {
                    return none;   // la cama cruza: la estructura tiene que acompañarla de extremo a extremo
                }
            }

            var countA = Math.Max(
                PushBackCellDepth.MinimumPalletsDeep, Math.Min(structureA, layout.PositionsA));
            var countB = Math.Max(
                PushBackCellDepth.MinimumPalletsDeep, Math.Min(structureB, layout.PositionsB));
            var startB = layout.TotalPositions - countB + 1;
            if (countA >= layout.PositionsA && countB >= layout.PositionsB)
            {
                // Los dos lados llegan a la interfaz: los tramos se juntan a traves de ella y la cobertura ES el
                // rango continuo. La posicion de interfaz queda dentro, que es lo que sostiene el separador central.
                return none;
            }

            return new[]
            {
                new DynamicDepthSegment(1, countA),
                new DynamicDepthSegment(startB, countB)
            };
        }

        /// <summary>
        /// La secuencia de modulos compartida: los de A tal cual, el hueco, y los de B INVERTIDOS y renombrados. Los
        /// ModuleId se conservan (los de B con prefijo), que es lo que permite a I-40 seguir localizando sus
        /// cabeceras por linea despues de una recomposicion.
        /// </summary>
        public static IReadOnlyList<DynamicRackModuleDesign> ComposeModules(
            PushBackCompositeLayout layout, DynamicRackSystem localA, DynamicRackSystem localB)
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

        /// <summary>
        /// Los modulos ALMACENADOS que corresponden a un lado, extraidos de la secuencia compartida por su IDENTIDAD
        /// y no por un conteo.
        ///
        /// <para>
        /// Esta es la reconciliacion fisica que conserva I-40 cuando la estructura crece o encoge. Un modulo
        /// SOBREVIVIENTE es el que sigue existiendo en la misma posicion contada DESDE EL EXTREMO EXTERIOR del lado
        /// —que es lo que no se mueve— y con el mismo caracter fisico (cabecera frente a separador). Ese conserva su
        /// ModuleId y su configuracion personalizada, y con ellos los <c>HeaderLineOverrides</c> que lo apuntan. Un
        /// modulo NUEVO no hereda nada; uno que dejo de existir simplemente desaparece, y no se intenta trasladar su
        /// override a otra pieza.
        /// </para>
        /// </summary>
        public static IReadOnlyList<DynamicRackModuleDesign> StoredSideModules(
            PushBackDesign design, PushBackCompositeLayout layout, PushBackSide side)
        {
            var stored = design?.Structure?.Modules?.Where(module => module != null).ToList()
                         ?? new List<DynamicRackModuleDesign>();
            if (stored.Count == 0)
            {
                return null;
            }

            var isComposite = stored.Any(module =>
                string.Equals(module.ModuleId, GapModuleId, StringComparison.Ordinal)
                || (module.ModuleId ?? string.Empty).StartsWith(SideBModulePrefix, StringComparison.Ordinal));

            if (!isComposite)
            {
                // Secuencia de un rack de un solo sentido: pertenece integramente al lado A. El lado B todavia no
                // tiene modulos almacenados y arranca de la receta estandar.
                return side == PushBackSide.A ? stored.Select(ToDesign).ToList() : null;
            }

            if (side == PushBackSide.A)
            {
                return stored
                    .TakeWhile(module => !string.Equals(module.ModuleId, GapModuleId, StringComparison.Ordinal)
                        && !(module.ModuleId ?? string.Empty).StartsWith(SideBModulePrefix, StringComparison.Ordinal))
                    .Select(ToDesign)
                    .ToList();
            }

            var tail = stored
                .Where(module => (module.ModuleId ?? string.Empty).StartsWith(SideBModulePrefix, StringComparison.Ordinal))
                .ToList();
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

        /// <summary>
        /// La secuencia de UN lado, de exactamente <paramref name="positions"/> modulos, reconciliando la almacenada
        /// contra la receta estandar de esa profundidad.
        ///
        /// <para>
        /// Regla, por posicion contada desde el extremo exterior del lado (el que no se mueve):
        /// <list type="bullet">
        /// <item>si la posicion existe en las dos y su CARACTER fisico coincide (cabecera / separador), sobrevive:
        /// conserva ModuleId, configuracion personalizada y su procedencia;</item>
        /// <item>si cambia de caracter, deja de ser la misma pieza: se toma la calculada;</item>
        /// <item>si la posicion es nueva, se toma la calculada;</item>
        /// <item>si la posicion desaparecio, desaparece.</item>
        /// </list>
        /// </para>
        /// <para>
        /// La LONGITUD es la calculada, salvo que el modulo llevara una longitud MANUAL de I-35: la de un extremo
        /// cambia cuando la estructura crece —deja de llevar la holgura de extremo— y arrastrar la vieja dejaria la
        /// secuencia sin cerrar, pero una medida que el usuario escribio sobre una pieza que sigue existiendo es
        /// intencion suya y se conserva. Lo demas que se conserva es la IDENTIDAD y la configuracion, que es lo que
        /// I-40 direcciona.
        /// </para>
        /// </summary>
        public static IReadOnlyList<DynamicRackModuleDesign> Reconcile(
            IReadOnlyList<DynamicRackModuleDesign> stored,
            IReadOnlyList<DynamicRackModule> standard)
        {
            var result = new List<DynamicRackModuleDesign>();
            var reference = standard ?? (IReadOnlyList<DynamicRackModule>)Array.Empty<DynamicRackModule>();
            for (var index = 0; index < reference.Count; index++)
            {
                var calculated = ToDesign(reference[index], reference[index].ModuleId);
                var previous = stored != null && index < stored.Count ? stored[index] : null;
                if (previous == null || previous.IsHeader != calculated.IsHeader)
                {
                    result.Add(calculated);
                    continue;
                }

                calculated.ModuleId = string.IsNullOrWhiteSpace(previous.ModuleId)
                    ? calculated.ModuleId
                    : previous.ModuleId;
                calculated.UseCalculatedHeaderConfiguration = previous.UseCalculatedHeaderConfiguration;
                calculated.HeaderConfiguration = previous.HeaderConfiguration;
                calculated.IsCalculated = previous.IsCalculated;
                calculated.IsManualOverride = previous.IsManualOverride;
                calculated.Notes = previous.Notes;
                if (previous.IsManualOverride && previous.Length > 0.0)
                {
                    // Una longitud MANUAL de I-35 es intencion del usuario sobre una pieza que sigue existiendo:
                    // devolverla a la calculada seria deshacer su edicion sin decirlo, y ademas dejaria el modulo
                    // marcado como manual mintiendo sobre su medida.
                    calculated.Length = previous.Length;
                }

                result.Add(calculated);
            }

            return result;
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
