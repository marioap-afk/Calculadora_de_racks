using System.Collections.Generic;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Domain.Systems.PushBack
{
    /// <summary>
    /// I-42 — la CONFIGURACION FUNCIONAL de un lado del Push Back compuesto. Contiene exclusivamente lo que un lado
    /// posee por si mismo: sus frentes (niveles, elevaciones, fondos y celdas), su configuracion Push Back por frente
    /// y su rejilla de topes.
    ///
    /// <para>
    /// Lo que NO vive aqui —y no puede vivir aqui— es la ESTRUCTURA FISICA: postes, perfil y peralte de poste,
    /// cabeceras, separadores, postes derivados, overrides de linea de I-40, anotaciones y seguridad. Todo eso es
    /// propiedad UNICA del rack y vive en <see cref="PushBackDesign.Structure"/>. Esa separacion es la regla
    /// arquitectonica de I-42: estructura fisica != configuracion funcional de almacenamiento.
    /// </para>
    /// <para>
    /// El lado A no usa este tipo: su configuracion funcional ES la del diseno legacy
    /// (<see cref="PushBackDesign.Structure"/>.Fronts + <see cref="PushBackDesign.Fronts"/>), y por eso un rack
    /// anterior a I-42 —que no tiene lado B— se comporta exactamente igual sin migrar nada.
    /// </para>
    /// </summary>
    public sealed class PushBackSideDesign
    {
        /// <summary>False deja el lado declarado pero AUSENTE: no aporta celda, cama, larguero ni tope.</summary>
        public bool IsPresent { get; set; } = true;

        /// <summary>Niveles por defecto del lado; un frente sin valor propio hereda este.</summary>
        public int LoadLevels { get; set; } = DynamicRackDefaults.DefaultLoadLevels;

        /// <summary>Elevacion del primer larguero del lado; un frente sin valor propio hereda esta.</summary>
        public double FirstLevelHeight { get; set; } = PushBackDefaults.DefaultFirstLevelHeight;

        /// <summary>Fallback rack-wide del peralte del larguero posterior de ESTE lado.</summary>
        public double LegacyHighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

        /// <summary>
        /// Configuracion transversal del lado, alineada POR INDICE con las ranuras transversales del rack. Una
        /// entrada nula significa que la ranura no existe en este lado (p. ej. A=3 y B=4: la cuarta entrada de A es
        /// nula). Las ranuras las gobierna la mayor demanda de los dos lados.
        /// </summary>
        public IList<DynamicRackFrontDesign> Fronts { get; } = new List<DynamicRackFrontDesign>();

        /// <summary>Configuracion Push Back por ranura (peralte posterior, fondo por celda, tarima), alineada por indice.</summary>
        public IList<PushBackFrontConfig> FrontConfigs { get; } = new List<PushBackFrontConfig>();

        /// <summary>Rejilla de topes posteriores del lado: activa por defecto, solo se persisten desactivaciones.</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

        /// <summary>
        /// I-42 (ronda 7E) — el TIPO de defensa de montacargas de ESTE lado: un id de catalogo,
        /// <see cref="PushBackDefaults.NonePieceId"/> para «ninguno», o NULL para el comportamiento historico (la
        /// pieza que la seleccion de seguridad del rack ya traia). Un lado es un pasillo propio y elige la suya; hoy
        /// el catalogo ofrece una sola, y el contrato no supone que vaya a seguir siendo asi.
        /// </summary>
        public string DefensePieceId { get; set; }

        /// <summary>La configuracion Push Back de la ranura <paramref name="frontIndex"/>, o null si no hay ninguna.</summary>
        public PushBackFrontConfig FrontConfig(int frontIndex)
            => frontIndex >= 0 && frontIndex < FrontConfigs.Count ? FrontConfigs[frontIndex] : null;

        /// <summary>El diseno estructural de la ranura <paramref name="frontIndex"/> en este lado, o null si no existe.</summary>
        public DynamicRackFrontDesign Front(int frontIndex)
            => frontIndex >= 0 && frontIndex < Fronts.Count ? Fronts[frontIndex] : null;

        public PushBackSideDesign DeepCopy()
        {
            var copy = new PushBackSideDesign
            {
                IsPresent = IsPresent,
                LoadLevels = LoadLevels,
                FirstLevelHeight = FirstLevelHeight,
                LegacyHighEndBeamPeralte = LegacyHighEndBeamPeralte,
                RearTope = RearTope?.DeepCopy() ?? new PushBackRearTopeConfig(),
                DefensePieceId = DefensePieceId
            };

            foreach (var front in Fronts)
            {
                copy.Fronts.Add(CopyFront(front));
            }

            foreach (var config in FrontConfigs)
            {
                copy.FrontConfigs.Add(config?.DeepCopy());
            }

            return copy;
        }

        /// <summary>
        /// Copia independiente de un frente dinamico. Vive aqui porque <see cref="DynamicRackFrontDesign"/> no expone
        /// una y I-42 no puede permitirse que dos lados compartan la misma instancia: editar B moveria A en silencio.
        /// </summary>
        public static DynamicRackFrontDesign CopyFront(DynamicRackFrontDesign source)
        {
            if (source == null)
            {
                return null;
            }

            var copy = new DynamicRackFrontDesign
            {
                IsActive = source.IsActive,
                PalletCount = source.PalletCount,
                LoadLevels = source.LoadLevels,
                PalletsDeep = source.PalletsDeep,
                DepthStartPosition = source.DepthStartPosition,
                BeamLengthOverride = source.BeamLengthOverride,
                FirstLevelHeight = source.FirstLevelHeight
            };

            foreach (var peralte in source.IntermediateBeamDepths)
            {
                copy.IntermediateBeamDepths.Add(peralte);
            }

            foreach (var level in source.Levels)
            {
                copy.Levels.Add(level == null
                    ? null
                    : new DynamicRackLevelDesign
                    {
                        PalletFront = level.PalletFront,
                        PalletHeight = level.PalletHeight,
                        PalletWeight = level.PalletWeight,
                        ClearHeight = level.ClearHeight,
                        InOutBeamCatalogId = level.InOutBeamCatalogId,
                        InOutBeamDepth = level.InOutBeamDepth,
                        BeamLengthOverride = level.BeamLengthOverride,
                        IntermediateBeamCatalogId = level.IntermediateBeamCatalogId,
                        IntermediateBeamDepth = level.IntermediateBeamDepth
                    });
            }

            return copy;
        }
    }
}
