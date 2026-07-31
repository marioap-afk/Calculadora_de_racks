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
    /// I-37D ronda 3, punto 5 — las bases de una estación DOBLE.
    ///
    /// El dueño reportó que salen «pegadas y sin troquel de un lado». Medido antes de tocar nada, el modelo
    /// resultó correcto —dos bases enteras, espejadas, con sus dieciséis troqueles cada una— y los defectos
    /// estaban en otras dos partes:
    ///
    /// <list type="number">
    /// <item>El espejo descomponía mal la reflexión: volteaba la Y local de la sección y compensaba en la X,
    /// que da un giro neto de 180°. En una W, doblemente simétrica, no cambia el contorno y por eso nadie lo
    /// veía; el patín inferior de la base espejada se dibujaba arriba.</item>
    /// <item>En la frontal las dos bases se proyectan una sobre otra —la cámara mira justo por el eje de
    /// simetría—, que es lo que una proyección ortogonal dice y no un defecto. Se intentó podar las gemelas y
    /// se retiró: la poda no distingue una gemela de dos piezas distintas que se tapan. Ver
    /// <see cref="EnLaFRONTALLasDosBasesCOINCIDENYEsoEsLoCorrecto"/>.</item>
    /// </list>
    /// </summary>
    public class CantileverDoubleStationLayoutTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static CantileverLineEditorComputation Build(
            CantileverStationFaceMode face = CantileverStationFaceMode.Double) =>
            new CantileverLineEditorAssembler(Catalog)
                .Build(CantileverRoundTwoCharacterizationTests.Reference(face));

        private static CantileverViewPlan View(CantileverLineEditorComputation c, CantileverViewKind view) =>
            CantileverViewPlanBuilder.Build(c.Line, view, Factory, 0);

        private static CantileverStationBaseSide Side(
            CantileverLineEditorComputation c, CantileverArmSide side) =>
            c.Line.Stations[0].Station.ColumnBase.Sides.Single(s => s.Side == side);

        // ---- 1. El MODELO: dos bases enteras y espejadas -------------------------------------------------

        [Fact]
        public void UnaEstacionDobleTieneDOSBasesCompletasYNoUnaCompartida()
        {
            var columnBase = Build().Line.Stations[0].Station.ColumnBase;

            Assert.Equal(2, columnBase.Sides.Count);
            Assert.Equal(2, columnBase.Sides.Select(s => s.Side).Distinct().Count());

            // Cada lado con TODAS sus piezas: base, dos placas, cartabón y sus troqueles. Un lado al que le
            // faltara algo es exactamente lo que el dueño describió como «sin troquel de un lado».
            foreach (var side in columnBase.Sides)
            {
                Assert.NotNull(side.Member);
                Assert.NotNull(side.FrontPlate);
                Assert.NotNull(side.RearPlate);
                Assert.NotNull(side.Gusset);
                Assert.NotEmpty(side.RearPlatePunches);
            }

            Assert.Equal(
                Side(Build(), CantileverArmSide.PositiveY).RearPlatePunches.Count,
                Side(Build(), CantileverArmSide.NegativeY).RearPlatePunches.Count);
        }

        [Fact]
        public void LasDosBasesCorrenEnSENTIDOSOpuestosYNoSeSolapan()
        {
            var c = Build();
            var positive = Side(c, CantileverArmSide.PositiveY);
            var negative = Side(c, CantileverArmSide.NegativeY);

            Assert.Equal(1.0, positive.Member.Direction.Y, 9);
            Assert.Equal(-1.0, negative.Member.Direction.Y, 9);

            // ARRANCAN EN LAS DOS CARAS DE LA COLUMNA, no las dos en y = 0. Corregido en la ronda 3 de
            // I-37D: hasta entonces las dos empezaban en la cara de conexion y la negativa se metia dentro
            // de la columna a lo largo de todo su canto. Esta prueba lo permitia porque comprobaba el limite
            // contra el CERO, que es justo esa cara, en vez de contra la columna.
            var pos = positive.Envelope();
            var neg = negative.Envelope();

            var column = c.Line.Stations[0].Station.ColumnBase.ColumnBottomPlate.Outline;
            var columnMinY = column.Min(q => q.Y);
            var columnMaxY = column.Max(q => q.Y);

            Assert.True(columnMinY < -1e-9, "La columna tiene que ocupar canto hacia y negativa.");

            Assert.True(pos.MinY >= columnMaxY - 1e-9, "La base positiva invade la columna.");
            Assert.True(neg.MaxY <= columnMinY + 1e-9, "La base negativa invade la columna.");

            // Y llegan igual de lejos MEDIDAS DESDE SU PROPIA CARA: una gondola doble es simetrica respecto
            // del plano medio de la columna, no respecto de una de sus caras.
            Assert.Equal(pos.MaxY - columnMaxY, columnMinY - neg.MinY, 9);
            Assert.Equal(positive.Member.GeometricLength, negative.Member.GeometricLength, 9);
        }

        [Fact]
        public void LaBaseEspejadaNOSeDibujaBOCAABAJO()
        {
            // El defecto que el espejo tenía. Se mide sobre la LATERAL, que es donde una base se ve de perfil:
            // el patín inferior de las dos tiene que estar abajo, no el de una arriba.
            var c = Build();
            var lateral = View(c, CantileverViewKind.Lateral);

            foreach (var side in new[] { "PY", "NY" })
            {
                var points = lateral.Curves
                    .Where(x => x.PieceId.Value.Contains("-" + side + "-BAS"))
                    .SelectMany(x => x.Points)
                    .ToList();

                Assert.NotEmpty(points);

                var bottom = points.Min(p => p.Y);
                var top = points.Max(p => p.Y);

                // El patín inferior ocupa la primera franja del canto. Se cuenta cuántos puntos caen en el
                // décimo inferior y en el décimo superior: en una W bien puesta son los mismos.
                var band = (top - bottom) / 10.0;
                var low = points.Count(p => p.Y <= bottom + band);
                var high = points.Count(p => p.Y >= top - band);

                Assert.True(low > 0 && high > 0, "El lado " + side + " no dibujo sus dos patines.");
                Assert.Equal(low, high);
            }
        }

        [Fact]
        public void ElEspejoConservaLaVERTICALDeLaSeccion()
        {
            // La afirmación directa sobre el marco, que es donde estaba el defecto: espejar cambia el sentido
            // del RECORRIDO y no cuál es el arriba de la sección. Antes se volteaba la Y local y se compensaba
            // en la X, que no es el mismo eje: el resultado era un giro de 180°.
            var c = Build();

            var positive = Side(c, CantileverArmSide.PositiveY).Member.Placement.Frame;
            var negative = Side(c, CantileverArmSide.NegativeY).Member.Placement.Frame;

            Assert.Equal(-positive.AxisZ.Y, negative.AxisZ.Y, 9);   // el recorrido se invierte
            Assert.Equal(positive.AxisY.Z, negative.AxisY.Z, 9);    // el arriba NO
        }

        // ---- 2. Las VISTAS ------------------------------------------------------------------------------

        [Fact]
        public void LaLATERALSeparaLasDosBases()
        {
            var lateral = View(Build(), CantileverViewKind.Lateral);

            var positive = lateral.Curves.Where(x => x.PieceId.Value.Contains("-PY-BAS")).SelectMany(x => x.Points).ToList();
            var negative = lateral.Curves.Where(x => x.PieceId.Value.Contains("-NY-BAS")).SelectMany(x => x.Points).ToList();

            Assert.NotEmpty(positive);
            Assert.NotEmpty(negative);

            // Una a cada lado de la columna, sin solaparse.
            Assert.True(
                positive.Max(p => p.X) <= negative.Min(p => p.X) + 1e-9 ||
                negative.Max(p => p.X) <= positive.Min(p => p.X) + 1e-9,
                "Las dos bases se pisan en la lateral.");
        }

        [Fact]
        public void LaPLANTATambienLasSepara()
        {
            var planta = View(Build(), CantileverViewKind.Planta);

            var positive = planta.Curves.Where(x => x.PieceId.Value.Contains("-PY-BAS")).SelectMany(x => x.Points).ToList();
            var negative = planta.Curves.Where(x => x.PieceId.Value.Contains("-NY-BAS")).SelectMany(x => x.Points).ToList();

            Assert.NotEmpty(positive);
            Assert.NotEmpty(negative);

            Assert.True(
                positive.Min(p => p.Y) >= negative.Max(p => p.Y) - 1e-9,
                "Las dos bases se pisan en la planta.");
        }

        [Fact]
        public void EnLaFRONTALLasDosBasesCOINCIDENYEsoEsLoCorrecto()
        {
            // Medido, y declarado como lo que es. La frontal mira JUSTO por el eje de simetria de una estacion
            // doble, asi que cada pieza del lado lejano se proyecta encima de su gemela del lado cercano: no
            // hay separacion que dibujar. La separacion se lee en la lateral y en la planta, y las dos pruebas
            // de arriba dicen que ahi sale correcta.
            //
            // Se intento podar las gemelas para no dejar entidades coincidentes en el bloque, y se RETIRO: una
            // huella geometrica no distingue una gemela de dos piezas DISTINTAS que se tapan -los troqueles de
            // una columna, apilados en Z, se proyectan todos en el mismo punto en planta-, asi que la poda se
            // llevaba por delante agujeros de verdad. El contrato «todo agujero resuelto se dibuja» es del
            // dueno, de la ronda 1, y pesa mas que no repetir un trazo. Trece pruebas lo dijeron.
            var frontal = View(Build(), CantileverViewKind.Frontal);

            string Shape(string token) => string.Join(";", frontal.Curves
                .Where(c => c.PieceId.Value.Contains(token))
                .SelectMany(c => c.Points)
                .Select(p => Math.Round(p.X, 6) + "," + Math.Round(p.Y, 6))
                .OrderBy(t => t, StringComparer.Ordinal));

            Assert.NotEmpty(Shape("-PY-BAS"));
            Assert.Equal(Shape("-PY-BAS"), Shape("-NY-BAS"));
            Assert.Equal(Shape("-PY-GUS"), Shape("-NY-GUS"));
            Assert.Equal(Shape("-PY-PREAR"), Shape("-NY-PREAR"));
        }

        [Fact]
        public void LaDOBLESigueDibujandoLoQueSOLOEllaTiene()
        {
            // La poda no puede llevarse por delante lo que distingue a una doble. En la LATERAL, donde nada
            // coincide, tienen que verse las piezas de los dos lados.
            var lateral = View(Build(), CantileverViewKind.Lateral);
            var ids = lateral.Curves.Select(c => c.PieceId.Value).ToList();

            foreach (var token in new[] { "-PY-BAS", "-NY-BAS", "-PY-GUS", "-NY-GUS", "-PY-PREAR", "-NY-PREAR" })
            {
                Assert.Contains(ids, id => id.Contains(token));
            }
        }
    }
}
