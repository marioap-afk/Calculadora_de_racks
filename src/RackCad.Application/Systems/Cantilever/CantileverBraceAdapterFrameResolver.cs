using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// De qué extremo de qué diagonal es un adaptador: las cuatro manos posibles.
    ///
    /// No es decoración. Un ángulo tiene talón, y el talón mira a un sitio distinto en cada esquina del panel;
    /// dibujar los cuatro iguales es lo que hacía que la pieza pareciera girada al azar.
    /// </summary>
    public enum CantileverBraceAdapterHand
    {
        LowerLeft = 0,
        LowerRight = 1,
        UpperLeft = 2,
        UpperRight = 3
    }

    /// <summary>
    /// El marco FÍSICO de un adaptador de tensor, con sus dos agujeros ya situados.
    ///
    /// Es lo que devuelve <see cref="CantileverBraceAdapterFrameResolver"/>: tres ejes ortonormales, los dos
    /// centros de agujero en coordenadas de mundo y la mano. Todo lo demás —el prisma, el contorno, la
    /// longitud del tensor— se deriva de aquí y no se vuelve a decidir en ningún otro sitio.
    /// </summary>
    public readonly struct CantileverBraceAdapterFrame
    {
        internal CantileverBraceAdapterFrame(
            Vector3D alongSeatedLeg,
            Vector3D alongRodLeg,
            Vector3D alongCut,
            Point3D separatorHoleCentre,
            Point3D rodHoleCentre,
            Point3D heel,
            CantileverBraceAdapterHand hand)
        {
            AlongSeatedLeg = alongSeatedLeg;
            AlongRodLeg = alongRodLeg;
            AlongCut = alongCut;
            SeparatorHoleCentre = separatorHoleCentre;
            RodHoleCentre = rodHoleCentre;
            Heel = heel;
            Hand = hand;
        }

        /// <summary>
        /// Hacia dónde CRECE el ala apoyada, desde el talón. Es la X de la sección.
        ///
        /// Apunta en sentido contrario al otro extremo del tensor: el ala apoyada se aleja del vano y el ala
        /// de la varilla queda del lado de dentro. Eso es lo que impide que el cuerpo del adaptador invada el
        /// tramo por el que pasa la varilla.
        /// </summary>
        public Vector3D AlongSeatedLeg { get; }

        /// <summary>
        /// Hacia dónde CRECE el ala que recibe la varilla, desde el talón. Es la Y de la sección.
        ///
        /// Es la NORMAL de la cara del separador: el ala apoyada está tumbada sobre esa cara y la otra sale
        /// perpendicular a ella, hacia fuera del plano del panel.
        /// </summary>
        public Vector3D AlongRodLeg { get; }

        /// <summary>El eje del CORTE de 2 in: la Z de la sección, por la que se extruye el prisma.</summary>
        public Vector3D AlongCut { get; }

        /// <summary>Centro del agujero que bolta al separador, en el plano medio REAL del ala apoyada.</summary>
        public Point3D SeparatorHoleCentre { get; }

        /// <summary>
        /// Centro del agujero de la varilla, en el plano medio REAL del ala perpendicular.
        ///
        /// Es el DATUM FÍSICO del extremo del tensor: de él —y no del perno del separador— salen la longitud
        /// nominal, el eje y el contorno visible de la varilla.
        /// </summary>
        public Point3D RodHoleCentre { get; }

        /// <summary>El talón del ángulo: la esquina exterior donde se cruzan las caras de fuera de las dos alas.</summary>
        public Point3D Heel { get; }

        public CantileverBraceAdapterHand Hand { get; }

        /// <summary>Lo que separa los dos agujeros. Tiene las TRES componentes, y ese es el punto.</summary>
        public Vector3D HoleSeparation => RodHoleCentre - SeparatorHoleCentre;
    }

    /// <summary>
    /// THE authority que orienta y sitúa un adaptador de tensor.
    ///
    /// <para><b>Qué se revocó y por qué.</b> Hasta la ronda 4 de I-37D el agujero de la varilla se ponía a
    /// <c>CutLength / 2</c> del agujero del separador, medido a lo largo de la diagonal, y la razón escrita
    /// era «medio corte, porque las dos caras son perpendiculares». Esa razón es justamente la que no se
    /// sostiene: si las dos caras son perpendiculares, la separación entre dos agujeros centrados cada uno en
    /// SU ala no puede ser un solo número a lo largo de un solo eje — tiene componente en los dos. El
    /// resultado era un <c>ΔY = 0</c> forzado, o sea una pieza plana metida en el plano del panel, que es
    /// exactamente la aproximación de dibujo a mano que esta ronda viene a quitar. El dueño la revocó.</para>
    ///
    /// <para><b>De dónde sale el marco ahora.</b> De la geometría, y de nada más:</para>
    ///
    /// <list type="number">
    ///   <item>la NORMAL DE LA CARA del separador da el eje del ala que recibe la varilla, porque el ala
    ///         apoyada está tumbada sobre esa cara y la otra sale perpendicular a ella;</item>
    ///   <item>el EJE NOMINAL DE LA DIAGONAL da el eje del ala apoyada, en sentido contrario al otro extremo,
    ///         para que el cuerpo del adaptador no invada el vano de la varilla;</item>
    ///   <item>el producto vectorial de los dos da el eje del CORTE, que por construcción queda en el plano
    ///         del panel y perpendicular a la diagonal.</item>
    /// </list>
    ///
    /// <para><b>Los dos agujeros, cada uno centrado en SU ala.</b> Un ángulo de brazo <c>L</c> y espesor
    /// <c>t</c>, cortado <c>c</c>, tiene sus dos agujeros —en coordenadas de talón, midiendo <c>a</c> a lo
    /// largo del ala apoyada y <c>b</c> a lo largo de la del tensor— en:</para>
    ///
    /// <list type="table">
    ///   <item><term>separador</term><description><c>a = L/2</c> (centrado en su ala), <c>b = t/2</c> (plano medio)</description></item>
    ///   <item><term>varilla</term><description><c>a = t/2</c> (plano medio), <c>b = L/2</c> (centrado en su ala)</description></item>
    /// </list>
    ///
    /// <para>De ahí sale todo lo demás sin decidir nada más. La separación entre los dos es
    /// <c>(L − t)/2</c> a lo largo de CADA uno de los dos ejes, y como los ejes son perpendiculares su módulo
    /// es <c>((L − t)/2)·√2</c> — para el <c>L2×2×3/16</c> del producto, <b>0.90625 in en cada eje y
    /// 1.28163 in en total</b>, contra el <c>1.0</c> plano y sin componente en Y de la aproximación
    /// revocada.</para>
    ///
    /// <para><b>Consecuencia sobre el tensor, dicha aquí porque es donde se origina.</b> El agujero de la
    /// varilla queda a <c>(L − t)/2</c> del perno hacia el otro extremo, y no a <c>c/2</c>. Con los números
    /// del producto eso son 0.90625 in en vez de 1.0, en los DOS extremos, así que la longitud nominal del
    /// tensor CRECE en <c>2 · (c/2 − (L − t)/2)</c>. Cuando el corte mide lo que mide el brazo —que es el
    /// caso del producto— eso se simplifica a <b>exactamente el espesor del ángulo</b>: 3/16 in. No es una
    /// coincidencia bonita, es que la aproximación revocada medía hasta la CARA del ala y la física mide
    /// hasta su PLANO MEDIO, y entre una y otra hay medio espesor por extremo.</para>
    ///
    /// <para>Es geometría de colocación, no de fabricación: aquí no hay preparación de bordes, ni tolerancias
    /// de armado, ni la soldadura del talón.</para>
    /// </summary>
    public static class CantileverBraceAdapterFrameResolver
    {
        /// <summary>
        /// Cuánto se separan los dos agujeros a lo largo de CADA uno de los dos ejes del ángulo.
        ///
        /// <c>(leg − thickness) / 2</c>, que es lo que queda entre «centrado en un ala de brazo <c>leg</c>» y
        /// «en el plano medio de un ala de espesor <c>thickness</c>». Se expone porque es EL número que la
        /// decisión del dueño fija, y una prueba que lo verifique tiene que poder pedirlo sin construir un
        /// adaptador entero.
        /// </summary>
        public static double HoleOffsetPerAxis(double leg, double thickness) => (leg - thickness) / 2.0;

        /// <summary>
        /// Cuánto CRECE la longitud nominal de un tensor al pasar de la aproximación revocada a la física.
        ///
        /// La revocada ponía el agujero a <c>cut/2</c> del perno; la física lo pone a
        /// <see cref="HoleOffsetPerAxis"/>. Son dos extremos, así que el tensor cambia el doble de la
        /// diferencia. Positivo significa que el tensor se alarga.
        /// </summary>
        public static double NominalLengthGrowth(double leg, double thickness, double cut) =>
            2.0 * ((cut / 2.0) - HoleOffsetPerAxis(leg, thickness));

        /// <summary>
        /// El marco físico de un adaptador, o un motivo por el que no lo hay.
        /// </summary>
        /// <param name="separatorFaceHole">
        /// El troquel de tensor del separador: el punto donde el perno cruza la CARA de la pieza. No es el
        /// centro del agujero del adaptador — ese está medio espesor más afuera, en el plano medio del ala
        /// apoyada— y distinguirlos es la mitad de esta corrección.
        /// </param>
        /// <param name="faceNormal">
        /// Normal de la cara del separador, hacia fuera del plano del panel. Es el eje por el que crece el
        /// ala que recibe la varilla.
        /// </param>
        /// <param name="towardsOtherEnd">
        /// Eje nominal de la diagonal, desde ESTE extremo hacia el otro. Da el lado y el extremo —o sea la
        /// mano— y, cambiado de signo, el sentido en que crece el ala apoyada.
        /// </param>
        public static bool TryResolve(
            Point3D separatorFaceHole,
            Vector3D faceNormal,
            Vector3D towardsOtherEnd,
            double leg,
            double thickness,
            out CantileverBraceAdapterFrame frame,
            out string reason)
        {
            frame = default;
            reason = null;

            if (!(leg > 0.0) || !(thickness > 0.0) || thickness >= leg)
            {
                reason =
                    "El adaptador declara un brazo de " + Format(leg) + " in y un espesor de " +
                    Format(thickness) + " in, y un angulo necesita espesor positivo menor que su brazo.";
                return false;
            }

            var dx = towardsOtherEnd.X;
            var dz = towardsOtherEnd.Z;

            // Una diagonal que no avanza en las dos direcciones no es una diagonal, y de una recta horizontal o
            // vertical no se puede leer ni el lado ni el extremo. Se rechaza en vez de inventarle una mano.
            if (Math.Abs(dx) <= GeometryTolerance.Length || Math.Abs(dz) <= GeometryTolerance.Length)
            {
                reason =
                    "El eje de la diagonal no define un extremo de adaptador: avanza " + Format(dx) +
                    " in en X y " + Format(dz) + " in en Z, y hacen falta las dos para saber hacia donde mira.";
                return false;
            }

            if (faceNormal.Length <= GeometryTolerance.Length)
            {
                reason = "La cara del separador no tiene normal, asi que el adaptador no sabe sobre que apoya.";
                return false;
            }

            var alongRodLeg = faceNormal.Normalized();

            // EL ALA APOYADA CRECE EN CONTRA DEL VANO. Si creciera hacia el otro extremo, el cuerpo del
            // adaptador se metería justo por donde pasa la varilla; con este signo el ala del tensor queda del
            // lado de dentro —tocando el vano con su espesor y nada más— y el resto de la pieza se aleja.
            var alongSeatedLeg = (towardsOtherEnd * -1.0).Normalized();

            // Ortogonalización explícita: la diagonal vive en el plano del panel y la normal de la cara es
            // perpendicular a ese plano, así que en el producto ya salen ortogonales. Se proyecta igualmente
            // para que un día que la cara deje de ser perpendicular al panel esto siga dando un marco y no una
            // terna torcida.
            alongSeatedLeg = (alongSeatedLeg - (alongRodLeg * alongSeatedLeg.Dot(alongRodLeg))).Normalized();

            if (!alongSeatedLeg.IsFinite || alongSeatedLeg.Length <= GeometryTolerance.Length)
            {
                reason =
                    "El eje de la diagonal es paralelo a la normal de la cara del separador: no definen un " +
                    "marco de adaptador.";
                return false;
            }

            // Z = X × Y, dextrogiro, para que el prisma se extruya por el eje del corte con la sección
            // orientada como la construye el catálogo: ala corta en X, ala larga en Y.
            var alongCut = alongSeatedLeg.Cross(alongRodLeg).Normalized();

            var offset = HoleOffsetPerAxis(leg, thickness);
            var half = thickness / 2.0;

            // El agujero del separador está en el PLANO MEDIO del ala apoyada, o sea medio espesor por fuera de
            // la cara sobre la que la pieza se sienta. El troquel del separador marca la cara; el centro del
            // agujero del adaptador no está ahí.
            var separatorHole = separatorFaceHole + (alongRodLeg * half);

            // Y el de la varilla está a `offset` de él en LOS DOS ejes: hacia el otro extremo por el ala
            // apoyada, y hacia fuera del panel por el ala del tensor. De aquí sale el ΔY que ya no es cero.
            var rodHole = separatorHole + (alongSeatedLeg * -offset) + (alongRodLeg * offset);

            // El talón, del que cuelga el contorno: medio brazo por detrás del agujero del separador a lo largo
            // del ala apoyada, y medio espesor por debajo a su través.
            var heel = separatorHole + (alongSeatedLeg * -(leg / 2.0)) + (alongRodLeg * -half);

            // La mano se nombra por el extremo del PANEL en el que está la pieza, que es como se lee un plano:
            // un adaptador cuyo tensor sube hacia la derecha está en la esquina de ABAJO a la izquierda.
            var otherIsUp = dz > 0.0;
            var otherIsRight = dx > 0.0;

            var hand = otherIsUp
                ? (otherIsRight ? CantileverBraceAdapterHand.LowerLeft : CantileverBraceAdapterHand.LowerRight)
                : (otherIsRight ? CantileverBraceAdapterHand.UpperLeft : CantileverBraceAdapterHand.UpperRight);

            frame = new CantileverBraceAdapterFrame(
                alongSeatedLeg, alongRodLeg, alongCut, separatorHole, rodHole, heel, hand);

            return true;
        }

        private static string Format(double value) =>
            value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
    }
}
