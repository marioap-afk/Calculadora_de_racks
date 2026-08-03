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
    /// I-37D ronda 2, motivo 5 del rechazo: «faltan troqueles y placas visibles en la representación de
    /// columnas».
    ///
    /// El modelo los tenía resueltos desde el primer día —204 en la línea de referencia— y la representación
    /// nunca los pedía, así que una columna llegaba al dibujo como un contorno pelado donde el producto tiene
    /// una columna troquelada. Estas pruebas fijan que <b>ningún agujero resuelto se pierde por el camino</b>,
    /// que es una afirmación más fuerte que «hay algunos círculos»: cuentan contra el modelo, no contra un
    /// número escrito a mano.
    /// </summary>
    public class CantileverPunchRepresentationTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverLineEditorAssembler Assembler() => new CantileverLineEditorAssembler(Catalog);

        private static CantileverLineEditorComputation Build(
            CantileverStationFaceMode face = CantileverStationFaceMode.Single) =>
            Assembler().Build(CantileverRoundTwoCharacterizationTests.Reference(face));

        private static CantileverViewPlan View(
            CantileverLineEditorComputation computation, CantileverViewKind view) =>
            computation.Views.Single(v => v.View == view);

        /// <summary>
        /// Todo agujero que la LINEA resolvio: los de las estaciones, los de las placas de separador, los de
        /// los separadores y el de VARILLA de cada adaptador de tensor.
        ///
        /// El de la varilla se sumo en la ronda 3, cuando el tensor cold rolled paso a dibujarse con su ancho
        /// fisico: hasta entonces el adaptador se dibujaba como un cuadrado sin agujeros y ese hueco no
        /// llegaba al plano.
        ///
        /// El OTRO agujero del adaptador —el de la cara del separador— NO se suma, y no es un descuido: es el
        /// mismo agujero fisico que el troquel de tensor del separador, cuyo datum el modelo obliga a que
        /// coincida. Contarlo dos veces pediria dos circulos identicos superpuestos.
        /// </summary>
        private static int ResolvedHolesOfTheWholeLine(CantileverLineAssembly line) =>
            line.Stations.Sum(s => s.Station.Punches.Count) +
            line.SeparatorColumnPlates.Count +
            line.Separators.Sum(s => s.Punches.Count) +
            line.Intervals.SelectMany(i => i.Braces).Sum(b => b.Adapters.Count(a => a.RodHoleDiameter > 0.0));

        /// <summary>Every diameter the line resolved, from the same three sources.</summary>
        private static System.Collections.Generic.HashSet<double> ResolvedDiameters(CantileverLineAssembly line) =>
            line.Stations.SelectMany(s => s.Station.Punches)
                .Concat(line.SeparatorColumnPlates.Select(p => p.Punch))
                .Concat(line.Separators.SelectMany(s => s.Punches))
                .Select(p => p.Diameter)
                .Concat(line.Intervals.SelectMany(i => i.Braces).SelectMany(b => b.Adapters)
                    .Where(a => a.RodHoleDiameter > 0.0)
                    .Select(a => a.RodHoleDiameter))
                .ToHashSet();

        // ---- 1. Nada se pierde --------------------------------------------------------------------------

        [Fact]
        public void LaFrontalDibujaTODOSLosAgujerosResueltosDeLaLinea()
        {
            var c = Build();
            var frontal = View(c, CantileverViewKind.Frontal);

            Assert.Equal(ResolvedHolesOfTheWholeLine(c.Line), frontal.Of(CantileverViewPieceKind.Punch).Count());
            Assert.True(frontal.Of(CantileverViewPieceKind.Punch).Any(), "La frontal no dibuja un solo troquel.");
        }

        [Fact]
        public void LaPlantaTambienLosDibuja()
        {
            // La planta nace SIN brazos ni tensores desde la ronda 3 de I-37D, asi que le faltan los agujeros
            // de esas dos familias y NO los de nadie mas. Se comprueban las dos cosas: cuantos faltan y que
            // encendiendolos vuelven exactamente todos.
            var c = Build();
            var planta = View(c, CantileverViewKind.Planta);

            var deBrazo = c.Line.Stations
                .SelectMany(s => s.Station.Punches)
                .Count(x => x.Surface == CantileverPunchSurface.ArmMountingPlate);

            var deTensor = c.Line.Intervals
                .SelectMany(i => i.Braces)
                .SelectMany(b => b.Adapters)
                .Count();

            Assert.True(deBrazo > 0 && deTensor > 0, "El caso de referencia tiene que traer de las dos.");

            Assert.Equal(
                ResolvedHolesOfTheWholeLine(c.Line) - deBrazo - deTensor,
                planta.Of(CantileverViewPieceKind.Punch).Count());

            var completa = CantileverViewPlanBuilder.Build(
                c.Line, CantileverViewKind.Planta, new StructuralSectionGeometryFactory(Catalog), 0,
                CantileverPlantaVisibilityDesign.ShowingEverything);

            Assert.Equal(
                ResolvedHolesOfTheWholeLine(c.Line),
                completa.Of(CantileverViewPieceKind.Punch).Count());
        }

        [Fact]
        public void LaLateralDibujaLosDeSuEstacionYNingunoDelIntervalo()
        {
            // La lateral es de UNA estación y no lleva arriostramiento, así que tampoco sus agujeros.
            var c = Build();
            var lateral = View(c, CantileverViewKind.Lateral);

            Assert.Equal(
                c.Line.Stations[0].Station.Punches.Count,
                lateral.Of(CantileverViewPieceKind.Punch).Count());
        }

        [Fact]
        public void LaGondolaDobleDibujaLosDeSusDosCaras()
        {
            var single = Build();
            var doble = Build(CantileverStationFaceMode.Double);

            var enSencilla = View(single, CantileverViewKind.Frontal).Of(CantileverViewPieceKind.Punch).Count();
            var enDoble = View(doble, CantileverViewKind.Frontal).Of(CantileverViewPieceKind.Punch).Count();

            Assert.Equal(ResolvedHolesOfTheWholeLine(doble.Line), enDoble);
            Assert.True(enDoble > enSencilla, "La doble debe dibujar más agujeros que la sencilla.");
        }

        // ---- 2. Las piezas que el dueño enumeró ---------------------------------------------------------

        [Fact]
        public void LosTroquelesRegularesDeColumnaEstanTodos()
        {
            var c = Build();
            var frontal = View(c, CantileverViewKind.Frontal);
            var dibujados = frontal.Of(CantileverViewPieceKind.Punch).Select(p => p.PieceId.Value).ToHashSet();

            foreach (var placement in c.Line.Stations)
            {
                foreach (var punch in placement.Station.ColumnBase.ColumnRegularPunches)
                {
                    Assert.Contains(placement.ScopedId(punch.Id).Value, dibujados);
                }
            }
        }

        [Fact]
        public void LosTroquelesDeLaPlacaInferiorYDeLaConexionEstanTodos()
        {
            var c = Build();
            var dibujados = View(c, CantileverViewKind.Frontal)
                .Of(CantileverViewPieceKind.Punch).Select(p => p.PieceId.Value).ToHashSet();

            foreach (var placement in c.Line.Stations)
            {
                var columnBase = placement.Station.ColumnBase;

                Assert.NotEmpty(columnBase.ColumnBottomPlatePunches);
                Assert.NotEmpty(columnBase.ColumnConnectionPunches);

                foreach (var punch in columnBase.ColumnBottomPlatePunches.Concat(columnBase.ColumnConnectionPunches))
                {
                    Assert.Contains(placement.ScopedId(punch.Id).Value, dibujados);
                }
            }
        }

        [Fact]
        public void LasPlacasDeSeparadorSeDibujanConSuAgujeroCentral()
        {
            var c = Build();
            var frontal = View(c, CantileverViewKind.Frontal);
            var placas = frontal.Of(CantileverViewPieceKind.Plate).Select(p => p.PieceId.Value).ToHashSet();
            var agujeros = frontal.Of(CantileverViewPieceKind.Punch).Select(p => p.PieceId.Value).ToHashSet();

            Assert.NotEmpty(c.Line.SeparatorColumnPlates);

            foreach (var plate in c.Line.SeparatorColumnPlates)
            {
                Assert.Contains(plate.Plate.Id.Value, placas);
                Assert.Contains(plate.Punch.Id.Value, agujeros);
            }
        }

        [Fact]
        public void UnaEstacionInteriorLlevaDosPlacasPorElevacionYUnaExtremaUna()
        {
            // Tres estaciones: la 0 y la 2 son extremas, la 1 interior. Es la comprobación que el punto E7 del
            // paquete manual pide, aquí en código.
            var c = Build();

            var porEstacion = c.Line.SeparatorColumnPlates
                .GroupBy(p => p.StationIndex)
                .ToDictionary(g => g.Key, g => g.Count());

            var elevaciones = c.Line.Intervals[0].Separators.Count;

            Assert.Equal(elevaciones, porEstacion[0]);
            Assert.Equal(elevaciones * 2, porEstacion[1]);
            Assert.Equal(elevaciones, porEstacion[2]);
        }

        [Fact]
        public void LasPlacasInferioresDeColumnaYLasDeLaBaseSiguenDibujandose()
        {
            var c = Build();
            var frontal = View(c, CantileverViewKind.Frontal);
            var placas = frontal.Of(CantileverViewPieceKind.Plate).Select(p => p.PieceId.Value).ToHashSet();

            foreach (var placement in c.Line.Stations)
            {
                foreach (var plate in placement.Station.Plates)
                {
                    Assert.Contains(placement.ScopedId(plate.Id).Value, placas);
                }

                Assert.NotEmpty(placement.Station.Gussets);
            }
        }

        // ---- 3. Cómo se dibuja un agujero ---------------------------------------------------------------

        [Fact]
        public void UnAgujeroVistoDeFrenteEsUnCirculoDeSuDiametroReal()
        {
            var c = Build();
            var circulos = View(c, CantileverViewKind.Frontal)
                .Of(CantileverViewPieceKind.Punch)
                .Where(p => p.IsCircle)
                .ToList();

            Assert.NotEmpty(circulos);
            Assert.All(circulos, p => Assert.Single(p.Points));
            Assert.All(circulos, p => Assert.True(p.CircleDiameter > 0.0));

            // El diámetro dibujado es el del troquel resuelto, no uno de conveniencia. Son DOS diámetros
            // distintos y los dos tienen que aparecer tal cual: 3/4 en la columna y 9/16 en las placas de
            // separador y en el separador.
            var diametros = ResolvedDiameters(c.Line);

            Assert.All(circulos, p => Assert.Contains(p.CircleDiameter.Value, diametros));
            Assert.True(diametros.Count >= 2, "La línea de referencia tiene más de un diámetro de troquel.");
        }

        [Fact]
        public void UnAgujeroDeCantoSeDibujaComoSuTrazaYNoDesaparece()
        {
            // En la frontal, los troqueles perforados a lo largo de Y se ven de frente; los perforados a lo
            // largo de Z se ven de canto. Los dos tienen que estar.
            var c = Build();
            var punches = View(c, CantileverViewKind.Frontal).Of(CantileverViewPieceKind.Punch).ToList();

            var trazas = punches.Where(p => !p.IsCircle).ToList();

            Assert.NotEmpty(trazas);
            Assert.All(trazas, p => Assert.Equal(2, p.Points.Count));
            Assert.All(trazas, p => Assert.False(p.IsClosed));
        }

        [Fact]
        public void LaTrazaMideElDiametroDelAgujero()
        {
            var c = Build();
            var trazas = View(c, CantileverViewKind.Frontal)
                .Of(CantileverViewPieceKind.Punch)
                .Where(p => !p.IsCircle)
                .ToList();

            var diametros = ResolvedDiameters(c.Line);

            foreach (var traza in trazas)
            {
                var dx = traza.Points[1].X - traza.Points[0].X;
                var dy = traza.Points[1].Y - traza.Points[0].Y;
                var largo = System.Math.Sqrt(dx * dx + dy * dy);

                Assert.Contains(diametros, d => System.Math.Abs(d - largo) < 1e-6);
            }
        }

        [Fact]
        public void NingunAgujeroSeOmitePorSerPequeno()
        {
            // El más pequeño de la línea sigue estando. «Es muy chico para verse» no es una razón para no
            // dibujarlo: es una razón para que el preview lo agrande, que es cosa del preview.
            var c = Build();
            var frontal = View(c, CantileverViewKind.Frontal);

            var minimo = c.Line.Stations.SelectMany(s => s.Station.Punches).Min(p => p.Diameter);
            var dibujados = frontal.Of(CantileverViewPieceKind.Punch).Count();

            Assert.True(minimo > 0.0);
            Assert.Equal(ResolvedHolesOfTheWholeLine(c.Line), dibujados);
        }

        // ---- 4. Determinismo ------------------------------------------------------------------------------

        [Fact]
        public void LaFirmaSigueSiendoDeterministaConLosAgujerosDentro()
        {
            var first = View(Build(), CantileverViewKind.Frontal).Signature();
            var second = View(Build(), CantileverViewKind.Frontal).Signature();

            Assert.Equal(first, second);
            Assert.Contains("Punch", first);
            Assert.Contains(":d=", first); // la firma distingue un círculo de una polilínea
        }
    }
}
