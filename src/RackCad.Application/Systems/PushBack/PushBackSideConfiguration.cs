using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — la lectura UNIFORME de la configuracion funcional de un lado, venga de donde venga. El lado A la lee de
    /// los campos legacy del diseno (<see cref="PushBackDesign.Structure"/>.Fronts + <see cref="PushBackDesign.Fronts"/>)
    /// y el lado B de <see cref="PushBackDesign.SideB"/>, pero a partir de aqui el resolver no vuelve a distinguirlos.
    ///
    /// <para>
    /// Esa uniformidad es lo que impide que aparezca una segunda regla para el lado nuevo: el fondo efectivo de una
    /// celda sigue siendo el de ADR-0030 (<c>override de la celda ?? fondo por defecto del frente</c>), ahora
    /// aplicado DENTRO de cada lado.
    /// </para>
    /// <para>
    /// Aqui no hay ni un dato de estructura fisica. La cadena de la estructura es:
    /// <c>demanda de celdas -&gt; envolvente por ranura -&gt; estructura PROPUESTA del lado -&gt; override manual -&gt;
    /// estructura EFECTIVA</c>, y la propuesta nunca es autoridad inmutable: se recalcula siempre y el override la
    /// sustituye por completo. Restaurar es borrar el override.
    /// </para>
    /// </summary>
    public sealed class PushBackSideConfiguration
    {
        private readonly List<DynamicRackFrontDesign> fronts = new List<DynamicRackFrontDesign>();
        private readonly List<PushBackFrontConfig> configs = new List<PushBackFrontConfig>();

        private PushBackSideConfiguration(PushBackSide side)
        {
            Side = side;
        }

        public PushBackSide Side { get; private set; }

        /// <summary>False = el lado no existe: no aporta celda, cama, larguero, tope ni demanda de estructura.</summary>
        public bool IsPresent { get; private set; }

        /// <summary>Niveles por defecto del lado (un frente sin valor propio los hereda).</summary>
        public int LoadLevels { get; private set; }

        /// <summary>Elevacion del primer larguero por defecto del lado.</summary>
        public double FirstLevelHeight { get; private set; }

        /// <summary>Fallback rack-wide del peralte del larguero posterior de este lado.</summary>
        public double LegacyHighEndBeamPeralte { get; private set; }

        /// <summary>La rejilla de topes del lado (nunca nula).</summary>
        public PushBackRearTopeConfig RearTope { get; private set; }

        /// <summary>El override manual de la estructura del lado, o null si sigue la propuesta.</summary>
        public int? StructureOverride { get; private set; }

        /// <summary>Numero de ranuras transversales que este lado declara (puede diferir del otro lado).</summary>
        public int SlotCount => fronts.Count;

        /// <summary>El frente de la ranura en este lado, o null si la ranura no existe aqui.</summary>
        public DynamicRackFrontDesign Front(int slot)
            => slot >= 0 && slot < fronts.Count ? fronts[slot] : null;

        /// <summary>La configuracion Push Back de la ranura, o null.</summary>
        public PushBackFrontConfig Config(int slot)
            => slot >= 0 && slot < configs.Count ? configs[slot] : null;

        /// <summary>True cuando la ranura tiene estructura y celdas en este lado.</summary>
        public bool HasSlot(int slot) => IsPresent && Front(slot) != null;

        /// <summary>Los niveles EFECTIVOS de una ranura en este lado (0 si la ranura no existe o esta en blanco).</summary>
        public int Levels(int slot)
        {
            var front = Front(slot);
            if (!IsPresent || front == null || !front.IsActive)
            {
                return 0;
            }

            return Math.Max(1, front.LoadLevels ?? LoadLevels);
        }

        /// <summary>
        /// El fondo POR DEFECTO de una ranura en este lado: el explicito de la configuracion Push Back y, si no lo
        /// hay, el estructural del frente. Es exactamente la precedencia de I-41, sin una tercera fuente.
        /// </summary>
        public int DefaultDeep(int slot)
        {
            var stored = Config(slot)?.DefaultPalletsDeep;
            if (stored.HasValue && stored.Value >= PushBackCellDepth.MinimumPalletsDeep)
            {
                return stored.Value;
            }

            var requested = Front(slot)?.PalletsDeep;
            return requested.HasValue && requested.Value >= PushBackCellDepth.MinimumPalletsDeep
                ? requested.Value
                : PushBackCellDepth.MinimumPalletsDeep;
        }

        /// <summary>El fondo EFECTIVO de una celda de este lado: <c>override de la celda ?? default del frente</c>.</summary>
        public int EffectiveDeep(int slot, int level)
            => PushBackCellDepth.Effective(Config(slot)?.PalletsDeepOverrideAt(level), DefaultDeep(slot));

        /// <summary>Si la celda de este lado dibuja su tarima (I-41/PB-016). Fuera del BOM, siempre.</summary>
        public bool DrawPallet(int slot, int level) => Config(slot)?.DrawPalletAt(level) ?? false;

        /// <summary>
        /// La ENVOLVENTE de una ranura en este lado: el mayor fondo efectivo de sus niveles activos. Es la demanda
        /// que esa ranura le pone a la estructura, no una decision estructural.
        /// </summary>
        public int Envelope(int slot)
        {
            var levels = Levels(slot);
            var envelope = DefaultDeep(slot);
            for (var level = 0; level < levels; level++)
            {
                envelope = Math.Max(envelope, EffectiveDeep(slot, level));
            }

            return envelope;
        }

        /// <summary>
        /// La estructura PROPUESTA del lado: la mayor envolvente de sus ranuras presentes. Derivada siempre, nunca
        /// almacenada — por eso «restaurar estructura» es simplemente borrar el override y volver a preguntar aqui.
        /// </summary>
        public int ProposedStructure()
        {
            if (!IsPresent)
            {
                return 0;
            }

            var proposed = 0;
            for (var slot = 0; slot < SlotCount; slot++)
            {
                if (Front(slot) != null)
                {
                    proposed = Math.Max(proposed, Envelope(slot));
                }
            }

            return Math.Max(PushBackCellDepth.MinimumPalletsDeep, proposed);
        }

        /// <summary>
        /// La estructura EFECTIVA del lado: el override manual si lo hay, y si no la propuesta. El override
        /// SUSTITUYE a la propuesta, no la acota: por eso un override menor deja celdas fisicamente imposibles, que
        /// se reportan como tales en vez de recortarse en silencio.
        /// </summary>
        public int EffectiveStructure()
        {
            if (!IsPresent)
            {
                return 0;
            }

            return StructureOverride.HasValue && StructureOverride.Value >= PushBackCellDepth.MinimumPalletsDeep
                ? StructureOverride.Value
                : ProposedStructure();
        }

        /// <summary>
        /// La profundidad ESTRUCTURAL de una ranura: su envolvente acotada por la estructura efectiva del lado. Un
        /// frente mas corto sigue siendo mas corto (asi conviven frentes cortos y largos sobre la misma estructura);
        /// uno mas largo que la estructura efectiva no la extiende — sus celdas quedan sin apoyo y se declaran.
        /// </summary>
        public int SlotStructure(int slot)
        {
            if (!HasSlot(slot))
            {
                return 0;
            }

            return Math.Max(PushBackCellDepth.MinimumPalletsDeep, Math.Min(Envelope(slot), EffectiveStructure()));
        }

        /// <summary>La lectura del lado A: los campos legacy del diseno, sin traduccion ni copia intermedia.</summary>
        public static PushBackSideConfiguration ForA(PushBackDesign design)
        {
            var result = new PushBackSideConfiguration(PushBackSide.A)
            {
                IsPresent = true,
                LoadLevels = Math.Max(1, design?.Structure?.LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels),
                FirstLevelHeight = design?.Structure?.FirstLevelHeight ?? PushBackDefaults.DefaultFirstLevelHeight,
                LegacyHighEndBeamPeralte = design?.LegacyHighEndBeamPeralte ?? PushBackDefaults.HighEndBeamDefaultPeralte,
                RearTope = design?.RearTope ?? new PushBackRearTopeConfig(),
                StructureOverride = design?.Composite?.StructureOverrideA
            };

            var designFronts = design?.Structure?.Fronts?.ToList() ?? new List<DynamicRackFrontDesign>();
            if (designFronts.Count == 0)
            {
                // El diseno legacy sin frentes explicitos resuelve UN frente: se representa igual aqui para que la
                // lectura uniforme no invente ranuras que el rack no tiene.
                designFronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = DynamicRackDefaults.DefaultPalletsWide,
                    PalletsDeep = design?.Structure?.PalletsDeep,
                    DepthStartPosition = 1
                });
            }

            result.fronts.AddRange(designFronts);
            for (var slot = 0; slot < designFronts.Count; slot++)
            {
                result.configs.Add(design?.FrontConfig(slot));
            }

            return result;
        }

        /// <summary>La lectura del lado B, desde su configuracion funcional propia.</summary>
        public static PushBackSideConfiguration ForB(PushBackDesign design)
        {
            var side = design?.SideB;
            var result = new PushBackSideConfiguration(PushBackSide.B)
            {
                IsPresent = side != null && side.IsPresent,
                LoadLevels = Math.Max(1, side?.LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels),
                FirstLevelHeight = side?.FirstLevelHeight ?? PushBackDefaults.DefaultFirstLevelHeight,
                LegacyHighEndBeamPeralte = side?.LegacyHighEndBeamPeralte ?? PushBackDefaults.HighEndBeamDefaultPeralte,
                RearTope = side?.RearTope ?? new PushBackRearTopeConfig(),
                StructureOverride = design?.Composite?.StructureOverrideB
            };

            if (side != null)
            {
                result.fronts.AddRange(side.Fronts);
                result.configs.AddRange(side.FrontConfigs);
            }

            return result;
        }

        /// <summary>La lectura del lado pedido.</summary>
        public static PushBackSideConfiguration For(PushBackDesign design, PushBackSide side)
            => side == PushBackSide.A ? ForA(design) : ForB(design);
    }
}
