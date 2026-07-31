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
                brace, CantileverPieceId.Create("TEST", "BRC"));

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

        /// <summary>
        /// Cuanto se separan del eje los puntos dibujados, medido perpendicularmente a el.
        ///
        /// Es la forma honesta de preguntar si tiene ancho a un cuerpo que se dibuja como un manojo de rectas:
        /// un eje da cero por definicion, y un perfil da su canto proyectado.
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

        // ---- 2. El adaptador es un PRISMA de seccion real -------------------------------------------------

        private static CantileverColdRolledAdapterPlan AnyAdapter() => Braces(Build()).First().Adapters[0];

        /// <summary>Coordenadas de un punto en los ejes del angulo, medidas DESDE EL TALON.</summary>
        private static (double A, double B, double W) InFrame(CantileverBraceAdapterFrame f, Point3D p)
        {
            var d = p - f.Heel;
            return (d.Dot(f.AlongSeatedLeg), d.Dot(f.AlongRodLeg), d.Dot(f.AlongCut));
        }

        [Fact]
        public void ElAdaptadorEsUnMIEMBROConLaSeccionEXACTADelCatalogo()
        {
            // Ya no es un contorno que el resolver de representacion dibujaba: es un miembro con su prisma, y
            // por eso lo proyecta la misma tuberia que una columna.
            var adapter = AnyAdapter();

            Assert.NotNull(adapter.Member);
            Assert.Equal("AISC-L-L2X2X3_16", adapter.Member.Placement.SectionId.Value);
            Assert.Equal(adapter.SectionId.Value, adapter.Member.Placement.SectionId.Value);
            Assert.Equal(CantileverMemberRole.ColdRolledAdapter, adapter.Member.Role);

            // Longitud de corte 2 in, la del producto, y es la del PRISMA y no un numero paralelo.
            Assert.Equal(2.0, adapter.CutLength, 9);
            Assert.Equal(adapter.CutLength, adapter.Member.Placement.Length, 9);
        }

        [Fact]
        public void LaRepresentacionYaNoDEVUELVEAdaptadores()
        {
            // La guarda de una sola implementacion de la proyeccion: si alguien volviera a dibujar la L a mano
            // aqui, apareceria un contorno que no es ni cuerpo ni cartabon.
            var kinds = Represent(Braces(Build()).First()).Contours.Select(c => c.Kind).Distinct().ToList();

            Assert.All(kinds, k => Assert.True(
                k == CantileverBracePieceKind.Body || k == CantileverBracePieceKind.Gusset,
                "La representacion volvio a emitir un contorno de tipo '" + k + "'."));
        }

        [Fact]
        public void LaSECCIONQueSeExtruyeTraeFileteDeRaizPuntasYTalonVIVO()
        {
            // Se comprueba sobre la SECCION que el prisma extruye, y no sobre una camara, y hay una razon
            // fisica: el corte del angulo corre dentro del plano del panel, asi que ninguna de las tres vistas
            // del sistema mira por su eje. La frontal ve el ala apoyada DE FRENTE —un cuadrado de 2 x 2 girado
            // con la diagonal— y la del tensor DE CANTO. La L completa se veria mirando por el eje del corte,
            // que es una camara que este sistema no tiene.
            //
            // Lo que importa es que la pieza este modelada con la seccion REAL, y eso es exactamente lo que se
            // mide aqui: si alguien volviera a poner una L a mano, este contorno perderia sus curvas.
            var adapter = AnyAdapter();
            var section = Factory.Get(adapter.SectionId, SectionDetailLevel.Tabulated);

            var flat = section.OuterContour
                .Flatten(SectionRepresentationOptions.DefaultChordTolerance)
                .ToList();

            // Una L a escuadra son SEIS vertices. Con filete de raiz y dos puntas redondeadas, teselada, trae
            // bastantes mas.
            Assert.True(flat.Count > 6, "El adaptador volvio a una L a mano de " + flat.Count + " puntos.");

            // AMBAS ALAS, con su brazo y su espesor reales.
            var bounds = section.Bounds;

            Assert.Equal(adapter.Leg, bounds.MaxX - bounds.MinX, 2);
            Assert.Equal(adapter.Leg, bounds.MaxY - bounds.MinY, 2);

            // Y no es un cuadrado lleno: el area del perfil ronda 0.715 in² contra las 4.0 de su caja.
            Assert.InRange(section.Area, 0.68, 0.75);

            // EL TALON, VIVO. Es el punto mas alejado del centroide en la esquina exterior, y ahi el contorno
            // tiene que hacer ESQUINA: dos puntos consecutivos separados por mas que la cuerda de un arco.
            var heel = section.ReferencePoints
                .Single(r => r.Kind == SectionReferencePointKind.AngleHeel)
                .Location;

            var nearest = flat
                .Select((q, i) => (Q: q, I: i))
                .OrderBy(x => ((x.Q.X - heel.X) * (x.Q.X - heel.X)) + ((x.Q.Y - heel.Y) * (x.Q.Y - heel.Y)))
                .First();

            // El talon es un VERTICE del contorno, no el centro de un arco: cae sobre un punto teselado.
            Assert.True(
                Math.Sqrt(
                    ((nearest.Q.X - heel.X) * (nearest.Q.X - heel.X)) +
                    ((nearest.Q.Y - heel.Y) * (nearest.Q.Y - heel.Y))) < 1e-9,
                "El talon dejo de ser un vertice del contorno: se redondeo.");
        }

        [Fact]
        public void LaVISTAEmiteElAdaptadorConSuPROPIANaturaleza()
        {
            // Y llega al dibujo con su rol, que es lo que le da capa y color. Pasar por AddMember sin decir el
            // rol lo dejaba con el que corresponde por defecto a su tipo de pieza, y el adaptador dejaba de
            // distinguirse de la varilla.
            var curves = View(Build(), CantileverViewKind.Frontal).Curves
                .Where(c => c.Role == CantileverVisualRole.BraceAdapter)
                .ToList();

            Assert.NotEmpty(curves);
            Assert.All(curves, c => Assert.Equal(CantileverViewPieceKind.ColdRolledAdapter, c.Kind));
        }

        [Fact]
        public void LasDosALASSiguenMidiendoSuBrazoYSuESPESOR()
        {
            var adapter = AnyAdapter();

            Assert.Equal(2.0, adapter.Leg, 9);
            Assert.Equal(3.0 / 16.0, adapter.Thickness, 9);

            var bounds = Factory.Get(adapter.SectionId, SectionDetailLevel.Tabulated).Bounds;

            // La caja de la seccion mide un brazo por lado: las DOS alas estan, y ninguna se comio a la otra.
            Assert.Equal(adapter.Leg, bounds.MaxX - bounds.MinX, 2);
            Assert.Equal(adapter.Leg, bounds.MaxY - bounds.MinY, 2);
        }

        // ---- 3. Los DOS agujeros, cada uno en SU ala ------------------------------------------------------

        [Fact]
        public void LosDosAgujerosEstanCentradosEnALASDISTINTAS()
        {
            // EL corazon de la decision del dueño. En coordenadas de talon, midiendo `a` a lo largo del ala
            // apoyada y `b` a lo largo de la del tensor:
            //
            //   separador -> (L/2, t/2)   centrado en su ala, en el plano medio de su ala
            //   varilla   -> (t/2, L/2)   lo mismo, en la OTRA
            var adapter = AnyAdapter();
            var frame = adapter.Frame;

            var sep = InFrame(frame, frame.SeparatorHoleCentre);
            var rod = InFrame(frame, frame.RodHoleCentre);

            Assert.Equal(adapter.Leg / 2.0, sep.A, 9);
            Assert.Equal(adapter.Thickness / 2.0, sep.B, 9);

            Assert.Equal(adapter.Thickness / 2.0, rod.A, 9);
            Assert.Equal(adapter.Leg / 2.0, rod.B, 9);

            // Y los dos centrados a lo largo del corte.
            Assert.Equal(0.0, sep.W, 9);
            Assert.Equal(0.0, rod.W, 9);
        }

        [Fact]
        public void LaSeparacionEntreAgujerosNOTieneDeltaYCERO()
        {
            // La regresion directa contra la aproximacion revocada: CutLength/2 a lo largo de la diagonal daba
            // exactamente DeltaY = 0, o sea una pieza plana metida en el plano del panel.
            foreach (var brace in Braces(Build()).Where(b => b.IntervalIndex == 0))
            {
                foreach (var adapter in brace.Adapters)
                {
                    var delta = adapter.Frame.HoleSeparation;

                    Assert.True(
                        Math.Abs(delta.Y) > 0.5,
                        "El adaptador volvio a tener los dos agujeros en el mismo plano Y.");

                    // Las TRES componentes existen: la diagonal aporta X y Z, y el ala del tensor aporta Y.
                    Assert.True(Math.Abs(delta.X) > 1e-6);
                    Assert.True(Math.Abs(delta.Z) > 1e-6);
                }
            }
        }

        [Fact]
        public void LaSeparacionEsLaQueLaGEOMETRIADelAnguloIMPONE()
        {
            var adapter = AnyAdapter();
            var offset = CantileverBraceAdapterFrameResolver.HoleOffsetPerAxis(
                adapter.Leg, adapter.Thickness);

            // (L - t)/2 en CADA eje: 0.90625 in para el L2x2x3/16 del producto.
            Assert.Equal(0.90625, offset, 9);

            // Y como los dos ejes son perpendiculares, el modulo es ese por raiz de dos.
            Assert.Equal(offset * Math.Sqrt(2.0), adapter.Frame.HoleSeparation.Length, 9);

            // La revocada daba 1.0 clavado. Que no vuelva.
            Assert.True(Math.Abs(adapter.Frame.HoleSeparation.Length - 1.0) > 0.01);
        }

        [Fact]
        public void ElAgujeroDelSeparadorNOEstaSobreLaCARASinoEnElPlanoMEDIO()
        {
            // El troquel marca la cara —tiene que coincidir con el del separador, son el mismo agujero fisico—
            // y el centro del agujero del adaptador esta medio espesor mas afuera.
            var adapter = AnyAdapter();
            var fromFace = adapter.Origin - adapter.SeparatorFacePunch.Centre;

            Assert.Equal(adapter.Thickness / 2.0, fromFace.Length, 9);
            Assert.Equal(adapter.Thickness / 2.0, Math.Abs(fromFace.Y), 9);
        }

        // ---- 3b. Las CUATRO orientaciones -----------------------------------------------------------------

        [Fact]
        public void LasCUATROManosSeDerivanYNoSeDeclaran()
        {
            // Exhaustivo por construccion: la mano sale del eje de la diagonal, asi que los cuatro casos
            // existen sin que nadie tenga que acordarse del cuarto.
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
                    new Vector3D(0.0, 1.0, 0.0),
                    new Vector3D(dx, 0.0, dz),
                    2.0, 3.0 / 16.0,
                    out var frame, out _));

                Assert.Equal(expected, frame.Hand);

                // El ala apoyada crece EN CONTRA del otro extremo: es lo que impide que el cuerpo del
                // adaptador invada el tramo por el que pasa la varilla.
                Assert.Equal(-Math.Sign(dx), Math.Sign(frame.AlongSeatedLeg.X));
                Assert.Equal(-Math.Sign(dz), Math.Sign(frame.AlongSeatedLeg.Z));

                // Y la del tensor sale perpendicular al panel, por la normal de la cara.
                Assert.Equal(1.0, frame.AlongRodLeg.Y, 9);
            }

            Assert.Equal(4, cases.Select(c => c.Expected).Distinct().Count());
        }

        [Fact]
        public void UnExtremoQueNoDefineDiagonalSeRECHAZA()
        {
            // Una diagonal que no avanza en X, o que no sube ni baja, no tiene extremo que orientar.
            foreach (var axis in new[] { new Vector3D(0.0, 0.0, 5.0), new Vector3D(5.0, 0.0, 0.0) })
            {
                Assert.False(CantileverBraceAdapterFrameResolver.TryResolve(
                    new Point3D(0.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0), axis,
                    2.0, 3.0 / 16.0, out _, out var reason));

                Assert.NotEmpty(reason);
            }
        }

        [Fact]
        public void UnAnguloSinESPESORValidoSeRECHAZA()
        {
            Assert.False(CantileverBraceAdapterFrameResolver.TryResolve(
                new Point3D(0.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0), new Vector3D(1.0, 0.0, 1.0),
                2.0, 2.0, out _, out var reason));

            Assert.NotEmpty(reason);
        }

        [Fact]
        public void LosCuatroExtremosDelPanelCubrenLasCUATROManos()
        {
            var manos = Braces(Build())
                .Where(b => b.IntervalIndex == 0)
                .SelectMany(b => b.Adapters)
                .Select(a => a.Hand)
                .ToList();

            Assert.Equal(4, manos.Count);
            Assert.Equal(4, manos.Distinct().Count());
        }

        [Fact]
        public void ElAdaptadorNOSaleIGUALEnLosCuatroExtremos()
        {
            // No girado arbitrariamente, comprobado sobre el marco fisico: si los cuatro salieran iguales, sus
            // ternas de ejes coincidirian.
            var ternas = new HashSet<string>(StringComparer.Ordinal);

            foreach (var adapter in Braces(Build()).Where(b => b.IntervalIndex == 0).SelectMany(b => b.Adapters))
            {
                var f = adapter.Frame;

                ternas.Add(string.Join("|", new[]
                {
                    f.AlongSeatedLeg.X, f.AlongSeatedLeg.Y, f.AlongSeatedLeg.Z,
                    f.AlongRodLeg.X, f.AlongRodLeg.Y, f.AlongRodLeg.Z,
                    f.AlongCut.X, f.AlongCut.Y, f.AlongCut.Z
                }.Select(v => Math.Round(v, 6).ToString(
                    "0.######", System.Globalization.CultureInfo.InvariantCulture))));
            }

            Assert.Equal(4, ternas.Count);
        }

        [Fact]
        public void LosDosAdaptadoresDeUnaDiagonalSonESPEJOS()
        {
            // El ala apoyada de cada extremo crece en contra de SU vano, asi que las dos miran a lados
            // contrarios. La del tensor, en cambio, es la misma en los dos: apoyan en la misma cara.
            foreach (var brace in Braces(Build()).Where(b => b.IntervalIndex == 0))
            {
                var lower = brace.Adapters[0].Frame;
                var upper = brace.Adapters[1].Frame;

                Assert.Equal(-1.0, lower.AlongSeatedLeg.Dot(upper.AlongSeatedLeg), 9);
                Assert.Equal(+1.0, lower.AlongRodLeg.Dot(upper.AlongRodLeg), 9);
            }
        }

        [Fact]
        public void ElTENSORNoATRAVIESAElAdaptador()
        {
            // La varilla va de agujero a agujero. El ala apoyada crece en contra del vano, asi que desde el
            // agujero de la varilla hacia el otro extremo no queda cuerpo del adaptador: solo el medio espesor
            // del ala que la varilla cruza.
            foreach (var brace in Braces(Build()).Where(b => b.IntervalIndex == 0))
            {
                var axis = (brace.UpperEnd - brace.LowerEnd).Normalized();

                foreach (var (adapter, towards) in new[]
                {
                    (brace.Adapters[0], axis),
                    (brace.Adapters[1], axis * -1.0)
                })
                {
                    var reach = (adapter.Frame.Heel - adapter.Frame.RodHoleCentre).Dot(towards);

                    Assert.True(
                        reach <= (adapter.Thickness / 2.0) + 1e-9,
                        "El cuerpo del adaptador invade " + reach.ToString("0.####") +
                        " in del vano por el que pasa la varilla.");
                }
            }
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
            // Uno en cada extremo del corte de 2 in, asi que se separan a lo LARGO del adaptador.
            //
            // CAMBIO DE LA RONDA 4 DE I-37D: ese eje ya no es Y. El corte del angulo corre PERPENDICULAR a la
            // diagonal DENTRO del plano del panel, porque es la unica orientacion en la que el agujero del ala
            // del tensor tiene por eje la propia varilla —que es como una varilla roscada se sujeta—. Antes se
            // media la separacion en Y y ahora en el eje del corte; en la frontal, por tanto, los dos
            // cartabones ya NO caen uno sobre otro, se ven aparte.
            var brace = Braces(Build()).First();
            var adapter = brace.Adapters[0];
            var frame = adapter.Frame;

            var gussets = Represent(brace).Contours
                .Where(c => c.Kind == CantileverBracePieceKind.Gusset)
                .Take(2)
                .ToList();

            var along = gussets
                .Select(g => (g.Outline[0] - frame.Heel).Dot(frame.AlongCut))
                .OrderBy(v => v)
                .ToList();

            Assert.Equal(adapter.CutLength, along[1] - along[0], 9);

            // Y centrados en el talon, que es donde el prisma tiene su plano de agujeros.
            Assert.Equal(0.0, (along[0] + along[1]) / 2.0, 9);

            // El eje del corte esta EN el plano del panel: no tiene componente fuera de el.
            Assert.Equal(0.0, frame.AlongCut.Y, 9);
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
            Assert.Equal(4, suelto.Curves.Count(x => x.Role == CantileverVisualRole.BraceGusset));
            Assert.Equal(2, suelto.Curves.Count(x => x.Role == CantileverVisualRole.BracePunch));

            // Los adaptadores YA NO son dos contornos: desde la ronda 4 de I-37D cada uno es un prisma que
            // proyecta la tuberia comun, y un prisma visto de lado emite las aristas de su silueta, no un
            // contorno unico. Lo que se comprueba es que los DOS estan —cada uno con su identidad— y no
            // cuantas curvas hace falta para dibujarlos.
            var adaptadores = suelto.Curves
                .Where(x => x.Role == CantileverVisualRole.BraceAdapter)
                .Select(x => x.PieceId.Value)
                .Distinct()
                .ToList();

            Assert.Equal(2, adaptadores.Count);
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
