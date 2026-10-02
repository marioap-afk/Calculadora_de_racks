using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// El lector de diseno por kind (I-63 D-26, E5): decide si el diseno de UNA hermana es legible. Es el costado
    /// donde se observan las lecturas (A-1.3): una prueba lo envuelve para contar, y los <i>stores</i> no se tocan.
    /// </summary>
    public interface IRackMetricDesignReader
    {
        /// <summary>True cuando el diseno de <paramref name="kindToken"/> es legible. Nunca lanza: una excepcion del store es «ilegible».</summary>
        bool IsReadable(string kindToken, string designJson);
    }

    /// <summary>
    /// Implementacion real de D-26. Solo reutiliza los <i>stores</i> vigentes; reproduce las comprobaciones de
    /// presencia de los <c>BuildBom</c> de los handlers sin cambiarlos (AQ-03).
    /// </summary>
    public sealed class RackMetricDesignReader : IRackMetricDesignReader
    {
        public bool IsReadable(string kindToken, string designJson)
        {
            try
            {
                switch (kindToken)
                {
                    case RackEmbedDocument.KindSelective:
                        return new SelectivePalletDesignStore().Deserialize(designJson) != null;

                    case RackEmbedDocument.KindDynamic:
                    {
                        var project = new RackProjectStore().Deserialize(designJson);
                        return (project?.DynamicDesign ?? (object)project?.DynamicSystem) != null;
                    }

                    case RackEmbedDocument.KindPushBack:
                        return new RackProjectStore().Deserialize(designJson)?.PushBackDesign != null;

                    case RackEmbedDocument.KindCantilever:
                        return new RackProjectStore().Deserialize(designJson)?.CantileverLineDesign != null;

                    case RackEmbedDocument.KindCabecera:
                        return new RackProjectStore().Deserialize(designJson)?.Header != null;

                    case RackEmbedDocument.KindCama:
                        return new FlowBedConfigurationStore().Deserialize(designJson) != null;

                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
