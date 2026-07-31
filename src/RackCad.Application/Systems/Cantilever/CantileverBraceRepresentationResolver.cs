using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>Qué es un contorno de la representación de un tensor, para quien deba darle capa y color.</summary>
    public enum CantileverBracePieceKind
    {
        /// <summary>El cuerpo del tensor: la banda de la varilla.</summary>
        Body = 0,

        /// <summary>El ángulo de extremo, con sus dos alas.</summary>
        Adapter = 1,

        /// <summary>Uno de los dos cartabones calibre 10 del adaptador.</summary>
        Gusset = 2
    }

    /// <summary>Un contorno cerrado de la representación, en coordenadas de mundo.</summary>
    public sealed class CantileverBraceContour
    {
        internal CantileverBraceContour(
            CantileverBracePieceKind kind, CantileverPieceId id, IReadOnlyList<Point3D> outline)
        {
            Kind = kind;
            Id = id;
            Outline = outline;
        }

        public CantileverBracePieceKind Kind { get; }

        public CantileverPieceId Id { get; }

        public IReadOnlyList<Point3D> Outline { get; }
    }

    /// <summary>
    /// Lo que un tensor DIBUJA: contornos físicos, sin AutoCAD y sin WPF.
    ///
    /// Es un plan neutral —puntos y una naturaleza por contorno— igual que el resto de esta capa. No sabe qué
    /// es una polilínea ni un pincel; la previa y el materializador lo consumen los dos, que es lo que impide
    /// que la imagen aprobada y el bloque entregado se separen.
    /// </summary>
    public sealed class CantileverBraceRepresentation
    {
        internal CantileverBraceRepresentation(
            IReadOnlyList<CantileverBraceContour> contours, double visibleWidth, IReadOnlyList<string> notes)
        {
            Contours = contours;
            VisibleWidth = visibleWidth;
            Notes = notes;
        }

        public IReadOnlyList<CantileverBraceContour> Contours { get; }

        /// <summary>Ancho visible del cuerpo, en pulgadas. Para una varilla es su DIÁMETRO.</summary>
        public double VisibleWidth { get; }

        /// <summary>Lo que no se pudo representar, dicho en vez de dibujado mal.</summary>
        public IReadOnlyList<string> Notes { get; }

        public bool IsEmpty => Contours.Count == 0;

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "w={0:0.####};n={1};{2}",
            VisibleWidth, Contours.Count,
            string.Join("|", Contours.Select(c => c.Kind + ":" + c.Id.Value + ":" + c.Outline.Count)));
    }

    /// <summary>
    /// THE authority que convierte un tensor resuelto en la geometría que se ve.
    ///
    /// <para><b>El eje sigue siendo el datum, pero deja de ser el dibujo.</b> Hasta la ronda 3 una varilla
    /// cold rolled se dibujaba como su eje —una recta de dos puntos— por la convención de ADR-0027 D7: sin
    /// fila de catálogo que respaldara una sección, dibujar una habría puesto en el plano una forma que nadie
    /// había aprobado. El dueño revisó esa decisión
    /// (<c>OWNER_REVISED_CANTILEVER_BRACE_VISUAL_REPRESENTATION</c>): el eje se conserva como datum geométrico
    /// —de él salen la longitud, los extremos y la conexión con los agujeros del separador— y la geometría
    /// VISIBLE pasa a tener ancho físico. No es una sección inventada: es el diámetro que el diseño ya
    /// declaraba, puesto a ambos lados del eje que ya existía.</para>
    ///
    /// <para><b>Es representación, no fabricación.</b> Ni preparación de bordes, ni destijeres, ni la
    /// soldadura del talón del ángulo, ni roscas. Lo que hay es la silueta que un lector necesita para
    /// entender la pieza, y está dicho aquí para que nadie la lea como un plano de taller.</para>
    /// </summary>
    public static class CantileverBraceRepresentationResolver
    {
        /// <summary>
        /// Ancho visible de una varilla: su DIÁMETRO, sin más.
        ///
        /// Ni un factor de dibujo ni un mínimo de trazo. El dueño pidió que se viera con su ancho físico, y
        /// cualquier otro número sería una escala que alguien mediría en el plano creyendo que es la pieza.
        /// </summary>
        public static double VisibleWidthOf(CantileverBracePlan brace) =>
            brace != null && brace.Kind == CantileverBraceBodyKind.ColdRolledRound
                ? brace.RoundDiameter
                : double.NaN;

        /// <summary>
        /// La representación de un tensor COLD ROLLED con sus dos adaptadores.
        ///
        /// Un tensor estructural no pasa por aquí: su cuerpo es una sección de catálogo y lo proyecta la
        /// tubería de secciones, que ya dibuja el contorno real —canal con sus dos alas y su alma, ángulo con
        /// las suyas— respetando marco, espejo y rotación. Duplicar esa tubería aquí sería una segunda
        /// implementación de la misma proyección, y la primera vez que una cambiara el canal saldría de una
        /// forma en la línea y de otra en el componente suelto.
        /// </summary>
        /// <param name="bodyId">
        /// El id del cuerpo. Se RECIBE en vez de componerse aqui: los tokens de propiedad son de la linea, y
        /// una guarda de arquitectura reserva ese vocabulario para sus archivos. Esta autoridad hace
        /// geometria, no nomenclatura.
        /// </param>
        public static CantileverBraceRepresentation Resolve(
            CantileverBracePlan brace, CantileverPieceId bodyId)
        {
            if (brace == null)
            {
                throw new ArgumentNullException(nameof(brace));
            }

            var contours = new List<CantileverBraceContour>();
            var notes = new List<string>();

            if (brace.Kind != CantileverBraceBodyKind.ColdRolledRound)
            {
                notes.Add(
                    "Tensor estructural: su cuerpo lo proyecta la tuberia de secciones, que dibuja el contorno " +
                    "real del catalogo. Esta autoridad no lo duplica.");

                return new CantileverBraceRepresentation(contours, double.NaN, notes);
            }

            var width = brace.RoundDiameter;

            if (!(width > 0.0))
            {
                notes.Add("La varilla no tiene diametro positivo, asi que no hay ancho visible que dibujar.");
                return new CantileverBraceRepresentation(contours, double.NaN, notes);
            }

            var body = BandBetween(brace.LowerEnd, brace.UpperEnd, width);

            if (body == null)
            {
                notes.Add("Los dos extremos del tensor coinciden: no hay eje sobre el que centrar una banda.");
            }
            else
            {
                contours.Add(new CantileverBraceContour(CantileverBracePieceKind.Body, bodyId, body));
            }

            foreach (var adapter in brace.Adapters)
            {
                AddAdapter(contours, notes, adapter);
            }

            return new CantileverBraceRepresentation(contours, width, notes);
        }

        /// <summary>
        /// La banda del cuerpo: dos bordes paralelos al eje y un cierre PERPENDICULAR en cada extremo.
        ///
        /// El cierre es perpendicular al eje y el ancho constante de punta a punta, que es lo que una varilla
        /// redonda enseña de perfil: no se afila hacia el adaptador ni se ensancha en él. La longitud es la que
        /// separa los dos datums de agujero de varilla, así que el dibujo mide lo mismo que el modelo.
        ///
        /// Devuelve null cuando el eje no tiene longitud: una banda alrededor de un punto no es una banda.
        /// </summary>
        private static IReadOnlyList<Point3D> BandBetween(Point3D from, Point3D to, double width)
        {
            var axis = to - from;
            var length = axis.Length;

            if (length <= GeometryTolerance.Length)
            {
                return null;
            }

            // El arriostramiento vive en el plano X–Z, así que la perpendicular en ese plano es girar el eje
            // un cuarto de vuelta. Tomarla en 3D pediría un vector de referencia que nadie ha declarado.
            var ux = axis.X / length;
            var uz = axis.Z / length;
            var half = width / 2.0;

            var px = -uz * half;
            var pz = ux * half;

            return new[]
            {
                new Point3D(from.X + px, from.Y, from.Z + pz),
                new Point3D(to.X + px, to.Y, to.Z + pz),
                new Point3D(to.X - px, to.Y, to.Z - pz),
                new Point3D(from.X - px, from.Y, from.Z - pz)
            };
        }

        private static void AddAdapter(
            ICollection<CantileverBraceContour> contours,
            ICollection<string> notes,
            CantileverColdRolledAdapterPlan adapter)
        {
            if (!CantileverBraceAdapterFrameResolver.TryResolve(
                    adapter.Origin, adapter.RodHoleCentre,
                    out var alongSeparator, out var towardsRod, out _, out var reason))
            {
                notes.Add(reason);
                return;
            }

            contours.Add(new CantileverBraceContour(
                CantileverBracePieceKind.Adapter,
                adapter.Id,
                AngleOutline(adapter, alongSeparator, towardsRod)));

            foreach (var gusset in GussetOutlines(adapter, alongSeparator, towardsRod))
            {
                contours.Add(new CantileverBraceContour(
                    CantileverBracePieceKind.Gusset,
                    adapter.Id.At(contours.Count(c => c.Kind == CantileverBracePieceKind.Gusset) + 1),
                    gusset));
            }
        }

        /// <summary>
        /// El contorno en L del ángulo: SEIS puntos, con su talón y sus dos alas.
        ///
        /// Un ángulo de 2 × 2 visto de frente NO es un cuadrado de 2 × 2, que es como se dibujaba: es una L de
        /// 2 in de brazo y 3/16 in de espesor, y la diferencia entre las dos es justamente la que permite ver
        /// dónde está el talón y, con él, hacia dónde mira la pieza. El talón se apoya en el cruce del ala del
        /// separador con la del tensor, y las dos alas salen de ahí en los sentidos que la orientación dice.
        /// </summary>
        private static IReadOnlyList<Point3D> AngleOutline(
            CantileverColdRolledAdapterPlan adapter, Vector3D alongSeparator, Vector3D towardsRod)
        {
            var heel = adapter.Origin;
            var leg = adapter.Leg;
            var t = adapter.Thickness;

            Point3D At(double a, double b) => new Point3D(
                heel.X + (alongSeparator.X * a) + (towardsRod.X * b),
                heel.Y + (alongSeparator.Y * a) + (towardsRod.Y * b),
                heel.Z + (alongSeparator.Z * a) + (towardsRod.Z * b));

            return new[]
            {
                At(0.0, 0.0),
                At(leg, 0.0),
                At(leg, t),
                At(t, t),
                At(t, leg),
                At(0.0, leg)
            };
        }

        /// <summary>
        /// Los DOS cartabones calibre 10, uno en cada extremo longitudinal del adaptador.
        ///
        /// Triángulos de verdad —tres vértices, no un rectángulo pequeño— que cierran el ángulo entre sus dos
        /// alas, que es lo que un cartabón hace. Van en los dos extremos del corte de 2 in, así que se separan
        /// a lo LARGO del adaptador: en la frontal caen uno sobre otro, porque esa cámara mira justo por ese
        /// eje, y se ven aparte en planta y en lateral. Es la misma coincidencia que tienen las dos bases de
        /// una estación doble, y por la misma razón.
        ///
        /// Forman parte del componente tensor y no son piezas sueltas: su cantidad y su calibre viajan en el
        /// adaptador, y el BOM los cuenta ahí.
        /// </summary>
        private static IEnumerable<IReadOnlyList<Point3D>> GussetOutlines(
            CantileverColdRolledAdapterPlan adapter, Vector3D alongSeparator, Vector3D towardsRod)
        {
            var half = adapter.CutLength / 2.0;
            var leg = adapter.Leg;

            foreach (var offset in new[] { -half, half })
            {
                var heel = new Point3D(adapter.Origin.X, adapter.Origin.Y + offset, adapter.Origin.Z);

                Point3D At(double a, double b) => new Point3D(
                    heel.X + (alongSeparator.X * a) + (towardsRod.X * b),
                    heel.Y + (alongSeparator.Y * a) + (towardsRod.Y * b),
                    heel.Z + (alongSeparator.Z * a) + (towardsRod.Z * b));

                yield return new[] { At(0.0, 0.0), At(leg, 0.0), At(0.0, leg) };
            }
        }
    }
}
