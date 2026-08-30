using System;

namespace RackCad.Domain.Systems.Selective
{
    /// <summary>
    /// I-42 (ronda 7E) — LA PIEZA QUE UNA CARA MATERIALIZA, cuando un sistema declara una por extremo.
    ///
    /// <para>
    /// Un Push Back compuesto tiene DOS pasillos —uno en cada extremo de la cobertura de cada linea (ronda 6D)— y
    /// cada uno elige su tipo de defensa por separado. Esta declaracion es lo que permite decirlo sin inventar una
    /// codificacion: la cara CERCANA y la LEJANA pueden llevar piezas distintas, o ninguna.
    /// </para>
    ///
    /// <para>
    /// Tiene tres estados y solo tres: <b>heredar</b> (la referencia es null: esa cara usa el
    /// <see cref="SelectiveSafetySelection.ElementId"/> de siempre, que es lo que hace todo sistema que no rellene
    /// esto y todo documento anterior), <b>ninguna</b> (<see cref="None"/>) y <b>una pieza concreta</b>.
    /// </para>
    ///
    /// <para>
    /// Es DERIVADA y no se persiste, igual que <see cref="SelectiveSafetySelection.LowEndOnly"/> y
    /// <see cref="SelectiveSafetySelection.BothEndsAreLoadFaces"/>: quien la persiste es el diseno de cada lado, y la
    /// autoridad del sistema la vuelve a imponer en cada limite que posee.
    /// </para>
    /// </summary>
    public sealed class SafetyFacePiece
    {
        private SafetyFacePiece(bool isNone, string elementId)
        {
            IsNone = isNone;
            ElementId = elementId;
        }

        /// <summary>Esa cara no lleva ninguna pieza. No es una pieza «vacia»: no hay bloque, ni linea de BOM.</summary>
        public static SafetyFacePiece None { get; } = new SafetyFacePiece(isNone: true, elementId: null);

        /// <summary>La cara lleva <paramref name="elementId"/>. Un id en blanco es <see cref="None"/>.</summary>
        public static SafetyFacePiece Of(string elementId)
            => string.IsNullOrWhiteSpace(elementId) ? None : new SafetyFacePiece(isNone: false, elementId.Trim());

        public bool IsNone { get; }

        /// <summary>El id de catalogo de la pieza, o null cuando la cara no lleva ninguna.</summary>
        public string ElementId { get; }

        /// <summary>
        /// El id que materializa <paramref name="face"/>, con <paramref name="inherited"/> como valor heredado
        /// cuando la cara no declara nada. NULL significa que esa cara no dibuja: es la unica lectura que los
        /// constructores necesitan hacer.
        /// </summary>
        public static string Resolve(SafetyFacePiece face, string inherited)
        {
            if (face == null)
            {
                return string.IsNullOrWhiteSpace(inherited) ? null : inherited;
            }

            return face.IsNone ? null : face.ElementId;
        }

        public override string ToString()
            => IsNone ? "(ninguna)" : ElementId ?? "(heredada)";

        public override bool Equals(object obj)
            => obj is SafetyFacePiece other
               && other.IsNone == IsNone
               && string.Equals(other.ElementId, ElementId, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode()
            => IsNone ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(ElementId ?? string.Empty);
    }
}
