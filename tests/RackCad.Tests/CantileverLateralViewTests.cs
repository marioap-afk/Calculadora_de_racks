using System.Collections.Generic;
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
    /// I-37D, corrección de columna y base — motivo 3 del rechazo: «la vista lateral omite las placas y el
    /// cartabón».
    ///
    /// La lateral es la única vista donde el subensamble base–columna se ve de perfil, así que es la que un
    /// fabricante mira para entender cómo se conecta. Una lateral que dibuja la columna y la base pero no las
    /// tres placas ni el cartabón muestra dos piezas flotando.
    ///
    /// Las pruebas cuentan CONTRA EL MODELO —cada pieza resuelta debe tener su curva, identificada por su
    /// <see cref="CantileverPieceId"/>— y no contra números escritos a mano: así siguen siendo verdad cuando
    /// cambie una sección, un espesor o el número de niveles.
    /// </summary>
    public class CantileverLateralViewTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static CantileverLineEditorComputation Build(
            CantileverStationFaceMode face = CantileverStationFaceMode.Single) =>
            new CantileverLineEditorAssembler(Catalog)
                .Build(CantileverRoundTwoCharacterizationTests.Reference(face));

        private static CantileverViewPlan Lateral(
            CantileverLineEditorComputation computation, int stationIndex = 0) =>
            CantileverViewPlanBuilder.Build(
                computation.Line, CantileverViewKind.Lateral, Factory, stationIndex);

        /// <summary>Every piece id the lateral drew, as text: the ids are scoped by station placement.</summary>
        private static HashSet<string> DrawnIds(CantileverViewPlan plan) =>
            plan.Curves.Select(c => c.PieceId.Value).ToHashSet();

        /// <summary>Whether some curve belongs to a piece whose id ENDS in this token.</summary>
        private static bool Drew(CantileverViewPlan plan, string token) =>
            plan.Curves.Any(c => c.PieceId.Value.Contains(token));

        private static CantileverStationColumnBaseAssembly ColumnBaseOf(
            CantileverLineEditorComputation computation, int stationIndex = 0) =>
            computation.Line.Stations[stationIndex].Station.ColumnBase;

        /// <summary>
        /// The SAME column–base the line's template describes, resolved on its own — which is exactly what the
        /// component editor does. Built through the template's own <c>ToColumnBaseDesign</c> so the comparison
        /// cannot drift by copying fields by hand.
        /// </summary>
        private static CantileverColumnBaseAssembly StandaloneColumnBase()
        {
            var line = CantileverRoundTwoCharacterizationTests.Reference();
            var height = ColumnBaseOf(Build()).ColumnHeight;

            return new CantileverColumnBaseResolver(
                    Catalog, Factory, CantileverCataloguePolicies.ColumnBase(Catalog))
                .Resolve(line.StationTopology.ColumnBaseTemplate.ToColumnBaseDesign(height));
        }

        /// <summary>The distinct piece NATURES a plan drew, which is what «shows the same thing» means.</summary>
        private static HashSet<CantileverViewPieceKind> DrawnKinds(CantileverViewPlan plan) =>
            plan.Curves.Select(c => c.Kind).ToHashSet();

        // ---- 1. Está todo lo que el modelo resolvió -------------------------------------------------------

        [Fact]
        public void LaLateralDibujaCadaMIEMBRODeSuEstacion()
        {
            var c = Build();
            var lateral = Lateral(c);
            var placement = c.Line.Stations[0];
            var drawn = DrawnIds(lateral);

            Assert.NotEmpty(placement.Station.Members);

            foreach (var member in placement.Station.Members)
            {
                Assert.Contains(placement.ScopedId(member.Id).Value, drawn);
            }
        }

        [Fact]
        public void LaLateralDibujaCadaPLACADeSuEstacion()
        {
            // Las tres del subensamble —inferior de columna, frontal y posterior de base— y las de montaje de
            // cada brazo. Es la afirmación que el rechazo declaró falsa.
            var c = Build();
            var lateral = Lateral(c);
            var placement = c.Line.Stations[0];
            var drawn = DrawnIds(lateral);

            Assert.NotEmpty(placement.Station.Plates);

            foreach (var plate in placement.Station.Plates)
            {
                Assert.Contains(placement.ScopedId(plate.Id).Value, drawn);
            }
        }

        [Fact]
        public void LaLateralDibujaCadaCARTABONDeSuEstacion()
        {
            var c = Build();
            var lateral = Lateral(c);
            var placement = c.Line.Stations[0];
            var drawn = DrawnIds(lateral);

            Assert.NotEmpty(placement.Station.Gussets);

            foreach (var gusset in placement.Station.Gussets)
            {
                Assert.Contains(placement.ScopedId(gusset.Id).Value, drawn);
            }
        }

        [Fact]
        public void LaLateralDibujaCadaTROQUELDeSuEstacion()
        {
            var c = Build();
            var lateral = Lateral(c);
            var placement = c.Line.Stations[0];
            var drawn = DrawnIds(lateral);

            Assert.NotEmpty(placement.Station.Punches);

            foreach (var punch in placement.Station.Punches)
            {
                Assert.Contains(placement.ScopedId(punch.Id).Value, drawn);
            }
        }

        [Fact]
        public void LasSEISPiezasDelSubensambleTienenSuCurva()
        {
            // Escrita por TOKEN y no por conteo: si alguien deja de emitir el cartabón, esta prueba nombra la
            // pieza que falta en vez de decir «esperaba 41, obtuve 40».
            var lateral = Lateral(Build());

            Assert.True(Drew(lateral, CantileverPieceTokens.Column), "Falta la columna.");
            Assert.True(Drew(lateral, CantileverPieceTokens.Base), "Falta la base.");
            Assert.True(Drew(lateral, CantileverPieceTokens.ColumnBottomPlate), "Falta la placa inferior.");
            Assert.True(Drew(lateral, CantileverPieceTokens.BaseFrontPlate), "Falta la placa frontal.");
            Assert.True(Drew(lateral, CantileverPieceTokens.BaseRearPlate), "Falta la placa posterior.");
            Assert.True(Drew(lateral, CantileverPieceTokens.Gusset), "Falta el cartabon.");
        }

        // ---- 2. Ninguna pieza se dibuja degenerada -------------------------------------------------------

        [Fact]
        public void NingunaCurvaDeLaLateralEsUnPUNTO()
        {
            // El modo de fallo silencioso de una proyección: una placa perpendicular a la cámara colapsa. Un
            // punto en el dibujo no es una placa, es basura que el lector no puede interpretar.
            var lateral = Lateral(Build());

            foreach (var curve in lateral.Curves.Where(c => !c.IsCircle))
            {
                var first = curve.Points[0];

                Assert.True(
                    curve.Points.Any(p =>
                        System.Math.Abs(p.X - first.X) > 1e-6 || System.Math.Abs(p.Y - first.Y) > 1e-6),
                    "La curva " + curve.PieceId.Value + " colapso a un punto en la lateral.");
            }
        }

        [Fact]
        public void LaColumnaSeDibujaPORENCIMADeSuPlacaInferior()
        {
            // La corrección del datum tiene que llegar al DIBUJO, no quedarse en el modelo.
            var c = Build();
            var lateral = Lateral(c);
            var columnBase = ColumnBaseOf(c);
            var thickness = columnBase.ColumnBottomPlate.Thickness;

            var column = lateral.Of(CantileverViewPieceKind.Column).Where(x => !x.IsCircle).ToList();

            Assert.NotEmpty(column);

            // En la lateral la vertical del papel es Z, así que la ordenada mínima de la columna es su arranque.
            Assert.True(
                column.SelectMany(x => x.Points).Min(p => p.Y) >= thickness - 1e-6,
                "La columna se dibuja arrancando por debajo de la cara superior de su placa.");
        }

        // ---- 3. Una cara y dos caras -------------------------------------------------------------------

        [Fact]
        public void LaLateralDeUnaEstacionDOBLEDibujaLosBrazosDeAMBOSLados()
        {
            // La lateral es LA vista donde se ve que una estación es doble: los dos lados se superponen en la
            // frontal y se separan aquí.
            var doble = Build(CantileverStationFaceMode.Double);
            var lateral = Lateral(doble);
            var placement = doble.Line.Stations[0];
            var drawn = DrawnIds(lateral);

            var positive = placement.Station.Members
                .Where(m => m.Owner.Contains(CantileverPieceTokens.StationSidePositive)).ToList();
            var negative = placement.Station.Members
                .Where(m => m.Owner.Contains(CantileverPieceTokens.StationSideNegative)).ToList();

            Assert.NotEmpty(positive);
            Assert.NotEmpty(negative);

            foreach (var member in positive.Concat(negative))
            {
                Assert.Contains(placement.ScopedId(member.Id).Value, drawn);
            }
        }

        [Fact]
        public void LaLateralDeUnaEstacionDOBLEDibujaMASQueLaDeUnaSIMPLE()
        {
            var simple = Lateral(Build(CantileverStationFaceMode.Single));
            var doble = Lateral(Build(CantileverStationFaceMode.Double));

            Assert.True(
                doble.Curves.Count > simple.Curves.Count,
                "La lateral de una estacion doble no muestra mas que la de una simple.");
        }

        [Fact]
        public void CadaEstacionTieneSuPROPIALateralYNoLaDeLaPrimera()
        {
            var c = Build();

            Assert.True(c.Line.Stations.Count > 1, "La linea de referencia necesita al menos dos estaciones.");

            var primera = Lateral(c, 0).Signature();
            var segunda = Lateral(c, 1).Signature();

            Assert.NotEqual(primera, segunda);
        }

        // ---- 4. La previa y el bloque son la MISMA proyección --------------------------------------------

        [Fact]
        public void LaLateralDelComponenteYLaDeLaLineaCoincidenPiezaAPIEZA()
        {
            // Una sola proyección con tres consumidores: la previa del editor de componente, la previa de la
            // línea y las entidades del dibujo. Si divergieran, el usuario aprobaría una imagen y recibiria
            // otra.
            var componente = CantileverViewPlanBuilder.BuildColumnBase(
                StandaloneColumnBase(), CantileverViewKind.Lateral, Factory);

            var deLaLinea = Lateral(Build());

            Assert.NotEmpty(componente.Curves);

            // Los ids llevan el prefijo de la estación que los aloja, así que lo que debe coincidir es qué
            // NATURALEZAS se dibujan: si la previa del componente muestra un cartabón que la línea no dibuja,
            // el usuario aprueba una imagen y recibe otra.
            foreach (var kind in DrawnKinds(componente))
            {
                Assert.Contains(kind, DrawnKinds(deLaLinea));
            }

            // Y la coincidencia fuerte, que sólo la lateral permite: su cámara mira a lo largo de X, que es
            // justo el eje por el que la línea separa sus estaciones, así que el desplazamiento NO mueve el
            // dibujo. La columna de la estación 0 y la de la previa son la misma curva, punto por punto.
            var enLaPrevia = componente.Of(CantileverViewPieceKind.Column)
                .SelectMany(c => c.Points).Select(p => (System.Math.Round(p.X, 9), System.Math.Round(p.Y, 9)))
                .ToHashSet();

            var enElDibujo = deLaLinea.Of(CantileverViewPieceKind.Column)
                .SelectMany(c => c.Points).Select(p => (System.Math.Round(p.X, 9), System.Math.Round(p.Y, 9)))
                .ToHashSet();

            Assert.NotEmpty(enLaPrevia);
            Assert.Equal(enLaPrevia, enElDibujo);
        }

        [Fact]
        public void LaLateralDelComponenteDibujaSusSEISPiezas()
        {
            var componente = CantileverViewPlanBuilder.BuildColumnBase(
                StandaloneColumnBase(), CantileverViewKind.Lateral, Factory);

            Assert.True(Drew(componente, CantileverPieceTokens.Column), "Falta la columna.");
            Assert.True(Drew(componente, CantileverPieceTokens.Base), "Falta la base.");
            Assert.True(Drew(componente, CantileverPieceTokens.ColumnBottomPlate), "Falta la placa inferior.");
            Assert.True(Drew(componente, CantileverPieceTokens.BaseFrontPlate), "Falta la placa frontal.");
            Assert.True(Drew(componente, CantileverPieceTokens.BaseRearPlate), "Falta la placa posterior.");
            Assert.True(Drew(componente, CantileverPieceTokens.Gusset), "Falta el cartabon.");
        }
    }
}
