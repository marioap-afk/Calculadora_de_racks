namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// The Cantilever constants the owner approved (I-37A), each one a DEFAULT and never a hard-coded rule:
    /// every value here is reachable through a design property, so changing one is an edit, not a rebuild.
    ///
    /// Two numbers the owner did NOT approve are deliberately absent — the offset from the ends of the
    /// column's bottom plate to its punches, and the offset from the column top to its last regular punch.
    /// They are REQUIRED design inputs (nullable in <see cref="CantileverPunchParameters"/>, rejected by the
    /// resolver when missing) precisely so that nobody can invent them here and have the invention look like
    /// an approved value.
    /// </summary>
    public static class CantileverDefaults
    {
        /// <summary>Punch diameter, inches. One diameter for every punch of the sub-assembly.</summary>
        public const double PunchDiameter = 0.75;

        /// <summary>
        /// Distancia horizontal, en pulgadas, del EXTERIOR DE LA PLACA POSTERIOR hacia adentro, hasta el
        /// centro de cada una de las dos filas de troqueles.
        ///
        /// La columna sigue gobernando el patrón —son sus filas, las que suben por la rejilla regular y a las
        /// que se atornilla un brazo— pero el borde desde el que se acota es el de la placa. Es el datum
        /// físicamente correcto: el troquel atraviesa las dos piezas, así que manda la más angosta, y la placa
        /// lo es porque su ancho es el del patín de la base.
        ///
        /// UNA PULGADA por corrección del dueño. Fue 1.5 in y él mismo declaró que ese valor era un error
        /// suyo. Es la ÚNICA autoridad del número: <see cref="CantileverPunchParameters.HorizontalEndOffset"/>
        /// lo toma de aquí y nadie más lo escribe, así que preview, dibujo, BOM y cualquier resolutor derivado
        /// se mueven juntos y no pueden quedar uno en 1.5 y otro en 1.0.
        /// </summary>
        public const double PunchHorizontalEndOffset = 1.00;

        /// <summary>Vertical spacing of the connection region, inches.</summary>
        public const double ConnectionPunchPitch = 2.00;

        /// <summary>
        /// Vertical distance from the bottom edge of the rear plate to its first punch, inches, and also the
        /// distance from its last punch to its top edge. One parameter, used at both ends on purpose: the
        /// plate is symmetric in how it treats its two ends.
        /// </summary>
        public const double RearPlateVerticalEndOffset = 2.50;

        /// <summary>Vertical spacing of the column's regular region, inches. Twice the connection pitch.</summary>
        public const double RegularColumnPunchPitch = 4.00;

        /// <summary>
        /// How many connection punches sit ABOVE the base section envelope. Exactly three, by the owner's
        /// decision — the number is not derived from the plate height; the plate height is derived from it.
        /// </summary>
        public const int ConnectionPunchesAboveBase = 3;

        /// <summary>Spacing of the column bottom plate punches along the section depth, inches.</summary>
        public const double ColumnBottomPlatePunchPitch = 2.00;

        /// <summary>
        /// Default thickness of a plate or of the gusset, inches.
        ///
        /// It is ONE constant used as the default of FOUR independent properties — front plate, rear plate,
        /// column bottom plate and gusset. Sharing a default is not sharing an authority: each component
        /// keeps its own value and changing one never moves the others.
        ///
        /// I-37B adds two more independent users — the arm's mounting plate and its end plate — on the same
        /// terms.
        /// </summary>
        public const double PlateThickness = 0.25;

        /// <summary>
        /// How many column punch elevations an arm's mounting plate uses by default (I-37B).
        ///
        /// Two is also the minimum the resolver enforces: a single bolt line is a hinge, not a connection.
        /// There is no maximum — more rows simply extend the plate upwards (ADR-0025, D6).
        /// </summary>
        public const int ArmVerticalPunchCount = 2;

        /// <summary>
        /// Pendiente por omisión del cuerpo del brazo: 7/16 de subida por cada 12 in de vuelo.
        ///
        /// Valor aprobado por el dueño. Se escribe como la división que es —no como 0.4375— porque lo que él
        /// dictó es la fracción, y un decimal redondeado a mano es la clase de número que nadie sabe de dónde
        /// salió cuando hay que revisarlo.
        /// </summary>
        public const double ArmSlopeRisePer12 = 7.0 / 16.0;

        /// <summary>
        /// Margen vertical por omisión de la placa de montaje del brazo, en pulgadas: del primer troquel
        /// elegido al borde inferior de la placa, y del último a su borde superior.
        ///
        /// DEJÓ DE SER un parámetro sin default aprobado. Hasta la ronda 3 era obligatorio y nulo a propósito,
        /// para que nadie inventara un número que pareciese aprobado; el dueño aprobó 2 in, así que ya lo es.
        /// La regla de rechazo sobrevive para un diseño que lo ponga en null explícitamente —uno leído de un
        /// JSON viejo— porque «ausente» y «aprobado» siguen sin ser lo mismo.
        /// </summary>
        public const double ArmMountingPlateVerticalEndOffset = 2.00;
    }
}
