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
    /// I-37D, corrección de columna y base — motivo 4: «la columna arranca desde el piso y no desde la cara
    /// superior de su placa inferior».
    ///
    /// La decisión del dueño es explícita y esta suite la fija entera: <b>la base se queda en el piso</b> y sólo
    /// la columna sube. Base y columna comparten el datum LÓGICO de conexión —las mismas elevaciones absolutas—
    /// pero no el mismo origen físico en Z, que es justo lo que ADR-0024 no distinguía.
    /// </summary>
    public class CantileverVerticalDatumTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const double Tolerance = 1e-9;

        private static StructuralSectionId Id(string v) => StructuralSectionId.Parse(v);

        private static CantileverColumnBaseSectionPolicy Policy() =>
            CantileverColumnBaseSectionPolicy.Create(
                new[]
                {
                    new CantileverColumnBaseVariant(
                        CantileverColumnBaseVariantKind.WFlangeConnected, Id(ColumnW), Id(BaseW))
                },
                new[] { StructuralSectionFamily.W });

        private static CantileverColumnBaseDesign Design(double thickness = 0.25, double cut = 96.0)
        {
            var design = new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign { SectionId = ColumnW, Height = cut },
                Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
            };

            design.Column.BottomPlate.Thickness = thickness;
            return design;
        }

        private static CantileverColumnBaseAssembly Resolve(double thickness = 0.25, double cut = 96.0) =>
            new CantileverColumnBaseResolver(Catalog, Factory, Policy()).Resolve(Design(thickness, cut));

        /// <summary>
        /// The lowest fibre of a member in world Z.
        ///
        /// A member's frame origin sits at its CENTROID, so its own <c>Start.Z</c> is not where it touches the
        /// ground — reading it as such is the same confusion between «datum» and «bottom edge» that this
        /// correction is about. The bottom edge is the origin plus the section's own lower bound, exactly as
        /// the frame authority computes it in the other direction.
        /// </summary>
        private static double BottomFibreZ(CantileverStructuralMemberPlan member) =>
            member.Start.Z + Factory.Get(member.SectionId, SectionDetailLevel.Tabulated).Bounds.MinY;

        // ---- 1-4: las cuatro elevaciones ------------------------------------------------------------------

        [Fact]
        public void LaBaseAPOYAEnElPiso()
        {
            Assert.Equal(CantileverColumnBaseDatum.BaseBottomZ, BottomFibreZ(Resolve().Base), 9);
        }

        [Fact]
        public void LaPlacaInferiorOcupaDeCeroAlEspesor()
        {
            var a = Resolve(thickness: 0.5);
            var plate = a.ColumnBottomPlate.Envelope();

            Assert.Equal(CantileverColumnBaseDatum.FloorZ, plate.MinZ, 12);
            Assert.Equal(0.5, plate.MaxZ, 12);
        }

        [Fact]
        public void LaColumnaARRANCAEnElEspesor()
        {
            var a = Resolve(thickness: 0.5);

            Assert.Equal(0.5, a.Column.Start.Z, 12);
        }

        [Fact]
        public void LaColumnaTERMINAEnEspesorMasCorteNominal()
        {
            var a = Resolve(thickness: 0.5, cut: 96.0);

            Assert.Equal(96.5, a.Column.End.Z, 12);
            Assert.Equal(96.0, a.Column.End.Z - a.Column.Start.Z, 12);
        }

        // ---- 5-8: quien se mueve y quien no ---------------------------------------------------------------

        [Fact]
        public void LaPlacaPOSTERIORDeLaBaseNoSeMueveAlCambiarElEspesor()
        {
            // Es una pieza de la BASE, y la base no se levanta. Si siguiera a la columna, la conexion se
            // separaria de la base que la sostiene.
            var delgada = Resolve(thickness: 0.25).BaseRearPlate.Envelope();
            var gruesa = Resolve(thickness: 1.0).BaseRearPlate.Envelope();

            Assert.Equal(delgada.MinZ, gruesa.MinZ, 12);
            Assert.Equal(delgada.MaxZ, gruesa.MaxZ, 12);
        }

        [Fact]
        public void ElCARTABONConservaSuContactoConLaBase()
        {
            var delgada = Resolve(thickness: 0.25).Gusset.Envelope();
            var gruesa = Resolve(thickness: 1.0).Gusset.Envelope();

            Assert.Equal(delgada.MinZ, gruesa.MinZ, 12);
            Assert.Equal(delgada.MaxZ, gruesa.MaxZ, 12);
        }

        [Fact]
        public void LosDatumsDeLaPlacaPosteriorYDeLaColumnaCOINCIDEN()
        {
            // El invariante que la correccion NO podia romper: un patron, dos caras, las mismas elevaciones
            // absolutas. Trasladar ciegamente los troqueles de la columna lo habria roto.
            var a = Resolve();

            var rear = a.RearPlatePunches.Select(p => Math.Round(p.Datum.V, 9)).OrderBy(v => v).ToList();
            var face = a.ColumnConnectionPunches.Select(p => Math.Round(p.Datum.V, 9)).OrderBy(v => v).ToList();

            Assert.NotEmpty(face);
            Assert.All(face, v => Assert.Contains(v, rear));
        }

        [Fact]
        public void LosTroquelesDeLaColumnaSubenConELLAYNingunoQuedaFuera()
        {
            var a = Resolve(thickness: 0.5);
            var radius = a.Pattern.Parameters.Diameter / 2.0;
            var start = a.Column.Start.Z;
            var end = a.Column.End.Z;

            Assert.NotEmpty(a.ColumnRegularPunches);

            foreach (var punch in a.ColumnConnectionPunches.Concat(a.ColumnRegularPunches))
            {
                Assert.True(punch.Datum.V - radius >= start - Tolerance,
                    "Un troquel de la columna cae por debajo de su cara de apoyo.");
                Assert.True(punch.Datum.V + radius <= end + Tolerance,
                    "Un troquel de la columna sobresale de su extremo superior.");
            }
        }

        // ---- 9-11: el espesor levanta, no alarga ----------------------------------------------------------

        [Fact]
        public void CambiarElEspesorMUEVELaColumnaYSuTope()
        {
            var delgada = Resolve(thickness: 0.25);
            var gruesa = Resolve(thickness: 1.0);

            Assert.Equal(0.75, gruesa.Column.Start.Z - delgada.Column.Start.Z, 12);
            Assert.Equal(0.75, gruesa.Column.End.Z - delgada.Column.End.Z, 12);
        }

        [Fact]
        public void CambiarElEspesorNOCambiaElCorteNominalDeColumnaNiDeBase()
        {
            var delgada = Resolve(thickness: 0.25);
            var gruesa = Resolve(thickness: 1.0);

            Assert.Equal(
                delgada.Column.End.Z - delgada.Column.Start.Z,
                gruesa.Column.End.Z - gruesa.Column.Start.Z, 12);

            Assert.Equal(
                delgada.Base.End.Y - delgada.Base.Start.Y,
                gruesa.Base.End.Y - gruesa.Base.Start.Y, 12);

            Assert.Equal(96.0, gruesa.Column.NominalCutLength, 12);
        }

        [Fact]
        public void ElBOMComercialNoCambiaAlLevantarLaColumna()
        {
            // La longitud nominal es la que se compra. Levantar la pieza no cambia lo que se pide al proveedor.
            var delgada = Resolve(thickness: 0.25);
            var gruesa = Resolve(thickness: 1.0);

            Assert.Equal(delgada.Column.NominalCutLength, gruesa.Column.NominalCutLength, 12);
            Assert.Equal(delgada.Base.NominalCutLength, gruesa.Base.NominalCutLength, 12);
            Assert.Equal(
                delgada.Column.Placement.SectionId.Value, gruesa.Column.Placement.SectionId.Value);
        }

        // ---- 12-14: lo que NO puede volver -----------------------------------------------------------------

        [Fact]
        public void LaBaseNOSeLevantaConLaColumna()
        {
            // Regresion 12 del encargo: restaurar la base elevada debe fallar. Si alguien la subiera, esta
            // comparacion la caza.
            Assert.Equal(CantileverColumnBaseDatum.FloorZ, BottomFibreZ(Resolve(thickness: 0.25).Base), 9);
            Assert.Equal(CantileverColumnBaseDatum.FloorZ, BottomFibreZ(Resolve(thickness: 1.0).Base), 9);
        }

        [Fact]
        public void NadaDeLaPiezaBajaDelPISO()
        {
            var envelope = Resolve().Envelope.Value;

            Assert.True(envelope.MinZ >= CantileverColumnBaseDatum.FloorZ - Tolerance,
                "Alguna pieza cuelga por debajo del piso.");
        }

        [Fact]
        public void LaColumnaNoATRAVIESASuPlaca()
        {
            var a = Resolve(thickness: 0.5);
            var plate = a.ColumnBottomPlate.Envelope();

            Assert.True(a.Column.Start.Z >= plate.MaxZ - Tolerance,
                "La columna empieza por debajo de la cara superior de su placa.");
        }

        [Fact]
        public void ElEspesorNoSeSumaDOSVeces()
        {
            // El modo mas facil de estropear esta correccion: levantar la columna Y volver a sumar el espesor
            // al calcular su tope.
            var a = Resolve(thickness: 0.5, cut: 96.0);

            Assert.Equal(96.5, a.Column.End.Z, 12);
            Assert.NotEqual(97.0, Math.Round(a.Column.End.Z, 9));
        }
    }
}
