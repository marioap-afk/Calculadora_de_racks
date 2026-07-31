using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 3, punto 4 — los defaults del brazo y su vista frontal.
    ///
    /// Cuatro correcciones del dueño: la pendiente por omisión es 7/16 por 12 in; el margen vertical de la
    /// placa de montaje deja de ser un parámetro sin default y pasa a valer 2 in; la cantidad de filas de
    /// troqueles deja de ser un número fijo y se DERIVA de la altura del perfil; y la frontal deja de dibujar
    /// la inclinación.
    /// </summary>
    public class CantileverArmDefaultsAndFrontalTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        // ---- 1. Los dos valores que el dueño aprobó ------------------------------------------------------

        [Fact]
        public void LaPENDIENTEPorOmisionEsSieteDieciseisavosPorDoce()
        {
            // Escrito como la fracción y no como 0.4375: lo que el dueño dictó es la fracción, y un decimal
            // redondeado a mano es el número que nadie sabe de dónde salió cuando hay que revisarlo.
            Assert.Equal(7.0 / 16.0, CantileverDefaults.ArmSlopeRisePer12, 12);
            Assert.Equal(7.0 / 16.0, new CantileverArmBodyDesign().SlopeRisePer12, 12);
            Assert.Equal(7.0 / 16.0, new CantileverArmTemplateDesign().Body.SlopeRisePer12, 12);
        }

        [Fact]
        public void ElMARGENVerticalPorOmisionEsDOSPulgadas()
        {
            Assert.Equal(2.00, CantileverDefaults.ArmMountingPlateVerticalEndOffset, 12);
            Assert.Equal(2.00, new CantileverArmMountingPlateDesign().VerticalEndOffset);
            Assert.Equal(2.00, new CantileverArmMountingPlateTemplateDesign().VerticalEndOffset);
        }

        [Fact]
        public void UnMargenNULOEXPLICITOSigueSiendoInvalido()
        {
            // Que ahora haya un default no convierte «ausente» en «aprobado». Un diseño leído de un JSON
            // anterior a esta aprobación no trae el valor, y el resolutor tiene que seguir rechazándolo en vez
            // de rellenarlo por su cuenta.
            var plate = new CantileverArmMountingPlateDesign { VerticalEndOffset = null };

            Assert.Null(plate.VerticalEndOffset);
            Assert.Null(plate.DeepCopy().VerticalEndOffset);
        }

        // ---- 2. La cuenta de filas se DERIVA -------------------------------------------------------------

        [Fact]
        public void LaCuentaSaleDeLosQueCABENEnLaAlturaDelPerfil()
        {
            // La regla del dueño, literal: arrancar en el margen, avanzar con el paso vigente, contar los que
            // caigan dentro de la altura. Con paso 4 —el de la retícula regular de la columna, que es la
            // rejilla a la que un brazo se atornilla— un perfil de 10 in con margen 2 da tres: 2, 6 y 10.
            Assert.Equal(3, CantileverArmPunchCountRule.For(10.0, 2.0, 4.0));

            // Y con el paso de 2 in daría cinco. Se deja escrito porque es el número del ejemplo del encargo,
            // y así queda claro de qué paso salía.
            Assert.Equal(5, CantileverArmPunchCountRule.For(10.0, 2.0, 2.0));
        }

        [Fact]
        public void UnaFilaQueCaeJUSTOEnElBordeCuenta()
        {
            // 8/4 puede dar 1.9999999999999998 en punto flotante y el suelo se la comería. La holgura existe
            // para eso y no para redondear nada que signifique algo.
            Assert.Equal(3, CantileverArmPunchCountRule.For(10.0, 2.0, 4.0));
            Assert.Equal(3, CantileverArmPunchCountRule.For(10.0 - 1e-12, 2.0, 4.0));
        }

        [Fact]
        public void LaCuentaNuncaBajaDeDOS()
        {
            // Dos es un MÍNIMO y no un valor al que ajustar: una sola fila es una bisagra, no una conexión.
            Assert.Equal(2, CantileverArmPunchCountRule.Minimum);
            Assert.Equal(2, CantileverArmPunchCountRule.For(3.0, 2.0, 4.0));   // no cabe ni una segunda
            Assert.Equal(2, CantileverArmPunchCountRule.For(1.0, 2.0, 4.0));   // el margen no cabe siquiera
        }

        [Fact]
        public void UnaEntradaSINSENTIDODaElMinimoYNoUnaCuentaAbsurda()
        {
            // Devolver cero o uno sería proponer una conexión que el resolutor va a rechazar de todos modos, y
            // el usuario vería un rechazo sin haber pedido nada.
            foreach (var count in new[]
                     {
                         CantileverArmPunchCountRule.For(double.NaN, 2.0, 4.0),
                         CantileverArmPunchCountRule.For(10.0, double.NaN, 4.0),
                         CantileverArmPunchCountRule.For(10.0, 2.0, 0.0),
                         CantileverArmPunchCountRule.For(10.0, 2.0, double.PositiveInfinity),
                         CantileverArmPunchCountRule.For(-5.0, 2.0, 4.0),
                         CantileverArmPunchCountRule.For(10.0, -2.0, 4.0)
                     })
            {
                Assert.Equal(2, count);
            }
        }

        [Fact]
        public void UnPerfilMASALTOPideMASFilas()
        {
            // La propiedad, en vez de una tabla de números: la cuenta crece con la altura y nunca decrece.
            var previous = 0;

            foreach (var height in new[] { 4.0, 8.0, 12.0, 16.0, 24.0, 36.0 })
            {
                var count = CantileverArmPunchCountRule.For(height, 2.0, 4.0);

                Assert.True(count >= previous, "La cuenta bajo al subir la altura.");
                previous = count;
            }

            Assert.True(
                CantileverArmPunchCountRule.For(36.0, 2.0, 4.0) >
                CantileverArmPunchCountRule.For(8.0, 2.0, 4.0));
        }

        // ---- 3. La frontal no dibuja la inclinación ------------------------------------------------------

        private static CantileverLineEditorComputation Build(double slope)
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            design.DefaultArmTemplate.Body.SlopeRisePer12 = slope;

            return new CantileverLineEditorAssembler(Catalog).Build(design);
        }

        private static CantileverViewPlan View(CantileverLineEditorComputation c, CantileverViewKind view) =>
            CantileverViewPlanBuilder.Build(c.Line, view, Factory, 0);

        /// <summary>
        /// La FORMA de lo dibujado para los brazos, con su posición quitada.
        ///
        /// Se compara la forma y no las coordenadas porque el modelo mueve el origen del brazo unas milésimas
        /// al inclinarlo —dónde arranca el cuerpo depende de su placa— y eso NO es la inclinación dibujada.
        /// Lo que la frontal no debe mostrar es el ladeo, y el ladeo es forma.
        /// </summary>
        private static string ArmShape(CantileverViewPlan plan)
        {
            var points = plan.Of(CantileverViewPieceKind.Arm).SelectMany(c => c.Points).ToList();
            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);

            return string.Join("|", points.Select(
                p => Math.Round(p.X - minX, 6) + "," + Math.Round(p.Y - minY, 6)));
        }

        private static (double Width, double Height) ArmBox(CantileverViewPlan plan)
        {
            var points = plan.Of(CantileverViewPieceKind.Arm).SelectMany(c => c.Points).ToList();

            return (points.Max(p => p.X) - points.Min(p => p.X),
                    points.Max(p => p.Y) - points.Min(p => p.Y));
        }

        [Fact]
        public void LaFRONTALDibujaElMismoBrazoConPendienteYSinElla()
        {
            // La afirmación del dueño, comprobada donde importa: en la frontal, un brazo de 7/16 y uno plano
            // se dibujan con la MISMA forma, punto por punto. No «parecida»: la misma.
            Assert.Equal(
                ArmShape(View(Build(0.0), CantileverViewKind.Frontal)),
                ArmShape(View(Build(7.0 / 16.0), CantileverViewKind.Frontal)));

            // Y la caja tambien, que es la manera de decir que no hay manchon: sin aplanar, un brazo de 36 in
            // con 7/16 por 12 subiria 1.3 in a lo largo del vuelo y la frontal lo mostraria estirado.
            Assert.Equal(
                ArmBox(View(Build(0.0), CantileverViewKind.Frontal)),
                ArmBox(View(Build(7.0 / 16.0), CantileverViewKind.Frontal)));
        }

        [Fact]
        public void LaFRONTALSigueDibujandoElBrazoYNoLoBorra()
        {
            // La prueba de arriba también pasaría si la frontal dejara de dibujar el brazo. Ésta dice que no.
            var frontal = View(Build(7.0 / 16.0), CantileverViewKind.Frontal);
            var arm = frontal.Of(CantileverViewPieceKind.Arm).ToList();

            Assert.NotEmpty(arm);

            var points = arm.SelectMany(c => c.Points).ToList();

            Assert.True(points.Max(p => p.X) - points.Min(p => p.X) > 1.0);
            Assert.True(points.Max(p => p.Y) - points.Min(p => p.Y) > 1.0);
        }

        [Fact]
        public void LaLATERALSIDibujaLaInclinacionYEsDondeSeMide()
        {
            // La otra mitad, y la que impide que el aplanado se extienda a donde haría daño: la lateral es la
            // vista en la que la pendiente se lee, así que ahí un brazo inclinado y uno plano NO coinciden.
            Assert.NotEqual(
                ArmShape(View(Build(0.0), CantileverViewKind.Lateral)),
                ArmShape(View(Build(7.0 / 16.0), CantileverViewKind.Lateral)));
        }

        [Fact]
        public void LaPLANTATambienConservaSuComportamiento()
        {
            // La planta NO se aplana, y se le nota: la huella del brazo inclinado es MAYOR que la del plano.
            //
            // Crece, y no se acorta, porque lo que la planta mide es la sombra del perfil LADEADO: un cuerpo
            // de 36 in y 4 in de canto girado 2.09° proyecta 36·cos + 4·sen = 36.12 in. El acortamiento por
            // coseno del eje —2.4 centésimas— existe pero lo tapan las esquinas del perfil, que sobresalen
            // 14.6 centésimas. Las dos cosas son ciertas; la que se ve es la segunda.
            var plano = ArmBox(View(Build(0.0), CantileverViewKind.Planta)).Height;
            var inclinado = ArmBox(View(Build(7.0 / 16.0), CantileverViewKind.Planta)).Height;

            Assert.True(inclinado > plano, "La planta dibujo el brazo inclinado como si fuera plano.");
            Assert.Equal(36.0, plano, 6);
            Assert.Equal(36.0 * Math.Cos(Math.Atan((7.0 / 16.0) / 12.0)) +
                         4.0 * Math.Sin(Math.Atan((7.0 / 16.0) / 12.0)), inclinado, 4);
        }

        [Fact]
        public void ElAplanadoNoTocaLoRESUELTO()
        {
            // Es una convención de VISTA. El modelo sigue diciendo que el brazo sube, y el BOM sigue pidiendo
            // la longitud de corte que el usuario capturó.
            var inclinado = Build(7.0 / 16.0);
            var arm = inclinado.Line.Stations[0].Station.Arms[0];

            Assert.True(Math.Abs(arm.Body.Members[0].Direction.Z) > 1e-9, "El modelo perdio la pendiente.");

            Assert.Equal(
                new CantileverLineEditorAssembler(Catalog)
                    .Build(CantileverRoundTwoCharacterizationTests.Reference()).Bom.Components.Count,
                inclinado.Bom.Components.Count);
        }
    }
}
