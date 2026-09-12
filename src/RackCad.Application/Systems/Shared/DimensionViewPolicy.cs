using System;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// I-50 (ADR-0035) — el TIPO de vista que pregunta por sus cotas. <c>Section</c> no crea tipo: las frontales de
    /// cada fondo, la de salida y la de entrada y los cortes frontales de Push Back son <see cref="Frontal"/>; el
    /// lateral entero y todos sus cortes son <see cref="Lateral"/>.
    /// </summary>
    public enum DimensionViewKind
    {
        Frontal,
        Lateral,
        Planta
    }

    /// <summary>
    /// I-50 (ADR-0035) — LA regla única de visibilidad de cotas por tipo de vista. Los emisores la consultan una vez por
    /// vista y dibujan con el detalle que devuelve, así que las cotas y el alcance de las etiquetas no pueden
    /// discrepar; el editor la usa para decidir qué valor guarda.
    /// </summary>
    public static class DimensionViewPolicy
    {
        /// <summary>Los únicos bits que la regla observa. Todo lo demás de un valor presente, signo incluido, se conserva.</summary>
        private const DimensionViewVisibility Known =
            DimensionViewVisibility.Frontal | DimensionViewVisibility.Lateral | DimensionViewVisibility.Planta;

        /// <summary>
        /// El detalle con que dibuja un tipo de vista. <see cref="DimensionDetail.None"/> siempre gana; una política
        /// <c>null</c> es el legacy exacto y devuelve <paramref name="detail"/>; una política presente lo devuelve si el
        /// bit del tipo está encendido y <see cref="DimensionDetail.None"/> si está apagado. Un tipo no definido lanza.
        /// </summary>
        public static DimensionDetail EffectiveDetail(
            DimensionDetail detail, DimensionViewVisibility? policy, DimensionViewKind viewKind)
        {
            var bit = BitOf(viewKind);
            if (detail == DimensionDetail.None)
            {
                return DimensionDetail.None;
            }

            if (policy == null)
            {
                return detail;
            }

            return (policy.Value & bit) != 0 ? detail : DimensionDetail.None;
        }

        /// <summary>
        /// El valor que guarda el editor. Sin tocar las casillas devuelve EXACTAMENTE lo cargado —<c>null</c> sigue
        /// <c>null</c> y un valor presente vuelve igual—. Tocadas, sustituye solo los bits Frontal, Lateral y Planta y
        /// conserva cualquier otro bit, incluido el de signo; nunca devuelve <c>null</c>, ni siquiera con las tres
        /// casillas marcadas.
        /// </summary>
        public static DimensionViewVisibility? FromEditor(
            DimensionViewVisibility? loaded, bool touched, bool frontal, bool lateral, bool planta)
        {
            if (!touched)
            {
                return loaded;
            }

            var checkboxes = (frontal ? DimensionViewVisibility.Frontal : DimensionViewVisibility.None)
                | (lateral ? DimensionViewVisibility.Lateral : DimensionViewVisibility.None)
                | (planta ? DimensionViewVisibility.Planta : DimensionViewVisibility.None);
            return ((loaded ?? DimensionViewVisibility.None) & ~Known) | checkboxes;
        }

        private static DimensionViewVisibility BitOf(DimensionViewKind viewKind)
        {
            switch (viewKind)
            {
                case DimensionViewKind.Frontal:
                    return DimensionViewVisibility.Frontal;
                case DimensionViewKind.Lateral:
                    return DimensionViewVisibility.Lateral;
                case DimensionViewKind.Planta:
                    return DimensionViewVisibility.Planta;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewKind), viewKind, "Tipo de vista de cotas no definido.");
            }
        }
    }
}
