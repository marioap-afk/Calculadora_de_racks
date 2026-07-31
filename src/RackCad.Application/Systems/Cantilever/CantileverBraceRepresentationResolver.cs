using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
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
            CantileverBracePlan brace,
            CantileverPieceId bodyId,
            StructuralSectionGeometryFactory geometryFactory)
        {
            // OBLIGATORIA, no opcional. Con un valor por omision un llamador que no la pasara se quedaba sin
            // adaptadores y sin enterarse: el dibujo salia incompleto y en silencio, que es justo el modo de
            // fallo que esta ronda viene a quitar.
            if (geometryFactory == null)
            {
                throw new ArgumentNullException(nameof(geometryFactory));
            }

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
                AddAdapter(contours, notes, adapter, geometryFactory);
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
            CantileverColdRolledAdapterPlan adapter,
            StructuralSectionGeometryFactory geometryFactory)
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
                AngleOutline(adapter, alongSeparator, towardsRod, geometryFactory, notes)));

            foreach (var gusset in GussetOutlines(adapter, alongSeparator, towardsRod))
            {
                contours.Add(new CantileverBraceContour(
                    CantileverBracePieceKind.Gusset,
                    adapter.Id.At(contours.Count(c => c.Kind == CantileverBracePieceKind.Gusset) + 1),
                    gusset));
            }
        }

        /// <summary>
        /// El contorno del ángulo, tomado de la TUBERÍA DE SECCIONES y ya no construido a mano.
        ///
        /// <para>Hasta la ronda 4 de I-37D esto devolvía seis puntos calculados aquí con el brazo y el espesor
        /// del plan. Salía una L a escuadra: sin filete de raíz, sin radios de punta y sin las cotas que el
        /// catálogo publica. El dueño la rechazó por eso, y la corrección no es dibujar mejor la L sino
        /// DEJAR DE DIBUJARLA: el contorno lo da <see cref="StructuralSectionGeometryFactory"/>, que es la
        /// misma autoridad que dibuja columnas, brazos y separadores.</para>
        ///
        /// <para><b>El anclaje también cambió.</b> Antes el talón se ponía en <c>adapter.Origin</c>, que es el
        /// centro del agujero que bolta al separador: el agujero quedaba EN la esquina de la pieza, que es un
        /// sitio donde nadie taladra. Ahora ese agujero queda CENTRADO en su ala —medio brazo a lo largo y
        /// medio espesor a través— y el talón se deduce de ahí.</para>
        ///
        /// <para>Sin fábrica de geometría se devuelve <c>null</c> y se deja una nota, en vez de volver a la L a
        /// mano: dos contornos para la misma pieza es lo que esta corrección viene a quitar.</para>
        /// </summary>
        private static IReadOnlyList<Point3D> AngleOutline(
            CantileverColdRolledAdapterPlan adapter,
            Vector3D alongSeparator,
            Vector3D towardsRod,
            StructuralSectionGeometryFactory geometryFactory,
            ICollection<string> notes)
        {
            StructuralSectionGeometry geometry;

            try
            {
                geometry = geometryFactory.Get(adapter.SectionId, SectionDetailLevel.Tabulated);
            }
            catch (System.Exception ex)
            {
                notes.Add(
                    "El adaptador '" + adapter.Id.Value + "' no se dibuja: la seccion '" +
                    adapter.SectionId.Value + "' no dio geometria (" + ex.Message + ").");

                return null;
            }

            // El TALÓN, en coordenadas de la sección. La tubería centra el contorno en el centroide tabulado y
            // deja el talón anotado como punto de referencia, así que no hay que recalcularlo.
            var heelPoint = geometry.ReferencePoints
                .FirstOrDefault(r => r.Kind == SectionReferencePointKind.AngleHeel);

            var heelU = heelPoint.Kind == SectionReferencePointKind.AngleHeel ? heelPoint.Location.X : 0.0;
            var heelV = heelPoint.Kind == SectionReferencePointKind.AngleHeel ? heelPoint.Location.Y : 0.0;

            // Dónde va el talón en el mundo: el agujero del separador queda centrado en su ala, así que el
            // talón está medio brazo por detrás a lo largo del ala y medio espesor por debajo a su través.
            var half = adapter.Leg / 2.0;
            var halfThickness = adapter.Thickness / 2.0;

            var origin = adapter.Origin;

            Point3D At(double u, double v)
            {
                // (u, v) son coordenadas de SECCIÓN. Se pasan a coordenadas de talón restando el talón, y de
                // ahí al mundo con los dos ejes que la orientación física ya resolvió.
                var a = (u - heelU) - half;
                var b = (v - heelV) - halfThickness;

                return new Point3D(
                    origin.X + (alongSeparator.X * a) + (towardsRod.X * b),
                    origin.Y + (alongSeparator.Y * a) + (towardsRod.Y * b),
                    origin.Z + (alongSeparator.Z * a) + (towardsRod.Z * b));
            }

            // El contorno se recorre por sus VÉRTICES muestreados: los arcos del filete y de las puntas ya
            // vienen teselados por la tubería con su propia tolerancia de cuerda, que es la misma que usa
            // cualquier otro perfil del sistema.
            return geometry.OuterContour
                .Flatten(SectionRepresentationOptions.DefaultChordTolerance)
                .Select(q => At(q.X, q.Y))
                .ToList();
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
