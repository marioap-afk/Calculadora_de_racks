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
    /// I-37D ronda 4, decisión 14.7 — la vista **Sección del adaptador**.
    ///
    /// <para>El adaptador no se lee como un ángulo en ninguna de las tres vistas de la línea, y no es un
    /// defecto: su eje de corte corre perpendicular a la diagonal y dentro del plano del panel, que es la
    /// única orientación en la que el agujero del ala del tensor tiene por eje la propia varilla. Ninguna de
    /// las tres cámaras mira por ese eje.</para>
    ///
    /// <para>El dueño decidió que <b>ninguna vista de la línea se deforma</b> para disimularlo (14.6) y que
    /// el detalle vive en el configurador de tensor (14.7). Esto comprueba las dos cosas: que la vista nueva
    /// enseña la L de verdad, y que las tres de la línea <b>no cambiaron de orientación</b>.</para>
    /// </summary>
    public class CantileverAdapterSectionViewTests
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

        private static CantileverViewPlan Section(int adapterIndex = 0) =>
            CantileverViewPlanBuilder.BuildAdapterSection(Braces(Build()).First(), adapterIndex, Factory);

        private static IReadOnlyList<CantileverViewCurve> Outline(CantileverViewPlan plan) =>
            plan.Curves
                .Where(c => c.Kind == CantileverViewPieceKind.ColdRolledAdapter && c.IsClosed)
                .ToList();

        /// <summary>Área con signo del polígono proyectado, en valor absoluto.</summary>
        private static double Area(IReadOnlyList<Point2D> points)
        {
            var sum = 0.0;

            for (var i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                sum += (a.X * b.Y) - (b.X * a.Y);
            }

            return Math.Abs(sum) / 2.0;
        }

        // ---- 1. La L sale como L ---------------------------------------------------------------------------

        [Fact]
        public void LaSECCIONDibujaElContornoCERRADODelAngulo()
        {
            // Lo que ninguna de las tres vistas de la linea puede dar: el contorno de la seccion, cerrado.
            var outline = Outline(Section());

            Assert.Single(outline);

            // Una L a escuadra son SEIS vertices. Con filete de raiz y dos puntas redondeadas, teselada, trae
            // bastantes mas. Si esto baja a seis, alguien volvio a dibujar la L a mano.
            Assert.True(
                outline[0].Points.Count > 6,
                "La seccion salio con " + outline[0].Points.Count + " puntos: es una L a escuadra.");
        }

        [Fact]
        public void ElContornoEsELMISMOQueElDelPRISMAYNoOtraGeometria()
        {
            // LA comprobacion que la decision 14.7 pide: la vista consume la MISMA StructuralSectionGeometry
            // que el prisma. Se mide por el AREA, que es independiente de la orientacion de la camara: si
            // alguien construyera aqui una segunda L, su area no coincidiria con la del catalogo.
            var adapter = Braces(Build()).First().Adapters[0];
            var section = Factory.Get(adapter.SectionId, SectionDetailLevel.Tabulated);

            var projected = Outline(Section())[0].Points;

            Assert.Equal(section.Area, Area(projected), 3);

            // Y para un L2x2x3/16 eso ronda 0.715 in². El cuadrado lleno de 2 x 2 daria 4.0.
            Assert.InRange(Area(projected), 0.68, 0.75);
        }

        [Fact]
        public void LaCAJADeLaSeccionMideUnBrazoPorLado()
        {
            // Las DOS alas estan, y ninguna se comio a la otra: la caja del contorno proyectado mide el brazo
            // en las dos direcciones. Si la camara no mirara por el eje de corte, una de las dos se veria
            // escorzada y esto caeria.
            var adapter = Braces(Build()).First().Adapters[0];
            var points = Outline(Section())[0].Points;

            var width = points.Max(p => p.X) - points.Min(p => p.X);
            var height = points.Max(p => p.Y) - points.Min(p => p.Y);

            Assert.Equal(adapter.Leg, width, 2);
            Assert.Equal(adapter.Leg, height, 2);

            // Y NO es un rectangulo lleno: el area de la L es bastante menor que la de su caja.
            Assert.True(Area(points) < width * height * 0.75, "La seccion se dibuja llena: perdio la escuadra.");
        }

        [Fact]
        public void ElAlaApoyadaCorreALaDERECHAYLaDelTensorHaciaARRIBA()
        {
            // La camara sale del MARCO: mira por AlongCut con AlongRodLeg como vertical. De ahi sale que el
            // angulo se lea como se lee un angulo, y no girado a un sitio cualquiera.
            var adapter = Braces(Build()).First().Adapters[0];
            var frame = adapter.Frame;
            var viewpoint = SectionViewpoint.Custom(frame.AlongCut, frame.AlongRodLeg);

            var heel = viewpoint.Project(frame.Heel);
            var alongSeated = viewpoint.Project(frame.Heel + (frame.AlongSeatedLeg * adapter.Leg));
            var alongRod = viewpoint.Project(frame.Heel + (frame.AlongRodLeg * adapter.Leg));

            // El ala apoyada avanza en X y no en Y; la del tensor al reves.
            Assert.Equal(adapter.Leg, alongSeated.X - heel.X, 6);
            Assert.Equal(0.0, alongSeated.Y - heel.Y, 6);

            Assert.Equal(0.0, alongRod.X - heel.X, 6);
            Assert.Equal(adapter.Leg, alongRod.Y - heel.Y, 6);
        }

        // ---- 2. Los agujeros -------------------------------------------------------------------------------

        [Fact]
        public void SeDibujanLosDOSCentrosDeAgujero()
        {
            var holes = Section().Curves
                .Where(c => c.Kind == CantileverViewPieceKind.Punch)
                .ToList();

            Assert.Equal(2, holes.Count);
            Assert.Equal(2, holes.Select(h => h.PieceId.Value).Distinct().Count());
        }

        [Fact]
        public void LosDosAgujerosSeVenDeCANTOEnEstaProyeccion()
        {
            // Los dos ejes de agujero son perpendiculares a la direccion de vista —uno corre por el ala
            // apoyada y el otro por la del tensor— asi que ninguno se ve como circulo. Se dibujan como su
            // TRAZA: dos puntos, no un circulo. Dibujar un circulo pondria en el papel una boca que desde
            // aqui no se ve.
            var holes = Section().Curves
                .Where(c => c.Kind == CantileverViewPieceKind.Punch)
                .ToList();

            Assert.All(holes, h =>
            {
                Assert.Null(h.CircleDiameter);
                Assert.Equal(2, h.Points.Count);
            });
        }

        [Fact]
        public void LosCentrosCAENDentroDeSuPropiaALA()
        {
            // La comprobacion de que esta vista enseña el mismo modelo fisico y no otro: proyectados, los dos
            // centros estan a media ala de una coordenada y a medio espesor de la otra, cada uno en la suya.
            var adapter = Braces(Build()).First().Adapters[0];
            var frame = adapter.Frame;
            var viewpoint = SectionViewpoint.Custom(frame.AlongCut, frame.AlongRodLeg);

            var heel = viewpoint.Project(frame.Heel);
            var sep = viewpoint.Project(adapter.Origin);
            var rod = viewpoint.Project(adapter.RodHoleCentre);

            // Separador: centrado a lo largo del ala apoyada (X), en el plano medio de esa ala (Y).
            Assert.Equal(adapter.Leg / 2.0, sep.X - heel.X, 6);
            Assert.Equal(adapter.Thickness / 2.0, sep.Y - heel.Y, 6);

            // Varilla: al reves.
            Assert.Equal(adapter.Thickness / 2.0, rod.X - heel.X, 6);
            Assert.Equal(adapter.Leg / 2.0, rod.Y - heel.Y, 6);
        }

        // ---- 3. Los dos adaptadores, y lo que NO es esta vista ---------------------------------------------

        [Fact]
        public void LosDosAdaptadoresDeUnTensorSeVenDISTINTOS()
        {
            // Son espejos fisicos, asi que sus secciones no pueden salir superpuestas punto por punto.
            var a = Outline(Section(0))[0].Points;
            var b = Outline(Section(1))[0].Points;

            var firmaA = string.Join("|", a.Select(p => Math.Round(p.X, 6) + "," + Math.Round(p.Y, 6)));
            var firmaB = string.Join("|", b.Select(p => Math.Round(p.X, 6) + "," + Math.Round(p.Y, 6)));

            Assert.NotEqual(firmaA, firmaB);
        }

        [Fact]
        public void UnTensorESTRUCTURALNoTieneSeccionDeAdaptadorYLoDICE()
        {
            // No lleva adaptadores porque se atornilla directo. Devolver un plan vacio y callar dejaria al
            // lector pensando que fallo el dibujo.
            var brace = Braces(Build("AISC-L-L4X4X1_2")).First();
            var plan = CantileverViewPlanBuilder.BuildAdapterSection(brace, 0, Factory);

            Assert.Empty(plan.Curves);
            Assert.Contains(
                plan.Diagnostics,
                d => d.Code == CantileverDiagnostics.BraceHasNoAdapterSection);

            Assert.DoesNotContain(plan.Diagnostics, d => d.IsBlocking);
        }

        [Fact]
        public void UnIndiceDeAdaptadorFUERADeRangoSeRECHAZA()
        {
            var brace = Braces(Build()).First();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverViewPlanBuilder.BuildAdapterSection(brace, 5, Factory));
        }

        [Fact]
        public void LaSeccionNOTieneCamaraFIJAYPedirlaFALLAConSuMotivo()
        {
            // La guarda de la decision 14.6: esta vista no puede entrar por la puerta de las vistas de linea,
            // porque su direccion la pone cada adaptador. Falla en cerrado y con el motivo escrito.
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverViewPlanBuilder.Viewpoint(CantileverViewKind.AdapterSection));

            Assert.Contains("eje de corte", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ---- 4. Las vistas de la LINEA no se movieron ------------------------------------------------------

        [Fact]
        public void LasTRESVistasDeLaLineaCONSERVANSuOrientacion()
        {
            // Decision 14.6, comprobada donde se decide: las camaras de las tres vistas de la linea son las
            // mismas de siempre. Añadir el detalle NO las toco.
            var frontal = CantileverViewPlanBuilder.Viewpoint(CantileverViewKind.Frontal).Camera;
            var lateral = CantileverViewPlanBuilder.Viewpoint(CantileverViewKind.Lateral).Camera;
            var planta = CantileverViewPlanBuilder.Viewpoint(CantileverViewKind.Planta).Camera;

            Assert.Equal(1.0, frontal.AxisZ.Y, 9);
            Assert.Equal(1.0, frontal.AxisY.Z, 9);

            Assert.Equal(-1.0, lateral.AxisZ.X, 9);
            Assert.Equal(1.0, lateral.AxisY.Z, 9);

            Assert.Equal(-1.0, planta.AxisZ.Z, 9);
            Assert.Equal(1.0, planta.AxisY.Y, 9);
        }
    }
}
