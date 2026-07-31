using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 3, punto 7 — la representación física de tensores y adaptadores.
    ///
    /// Hasta esta ronda una varilla cold rolled se dibujaba como su EJE —una recta de dos puntos— y su
    /// adaptador como un CUADRADO de 2 × 2. Las dos cosas eran convenciones declaradas, no descuidos: sin fila
    /// de catálogo que respaldara una sección, dibujar una habría puesto en el plano una forma que nadie
    /// aprobó. El dueño revisó esa decisión —<c>OWNER_REVISED_CANTILEVER_BRACE_VISUAL_REPRESENTATION</c>—: el
    /// eje sigue siendo el <b>datum</b> geométrico y la geometría <b>visible</b> pasa a tener ancho físico.
    ///
    /// Nada de esto es fabricación. No hay preparación de bordes, ni destijeres, ni la soldadura del talón del
    /// ángulo, ni roscas: es la silueta que un lector necesita para entender la pieza.
    /// </summary>
    public class CantileverBraceRepresentationTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static CantileverLineEditorComputation Build(string braceSectionId = null)
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference();

            if (braceSectionId != null)
            {
                design.Bracing.BraceKind = CantileverBraceBodyKind.StructuralSection;
                design.Bracing.BraceSectionId = braceSectionId;
            }

            return new CantileverLineEditorAssembler(Catalog).Build(design);
        }

        private static IReadOnlyList<CantileverBracePlan> Braces(CantileverLineEditorComputation c) =>
            c.Line.Intervals.SelectMany(i => i.Braces).ToList();

        private static CantileverViewPlan View(CantileverLineEditorComputation c, CantileverViewKind view) =>
            CantileverViewPlanBuilder.Build(c.Line, view, Factory);

        private static CantileverBraceRepresentation Represent(CantileverBracePlan brace) =>
            CantileverBraceRepresentationResolver.Resolve(
                brace, CantileverPieceId.Create("TEST", "BRC"), Factory);

        // ---- 1. El cuerpo cold rolled tiene ANCHO -------------------------------------------------------

        [Fact]
        public void ElCuerpoEsUnaBANDADelAnchoDelDIAMETRO()
        {
            var brace = Braces(Build()).First();
            var representation = Represent(brace);

            Assert.Equal(brace.RoundDiameter, representation.VisibleWidth, 12);
            Assert.Equal(0.75, representation.VisibleWidth, 12);

            var body = representation.Contours.Single(c => c.Kind == CantileverBracePieceKind.Body);

            // Cuatro esquinas: dos bordes paralelos al eje y un cierre en cada extremo.
            Assert.Equal(4, body.Outline.Count);
        }

        [Fact]
        public void LosDosBordesSonPARALELOSAlEjeYDistanUnDIAMETRO()
        {
            var brace = Braces(Build()).First();
            var body = Represent(brace).Contours.Single(c => c.Kind == CantileverBracePieceKind.Body);

            var axis = brace.UpperEnd - brace.LowerEnd;
            var length = axis.Length;

            // Los lados 0→1 y 3→2 son los bordes largos: los dos paralelos al eje.
            foreach (var (a, b) in new[] { (0, 1), (3, 2) })
            {
                var edge = body.Outline[b] - body.Outline[a];

                Assert.Equal(length, edge.Length, 9);

                var cross = (edge.X * axis.Z) - (edge.Z * axis.X);

                Assert.Equal(0.0, cross, 6);
            }

            // Y el cierre de cada extremo mide justo un diametro, perpendicular al eje.
            foreach (var (a, b) in new[] { (0, 3), (1, 2) })
            {
                var closure = body.Outline[b] - body.Outline[a];

                Assert.Equal(brace.RoundDiameter, closure.Length, 9);
                Assert.Equal(0.0, (closure.X * axis.X) + (closure.Z * axis.Z), 6);
            }
        }

        [Fact]
        public void ElEJESigueSiendoElDATUMYLaBandaEstaCENTRADASobreEl()
        {
            // Lo que la revisión conserva: la longitud se sigue midiendo entre los dos datums de agujero de
            // varilla, y la banda no desplaza el eje, lo envuelve.
            var brace = Braces(Build()).First();
            var body = Represent(brace).Contours.Single(c => c.Kind == CantileverBracePieceKind.Body);

            var centreLow = new Point3D(
                (body.Outline[0].X + body.Outline[3].X) / 2.0,
                (body.Outline[0].Y + body.Outline[3].Y) / 2.0,
                (body.Outline[0].Z + body.Outline[3].Z) / 2.0);

            var centreHigh = new Point3D(
                (body.Outline[1].X + body.Outline[2].X) / 2.0,
                (body.Outline[1].Y + body.Outline[2].Y) / 2.0,
                (body.Outline[1].Z + body.Outline[2].Z) / 2.0);

            Assert.Equal(brace.LowerEnd.X, centreLow.X, 9);
            Assert.Equal(brace.LowerEnd.Z, centreLow.Z, 9);
            Assert.Equal(brace.UpperEnd.X, centreHigh.X, 9);
            Assert.Equal(brace.UpperEnd.Z, centreHigh.Z, 9);

            Assert.Equal(brace.BodyLength, (centreHigh - centreLow).Length, 9);
        }

        [Fact]
        public void EnLaFRONTALNingunTensorEsUnaLINEASIMPLE()
        {
            var frontal = View(Build(), CantileverViewKind.Frontal);
            var bodies = frontal.Of(CantileverViewPieceKind.Brace).ToList();

            Assert.NotEmpty(bodies);

            foreach (var body in bodies)
            {
                Assert.True(body.IsClosed, "Un tensor volvio a dibujarse como una polilinea abierta.");
                Assert.Equal(4, body.Points.Count);

                // Y con ancho de verdad: el menor de los dos lados de su caja no puede ser cero.
                var width = body.Points.Max(p => p.X) - body.Points.Min(p => p.X);
                var height = body.Points.Max(p => p.Y) - body.Points.Min(p => p.Y);

                Assert.True(Math.Min(width, height) > 0.5, "El tensor sigue sin ancho visible.");
            }
        }

        // ---- 2. El adaptador es una L de verdad ----------------------------------------------------------

        [Fact]
        public void ElAdaptadorUsaElCONTORNODELCATALOGOYNoUnaLAMano()
        {
            // Eran SEIS puntos hasta la ronda 4 de I-37D, porque el contorno se construía aquí con el brazo y
            // el espesor: una L a escuadra, sin filete de raíz ni radios de punta. Ahora sale de la tubería de
            // secciones, así que trae los arcos teselados del perfil REAL — bastantes más de seis.
            var adapters = Represent(Braces(Build()).First()).Contours
                .Where(c => c.Kind == CantileverBracePieceKind.Adapter)
                .ToList();

            Assert.Equal(2, adapters.Count);
            Assert.All(adapters, a => Assert.True(
                a.Outline.Count > 6,
                "El adaptador volvio a dibujarse con una L a mano de " + a.Outline.Count + " puntos."));

            // Y es el contorno de la sección que el plan declara, no otro: su área coincide con la del
            // catálogo. Es la comprobación de que no hay una segunda geometría paralela.
            var section = Factory.Get(
                Braces(Build()).First().Adapters[0].SectionId, SectionDetailLevel.Tabulated);

            foreach (var adapter in adapters)
            {
                Assert.Equal(section.Area, Area(adapter.Outline), 3);
            }
        }

        [Fact]
        public void LaLCONSERVASusDOSAlasYSuESPESOR()
        {
            var brace = Braces(Build()).First();
            var adapter = brace.Adapters[0];
            var outline = Represent(brace).Contours
                .First(c => c.Kind == CantileverBracePieceKind.Adapter).Outline;

            // El plan sigue declarando sus cotas nominales: 2 in de brazo y 3/16 in de espesor.
            Assert.Equal(2.0, adapter.Leg, 9);
            Assert.Equal(3.0 / 16.0, adapter.Thickness, 9);

            // La CAJA del contorno mide un brazo por lado. Se comprueba sobre la caja y no sobre vértices
            // concretos porque el contorno ya no tiene seis puntos nombrables: viene teselado del catálogo.
            var width = outline.Max(p => p.X) - outline.Min(p => p.X);
            var height = outline.Max(p => p.Z) - outline.Min(p => p.Z);

            // Las alas de catálogo miden 2 in exactas; la tolerancia cubre la tesela de las puntas.
            Assert.Equal(adapter.Leg, Math.Max(width, height), 2);

            // Y NO es un rectangulo: el area de la L es bastante menor que la del cuadrado que la envuelve.
            Assert.True(
                Area(outline) < width * height * 0.75,
                "El adaptador se dibuja lleno: perdio la escuadra.");

            // Su area es la del perfil, que para un L2x2x3/16 ronda 0.72 in². Un cuadrado de 2 x 2 daria 4.
            Assert.InRange(Area(outline), 0.5, 1.0);
        }

        /// <summary>
        /// Cuánto se separan del eje los puntos dibujados, medido perpendicularmente a él.
        ///
        /// Es la forma honesta de preguntar «¿tiene ancho?» a un cuerpo que se dibuja como un manojo de
        /// rectas: un eje da cero por definición, y un perfil da su canto proyectado.
        /// </summary>
        private static double SpreadAcrossAxis(IReadOnlyList<CantileverViewCurve> bodies)
        {
            var points = bodies.SelectMany(b => b.Points).ToList();
            var from = points.First();
            var to = points.OrderByDescending(
                p => ((p.X - from.X) * (p.X - from.X)) + ((p.Y - from.Y) * (p.Y - from.Y))).First();

            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var length = Math.Sqrt((dx * dx) + (dy * dy));

            if (length <= 1e-9)
            {
                return 0.0;
            }

            return points.Max(p => Math.Abs((((p.X - from.X) * dy) - ((p.Y - from.Y) * dx)) / length));
        }

        /// <summary>Área con signo del polígono en el plano X–Z, en valor absoluto.</summary>
        private static double Area(IReadOnlyList<Point3D> outline)
        {
            var sum = 0.0;

            for (var i = 0; i < outline.Count; i++)
            {
                var a = outline[i];
                var b = outline[(i + 1) % outline.Count];
                sum += (a.X * b.Z) - (b.X * a.Z);
            }

            return Math.Abs(sum) / 2.0;
        }

        // ---- 3. Las CUATRO orientaciones -----------------------------------------------------------------

        [Fact]
        public void LasCUATROManosSeDerivanYNoSeDeclaran()
        {
            // Exhaustivo por construcción: la mano sale de hacia dónde queda el agujero de la varilla respecto
            // del del separador, así que los cuatro casos existen sin que nadie tenga que acordarse del cuarto.
            var cases = new (double Dx, double Dz, CantileverBraceAdapterHand Expected)[]
            {
                (+1.0, +1.0, CantileverBraceAdapterHand.LowerLeft),
                (-1.0, +1.0, CantileverBraceAdapterHand.LowerRight),
                (+1.0, -1.0, CantileverBraceAdapterHand.UpperLeft),
                (-1.0, -1.0, CantileverBraceAdapterHand.UpperRight)
            };

            foreach (var (dx, dz, expected) in cases)
            {
                Assert.True(CantileverBraceAdapterFrameResolver.TryResolve(
                    new Point3D(0.0, 0.0, 0.0),
                    new Point3D(dx, 0.0, dz),
                    out var along, out var towards, out var hand, out _));

                Assert.Equal(expected, hand);
                Assert.Equal(Math.Sign(dx), Math.Sign(along.X));
                Assert.Equal(Math.Sign(dz), Math.Sign(towards.Z));
            }

            Assert.Equal(4, cases.Select(c => c.Expected).Distinct().Count());
        }

        [Fact]
        public void UnExtremoQueNoDefineDiagonalSeRECHAZA()
        {
            // Una diagonal que no avanza en X, o que no sube ni baja, no tiene extremo que orientar. Se dice
            // en vez de inventarle un sentido.
            foreach (var rod in new[] { new Point3D(0.0, 0.0, 5.0), new Point3D(5.0, 0.0, 0.0) })
            {
                Assert.False(CantileverBraceAdapterFrameResolver.TryResolve(
                    new Point3D(0.0, 0.0, 0.0), rod, out _, out _, out _, out var reason));

                Assert.NotEmpty(reason);
            }
        }

        [Fact]
        public void LosDosExtremosDeUnaDiagonalTienenManosOPUESTAS()
        {
            // Sobre la línea de verdad, no sobre puntos de laboratorio: los adaptadores de una misma diagonal
            // miran a lados contrarios, y los de las dos diagonales del panel cubren las cuatro esquinas.
            var manos = new List<CantileverBraceAdapterHand>();

            foreach (var brace in Braces(Build()).Where(b => b.IntervalIndex == 0))
            {
                foreach (var adapter in brace.Adapters)
                {
                    Assert.True(CantileverBraceAdapterFrameResolver.TryResolve(
                        adapter.Origin, adapter.RodHoleCentre, out _, out _, out var hand, out _));

                    manos.Add(hand);
                }
            }

            Assert.Equal(4, manos.Count);
            Assert.Equal(4, manos.Distinct().Count());
        }

        [Fact]
        public void ElAdaptadorNOSaleIGUALEnLosCuatroExtremos()
        {
            // La comprobación directa de «no girado arbitrariamente»: si los cuatro salieran iguales, sus
            // contornos trasladados al origen coincidirían. Se redondea a tres decimales porque el contorno
            // viene teselado y dos orientaciones distintas pueden coincidir en la sexta cifra.
            var formas = new HashSet<string>(StringComparer.Ordinal);

            foreach (var brace in Braces(Build()).Where(b => b.IntervalIndex == 0))
            {
                foreach (var contour in Represent(brace).Contours
                             .Where(c => c.Kind == CantileverBracePieceKind.Adapter))
                {
                    var o = contour.Outline[0];

                    formas.Add(string.Join("|", contour.Outline.Select(
                        p => Math.Round(p.X - o.X, 3) + "," + Math.Round(p.Z - o.Z, 3))));
                }
            }

            Assert.Equal(4, formas.Count);
        }

        // ---- 4. Los cartabones ---------------------------------------------------------------------------

        [Fact]
        public void CadaAdaptadorMuestraSusDOSCartabonesComoTRIANGULOS()
        {
            var brace = Braces(Build()).First();
            var gussets = Represent(brace).Contours
                .Where(c => c.Kind == CantileverBracePieceKind.Gusset)
                .ToList();

            Assert.Equal(2 * brace.Adapters.Count, gussets.Count);
            Assert.All(gussets, g => Assert.Equal(3, g.Outline.Count));   // triángulos, no rectángulos

            // Y el modelo sigue diciendo lo mismo que el dibujo: dos por adaptador, calibre 10.
            Assert.All(brace.Adapters, a =>
            {
                Assert.Equal(2, a.GussetCount);
                Assert.Equal(10, a.GussetGaugeNumber);
            });
        }

        [Fact]
        public void LosDosCartabonesVanEnLosDosEXTREMOSDelAdaptador()
        {
            // Uno en cada extremo del corte de 2 in, así que se separan a lo LARGO del adaptador. En la
            // frontal caen uno sobre otro porque esa cámara mira justo por ese eje; se ven aparte en planta.
            var brace = Braces(Build()).First();
            var adapter = brace.Adapters[0];

            var gussets = Represent(brace).Contours
                .Where(c => c.Kind == CantileverBracePieceKind.Gusset)
                .Take(2)
                .ToList();

            var ys = gussets.Select(g => g.Outline[0].Y).OrderBy(y => y).ToList();

            Assert.Equal(adapter.CutLength, ys[1] - ys[0], 9);
            Assert.Equal(adapter.Origin.Y, (ys[0] + ys[1]) / 2.0, 9);
        }

        // ---- 5. Los agujeros ------------------------------------------------------------------------------

        [Fact]
        public void ElAgujeroDeLaVARILLASeDibujaYEsUnCIRCULO()
        {
            var frontal = View(Build(), CantileverViewKind.Frontal);
            var brace = Braces(Build()).First();
            var adapter = brace.Adapters[0];

            var hole = frontal.Curves.Single(c => c.PieceId.Value == adapter.Id.At(0).Value);

            Assert.True(hole.IsCircle);
            Assert.Equal(adapter.RodHoleDiameter, hole.CircleDiameter.Value, 9);
            Assert.Equal(9.0 / 16.0, adapter.RodHoleDiameter, 9);
        }

        [Fact]
        public void ElAgujeroDeLaCARADelSeparadorNoSeDibujaDOSVeces()
        {
            // Es el MISMO agujero fisico que el troquel de tensor del separador —el modelo obliga a que sus
            // datums coincidan— asi que lo dibuja el separador y el adaptador no lo repite.
            var brace = Braces(Build()).First();
            var adapter = brace.Adapters[0];

            Assert.NotNull(adapter.SeparatorFacePunch);

            var frontal = View(Build(), CantileverViewKind.Frontal);

            Assert.DoesNotContain(
                frontal.Curves, c => c.PieceId.Value == adapter.SeparatorFacePunch.Id.Value);
        }

        // ---- 6. Los tensores ESTRUCTURALES no pasan por aquí ----------------------------------------------

        [Fact]
        public void UnTensorDeCANALDibujaSuCONTORNOYNoSoloSuEje()
        {
            var frontal = View(Build("AISC-C-C4X4_5"), CantileverViewKind.Frontal);
            var bodies = frontal.Of(CantileverViewPieceKind.Brace).ToList();

            Assert.NotEmpty(bodies);

            // Por CURVA y no por suma: sumar entre los cuatro tensores de la linea dejaba pasar cuatro ejes
            // de dos puntos, que suman ocho. Lo destapo el ejercicio de regresiones.
            // Lo que distingue un PERFIL de un EJE no es cuantos vertices tiene una curva: un prisma
            // diagonal visto de lado se dibuja como sus dos caras extremas mas las generatrices que las unen,
            // y todas ellas son rectas de dos puntos. Lo que un eje no tiene es ANCHO.
            //
            // Se mide la separacion perpendicular al eje: un eje da cero y un canal da su canto.
            // Se mide sobre UN tensor y no sobre los cuatro de la linea: cuatro ejes paralelos tambien dan
            // varias curvas y mucha dispersion, y con eso pasaba una regresion que no debia. Lo destapo el
            // ejercicio de regresiones.
            var uno = bodies.GroupBy(b => b.PieceId.Value).First().ToList();

            Assert.True(uno.Count > 1, "El canal se dibujo con una sola curva: eso es un eje.");
            Assert.True(SpreadAcrossAxis(uno) > 1.0, "El canal se dibujo sin ancho: eso es un eje.");

            // Y con extension en las dos direcciones del papel.
            var points = bodies.SelectMany(b => b.Points).ToList();

            Assert.True(points.Max(p => p.X) - points.Min(p => p.X) > 1.0);
            Assert.True(points.Max(p => p.Y) - points.Min(p => p.Y) > 1.0);
        }

        [Fact]
        public void UnTensorDeANGULOConservaSuPerfilLYNoSeVuelveUnRECTANGULO()
        {
            var c = Build("AISC-L-L2X2X3_16");
            var frontal = View(c, CantileverViewKind.Frontal);

            Assert.NotEmpty(Braces(c));

            var bodies = frontal.Of(CantileverViewPieceKind.Brace).ToList();

            Assert.NotEmpty(bodies);

            // Por CURVA, por la misma razon que en el canal: una suma dejaba pasar cuatro rectangulos.
            // Igual que el canal: lo que se pide es ancho, no vertices. Y ademas MAS curvas de las que un
            // rectangulo necesita, porque un perfil L tiene mas aristas longitudinales que una caja.
            var uno = bodies.GroupBy(b => b.PieceId.Value).First().ToList();

            Assert.True(uno.Count > 4, "El angulo se dibujo con las curvas de una caja: es un rectangulo.");
            Assert.True(SpreadAcrossAxis(uno) > 0.5, "El angulo se dibujo sin ancho.");
        }

        [Fact]
        public void LaAutoridadNoDUPLICALaTuberiaDeSecciones()
        {
            // Un tensor estructural no produce contornos aqui: los produce la tuberia de secciones, que ya
            // respeta marco, espejo y rotacion. Dos implementaciones de la misma proyeccion se separarian.
            var brace = Braces(Build("AISC-C-C4X4_5")).First();
            var representation = Represent(brace);

            Assert.True(representation.IsEmpty);
            Assert.NotEmpty(representation.Notes);
        }

        // ---- 7. La X del panel ---------------------------------------------------------------------------

        [Fact]
        public void LasDosDiagonalesFormanUnaXSINUnionCentral()
        {
            var c = Build();
            var frontal = View(c, CantileverViewKind.Frontal);

            var panel = Braces(c).Where(b => b.IntervalIndex == 0 && b.PanelIndex == 0).ToList();

            Assert.Equal(2, panel.Count);
            Assert.Equal(2, panel.Select(b => b.Diagonal).Distinct().Count());

            // Se cruzan: sus cajas se solapan. Y no hay pieza alguna en el cruce.
            var a = panel[0];
            var b = panel[1];

            var crossX = (a.LowerEnd.X + a.UpperEnd.X) / 2.0;
            var crossZ = (a.LowerEnd.Z + a.UpperEnd.Z) / 2.0;

            Assert.True(Math.Abs(((b.LowerEnd.X + b.UpperEnd.X) / 2.0) - crossX) < 1.0);
            Assert.True(Math.Abs(((b.LowerEnd.Z + b.UpperEnd.Z) / 2.0) - crossZ) < 1.0);

            // Ningun adaptador ni cartabon vive en el cruce: los cuatro estan en los extremos.
            foreach (var adapter in panel.SelectMany(x => x.Adapters))
            {
                Assert.True(
                    Math.Abs(adapter.Origin.X - crossX) > 1.0 || Math.Abs(adapter.Origin.Z - crossZ) > 1.0,
                    "Hay un adaptador en el cruce de la X: eso es una union central.");
            }

            // Y en el dibujo tampoco: ninguna pieza fabricada -adaptador ni cartabon- tiene su centro cerca
            // del cruce. Se mide por CENTRO y no exigiendo que todos sus puntos caigan dentro de un radio: una
            // union central podria ser mas grande que el radio y colarse por el hueco.
            foreach (var piece in frontal.Curves.Where(
                         x => x.Kind == CantileverViewPieceKind.ColdRolledAdapter))
            {
                var cx = piece.Points.Average(p => p.X);
                var cy = piece.Points.Average(p => p.Y);

                Assert.True(
                    Math.Abs(cx - crossX) > 2.0 || Math.Abs(cy - crossZ) > 2.0,
                    "Hay una pieza dibujada en el cruce de la X: eso es una union central.");
            }
        }

        // ---- 8. El componente suelto dibuja LO MISMO que la linea ----------------------------------------

        [Fact]
        public void ElTENSORSUELTODibujaElMISMOPlanQueDentroDeLaLinea()
        {
            // El configurador de tensor inserta lo que su previa muestra, y su previa sale del MISMO
            // constructor que la linea. Un componente con una version simplificada propia es exactamente como
            // se separan la imagen que el usuario aprueba y el bloque que recibe.
            var c = Build();
            var brace = Braces(c).First();

            var suelto = CantileverViewPlanBuilder.BuildBrace(brace, CantileverViewKind.Frontal, Factory);

            Assert.NotEmpty(suelto.Curves);

            // Mismas naturalezas, mismos conteos de puntos y misma condicion de cerrado que en la linea.
            static string Shape(IEnumerable<CantileverViewCurve> curves) => string.Join("|", curves
                .Select(x => x.Kind + ":" + x.Role + ":" + x.Points.Count + ":" + (x.IsClosed ? "C" : "O"))
                .OrderBy(t => t, StringComparer.Ordinal));

            // Se comparan POR ID y no por un filtro de texto: los dos planes salen del mismo constructor,
            // asi que nombran sus piezas igual, y cualquier otro criterio se cuela piezas de la otra diagonal.
            var ids = suelto.Curves.Select(x => x.PieceId.Value).ToHashSet(StringComparer.Ordinal);

            var enLaLinea = View(c, CantileverViewKind.Frontal).Curves
                .Where(x => ids.Contains(x.PieceId.Value));

            Assert.Equal(Shape(enLaLinea), Shape(suelto.Curves));
        }

        [Fact]
        public void ElTENSORSUELTOTraeSusAdaptadoresYCartabones()
        {
            // No una version reducida: el ángulo, sus dos alas, sus cartabones y su agujero de varilla.
            var suelto = CantileverViewPlanBuilder.BuildBrace(
                Braces(Build()).First(), CantileverViewKind.Frontal, Factory);

            Assert.Contains(suelto.Curves, x => x.Role == CantileverVisualRole.Brace && x.IsClosed);
            Assert.Equal(2, suelto.Curves.Count(x => x.Role == CantileverVisualRole.BraceAdapter));
            Assert.Equal(4, suelto.Curves.Count(x => x.Role == CantileverVisualRole.BraceGusset));
            Assert.Equal(2, suelto.Curves.Count(x => x.Role == CantileverVisualRole.BracePunch));
        }

        // ---- 9. Los roles visuales -----------------------------------------------------------------------

        [Fact]
        public void CADAPiezaDelArriostramientoLlevaSuNATURALEZA()
        {
            var frontal = View(Build(), CantileverViewKind.Frontal);

            foreach (var role in new[]
                     {
                         CantileverVisualRole.Brace,
                         CantileverVisualRole.BraceAdapter,
                         CantileverVisualRole.BraceGusset,
                         CantileverVisualRole.BracePunch
                     })
            {
                Assert.Contains(frontal.Curves, x => x.Role == role);

                // Cada una con su capa: se pueden apagar por separado.
                Assert.StartsWith(
                    CantileverVisualRoles.LayerPrefix, CantileverVisualRoles.LayerNameOf(role));
            }

            // EL CONJUNTO ENTERO EN CIAN. El dueno reviso en la ronda 3 el reparto de la ronda anterior
            // -cuerpo azul, adaptador cian, cartabon magenta-: un tensor es UNA pieza compuesta y sus tres
            // partes se leen juntas. Lo que se conserva es la separacion por CAPA, que es lo que permite
            // apagar solo los cartabones; lo que se retira es el contraste dentro del conjunto.
            Assert.Equal(4, CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.Brace));
            Assert.Equal(4, CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.BraceAdapter));
            Assert.Equal(4, CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.BraceGusset));

            // Y las tres capas siguen siendo distintas: mismo color no es misma capa.
            Assert.Equal(3, new[]
            {
                CantileverVisualRole.Brace,
                CantileverVisualRole.BraceAdapter,
                CantileverVisualRole.BraceGusset
            }.Select(CantileverVisualRoles.LayerNameOf).Distinct(StringComparer.Ordinal).Count());

            // Y los agujeros en blanco, como el resto de los agujeros del sistema.
            Assert.Equal(7, CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.BracePunch));
        }

        // ---- 8. Nada de esto toca el producto -------------------------------------------------------------

        [Fact]
        public void LaMejoraVISUALNoCambiaElMODELO()
        {
            // Longitud nominal, diámetro, cantidad de adaptadores y de cartabones: la representación los LEE y
            // no los toca. Es lo que separa un cambio de dibujo de un cambio de producto.
            var brace = Braces(Build()).First();
            var before = brace.Signature();

            Represent(brace);

            Assert.Equal(before, brace.Signature());
            Assert.Equal(2, brace.Adapters.Count);
            Assert.Equal(0.75, brace.RoundDiameter, 12);
            Assert.All(brace.Adapters, a => Assert.Equal("L2x2x0.1875@2;gus=2xCAL_10", a.Signature()));
        }
    }
}
